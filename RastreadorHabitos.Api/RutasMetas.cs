namespace RastreadorHabitos.Api;

public static class RutasMetas
{
    public static void Mapear(WebApplication app)
    {
        var metas = app.MapGroup("/api/metas").RequireRol(Rol.Estandar);

        metas.MapGet("/", (HttpContext contexto, ServicioAcceso acceso, ServicioMetas servicio) =>
        {
            var actor = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(servicio.ListarPropias(actor));
        });

        metas.MapPost("/", (CrearMetaPeticion peticion, HttpContext contexto,
            ServicioAcceso acceso, ServicioMetas servicio) =>
        {
            var actor = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            var meta = servicio.Crear(actor, peticion.Nombre);
            return Results.Created($"/api/metas/{meta.Id}", meta);
        });

        metas.MapPut("/{id:guid}/activar", (Guid id, HttpContext contexto,
            ServicioAcceso acceso, ServicioMetas servicio) =>
        {
            var actor = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(servicio.Activar(actor, id));
        });

        metas.MapPut("/{id:guid}/completar", (Guid id, HttpContext contexto,
            ServicioAcceso acceso, ServicioMetas servicio) =>
        {
            var actor = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(servicio.Completar(actor, id));
        });

        metas.MapPut("/{id:guid}/cancelar", (Guid id, HttpContext contexto,
            ServicioAcceso acceso, ServicioMetas servicio) =>
        {
            var actor = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(servicio.Cancelar(actor, id));
        });

        metas.MapDelete("/{id:guid}", (Guid id, HttpContext contexto,
            ServicioAcceso acceso, ServicioMetas servicio) =>
        {
            var actor = acceso.Autenticar(PoliticasDeAcceso.Token(contexto));
            return Results.Ok(servicio.Eliminar(actor, id));
        });
    }
}
