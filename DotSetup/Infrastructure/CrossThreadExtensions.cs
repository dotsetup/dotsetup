// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Windows.Forms;

namespace DotSetup
{
    public static class CrossThreadExtensions
    {
        public static void PerformSafely(this Control target, Action action)
        {
            if (target.InvokeRequired)
            {
                target.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        public static void PerformSafely<T1>(this Control target, Action<T1> action, T1 parameter)
        {
            if (target.InvokeRequired)
            {
                target.BeginInvoke(action, parameter);
            }
            else
            {
                action(parameter);
            }
        }

        public static void PerformSafely<T1, T2>(this Control target, Action<T1, T2> action, T1 p1, T2 p2)
        {
            if (target.InvokeRequired)
            {
                target.BeginInvoke(action, p1, p2);
            }
            else
            {
                action(p1, p2);
            }
        }
    }
}
