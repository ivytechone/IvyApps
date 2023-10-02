FROM mcr.microsoft.com/dotnet/aspnet:7.0
COPY ./IvyApps/bin/Release/net7.0/ /ivyapps/
WORKDIR ./ivyapps
ENTRYPOINT ./IvyApps