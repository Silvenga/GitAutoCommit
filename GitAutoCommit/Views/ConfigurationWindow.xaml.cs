using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Forms;

using GitAutoCommit.Annotations;
using GitAutoCommit.Models;

namespace GitAutoCommit.Views {

    public partial class ConfigurationWindow : INotifyPropertyChanged {

        public ConfigurationWindow() {

            InitializeComponent();
        }

        public static ConfigurationWindow NewRepository() {

            return new ConfigurationWindow {
                Repository = new GitRepository(Directory.GetCurrentDirectory())
            };
        }

        public GitRepository Repository {
            get;
            set;
        }

        private void OpenPathButton_OnClick(object sender, RoutedEventArgs e) {

            var folderBrowserDialog = new FolderBrowserDialog {
                Description = "Select Git Repository",
                ShowNewFolderButton = false,
                SelectedPath = Repository.WorkingDirectory
            };

            folderBrowserDialog.ShowDialog();

            Repository.WorkingDirectory = folderBrowserDialog.SelectedPath;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {

            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
