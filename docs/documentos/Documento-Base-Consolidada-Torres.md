# Base consolidada del sistema — Juego de Torres

**Documento:** `Documento-Base-Consolidada-Torres.md` · 9 sep 2026
**Qué es:** el estado vigente del sistema. Funcionalidades, casos de uso, entidades y reglas
que quedan establecidos y que deben respetarse en el diseño.

**Cómo leerlo.** Cada elemento lleva su procedencia entre acentos graves: `R` documento de
reglas del juego, `DR-` reglas del dominio cerradas, `D-` decisiones de persistencia, `P-`
respuestas del proyecto, `RES-` restricciones técnicas, `ORG-` condiciones organizacionales.
Lo que no lleva marca procede de las decisiones de STK-02 recogidas en el análisis de cobertura
de casos de uso.

**Dónde vive cada dato.** `[BD]` permanente en base de datos · `[BD-T]` temporal en base de
datos · `[MEM]` memoria del servidor · `[DER]` calculado · `[CLI]` solo cliente · `[ARCH]`
archivo en el disco del servidor.

---

# 1. Funcionalidades del sistema

Cuarenta y tres funcionalidades, en siete bloques.

## 1.1 Identidad, cuenta y perfil

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-01** | **Crear cuenta** con username, correo y contraseña | Usuario sin cuenta | `[BD]` crea la cuenta |
| **FC-02** | **Iniciar sesión** verificando la contraseña contra su hash | Usuario con cuenta | `[BD]` solo lectura |
| **FC-03** | **Recuperar el acceso**: el sistema envía un código al correo, el usuario lo introduce y establece una contraseña nueva | Usuario con cuenta | `[BD]` lee la cuenta, escribe la contraseña; `[BD-T]` el código |
| **FC-04** | **Consultar y modificar el perfil**: nombre de usuario y avatar | Usuario con cuenta | `[BD]` actualiza; `[ARCH]` guarda la imagen |
| **FC-05** | **Eliminar la cuenta** y todo lo asociado a ella | Usuario con cuenta | `[BD]` borra en cascada; `[ARCH]` borra el avatar |
| **FC-06** | **Obtener identidad de invitado** con un alias libre y un identificador temporal | Usuario sin cuenta | `[MEM]` nada persistente |
| **FC-07** | **Cambiar el idioma de la interfaz** entre español e inglés | Cualquier usuario | `[CLI]` |

## 1.2 Amistades

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-08** | **Buscar un jugador por su nombre de usuario** | Usuario con cuenta | `[BD]` lectura |
| **FC-09** | **Enviar una solicitud de amistad** | Usuario con cuenta | `[BD]` crea la solicitud |
| **FC-10** | **Aceptar una solicitud de amistad** | Usuario con cuenta | `[BD]` actualiza a aceptada |
| **FC-11** | **Rechazar una solicitud de amistad** | Usuario con cuenta | `[BD]` la borra |
| **FC-34** | **Eliminar una amistad ya aceptada** | Usuario con cuenta | `[BD]` la borra |

## 1.3 Salas e invitaciones

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-12** | **Crear una sala**, pública o privada, con su código de cuatro caracteres | Usuario con cuenta o Invitado | `[BD]` crea la sala; `[MEM]` miembros, anfitrión, colores, chat y ranking |
| **FC-13** | **Consultar las salas públicas abiertas y buscar una por su código** | Usuario con cuenta o Invitado | `[MEM]` lectura |
| **FC-14** | **Entrar a una sala** | Usuario con cuenta o Invitado | `[MEM]` actualiza la lista |
| **FC-15** | **Salir de una sala**, cerrándola si era el último | Usuario con cuenta o Invitado | `[MEM]`, y `[BD]` al cerrarla |
| **FC-35** | **Expulsar a un jugador de la sala** | Anfitrión | `[MEM]` |
| **FC-16** | **Invitar a un jugador a la sala desde dentro del juego** | Usuario con cuenta | `[BD-T]` crea la invitación |
| **FC-17** | **Invitar a un jugador a la sala por correo**, enviándole el código de la sala | Usuario con cuenta | `[BD-T]` crea la invitación |
| **FC-18** | **Responder una invitación**, aceptándola o rechazándola | Usuario con cuenta | `[BD-T]` la borra |

## 1.4 Historial y clasificaciones

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-19** | **Consultar el historial de partidas** terminadas propias | Usuario con cuenta | `[BD]` lectura |
| **FC-20** | **Consultar el detalle de una partida** del historial | Usuario con cuenta | `[BD]` lectura |
| **FC-21** | **Consultar el ranking global** | Usuario con cuenta o Invitado | `[DER]` |
| **FC-22** | **Consultar el ranking de la sala** | Usuario con cuenta o Invitado | `[MEM]` lectura |
| **FC-23** | **Actualizar el ranking de la sala** al terminar cada partida | El sistema | `[MEM]` |

## 1.5 La partida

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-24** | **Iniciar la partida** con los jugadores activos de la sala | Anfitrión | `[BD]` crea partida y participaciones; `[BD-T]` primer estado; `[MEM]` puestos y colores |
| **FC-25** | **Colocar el caballero inicial**, uno por jugador, en orden de puesto de mesa | Jugador | `[MEM]` |
| **FC-26** | **Colocar el rey** | Jugador designado por la regla | `[MEM]` |
| **FC-27** | **Jugar un turno**: gastar los puntos de acción en las acciones permitidas | Jugador en turno | `[MEM]` |
| **FC-28** | **Cerrar el turno**, por decisión del jugador o por agotarse su tiempo, guardando el estado | Jugador en turno / el sistema | `[BD-T]` escribe el estado |
| **FC-29** | **Cerrar la ronda**: puntuación parcial y designación del colocador del rey. **El reparto de construcciones no pertenece al cierre de ronda**: cada jugador las recibe **en cada turno**, según la ronda y el turno `R 1.3`, `R 2.2` | El sistema | `[MEM]` |
| **FC-30** | **Terminar la partida y registrar el resultado** | El sistema | `[BD]` resultado; `[BD-T]` elimina el estado; `[MEM]` ranking de sala |
| **FC-31** | **Perder la conexión y reconectar** dentro de la ventana, retomando el turno | Jugador | `[MEM]` |
| **FC-32** | **Retirar de la partida** al jugador que agota su ventana de reconexión | El sistema | `[BD]` marca el retiro; `[MEM]` tablero |
| **FC-33** | **Reanudar una partida** tras una caída del servidor | El sistema, con los jugadores que vuelven | `[BD]`, `[BD-T]`, `[MEM]` |
| **FC-44** | **Abandonar la partida**, rindiéndose desde el menú de la partida o desde el aviso de partida en curso tras una caída | Jugador | `[BD]` marca el retiro; `[MEM]` tablero |

## 1.6 Chat, reportes y sanciones

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-36** | **Enviar un mensaje al chat de la sala**, visible también durante sus partidas | Usuario con cuenta o Invitado | `[MEM]` |
| **FC-37** | **Consultar el chat**, incluidos los mensajes anteriores a la llegada | Usuario con cuenta o Invitado | `[MEM]` |
| **FC-38** | **Reportar a un jugador** por un mensaje ofensivo | Usuario con cuenta | `[BD]` crea el reporte con copia del mensaje |
| **FC-39** | **Aplicar una sanción** al alcanzarse el umbral de reportes, y el baneo permanente por reincidencia | El sistema | `[BD]` crea la sanción |
| **FC-40** | **Comprobar la sanción vigente** de una cuenta | El sistema | `[BD]` lectura |

## 1.7 Presencia e identidad visible

| ID | Funcionalidad | Actor | Datos |
|---|---|---|---|
| **FC-41** | **Asignar y mostrar el color de cada jugador** dentro de la sala | El sistema | `[MEM]` |
| **FC-42** | **Marcar inactivo** al jugador que se desconecta sin salir, **traspasando la condición de anfitrión** si la tenía | El sistema | `[MEM]` |
| **FC-43** | **Retirar de la sala al inactivo** que agota el plazo, cerrando la sala si era el último | El sistema | `[MEM]`, y `[BD]` al cerrarla |

---

# 2. Casos de uso

## 2.1 Actores

| Actor | Quién es |
|---|---|
| **Usuario sin cuenta** | Quien usa la aplicación sin haberse identificado |
| **Usuario con cuenta** | Quien tiene sesión iniciada con su cuenta |
| **Invitado** | Identidad temporal con alias libre. Juega, crea salas y puede ser anfitrión; no tiene historial, ranking global, amigos ni invitaciones; no invita, no reporta y no puede ser reportado |
| **Anfitrión** | **Rol dentro de una sala**, que asume un Usuario con cuenta o un Invitado. Expulsa jugadores e inicia la partida |
| **Jugador** | **Rol dentro de una partida**, que asume un Usuario con cuenta o un Invitado. Cuando le toca, es el **jugador en turno** |
| **El sistema** | Dispara por sí solo, o por vencimiento de un plazo, los comportamientos de §2.5. No es un actor humano |

## 2.2 Qué funcionalidad es un caso de uso

**Criterio aplicado:** es caso de uso lo que un actor persigue como objetivo observable. **No lo
es** un paso interno de una operación, ni un comportamiento que dispara el sistema o el tiempo.

| Clase | Funcionalidades |
|---|---|
| **Caso de uso con actor** (29) | FC-01, FC-02, FC-03, FC-04, FC-05, FC-06, FC-07, FC-09, FC-10, FC-11, FC-34, FC-12, FC-13, FC-14, FC-15, FC-35, FC-16, FC-17, FC-18, FC-19, FC-20, FC-21, FC-24, FC-25, FC-26, FC-27, FC-31, FC-36, FC-38 |
| **Paso o inclusión dentro de otro caso de uso** (6) | FC-08 (búsqueda, dentro de FC-09 y FC-16) · FC-22 (el ranking se muestra al entrar a la sala) · FC-28 (el cierre de turno pertenece al turno) · FC-37 (leer el chat es parte de estar en la sala o en la partida) · FC-40 (la comprobación de sanción se ejecuta siempre dentro de otro caso) · FC-41 (el color se asigna al entrar a la sala) |
| **Comportamiento del sistema, sin actor humano** (8) | FC-23, FC-29, FC-30, FC-32, FC-33, FC-39, FC-42, FC-43 |

**Dos funcionalidades no generan caso de uso propio aunque lo pareciera.** **FC-17**, invitar
por correo, es **una elección de canal dentro del caso de uso de invitar**, no otro caso: el
correo lleva el código de la sala y la invitación ya figura en la bandeja del destinatario. Y
**el canje de esa invitación** no es un caso nuevo: se resuelve entrando por código o
respondiendo la invitación desde la bandeja, que ya son casos de uso existentes.

## 2.3 Casos de uso ya identificados

Diecinueve.

| ID | Caso de uso | Actor principal |
|---|---|---|
| **CU-01** | Iniciar sesión | Usuario con cuenta |
| **CU-02** | Crear cuenta | Usuario sin cuenta |
| **CU-03** | Recuperar acceso | Usuario con cuenta |
| **CU-04** | Jugar como invitado | Usuario sin cuenta |
| **CU-05** | Cambiar el idioma de la interfaz | Usuario sin cuenta, Usuario con cuenta, Invitado |
| **CU-06** | Modificar el perfil | Usuario con cuenta |
| **CU-07** | Eliminar la cuenta | Usuario con cuenta |
| **CU-08** | Enviar una solicitud de amistad | Usuario con cuenta |
| **CU-09** | Responder una solicitud de amistad | Usuario con cuenta |
| **CU-10** | Consultar el historial de partidas | Usuario con cuenta |
| **CU-11** | Consultar el detalle de una partida | Usuario con cuenta |
| **CU-12** | Consultar el ranking global | Usuario con cuenta, Invitado |
| **CU-13** | Entrar a la sala | Usuario con cuenta, Invitado |
| **CU-14** | Unirse a una sala | Usuario con cuenta, Invitado |
| **CU-15** | Crear una sala | Usuario con cuenta, Invitado |
| **CU-16** | Invitar jugadores a la sala | Usuario con cuenta |
| **CU-17** | Responder una invitación a una sala | Usuario con cuenta |
| **CU-18** | Salir de la sala | Usuario con cuenta, Invitado |
| **CU-19** | Iniciar la partida | Anfitrión |

## 2.4 Casos de uso que faltan por identificar

Ocho. Se enuncian con su actor; su descripción detallada es la etapa siguiente.

| ID | Caso de uso | Actor principal | Qué funcionalidad cubre |
|---|---|---|---|
| **CU-20** | Eliminar un amigo | Usuario con cuenta | FC-34 |
| **CU-21** | Expulsar a un jugador de la sala | Anfitrión | FC-35 |
| **CU-22** | Enviar un mensaje al chat | Usuario con cuenta, Invitado | FC-36 |
| **CU-23** | Reportar a un jugador | Usuario con cuenta | FC-38 |
| **CU-24** | Preparar la partida: colocar el caballero inicial y el rey | Jugador | FC-25, FC-26 |
| **CU-25** | Jugar un turno | Jugador en turno | FC-27, y FC-28 como inclusión |
| **CU-26** | Reconectar a una partida en curso | Jugador | FC-31 |
| **CU-27** | Volver a una partida reanudada | Jugador | Parte de FC-33 desde el lado del jugador. Un Usuario con cuenta se identifica con su cuenta y un Invitado presenta su pase de asiento: es **un flujo distinto dentro del mismo caso**, no otro caso |
| **CU-28** | Abandonar la partida | Jugador | FC-44 |

**Decisión de granularidad pendiente del equipo.** Las acciones que caben dentro de un turno
—colocar un caballero, moverlo, subirle un nivel, colocar una construcción, obtener una carta y
utilizar una carta— son **puntos de extensión de CU-25**. Que cada una sea o no un caso de uso
con nombre propio es una decisión de corte; lo que no admite discusión es que **son opcionales**
y por tanto nunca inclusiones.

## 2.5 Comportamientos del sistema que deben quedar cubiertos

No tienen actor humano: los dispara el servidor o el vencimiento de un plazo. Modelarlos como
casos de uso o como comportamiento del sistema es una decisión del equipo; **cubrirlos no es
opcional**.

| ID | Comportamiento | Funcionalidad |
|---|---|---|
| **S-1** | Cerrar las salas que quedaran abiertas al arrancar el servidor | — `DR-18a` |
| **S-2** | Cerrar el turno por agotarse los 90 segundos | FC-28 |
| **S-3** | Cerrar la ronda: puntuación parcial y colocador del rey | FC-29 |
| **S-4** | Terminar la partida y registrar el resultado | FC-30 |
| **S-5** | Retirar de la partida al jugador que agota su ventana de reconexión | FC-32 |
| **S-6** | Reanudar las partidas en curso tras una caída, o cerrarlas si nadie vuelve | FC-33 |
| **S-7** | Actualizar el ranking de la sala al terminar cada partida | FC-23 |
| **S-8** | Aplicar la sanción al alcanzarse el umbral, y el baneo permanente por reincidencia | FC-39 |
| **S-9** | Caducar la prohibición temporal al cumplirse su duración | FC-39 |
| **S-10** | Marcar inactivo al desconectado y traspasar la condición de anfitrión | FC-42 |
| **S-11** | Retirar de la sala al inactivo que agota el plazo, cerrando la sala si era el último | FC-43 |
| **S-12** | Registrar los eventos del sistema | — `RES-06` |

## 2.6 Relaciones

### Inclusiones — el comportamiento se ejecuta siempre

| Base | Incluye | Justificación |
|---|---|---|
| CU-14, CU-15, CU-17 | **CU-13 Entrar a la sala** | Es el comportamiento común e ineludible de unirse, crear y aceptar una invitación |
| CU-01 Iniciar sesión | **Comprobar el baneo permanente** | El baneo permanente impide iniciar sesión, luego se comprueba siempre |
| CU-13 Entrar a la sala | **Comprobar la sanción temporal** | La sanción temporal prohíbe entrar a las salas |
| CU-19 Iniciar la partida | **Comprobar la sanción temporal** | La sanción temporal prohíbe jugar |
| CU-19 Iniciar la partida | **CU-24 Preparar la partida** | La colocación inicial ocurre siempre antes del primer turno `DR-05` |
| CU-25 Jugar un turno | **Cerrar el turno y guardar el estado** | El estado se escribe en cada cierre de turno `P-27` |
| Terminar la partida | **Calcular la puntuación** | Se calcula siempre, al final de cada ronda y de la partida `DR-02` |
| CU-18 Salir de la sala · S-10 Marcado de inactividad | **Traspasar la condición de anfitrión** | Mismo efecto y **misma regla** —pasa al siguiente del orden guardado— con dos disparos distintos |

### Extensiones — el comportamiento es opcional

| Base | Se extiende con | Justificación |
|---|---|---|
| CU-01 Iniciar sesión | CU-02, CU-03, **CU-04** | Las tres son salidas opcionales de la misma ventana |
| CU-06 Modificar el perfil | CU-05, CU-07 | Alcanzables desde «Configurar cuenta» |
| CU-08 Enviar una solicitud | **CU-20 Eliminar un amigo** | La lista de amigos del caso base ofrece esa opción |
| CU-10 Historial | CU-11 Detalle de una partida | Se elige una partida de la tabla |
| CU-14 Unirse a una sala | CU-15 Crear una sala | Salida opcional de la ventana de salas |
| CU-13 Entrar a la sala | CU-16, CU-18, CU-19, **CU-21**, **CU-22** | Todas son opciones de la ventana de sala, y ninguna es obligatoria |
| CU-25 Jugar un turno | Las acciones con coste en puntos de acción | El jugador gasta sus puntos como quiera, o no los gasta `R 3.2` |
| CU-26 Reconectar | Retirar de la partida al jugador | Solo ocurre si vence la ventana `R 5.2` |
| CU-22 Enviar un mensaje al chat | **CU-23 Reportar a un jugador** | El motivo declarado son los mensajes ofensivos, y reportar es opcional |
| CU-23 Reportar | **Aplicar la sanción** | Solo al alcanzarse la quinta ocasión |
| Aplicar la sanción | **Banear permanentemente** | Solo al cuarto cruce del umbral |
| Terminar la partida | **Aplicar la sanción diferida** | Solo si el umbral se cruzó durante esa partida |

### Lo que no se declara como relación

El borrado del archivo del avatar dentro de la eliminación de cuenta, la generación del
identificador temporal del invitado, la asignación de color y la búsqueda por nombre de usuario
**son pasos de una operación**, no objetivos de un actor. **Mostrar el chat, el ranking de sala y
el color forman parte de CU-13**, no son casos aparte.

*Decisión de corte abierta al equipo, no obligatoria:* CU-18 y CU-21 comparten «retirar de la
lista y avisar a los demás»; puede factorizarse o describirse por separado.

---

# 3. Entidades del sistema

**Principio que ordena todo este apartado.** Se guarda en la base de datos lo que debe
sobrevivir a la sala y al reinicio. **De los invitados no se guarda ninguna fila** `P-08`, y por
eso todo lo que los incluye —miembros de la sala, anfitrión, ranking de sala y chat— vive en la
memoria del servidor: una tabla que solo contuviera a los jugadores con cuenta nunca sabría si
una sala está vacía, que es la condición que decide si la sala sigue existiendo.

## 3.1 Entidades permanentes

### `Cuenta`
**Propósito:** identidad persistente del jugador. Absorbe el perfil `D-22`.

| Atributo | Tipo | Notas |
|---|---|---|
| Identificador | entero generado | Clave primaria. Es el único identificador de persona que puede aparecer en el registro de eventos `RES-06` |
| Nombre de usuario | texto de 3 a 20, letras, dígitos y guion bajo | **Único sin distinguir mayúsculas.** Credencial, nombre visible y único criterio de búsqueda `P-16` |
| Correo | texto | **Único sin distinguir mayúsculas.** Recuperación de acceso y destino de invitaciones `D-13` |
| Hash de la contraseña | texto | BCrypt con factor de coste configurable. **La contraseña nunca se guarda ni se registra** `D-03` |
| Referencia del avatar | texto, opcional | Ruta donde el servidor dejó la imagen. **El archivo vive fuera de la base** |
| Fecha de alta | marca de tiempo con zona | `D-04` |

**Relaciones:** con `Amistad` como solicitante y como destinataria · con `Invitación a sala`
como quien invita y como invitada · con `Participación` · con `Reporte` como autora y como
reportada · con `Sanción` · con `Código de recuperación`. **Al eliminarse la cuenta, todo lo
suyo se va en cascada.**

### `Amistad`
**Propósito:** solicitud pendiente y amistad aceptada entre dos cuentas.

| Atributo | Tipo | Notas |
|---|---|---|
| Solicitante y destinataria | referencias a `Cuenta` | Clave primaria conjunta |
| Estado | *pendiente* · *aceptada* | La rechazada **se borra**, no se conserva `P-15` |
| Fecha de solicitud | marca de tiempo | |
| Fecha de respuesta | marca de tiempo, opcional | Vacía mientras está pendiente |

**Restricciones:** una sola fila por pareja —el par invertido es el mismo par—; nadie es amigo de
sí mismo; **no existe el bloqueo** `P-15`.

### `Sala`
**Propósito:** lugar de encuentro y referencia de las partidas que se juegan en ella.

| Atributo | Tipo | Notas |
|---|---|---|
| Identificador | entero generado | Clave primaria |
| Código | texto de 4 caracteres alfanuméricos | **Único solo entre las salas abiertas**, para poder ser corto y reutilizarse |
| Visibilidad | *pública* · *privada* | Determina únicamente si la sala aparece en el listado. Se fija al crearla y no cambia |
| Fecha de creación | marca de tiempo | |
| Fecha de cierre | marca de tiempo, opcional | Vacía mientras está abierta. **La sala vacía se cierra, no se borra** `D-23` |

**Relaciones:** con `Invitación a sala` —ninguna sobrevive a su sala— y con `Partida`, que impide
borrarla. **No guarda miembros, ni anfitrión, ni ranking, ni chat:** todo eso es estado de
memoria `D-26`.

### `Invitación a sala`
**Propósito:** invitación pendiente a una sala, por los dos canales.

| Atributo | Tipo | Notas |
|---|---|---|
| Identificador | entero generado | Clave primaria |
| Sala | referencia a `Sala` | Se borra con ella |
| Quien invita y quien es invitada | referencias a `Cuenta` | Distintas. **Siempre dirigida a una cuenta** `P-22` |
| Canal | *en el juego* · *correo* | El correo lleva el código de la sala |
| Fecha de creación | marca de tiempo | |

**Restricciones:** **una sola invitación pendiente por sala y destinatario**; **existe solo
mientras está pendiente** —se borra al responderse o al cerrarse la sala `D-24`—; **no caduca**
`P-39`.

### `Partida`
**Propósito:** una partida, en curso o archivada.

| Atributo | Tipo | Notas |
|---|---|---|
| Identificador | entero generado | Clave primaria |
| Sala | referencia a `Sala` | Obligatoria: toda partida nace en una sala `P-17` |
| Estado | *en curso* · *terminada* | |
| Número de jugadores | entero de 2 a 4 | Cuántos se sentaron. **No es deducible**: los invitados no dejan fila y una cuenta borrada se lleva la suya `D-25` |
| Inicio | marca de tiempo | |
| Fin | marca de tiempo, opcional | Vacío mientras está en curso |
| Motivo de fin | *completada* · *abandonada* · *interrumpida* | Vacío mientras está en curso |

**Coherencia obligatoria:** en curso no tiene fin ni motivo; terminada tiene los dos. **La
secuencia de jugadas no se guarda nunca.**

### `Participación`
**Propósito:** un jugador **con cuenta** dentro de una partida, y su resultado.

| Atributo | Tipo | Notas |
|---|---|---|
| Partida y puesto de mesa | referencia y entero de 1 a 4 | Clave primaria conjunta. **El puesto es el identificador del jugador dentro de la partida** |
| Cuenta | referencia a `Cuenta` | Obligatoria, y única dentro de la partida |
| Puntuación final | entero ≥ 0, opcional | Vacía hasta que la partida termina |
| Puesto final | entero de 1 a 4, opcional, único en la partida | 1 es el ganador. **Se guarda, no se deriva**: el desempate lo resuelve el dominio |
| Retirado | booleano | Se marca al agotar la ventana de reconexión. **La partida sigue contando para el ranking** `P-07` |

**Los invitados no generan fila**, así que los puestos guardados **pueden tener huecos**: si
ganó un invitado, ninguna fila tendrá el puesto 1.

### `Reporte`
**Propósito:** dejar constancia de un mensaje ofensivo para poder acumularlo hacia una sanción.

| Atributo | Tipo | Notas |
|---|---|---|
| Identificador | entero generado | Clave primaria |
| Quien reporta | referencia a `Cuenta` | **Solo cuentas reportan** |
| Reportado | referencia a `Cuenta` | Distinto del anterior. **Solo se reporta a cuentas** |
| Sala | referencia a `Sala` | Dónde se dijo el mensaje |
| Partida | referencia a `Partida`, opcional | Vacía si el mensaje se dijo en la sala fuera de toda partida |
| Texto reportado | texto | **Copia del mensaje.** El chat no se conserva, así que sin esta copia el reporte se quedaría sin sustento |
| Fecha | marca de tiempo | |

**Se elimina con la cuenta.** La sala y la partida son lo que permite contar **ocasiones**, que
es la unidad del umbral.

### `Sanción`
**Propósito:** prohibición vigente o pasada sobre una cuenta.

| Atributo | Tipo | Notas |
|---|---|---|
| Identificador | entero generado | Clave primaria |
| Cuenta | referencia a `Cuenta` | Se elimina con ella |
| Nivel | entero de 1 a 3 | Determina la duración según el catálogo |
| Permanente | booleano | El cuarto cruce del umbral |
| Instante del umbral | marca de tiempo | Cuándo se alcanzó la quinta ocasión |
| Entrada en vigor | marca de tiempo | **Distinta del anterior**: si el umbral se cruza durante una partida, la sanción entra en vigor al terminarla |
| Fin | marca de tiempo, opcional | Vacío si es permanente |

**El número de sanciones temporales previas no se guarda:** se cuenta sobre las filas de la
cuenta.

### `Código de recuperación`
**Propósito:** probar que quien pide cambiar la contraseña es el titular del correo.

| Atributo | Tipo | Notas |
|---|---|---|
| Cuenta | referencia a `Cuenta` | Se elimina con ella |
| Código | texto | Enviado al correo de la cuenta |
| Fecha de creación | marca de tiempo | El código **vale cinco minutos** desde este instante |

**Se elimina al usarse**, y **caduca por sí solo a los cinco minutos** de haberse generado.

## 3.2 Catálogos de parámetros

Son **de solo lectura** y se cargan por script: `D-31` descarta el rol de administración, así que
**ninguno tiene caso de uso de mantenimiento**. Existen para poder cambiar números sin tocar el
dominio ni recompilar.

| Catálogo | Clave | Contenido |
|---|---|---|
| **Turnos por ronda** | número de jugadores + ronda | Turnos **por jugador** en esa ronda `R 1.3` |
| **Construcciones por turno** | número de jugadores + ronda + turno | Construcciones que recibe cada jugador `R 1.3` |
| **Coste de las acciones** | código de acción | Puntos de acción que cuesta `R 3.2` |
| **Niveles de sanción** | nivel 1 a 3 | Duración de la prohibición: 5 horas, 1 día y 3 días |
| **Parámetros del sistema** | código de parámetro | Umbral de ocasiones para sancionar (5) · longitud máxima de un mensaje (200) · mensajes conservados por sala (200) · periodo del latido y número de latidos perdidos para dar por caída una conexión · plazo de inactividad en sala (3 minutos) |

## 3.3 Consulta derivada

**Ranking global.** No almacena nada propio: se calcula sobre las partidas terminadas con
resultado. Ordena por **victorias**, después por **puntos acumulados** y después por **menos
partidas jugadas** `DR-15`. Entran las partidas jugadas con invitados y aquellas en las que el
jugador fue retirado; **quedan fuera las partidas interrumpidas y los invitados**, que no
existen en la base.

## 3.4 Entidades en memoria del servidor

No son tablas, pero son entidades del sistema y el diseño debe contemplarlas.

| Entidad | Contenido | Ciclo de vida |
|---|---|---|
| **Sala viva** | Lista **ordenada** de participantes, anfitrión, colores en uso, chat y ranking de sala | Nace al crear la sala; muere al cerrarse. **No se reanuda tras una caída** `P-32` |
| **Participante de sala** | Identidad —cuenta o invitado—, color asignado, orden de entrada, estado *activo* o *inactivo*, marca de invitado | Mientras esté en la sala |
| **Invitado** | Alias libre e **identificador temporal** generado por el servidor, que es también su **pase de asiento** | Muere con la sesión. El pase se copia al estado de la partida al iniciarla |
| **Mensaje de chat** | Autor, texto e instante | Ventana deslizante de 200 mensajes por sala. **No se guarda en la base** |
| **Ranking de sala** | Por participante, puntos acumulados y victorias en esa sala | Se acumula al terminar cada partida y **muere con la sala** `D-29`. **Incluye a los invitados**, por eso ninguna consulta podría producirlo |
| **Partida viva** | Tablero, castillos, caballeros, rey, cartas obtenidas y usadas, puntuación acumulada, puestos, colores, reloj del turno y puntos de acción restantes | Mientras la partida está en curso. **El servidor es la única autoridad sobre este estado** |

---

# 4. Reglas y elementos definidos del sistema

## 4.1 Restricciones técnicas y condiciones del proyecto

| ID | Regla |
|---|---|
| `RES-01` | El sistema se escribe en **C#** |
| `RES-02` | La comunicación cliente–servidor usa **WCF con callbacks duplex sobre net.tcp**, con llamadas asíncronas |
| `RES-03` | **Prohibido el stack web y los WebSockets**: quedan excluidos SignalR, gRPC y REST |
| `RES-04` | **Se evalúa el código, no el acabado visual** |
| `RES-05` | Cuatro capas: cliente → servicios → dominio + persistencia. **El dominio no depende de nadie y el cliente no decide resultados** |
| `RES-06` | El registro de eventos se hace **solo con log4net**; **prohibido registrar contraseñas, tokens y datos personales** |
| `RES-07` | Régimen de pruebas: nombrado fijado, tres bloques, una sola ejecución, sin lógica en la prueba |
| `RES-08` | `DesktopGL`, nunca `WindowsDX`; todos los proyectos en `net10.0`; **nunca** `PublishTrimmed` ni `PublishAot` |
| `RES-09` | `SecurityMode.None` en el binding, porque el modo por defecto no funciona entre macOS y Linux |
| `RES-10` | Mayúsculas exactas en los nombres de archivo |
| `ORG-01`…`ORG-05` | Equipo de dos personas con cinco papeles cruzados, trabajando en macOS y Linux, con **una sola máquina Windows**, y **las dos deben poder correr servidor y clientes en la suya** |
| `DR-33` | El motor de base de datos es **PostgreSQL** |
| `DR-16` | El catálogo de atributos de calidad del proyecto es **ISO/IEC 25010** |
| `DR-10` | El sistema **no limita** el número de partidas simultáneas |

## 4.2 Identidad, cuenta y perfil

- El **nombre de usuario** tiene de 3 a 20 caracteres entre letras, dígitos y guion bajo, y es
  **único sin distinguir mayúsculas**. Es credencial, nombre visible y **único criterio de
  búsqueda** de jugadores `P-16`.
- El **correo** es único y obligatorio. Sirve para recuperar el acceso y para recibir
  invitaciones `D-13`.
- La **contraseña** tiene de **8 a 20 caracteres** e incluye letras mayúsculas y minúsculas,
  números y caracteres especiales `CU-02`, `CU-03`.
- La contraseña se guarda **solo como hash BCrypt**, con factor de coste configurable, y **nunca
  se registra** `D-03`, `RES-06`.
- **Recuperar el acceso** consiste en recibir un **código** en el correo, introducirlo y
  establecer una contraseña nueva.
- El **perfil** son el nombre de usuario y el **avatar**: imagen **PNG o JPG de hasta 5 MB**,
  validada en la capa de servicios; **el archivo lo guarda el servidor y la base solo conserva su
  referencia** `P-43`. Si el archivo desaparece del disco, la aplicación muestra el avatar por
  defecto.
- **Eliminar la cuenta** es irreversible y arrastra en cascada perfil, amistades, solicitudes,
  invitaciones, participaciones, reportes y sanciones; **borra también el archivo del avatar** y
  **las partidas que queden sin ninguna participación** `D-28`, `PER §7.5`.
- **No puede eliminarse una cuenta que participa en una partida en curso** `P-42`. Lo comprueba
  la operación, no el motor.
- El **invitado** elige **cualquier alias**, sin formato ni unicidad, y el servidor le da un
  **identificador temporal**. En pantalla lleva la **marca de invitado junto al nombre**, y **dos
  invitados con el mismo alias se distinguen por su color**.
- El invitado **juega, crea salas y puede ser anfitrión**; **no tiene** historial, ranking
  global, amigos ni invitaciones; **no invita, no reporta y no puede ser reportado**. De él **no
  queda ninguna fila** `P-08`.
- El **idioma** de la interfaz es **español o inglés**, se elige **por equipo** y no se guarda en
  la cuenta.
- La opción **«Salir»** del menú principal **cierra la aplicación**.

## 4.3 Amistades

- La búsqueda para agregar es **solo por nombre de usuario** `P-16`.
- Entre dos cuentas existe **una sola relación**: no pueden coexistir la solicitud en un sentido
  y en el otro, y **nadie es amigo de sí mismo**.
- La solicitud **rechazada se borra** sin dejar registro. **No existe el bloqueo** `P-15`.
- La solicitud pendiente **se conserva aunque el destinatario esté desconectado**.
- **Eliminar una amistad es simétrico** —desaparece de las dos listas— y **al eliminado no se le
  avisa**. Nada impide volver a enviar una solicitud después.

## 4.4 Salas

- Una sala admite **como máximo cuatro personas**, **contando también a los inactivos**, y **un
  jugador no puede estar en dos salas a la vez** `P-34`, `P-33`.
- El **código** es **alfanumérico de cuatro caracteres** y **único solo entre las salas
  abiertas**; si el sorteado ya está en uso, se vuelve a generar. Como se teclea desde un correo,
  el alfabeto **excluye los caracteres que se confunden**.
- **Pública o privada cambia una sola cosa: si la sala aparece en el listado.** A una sala
  privada se entra **por código o por invitación**, que son las únicas vías que existen.
- El **anfitrión** es quien crea la sala; si sale, la condición pasa **al siguiente jugador en el
  orden en que están guardados**, sin más criterio. **Puede ser un invitado** `P-41`.
- **Solo el anfitrión expulsa e inicia la partida.**
- **La expulsión no puede hacerse con una partida en curso**, **se avisa al expulsado** y **este
  puede volver a entrar con el código**: no hay veto.
- **No se puede salir de la sala mientras se juega** `P-35`.
- **La sala vacía se cierra, no se borra**: libera su código y elimina sus invitaciones
  pendientes; sus partidas terminadas permanecen `D-23`. **Al arrancar el servidor se cierran
  todas las salas que quedaran abiertas** `DR-18a`.
- **El color** se asigna **en la sala**, hay cuatro, el de quien sale vuelve al conjunto, y el
  que cada jugador tiene al empezar es el que **identifica sus piezas en la partida**.
- **Inactividad, en dos fases.** El servidor detecta la desconexión **por el latido**; marca al
  jugador **inactivo**, que **sigue en la sala ocupando su plaza** pero **no entra a la partida**,
  y **traspasa la condición de anfitrión** si la tenía. **Al vencer el plazo de inactividad se le
  retira de la sala**, liberando su plaza; si era el último, la sala queda vacía y se cierra.
  **Este plazo no se aplica a quien está dentro de una partida en curso**, que se rige por la
  regla de reconexión.

## 4.5 Invitaciones

- La invitación va **siempre dirigida a una cuenta** `P-22`. **Un invitado no puede invitar.**
- **Una sola invitación pendiente por sala y destinatario**, y **se conserva aunque el
  destinatario esté desconectado** `P-21`.
- **No caduca** `P-39`: vive mientras viva la sala.
- **Se borra al responderse** —aceptada o rechazada— **o al cerrarse la sala**, y eso es lo que la
  hace de un solo uso `D-24`.
- Dos canales: **dentro del juego** y **por correo**. El correo **lleva el código de la sala**, y
  el canje comprueba que **quien lo presenta es la cuenta destinataria**.

## 4.6 Chat

- **Hay un solo chat: el de la sala**, y la partida lo muestra. **Se conserva de la sala a las
  partidas que se jueguen en ella.**
- **No se guarda en la base de datos**: vive en memoria, **muere con la sala** y **una caída del
  servidor se lo lleva**.
- **Quien entra tarde ve los mensajes anteriores**, hasta donde llegue la ventana conservada.
- **Escribir consume el tiempo del turno**: el reloj no se detiene.
- Límites: **200 caracteres por mensaje** y **200 mensajes conservados por sala**, en ventana
  deslizante.
- **Los invitados participan en el chat** igual que las cuentas.

## 4.7 Reportes y sanciones

- Se reporta **por mensajes ofensivos**. **Solo una cuenta puede reportar, y solo a una cuenta**:
  el régimen **no alcanza a los invitados**.
- **El reporte guarda una copia del mensaje**, porque el chat no se conserva.
- **Lo que se acumula son ocasiones, no reportes sueltos:** cada **partida distinta** en la que a
  un jugador se le reporta cuenta **una**, y **todos los reportes hechos en una sala fuera de
  partida cuentan también una**.
- **A las cinco ocasiones se aplica una prohibición temporal**, y **el contador de ocasiones
  vuelve a cero**.
- **Escalera de prohibiciones: la primera dura 5 horas, la segunda 1 día y la tercera 3 días.**
  **El cuarto cruce del umbral es el baneo permanente.** El contador de prohibiciones **no** se
  reinicia.
- **La prohibición temporal impide entrar a las salas y jugar**, pero **no impide iniciar
  sesión**. **El baneo permanente impide también iniciar sesión.**
- **Si el umbral se cruza durante una partida, la sanción se aplica al terminarla**: el jugador
  termina lo que está haciendo, y **la duración empieza a contar cuando la sanción entra en
  vigor**, no cuando se cruzó el umbral.
- **Reportes y sanciones se eliminan con la cuenta.**
- **No hay revisión humana**: las sanciones son automáticas por umbral y **no existe rol de
  administración** `D-31`.

## 4.8 La partida

### Estructura

- **De 2 a 4 jugadores**, y **solo cuentan los activos**. **Nadie puede incorporarse una vez
  iniciada** `R 1.1`.
- **Tablero de 8 × 8, con distribución fija en todas las partidas** `R 1.2`.
- **Tres rondas siempre.** Cada ronda tiene, **por jugador**, los turnos que dicte la carta
  resumen: con 2 jugadores 4/4/4; con 3 y con 4 jugadores 4/3/3 `DR-01`, `R 1.3`.
- **Un turno es el periodo de un solo jugador y dura como máximo 90 segundos.** Si se agota, el
  turno termina y pasa al siguiente `R 5.1`.
- **El puesto de mesa** se asigna según el orden en que los jugadores se acomodan al empezar, y
  **es el identificador del jugador dentro de la partida** `DR-24`.
- **Antes del primer turno cada jugador coloca un caballero sobre una torre**, en orden de puesto
  de mesa `DR-05`. **Cada colocación dispone del mismo tiempo que un turno, 90 segundos, y se rige
  por las mismas reglas**: si se agota, se pasa al siguiente, y si el jugador pierde la conexión
  se le aplica la misma ventana de reconexión.

### Elementos

- **Cinco caballeros por jugador** `R 2.1`.
- **Castillo = conjunto de torres contiguas.** Los castillos **nacen y quedan delineados por las
  construcciones iniciales**: **no se crean nuevos ni se unen dos** `DR-08`.
- **Altura máxima de una torre: nivel 3** `DR-03`.
- **Movimiento de caballeros:** de casilla a casilla y entre niveles; **se sube un solo nivel por
  movimiento** y se baja cualquier cantidad; **no puede ocuparse una torre que ya tenga un
  caballero**; y **todo movimiento ocurre dentro del mismo castillo** `DR-22`.
- **Construcciones:** se colocan **sobre un castillo existente**, **solo ortogonalmente**; las
  torres pueden tocarse en diagonal; **nunca puede unirse ortogonalmente una torre de un castillo
  con la de otro** `R 2.2`. Cada jugador coloca como mucho las que le asigne la carta resumen de
  esa ronda y ese turno.
- **El rey** se sitúa **siempre sobre una torre**, nunca sobre el tablero. En la primera ronda lo
  coloca **el último puesto de mesa**; en las siguientes, **el jugador con menor puntuación** y,
  si hay empate, **uno al azar entre los empatados** `R 2.3`, `DR-24`.

### Puntos de acción y costes

- **5 puntos de acción por turno**, que **se reinician al empezar cada turno** y **no se acumulan**
  entre turnos ni entre rondas `R 3.1`.
- Costes `R 3.2`: **colocar un nuevo caballero 2** · **mover un caballero 1** · **subir un nivel
  1** · **colocar una construcción 1** · **obtener una carta 1** · **utilizar una carta ya
  obtenida 0**.

### Cartas de acción

- **Ocho cartas**, **el mismo mazo para todos**, que **se obtienen al azar** y no están
  disponibles desde el principio `R 2.4`, `DR-06`.
- **Utilizar una carta obtenida no cuesta nada**; lo que cuesta son **las acciones que habilita**.

| Carta | Efecto |
|---|---|
| **1** | Otorga **6** puntos de acción ese turno, en sustitución de los 5 |
| **2** | Otorga **7** puntos de acción ese turno, en sustitución de los 5 |
| **3** | Mueve un caballero **del nivel 1 al nivel 3 de una torre**. Es la excepción a subir un solo nivel, y **no cuesta puntos de acción** |
| **4** | Permite que un caballero propio, al desplazarse sobre el nivel del tablero, **pase por encima de una casilla ocupada** |
| **5** | **Mueve un nivel de torre** de un castillo o torre a otro |
| **6** | **Cambia de casilla** a un caballero |
| **7** | Permite recorrer el castillo **de una torre a otra sin importar las casillas ni el nivel**, y **subirse al castillo desde el tablero**. **Se pagan el movimiento y los niveles que suba**: 1 punto por moverse y 1 por cada nivel ganado; bajar no cuesta |
| **8** | **Elimina dos niveles** de torre, uno a uno |

- **Cartas 1 y 2:** pueden usarse **en cualquier momento del turno**, incluso después de haber
  gastado puntos, y **nunca las dos a la vez** `DR-26`.
- **Cartas 5 y 8:** el nivel afectado **no debe estar ocupado** por un caballero; la operación
  **no puede partir el castillo de origen ni separar sus torres**; **no puede retirarse el nivel 1
  de una torre de una sola altura**; y **mover un nivel no puede unir dos castillos** `DR-07`.
- **Cada carta obtenida y no utilizada vale 1 punto al final de la partida** `R 4.3`.

### Puntuación y final

- **La puntuación se calcula al final de cada ronda** —lo que designa quién coloca el rey en la
  siguiente— **y otra vez al final de la partida** `DR-02`.
- **El corte de ronda no retira nada del tablero** `DR-23`: los castillos crecen durante toda la
  partida.
- **Castillos:** un jugador puntúa un castillo **solo si tiene un único caballero propio en él**,
  y suma **superficie del castillo × nivel de la torre donde está ese caballero** `R 4.1`.
- **Rey:** otorga **5, 10 y 15 puntos** en las rondas 1, 2 y 3 a **quien tenga un caballero en su
  mismo castillo y en su mismo nivel**; puede otorgarlos a más de un jugador `R 4.2`.
- **Ganador:** mayor puntuación total; si hay empate, **mayor número de castillos en los que
  puntuó**; si persiste, **el puesto de mesa más bajo** `DR-15`.
- **La partida termina** al jugarse las tres rondas —*completada*—, o **cuando quedan menos de
  dos jugadores**, en cuyo caso **gana el que queda** —*abandonada*—. Si tras una caída no vuelve
  nadie, queda *interrumpida*, **sin puestos ni puntuaciones**.
- **Un jugador puede abandonar la partida** en cualquier momento, rindiéndose desde el menú de la
  ventana de la partida, sea o no su turno. El abandono tiene **el mismo efecto que el retiro por
  ventana de reconexión agotada**: sus caballeros salen del tablero, sus construcciones se quedan,
  deja de contar para el resultado y para «el de menor puntuación», y la partida sigue contando
  para su ranking.
- **Al terminar la partida, los jugadores vuelven a la sala en la que se jugó**, que sigue
  abierta, porque la invitación es **a la sala** y en una sala pueden jugarse varias partidas
  `P-26`. Si esa sala ya no existe —el caso de una partida reanudada, porque las salas no se
  reanudan—, vuelven al menú principal.

## 4.9 Conexión, reconexión y reanudación

- **El servidor es la única autoridad sobre el estado**: el cliente no decide resultados
  `RES-05`.
- **El reloj del turno es del servidor**, y **la latencia y el proceso van dentro de los 90
  segundos** del jugador.
- **La pérdida de conexión se detecta por latido**: el cliente envía uno cada cierto periodo y el
  servidor da la conexión por perdida tras un número de latidos sin recibir. **Ese instante es el
  que inicia la ventana de reconexión y el marcado de inactividad.**
- **Ventana de reconexión** `R 5.2`: si el jugador cae **en su propio turno**, dispone del tiempo
  que le quedaba; si cae **en el turno de otro**, dispone de **90 segundos**. **El reloj sigue
  corriendo durante la desconexión** `DR-20`.
- **Al reconectar dentro de la ventana, el jugador retoma su turno en el estado exacto** en que
  lo dejó, con sus puntos no gastados y sus acciones ya confirmadas `DR-30`.
- **Si agota la ventana, queda retirado de la partida:** deja de contar para «el de menor
  puntuación» y para el resultado, **sus construcciones se quedan en el tablero y sus caballeros
  se retiran de él** `DR-25`, `DR-32`. La partida continúa con la configuración del número
  inicial de jugadores `DR-09`, y **sigue contando para el ranking** `P-07`.
- **Con el servidor caído los 90 segundos no corren** `DR-13`, y **el sistema indica en la
  interfaz que no hay conexión con el servidor** mientras dura la caída.
- **Si el servidor vuelve y el jugador no ha salido del juego, su cliente se reconecta y la
  partida se reanuda sin ninguna intervención suya.** Si sí había salido, al volver a entrar **se
  le indica que su partida sigue en curso** y elige volver a ella o abandonarla.
- **Reanudación:** al arrancar, el servidor busca las partidas en curso y **las reanuda cuando
  han vuelto al menos dos jugadores**; **los invitados cuentan**, porque vuelven **presentando su
  pase de asiento**. **El turno interrumpido se juega de nuevo con 90 segundos completos** y se
  descarta entero, incluidas las cartas obtenidas o usadas en él `P-28`.
- Se espera un plazo desde el arranque —**3 minutos**, valor de pruebas y configuración de la
  aplicación `D-27`—. Si vuelve **uno solo**, la partida termina como *abandonada* y gana el que
  queda; si **no vuelve nadie**, queda *interrumpida*.
- **Las salas no se reanudan** `P-32`, y con ellas se pierden su chat y su ranking.

## 4.10 Persistencia

- **Permanente:** cuentas, amistades, salas, partidas terminadas, participaciones, reportes y
  sanciones.
- **Temporal en la base:** invitaciones pendientes, solicitudes de amistad pendientes, códigos de
  recuperación y **el estado de la partida en curso**.
- **En memoria, nunca en la base:** quién está en cada sala, quién es el anfitrión, los colores,
  el chat y el ranking de sala; el estado de conexión, el reloj del turno y los puntos de acción
  restantes.
- **Derivado:** el ranking global, el número de jugadores de una sala y la duración de una
  partida.
- **No se guarda en absoluto:** la secuencia de jugadas, las sesiones y los tokens, el desglose
  de puntos por ronda y los mensajes de chat.
- **El estado de la partida se escribe al empezarla y al cerrar cada turno** `DR-18b`, `P-27`, y
  **se elimina al terminar** `D-21`. **No guarda el reloj, los puntos restantes ni las
  construcciones pendientes**, porque el estado guardado es siempre una **frontera de turno**
  `P-28`.
- **Una partida jugada solo por invitados no deja ninguna participación**: existe mientras está en
  curso, para poder reanudarse, y **se elimina al terminar** `PER §7.4`.

## 4.11 Fuera de alcance

**No pertenecen al sistema y no deben aparecer:** espectadores, notificaciones más allá de las
invitaciones, logros, temporadas, bloqueo de jugadores, historial de invitaciones, rol de
administración `D-31`, cartas maestras `DR-27`, y cualquier estadística distinta de las que
calculan los rankings.

---

# 5. Estado final del análisis

## 5.1 Cómo queda definido el sistema

**Torres es un juego de mesa por turnos para dos a cuatro jugadores, cliente–servidor, con el
servidor como única autoridad del estado.** Alrededor de la partida hay una capa social —cuentas,
perfil, amistades, salas, invitaciones, chat, clasificaciones— y una capa de convivencia
—reportes y sanciones automáticas—.

El sistema tiene **cuarenta y tres funcionalidades** repartidas en siete bloques: identidad y
cuenta, amistades, salas e invitaciones, historial y clasificaciones, la partida, chat y
sanciones, y presencia.

De ellas, **veintinueve son casos de uso con actor**, **seis son pasos o inclusiones dentro de
otro caso de uso**, y **ocho son comportamientos que dispara el sistema o el vencimiento de un
plazo**. Los veintinueve se agrupan en **veintisiete casos de uso**, porque dos de ellos reúnen
más de una funcionalidad.

**Los casos de uso son veintiocho.** Los veintisiete identificados en este documento más
`CU-28 Abandonar la partida`, que nace de la decisión de que los jugadores puedan rendirse. Los
ocho que faltan son eliminar un amigo, expulsar de la sala, enviar un mensaje al chat, reportar a
un jugador, y los cuatro de la partida: preparar la partida, jugar un turno, reconectar y volver
a una partida reanudada. **La partida es la zona con menos cobertura**: hoy los casos de uso
identificados terminan en el instante en que se inicia.

Además, **doce comportamientos del sistema** deben quedar cubiertos aunque no tengan actor
humano: cierre de turno por tiempo, cierre de ronda, fin de partida, retiro por reconexión
agotada, reanudación, ranking de sala, aplicación y caducidad de sanciones, marcado y retirada
por inactividad, cierre de salas al arrancar y registro de eventos.

**Tres identidades, dos roles y el sistema.** Usuario sin cuenta, Usuario con cuenta e Invitado
son identidades; Anfitrión y Jugador son roles que se asumen dentro de una sala y de una partida;
el sistema dispara por sí solo los comportamientos que no tienen actor humano. El invitado es
una **identidad cerrada**, no una cuenta recortada: juega y puede mandar en una sala, pero no
deja rastro y queda fuera del régimen de amistades, invitaciones, reportes y ranking global.

**Diez entidades permanentes o temporales en la base**, cinco catálogos de parámetros de solo
lectura y una consulta derivada; y **seis entidades que viven en la memoria del servidor**. La
línea que las separa no es de comodidad: **de los invitados no se guarda nada**, así que todo lo
que los incluye —miembros, anfitrión, chat y ranking de sala— no puede vivir en la base sin
quedar incompleto.

**Tres decisiones estructurales gobiernan el resto del diseño:**

1. **El servidor decide todo.** El reloj, las reglas y el estado son suyos; el cliente muestra.
2. **Lo que incluye invitados no se persiste.** De ahí que sala, anfitrión, chat, colores y
   ranking de sala sean memoria, y que la sala vacía se cierre en vez de borrarse.
3. **El estado guardado de una partida es siempre una frontera de turno.** Por eso un turno
   interrumpido se rejuega entero y no hace falta guardar reloj ni puntos restantes.

## 5.2 Lo que queda listo para la etapa siguiente

- **Las reglas del juego no tienen ninguna pregunta abierta.** Estructura, elementos, cartas,
  costes, puntuación, desempate y tiempos están cerrados.
- **El régimen de sanciones está definido de punta a punta**: qué se cuenta, con qué umbral, con
  qué escalera de duraciones, qué prohíbe cada nivel, cuándo entra en vigor y dónde viven sus
  números.
- **El modelo de datos está determinado**, incluidas las tres entidades que la capa de convivencia
  y la recuperación de acceso requieren, y los dos catálogos de parámetros nuevos.
- **Las relaciones entre casos de uso están justificadas una a una**, distinguiendo lo que ocurre
  siempre —inclusión— de lo que ocurre solo si el actor lo elige o si se cumple una condición
  —extensión—.

## 5.3 Lo único que sigue sin poder determinarse

**Dos puntos**, los dos de proyecto y ninguno de análisis. Los huecos de los casos de uso se
cerraron el 10 sep 2026 y están integrados en `Documento-Casos-de-Uso-Torres.md` v4.0.

| # | Sin determinar | A qué afecta |
|---|---|---|
| 1 | **La tecnología de envío de correo.** Qué hace el sistema si el envío falla **ya está decidido**: informa al usuario con un mensaje, no reintenta, y ni el código de recuperación ni la invitación se pierden | Recuperación de acceso e invitación por correo |
| 2 | **La decisión de seguridad del transporte.** Con `SecurityMode.None`, la credencial, los mensajes de chat y el pase de asiento del invitado **viajan sin cifrar** | Es una decisión de proyecto pendiente, no un hueco de análisis |

## 5.4 Valores fijados en este análisis

Están en vigor y el diseño debe tomarlos como dados. Todos viven en los catálogos de parámetros,
de modo que cambiarlos no toca el dominio.

| Valor | Cifra |
|---|---|
| Umbral de ocasiones para sancionar | **5** |
| Duración de las prohibiciones | **5 horas · 1 día · 3 días**, y el cuarto cruce permanente |
| Longitud máxima de un mensaje de chat | **200 caracteres** |
| Mensajes conservados por sala | **200**, en ventana deslizante |
| Plazo de inactividad antes de retirar de la sala | **3 minutos** |
| Plazo de reanudación tras una caída | **3 minutos** `P-31` |
| Longitud del código de sala | **4 caracteres alfanuméricos**, sin caracteres confundibles |
| Detección de caída | **Latido con periodo y número de latidos perdidos declarados** |
| Factor de coste de BCrypt | **11**, configurable |
| Caducidad del código de recuperación | **5 minutos** desde que se genera |
| Tiempo de cada colocación previa al primer turno | **90 segundos**, el mismo que un turno |

## 5.5 Qué sigue

Con esta base cerrada, la etapa siguiente es **describir en detalle los veintisiete casos de
uso** —los diecinueve existentes, completados con lo que este documento fija, y los ocho nuevos—
y **decidir el corte de las acciones dentro del turno**, que es la única cuestión de granularidad
que queda abierta y que corresponde al equipo.
