using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcutesClient.Helpers
{
    public class Logger
    {
        private static string LogFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        public static void WriteLog(string msg)
        {

            string logFile = Path.Combine(LogFolder, "2WYS", string.Format("2WYS_{0}.log", DateTime.Now.ToString("yyyy-MM-dd_HH")));
            Directory.CreateDirectory(Path.GetDirectoryName(logFile));
            File.AppendAllText(logFile, DateTime.Now.ToString() + " : " + msg + Environment.NewLine);
        }
    }
}
