# ETAPA 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia todos los archivos .csproj conservando la estructura de carpetas
COPY ["maxi-movie-mvc/*.csproj", "maxi-movie-mvc/"]
RUN dotnet restore "maxi-movie-mvc/*.csproj"

# Copia todo el código fuente
COPY . .
WORKDIR "/src/maxi-movie-mvc"

# Compila y publica el proyecto de la carpeta actual
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Revisa el nombre exacto del .dll que genera tu proyecto (suele ser maxi-movie-mvc.dll o maxi_movie_mvc.dll)
ENTRYPOINT ["dotnet", "maxi-movie-mvc.dll"]