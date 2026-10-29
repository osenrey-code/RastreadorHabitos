namespace RastreadorHabitos.Api;

public enum EstadoMeta { Pendiente, Activa, Completada, Cancelada }

public sealed class Meta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = "";
    public EstadoMeta Estado { get; set; } = EstadoMeta.Pendiente;
}

public sealed record MetaPublica(Guid Id, Guid UsuarioId, string Nombre, EstadoMeta Estado);
public sealed record CrearMetaPeticion(string? Nombre);

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

    public static void CambiarEstado(Meta meta, EstadoMeta hacia)
    {
        if (!PuedeCambiar(meta.Estado, hacia))
            throw new ErrorDeDominio(409,
                $"La transicion {meta.Estado} -> {hacia} no esta permitida.");
        meta.Estado = hacia;
    }
}
