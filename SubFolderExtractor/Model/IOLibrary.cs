using System;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using NLog;

namespace SubFolderExtractor.Model
{
    public static class IOLibrary
    {
        private static readonly Logger Logger = LogManager.GetLogger("IOLibrary");

        public static void RenameFileToFolder(string extractedFileFullPath, string directoryName)
        {
            if (!File.Exists(extractedFileFullPath)) return;

            bool onlyDifferByCase = false;
            if (Path.GetFileNameWithoutExtension(extractedFileFullPath) == Path.GetFileName(directoryName)) // case sensitive comparison
            {
                Logger.Info("No need to rename file as it already matches the folder name");
                return;
            }
            if (Path.GetFileNameWithoutExtension(extractedFileFullPath).IsEqualTo(Path.GetFileName(directoryName)))
            {
                onlyDifferByCase = true;
            }

            var fileInfo = new FileInfo(extractedFileFullPath);
            string newFileName = string.Format("{0}{1}", directoryName, fileInfo.Extension);
            string newFileFullPath = Path.Combine(fileInfo.DirectoryName, newFileName);

            Logger.Info("Renaming file from {0} to {1}", Path.GetFileName(extractedFileFullPath), newFileName);

            if (onlyDifferByCase) // perform case sensitive rename
            {
                var tmpName = Path.Combine(fileInfo.DirectoryName, Path.GetRandomFileName());
                Logger.Info("File names only differ by case, using temporary file to allow case sensitive rename: " + tmpName);

                File.Move(extractedFileFullPath, tmpName);
                File.Move(tmpName, newFileFullPath);
            }
            else if (File.Exists(newFileFullPath)) // Guard against existing file with same name
                Logger.Error(string.Format("Unable to rename file {0} as a file with the same name already exists", newFileFullPath));
            else
                File.Move(extractedFileFullPath, newFileFullPath);
        }

        public static void DeleteDirectory(string directoryName)
        {
            try
            {
                Logger.Info("Deleting directory {0}", directoryName);
                FileSystem.DeleteDirectory(directoryName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ExtractErrorMessage);
            }
        }

        public static void DeleteFile(string fileFullName)
        {
            try
            {
                Logger.Info("Deleting file {0}", fileFullName);
                FileSystem.DeleteFile(fileFullName, UIOption.OnlyErrorDialogs, RecycleOption.DeletePermanently);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ExtractErrorMessage);
            }
        }
    }
}
