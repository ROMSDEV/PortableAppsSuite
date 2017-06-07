### innounp, the Inno Setup Unpacker
### Version 0.46
### Supports Inno Setup versions 2.0.7 through 5.5.9

[Inno Setup](http://www.jrsoftware.org/isinfo.php) is a popular program for making software installations. Unfortunately, there is no official unpacker - the only method of getting the files out of the self-extracting executable is to run it. One piece of software that addresses this issue is Sergei Wanin's [InstallExplorer](http://plugring.farmanager.com/downld/files/instexpl_v0.3.rar), a plug-in for the [FAR Manager](http://farmanager.com) that unpacks several types of installations, including Inno Setup (IS). But since it is not updated in a timely fashion, and so does not support the latest IS most of the time, this program was born. The advantages over InstallExplorer are:

- Innounp is open source and based on IS source. Therefore, it is more likely to support future IS versions.
- It recovers portions of the installation script (.iss file), including the registry changes and the compiled Innerfuse/RemObjects Pascal Script, if available.

If you want to report a bug, request a feature, or discuss anything else related to the program, please write to the forum.

#### On this page:

- Usage
- How to report bugs
- What's new/History
- MultiArc settings
- Copyrights and licensing

#### In other places:

- [Download](http://sourceforge.net/projects/innounp/files)
- [Forum](http://sourceforge.net/projects/innounp/forums/forum/353235)
- [Project summary page on SF.net](http://sourceforge.net/projects/innounp)
- [Homepage](http://innounp.sf.net)

Both the source and the executable packages are compressed with [WinRar](http://www.rarlab.com). While the full-featured packer is shareware, the UnRar utility that can only extract files is free. And there are lots of free third-party programs that unpack rar just fine, e.g. [7-Zip](http://www.7-zip.org).

As a bonus, a simple unpacker for [Setup Factory](http://www.indigorose.com/sf/index.php) installations is available on the download page. It is ripped from [the SynCE project](http://synce.sourceforge.net).


### Usage

Innounp is a console application, and it uses command-line options to find out what to do. For a more human-friendly interface utilizing FAR or Total Commander as a front-end see the MultiArc section below. Windows Explorer fans: nullz has made some [.reg scripts](http://sourceforge.net/forum/forum.php?thread_id=1122068&forum_id=353235) to add innounp into the right-click menu and Richard Santaella crafted a graphical wrapper for innounp (get it on the download page).

```php
innounp [command] [options] <setup.exe or setup.0> [@filelist] [filemask ...]
```

| Command | Description |
| --- | --- |
| (no)   | display general installation info |
| -v     | verbosely list the files (with sizes and timestamps) |
| -x     | extract the files from the installation (to the current directory, also see -d) |
| -e     | extract files without paths |
| -t     | test files for integrity |

| Option | Description |
| --- | --- |
| -b     | batch (non-interactive) mode - will not prompt for password or disk changes |
| -q     | do not indicate progress while extracting |
| -m     | process internal embedded files (such as license and uninstall.exe) |
| -pPASS | decrypt the installation with a password |
| -dDIR  | extract the files into DIR (can be absolute or relative path) |
| -cDIR  | specifies that DIR is the current directory in the installation |
| -n     | don't attempt to unpack new versions |
| -fFILE | same as -p but reads the password from FILE |
| -a     | process all copies of duplicate files |
| -y     | assume Yes on all queries (e.g. overwrite files) |

If an installation has setup.0 (it is made without using SetupLdr), run innounp on setup.0 instead of setup.exe.

To extract all files from a specific directory, use dirname\*.*, not just dirname.

By default all files are extracted to the current directory. Use -d to override this behaviour. For example, -dUnpacked will create a directory named Unpacked inside the current directory and put the extracted files there.

The -c option is a little more tricky to explain. Suppose you opened an installation in a file manager and browsed to {app}\subdir\program.exe. Now if you copied program.exe to another location, the entire directory tree ({app}\subdir\) would be created and program.exe would be extracted there. -c notifies innounp that you are only interested in paths from the current directory and below, so that your file, program.exe, is extracted right where you intended to copy it, not several directory levels deeper. Note that in order to avoid confusion, files must still be specified by their full path names inside the installation.

Note that an installation can contain several identical files (possibly under different names). Inno Setup stores only one copy of such files, and by default innounp will also unpack one file. If you want to have all files that could ever be installed anywhere, regardless of how many identical files this may get you, -a option will do it.

If -m is specified, the file listing includes embedded\CompiledCode.bin which is the code made by the RemObjects Pascal Script compiler. It is possible to disassemble it using the ifps3_disasm.rar package on the download page. The result is not very readable though since it uses the basic 'disassembler' from IFPS3. Anyone wants to write a decompiler?


### How to report bugs

OK, I know innounp is far from being perfect, but it is my intention to make the program usable. User feedback is a great way to achieve this. Here's what you should do if you find a bug and want it fixed.

Tell me what's wrong with innounp. If you encountered incorrect behaviour, say what you think it should do and what it actually does. If it crashed or gave an error message, say what did that - innounp, Windows, FAR, etc, and include the details.

Describe the exact steps necessary to reproduce the bug. Say what are the preconditions. Is the bug specific to some system settings? To a setup file you have? (include the problem part of the installation script or a link to the compiled setup, if it is small enough) To an IS version? To something else? Or does the bug occur regardless of these things?

Once you have the bug report ready, post it to the forum. Remember, if I can't reproduce the bug using the description you gave, the chances that it will be fixed fall dramatically.

If the above guidelines were not obvious for you, I suggest that you read the following articles.

[How to Ask Questions the Smart](http://www.catb.org/~esr/faqs/smart-questions.html) Way by Eric Raymond
[How to Report Bugs Effectively](http://www.chiark.greenend.org.uk/~sgtatham/bugs.html) by Simon Tatham


### What's new / History

#### 0.46 (2016.04.11)

- Increased max. LZMA dictionary size to 1Gb (implemented in IS 5.5.9).

#### 0.45 (2015.12.31)

- Added support for IS 5.5.7.

#### 0.44 (2015.11.24)

- Fixed infinite loop issue on some unsupported versions.
- Now -m flag does not affect reconstructed script content. It always contains all info.
- Minor changes.

#### 0.43 (2015.07.18)

- Added support for IS 5.5.6.

#### 0.42 (2015.05.27)

- Added support for IS 1.3.21 and 1.3.25.
- Experimental support for some custom IS versions.
- Fixed encoding of several entries in reconstructed script.

#### 0.41 (2015.03.18)

- All slashes in file paths are converted to Windows style for consistency.
- Improved some error messages.
- Fixed several parameter names in [INI] section of the script.

#### 0.40 (2013.12.20)

- Synchronized Description fields encoding in reconstructed script.
- Minor changes.

#### 0.39 (2013.07.12)

- Fixed CRC32 calculation during unpacking (regression bug).

#### 0.38 (2013.02.01)

- Added InstallDelete and UninstallDelete sections to reconstructed script.
- Added some more values to Setup section of reconstructed script.
- Several minor reconstructed script improvements.

#### 0.37 (2012.06.02)

- Added support for IS 5.5.0.
- Fixed problem with reading of large files.
- Added some more values to reconstructed script.
- Improved FAR MultiArc settings.

#### 0.36 (2011.06.01)

- Fixed support for IS 5.4.2.
- Fixed issue with '{' symbol in file names.

#### 0.35 (2010.10.01)

- Added support for IS 5.2.5 (wasn't released, but such installers can be found).
- Added command to test files for integrity.

#### 0.34 (2010.09.16)

- Less technical text in some error messages.
- Added dump of password hash to reconstructed script.
- Added some more values to reconstructed script.

#### 0.33 (2010.07.05)

- Fixed encoding for custom messages in reconstructed script.
- Fixed several parameters in [LangOptions] section.
- Fixed language names in *.isl files for Unicode-based installers.
- Added support for legacy IS versions 2.0.8 - 2.0.10.

#### 0.32 (2010.06.14)

- Added support for IS versions 5.3.10 (both ANSI and Unicode).
- Added support for INI section in reconstructed script.

#### 0.31 (2010.04.19)

- Fixed issue with endless decompression loop on incompatible files.

#### 0.30 (2010.04.12)

- Fixed issue with password processing for Unicode versions.
- Added support for IS versions 5.3.9 (both ANSI and Unicode).
- Added support for LZMA2 compression, introduced in 5.3.9.

#### 0.29 (2010.02.19)

- Added support for IS versions 5.3.8 (both ANSI and Unicode).

#### 0.28 (2010.01.14)

- Added support for IS versions 5.3.7 (both ANSI and Unicode).
- Added support for legacy IS versions 2.0.11 - 2.0.17.
- Fixed renaming of duplicate files. If we do not use -a then don't append numbers to names
- (this switch does not affect different files with same name, only duplicates with same content).

#### 0.27 (2009.12.04)

- Yet another tuning for file mask processing.
- Added overwrite prompt for files extraction (and option for auto-overwrite).
- Several tweaks to reconstructed script.

#### 0.26 (2009.11.30)

- Added manifest resource to resolve Vista/Win7 UAC issue.
- Added restored %n formatter to custom messages.
- Added default OutputBaseFilename value if one from header is empty.

#### 0.25 (2009.11.26)

- Added support fro [Dirs] section in reconstructed script.
- Moved version parameter in script to comment (since it is not original IS parameter).
- Fixed ArchitecturesInstallIn64BitMode and ArchitecturesAllowed flags in script.
- Fixed file mask processing in some cases.

#### 0.24 (2009.11.20)

- Added support for IS versions 5.3.6 (both ANSI and Unicode).
- Added version information resource.
- Fixed extraction of multiple files with same name.

#### 0.23 (2009.09.25)

- Added support for IS versions 5.3.5 (both ANSI and Unicode).
- Added Inno Setup version info to reconstructed install script.

#### 0.22 (2009.08.24)

- Added support for Unicode versions.
- Added support for IS versions 5.3.0 - 5.3.4 (both ANSI and Unicode).
- Fixed rare issue with double backslashes in file path.

#### 0.21 (2009.04.24)

- Supports legacy IS versions 2.0.18 - 2.0.19

#### 0.20 (2008.05.23)

- Supports IS up to version 5.2.3
- Several bugs fixed.

#### 0.19 (2007.02.23)

- Supports IS up to version 5.1.10
- Fixed wrong representation of Unicode characters in LanguageName.
- Another fix to the handling of duplicate file names.
- New option -a to extract all copies of duplicate files.

#### 0.18 (2006.11.23)

- The reconstructed script now includes the [Types], [CustomMessages], and [Languages] sections.
- ROPS disassembler updated to support the latest build of ROPS.
- New option -f to read the password from file. This way it can include any special characters.
- Be sure to save the file in the correct character encoding as no translations are applied.
- Fixed the bug that caused the file timestamps to be inconsistently reported and applied (UTC vs. local).
- Updated the decompression libraries: zlib to version 1.2.3, bzip2 to version 1.03, and LZMA to version 4.43 (optimized for speed).

#### 0.17 (2005.08.31)

- Supports IS up to version 5.1.5.
- Supports Martijn Laan's My Inno Setup Extensions 3.0.6.1 (by request).
- The Types parameter is now space-separated, as required by the IS script specification.

#### 0.16 (2005.04.30)

- Supports IS up to 5.1.2-beta.
- Innounp will try to unpack new versions of IS to handle the cases when the binary format is compatible with one of the previous versions. Use -n to disable this attempt.

#### 0.15 (2005.03.08)

- Supports IS up to 5.1.0-beta.
- The old bug that prevented innounp from working properly with {reg:...} constants and the like has got another fix.
- Preliminary support for the 64-bit extensions that appeared in IS 5.1.0.

#### 0.14 (2004.10.14)

- Supports IS up to 5.0.4-beta.
- It is now possible to specify the destination directory to extract files into using the -d option. This directory will be created if necessary.
- New option -c specifies the current directory inside an installation and prevents the creation of the upper-level directories. MultiArc settings are updated accordingly.
- The old -c command is removed. To get the compiled Pascal script, use -m and extract it like a normal file.

#### 0.13 (2004.08.26)

- Supports IS up to 5.0.3-beta.
- Supports the Components and Tasks sections.

#### 0.12 (2004.07.28)

- Supports IS up to 5.0.0-beta.
- Improved processing of big installations with many files.
- Innounp now supports a certain level of user interaction - it prompts the user for password and disk changes as necessary. To switch this functionality off (e.g. in batch mode), use the -b option.
- If no command is specified, innounp displays a brief summary of the specified installation. The old -i command is removed. To get the setup script, extract it like a normal file.

#### 0.11 (2004.05.04)

- Supports IS 4.2.2.
- Supports ArcFour encryption. Use the -p switch to specify a password if files are encrypted

#### 0.10 (2004.04.26)

- Fixed (again): filenames containing invalid characters could not be specified on the command line or in a list file.

#### 0.09 (2004.04.22)

- Fixed (again): invalid characters in filenames (such as ':' and '|') made innounp crash.
- Updated TC MultiArc settings.

#### 0.08 (2004.04.14)

- Added support for IS versions up to 4.2.1.
- Added MultiArc settings for Total Commander (thanks to Gnozal).
- Fixed a bug in MultiArc settings that prevented shells from displaying file dates and times (thanks to Maxim Ryazanov).
- The reconstructed setup script (.iss) is now included together with the 'normal' files. Using -m option it's possible to view/extract other internal files in the same way.

#### 0.07 (2004.03.16)

- Multiple files with the same name are not overwritten now, instead they are appended with numbers.
- -c command extracts the compiled Innerfuse Pascal Script code to a file. It can then be 'disassembled' with a separate tool. Get one on the download page.
- The output of -i command now looks more like .iss script. More data is included.

#### 0.06 (2004.03.11)

- Added support for IS versions 3.0.0 - 4.0.0.
- Supports installations that were not packaged into a single exe using SetupLdr (these can be identified by the presence of setup.0 which is appended to setup.exe in packaged installations).
- -i command displays registry changes made by an installation.
- Supports (displays and reads from filelists) filenames with national characters (single-byte character encodings only, Unicode/MBCS was not tested). The correct code page must be set in Windows for this function to work properly.
- [fix] File dates and times were not set during extraction.

#### 0.05 (2004.03.09)

- Improved batch processing. Now it's possible to browse and extract IS installations in FAR using the supplied settings for the standard MultiArc plug-in.
- Removed isbunzip.dll. Bzip2 library is linked statically.

#### 0.04 (2004.02.27)

- Initial release. Supports IS versions 4.0.1 - 4.1.8.


### MultiArc settings

Unless you are a die-hard fan of command line, you may like the idea of working with IS installations like with conventional archives in a file manager. Right now two programs support this: FAR and Total Commander. Below are the instructions how to integrate innounp into each.


### FAR

Copy innounp.exe to a directory in your PATH and edit your FAR\Plugins\MultiArc\Formats\Custom.ini file. There are two alternate settings differing in several aspects and each having its own pros and cons. Try the recommended setting first, if it does not work well for you, try the other setting or even combine them.


### Co-operation with InstallExplorer

If you have InstallExplorer installed (or another plug-in that handles IS, but you will need to adjust the settings accordingly), you might want to let it process all the other types of installations but keep IS installations for innounp. FAR does not provide a means of customizing the plug-in call order; however, an empirical study has shown that it loads plug-ins and applies them to files in lexical order. So the solution is to rename InstallExplorer's dll file from 6InstExpl.dll to e.g. zInstExpl.dll (and restart FAR).


### FAR: recommended setting

```ini
[InnoSetup5]
TypeName=InnoSetup5
ID=49 6E 6E 6F 20 53 65 74 75 70 20 53 65 74 75 70 20 44 61 74 61 20 28 35 2E
IDOnly=1
List=innounp -v -m
Errorlevel=1
Start="^---------"
End="^---------"
Format0="/^\s+(?P<size>\d+)\s+(?P<mYear>\d+)\.(?P<mMonth>\d+)\.(?P<mDay>\d+)\s+(?P<mHour>\d+):(?P<mMin>\d+)\s+(?P<name>.*)$/i"
Extract=innounp -x -m {-c%%R} %%A {@%%LMQ}
ExtractWithoutPath=innounp -e -m {-c%%R} %%A {@%%LMQ}
Test=innounp -t -m %%A
AllFilesMask="*.*"

[InnoSetup4]
TypeName=InnoSetup4
ID=49 6E 6E 6F 20 53 65 74 75 70 20 53 65 74 75 70 20 44 61 74 61 20 28 34 2E
IDOnly=1
List="innounp -v -m"
Errorlevel=1
Start="^---------"
End="^---------"
Format0="/^\s+(?P<size>\d+)\s+(?P<mYear>\d+)\.(?P<mMonth>\d+)\.(?P<mDay>\d+)\s+(?P<mHour>\d+):(?P<mMin>\d+)\s+(?P<name>.*)$/i"
Extract=innounp -x -m {-c%%R} %%A {@%%LMQ}
ExtractWithoutPath=innounp -e -m {-c%%R} %%A {@%%LMQ}
Test=innounp -t -m %%A
AllFilesMask="*.*"

[InnoSetup3]
TypeName=InnoSetup3
ID=49 6E 6E 6F 20 53 65 74 75 70 20 53 65 74 75 70 20 44 61 74 61 20 28 33 2E
IDOnly=1
List="innounp -v -m"
Errorlevel=1
Start="^---------"
End="^---------"
Format0="/^\s+(?P<size>\d+)\s+(?P<mYear>\d+)\.(?P<mMonth>\d+)\.(?P<mDay>\d+)\s+(?P<mHour>\d+):(?P<mMin>\d+)\s+(?P<name>.*)$/i"
Extract=innounp -x -m {-c%%R} %%A {@%%LMQ}
ExtractWithoutPath=innounp -e -m {-c%%R} %%A {@%%LMQ}
Test=innounp -t -m %%A
AllFilesMask="*.*"

[InnoSetup2]
TypeName=InnoSetup2
ID=49 6E 6E 6F 20 53 65 74 75 70 20 53 65 74 75 70 20 44 61 74 61 20 28 32 2E
IDOnly=1
List="innounp -v -m"
Errorlevel=1
Start="^---------"
End="^---------"
Format0="/^\s+(?P<size>\d+)\s+(?P<mYear>\d+)\.(?P<mMonth>\d+)\.(?P<mDay>\d+)\s+(?P<mHour>\d+):(?P<mMin>\d+)\s+(?P<name>.*)$/i"
Extract=innounp -x -m {-c%%R} %%A {@%%LMQ}
ExtractWithoutPath=innounp -e -m {-c%%R} %%A {@%%LMQ}
Test=innounp -t -m %%A
AllFilesMask="*.*"
```


### FAR: alternate setting

Will not work for IS 5.1.5 and up because new versions no longer have this signature.

```ini
[InnoSetup]
TypeName=InnoSetup
ID=49 6E 6E 6F
IDPos=48
Extension=exe
List="innounp -v -m"
Errorlevel=1
Start="^---------"
End="^---------"
Format0="zzzzzzzzzz  yyyy tt dd hh:mm  nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn"
Extract=innounp -x -m {-c%%R} %%A {@%%LMQ}
ExtractWithoutPath=innounp -e -m {-c%%R} %%A {@%%LMQ}
AllFilesMask="*.*"
```


### Total Commander

Will not work for IS 5.1.5 and up because new versions no longer have this signature. I guess the version-specific settings from above have to be cloned.

Configuration made up by Gnozal and Maxwish and posted on TC forum. Change the path below to where you have innounp installed and add this to your MultiArc.ini. Note that MultiArc is not included in the default TC installation, instead it is available as a separate download from Siarzhuk Zharski's web site. Refer to the help file for information on any additional configuration necessary.

```ini
[InnoSetup]
Description="InnoSetup"
Archiver=C:\PROGRAM FILES\WINCMD\WCXPlugin\MultiArc\innounp.exe
Extension=exe
ID=49 6E 6E 6F
IDPos=48
Start="^--------------------------------------"
End="^--------------------------------------"
Format0="zzzzzzzzzz  yyyy.tt.dd hh:mm  nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn"
List=%P -v -m %AQ
Extract=%P -e -m -c%R %AQ @%LQ
ExtractWithPath=%P -x -m -c%R %AQ @%LQ
IgnoreErrors=0
SkipEmpty=0
SkipDirsInFileList=0
SearchForUglyDirs=0
BatchUnpack=1
UnixPath=0
AskMode=0
SkipLIST=1
Debug=0
```

### Copyrights and licensing

Copyright © 2004-2015 QuickeneR, 2009-2015 Ariman
This program is licensed under the terms of the [GNU General Public License (GPL)](http://www.gnu.org/copyleft/gpl.html). A copy of the license is included with the source files.
If you distribute innounp on the WWW, please put a link to its home page, http://innounp.sourceforge.net

Over 90% of code is ripped from Inno Setup which is Copyright © 1997-2010 Jordan Russell. All rights reserved.
Portions Copyright © 2000-2006 Martijn Laan. All rights reserved.
See http://www.jrsoftware.org for details.

Contains zlib code, Copyright © 1995-2005 Jean-loup Gailly and Mark Adler.

Contains bzip2 code, Copyright © 1996-2009 Julian R Seward. All rights reserved.

Contains LZMA code, Copyright © 1999-2009 Igor Pavlov.

Innerfuse Pascal Script is Copyright © 2000-2004 by Carlo Kok, Innerfuse.

StripReloc is Copyright © 1999-2005 Jordan Russell, www.jrsoftware.org