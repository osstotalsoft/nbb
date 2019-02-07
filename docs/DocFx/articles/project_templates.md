Install and use NBB Unit Test Template
==

## Prerequisites
* .NET Core SDK

## Install/Uninstall
1. Open a command line window.
2. To install the template package use the following command:
```cmd
dotnet new -i NBB.UnitTest --nuget-source https://incbnuget.appservice.online/nuget
```
- This command will install the latest "NBB.UnitTest" package from the "https://incbnuget.appservice.online/nuget" source.
- If you have the "https://incbnuget.appservice.online/nuget" source registered on your machine you can skip the `--nuget-source` parameter.
3. To uninstall the package use the following command:
```cmd
dotnet new --debug:reinit
```
>**NOTE:** This will remove all the templates installed.
>Right now there is no way of uninstalling a specific template.

- The `dotnet new -u` command works only if you install the package from the template source and not from a NuGet package.

## Create a Project from the Template
1. Navigate in your command window to the path where you want your project to be created
2. Use the following command to create the project:
```cmd
dotnet new NbbUnitTest -n <Your_project_name>
```
>**NOTE:** This will create a folder in your current location with the same name as your project and add the files inside the folder

* This command has a `-o, --output` parameter that you can use to specify a path where to create the project.
>**NOTE:** Using the `-o, --output` parameter will not create a new folder with the name you specified in the `-n, --name` parameter and will generate the files inside the specified path.
* Omitting the `-n, --name` parameter will take the parent directory name as your project name.