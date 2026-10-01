# Fuentes del Bloque 2

Referencias recomendadas para las semanas 2 a 5, la Asignación 1 y la Práctica 1 de Programación III.

## Cómo usar esta lista

- Las fuentes marcadas como **Oficial** tienen prioridad cuando contradicen a un tutorial.
- Solo es necesario leer las secciones indicadas, salvo cuando se especifica la página completa.
- Estas fuentes orientan el diseño de componentes y el flujo de trabajo con Git. No agregan funcionalidades al rastreador ni sustituyen los requisitos del Core.

## 1. Componentes, responsabilidad única y ocultamiento de información

### Oficial: Principios de arquitectura de Microsoft

- URL: https://learn.microsoft.com/es-es/dotnet/architecture/modern-web-apps-azure/architectural-principles
- Para qué sirve: ayuda a decidir la responsabilidad de cada componente y evita que una pieza conozca el estado interno de otra.
- Leer: dentro de "Principios de diseño comunes", las secciones "Separación de intereses", "Encapsulación" y "Responsabilidad única". Si la traducción automática resulta confusa, cambiar `/es-es/` por `/en-us/` en la URL.

### Oficial: Arquitecturas comunes de aplicación de Microsoft

- URL: https://learn.microsoft.com/es-es/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures
- Para qué sirve: aporta vocabulario para ubicar los componentes por capas y marcar la frontera entre el Core y el módulo de negocio.
- Leer: "Arquitectura monolítica" y la arquitectura en capas, antes de la sección de microservicios.

### Tutorial: Refactoring Guru, clase grande

- URL: https://refactoring.guru/es/smells/large-class
- Para qué sirve: permite reconocer componentes con demasiadas responsabilidades y posibles divisiones.
- Leer: la página completa, prestando atención a "Signos y síntomas" y a `Extract Class` y `Extract Interface`.

## 2. Interfaces y comunicación entre componentes

### Oficial: UML 2.5.1 de la OMG

- URL: https://www.omg.org/spec/UML/2.5.1/
- Para qué sirve: define formalmente los conceptos de componente, interfaz provista e interfaz requerida.
- Leer: solamente la cláusula "Components" y, dentro de ella, las interfaces provistas y requeridas. Usar el documento como diccionario.

### Tutorial: Modelo C4, abstracción de componente

- URL: https://c4model.com/abstractions/component
- Para qué sirve: define un componente como una agrupación de código con una interfaz bien definida.
- Leer: la página completa. Un componente no implica necesariamente una unidad desplegable independiente ni un microservicio.

## 3. Notación gráfica del diseño

### Tutorial: Modelo C4, diagrama de componentes

- URL: https://c4model.com/diagrams/component
- Para qué sirve: orienta el diagrama de componentes con nombre, responsabilidad, tecnología y comunicaciones etiquetadas.
- Leer: la página indicada y luego https://c4model.com/diagrams/notation. El alcance esperado es un solo contenedor, no toda la aplicación.

### Oficial: Sintaxis de diagramas de flujo de Mermaid

- URL: https://mermaid.js.org/syntax/flowchart.html
- Para qué sirve: permite mantener el diagrama como texto versionado dentro de un archivo Markdown.
- Leer: nodos, aristas y `subgraph`, útil para separar visualmente el Core del módulo de negocio.

### Oficial: Diagramas Mermaid en GitHub

- URL: https://docs.github.com/es/get-started/writing-on-github/working-with-advanced-formatting/creating-diagrams
- Para qué sirve: confirma qué sintaxis de Mermaid puede renderizar GitHub dentro del README.
- Leer: la sección "Crear diagramas de Mermaid".

## 4. Estrategia de ramas

### Oficial: Pro Git en español, ramificaciones

- URL: https://git-scm.com/book/es/v2/Ramificaciones-en-Git-¿Qué-es-una-rama?
- Para qué sirve: explica que una rama es un puntero y prepara para comprender las fusiones y sus conflictos.
- Leer: 3.1 "¿Qué es una rama?" y 3.2 "Procedimientos básicos para ramificar y fusionar". La sección 3.6 sobre `rebase` no es necesaria para la Práctica 1.

### Oficial: GitHub Flow

- URL: https://docs.github.com/es/get-started/using-github/github-flow
- Para qué sirve: describe el flujo esperado en el curso: rama, commits, pull request, revisión y fusión.
- Leer: la página completa.

### Tutorial: Learn Git Branching

- URL: https://learngitbranching.js.org/?locale=es_ES
- Para qué sirve: permite practicar visualmente ramas y fusiones sin modificar el repositorio real.
- Leer: la secuencia "Introducción a Git" completa. La sección de repositorios remotos prepara para la sesión 3.

## 5. Commits atómicos, mensajes y `.gitignore`

### Oficial: Documentación de `gitignore`

- URL: https://git-scm.com/docs/gitignore
- Para qué sirve: define el comportamiento de los patrones `*`, `/`, `!` y `**`.
- Leer: la sección "PATTERN FORMAT". Agregar una ruta a `.gitignore` no deja de rastrear un archivo que Git ya conoce.

### Oficial: Plantillas `.gitignore` de GitHub

- URL: https://github.com/github/gitignore
- Para qué sirve: ofrece plantillas para excluir compilaciones, dependencias, configuraciones del editor y credenciales locales.
- Leer: localizar la plantilla correspondiente a .NET y a las herramientas del proyecto; se pueden combinar varias.

### Oficial: Conventional Commits 1.0.0 en español

- URL: https://www.conventionalcommits.org/es/v1.0.0/
- Para qué sirve: establece el formato de los mensajes de commit esperado en la Práctica 1.
- Leer: el resumen y los tipos `feat`, `fix`, `docs`, `refactor` y `test`. `BREAKING CHANGE` y el versionado semántico no son necesarios para este bloque.

### Tutorial: How to Write a Git Commit Message, Chris Beams

- URL: https://cbea.ms/git-commit/
- Para qué sirve: ayuda a redactar mensajes que expliquen el motivo del cambio en lugar de repetir el diff.
- Leer: las siete reglas como lista de comprobación antes de cada commit.

## 6. Pull requests

### Estructura obligatoria para la práctica

Todo pull request de la práctica debe incluir estas cuatro secciones:

1. **Qué cambia:** una o dos frases que expliquen qué hace ahora el sistema que antes no hacía.
2. **Por qué:** indicar los requisitos del Core que cubre mediante sus identificadores, por ejemplo `RF-CA-01` y `RD-05`.
3. **Cómo probarlo:** proporcionar los comandos exactos y describir el resultado que debe observarse, sin obligar al revisor a adivinar el procedimiento.
4. **Qué NO incluye:** aclarar lo que queda fuera del alcance y se implementará en otro pull request.

Cada pull request debe representar una sola funcionalidad. Si el título necesita una "y" para describir el cambio, deben crearse dos pull requests.

### Oficial: Acerca de los pull requests en GitHub

- URL: https://docs.github.com/es/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests
- Para qué sirve: explica cómo entregar un cambio mediante un pull request revisable y contra qué rama abrirlo.
- Leer: esta página y la guía para crear un pull request.

### Oficial: Plantilla de pull request para el repositorio

- URL: https://docs.github.com/es/communities/using-templates-to-encourage-useful-issues-and-pull-requests/creating-a-pull-request-template-for-your-repository
- Para qué sirve: permite mantener una estructura uniforme en las descripciones de los pull requests mediante un archivo dentro de `.github/`.
- Leer: la página completa.

## 7. Conflictos de fusión y revisión de código

### Oficial: Conflictos de fusión en GitHub

- URL: https://docs.github.com/es/pull-requests/collaborating-with-pull-requests/addressing-merge-conflicts/about-merge-conflicts
- Para qué sirve: explica los marcadores `<<<<<<<`, `=======` y `>>>>>>>` y cómo resolver conflictos sin perder trabajo.
- Leer: esta página y la guía para resolver un conflicto mediante la línea de comandos. Para el parcial se requiere la línea de comandos, no solamente el editor web.

### Oficial: Revisiones de pull requests en GitHub

- URL: https://docs.github.com/es/pull-requests/collaborating-with-pull-requests/reviewing-changes-in-pull-requests/about-pull-request-reviews
- Para qué sirve: describe comentarios de línea, sugerencias de cambio y los estados de una revisión.
- Leer: la descripción de la revisión y la diferencia entre comentar, aprobar y solicitar cambios.

### Tutorial: Google Engineering Practices, qué revisar

- URL: https://google.github.io/eng-practices/review/reviewer/looking-for.html
- Para qué sirve: aporta criterios de revisión sobre diseño, funcionalidad, complejidad, pruebas, nombres y comentarios.
- Leer: la página completa.

### Tutorial: Google Engineering Practices, responder comentarios

- URL: https://google.github.io/eng-practices/review/developer/handling-comments.html
- Para qué sirve: explica cómo responder a observaciones, cuándo corregir y cuándo justificar una decisión técnica.
- Leer: la página completa.

## Implicaciones para RastreadorHabitos

- Mantener una frontera clara entre el Core y el rastreador de hábitos.
- Diseñar componentes con una sola responsabilidad e interfaces explícitas.
- Versionar los diagramas en Markdown con Mermaid cuando corresponda.
- Trabajar cada funcionalidad en una rama corta y entregar cambios mediante pull requests revisables.
- Usar commits atómicos y mensajes con un tipo convencional.
- Mantener fuera del repositorio archivos generados, configuraciones locales y credenciales.
- Compilar y ejecutar las pruebas antes de fusionar cambios o cerrar conflictos.
