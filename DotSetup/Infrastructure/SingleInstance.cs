// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace DotSetup
{
    public class PipeCmdEventArgs : EventArgs
    {
        private readonly string mData;
        public PipeCmdEventArgs(string _myData)
        {
            mData = _myData;
        }

        public string Data { get { return mData; } }
    }

    public delegate void PipeCmdDelegate(PipeCmdEventArgs _args);

    public class SingleInstance
    {
        private readonly string pipeName, mutexName;
        private readonly object _namedPiperServerThreadLock = new object();
        private NamedPipeServerStream _namedPipeServerStream;
        private bool _firstApplicationInstance;
        private Mutex _mutexApplication;
        public event PipeCmdDelegate OnPipeCmdEvent;

        public SingleInstance(string appID)
        {
            string formattedAppID = appID.ToUpper().Replace(" ", "_");
            formattedAppID = string.Join(String.Empty, formattedAppID.Split(Path.GetInvalidFileNameChars())); // no invalid filename characters
            formattedAppID = formattedAppID.Substring(0, Math.Min(formattedAppID.Length, 50)); // max length 50
            this.pipeName = "PIPE_" + formattedAppID;
            this.mutexName = "MUTEX_" + formattedAppID;

            // If are the first instance then we start the named pipe server listening and allow the form to load
            if (IsApplicationFirstInstance())
            {
                // Create a new pipe - it will return immediately and async wait for connections
                NamedPipeServerCreateServer();
            }
        }

        ~SingleInstance()
        {
            // Dispose the named pipe steam
            if (_namedPipeServerStream != null)
            {
                _namedPipeServerStream.Dispose();
            }
        }

        internal bool IsApplicationFirstInstance()
        {
            // Allow for multiple runs but only try and get the mutex once
            if (_mutexApplication == null)
            {
                _mutexApplication = new Mutex(true, mutexName, out _firstApplicationInstance);
                GC.KeepAlive(_mutexApplication);
            }

            return _firstApplicationInstance;
        }

        /// <summary>
        ///     Starts a new pipe server if one isn't already active.
        /// </summary>
        private void NamedPipeServerCreateServer()
        {
            // Create a new pipe accessible by local authenticated users, disallow network
            var sidNetworkService = new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null);
            var sidWorld = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            var pipeSecurity = new PipeSecurity();

            // Deny network access to the pipe
            var accessRule = new PipeAccessRule(sidNetworkService, PipeAccessRights.ReadWrite, AccessControlType.Deny);
            pipeSecurity.AddAccessRule(accessRule);

            // Alow Everyone to read/write
            accessRule = new PipeAccessRule(sidWorld, PipeAccessRights.ReadWrite, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);

            // Current user is the owner
            SecurityIdentifier sidOwner = WindowsIdentity.GetCurrent().Owner;
            if (sidOwner != null)
            {
                accessRule = new PipeAccessRule(sidOwner, PipeAccessRights.FullControl, AccessControlType.Allow);
                pipeSecurity.AddAccessRule(accessRule);
            }

            // Create pipe and start the async connection wait
            _namedPipeServerStream = new NamedPipeServerStream(
                pipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous,
                0,
                0,
                pipeSecurity);

            // Begin async wait for connections
            _namedPipeServerStream.BeginWaitForConnection(NamedPipeServerConnectionCallback, _namedPipeServerStream);
        }

        /// <summary>
        ///     The function called when a client connects to the named pipe. Note: This method is called on a non-UI thread.
        /// </summary>
        /// <param name="iAsyncResult"></param>
        private void NamedPipeServerConnectionCallback(IAsyncResult iAsyncResult)
        {
            try
            {
                // End waiting for the connection
                _namedPipeServerStream.EndWaitForConnection(iAsyncResult);

                // Could not create handle - server probably not running
                if (!_namedPipeServerStream.IsConnected)
                    return;

                // Read data and prevent access to _namedPipeXmlPayload during threaded operations
                lock (_namedPiperServerThreadLock)
                {
                    // Read data from client                   
                    StreamReader reader = new StreamReader(_namedPipeServerStream);
                    while (!reader.EndOfStream)
                    {
                        string namedPipeServerData = reader.ReadLine();
                        // raise event
                        OnPipeCmdEvent?.Invoke(new PipeCmdEventArgs(namedPipeServerData));
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // EndWaitForConnection will exception when someone closes the pipe before connection made
                // In that case we dont create any more pipes and just return
                // This will happen when app is closing and our pipe is closed/disposed
                return;
            }
#if DEBUG
            catch (Exception e)
            {
                Logger.GetLogger().Error("Exception caught on single instance pipe server's listener: " + e.Message);
            }
#endif
            finally
            {
                // Close the original pipe (we will create a new one each time)
                _namedPipeServerStream.Dispose();
            }

            // Create a new pipe for next connection
            NamedPipeServerCreateServer();
        }

        /// <summary>
        ///     Uses a named pipe to send the currently parsed options to an already running instance.
        /// </summary>
        /// <param name="namedPipePayload"></param>
        public bool NamedPipeClientSendCmd(string cmd)
        {
            try
            {
                using (var namedPipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
                {
                    namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds
                    namedPipeClientStream.Write(Encoding.UTF8.GetBytes(cmd), 0, cmd.Length);
                }
                return true;
            }
            catch (Exception)
            {
                // Error connecting or sending
                return false;
            }
        }
    }
}
