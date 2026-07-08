FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Services/AuthService/AuthService.csproj", "Services/AuthService/"]
RUN dotnet restore "Services/AuthService/AuthService.csproj"

# Copy source and publish
COPY Services/AuthService/ Services/AuthService/
WORKDIR "/src/Services/AuthService"
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "AuthService.dll"]
