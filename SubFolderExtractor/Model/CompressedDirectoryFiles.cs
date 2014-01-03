using System.IO;

namespace SubFolderExtractor.Model
{
    public class CompressedDirectoryFiles
    {
        public CompressedDirectoryFiles(DirectoryInfo directory, FileInfo[] compressedFiles)
        {
            Directory = directory;
            CompressedFiles = compressedFiles;
        }

        public DirectoryInfo Directory { get; private set; }
        public FileInfo[] CompressedFiles { get; private set; }
    }
}