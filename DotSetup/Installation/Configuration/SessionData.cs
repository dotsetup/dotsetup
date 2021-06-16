// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Globalization;
using DotSetup.Infrastructure;

namespace DotSetup.Installation.Configuration
{
    internal class SessionData
    {
        public string this[string key]
        {
            get => GetEntry(key);
            set => SetEntry(key, value);
        }
        
        public void SetEntry(string key, string value) => _data[StripRoot(key)] = value;

        public string GetEntry(string key)
        {
            key = StripRoot(key);
            if (_data.ContainsKey(key))
                return _data[key];

            string knownEntry = GetSessionData(key);
            if (!string.IsNullOrEmpty(knownEntry))
                return knownEntry;

            return ConfigParser.GetConfig().GetStringValue(SessionDataConsts.ROOT + key);
        }

        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
        private readonly string _today = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        private readonly string _unixTimeMS = DateTimeUtils.UnixTimeMSNow.ToString();

        private string GetSessionData(string key)
        {
            return key.ToUpper() switch
            {
                SessionDataConsts.TodayYYYYMMDD => _today,
                SessionDataConsts.UNIX_TIME_MS => _unixTimeMS,
                SessionDataConsts.EDGE_EXE => UriUtils.Instance.GetEdgeExe(),
                SessionDataConsts.CHROME_EXE => UriUtils.Instance.GetChromeExe(),
                SessionDataConsts.FIREFOX_EXE => UriUtils.Instance.GetFirefoxExe(),
                SessionDataConsts.IE_EXE  => UriUtils.Instance.GetIEExe(),
                SessionDataConsts.OPERA_EXE => UriUtils.Instance.GetOperaEXE(),
                _ => string.Empty,
            };
        }
        private string StripRoot(string key) => key.StartsWith(SessionDataConsts.ROOT) ? key.Substring(SessionDataConsts.ROOT.Length) : key;        
    }
}
