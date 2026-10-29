# Máquina de estados del rastreador de hábitos

La entidad central es `Meta`, relacionada con un usuario y su estado persistido. Los cuatro estados están declarados en `EstadoMeta` y sus transiciones se resuelven en un solo lugar, `TransicionesMeta.CambiarEstado`, dentro de `RastreadorHabitos.Api/EstadoMeta.cs`.

| Desde | Hacia | Quien la ejecuta | Condicion |
| --- | --- | --- | --- |
| Pendiente | Activa | Usuario propietario | La meta está preparada para comenzar. |
| Pendiente | Cancelada | Usuario propietario | La meta se cancela antes de comenzar. |
| Activa | Completada | Usuario propietario | Se alcanzó el objetivo de la meta. |
| Activa | Cancelada | Usuario propietario | El usuario decide detenerla. |
| Pendiente | Completada | Nadie | Prohibida explícitamente: la meta debe activarse primero. |
| Completada | Cualquier estado | Nadie | Estado terminal; no permite nuevas transiciones. |
| Cancelada | Cualquier estado | Nadie | Estado terminal; no permite nuevas transiciones. |

Las operaciones de metas solo actúan sobre metas del usuario autenticado. `Pendiente → Activa` exige que actúe el propietario. Cualquier transición no declarada también se rechaza sin cambiar el estado.

## Operaciones implementadas

- Crear una meta en estado `Pendiente`.
- Listar las metas propias.
- Activar una meta pendiente.
- Completar una meta activa.
- Cancelar una meta pendiente o activa.
- Eliminar una meta propia con el permiso `habitos.eliminar` aprobado y aplicado.

Las rutas del negocio requieren sesión válida de usuario Estándar. La política de transición es independiente de la máquina de estados de Gestión de permisos; el permiso se consulta solamente para permitir la operación de eliminación.
