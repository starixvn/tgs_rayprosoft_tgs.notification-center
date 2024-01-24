#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.NotificationCenter/Tgs.NotificationCenter.csproj", "Tgs.NotificationCenter/"]
COPY ["Notification.Core/Notification.Core.csproj", "Notification.Core/"]
COPY ["Notification.Common/Notification.Common.csproj", "Notification.Common/"]
COPY ["Notification.Repositories/Notification.Repositories.csproj", "Notification.Repositories/"]
RUN dotnet restore "./Tgs.NotificationCenter/./Tgs.NotificationCenter.csproj"
COPY . .
WORKDIR "/src/Tgs.NotificationCenter"
RUN dotnet build "./Tgs.NotificationCenter.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.NotificationCenter.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.NotificationCenter.dll"]