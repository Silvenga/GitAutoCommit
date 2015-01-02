#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using GitAutoCommit.Models;

#endregion

namespace GitAutoCommit.Actors {

    public static class Git {

        public static ProcessStartInfo CreateGit(string args, string workingPath) {

            return new ProcessStartInfo {
                CreateNoWindow = true,
                Arguments = args,
                UseShellExecute = false,
                WorkingDirectory = workingPath,
                FileName = "git.exe",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
        }

        public static Process Run(this ProcessStartInfo info) {

            var process = Process.Start(info);

            if(process == null) {
                throw new Exception("Program was not started.");
            }

            process.WaitForExit();

            return process;
        }

        public static void AddAll(this GitRepository repository) {

            var args = string.Format("add -A :/");
            var startInfo = CreateGit(args, repository.WorkingDirectory);
            startInfo.Run().Dispose();
        }

        public static void Commit(this GitRepository repository) {

            var commitMessage = repository.CommitMessage();

            if(!string.IsNullOrWhiteSpace(commitMessage)) {

                var args = string.Format("commit -m \"{0}\"", commitMessage);
                var startInfo = CreateGit(args, repository.WorkingDirectory);
                startInfo.Run().Dispose();
            }
        }

        public static bool HasChanges(this GitRepository repository) {

            var args = string.Format("status -z");
            var startInfo = CreateGit(args, repository.WorkingDirectory);

            using(var info = startInfo.Run()) {
                return !string.IsNullOrWhiteSpace(info.StandardOutput.ReadToEnd());
            }
        }

        public static IEnumerable<string> StagedFiles(this GitRepository repository) {

            var args = string.Format("diff-index --name-only --exit-code --cached -z HEAD");
            var startInfo = CreateGit(args, repository.WorkingDirectory);
            
            using(var info = startInfo.Run()) {

                var stdOut = info.StandardOutput;

                while(!stdOut.EndOfStream) {

                    var line = stdOut.ReadLine();

                    if(!string.IsNullOrWhiteSpace(line)) {

                        yield return line;
                    }
                }
            }
        }

    }

}