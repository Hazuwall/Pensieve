using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Pensieve
{
    public sealed partial class EditableTextBlock : UserControl
    {
        private static readonly DependencyProperty _TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(String.Empty));
        public static DependencyProperty TextProperty { get { return _TextProperty; } }

        private TextBox Box;
        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Событие, вызываемое при потере фокуса на поле изменения. Может использоваться для избежания некорректных значений блока
        /// </summary>
        public event TextEditedEventHandler TextEdited = delegate { };

        public EditableTextBlock()
        {
            this.InitializeComponent();
            Binding binding = new Binding() {
                Source = this,
                Path=new PropertyPath("Text")
            };
            this.Block.SetBinding(TextBlock.TextProperty, binding);
        }

        /// <summary>
        /// Изменить текст внутри блока, отобразив соответствующее поле. Во время выполнения функции должны быть прекращены манипуляции с фокусом
        /// </summary>
        public void Edit()
        {
            if (this.Box != null)
                return;

            TextBox box = new TextBox()
            {
                Text = this.Text,
                SelectionStart = this.Text.Length,
                Margin = this.Block.Margin,
                Padding = this.Block.Padding,
                HorizontalAlignment = this.Block.HorizontalAlignment,
                VerticalAlignment = this.Block.VerticalAlignment,
                TextWrapping = TextWrapping.Wrap,
                FontSize = this.Block.FontSize,
                FontWeight = this.Block.FontWeight
            };
            box.LostFocus += Box_LostFocus;
            this.Block.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.Container.Children.Add(box);
            this.Box = box;
            box.Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }

        private void Box_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.Box == null)
                return;

            TextEditedEventArgs args = new TextEditedEventArgs(this.Text, this.Box.Text);
            this.TextEdited.Invoke(this, args);
            if (args.IsValid)
                this.Text = this.Box.Text;

            this.Container.Children.Remove(this.Box);
            this.Box = null;
            this.Block.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }
}
