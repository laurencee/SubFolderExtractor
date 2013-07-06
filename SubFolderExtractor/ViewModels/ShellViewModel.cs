using System;
using Caliburn.Micro;

namespace SubFolderExtractor.ViewModels
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private readonly MainViewModel mainViewModel;

        public ShellViewModel(MainViewModel mainViewModel)
        {
            if (mainViewModel == null) throw new ArgumentNullException("mainViewModel");
            this.mainViewModel = mainViewModel;

            Items.Add(mainViewModel);
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
