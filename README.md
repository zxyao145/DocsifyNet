# DocsifyNet
Docsify host on .NET 6. 

Build a document site or knowledge base with one click, out of the box.


# Usage

## Web Service

1. Download zip from [release](https://github.com/zxyao145/DocsifyNet/releases)
2. run the file:
   - windows：DocsifyNet.exe
   - osx-x64/osx-arm64/linux-x64：DocsifyNet

## Visit
You can visit it by `http://localhost:5177`. And you can change it in `appsettings.json` config file.


## Write md file

All static markdown file show write in `wwwroot/docs` directory.

# Sidebar

It supported by `_sidebar.md` (see [docsify](https://github.com/docsifyjs/docsify)). And it will **automatically** created  _sidebar.md file, if you change file or directory  (changes event within 3S will not be triggered again). 

You can also manually trigger it to generate the _sidebar.md file by accessing `/api/Sidebar/Gen`  too.

## What kind of file will not appear in the _sidebar.md

### File

1. All file name start with `.` will be ignore
2. default file name (default value is `README.md`, you can change it in appsettings.json --> SidebarCreatorOption --> IndexFileName) will be ignore.

### Directory 

1. All folders with names starting with `.` or `_` will be ignored
2. directory named `assets` will be ignore.

#  Other

`wwwroot\pages\404.html` is the default HTTP 404 file.


# Reference

https://github.com/xxxxue/Docsify-Build-Sidebar







