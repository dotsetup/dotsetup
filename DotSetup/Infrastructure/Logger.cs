// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DotSetup
{
    public sealed class Logger
    {
#if DEBUG
        private const string FILE_EXT = ".log";
        private readonly string datetimeFormat;
        private string logFilename;
        private int logLevel;
        private bool isActive = false;
        public static class Level { public const int LOW_DEBUG_LEVEL = 0, MEDIUM_DEBUG_LEVEL = 1, HIGH_DEBUG_LEVEL = 2, AUTOMATION_DEBUG_LEVEL = 3; }

        private static Logger _logger = null;

        public static Logger GetLogger()
        {
            if (_logger == null)
                _logger = new Logger();
            return _logger;
        }

        private Logger()
        {
            datetimeFormat = "yyyy/MM/dd HH:mm:ss:fff";
            logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + "_" + DateTime.Now.ToString("yyyyMMddHHmmfffff") + FILE_EXT;
            logLevel = Level.LOW_DEBUG_LEVEL;
        }

        public void ActivateLogger(string logFilename = "", string logLevelStr = "")
        {
            isActive = true;
            if (!string.IsNullOrEmpty(logFilename))
            {
                try
                {
                    this.logFilename = Path.GetFullPath(logFilename);
                }
                catch (Exception ex)
                {
                    Error(ex.Message);
                }
            }
            if (!String.IsNullOrEmpty(logLevelStr))
                logLevel = Convert.ToInt32(logLevelStr);
        }

        public void Debug(string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {
            WriteFormattedLog(LogType.DEBUG, text, logLevel);
        }

        public void Error(string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {
            WriteFormattedLog(LogType.ERROR, text, logLevel);
        }

        public void Fatal(string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {
            WriteFormattedLog(LogType.FATAL, text, logLevel);
        }

        public void Info(string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {
            WriteFormattedLog(LogType.INFO, text, logLevel);
        }

        public void Trace(string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {
            WriteFormattedLog(LogType.TRACE, text, logLevel);
        }

        public void Warning(string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {
            WriteFormattedLog(LogType.WARNING, text, logLevel);
        }

        private void WriteLine(string text)
        {
            try
            {
                if (isActive)
                {
                    using (var stream = GetWriteStream(logFilename, 1000))
                    {
                        using (StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8))
                        {
                            if (!string.IsNullOrEmpty(text))
                            {
                                writer.WriteLine(text);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void WriteFormattedLog(LogType level, string text, int logLevel = Level.LOW_DEBUG_LEVEL)
        {

            if (!isActive)
                return;

            if (this.logLevel < logLevel)
                return;


            string pretext = "[" + Process.GetCurrentProcess().Id.ToString().PadLeft(4, '0') + "][";
            pretext += Thread.CurrentThread.ManagedThreadId.ToString().PadLeft(4, '0') + "]\t";
            pretext += System.DateTime.Now.ToString(datetimeFormat);
            switch (level)
            {
                case LogType.TRACE:
                    pretext += "\tTRACE";
                    break;
                case LogType.INFO:
                    break;
                case LogType.DEBUG:
                    pretext += "\tDEBUG";
                    break;
                case LogType.WARNING:
                    pretext += "\tWARNING";
                    break;
                case LogType.ERROR:
                    pretext += "\tERROR";
                    break;
                case LogType.FATAL:
                    pretext += "\tFATAL ERROR";
                    break;
                default:
                    break;
            }
            StackFrame frame;

            frame = new StackTrace().GetFrame(2);
            if (frame == null)
            {
                frame = new StackTrace().GetFrame(1);
            }

            if (frame != null)
                pretext += "\t[" + (frame.GetMethod()).ReflectedType.Name + "]\t";

            WriteLine(pretext + text);

        }

        private FileStream GetWriteStream(string path, int timeoutMs)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    return File.Open(logFilename, FileMode.Append, FileAccess.Write, FileShare.Read);                    
                }
                catch (IOException)
                {
                    
                }
            }

            throw new TimeoutException($"Failed to get a write handle to {path} within {timeoutMs}ms.");
        }

        [System.Flags]
        private enum LogType
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }
#endif
    }
}
