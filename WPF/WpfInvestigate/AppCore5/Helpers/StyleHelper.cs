using System.Windows;

namespace AppCore5.Helpers
{
    public static class StyleHelper
    {
        public static Style StyleClone(this Style originalStyle)
        {
            var newStyle = new Style();

            if (originalStyle.TargetType != null)
                newStyle.TargetType = originalStyle.TargetType;

            foreach (var setterBase in originalStyle.Setters)
            {
                var setter = (Setter)setterBase;
                // Clone the setter's value if it's a Freezable and not frozen
                var clonedValue = (setter.Value is Freezable freezable && freezable.CanFreeze) ? freezable.Clone() : setter.Value;
                newStyle.Setters.Add(new Setter(setter.Property, clonedValue));
            }

            foreach (var triggerBase in originalStyle.Triggers)
            {
                if (triggerBase is Trigger trigger)
                {
                    var clonedTrigger = new Trigger { Property = trigger.Property, Value = trigger.Value };
                    foreach (var setterBase in trigger.Setters)
                    {
                        var triggerSetter = (Setter)setterBase;
                        var clonedTriggerValue = (triggerSetter.Value is Freezable freezable && freezable.CanFreeze) ? freezable.Clone() : triggerSetter.Value;
                        clonedTrigger.Setters.Add(new Setter(triggerSetter.Property, clonedTriggerValue));
                    }
                    newStyle.Triggers.Add(clonedTrigger);
                }

                if (originalStyle.BasedOn != null)
                    newStyle.BasedOn = originalStyle.BasedOn;
            }

            return newStyle;
        }
    }
}
