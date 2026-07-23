# ETAPA 1: Compilación (Build)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo del proyecto y restaurar dependencias
COPY ["maxi_movie_mvc.csproj", "./"]
RUN dotnet restore "maxi_movie_mvc.csproj"

# Copiar todo el código fuente y compilar
COPY . .
RUN dotnet build "maxi_movie_mvc.csproj" -c Release -o /app/build
RUN dotnet publish "maxi_movie_mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Ejecución (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto predeterminado que usa Railway
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "maxi_movie_mvc.dll"]