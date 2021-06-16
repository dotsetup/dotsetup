// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Globalization;
using System.IO;

namespace DotSetup.Infrastructure
{
    public static class XmlProcessor
    {
        public static class Prefix { 
            public const string Encode = "encode_", 
                Decode = "decode_",
                Add = "add_",
                Substruct = "sub_"; 
        }
        public static class AddSubSuffix
        {
            public const string Number = "num",
                Years = "years",
                Months = "months",
                Days = "days";
        }

        internal static string Process(string action, string leftOp, string rightOp)
        {
            Processor processor = action.ToLower() switch
            {
                CryptUtils.Hash.SHA1 => SHA1,
                CryptUtils.Hash.MD5 => MD5,
                Prefix.Encode + CryptUtils.EncDec.BASE62 => EncodeBase62,
                Prefix.Decode + CryptUtils.EncDec.BASE62 => DecodeBase62,
                Prefix.Encode + CryptUtils.EncDec.BASE64 => EncodeBase64,
                Prefix.Decode + CryptUtils.EncDec.BASE64 => DecodeBase64, 
                Prefix.Add + AddSubSuffix.Number => AddNum,
                Prefix.Substruct + AddSubSuffix.Number => SubstructNum,
                Prefix.Add + AddSubSuffix.Years => AddYears,
                Prefix.Substruct + AddSubSuffix.Years => SubstructYears,
                Prefix.Add + AddSubSuffix.Months => AddMonths,
                Prefix.Substruct + AddSubSuffix.Months => SubstructMonths,
                Prefix.Add + AddSubSuffix.Days => AddDays,
                Prefix.Substruct + AddSubSuffix.Days => SubstructDays,
                _ => null,
            };

            return (processor == null) ? string.Empty : processor(leftOp, rightOp);            
        }

        private delegate string Processor(string leftOp, string rightOp);

        private static string SHA1(string leftOp, string rightOp) => CryptUtils.ComputeHash(leftOp, CryptUtils.Hash.SHA1);
        private static string MD5(string leftOp, string rightOp) => CryptUtils.ComputeHash(leftOp, CryptUtils.Hash.MD5);
        private static string EncodeBase62(string leftOp, string rightOp) => new StreamReader(CryptUtils.Encode(leftOp, CryptUtils.EncDec.BASE62)).ReadToEnd();
        private static string DecodeBase62(string leftOp, string rightOp) => new StreamReader(CryptUtils.Decode(leftOp, CryptUtils.EncDec.BASE62)).ReadToEnd();
        private static string EncodeBase64(string leftOp, string rightOp) => new StreamReader(CryptUtils.Encode(leftOp, CryptUtils.EncDec.BASE64)).ReadToEnd();
        private static string DecodeBase64(string leftOp, string rightOp) => new StreamReader(CryptUtils.Decode(leftOp, CryptUtils.EncDec.BASE64)).ReadToEnd();        
        private static string AddNum(string leftOp, string rightOp)
        {
            if (double.TryParse(leftOp, out double left) && double.TryParse(rightOp, out double right))
                return (left + right).ToString();
            return leftOp;
        }
        private static string SubstructNum(string leftOp, string rightOp) => AddNum(leftOp, '-' + rightOp);
        private static string AddDays(string leftOp, string rightOp)
        {
            if (DateTimeUtils.TryParseYYYYMMDD(leftOp, out DateTime left) && double.TryParse(rightOp, out double right))
                return left.AddDays(right).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return leftOp;
        }
        private static string SubstructDays(string leftOp, string rightOp) => AddDays(leftOp, '-' + rightOp);
        private static string AddMonths(string leftOp, string rightOp)
        {
            if (DateTimeUtils.TryParseYYYYMMDD(leftOp, out DateTime left) && int.TryParse(rightOp, out int right))
                return left.AddMonths(right).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return leftOp;
        }
        private static string SubstructMonths(string leftOp, string rightOp) => AddMonths(leftOp, '-' + rightOp);
        private static string AddYears(string leftOp, string rightOp)
        {
            if (DateTimeUtils.TryParseYYYYMMDD(leftOp, out DateTime left) && int.TryParse(rightOp, out int right))
                return left.AddYears(right).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            return leftOp;
        }
        private static string SubstructYears(string leftOp, string rightOp) => AddYears(leftOp, '-' + rightOp);
    }
}
