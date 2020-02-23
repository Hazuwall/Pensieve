using System;
using System.Collections.Generic;
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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace Pensieve
{
    public sealed partial class BackupSettingsFlyout : SettingsFlyout
    {
        public bool IsLoading
        {
            get { return this.Ring.IsActive; }
            set { this.Ring.IsActive = value; }
        }

        public BackupSettingsFlyout()
        {
            this.InitializeComponent();
            this.IsLoading = false;
        }

        private void ImportDatabase_Click(object sender, RoutedEventArgs e)
        {
            this.TryExecute(Database.Current.ImportAsync);
        }
        private void ExportDatabase_Click(object sender, RoutedEventArgs e)
        {
            this.TryExecute(Database.Current.ExportAsync);
        }
        private void ImportLibrary_Click(object sender, RoutedEventArgs e)
        {
            this.TryExecute(Library.ImportAsync);
        }
        private void ExportLibrary_Click(object sender, RoutedEventArgs e)
        {
            this.TryExecute(()=>Library.ExportAsync((bool)this.MusicCheckBox.IsChecked));
        }
        private void ScanLibrary_Click(object sender, RoutedEventArgs e)
        {
            this.TryExecute(()=>this.ScanLibrary());
        }
        private async Task<bool> ScanLibrary()
        {
            foreach (string name in await Library.GetResourceListAsync(ResourceType.Songs))
                Database.Current.AddResource(ResourceType.Songs, Path.GetFileNameWithoutExtension(name));
            foreach (string name in await Library.GetResourceListAsync(ResourceType.Docs))
                Database.Current.AddResource(ResourceType.Docs, name);
            return true;
        }

        private async void TryExecute(Func<Task<bool>>Operation)
        {
            if (this.IsLoading == true)
                this.ResultBox.Text = "Дождитесь выполнения";
            else
            {
                this.ResultBox.Text = String.Empty;
                this.IsLoading = true;
                if (await Operation.Invoke())
                    this.ResultBox.Text = "Успешно выполнено";
                else
                    this.ResultBox.Text = "Не выполнено";
                this.Show();
                this.IsLoading = false;
            }
        }
    }
}
