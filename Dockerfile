# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Restore dependencies (layer cache)
COPY ["SingleSignOnDemo/SingleSignOnDemo.csproj", "SingleSignOnDemo/"]
COPY ["SaaSDataLayer/SaaSDataLayer.csproj", "SaaSDataLayer/"]
RUN dotnet restore "SingleSignOnDemo/SingleSignOnDemo.csproj"

# Publish
COPY . .
WORKDIR /src/SingleSignOnDemo
RUN dotnet publish "SingleSignOnDemo.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production \
    HTTP_PORT=8080 \
    ENABLE_HTTPS=false

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "SingleSignOnDemo.dll"]
