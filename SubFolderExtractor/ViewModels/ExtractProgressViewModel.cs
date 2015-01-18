using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.VisualBasic.FileIO;
using SubFolderExtractor.Interfaces;
using SubFolderExtractor.Model;
using SubFolderExtractor.Properties;
using Action = System.Action;

namespace SubFolderExtractor.ViewModels
{
    public class ExtractProgressViewModel : Screen
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IOptions options;
        private readonly IEventAggregator eventAggregator;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenRegistration cancellationTokenRegistration;
        private FileSystemWatcher fileSystemWatcher;
        private DirectoryInfo rootDirectory;
        private string currentCompressedFile;
        private int currentDirectoryCount;
        private string currentWorkingFolder;
        private bool isExecuting;
        private string status;
        private int totalDirectoriesCount;
        private bool progressIsIndeterminate;
        private string extractionToolFullPath;

        public ExtractProgressViewModel(IOptions options,
                                        IEventAggregator eventAggregator)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (eventAggregator == null)
                throw new ArgumentNullException("eventAggregator");

            this.options = options;
            this.eventAggregator = eventAggregator;
        }

        private void CreateCancellationTokenSource()
        {
            if (cancellationTokenSource != null)
                cancellationTokenRegistration.Dispose();

            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenRegistration = cancellationTokenSource.Token.Register(() => NotifyOfPropertyChange(() => CanCancel));
        }

        public bool CanCancel
        {
            get { return IsExecuting && !cancellationTokenSource.IsCancellationRequested; }
        }

        public bool CanTryClose
        {
            get { return !IsExecuting; }
        }

        public string CurrentCompressedFile
        {
            get { return currentCompressedFile; }
            private set
            {
                if (currentCompressedFile == value) return;

                currentCompressedFile = value;
                NotifyOfPropertyChange(() => CurrentCompressedFile);
            }
        }

        public string CurrentFolder
        {
            get { return currentWorkingFolder; }
            private set
            {
                if (currentWorkingFolder == value) return;

                currentWorkingFolder = value;
                NotifyOfPropertyChange(() => CurrentFolder);
            }
        }

        public bool IsExecuting
        {
            get { return isExecuting; }
            private set
            {
                if (isExecuting == value) return;

                isExecuting = value;
                NotifyOfPropertyChange(() => IsExecuting);
                NotifyOfPropertyChange(() => CanCancel);
                NotifyOfPropertyChange(() => CanTryClose);
                eventAggregator.PublishOnUIThread(new ExtractionStartedEvent(isExecuting));
            }
        }

        public bool ProgressIsIndeterminate
        {
            get { return progressIsIndeterminate; }
            set
            {
                if (progressIsIndeterminate == value) return;

                progressIsIndeterminate = value;
                NotifyOfPropertyChange(() => ProgressIsIndeterminate);
            }
        }

        public int Progress
        {
            get { return GetCurrentProgress(); }
        }

        public string RootFolder
        {
            get { return rootDirectory == null ? "Not yet set" : rootDirectory.FullName; }
        }

        public string Status
        {
            get { return status; }
            set
            {
                if (status == value) return;

                status = value;
                Logger.Info(status);
                NotifyOfPropertyChange(() => Status);
            }
        }

        /// <summary>
        ///   Cancels the current extraction process
        /// </summary>
        public void Cancel()
        {
            Status = "Cancelling...";
            cancellationTokenSource.Cancel();
            Logger.Info("Cancellation requested for {0} - finishing current extraction and then stopping.", RootFolder);
        }

        /// <summary>
        ///   Starts the extraction process from subfolders to the root folder
        /// </summary>
        public void StartExtraction(string startDirectory)
        {
            if (!Directory.Exists(startDirectory))
                throw new DirectoryNotFoundException(startDirectory);

            // Reset execution variables
            this.rootDirectory = new DirectoryInfo(startDirectory);
            fileSystemWatcher = new FileSystemWatcher(this.rootDirectory.FullName);
            fileSystemWatcher.EnableRaisingEvents = true;

            NotifyOfPropertyChange(() => RootFolder);

            CreateCancellationTokenSource();
            ProgressIsIndeterminate = true; // Change progress bar to cycle during initial extraction to show extraction in progress
            IsExecuting = true;
            currentDirectoryCount = 0;

            Logger.Info("Starting extraction process from root folder {0}", RootFolder);
            Task extractionTask = Task.Factory.StartNew(() =>
            {
                Status = "Finding directories with compressed files...";
                var targetDirectories = GetSubDirectoriesWithCompressedFiles(rootDirectory);
                totalDirectoriesCount = targetDirectories.Count;

                Status = "Running extractions...";
                foreach (var compressedDirectoryFiles in targetDirectories)
                {
                    if (cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        Logger.Info("Cancel requested, stopping on directory {0}.", compressedDirectoryFiles.Directory.FullName);
                        break;
                    }

                    ExtractFromDirectory(compressedDirectoryFiles);
                    currentDirectoryCount++;
                    ProgressIsIndeterminate = false;
                    NotifyOfPropertyChange(() => Progress);
                }
            }, cancellationTokenSource.Token);

            CompleteExtraction(extractionTask);
        }

        private List<CompressedDirectoryFiles> GetSubDirectoriesWithCompressedFiles(DirectoryInfo baseDirectory)
        {
            var compressedDirectoryFiles = new List<CompressedDirectoryFiles>();

            foreach (var directory in baseDirectory.GetDirectories())
            {
                compressedDirectoryFiles.AddRange(GetCompressedFilesInDirectory(directory));
            }

            return compressedDirectoryFiles;
        }

        private List<CompressedDirectoryFiles> GetCompressedFilesInDirectory(DirectoryInfo directory)
        {
            var compressedDirectoryFiles = new List<CompressedDirectoryFiles>();
            if (!directory.Exists) return compressedDirectoryFiles;

            foreach (var compressionExtension in Settings.Default.CompressionExtensions)
            {
                // dont add the same file for r00 if we already have a rar file from the same folder
                // This means we skip r00 when file names are not the same as the rar file but that's not common or very important
                if (compressionExtension == "r00" &&
                    compressedDirectoryFiles.Any(x => x.CompressedFiles[0].Extension == ".rar"))
                {
                    continue;
                }

                string searchFilter = string.Format("*.{0}", compressionExtension);
                var compressedFiles = directory.GetFiles(searchFilter);

                if (compressedFiles.Any())
                    compressedDirectoryFiles.Add(new CompressedDirectoryFiles(directory, compressedFiles));
            }

            return compressedDirectoryFiles;
        }

        protected override void OnDeactivate(bool close)
        {
            if (IsExecuting && cancellationTokenSource.IsCancellationRequested == false)
            {
                cancellationTokenSource.Cancel();
                Logger.Info("User closed window - cancelling extractions from {0}{1}", RootFolder, Environment.NewLine);
            }

            base.OnDeactivate(close);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DisplayName = "Folder Extraction Progress";
        }

        private void CompleteExtraction(Task extractionTask)
        {
            extractionTask.ContinueWith(task =>
            {
                IsExecuting = false;
                ProgressIsIndeterminate = false;
                currentDirectoryCount = totalDirectoriesCount;
                NotifyOfPropertyChange(() => Progress);

                if (task.IsCanceled)
                    Status = "Cancelled Extractions";
                else if (task.IsCompleted)
                    Status = "Extraction completed";
            }).IgnoreExceptions();
        }

        private void ExtractFromDirectory(CompressedDirectoryFiles compressedDirectoryFiles)
        {
            CurrentFolder = compressedDirectoryFiles.Directory.FullName;
            var compressedFiles = GetUnchainedCompressedFiles(compressedDirectoryFiles.CompressedFiles);

            if (compressedFiles.Any())
            {
                ExtractFromFiles(compressedFiles);
                if (cancellationTokenSource.IsCancellationRequested == false && options.DeleteAfterExtract)
                    DeleteDirectory(compressedDirectoryFiles.Directory.FullName);
            }
            else
                Logger.Info("Skipping folder as it does not contain any known compressed file types {0}", CurrentFolder);
        }

        /// <summary>
        ///   Checks compressed files against known chained compressed file naming formats as specified in Settings.Default.ChainedFileRegularExpressions
        ///   <example>myrar.part001, myrar.part002</example>
        /// </summary>
        /// <param name="fileInfos"> Files to check for any known chain names </param>
        /// <returns> Compressed files that are either the start or are not part of a compression chain </returns>
        private List<FileInfo> GetUnchainedCompressedFiles(FileInfo[] fileInfos)
        {
            var unchainedCompressedFiles = new List<FileInfo>();

            var processedFileNames = new HashSet<string>();
            string fileNames = string.Join(Environment.NewLine, fileInfos.Select(e => e.Name));
            foreach (var chainedFileRegularExpression in Settings.Default.ChainedFileRegularExpressions)
            {
                var matches = Regex.Matches(fileNames, chainedFileRegularExpression);
                if (matches.Count == 0) continue; // no matches so nothing to process

                string firstFileName = matches.Cast<Match>().OrderBy(match => match.Value).Select(match => match.Value).FirstOrDefault();

                // Add just the first file in the chain
                unchainedCompressedFiles.Add(fileInfos.First(e => e.Name == firstFileName));

                foreach (Match match in matches)
                {
                    if (!processedFileNames.Contains(match.Value))
                        processedFileNames.Add(match.Value);
                }
            }

            // Add remaining unprocessed files as they must not be part of any known chain
            unchainedCompressedFiles.AddRange(fileInfos.Where(e => !processedFileNames.Contains(e.Name)));
            return unchainedCompressedFiles;
        }

        private void ExtractFromFiles(IEnumerable<FileInfo> fileInfos)
        {
            foreach (var fileInfo in fileInfos)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    Logger.Info("Extraction for file {0} cancelled.", fileInfo.FullName);
                    return;
                }

                CurrentCompressedFile = fileInfo.Name;
                ExtractToRoot(fileInfo);
            }
        }

        private void ExtractToRoot(FileInfo compressedFile)
        {
            var extractionProcess = CreateExtractionProcess(compressedFile);

            string extractedFileFullPath = DiscoverExtractedFileName(() =>
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                    throw new OperationCanceledException(cancellationTokenSource.Token);

                Logger.Info("Extracting from compressed file {0}", compressedFile.FullName);
                extractionProcess.Start();

                WaitOnCompletion(extractionProcess);
                LogExtractionOutput(extractionProcess);
            });

            bool foundExtractedFileName = !string.IsNullOrWhiteSpace(extractedFileFullPath);

            if (!foundExtractedFileName)
                Logger.Warn("Unable to find extracted file name for compressed file: " + compressedFile.FullName);

            if (cancellationTokenSource.IsCancellationRequested)
                DeleteFile(extractedFileFullPath);
            else if (options.RenameToFolder && foundExtractedFileName)
                RenameFileToFolder(extractedFileFullPath, compressedFile.DirectoryName);
        }

        private string DiscoverExtractedFileName(Action action)
        {
            Logger.Trace("Attempting to discover file name");
            var time = DateTime.Now;
            string extractedFile = string.Empty;
            FileSystemEventHandler eventHandler = (sender, fileSystemEventArgs) =>
            {
                extractedFile = fileSystemEventArgs.FullPath;
                Logger.Info("Discovered file name: {0}", extractedFile);
            };

            fileSystemWatcher.Created += eventHandler;
            action.Invoke();
            fileSystemWatcher.Created -= eventHandler;

            // Fallback method as this has issues when the file has just been copied from a different location as the creation date wont be correct
            if (string.IsNullOrWhiteSpace(extractedFile))
            {
                FileInfo newlyCreatedFile = rootDirectory.GetFiles().FirstOrDefault(file => file.CreationTime > time);
                if (newlyCreatedFile != null)
                    extractedFile = newlyCreatedFile.FullName;
            }

            return extractedFile;
        }

        private Process CreateExtractionProcess(FileInfo compressedFile)
        {
            var extractor = new Process();

            var extractionToolPath = GetExtractionToolPath();
            extractor.StartInfo.FileName = extractionToolPath;
            extractor.StartInfo.CreateNoWindow = true;
            extractor.StartInfo.UseShellExecute = false;
            extractor.StartInfo.RedirectStandardOutput = true;
            extractor.StartInfo.RedirectStandardError = true;

            var startArgs = GetStartArgs(compressedFile);
            extractor.StartInfo.Arguments = startArgs;

            return extractor;
        }

        private string GetExtractionToolPath()
        {
            if (!string.IsNullOrEmpty(extractionToolFullPath))
                return extractionToolFullPath;

            string extractionToolPath = Settings.Default.ExtractionToolPath;

            if (!Path.IsPathRooted(extractionToolPath))
            {
                var executingAssemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
                extractionToolPath = Path.Combine(executingAssemblyLocation, Settings.Default.ExtractionToolPath);
            }

            extractionToolFullPath = Path.GetFullPath(extractionToolPath);
            return extractionToolFullPath;
        }

        private void WaitOnCompletion(Process extractionProcess)
        {
            var cancellationRegistration = cancellationTokenSource.Token.Register(() =>
            {
                if (extractionProcess.HasExited) return;

                try
                {
                    Logger.Info("Cancellation requested - cancelling extraction process");
                    extractionProcess.Kill();
                }
                catch (InvalidOperationException ioe)
                {
                    string processAlreadyCompleted = string.Format("Cannot process request because the process ({0}) has exited.", extractionProcess.Id);
                    if (ioe.Message == processAlreadyCompleted)
                        Logger.Info("Attempted to cancel process but extraction had already completed");
                    else
                        Logger.Error(ioe);
                }
            });

            extractionProcess.WaitForExit();
            cancellationRegistration.Dispose();
        }

        private void LogExtractionOutput(Process extractionProcess)
        {
            var errorOutput = extractionProcess.StandardError.ReadToEnd();
            var standardOutput = extractionProcess.StandardOutput.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(errorOutput))
                Logger.Error(errorOutput);

            if (!string.IsNullOrWhiteSpace(standardOutput))
                Logger.Info(standardOutput);
        }

        private int GetCurrentProgress()
        {
            if (totalDirectoriesCount == 0 || totalDirectoriesCount == currentDirectoryCount)
                return 100;

            int currentProgress = (100 / totalDirectoriesCount) * currentDirectoryCount;
            return currentProgress;
        }

        private string GetStartArgs(FileInfo compressedFile)
        {
            var startArgs = Settings.Default.ExtractionCommand;
            startArgs = startArgs.Replace(@"[CompressedFile]", compressedFile.FullName);
            startArgs = startArgs.Replace(@"[Destination]", RootFolder);

            Logger.Info("Extraction arguments {0}", startArgs);
            return startArgs;
        }

        private void RenameFileToFolder(string extractedFileFullPath, string directoryName)
        {
            if (!File.Exists(extractedFileFullPath)) return;

            bool onlyDifferByCase = false;
            if (Path.GetFileNameWithoutExtension(extractedFileFullPath) == Path.GetFileName(directoryName)) // case sensitive comparison
            {
                Logger.Info("No need to rename file as it already matches the folder name");
                return;
            }
            if (Path.GetFileNameWithoutExtension(extractedFileFullPath).IsEqualTo(Path.GetFileName(directoryName)))
            {
                onlyDifferByCase = true;
            }

            var fileInfo = new FileInfo(extractedFileFullPath);
            string newFileName = string.Format("{0}{1}", directoryName, fileInfo.Extension);
            string newFileFullPath = Path.Combine(fileInfo.DirectoryName, newFileName);

            Logger.Info("Renaming file from {0} to {1}", Path.GetFileName(extractedFileFullPath), newFileName);

            if (onlyDifferByCase) // perform case sensitive rename
            {
                var tmpName = Path.Combine(fileInfo.DirectoryName, Path.GetRandomFileName());
                Logger.Info("File names only differ by case, using temporary file to allow case sensitive rename: " + tmpName);

                File.Move(extractedFileFullPath, tmpName);
                File.Move(tmpName, newFileFullPath);
            }
            else if (File.Exists(newFileFullPath)) // Guard against existing file with same name
                Logger.Error(string.Format("Unable to rename file {0} as a file with the same name already exists", newFileFullPath));
            else
                File.Move(extractedFileFullPath, newFileFullPath);
        }

        private void DeleteDirectory(string directoryName)
        {
            try
            {
                Logger.Info("Deleting directory {0}", directoryName);
                FileSystem.DeleteDirectory(directoryName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ExtractErrorMessage);
            }
        }

        private void DeleteFile(string fileFullName)
        {
            try
            {
                Logger.Info("Deleting file {0}", fileFullName);
                FileSystem.DeleteFile(fileFullName, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ExtractErrorMessage);
            }
        }
    }
}