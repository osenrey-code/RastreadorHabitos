namespace RastreadorHabitos.Api;

public sealed class ServicioMetas(AlmacenJson almacen, ServicioAcceso acceso)
{
    public const string PermisoEliminar = "habitos.eliminar";

    public IReadOnlyList<MetaPublica> ListarPropias(UsuarioPublico actor) => almacen.Leer(estado =>
    {
        ValidarActor(estado, actor);
        return estado.Metas
            .Where(meta => meta.UsuarioId == actor.Id)
            .OrderBy(meta => meta.Nombre)
            .Select(Publica)
            .ToList();
    });

    public MetaPublica Crear(UsuarioPublico actor, string? nombre)
    {
        var nombreNormalizado = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombreNormalizado) || nombreNormalizado.Length > 100)
            throw new ErrorDeDominio(400, "Indique un nombre de meta de 1 a 100 caracteres.");

        return almacen.Cambiar(estado =>
        {
            ValidarActor(estado, actor);
            var meta = new Meta
            {
                UsuarioId = actor.Id,
                Nombre = nombreNormalizado,
                Estado = EstadoMeta.Pendiente
            };
            estado.Metas.Add(meta);
            return Publica(meta);
        });
    }

    public MetaPublica Activar(UsuarioPublico actor, Guid metaId) =>
        CambiarEstado(actor, metaId, EstadoMeta.Activa);

    public MetaPublica Completar(UsuarioPublico actor, Guid metaId) =>
        CambiarEstado(actor, metaId, EstadoMeta.Completada);

    public MetaPublica Cancelar(UsuarioPublico actor, Guid metaId)
    {
        return almacen.Cambiar(estado =>
        {
            var meta = BuscarPropia(estado, actor, metaId);
            if (meta.Estado is not (EstadoMeta.Pendiente or EstadoMeta.Activa))
                throw new ErrorDeDominio(409, $"No se puede cancelar una meta en estado {meta.Estado}.");
            TransicionesMeta.CambiarEstado(meta, EstadoMeta.Cancelada);
            return Publica(meta);
        });
    }

    public MetaPublica Eliminar(UsuarioPublico actor, Guid metaId)
    {
        _ = almacen.Leer(estado => BuscarPropia(estado, actor, metaId));
        if (!acceso.TienePermiso(actor.Id, PermisoEliminar))
            throw new ErrorDeDominio(403, "Se requiere el permiso habitos.eliminar.");

        return almacen.Cambiar(estado =>
        {
            var meta = BuscarPropia(estado, actor, metaId);
            estado.Metas.Remove(meta);
            return Publica(meta);
        });
    }

    private MetaPublica CambiarEstado(UsuarioPublico actor, Guid metaId, EstadoMeta destino) =>
        almacen.Cambiar(estado =>
        {
            var meta = BuscarPropia(estado, actor, metaId);
            TransicionesMeta.CambiarEstado(meta, destino);
            return Publica(meta);
        });

    private static Meta BuscarPropia(EstadoAplicacion estado, UsuarioPublico actor, Guid metaId)
    {
        ValidarActor(estado, actor);
        var meta = estado.Metas.FirstOrDefault(item => item.Id == metaId)
            ?? throw new ErrorDeDominio(404, "Meta no encontrada.");
        if (meta.UsuarioId != actor.Id)
            throw new ErrorDeDominio(403, "No puede modificar metas de otro usuario.");
        return meta;
    }

    private static void ValidarActor(EstadoAplicacion estado, UsuarioPublico actor)
    {
        var usuario = estado.Usuarios.FirstOrDefault(item => item.Id == actor.Id);
        if (usuario is null || !usuario.Activo)
            throw new ErrorDeDominio(401, "Sesion no valida.");
        if (usuario.Rol != Rol.Estandar)
            throw new ErrorDeDominio(403, "La operacion requiere un usuario Estandar.");
    }

    private static MetaPublica Publica(Meta meta) =>
        new(meta.Id, meta.UsuarioId, meta.Nombre, meta.Estado);
}
