namespace RastreadorHabitos.Api;

public static class RutasPermisos
{
    public static void Mapear(WebApplication app)
    {
        var solicitudes = app.MapGroup("/api/permisos").RequireRol(Rol.Estandar);
        solicitudes.MapPost("/solicitudes", (
            CrearSolicitudPermisoPeticion peticion,
            HttpContext contexto,
            ServicioAcceso acceso) =>
        {
            var usuario = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            var solicitud = acceso.CrearSolicitudPermiso(usuario.Id, peticion.Permiso);
            return Results.Created($"/api/permisos/solicitudes/{solicitud.Id}", solicitud);
        });
        solicitudes.MapGet("/solicitudes", (HttpContext contexto, ServicioAcceso acceso) =>
        {
            var usuario = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(acceso.ListarSolicitudesPropias(usuario.Id));
        });
        solicitudes.MapGet("/solicitudes/{id:guid}", (Guid id, HttpContext contexto, ServicioAcceso acceso) =>
        {
            var usuario = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(acceso.ObtenerSolicitudPermiso(usuario.Id, id));
        });
        solicitudes.MapGet("/{permiso}/comprobar", (string permiso, HttpContext contexto, ServicioAcceso acceso) =>
        {
            var usuario = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(new { permiso, concedido = acceso.TienePermiso(usuario.Id, permiso) });
        });

        var administracion = app.MapGroup("/api/admin/permisos").RequireRol(Rol.Administrador);
        administracion.MapGet("/solicitudes/pendientes", (HttpContext contexto, ServicioAcceso acceso) =>
        {
            var administrador = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(acceso.ListarSolicitudesPendientes(administrador.Id));
        });
        administracion.MapPost("/solicitudes/{id:guid}/aprobar", (
            Guid id,
            HttpContext contexto,
            ServicioAcceso acceso) =>
        {
            var administrador = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(acceso.AprobarSolicitudPermiso(administrador.Id, id));
        });
        administracion.MapPost("/solicitudes/{id:guid}/rechazar", (
            Guid id,
            RechazarSolicitudPermisoPeticion peticion,
            HttpContext contexto,
            ServicioAcceso acceso) =>
        {
            var administrador = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(acceso.RechazarSolicitudPermiso(administrador.Id, id, peticion.Motivo));
        });
    }
}
