// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using System.Linq;

namespace DotSetup.Infrastructure
{
    public sealed class FileUtils
    {
        private static readonly FileUtils instance = new FileUtils();

        static FileUtils()
        {
        }

        private FileUtils()
        {
        }

        public static FileUtils Instance => instance;

        public static string GetMagicNumbers(string filepath, int bytesCount)
        {
            using var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            return GetMagicNumbers(fs, bytesCount);
        }

        public static string GetMagicNumbers(FileStream fs, int bytesCount)
        {
            // https://en.wikipedia.org/wiki/List_of_file_signatures
            byte[] buffer;
            using (var reader = new BinaryReader(fs))
                buffer = reader.ReadBytes(bytesCount);

            var hex = BitConverter.ToString(buffer);
            return hex.Replace("-", string.Empty).ToLower();
        }

        public static bool IsRunnableFileExtension(string fileName) =>
            new string[]{ ".msi", ".exe" }.Any(Path.GetExtension(fileName).ToLower().Equals);


        public static bool IsRunnableFile(string fileName)
        {
            bool res = false;
            FileStream fs = null;


            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                if (IsRunnableFileExtension(fileName) ||
                    GetMagicNumbers(fs, 2) == "4d5a" || //MZ = exe
                    GetMagicNumbers(fs, 8) == "d0cf11e0a1b11ae1")  //msi
                    res = true;
            }
            catch (Exception)
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            return res;
        }
    }
}
