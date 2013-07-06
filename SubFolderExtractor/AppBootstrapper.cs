using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using SubFolderExtractor.Interfaces;
using SubFolderExtractor.ViewModels;

namespace SubFolderExtractor
{
    public class AppBootstrapper : AutofacBootstrapper<ShellViewModel>
    {
        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();

            AutoSubscribeEventAggegatorHandlers = true;
            EnforceNamespaceConvention = false;
            ViewModelBaseType = typeof(IScreen);
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);
            
            builder.Register<IContextMenuRegistrator>(c => new ContextMenuRegistrator());
            builder.Register<IOptions>(c => new Options()).SingleInstance();
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            if (e.Args.Any())
            {
                ProcessArgs(e.Args);
                return;
            }

            base.OnStartup(sender, e);
        }

        protected override void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledException(sender, e);
            NLog.LogManager.GetCurrentClassLogger().Fatal(e.Exception); // log any unhandled errors
        }
        
        private void ProcessArgs(string[] args)
        {
            var rootDirectory = args[0];
            var windowManager = IoC.Get<IWindowManager>();
            var options = Container.Resolve<IOptions>();

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