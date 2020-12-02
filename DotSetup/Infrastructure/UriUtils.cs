// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DotSetup
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

        public void OpenUrl()
        {

        }

        public string GetEdgeExe()
        {
            string WinDir = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System)).ToString();

            if (string.IsNullOrEmpty(WinDir))
                return null;

            string edgeexepath = WinDir + EdgeAppFileName;
            return (File.Exists(edgeexepath) ? edgeexepath : null);
        }


        public string GetOperaEXE()
        {
            try
            {
                string[] arg = new string[2];
                string subKey = "\\Software\\Opera Software";
                string OperaExePath = "";

                arg[0] = "HKCU" + subKey;
                arg[1] = "Last Stable Install Path";

                OperaExePath = RegistryUtils.Instance.GetRegKeyValue(arg) + "launcher.exe";
                if (File.Exists(OperaExePath) && !OperaExePath.Equals("launcher.exe"))
                    return OperaExePath;

                OperaExePath = RegistryUtils.Instance.GetRegKeyValue(arg) + "opera.exe";
                if (File.Exists(OperaExePath) && !OperaExePath.Equals("opera.exe"))
                    return OperaExePath;

                arg[1] = "Last Directory3";
                OperaExePath = RegistryUtils.Instance.GetRegKeyValue(arg) + "opera.exe";
                if (File.Exists(OperaExePath) && !OperaExePath.Equals("opera.exe"))
                    return OperaExePath;

                arg[1] = "Last CommandLine";
                OperaExePath = RegistryUtils.Instance.GetRegKeyValue(arg);
                if (File.Exists(OperaExePath))
                    return OperaExePath;

                arg[1] = "Last CommandLine v2";
                OperaExePath = RegistryUtils.Instance.GetRegKeyValue(arg);
                if ((OperaExePath != null) && File.Exists(OperaExePath.TrimStart()))
                    return OperaExePath;


                if (!string.IsNullOrEmpty(programFiles))
                {
                    OperaExePath = programFiles.ToString() + "\\Opera\\opera.exe";
                    if (File.Exists(OperaExePath))
                        return OperaExePath;
                }

                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    OperaExePath = programFilesX86.ToString() + "\\Opera\\opera.exe";
                    if (File.Exists(OperaExePath))
                        return OperaExePath;
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
            finally
            {
            }
            return null;
        }

        public string GetChromeExe()
        {
            try
            {
                string[] arg = new string[1];

                string subKey = "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe\\";
                string partialExePath = "\\Google\\Chrome\\Application\\chrome.exe";

                arg[0] = "HKLM" + subKey;
                string ChromeExePath = RegistryUtils.Instance.GetRegKeyValue(arg);
                if (File.Exists(ChromeExePath))
                    return ChromeExePath;

                arg[0] = "HKCU" + subKey;
                ChromeExePath = RegistryUtils.Instance.GetRegKeyValue(arg);
                if (File.Exists(ChromeExePath))
                    return ChromeExePath;

                ChromeExePath = programFiles + partialExePath;
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
                Logger.GetLogger().Warning("GetChromeEXE failed: " + e.Message);
#endif
            }
            finally
            {
            }
            return null;
        }

        private string ReadLocationFromRegistry(string rootKey)
        {
            string[] arg = new string[2];
            string subKey = "\\SOFTWARE\\Mozilla\\Mozilla Firefox";

            arg[0] = rootKey + subKey;
            arg[1] = "CurrentVersion";
            string wst = RegistryUtils.Instance.GetRegKeyValue(arg);
            if (!string.IsNullOrEmpty(wst))
            {
                arg[0] = arg[0] + "\\" + wst + "\\Main";
                arg[1] = "PathToExe";
                return RegistryUtils.Instance.GetRegKeyValue(arg);
            }

            return null;
        }

        public string GetFirefoxExe()
        {
            try
            {
                string[] arg = new string[1];
                string subKey = "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\firefox.exe\\";

                arg[0] = "HKLM" + subKey;
                string FirefoxExePath = RegistryUtils.Instance.GetRegKeyValue(arg);
                if (File.Exists(FirefoxExePath))
                    return FirefoxExePath;

                arg[0] = "HKCU" + subKey;
                FirefoxExePath = RegistryUtils.Instance.GetRegKeyValue(arg);
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
                Logger.GetLogger().Warning("GetFirefoxEXE failed: " + e.Message);
#endif
            }
            finally
            {
            }

            return null;
        }


        public string GetIEExe()
        {
            try
            {
                string[] arg = new string[1];
                string subKey = "\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\iexplore.exe\\";

                arg[0] = "HKLM" + subKey;
                string IEExe = RegistryUtils.Instance.GetRegKeyValue(arg);

                if (File.Exists(IEExe))
                    return IEExe;

                arg[0] = "HKCU" + subKey;
                IEExe = RegistryUtils.Instance.GetRegKeyValue(arg);
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
                Logger.GetLogger().Warning("GetIEEXE failed: " + e.Message);
#endif
            }
            finally
            {
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
    }
}

