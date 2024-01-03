// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class SystemEventsModel : ViewModel
    {
        public event Action<string> OnEventFinished;
        private ZIBridge.ModuleWrapper<SystemEvents, SystemEventsChanges> SystemEvents => Bridge.SystemEvents;

        private Dictionary<string, SystemEventSet> systemEventsList;
        public Dictionary<string, SystemEventSet> SystemEventsList => systemEventsList ??= GetSystemEvents();

        public override void Initialize()
        {
            SystemEvents.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
        }

        public override void UnInitialize()
        {
            SystemEvents.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsSessionConnected && SystemEvents.IsHandleConnected;
        }

        public string GetEventWarningLabel(string eventName)
        {
            ReturnedResultString stringValue = SystemEvents.Handle.IsEnabled(GetEventId(eventName));
            return stringValue.second;
        }

        public string GetEventDescription(string systemEventName, string eventName)
        {
            return SystemEventsList[systemEventName].Events[eventName].Description;
        }

        public Dictionary<string, EventParameter> GetParameters(string systemEventName, string eventName)
        {
            return SystemEventsList[systemEventName].Events[eventName].Parameters;
        }

        public void FireEvent(string key, string eventName, List<KeyValuePair<string, string>> parameters)
        {
            ParameterValuesList parametersList = ParameterValuesList.Alloc();

            foreach (var keyValuePair in parameters)
            {
                ParameterValuesBuilder @event = ParameterValuesBuilder.Alloc();
                @event.SetEventName(GetEventId(eventName));
                
                StringList eventValues = StringList.Alloc();
                eventValues.Append(keyValuePair.Value);
                @event.SetValues(eventValues);
                
                parametersList.Append(@event);
            }

            Task.Run(() => SystemEvents.Handle.Fire(GetEventId(eventName), parametersList,
                ProgressMonitorDisplayWithCallback.Show($"Processing {eventName}", () => OnEventFinished?.Invoke(key))));
        }
        
        private string GetEventId(string eventName)
        {
            foreach (var systemEvents in SystemEventsList.Values)
            {
                if (systemEvents.Events.ContainsKey(eventName))
                {
                    return systemEvents.Events[eventName].Name;
                }
            }

            return string.Empty;
        }

        private Dictionary<string, SystemEventSet> GetSystemEvents()
        {
            Dictionary<string, SystemEventSet> result = new Dictionary<string, SystemEventSet>();

            SystemEventSetList list = SystemEventSetList.Alloc();
            SystemEvents.Handle.GetSystemEventSets(list);

            for (uint i = 0; i < list.GetSize(); i++)
            {
                result.Add(list.Get(i).Label, new SystemEventSet()
                {
                    Name = list.Get(i).Name,
                    Label = list.Get(i).Label,
                    Description = list.Get(i).Description,
                    Events = GetSystemEventList(list.Get(i))
                });
            }

            return result;
        }

        private Dictionary<string, SystemEvent> GetSystemEventList(ml.zi.SystemEventSet set)
        {
            Dictionary<string, SystemEvent> eventList = new Dictionary<string, SystemEvent>();
            SystemEventList events = set.GetEvents();
            SystemEventSequenceList sequences = set.GetSequences();

            for (uint j = 0; j < events.GetSize(); j++)
            {
                eventList.Add(events.Get(j).Label, new SystemEvent()
                {
                    Name = events.Get(j).Name,
                    Label = events.Get(j).Label,
                    Description = events.Get(j).Description,
                    Parameters = GetEventParametersList(events.Get(j).ParameterTypes)
                });
            }

            for (uint j = 0; j < sequences.GetSize(); j++)
            {
                eventList.Add(sequences.Get(j).Label, new SystemEvent()
                {
                    Name = sequences.Get(j).Name,
                    Label = sequences.Get(j).Label,
                    Description = sequences.Get(j).Description,
                    Parameters = new Dictionary<string, EventParameter>()
                });
            }

            return eventList;
        }

        private Dictionary<string, EventParameter> GetEventParametersList(ml.zi.ParameterTypeList typeList)
        {
            if (typeList == null)
                return null;

            Dictionary<string, EventParameter> result = new Dictionary<string, EventParameter>();

            for (uint i = 0; i < typeList.GetSize(); i++)
            {
                result.Add(typeList.Get(i).Name, new EventParameter()
                {
                    Name = typeList.Get(i).Name,
                    Values = typeList.Get(i).Values.ToStringList()
                });
            }

            return result;
        }
    }
}
