using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using NLog;
using SevenZip;
using SubFolderExtractor.Interfaces;
using LogManager = NLog.LogManager;

namespace SubFolderExtractor.Model
{
    public class ExtractionModel : PropertyChangedBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IOptions options;
        private readonly IEventAggregator eventAggregator;
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        private string currentWorkingFolder, extractedFileFullPath;
        private bool cancel, complete, isExecuting;
        private byte percentDone;
        private int totalDirectoriesCount, currentDirectoryCount;
        private FileInfo currentCompressedFile;
        private DirectoryInfo rootDirectory;
        private SevenZipExtractor extractor;

        public ExtractionModel(
            IOptions options,
            IEventAggregator eventAggregator)
        {
            this.options = options;
            this.eventAggregator = eventAggregator;
        }

        public DirectoryInfo RootDirectory
        {
            get { return rootDirectory; }
            private set
            {
                if (Equals(value, rootDirectory)) return;
                rootDirectory = value;
                NotifyOfPropertyChange(() => RootDirectory);
            }
        }

        public int MaxProgress
        {
            get { return totalDirectoriesCount * 100; }
        }

        public FileInfo CurrentCompressedFile
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
                eventAggregator.PublishOnUIThread(new ExtractionStartedEvent(isExecuting));
            }
        }

        public bool CanCancel
        {
            get { return IsExecuting && !cancel; }
        }

        public int Progress
        {
            get { return GetCurrentProgress(); }
        }

        /// <summary> Blocking call, run in a background thread to allow UI updates </summary>
        /// <param name="startDirectory">The root directory to begin the extraction process from</param>
        public void StartExtraction(string startDirectory)
        {
            if (!Directory.Exists(startDirectory))
                throw new DirectoryNotFoundException(startDirectory);

            RootDirectory = new DirectoryInfo(startDirectory);

            complete = false;
            cancel = false;
            currentDirectoryCount = 0;

            var targetDirectories = FindCompressedFiles.GetSubDirectoriesWithCompressedFiles(RootDirectory);
            totalDirectoriesCount = targetDirectories.Count;
            NotifyOfPropertyChange(() => MaxProgress);
            IsExecuting = true;

            foreach (var compressedDirectoryFiles in targetDirectories)
            {
                if (cancel)
                {
                    Logger.Info("Cancel requested, stopping on directory {0}.", compressedDirectoryFiles.Directory.FullName);
                    break;
                }

                autoResetEvent.WaitOne(); // ensure only 1 extraction occurs at a time
                ExtractFromDirectory(compressedDirectoryFiles);
                NotifyOfPropertyChange(() => Progress);
            }

            if (totalDirectoriesCount == 0) IsExecuting = false;
        }

        public void Cancel()
        {
            cancel = true;
            NotifyOfPropertyChange(() => CanCancel);
        }

        public void ExtractFromDirectory(CompressedDirectoryFiles compressedDirectoryFiles)
        {
            CurrentFolder = compressedDirectoryFiles.Directory.FullName;
            var compressedFiles = FindCompressedFiles.GetUnchainedCompressedFiles(compressedDirectoryFiles.CompressedFiles);

            if (compressedFiles.Any())
            {
                ExtractFromFiles(compressedFiles);
            }
            else
                Logger.Info("Skipping folder as it does not contain any known compressed file types {0}", CurrentFolder);
        }

        private void ExtractFromFiles(IEnumerable<FileInfo> fileInfos)
        {
            foreach (var fileInfo in fileInfos)
            {
                if (cancel)
                {
                    Logger.Info("Extraction for file {0} cancelled.", fileInfo.FullName);
                    autoResetEvent.Set();
                    return;
                }

                CurrentCompressedFile = fileInfo;
                ExtractToRoot(fileInfo);
            }
        }

        private void ExtractToRoot(FileInfo compressedFile)
        {
            extractor = new SevenZipExtractor(compressedFile.FullName);
            extractor.Extracting += ExtractionProgress;
            extractor.FileExtractionStarted += ExtractionStarted;
            extractor.FileExists += (sender, args) => { args.Cancel = cancel; };
            extractor.ExtractionFinished += ExtractionFinished;
            extractor.BeginExtractArchive(RootDirectory.FullName);
        }

        private void ExtractionStarted(object sender, FileInfoEventArgs e)
        {
            IsExecuting = true;
            extractedFileFullPath = Path.Combine(RootDirectory.FullName, e.FileInfo.FileName);
            e.Cancel = cancel;
            NotifyOfPropertyChange(() => Progress);
        }

        private void ExtractionFinished(object sender, EventArgs e)
        {
            extractor.Dispose();
            if (cancel)
            {
                IOLibrary.DeleteFile(extractedFileFullPath);
                IsExecuting = false;
            }
            else
            {
                if (options.RenameToFolder)
                    IOLibrary.RenameFileToFolder(extractedFileFullPath, CurrentCompressedFile.DirectoryName);
                if (options.DeleteAfterExtract)
                    IOLibrary.DeleteDirectory(CurrentFolder);
            }

            percentDone = 0;
            currentDirectoryCount++;
            complete = totalDirectoriesCount == currentDirectoryCount;

            if (complete)
                IsExecuting = false;
            else
                autoResetEvent.Set(); // allow the next extraction to start (if there is one)

            NotifyOfPropertyChange(() => Progress);
        }

        private void ExtractionProgress(object sender, ProgressEventArgs e)
        {
            e.Cancel = cancel;
            percentDone = e.PercentDone;
            NotifyOfPropertyChange(() => Progress);
        }

        private int GetCurrentProgress()
        {
            if (totalDirectoriesCount == 0 || totalDirectoriesCount == currentDirectoryCount || complete)
                return MaxProgress;

            return (currentDirectoryCount * 100) + percentDone;
        }
    }
}
