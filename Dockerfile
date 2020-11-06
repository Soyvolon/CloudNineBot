FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG PATH=/root/CloudNine/src/build

COPY $PATH App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]