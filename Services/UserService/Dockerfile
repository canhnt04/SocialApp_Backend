FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Services/UserService/UserService.csproj", "Services/UserService/"]
RUN dotnet restore "Services/UserService/UserService.csproj"

# Copy source and publish
COPY Services/UserService/ Services/UserService/
WORKDIR "/src/Services/UserService"
RUN dotnet publish "UserService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "UserService.dll"]
