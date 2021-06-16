// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using DotSetup.Infrastructure;
using static DotSetup.Installation.Configuration.ProductSettings;

namespace DotSetup.Installation.Packages
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

        private static readonly Dictionary<string, Func<List<RequirementKey>, string>> methodsMap = new Dictionary<string, Func<List<RequirementKey>, string>>
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
            bool val1Parsed = double.TryParse(value1, out double val1);
            bool val2Parsed = double.TryParse(value2, out double val2);

            bool match = operation switch
            {
                CompareOperationType.Contains => value1.Contains(value2),
                CompareOperationType.StartsWith => value1.StartsWith(value2),
                CompareOperationType.EndsWith => value1.EndsWith(value2),
                CompareOperationType.Equal => (val1Parsed && val2Parsed && Equals(val1, val2)) || value1.Equals(value2),
                CompareOperationType.Greater => val1Parsed && val2Parsed && val1 > val2,
                CompareOperationType.GreaterEqual => val1Parsed && val2Parsed && val1 >= val2,
                CompareOperationType.Less => val1Parsed && val2Parsed && val1 < val2,
                CompareOperationType.LessEqual => val1Parsed && val2Parsed && val1 <= val2,
                _ => throw new ArgumentOutOfRangeException(),
            };


            if (logicalOperatorType == LogicalOperatorType.NOT)
                match = !match;
            return match;
        }

        public static bool ToBoolean(string value)
        {
            return value.ToLower() switch
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

        public static string[] KeysToArrayOfValues(List<RequirementKey> keys)
        {
            List<string> KeyReqList = new List<string>();
            foreach (var reqKeyName in keys)
            {
                KeyReqList.Add(reqKeyName.Value);
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

        public bool HandlersResult(ref ProductRequirements requirements)
        {
            bool resB = true;
            if (requirements.RequirementList == null || requirements.RequirementsList == null ||
                requirements.RequirementList.Count + requirements.RequirementsList.Count == 0)
                return resB;
            LogicalOperatorType logicaloperatorType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), requirements.LogicalOperator);
            int reqCount = requirements.RequirementList.Count + requirements.RequirementsList.Count;
            List<bool> arrBool = new List<bool>();
            foreach (var req in requirements.RequirementList)
            {
                resB = HandlersReqResult(req);

                // if resB is false, the requirement can still pass, but in case it won't, we want to document the "guilty" here 
                if (!resB)
                {
                    requirements.UnfulfilledRequirementType = req.Type;
                    requirements.UnfulfilledRequirementDelta = req.Delta;
                }

                if (reqCount > 1 &&
                    (resB && logicaloperatorType == LogicalOperatorType.OR ||
                    !resB && logicaloperatorType == LogicalOperatorType.AND))
                {
#if DEBUG
                    Logger.GetLogger().Info(reqCount + " Requirements {" + logicaloperatorType + "} => " + resB);
#endif                    
                    return resB;
                }
                arrBool.Add(resB);
            }
            foreach (ProductRequirements reqs in requirements.RequirementsList)
            {
                ProductRequirements reqsCopy = reqs;
                resB = HandlersResult(ref reqsCopy);
                
                if (!resB)
                {
                    requirements.UnfulfilledRequirementType = reqsCopy.UnfulfilledRequirementType;
                    requirements.UnfulfilledRequirementDelta = reqsCopy.UnfulfilledRequirementDelta;
                }

                if (reqCount > 1 &&
                    (resB && logicaloperatorType == LogicalOperatorType.OR ||
                    !resB && logicaloperatorType == LogicalOperatorType.AND))
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
            Logger.GetLogger().Info((reqCount > 1 ? reqCount.ToString() + " " : "") + "Requirements {" + logicaloperatorType + "} => " + resB);
#endif
            return resB;
        }

        public bool HandlersReqResult(ProductRequirement requirement)
        {
            bool resB = false;
            try
            {
                LogicalOperatorType logicalOperatorType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), requirement.LogicalOperator);
                CompareOperationType operatorType = (CompareOperationType)Enum.Parse(typeof(CompareOperationType), requirement.ValueOperator);

                string requirementType = requirement.Type.ToLower();
                string methodResult = EvalMethod(requirementType, requirement.Keys, logicalOperatorType);
                resB = EvalOperator(methodResult, requirementType, requirement.Value, operatorType, logicalOperatorType);
#if DEBUG
                Logger.GetLogger().Info
                    ($"{requirement.Type}{(requirement.Keys.Count() > 1 ? $" {{{logicalOperatorType}}}" : string.Empty)} ({string.Join(", ", KeysToArrayOfValues(requirement.Keys))}){(string.IsNullOrWhiteSpace(methodResult) ? string.Empty : $" = [{methodResult}]")} <{operatorType}> [{requirement.Value}] => {resB}");
#endif
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"Product Requirement {requirement.Type} failed with error message: {e}");
#endif
            }

            return resB;
        }

        public string EvalRequirement(ProductRequirement requirement)
        {
            LogicalOperatorType logicaloperatorType = (LogicalOperatorType)Enum.Parse(typeof(LogicalOperatorType), requirement.LogicalOperator);
            string resStr = EvalMethod(requirement.Type.ToLower(), requirement.Keys, logicaloperatorType);
#if DEBUG
            Logger.GetLogger().Info(requirement.Type + (requirement.Keys.Count() > 1 ? " {" + logicaloperatorType + "}" : string.Empty) + " (" + string.Join(", ", KeysToArrayOfValues(requirement.Keys)) + ") => " + resStr);
#endif
            return resStr;
        }

        public string EvalMethod(string requirementType, List<RequirementKey> keys, LogicalOperatorType logicalOperatorType)
        {
            string retStr = "";
            switch (requirementType)
            {
                case RequirementType.Disk:
                case RequirementType.Ram:
                case RequirementType.BrowserVersion:
                case RequirementType.RegistryKeyValue:
                case RequirementType.ConfigValue:
                case RequirementType.Processor:
                case RequirementType.BrowserDefault:
                case RequirementType.OSVersion:
                case RequirementType.PlatformVersion:
                case RequirementType.SystemType:
                    retStr = methodsMap[requirementType](keys);
                    break;
                case RequirementType.Process:
                case RequirementType.BrowserInstalled:
                case RequirementType.RegistryKeyExists:
                case RequirementType.HasAdminPrivileges:
                case RequirementType.UserAdmin:
                case RequirementType.FileExists:
                    List<bool> arrBool = new List<bool>();
                    foreach (RequirementKey key in keys)
                        arrBool.Add(ToBoolean(methodsMap[requirementType](new List<RequirementKey> { key })));
                    retStr = CompareLogicalOper(arrBool.ToArray(), logicalOperatorType);
                    break;
                default:
#if DEBUG
                    Logger.GetLogger().Warning($"Unknown requirement type: {requirementType}. Will be considered as TRUE...");
#endif
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
                        resB = LogicalOperatorType.NOT == logicalOperatorType;
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

        private static string DiskHandler(List<RequirementKey> keys)
        {
            return keys.FirstOrDefault().Value.ToLower() switch
            {
                "freespacemb" => HardwareUtils.Instance.DiskFreeSpaceInMB().ToString(),
                "totalspacemb" => HardwareUtils.Instance.DiskTotalSpaceInMB().ToString(),
                _ => string.Empty,
            };
        }

        private static string ProcessorHandler(List<RequirementKey> _)
        {
            return HardwareUtils.Instance.ProcessorSpeedInGHz();
        }

        private static string RAMHandler(List<RequirementKey> keys)
        {
            return keys.FirstOrDefault().Value.ToLower() switch
            {
                "totalphysicalmb" => HardwareUtils.Instance.TotalPhysicalRamInMB().ToString(),
                "availablephysicalmb" => HardwareUtils.Instance.FreePhysicalRamInMB().ToString(),
                "totalvirtualmb" => HardwareUtils.Instance.TotalVirtualRamInMB().ToString(),
                "availablelvirtualmb" => HardwareUtils.Instance.FreeVirtualRamInMB().ToString(),
                _ => string.Empty,
            };
        }

        private static string RegistryKeyExistsHandler(List<RequirementKey> keys)
        {
            if (string.IsNullOrWhiteSpace(keys.FirstOrDefault().Value))
                return bool.FalseString;

            bool allowRegex = keys.FirstOrDefault().Type.ToLower() == "regex";

            return RegistryUtils.Instance.IsRegKeyExists(keys.FirstOrDefault().Value, allowRegex).ToString();
        }

        private static string RegistryKeyValueHandler(List<RequirementKey> keys)
        {
            if (string.IsNullOrWhiteSpace(keys.FirstOrDefault().Value))
                return null;

            string path = string.Empty;
            string subKeyName = string.Empty;

            foreach (RequirementKey key in keys)
            {
                switch (key.Type.ToLower())
                {
                    case "path":
                        path = key.Value;
                        break;
                    case "value":
                        subKeyName = key.Value;
                        break;
                    default:
                        break;
                }
            }

            return RegistryUtils.Instance.GetRegKeyValue(path, subKeyName);
        }

        private static string ProcessesHandler(List<RequirementKey> keys)
        {
            if (string.IsNullOrWhiteSpace(keys.FirstOrDefault().Value))
                return "false";

            Process[] localAllProcs = Process.GetProcesses();
            string[] procData = localAllProcs.Select(s => s.ProcessName).ToArray();            

            string proccessName = keys.FirstOrDefault().Value.ToLower();
            procData = Array.ConvertAll(procData, d => d.ToLower());

            string extension = Path.GetExtension(proccessName);
            string resultFileName = proccessName.Substring(0, proccessName.Length - extension.Length);

            return (Array.IndexOf(procData, resultFileName) >= 0).ToString();
        }

        private static string BrowserVersionHandler(List<RequirementKey> keys) => UriUtils.Instance.GetBrowserVer(keys.FirstOrDefault().Value.ToLower());

        private static string OSVersionHandler(List<RequirementKey> _) => HardwareUtils.Instance.OsName();

        private static string PlatformVersionHandler(List<RequirementKey> _) => AssemblyUtils.GetFileVersion(ResourcesUtils.libraryAssembly);

        private static string BrowsersInstalledHandler(List<RequirementKey> keys)
        {
            bool res;
            switch (keys.FirstOrDefault().Value.ToLower())
            {
                case "ie":
                    res = UriUtils.Instance.GetIEExe() != null;
                    break;
                case "chrome":
                    res = UriUtils.Instance.GetChromeExe() != null;
                    break;
                case "firefox":
                    res = UriUtils.Instance.GetFirefoxExe() != null;
                    break;
                case "opera":
                    res = UriUtils.Instance.GetOperaEXE() != null;
                    break;
                case "edge":
                    res = UriUtils.Instance.GetEdgeExe() != null;
                    break;
                default:
#if DEBUG
                    Logger.GetLogger().Warning($"{keys.FirstOrDefault().Value} browser not supported");
#endif
                    res = false;
                    break;
            }

            return res.ToString().ToLower();
        }

        private static string BrowserDefaultHandler(List<RequirementKey> _) => UriUtils.Instance.GetDefault();

        private static string HasAdminPrivilegesHandler(List<RequirementKey> _)
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
             .IsInRole(WindowsBuiltInRole.Administrator).ToString();
        }

        private static string UserAdminHandler(List<RequirementKey> _)
        {
            // read all roles for the current identity name, asking ActiveDirectory
            return UserUtils.IsAdministratorNoCache(WindowsIdentity.GetCurrent().Name).ToString();
        }

        private static string SystemTypeHandler(List<RequirementKey> _)
        {
            // read all roles for the current identity name, asking ActiveDirectory
            return OSUtils.Is64BitOperatingSystem() ? "64" : "32";
        }

        private static string FileExistsHandler(List<RequirementKey> keys)
        {
            if (string.IsNullOrWhiteSpace(keys.FirstOrDefault().Value))
                return "false";
            return File.Exists(keys.FirstOrDefault().Value).ToString().ToLower();
        }

        private static string ConfigValueHandler(List<RequirementKey> keys)
        {
            if (string.IsNullOrWhiteSpace(keys.FirstOrDefault().Value))
                return "false";
            return keys.FirstOrDefault().Value;
        }
    }
}
