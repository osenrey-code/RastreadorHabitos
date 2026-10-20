# Bitácora de la Asignación 2

## Sesión del 20 de octubre de 2026

### Qué le pedí al agente

Le compartí el documento de la Asignación 2 y le pedí: “Vamos a meter mano con esta tarea”. La tarea delegada fue revisar el enunciado, integrar Gestión de permisos en el proyecto existente y preparar la primera suite de pruebas unitarias.

### Qué me devolvió

El agente identificó los requisitos RF-GP-01 a RF-GP-07 y propuso integrarlos en la API existente. Añadió el modelo persistente de solicitud, un único punto para validar las transiciones, operaciones para crear, consultar, aprobar y rechazar, autorización del lado del servidor y aplicación automática del permiso. También creó un proyecto MSTest con diez pruebas directas del servicio, sin levantar la aplicación completa, y documentó el comando de ejecución y los pasos manuales en el README.

### Error del agente, cómo lo detecté y cómo se corrigió

En la primera versión de las pruebas el agente usó `Assert.Multiple`, una API que no pertenece a MSTest. El error se detectó al ejecutar `dotnet test`: el compilador informó cinco veces `CS0117: 'Assert' does not contain a definition for 'Multiple'`.

La corrección fue reemplazar cada bloque por aserciones normales de MSTest y volver a ejecutar la suite. También se habilitó la ejecución paralela por método para comprobar que cada prueba usa sus propios datos y no depende del orden. El resultado posterior fue correcto; al cerrar la asignación, la suite quedó con 10 pruebas ejecutadas, 10 aprobadas y 0 fallidas.

### Ajuste de interpretación antes de programar

Al leer solo RF-GP-03 parecía posible dejar una aprobación en estado `Aprobada`. Al contrastarlo con la secuencia obligatoria `Pendiente → Aprobada → Aplicada` y con RF-GP-05, se detectó que eso dejaría incompleta la aplicación automática. Se corrigió el diseño antes de cerrar la implementación: una aprobación válida registra quién y cuándo, concede el permiso y termina en `Aplicada`; ambas transiciones pasan por el mismo método de validación.

### Evidencia verificable

- Proyecto de pruebas: `RastreadorHabitos.Pruebas`.
- Prueba válida: `RF_GP_03_Y_05_AprobarSolicitud_Pendiente_AplicaPermisoYRegistraResolucion`.
- Prueba prohibida: `RF_GP_06_AprobarSolicitud_DosVeces_RechazaYConservaEstadoAplicada`.
- Comando único: `dotnet test .\RastreadorHabitos.Pruebas\RastreadorHabitos.Pruebas.csproj`.
- Resultado final de la sesión: 10 aprobadas, 0 fallidas.
- Flujo HTTP real: creación `201`, aprobación `Aplicada`, permiso concedido, segunda aprobación `409`, rechazo sin motivo `400` y rechazo válido `Rechazada`.
- Persistencia real: después de reiniciar la API con el mismo directorio, se recuperaron las dos solicitudes y el permiso aplicado.
