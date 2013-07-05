using SubFolderExtractor.Interfaces;

namespace SubFolderExtractor
{
    public class Options : IOptions
    {
        public Options()
        {
            _renameToFolder = Properties.Settings.Default.RenameToFolder;
            _deleteAfterExtract = Properties.Settings.Default.DeleteAfterExtract;
        }

        private bool _renameToFolder;
        private bool _deleteAfterExtract;

        public bool RenameToFolder
        {
            get { return _renameToFolder; }
            set
            {
                if (_renameToFolder == value) return;

                Properties.Settings.Default.RenameToFolder = value;
                Properties.Settings.Default.Save();
                _renameToFolder = value;
            }
        }

        public bool DeleteAfterExtract
        {
            get { return _deleteAfterExtract; }
            set
            {
                if (_deleteAfterExtract == value) return;

                Properties.Settings.Default.DeleteAfterExtract = value;
                Properties.Settings.Default.Save();
                _deleteAfterExtract = value;
            }
        }
    }
}