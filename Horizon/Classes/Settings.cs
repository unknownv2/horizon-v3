using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NoDev.Common.IO;
using Newtonsoft.Json;

namespace NoDev.Horizon
{
    internal static class Settings
    {
        private static readonly Dictionary<string, object> ProgramSettings;

        private static readonly string DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Application.CompanyName, Application.ProductName
        );

        internal static string GetFilePath(string fileName)
        {
            string filePath = Path.Combine(DataFolder, fileName);

            string fileDir = filePath.Substring(0, filePath.LastIndexOf('\\'));
            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            return filePath;
        }

        internal static bool ContainsFile(string fileName)
        {
            return File.Exists(GetFilePath(fileName));
        }

        internal static EndianIO OpenRead(string fileName)
        {
            return new EndianIO(File.OpenRead(GetFilePath(fileName)), EndianType.Little);
        }

        internal static EndianIO OpenWrite(string fileName)
        {
            return new EndianIO(File.OpenWrite(GetFilePath(fileName)), EndianType.Little);
        }

        internal static string ReadString(string fileName)
        {
            string filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
                return null;

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        internal static void WriteString(string fileName, string text)
        {
            File.WriteAllText(GetFilePath(fileName), text, Encoding.UTF8);
        }

        static Settings()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);

            string settingsPath = GetFilePath("Settings");

            if (!File.Exists(settingsPath))
                ProgramSettings = new Dictionary<string, object>();
            else
            {
                try
                {
                    ProgramSettings = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(settingsPath));
                }
                catch
                {
                    ProgramSettings = new Dictionary<string, object>();
                }
            }

            PopulateDefaultValues();
        }

        private static void PopulateDefaultValues()
        {
            SetNew("ShowTutorial", true);
            SetNew("AlwaysOnTop", false);
            SetNew("DeviceExplorerEnabled", true);
            SetNew("BackupsEnabled", false);
            SetNew("Username", null);
            SetNew("Password", null);
            SetNew("WindowX", int.MinValue);
            SetNew("WindowY", int.MinValue);
            SetNew("WindowMaximized", false);
            SetNew("AdvancedMode", false);
        }

        private static void SetNew(string key, object value)
        {
            if (!ProgramSettings.ContainsKey(key))
                ProgramSettings.Add(key, value);
        }

        internal static void Save()
        {
            try
            {
                File.WriteAllText(GetFilePath("Settings"), JsonConvert.SerializeObject(ProgramSettings, Formatting.Indented));
            }
            catch
            {
                
            }
        }

        internal static bool Contains(string key)
        {
            return ProgramSettings.ContainsKey(key);
        }

        internal static string GetString(string key)
        {
            return (string)ProgramSettings[key];
        }

        internal static bool GetBoolean(string key)
        {
            return (bool)ProgramSettings[key];
        }

        internal static int GetInt32(string key)
        {
            return Convert.ToInt32(ProgramSettings[key]);
        }

        internal static void Set(string key, object value)
        {
            if (ProgramSettings.ContainsKey(key))
                ProgramSettings[key] = value;
            else
                ProgramSettings.Add(key, value);
        }
    }
}
