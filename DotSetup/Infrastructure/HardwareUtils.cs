// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
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
        private const long BYTES_PER_MEGABYTES = 1024 * 1024; //million bytes in one megabyte 

        private readonly SYSTEM_POWER_CAPABILITIES systemPowerCapabilites;
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

            GetPwrCapabilities(out systemPowerCapabilites);
        }

        public ulong TotalPhysicalRamInMB()
        {
            return (totalPhysicalRam / BYTES_PER_MEGABYTES);
        }

        public ulong FreePhysicalRamInMB()
        {
            return (freePhysicalRam / BYTES_PER_MEGABYTES);
        }

        public ulong TotalVirtualRamInMB()
        {
            return (totalVirtualRam / BYTES_PER_MEGABYTES);
        }

        public ulong FreeVirtualRamInMB()
        {
            return (freeVirtualRam / BYTES_PER_MEGABYTES);
        }

        public long DiskTotalSpaceInMB()
        {
            return (diskTotalSpace / BYTES_PER_MEGABYTES);
        }

        public long DiskFreeSpaceInMB()
        {
            return (diskFreeSpace / BYTES_PER_MEGABYTES);
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

        [DllImport("powrprof.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        static extern bool GetPwrCapabilities(out SYSTEM_POWER_CAPABILITIES systemPowerCapabilites);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SYSTEM_POWER_CAPABILITIES
        {
            [MarshalAs(UnmanagedType.U1)]
            public bool PowerButtonPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool SleepButtonPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool LidPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS1;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS2;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS3;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS4;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemS5;
            [MarshalAs(UnmanagedType.U1)]
            public bool HiberFilePresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool FullWake;
            [MarshalAs(UnmanagedType.U1)]
            public bool VideoDimPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool ApmPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool UpsPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool ThermalControl;
            [MarshalAs(UnmanagedType.U1)]
            public bool ProcessorThrottle;
            public byte ProcessorMinThrottle;
            public byte ProcessorMaxThrottle;    // Also known as ProcessorThrottleScale before Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool FastSystemS4;   // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool Hiberboot;  // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool WakeAlarmPresent;   // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool AoAc;   // Ignore if earlier than Windows XP
            [MarshalAs(UnmanagedType.U1)]
            public bool DiskSpinDown;
            public byte HiberFileType;  // Ignore if earlier than Windows 10 (10.0.10240.0)
            [MarshalAs(UnmanagedType.U1)]
            public bool AoAcConnectivitySupported;  // Ignore if earlier than Windows 10 (10.0.10240.0)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            private readonly byte[] spare3;
            [MarshalAs(UnmanagedType.U1)]
            public bool SystemBatteriesPresent;
            [MarshalAs(UnmanagedType.U1)]
            public bool BatteriesAreShortTerm;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public BATTERY_REPORTING_SCALE[] BatteryScale;
            public SYSTEM_POWER_STATE AcOnLineWake;
            public SYSTEM_POWER_STATE SoftLidWake;
            public SYSTEM_POWER_STATE RtcWake;
            public SYSTEM_POWER_STATE MinDeviceWakeState;
            public SYSTEM_POWER_STATE DefaultLowLatencyWake;
        }

        private struct BATTERY_REPORTING_SCALE
        {
            public uint Granularity;
            public uint Capacity;
        }

        enum SYSTEM_POWER_STATE
        {
            PowerSystemUnspecified = 0,
            PowerSystemWorking = 1,
            PowerSystemSleeping1 = 2,
            PowerSystemSleeping2 = 3,
            PowerSystemSleeping3 = 4,
            PowerSystemHibernate = 5,
            PowerSystemShutdown = 6,
            PowerSystemMaximum = 7
        }

        /// <summary>
        /// Check out if there is a lid switch present in the system
        /// </summary>
        /// <returns> true if there is a lid switch </returns>
        public bool LidPresent()
        {
            return systemPowerCapabilites.LidPresent;
        }

        /// <summary>
        /// Check if the power line is offline (relevant for laptops)
        /// </summary>
        /// <returns> true if the machine's power is based on a battery</returns>
        public bool OnBattery()
        {
            return SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline;
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
