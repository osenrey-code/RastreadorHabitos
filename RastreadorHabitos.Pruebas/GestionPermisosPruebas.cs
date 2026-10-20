using Microsoft.VisualStudio.TestTools.UnitTesting;
using RastreadorHabitos.Api;

namespace RastreadorHabitos.Pruebas;

[TestClass]
public sealed class GestionPermisosPruebas
{
    private string _directorio = null!;
    private AlmacenJson _almacen = null!;
    private ServicioAcceso _servicio = null!;
    private RelojDePrueba _reloj = null!;
    private Usuario _administrador = null!;
    private Usuario _estandar = null!;

    [TestInitialize]
    public void PrepararPrueba()
    {
        _directorio = Path.Combine(Path.GetTempPath(), "permisos-prueba-" + Guid.NewGuid().ToString("N"));
        _almacen = new AlmacenJson(_directorio);
        _reloj = new RelojDePrueba();
        _administrador = CrearUsuario("Administrador", "admin@example.test", Rol.Administrador);
        _estandar = CrearUsuario("Ana", "ana@example.test", Rol.Estandar);
        _servicio = new ServicioAcceso(_almacen, _reloj, "http://localhost:5075");
    }

    [TestCleanup]
    public void LimpiarPrueba()
    {
        if (Directory.Exists(_directorio)) Directory.Delete(_directorio, true);
    }

    [TestMethod]
    public void RF_GP_01_CrearSolicitud_UsuarioEstandar_LaDejaPendiente()
    {
        // Preparar
        const string permiso = "habitos.exportar";

        // Actuar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, permiso);

        // Verificar
        Assert.AreEqual(EstadoSolicitudPermiso.Pendiente, solicitud.Estado);
    }

    [TestMethod]
    public void RD_07_CrearSolicitud_SinPermiso_LaRechazaDeFormaControlada()
    {
        // Preparar
        const string permisoInvalido = "   ";

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            _servicio.CrearSolicitudPermiso(_estandar.Id, permisoInvalido));

        // Verificar
        Assert.AreEqual(400, error.Codigo);
        Assert.IsEmpty(_servicio.ListarSolicitudesPropias(_estandar.Id));
    }

    [TestMethod]
    public void RF_GP_02_ObtenerSolicitud_DeOtroUsuarioEstandar_LaRechaza()
    {
        // Preparar
        var otroUsuario = CrearUsuario("Luis", "luis@example.test", Rol.Estandar);
        var solicitud = _servicio.CrearSolicitudPermiso(otroUsuario.Id, "habitos.exportar");

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            _servicio.ObtenerSolicitudPermiso(_estandar.Id, solicitud.Id));

        // Verificar
        Assert.AreEqual(403, error.Codigo);
    }

    [TestMethod]
    public void RF_GP_02_ListarPendientes_Administrador_DevuelveSoloSolicitudesPendientes()
    {
        // Preparar
        var pendiente = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.exportar");
        var rechazada = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.eliminar");
        _servicio.RechazarSolicitudPermiso(_administrador.Id, rechazada.Id, "No corresponde al perfil");

        // Actuar
        var solicitudes = _servicio.ListarSolicitudesPendientes(_administrador.Id);

        // Verificar
        CollectionAssert.AreEqual(new[] { pendiente.Id }, solicitudes.Select(s => s.Id).ToArray());
    }

    [TestMethod]
    public void RF_GP_03_Y_05_AprobarSolicitud_Pendiente_AplicaPermisoYRegistraResolucion()
    {
        // Preparar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.exportar");

        // Actuar
        var resultado = _servicio.AprobarSolicitudPermiso(_administrador.Id, solicitud.Id);

        // Verificar
        Assert.AreEqual(EstadoSolicitudPermiso.Aplicada, resultado.Estado);
        Assert.AreEqual(_administrador.Id, resultado.ResueltaPor);
        Assert.AreEqual(_reloj.GetUtcNow(), resultado.Resuelta);
        Assert.IsTrue(_servicio.TienePermiso(_estandar.Id, "habitos.exportar"));
    }

    [TestMethod]
    public void RF_GP_04_RechazarSolicitud_SinMotivo_LaMantienePendiente()
    {
        // Preparar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.eliminar");

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            _servicio.RechazarSolicitudPermiso(_administrador.Id, solicitud.Id, "   "));

        // Verificar
        Assert.AreEqual(400, error.Codigo);
        Assert.AreEqual(EstadoSolicitudPermiso.Pendiente,
            _servicio.ObtenerSolicitudPermiso(_estandar.Id, solicitud.Id).Estado);
    }

    [TestMethod]
    public void RF_GP_04_RechazarSolicitud_ConMotivo_RegistraElMotivo()
    {
        // Preparar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.eliminar");

        // Actuar
        var resultado = _servicio.RechazarSolicitudPermiso(
            _administrador.Id,
            solicitud.Id,
            "No corresponde al perfil");

        // Verificar
        Assert.AreEqual(EstadoSolicitudPermiso.Rechazada, resultado.Estado);
        Assert.AreEqual("No corresponde al perfil", resultado.MotivoRechazo);
    }

    [TestMethod]
    public void RF_GP_06_AprobarSolicitud_DosVeces_RechazaYConservaEstadoAplicada()
    {
        // Preparar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.exportar");
        _servicio.AprobarSolicitudPermiso(_administrador.Id, solicitud.Id);

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            _servicio.AprobarSolicitudPermiso(_administrador.Id, solicitud.Id));

        // Verificar
        Assert.AreEqual(409, error.Codigo);
        Assert.AreEqual(EstadoSolicitudPermiso.Aplicada,
            _servicio.ObtenerSolicitudPermiso(_estandar.Id, solicitud.Id).Estado);
    }

    [TestMethod]
    public void RF_GP_07_ResolverSolicitud_Propia_LaRechaza()
    {
        // Preparar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.exportar");
        _servicio.CambiarRol(_estandar.Id, Rol.Administrador);

        // Actuar
        var error = Assert.ThrowsExactly<ErrorDeDominio>(() =>
            _servicio.AprobarSolicitudPermiso(_estandar.Id, solicitud.Id));

        // Verificar
        Assert.AreEqual(403, error.Codigo);
        Assert.AreEqual(EstadoSolicitudPermiso.Pendiente,
            _servicio.ObtenerSolicitudPermiso(_estandar.Id, solicitud.Id).Estado);
    }

    [TestMethod]
    public void RD_09_ReiniciarAlmacen_ConSolicitudCreada_LaConserva()
    {
        // Preparar
        var solicitud = _servicio.CrearSolicitudPermiso(_estandar.Id, "habitos.exportar");

        // Actuar
        var servicioReiniciado = new ServicioAcceso(new AlmacenJson(_directorio), _reloj, "http://localhost:5075");
        var recuperada = servicioReiniciado.ObtenerSolicitudPermiso(_estandar.Id, solicitud.Id);

        // Verificar
        Assert.AreEqual(solicitud.Id, recuperada.Id);
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

    private sealed class RelojDePrueba : TimeProvider
    {
        private readonly DateTimeOffset _ahora = new(2026, 10, 20, 20, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => _ahora;
    }
}
