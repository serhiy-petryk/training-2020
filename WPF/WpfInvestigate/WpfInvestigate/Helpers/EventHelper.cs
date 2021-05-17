﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Animation;
using WpfInvestigate.Common;

namespace WpfInvestigate.Helpers
{
    public static class EventHelper
    {
        public static void RemoveWpfEventHandlers(object o)
        {
            RemovePropertyChangeEventHandlers(o); // routed events
            RemoveDependencyPropertyEventHandlers(o); // dpd.RemoveValueChanged
            RemoveSimpleEventSubsriptions(o);
        }

        #region =========  RemovePropertyChangeEventHandlers  ========
        private static Dictionary<Type, Tuple<PropertyInfo, MethodInfo>> _eventHandlersStoreData =
            new Dictionary<Type, Tuple<PropertyInfo, MethodInfo>>
            {
                { typeof(UIElement), new Tuple<PropertyInfo, MethodInfo>(
                    typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(UIElement).GetMethod("EventHandlersStoreRemove", BindingFlags.Instance | BindingFlags.NonPublic)) },
                { typeof(UIElement3D), new Tuple<PropertyInfo, MethodInfo>(
                    typeof(UIElement3D).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(UIElement3D).GetMethod("EventHandlersStoreRemove", BindingFlags.Instance | BindingFlags.NonPublic)) },
                { typeof(ContentElement), new Tuple<PropertyInfo, MethodInfo>(
                    typeof(ContentElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(ContentElement).GetMethod("EventHandlersStoreRemove", BindingFlags.Instance | BindingFlags.NonPublic)) },
                { typeof(Timeline), new Tuple<PropertyInfo, MethodInfo>(
                    typeof(Timeline).GetProperty("InternalEventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic),
                    typeof(Timeline).GetMethod("RemoveEventHandler", BindingFlags.Instance | BindingFlags.NonPublic)) },
                //{ typeof(UIElement3D), typeof(UIElement3D).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic) },
                //{ typeof(ContentElement), typeof(ContentElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic) },
                //{ typeof(Timeline), typeof(Timeline).GetProperty("InternalEventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic) },
                //{ typeof(Style), typeof(Style).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic) },
                //{ typeof(FrameworkTemplate), typeof(FrameworkTemplate).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic) }
            };
        private static FieldInfo _fiEntriesOfEventHandlersStore;
        private static PropertyInfo _piCountOfEntries;
        private static MethodInfo _miGetKeyValuePairOfEntries;
        private static Type _globalEventManagerType;
        private static FieldInfo _globalIndexToEventMapOfGlobalEventManager;
        private static ArrayList _globalIndexOfEvents;
        // private static MethodInfo _miEventHandlersStoreRemove = typeof(UIElement).GetMethod("EventHandlersStoreRemove", BindingFlags.NonPublic | BindingFlags.Instance);
        // private static MethodInfo _miEventHandlersStoreRemoveOfTimeline = typeof(Timeline).GetMethod("RemoveEventHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo _miToArrayOfFrugalObjectList;
        public static void RemovePropertyChangeEventHandlers(object o)
        {
            if (o == null) return;

            var type = o.GetType();
            var eventHandlersStoreData = _eventHandlersStoreData.Where(kvp=> kvp.Key.IsAssignableFrom(type)).Select(kvp=> kvp.Value).FirstOrDefault();
            if (eventHandlersStoreData == null)
                return;

            var eventHandlersStore = eventHandlersStoreData.Item1.GetValue(o, null);
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
                                _miToArrayOfFrugalObjectList = a1.Item2.GetType().GetMethod("ToArray", BindingFlags.Instance | BindingFlags.Public);
                            var handlerInfos = _miToArrayOfFrugalObjectList.Invoke(a1.Item2, null) as RoutedEventHandlerInfo[];
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
                /*else if (a1.Item2 is Delegate _delegate)
                {
                    var eventPrivateKey = GetEventByGlobalIndex(a1.Item1);
                    if (eventPrivateKey != null)
                    {
                        //if (o is Timeline)
                          //  _miEventHandlersStoreRemoveOfTimeline.Invoke(o, new[] { eventPrivateKey, a1.Item2 });
                        //else 
                        // Debug.Print($"RemovePropertyChangeEventHandlers2. {o.GetType().Name}, {(o is FrameworkElement fe ? fe.Name : null)}, {_delegate.Method.Name}");
                        eventHandlersStoreData.Item2.Invoke(o, new[] {eventPrivateKey, a1.Item2});
                    }
                }
                else
                    throw new NotImplementedException($"RemovePropertyChangeEventHandlers not implemented yet for {a1.Item2.GetType().Name}");*/
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
        #endregion

        #region =========  RemoveDependencyPropertyEventHandlers  ========
        private static Dictionary<Type, DependencyPropertyDescriptor[]> _dpdOfType = new Dictionary<Type, DependencyPropertyDescriptor[]>();
        private static PropertyInfo _piPropertyOfDpd = typeof(DependencyPropertyDescriptor).GetProperty("Property", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo _fiTrackersOfProperty = null;
        private static FieldInfo _fiChangedHandlerOfTrackers = null;
        public static void RemoveDependencyPropertyEventHandlers(object o)
        {
            foreach (var dpd in GetDependencyPropertyDescriptorsForType(o.GetType()))
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
        }

        private static readonly Attribute[] _attrs = { new PropertyFilterAttribute(PropertyFilterOptions.All) };
        private static DependencyPropertyDescriptor[] GetDependencyPropertyDescriptorsForType(Type type)
        {
            if (!_dpdOfType.ContainsKey(type))
                _dpdOfType.Add(type, TypeDescriptor.GetProperties(type, _attrs).OfType<PropertyDescriptor>()
                    .Select(a => DependencyPropertyDescriptor.FromProperty(a)).Where(a => a != null).ToArray());
            return _dpdOfType[type];
        }
        #endregion

        #region =========  RemoveSimpleEventSubsriptions  ========
        // Based on my old code (2010 year): VS2005, Windows Form
        private static Dictionary<Type, Tuple<EventInfo, FieldInfo>[]> _simpleIventInfoCache = new Dictionary<Type, Tuple<EventInfo, FieldInfo>[]>();
        public static void RemoveSimpleEventSubsriptions(object target)
        {
            var type = target.GetType();
            if (!_simpleIventInfoCache.ContainsKey(type))
            {
                _simpleIventInfoCache.Add(type,
                    type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(ei =>
                            new Tuple<EventInfo, FieldInfo>(ei,
                                ei.DeclaringType?.GetField(ei.Name,
                                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)))
                        .Where(a => a.Item2 != null).ToArray());
            }

            var eis = _simpleIventInfoCache[type];
            foreach (var ei in eis)
            {
                if (ei.Item2.GetValue(target) is Delegate handler)
                {
                    var miRemove = ei.Item1.GetRemoveMethod() ?? ei.Item1.GetRemoveMethod(true);
                    miRemove.Invoke(target, new object[] { handler });
                    /*foreach (var d in handler.GetInvocationList())
                    {
                        // string s = d.Method.Name;
                        // Debug.Print($"RemoveDelegates: {target.GetType()}, {s}, {ei.Name}");
                        miRemove.Invoke(target, new object[] { d });
                        // ei.RemoveEventHandler(target, d);
                    }*/
                }
            }
        }
        #endregion
        private static string GetName(object d)
        {
            return d is FrameworkElement fe ? fe.Name : null;
        }
    }
}
