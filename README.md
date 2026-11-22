# 🎬 MaxiMovie

Catálogo de películas con reseñas de usuarios y resúmenes generados con IA. Aplicación web full-stack construida con **ASP.NET Core MVC (.NET 9)**, **PostgreSQL** y **Google Gemini**, empaquetada con **Docker** y desplegada en un servidor propio.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Google Gemini](https://img.shields.io/badge/Gemini-2.5_Flash-8E7CC3?style=flat&logo=googlegemini&logoColor=white)](https://ai.google.dev/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker&logoColor=white)](https://docs.docker.com/compose/)
[![Demo](https://img.shields.io/badge/Demo-en_vivo-brightgreen?style=flat)](https://maximovie.robertomartinez.dev)

**🔗 Demo en vivo: [maximovie.robertomartinez.dev](https://maximovie.robertomartinez.dev)**

> La demo corre en un servidor propio (home lab), por lo que pueden ocurrir cortes ocasionales.

---

## 📸 Capturas

| Catálogo | Detalle y reseñas | Resumen con IA |
|---|---|---|
| ![Catálogo](docs/screenshots/home.png) | ![Detalle](docs/screenshots/detalle.png) | ![Resumen con IA](docs/screenshots/resumen-ia.png) |

---

## ✨ Funcionalidades

**Catálogo**
- Búsqueda por título, filtro por género y vista de cuadrícula o de lista.
- Cada película tiene su género y su plataforma de streaming (Netflix, Prime Video, Disney+, Max).

**Reseñas**
- Calificación y comentario por película, con estado oculto/visible para moderación.
- Control de concurrencia optimista usando la columna `xmin` de PostgreSQL.

**IA generativa (Google Gemini 2.5 Flash)**
- Resumen y spoiler de cada película, generados mediante `Microsoft.Extensions.AI`.
- **Caché persistente en la base de datos:** cada respuesta se guarda y se reutiliza, lo que reduce llamadas a la API, costo y latencia.
- Degradación elegante: si no hay API key, la aplicación arranca igual y las funciones de IA quedan deshabilitadas.

**Cuentas y seguridad**
- Registro e inicio de sesión con ASP.NET Core Identity, con roles `Admin` y `User`.
- Recuperación de contraseña por correo con token, y correo de bienvenida.
- Claves de DataProtection guardadas en la base de datos: las sesiones sobreviven al reinicio o recreación de los contenedores.

**Avatares**
- Subida de imagen con verificación de la firma real del archivo (anti-spoofing) usando ImageSharp.
- Recorte y conversión automática a `.webp` de 512×512, con límite de 2 MB.

**Correo transaccional**
- Envío con `MailKit` por SMTP cifrado (`StartTls`).

---

## 🏗️ Arquitectura de despliegue

```mermaid
flowchart LR
    U[Usuario] -->|HTTPS| CF[Cloudflare]
    CF -->|Túnel| CD[cloudflared]
    CD --> CA[Caddy]
    CA --> APP
    subgraph DC["Docker Compose"]
        APP["MaxiMovie<br/>ASP.NET Core 9"] --> DB[("PostgreSQL 16")]
    end
    APP -->|API| GEM[Google Gemini]
    APP -->|SMTP| MAIL[Servidor de correo]
```

La base de datos vive en una red interna de Docker sin puertos publicados, y la aplicación solo escucha en la interfaz local del servidor: todo el tráfico entra a través del proxy inverso y el túnel.

---

## 🧠 Decisiones técnicas

| Decisión | Motivo |
|---|---|
| Caché de respuestas de IA en PostgreSQL | Evitar llamadas repetidas a Gemini para el mismo contenido. |
| Claves de DataProtection en la base de datos | Mantener cookies de sesión válidas tras reiniciar o recrear contenedores. |
| `xmin` como token de concurrencia en reseñas | Detectar ediciones simultáneas sin agregar columnas de versión. |
| Configuración por variables de entorno | Ningún secreto vive en el repositorio. |
| Cadena de conexión flexible | Acepta `ConnectionStrings__MovieDbContext` o una `DATABASE_URL` con formato `postgres://...`, según dónde se despliegue. |
| Migraciones y `DbSeeder` al iniciar | La base se crea y se siembra (roles, admin, géneros, plataformas y 20 películas) sin pasos manuales. |
| `Dockerfile` multi-etapa | Imagen final basada solo en el runtime de ASP.NET. |

---

## 🛠️ Stack

- **Lenguaje y framework:** C# (.NET 9), ASP.NET Core MVC
- **Datos:** Entity Framework Core (Code-First), PostgreSQL 16, `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Identidad:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore`
- **IA:** `Microsoft.Extensions.AI`, `Google.GenAI`
- **Imágenes:** `SixLabors.ImageSharp`
- **Correo:** `MailKit`, `MimeKit`
- **Infraestructura:** Docker, Docker Compose, Caddy, Cloudflare Tunnel

---

## 🚀 Cómo ejecutarlo

### Opción 1: Docker Compose (recomendada)

Requisitos: Docker con Compose.

```bash
git clone https://github.com/Rml2734/ejemplo-maxi-movie.git
cd ejemplo-maxi-movie
cp .env.example .env        # en PowerShell: Copy-Item .env.example .env
```

Edita `.env` y define como mínimo `DB_PASSWORD` y `ADMIN_PASSWORD`. Después:

```bash
docker compose up -d --build
```

Abre http://localhost:8080 e inicia sesión con `admin@admin.com` y la contraseña que definiste en `ADMIN_PASSWORD`.

> ⚠️ **El usuario admin se crea solo en el primer arranque**, cuando la base está vacía y `ADMIN_PASSWORD` tiene valor. Si arrancas sin esa variable, borra el volumen (`docker compose down -v`) y vuelve a empezar.

### Opción 2: Ejecución local con .NET SDK

Requisitos: [.NET 9 SDK](https://dotnet.microsoft.com/download) y PostgreSQL 16.

Bash:

```bash
export ConnectionStrings__MovieDbContext="Host=localhost;Port=5432;Database=MaxiMovieDb;Username=postgres;Password=tu_password"
export AdminCredentials__Password="una_clave_segura"
export GEMINI_API_KEY="tu_api_key"   # opcional
dotnet run --project maxi-movie-mvc
```

PowerShell:

```powershell
$env:ConnectionStrings__MovieDbContext="Host=localhost;Port=5432;Database=MaxiMovieDb;Username=postgres;Password=tu_password"
$env:AdminCredentials__Password="una_clave_segura"
$env:GEMINI_API_KEY="tu_api_key"   # opcional
dotnet run --project maxi-movie-mvc
```

La aplicación aplica las migraciones pendientes y ejecuta el `DbSeeder` al iniciar; la URL aparece en la consola. Una API key de Gemini se obtiene en [Google AI Studio](https://aistudio.google.com/apikey).

---

## ⚙️ Variables de configuración

| Variable | Descripción | ¿Requerida? |
|---|---|---|
| `ConnectionStrings__MovieDbContext` | Cadena de conexión de Npgsql. | Sí (o `DATABASE_URL`) |
| `DATABASE_URL` | URL `postgres://usuario:clave@host:puerto/base`. Fuerza SSL; pensada para servicios en la nube que entregan la base por URL. | Alternativa a la anterior |
| `AdminCredentials__Password` | Contraseña del administrador inicial. | Sí, para crear el admin |
| `AdminCredentials__Email` | Correo del administrador (por defecto `admin@admin.com`). | No |
| `GEMINI_API_KEY` (o `GOOGLE_API_KEY`) | Clave de Google Gemini. | No: sin ella la IA se deshabilita |
| `SmtpSettings__User` / `SmtpSettings__Password` | Cuenta SMTP (con Gmail, una contraseña de aplicación). | Necesaria para los correos |
| `SmtpSettings__Host` / `Port` / `FromName` / `UseStartTls` | Ajustes SMTP (por defecto `smtp.gmail.com`, 587, `MaxiMovie`, `true`). | No |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Poner en `true` al publicar detrás de un proxy inverso. | Solo detrás de un proxy |

---

## 📁 Estructura

```
.
├── Dockerfile
├── docker-compose.yml
├── .env.example
├── maxi-movie-mvc-EJEMPLO.sln
└── maxi-movie-mvc/
    ├── Controllers/
    ├── Data/          # DbContext y DbSeeder
    ├── Migrations/    # Migraciones de EF Core
    ├── Service/       # Correo (MailKit), almacenamiento de imágenes y otros servicios
    ├── Views/
    └── wwwroot/
```

---

## 🗺️ Próximos pasos

- Reintentos automáticos y modelo de respaldo ante picos de demanda de la API de Gemini.
- Pruebas automatizadas.
- Integración continua con GitHub Actions.

---

## 👤 Autor

**Roberto Martínez**
[GitHub](https://github.com/Rml2734) · [Portafolio](https://portafolio.robertomartinez.dev)
