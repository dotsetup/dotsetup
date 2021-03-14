// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;

namespace DotSetup
{
    public class RequirementHandlers
    {

        public static class RequirementType
        {
            public const string Disk = "disk",
                Processor = "processor",
                Ram = "ram",
                RegistryKeyExists = "registrykeyexists",
                Process = "process",
                BrowserVersion = "browserversion",
                OSVersion = "osversion",
                PlatformVersion = "platformversion",
                BrowserInstalled = "browserinstalled",
                BrowserDefault = "browserdefault",
                RegistryKeyValue = "registrykeyvalue",
                HasAdminPrivileges = "hasadminprivileges",
                UserAdmin = "useradmin",
                SystemType = "systemtype",
                FileExists = "fileexists",
                ConfigValue = "configvalue";
        }

        public enum CompareOperationType
        {
            Equal,
            Less,
            Greater,
            LessEqual,
            GreaterEqual,
            Contains,
            StartsWith,
            EndsWith
        }


        public enum LogicalOperatorType
        {
            AND,
            OR,
            NOT
        }

        private static readonly Dictionary<string, Func<string[], string>> methodsMap = new Dictionary<string, Func<string[], string>>
        {
            { RequirementType.Disk, DiskHandler },
            { RequirementType.Processor, ProcessorHandler },
            { RequirementType.Ram, RAMHandler },
            { RequirementType.RegistryKeyExists, RegistryKeyExistsHandler },
            { RequirementType.Process, ProcessesHandler },
            { RequirementType.BrowserVersion, BrowserVersionHandler },
            { RequirementType.OSVersion, OSVersionHandler },
            { RequirementType.PlatformVersion, PlatformVersionHandler },
            { RequirementType.BrowserInstalled, BrowsersInstalledHandler },
            { RequirementType.BrowserDefault, BrowserDefaultHandler },
            { RequirementType.RegistryKeyValue, RegistryKeyValueHandler },
            { RequirementType.HasAdminPrivileges, HasAdminPrivilegesHandler },
            { RequirementType.UserAdmin, UserAdminHandler },
            { RequirementType.SystemType, SystemTypeHandler },
            { RequirementType.FileExists, FileExistsHandler },
            { RequirementType.ConfigValue, ConfigValueHandler }
        };

        public static bool CompareOperation(string value1, string value2,
                       CompareOperationType operation, LogicalOperatorType logicalOperatorType = LogicalOperatorType.OR)
        {
            double.TryParse(value1, out double val1);
            double.TryParse(value2, out double val2);

            bool match = operation switch
            {
                CompareOperationType.Contains => value1.Contains(value2),
                CompareOperationType.StartsWith => value1.StartsWith(value2),
                CompareOperationType.EndsWith => value1.EndsWith(value2),
                CompareOperationType.Equal => (Equals(val1, val2) && (val1 != 0 || val1 != 0)) || value1.Equals(value2),
                CompareOperationType.Greater => val1 > val2,
                CompareOperationType.GreaterEqual => val1 >= val2,
                CompareOperationType.Less => val1 < val2,
                CompareOperationType.LessEqual => val1 <= val2,
                _ => throw new ArgumentOutOfRangeException(),
            };


            if (logicalOperatorType == LogicalOperatorType.NOT)
                match = !match;
            return match;
        }

        public static bool ToBoolean(string value)
        {
            return (value.ToLower()) switch
            {
                "true" => true,
                "t" => true,
                "1" => true,
                "0" => false,
                "false" => false,
                "f" => false,
                _ => throw new InvalidCastException("You can't cast that value to a bool!"),
            };
        }

        public static string CompareLogicalOper(bool[] arrB, LogicalOperatorType operation)
        {
            string match = "false";

            switch (operation)
            {
                case LogicalOperatorType.AND:
                    if (arrB.All(x => x))
                        match = "true";
                    break;
                case LogicalOperatorType.OR:
                    if (arrB.Any(x => x))
                        match = "true";
                    break;
                case LogicalOperatorType.NOT:
                    if (arrB.All(x => !x))
                        match = "true";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return match;
        }

        public RequirementHandlers()
        {
        }

        public static string[] RequirementToArray(ProductSettings.ProductRequirement requirement)
        {
            List<string> KeyReqList = new List<string>();
            foreach (var reqKeyName in requirement.Keys)
            {
                KeyReqList.Add(reqKeyName.KeyValue);
            }

            return KeyReqList.ToArray(); //requirement.Keys.ToArray();          
        }

        public static bool IsVersionOnly(string str) => Version.TryParse(str, out Version _);

        public static bool CompareTwoVersions(string value1, string value2, CompareOperationType operation)
        {
            if (operation == CompareOperationType.Contains || operation == CompareOperationType.StartsWith || operation == CompareOperationType.EndsWith)
                return CompareOperation(value1, value2, operation);

            if (!Version.TryParse(value1, out Version ver1) || !Version.TryParse(value2, out Version ver2))
                return true;

            return operation switch
            {
                CompareOperationType.Less => ver1 < ver2,
                CompareOperationType.Greater => ver1 > ver2,
                CompareOperationType.LessEqual => ver1 <= ver2,
                CompareOperationType.GreaterEqual => ver1 >= ver2,                
                CompareOperationType.Equal => ver1 == ver2,
                _ => true
            };            
        }

        public bool HandlersResult(ref ProductSettings.ProductRequirements requirements)
        {
            bool resB = true;
            if ((requirements.RequirementList == null) || (requirements.RequirementList == null) ||
                (requirements.RequirementList.Count + requirements.RequirementsList.Count == 0))
                return resB;
            LogicalOperatorType logicaloperatorType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), requirements.LogicalOperator);
            int reqCount = requirements.RequirementList.Count + requirements.RequirementsList.Count;
            List<bool> arrBool = new List<bool>();
            foreach (var req in requirements.RequirementList)
            {
                resB = HandlersReqResult(req);
                if ((reqCount > 1) &&
                    ((resB && (logicaloperatorType == LogicalOperatorType.OR)) ||
                    (!resB && (logicaloperatorType == LogicalOperatorType.AND))))
                {
#if DEBUG
                    Logger.GetLogger().Info(reqCount + " Requirements {" + logicaloperatorType + "} => " + resB);
#endif
                    if (!resB)
                    {
                        requirements.UnfulfilledRequirementType = req.Type;
                        requirements.UnfulfilledRequirementDelta = req.Delta;
                    }
                    return resB;
                }
                arrBool.Add(resB);
            }
            foreach (ProductSettings.ProductRequirements reqs in requirements.RequirementsList)
            {
                ProductSettings.ProductRequirements reqsCopy = reqs;
                resB = HandlersResult(ref reqsCopy);
                if ((reqCount > 1) &&
                    ((resB && (logicaloperatorType == LogicalOperatorType.OR)) ||
                    (!resB && (logicaloperatorType == LogicalOperatorType.AND))))
                {
#if DEBUG
                    Logger.GetLogger().Info(reqCount + " Requirements {" + logicaloperatorType + "} => " + resB);
#endif
                    return resB;
                }
                arrBool.Add(resB);
            }
            resB = bool.Parse(CompareLogicalOper(arrBool.ToArray(), logicaloperatorType));
#if DEBUG
            Logger.GetLogger().Info(((reqCount > 1) ? reqCount.ToString() + " " : "") + "Requirements {" + logicaloperatorType + "} => " + resB);
#endif
            return resB;
        }

        public bool HandlersReqResult(ProductSettings.ProductRequirement requirement)
        {
            bool resB = false;
            try
            {
                string[] KeysArr = RequirementToArray(requirement);
                LogicalOperatorType logicaloperatorType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), requirement.LogicalOperator);
                CompareOperationType operatorType = (CompareOperationType)Enum.Parse(typeof(CompareOperationType), requirement.ValueOperator);

                string requirementType = requirement.Type.ToLower();
                string methodResult = EvalMethod(requirementType, KeysArr, logicaloperatorType);
                resB = EvalOperator(methodResult, requirementType, requirement.Value, operatorType, logicaloperatorType);
#if DEBUG
                Logger.GetLogger().Info(requirement.Type + ((KeysArr.Count() > 1) ? " {" + logicaloperatorType + "}" : "") + " (" + string.Join(", ", KeysArr) + ") <" + operatorType + "> [" + requirement.Value + "] => " + resB);
#endif
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error("Product Requirement " + requirement.Type + "failed with error message: " + e.Message);
#endif
            }

            return resB;
        }

        public string EvalRequirement(ProductSettings.ProductRequirement requirement)
        {
            string[] KeysArr = RequirementToArray(requirement);
            LogicalOperatorType logicaloperatorType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), requirement.LogicalOperator);
            string resStr = EvalMethod(requirement.Type.ToLower(), KeysArr, logicaloperatorType);
#if DEBUG
            Logger.GetLogger().Info(requirement.Type + ((KeysArr.Count() > 1) ? " {" + logicaloperatorType + "}" : "") + " (" + string.Join(", ", KeysArr) + ") => " + resStr);
#endif
            return resStr;
        }

        public string EvalMethod(string requirementType, string[] KeysArr, LogicalOperatorType logicalOperatorType)
        {
            string retStr = "";
            switch (requirementType)
            {
                case RequirementType.Disk:
                case RequirementType.Ram:
                case RequirementType.BrowserVersion:
                case RequirementType.RegistryKeyValue:
                case RequirementType.ConfigValue:
                    retStr = methodsMap[requirementType](KeysArr);
                    break;
                case RequirementType.Processor:
                case RequirementType.BrowserDefault:
                case RequirementType.OSVersion:
                case RequirementType.PlatformVersion:
                case RequirementType.SystemType:
                    retStr = methodsMap[requirementType](new string[] { });
                    break;
                case RequirementType.Process:
                case RequirementType.BrowserInstalled:
                case RequirementType.RegistryKeyExists:
                case RequirementType.HasAdminPrivileges:
                case RequirementType.UserAdmin:
                case RequirementType.FileExists:
                    List<bool> arrBool = new List<bool>();
                    foreach (var key in KeysArr)
                        arrBool.Add(ToBoolean(methodsMap[requirementType](new string[] { key })));
                    retStr = CompareLogicalOper(arrBool.ToArray(), logicalOperatorType);
                    break;
                default:
                    break;
            }
            return retStr;
        }

        private bool EvalOperator(string methodResult, string requirementType, string requirementValue, CompareOperationType operatorType, LogicalOperatorType logicalOperatorType)
        {
            bool resB = true;
            switch (requirementType)
            {
                case RequirementType.Disk:
                case RequirementType.Ram:
                case RequirementType.BrowserDefault:
                case RequirementType.SystemType:
                case RequirementType.Processor:
                case RequirementType.ConfigValue:
                    resB = CompareOperation(methodResult, requirementValue, operatorType, logicalOperatorType);
                    break;
                case RequirementType.OSVersion:
                case RequirementType.PlatformVersion:
                    resB = CompareTwoVersions(methodResult, requirementValue, operatorType);
                    break;
                case RequirementType.BrowserVersion:
                case RequirementType.RegistryKeyValue:
                    if (string.IsNullOrEmpty(methodResult))
                    {
                        resB = (LogicalOperatorType.NOT == logicalOperatorType);
                    }
                    else if (operatorType >= CompareOperationType.Contains || double.TryParse(methodResult, out _))
                    //True if it succeeds in parsing, false if it fails
                    {
                        resB = CompareOperation(methodResult, requirementValue, operatorType, logicalOperatorType);
                    }
                    else if (IsVersionOnly(methodResult) && IsVersionOnly(requirementValue))
                    {
                        resB = CompareTwoVersions(methodResult, requirementValue, operatorType);
                    }
                    else
                        resB = methodResult.Equals(requirementValue);
                    break;
                case RequirementType.Process:
                case RequirementType.BrowserInstalled:
                case RequirementType.RegistryKeyExists:
                case RequirementType.HasAdminPrivileges:
                case RequirementType.UserAdmin:
                case RequirementType.FileExists:
                    resB = ToBoolean(methodResult) == ToBoolean(requirementValue);
                    break;
                default:
                    break;
            }
            return resB;
        }

        private static string DiskHandler(string[] arg)
        {
            return (arg[0].ToLower()) switch
            {
                "freespacemb" => HardwareUtils.Instance.DiskFreeSpaceInMB().ToString(),
                "totalspacemb" => HardwareUtils.Instance.DiskTotalSpaceInMB().ToString(),
                _ => string.Empty,
            };
        }

        private static string ProcessorHandler(string[] arg)
        {
            return HardwareUtils.Instance.ProcessorSpeedInGHz();
        }

        private static string RAMHandler(string[] arg)
        {
            return (arg[0].ToLower()) switch
            {
                "totalphysicalmb" => HardwareUtils.Instance.TotalPhysicalRamInMB().ToString(),
                "availablephysicalmb" => HardwareUtils.Instance.FreePhysicalRamInMB().ToString(),
                "totalvirtualmb" => HardwareUtils.Instance.TotalVirtualRamInMB().ToString(),
                "availablelvirtualmb" => HardwareUtils.Instance.FreeVirtualRamInMB().ToString(),
                _ => string.Empty,
            };
        }

        private static string RegistryKeyExistsHandler(string[] arg)
        {
            if (arg[0] == null)
                return "false";

            return RegistryUtils.Instance.IsRegKeyExists(arg).ToString();
        }

        private static string RegistryKeyValueHandler(string[] arg)
        {
            if (arg[0] == null)
                return null;

            string strValue = RegistryUtils.Instance.GetRegKeyValue(arg);
            return strValue ?? "";
        }

        private static string ProcessesHandler(string[] arg)
        {
            Process[] localAllProcs = Process.GetProcesses();
            string[] procData = localAllProcs.Select(s => s.ProcessName).ToArray();

            if (arg[0] == null)
                return "false";

            arg[0] = arg[0].ToLower();
            procData = Array.ConvertAll(procData, d => d.ToLower());

            string extension = System.IO.Path.GetExtension(arg[0]);
            string resultFileName = arg[0].Substring(0, arg[0].Length - extension.Length);

            return (Array.IndexOf(procData, resultFileName) >= 0).ToString();
        }

        private static string BrowserVersionHandler(string[] arg) => UriUtils.Instance.GetBrowserVer(arg[0].ToLower());

        private static string OSVersionHandler(string[] arg) => HardwareUtils.Instance.OsName();

        private static string PlatformVersionHandler(string[] arg) => ResourcesUtils.libraryAssembly.GetName().Version.ToString();

        private static string BrowsersInstalledHandler(string[] arg)
        {
            switch (arg[0].ToLower())
            {
                case "ie":
                    return (UriUtils.Instance.GetIEExe() != null) ? "true" : "false";
                case "chrome":
                    return (UriUtils.Instance.GetChromeExe() != null) ? "true" : "false";
                case "firefox":
                    return (UriUtils.Instance.GetFirefoxExe() != null) ? "true" : "false";
                case "opera":
                    return (UriUtils.Instance.GetOperaEXE() != null) ? "true" : "false";
                case "edge":
                    return (UriUtils.Instance.GetEdgeExe() != null) ? "true" : "false";
                default:
#if DEBUG
                    Logger.GetLogger().Warning(arg[0].ToLower() + " browser not supported.");
#endif
                    return "false";
            }
        }

        private static string BrowserDefaultHandler(string[] arg)
        {
            return UriUtils.Instance.GetDefault();
        }

        private static string HasAdminPrivilegesHandler(string[] arg)
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
             .IsInRole(WindowsBuiltInRole.Administrator).ToString();
        }

        private static string UserAdminHandler(string[] arg)
        {
            // read all roles for the current identity name, asking ActiveDirectory
            return UserUtils.IsAdministratorNoCache(WindowsIdentity.GetCurrent().Name).ToString();
        }

        private static string SystemTypeHandler(string[] arg)
        {
            // read all roles for the current identity name, asking ActiveDirectory
            return OSUtils.Is64BitOperatingSystem()?"64":"32";
        }

        private static string FileExistsHandler(string[] arg)
        {
            if (arg[0] == null)
                return "false";
            return System.IO.File.Exists(arg[0]).ToString();
        }

        private static string ConfigValueHandler(string[] arg)
        {
            if (arg[0] == null)
                return "false";
            return arg[0];
        }
    }
}
