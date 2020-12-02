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
            // https://en.wikipedia.org/wiki/List_of_file_signatures

            byte[] buffer;
            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
                buffer = reader.ReadBytes(bytesCount);

            var hex = BitConverter.ToString(buffer);
            return hex.Replace("-", string.Empty).ToLower();
        }

        public static bool IsRunnableFileExtension(string fileName)
        {
            string[] runnableExtensions = { ".msi", ".exe" };
            return runnableExtensions.Any(Path.GetExtension(fileName).ToLower().Equals);
        }
    }
}
