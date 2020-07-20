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
    /// <summary>
    /// Выпадающий список строк с возможностью добавления пользователем своего значения
    /// </summary>
    public sealed partial class AddableComboBox : UserControl
    {
        public AddableComboBox()
        {
            this.InitializeComponent();
            this._ItemsSource = new List<string>() { "< Пропустить >", "< Добавить >" };
        }

        /// <summary>
        /// Исходный список элементов
        /// </summary>
        public IEnumerable<string> ItemsSource {
            get
            {
                return this._ItemsSource.Skip(2);
            }
            set
            {
                this._ItemsSource.RemoveRange(2, this._ItemsSource.Count - 2);
                this._ItemsSource.AddRange(value);
                this.Combo.SelectedIndex = 0;
                this.IsAddMode = false;
            }
        }
        private readonly List<string> _ItemsSource;

        /// <summary>
        /// Режим добавления собственного значения через текстовое поле
        /// </summary>
        public bool IsAddMode
        {
            get
            {
                return this.Text.Visibility == Visibility.Visible;
            }
            set
            {
                if (this.IsAddMode == value)
                    return;
                if (value)
                {
                    this.Combo.Visibility = Visibility.Collapsed;
                    this.Text.Text = String.Empty;
                    this.Text.Visibility = Visibility.Visible;
                    this.Text.Focus(FocusState.Keyboard);
                }
                else
                {
                    this.Text.Visibility = Visibility.Collapsed;
                    this.Combo.SelectedIndex = 0;
                    this.Combo.Visibility = Visibility.Visible;
                    this.Combo.Focus(FocusState.Pointer);
                }
            }
        }

        /// <summary>
        /// Выбранное или введённое значение
        /// </summary>
        public string Value
        {
            get
            {
                string result = String.Empty;
                if (this.IsAddMode && !String.IsNullOrWhiteSpace(this.Text.Text))
                    result = this.Text.Text;
                else if (this.Combo.SelectedIndex > 1)
                    result = (string)this.Combo.SelectedItem;
                return result;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    this.IsAddMode = false;
                    this.Combo.SelectedIndex = 0;
                }
                else if (this.ItemsSource.Contains(value))
                {
                    this.IsAddMode = false;
                    this.Combo.SelectedItem = value;
                }
                else
                {
                    this.IsAddMode = true;
                    this.Text.Text = value;
                }
            }
        }
        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Combo.SelectedIndex == -1)
                this.Combo.SelectedIndex = 0;
            else if (this.Combo.SelectedIndex == 1)
                this.IsAddMode = true;
            this.SelectionChanged.Invoke(this, e);
        }
        public event SelectionChangedEventHandler SelectionChanged = delegate { };
    }
}
