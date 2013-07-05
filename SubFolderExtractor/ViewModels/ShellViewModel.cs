using System;
using Caliburn.Micro;

namespace SubFolderExtractor.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly MainViewModel mainViewModel;
        private readonly OptionsViewModel optionsViewModel;

        public ShellViewModel(MainViewModel mainViewModel, OptionsViewModel optionsViewModel)
        {
            this.mainViewModel = mainViewModel;
            this.optionsViewModel = optionsViewModel;
            if (mainViewModel == null) throw new ArgumentNullException("mainViewModel");

            Items.Add(mainViewModel);
            Items.Add(optionsViewModel);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ActivateItem(mainViewModel);
        }

        public override void ActivateItem(IScreen item)
        {
            base.ActivateItem(item);
            DisplayName = item.DisplayName;
        }
    }
}
