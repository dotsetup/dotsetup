// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;

namespace DotSetup
{
    public sealed class HardwareUtils
    {
        private static readonly HardwareUtils instance = new HardwareUtils();
        private readonly ulong totalPhysicalRam;
        private readonly ulong freePhysicalRam;
        private readonly ulong totalVirtualRam;
        private readonly ulong freeVirtualRam;

        private readonly string osName;
        private readonly long diskTotalSpace = 0;
        private readonly long diskFreeSpace = 0;
        private const long bytesPerMegabyte = 1024 * 1024; //million bytes in one megabyte 
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static HardwareUtils()
        {
        }

        private HardwareUtils()
        {
            ComputerInfo myCompInfo = new ComputerInfo();
            totalPhysicalRam = myCompInfo.TotalPhysicalMemory;
            freePhysicalRam = myCompInfo.AvailablePhysicalMemory;
            totalVirtualRam = myCompInfo.TotalVirtualMemory;
            freeVirtualRam = myCompInfo.AvailableVirtualMemory;
            osName = myCompInfo.OSVersion;

            DriveInfo myDriveInfo = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
            if (myDriveInfo.IsReady)
            {
                diskTotalSpace = myDriveInfo.TotalSize;
                diskFreeSpace = myDriveInfo.AvailableFreeSpace;
            }
        }

        public ulong TotalPhysicalRamInMB()
        {
            return (totalPhysicalRam / bytesPerMegabyte);
        }

        public ulong FreePhysicalRamInMB()
        {
            return (freePhysicalRam / bytesPerMegabyte);
        }

        public ulong TotalVirtualRamInMB()
        {
            return (totalVirtualRam / bytesPerMegabyte);
        }

        public ulong FreeVirtualRamInMB()
        {
            return (freeVirtualRam / bytesPerMegabyte);
        }

        public long DiskTotalSpaceInMB()
        {
            return (diskTotalSpace / bytesPerMegabyte);
        }

        public long DiskFreeSpaceInMB()
        {
            return (diskFreeSpace / bytesPerMegabyte);
        }

        public string OsName()
        {
            return osName;
        }


        public string ProcessorSpeedInGHz()
        {
            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);   //This registry entry contains entry for processor info.
            string freq = "";
            char[] charsToTrim = { ' ', 'G', 'H', 'z' };
            if (processor_name != null)
            {
                if (processor_name.GetValue("ProcessorNameString") != null)
                {
                    string value = processor_name.GetValue("ProcessorNameString").ToString();
                    freq = value.Split('@')[1];

                }
            }
            return freq.Trim(charsToTrim);

        }

        public long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return (drive.AvailableFreeSpace / (1024 * 1024));
                }
            }
            return -1;
        }

        public static HardwareUtils Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
