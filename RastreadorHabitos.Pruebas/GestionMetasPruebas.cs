using Microsoft.VisualStudio.TestTools.UnitTesting;
using RastreadorHabitos.Api;

namespace RastreadorHabitos.Pruebas;

[TestClass]
public sealed class GestionMetasPruebas
{
    private string _directorio = null!;
    private AlmacenJson _almacen = null!;
    private ServicioAcceso _acceso = null!;
    private ServicioMetas _metas = null!;
    private UsuarioPublico _ana = null!;
    private UsuarioPublico _luis = null!;

    [TestInitialize]
    public void PrepararPrueba()
    {
        _directorio = Path.Combine(Path.GetTempPath(), "metas-prueba-" + Guid.NewGuid().ToString("N"));
        _almacen = new AlmacenJson(_directorio);
        CrearUsuario("Admin", "admin@example.test", Rol.Administrador);
        var ana = CrearUsuario("Ana", "ana@example.test", Rol.Estandar);
        var luis = CrearUsuario("Luis", "luis@example.test", Rol.Estandar);
        _ana = Publico(ana);
        _luis = Publico(luis);
        _acceso = new ServicioAcceso(_almacen, new RelojDePrueba(), "http://localhost:5075");
        _metas = new ServicioMetas(_almacen, _acceso);
    }

    [TestCleanup]
    public void LimpiarPrueba()
    {
        if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
    }

    [TestMethod]
    public void RF_NEG_02_CrearMeta_UsuarioEstandar_LaDejaPendiente()
    {
        // Preparar
        const string nombre = "Leer veinte minutos";

        // Actuar
        var meta = _metas.Crear(_ana, nombre);

        // Verificar
        Assert.AreEqual(nombre, meta.Nombre);
        Assert.AreEqual(EstadoMeta.Pendiente, meta.Estado);
    }

    [TestMethod]
    public void RD_07_CrearMeta_SinNombre_LaRechazaSinGuardar()
    {
        // Preparar
        const string nombre = "   ";

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() => _metas.Crear(_ana, nombre));

        // Verificar
        Assert.AreEqual(400, error.Codigo);
        Assert.IsEmpty(_metas.ListarPropias(_ana));
    }

    [TestMethod]
    public void RF_NEG_02_ListarMetas_UsuarioEstandar_SoloVeLasPropias()
    {
        // Preparar
        var metaPropia = _metas.Crear(_ana, "Meta de Ana");
        _metas.Crear(_luis, "Meta de Luis");

        // Actuar
        var metas = _metas.ListarPropias(_ana);

        // Verificar
        CollectionAssert.AreEqual(new[] { metaPropia.Id }, metas.Select(meta => meta.Id).ToArray());
    }

    [TestMethod]
    public void RF_NEG_03_PendienteAActiva_UsuarioPropietario_LaPermite()
    {
        // Preparar
        var meta = _metas.Crear(_ana, "Meta para activar");

        // Actuar
        var resultado = _metas.Activar(_ana, meta.Id);

        // Verificar
        Assert.AreEqual(EstadoMeta.Activa, resultado.Estado);
    }

    [TestMethod]
    public void RF_NEG_03_PendienteACancelada_UsuarioPropietario_LaPermite()
    {
        // Preparar
        var meta = _metas.Crear(_ana, "Meta para cancelar");

        // Actuar
        var resultado = _metas.Cancelar(_ana, meta.Id);

        // Verificar
        Assert.AreEqual(EstadoMeta.Cancelada, resultado.Estado);
    }

    [TestMethod]
    public void RF_NEG_03_ActivaACompletada_UsuarioPropietario_LaPermite()
    {
        // Preparar
        var meta = _metas.Crear(_ana, "Meta para completar");
        _metas.Activar(_ana, meta.Id);

        // Actuar
        var resultado = _metas.Completar(_ana, meta.Id);

        // Verificar
        Assert.AreEqual(EstadoMeta.Completada, resultado.Estado);
    }

    [TestMethod]
    public void RF_NEG_03_ActivaACancelada_UsuarioPropietario_LaPermite()
    {
        // Preparar
        var meta = _metas.Crear(_ana, "Meta activa para cancelar");
        _metas.Activar(_ana, meta.Id);

        // Actuar
        var resultado = _metas.Cancelar(_ana, meta.Id);

        // Verificar
        Assert.AreEqual(EstadoMeta.Cancelada, resultado.Estado);
    }

    [TestMethod]
    public void RF_NEG_04_PendienteACompletada_TransicionProhibida_RechazaYSinCambiarEstado()
    {
        // Preparar
        var meta = new Meta { Estado = EstadoMeta.Pendiente };

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            TransicionesMeta.CambiarEstado(meta, EstadoMeta.Completada));

        // Verificar
        Assert.AreEqual(409, error.Codigo);
        Assert.AreEqual(EstadoMeta.Pendiente, meta.Estado);
    }

    [TestMethod]
    public void RF_NEG_05_Completada_EstadoTerminal_NoPermiteSalir()
    {
        // Preparar
        var meta = new Meta { Estado = EstadoMeta.Completada };

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            TransicionesMeta.CambiarEstado(meta, EstadoMeta.Activa));

        // Verificar
        Assert.AreEqual(409, error.Codigo);
        Assert.AreEqual(EstadoMeta.Completada, meta.Estado);
    }

    [TestMethod]
    public void RF_NEG_05_Cancelada_EstadoTerminal_NoPermiteSalir()
    {
        // Preparar
        var meta = new Meta { Estado = EstadoMeta.Cancelada };

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            TransicionesMeta.CambiarEstado(meta, EstadoMeta.Pendiente));

        // Verificar
        Assert.AreEqual(409, error.Codigo);
        Assert.AreEqual(EstadoMeta.Cancelada, meta.Estado);
    }

    [TestMethod]
    public void RF_NEG_06_ActivarMeta_OtroUsuario_LaRechaza()
    {
        // Preparar
        var meta = _metas.Crear(_ana, "Meta privada");

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() => _metas.Activar(_luis, meta.Id));

        // Verificar
        Assert.AreEqual(403, error.Codigo);
        Assert.AreEqual(EstadoMeta.Pendiente, _metas.ListarPropias(_ana).Single().Estado);
    }

    [TestMethod]
    public void RD_09_ReiniciarAlmacen_ConMetaCreada_LaConserva()
    {
        // Preparar
        var meta = _metas.Crear(_ana, "Persistente");

        // Actuar
        var metasReiniciadas = new ServicioMetas(new AlmacenJson(_directorio), _acceso);
        var recuperada = metasReiniciadas.ListarPropias(_ana).Single();

        // Verificar
        Assert.AreEqual(meta.Id, recuperada.Id);
        Assert.AreEqual(meta.Nombre, recuperada.Nombre);
    }

    private Usuario CrearUsuario(string nombre, string correo, Rol rol)
    {
        var usuario = new Usuario
        {
            Nombre = nombre,
            Correo = correo,
            Rol = rol,
            Activo = true,
            CorreoConfirmado = true
        };
        _almacen.Cambiar(estado =>
        {
            estado.Usuarios.Add(usuario);
            return true;
        });
        return usuario;
    }

    private static UsuarioPublico Publico(Usuario usuario) =>
        new(usuario.Id, usuario.Nombre, usuario.Correo, usuario.Rol, usuario.Activo);

    private sealed class RelojDePrueba : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
    }
}
