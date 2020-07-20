using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            int year = MainPage.Current.Year;
            int month = MainPage.Current.Month;
            int firstDayOfMonth = DateHelper.GetDayOfWeekIndex(new DateTime(year, month, 1));
            List<DayInfo> Days = Database.Current.GetNoteDays(MainPage.Current.SearchParams, year, month);
            int listIndex = 0;
            bool isListEnded = Days.Count == 0;
            DayInfo current;
            for (int i = 1; i <= DateTime.DaysInMonth(year, month); i++)
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
