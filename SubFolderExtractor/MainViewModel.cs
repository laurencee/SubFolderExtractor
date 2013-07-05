using System;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using SubFolderExtractor.Properties;
using Application = System.Windows.Application;
using Screen = Caliburn.Micro.Screen;

namespace SubFolderExtractor
{
    [Export(typeof(MainViewModel))]
    public class MainViewModel : Screen, IHandle<ExtractionStartedEvent>
    {
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly OptionsViewModel _optionsViewModel;
        private readonly ExtractProgressViewModel _extractProgressViewModel;
        private bool _isExecuting;
        private string _rootFolder;

        [ImportingConstructor]
        public MainViewModel(IWindowManager windowManager,
                             IEventAggregator eventAggregator,
                             OptionsViewModel optionsViewModel,
                             ExtractProgressViewModel extractProgressViewModel)
        {
            if (windowManager == null)
                throw new ArgumentNullException("windowManager");
            if (eventAggregator == null)
                throw new ArgumentNullException("eventAggregator");
            if (optionsViewModel == null)
                throw new ArgumentNullException("optionsViewModel");
            if (extractProgressViewModel == null)
                throw new ArgumentNullException("extractProgressViewModel");

            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _optionsViewModel = optionsViewModel;
            _extractProgressViewModel = extractProgressViewModel;

            _eventAggregator.Subscribe(this);
        }

        public bool CanBrowse
        {
            get { return !IsExecuting; }
        }

        public bool CanExecuteExtract
        {
            get { return RootFolder != null && !_isExecuting && Directory.Exists(RootFolder); }
        }

        public bool CanOptions
        {
            get { return !_isExecuting; }
        }
        
        public bool IsExecuting
        {
            get { return _isExecuting; }
            set
            {
                if (_isExecuting == value)
                    return;
                _isExecuting = value;
                NotifyOfPropertyChange(() => IsExecuting);
                NotifyOfPropertyChange(() => CanExecuteExtract);
                NotifyOfPropertyChange(() => CanBrowse);
                NotifyOfPropertyChange(() => CanOptions);
            }
        }
        
        public string RootFolder
        {
            get { return _rootFolder; }
            set
            {
                if (_rootFolder == value)
                    return;
                _rootFolder = value;
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
            _extractProgressViewModel.DeactivateWith(this);

            _windowManager.ShowWindow(_extractProgressViewModel);
            _extractProgressViewModel.StartExtraction(RootFolder);
        }

        public void Options()
        {
            _optionsViewModel.DeactivateWith(this);

            dynamic settings = new ExpandoObject();
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _windowManager.ShowDialog(_optionsViewModel, null, settings);
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