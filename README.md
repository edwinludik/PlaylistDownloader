# PlaylistDownloader

## Publish 

dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true  

scp bin/Release/net9.0/linux-x64/publish/* user@server:~/PlaylistDownloader/  

