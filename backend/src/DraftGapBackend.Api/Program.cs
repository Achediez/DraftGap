// Programa principal de la API DraftGapBackend
// Configura y arranca la aplicación web ASP.NET Core
//
// Características principales:
// - Configuración de servicios y dependencias
// - Integración de Swagger para documentación OpenAPI
// - Registro de controladores y endpoints
// - Configuración del pipeline HTTP
//
// .NET 9, C# 13

using DraftGapBackend.Infrastructure;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics; // Para abrir el navegador automáticamente

// Crea el builder para configurar la aplicación web
var builder = WebApplication.CreateBuilder(args);

// Registro de servicios en el contenedor de dependencias
// Controladores para la API REST
builder.Services.AddControllers();
// Descubrimiento de endpoints para Swagger
builder.Services.AddEndpointsApiExplorer();
// Configuración de Swagger/OpenAPI para documentación interactiva
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DraftGap API", Version = "v1" });
});
// Registro de servicios de infraestructura (repositorios, servicios de dominio, etc.)
// Pasamos la configuración para que los servicios puedan leer secrets.json
builder.Services.AddInfrastructure(builder.Configuration);

// Construye la aplicación
var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    // Habilita Swagger y su UI solo en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI();

    // Detecta la URL real de Swagger y la abre en el navegador predeterminado
    var serverAddresses = app.Urls.Count > 0 ? app.Urls : builder.WebHost.GetSetting("urls")?.Split(';') ?? new string[] { "https://localhost:5001" };
    var swaggerUrl = serverAddresses.First().TrimEnd('/') + "/swagger";
    try
    {
        Process.Start(new ProcessStartInfo { FileName = swaggerUrl, UseShellExecute = true });
    }
    catch { /* Ignorar errores si no se puede abrir el navegador */ }
}

// Redirección HTTPS obligatoria
app.UseHttpsRedirection();
// Middleware de autorización (puede requerir configuración adicional)
app.UseAuthorization();
// Mapeo de controladores a rutas
app.MapControllers();
// Inicia la aplicación
app.Run();
