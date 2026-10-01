namespace RastreadorHabitos.Api;

public static class RutasRegistro
{
    public static void Mapear(WebApplication app)
    {
        var grupo = app.MapGroup("/api/auth");
        grupo.MapPost("/register", (RegistroPeticion peticion, ServicioAcceso acceso) =>
            Results.Created("/api/auth/login", acceso.Registrar(peticion)));
        grupo.MapPost("/resend-activation", (CorreoPeticion peticion, ServicioAcceso acceso) =>
            Results.Ok(new { mensaje = acceso.ReenviarActivacion(peticion) }));
        grupo.MapGet("/activate", (string? token, ServicioAcceso acceso) =>
        {
            acceso.Activar(token);
            return Results.Ok(new { mensaje = "Cuenta activada. Ya puede iniciar sesion." });
        });
    }
}
