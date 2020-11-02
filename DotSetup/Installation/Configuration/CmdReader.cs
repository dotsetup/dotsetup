// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotSetup
{
    public class CmdReader
    {
        public static Dictionary<string, string> CmdParams;
        public CmdReader(string[] args)
        {
            CmdParams = ResourcesUtils.GetCmdAsDictionary(args);
            if (CmdParams.ContainsKey("log"))
            {
#if DEBUG
                if (CmdParams.ContainsKey("verbose"))
                    Logger.GetLogger().ActivateLogger(CmdParams["log"], CmdParams["verbose"]);
                else
                    Logger.GetLogger().ActivateLogger(CmdParams["log"]);
                Logger.GetLogger().Info("--- Command line args: " + String.Join(" ", args.ToArray()));
#endif
            }

        }
    }
}
