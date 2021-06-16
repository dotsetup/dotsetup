// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using System.Linq;
using DotSetup.Infrastructure;

namespace DotSetup.Installation.Configuration
{
    public class CmdReader
    {
        public static Dictionary<string, string> CmdParams;

        private static CmdReader instance = null;

        public static CmdReader GetReader(string[] args = null)
        {
            if (instance == null && args != null)
            {
                instance = new CmdReader(args);
            }

            return instance;
        }
        private CmdReader(string[] args)
        {
            CmdParams = ResourcesUtils.GetCmdAsDictionary(args);
            if (CmdParams.ContainsKey("log"))
            {
#if DEBUG
                if (CmdParams.ContainsKey("verbose"))
                    Logger.GetLogger().ActivateLogger(CmdParams["log"], CmdParams["verbose"]);
                else
                    Logger.GetLogger().ActivateLogger(CmdParams["log"]);
                Logger.GetLogger().Info("--- Command line args: " + string.Join(" ", args.ToArray()));
#endif
            }

        }
    }
}
