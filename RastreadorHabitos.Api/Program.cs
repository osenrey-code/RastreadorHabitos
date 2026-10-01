using RastreadorHabitos.Api;
using System.Text.Json.Serialization;

var directorio = Path.GetFullPath(Environment.GetEnvironmentVariable("RASTREADOR_DATA_DIR") ??
    Path.Combine(Environment.CurrentDirectory, "data"));
var almacen = new AlmacenJson(directorio);
var servicio = new ServicioAcceso(almacen, TimeProvider.System,
    Environment.GetEnvironmentVariable("RASTREADOR_BASE_URL") ?? "http://localhost:5075");

if (args.Contains("send-mail", StringComparer.OrdinalIgnoreCase))
{
    try
    {
        var (enviados, pendientes) = await new ColaCorreo(almacen, directorio).ProcesarAsync();
        Console.WriteLine($"Enviados: {enviados}. Pendientes: {pendientes}.");
        return;
    }
    catch (Exception ex) when (ex is InvalidOperationException or IOException or System.Net.Mail.SmtpException)
    {
        Console.Error.WriteLine("No se pudo procesar la cola. Revise la configuracion SMTP y la conexion.");
        Environment.ExitCode = 1;
        return;
    }
}

servicio.CrearAdministradorInicial(
    Environment.GetEnvironmentVariable("RASTREADOR_ADMIN_EMAIL"),
    Environment.GetEnvironmentVariable("RASTREADOR_ADMIN_PASSWORD"));

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(opciones =>
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton(servicio);
builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);
var app = builder.Build();

app.Use(async (context, siguiente) =>
{
    try
    {
        await siguiente();
    }
    catch (ErrorDeDominio error)
    {
        context.Response.StatusCode = error.Codigo;
        await context.Response.WriteAsJsonAsync(new { error = error.Message });
    }
    catch (Exception error)
    {
        app.Logger.LogError(error, "Error interno");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Ocurrio un error interno." });
    }
});

app.MapGet("/", () => Results.Ok(new { nombre = "RastreadorHabitos", version = "practica-1" }));
RutasRegistro.Mapear(app);
RutasSesion.Mapear(app);
RutasContrasenas.Mapear(app);
RutasAdministracion.Mapear(app);

app.Run();
