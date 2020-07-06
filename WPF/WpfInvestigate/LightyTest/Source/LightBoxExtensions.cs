using System.Windows;
using System.Windows.Input;

namespace LightyTest.Source
{
    public static class LightBoxExtensions
    {
        public static void Close(this FrameworkElement dialog)
        {
            ApplicationCommands.Close.Execute(dialog, null);
        }
    }
}
