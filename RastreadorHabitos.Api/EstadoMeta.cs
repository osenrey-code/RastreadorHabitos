namespace RastreadorHabitos.Api;

public enum EstadoMeta { Pendiente, Activa, Completada, Cancelada }

public sealed class Meta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public EstadoMeta Estado { get; set; } = EstadoMeta.Pendiente;
}

public static class TransicionesMeta
{
    private static readonly HashSet<(EstadoMeta Desde, EstadoMeta Hacia)> Permitidas =
    [
        (EstadoMeta.Pendiente, EstadoMeta.Activa),
        (EstadoMeta.Pendiente, EstadoMeta.Cancelada),
        (EstadoMeta.Activa, EstadoMeta.Completada),
        (EstadoMeta.Activa, EstadoMeta.Cancelada)
    ];

    public static readonly (EstadoMeta Desde, EstadoMeta Hacia) ProhibidaExplicita =
        (EstadoMeta.Pendiente, EstadoMeta.Completada);

    public static bool PuedeCambiar(EstadoMeta desde, EstadoMeta hacia) =>
        Permitidas.Contains((desde, hacia));
}
