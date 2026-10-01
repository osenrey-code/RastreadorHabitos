using System.Net;
using System.Net.Mail;

namespace RastreadorHabitos.Api;

public sealed class ColaCorreo(AlmacenJson almacen, string directorio)
{
    public async Task<(int Enviados, int Pendientes)> ProcesarAsync()
    {
        var host = Environment.GetEnvironmentVariable("RASTREADOR_SMTP_HOST");
        var puertoTexto = Environment.GetEnvironmentVariable("RASTREADOR_SMTP_PORT");
        var remitente = Environment.GetEnvironmentVariable("RASTREADOR_SMTP_FROM");
        var usuario = Environment.GetEnvironmentVariable("RASTREADOR_SMTP_USER");
        var contrasena = Environment.GetEnvironmentVariable("RASTREADOR_SMTP_PASSWORD");
        if (string.IsNullOrWhiteSpace(host) || !int.TryParse(puertoTexto, out var puerto) ||
            puerto is < 1 or > 65535 || !MailAddress.TryCreate(remitente, out _) ||
            string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
            throw new InvalidOperationException("Faltan las variables SMTP requeridas.");

        using var candado = new FileStream(Path.Combine(directorio, "mail-worker.lock"),
            FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        almacen.Cambiar(estado =>
        {
            foreach (var correo in estado.Correos.Where(c => c.Estado == "Procesando"))
                correo.Estado = "Pendiente";
            return true;
        });

        var enviados = 0;
        using var smtp = new SmtpClient(host, puerto)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(usuario, contrasena)
        };
        while (true)
        {
            var correo = almacen.Cambiar(estado =>
            {
                var primero = estado.Correos.FirstOrDefault(c => c.Estado == "Pendiente");
                if (primero is not null) primero.Estado = "Procesando";
                return primero is null ? null : new CorreoEnCola
                {
                    Id = primero.Id,
                    Destinatario = primero.Destinatario,
                    Asunto = primero.Asunto,
                    Cuerpo = primero.Cuerpo
                };
            });
            if (correo is null) break;
            try
            {
                using var mensaje = new MailMessage(remitente!, correo.Destinatario, correo.Asunto, correo.Cuerpo);
                await smtp.SendMailAsync(mensaje);
                almacen.Cambiar(estado =>
                {
                    var guardado = estado.Correos.Single(c => c.Id == correo.Id);
                    guardado.Estado = "Enviado";
                    guardado.Enviado = DateTimeOffset.UtcNow;
                    return true;
                });
                enviados++;
            }
            catch (SmtpException)
            {
                DevolverPendiente(correo.Id);
                break;
            }
            catch (IOException)
            {
                DevolverPendiente(correo.Id);
                break;
            }
        }
        var pendientes = almacen.Leer(estado => estado.Correos.Count(c => c.Estado == "Pendiente"));
        return (enviados, pendientes);
    }

    private void DevolverPendiente(Guid id) => almacen.Cambiar(estado =>
    {
        estado.Correos.Single(c => c.Id == id).Estado = "Pendiente";
        return true;
    });
}
