FROM mcr.microsoft.com/dotnet/aspnet:5.0

COPY build/CloudNine App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]
