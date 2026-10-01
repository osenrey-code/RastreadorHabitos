namespace RastreadorHabitos.Api;

public static class RutasSesion
{
    public static void Mapear(WebApplication app)
    {
        app.MapPost("/api/auth/login", (InicioPeticion peticion, ServicioAcceso acceso) =>
            Results.Ok(new { token = acceso.Iniciar(peticion) }));

        var grupo = app.MapGroup("/api/me").RequireRol(Rol.Estandar);
        grupo.MapGet("/", (HttpContext contexto, ServicioAcceso acceso) =>
            Results.Ok(acceso.Autenticar(PoliticasDeAcceso.Token(contexto))));
        grupo.MapPost("/logout", (HttpContext contexto, ServicioAcceso acceso) =>
        {
            acceso.Cerrar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(new { mensaje = "Sesion cerrada." });
        });
    }
}
