﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace DotSetup.Infrastructure
{
    public class CommunicationUtils
    {
        public static string GetUA()
        {
            string os = Environment.OSVersion.Version.ToString();
            return $"Mozilla/5.0 (Windows NT {os.Substring(0, os.IndexOf('.', 3))}; {((Environment.Is64BitOperatingSystem) ? "WOW64; " : "")}Trident/7.0; rv:11.0) like Gecko";
        }

        public static void EnableHighestTlsVersion()
        {
            Type type = Type.GetType("System.AppContext");
            if (type != null)
            {
                MethodInfo setSwitch = type.GetMethod("SetSwitch", BindingFlags.Public | BindingFlags.Static);
                setSwitch.Invoke(null, new object[] { "Switch.System.Net.DontEnableSystemDefaultTlsVersions", false });
            }
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xF00; // Allow variety of protocols to support different clients 
        }
    }
}