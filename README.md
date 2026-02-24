Bienvenido a DraftGap, una aplicación diseñada para ayudarte a tomar mejores decisiones durante la fase de selección de campeones en League of Legends.
Si nunca has usado una herramienta de este tipo o no tienes conocimientos técnicos, no te preocupes: esta guía está hecha para ti.

¿Qué es DraftGap?
DraftGap es una aplicación que analiza:

Tu equipo

El equipo enemigo

Las sinergias entre campeones

Los enfrentamientos (matchups)

La situación actual del meta

Con esa información, te sugiere qué campeones son mejores para elegir en cada momento de la fase de draft.
Su objetivo es ayudarte a tomar decisiones más inteligentes y aumentar tus probabilidades de ganar.

¿Para qué sirve?
DraftGap te ayuda a:

Elegir campeones que funcionen bien con tu equipo.

Evitar picks que sean débiles contra los rivales.

Entender por qué un campeón es una buena o mala opción.

Mejorar tu conocimiento del meta sin tener que estudiar estadísticas manualmente.

Es como tener un analista profesional a tu lado mientras haces el draft.

¿Cómo funciona?
La aplicación se conecta con el cliente de League of Legends (en las versiones que lo permiten) y detecta automáticamente:

Qué campeones se están seleccionando.

En qué orden.

Qué rol estás jugando.

Luego, usando datos estadísticos y modelos de análisis, calcula:

Matchups favorables

Sinergias con tus aliados

Riesgos del pick

Opciones alternativas

Todo esto se muestra de forma visual y sencilla.

Cómo empezar (para usuarios sin experiencia)
1. Descargar la aplicación
En el repositorio original de DraftGap existen versiones para Windows y macOS.
(Tu repositorio puede incluir instrucciones propias si has modificado el proyecto.)

2. Instalar
La instalación es igual que cualquier programa:

En Windows: doble clic en el .exe o .msi

En Mac: abrir el .dmg y arrastrar a Aplicaciones

3. Abrir League of Legends
DraftGap funciona mejor si el cliente está abierto antes de iniciar la app.

4. Iniciar DraftGap
La aplicación detectará automáticamente la fase de selección cuando entres en una partida.

5. Seguir las recomendaciones
Verás sugerencias de campeones y explicaciones de por qué son buenas opciones.

Estructura del proyecto (explicado para principiantes)
Aunque no tengas conocimientos técnicos, aquí tienes una explicación sencilla de cómo está organizado el proyecto:

Carpeta	Para qué sirve
apps/	Contiene las aplicaciones principales (web o escritorio).
packages/core/	Aquí vive la lógica del análisis: cálculos, estadísticas, recomendaciones.
scripts/	Herramientas internas para automatizar tareas del proyecto.
.github/	Configuraciones para automatizar procesos en GitHub.
package.json / pnpm / tsconfig	Archivos técnicos que gestionan dependencias y configuración del proyecto.
(Esta estructura es típica del proyecto original y forks derivados.) 

¿Qué tecnologías usa?
Aunque no necesites saber programar, es útil conocer qué hay detrás:

TypeScript / JavaScript → Lenguajes principales del proyecto.

Framework web moderno (React o similar) → Para la interfaz.

Integración con el cliente de LoL → Para leer datos del draft.

Sistema de análisis estadístico → Para calcular recomendaciones.

¿Qué puedes modificar tú?
Si tu repositorio es un fork o una versión personalizada, puedes:

Cambiar estilos visuales.

Ajustar cómo se muestran las recomendaciones.

Añadir nuevos criterios de análisis.

Actualizar datos del meta.

Preguntas frecuentes
¿Necesito saber programar para usar DraftGap?
No. Solo descargas, instalas y usas.

¿Es legal usarlo?
Sí. No modifica el juego ni interactúa con él de forma ilegal. Solo lee información pública del cliente.

¿Me puede banear Riot?
No hay evidencia de que herramientas de análisis externas como esta causen baneos, ya que no alteran el juego.

¿Funciona en ARAM o URF?
Normalmente está pensado para partidas clasificatorias o normales con draft.

