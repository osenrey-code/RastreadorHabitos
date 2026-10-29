using Microsoft.VisualStudio.TestTools.UnitTesting;
using RastreadorHabitos.Api;

namespace RastreadorHabitos.Pruebas;

[TestClass]
public sealed class IntegracionMetasPermisosPruebas
{
    [TestMethod]
    public void RF_NEG_06_EliminarMeta_SoloConPermisoAprobado_PermiteOperacion()
    {
        // Preparar: almacén y servicios reales, sin dobles ni servidor HTTP.
        var directorio = Path.Combine(Path.GetTempPath(), "integracion-meta-permiso-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directorio);
        try
        {
            var almacen = new AlmacenJson(directorio);
            var (administrador, ana) = CrearUsuarios(almacen);
            var acceso = new ServicioAcceso(almacen, new RelojDePrueba(), "http://localhost:5075");
            var negocio = new ServicioMetas(almacen, acceso);
            var meta = negocio.Crear(ana, "Meta protegida");
            var solicitud = acceso.CrearSolicitudPermiso(ana.Id, ServicioMetas.PermisoEliminar);

            // Actuar: falla antes de la aprobación y funciona luego de aprobar con el servicio real.
            var sinPermiso = Assert.ThrowsExactly<ErrorDeDominio>(() => negocio.Eliminar(ana, meta.Id));
            var aprobada = acceso.AprobarSolicitudPermiso(administrador.Id, solicitud.Id);
            var eliminada = negocio.Eliminar(ana, meta.Id);

            // Verificar: la regla de negocio consultó el permiso que Gestión de permisos concedió.
            Assert.AreEqual(403, sinPermiso.Codigo);
            Assert.AreEqual(EstadoSolicitudPermiso.Aplicada, aprobada.Estado);
            Assert.AreEqual(meta.Id, eliminada.Id);
            Assert.IsEmpty(negocio.ListarPropias(ana));
        }
        finally
        {
            if (Directory.Exists(directorio)) Directory.Delete(directorio, true);
        }
    }

    private static (Usuario Admin, UsuarioPublico Ana) CrearUsuarios(AlmacenJson almacen)
    {
        var admin = new Usuario
        {
            Nombre = "Admin",
            Correo = "admin@example.test",
            Rol = Rol.Administrador,
            Activo = true,
            CorreoConfirmado = true
        };
        var ana = new Usuario
        {
            Nombre = "Ana",
            Correo = "ana@example.test",
            Rol = Rol.Estandar,
            Activo = true,
            CorreoConfirmado = true
        };
        almacen.Cambiar(estado =>
        {
            estado.Usuarios.Add(admin);
            estado.Usuarios.Add(ana);
            return true;
        });
        return (admin, new UsuarioPublico(ana.Id, ana.Nombre, ana.Correo, ana.Rol, ana.Activo));
    }

    private sealed class RelojDePrueba : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
    }
}
