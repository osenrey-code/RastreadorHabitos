using System.Text.RegularExpressions;
using RastreadorHabitos.Api;

var directorio = Path.Combine(Path.GetTempPath(), "rastreador-verificacion-" + Guid.NewGuid().ToString("N"));
var reloj = new RelojDePrueba();
var almacen = new AlmacenJson(directorio);
var acceso = new ServicioAcceso(almacen, reloj, "http://localhost:5075");
var pruebas = 0;

void Comprobar(bool condicion, string descripcion)
{
    if (!condicion) throw new Exception("Fallo: " + descripcion);
    pruebas++;
}

void Rechaza(Action accion, int codigo, string descripcion)
{
    try { accion(); }
    catch (ErrorDeDominio error) when (error.Codigo == codigo)
    {
        pruebas++;
        return;
    }
    throw new Exception("No rechazo correctamente: " + descripcion);
}

try
{
    acceso.CrearAdministradorInicial("admin@example.test", "Admin12345");
    var ana = acceso.Registrar(new RegistroPeticion("Ana", "ana@example.test", "Clave12345"));
    var luis = acceso.Registrar(new RegistroPeticion("Luis", "luis@example.test", "Clave12345"));
    Comprobar(ana.Rol == Rol.Estandar && !ana.Activo, "usuario estandar inactivo");
    Rechaza(() => acceso.Registrar(new RegistroPeticion("Otra", "ANA@example.test", "Clave12345")), 409, "correo duplicado");
    Rechaza(() => acceso.Registrar(new RegistroPeticion("Eva", "correo-invalido", "Clave12345")), 400, "correo invalido");
    Rechaza(() => acceso.Registrar(new RegistroPeticion("Eva", "eva@example.test", "corta")), 400, "politica de contrasena");

    var usuarios = almacen.Leer(e => e.Usuarios.ToList());
    Comprobar(usuarios.All(u => !u.HashContrasena.Contains("Clave12345")), "hash sin texto plano");
    Comprobar(usuarios.Single(u => u.Id == ana.Id).HashContrasena != usuarios.Single(u => u.Id == luis.Id).HashContrasena,
        "sal diferente por usuario");
    Rechaza(() => acceso.Iniciar(new InicioPeticion(ana.Correo, "Clave12345")), 403, "login inactivo");

    string TokenActivacion(Guid id) => Regex.Match(almacen.Leer(e => e.Correos.Last(c => c.Destinatario ==
        e.Usuarios.Single(u => u.Id == id).Correo).Cuerpo), "token=([0-9a-f]{64})").Groups[1].Value;
    var primerEnlace = TokenActivacion(ana.Id);
    var respuestaExistente = acceso.ReenviarActivacion(new CorreoPeticion(ana.Correo));
    var respuestaAusente = acceso.ReenviarActivacion(new CorreoPeticion("nadie@example.test"));
    Comprobar(respuestaExistente == respuestaAusente, "reenvio no revela usuarios");
    Rechaza(() => acceso.Activar(primerEnlace), 400, "enlace invalidado por reenvio");
    var segundoEnlace = TokenActivacion(ana.Id);
    acceso.Activar(segundoEnlace);
    Rechaza(() => acceso.Activar(segundoEnlace), 400, "enlace de un uso");
    var enlaceLuis = TokenActivacion(luis.Id);
    reloj.Avanzar(TimeSpan.FromHours(25));
    Rechaza(() => acceso.Activar(enlaceLuis), 400, "enlace vencido");

    var mensajeInexistente = "";
    var mensajeIncorrecto = "";
    try { acceso.Iniciar(new InicioPeticion("nadie@example.test", "Clave12345")); }
    catch (ErrorDeDominio ex) { mensajeInexistente = ex.Message; }
    try { acceso.Iniciar(new InicioPeticion(ana.Correo, "Mal123456")); }
    catch (ErrorDeDominio ex) { mensajeIncorrecto = ex.Message; }
    Comprobar(mensajeInexistente == mensajeIncorrecto, "rechazo de login identico");
    for (var i = 0; i < 4; i++) Rechaza(() => acceso.Iniciar(new InicioPeticion(ana.Correo, "Mal123456")), 401, "intento fallido");
    Rechaza(() => acceso.Iniciar(new InicioPeticion(ana.Correo, "Clave12345")), 429, "sexto intento bloqueado");
    reloj.Avanzar(TimeSpan.FromMinutes(16));
    var tokenAna = acceso.Iniciar(new InicioPeticion(ana.Correo, "Clave12345"));
    Comprobar(acceso.Autenticar(tokenAna).Id == ana.Id, "sesion valida");
    acceso.Cerrar(tokenAna);
    Rechaza(() => acceso.Autenticar(tokenAna), 401, "logout invalida sesion");

    var recuperacionExistente = acceso.SolicitarRecuperacion(new CorreoPeticion(ana.Correo));
    var recuperacionAusente = acceso.SolicitarRecuperacion(new CorreoPeticion("nadie@example.test"));
    Comprobar(recuperacionExistente == recuperacionAusente, "recuperacion no revela usuarios");
    string Codigo(Guid id) => Regex.Match(almacen.Leer(e => e.Correos.Last(c => c.Destinatario ==
        e.Usuarios.Single(u => u.Id == id).Correo).Cuerpo), "([0-9a-f]{64})").Groups[1].Value;
    var codigo = Codigo(ana.Id);
    var sesionAntes = acceso.Iniciar(new InicioPeticion(ana.Correo, "Clave12345"));
    acceso.Restablecer(new RecuperarPeticion(ana.Correo, codigo, "Nueva12345"));
    Rechaza(() => acceso.Restablecer(new RecuperarPeticion(ana.Correo, codigo, "Otra12345")), 400, "codigo de un uso");
    Rechaza(() => acceso.Autenticar(sesionAntes), 401, "sesiones anteriores revocadas");
    Rechaza(() => acceso.Iniciar(new InicioPeticion(ana.Correo, "Clave12345")), 401, "contrasena antigua revocada");
    var sesionNueva = acceso.Iniciar(new InicioPeticion(ana.Correo, "Nueva12345"));
    Rechaza(() => acceso.CambiarContrasena(ana.Id, new CambiarPeticion("Incorrecta1", "Tercera123")), 400,
        "cambio exige contrasena actual");
    acceso.CambiarContrasena(ana.Id, new CambiarPeticion("Nueva12345", "Tercera123"));
    Rechaza(() => acceso.Autenticar(sesionNueva), 401, "cambio revoca sesiones");

    var lista = acceso.ListarUsuarios();
    Comprobar(lista.Count == 3 && lista.All(u => u.GetType().GetProperties().All(p =>
        !p.Name.Contains("Hash") && !p.Name.Contains("Token"))), "listado sin secretos");
    acceso.CambiarRol(ana.Id, Rol.Administrador);
    Comprobar(acceso.ListarUsuarios().Single(u => u.Id == ana.Id).Rol == Rol.Administrador, "cambio de rol");
    var admin = lista.Single(u => u.Correo == "admin@example.test");
    Rechaza(() => acceso.CambiarActivo(admin.Id, admin.Id, false), 400, "autodesactivacion");
    var antesDeDesactivar = acceso.Iniciar(new InicioPeticion(ana.Correo, "Tercera123"));
    acceso.CambiarActivo(admin.Id, ana.Id, false);
    Rechaza(() => acceso.Autenticar(antesDeDesactivar), 401, "desactivacion revoca sesiones");
    acceso.ReenviarActivacion(new CorreoPeticion(ana.Correo));
    Rechaza(() => acceso.Iniciar(new InicioPeticion(ana.Correo, "Tercera123")), 403,
        "reenvio no reactiva desactivados");
    acceso.CambiarActivo(admin.Id, ana.Id, true);
    acceso.ForzarRestablecimiento(ana.Id);
    Rechaza(() => acceso.Iniciar(new InicioPeticion(ana.Correo, "Tercera123")), 401, "restablecimiento forzado revoca clave");
    acceso.Restablecer(new RecuperarPeticion(ana.Correo, Codigo(ana.Id), "Forzada123"));
    Comprobar(!string.IsNullOrWhiteSpace(acceso.Iniciar(new InicioPeticion(ana.Correo, "Forzada123"))),
        "restablecimiento forzado permite clave nueva");

    var segundoAlmacen = new AlmacenJson(directorio);
    Comprobar(segundoAlmacen.Leer(e => e.Usuarios.Count) == 3, "persistencia tras reinicio");
    Comprobar(segundoAlmacen.Leer(e => e.Correos.All(c => c.Estado == "Pendiente")), "correo queda en cola");
    Comprobar(!TransicionesMeta.PuedeCambiar(EstadoMeta.Pendiente, EstadoMeta.Completada) &&
        !TransicionesMeta.PuedeCambiar(EstadoMeta.Completada, EstadoMeta.Activa), "transiciones prohibida y terminal");

    Console.WriteLine($"Correcto: {pruebas} comprobaciones.");
}
finally
{
    if (Directory.Exists(directorio)) Directory.Delete(directorio, true);
}

sealed class RelojDePrueba : TimeProvider
{
    private DateTimeOffset _ahora = new(2026, 9, 30, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _ahora;
    public void Avanzar(TimeSpan tiempo) => _ahora = _ahora.Add(tiempo);
}
