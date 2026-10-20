using System.Text.Json.Serialization;

namespace RastreadorHabitos.Api;

public enum Rol { Estandar, Administrador }

public enum EstadoSolicitudPermiso { Pendiente, Aprobada, Aplicada, Rechazada }

public sealed class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string HashContrasena { get; set; } = "";
    public Rol Rol { get; set; } = Rol.Estandar;
    public bool CorreoConfirmado { get; set; }
    public bool Activo { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTimeOffset? BloqueadoHasta { get; set; }
    public string? HashActivacion { get; set; }
    public DateTimeOffset? ActivacionVence { get; set; }
    public List<string> Permisos { get; set; } = [];
}

public sealed class SolicitudPermiso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SolicitanteId { get; set; }
    public string Permiso { get; set; } = "";
    public EstadoSolicitudPermiso Estado { get; set; } = EstadoSolicitudPermiso.Pendiente;
    public DateTimeOffset Creada { get; set; }
    public Guid? ResueltaPor { get; set; }
    public DateTimeOffset? Resuelta { get; set; }
    public string? MotivoRechazo { get; set; }
}

public sealed class Sesion
{
    public Guid UsuarioId { get; set; }
    public string HashToken { get; set; } = "";
    public DateTimeOffset Vence { get; set; }
}

public sealed class CodigoRecuperacion
{
    public Guid UsuarioId { get; set; }
    public string HashCodigo { get; set; } = "";
    public DateTimeOffset Vence { get; set; }
    public bool Usado { get; set; }
}

public sealed class CorreoEnCola
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Destinatario { get; set; } = "";
    public string Asunto { get; set; } = "";
    public string Cuerpo { get; set; } = "";
    public string Estado { get; set; } = "Pendiente";
    public DateTimeOffset Creado { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Enviado { get; set; }
}

public sealed class EstadoAplicacion
{
    public List<Usuario> Usuarios { get; set; } = [];
    public List<Sesion> Sesiones { get; set; } = [];
    public List<CodigoRecuperacion> Codigos { get; set; } = [];
    public List<CorreoEnCola> Correos { get; set; } = [];
    public List<Meta> Metas { get; set; } = [];
    public List<SolicitudPermiso> SolicitudesPermisos { get; set; } = [];
}

public sealed record UsuarioPublico(Guid Id, string Nombre, string Correo, Rol Rol, bool Activo);

public sealed record RegistroPeticion(string? Nombre, string? Correo, string? Contrasena);
public sealed record CorreoPeticion(string? Correo);
public sealed record InicioPeticion(string? Correo, string? Contrasena);
public sealed record RecuperarPeticion(string? Correo, string? Codigo, string? ContrasenaNueva);
public sealed record CambiarPeticion(string? ContrasenaActual, string? ContrasenaNueva);
public sealed record RolPeticion(string? Rol);
public sealed record CrearSolicitudPermisoPeticion(string? Permiso);
public sealed record RechazarSolicitudPermisoPeticion(string? Motivo);
public sealed record SolicitudPermisoPublica(
    Guid Id,
    Guid SolicitanteId,
    string Permiso,
    EstadoSolicitudPermiso Estado,
    DateTimeOffset Creada,
    Guid? ResueltaPor,
    DateTimeOffset? Resuelta,
    string? MotivoRechazo);

public sealed class ErrorDeDominio(int codigo, string mensaje) : Exception(mensaje)
{
    public int Codigo { get; } = codigo;
}
