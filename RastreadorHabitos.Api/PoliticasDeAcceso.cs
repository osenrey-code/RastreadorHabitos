namespace RastreadorHabitos.Api;

internal static class PoliticasDeAcceso
{
    public static string Token(HttpContext contexto)
    {
        var encabezado = contexto.Request.Headers.Authorization.ToString();
        if (!encabezado.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            throw new ErrorDeDominio(401, "Sesion no valida.");
        return encabezado[7..].Trim();
    }

    // Todas las operaciones del grupo declaran aqui su rol minimo.
    public static RouteGroupBuilder RequireRol(this RouteGroupBuilder grupo, Rol minimo)
    {
        grupo.AddEndpointFilter(async (contexto, siguiente) =>
        {
            var solicitud = contexto.HttpContext;
            var acceso = solicitud.RequestServices.GetRequiredService<ServicioAcceso>();
            var usuario = acceso.Autenticar(Token(solicitud));
            if (minimo == Rol.Administrador && usuario.Rol != Rol.Administrador)
                throw new ErrorDeDominio(403, "No tiene permiso para esta operacion.");
            return await siguiente(contexto);
        });
        return grupo;
    }
}
