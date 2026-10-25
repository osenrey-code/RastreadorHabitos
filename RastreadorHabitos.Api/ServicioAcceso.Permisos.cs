namespace RastreadorHabitos.Api;

public sealed partial class ServicioAcceso
{
    private static readonly HashSet<(EstadoSolicitudPermiso Desde, EstadoSolicitudPermiso Hasta)> TransicionesPermiso =
    [
        (EstadoSolicitudPermiso.Pendiente, EstadoSolicitudPermiso.Aprobada),
        (EstadoSolicitudPermiso.Aprobada, EstadoSolicitudPermiso.Aplicada),
        (EstadoSolicitudPermiso.Pendiente, EstadoSolicitudPermiso.Rechazada)
    ];

    public SolicitudPermisoPublica CrearSolicitudPermiso(Guid solicitanteId, string? permiso) => almacen.Cambiar(estado =>
    {
        var solicitante = BuscarUsuario(estado, solicitanteId);
        if (!solicitante.Activo)
            throw new ErrorDeDominio(403, "La cuenta no esta activa.");
        if (solicitante.Rol != Rol.Estandar)
            throw new ErrorDeDominio(403, "Solo un usuario Estandar puede solicitar permisos.");

        var permisoNormalizado = ValidarPermiso(permiso);
        var solicitud = new SolicitudPermiso
        {
            SolicitanteId = solicitante.Id,
            Permiso = permisoNormalizado,
            Creada = Ahora
        };
        estado.SolicitudesPermisos.Add(solicitud);
        return Publica(solicitud);
    });

    public IReadOnlyList<SolicitudPermisoPublica> ListarSolicitudesPropias(Guid usuarioId) => almacen.Leer(estado =>
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        if (!usuario.Activo)
            throw new ErrorDeDominio(403, "La cuenta no esta activa.");
        return estado.SolicitudesPermisos
            .Where(s => s.SolicitanteId == usuario.Id)
            .OrderByDescending(s => s.Creada)
            .Select(Publica)
            .ToList();
    });

    public SolicitudPermisoPublica ObtenerSolicitudPermiso(Guid usuarioId, Guid solicitudId) => almacen.Leer(estado =>
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        var solicitud = BuscarSolicitud(estado, solicitudId);
        if (!usuario.Activo)
            throw new ErrorDeDominio(403, "La cuenta no esta activa.");
        if (usuario.Rol != Rol.Administrador && solicitud.SolicitanteId == usuario.Id)
            throw new ErrorDeDominio(403, "No puede consultar solicitudes de otros usuarios.");
        return Publica(solicitud);
    });

    public IReadOnlyList<SolicitudPermisoPublica> ListarSolicitudesPendientes(Guid administradorId) =>
        almacen.Leer(estado =>
        {
            ExigirAdministrador(estado, administradorId);
            return estado.SolicitudesPermisos
                .Where(s => s.Estado == EstadoSolicitudPermiso.Pendiente)
                .OrderBy(s => s.Creada)
                .Select(Publica)
                .ToList();
        });

    public SolicitudPermisoPublica AprobarSolicitudPermiso(Guid administradorId, Guid solicitudId) =>
        almacen.Cambiar(estado =>
        {
            var administrador = ExigirAdministrador(estado, administradorId);
            var solicitud = BuscarSolicitud(estado, solicitudId);
            ValidarResolutor(administrador, solicitud);

            CambiarEstado(solicitud, EstadoSolicitudPermiso.Aprobada);
            solicitud.ResueltaPor = administrador.Id;
            solicitud.Resuelta = Ahora;

            var solicitante = BuscarUsuario(estado, solicitud.SolicitanteId);
            if (!solicitante.Permisos.Contains(solicitud.Permiso, StringComparer.OrdinalIgnoreCase))
                solicitante.Permisos.Add(solicitud.Permiso);

            CambiarEstado(solicitud, EstadoSolicitudPermiso.Aplicada);
            return Publica(solicitud);
        });

    public SolicitudPermisoPublica RechazarSolicitudPermiso(
        Guid administradorId,
        Guid solicitudId,
        string? motivo) => almacen.Cambiar(estado =>
    {
        var administrador = ExigirAdministrador(estado, administradorId);
        var solicitud = BuscarSolicitud(estado, solicitudId);
        ValidarResolutor(administrador, solicitud);
        var motivoNormalizado = ValidarMotivo(motivo);

        CambiarEstado(solicitud, EstadoSolicitudPermiso.Rechazada);
        solicitud.ResueltaPor = administrador.Id;
        solicitud.Resuelta = Ahora;
        solicitud.MotivoRechazo = motivoNormalizado;
        return Publica(solicitud);
    });

    public bool TienePermiso(Guid usuarioId, string? permiso) => almacen.Leer(estado =>
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        var permisoNormalizado = ValidarPermiso(permiso);
        return usuario.Activo && usuario.Permisos.Contains(permisoNormalizado, StringComparer.OrdinalIgnoreCase);
    });

    private static Usuario ExigirAdministrador(EstadoAplicacion estado, Guid usuarioId)
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        if (!usuario.Activo || usuario.Rol != Rol.Administrador)
            throw new ErrorDeDominio(403, "Se requiere un Administrador activo.");
        return usuario;
    }

    private static SolicitudPermiso BuscarSolicitud(EstadoAplicacion estado, Guid solicitudId) =>
        estado.SolicitudesPermisos.FirstOrDefault(s => s.Id == solicitudId) ??
        throw new ErrorDeDominio(404, "Solicitud de permiso no encontrada.");

    private static string ValidarPermiso(string? permiso)
    {
        var valor = permiso?.Trim();
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > 100)
            throw new ErrorDeDominio(400, "Indique un permiso valido de hasta 100 caracteres.");
        return valor;
    }

    private static string ValidarMotivo(string? motivo)
    {
        var valor = motivo?.Trim();
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > 500)
            throw new ErrorDeDominio(400, "Indique un motivo de rechazo de hasta 500 caracteres.");
        return valor;
    }

    private static void ValidarResolutor(Usuario administrador, SolicitudPermiso solicitud)
    {
        if (administrador.Id == solicitud.SolicitanteId)
            throw new ErrorDeDominio(403, "Nadie puede resolver su propia solicitud.");
    }

    internal static void CambiarEstado(SolicitudPermiso solicitud, EstadoSolicitudPermiso nuevoEstado)
    {
        if (!TransicionesPermiso.Contains((solicitud.Estado, nuevoEstado)))
            throw new ErrorDeDominio(409,
                $"La transicion {solicitud.Estado} -> {nuevoEstado} no esta permitida.");
        solicitud.Estado = nuevoEstado;
    }

    private static SolicitudPermisoPublica Publica(SolicitudPermiso solicitud) => new(
        solicitud.Id,
        solicitud.SolicitanteId,
        solicitud.Permiso,
        solicitud.Estado,
        solicitud.Creada,
        solicitud.ResueltaPor,
        solicitud.Resuelta,
        solicitud.MotivoRechazo);
}
