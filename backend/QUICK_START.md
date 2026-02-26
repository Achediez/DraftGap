# 🚀 Guía Rápida de Comandos - DraftGap Backend

> **📢 Nota:** La estructura de DTOs y Validadores ha sido reorganizada. Ver [REORGANIZATION.md](REORGANIZATION.md) para más detalles.

## 🏗️ Build & Ejecución

### Compilar la solución
```bash
dotnet build
```

### Limpiar y recompilar
```bash
dotnet clean
dotnet build
```

### Ejecutar la API
```bash
dotnet run --project src/DraftGapBackend.Api
```

### Ejecutar con watch (auto-reload)
```bash
dotnet watch --project src/DraftGapBackend.Api
```

---

## 🧪 Testing

### Ejecutar todos los tests
```bash
dotnet test
```

### Ejecutar tests con verbosidad detallada
```bash
dotnet test --verbosity normal
```

### Ejecutar tests sin rebuilding
```bash
dotnet test --no-build
```

### Ver cobertura de tests
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Ejecutar un test específico
```bash
dotnet test --filter "FullyQualifiedName~DashboardServiceTests"
```

---

## 📦 Gestión de Paquetes

### Restaurar dependencias
```bash
dotnet restore
```

### Agregar paquete NuGet
```bash
dotnet add src/DraftGapBackend.Application package PackageName
```

### Actualizar todos los paquetes
```bash
dotnet list package --outdated
dotnet add package PackageName --version x.x.x
```

---

## 🗄️ Base de Datos (Entity Framework)

### Crear nueva migración
```bash
dotnet ef migrations add MigrationName --project src/DraftGapBackend.Infrastructure --startup-project src/DraftGapBackend.Api
```

### Aplicar migraciones
```bash
dotnet ef database update --project src/DraftGapBackend.Infrastructure --startup-project src/DraftGapBackend.Api
```

### Ver migraciones pendientes
```bash
dotnet ef migrations list --project src/DraftGapBackend.Infrastructure --startup-project src/DraftGapBackend.Api
```

### Revertir última migración
```bash
dotnet ef database update PreviousMigrationName --project src/DraftGapBackend.Infrastructure --startup-project src/DraftGapBackend.Api
```

### Eliminar última migración
```bash
dotnet ef migrations remove --project src/DraftGapBackend.Infrastructure --startup-project src/DraftGapBackend.Api
```

### Generar script SQL de migración
```bash
dotnet ef migrations script --project src/DraftGapBackend.Infrastructure --startup-project src/DraftGapBackend.Api --output migration.sql
```

---

## 🔍 Análisis de Código

### Listar warnings del build
```bash
dotnet build /warnaserror
```

### Ver paquetes instalados
```bash
dotnet list package
```

### Ver referencias de proyectos
```bash
dotnet list reference
```

---

## 🐳 Docker (si aplica)

### Build de imagen Docker
```bash
docker build -t draftgap-api .
```

### Ejecutar contenedor
```bash
docker run -p 5057:8080 -e ConnectionStrings__DefaultConnection="server=host.docker.internal;database=draftgap;user=root;password=yourpassword" draftgap-api
```

---

## 🔧 Configuración

### User Secrets (desarrollo)
```bash
# Ver User Secrets ID
dotnet user-secrets list --project src/DraftGapBackend.Api

# Agregar secret
dotnet user-secrets set "Jwt:SecretKey" "YourSecretKey" --project src/DraftGapBackend.Api
dotnet user-secrets set "RiotApi:ApiKey" "RGAPI-your-key" --project src/DraftGapBackend.Api

# Ver todos los secrets
dotnet user-secrets list --project src/DraftGapBackend.Api

# Limpiar todos los secrets
dotnet user-secrets clear --project src/DraftGapBackend.Api
```

---

## 🌐 Endpoints de Prueba (con curl)

### Health Check
```bash
curl http://localhost:5057/health
```

### Login
```bash
curl -X POST http://localhost:5057/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

### Obtener Dashboard (con token)
```bash
curl -X GET http://localhost:5057/api/dashboard/summary \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Matches paginados
```bash
curl -X GET "http://localhost:5057/api/matches?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Stats de Champions
```bash
curl -X GET http://localhost:5057/api/champions/stats \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 📊 URLs Importantes

| Recurso | URL |
|---------|-----|
| Swagger UI | http://localhost:5057 |
| Health Check | http://localhost:5057/health |
| API Base | http://localhost:5057/api |

---

## 🐛 Debugging

### Ver logs del build
```bash
dotnet build /v:detailed
```

### Ver logs de la app en tiempo real
```bash
dotnet run --project src/DraftGapBackend.Api | Select-String "ERROR"
```

### Verificar configuración
```bash
dotnet run --project src/DraftGapBackend.Api --environment Development
```

---

## 📚 Estructura de Archivos Creados

```
backend/
├── src/
│   ├── DraftGapBackend.Api/
│   │   ├── Controllers/
│   │   │   ├── ProfileController.cs         ✅ NEW
│   │   │   ├── DashboardController.cs       ✅ NEW
│   │   │   ├── MatchesController.cs         ✅ NEW
│   │   │   ├── ChampionsController.cs       ✅ NEW
│   │   │   ├── RankedController.cs          ✅ NEW
│   │   │   ├── FriendsController.cs         ✅ NEW
│   │   │   ├── SyncController.cs            ✅ NEW
│   │   │   └── AdminController.cs           ✅ UPDATED
│   │   ├── Middleware/
│   │   │   └── GlobalExceptionHandler.cs    ✅ NEW
│   │   └── Program.cs                       ✅ UPDATED
│   │
│   ├── DraftGapBackend.Application/
│   │   ├── Dtos/                            ✅ NEW (reorganizado)
│   │   │   ├── Common/
│   │   │   │   ├── PaginationDto.cs         ✅ DTOs comunes
│   │   │   │   └── ApiResponse.cs
│   │   │   ├── Profile/
│   │   │   │   └── ProfileDto.cs            ✅ DTOs de perfil
│   │   │   ├── Dashboard/
│   │   │   │   └── DashboardDto.cs          ✅ DTOs de dashboard
│   │   │   ├── Matches/
│   │   │   │   └── MatchDto.cs              ✅ DTOs de partidas
│   │   │   ├── Champions/
│   │   │   │   └── ChampionDto.cs           ✅ DTOs de campeones
│   │   │   ├── Ranked/
│   │   │   │   └── RankedDto.cs             ✅ DTOs de ranked
│   │   │   ├── Friends/
│   │   │   │   └── FriendsDto.cs            ✅ DTOs de amigos
│   │   │   ├── Sync/
│   │   │   │   └── SyncDto.cs               ✅ DTOs de sync
│   │   │   └── Admin/
│   │   │       └── AdminDto.cs              ✅ DTOs de admin
│   │   ├── Validators/                      ✅ NEW (centralizados)
│   │   │   ├── CommonValidators.cs          ✅ Validadores comunes
│   │   │   ├── ProfileValidators.cs         ✅ Validadores de perfil
│   │   │   ├── MatchValidators.cs           ✅ Validadores de matches
│   │   │   └── FriendsValidators.cs         ✅ Validadores de friends
│   │   └── Interfaces/
│   │       ├── IProfileService.cs           ✅ NEW
│   │       ├── IDashboardService.cs         ✅ NEW
│   │       ├── IMatchService.cs             ✅ NEW
│   │       ├── IChampionService.cs          ✅ NEW
│   │       ├── IRankedService.cs            ✅ NEW
│   │       ├── IFriendsService.cs           ✅ NEW
│   │       └── IUserSyncService.cs          ✅ NEW
│   │
│   ├── DraftGapBackend.Domain/
│   │   └── Abstractions/
│   │       ├── IMatchRepository.cs          ✅ NEW
│   │       ├── IChampionRepository.cs       ✅ NEW
│   │       ├── IRankedRepository.cs         ✅ NEW
│   │       └── IPlayerRepository.cs         ✅ NEW
│   │
│   └── DraftGapBackend.Infrastructure/
│       ├── Services/
│       │   ├── ProfileService.cs            ✅ NEW
│       │   ├── DashboardService.cs          ✅ NEW
│       │   ├── MatchService.cs              ✅ NEW
│       │   ├── ChampionService.cs           ✅ NEW
│       │   ├── RankedService.cs             ✅ NEW
│       │   ├── FriendsService.cs            ✅ NEW
│       │   └── UserSyncService.cs           ✅ NEW
│       └── Persistence/
│           ├── MatchRepository.cs           ✅ NEW
│           ├── ChampionRepository.cs        ✅ NEW
│           ├── RankedRepository.cs          ✅ NEW
│           └── PlayerRepository.cs          ✅ NEW
│
├── tests/
│   └── DraftGapBackend.Tests/
│       ├── Controllers/
│       │   └── AuthControllerTests.cs       ✅ NEW
│       ├── Services/
│       │   ├── DashboardServiceTests.cs     ✅ NEW
│       │   ├── MatchServiceTests.cs         ✅ NEW
│       │   └── AdminServiceTests.cs         ✅ NEW
│       └── Validators/
│           └── ValidationTests.cs           ✅ NEW
│
├── API_DOCUMENTATION.md                     ✅ NEW
└── IMPLEMENTATION_SUMMARY.md                ✅ NEW
```

---

## 🎯 Comandos de Verificación

### Verificar que todo compile
```bash
dotnet build
```

### Verificar que todos los tests pasen
```bash
dotnet test
```

### Ver estructura de solución
```bash
dotnet sln list
```

### Ver dependencias de un proyecto
```bash
dotnet list src/DraftGapBackend.Api reference
```

---

## 💡 Tips de Desarrollo

1. **Hot Reload:** Usa `dotnet watch` para desarrollo
2. **Swagger:** Abre http://localhost:5057 para probar endpoints interactivamente
3. **Logs:** Busca logs en consola con colores para identificar rápido errores
4. **Tests:** Ejecuta `dotnet test --filter "FullyQualifiedName~NombreTest"` para test específico
5. **Coverage:** Usa `dotnet test --collect:"XPlat Code Coverage"` y visualiza con ReportGenerator

---

## 🔐 Variables de Entorno Requeridas

```bash
# User Secrets (desarrollo)
ConnectionStrings__DefaultConnection="server=localhost;database=draftgap;user=root;password=yourpass"
Jwt__SecretKey="YourVeryLongSecretKeyHere123456"
Jwt__Issuer="DraftGapAPI"
Jwt__Audience="DraftGapClient"
RiotApi__ApiKey="RGAPI-your-key-here"
Admin__AllowedEmails__0="admin@example.com"
```

Para configurar en User Secrets:
```bash
cd src/DraftGapBackend.Api
dotnet user-secrets set "Jwt:SecretKey" "YourVeryLongSecretKeyHere123456"
dotnet user-secrets set "RiotApi:ApiKey" "RGAPI-your-key-here"
```

---

## ✅ Checklist Pre-Deploy

- [ ] Todos los tests pasan (`dotnet test`)
- [ ] Build exitoso sin warnings (`dotnet build`)
- [ ] Configuración de producción en `appsettings.Production.json`
- [ ] Connection strings configurados
- [ ] JWT SecretKey seguro (32+ caracteres)
- [ ] Riot API Key configurado
- [ ] CORS configurado para origen específico (no AllowAll)
- [ ] HTTPS habilitado
- [ ] Logging configurado
- [ ] Health check funciona

---

## 📖 Recursos Adicionales

- [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - Documentación completa de endpoints
- [IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md) - Resumen técnico
- Swagger UI: http://localhost:5057
- Health Check: http://localhost:5057/health
