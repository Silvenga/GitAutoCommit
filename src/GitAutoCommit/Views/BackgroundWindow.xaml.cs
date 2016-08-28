#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using GitAutoCommit.Actors;
using GitAutoCommit.Models;

using Application = System.Windows.Application;

#endregion

namespace GitAutoCommit.Views {

    public partial class BackgroundWindow {

        public BackgroundWindow() {
            InitializeComponent();

            NotifyIcon = new NotifyIcon {
                Visible = false
            };

            RepositoryWatchers = new List<RepositoryWatcher>();

            SetupRepositoriesFromCli();
            UpdateMenu();
        }

        public static NotifyIcon NotifyIcon {
            get;
            private set;
        }

        public List<RepositoryWatcher> RepositoryWatchers {
            get;
            private set;
        }

        public void SetupRepositoriesFromCli() {

            foreach(var workingDirectory in Environment.GetCommandLineArgs().Where(IsGitRepository)) {

                AddRepository(workingDirectory);
            }
        }

        public void UpdateMenu() {

            var exit = new MenuItem {
                DefaultItem = true,
                Text = "Exit"
            };
            exit.Click += (s, a) => Exit();

            var isChanging = false;
            var items = new List<MenuItem>();

            foreach(var watcher in RepositoryWatchers) {

                var repo = watcher.Repository;
                var localWatcher = watcher;

                var name = repo.Name;
                var updated = WatcherStatus(localWatcher);

                var item = new MenuItem {
                    Text = string.Format("{0} - {1}", name, updated),
                    Checked = localWatcher.IsWatching
                };

                item.Click += delegate {
                    localWatcher.IsWatching = !localWatcher.IsWatching;
                    UpdateMenu();
                    Console.WriteLine("{0} IsWatching: {1}", repo.Name, localWatcher.IsWatching);
                };

                items.Add(item);
                isChanging |= localWatcher.IsChanging;
            }

            items.Add(exit);

            NotifyIcon.Visible = true;
            NotifyIcon.Text = isChanging ? "Commiting changes" : "Wating for changes...";
            NotifyIcon.Icon = isChanging ? Properties.Resources.git_icon_active : Properties.Resources.git_icon;
            NotifyIcon.ContextMenu = new ContextMenu(items.ToArray());
        }

        private static string WatcherStatus(RepositoryWatcher watcher) {

            var text = "Paused";

            if(!watcher.IsWatching) {

                text = watcher.IsChanging
                      ? "Commiting Changes"
                      : string.Format("Last Updated: {0}", watcher.LastChange.ToString("h:mm:ss tt"));
            }

            return text;
        }

        private void RepoOnChangeStart(RepositoryWatcher repositoryWatcher, GitRepository gitRepository) {

            Console.WriteLine("Changed Start.");
            UpdateMenu();
        }

        private void RepoOnChangeEnd(RepositoryWatcher repositoryWatcher, GitRepository gitRepository) {

            Console.WriteLine("Changed End.");
            UpdateMenu();
            gitRepository.AddAll();
            gitRepository.Commit();
        }

        public static void Exit() {

            Application.Current.Shutdown();
        }

        private static bool IsGitRepository(string path) {

            return Directory.Exists(path) && Directory.EnumerateDirectories(path).Any(x => x.EndsWith(Path.DirectorySeparatorChar + ".git"));
        }

        public bool AddRepository(string workingDirectory) {

            if(RepositoryWatchers.Any(x => x.Repository.WorkingDirectory == workingDirectory)) {
                return false;
            }

            var gitRepository = new GitRepository(workingDirectory);
            var watcher = new RepositoryWatcher(gitRepository);

            Console.WriteLine("Starting {0} ({1})", gitRepository.Name, gitRepository.WorkingDirectory);

            watcher.ChangeStart += RepoOnChangeStart;
            watcher.ChangeEnd += RepoOnChangeEnd;
            watcher.IsWatching = true;

            RepositoryWatchers.Add(watcher);

            return true;
        }

        public bool RemoveRepository(string workingDirectory) {

            var watcher = RepositoryWatchers.FirstOrDefault(x => x.Repository.WorkingDirectory != workingDirectory);

            if(watcher == null) {
                return false;
            }

            Console.WriteLine("Stopping {0} ({1})", watcher.Repository.Name, watcher.Repository.WorkingDirectory);

            watcher.Dispose();

            return RepositoryWatchers.Remove(watcher);
        }

    }

}