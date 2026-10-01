namespace RastreadorHabitos.Api;

public static class RutasAdministracion
{
    public static void Mapear(WebApplication app)
    {
        var grupo = app.MapGroup("/api/admin").RequireRol(Rol.Administrador);
        grupo.MapGet("/users", (ServicioAcceso acceso) => Results.Ok(acceso.ListarUsuarios()));
        grupo.MapPut("/users/{id:guid}/role", (Guid id, RolPeticion peticion, ServicioAcceso acceso) =>
        {
            if (!Enum.TryParse<Rol>(peticion.Rol, true, out var rol) || !Enum.IsDefined(rol))
                throw new ErrorDeDominio(400, "Rol no valido.");
            return Results.Ok(acceso.CambiarRol(id, rol));
        });
        grupo.MapPut("/users/{id:guid}/deactivate", (Guid id, HttpContext contexto, ServicioAcceso acceso) =>
            Results.Ok(acceso.CambiarActivo(acceso.Autenticar(PoliticasDeAcceso.Token(contexto)).Id, id, false)));
        grupo.MapPut("/users/{id:guid}/reactivate", (Guid id, HttpContext contexto, ServicioAcceso acceso) =>
            Results.Ok(acceso.CambiarActivo(acceso.Autenticar(PoliticasDeAcceso.Token(contexto)).Id, id, true)));
        grupo.MapPost("/users/{id:guid}/force-reset", (Guid id, ServicioAcceso acceso) =>
        {
            acceso.ForzarRestablecimiento(id);
            return Results.Ok(new { mensaje = "Restablecimiento solicitado por correo." });
        });
    }
}
