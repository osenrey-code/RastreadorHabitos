namespace RastreadorHabitos.Api;

public sealed partial class ServicioAcceso
{
    public void CrearAdministradorInicial(string? correo, string? contrasena)
    {
        if (string.IsNullOrWhiteSpace(correo) && string.IsNullOrWhiteSpace(contrasena)) return;
        ValidarCorreo(correo);
        ValidarContrasena(contrasena);
        almacen.Cambiar(estado =>
        {
            if (estado.Usuarios.Any(u => u.Rol == Rol.Administrador)) return false;
            if (estado.Usuarios.Any(u => u.Correo == NormalizarCorreo(correo!)))
                throw new InvalidOperationException("El correo inicial de Administrador ya esta en uso.");
            estado.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador",
                Correo = NormalizarCorreo(correo!),
                HashContrasena = CrearHashContrasena(contrasena!),
                Rol = Rol.Administrador,
                CorreoConfirmado = true,
                Activo = true
            });
            return true;
        });
    }

    public IReadOnlyList<UsuarioPublico> ListarUsuarios() =>
        almacen.Leer(estado => estado.Usuarios.Select(Publico).ToList());

    public UsuarioPublico CambiarRol(Guid usuarioId, Rol rol) => almacen.Cambiar(estado =>
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        if (usuario.Rol == Rol.Administrador && rol != Rol.Administrador &&
            estado.Usuarios.Count(u => u.Rol == Rol.Administrador && u.Activo) <= 1)
            throw new ErrorDeDominio(400, "Debe quedar al menos un administrador activo.");
        usuario.Rol = rol;
        estado.Sesiones.RemoveAll(s => s.UsuarioId == usuario.Id);
        return Publico(usuario);
    });

    public UsuarioPublico CambiarActivo(Guid administradorId, Guid usuarioId, bool activo) => almacen.Cambiar(estado =>
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        if (!activo && usuario.Id == administradorId)
            throw new ErrorDeDominio(400, "Un administrador no puede desactivarse a si mismo.");
        if (activo && !usuario.CorreoConfirmado)
            throw new ErrorDeDominio(400, "El usuario debe confirmar su correo primero.");
        usuario.Activo = activo;
        if (!activo) estado.Sesiones.RemoveAll(s => s.UsuarioId == usuarioId);
        return Publico(usuario);
    });

    public void ForzarRestablecimiento(Guid usuarioId) => almacen.Cambiar(estado =>
    {
        var usuario = BuscarUsuario(estado, usuarioId);
        usuario.HashContrasena = CrearHashContrasena(CrearToken());
        estado.Sesiones.RemoveAll(s => s.UsuarioId == usuario.Id);
        EncolarRecuperacion(estado, usuario);
        return true;
    });
}
