﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public static class EventHelper
    {
        public static void RemoveWpfEventHandlers(object o)
        {
            RemovePropertyChangeEventHandlers(o); // routed events
            RemoveDependencyPropertyEventHandlers(o); // dpd.RemoveValueChanged
        }

        private static Dictionary<Type, DependencyPropertyDescriptor[]> _dpdOfType = new Dictionary<Type, DependencyPropertyDescriptor[]>();

        // RemovePropertyChangeEventHandlers
        private static FieldInfo _fiEntriesOfEventHandlersStore;
        private static PropertyInfo _piCountOfEntries;
        private static MethodInfo _miGetKeyValuePairOfEntries;
        private static Type _globalEventManagerType;
        private static FieldInfo _globalIndexToEventMapOfGlobalEventManager;
        private static ArrayList _globalIndexOfEvents;
        private static MethodInfo _miEventHandlersStoreRemove = typeof(UIElement).GetMethod("EventHandlersStoreRemove", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo _miToArrayOfFrugalObjectList;

        // RemoveDependencyPropertyEventHandlers
        private static PropertyInfo _piPropertyOfDpd = typeof(DependencyPropertyDescriptor).GetProperty("Property", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _fiTrackersOfProperty = null;
        private static FieldInfo _fiChangedHandlerOfTrackers = null;

        public static void RemovePropertyChangeEventHandlers(object o)
        {
            if (o == null) return;
            var piEventHandlersStore = o.GetType().GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (piEventHandlersStore == null) return;
            var eventHandlersStore = piEventHandlersStore.GetValue(o, null);
            if (eventHandlersStore == null) return;

            if (_fiEntriesOfEventHandlersStore == null)
                _fiEntriesOfEventHandlersStore = eventHandlersStore.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
            var entries = _fiEntriesOfEventHandlersStore.GetValue(eventHandlersStore);

            if (_piCountOfEntries == null)
                _piCountOfEntries= entries.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            var count = (int)_piCountOfEntries.GetValue(entries);

            if(_miGetKeyValuePairOfEntries == null)
                _miGetKeyValuePairOfEntries = entries.GetType().GetMethod("GetKeyValuePair", BindingFlags.Public | BindingFlags.Instance);
            var values = new List<Tuple<int, object>>();
            for (var k = 0; k < count; k++)
            {
                var args = new object[] { k, null, null };
                _miGetKeyValuePairOfEntries.Invoke(entries, args);
                values.Add(new Tuple<int, object>((int)args[1], args[2]));
            }

            foreach (var a1 in values)
            {
                if (a1.Item2.GetType().Name == "FrugalObjectList`1")
                {
                    var eventPrivateKey = GetEventByGlobalIndex(a1.Item1);
                    if (eventPrivateKey is RoutedEvent routedEvent && o is UIElement uiElement)
                    {
                        if (routedEvent.Name != "Unloaded")
                        {
                            if (_miToArrayOfFrugalObjectList == null)
                                _miToArrayOfFrugalObjectList = a1.Item2.GetType()
                                    .GetMethod("ToArray", BindingFlags.Instance | BindingFlags.Public);
                            var handlerInfos =
                                _miToArrayOfFrugalObjectList.Invoke(a1.Item2, null) as RoutedEventHandlerInfo[];
                            foreach (var handlerInfo in handlerInfos)
                            {
                                // Debug.Print($"RemovePropertyChangeEventHandlers. {o.GetType().Name}, {(o is FrameworkElement fe ? fe.Name : null)}, {routedEvent.Name}");
                                uiElement.RemoveHandler(routedEvent, handlerInfo.Handler);
                            }
                        }
                    }
                    else
                        throw new NotImplementedException($"RemovePropertyChangeEventHandlers not implemented yet for FrugalObjectList where EventPrivateKey is " + $" {eventPrivateKey.GetType().Name}");
                }
                else if (a1.Item2 is Delegate _delegate)
                {
                    var eventPrivateKey = GetEventByGlobalIndex(a1.Item1);
                    if (eventPrivateKey != null)
                    {
                        // Debug.Print($"RemovePropertyChangeEventHandlers2. {o.GetType().Name}, {(o is FrameworkElement fe ? fe.Name : null)}, {_delegate.Method.Name}");
                        _miEventHandlersStoreRemove.Invoke(o, new[] {eventPrivateKey, a1.Item2});
                    }
                }
                else
                    throw new NotImplementedException($"RemovePropertyChangeEventHandlers not implemented yet for {a1.Item2.GetType().Name}");
            }
        }

        private static object GetEventByGlobalIndex(int globalIndex)
        {
            if (_globalIndexOfEvents == null || _globalIndexOfEvents.Count < globalIndex)
            {
                if (_globalEventManagerType == null)
                    _globalEventManagerType = Tips.TryGetType("System.Windows.GlobalEventManager");
                if (_globalEventManagerType == null) return null;

                if (_globalIndexToEventMapOfGlobalEventManager == null)
                    _globalIndexToEventMapOfGlobalEventManager = _globalEventManagerType.GetField("_globalIndexToEventMap", BindingFlags.Static | BindingFlags.NonPublic);
                _globalIndexOfEvents = _globalIndexToEventMapOfGlobalEventManager.GetValue(null) as ArrayList;
            }

            return _globalIndexOfEvents.Count < globalIndex ? null : _globalIndexOfEvents[globalIndex];
        }

        public static void RemoveDependencyPropertyEventHandlers(object o)
        {
            var type = o.GetType();
            while (type != typeof(Visual) && type != typeof(object))
            {
                foreach (var dpd in GetDependencyPropertyDescriptorsForType(type))
                {
                    var property = _piPropertyOfDpd.GetValue(dpd);
                    if (_fiTrackersOfProperty == null)
                        _fiTrackersOfProperty = property.GetType().GetField("_trackers", BindingFlags.Instance | BindingFlags.NonPublic);
                    var trackers = _fiTrackersOfProperty.GetValue(property) as IDictionary;
                    if (trackers != null && trackers.Contains(o))
                    {
                        // Debug.Print($"RemoveDPD: {type.Name}, {dpd.Name}");
                        var tracker = trackers[o];
                        if (_fiChangedHandlerOfTrackers == null)
                            _fiChangedHandlerOfTrackers = tracker.GetType().GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic);
                        var changed = _fiChangedHandlerOfTrackers.GetValue(tracker) as EventHandler;
                        dpd.RemoveValueChanged(o, changed);
                    }
                    
                }
                type = type.BaseType;
            }
        }

        private static DependencyPropertyDescriptor[] GetDependencyPropertyDescriptorsForType(Type type)
        {
            if (!_dpdOfType.ContainsKey(type))
                _dpdOfType[type] = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(f => f.FieldType == typeof(DependencyProperty))
                    .Select(fieldInfo => DependencyPropertyDescriptor.FromProperty(fieldInfo.GetValue(null) as DependencyProperty, type)).ToArray();
            return _dpdOfType[type];
        }
    }
}