FROM mcr.microsoft.com/dotnet/aspnet:5.0

LABEL "com.datadoghq.ad.logs"='[{"source": "dotnetclr", "service": "discord-bot"}]'

COPY build/CloudNine App/
WORKDIR /App
ENTRYPOINT ["dotnet", "CloudNine.dll"]
