FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Services/PostService/PostService.csproj", "Services/PostService/"]
RUN dotnet restore "Services/PostService/PostService.csproj"

# Copy source and publish
COPY Services/PostService/ Services/PostService/
WORKDIR "/src/Services/PostService"
RUN dotnet publish "PostService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "PostService.dll"]
