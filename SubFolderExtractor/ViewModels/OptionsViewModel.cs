using System;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using SubFolderExtractor.Interfaces;
using SubFolderExtractor.Properties;

namespace SubFolderExtractor.ViewModels
{
    public class OptionsViewModel : Screen
    {
        private const string ContextMenuDisplayText = "Execute Sub-Folder Extractor";
        private readonly IContextMenuRegistrator contextMenuRegistrator;
        private readonly IOptions options;

        private bool isContextMenuRegistered;
        private bool canModifyContextMenuRegistration;

        public OptionsViewModel(IOptions options, IContextMenuRegistrator contextMenuRegistrator)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (contextMenuRegistrator == null) throw new ArgumentNullException("contextMenuRegistrator");
            
            CanModifyContextMenuRegistration = false;
            this.options = options;
            this.contextMenuRegistrator = contextMenuRegistrator;
        }

        private void OnRegistrationExceptionEvent(object sender, RegistrationExceptionEvent registrationExceptionEvent)
        {
            MessageBox.Show(registrationExceptionEvent.Exception.ExtractErrorMessage(), "Registration Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DisplayName = "Options";
            CheckRegistration();
            contextMenuRegistrator.RegistrationExceptionEvent += OnRegistrationExceptionEvent;
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            contextMenuRegistrator.RegistrationExceptionEvent -= OnRegistrationExceptionEvent;
        }

        private void CheckRegistration()
        {
            CanModifyContextMenuRegistration = false;
            Task.Factory.StartNew(() => contextMenuRegistrator.IsContextMenuRegistered())
            .ContinueWith(task =>
            {
                var registrationState = new RegistrationState();
                registrationState.IsContextMenuRegistered = task.Result;
                registrationState.IsRegistrationLocationCorrect = contextMenuRegistrator.IsRegistrationLocationCorrect();

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
                        contextMenuRegistrator.RemoveRegistration();
                        contextMenuRegistrator.AddRegistration(ContextMenuDisplayText);
                    }
                }

                IsContextMenuRegistered = task.Result.IsContextMenuRegistered;
                CanModifyContextMenuRegistration = true;
            }));
        }

        public IOptions Options
        {
            get { return options; }
        }

        public bool RenameToFolder
        {
            get { return options.RenameToFolder; }
            set
            {
                options.RenameToFolder = value;
                NotifyOfPropertyChange(() => RenameToFolder);
            }
        }

        public bool DeleteAfterExtract
        {
            get { return options.DeleteAfterExtract; }
            set
            {
                options.DeleteAfterExtract = value;
                NotifyOfPropertyChange(() => DeleteAfterExtract);
            }
        }

        public bool IsContextMenuRegistered
        {
            get { return isContextMenuRegistered; }
            set
            {
                if (isContextMenuRegistered == value) return;
                isContextMenuRegistered = value;
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
            get { return canModifyContextMenuRegistration; }
            set
            {
                if (canModifyContextMenuRegistration == value) return;
                canModifyContextMenuRegistration = value;
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
                contextMenuRegistrator.RemoveRegistration();
            else
                contextMenuRegistrator.AddRegistration(ContextMenuDisplayText);

            CheckRegistration();
        }

        private class RegistrationState
        {
            public bool IsContextMenuRegistered;
            public bool IsRegistrationLocationCorrect;
        }
    }
}
