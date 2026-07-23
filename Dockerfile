# ETAPA 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto .csproj desde la subcarpeta
COPY ["maxi-movie-mvc/maxi-movie-mvc.csproj", "maxi-movie-mvc/"]
RUN dotnet restore "maxi-movie-mvc/maxi-movie-mvc.csproj"

# Copiar el resto del código fuente
COPY . .
WORKDIR "/src/maxi-movie-mvc"

# Compilar y publicar
RUN dotnet build "maxi-movie-mvc.csproj" -c Release -o /app/build
RUN dotnet publish "maxi-movie-mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "maxi-movie-mvc.dll"]