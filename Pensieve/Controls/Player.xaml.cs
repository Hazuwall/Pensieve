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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pensieve
{
    public sealed partial class Player : UserControl
    {
        private static void _onPlayListChanged(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
        {
            var player = (Player)d;
            if(player.IsAutoplay)
                player.PlayNext();
        }
        private static readonly DependencyProperty _PlayListProperty = DependencyProperty.Register(
            "PlayList", typeof(ObservableCollection<Resource>), typeof(Player), new PropertyMetadata(null, _onPlayListChanged));
        public static DependencyProperty PlayListProperty { get { return _PlayListProperty; } }


        private int current = -1;

        public ObservableCollection<Resource> PlayList
        {
            get { return (ObservableCollection<Resource>)this.GetValue(PlayListProperty); }
            set { this.SetValue(PlayListProperty, value); System.Diagnostics.Debug.WriteLine("f"); }
        }
        public bool IsAutoplay { get; set; } = false;

        public Player()
        {
            this.InitializeComponent();
            Binding binding = new Binding()
            {
                Source = this,
                Path = new PropertyPath("PlayList")
            };
            this.ListBox.SetBinding(ListView.ItemsSourceProperty, binding);
        }

        private async void StartPlay(StorageFile songFile)
        {
            var stream = await songFile.OpenAsync(FileAccessMode.Read);
            this.MediaBox.SetSource(stream, songFile.ContentType);
            this.MediaBox.Play();
        }

        /// <summary>
        /// Начать проигрывать следующую песню
        /// </summary>
        /// <returns></returns>
        public void PlayNext()
        {
            current++;
            if (this.PlayList == null || this.PlayList.Count == 0 || current >= this.PlayList.Count)
            {
                current = -1;
            }
            else
            {
                Resource currentSong = this.PlayList[current];
                this.ListBox.SelectedItem = currentSong;
                if (!currentSong.IsAvailable)
                    this.PlayNext();
                else
                    this.StartPlay(currentSong.File);
            }
        }
        private void MediaBox_Ended(object sender, RoutedEventArgs e)
        {
            this.PlayNext();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                Resource song = e.AddedItems[0] as Resource;
                if (song.IsAvailable)
                {
                    this.current = this.PlayList.IndexOf(song);
                    this.StartPlay(song.File);
                }
            }
        }
    }
}
