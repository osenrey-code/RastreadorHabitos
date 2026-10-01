using System.Text.Json;
using System.Text.Json.Serialization;

namespace RastreadorHabitos.Api;

public sealed class AlmacenJson
{
    private readonly string _archivo;
    private readonly string _candado;
    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public AlmacenJson(string directorio)
    {
        Directory.CreateDirectory(directorio);
        _archivo = Path.Combine(directorio, "rastreador.json");
        _candado = Path.Combine(directorio, "rastreador.lock");
    }

    public T Leer<T>(Func<EstadoAplicacion, T> accion) => Ejecutar(accion, false);

    public T Cambiar<T>(Func<EstadoAplicacion, T> accion) => Ejecutar(accion, true);

    private T Ejecutar<T>(Func<EstadoAplicacion, T> accion, bool guardar)
    {
        using var bloqueo = AbrirCandado();
        var estado = File.Exists(_archivo)
            ? JsonSerializer.Deserialize<EstadoAplicacion>(File.ReadAllText(_archivo), Opciones) ?? new EstadoAplicacion()
            : new EstadoAplicacion();
        var resultado = accion(estado);
        if (guardar)
        {
            var temporal = Path.Combine(Path.GetDirectoryName(_archivo)!, $".{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporal, JsonSerializer.Serialize(estado, Opciones));
                File.Move(temporal, _archivo, true);
            }
            finally
            {
                if (File.Exists(temporal)) File.Delete(temporal);
            }
        }
        return resultado;
    }

    private FileStream AbrirCandado()
    {
        for (var intento = 0; intento < 100; intento++)
        {
            try
            {
                return new FileStream(_candado, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException) when (intento < 99)
            {
                Thread.Sleep(50);
            }
        }
        throw new IOException("No se pudo acceder al almacenamiento.");
    }
}
