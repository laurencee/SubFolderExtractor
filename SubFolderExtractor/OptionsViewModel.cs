using System;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using SubFolderExtractor.Interfaces;
using SubFolderExtractor.Properties;

namespace SubFolderExtractor
{
    public class OptionsViewModel : Screen
    {
        private const string ContextMenuDisplayText = "Execute Sub-Folder Extractor";
        private readonly IContextMenuRegistrator _contextMenuRegistrator;
        private readonly IOptions _options;

        private bool _isContextMenuRegistered;
        private bool _canModifyContextMenuRegistration;

        public OptionsViewModel(IOptions options, IContextMenuRegistrator contextMenuRegistrator)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (contextMenuRegistrator == null)
                throw new ArgumentNullException("contextMenuRegistrator");

            CanModifyContextMenuRegistration = false;
            _options = options;
            _contextMenuRegistrator = contextMenuRegistrator;
        }

        private void OnRegistrationExceptionEvent(object sender, RegistrationExceptionEvent registrationExceptionEvent)
        {
            MessageBox.Show(registrationExceptionEvent.Exception.ExtractErrorMessage(), "Registration Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            CheckRegistration();
            _contextMenuRegistrator.RegistrationExceptionEvent += OnRegistrationExceptionEvent;
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _contextMenuRegistrator.RegistrationExceptionEvent -= OnRegistrationExceptionEvent;
        }

        private void CheckRegistration()
        {
            CanModifyContextMenuRegistration = false;
            Task.Factory.StartNew(() => _contextMenuRegistrator.IsContextMenuRegistered())
            .ContinueWith(task =>
            {
                var registrationState = new RegistrationState();
                registrationState.IsContextMenuRegistered = task.Result;
                registrationState.IsRegistrationLocationCorrect = _contextMenuRegistrator.IsRegistrationLocationCorrect();

                return registrationState;
            })
            .ContinueWith(task => Execute.OnUIThread(() =>
            {
                if (task.IsFaulted && task.Exception != null)
                    throw task.Exception;

                RegistrationState registrationState = task.Result;

                // Fix registration location if necessary
                if (registrationState.IsContextMenuRegistered && registrationState.IsRegistrationLocationCorrect == false)
                {
                    var msgBoxResult = MessageBox.Show("Registration location doesn't match current location, do you want to fix this?", "Context Menu Registration Incorrect", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        _contextMenuRegistrator.RemoveRegistration();
                        _contextMenuRegistrator.AddRegistration(ContextMenuDisplayText);
                    }
                }

                IsContextMenuRegistered = task.Result.IsContextMenuRegistered;
                CanModifyContextMenuRegistration = true;
            }));
        }

        public IOptions Options
        {
            get { return _options; }
        }

        public bool RenameToFolder
        {
            get { return _options.RenameToFolder; }
            set
            {
                _options.RenameToFolder = value;
                NotifyOfPropertyChange(() => RenameToFolder);
            }
        }

        public bool DeleteAfterExtract
        {
            get { return _options.DeleteAfterExtract; }
            set
            {
                _options.DeleteAfterExtract = value;
                NotifyOfPropertyChange(() => DeleteAfterExtract);
            }
        }

        public bool IsContextMenuRegistered
        {
            get { return _isContextMenuRegistered; }
            set
            {
                if (_isContextMenuRegistered == value) return;
                _isContextMenuRegistered = value;
                NotifyOfPropertyChange(() => IsContextMenuRegistered);
                NotifyOfPropertyChange(() => ContextMenuRegistrationText);
                NotifyOfPropertyChange(() => ContextMenuRegistrationToolTip);
            }
        }

        public string ContextMenuRegistrationText
        {
            get
            {
                if (CanModifyContextMenuRegistration)
                    return IsContextMenuRegistered ? "Unregister Context Menu" : "Register Context Menu";

                return "Checking for registration...";
            }
        }

        public string ContextMenuRegistrationToolTip
        {
            get 
            {
                if (CanModifyContextMenuRegistration)
                    return IsContextMenuRegistered ? Resources.ContextMenuRegisteredTooltip : Resources.ContextMenuNotRegisteredTooltip;

                return Resources.ContextMenuCheckingRegistrationTooltip;
            }
        }


        public bool CanModifyContextMenuRegistration
        {
            get { return _canModifyContextMenuRegistration; }
            set
            {
                if (_canModifyContextMenuRegistration == value) return;
                _canModifyContextMenuRegistration = value;
                NotifyOfPropertyChange(() => CanModifyContextMenuRegistration);
                NotifyOfPropertyChange(() => ContextMenuRegistrationText);
                NotifyOfPropertyChange(() => ContextMenuRegistrationToolTip);
            }
        }

        public void ModifyContextMenuRegistration()
        {
            if (!CanModifyContextMenuRegistration) return;

            CanModifyContextMenuRegistration = false;

            if (IsContextMenuRegistered)
                _contextMenuRegistrator.RemoveRegistration();
            else
                _contextMenuRegistrator.AddRegistration(ContextMenuDisplayText);

            CheckRegistration();
        }

        private class RegistrationState
        {
            public bool IsContextMenuRegistered;
            public bool IsRegistrationLocationCorrect;
        }
    }
}
