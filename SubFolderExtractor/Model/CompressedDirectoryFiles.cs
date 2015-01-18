using System.Collections.Generic;
using System.IO;

namespace SubFolderExtractor.Model
{
    public class CompressedDirectoryFiles
    {
        public CompressedDirectoryFiles(DirectoryInfo directory, List<FileInfo> compressedFiles)
        {
            Directory = directory;
            CompressedFiles = compressedFiles;
        }

        public DirectoryInfo Directory { get; private set; }
        public List<FileInfo> CompressedFiles { get; private set; }
    }
}