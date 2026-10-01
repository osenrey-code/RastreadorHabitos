namespace RastreadorHabitos.Api;

public static class RutasContrasenas
{
    public static void Mapear(WebApplication app)
    {
        var publico = app.MapGroup("/api/auth");
        publico.MapPost("/request-reset", (CorreoPeticion peticion, ServicioAcceso acceso) =>
            Results.Ok(new { mensaje = acceso.SolicitarRecuperacion(peticion) }));
        publico.MapPost("/reset-password", (RecuperarPeticion peticion, ServicioAcceso acceso) =>
        {
            acceso.Restablecer(peticion);
            return Results.Ok(new { mensaje = "Contrasena restablecida." });
        });

        var privado = app.MapGroup("/api/me").RequireRol(Rol.Estandar);
        privado.MapPost("/change-password", (HttpContext contexto, CambiarPeticion peticion, ServicioAcceso acceso) =>
        {
            var usuario = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            acceso.CambiarContrasena(usuario.Id, peticion);
            return Results.Ok(new { mensaje = "Contrasena cambiada. Inicie sesion nuevamente." });
        });
    }
}
