// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System;
using System.Collections.Generic;

namespace DotSetup
{
    public delegate Boolean InstallerEventHandler(object sender, EventArgs e);

    public class EventManager
    {
        private static readonly EventManager _eventManager = new EventManager();
        private Dictionary<string, List<InstallerEventHandler>> eventTable;

        public static EventManager GetManager()
        {
            return _eventManager;
        }

        public EventManager()
        {
            eventTable = new Dictionary<string, List<InstallerEventHandler>>();
        }

        public void AddEvent(string eventName, InstallerEventHandler instEventHandler)
        {
            lock (eventTable)
            {
                if (!eventTable.ContainsKey(eventName))
                {
                    eventTable[eventName] = new List<InstallerEventHandler>();
                }
                //add event name to the table
                eventTable[eventName].Add(instEventHandler);
#if DEBUG
                Logger.GetLogger().Info("Adding to event " + eventName + " the method " + instEventHandler.Method.DeclaringType.Name + "." + instEventHandler.Method.Name);
#endif
            }
        }

        public void RemoveEvent(string eventName, InstallerEventHandler instEventHandler = null)
        {
            lock (eventTable)
            {
                if (instEventHandler == null)
                {
                    eventTable.Remove(eventName);
#if DEBUG
                    Logger.GetLogger().Info("Removing from event " + eventName + " all methods.");
#endif
                }

                else
                {
                    eventTable[eventName].Remove(instEventHandler);
#if DEBUG
                    Logger.GetLogger().Info("Removing from event " + eventName + " the method " + instEventHandler.Method.DeclaringType.Name + "." + instEventHandler.Method.Name);
#endif
                }
            }
        }

        public int EventCount(string eventName)
        {
            int count = 0;
            if (eventTable.ContainsKey(eventName))
                count = eventTable[eventName].Count;
            return count;
        }

        public Boolean DispatchEvent(string eventName, object sender = null)
        {
            return DispatchEvent(eventName, sender, EventArgs.Empty);
        }

        public Boolean DispatchEvent(string eventName, object sender, EventArgs eventArgs)
        {
            Boolean ret = true;
            if (eventTable.ContainsKey(eventName))
            {
                foreach (InstallerEventHandler handler in eventTable[eventName])
                {
                    ret &= handler(sender, eventArgs);
#if DEBUG
                    if (eventArgs != null)
                        Logger.GetLogger().Info("Dispatching event " + eventArgs.ToString());
#endif
                }
            }
            return ret;
        }
    }

}
