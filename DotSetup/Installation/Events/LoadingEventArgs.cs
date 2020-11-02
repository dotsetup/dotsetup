// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;

namespace DotSetup
{
    public class LoadingEventArgs : EventArgs
    {
        private readonly bool started;
        public LoadingEventArgs(bool bStart)
        {
            this.started = bStart;
        }

        public override string ToString()
        {
            return this.started.ToString();
        }
    }
}
