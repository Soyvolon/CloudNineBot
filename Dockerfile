FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

COPY /root/CloudNine/src/build App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]