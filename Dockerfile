FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

COPY /home/vsts/work/1/s/CloudNine/bin/Debug/netcoreapp3.1/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]