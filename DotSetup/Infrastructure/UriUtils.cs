// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DotSetup.Infrastructure
{
    public sealed class UriUtils
    {
        private const string EdgeAppFileName = "\\SystemApps\\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\\MicrosoftEdge.exe";
        private static readonly UriUtils instance = new UriUtils();
        private readonly string programFiles;
        private readonly string programFilesX86;
        private readonly string localAppData;

        static UriUtils()
        {
        }

        private UriUtils()
        {
            try
            {
                programFiles = KnownFolders.GetPath(KnownFolder.ProgramFiles).ToString();  //Environment.ExpandEnvironmentVariables("%ProgramW6432%");
                programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");
                localAppData = KnownFolders.GetPath(KnownFolder.LocalAppData).ToString();
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning("KnownFolders failed: " + e.Message);
#endif
            }
            finally
            {
            }
        }

        public static UriUtils Instance => instance;

        public string GetEdgeExe()
        {
            string WinDir = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).ToString();

            if (string.IsNullOrEmpty(WinDir))
                return null;

            string edgeexepath = WinDir + EdgeAppFileName;
            return File.Exists(edgeexepath) ? edgeexepath : null;
        }


        public string GetOperaEXE()
        {
            try
            {
                string operaExePath;
                const string operaRegBase = "HKCU\\Software\\Opera Software";
                string regKeyValue = RegistryUtils.Instance.GetRegKeyValue(operaRegBase, "Last Stable Install Path");

                if (!string.IsNullOrEmpty(regKeyValue))
                {
                    operaExePath = regKeyValue + "launcher.exe";
                    if (File.Exists(operaExePath))
                        return operaExePath;

                    operaExePath = regKeyValue + "opera.exe";
                    if (File.Exists(operaExePath))
                        return operaExePath;
                }

                regKeyValue = RegistryUtils.Instance.GetRegKeyValue(operaRegBase, "Last Directory3");
                operaExePath = regKeyValue + "opera.exe";
                if (!string.IsNullOrEmpty(regKeyValue) && File.Exists(operaExePath))
                    return operaExePath;

                regKeyValue = RegistryUtils.Instance.GetRegKeyValue(operaRegBase, "Last CommandLine");
                if (!string.IsNullOrEmpty(regKeyValue) && File.Exists(regKeyValue))
                    return regKeyValue;

                regKeyValue = RegistryUtils.Instance.GetRegKeyValue(operaRegBase, "Last CommandLine v2");
                if (!string.IsNullOrEmpty(regKeyValue) && File.Exists(regKeyValue.TrimStart()))
                    return regKeyValue;


                if (!string.IsNullOrEmpty(programFiles))
                {
                    operaExePath = programFiles + "\\Opera\\opera.exe";
                    if (File.Exists(operaExePath))
                        return operaExePath;
                }

                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    operaExePath = programFilesX86 + "\\Opera\\opera.exe";
                    if (File.Exists(operaExePath))
                        return operaExePath;
                }
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning("GetOperaEXE failed: " + e.Message);
#endif
            }

            return null;
        }

        public string GetChromeExe()
        {
            // Reads a string value from the specified product's registry key. Returns true
            // if the value is present and successfully read.
            string GetClientStateValue(bool userLevel, string appGuid, string valueName)
            {
                string installationRegKey = "Software\\Google\\Update\\ClientState";
                if (!string.IsNullOrEmpty(appGuid))
                    installationRegKey = $"{installationRegKey}\\{appGuid}";

                return RegistryUtils.Instance.GetRegKeyValue(userLevel ? "HKCU" : "HKLM", installationRegKey, valueName, Microsoft.Win32.RegistryView.Registry32);
            }

            // Reads the path to setup.exe from the value "UninstallString" within the
            // specified product's registry key. Returns an empty string if an error
            // occurs or the product is not installed at the specified level.
            string GetSetupExeFromRegistry(bool userLevel, string appGuid)
            {
                string setupExePath = GetClientStateValue(userLevel, appGuid, "UninstallString");
                if (!File.Exists(setupExePath))
                    return string.Empty;
                return setupExePath;
            }

            // Returns the path to an existing setup.exe at the specified level, if it can
            // be found via the registry.
            string GetSetupExeForInstallationLevel(bool userLevel)
            {
                // Look in the registry for Chrome Binaries first.
                string setupExePath = GetSetupExeFromRegistry(userLevel, "{4DC8B4CA-1BDA-483e-B5FA-D3C12E15B62D}");
                // If the above fails, look in the registry for Chrome next.
                if (string.IsNullOrEmpty(setupExePath))
                    setupExePath = GetSetupExeFromRegistry(userLevel, "{8A69D345-D564-463c-AFF1-A69D9E530F96}");
                // If we fail again, then setupExePath would be empty.
                return setupExePath;
            }

            // Returns the path to an installed |exeFile| (e.g. chrome.exe) at the
            // specified level, given |setupExePath| from the registry.  Returns empty
            // string if none found, or if |setupExePath| is empty.
            string FindExeRelativeToSetupExe(string setupExePath, string exeFile)
            {
                if (string.IsNullOrEmpty(setupExePath))
                    return string.Empty;

                // The uninstall path contains the path to setup.exe, which is two levels
                // down from |exeFile|. Move up two levels (plus one to drop the file
                // name) and look for chrome.exe from there.
                string exePath = Path.Combine(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(setupExePath)).ToString()).ToString(), exeFile);
                if (File.Exists(exePath))
                    return exePath;

                // By way of mild future proofing, look up one to see if there's a
                // |exeFile| in the version directory
                exePath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(setupExePath)).ToString(), exeFile);
                if (File.Exists(exePath))
                    return exePath;

                return string.Empty;
            }

            string GetChromePathForInstallationLevel(bool userLevel) => FindExeRelativeToSetupExe(GetSetupExeForInstallationLevel(userLevel), "chrome.exe");

            string path = GetChromePathForInstallationLevel(false);
            if (string.IsNullOrWhiteSpace(path))
                path = GetChromePathForInstallationLevel(true);

            if (File.Exists(path))
                return path;

            //if we didn't manage to locate Chrome with the official algorithm, let's try again with unofficial algorithm
            try
            {
                const string subKey = "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe\\";
                const string partialExePath = "\\Google\\Chrome\\Application\\chrome.exe";

                string regKeyValue = RegistryUtils.Instance.GetRegKeyValue("HKLM" + subKey);
                if (File.Exists(regKeyValue))
                    return regKeyValue;

                regKeyValue = RegistryUtils.Instance.GetRegKeyValue("HKCU" + subKey);
                if (File.Exists(regKeyValue))
                    return regKeyValue;

                string ChromeExePath = programFiles + partialExePath;
                if (File.Exists(ChromeExePath))
                    return ChromeExePath;

                ChromeExePath = programFilesX86 + partialExePath;
                if (File.Exists(ChromeExePath))
                    return ChromeExePath;

                ChromeExePath = localAppData + partialExePath;
                if (File.Exists(ChromeExePath))
                    return ChromeExePath;
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning($"GetChromeEXE failed: {e}");
#endif
            }

            return null;
        }

        public string GetFirefoxExe()
        {
            static string ReadLocationFromRegistry(string rootKey)
            {
                string subKey = rootKey + "\\SOFTWARE\\Mozilla\\Mozilla Firefox";
                string regKeyValue = RegistryUtils.Instance.GetRegKeyValue(subKey, "CurrentVersion");
                
                if (!string.IsNullOrEmpty(regKeyValue))
                {
                    return RegistryUtils.Instance.GetRegKeyValue(subKey + "\\" + regKeyValue + "\\Main", "PathToExe");
                }

                return null;
            };

            try
            {
                string subKey = "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\firefox.exe\\";

                string FirefoxExePath = RegistryUtils.Instance.GetRegKeyValue("HKLM" + subKey);
                if (File.Exists(FirefoxExePath))
                    return FirefoxExePath;

                FirefoxExePath = RegistryUtils.Instance.GetRegKeyValue("HKCU" + subKey);
                if (File.Exists(FirefoxExePath))
                    return FirefoxExePath;

                FirefoxExePath = ReadLocationFromRegistry("HKLM");
                if (File.Exists(FirefoxExePath))
                    return FirefoxExePath;

                FirefoxExePath = ReadLocationFromRegistry("HKCU");
                if (File.Exists(FirefoxExePath))
                    return FirefoxExePath;
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning($"GetFirefoxEXE failed: {e}");
#endif
            }

            return null;
        }


        public string GetIEExe()
        {
            try
            {
                const string subKey = "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\iexplore.exe\\";

                string IEExe = RegistryUtils.Instance.GetRegKeyValue("HKLM" + subKey);

                if (File.Exists(IEExe))
                    return IEExe;

                IEExe = RegistryUtils.Instance.GetRegKeyValue("HKCU" + subKey);
                if (File.Exists(IEExe))
                    return IEExe;
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Warning($"GetIEEXE failed: {e}");
#endif
            }

            return null;
        }


        public string GetBrowserVer(string arg)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string ExeFilePath = "";

            switch (arg)
            {
                case "ie":
                    ExeFilePath = GetIEExe();
                    break;
                case "chrome":
                    ExeFilePath = GetChromeExe();
                    break;
                case "firefox":
                    ExeFilePath = GetFirefoxExe();
                    break;
                case "opera":
                    ExeFilePath = GetOperaEXE();
                    break;
                case "edge":
                    ExeFilePath = GetEdgeExe();
                    break;
                default:
#if DEBUG
                    Logger.GetLogger().Warning(arg + " browser not supported.");
#endif
                    return "";
            }

            if (ExeFilePath == null)
                return "";

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(ExeFilePath);

            if (arg.Equals("opera") || arg.Equals("firefox"))
                return fileVersionInfo.FileVersion ?? "";


            return fileVersionInfo.ProductVersion ?? "";

        }

        internal string GetDefault()
        {
            const string userChoice = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
            string progId = "";
            string defaultBrowser = "";
            using (Microsoft.Win32.RegistryKey userChoiceKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(userChoice))
            {
                if (userChoiceKey != null)
                {
                    var progIdValue = userChoiceKey.GetValue("Progid");
                    if (progIdValue != null)
                    {
                        progId = progIdValue.ToString();
                    }
                }

                if (progId.StartsWith("IE.HTTP"))
                    defaultBrowser = "IEXPLORE.EXE";
                else if (progId.StartsWith("FirefoxURL"))
                    defaultBrowser = "FIREFOX.EXE";
                else if (progId.StartsWith("ChromeHTML"))
                    defaultBrowser = "CHROME.EXE";
                else if (progId.StartsWith("OperaStable"))
                    defaultBrowser = "LAUNCHER.EXE";
                else if (progId.StartsWith("AppXq0fevzme2pys62n3e0fbqa7peapykr8v"))
                    defaultBrowser = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe!MicrosoftEdge";
            }
            return defaultBrowser;
        }

        public static bool CheckURLValid(string url) => Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

