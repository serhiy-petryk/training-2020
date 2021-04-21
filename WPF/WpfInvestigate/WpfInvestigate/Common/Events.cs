using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Primitives;

namespace WpfInvestigate.Common
{
    public static class Events
    {
        private static int handlerCount = 0;
        public static void RemoveAllRoutedEventHandlers(UIElement element)
        {
            // Based on Douglas comment in https://stackoverflow.com/questions/9434817/how-to-remove-all-click-event-handlers

            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            var eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);
            if (eventHandlersStore == null) return;

            var type = element.GetType();
            var types = new List<Type>{type};
            while (type != typeof(UIElement))
            {
                type = type.BaseType;
                types.Add(type);
            }

            /*PropertyInfo pi = typeof(UIElement).GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(element, null);

            var ei1 = typeof(UIElement).GetEvent("PreviewMouseLeftButtonDown");
            var a11 = eventHandlersStore.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var a12 = (RoutedEventHandlerInfo[])a11.Invoke(eventHandlersStore, new object[] { ei1 });

            var ei2 = typeof(UIElement).GetEvent("IsHitTestVisibleChanged");*/

            foreach (var re in EventManager.GetRoutedEvents().OfType<RoutedEvent>().Where(e=> types.Contains(e.OwnerType)))
            {
                var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(eventHandlersStore, new object[] { re });
                if (routedEventHandlers != null)
                {
                    foreach (var reHandler in routedEventHandlers)
                    {
                        Debug.Print($"RemoveEventHandler. {handlerCount++}: {element.GetType().Name}, {re.Name}, {(element is FrameworkElement fe ? fe.Name : null)}");
                        // element.RemoveHandler(re, reHandler.Handler);
                    }
                }
            }
        }

        public static void RemoveAllEventSubsriptions(object target)
        {
            RemoveEventSubsriptions(target, null);
        }
        public static void RemoveEventSubsriptions(object target, object subscriber)
        {
            EventInfo[] eis = target.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (EventInfo ei in eis)
            {
                RemoveDelegates(ei, target, subscriber);
            }
            PropertyInfo[] pis = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (PropertyInfo pi in pis)
            {
                if (pi.PropertyType == typeof(EventHandlerList) || pi.PropertyType.IsSubclassOf(typeof(EventHandlerList)))
                {
                    EventHandlerList ehl = (EventHandlerList)pi.GetValue(target, null);
                    RemoveDelegates(ehl, subscriber);
                }
            }
            FieldInfo[] fis = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fi in fis)
            {
                if (fi.FieldType == typeof(EventHandlerList) || fi.FieldType.IsSubclassOf(typeof(EventHandlerList)))
                {
                    EventHandlerList ehl = (EventHandlerList)fi.GetValue(target);
                    RemoveDelegates(ehl, subscriber);
                }
            }
        }

        //==============  Private section  ==============
        private static FieldInfo fiHandler;
        private static FieldInfo fiNext;
        private static FieldInfo fiKey;
        private static void RemoveDelegates(EventHandlerList ehl, object subscriber)
        {
            if (subscriber == null && false)
            {// Remove all events
                ehl.Dispose();
                return;
            }
            List<Delegate> delegates = new List<Delegate>();
            List<object> keys = new List<object>();
            if (ehl != null)
            {
                FieldInfo fiHead = typeof(EventHandlerList).GetField("head", BindingFlags.Instance | BindingFlags.NonPublic);
                object head = fiHead.GetValue(ehl);
                if (fiHandler == null)
                {
                    fiHandler = head.GetType().GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
                    fiNext = head.GetType().GetField("next", BindingFlags.Instance | BindingFlags.NonPublic);
                    fiKey = head.GetType().GetField("key", BindingFlags.Instance | BindingFlags.NonPublic);
                }
                //    FieldInfo fiHandler = head.GetType().GetField("handler", BindingFlags.Instance | BindingFlags.NonPublic);
                //  FieldInfo fiNext = head.GetType().GetField("next", BindingFlags.Instance | BindingFlags.NonPublic);
                while (head != null)
                {
                    Delegate d = (Delegate)fiHandler.GetValue(head);
                    if (d != null)
                    {
                        delegates.Add(d);
                        keys.Add(fiKey.GetValue(head));
                    }
                    head = fiNext.GetValue(head);
                }
            }
            if (subscriber == null)
            {// delete all events of target
                for (int i = 0; i < delegates.Count; i++)
                {
                    string s = delegates[i].Method.Name;
                    ehl.RemoveHandler(keys[i], delegates[i]);
                }
            }
            else
            {
                for (int i = 0; i < delegates.Count; i++)
                {
                    string s = delegates[i].Method.Name;
                    if (delegates[i].Target == subscriber)
                    {
                        ehl.RemoveHandler(keys[i], delegates[i]);
                    }
                }
            }
        }

        private static void RemoveDelegates(EventInfo ei, object target, object subcriber)
        {
            FieldInfo fi = ei.DeclaringType.GetField(ei.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (fi == null)
            {
                if (ei.DeclaringType == typeof(Component))
                {
                }
            }
            if (fi != null)
            {
                Delegate handler = (Delegate)fi.GetValue(target);
                if (handler != null)
                {
                    Delegate[] dd = handler.GetInvocationList();
                    if (subcriber == null)
                    {
                        foreach (Delegate d in dd)
                        {
                            string s = d.Method.Name;
                            ei.RemoveEventHandler(target, d);
                        }
                    }
                    else
                    {
                        foreach (Delegate d in dd)
                        {
                            string s = d.Method.Name;
                            if (d.Target == subcriber)
                                ei.RemoveEventHandler(target, d);
                        }
                    }
                }
            }
        }

        //========================================
        // https://stackoverflow.com/questions/4794071/how-to-enumerate-all-dependency-properties-of-control
        public static IEnumerable<DependencyProperty> EnumerateDependencyProperties(this DependencyObject obj)
        {
            if (obj != null)
            {
                var lve = obj.GetLocalValueEnumerator();
                while (lve.MoveNext())
                    yield return lve.Current.Property;
            }
        }
        // https://social.msdn.microsoft.com/Forums/vstudio/en-US/d47114a7-b182-485c-b1dd-1078ddd42be9/enumerate-bindings?forum=wpf
        public static IEnumerable<Binding> EnumerateBindings(DependencyObject obj)
        {
            if (obj != null)
            {
                var lve = obj.GetLocalValueEnumerator();
                while (lve.MoveNext())
                {
                    var entry = lve.Current;
                    if (BindingOperations.IsDataBound(obj, entry.Property))
                    {
                        var binding = (entry.Value as BindingExpression)?.ParentBinding;
                        yield return binding;
                    }
                }
            }
        }

        public static List<DependencyProperty> GetDependencyProperties(Object element)
        {
            List<DependencyProperty> properties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (MarkupProperty mp in markupObject.Properties)
                {
                    if (mp.DependencyProperty != null)
                    {
                        properties.Add(mp.DependencyProperty);
                    }
                }
            }

            return properties;
        }

        public static List<DependencyProperty> GetAttachedProperties(Object element)
        {
            List<DependencyProperty> attachedProperties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
            {
                foreach (MarkupProperty mp in markupObject.Properties)
                {
                    if (mp.IsAttached)
                    {
                        attachedProperties.Add(mp.DependencyProperty);
                    }
                }
            }

            return attachedProperties;
        }
    }
}
