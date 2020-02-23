using Hazuwall;
using Hazuwall.Controls;
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
    public sealed partial class ResourcesPage : Page
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

        private EditableTextBlock SelectedBlock;
        private ResourceType ResType;
        private string[] Types = new string[] { "Ключевые слова", "Музыка", "Документы" };

        public ObservableCollection<Resource> Items { get; set; }

        public ResourcesPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            this.Items = new ObservableCollection<Resource>();
            this.TypeBox.ItemsSource = this.Types;
            this.TypeBox.SelectedIndex = 0;
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private async void TypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TypeBox.SelectedIndex != -1)
            {
                this.ResType = (ResourceType)this.TypeBox.SelectedIndex;
                this.Items.Clear();
                foreach (string itemName in Database.Current.GetResources(this.ResType))
                {
                    Resource item = new Resource(itemName);
                    if (this.ResType != ResourceType.Tags)
                    {
                        try
                        {
                            string fileName = itemName;
                            if (this.ResType == ResourceType.Songs)
                                fileName += ".mp3";
                            item.File = await Library.GetResourceAsync(this.ResType, fileName);
                        }
                        catch { }
                    }
                    this.Items.Add(item);
                }
                this.pageTitle.Text = this.Types[(int)this.ResType];

                if(this.ResType == ResourceType.Tags) {
                    this.DeleteFileButton.Visibility = Visibility.Collapsed;
                    this.TagBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    this.DeleteFileButton.Visibility = Visibility.Visible;
                    this.TagBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private async void Item_Click(object sender, ItemClickEventArgs e)
        {
            Resource item = e.ClickedItem as Resource;
            if (item.IsAvailable)
                await Launcher.LaunchFileAsync(item.File);
        }

        private void Item_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            this.SelectedBlock = (sender as Grid).Children[0] as EditableTextBlock;
            this.ContextMenu.ShowAt((FrameworkElement)sender);
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            if (this.ResType == ResourceType.Tags)
            {
                string name = this.TagBox.Text;
                if(String.IsNullOrWhiteSpace(name))
                    UIHelper.ShowMessageDialogAsync("Имя не заполнено");
                else if (Database.Current.AddResource(ResourceType.Tags, name))
                    this.Items.Insert(0, new Resource(name));
                else
                    UIHelper.ShowMessageDialogAsync("Ключевое слово с таким именем уже существует");
                this.TagBox.Text = String.Empty;
            }
            else
            {
                foreach (StorageFile file in await Library.PickAndAddResourcesAsync(this.ResType))
                {
                    string name = file.Name;
                    //Названия песен хранятся в базе данных без расширения
                    if (this.ResType == ResourceType.Songs)
                        name = Path.GetFileNameWithoutExtension(name);
                    if (Database.Current.AddResource(this.ResType, name))
                    {
                        Resource item = new Resource(name);
                        item.File = file;
                        this.Items.Insert(0, item);
                    }
                }
            }
        }

        #region Изменить
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedBlock.TextEdited += this.SelectedBlock_TextEdited;
            this.ContextMenu.Closed += this.ContextMenu_ClosedOnEditing;
        }
        private void ContextMenu_ClosedOnEditing(object sender, object e)
        {
            (sender as FlyoutBase).Closed -= this.ContextMenu_ClosedOnEditing;
            this.SelectedBlock.Edit();
            this.SelectedBlock = null;
        }
        private void SelectedBlock_TextEdited(object sender, TextEditedEventArgs e)
        {
            try
            {
                EditableTextBlock block = sender as EditableTextBlock;
                block.TextEdited -= this.SelectedBlock_TextEdited;
                if (e.OldValue == e.NewValue)
                    return;

                if(String.IsNullOrWhiteSpace(e.NewValue))
                    throw new FormatException("Имя не заполнено");
                
                if(this.ResType != ResourceType.Tags && (e.NewValue.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || e.NewValue.Last()=='.'))
                    throw new FormatException("Недопустимое имя файла");
                
                if (!Database.Current.UpdateResource(this.ResType, e.OldValue, e.NewValue))
                    throw new ArgumentException("Ресурс с именем " + e.NewValue + " уже существует. Выберите другое название");

                if (this.ResType != ResourceType.Tags)
                {
                    string oldName = e.OldValue;
                    string newName = e.NewValue;
                    //Названия песен хранятся в базе данных без расширения
                    if (this.ResType == ResourceType.Songs)
                    {
                        oldName += ".mp3";
                        newName += ".mp3";
                    }
                    AsyncHelper.RunSync(() => this.RenameIfExistAsync(oldName, newName, this.ResType));
                }
                //Так как не работает двусторонняя привязка
                (block.Tag as Resource).FullName = e.NewValue;
            }
            catch (Exception ex)
            {
                e.IsValid = false;
                UIHelper.ShowMessageDialogAsync(ex.Message);
            }
        }
        /// <summary>
        /// Изменить имя файла ресурса, если он присутствует в библиотеке, с заменой при совпадении имён
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task RenameIfExistAsync(string oldName, string newName, ResourceType type)
        {
            try
            {
                StorageFile file = await Library.GetResourceAsync(type, oldName);
                await file.RenameAsync(newName, NameCollisionOption.ReplaceExisting);
            }
            catch { }
        }
        #endregion

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Resource item = this.SelectedBlock.Tag as Resource;
            this.SelectedBlock = null;
            UIHelper.ShowWarningDialogAsync(t =>
            {
                if (Database.Current.DeleteResource(this.ResType, item.FullName))
                {
                    this.Items.Remove(item);
                    if (item.IsAvailable)
                    {
                        string fileName = item.FullName;
                        if (this.ResType == ResourceType.Songs)
                            fileName += ".mp3";
                        AsyncHelper.RunSync(() => Library.DeleteResourceAsync(this.ResType, fileName));
                    }
                }
                else
                    UIHelper.ShowMessageDialogAsync("Ресурс " + item.FullName + " используется в дневнике и не может быть удалён");
            });
        }
        private async void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            Resource item = this.SelectedBlock.Tag as Resource;
            this.SelectedBlock = null;
            if (item.IsAvailable)
            {
                string fileName = item.FullName;
                if (this.ResType == ResourceType.Songs)
                    fileName += ".mp3";
                await Library.DeleteResourceAsync(this.ResType, fileName);
                item.File = null;
                int index = this.Items.IndexOf(item);
                this.Items.RemoveAt(index);
                this.Items.Insert(index, item);
            }
        }
    }
}