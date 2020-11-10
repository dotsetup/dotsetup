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
                BrowserInstalled = "browserinstalled",
                BrowserDefault = "browserdefault",
                RegistryKeyValue = "registrykeyvalue",
                HasAdminPrivileges = "hasadminprivileges",
                UserAdmin = "useradmin",
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

        static readonly Dictionary<string, Func<string[], string>> methodsMap = new Dictionary<string, Func<string[], string>>
        {
            { RequirementType.Disk, DiskHandler },
            { RequirementType.Processor, ProcessorHandler },
            { RequirementType.Ram, RAMHandler },
            { RequirementType.RegistryKeyExists, RegistryKeyExistsHandler },
            { RequirementType.Process, ProcessesHandler },
            { RequirementType.BrowserVersion, BrowserVersionHandler },
            { RequirementType.OSVersion, OSVersionHandler },
            { RequirementType.BrowserInstalled, BrowsersInstalledHandler },
            { RequirementType.BrowserDefault, BrowserDefaultHandler },
            { RequirementType.RegistryKeyValue, RegistryKeyValueHandler },
            { RequirementType.HasAdminPrivileges, HasAdminPrivilegesHandler },
            { RequirementType.UserAdmin, UserAdminHandler },
            { RequirementType.FileExists, FileExistsHandler },
            { RequirementType.ConfigValue, ConfigValueHandler }
        };

        public static bool CompareOperation(string value1, string value2,
                       CompareOperationType operation, LogicalOperatorType logicalOperatorType = LogicalOperatorType.OR)
        {
            bool match;
            if (operation == CompareOperationType.Contains)
                match = value1.Contains(value2);
            else if (operation == CompareOperationType.StartsWith)
                match = value1.StartsWith(value2);
            else if (operation == CompareOperationType.EndsWith)
                match = value1.EndsWith(value2);
            else
            {
                var val1 = Convert.ToDouble(value1);
                var val2 = Convert.ToDouble(value2);

                switch (operation)
                {
                    case CompareOperationType.Equal:
                        match = Equals(val1, val2);
                        break;
                    case CompareOperationType.Greater:
                        match = val1 > val2;
                        break;
                    case CompareOperationType.GreaterEqual:
                        match = val1 >= val2;
                        break;
                    case CompareOperationType.Less:
                        match = val1 < val2;
                        break;
                    case CompareOperationType.LessEqual:
                        match = val1 <= val2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (logicalOperatorType == LogicalOperatorType.NOT)
                match = !match;
            return match;
        }



        public static bool ToBoolean(string value)
        {
            switch (value.ToLower())
            {
                case "true":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                default:
                    throw new InvalidCastException("You can't cast that value to a bool!");
            }
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

        public static bool IsVersionOnly(string str)
        {

            string[] partsOfString = str.Split('.');
            if ((partsOfString.Length < 2) || (partsOfString.Length > 4))
                return false;
            foreach (var strPart in partsOfString)
            {
                if (!strPart.All(Char.IsDigit))
                    return false;
            }

            return true;
        }

        public static bool CompareTwoVersions(string value1, string value2, CompareOperationType operation)
        {
            bool match = true;

            string[] val1 = value1.Split('.');
            string[] val2 = value2.Split('.');

            int min = Math.Min(val1.Length, val2.Length);

            for (int i = 0; i < min; i++)
            {
                match = CompareOperation(val1[i], val2[i], operation);
                if (!match)
                    break;
            }

            return match;
        }

        public bool HandlersResult(ProductSettings.ProductRequirements requirements)
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
                    return resB;
                }
                arrBool.Add(resB);
            }
            foreach (var reqs in requirements.RequirementsList)
            {
                resB = HandlersResult(reqs);
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
            resB = Boolean.Parse(CompareLogicalOper(arrBool.ToArray(), logicaloperatorType));
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
                case RequirementType.Processor:
                case RequirementType.ConfigValue:
                    resB = CompareOperation(methodResult, requirementValue, operatorType, logicalOperatorType);
                    break;
                case RequirementType.OSVersion:
                    int index = methodResult.IndexOf('.');
                    resB = CompareOperation(methodResult.Substring(0, index + 2), requirementValue, operatorType, logicalOperatorType);
                    break;
                case RequirementType.BrowserVersion:
                case RequirementType.RegistryKeyValue:
                    if (string.IsNullOrEmpty(methodResult))
                    {
                        resB = (LogicalOperatorType.NOT == logicalOperatorType);
                    }
                    else if (operatorType >= CompareOperationType.Contains || Double.TryParse(methodResult, out _))
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
            switch (arg[0].ToLower())
            {
                case "freespacemb":
                    return HardwareUtils.Instance.DiskFreeSpaceInMB().ToString();
                case "totalspacemb":
                    return HardwareUtils.Instance.DiskTotalSpaceInMB().ToString();
                default:
                    return "";
            }
        }


        private static string ProcessorHandler(string[] arg)
        {
            return HardwareUtils.Instance.ProcessorSpeedInGHz();
        }


        private static string RAMHandler(string[] arg)
        {
            switch (arg[0].ToLower())
            {
                case "totalphysicalmb":
                    return HardwareUtils.Instance.TotalPhysicalRamInMB().ToString();
                case "availablephysicalmb":
                    return HardwareUtils.Instance.FreePhysicalRamInMB().ToString();
                case "totalvirtualmb":
                    return HardwareUtils.Instance.TotalVirtualRamInMB().ToString();
                case "availablelvirtualmb":
                    return HardwareUtils.Instance.FreeVirtualRamInMB().ToString();
                default:
                    return "";
            }
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

        private static string BrowserVersionHandler(string[] arg)
        {
            return UriUtils.Instance.GetBrowserVer(arg[0].ToLower());
        }


        private static string OSVersionHandler(string[] arg)
        {
            return HardwareUtils.Instance.OsName();
        }


        private static string BrowsersInstalledHandler(string[] arg)
        {
            switch (arg[0].ToLower())
            {
                case "ie":
                    return ((UriUtils.Instance.GetIEExe() != null) ? "true" : "false");
                case "chrome":
                    return ((UriUtils.Instance.GetChromeExe() != null) ? "true" : "false");
                case "firefox":
                    return ((UriUtils.Instance.GetFirefoxExe() != null) ? "true" : "false");
                case "opera":
                    return ((UriUtils.Instance.GetOperaEXE() != null) ? "true" : "false");
                case "edge":
                    return ((UriUtils.Instance.GetEdgeExe() != null) ? "true" : "false");
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
