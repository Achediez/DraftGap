# DraftGap

**DraftGap** es un tracker de estadísticas para League of Legends, similar a plataformas como u.gg u op.gg. Permite registrar y vincular una cuenta de usuario con un perfil de Riot Games, validar el Riot ID / PUUID en tiempo real contra la API de Riot, y persiste toda la información en una base de datos MySQL. El proyecto está construido sobre una arquitectura limpia (Clean Architecture / DDD) con un backend en ASP.NET Core 8 y un frontend en Angular 17+.

---

## Índice

1. [Stack Tecnológico](#stack-tecnológico)
2. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
3. [Estructura de Directorios](#estructura-de-directorios)
4. [Mapa de Pantallas y Rutas](#mapa-de-pantallas-y-rutas)
5. [Endpoints de la API](#endpoints-de-la-api)
6. [Configuración del Entorno](#configuración-del-entorno)
7. [Puesta en Marcha con Docker](#puesta-en-marcha-con-docker)
8. [Puesta en Marcha en Local (sin Docker)](#puesta-en-marcha-en-local-sin-docker)
9. [Variables de Entorno y Secretos](#variables-de-entorno-y-secretos)
10. [Usuarios de Prueba](#usuarios-de-prueba)
11. [Notas de Desarrollo](#notas-de-desarrollo)

---

## Stack Tecnológico

| Capa                      | Tecnología                                           |
| ------------------------- | ----------------------------------------------------- |
| Backend                   | ASP.NET Core 8, C#                                    |
| ORM / Base de Datos       | Entity Framework Core 8 + MySQL 8 (Pomelo)            |
| Autenticación            | JWT Bearer (HS256)                                    |
| Caché                    | Redis 7                                               |
| Frontend                  | Angular 17+ (Standalone Components, Signals, OnPush)  |
| Contenerización          | Docker + Docker Compose                               |
| Gestor de paquetes (.NET) | NuGet                                                 |
| Gestor de paquetes (Node) | npm                                                   |
| Proxy de desarrollo       | `proxy.conf.json` (Angular → Backend en `:5000`) |

---

## Arquitectura del Proyecto

El backend sigue **Domain-Driven Design (DDD)** con separación estricta en cuatro capas:

- **Domain** — Entidades puras e interfaces de repositorio. Sin dependencias externas.
- **Application** — Casos de uso, servicios de aplicación, DTOs y validadores.
- **Infrastructure** — Implementaciones de repositorio (EF Core), integración con la Riot API (`RiotService`, `DataDragonService`) y sincronización en background.
- **API** — Controladores HTTP, configuración de middlewares y punto de entrada (`Program.cs`).

El frontend Angular sigue una arquitectura basada en **feature modules** con separación en capas:

- **`core/`** — Servicios transversales (token, interceptor HTTP, configuración global).
- **`features/`** — Módulos funcionales independientes: `auth`, `admin`, `dashboard`.

---

## Estructura de Directorios

```
DraftGap/
├── backend/
│   ├── claves/
│   │   └── claves.json                         # Archivo local de claves — NO commiteado (ver .gitignore)
│   ├── db/
│   │   ├── docker-compose.yml                  # Compose específico para levantar solo la BD
│   │   ├── init.sql                            # Script de inicialización del esquema MySQL
│   │   └── setup.md                            # Instrucciones de configuración de la BD
│   ├── src/
│   │   ├── DraftGapBackend.Api/                # Capa de presentación
│   │   │   ├── Controllers/
│   │   │   │   ├── AdminController.cs          # Endpoints protegidos [Authorize(Roles="Admin")] — CRUD de usuarios
│   │   │   │   ├── AuthController.cs           # Registro, login, /me
│   │   │   │   └── RiotController.cs           # Consultas a Riot API expuestas al cliente
│   │   │   ├── Program.cs                      # Configuración completa del pipeline (JWT, EF Core, Redis, CORS, Swagger)
│   │   │   ├── appsettings.json                # Configuración no sensible (seguro para Git)
│   │   │   └── appsettings.Development.json    # Overrides de desarrollo (no commiteado)
│   │   ├── DraftGapBackend.Application/        # Capa de aplicación
│   │   │   ├── Interfaces/
│   │   │   │   └── IDataSyncService.cs
│   │   │   └── Users/
│   │   │       ├── IUserService.cs
│   │   │       ├── UserService.cs
│   │   │       ├── UserDtos.cs
│   │   │       └── UserValidator.cs
│   │   ├── DraftGapBackend.Domain/             # Capa de dominio — sin dependencias externas
│   │   │   ├── Abstractions/
│   │   │   │   └── IUserRepository.cs
│   │   │   └── Entities/
│   │   │       ├── User.cs                     # Usuario de la aplicación (auth)
│   │   │       ├── Player.cs                   # Perfil de Riot Games (PUUID, summoner data)
│   │   │       ├── Champion.cs                 # Datos estáticos de campeones (Data Dragon)
│   │   │       ├── Match.cs                    # Metadatos de partida
│   │   │       ├── MatchParticipant.cs         # Rendimiento de un jugador en una partida
│   │   │       ├── PlayerRankedStat.cs         # Estadísticas de ranked por tipo de cola
│   │   │       ├── Items.cs                    # Datos estáticos de objetos
│   │   │       ├── Rune.cs                     # Datos estáticos de runas
│   │   │       ├── RunePath.cs                 # Árboles de runas
│   │   │       ├── SummonerSpells.cs           # Hechizos de invocador
│   │   │       └── SyncJob.cs                  # Registro de jobs de sincronización en background
│   │   └── DraftGapBackend.Infrastructure/     # Capa de infraestructura
│   │       ├── Data/
│   │       │   ├── ApplicationDbContext.cs     # DbContext de EF Core — mapea todas las entidades a MySQL
│   │       │   └── Configurations/             # Fluent API config por entidad (IEntityTypeConfiguration)
│   │       │       ├── ChampionConfiguration.cs
│   │       │       ├── ItemConfiguration.cs
│   │       │       ├── MatchConfiguration.cs
│   │       │       ├── MatchParticipantConfiguration.cs
│   │       │       ├── PlayerConfiguration.cs
│   │       │       ├── PlayerRankedStatConfiguration.cs
│   │       │       ├── RuneConfiguration.cs
│   │       │       ├── RunePathConfiguration.cs
│   │       │       ├── SummonerSpellConfiguration.cs
│   │       │       ├── SyncJobConfiguration.cs
│   │       │       └── UserConfiguration.cs
│   │       ├── Persistence/
│   │       │   ├── UserRepository.cs           # Implementación real con EF Core + MySQL
│   │       │   └── InMemoryUserRepository.cs   # Implementación en memoria para testing rápido
│   │       ├── Riot/
│   │       │   ├── RiotService.cs              # Llamadas a Riot Games API (cuenta, partidas)
│   │       │   ├── DataDragonService.cs        # Sincronización de datos estáticos (campeones, items, runas)
│   │       │   ├── IRiotService.cs
│   │       │   ├── IDataDragonService.cs
│   │       │   └── RiotApiModels.cs            # DTOs de respuesta de la Riot API
│   │       ├── Sync/
│   │       │   ├── DataSyncService.cs          # Lógica de sincronización de historial de partidas
│   │       │   └── RiotSyncBackgroundService.cs # IHostedService — ejecuta sync periódicamente
│   │       └── DependencyInjection.cs          # Registro de todos los servicios de Infrastructure
│   ├── tests/
│   │   └── DraftGapBackend.Tests/
│   │       └── UnitTest1.cs
│   └── DraftGapBackendSolucion.sln
├── frontend/
│   └── draftgap-app/
│       └── src/
│           └── app/
│               ├── core/
│               │   ├── auth/
│               │   │   └── auth-token.service.ts      # Gestión del JWT en localStorage
│               │   ├── config/
│               │   │   └── app-settings.ts            # URL base de la API (token de inyección)
│               │   └── http/
│               │       └── auth.interceptor.ts        # Inyecta "Authorization: Bearer <token>" en cada petición
│               └── features/
│                   ├── admin/
│                   │   ├── admin-api.service.ts       # Llamadas HTTP al AdminController
│                   │   ├── admin.component.ts         # Panel de administración — CRUD de usuarios
│                   │   ├── admin.component.html
│                   │   └── admin.component.scss
│                   ├── auth/
│                   │   ├── data/
│                   │   │   └── auth-api.service.ts    # Llamadas HTTP a AuthController (login, register)
│                   │   ├── models/
│                   │   │   └── auth.models.ts         # Interfaces TypeScript — mapean exactamente la respuesta del backend
│                   │   ├── pages/auth-page/
│                   │   │   ├── auth-page.component.ts # Formularios de login y registro con Signals
│                   │   │   ├── auth-page.component.html
│                   │   │   └── auth-page.component.scss
│                   │   └── services/
│                   │       └── auth.service.ts        # Estado reactivo de autenticación (Signals)
│                   └── dashboard/
│                       ├── dashboard.component.ts     # Vista principal del usuario autenticado
│                       ├── dashboard.component.html
│                       └── dashboard.component.scss
├── docs/
├── Dockerfile.backend
├── Dockerfile.frontend
├── Dockerfile.database
├── docker-compose.yml                                 # Compose raíz — levanta todos los servicios
└── .gitignore
```

---

## Mapa de Pantallas y Rutas

### Frontend Angular

| Ruta           | Componente             |    Acceso    | Descripción                                                                 |
| -------------- | ---------------------- | :-----------: | ---------------------------------------------------------------------------- |
| `/auth`      | `AuthPageComponent`  |   Público   | Formularios de login y registro. Redirige a `/dashboard` tras autenticarse |
| `/dashboard` | `DashboardComponent` |  JWT válido  | Vista principal del usuario — estadísticas y perfil                        |
| `/admin`     | `AdminComponent`     | Rol `Admin` | Panel de administración — listado, creación y borrado de usuarios         |

### Backend — Rutas principales

| Módulo | Ruta base          | Descripción                                        |
| ------- | ------------------ | --------------------------------------------------- |
| Auth    | `/api/auth`      | Registro, login, perfil del usuario autenticado     |
| Admin   | `/api/admin`     | Gestión de usuarios y sincronización (solo Admin) |
| Riot    | `/api/riot`      | Exposición de datos de Riot API al cliente         |
| Health  | `/health`        | Estado del servicio                                 |
| Swagger | `/` (desarrollo) | Documentación interactiva de la API                |

---

## Endpoints de la API

### Autenticación — `/api/auth`

| Método  | Ruta                   | Descripción                                        |  Auth  |
| -------- | ---------------------- | --------------------------------------------------- | :----: |
| `POST` | `/api/auth/register` | Registro con validación de Riot ID contra Riot API |   ❌   |
| `POST` | `/api/auth/login`    | Login con email y contraseña — devuelve JWT       |   ❌   |
| `GET`  | `/api/auth/me`       | Perfil del usuario autenticado                      | ✅ JWT |

### Administración — `/api/admin` (solo rol `Admin`)

| Método  | Ruta                       | Descripción                                         |   Auth   |
| -------- | -------------------------- | ---------------------------------------------------- | :------: |
| `GET`  | `/api/admin/stats`       | Estadísticas del sistema (usuarios, partidas, jobs) | ✅ Admin |
| `POST` | `/api/admin/sync`        | Dispara sincronización manual con Riot API          | ✅ Admin |
| `GET`  | `/api/admin/sync-status` | Estado de los jobs de sincronización activos        | ✅ Admin |

> Todos los endpoints protegidos requieren el header: `Authorization: Bearer <token>`

---

## Configuración del Entorno

### Requisitos previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows / macOS) o Docker Engine (Linux)
- [.NET SDK 8](https://dotnet.microsoft.com/download) — solo para desarrollo local sin Docker
- [Node.js 20+](https://nodejs.org/) — solo para desarrollo local del frontend
- Una **Riot API Key** obtenida en [developer.riotgames.com](https://developer.riotgames.com)

### Archivo `.env` (requerido para Docker)

Copia el archivo de ejemplo y rellena los valores:

```bash
cp .env.example .env
```

Contenido esperado en `.env` (este archivo **nunca** se sube a Git):

```env
# MySQL
MYSQL_ROOT_PASSWORD=tu-password-root
MYSQL_USER=draftgapuser
MYSQL_PASSWORD=tu-password-usuario

# Riot API
RIOT_API_KEY=RGAPI-tu-api-key-aqui

# JWT — genera con: openssl rand -base64 32
JWT_SECRET_KEY=tu-clave-secreta-jwt-minimo-32-chars
```

---

## Puesta en Marcha con Docker

Esta es la forma recomendada. Levanta todos los servicios (MySQL, Redis, backend y frontend) con un único comando desde la raíz del repositorio.

```bash
docker-compose up -d --build
```

Una vez iniciado, los servicios quedan disponibles en:

| Servicio             | URL                   |
| -------------------- | --------------------- |
| Frontend Angular     | http://localhost:4200 |
| Backend (Swagger UI) | http://localhost:5057 |
| MySQL                | localhost:3306        |
| Redis                | localhost:6379        |

Para detener todos los servicios:

```bash
docker-compose down
```

Para detener y eliminar los volúmenes (borra los datos de la BD):

```bash
docker-compose down -v
```

---

## Puesta en Marcha en Local (sin Docker)

### Backend

```bash
# 1. Asegúrate de tener MySQL corriendo localmente en el puerto 3306

# 2. Configura los secretos de usuario (nunca se suben a Git)
cd backend/src/DraftGapBackend.Api
dotnet user-secrets init
dotnet user-secrets set "RiotApi:ApiKey" "RGAPI-tu-api-key"
dotnet user-secrets set "Jwt:SecretKey" "tu-clave-secreta-jwt-minimo-32-chars"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=draftgap;Uid=root;Pwd=tu-password;OldGuids=true;"

# 3. Restaurar dependencias y ejecutar
dotnet restore
dotnet run
```

La API estará disponible en `http://localhost:5057`. Swagger UI accesible en la raíz.

### Frontend

```bash
cd frontend/draftgap-app
npm install
npx ng serve
```

El frontend estará disponible en `http://localhost:4200`. Las peticiones al backend se redirigen automáticamente a través de `proxy.conf.json`.

---

## Usuarios de Prueba

El usuario administrador se registra con el email configurado en `Admin:AllowedEmails` dentro de `appsettings.json`. Los usuarios normales requieren un Riot ID válido verificado contra la API de Riot Games.

| Tipo      | Email                        | Contraseña               | Riot ID              | Región |
| --------- | ---------------------------- | ------------------------- | -------------------- | ------- |
| Admin     | `admin@draftgap.local`     | `AdminTest123` | —                   | —      |
| Usuario A | `promister@draftgap.local` | `AdminTest123`        | `DG Pr0m1sTeR#GGG` | EUW     |
| Usuario B | `achedie@draftgap.local`   | `AdminTest123`        | `DG AchediezH10`   | EUW     |

> El sistema solo acepta los Riot IDs `DG Pr0m1sTeR#GGG` y `DG AchediezH10` durante el registro (whitelist de desarrollo). Esto puede modificarse en `AuthController.cs`.

---

## Notas de Desarrollo

### Sincronización de Datos Estáticos (Data Dragon)

El servicio `DataDragonService` sincroniza campeones, items, runas y hechizos de invocador desde la CDN de Riot al arrancar la aplicación. La sincronización también puede dispararse manualmente desde el panel de administración (`POST /api/admin/sync`). Los datos se almacenan en la BD local y se sirven desde ahí — no se realizan llamadas a Data Dragon en tiempo real.

### Generación del JWT Secret

```bash
openssl rand -base64 32
```

### Rama activa de desarrollo

- `main` — Código frontend estable
- `RiotAPI-Fetch` — Integración backend con Riot API

### Estructura de la solución .NET

```bash
# Desde backend/
dotnet build DraftGapBackendSolucion.sln
dotnet test tests/DraftGapBackend.Tests/
```
