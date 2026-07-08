FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Services/ChatService/ChatService.csproj", "Services/ChatService/"]
RUN dotnet restore "Services/ChatService/ChatService.csproj"

# Copy source and publish
COPY Services/ChatService/ Services/ChatService/
WORKDIR "/src/Services/ChatService"
RUN dotnet publish "ChatService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "ChatService.dll"]
