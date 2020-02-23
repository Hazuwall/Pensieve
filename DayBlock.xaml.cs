using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Pensieve
{
    public sealed partial class DayBlock : UserControl
    {
        private bool _IsActive = true;

        public bool IsActive
        {
            get
            {
                return this._IsActive;
            }
            set
            {
                this._IsActive = value;
                if (value)
                {
                    this.NumberBox.Style = (Style)this.Resources["ActiveTextBlockStyle"];
                    this.Grid.PointerEntered += this.Grid_PointerEntered;
                    this.Grid.PointerExited += this.Grid_PointerExited;
                }
                else
                {
                    this.NumberBox.Style = (Style)this.Resources["DisabledTextBlockStyle"];
                    this.Grid.Style = null;
                    this.Grid.PointerEntered -= this.Grid_PointerEntered;
                    this.Grid.PointerExited -= this.Grid_PointerExited;
                }
            }
        }
        public int Number
        {
            get
            {
                return this.NumberBox.Text == String.Empty ? 0 : Int32.Parse(this.NumberBox.Text);
            }
            set
            {
                this.NumberBox.Text = value == 0 ? String.Empty : value.ToString();
            }
        }
        public string Title
        {
            get
            {
                return this.TitleBox.Text;
            }
            set
            {
                this.TitleBox.Text = value;
            }
        }
        public bool IsImportant
        {
            get
            {
                return this.ImportantBox.Fill == (SolidColorBrush)this.Resources["ImportantBrush"];
            }
            set
            {
                this.ImportantBox.Fill = value? (SolidColorBrush)this.Resources["ImportantBrush"] : null;
            }
        }

        public DayBlock()
        {
            this.InitializeComponent();
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            e.Handled = !this._IsActive;
            base.OnTapped(e);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.Grid.Background = (SolidColorBrush)this.Resources["SelectedBrush"];
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.Grid.Background = null;
        }

        private void Grid_SizeChanged(object sender, RoutedEventArgs e)
        {
            this.TitleBox.Visibility = this.Grid.ActualWidth > 100 ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
