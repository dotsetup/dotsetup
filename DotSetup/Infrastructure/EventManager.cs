// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;

namespace DotSetup.Infrastructure
{
    public delegate bool InstallerEventHandler(object sender, EventArgs e);

    public class EventManager
    {
        private static readonly EventManager _eventManager = new EventManager();
        private readonly Dictionary<string, List<InstallerEventHandler>> _eventTable;

        public static EventManager GetManager()
        {
            return _eventManager;
        }

        public EventManager()
        {
            _eventTable = new Dictionary<string, List<InstallerEventHandler>>();
        }

        public void AddEvent(string eventName, InstallerEventHandler instEventHandler)
        {
            lock (_eventTable)
            {
                if (!_eventTable.ContainsKey(eventName))
                {
                    _eventTable[eventName] = new List<InstallerEventHandler>();
                }
                //add event name to the table
                _eventTable[eventName].Add(instEventHandler);
#if DEBUG
                Logger.GetLogger().Info("Adding to event " + eventName + " the method " + instEventHandler.Method.DeclaringType.Name + "." + instEventHandler.Method.Name);
#endif
            }
        }

        public void RemoveEvent(string eventName, InstallerEventHandler instEventHandler = null)
        {
            lock (_eventTable)
            {
                if (instEventHandler == null)
                {
                    _eventTable.Remove(eventName);
#if DEBUG
                    Logger.GetLogger().Info("Removing from event " + eventName + " all methods.");
#endif
                }

                else
                {
                    _eventTable[eventName].Remove(instEventHandler);
#if DEBUG
                    Logger.GetLogger().Info("Removing from event " + eventName + " the method " + instEventHandler.Method.DeclaringType.Name + "." + instEventHandler.Method.Name);
#endif
                }
            }
        }

        public int EventCount(string eventName)
        {
            int count = 0;
            if (_eventTable.ContainsKey(eventName))
                count = _eventTable[eventName].Count;
            return count;
        }

        public bool DispatchEvent(string eventName, object sender = null)
        {
            return DispatchEvent(eventName, sender, EventArgs.Empty);
        }

        public bool DispatchEvent(string eventName, object sender, EventArgs eventArgs)
        {
            bool ret = true;
            if (_eventTable.ContainsKey(eventName))
            {
                foreach (InstallerEventHandler handler in _eventTable[eventName])
                {
                    ret &= handler(sender, eventArgs);
#if DEBUG
                    if (eventArgs != null)
                        Logger.GetLogger().Info($"Dispatching event: {eventName}");
#endif
                }
            }
            return ret;
        }
    }

}
