using Hazuwall;
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
    public sealed partial class ChooseDayPage : Page
    {
        public ChooseDayPage()
        {
            this.InitializeComponent();
        }
        private void Choose_Click(object sender, RoutedEventArgs e)
        {
            DateTime date = new DateTime(MainPage.Current.Year, MainPage.Current.Month, ((DayBlock)sender).Number);
            MainPage.Current.Frame.Navigate(typeof(ViewPage), date.Ticks / TimeSpan.TicksPerDay);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int Year = MainPage.Current.Year;
            int Month = MainPage.Current.Month;
            int firstDayOfMonth = (new SpecificDate(Year, Month, 1, SpecificDate.AccuracyType.Exact)).GetDayOfWeekIndex();
            List<DayInfo> Days = Database.Current.GetNoteDays(MainPage.Current.SearchParams, Year, Month);
            int listIndex = 0;
            bool isListEnded = Days.Count == 0;
            DayInfo current;
            for (int i = 1; i <= DateTime.DaysInMonth(Year, Month); i++)
            {
                DayBlock block = new DayBlock();
                block.Number = i;
                if (!isListEnded && (current = Days[listIndex]).Num==i)
                {
                    block.IsActive = true;
                    block.IsImportant = current.IsImportant;
                    block.Title = current.Title;
                    block.Tapped += this.Choose_Click;
                    listIndex++;
                    isListEnded = Days.Count - listIndex <= 0;
                }
                else
                    block.IsActive = false;
                int index = firstDayOfMonth + i + 5;
                block.SetValue(Grid.RowProperty, index / 7);
                block.SetValue(Grid.ColumnProperty, index % 7);
                this.Cal.Children.Add(block);
            }
        }
    }
}
