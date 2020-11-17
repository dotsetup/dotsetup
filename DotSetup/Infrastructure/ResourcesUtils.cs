// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;

namespace DotSetup
{
    public class ResourcesUtils
    {
        public static Assembly wrapperAssembly;
        public static Assembly libraryAssembly;
        public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceName)
        {
            Stream res = null;
            try
            {
                if (assembly == null)
                    assembly = Assembly.GetExecutingAssembly();
                string reasourceFileName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));

                res = assembly.GetManifestResourceStream(reasourceFileName);
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error("No resource called " + resourceName + " - " + e.Message);  
#endif
            }
            finally
            {
            }
            return res;
        }

        public static bool EmbeddedResourceExists(Assembly assembly, string resourceName)
        {
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceNames().Any(s => s.EndsWith(resourceName));
        }

        public static List<string> GetEmbeddedResourceNames(Assembly assembly, string resourceNameEnding)
        {
            List<string> res = new List<string>();
            try
            {
                if (assembly == null)
                    assembly = Assembly.GetExecutingAssembly();
                res = assembly.GetManifestResourceNames().Where(str => str.EndsWith(resourceNameEnding)).ToList();
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error("No resources that end with " + resourceNameEnding + " - " + e.Message);
#endif
            }
            finally
            {
            }
            return res;
        }

        public static bool WriteResourceToFile(string resourceName, string fileName)
        {
            bool isSuccess = false;
            Stream ttfStream = ResourcesUtils.GetEmbeddedResourceStream(ResourcesUtils.wrapperAssembly, resourceName);

            if (ttfStream != null)
            {
                Stream fileStream = File.Create(fileName);

                byte[] fileBytes = new byte[ttfStream.Length];
                int len;
                while ((len = ttfStream.Read(fileBytes, 0, fileBytes.Length)) > 0)
                    fileStream.Write(fileBytes, 0, len);

                fileStream.Close();
                isSuccess = true;
            }

            return isSuccess;
        }

        public static Dictionary<string, string> GetPropertiesResources(Assembly assembly)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            if (assembly == null)
                return res;

            foreach (Type type in assembly.GetTypes())
            {
                if (type.ToString().EndsWith("Properties.Resources"))
                {
                    string baseName = type.ToString();
                    ResourceManager MyResourceClass = new ResourceManager(baseName, assembly);
                    ResourceSet resourceSet = MyResourceClass.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);

                    foreach (DictionaryEntry entry in resourceSet)
                    {
                        if (!res.ContainsKey(entry.Key.ToString()) && entry.Value.ToString() != "System.Drawing.Bitmap")
                            res.Add(entry.Key.ToString(), entry.Value.ToString());
                    }
                }
            }
            return res;
        }

        public static Dictionary<string, string> GetCmdAsDictionary(string[] args)
        {
            Dictionary<string, string> CmdParams = new Dictionary<string, string>();
            foreach (string param in args)
            {
                string key, value = "";

                // split by Colon that is not surrounded by quotes
                Match match = Regex.Match(param, ":(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                if (match.Success)
                {
                    key = param.Substring(0, match.Index);
                    value = param.Substring(match.Index + 1);
                }
                else
                    key = param;

                key = key.ToLower();
                if (key.ElementAt(0) == '/')
                    key = key.Substring(1);

                CmdParams[key] = value;

            }
            return CmdParams;
        }

        public static bool IsPathDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            path = path.Trim();

            // if has trailing slash then it's a directory
            if (new[] { "\\", "/" }.Any(x => path.EndsWith(x)))
                return true; // ends with slash

            // if has extension then its a file; directory otherwise
            return string.IsNullOrEmpty(Path.GetExtension(path));
        }
    }
}
