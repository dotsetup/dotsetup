// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Globalization;

namespace DotSetup.Infrastructure
{
    public static class DateTimeUtils
    {
        public static long UnixTimeMSNow => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;

        public static bool TryParseYYYYMMDD(string date, out DateTime res) => DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out res);
    }
}
