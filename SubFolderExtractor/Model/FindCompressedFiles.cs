using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SubFolderExtractor.Properties;

namespace SubFolderExtractor.Model
{
    public static class FindCompressedFiles
    {
        /// <summary>
        ///   Checks compressed files against known chained compressed file naming formats as specified in Settings.Default.ChainedFileRegularExpressions
        ///   <example>myrar.part001, myrar.part002</example>
        /// </summary>
        /// <param name="fileInfos"> Files to check for any known chain names </param>
        /// <returns> Compressed files that are either the start or are not part of a compression chain </returns>
        public static List<FileInfo> GetUnchainedCompressedFiles(List<FileInfo> fileInfos)
        {
            var unchainedCompressedFiles = new List<FileInfo>();

            var processedFileNames = new HashSet<string>();
            string fileNames = string.Join(Environment.NewLine, fileInfos.Select(e => e.Name));
            foreach (var chainedFileRegularExpression in Settings.Default.ChainedFileRegularExpressions)
            {
                var matches = Regex.Matches(fileNames, chainedFileRegularExpression);
                if (matches.Count == 0) continue; // no matches so nothing to process

                string firstFileName = matches.Cast<Match>().OrderBy(match => match.Value).Select(match => match.Value).FirstOrDefault();

                // Add just the first file in the chain
                unchainedCompressedFiles.Add(fileInfos.First(e => e.Name == firstFileName));

                foreach (Match match in matches)
                {
                    if (!processedFileNames.Contains(match.Value))
                        processedFileNames.Add(match.Value);
                }
            }

            // Add remaining unprocessed files as they must not be part of any known chain
            unchainedCompressedFiles.AddRange(fileInfos.Where(e => !processedFileNames.Contains(e.Name)));
            return unchainedCompressedFiles;
        }

        public static List<FileInfo> GetCompressedFilesInDirectory(DirectoryInfo directory)
        {
            var compressedDirectoryFiles = new List<FileInfo>();
            if (!directory.Exists) return compressedDirectoryFiles;

            var fileInfos = directory.GetFiles();
            if (fileInfos.Length == 0) return compressedDirectoryFiles;

            foreach (var compressionExtension in Settings.Default.CompressionExtensions)
            {
                // dont add the same file for r00 if we already have a rar file from the same folder
                // This means we skip r00 when file names are not the same as the rar file but that's not common or very important
                if (compressionExtension == "r00" &&
                    compressedDirectoryFiles.Any(x => x.Extension == ".rar"))
                {
                    continue;
                }

                var extension = compressionExtension;
                var compressedFiles = fileInfos.Where(x => x.Extension == ("." + extension)).ToList();

                if (compressedFiles.Any())
                    compressedDirectoryFiles.AddRange(compressedFiles);
            }

            return compressedDirectoryFiles;
        }

        public static List<CompressedDirectoryFiles> GetSubDirectoriesWithCompressedFiles(DirectoryInfo baseDirectory)
        {
            var compressedDirectoryFiles = new List<CompressedDirectoryFiles>();

            foreach (var directory in baseDirectory.GetDirectories())
            {
                var compresedFilesInDirectory = GetCompressedFilesInDirectory(directory);
                if (compresedFilesInDirectory.Any()) compressedDirectoryFiles.Add(new CompressedDirectoryFiles(directory, compresedFilesInDirectory));
            }

            return compressedDirectoryFiles;
        }
    }
}
