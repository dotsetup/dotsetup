// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotSetup
{
    class Exports
    {
        public enum ExportedFunctions
        {
            Init = 1,
            SetWindow,
            Show,
            Hide,
            Accept,
            Decline,
            Install,
            GetProgress,
            InstallationSuccess,
            Finalize
        }

#if WithExports
        [DllExport]
#endif
        public static int Invoke(int functionNum, string parameters)
        {
            int res = 0;

            if (!IsExportInRange(functionNum))
            {
#if DEBUG
                Logger.GetLogger().Error("Function number " + functionNum.ToString() + " is not supported");
#endif
                return res;
            }

            ExportedFunctions func = (ExportedFunctions)functionNum;
#if DEBUG
            Logger.GetLogger().Info("Invoke " + func.ToString() + " parameters: " + parameters);
#endif
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            Dictionary<string, object> parametersDict = new Dictionary<string, object>(JSONParser.JsonToDictionary(parameters), comparer);

            switch (func)
            {
                case ExportedFunctions.Init:
                    {
                        if (!parametersDict.ContainsKey("AccountID"))
                            break;
                        res = DotSetupSDK.Init(parametersDict["AccountID"].ToString());
                        break;
                    }
                case ExportedFunctions.SetWindow:
                    {
                        if (!parametersDict.ContainsKey("Wnd") ||
                            !parametersDict.ContainsKey("X") ||
                            !parametersDict.ContainsKey("Y") ||
                            !parametersDict.ContainsKey("Width") ||
                            !parametersDict.ContainsKey("Height"))
                            break;

                        try
                        {
                            res = DotSetupSDK.SetWindow((IntPtr)int.Parse(parametersDict["Wnd"].ToString()),
                                int.Parse(parametersDict["X"].ToString()),
                                int.Parse(parametersDict["Y"].ToString()),
                                int.Parse(parametersDict["Width"].ToString()),
                                int.Parse(parametersDict["Height"].ToString()));
                        }
                        catch (FormatException)
                        {
                            res = 0;
                        }
                        break;
                    }
                case ExportedFunctions.Show:
                    {
                        res = DotSetupSDK.Show();
                        break;
                    }
                case ExportedFunctions.Hide:
                    {
                        res = DotSetupSDK.Hide();
                        break;
                    }
                case ExportedFunctions.Accept:
                    {
                        res = DotSetupSDK.Accept();
                        break;
                    }
                case ExportedFunctions.Decline:
                    {
                        res = DotSetupSDK.Decline();
                        break;
                    }
                case ExportedFunctions.Install:
                    {
                        res = DotSetupSDK.Install();
                        break;
                    }
                case ExportedFunctions.GetProgress:
                    {
                        res = DotSetupSDK.GetProgress();
                        break;
                    }
                case ExportedFunctions.InstallationSuccess:
                    {
                        res = DotSetupSDK.InstallationSuccess();
                        break;
                    }
                case ExportedFunctions.Finalize:
                    {
                        res = DotSetupSDK.Finalize();
                        break;
                    }
                default:
#if DEBUG
                    Logger.GetLogger().Info("Unknown function, no support for function number: " + functionNum.ToString());
#endif
                    break;
            }

#if DEBUG
            Logger.GetLogger().Info("Leaving " + func.ToString() + ", result: " + res.ToString());
#endif
            return res;
        }

        private static bool IsExportInRange(int value)
        {
            var values = Enum.GetValues(typeof(ExportedFunctions)).Cast<int>().OrderBy(x => x);

            return value >= values.First() && value <= values.Last();
        }

    }
}
