// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace DotSetup.Infrastructure
{
    internal enum RegWow64Options
    {
        None = 0,
        KEY_WOW64_64KEY = 0x0100,
        KEY_WOW64_32KEY = 0x0200
    }

    internal enum RegistryRights
    {
        ReadKey = 131097,
        WriteKey = 131078
    }

    public sealed class RegistryUtils
    {
        private static readonly RegistryUtils instance = new RegistryUtils();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        public static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int ulOptions, int samDesired, out int phkResult);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint RegEnumValueW(IntPtr hKey, uint dwIndex, StringBuilder lpValueName, ref uint lpcValueName, IntPtr lpReserved, ref uint lpType, StringBuilder lpData, ref uint lpcbData);

        static RegistryUtils()
        {
        }

        private RegistryUtils()
        {
        }

        public static RegistryUtils Instance => instance;

        private static RegistryKey OpenSubKey(RegistryKey hkey, string subKeyName, RegWow64Options options)
        {

            //Sanity check
            if (hkey == null || RegistryKeyHandle(hkey) == IntPtr.Zero)
            {
                return null;
            }

            //Set rights
            int rights = (int)RegistryRights.ReadKey;


            //Call the native function
            int subKeyHandle, result = RegOpenKeyEx(RegistryKeyHandle(hkey), subKeyName, 0, rights | (int)options, out subKeyHandle);

            //If we had an error, return null
            if (result != 0)
                return null;

            RegistryKey subKey = PointerToRegistryKey((IntPtr)subKeyHandle, false, false);
            return subKey;


        }

        // Get a pointer to a registry key. (convert RegistryKey to HKEY)
        private static IntPtr RegistryKeyHandle(RegistryKey registryKey)
        {
            //Get the type of the RegistryKey
            Type registryKeyType = typeof(RegistryKey);
            //Get the FieldInfo of the 'hkey' member of RegistryKey
            System.Reflection.FieldInfo fieldInfo =
            registryKeyType.GetField("hkey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Get the handle held by hkey
            SafeHandle handle = (SafeHandle)fieldInfo.GetValue(registryKey);
            //Get the unsafe handle
            IntPtr dangerousHandle = handle.DangerousGetHandle();
            return dangerousHandle;
        }

        private static RegistryKey PointerToRegistryKey(IntPtr hKey, bool writable, bool ownsHandle)
        {
#if NET35
            //Get the constructor for the SafeRegistryHandle
            var safeRegistryHandleType = typeof(Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
            var safeRegistryHandleConstructor = safeRegistryHandleType.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(IntPtr), typeof(bool) }, null); // .NET < 4
            if (safeRegistryHandleConstructor == null)
                safeRegistryHandleConstructor = safeRegistryHandleType.GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, new[] { typeof(IntPtr), typeof(bool) }, null); // .NET >= 4

            //Invoke the constructor, getting us a SafeRegistryHandle
            var safeHandle = safeRegistryHandleConstructor.Invoke(new object[] { hKey, ownsHandle });

            //Get the RegistryKey constructor
            var net3Constructor = typeof(RegistryKey).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { safeRegistryHandleType, typeof(bool) }, null);
            var net4Constructor = typeof(RegistryKey).GetConstructor(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(IntPtr), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }, null);

            //Invoke the constructor, getting us a RegistryKey
			object key;
            if (net3Constructor != null)
                key = net3Constructor.Invoke(new object[] { safeHandle, writable });
            else if (net4Constructor != null)
                key = net4Constructor.Invoke(new object[] { hKey, writable, false, false, false });
            else
            {
                var keyFromHandleMethod = typeof(RegistryKey).GetMethod("FromHandle", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new[] { safeRegistryHandleType }, null);
                key = keyFromHandleMethod.Invoke(null, new object[] { safeHandle });
            }
            var field = typeof(RegistryKey).GetField("keyName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(key, String.Empty);
            RegistryKey resultKey = (RegistryKey)key;
#else              

            //Get the BindingFlags for public contructors
            System.Reflection.BindingFlags publicConstructors = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public; // NOTE: this has been changed since .NET 3.5 (was 'private').
            //Get the Type for the SafeRegistryHandle
            Type safeRegistryHandleType =
                typeof(Microsoft.Win32.SafeHandles.SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
            //Get the array of types matching the args of the ctor we want
            Type[] safeRegistryHandleCtorTypes = new Type[] { typeof(IntPtr), typeof(bool) };
            //Get the constructor info for our object
            System.Reflection.ConstructorInfo safeRegistryHandleCtorInfo = safeRegistryHandleType.GetConstructor(
                publicConstructors, null, safeRegistryHandleCtorTypes, null);
            //Get the SafeRegistryHandle
            Microsoft.Win32.SafeHandles.SafeRegistryHandle safeHandle = (Microsoft.Win32.SafeHandles.SafeRegistryHandle)safeRegistryHandleCtorInfo.Invoke(new object[] { hKey, ownsHandle });
            //Get the RegistryKey
            RegistryKey resultKey = RegistryKey.FromHandle(safeHandle); // ONLY possible under .NET 4.0! In .NET 3.5 there is no .FromHandle Method available!    
#endif
            //return the resulting key
            return resultKey;
        }

        private RegistryKey GetHiveKey(string hiveKey, RegistryView view = RegistryView.Default)
        {
            RegistryHive hive = hiveKey.ToUpper() switch
            {
                "HKCU" => RegistryHive.CurrentUser,
                "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                "HKLM" => RegistryHive.LocalMachine,
                "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKU" => RegistryHive.Users,
                "HKEY_USERS" => RegistryHive.Users,
                "HKCC" => RegistryHive.CurrentConfig,
                "HKEY_CURRENT_CONFIG" => RegistryHive.CurrentConfig,
                "HKCR" => RegistryHive.ClassesRoot,
                "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
                "HKEY_PERFORMANCE_DATA" => RegistryHive.PerformanceData,
                _ => throw new ArgumentOutOfRangeException(),
            };

            RegistryKey rk = RegistryKey.OpenBaseKey(hive, view);
            return rk;
        }

        public RegistryKey GetRegKey(string hiveKey, string subKey, RegistryView view = RegistryView.Default)
        {
            using RegistryKey rk = GetHiveKey(hiveKey, view);
            RegistryKey rkRes = OpenSubKey(rk, subKey, RegWow64Options.KEY_WOW64_64KEY);
            if (rkRes == null)
            {
                rkRes = OpenSubKey(rk, subKey, RegWow64Options.KEY_WOW64_32KEY);
            }

            return rkRes;
        }

        public bool IsRegKeyExists(string path, bool allowRegex = false)
        {
            string hive = string.Empty;
            string rootSubKey = string.Empty;
            string relativeSubKey = string.Empty;

            ParseRegistryPath(path, ref hive, ref rootSubKey, ref relativeSubKey);

            RegistryKey regKey = GetRegKey(hive, rootSubKey);
            if (regKey == null)
                return false;

            return allowRegex ? regKey.GetSubKeyNames().Any(s => Regex.IsMatch(s, relativeSubKey))
                : regKey.GetSubKeyNames().Any(relativeSubKey.Contains);
        }

        private void ParseRegistryPath(string path, ref string hkey, ref string subKey)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string[] subKeys = path.Split('\\');
            hkey = subKeys[0];
            int hiveLen = hkey.Length;
            subKey = path.Substring(hiveLen + 1);
        }

        private void ParseRegistryPath(string path, ref string hive, ref string rootSubKey, ref string relativeSubKey)
        {
            ParseRegistryPath(path, ref hive, ref rootSubKey);
            string[] subKeys = rootSubKey.Split('\\');
            if (subKeys.Length > 1)
            {
                relativeSubKey = subKeys[subKeys.Length - 1];
                rootSubKey = rootSubKey.Substring(0, rootSubKey.Length - relativeSubKey.Length - 1);
            }
        }

        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_MORE_DATA = 234;
        private const uint REG_DWORD = 4;
        private const uint REG_QWORD = 11;
        public string GetRegKeyValue(string path, string subKeyName = "") //string hkey, string subKey, string subKeyName)
        {
            string hive = string.Empty;
            string subKey = string.Empty;            
            ParseRegistryPath(path, ref hive, ref subKey);
            return GetRegKeyValue(hive, subKey, subKeyName);
        }

        public string GetRegKeyValue(string hive, string subKey, string subKeyName = "", RegistryView view = RegistryView.Default)
        {
            string res = string.Empty;

            try
            {
                RegistryKey regKey = GetRegKey(hive, subKey, view);
                if (regKey == null)
                    return null;
                uint hError = ERROR_SUCCESS;
                StringBuilder valueName = new StringBuilder(1024);
                uint lpcchValueName = (uint)valueName.Capacity;
                uint dwIndex = 0;
                uint lpType = 0;
                StringBuilder lpData = new StringBuilder(1024);
                uint lpcbData = (uint)lpData.Capacity;

                while (hError == ERROR_SUCCESS)
                {
                    hError = RegEnumValueW(RegistryKeyHandle(regKey), dwIndex, valueName, ref lpcchValueName, IntPtr.Zero, ref lpType, lpData, ref lpcbData);
                    if (hError == ERROR_MORE_DATA)
                    {
                        valueName = new StringBuilder(Convert.ToInt32(lpcchValueName));
                        lpData = new StringBuilder(Convert.ToInt32(lpcbData));
                        hError = RegEnumValueW(RegistryKeyHandle(regKey), dwIndex, valueName, ref lpcchValueName, IntPtr.Zero, ref lpType, lpData, ref lpcbData);
                    }

                    if (hError == ERROR_SUCCESS && valueName.ToString() == subKeyName)
                    {
                        res = lpData.ToString();
                        if (lpType == REG_DWORD || lpType == REG_QWORD)
                            res = StringToNumberString(res);
                        break;
                    }
                    lpcchValueName = 1024;
                    lpcbData = 1024;
                    valueName = new StringBuilder(Convert.ToInt32(lpcchValueName));
                    lpData = new StringBuilder(Convert.ToInt32(lpcbData));
                    dwIndex++;
                }

                return res;
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error("Cannot get the value of the subkey - " + e);
#endif
                return null;
            }
        }

        public bool WriteRegKey(string path, string subKeyName = "", string subKeyValue = "", RegistryValueKind valueKind = RegistryValueKind.String, RegistryView view = RegistryView.Default)
        {
            try
            {
                string hive = string.Empty;
                string subKey = string.Empty;
                ParseRegistryPath(path, ref hive, ref subKey);
                using RegistryKey key = GetHiveKey(hive, view).CreateSubKey(subKey);
                if (key == null)
                    return false;

                if (!string.IsNullOrWhiteSpace(subKeyName))
                    key.SetValue(subKeyName, subKeyValue, valueKind);

                return true;
            }
#if DEBUG
            catch (Exception e)
#else
            catch (Exception)
#endif
            {
#if DEBUG
                Logger.GetLogger().Error($"Cannot write registry key in {path}, subkey {subKeyName}={subKeyValue}, error: {e}");
#endif
                return false;
            }
        }

        private string StringToNumberString(string str)
        {
            var codePoints = new List<int>(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                codePoints.Add(char.ConvertToUtf32(str, i));
                if (char.IsHighSurrogate(str[i]))
                    i += 1;
            }
            if (codePoints.Count == 1)
                return codePoints[0].ToString();
            if (codePoints.Count == 2)
                return ((long)codePoints[1] << 16 | (uint)codePoints[0]).ToString();
            else
                return str;
        }
    }
}
