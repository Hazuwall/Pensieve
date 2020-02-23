﻿using Hazuwall;
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
    public sealed partial class ChooseMonthPage : Page
    {
        public YearInfo YearInfo {get;set;}
        public ObservableCollection<UniqueString> Months { get; set; }

        public ChooseMonthPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.YearInfo = Database.Current.GetYearInfo(MainPage.Current.Year);
            this.Months = new ObservableCollection<UniqueString>();
            foreach (int num in Database.Current.GetNoteMonths(MainPage.Current.SearchParams, this.YearInfo.Number))
                this.Months.Add(new UniqueString(num, SpecificDate.GetMonthName(num, true, false)));
        }
        private void Choose_Click(object sender, ItemClickEventArgs e)
        {
            MainPage.Current.Month = ((UniqueString)e.ClickedItem).ID;
        }
        private void EditYear_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Current.Frame.Navigate(typeof(EditYearInfoPage), this.YearInfo.Number);
        }


    }
}
