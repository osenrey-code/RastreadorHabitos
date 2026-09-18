```mermaid
graph TD
    %% Definición del Core
    subgraph Core ["Core del Sistema (Estándar)"]
        Acceso[Control de Acceso]
        Permisos[Gestión de Permisos]
        Auditoria[Auditoría]
        Notificaciones[Notificaciones]
        Reportes[Reportes]
        Documentos[Documentos]
    end

    %% Módulo de Negocio
    subgraph Negocio ["Módulo de Negocio"]
        Habitos[Rastreador de Hábitos]
    end

    %% Relaciones y flechas etiquetadas
    Acceso --> |Autentica al usuario| Habitos
    Habitos --> |Verifica autorización| Permisos
    Habitos --> |Registra cambios críticos| Auditoria
    Habitos --> |Alerta de rachas/fallos| Notificaciones
    Habitos --> |Genera estadísticas| Reportes
    Habitos --> |Exporta rutinas| Documentos
    ```
