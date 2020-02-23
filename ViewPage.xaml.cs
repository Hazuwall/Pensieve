using Hazuwall;
using Hazuwall.Tools;
using Pensieve.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
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
    public sealed partial class ViewPage : Page
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

        private long ID;

        public ObservableCollection<StorageFile> Images { get; set; }
        public ObservableCollection<Resource> Songs { get; set; }
        public ObservableCollection<Resource> Docs { get; set; }

        public ViewPage()
        {
            this.Songs = new ObservableCollection<Resource>();
            this.Images = new ObservableCollection<StorageFile>();
            this.Docs = new ObservableCollection<Resource>();
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.ID = (long)e.NavigationParameter;
            Note Note = Database.Current.GetNote(this.ID);

            this.LoadResourcesAsync(Note);

            this.pageTitle.Text = Note.Date.GetFullDate();

            if (!String.IsNullOrWhiteSpace(Note.Title))
                this.TitleBox.Text = Note.Title;
            else
                this.TitleBox.Visibility = Visibility.Collapsed;

            this.ContentBox.NavigateToString("<html><head></head><body style='text-align:justify;padding-right:20px;font-size:15px;font-family:Arial;'>" + Note.Text + "</body></html>");

            if (Note.Tags.Count > 0)
                this.TagBox.Text = "Теги: " + Note.Tags.ToArray().ToPlainList();
            else
                this.TagBox.Visibility = Visibility.Collapsed;

            if (!String.IsNullOrWhiteSpace(Note.Extra))
                this.ExtraBox.Text = Note.Extra;
            else
                this.ExtraBox.Visibility = Visibility.Collapsed;
        }
        private async void LoadResourcesAsync(Note Note)
        {
            foreach (StorageFile file in await Library.GetImagesAsync(Note.ID))
                this.Images.Add(file);

            if (Note.Songs.Count == 0)
                this.Player.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            else
            {
                foreach (string songName in Note.Songs)
                {
                    string fileName = songName + ".mp3";
                    Resource song = new Resource(fileName);
                    try
                    {
                        song.File = await Library.GetResourceAsync(ResourceType.Songs, fileName);
                    }
                    catch { }
                    this.Songs.Add(song);
                }
                this.Player.PlayNext();
            }

            foreach (string docName in Note.Docs)
            {
                Resource item = new Resource(docName);
                try
                {
                    item.File = await Library.GetResourceAsync(ResourceType.Docs, docName);
                }
                catch { }
                this.Docs.Add(item);
            }
        }

        private async void Preview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile file = ((Image)sender).Tag as StorageFile;
            this.LargeImage.Source = await file.GetImageSourceAsync();
            this.LargeImageFrame.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
        private void LargeImageFrame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as FrameworkElement).Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void Doc_Click(object sender, ItemClickEventArgs e)
        {
            Resource doc = e.ClickedItem as Resource;
            if(doc.IsAvailable)
                await Launcher.LaunchFileAsync(doc.File);
        }

        public void Edit_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NoteMakerPage), this.ID);
        }

        public void WrapGrid_SizeChanged(object sender, RoutedEventArgs e)
        {
            if (this.WrapGrid.ActualWidth > 600)
            {
                this.Player.Margin = new Thickness(0, 0, 50, 50);
                this.DocBox.SetValue(Grid.ColumnProperty, 1);
                this.DocBox.SetValue(Grid.RowProperty, 0);
            }
            else
            {
                this.Player.Margin = new Thickness(0, 0, 0, 50);
                this.DocBox.SetValue(Grid.ColumnProperty, 0);
                this.DocBox.SetValue(Grid.RowProperty, 1);
            }
        }
    }
}
