using System;
using System.Dynamic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using SubFolderExtractor.Properties;
using Application = System.Windows.Application;
using Screen = Caliburn.Micro.Screen;

namespace SubFolderExtractor.ViewModels
{
    public class MainViewModel : Screen, IHandle<ExtractionStartedEvent>
    {
        private readonly IWindowManager windowManager;
        private readonly OptionsViewModel optionsViewModel;
        private readonly ExtractProgressViewModel extractProgressViewModel;
        private bool isExecuting;
        private string rootFolder;

        public MainViewModel(IWindowManager windowManager,
                             IEventAggregator eventAggregator,
                             OptionsViewModel optionsViewModel,
                             ExtractProgressViewModel extractProgressViewModel)
        {
            if (windowManager == null) throw new ArgumentNullException("windowManager");
            if (eventAggregator == null) throw new ArgumentNullException("eventAggregator");
            if (optionsViewModel == null) throw new ArgumentNullException("optionsViewModel");
            if (extractProgressViewModel == null) throw new ArgumentNullException("extractProgressViewModel");

            this.windowManager = windowManager;
            this.optionsViewModel = optionsViewModel;
            this.extractProgressViewModel = extractProgressViewModel;

            eventAggregator.Subscribe(this);
        }

        public bool CanBrowse
        {
            get { return !IsExecuting; }
        }

        public bool CanExecuteExtract
        {
            get { return RootFolder != null && !isExecuting && Directory.Exists(RootFolder); }
        }

        public bool CanOptions
        {
            get { return !isExecuting; }
        }
        
        public bool IsExecuting
        {
            get { return isExecuting; }
            set
            {
                if (isExecuting == value)
                    return;
                isExecuting = value;
                NotifyOfPropertyChange(() => IsExecuting);
                NotifyOfPropertyChange(() => CanExecuteExtract);
                NotifyOfPropertyChange(() => CanBrowse);
                NotifyOfPropertyChange(() => CanOptions);
            }
        }
        
        public string RootFolder
        {
            get { return rootFolder; }
            set
            {
                if (rootFolder == value)
                    return;
                rootFolder = value;
                NotifyOfPropertyChange(() => RootFolder);
                NotifyOfPropertyChange(() => CanExecuteExtract);
            }
        }

        public void Browse()
        {
            var folderDialog = new FolderBrowserDialog();
            var dialogResult = folderDialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
                RootFolder = folderDialog.SelectedPath;
        }

        public void ExecuteExtract()
        {
            extractProgressViewModel.DeactivateWith(this);

            windowManager.ShowWindow(extractProgressViewModel);
            extractProgressViewModel.StartExtraction(RootFolder);
        }

        public void Options()
        {
            optionsViewModel.DeactivateWith(this);

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            windowManager.ShowDialog(optionsViewModel, null, settings);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Settings.Default.Save();
            Application.Current.Shutdown();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            DisplayName = "Sub-Folder Extractor";
        }

        public void Handle(ExtractionStartedEvent message)
        {
            IsExecuting = message.IsExtracting;
        }
    }
}