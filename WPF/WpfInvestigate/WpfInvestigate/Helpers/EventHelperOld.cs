// Based on my old code (2010 year): VS2005, Windows Form
using System;
using System.Reflection;

namespace WpfInvestigate.Helpers
{
    public static class EventHelperOld
    {
        public static void RemoveAllEventSubsriptions(object target)
        {
            var eis = target.GetType().GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var ei in eis)
            {
                var fi = ei.DeclaringType?.GetField(ei.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (!(fi is null) && fi.GetValue(target) is Delegate handler)
                {
                    // var miRemove = ei.GetRemoveMethod() ?? ei.GetRemoveMethod(true);
                    foreach (var d in handler.GetInvocationList())
                    {
                        // string s = d.Method.Name;
                        // Debug.Print($"RemoveDelegates: {target.GetType()}, {s}, {ei.Name}");
                        // miRemove.Invoke(target, new object[] { d });
                        ei.RemoveEventHandler(target, d);
                    }
                }
            }
        }
    }
}
