using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Выпадающий список для выбора множества значений, поддерживающий опцию добавления пользователем своих значений
    /// </summary>
    public sealed partial class MultipleComboBox : UserControl
    {
        private ObservableCollection<string> NewListBoxItemsSource { get; set; }

        /// <summary>
        /// Обновление отображающей панели
        /// </summary>
        private bool IsUpdateRequested {
            get {
                return this._IsUpdateRequested;
            }
            set{
                if (this._IsUpdateRequested == value)
                    return;
                this.Panel.Children.Clear();
                if(value)
                    this.Panel.Children.Add(new TextBlock() { Foreground = new SolidColorBrush(Windows.UI.Colors.Gray), Text = "Обновление..." });
                else
                {
                    foreach (string item in this.NewListBoxItemsSource)
                        this.Panel.Children.Add(new TextBlock() { Text = item });
                    foreach (object item in this.ListBox.SelectedItems)
                        this.Panel.Children.Add(new TextBlock() { Text = item.ToString() });
                    if (this.Panel.Children.Count == 0)
                        this.Panel.Children.Add(new TextBlock() { Foreground = new SolidColorBrush(Windows.UI.Colors.Gray), Text = "Выберите значения..." });
                }
                this._IsUpdateRequested=value;
            }
        }
        private bool _IsUpdateRequested = false;

        /// <summary>
        /// Событие закрытие выпадающего списка
        /// </summary>
        public event EventHandler FlyoutClosed = delegate { };

        /// <summary>
        /// Можно ли добавить новый элемент с помощью текстового поля
        /// </summary>
        public bool CanAddItems
        {
            get
            {
                return this.TextBox.Visibility == Windows.UI.Xaml.Visibility.Visible;
            }
            set
            {
                Windows.UI.Xaml.Visibility visibility = value ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
                this.TextBox.Visibility = visibility;
                this.AddNewButton.Visibility = visibility;
                this.NewListBox.Visibility = visibility;
            }
        }

        /// <summary>
        /// Источник элементов для выбора
        /// </summary>
        public object ItemsSource
        {
            get { return this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(object), typeof(MultipleComboBox), new PropertyMetadata(null));

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)this.GetValue(VerticalScrollBarVisibilityProperty); }
            set { this.SetValue(VerticalScrollBarVisibilityProperty, value); }
        }
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(
            "VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(MultipleComboBox), new PropertyMetadata(ScrollBarVisibility.Visible));

        /// <summary>
        /// Получить список добавленных строк
        /// </summary>
        public List<string> AddedItems { get { return this.NewListBoxItemsSource.ToList(); } }

        /// <summary>
        /// Список выбранных элементов
        /// </summary>
        public IList<object> SelectedItems
        {
            get
            {
                return this.ListBox.SelectedItems;
            }
            set
            {
                this.ListBox.SelectedIndex = -1;
                foreach (object item in value)
                    this.ListBox.SelectedItems.Add(item);
                this.IsUpdateRequested = false;
            }
        }


        public MultipleComboBox()
        {
            this.InitializeComponent();
            this.FlyoutContentRoot.DataContext = this;
            this.ViewerContentRoot.DataContext = this;
            this.NewListBoxItemsSource = new ObservableCollection<string>();
            this.NewListBox.ItemsSource = this.NewListBoxItemsSource;
            this.Panel.Children.Add(new TextBlock() { Foreground = new SolidColorBrush(Windows.UI.Colors.Gray), Text = "Выберите значения..." });
        }

        private void Open_Click(object sender, TappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout(this);
        }
        private void ScrollViewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ((ScrollViewer)sender).BorderBrush = new SolidColorBrush(Windows.UI.Colors.Gray);
        }
        private void ScrollViewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((ScrollViewer)sender).BorderBrush = new SolidColorBrush(Windows.UI.Colors.DarkGray);
        }
        /// <summary>
        /// При нажатии на кнопку добавить новое значение в список и выбрать его
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string item = this.TextBox.Text;
            this.TextBox.Text = String.Empty;
            if (String.IsNullOrWhiteSpace(item)||this.NewListBoxItemsSource.Contains(item))
                return;
            this.NewListBoxItemsSource.Insert(0,item);
            this.NewListBox.SelectedItems.Add(item);
        }
        /// <summary>
        /// Удалить из списка невыделенные новые элементы, запросить обновление панели выбранных элементов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsUpdateRequested = true;
            foreach (string item in e.RemovedItems)
                this.NewListBoxItemsSource.Remove(item);
        }
        /// <summary>
        /// Запросить обновление панели выбранных элементов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.IsUpdateRequested = true;
        }
        /// <summary>
        /// Обновить панель выбранных элементов при закрытии Flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Flyout_Closed(object sender, object e)
        {
            this.IsUpdateRequested = false;
            this.FlyoutClosed.Invoke(this, null);
        }
    }
}
