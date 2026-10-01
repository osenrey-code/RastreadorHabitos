using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace RastreadorHabitos.Api;

public sealed partial class ServicioAcceso(AlmacenJson almacen, TimeProvider reloj, string urlBase)
{
    private const string RespuestaNeutra = "Si la cuenta existe, recibira un correo con las instrucciones.";
    private const string ErrorCredenciales = "Credenciales incorrectas.";

    private DateTimeOffset Ahora => reloj.GetUtcNow();
    private static Usuario BuscarUsuario(EstadoAplicacion estado, Guid id) =>
        estado.Usuarios.FirstOrDefault(u => u.Id == id) ?? throw new ErrorDeDominio(404, "Usuario no encontrado.");
    private static UsuarioPublico Publico(Usuario u) => new(u.Id, u.Nombre, u.Correo, u.Rol, u.Activo);
    private static string NormalizarCorreo(string correo) => correo.Trim().ToLowerInvariant();

    private static void ValidarCorreo(string? correo)
    {
        if (string.IsNullOrWhiteSpace(correo) || correo.Length > 254 ||
            !MailAddress.TryCreate(correo.Trim(), out var direccion) ||
            direccion.Address != correo.Trim())
            throw new ErrorDeDominio(400, "Indique un correo valido.");
    }

    private static void ValidarContrasena(string? contrasena)
    {
        if (string.IsNullOrEmpty(contrasena) || contrasena.Length < 8 || contrasena.Length > 256 ||
            !contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
            throw new ErrorDeDominio(400, "La contrasena debe tener al menos 8 caracteres, letras y numeros.");
    }

    private static string CrearHashContrasena(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, 210_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerificarContrasena(string contrasena, string almacenado)
    {
        var partes = almacenado.Split('.');
        if (partes.Length != 2) return false;
        var sal = Convert.FromBase64String(partes[0]);
        var esperado = Convert.FromBase64String(partes[1]);
        var recibido = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, 210_000, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(recibido, esperado);
    }

    private static string CrearToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
