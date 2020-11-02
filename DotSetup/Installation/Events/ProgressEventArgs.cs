// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;

namespace DotSetup
{
    class ProgressEventArgs : EventArgs
    {
        public static class State { public const int Error = -1, Init = 0, Download = 1, Extract = 2, Run = 3, Done = 4; }
        internal int state;
        internal string errorMessage;
        internal int downloadPercentage;
        internal long bytesReceived;
        internal long totalBytes;
        internal double dwnldSpeed;

        public ProgressEventArgs(string errorMessage, int downloadPercentage, long bytesReceived, long totalBytes, double dwnldSpeed)
        {
            if (errorMessage != "")
            {
                this.errorMessage = errorMessage;
                state = State.Error;
            }
            else
            {
                if (downloadPercentage == 100)
                    state = State.Done;
                else
                    state = State.Download;

                this.downloadPercentage = downloadPercentage;
                this.bytesReceived = bytesReceived;
                this.totalBytes = totalBytes;
                this.dwnldSpeed = dwnldSpeed;
            }

        }

        public ProgressEventArgs(string errorMessage)
        {
            state = State.Error;
            this.errorMessage = errorMessage;
        }

        public override string ToString()
        {
            return "PackageState: " + state + ", " +
                (String.IsNullOrEmpty(errorMessage) ? "" : "errorMessage: " + errorMessage + ", ") +
                "downloadPercentage: " + downloadPercentage + ", " +
                "bytesReceived: " + bytesReceived + ", " +
                "totalBytes: " + totalBytes;
        }
    }
}
