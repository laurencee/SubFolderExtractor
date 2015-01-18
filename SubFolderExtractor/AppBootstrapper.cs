using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Caliburn.Micro;
using SubFolderExtractor.Interfaces;
using SubFolderExtractor.ViewModels;

namespace SubFolderExtractor
{
    public class AppBootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<ShellViewModel>();
            container.PerRequest<MainViewModel>();
            container.PerRequest<OptionsViewModel>();
            container.PerRequest<ExtractProgressViewModel>();
            container.Singleton<IOptions, Options>();
            container.Singleton<IContextMenuRegistrator, ContextMenuRegistrator>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);

            NLog.LogManager.GetCurrentClassLogger().Fatal(e.Exception); // log any unhandled errors
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Any())
            {
                ProcessArgs(e.Args);
                return;
            }

            DisplayRootViewFor<ShellViewModel>();
        }

        private void ProcessArgs(string[] args)
        {
            var rootDirectory = args[0];
            var windowManager = IoC.Get<IWindowManager>();
            var options = IoC.Get<IOptions>();

            if (args.Contains("RenameToFolder"))
                options.RenameToFolder = true;
            if (args.Contains("DeleteFolderAfterExtraction"))
                options.DeleteAfterExtract = true;

            var extractProgressViewModel = IoC.Get<ExtractProgressViewModel>();

            windowManager.ShowWindow(extractProgressViewModel);
            extractProgressViewModel.StartExtraction(rootDirectory);

            // Shutdown the application when execution has completed
            extractProgressViewModel.PropertyChanged += (sender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == "IsExecuting" && !extractProgressViewModel.IsExecuting)
                    Application.Shutdown();
            };
        }
    }
}