Debugging referenced NBB assemblies from target applications
==
The symbol files (pdb) are packaged in separate nuget symbol packages (*.snupkg). In order to use them for debugging you must use the nuget symbol server where they are published.

Additionally during debugging you can access the original source code files from github via SourceLink. 

### To use the Nuget.org symbol server:
* In Visual Studio, open *Tools > Options > Debugging > Symbols* (or *Debug > Options > Symbols*).
   1. Under Symbol file (.pdb) locations, add a new symbol server location by selecting the + symbol in the toolbar. Use the following URL https://symbols.nuget.org/download/symbols.
   2. (OPTIONAL) For performance reasons select "Load only specified modules" and enter "NBB.*"

### To use source SourceLink:
* In Visual studio open Tools > Options > Debugging > General
    - Ensure that "Enable Source Link support" is enabled

### To debug NBB assemblies:
* In Visual studio open Tools > Options > Debugging > General
    - Ensure that "Enable Just My Code" is disabled
