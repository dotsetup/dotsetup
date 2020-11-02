// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup
{
    public static class ErrorConsts
    {
        public const int
        // --- ERROR CODES
        ERR_NONE = 0, // no error - normal PackageState
        // permanent errors
        ERR_DOWNLOAD_GENERAL = -1, // general download error
        ERR_EXTRACT_GENERAL = -2, // general extract error
        ERR_RUN_GENERAL = -3, // general run error
        ERR_UNHANDLED = -95, // unknown error
        ERR_MAIN_THREAD_FAIL = -96, // main installer thread has failed
        ERR_PKG_PROCESS_FAIL = -97, // package internal processing failed

        ERR_ARCHIVE_CORRUPTED = -101,
        ERR_NEED_ADMIN = -102,
        ERR_EXE_NOT_FOUND = -103,

        // recoverable errors:
        ERR_RECOVERABLE = -200, // used to identify recoverable errors (<ERR_RECOVERABLE)
        ERR_DISCONNECTED = -201,
        ERR_DISK_SPACE = -202,
        ERR_FILE_LOCKED = -203,
        ERR_EXE_RUNNING = -204,
        ERR_ILLEGAL_DIR = -205,

        ERR_MAIN_EXCEPT = -400;
    }
}
