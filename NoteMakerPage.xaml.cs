using Hazuwall;
using Hazuwall.Tools;
using Pensieve.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class NoteMakerPage : Page
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

        private long ID = 0;
        public ObservableCollection<NamedFile> NamedImageFiles { get; set; }

        public NoteMakerPage()
        {
            this.NamedImageFiles = new ObservableCollection<NamedFile>();

            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            this.TagBox.ItemsSource = Database.Current.GetResources(ResourceType.Tags);
            this.SongBox.ItemsSource = Database.Current.GetResources(ResourceType.Songs);
            this.DocBox.ItemsSource = Database.Current.GetResources(ResourceType.Docs);
        }
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await StorageHelper.CleanTempFolderAsync();
            if (e.NavigationParameter != null)
            {
                this.pageTitle.Text = "Изменить запись";

                this.ID = (long)e.NavigationParameter;
                Note note = Database.Current.GetNote(ID);
                this.DatePicker.Date = note.Date;
                this.ImportantBox.IsChecked = note.IsImportant;
                this.TitleBox.Text = note.Title;
                this.TextBox.Document.SetText(Windows.UI.Text.TextSetOptions.None,note.Text.Substring(3, note.Text.Length - 7).Replace("</p><p>", "\r\n"));
                this.ExtraBox.Text = note.Extra;

                this.TagBox.SelectedItems = note.Tags.Cast<object>().ToList();
                this.SongBox.SelectedItems = note.Songs.Cast<object>().ToList();
                this.DocBox.SelectedItems = note.Docs.Cast<object>().ToList();

                foreach (StorageFile item in await Library.GetImagesAsync(this.ID))
                {
                    string name = item.Name.Substring(4, item.Name.Length - 8);
                    this.NamedImageFiles.Add(new NamedFile(await StorageHelper.GetTemporaryCopyAsync(item),name));
                }
            }
            else
                this.pageTitle.Text = "Создать запись";
        }
        private async void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            await StorageHelper.CleanTempFolderAsync();
        }

        #region Управление изображениями
        private async void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(".jpg");
            IReadOnlyList<StorageFile> files = await picker.PickMultipleFilesAsync();
            foreach (var file in files)
                this.NamedImageFiles.Add(new NamedFile(await StorageHelper.GetTemporaryCopyAsync(file)));
        }
        private void UpImage_Click(object sender, RoutedEventArgs e)
        {
            NamedFile file = (sender as FrameworkElement).Tag as NamedFile;
            int index = this.NamedImageFiles.IndexOf(file);
            if (index != 0)
                this.NamedImageFiles.Move(index, index - 1);
        }
        private async void CancelImage_Click(object sender, RoutedEventArgs e)
        {
            NamedFile file = (sender as FrameworkElement).Tag as NamedFile;
            this.NamedImageFiles.Remove(file);
            await file.EmbeddedFile.DeleteAsync();
        }
        #endregion

        #region Получение введённых данных
        private Note RetrieveNote()
        {
            Note note = new Note();

            if(this.DatePicker.Date==SpecificDate.Unknown)
                throw new FormatException("Дата не может быть неизвестной");
            note.Date = this.DatePicker.Date;
            note.Title = this.TitleBox.Text;

            string text;
            this.TextBox.Document.GetText(Windows.UI.Text.TextGetOptions.UseCrlf,out text);
            if (String.IsNullOrWhiteSpace(text))
                throw new FormatException("Содержание записи не заполнено");
            note.Text = "<p>" + text.Trim().Replace("\r\n", "</p><p>") + "</p>";

            note.Tags = this.TagBox.SelectedItems.Cast<string>().ToList();
            if (note.Tags.Count == 0)
                throw new FormatException("Не выбрано ни одного ключевого слова");
            note.Songs = this.SongBox.SelectedItems.Cast<string>().ToList();
            note.Docs = this.DocBox.SelectedItems.Cast<string>().ToList();

            note.IsImportant = (bool)this.ImportantBox.IsChecked;
            note.Extra = this.ExtraBox.Text;
            return note;
        }
        private Dictionary<string, StorageFile> RetrieveImageFiles()
        {
            Dictionary<string, StorageFile> imageFiles = new Dictionary<string, StorageFile>();
            for (int i = 0; i < this.NamedImageFiles.Count; i++)
            {
                NamedFile file = this.NamedImageFiles[i];
                string fileName = String.Format("{0:D2}. {1}.jpg", i, file.CustomName);
                imageFiles.Add(fileName, file.EmbeddedFile);
            }
            return imageFiles;
        }
        #endregion

        #region Сохранение и удаление
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Button)sender).IsEnabled = false;
                Note note = this.RetrieveNote();
                bool success = this.ID == 0?
                    Database.Current.AddNote(note)
                    :
                    Database.Current.UpdateNote(this.ID, note);
                if (!success)
                    throw new ArgumentException(String.Format("Запись за {0:dd.MM.yyyy} уже существует",note.Date.DateTime));
                await Library.SetImagesAsync(note.ID, this.RetrieveImageFiles());

                if (this.ID == 0)
                    Frame.Navigate(typeof(ViewPage), note.ID);
                else
                {
                    if (this.ID != note.ID)
                    {
                        await Library.SetImagesAsync(this.ID, null);
                        //Внесение поправок в журнал
                        NavJournalHelper.EditLastEntries(Frame.BackStack, note.ID); //страница записи
                        Frame.GoBack();
                        NavJournalHelper.EditLastEntries(Frame.ForwardStack, note.ID); //страница изменения
                    }
                    else
                        Frame.GoBack(); 
                }
            }
            catch(Exception ex) {
                UIHelper.ShowMessageDialogAsync(ex.Message);
            }
            finally
            {
                ((Button)sender).IsEnabled = true;
            }
        }
        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await UIHelper.ShowWarningDialogAsync(async t =>
            {
                if (this.ID != 0)
                {
                    Database.Current.DeleteNote(this.ID);
                    await Library.SetImagesAsync(this.ID, null);
                }
                Frame.AnchorToLastPageOfType(typeof(MainPage));
            });
            ((Button)sender).IsEnabled = true;
        }
        #endregion
    }
}
