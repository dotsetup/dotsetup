// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotSetup.Infrastructure
{
    public static class ProcessUtils
    {
        public static class Toolhelp32
        {
            public const uint Inherit = 0x80000000;
            public const uint SnapModule32 = 0x00000010;
            public const uint SnapAll = SnapHeapList | SnapModule | SnapProcess | SnapThread;
            public const uint SnapHeapList = 0x00000001;
            public const uint SnapProcess = 0x00000002;
            public const uint SnapThread = 0x00000004;
            public const uint SnapModule = 0x00000008;

            [DllImport("kernel32.dll")]
            private static extern bool CloseHandle(IntPtr handle);
            [DllImport("kernel32.dll")]
            private static extern IntPtr CreateToolhelp32Snapshot(uint flags, int processId);

            public static IEnumerable<T> TakeSnapshot<T>(uint flags, int id) where T : IEntry, new()
            {
                using Snapshot snap = new Snapshot(flags, id);
                for (IEntry entry = new T { }; entry.TryMoveNext(snap, out entry);)
                    yield return (T)entry;
            }

            public interface IEntry
            {
                bool TryMoveNext(Snapshot snap, out IEntry entry);
            }

            public struct Snapshot : IDisposable
            {
                void IDisposable.Dispose()
                {
                    CloseHandle(m_handle);
                }
                public Snapshot(uint flags, int processId)
                {
                    m_handle = CreateToolhelp32Snapshot(flags, processId);
                }

                private readonly IntPtr m_handle;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WinProcessEntry : Toolhelp32.IEntry
        {
            [DllImport("kernel32.dll")]
            public static extern bool Process32Next(Toolhelp32.Snapshot snap, ref WinProcessEntry entry);

            public bool TryMoveNext(Toolhelp32.Snapshot snap, out Toolhelp32.IEntry entry)
            {
                var x = new WinProcessEntry { dwSize = Marshal.SizeOf(typeof(WinProcessEntry)) };
                var b = Process32Next(snap, ref x);
                entry = x;
                return b;
            }

            public int dwSize;
            public int cntUsage;
            public int th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public int th32ModuleID;
            public int cntThreads;
            public int th32ParentProcessID;
            public int pcPriClassBase;
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string fileName;
            //byte fileName[260];
            //public const int sizeofFileName = 260;
        }
        public static Process Parent(this Process p)
        {
            try
            {
                IEnumerable<WinProcessEntry> entries = Toolhelp32.TakeSnapshot<WinProcessEntry>(Toolhelp32.SnapAll, 0);
                int parentid = entries.First(x => x.th32ProcessID == p.Id).th32ParentProcessID;
                return Process.GetProcessById(parentid);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
