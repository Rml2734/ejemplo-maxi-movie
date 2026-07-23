# ETAPA 1: Compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Notar que ahora apunta a la subcarpeta:
COPY ["maxi-movie-mvc/maxi_movie_mvc.csproj", "maxi-movie-mvc/"]
RUN dotnet restore "maxi-movie-mvc/maxi_movie_mvc.csproj"

COPY . .
WORKDIR "/src/maxi-movie-mvc"
RUN dotnet build "maxi_movie_mvc.csproj" -c Release -o /app/build
RUN dotnet publish "maxi_movie_mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "maxi_movie_mvc.dll"]