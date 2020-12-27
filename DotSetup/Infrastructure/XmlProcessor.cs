// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.IO;

namespace DotSetup
{
    public class XmlProcessor
    {
        public static class Prefix { public const string Encode = "encode_", Decode = "decode_"; }

        internal static string Process(string action, string value)
        {
            return (action.ToLower()) switch
            {
                CryptUtils.Hash.SHA1 => CryptUtils.ComputeHash(value, CryptUtils.Hash.SHA1),
                CryptUtils.Hash.MD5 => CryptUtils.ComputeHash(value, CryptUtils.Hash.MD5),
                Prefix.Encode + CryptUtils.EncDec.BASE62 => new StreamReader(CryptUtils.Encode(value, CryptUtils.EncDec.BASE62)).ReadToEnd(),
                Prefix.Decode + CryptUtils.EncDec.BASE62 => new StreamReader(CryptUtils.Decode(value, CryptUtils.EncDec.BASE62)).ReadToEnd(),
                Prefix.Encode + CryptUtils.EncDec.BASE64 => new StreamReader(CryptUtils.Encode(value, CryptUtils.EncDec.BASE64)).ReadToEnd(),
                Prefix.Decode + CryptUtils.EncDec.BASE64 => new StreamReader(CryptUtils.Decode(value, CryptUtils.EncDec.BASE64)).ReadToEnd(),
                _ => string.Empty,
            };
        }
    }
}
