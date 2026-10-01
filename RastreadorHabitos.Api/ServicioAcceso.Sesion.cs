namespace RastreadorHabitos.Api;

public sealed partial class ServicioAcceso
{
    public string Iniciar(InicioPeticion peticion)
    {
        var correo = NormalizarCorreo(peticion.Correo ?? "");
        return almacen.Cambiar(estado =>
        {
            var usuario = estado.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario is null) throw new ErrorDeDominio(401, ErrorCredenciales);
            if (usuario.BloqueadoHasta > Ahora)
                throw new ErrorDeDominio(429, "Cuenta bloqueada temporalmente. Intente mas tarde.");
            if (usuario.BloqueadoHasta is not null)
            {
                usuario.BloqueadoHasta = null;
                usuario.IntentosFallidos = 0;
            }
            if (!VerificarContrasena(peticion.Contrasena ?? "", usuario.HashContrasena))
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= 5) usuario.BloqueadoHasta = Ahora.AddMinutes(15);
                return "";
            }
            if (!usuario.Activo) throw new ErrorDeDominio(403, "La cuenta no esta activa.");
            usuario.IntentosFallidos = 0;
            var token = CrearToken();
            estado.Sesiones.Add(new Sesion
            {
                UsuarioId = usuario.Id,
                HashToken = HashToken(token),
                Vence = Ahora.AddHours(12)
            });
            return token;
        }) switch
        {
            "" => throw new ErrorDeDominio(401, ErrorCredenciales),
            var token => token
        };
    }

    public UsuarioPublico Autenticar(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ErrorDeDominio(401, "Sesion no valida.");
        return almacen.Leer(estado =>
        {
            var sesion = estado.Sesiones.FirstOrDefault(s => s.HashToken == HashToken(token) && s.Vence > Ahora);
            var usuario = estado.Usuarios.FirstOrDefault(u => u.Id == sesion?.UsuarioId);
            if (usuario is null || !usuario.Activo) throw new ErrorDeDominio(401, "Sesion no valida.");
            return Publico(usuario);
        });
    }

    public void Cerrar(string token) => almacen.Cambiar(estado =>
    {
        estado.Sesiones.RemoveAll(s => s.HashToken == HashToken(token));
        return true;
    });
}
