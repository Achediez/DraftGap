# DraftGap API - Documentación de Endpoints REST

## 📋 Índice
1. [Autenticación](#autenticación)
2. [Perfil de Usuario](#perfil-de-usuario)
3. [Dashboard](#dashboard)
4. [Matches](#matches)
5. [Champions](#champions)
6. [Ranked](#ranked)
7. [Friends](#friends)
8. [Sync](#sync)
9. [Admin](#admin)

---

## 🔐 Autenticación

### POST `/api/auth/register`
Registra un nuevo usuario con credenciales y Riot ID.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123",
  "riotId": "GameName#TAG",
  "region": "euw1"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "riotId": "GameName#TAG",
  "puuid": "abc123...",
  "region": "euw1",
  "isAdmin": false,
  "expiresAt": "2024-01-02T12:00:00Z"
}
```

### POST `/api/auth/login`
Inicia sesión con email y contraseña.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "riotId": "GameName#TAG",
  "isAdmin": false,
  "expiresAt": "2024-01-02T12:00:00Z"
}
```

### GET `/api/auth/me`
Obtiene información del usuario autenticado.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "email": "user@example.com",
  "riotId": "GameName#TAG",
  "isAdmin": false,
  "lastSync": "2024-01-01T10:30:00Z",
  "createdAt": "2023-12-01T08:00:00Z"
}
```

---

## 👤 Perfil de Usuario

### GET `/api/profile`
Obtiene el perfil completo del usuario autenticado.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "riotId": "GameName#TAG",
  "region": "euw1",
  "lastSync": "2024-01-01T10:30:00Z",
  "isAdmin": false,
  "createdAt": "2023-12-01T08:00:00Z",
  "summoner": {
    "puuid": "abc123...",
    "summonerName": "GameName",
    "profileIconId": 4901,
    "summonerLevel": 250
  }
}
```

### PUT `/api/profile`
Actualiza el perfil del usuario autenticado.

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "riotId": "NewName#TAG",
  "region": "na1"
}
```

**Response (200):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "riotId": "NewName#TAG",
  "region": "na1"
}
```

---

## 📊 Dashboard

### GET `/api/dashboard/summary`
Obtiene resumen del dashboard con ranked, matches recientes y top champions.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "rankedOverview": {
    "soloQueue": {
      "queueType": "RANKED_SOLO_5x5",
      "tier": "GOLD",
      "rank": "II",
      "leaguePoints": 45,
      "wins": 50,
      "losses": 45,
      "totalGames": 95,
      "winrate": 52.63
    },
    "flexQueue": null
  },
  "recentMatches": [
    {
      "matchId": "EUW1_123456",
      "gameCreation": 1700000000000,
      "gameDuration": 1800,
      "championName": "Aatrox",
      "win": true,
      "kills": 10,
      "deaths": 3,
      "assists": 7,
      "kda": 5.67,
      "teamPosition": "TOP"
    }
  ],
  "performanceStats": {
    "totalMatches": 20,
    "wins": 12,
    "losses": 8,
    "winrate": 60.0,
    "avgKills": 8.5,
    "avgDeaths": 4.2,
    "avgAssists": 6.8,
    "avgKda": 3.64
  },
  "topChampions": [
    {
      "championId": 266,
      "championName": "Aatrox",
      "gamesPlayed": 15,
      "wins": 9,
      "winrate": 60.0,
      "avgKda": 4.2
    }
  ]
}
```

---

## 🎮 Matches

### GET `/api/matches?page=1&pageSize=10`
Obtiene historial de partidas paginado con filtros opcionales.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `page` (int, default: 1): Número de página
- `pageSize` (int, default: 10, max: 100): Tamaño de página
- `championName` (string, optional): Filtrar por campeón
- `teamPosition` (string, optional): Filtrar por posición (TOP, JUNGLE, MIDDLE, BOTTOM, UTILITY)
- `win` (bool, optional): Filtrar por victoria (true/false)
- `queueId` (int, optional): Filtrar por tipo de cola (420 = Ranked Solo, 440 = Flex)

**Response (200):**
```json
{
  "items": [
    {
      "matchId": "EUW1_123456",
      "gameCreation": 1700000000000,
      "gameDuration": 1800,
      "championName": "Aatrox",
      "win": true,
      "kills": 10,
      "deaths": 3,
      "assists": 7,
      "kda": 5.67,
      "teamPosition": "TOP",
      "queueId": 420
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### GET `/api/matches/{matchId}`
Obtiene detalles completos de una partida específica.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "matchId": "EUW1_123456",
  "gameCreation": 1700000000000,
  "gameDuration": 1800,
  "gameMode": "CLASSIC",
  "queueId": 420,
  "gameVersion": "13.24.1",
  "teams": [
    {
      "teamId": 100,
      "win": true,
      "participants": [
        {
          "puuid": "abc123...",
          "riotIdGameName": "GameName#TAG",
          "championId": 266,
          "championName": "Aatrox",
          "champLevel": 18,
          "teamPosition": "TOP",
          "win": true,
          "kills": 10,
          "deaths": 3,
          "assists": 7,
          "kda": 5.67,
          "goldEarned": 15000,
          "totalDamageDealtToChampions": 25000,
          "totalDamageTaken": 30000,
          "visionScore": 35,
          "cs": 250,
          "item0": 3078,
          "item1": 6333,
          "summoner1Id": 4,
          "summoner2Id": 12,
          "primaryRuneId": 8005,
          "secondaryRunePathId": 8200
        }
      ]
    }
  ]
}
```

---

## 🏆 Champions

### GET `/api/champions`
Obtiene lista de todos los campeones (datos estáticos).

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
[
  {
    "championId": 266,
    "championKey": "Aatrox",
    "championName": "Aatrox",
    "title": "la Espada de los Oscuros",
    "imageUrl": "https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Aatrox.png",
    "version": "13.24.1"
  }
]
```

### GET `/api/champions/{championId}`
Obtiene datos de un campeón específico.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "championId": 266,
  "championKey": "Aatrox",
  "championName": "Aatrox",
  "title": "la Espada de los Oscuros",
  "imageUrl": "https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Aatrox.png",
  "version": "13.24.1"
}
```

### GET `/api/champions/stats`
Obtiene estadísticas de todos los campeones jugados por el usuario.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
[
  {
    "championId": 266,
    "championName": "Aatrox",
    "imageUrl": "https://ddragon.leagueoflegends.com/cdn/13.24.1/img/champion/Aatrox.png",
    "gamesPlayed": 25,
    "wins": 15,
    "losses": 10,
    "winrate": 60.0,
    "avgKills": 8.2,
    "avgDeaths": 4.5,
    "avgAssists": 6.3,
    "avgKda": 3.22,
    "totalKills": 205,
    "totalDeaths": 112,
    "totalAssists": 158
  }
]
```

---

## 🏅 Ranked

### GET `/api/ranked`
Obtiene estadísticas ranked del usuario (Solo/Duo y Flex).

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "soloQueue": {
    "queueType": "RANKED_SOLO_5x5",
    "tier": "GOLD",
    "rank": "II",
    "leaguePoints": 45,
    "wins": 50,
    "losses": 45,
    "totalGames": 95,
    "winrate": 52.63,
    "updatedAt": "2024-01-01T10:30:00Z"
  },
  "flexQueue": null
}
```

---

## 👥 Friends

### POST `/api/friends/search`
Busca un usuario por su Riot ID.

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "riotId": "GameName#TAG"
}
```

**Response (200):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "riotId": "GameName#TAG",
  "region": "euw1",
  "summonerName": "GameName",
  "profileIconId": 4901,
  "summonerLevel": 250,
  "isActive": true
}
```

---

## 🔄 Sync

### POST `/api/sync/trigger`
Dispara una sincronización manual de datos de Riot API.

**Headers:** `Authorization: Bearer {token}`

**Request Body (opcional):**
```json
{
  "forceRefresh": false
}
```

**Response (200):**
```json
{
  "jobId": 12345,
  "puuid": "abc123...",
  "jobType": "USER_SYNC",
  "status": "PENDING",
  "createdAt": "2024-01-01T12:00:00Z",
  "startedAt": null,
  "completedAt": null,
  "errorMessage": null
}
```

### GET `/api/sync/history`
Obtiene historial de sincronizaciones del usuario.

**Headers:** `Authorization: Bearer {token}`

**Response (200):**
```json
{
  "lastSync": "2024-01-01T10:30:00Z",
  "totalSyncs": 10,
  "successfulSyncs": 9,
  "failedSyncs": 1,
  "latestJob": {
    "jobId": 12345,
    "puuid": "abc123...",
    "jobType": "USER_SYNC",
    "status": "COMPLETED",
    "createdAt": "2024-01-01T10:00:00Z",
    "completedAt": "2024-01-01T10:30:00Z"
  }
}
```

---

## 🛡️ Admin (Requiere rol Admin)

### GET `/api/admin/users`
Lista todos los usuarios registrados.

**Headers:** `Authorization: Bearer {token}` (Admin)

**Response (200):**
```json
[
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "riotId": "GameName#TAG",
    "region": "euw1",
    "lastSync": "2024-01-01T10:30:00Z",
    "hasPuuid": true,
    "isAdmin": false,
    "isActive": true,
    "createdAt": "2023-12-01T08:00:00Z"
  }
]
```

### GET `/api/admin/users/{userId}`
Obtiene un usuario específico por ID.

**Headers:** `Authorization: Bearer {token}` (Admin)

**Response (200):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "riotId": "GameName#TAG",
  "region": "euw1",
  "lastSync": "2024-01-01T10:30:00Z",
  "hasPuuid": true,
  "isAdmin": false,
  "isActive": true,
  "createdAt": "2023-12-01T08:00:00Z"
}
```

### DELETE `/api/admin/users/{userId}`
Elimina un usuario por ID.

**Headers:** `Authorization: Bearer {token}` (Admin)

**Response:** `204 No Content`

### POST `/api/admin/sync`
Dispara sincronización para todos los usuarios o usuarios específicos.

**Headers:** `Authorization: Bearer {token}` (Admin)

**Request Body:**
```json
{
  "syncType": "FULL_SYNC",
  "forceRefresh": false,
  "userIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ]
}
```

**Response (200):**
```json
{
  "jobsCreated": 1,
  "message": "Created 1 sync jobs for specified users.",
  "jobs": [
    {
      "jobId": 12345,
      "userId": "abc123...",
      "jobType": "FULL_SYNC",
      "status": "PENDING",
      "createdAt": "2024-01-01T12:00:00Z"
    }
  ]
}
```

### GET `/api/admin/sync/status`
Obtiene estado agregado de todos los jobs de sincronización.

**Headers:** `Authorization: Bearer {token}` (Admin)

**Response (200):**
```json
{
  "pendingJobs": 5,
  "processingJobs": 2,
  "completedJobs": 150,
  "failedJobs": 3,
  "lastCompletedAt": "2024-01-01T11:45:00Z"
}
```

### GET `/api/admin/stats`
Obtiene estadísticas del sistema completo.

**Headers:** `Authorization: Bearer {token}` (Admin)

**Response (200):**
```json
{
  "totalUsers": 100,
  "activeUsers": 95,
  "totalMatches": 5000,
  "matchesToday": 250,
  "pendingSyncJobs": 5,
  "failedSyncJobs": 3,
  "lastSyncTime": "2024-01-01T11:45:00Z"
}
```

---

## 📝 Validación

Todos los endpoints validan los datos de entrada usando **FluentValidation**. En caso de error, devuelven:

**Response (400):**
```json
{
  "error": "Validation failed",
  "errors": [
    "Page must be greater than 0",
    "Invalid Riot ID format. Must be GameName#TAG"
  ]
}
```

---

## 🔒 Autenticación y Autorización

- **Autenticación:** JWT Bearer Token
- **Roles:** `User`, `Admin`
- **Token válido por:** 1 día
- **Header requerido:** `Authorization: Bearer {token}`

### Emails Admin
Configurados en `appsettings.json` o User Secrets:

```json
{
  "Admin": {
    "AllowedEmails": [
      "admin@example.com"
    ]
  }
}
```

---

## 🧪 Testing

Ejecutar todos los tests:
```bash
dotnet test
```

Ejecutar con cobertura:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🚀 Iniciar la API

```bash
dotnet run --project src/DraftGapBackend.Api
```

La API estará disponible en:
- Swagger UI: `http://localhost:5057`
- Health Check: `http://localhost:5057/health`

---

## 📦 Estructura de Capas

```
DraftGapBackend.Api/          # Controllers, Middleware
├── Controllers/
│   ├── AuthController.cs
│   ├── ProfileController.cs
│   ├── DashboardController.cs
│   ├── MatchesController.cs
│   ├── ChampionsController.cs
│   ├── RankedController.cs
│   ├── FriendsController.cs
│   ├── SyncController.cs
│   └── AdminController.cs
└── Middleware/
    └── GlobalExceptionHandler.cs

DraftGapBackend.Application/   # DTOs, Interfaces, Validators
├── Common/
│   ├── PaginationDto.cs
│   ├── ApiResponse.cs
│   └── CommonValidators.cs
├── Profile/
│   ├── ProfileDto.cs
│   └── ProfileValidators.cs
├── Dashboard/
│   └── DashboardDto.cs
├── Matches/
│   ├── MatchDto.cs
│   └── MatchValidators.cs
├── Champions/
│   └── ChampionDto.cs
├── Ranked/
│   └── RankedDto.cs
├── Friends/
│   ├── FriendsDto.cs
│   └── FriendsValidators.cs
├── Sync/
│   └── SyncDto.cs
├── Admin/
│   └── AdminDto.cs
└── Interfaces/
    ├── IProfileService.cs
    ├── IDashboardService.cs
    ├── IMatchService.cs
    ├── IChampionService.cs
    ├── IRankedService.cs
    ├── IFriendsService.cs
    └── IUserSyncService.cs

DraftGapBackend.Domain/        # Entities, Repository Interfaces
├── Entities/
│   ├── User.cs
│   ├── Player.cs
│   ├── Match.cs
│   ├── MatchParticipant.cs
│   ├── Champion.cs
│   ├── PlayerRankedStat.cs
│   └── SyncJob.cs
└── Abstractions/
    ├── IUserRepository.cs
    ├── IMatchRepository.cs
    ├── IChampionRepository.cs
    ├── IRankedRepository.cs
    └── IPlayerRepository.cs

DraftGapBackend.Infrastructure/ # Implementations
├── Services/
│   ├── ProfileService.cs
│   ├── DashboardService.cs
│   ├── MatchService.cs
│   ├── ChampionService.cs
│   ├── RankedService.cs
│   ├── FriendsService.cs
│   └── UserSyncService.cs
└── Persistence/
    ├── UserRepository.cs
    ├── MatchRepository.cs
    ├── ChampionRepository.cs
    ├── RankedRepository.cs
    └── PlayerRepository.cs

DraftGapBackend.Tests/         # Unit Tests
├── Controllers/
│   └── AuthControllerTests.cs
├── Services/
│   ├── DashboardServiceTests.cs
│   ├── MatchServiceTests.cs
│   └── AdminServiceTests.cs
└── Validators/
    └── ValidationTests.cs
```

---

## ✅ Características Implementadas

- ✅ DTOs request/response tipados para todos los endpoints
- ✅ Interfaces en Application y repositorios en Domain/Infrastructure
- ✅ Controladores con rutas `/api/*` y `CancellationToken`
- ✅ JWT authentication + `[Authorize]`
- ✅ Política de rol Admin para `/api/admin/*`
- ✅ Validación con FluentValidation
- ✅ Respuestas 400 consistentes con lista de errores
- ✅ Paginación estándar (page, pageSize, total)
- ✅ Manejo global de errores con ProblemDetails
- ✅ Swagger con JWT Bearer support
- ✅ Tests básicos de controladores/servicios
- ✅ CancellationToken en todos los endpoints
- ✅ Arquitectura por capas limpia (Api/Application/Domain/Infrastructure)

---

## 🔧 Configuración Requerida

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=draftgap;user=root;password=yourpassword"
  },
  "Jwt": {
    "SecretKey": "YourVeryLongSecretKeyHere123456",
    "Issuer": "DraftGapAPI",
    "Audience": "DraftGapClient"
  },
  "RiotApi": {
    "ApiKey": "RGAPI-your-key-here"
  },
  "Admin": {
    "AllowedEmails": [
      "admin@example.com"
    ]
  }
}
```

---

## 📚 Recursos

- [Riot Games API Documentation](https://developer.riotgames.com/)
- [Data Dragon CDN](https://ddragon.leagueoflegends.com/)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
