using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using WpfInvestigate.Common;

namespace WpfInvestigate.Controls
{
    public class ObjectEditor : UserControl, INotifyPropertyChanged
    {
        static ObjectEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ObjectEditor), new FrameworkPropertyMetadata(typeof(ObjectEditor)));
            EventManager.RegisterClassHandler(typeof(ObjectEditor), UIElement.GotFocusEvent, new RoutedEventHandler(OnGotFocus));
        }
        public ObjectEditor()
        {
            DataContext = this;
        }

        public decimal NumericMinValue => Metadata.Template == DataTypeMetadata.DataTemplate.NumericBox ? Convert.ToDecimal(Metadata.MinValue) : decimal.MinValue;
        public decimal NumericMaxValue => Metadata.Template == DataTypeMetadata.DataTemplate.NumericBox ? Convert.ToDecimal(Metadata.MaxValue) : decimal.MaxValue;
        public DataTypeMetadata Metadata => DataTypeMetadata.MetadataList[ValueDataType];

        private bool _isTemplateChanging;

        //=============================
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(ObjectEditor));

        public static readonly DependencyProperty ValueDataTypeProperty = DependencyProperty.Register("ValueDataType", 
            typeof(DataTypeMetadata.DataType), typeof(ObjectEditor), 
            new FrameworkPropertyMetadata(DataTypeMetadata.DataType.String, OnValueDataTypeChanged));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ObjectEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));
        public static readonly DependencyProperty IsNullableProperty = DependencyProperty.Register("IsNullable", typeof(bool), typeof(ObjectEditor), new FrameworkPropertyMetadata(false, OnIsNullableChanged));

        //=============================
        public event RoutedPropertyChangedEventHandler<object> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        public DataTypeMetadata.DataType ValueDataType
        {
            get => (DataTypeMetadata.DataType)GetValue(ValueDataTypeProperty);
            set => SetValue(ValueDataTypeProperty, value);
        }
        public object Value
        {
            get => (object)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Can the value be null?
        /// </summary>
        public bool IsNullable
        {
            get => (bool)GetValue(IsNullableProperty);
            set => SetValue(IsNullableProperty, value);
        }
        //=================================
        private static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            var editor = (ObjectEditor)sender;
            if (!e.Handled && editor.Focusable)
            {
                if (Equals(e.OriginalSource, editor))
                {
                    // MoveFocus takes a TraversalRequest as its argument.
                    var request = new TraversalRequest((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
                    // Gets the element with keyboard focus.
                    var elementWithFocus = Keyboard.FocusedElement as UIElement;
                    // Change keyboard focus.
                    if (elementWithFocus != null)
                        elementWithFocus.MoveFocus(request);
                    else
                        editor.Focus();

                    e.Handled = true;
                }
                else if (e.OriginalSource is TextBox)
                {
                    ((TextBox)e.OriginalSource).SelectAll();
                    e.Handled = true;
                }
                else if (e.OriginalSource is CheckBox)
                    e.Handled = true;
            }
        }

        private static void OnValueDataTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (ObjectEditor)d;
            var metadata = DataTypeMetadata.MetadataList[(DataTypeMetadata.DataType)e.NewValue];
            
            editor.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                editor._isTemplateChanging = true;
                editor.Template = editor.FindResource(metadata.Template.ToString()) as ControlTemplate;
                editor.Value = Tips.GetDefaultOfType(metadata.Type);

                foreach (var textBox in Tips.GetVisualChildren(editor).OfType<DatePickerTextBox>())
                    Helpers.ControlHelper.HideBorderOfDatePickerTextBox(textBox);

                editor._isTemplateChanging = false;
                
                var newValue = editor.IsNullable ? null : Tips.GetDefaultOfType(metadata.Type);
                if (Equals(newValue, editor.Value))
                    editor.OnValueChanged(null, newValue);
                else
                    editor.Value = newValue;

                editor.RefreshUI();
            }));

        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var objectEditor = (ObjectEditor)d;
            objectEditor.OnValueChanged(e.OldValue, e.NewValue);
        }

        private static object CoerceValue(DependencyObject d, object value)
        {
            var editor = (ObjectEditor)d;
            if (editor._isTemplateChanging)
                return value;
            if (value == null)
                return editor.IsNullable ? null : Tips.GetDefaultOfType(editor.Metadata.Type);

            if (editor.Metadata.MinValue != null)
            {
                var min = ChangeTypeConverter.Instance.Convert(editor.Metadata.MinValue, value.GetType(), null, null);
                if (((IComparable)min).CompareTo(value) == 1)
                    value = min;
            }
            if (editor.Metadata.MaxValue != null)
            {
                var max = ChangeTypeConverter.Instance.Convert(editor.Metadata.MaxValue, value.GetType(), null, null);
                if (((IComparable)max).CompareTo(value) == -1)
                    value = max;
            }
            return ChangeTypeConverter.Instance.Convert(value, editor.Metadata.Type, null, null);
        }

        //=========================================================
        protected virtual void OnValueChanged(object oldValue, object newValue)
        {
            if (!_isTemplateChanging)
                RaiseEvent(new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, ValueChangedEvent));
        }

        private static void OnIsNullableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var objectEditor = (ObjectEditor)d;
                objectEditor.Value = objectEditor.Value; // Coerce value
            }
        }
        //===========  INotifyPropertyChanged  =======================
        #region ===========  INotifyPropertyChanged  ===============
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertiesChanged(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void NumericBox_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal?> e) => Value = e.NewValue;
        private void TextBox_OnValueChanged(object sender, TextChangedEventArgs e) => Value = ((TextBox)sender).Text;
        private void CheckBox_OnValueChanged(object sender, RoutedEventArgs e) => Value = ((CheckBox)sender).IsChecked;
        private void DatePicker_OnValueChanged(object sender, SelectionChangedEventArgs e) => Value = ((DatePicker)sender).SelectedDate;
        private void DateTimePicker_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e) => Value = e.NewValue;
        private void TimePicker_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<TimeSpan?> e) => Value = e.NewValue;

        //================================
        private void ObjectEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            OnValueDataTypeChanged(this, new DependencyPropertyChangedEventArgs(ValueDataTypeProperty, null, ValueDataType));
        }

        private void RefreshUI()
        {
            OnPropertiesChanged(new[] {nameof(Metadata)});
        }
    }
}
