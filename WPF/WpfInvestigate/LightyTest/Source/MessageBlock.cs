using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using LightyTest.Examples;

namespace LightyTest.Source
{
    public class MessageBlock
    {
        // public enum MessageBlockIcon { Hand, Question, Exclamation, Asterisk, Stop, Error, Warning, Information }
        public enum MessageBlockIcon { Question, Stop, Error, Warning, Information, Ok }
        // private static Color[] _iconColors = { Colors.Blue, Colors.Red, Colors.Red, Colors.Yellow, Colors.RoyalBlue, Colors.LimeGreen };

        public static string Show(string messageText, string caption, MessageBlockIcon? icon = null, string[] buttons = null )
        {
            var style = Application.Current.TryFindResource("MovableDialogStyle") as Style;
            var data = new MessageBlock(messageText, caption, icon, buttons);
            var content = new MessageBlockControl(data);
            DialogItems.ShowDialog(null, content, style, DialogItems.GetAfterCreationCallbackForMovableDialog(content, true));
            return data.Result;
        }

        // =========================
        public string MessageText { get; }
        public string Caption { get; }
        public FrameworkElement Icon { get; }
        public string[] Buttons { get; }
        public string Result;

        private MessageBlock(string messageText, string caption, MessageBlockIcon? icon, string[] buttons)
        {
            MessageText = messageText;
            Caption = caption;
            if (icon != null)
                Icon =  Application.Current.TryFindResource($"MessageBlock{icon.Value}Icon") as FrameworkElement;

            Buttons = buttons ?? new string[0];
        }
    }
}
