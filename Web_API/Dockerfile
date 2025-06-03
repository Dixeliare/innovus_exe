#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081

#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["Web_API/Web_API.csproj", "Web_API/"]
#RUN dotnet restore "Web_API/Web_API.csproj"
#COPY . .
#WORKDIR "/src/Web_API"
#RUN dotnet build "./Web_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./Web_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#!ENTRYPOINT ["dotnet", "Web_API.dll"]




# Giai đoạn base (Runtime Environment)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080 # API sẽ lắng nghe trên cổng 8080 trong Docker
EXPOSE 8081

# Giai đoạn build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy file Solution đầu tiên để khôi phục các gói NuGet
COPY ["innovus_exe.sln", "."] 

# Copy từng project.csproj để tận dụng Docker cache khi chỉ thay đổi code, không phải dependencies
COPY ["Web_API/Web_API.csproj", "Web_API/"]
COPY ["Repository/Repository.csproj", "Repository/"] 
COPY ["Services/Services.csproj", "Services/"]       
COPY ["DTOs/DTOs.csproj", "DTOs/"]                   

# Chạy dotnet restore cho toàn bộ solution
RUN dotnet restore "innovus_exe.sln" 

# Copy toàn bộ source code của solution
COPY . .

# Di chuyển vào thư mục của project API chính để build và publish
WORKDIR "/src/Web_API"
RUN dotnet build "./Web_API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Giai đoạn publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web_API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Giai đoạn final (Production ready image)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web_API.dll"]