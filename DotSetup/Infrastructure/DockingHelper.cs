// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DotSetup
{
    class DockingHelper
    {
        private readonly uint processId, threadId;
        private readonly IntPtr targetWnd;
        private readonly Control dockedControl;
        private RECT lastKnownWindowLocation;
        // Needed to prevent the GC from sweeping up our callback
        private readonly WinEventDelegate winEventDelegate;
        private IntPtr hook;

        public DockingHelper(IntPtr wnd, Control ctl)
        {
            targetWnd = wnd;
            dockedControl = ctl;
            lastKnownWindowLocation = GetWindowLocation();

            threadId = GetWindowThreadProcessId(targetWnd, out processId);
            LogOnWin32Error("Failed to get process id");

            winEventDelegate = WhenWindowMoveStartsOrEnds;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private void LogOnWin32Error(string message)
        {
            int err = Marshal.GetLastWin32Error();
#if DEBUG
            if (err != 0)
                Logger.GetLogger().Error(message + ", LastWin32Error: " + err);
#endif
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private RECT GetWindowLocation()
        {
            GetWindowRect(targetWnd, out RECT loc);
            if (Marshal.GetLastWin32Error() != 0)
            {
                return lastKnownWindowLocation;
            }
            return loc;
        }

        private const int EVENT_SYSTEM_MOVESIZESTART = 0x000A;
        private const int EVENT_SYSTEM_MOVESIZEEND = 0x000B;
        private const int WINEVENT_OUTOFCONTEXT = 0x00000;
        public void Subscribe()
        {
            hook = SetWinEventHook(EVENT_SYSTEM_MOVESIZESTART, EVENT_SYSTEM_MOVESIZEEND, targetWnd, winEventDelegate, processId, threadId, WINEVENT_OUTOFCONTEXT);
        }

        private void PollWindowLocation()
        {
            RECT location = GetWindowLocation();
            dockedControl.PerformSafely(() => dockedControl.Width += (location.Right - location.Left) - (lastKnownWindowLocation.Right - lastKnownWindowLocation.Left));
            dockedControl.PerformSafely(() => dockedControl.Height += (location.Bottom - location.Top) - (lastKnownWindowLocation.Bottom - lastKnownWindowLocation.Top));
            lastKnownWindowLocation = location;
        }

        public void Unsubscribe()
        {
            UnhookWinEvent(hook);
        }

        private void WhenWindowMoveStartsOrEnds(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd != targetWnd) // We only want events from our target window, not other windows owned by the thread.
                return;

            if (eventType == EVENT_SYSTEM_MOVESIZEEND)
            {
                PollWindowLocation();
            }
        }

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    }
}
