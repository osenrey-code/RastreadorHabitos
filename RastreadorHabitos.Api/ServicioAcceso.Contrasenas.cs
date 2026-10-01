namespace RastreadorHabitos.Api;

public sealed partial class ServicioAcceso
{
    public string SolicitarRecuperacion(CorreoPeticion peticion)
    {
        ValidarCorreo(peticion.Correo);
        almacen.Cambiar(estado =>
        {
            var usuario = estado.Usuarios.FirstOrDefault(u => u.Correo == NormalizarCorreo(peticion.Correo!));
            if (usuario is not null) EncolarRecuperacion(estado, usuario);
            return true;
        });
        return RespuestaNeutra;
    }

    public void Restablecer(RecuperarPeticion peticion)
    {
        ValidarContrasena(peticion.ContrasenaNueva);
        if (string.IsNullOrWhiteSpace(peticion.Correo) || string.IsNullOrWhiteSpace(peticion.Codigo))
            throw new ErrorDeDominio(400, "Codigo no valido o vencido.");
        almacen.Cambiar(estado =>
        {
            var usuario = estado.Usuarios.FirstOrDefault(u => u.Correo == NormalizarCorreo(peticion.Correo));
            var codigo = estado.Codigos.FirstOrDefault(c => c.UsuarioId == usuario?.Id &&
                c.HashCodigo == HashToken(peticion.Codigo) && !c.Usado && c.Vence > Ahora);
            if (usuario is null || codigo is null)
                throw new ErrorDeDominio(400, "Codigo no valido o vencido.");
            usuario.HashContrasena = CrearHashContrasena(peticion.ContrasenaNueva!);
            foreach (var otro in estado.Codigos.Where(c => c.UsuarioId == usuario.Id)) otro.Usado = true;
            estado.Sesiones.RemoveAll(s => s.UsuarioId == usuario.Id);
            return true;
        });
    }

    public void CambiarContrasena(Guid usuarioId, CambiarPeticion peticion)
    {
        ValidarContrasena(peticion.ContrasenaNueva);
        almacen.Cambiar(estado =>
        {
            var usuario = BuscarUsuario(estado, usuarioId);
            if (!VerificarContrasena(peticion.ContrasenaActual ?? "", usuario.HashContrasena))
                throw new ErrorDeDominio(400, "La contrasena actual es incorrecta.");
            usuario.HashContrasena = CrearHashContrasena(peticion.ContrasenaNueva!);
            estado.Sesiones.RemoveAll(s => s.UsuarioId == usuarioId);
            return true;
        });
    }

    private void EncolarRecuperacion(EstadoAplicacion estado, Usuario usuario)
    {
        var codigo = CrearToken();
        estado.Codigos.Add(new CodigoRecuperacion
        {
            UsuarioId = usuario.Id,
            HashCodigo = HashToken(codigo),
            Vence = Ahora.AddMinutes(30)
        });
        estado.Correos.Add(new CorreoEnCola
        {
            Destinatario = usuario.Correo,
            Asunto = "Recupera tu contrasena",
            Cuerpo = $"Tu codigo de un solo uso vence en 30 minutos: {codigo}"
        });
    }
}
