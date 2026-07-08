# Multi-stage Dockerfile for ChatService
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Services/ChatService/ChatService.csproj", "Services/ChatService/"]
RUN dotnet restore "Services/ChatService/ChatService.csproj"

# Copy the remaining source files and build
COPY . .
WORKDIR "/src/Services/ChatService"
RUN dotnet build "ChatService.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ChatService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port (HTTP port defined in launchSettings.json / appsettings.json)
EXPOSE 5203
ENV ASPNETCORE_URLS=http://+:5203
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ChatService.dll"]
