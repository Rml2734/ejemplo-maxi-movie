# 🎬 MaxiMovie — Plataforma Web Full-Stack & IA Generativa

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat&logo=postgresql)](https://www.postgresql.org/)
[![Google Gemini](https://img.shields.io/badge/Google_Gemini-2.5_Flash-8E7CC3?style=flat&logo=googlegemini)](https://ai.google.dev/)
[![Railway](https://img.shields.io/badge/Deployed_on-Railway-0B0D0E?style=flat&logo=railway)](https://railway.app/)

Plataforma web desarrollada bajo el patrón **ASP.NET Core MVC** para la gestión de catálogos cinematográficos, sistema de reseñas de usuarios e integración avanzada con Inteligencia Artificial Generativa.

---

## 🌟 Características Claves

* **🤖 Integración con IA Generativa (Google Gemini 2.5 Flash):**
  * Generación dinámica de spoilers y resúmenes de películas utilizando `Microsoft.Extensions.AI`.
  * **Mecanismo de Caché Persistente en BD:** Almacena respuestas previas en PostgreSQL para reducir drásticamente el consumo de tokens de API y la latencia en lecturas repetidas.
* **🔐 Autenticación y Autorización (ASP.NET Core Identity):**
  * Gestión de usuarios, roles (Admin/User) y recuperación de contraseña.
  * **Persistencia de DataProtection:** Guardado de claves de cifrado en la base de datos para preservar cookies de sesión tras el reinicio de contenedores.
* **🖼️ Procesamiento Seguro de Archivos (ImageSharp):**
  * Subida de avatares con verificación de firma real de imagen (anti-spoofing).
  * Recorte y conversión automática a formato optimizado `.webp` (512x512).
* **✉️ Servicio de Correo Transaccional:** Notificaciones mediante `MailKit` con autenticación SMTP cifrada vía `StartTls`.
* **☁️ Despliegue Adaptativo:** Configuración inteligente de cadenas de conexión Npgsql capaz de parsear dinámicamente variables `DATABASE_URL` en entornos como **Railway**.

---

## 🛠️ Tecnologías Utilizadas

* **Lenguaje:** C# (.NET 9)
* **Framework Web:** ASP.NET Core MVC
* **ORM:** Entity Framework Core (Code-First)
* **Base de Datos:** PostgreSQL
* **Librerías / Paquetes Principales:**
  * `Microsoft.Extensions.AI` / `Google.GenAI`
  * `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  * `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`
  * `SixLabors.ImageSharp`
  * `MailKit` & `MimeKit`
  * `Npgsql.EntityFrameworkCore.PostgreSQL`

---

## 🚀 Configuración e Instalación Local
## Requisitos Previos
-.NET 9.0 SDK

-PostgreSQL instalado localmente o una instancia remota.

-Una API Key de Google Gemini (Google AI Studio).

## Pasos para Ejecutar
Clonar el repositorio:

git clone https://github.com/Rml2734/ejemplo-maxi-movie.git
cd ejemplo-maxi-movie

## Configurar las variables de entorno / appsettings.json:

Configurá la cadena de conexión a tu base de datos local y la clave de la API de Gemini:
```
{
"ConnectionStrings": {
"MovieDbContext": "Host=localhost;Database=MaxiMovieDb;Username=postgres;Password=tu_password"
},
"GEMINI_API_KEY": "TU_API_KEY_AQUI"
}
```

## Ejecutar migraciones e iniciar la aplicación:

dotnet ef database update
dotnet run --project maxi-movie-mvc

## Nota: La aplicación aplica automáticamente las migraciones pendientes y ejecuta el DbSeeder al iniciar.

## ⚙️ Despliegue
## La aplicación cuenta con soporte nativo para Docker e integración continua en Railway. Al detectar la 
## variable de entorno DATABASE_URL, el sistema adapta automáticamente los parámetros de conexión para el proveedor Npgsql.