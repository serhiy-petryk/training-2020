using System.Windows;

namespace AppCore5.Helpers
{
    public static class StyleHelper
    {
        public static Style StyleClone(this Style original)
        {
            var clone = new Style
            {
                TargetType = original.TargetType,
                BasedOn = original.BasedOn
            };

            foreach (var setter in original.Setters)
                clone.Setters.Add(setter);

            foreach (var trigger in original.Triggers)
                clone.Triggers.Add(trigger);

            foreach (var key in original.Resources.Keys)
                clone.Resources[key] = original.Resources[key];

            return clone;
        }
    }
}
