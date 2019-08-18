FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 1883

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY MQTTServer/MQTTServer.csproj MQTTServer/
RUN dotnet restore MQTTServer/MQTTServer.csproj
COPY . .
WORKDIR /src/MQTTServer
RUN dotnet build MQTTServer.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish MQTTServer.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MQTTServer.dll"]
