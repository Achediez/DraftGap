# 🏗️ Arquitectura del Proyecto - DraftGap Backend

## 📋 Tabla de Contenidos
- [Visión General](#visión-general)
- [Arquitectura por Capas](#arquitectura-por-capas)
- [Estructura de Carpetas](#estructura-de-carpetas)
- [Patrones de Diseño](#patrones-de-diseño)
- [Flujo de Datos](#flujo-de-datos)

---

## 🎯 Visión General

DraftGap Backend es una API REST construida con **.NET 9** siguiendo los principios de **Clean Architecture** y **Domain-Driven Design (DDD)**.

### Principios Arquitectónicos

✅ **Separación de Responsabilidades** - Cada capa tiene un propósito específico  
✅ **Inversión de Dependencias** - Las capas internas no conocen las externas  
✅ **Inyección de Dependencias** - Acoplamiento bajo mediante interfaces  
✅ **SOLID Principles** - Código mantenible y escalable  

---

## 🏗️ Arquitectura por Capas

```
┌─────────────────────────────────────────────────────────┐
│              🌐 API Layer (Presentation)                │
│  Controllers, Middleware, Authentication, Validation    │
└─────────────────────┬───────────────────────────────────┘
                      │ DTOs
┌─────────────────────▼───────────────────────────────────┐
│           📦 Application Layer (Use Cases)              │
│    Interfaces, DTOs, Validators, Business Rules         │
└─────────────────────┬───────────────────────────────────┘
                      │ Entities, Abstractions
┌─────────────────────▼───────────────────────────────────┐
│            💼 Domain Layer (Business Logic)             │
│         Entities, Value Objects, Abstractions           │
└─────────────────────┬───────────────────────────────────┘
                      │ Implementations
┌─────────────────────▼───────────────────────────────────┐
│      🔧 Infrastructure Layer (Data & External APIs)     │
│  Repositories, Services, Database, Riot API Integration │
└─────────────────────────────────────────────────────────┘
```

---

## 📂 Estructura de Carpetas

### 🎨 **API Layer** (`DraftGapBackend.Api`)

```
Api/
├── Controllers/          # Endpoints REST
│   ├── AuthController.cs        # Autenticación y registro
│   ├── ProfileController.cs     # Gestión de perfil
│   ├── DashboardController.cs   # Dashboard resumen
│   ├── MatchesController.cs     # Historial de partidas
│   ├── ChampionsController.cs   # Estadísticas de campeones
│   ├── RankedController.cs      # Stats de ranked
│   ├── FriendsController.cs     # Búsqueda de usuarios
│   ├── SyncController.cs        # Sincronización manual
│   └── AdminController.cs       # Operaciones admin
│
├── Middleware/          # Middleware personalizado
│   └── GlobalExceptionHandler.cs  # Manejo global de errores
│
└── Program.cs          # Configuración de la app (DI, JWT, CORS, Swagger)
```

**Responsabilidades:**
- 🔐 Autenticación y autorización (JWT)
- ✅ Validación de requests
- 🔄 Transformación DTO ↔ Response
- 🚨 Manejo de errores
- 📊 Documentación Swagger/OpenAPI

---

### 📦 **Application Layer** (`DraftGapBackend.Application`)

```
Application/
├── Dtos/                    # 🆕 Data Transfer Objects (reorganizados)
│   ├── Common/             # DTOs compartidos
│   │   ├── PaginationDto.cs       # Paginación estándar
│   │   └── ApiResponse.cs         # Wrapper de respuestas
│   ├── Profile/            # DTOs de perfil
│   │   └── ProfileDto.cs
│   ├── Dashboard/          # DTOs de dashboard
│   │   └── DashboardDto.cs
│   ├── Matches/            # DTOs de partidas
│   │   └── MatchDto.cs
│   ├── Champions/          # DTOs de campeones
│   │   └── ChampionDto.cs
│   ├── Ranked/             # DTOs de ranked
│   │   └── RankedDto.cs
│   ├── Friends/            # DTOs de amigos
│   │   └── FriendsDto.cs
│   ├── Sync/               # DTOs de sincronización
│   │   └── SyncDto.cs
│   └── Admin/              # DTOs administrativos
│       └── AdminDto.cs
│
├── Validators/              # 🆕 Validadores (centralizados)
│   ├── CommonValidators.cs      # Validación de paginación
│   ├── ProfileValidators.cs     # Validación de perfil
│   ├── MatchValidators.cs       # Validación de filtros
│   └── FriendsValidators.cs     # Validación de búsqueda
│
└── Interfaces/              # Contratos de servicios
    ├── IProfileService.cs
    ├── IDashboardService.cs
    ├── IMatchService.cs
    ├── IChampionService.cs
    ├── IRankedService.cs
    ├── IFriendsService.cs
    └── IUserSyncService.cs
```

**Responsabilidades:**
- 🎯 Definir casos de uso (interfaces)
- 📋 DTOs para transferencia de datos
- ✅ Validación con FluentValidation
- 🔄 Mapeo de datos entre capas

**Convenciones de Namespace:**
```csharp
DraftGapBackend.Application.Dtos.Profile
DraftGapBackend.Application.Validators
DraftGapBackend.Application.Interfaces
```

---

### 💼 **Domain Layer** (`DraftGapBackend.Domain`)

```
Domain/
├── Entities/            # Entidades de dominio (User, Match, Player, etc.)
│   ├── User.cs
│   ├── Match.cs
│   ├── MatchParticipant.cs
│   ├── Player.cs
│   ├── Champion.cs
│   └── ...
│
└── Abstractions/        # Interfaces de repositorios
    ├── IUserRepository.cs
    ├── IMatchRepository.cs
    ├── IChampionRepository.cs
    ├── IRankedRepository.cs
    └── IPlayerRepository.cs
```

**Responsabilidades:**
- 🏢 Entidades de negocio (sin lógica de persistencia)
- 🔗 Definir abstracciones (interfaces de repositorios)
- 🎨 Lógica de dominio pura

**Características:**
- ❌ Sin dependencias externas
- ✅ POCO (Plain Old CLR Objects)
- ✅ Modelos ricos con validación

---

### 🔧 **Infrastructure Layer** (`DraftGapBackend.Infrastructure`)

```
Infrastructure/
├── Services/            # Implementaciones de servicios
│   ├── ProfileService.cs      # Gestión de perfil
│   ├── DashboardService.cs    # Agregaciones de dashboard
│   ├── MatchService.cs        # Historial y filtros
│   ├── ChampionService.cs     # Stats de campeones
│   ├── RankedService.cs       # Stats de ranked
│   ├── FriendsService.cs      # Búsqueda de usuarios
│   ├── UserSyncService.cs     # Sincronización manual
│   ├── DataSyncService.cs     # Sync con Riot API
│   └── RiotService.cs         # Cliente de Riot API
│
├── Persistence/         # Implementaciones de repositorios
│   ├── UserRepository.cs
│   ├── MatchRepository.cs
│   ├── ChampionRepository.cs
│   ├── RankedRepository.cs
│   └── PlayerRepository.cs
│
├── Data/                # Configuración de base de datos
│   └── ApplicationDbContext.cs
│
└── BackgroundServices/  # Workers en background
    └── RiotSyncBackgroundService.cs
```

**Responsabilidades:**
- 💾 Persistencia de datos (EF Core + MySQL)
- 🌐 Integración con APIs externas (Riot API)
- ⚙️ Servicios de infraestructura
- 🔄 Background workers

---

## 🎨 Patrones de Diseño

### 🏭 **Repository Pattern**
Abstracción del acceso a datos.

```csharp
// Domain: Interface
public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(string matchId);
    Task<List<MatchParticipant>> GetUserMatchParticipantsAsync(string puuid, int skip, int take);
}

// Infrastructure: Implementation
public class MatchRepository : IMatchRepository
{
    private readonly ApplicationDbContext _context;
    // Implementación con EF Core
}
```

### 💉 **Dependency Injection**
Todas las dependencias se inyectan mediante interfaces.

```csharp
// Program.cs
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
```

### 🎯 **Service Layer Pattern**
Lógica de negocio en servicios.

```csharp
public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IRankedRepository _rankedRepository;
    
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId)
    {
        // Agregación de datos de múltiples fuentes
    }
}
```

### 📦 **DTO Pattern**
Objetos específicos para transferencia de datos.

```csharp
// Request DTO
public class UpdateProfileRequest
{
    public string? RiotId { get; set; }
    public string? Region { get; set; }
}

// Response DTO
public class ProfileDto
{
    public string Email { get; set; }
    public ProfileSummonerDto? Summoner { get; set; }
}
```

### ✅ **Validator Pattern** (FluentValidation)
Validación declarativa de DTOs.

```csharp
public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.RiotId), () =>
        {
            RuleFor(x => x.RiotId)
                .Must(id => id.Contains('#'))
                .WithMessage("Riot ID must be in format: GameName#TAG");
        });
    }
}
```

---

## 🔄 Flujo de Datos

### Ejemplo: Obtener Dashboard

```
1. 📥 Request HTTP
   GET /api/dashboard/summary
   Authorization: Bearer <token>

2. 🎯 Controller (API Layer)
   DashboardController.GetDashboardSummary()
   ├─ Extrae userId del token JWT
   └─ Llama a IDashboardService

3. 📦 Service (Infrastructure Layer)
   DashboardService.GetDashboardSummaryAsync(userId)
   ├─ Obtiene datos de IUserRepository
   ├─ Obtiene ranked stats de IRankedRepository
   ├─ Obtiene partidas de IMatchRepository
   ├─ Calcula agregaciones (KDA, winrate)
   └─ Mapea a DashboardSummaryDto

4. 💾 Repository (Infrastructure Layer)
   MatchRepository.GetUserMatchParticipantsAsync()
   └─ Query a MySQL con EF Core

5. 📤 Response HTTP
   200 OK
   {
     "rankedOverview": {...},
     "recentMatches": [...],
     "performanceStats": {...},
     "topChampions": [...]
   }
```

---

## 🔐 Autenticación y Autorización

### JWT Authentication Flow

```
1. 🔑 Login
   POST /api/auth/login
   {
     "email": "user@example.com",
     "password": "password123"
   }

2. ✅ Validación
   UserService.LoginAsync()
   ├─ Busca usuario en BD
   ├─ Verifica password con BCrypt
   └─ Genera JWT token

3. 🎫 Token JWT
   {
     "userId": "...",
     "email": "...",
     "role": "User"
   }

4. 🔒 Requests Protegidos
   GET /api/profile
   Authorization: Bearer <token>
   
   Controller verifica:
   ├─ [Authorize] attribute
   ├─ Token válido y no expirado
   └─ Claims presentes (userId, role)
```

---

## 📊 Base de Datos

### Entidades Principales

```
users ──────┬────> players (PUUID)
            │
            ├────> match_participants (PUUID)
            │      └────> matches (matchId)
            │
            └────> player_ranked_stats (PUUID)
                   
champions ─────> match_participants (championId)
sync_jobs ─────> users (userId)
```

### Estrategia de Sincronización

1. **Registro**: Se vincula Riot account → PUUID
2. **Background Worker**: Procesa `sync_jobs` cada 5 segundos
3. **DataSyncService**: 
   - Actualiza ranked stats desde Riot API
   - Obtiene nuevas partidas desde Riot API
   - Persiste en BD con transacciones

---

## 🧪 Testing

```
tests/DraftGapBackend.Tests/
├── Controllers/         # Tests de integración de endpoints
├── Services/            # Tests unitarios de servicios
└── Validators/          # Tests de validación
```

**Herramientas:**
- xUnit
- Moq (mocking)
- FluentAssertions

---

## 🚀 Mejoras Futuras

### 📈 Performance
- [ ] Implementar caching (Redis)
- [ ] Optimizar queries N+1
- [ ] Implementar CQRS para queries complejas

### 🔐 Seguridad
- [ ] Rate limiting por usuario
- [ ] Refresh token rotation
- [ ] HTTPS only en producción

### 🏗️ Arquitectura
- [ ] Implementar MediatR para CQRS
- [ ] Event sourcing para auditoría
- [ ] Microservicios (separar sync worker)

---

## 📚 Referencias

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET Architecture Guides](https://dotnet.microsoft.com/learn/dotnet/architecture-guides)
- [Entity Framework Core Best Practices](https://docs.microsoft.com/ef/core/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)

---

**Última actualización:** Febrero 2026  
**Versión:** .NET 9  
**Autor:** DraftGap Development Team
