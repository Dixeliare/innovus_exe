# Multi-stage Dockerfile for Innovus_exe (monolith)
# Builds the solution and publishes the Web_API project to a small runtime image.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files (restore dependency graph fast)
COPY *.sln ./
COPY Web_API/*.csproj ./Web_API/
COPY Services/*.csproj ./Services/
COPY Repository/*.csproj ./Repository/
COPY DTOs/*.csproj ./DTOs/

# Restore packages
RUN dotnet restore --no-cache

# Copy everything and publish
COPY . ./
WORKDIR /src/Web_API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Configure runtime
ENV ASPNETCORE_URLS="http://+:8080"
EXPOSE 8080

ENTRYPOINT ["dotnet", "Web_API.dll"]
