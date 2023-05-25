#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.NotificationCenter/Tgs.NotificationCenter.csproj", "Tgs.NotificationCenter/"]
COPY ["Notification.Core/Notification.Core.csproj", "Notification.Core/"]
COPY ["Notification.Common/Notification.Common.csproj", "Notification.Common/"]
COPY ["Notification.Repositories/Notification.Repositories.csproj", "Notification.Repositories/"]
RUN dotnet restore "Tgs.NotificationCenter/Tgs.NotificationCenter.csproj"
COPY . .
WORKDIR "/src/Tgs.NotificationCenter"
RUN dotnet build "Tgs.NotificationCenter.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.NotificationCenter.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.NotificationCenter.dll"]