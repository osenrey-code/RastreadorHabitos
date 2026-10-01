namespace RastreadorHabitos.Api;

public sealed partial class ServicioAcceso
{
    public UsuarioPublico Registrar(RegistroPeticion peticion)
    {
        ValidarCorreo(peticion.Correo);
        ValidarContrasena(peticion.Contrasena);
        var nombre = peticion.Nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
            throw new ErrorDeDominio(400, "Indique un nombre de 1 a 100 caracteres.");
        return almacen.Cambiar(estado =>
        {
            var correo = NormalizarCorreo(peticion.Correo!);
            if (estado.Usuarios.Any(u => u.Correo == correo))
                throw new ErrorDeDominio(409, "El correo ya esta registrado.");
            var usuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                HashContrasena = CrearHashContrasena(peticion.Contrasena!),
                Activo = false
            };
            estado.Usuarios.Add(usuario);
            EncolarActivacion(estado, usuario);
            return Publico(usuario);
        });
    }

    public string ReenviarActivacion(CorreoPeticion peticion)
    {
        ValidarCorreo(peticion.Correo);
        almacen.Cambiar(estado =>
        {
            var usuario = estado.Usuarios.FirstOrDefault(u => u.Correo == NormalizarCorreo(peticion.Correo!));
            if (usuario is { CorreoConfirmado: false }) EncolarActivacion(estado, usuario);
            return true;
        });
        return RespuestaNeutra;
    }

    public void Activar(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ErrorDeDominio(400, "El enlace de activacion no es valido.");
        var hash = HashToken(token);
        almacen.Cambiar(estado =>
        {
            var usuario = estado.Usuarios.FirstOrDefault(u => u.HashActivacion == hash);
            if (usuario is null || usuario.CorreoConfirmado || usuario.ActivacionVence <= Ahora)
                throw new ErrorDeDominio(400, "El enlace de activacion no es valido o ha vencido.");
            usuario.CorreoConfirmado = true;
            usuario.Activo = true;
            usuario.HashActivacion = null;
            usuario.ActivacionVence = null;
            return true;
        });
    }

    private void EncolarActivacion(EstadoAplicacion estado, Usuario usuario)
    {
        estado.Correos.RemoveAll(c => c.Destinatario == usuario.Correo &&
            c.Asunto == "Activa tu cuenta" && c.Estado == "Pendiente");
        var token = CrearToken();
        usuario.HashActivacion = HashToken(token);
        usuario.ActivacionVence = Ahora.AddHours(24);
        var enlace = $"{urlBase.TrimEnd('/')}/api/auth/activate?token={Uri.EscapeDataString(token)}";
        estado.Correos.Add(new CorreoEnCola
        {
            Destinatario = usuario.Correo,
            Asunto = "Activa tu cuenta",
            Cuerpo = $"Abre este enlace para activar tu cuenta (vence en 24 horas): {enlace}"
        });
    }
}
