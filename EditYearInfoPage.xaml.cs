using Hazuwall.Tools;
using Pensieve.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Pensieve
{
    public sealed partial class EditYearInfoPage : Page
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

        private int Year=0;

        public EditYearInfoPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            YearInfo yearInfo = Database.Current.GetYearInfo((int)e.NavigationParameter);
            this.Year = yearInfo.Number;
            this.pageTitle.Text = yearInfo.Number.ToString();
            this.BriefBox.Document.SetText(Windows.UI.Text.TextSetOptions.None, yearInfo.Brief);
            this.ColorBox.Text = yearInfo.ColorCode;
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            try
            {
                string brief;
                this.BriefBox.Document.GetText(Windows.UI.Text.TextGetOptions.UseCrlf, out brief);
                UIHelper.GetColorFromHex(this.ColorBox.Text); //проверка корректности формата
                YearInfo yearInfo = new YearInfo(this.Year, brief, this.ColorBox.Text);
                Database.Current.SetYearInfo(yearInfo);
                Frame.GoBack();
            }
            catch
            {
                UIHelper.ShowMessageDialogAsync("Цвет не соответствует формату \"#FFFFFF\"");
            }
            ((Button)sender).IsEnabled = true;
        }
    }
}
