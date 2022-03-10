# DocsifyNet
Docsify host on .NET 6. 

Build a document site or knowledge base with one click, out of the box.


# usage

## Web Service

Download `FrameworkDependency.zip`, and run `dotnet DocsifyNet.dll`.

## Visit
You can visit it by `http://localhost:5177`. And you can change it in `appsettings.json` config file.


## write md fild

All static markdown file show write in `wwwroot/docs` directory.

# Document structure directory

It supported by `_sidebar.md` (see [docsify](https://github.com/docsifyjs/docsify)). And It will be created **automatically** _sidebar.md file if you change file or directory  (changes event within 3S will not be triggered again). 

you can trigger it manually by accessing `/api/Sidebar/Gen ` too.

##  _sidebar.md rule

### File

1. All file name start with "." will be ignore
2. default file name (default is README.md, you can change it in appsettings.json --> SidebarCreatorOption --> IndexFileName) will be ignore.

### Directory 

1. all directory name start with "." or "_" will be ignore.
2. directory named assets will be ignore.

#  Other

`wwwroot\pages\404.html` is the default HTTP 404 file.


# Reference

https://github.com/xxxxue/Docsify-Build-Sidebar







