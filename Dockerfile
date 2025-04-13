FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["NavData.csproj", "./"]
RUN dotnet restore "NavData.csproj"
COPY . .
WORKDIR "/src/"
RUN npm install 
RUN dotnet build "NavData.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "NavData.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apt-get update \
    && apt-get install -y wget curl
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NavData.dll"]
