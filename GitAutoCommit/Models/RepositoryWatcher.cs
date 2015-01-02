#region Usings

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using GitAutoCommit.Actors;

#endregion

namespace GitAutoCommit.Models {

    public class RepositoryWatcher : IDisposable {

        private bool _isChanging;

        public const NotifyFilters AllChanges =
            NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        public RepositoryWatcher(GitRepository repository) {

            Repository = repository;

            Watcher = new FileSystemWatcher {
                Path = Repository.WorkingDirectory,
                NotifyFilter = AllChanges,
                IncludeSubdirectories = true
            };

            LastChange = DateTime.Now;

            Watcher.Changed += OnChanged;
            Watcher.Created += OnCreated;
            Watcher.Deleted += OnDeleted;
            Watcher.Renamed += OnRenamed;
        }

        public bool IsDisposed {
            get;
            private set;
        }

        private FileSystemWatcher Watcher {
            get;
            set;
        }

        public GitRepository Repository {
            get;
            private set;
        }

        public DateTime LastChange {
            get;
            private set;
        }

        public bool IsWatching {
            get {
                return !IsDisposed && Watcher.EnableRaisingEvents;
            }
            set {
                if(IsDisposed) {
                    throw new Exception("Cannot set IsWatching after calling disposed.");
                }
                Watcher.EnableRaisingEvents = value;
            }
        }

        public bool IsChanging {
            get {
                return _isChanging;
            }
            private set {
                if(_isChanging != value) {

                    _isChanging = value;

                    if(_isChanging) {
                        OnChangeStart();
                    } else {
                        OnChangeEnd();
                    }
                }
            }
        }

        public event Action<RepositoryWatcher, GitRepository> ChangeStart;

        protected virtual void OnChangeStart() {

            var handler = ChangeStart;
            if(handler != null) {
                handler(this, Repository);
            }
        }

        public event Action<RepositoryWatcher, GitRepository> ChangeEnd;

        protected virtual void OnChangeEnd() {

            var handler = ChangeEnd;
            if(handler != null) {
                handler(this, Repository);
            }
        }

        protected virtual void OnChange() {

            var sleepTime = TimeSpan.FromSeconds(5);

            Task.Run(
                delegate {

                    IsChanging = true;
                    LastChange = DateTime.Now;

                    Thread.Sleep(sleepTime);

                    var lastChange = DateTime.Now - LastChange;
                    IsChanging = lastChange <= sleepTime;

                });
        }

        private void OnChanged(object source, FileSystemEventArgs e) {

            if(IsRaisable(e.FullPath)) {
                OnChange();
            }
        }

        private void OnCreated(object source, FileSystemEventArgs e) {

            if(IsRaisable(e.FullPath) && !Directory.Exists(e.FullPath)) {
                OnChange();
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e) {

            if(IsRaisable(e.FullPath)) {
                OnChange();
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e) {

            if(IsRaisable(e.FullPath)) {
                OnChange();
            }
        }

        private bool IsRaisable(string fullPath) {

            return !fullPath.Contains(".git") && (Repository.HasChanges());
        }

        public void Dispose() {

            IsDisposed = true;
            _isChanging = false;

            Watcher.Changed -= OnChanged;
            Watcher.Created -= OnCreated;
            Watcher.Deleted -= OnDeleted;
            Watcher.Renamed -= OnRenamed;

            Watcher.Dispose();
            Watcher = null;
        }

    }

}