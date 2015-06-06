using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLog;
using SubFolderExtractor.Model;
using LogManager = NLog.LogManager;

namespace SubFolderExtractor.ViewModels
{
    public class ExtractProgressViewModel : Screen
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private bool cancel, progressIsIndeterminate;
        private string status;

        public ExtractProgressViewModel()
        {
            if (!Execute.InDesignMode)
                throw new InvalidOperationException("Default constructor only used for design time");

            Model = new ExtractionModel(new Options(), new EventAggregator());
        }

        public ExtractProgressViewModel(ExtractionModel extractionModel)
        {
            if (extractionModel == null)
                throw new ArgumentNullException("extractionModel");

            this.Model = extractionModel;
            Model.PropertyChanged += ModelOnPropertyChanged;
        }

        public ExtractionModel Model { get; private set; }

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

        public string RootFolder
        {
            get { return Model.RootDirectory == null ? "Not yet set" : Model.RootDirectory.FullName; }
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

        public bool CanTryClose
        {
            get { return !Model.CanCancel; }
        }

        /// <summary>
        ///   Cancels the current extraction process
        /// </summary>
        public void Cancel()
        {
            Status = "Cancelling...";
            Model.Cancel();
            Logger.Info("Cancellation requested for {0} - finishing current extraction and then stopping.", RootFolder);
        }

        /// <summary>
        ///   Starts the extraction process from subfolders to the root folder
        /// </summary>
        public void StartExtraction(string startDirectory)
        {
            if (!Directory.Exists(startDirectory))
                throw new DirectoryNotFoundException(startDirectory);
            
            ProgressIsIndeterminate = true; // Change progress bar to cycle during initial extraction to show extraction in progress
            cancel = false;

            Logger.Info("Starting extraction process from root folder {0}", startDirectory);
            Status = "Running extractions...";
            Task.Factory.StartNew(() => Model.StartExtraction(startDirectory));
        }

        protected override void OnDeactivate(bool close)
        {
            if (Model.IsExecuting && cancel == false)
            {
                cancel = true;
                Logger.Info("User closed window - cancelling extractions from {0}{1}", RootFolder, Environment.NewLine);
            }

            base.OnDeactivate(close);
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DisplayName = "Folder Extraction Progress";
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.IsExecuting))
            {
                if (!Model.IsExecuting)
                {
                    Status = cancel ? "Cancelled Extractions" : "Extraction completed";
                }
                ProgressIsIndeterminate = false;
                NotifyOfPropertyChange(() => CanTryClose);
            }
            else if (e.PropertyName == nameof(Model.RootDirectory))
                NotifyOfPropertyChange(() => RootFolder);
        }
    }
}