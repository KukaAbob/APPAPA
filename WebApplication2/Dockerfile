# Указываем базовый образ с ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Указываем образ для сборки с .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем файл проекта и выполняем dotnet restore
COPY ["WebApplication2/WebApplication2.csproj", "WebApplication2/"]
RUN dotnet restore "./WebApplication2/WebApplication2.csproj"

# Копируем остальные файлы и выполняем сборку проекта
COPY . .
WORKDIR "/src/WebApplication2"
RUN dotnet build "./WebApplication2.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация приложения
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApplication2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ, на котором будет запущено приложение
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Указываем точку входа для запуска приложения
ENTRYPOINT ["dotnet", "WebApplication2.dll"]
