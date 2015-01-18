SubFolderExtractor
==================

[Latest release](https://github.com/laurencee/SubFolderExtractor/releases/latest)

##Description: 
Extracts files from folders under the specified root folder. 

The extraction process can be launched through the main application or as a right click context menu option in explorer on any folder (context menu option can be enable from options menu).

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

SubFolderExtractor also supports split compressed files such as 
```
compressedFile_part000.rar
compressedFile_part001.rar
compressedFile_part002.rar
etc.
```

## Additional options
There are additional options for renaming extracted files to origin folder and deleting folders after extraction (recycle bin).

With Rename on:
```
rootfolder\foo.avi
rootfolder\bar.avi
```

Folder deletion can be turned on to delete the foo/bar folders after extraction has finished (occurs per folder extracted, not at the end)

As mentioned in the description a context menu action can be added to explorer to allow quick access for sub folder extraction.
