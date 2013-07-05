SubFolderExtractor
==================

Extracts files from folders under the selected folder. Supports split compressed files such as 
```
compressedFile_part000.rar
compressedFile_part001.rar
compressedFile_part002.rar
etc.
```

Provides options for renaming extracted files to origin folder and deleting folders after extraction (recycle bin).

##Example:
```
rootfolder
  foo\compressedFileA.zip - contains compressedFileA.avi
  bar\compressedFileB.zip - contains compressedFileB.avi
```
  
Extraction process without rename:
```
rootfolder\compressedFileA.avi
rootfolder\compressedFileA.avi
```

With Rename on:
```
rootfolder\foo.avi
rootfolder\bar.avi
```

Folder deletion can be turned on to delete the foo/bar folders after extraction has finished (occurs per folder extracted, not at the end)
