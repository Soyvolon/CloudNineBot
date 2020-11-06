FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

COPY build/CloudNine App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]