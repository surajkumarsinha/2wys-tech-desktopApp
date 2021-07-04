using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcutesClient.Helpers
{
    public static class AppSettings
    {
        public static string Url = AppSettingsUtil.GetString("Url");
        public static int SignBoxMargin = AppSettingsUtil.GetInt("SignBoxMargin");
        public static int SignBoxDimensions = AppSettingsUtil.GetInt("SignBoxDimensions");
    }

    /// <summary>
    /// Utility class for reading values from <tt>App.config</tt> and <tt>Web.config</tt> files.  Methods of this
    /// class support reading values of various types.  Overloads of these methods support case where value is
    /// mandatory or optional.  For optional keys use overloads which accept default values.
    /// </summary>
    public static class AppSettingsUtil
    {
        /// <summary>
        /// Gets the configuration value for the specified key.  If the value is not present or is blank,
        /// an exception is thrown.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <exception cref="ConfigurationErrorsException">The value is not specified, or is blank.</exception>
        /// <returns>The value in the config file.</returns>
        public static string GetString(string keyName)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            if (str == null || str.Trim().Length == 0)
                throw new ConfigurationErrorsException("No application setting available for key: " + keyName);
            return str;
        }

        /// <summary>
        /// Gets the configuration value for the specified key.  If the value is not present or is blank,
        /// the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultValue"></param>
        /// <returns>The value in the config file, or <paramref name="defaultValue"/> if the config value does not exist or is blank.</returns>
        public static string GetString(string keyName, string defaultValue)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            if (string.IsNullOrWhiteSpace(str))
            {
                Logger.WriteLog(string.Format("'{0}' Setting missing from Configuration File. Using Default value '{1}'.", keyName, defaultValue));
                return defaultValue;
            }
            return str;
        }

        /// <summary>
        /// Gets the config value for specified key and parses it as an int.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as an int.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as in int.</exception>
        public static int GetInt(string keyName)
        {
            string str = GetString(keyName);
            int value;
            if (!int.TryParse(str, out value))
            {
                string message = string.Format("Unable to parse app setting value for {0} as an int: {1}", keyName, str);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }

        /// <summary>
        /// If the value is not present or is blank,
        /// the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultValue"></param>
        /// <returns>The value in the config file as an int, or <paramref name="defaultValue"/> if the config value does not exist or is blank.</returns>
        public static int GetInt(string keyName, int defaultValue)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            int value;
            if (!int.TryParse(str, out value))
            {
                Logger.WriteLog(string.Format("'{0}' Setting missing from Configuration File. Using Default value '{1}'.", keyName, defaultValue));
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Gets the config value for specified key and parses it as an int.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as an int.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as in int.</exception>
        public static short GetShort(string keyName)
        {
            string str = GetString(keyName);
            short value;
            if (!short.TryParse(str, out value))
            {
                string message = string.Format("Unable to parse app setting value for {0} as an short: {1}", keyName, str);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }

        /// <summary>
        /// If the value is not present or is blank,
        /// the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultValue"></param>
        /// <returns>The value in the config file as an short, or <paramref name="defaultValue"/> if the config value does not exist or is blank.</returns>
        public static short GetShort(string keyName, short defaultValue)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            short value;
            if (!short.TryParse(str, out value))
            {
                Logger.WriteLog(string.Format("'{0}' Setting missing from Configuration File. Using Default value '{1}'.", keyName, defaultValue));
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as a double.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as a double.</exception>
        public static double GetDouble(string keyName)
        {
            string str = GetString(keyName);
            double value;
            if (!double.TryParse(str, out value))
            {
                string message = string.Format("Unable to parse app setting value for {0} as a double: {1}", keyName, str);
                throw new ConfigurationErrorsException(message);
            }
            return value;
        }

        /// <summary>
        /// If the value is not present or is blank,
        /// the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultValue"></param>
        /// <returns>The value in the config file as a double, or <paramref name="defaultValue"/> if the config value does not exist or is blank.</returns>
        public static double GetDouble(string keyName, double defaultValue)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            double value;
            if (!double.TryParse(str, out value))
            {
                Logger.WriteLog(string.Format("'{0}' Setting missing from Configuration File. Using Default value '{1}'.", keyName, defaultValue));
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as a bool.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as a bool.</exception>
        public static bool GetBool(string keyName)
        {
            string str = GetString(keyName);
            bool result;
            if (!bool.TryParse(str, out result))
            {
                string message = string.Format("Unable to parse app setting value for {0} as a bool: {1}", keyName, str);
                throw new ConfigurationErrorsException(message);
            }
            return result;
        }

        /// <summary>
        /// If the value is not present or is blank, the <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultValue"></param>
        /// <returns>The value in the config file as a bool, or <paramref name="defaultValue"/> if the config value does not exist or is blank.</returns>
        public static bool GetBool(string keyName, bool defaultValue)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            bool result;
            if (!bool.TryParse(str, out result))
            {
                Logger.WriteLog(string.Format("'{0}' Setting missing from Configuration File. Using Default value '{1}'.", keyName, defaultValue));
                return defaultValue;
            }
            return result;
        }

        /// <summary>
        /// Tries to read the key from the config file.  If the value is not specified, false is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="value">The value as specified in the config file.  This value will be false if no key value exists in config.</param>
        /// <returns>True if the key exists, otherwise false.</returns>
        public static bool TryGetBool(string keyName, out bool value)
        {
            string str = ConfigurationManager.AppSettings[keyName];
            return bool.TryParse(str, out value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as a TimeSpan.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as a double.</exception>
        public static TimeSpan GetMilliseconds(string keyName)
        {
            return TimeSpan.FromMilliseconds(GetDouble(keyName));
        }

        /// <summary>
        /// If the value is not present or is blank, the <paramref name="defaultMillis"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultMillis"></param>
        /// <returns>The value in the config file as a TimeSpan, or <paramref name="defaultMillis"/> if the config value does not exist or is blank.</returns>
        public static TimeSpan GetMilliseconds(string keyName, double defaultMillis)
        {
            return TimeSpan.FromMilliseconds(GetDouble(keyName, defaultMillis));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as a TimeSpan.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as a double.</exception>
        public static TimeSpan GetSeconds(string keyName)
        {
            return TimeSpan.FromSeconds(GetDouble(keyName));
        }

        /// <summary>
        /// If the value is not present or is blank,
        /// the <paramref name="defaultSeconds"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultSeconds"></param>
        /// <returns>The value in the config file as a TimeSpan, or <paramref name="defaultSeconds"/> if the config value does not exist or is blank.</returns>
        public static TimeSpan GetSeconds(string keyName, double defaultSeconds)
        {
            return TimeSpan.FromSeconds(GetDouble(keyName, defaultSeconds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The value in the config file as a TimeSpan.</returns>
        /// <exception cref="ConfigurationErrorsException">The value is not specified or cannot be parsed as a double.</exception>
        public static TimeSpan GetMinutes(string keyName)
        {
            return TimeSpan.FromMinutes(GetDouble(keyName));
        }

        /// <summary>
        /// If the value is not present or is blank,
        /// the <paramref name="defaultMinutes"/> is returned.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <param name="defaultMinutes"></param>
        /// <returns>The value in the config file as a TimeSpan, or <paramref name="defaultMinutes"/> if the config value does not exist or is blank.</returns>
        public static TimeSpan GetMinutes(string keyName, double defaultMinutes)
        {
            return TimeSpan.FromMinutes(GetDouble(keyName, defaultMinutes));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The CSV value in the config file, split into its components and returned as a string array.</returns>
        public static string[] GetCsv(string keyName)
        {
            string str = GetString(keyName);
            return str.Split(',');
        }

        /// <summary>
        /// Gets the path string associated with the specified key, and ensures that the path exists.
        /// If the path does not exist, a <see cref="ConfigurationErrorsException"/> is thrown.
        /// </summary>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The path specified in the config file, with a guarantee that it exists on the disk.</returns>
        /// <exception cref="ConfigurationErrorsException">The key is not present in appSettings, or the path does not exist.</exception>
        public static string GetExistingFilePath(string keyName)
        {
            string path = GetString(keyName);
            if (!File.Exists(path))
                throw new ConfigurationErrorsException(
                    string.Format("Configuration key '{0}' holds a non-existant file path: {1}", keyName, path));
            return path;
        }

        /// <summary>
        /// Gets the enum value from the config document having the specified key.  The enum value is compared in a case-insensitive fashion.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>The parsed enum value.</returns>
        /// <exception cref="ConfigurationErrorsException">The key was not present</exception>
        /// <exception cref="ConfigurationErrorsException">The config value could not be parsed as a value of enum <typeparamref name="T"/></exception>
        public static T GetEnum<T>(string keyName)
        {
            string value = GetString(keyName);
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (ArgumentException ex)
            {
                string message = string.Format("Configuration key '{0}' has value '{1}' that could not be parsed as a member of the {2} enum type.", keyName, value, typeof(T).Name);
                throw new ConfigurationErrorsException(message, ex);
            }
        }

        /// <summary>
        /// Gets a connection string from the connectionStrings node in config.  Note that this method reads
        /// from the <tt>configuration</tt> section, not the <tt>appSettings</tt> one.
        /// </summary>
        /// <remarks>
        /// Config sections for connection strings look like this:
        /// <code>
        /// &lt;configuration&gt;
        ///     &lt;connectionStrings&gt;
        ///         &lt;add name="NAV" connectionString="Data Source=SERVER;Initial Catalog=DATABASE;User ID=USER;Password=PASSWORD"/&gt;
        ///     &lt;/connectionStrings&gt;
        /// &lt;/configuration&gt;
        /// </code>
        /// </remarks>
        /// <param name="keyName">The name of the key, as specified in application config.</param>
        /// <returns>A connection string from the config file.</returns>
        /// <exception cref="ConfigurationErrorsException">No connection string section was found or no entry present with specified key.</exception>
        public static string GetConnectionString(string keyName)
        {
            /*
            <configuration>
                <connectionStrings>
                    <add name="NAV" connectionString="Data Source=SERVER;Initial Catalog=DATABASE;User ID=USER;Password=PASSWORD"/>
                </connectionStrings>
            </configuration>
            */

            ConnectionStringSettings connStr = ConfigurationManager.ConnectionStrings[keyName];
            if (connStr == null)
                throw new ConfigurationErrorsException("No connection string found for key: " + keyName);
            return connStr.ConnectionString;
        }

        public static bool ConnectionStringExists(string keyName)
        {
            return ConfigurationManager.ConnectionStrings[keyName] != null;
        }
    }
}
