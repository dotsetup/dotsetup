// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotSetup
{
    internal class CryptUtils
    {
        public static class Hash { public const string SHA1 = "SHA1", MD5 = "MD5"; }
        public static class EncDec { public const string BASE64 = "base64"; }

        public static string ComputeHash(string str, string hashName)
        {
            byte[] strBytes = new UTF8Encoding().GetBytes(str);
            byte[] strHashed = HashAlgorithm.Create(hashName).ComputeHash(strBytes);
            return BitConverter.ToString(strHashed).Replace("-", String.Empty).ToLower();
        }

        public static string ComputeHash(Stream stream, string hashName)
        {
            byte[] strHashed = HashAlgorithm.Create(hashName).ComputeHash(stream);
            return BitConverter.ToString(strHashed).Replace("-", string.Empty).ToLower();
        }

        public static Stream Decode(string str, string decode)
        {
            Stream res;
            switch (decode.ToLower())
            {
                case EncDec.BASE64:
                    str = str.Replace("\r\n", string.Empty).Replace(" ", string.Empty);
                    res = new System.IO.MemoryStream(Convert.FromBase64String(str));
                    break;

                default:
                    res = new MemoryStream(Encoding.Unicode.GetBytes(str));
                    break;
            }

            return res;
        }
    }
}
