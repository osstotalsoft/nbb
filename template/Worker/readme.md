### Local
- Build project
- Install template: 
```console
  dotnet new -i <Your_project_path>\NBB\template\Worker
```
- Use template:
```console
  dotnet new nbbworker -n MyTest.Worker -mt Nats
```


### Nuget
- Build project
- Increase version in file "NBB.WorkerNBB.Worker.nuspec"
- Package - use nuget.exe instead of dotnet pack:
```console
  cd <Your_project_path>\NBB\template\Worker
  nuget pack
``` 
- Publish package
```console
  cd <Your_project_path>\NBB\template\Worker
  nuget push NBB.Worker.*.nupkg -source <Your_nuget_repo_url>
``` 
- Install template
```console
  dotnet new -i NBB.Worker::* --nuget-source <Your_nuget_repo_url>
```
- Use template:
```console
  dotnet new nbbworker -n MyTest.Worker -mt Nats
```

### Uninstall
This will reset all templates:
```console
  dotnet new --debug:reinit
``` 