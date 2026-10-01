# Maquina de estados del rastreador de habitos

La entidad central de esta etapa es `Meta`, relacionada con un usuario y con su estado almacenado. Los cuatro estados estan declarados en `EstadoMeta` y las transiciones permitidas en `TransicionesMeta`, dentro de `RastreadorHabitos.Api/EstadoMeta.cs`. Esta practica entrega la estructura; las operaciones y pruebas completas del modulo de negocio corresponden a entregas posteriores.

| Desde | Hacia | Quien la ejecuta | Condicion |
| --- | --- | --- | --- |
| Pendiente | Activa | Usuario propietario | La meta esta preparada para comenzar. |
| Pendiente | Cancelada | Usuario propietario | La meta se cancela antes de comenzar. |
| Activa | Completada | Usuario propietario | Se alcanzo el objetivo de la meta. |
| Activa | Cancelada | Usuario propietario | El usuario decide detenerla. |
| Pendiente | Completada | Nadie | Prohibida explicitamente: la meta debe activarse primero. |
| Completada | Cualquier estado | Nadie | Estado terminal; no permite nuevas transiciones. |
| Cancelada | Cualquier estado | Nadie | Estado terminal; no permite nuevas transiciones. |

Cualquier otra combinacion tambien se rechaza. La regla se define en un solo componente para evitar validaciones de estado repartidas por la aplicacion.
