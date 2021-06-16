// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotSetup.Infrastructure
{
    public static class CryptUtils
    {
        public static class Hash { public const string SHA1 = "sha1", MD5 = "md5"; }
        public static class EncDec { public const string BASE64 = "base64", BASE62 = "base62"; }

        public static string ComputeHash(string str, string hashName)
        {
            byte[] strBytes = new UTF8Encoding().GetBytes(str);
            byte[] strHashed = HashAlgorithm.Create(hashName).ComputeHash(strBytes);
            return BitConverter.ToString(strHashed).Replace("-", string.Empty).ToLower();
        }

        public static string ComputeHash(Stream stream, string hashName)
        {
            byte[] strHashed = HashAlgorithm.Create(hashName).ComputeHash(stream);
            return BitConverter.ToString(strHashed).Replace("-", string.Empty).ToLower();
        }


        public static Stream Encode(string str, string algo)
        {
            return Encode(Encoding.UTF8.GetBytes(str), algo);
        }

        public static Stream Encode(byte[] bytes, string algo)
        {
            return algo.ToLower().Trim() switch
            {
                EncDec.BASE64 => new MemoryStream(Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes))),
                EncDec.BASE62 => new MemoryStream(ToBaseX(bytes, BASE62_ALPHABET)),
                _ => new MemoryStream(bytes),
            };
        }

        public static Stream Decode(string str, string algo)
        {
            return Decode(Encoding.UTF8.GetBytes(str), algo);
        }

        public static Stream Decode(byte[] bytes, string algo)
        {
            return algo.ToLower().Trim() switch
            {
                EncDec.BASE64 => new MemoryStream(Convert.FromBase64String(Encoding.UTF8.GetString(bytes).Replace("\r\n", string.Empty).Replace(" ", string.Empty))),
                EncDec.BASE62 => new MemoryStream(FromBaseX(bytes, BASE62_ALPHABET)),
                _ => new MemoryStream(bytes),
            };
        }

        private const string BASE62_ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private static byte[] ToBaseX(byte[] source, string alphabet)
        {
            if (source.Length == 0)
                return new byte[0];

            int baseNum = alphabet.Length, zeroes = 0, length = 0, pbegin = 0, pend = source.Length;

            while (pbegin != pend && source[pbegin] == 0) // Allocate enough space in big-endian representation.
            {
                pbegin++;
                zeroes++;
            }

            int size = (pend - pbegin) * (int)Math.Ceiling(Math.Log(256) / Math.Log(baseNum)) + 1;
            int[] byteArray = new int[size];

            while (pbegin != pend)
            {
                int carry = source[pbegin];
                int _i2 = 0;
                for (int it1 = size - 1; (carry != 0 || _i2 < length) && it1 != -1; it1--, _i2++)
                {
                    carry += 256 * byteArray[it1];
                    byteArray[it1] = carry % baseNum;
                    carry /= baseNum;
                }

                if (carry != 0)
                    throw new Exception("Non-zero carry");
                length = _i2;
                pbegin++;
            }

            // Skip leading zeroes in result
            int it2 = size - length;
            while (it2 != size && byteArray[it2] == 0)
                it2++;

            string res = new string('0', zeroes);
            for (; it2 < size; ++it2)
                res += alphabet[byteArray[it2]];

            return Encoding.UTF8.GetBytes(res);
        }

        private static byte[] FromBaseX(byte[] source, string alphabet)
        {
            if (source.Length == 0)
                return new byte[0];
            int baseNum = alphabet.Length, psz = 0, zeroes = 0, length = 0;

            while (source[psz] == 0) // Allocate enough space in big-endian representation.
            {
                zeroes++;
                psz++;
            }

            int factor = (int)Math.Ceiling(Math.Log(baseNum) / Math.Log(256));
            int size = ((source.Length - psz) * factor) + 1;

            byte[] byteArray = new byte[size]; // Process the characters.

            while (psz < source.Length)
            {
                int carry = alphabet.IndexOf(Convert.ToChar(source[psz]));
                int _i3 = 0;
                for (int it3 = size - 1; (carry != 0 || _i3 < length) && it3 != -1; it3--, _i3++)
                {
                    carry += baseNum * byteArray[it3];
                    byteArray[it3] = (byte)(carry % 256);
                    carry /= 256;
                }

                if (carry != 0)
                    throw new Exception("Non-zero carry");
                length = _i3;
                psz++;
            }

            // Skip trailing spaces
            int it4 = size - length;
            while (it4 != size && byteArray[it4] == 0)
                it4++;

            byte[] vch = new byte[zeroes + (size - it4)];
            Array.Clear(vch, 0, zeroes);

            int j = zeroes;
            while (it4 != size)
                vch[j++] = byteArray[it4++];

            return vch;
        }
    }
}
