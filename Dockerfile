FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG PATH=CloudNine\bin\Release\netcoreapp3.1\publish

COPY $PATH App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]