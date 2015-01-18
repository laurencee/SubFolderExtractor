SubFolderExtractor
==================

[Latest release](https://github.com/laurencee/SubFolderExtractor/releases/latest)

##Description: 
Extracts files from folders under the specified root folder. 

The extraction process can be launched through the main application or as a right click context menu option in explorer on any folder (context menu option can be enable from options menu).

##Example:
```
rootfolder\
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

SubFolderExtractor also supports split compressed files such as 
```
compressedFile_part000.rar
compressedFile_part001.rar
compressedFile_part002.rar
etc.
```

## Additional options
There are a few other options that can be toggled on or off through the options menu:
* Rename after extraction
* Delete after extraction
* Register context menu action

Delete after extraction will delete the subfolders after files have been extracted.
