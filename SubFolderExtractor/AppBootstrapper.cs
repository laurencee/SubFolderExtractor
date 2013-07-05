using System.ComponentModel;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using SubFolderExtractor.Interfaces;

namespace SubFolderExtractor
{
    public class AppBootstrapper : AutofacBootstrapper<MainViewModel>
    {
        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
            
            EnforceNamespaceConvention = false;
            ViewModelBaseType = typeof(IScreen);
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("ViewModel"))
                .Where(type => !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.EndsWith("ViewModels"))
                .Where(type => type.GetInterface(typeof(INotifyPropertyChanged).Name) != null)
                .AsSelf().InstancePerDependency();

            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("View"))
                .Where(type => !string.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.EndsWith("View"))
                .AsSelf().InstancePerDependency();

            builder.Register<IContextMenuRegistrator>(c => new ContextMenuRegistrator());
            builder.Register<IOptions>(c => new Options());
            builder.Register<IWindowManager>(c => new WindowManager()).InstancePerLifetimeScope();
            builder.Register<IEventAggregator>(c => new EventAggregator()).InstancePerLifetimeScope();
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