using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Fleck;
using AcutesClient.Helpers;
using Newtonsoft.Json.Linq;
using System.Text;
using Rite.DigiSign;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.Security.Cryptography;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using iTextSharp.text;
using System.Diagnostics;

namespace AcutesClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
   
    /*
    public class Ngrok
    {
        //static void Main()
        {
            LaunchCommandLineApp();
        }

        /// <summary>
        /// Launch the application with some options set.
        /// </summary>
        static void LaunchCommandLineApp()
        {
           
            const string ex1 = "C:\\";
            const string ex2 = "C:\\Dir";

            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "ngrok.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "-f j -o \"" + ex1 + "\" -z 1.0 -s y " + ex2;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }
    }

    */
    public partial class App : Application
    {
        public App()
        {
            #region Cef Settings
            var settings = new CefSettings()
            {
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };

            string binPath = System.AppDomain.CurrentDomain.BaseDirectory;
            settings.BrowserSubprocessPath = Path.Combine(binPath, @"CefSharp.BrowserSubprocess.exe");
            //Example of setting a command line argument
            //Enables WebRTC
            settings.CefCommandLineArgs.Add("enable-media-stream");

            //Perform dependency check to make sure all relevant resources are in our output directory.
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            #endregion

            #region WebSocket
            var server = new WebSocketServer("ws://127.0.0.1:8085");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Logger.WriteLog("Web Socket opened on Port 8085!");
                };
                socket.OnClose = () =>
                {
                    Logger.WriteLog("Web Socket closed!");
                };
                socket.OnMessage = message =>
                {
                    ParseMessage(message, socket);
                };
            });
            Logger.WriteLog("Web Socket Listner Started!");
            #endregion
        }

        X509Certificate2 SelectedSignature {get;set;}
        private string SelectSignatureName {get;set;}
        public Tuple<X509Certificate2, object> Certificate { get; set; }

        private void ParseMessage(string message, IWebSocketConnection socket)
        {
            Logger.WriteLog("Message Received");
            if (message.Length < 4)
                return;
            Logger.WriteLog("Message Received : " + message.Substring(0, 4));
            
            var res = new JObject();
            switch (message.Substring(0, 3).ToUpper())
            {
                #region Sign
                case "SGN":
                    {
                        string tempFile = Path.GetTempFileName();
                        string tempSignImageFile = Path.GetTempFileName();
                        string signedFile = Path.GetTempFileName();
                        try
                        {
                            var msg = message.Substring(4);
                            //convert this into json
                            var json = JObject.Parse(msg);

                            string payload = Convert.ToString(json["PdfFile"]);

                            //payload is the base64 of the pdf file...
                            //write it down in a temp place
                            File.WriteAllBytes(tempFile, Convert.FromBase64String(payload));

                            //sign this file using the SignAppearance options and the pfx/dsc received
                            string pageToSign = Convert.ToString(json["PageToSign"]);
                            string positionOnPage = Convert.ToString(json["PositionOnPage"]);
                            string visibleText = Convert.ToString(json["VisibleText"]);
                            var visibleTextFont = GetVisibleTextFont(Convert.ToString(json["VisibleTextFont"]));
                            
                            string signImageBytes = Convert.ToString(json["SignImage"]);
                            if (!string.IsNullOrWhiteSpace(signImageBytes))
                                File.WriteAllBytes(tempSignImageFile, Convert.FromBase64String(signImageBytes));

                            bool ret = SignPdf(tempFile, signedFile, "", Certificate.Item1, "2WYS Pdf Digital Signature", "Online", positionOnPage, pageToSign, visibleText, visibleTextFont, !string.IsNullOrWhiteSpace(signImageBytes) ? tempSignImageFile : ""); 
                            //return base64 of the signed file

                            if (ret)
                            {
                                res["Status"] = "Ok";
                                res["Data"] = Convert.ToBase64String(File.ReadAllBytes(signedFile));
                            }
                            else
                            {
                                throw new ApplicationException("Signing failed.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLog("Error during Singing data : " + ex.ToString());
                            res["Status"] = "Error";
                            res["Data"] = "Error Occurred during signing data";
                        }
                        finally
                        {
                            if (File.Exists(tempFile))
                                File.Delete(tempFile);
                            if (File.Exists(signedFile))
                                File.Delete(signedFile);
                            if (File.Exists(tempSignImageFile))
                                File.Delete(tempSignImageFile);
                        }
                        break;
                    }
                #endregion
                #region Get
                case "GET":
                    {
                        string pfxFilePath = Path.GetTempFileName();
                        try
                        {
                            Logger.WriteLog("Came to Load Signatures.");

                            var msg = message.Substring(4);
                            //convert this into json
                            var json = JObject.Parse(msg);
                            //Log(Convert.ToString(json["Subject"]));
                            string SignatureType = Convert.ToString(json["SignatureType"]);

                            Logger.WriteLog(string.Format("Signature Load Type : '{0}'", SignatureType));

                            if (SignatureType.ToUpper().Equals("PFX"))
                            {
                                string pfxFileContent = Convert.ToString(json["PfxFile"]);
                                File.WriteAllBytes(pfxFilePath, Convert.FromBase64String(pfxFileContent));

                                string pfxPassword = Convert.ToString(json["PfxPassword"]);

                                X509Certificate2 certificate = new X509Certificate2(pfxFilePath, pfxPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                                var rsa = certificate.PrivateKey as RSACryptoServiceProvider;
                                var alg = rsa.SignatureAlgorithm;

                                //if (alg.ToLower().Contains("sha1"))
                                //    throw new ApplicationException("Certificate does not implement SHA 256");
                                Certificate = new Tuple<X509Certificate2, object>(certificate, null);
                            }
                            else
                            {
                                X509Store store = new X509Store("MY", StoreLocation.CurrentUser);
                                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                                X509Certificate2Collection fcollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                                X509Certificate2Collection scollection = System.Security.Cryptography.X509Certificates.X509Certificate2UI.SelectFromCollection(fcollection, "2WYS Client Signature Selector", "Select a certificate from the following list for Signing", X509SelectionFlag.SingleSelection);

                                //scollection should only have one certificate
                                if (scollection.Count == 1)
                                {
                                    SelectedSignature = scollection[0];
                                    SelectSignatureName = SelectedSignature.SubjectName.Name;

                                    var rsa = SelectedSignature.PrivateKey as RSACryptoServiceProvider;
                                    byte[] data = Encoding.UTF8.GetBytes("sample");
                                    byte[] sign = rsa.SignData(data, CryptoConfig.MapNameToOID("SHA1"));

                                    Certificate = new Tuple<X509Certificate2, object>(SelectedSignature, rsa);
                                }
                            }
                            res["Status"] = "Ok";
                            res["Data"] = "";
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLog("Error during getting Signatures : " + ex.ToString());
                            res["Status"] = "Error";
                            res["Data"] = "Error Occurred during getting signatures";
                        }
                        finally
                        {
                            if (File.Exists(pfxFilePath))
                                File.Delete(pfxFilePath);
                        }
                        break;
                    }
                #endregion

                #region Tunnel
                case "TNT":
                    {
                        //this refers to start of the tunnel
                        // string strCmdText;
                        // strCmdText = "ngrok http 80";
                        try
                        {
                            string o = "";
                            string error = string.Empty;
                            Process ngrok = new Process();
                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.FileName = "CMD.EXE";
                            startInfo.Arguments = "/K ngrok http 80";

                            startInfo.UseShellExecute = false;
                            startInfo.RedirectStandardOutput = true;
                            startInfo.RedirectStandardError = true;
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            ngrok.StartInfo = startInfo;
                            ngrok.ErrorDataReceived += ngrok_ErrorDataReceived;
                            ngrok.OutputDataReceived += ngrok_ErrorDataReceived;
                            ngrok.EnableRaisingEvents = true;
                            ngrok.Start();
                            ngrok.BeginOutputReadLine();
                            ngrok.BeginErrorReadLine();
                            //

                            
                            //ngrok.Start();
                            //StreamReader reader = ngrok.StandardOutput;
                            //string output = reader.ReadToEnd();

                            /*while (!ngrok.StandardOutput.EndOfStream)
                            {
                                var line = ngrok.StandardOutput.ReadLine();     
                                Console.WriteLine(line);
                            }*/
                            
                            //using (StreamReader myOutput = ngrok.StandardOutput)
                            //{
                            //    o = myOutput.ReadToEnd();
                            //}

                            //using (StreamReader myError = ngrok.StandardError)
                            //{
                            //    error = myError.ReadToEnd();                         
                            //}

                            ngrok.WaitForExit();
                            
                            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            // var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                            // path = path.Substring(6);
                            // startInfo.FileName = Path.Combine(path,  @"ngrok.exe"); 
                            
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLog("Error during performing command " + ex.ToString());
                            res["Status"] = "Error";
                            res["Data"] = "Error Occurred command line execution";
                        }
                        
                        break;
                    }

                #endregion

                default:
                    {
                        res["Status"] = "Error";
                        res["Data"] = "Invalid Message";
                        break;
                    }
            }
            socket.Send(JsonConvert.SerializeObject(res));
        }

        private void ngrok_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }

        public bool SignPdf(string sourceFile, string destinationFile, string password, X509Certificate2 cert, string reason, string location, string positionOnPage, string pagetoSign, string visibleText, Font visibleTextFont, string signImage)
        {
            PdfReader pdfReader = null;
            try
            {
                Org.BouncyCastle.X509.X509CertificateParser cp = new Org.BouncyCastle.X509.X509CertificateParser();
                Org.BouncyCastle.X509.X509Certificate[] chain = new Org.BouncyCastle.X509.X509Certificate[] { cp.ReadCertificate(cert.RawData) };
                IExternalSignature externalSignature = new X509Certificate2Signature(cert, "SHA-1");


                if (string.IsNullOrEmpty(password))
                    pdfReader = new PdfReader(sourceFile);
                else
                    pdfReader = new PdfReader(sourceFile, new System.Text.ASCIIEncoding().GetBytes(password));

                using (FileStream signedPdf = new FileStream(destinationFile, FileMode.Create))
                {
                    PdfStamper pdfStamper = PdfStamper.CreateSignature(pdfReader, signedPdf, '\0');

                    pdfStamper.SetEncryption(false, password, password, PdfWriter.ALLOW_COPY | PdfWriter.ALLOW_DEGRADED_PRINTING | PdfWriter.ALLOW_PRINTING);

                    PdfSignatureAppearance signatureAppearance = pdfStamper.SignatureAppearance;

                    //Reason
                    if (!string.IsNullOrEmpty(reason))
                        signatureAppearance.Reason = reason;
                    //Location
                    if (!string.IsNullOrEmpty(location))
                        signatureAppearance.Location = location;

                    //PrintableText and PrintableTextFont
                    if (!string.IsNullOrEmpty(visibleText))
                    {
                        signatureAppearance.Layer2Text = visibleText;
                        signatureAppearance.Layer2Font = visibleTextFont;
                    }

                    //RenderMode : Graphic / Description
                    signatureAppearance.SignatureRenderingMode = !string.IsNullOrWhiteSpace(signImage) ? PdfSignatureAppearance.RenderingMode.GRAPHIC : PdfSignatureAppearance.RenderingMode.DESCRIPTION;
                    
                    //Sign Image
                    if (!string.IsNullOrEmpty(signImage))
                        signatureAppearance.SignatureGraphic = Image.GetInstance(signImage);

                    //Page to Sign
                    int pageNo = pagetoSign.ToUpper().Equals("LAST") ? pdfReader.NumberOfPages : 1;

                    //Position On Page
                    float height = AppSettings.SignBoxDimensions;
                    float width = AppSettings.SignBoxDimensions;
                    float x = 0;
                    float y = 0;
                    switch (positionOnPage.ToUpper())
                    {
                        case "TOP RIGHT":
                            Rectangle mediabox1 = pdfReader.GetPageSize(1); 
                            x = mediabox1.Width - width - AppSettings.SignBoxMargin;
                            y = mediabox1.Height - height - AppSettings.SignBoxMargin;
                            break;
                        case "TOP LEFT":
                            Rectangle mediabox2 = pdfReader.GetPageSize(1);
                            x = AppSettings.SignBoxMargin;
                            y = mediabox2.Height - height - AppSettings.SignBoxMargin;
                            break;
                        case "BOTTOM RIGHT":
                            Rectangle mediabox3 = pdfReader.GetPageSize(1);
                            x = mediabox3.Width - width - AppSettings.SignBoxMargin;
                            y = AppSettings.SignBoxMargin;
                            break;
                        case "BOTTOM LEFT":
                            x = AppSettings.SignBoxMargin;
                            y = AppSettings.SignBoxMargin;
                            break;
                        default:
                            throw new ApplicationException(string.Format("Invalid Value for Position on Page [{0}].", positionOnPage));
                    }

                    string fieldName = Guid.NewGuid().ToString();
                    iTextSharp.text.Rectangle rectarea = new iTextSharp.text.Rectangle(x, y, x + width, y + height);
                    signatureAppearance.SetVisibleSignature(rectarea, pageNo, fieldName);
                    MakeSignature.SignDetached(signatureAppearance, externalSignature, chain, null, null, null, 0, CryptoStandard.CMS);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(string.Format("Error occured during signing Pdf '{0}' : {1}", Path.GetFileName(sourceFile), ex.Message));
                return false;
            }
            finally
            {
                pdfReader.Close();
                pdfReader.Dispose();
            }
        }

        public Font GetVisibleTextFont(string PrintableTextFont)
        {
            Font VisibleTextFont = null;
            if (!string.IsNullOrEmpty(PrintableTextFont))
            {
                var fnt = FontFactory.GetFont(PrintableTextFont);
                if (fnt == null)
                    VisibleTextFont = FontFactory.GetFont(FontFactory.HELVETICA);
                else
                    VisibleTextFont = fnt;
            }
            return VisibleTextFont;
        }

    }
}
