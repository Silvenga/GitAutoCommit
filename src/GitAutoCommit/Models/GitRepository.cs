#region Usings

using System;
using System.IO;
using System.Linq;

using GitAutoCommit.Actors;

#endregion

namespace GitAutoCommit.Models {

    public class GitRepository {

        private Func<string> _commitMessage;

        public Func<string> CommitMessage {
            get {
                return _commitMessage
                       ?? (_commitMessage = () => string.Join("; ", this.StagedFiles().Distinct().Take(10)));
            }
            set {
                _commitMessage = value;
            }
        }

        public string WorkingDirectory {
            get;
            set;
        }

        public string Name {
            get {
                return new DirectoryInfo(WorkingDirectory).Name;
            }
        }

        public GitRepository(string workingDirectory) {

            WorkingDirectory = workingDirectory;
        }
    }

}