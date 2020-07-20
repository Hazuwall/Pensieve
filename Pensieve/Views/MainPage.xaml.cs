using Pensieve.Common;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace Pensieve
{
    public sealed partial class MainPage : Page
    {
        #region NavigationHelper
        private NavigationHelper navigationHelper;

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private int _year = 0;
        private int _month = 0;

        public static MainPage Current { get; set; }

        /// <summary>
        /// Текущий год
        /// </summary>
        public int Year
        {
            get
            {
                return this._year;
            }
            set
            {
                this._year = value;
                this.UpdateChoiceFramePage();
            }
        }

        /// <summary>
        /// Текущий месяц
        /// </summary>
        public int Month
        {
            get
            {
                return this._month;
            }
            set
            {
                this._month = value;
                this.UpdateChoiceFramePage();
            }
        }

        /// <summary>
        /// Параметры панели поиска
        /// </summary>
        public Database.SearchParams SearchParams
        {
            get
            {
                return new Database.SearchParams(
                    (bool)this.ImportantBox.IsChecked,
                    this.TagBox.SelectedItems.Cast<string>().ToList(),
                    this.SearchBox.QueryText
                    );
            }
            set
            {
                this.ImportantBox.IsChecked = value.ImportantOnly;
                this.TagBox.SelectedItems = value.Tags.Cast<object>().ToList();
                this.SearchBox.QueryText = value.SearchString;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            
            this.TagBox.ItemsSource = Database.Current.GetResources(ResourceType.Tags);
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //Загрузка прежнего состояния
            if (Current != null)
            {
                this._year = Current.Year;
                this._month = Current.Month;
                this.SearchParams = Current.SearchParams;
            }
            Current = this;
            this.UpdateChoiceFramePage();
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region Панель поиска
        private void ShowSearchPanel_Click(object sender, RoutedEventArgs e)
        {
            this.SearchPanel.Visibility = this.SearchPanel.Visibility == Windows.UI.Xaml.Visibility.Visible ?
                Windows.UI.Xaml.Visibility.Collapsed
                :
                Windows.UI.Xaml.Visibility.Visible;
        }
        private void Search_Click(object sender, SearchBoxQuerySubmittedEventArgs e)
        {
            this.UpdateChoiceFramePage();
        }
        #endregion

        #region Навигация
        /// <summary>
        /// Провести навигацию страницы выбора к текущим году и месяцу
        /// </summary>
        private void UpdateChoiceFramePage()
        {
            if (this._year == 0)
                this.ChoiceFrame.Navigate(typeof(ChooseYearPage));
            else if (this._month == 0)
                this.ChoiceFrame.Navigate(typeof(ChooseMonthPage));
            else
                this.ChoiceFrame.Navigate(typeof(ChooseDayPage));
        }

        /// <summary>
        /// Обновить хлебные крошки и заголовок при совершении навигации страницы выбора
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChoiceFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //Параметр команды хранит глубину кнопки, тэг - числовое предстаавление
            this.PathPanel.Children.Clear();
            HyperlinkButton link = new HyperlinkButton() { Content = "Дневник", CommandParameter = 0 };
            link.Click += PathElement_Click;
            this.PathPanel.Children.Add(link);
            if (this._year == 0)
                this.pageTitle.Text = "Дневник";
            else
            {
                this.PathPanel.Children.Add(new TextBlock());
                link = new HyperlinkButton() { Content = this._year, Tag = this._year, CommandParameter = 1 };
                link.Click += PathElement_Click;
                this.PathPanel.Children.Add(link);
                if (this._month == 0)
                    this.pageTitle.Text = this._year.ToString();
                else
                {
                    this.PathPanel.Children.Add(new TextBlock());
                    link = new HyperlinkButton() { Content = DateHelper.GetMonthName(this._month, true, false), Tag = this._month, CommandParameter = 2 };
                    link.Click += PathElement_Click;
                    this.PathPanel.Children.Add(link);
                    this.pageTitle.Text = DateHelper.GetMonthName(this._month, true, false) + " " + this._year;
                }
            }
        }

        /// <summary>
        /// Навигация в соответствии с нажатой кнопкой обратного пути
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PathElement_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton button = sender as HyperlinkButton;
            if ((int)button.CommandParameter == 2)
                this._month = (int)button.Tag;
            else if ((int)button.CommandParameter == 1)
            {
                this._year = (int)button.Tag;
                this._month = 0;
            }
            else
            {
                this._year = 0;
                this._month = 0;
            }
            this.UpdateChoiceFramePage();
        }
        #endregion

        #region Меню
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NoteMakerPage));
        }
        private void Resources_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ResourcesPage));
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            (App.Current as App).ShowBackupSettingsFlyout();
        }
        #endregion
    }
}