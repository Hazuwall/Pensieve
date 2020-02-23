using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class ChooseYearPage : Page
    {
        public ObservableCollection<YearInfo> Years { get; set; }

        public ChooseYearPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Years = new ObservableCollection<YearInfo>(Database.Current.GetNoteYears(MainPage.Current.SearchParams));
        }
        private void Choose_Click(object sender, ItemClickEventArgs e)
        {
            MainPage.Current.Year = (e.ClickedItem as YearInfo).Number;
        }
    }
}
