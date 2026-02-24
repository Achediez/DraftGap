Documentación General del Proyecto
1. Introducción
DraftGap es una aplicación orientada a proporcionar asistencia estratégica durante la fase de selección de campeones (champion draft) en League of Legends. Su función principal consiste en analizar la composición de ambos equipos, evaluar sinergias y matchups, y ofrecer recomendaciones fundamentadas en datos para optimizar la toma de decisiones del jugador.

Este documento presenta una descripción formal y accesible del proyecto, dirigida tanto a usuarios sin experiencia técnica como a aquellos interesados en comprender su funcionamiento general.

2. Objetivo del Proyecto
El propósito de DraftGap es mejorar la calidad de las decisiones tomadas durante el proceso de selección de campeones. Para ello, la aplicación:

Evalúa la composición del equipo propio y del equipo enemigo.

Analiza sinergias entre campeones.

Identifica enfrentamientos favorables y desfavorables.

Considera tendencias estadísticas del metajuego actual.

A partir de estos elementos, el sistema genera recomendaciones justificadas y alternativas viables, con el fin de proporcionar una guía clara y objetiva al usuario.

3. Funcionamiento General
DraftGap opera como una herramienta externa que observa la fase de selección y procesa la información disponible. Su funcionamiento puede dividirse en tres etapas principales:

3.1. Detección del contexto
La aplicación identifica automáticamente:

El rol asignado al usuario.

Los campeones seleccionados por aliados y oponentes.

El orden de selección y bloqueo.

3.2. Análisis
Una vez recopilados los datos, DraftGap aplica modelos de evaluación basados en:

Estadísticas de rendimiento por campeón.

Sinergias históricas entre combinaciones de campeones.

Matchups relevantes según el metajuego.

3.3. Recomendación
El sistema presenta:

Campeones sugeridos para el rol del usuario.

Justificaciones basadas en datos.

Alternativas secundarias en caso de indisponibilidad del pick principal.

4. Instalación y Uso
DraftGap está diseñado para ser accesible incluso para usuarios sin conocimientos técnicos. Existen dos métodos principales de instalación: instalación tradicional y despliegue mediante Docker.

4.1. Instalación Tradicional
Descargue la versión correspondiente a su sistema operativo desde el repositorio.

Ejecute el instalador y siga las instrucciones en pantalla.

Abra el cliente de League of Legends.

Inicie DraftGap.

La aplicación detectará automáticamente la fase de selección cuando comience una partida.

4.2. Instalación mediante Docker
Esta opción es recomendable para usuarios que deseen ejecutar DraftGap en un entorno aislado, reproducible y sin necesidad de instalar dependencias manualmente.

4.2.1. Requisitos previos
Tener instalado Docker (Docker Desktop en Windows/macOS o Docker Engine en Linux).

Conexión a Internet para descargar la imagen.

4.2.2. Construcción de la imagen
Si desea construir la imagen localmente desde el repositorio:

bash
docker build -t draftgap .
Este comando generará una imagen llamada draftgap basada en el Dockerfile incluido en el proyecto.

4.2.3. Ejecución del contenedor
Para ejecutar la aplicación:

bash
docker run --name draftgap-container -p 3000:3000 draftgap
--name draftgap-container asigna un nombre al contenedor.

-p 3000:3000 expone el puerto de la aplicación para acceder a la interfaz.

Una vez iniciado, podrá acceder a DraftGap desde su navegador en:

Código
http://localhost:3000
4.2.4. Detener y eliminar el contenedor
Para detener el contenedor:

bash
docker stop draftgap-container
Para eliminarlo:

bash
docker rm draftgap-container
5. Estructura del Proyecto
Carpeta / Archivo	Descripción
apps/	Contiene las aplicaciones principales (interfaz web o escritorio).
packages/core/	Implementa la lógica central del análisis y las recomendaciones.
scripts/	Herramientas auxiliares para automatización.
.github/	Configuración de flujos de trabajo e integración continua.
package.json / pnpm / tsconfig	Archivos de configuración del entorno de desarrollo.
6. Tecnologías Utilizadas
DraftGap se desarrolla utilizando tecnologías modernas:

TypeScript como lenguaje principal.

Frameworks web modernos para la interfaz.

Sistemas de análisis estadístico para la evaluación de campeones.

Integración con el cliente de League of Legends mediante lectura de datos expuestos por el propio juego.

7. Seguridad y Legalidad
DraftGap no modifica el juego ni interactúa con él de forma intrusiva.
Se limita a leer información accesible públicamente desde el cliente de League of Legends.

Por ello:

No altera archivos del juego.

No ejecuta acciones en nombre del usuario.

No proporciona ventajas mecánicas.

Su uso se considera seguro y no existen indicios de que pueda ocasionar sanciones por parte de Riot Games.
