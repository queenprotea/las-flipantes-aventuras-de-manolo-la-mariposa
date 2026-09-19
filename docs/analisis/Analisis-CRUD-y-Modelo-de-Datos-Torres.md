# Análisis CRUD y modelo de datos — Juego de Torres

**Documento:** `Analisis-CRUD-y-Modelo-de-Datos-Torres.md` · 11 sep 2026
**Deriva de:** las 44 funcionalidades `FC-01`…`FC-44`, los 28 casos de uso `CU-01`…`CU-28`, las
8 historias de usuario `HU-01`…`HU-08` y las reglas del juego ya cerradas.
**Implementación:** [`code/database/schema.sql`](code/database/schema.sql) ·
diagrama: [`code/database/Modelo-BD-Torres.puml`](code/database/Modelo-BD-Torres.puml)

---

## Lo que este análisis encontró, antes de los detalles

Tres cosas que conviene leer antes de la tabla larga:

1. **De las 44 funcionalidades del sistema, solo 26 tocan la base de datos.** Las otras 18 operan
   sobre el estado en memoria del servidor o sobre el cliente. No es una carencia: es la
   consecuencia directa de que **de los invitados no se guarda ninguna fila**, y de que todo lo que
   puede contener invitados —sala, anfitrión, colores, chat y ranking de sala— no pueda vivir en
   una tabla sin quedar incompleto.
2. **Ninguna entidad del sistema tiene las cuatro operaciones.** Ni una sola. La que más tiene es
   `player`, con crear, consultar, actualizar y eliminar, pero incluso ahí el borrado es el único
   borrado físico que un actor puede provocar directamente.
3. **Hay un hueco real y está señalado**: el modelo **no puede representar una sanción cuyo umbral
   se cruzó durante una partida y todavía no ha entrado en vigor**. Está en el apartado 13, con las
   dos salidas posibles y una recomendación.

**Sobre el actor «Administrador»:** en este proyecto **no existe**. No hay rol de administración,
las sanciones son automáticas por umbral, nadie las revisa ni las levanta, y los catálogos de
parámetros son de solo lectura y se cargan por script. Por eso **ninguna operación de este
documento tiene ese actor**, y los cinco catálogos no tienen CRUD de mantenimiento.

**Los actores son los de los casos de uso, y no hay otros:** `Jugador` —equivale a la entidad
`player`—, `Invitado`, `Usuario sin cuenta` y `El sistema`, que no es un actor humano: dispara por
sí solo o por el vencimiento de un plazo.

---

# 1. Listado de funcionalidades CRUD

**Treinta y seis operaciones sobre datos persistentes**, agrupadas por la entidad que gestionan.

| ID | Operación | Entidad | Tipo | Actor |
|---|---|---|---|---|
| **CR-01** | Crear cuenta | `player` | Crear | Usuario sin cuenta |
| **CR-02** | Comprobar credenciales | `player` | Consultar | Jugador |
| **CR-03** | Buscar jugador por nombre de usuario | `player` | Consultar | Jugador |
| **CR-04** | Consultar el perfil propio | `player` | Consultar | Jugador |
| **CR-05** | Actualizar el perfil | `player` | Actualizar | Jugador |
| **CR-06** | Actualizar la contraseña | `player` | Actualizar | Jugador |
| **CR-07** | Eliminar la cuenta | `player` | Eliminar | Jugador |
| **CR-08** | Crear una solicitud de amistad | `friendship` | Crear | Jugador |
| **CR-09** | Consultar amigos y solicitudes | `friendship` | Consultar | Jugador |
| **CR-10** | Aceptar una solicitud | `friendship` | Actualizar | Jugador |
| **CR-11** | Eliminar una solicitud o una amistad | `friendship` | Eliminar | Jugador |
| **CR-12** | Crear una sala | `room` | Crear | Jugador · Invitado |
| **CR-13** | Consultar una sala por su código | `room` | Consultar | Jugador · Invitado |
| **CR-14** | Cerrar una sala | `room` | Actualizar | Jugador · Invitado · El sistema |
| **CR-15** | Crear una invitación a sala | `room_invitation` | Crear | Jugador |
| **CR-16** | Consultar las invitaciones recibidas | `room_invitation` | Consultar | Jugador |
| **CR-17** | Eliminar una invitación | `room_invitation` | Eliminar | Jugador · El sistema |
| **CR-18** | Crear la partida | `match` | Crear | Jugador · Invitado |
| **CR-19** | Consultar el historial de partidas | `match` + `match_participant` | Consultar | Jugador |
| **CR-20** | Consultar el detalle de una partida | `match` + `match_participant` | Consultar | Jugador |
| **CR-21** | Consultar las partidas en curso | `match` | Consultar | El sistema |
| **CR-22** | Terminar la partida | `match` | Actualizar | El sistema |
| **CR-23** | Eliminar una partida sin participaciones | `match` | Eliminar | El sistema |
| **CR-24** | Crear las participaciones | `match_participant` | Crear | Jugador · Invitado |
| **CR-25** | Escribir el resultado de cada participante | `match_participant` | Actualizar | El sistema |
| **CR-26** | Marcar a un participante como retirado | `match_participant` | Actualizar | El sistema · Jugador · Invitado |
| **CR-27** | Crear el primer estado de la partida | `match_state` | Crear | Jugador · Invitado |
| **CR-28** | Actualizar el estado al cerrar el turno | `match_state` | Actualizar | Jugador · Invitado · El sistema |
| **CR-29** | Consultar el estado para reanudar | `match_state` | Consultar | El sistema |
| **CR-30** | Eliminar el estado al terminar | `match_state` | Eliminar | El sistema |
| **CR-31** | Crear un código de recuperación | `password_recovery` | Crear | Jugador |
| **CR-32** | Comprobar un código de recuperación | `password_recovery` | Consultar | Jugador |
| **CR-33** | Eliminar el código usado | `password_recovery` | Eliminar | El sistema |
| **CR-34** | Crear un reporte | `report` | Crear | Jugador |
| **CR-35** | Contar las ocasiones acumuladas | `report` | Consultar | El sistema |
| **CR-36** | Crear una sanción | `sanction` | Crear | El sistema |
| **CR-37** | Comprobar la sanción vigente | `sanction` | Consultar | El sistema |
| **CR-38** | Consultar el ranking global | `player_ranking` | Consultar | Jugador · Invitado |
| **CR-39** | Consultar los catálogos de parámetros | catálogos | Consultar | El sistema |

## 1.1 Funcionalidades que NO son CRUD sobre la base de datos

Dieciocho de las cuarenta y cuatro. **Esto es un resultado del análisis, no una omisión.**

| Funcionalidad | Dónde vive | Por qué no puede ser una tabla |
|---|---|---|
| `FC-06` Obtener identidad de invitado | Memoria | **De un invitado no se guarda ninguna fila.** Es la decisión que ordena todo el modelo |
| `FC-07` Cambiar el idioma | Cliente | Es preferencia del equipo, no de la cuenta. No se guarda en ningún sitio |
| `FC-13` Consultar las salas disponibles | Memoria | El listado necesita **la ocupación**, y la ocupación incluye invitados |
| `FC-14` Entrar a una sala | Memoria | La lista de participantes de la sala incluye invitados |
| `FC-15` Salir de una sala | Memoria | Igual. Solo su **efecto colateral** —cerrar la sala vacía— toca la base: es `CR-14` |
| `FC-35` Expulsar a un jugador | Memoria | Retira de una lista que vive en memoria |
| `FC-22` Consultar el ranking de la sala | Memoria | **Incluye a los invitados**, así que ninguna consulta podría producirlo |
| `FC-23` Actualizar el ranking de la sala | Memoria | Lo mismo, y muere con la sala |
| `FC-25` Colocar la oruga inicial | Memoria y `match_state` | Es una jugada; queda dentro del documento de estado al cerrar el turno |
| `FC-26` Colocar la mariposa | Memoria y `match_state` | Igual |
| `FC-27` Jugar un turno | Memoria y `match_state` | Igual. Ni el reloj ni los puntos restantes se guardan |
| `FC-29` Cerrar la ronda | Memoria | La puntuación parcial no se persiste: solo la final |
| `FC-31` Reconectar | Memoria | El estado de conexión es volátil por definición |
| `FC-36` Enviar un mensaje al chat | Memoria | **El chat no se guarda.** Lo único que sobrevive de un mensaje es la copia dentro de un reporte |
| `FC-37` Consultar el chat | Memoria | Igual |
| `FC-41` Asignar y mostrar el color | Memoria | Se asigna en la sala, y la sala es memoria |
| `FC-42` Marcar inactivo y traspasar el anfitrión | Memoria | El anfitrión es memoria |
| `FC-43` Retirar de la sala al inactivo | Memoria | Igual. Solo su efecto colateral —cerrar la sala vacía— toca la base |

---

# 2. Prototipos de análisis CRUD

Cada operación con los siete apartados pedidos. **`[O]`** marca el dato obligatorio, **`[opc]`** el
opcional y **`[auto]`** el que genera el sistema sin que nadie lo teclee.

---

## `player` — la cuenta del jugador

### CR-01 · Crear cuenta

| | |
|---|---|
| **Qué gestiona** | La cuenta con la que un jugador tendrá identidad persistente: credenciales, perfil, historial y ranking |
| **Actor** | **Usuario sin cuenta** — todavía no es un Jugador; lo será al terminar esta operación |
| **Operación** | **Crear** |
| **Entrada** | `username` **[O]** · `email` **[O]** · contraseña **[O]** · `player_id` **[auto]** · `created_at` **[auto]** · `password_hash` **[auto]**, calculado a partir de la contraseña |
| **Datos afectados** | **Se crea** una fila de `player`. **Se consulta** `player` dos veces antes, para comprobar que el nombre de usuario y el correo no estén tomados |
| **Reglas** | Nombre de usuario de **3 a 20** caracteres entre letras, dígitos y guion bajo, **único sin distinguir mayúsculas** · correo **único sin distinguir mayúsculas** · contraseña de 8 a 20 con mayúsculas, minúsculas, números y caracteres especiales · **la contraseña se guarda solo como hash BCrypt y nunca se registra** |
| **Resultado** | La cuenta queda registrada y **la sesión se inicia con ella**. Si el nombre o el correo ya existen, no se crea nada y el campo correspondiente queda marcado como no disponible; si falla la conexión, no se crea nada |
| **Caso de uso** | `CU-02` |

### CR-02 · Comprobar credenciales

| | |
|---|---|
| **Qué gestiona** | La comprobación de que quien dice ser el titular de una cuenta lo es |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | `username` **[O]** · contraseña **[O]** |
| **Datos afectados** | **Se consulta** `player` —`username` y `password_hash`— y `sanction`, para el baneo permanente. **No se escribe nada**: las sesiones no se guardan |
| **Reglas** | La comparación del nombre de usuario **no distingue mayúsculas** · la contraseña se comprueba contra el hash, nunca en claro · **un baneo permanente impide iniciar sesión; una prohibición temporal no** |
| **Resultado** | El jugador queda identificado durante la sesión. Si las credenciales no coinciden, el sistema no dice cuál de las dos falló; si hay baneo permanente, el acceso se rechaza y se dice por qué |
| **Caso de uso** | `CU-01` |

### CR-03 · Buscar jugador por nombre de usuario

| | |
|---|---|
| **Qué gestiona** | La localización de otra cuenta para enviarle una solicitud de amistad o una invitación a sala |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | `username` completo **[O]** |
| **Datos afectados** | **Se consulta** `player` —`username` y `avatar_reference`—. Según desde dónde se busque, se consulta además `friendship` o `room_invitation` para saber qué ofrecer sobre esa fila |
| **Reglas** | **El nombre de usuario es el único criterio de búsqueda**: no se busca por correo · la coincidencia es **exacta**, sin distinguir mayúsculas · **el propio jugador no aparece en los resultados** |
| **Resultado** | Se muestra la fila del jugador encontrado con la acción que corresponda, o la búsqueda sin resultados |
| **Casos de uso** | `CU-08`, `CU-16` |

### CR-04 · Consultar el perfil propio

| | |
|---|---|
| **Qué gestiona** | Los dos datos que los demás ven de un jugador, y su correo, que solo ve él |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: el perfil es el de la sesión iniciada |
| **Datos afectados** | **Se consulta** `player`. El archivo del avatar **no está en la base**: se lee del disco con la ruta que guarda `avatar_reference` |
| **Reglas** | El correo se muestra **solo para consulta** y no puede modificarse · si el archivo del avatar no existe en el disco, se muestra el avatar por defecto y **la referencia no se toca** |
| **Resultado** | El jugador ve su nombre de usuario, su avatar y su correo |
| **Caso de uso** | `CU-06` |

### CR-05 · Actualizar el perfil

| | |
|---|---|
| **Qué gestiona** | El nombre de usuario y el avatar |
| **Actor** | **Jugador** |
| **Operación** | **Actualizar** |
| **Entrada** | `username` nuevo **[opc]** · archivo de imagen **[opc]** · `avatar_reference` **[auto]**, la ruta donde el servidor dejó el archivo |
| **Datos afectados** | **Se modifican** `player.username` y `player.avatar_reference`. **Se consulta** `player` antes, para la disponibilidad del nombre. **Se escribe un archivo** en el disco del servidor |
| **Reglas** | Mismo formato y misma unicidad que al crear la cuenta · el avatar es **PNG o JPG de hasta 5 MB**, validado en la capa de servicios · **la base solo guarda la referencia**, nunca la imagen |
| **Resultado** | El perfil queda actualizado y los demás jugadores lo ven así, también en las salas donde esté. Si el nombre está tomado o el archivo no se admite, no se modifica nada |
| **Caso de uso** | `CU-06` |

### CR-06 · Actualizar la contraseña

| | |
|---|---|
| **Qué gestiona** | La credencial de acceso, cuando el jugador no puede entrar |
| **Actor** | **Jugador** |
| **Operación** | **Actualizar** |
| **Entrada** | Código de recuperación **[O]** · contraseña nueva y su repetición **[O]** · `password_hash` **[auto]** |
| **Datos afectados** | **Se modifica** `player.password_hash`. **Se elimina** la fila de `password_recovery` usada |
| **Reglas** | El código debe corresponder a la cuenta y **no haber caducado: vale cinco minutos** · las dos contraseñas deben coincidir y cumplir el formato · **se guarda como hash BCrypt** |
| **Resultado** | La contraseña queda cambiada y el código deja de existir. La sesión **no** se inicia: el jugador vuelve a la ventana de acceso |
| **Caso de uso** | `CU-03` |

### CR-07 · Eliminar la cuenta

| | |
|---|---|
| **Qué gestiona** | La cuenta y absolutamente todo lo asociado a ella |
| **Actor** | **Jugador** |
| **Operación** | **Eliminar** — es el **único borrado físico que un actor provoca directamente** |
| **Entrada** | Confirmación explícita **[O]** |
| **Datos afectados** | **Se elimina** la fila de `player` y, en cascada, sus `friendship`, `room_invitation` —enviadas y recibidas—, `match_participant`, `report` —hechos y recibidos—, `sanction` y `password_recovery`. **Se elimina** el archivo del avatar del disco. **Se eliminan** las `match` que queden sin ninguna participación. **Se consulta** antes `match` y `match_participant`, para comprobar que no haya partida en curso |
| **Reglas** | **No puede eliminarse una cuenta que participa en una partida en curso** · las partidas terminadas conservan `player_count`, así que **siguen sabiendo cuántos jugaron** aunque desaparezca un resultado · **las sanciones se van con la cuenta**, de modo que un baneo permanente es evitable borrándose |
| **Resultado** | La cuenta y sus datos desaparecen sin posibilidad de recuperarlos, y el jugador queda sin sesión. Si está jugando, no se elimina nada y se le dice por qué |
| **Caso de uso** | `CU-07` |

---

## `friendship` — la relación entre dos cuentas

### CR-08 · Crear una solicitud de amistad

| | |
|---|---|
| **Qué gestiona** | La petición de amistad de una cuenta a otra, mientras está sin responder |
| **Actor** | **Jugador** |
| **Operación** | **Crear** |
| **Entrada** | Jugador destinatario, elegido de la búsqueda **[O]** · `status` = *pending* **[auto]** · `requested_at` **[auto]** |
| **Datos afectados** | **Se crea** una fila de `friendship`. **Se consulta** antes `friendship` para descartar que ya exista algo entre los dos |
| **Reglas** | **Entre dos cuentas solo puede existir una relación**, y la pareja invertida es la misma pareja: no pueden coexistir A→B y B→A · **nadie es amigo de sí mismo** · la solicitud **se conserva aunque el destinatario esté desconectado** y **no caduca** |
| **Resultado** | La solicitud aparece en las «Enviadas» del solicitante y en las «Recibidas» del destinatario. Si ya eran amigos o ya había una solicitud en cualquier sentido, no se crea nada y la fila muestra la situación real |
| **Caso de uso** | `CU-08` |

### CR-09 · Consultar amigos y solicitudes

| | |
|---|---|
| **Qué gestiona** | Las tres listas de la ventana de amigos: amistades aceptadas, recibidas pendientes y enviadas pendientes |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: son las relaciones de la cuenta de la sesión |
| **Datos afectados** | **Se consulta** `friendship` unida a `player`, para el nombre y el avatar del otro |
| **Reglas** | **La dirección solo importa mientras está pendiente**: una vez aceptada, la misma fila alimenta las dos listas de amigos · **las rechazadas no aparecen en ninguna parte**, porque el rechazo las borra |
| **Resultado** | El jugador ve sus amigos y sus solicitudes en los dos sentidos. Si no tiene ninguna, la lista se muestra vacía con su indicación |
| **Casos de uso** | `CU-08`, `CU-09`, y `CU-16` para la pestaña «Mis amigos» |

### CR-10 · Aceptar una solicitud de amistad

| | |
|---|---|
| **Qué gestiona** | El paso de solicitud pendiente a amistad |
| **Actor** | **Jugador** |
| **Operación** | **Actualizar** — no se crea una fila nueva: **la solicitud se convierte en la amistad** |
| **Entrada** | Solicitud elegida de la lista **[O]** · `status` = *accepted* **[auto]** · `responded_at` **[auto]** |
| **Datos afectados** | **Se modifican** `friendship.status` y `friendship.responded_at` |
| **Reglas** | Solo puede aceptarla el destinatario · **aceptar no crea ninguna invitación a sala**: amistad e invitación son cosas distintas |
| **Resultado** | Cada uno aparece en la lista de amigos del otro, con una sola fila para la pareja |
| **Caso de uso** | `CU-09` |

### CR-11 · Eliminar una solicitud o una amistad

| | |
|---|---|
| **Qué gestiona** | El rechazo de una solicitud y la eliminación de una amistad ya aceptada. **Es la misma operación**: las dos borran la única fila de la pareja |
| **Actor** | **Jugador** |
| **Operación** | **Eliminar** |
| **Entrada** | Solicitud o amigo elegido de la lista **[O]** · confirmación **[O]** al eliminar un amigo |
| **Datos afectados** | **Se elimina** la fila de `friendship` |
| **Reglas** | **La solicitud rechazada se borra y no deja registro** · **eliminar una amistad es simétrico** y **al eliminado no se le avisa** · **no existe el bloqueo**: nada impide volver a enviar una solicitud después · las invitaciones a sala entre los dos **no se ven afectadas**, porque dependen de la sala |
| **Resultado** | La relación deja de existir para los dos. Rechazar no avisa a quien la envió |
| **Casos de uso** | `CU-09` para el rechazo, `CU-20` para la eliminación |

---

## `room` — la sala

### CR-12 · Crear una sala

| | |
|---|---|
| **Qué gestiona** | La sala como identidad persistente: su código y su visibilidad. **No sus miembros**, que son memoria |
| **Actor** | **Jugador** o **Invitado** — un invitado también crea salas y es su anfitrión |
| **Operación** | **Crear** |
| **Entrada** | Visibilidad, pública o privada **[O]** · `room_id` **[auto]** · `code` **[auto]** · `created_at` **[auto]** · `closed_at` vacío **[auto]** |
| **Datos afectados** | **Se crea** una fila de `room`. **Se consulta** `room` antes, para comprobar que el código no lo esté usando ninguna sala abierta |
| **Reglas** | Código de **cuatro caracteres**, de un alfabeto que **excluye la O y el 0, y la I, la L y el 1**, porque se copia a mano de un correo · **único solo entre las salas abiertas**, lo que permite que sea tan corto y que se reutilice · la visibilidad **se fija al crearla y no cambia nunca** · la sala nace **abierta** |
| **Resultado** | La sala existe y quien la creó está dentro como anfitrión. Si el código sorteado ya está en uso, se genera otro sin que el jugador se entere |
| **Caso de uso** | `CU-15` |

### CR-13 · Consultar una sala por su código

| | |
|---|---|
| **Qué gestiona** | La localización de una sala abierta a partir del código que alguien tecleó |
| **Actor** | **Jugador** o **Invitado** |
| **Operación** | **Consultar** |
| **Entrada** | `code` **[O]** |
| **Datos afectados** | **Se consulta** `room` entre las que tienen `closed_at` vacío. La **ocupación** no sale de aquí: sale de la memoria, porque incluye invitados |
| **Reglas** | La búsqueda **no distingue mayúsculas** · solo se buscan **salas abiertas**: el código de una cerrada puede estar libre o pertenecer ya a otra · **el código es la credencial**: quien lo tenga entra, tenga o no invitación, sea la sala pública o privada |
| **Resultado** | Se localiza la sala y se pasa a admitir al jugador. Si no hay ninguna abierta con ese código, se avisa de que no se encontró |
| **Casos de uso** | `CU-14`, `CU-17` |

### CR-14 · Cerrar una sala

| | |
|---|---|
| **Qué gestiona** | El fin de vida de una sala. **No es un borrado**: la sala se cierra y la fila se conserva |
| **Actor** | **Jugador** o **Invitado** al salir siendo el último; **El sistema** al retirar al último inactivo o al arrancar el servidor |
| **Operación** | **Actualizar** |
| **Entrada** | Ninguna del actor: el cierre es consecuencia de que la sala se quede vacía · `closed_at` **[auto]** |
| **Datos afectados** | **Se modifica** `room.closed_at`. **Se eliminan** las `room_invitation` pendientes de esa sala. **Se descarta** de la memoria su lista de participantes, colores, ranking y chat |
| **Reglas** | **La sala vacía se cierra, no se borra**: sus partidas terminadas la referencian y siguen apareciendo en el historial · al cerrarse **libera su código** para otra sala · **al arrancar el servidor se cierran todas las que quedaran abiertas**, porque el estado de memoria no se reanuda |
| **Resultado** | La sala deja de admitir a nadie, su código queda libre y sus invitaciones desaparecen. Su chat y su ranking se pierden, porque nunca estuvieron guardados |
| **Casos de uso** | `CU-18`, y las historias `HU-01` y `HU-03` |

---

## `room_invitation` — la invitación pendiente

### CR-15 · Crear una invitación a sala

| | |
|---|---|
| **Qué gestiona** | La invitación mientras está pendiente. **Es lo único de una sala que tiene que sobrevivir a que el destinatario esté desconectado** |
| **Actor** | **Jugador** — **un invitado no puede invitar** |
| **Operación** | **Crear** |
| **Entrada** | Jugador destinatario **[O]**, de la lista de amigos o de la búsqueda · canal, dentro del juego o por correo **[O]** · `room_id` **[auto]**, la sala donde está quien invita · `created_at` **[auto]** |
| **Datos afectados** | **Se crea** una fila de `room_invitation`. **Se consulta** antes para descartar que ya haya una pendiente para ese jugador en esa sala. Si el canal es el correo, **se lee** `player.email` del destinatario |
| **Reglas** | **Siempre dirigida a una cuenta**: no se puede invitar a quien no la tiene · **una sola invitación pendiente por sala y destinatario** · **no caduca**: vive mientras viva la sala · **no hace falta ser amigo** para invitar · el correo lleva **el código de la sala y quién invita**, y **ningún enlace** |
| **Resultado** | La invitación aparece en la bandeja del destinatario, esté o no conectado. Si el envío del correo falla, **la invitación queda igualmente** y se informa a quien invitó |
| **Caso de uso** | `CU-16` |

### CR-16 · Consultar las invitaciones recibidas

| | |
|---|---|
| **Qué gestiona** | La bandeja de invitaciones a sala pendientes de una cuenta |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: son las dirigidas a la cuenta de la sesión |
| **Datos afectados** | **Se consulta** `room_invitation` unida a `player`, para saber quién invita, y a `room`, para el código |
| **Reglas** | Solo existen las **pendientes**: responder una la borra · **incluye las que llegaron con el jugador desconectado** · las de una sala que se cerró **ya no están**, porque el cierre las elimina |
| **Resultado** | El jugador ve quién le invitó y a qué sala. Si no tiene ninguna, la bandeja se muestra vacía |
| **Caso de uso** | `CU-17` |

### CR-17 · Eliminar una invitación

| | |
|---|---|
| **Qué gestiona** | El fin de la invitación, que ocurre por tres caminos distintos |
| **Actor** | **Jugador** al aceptarla o rechazarla; **El sistema** al cerrarse la sala |
| **Operación** | **Eliminar** |
| **Entrada** | Invitación elegida de la bandeja **[O]**, cuando la borra el jugador |
| **Datos afectados** | **Se elimina** la fila de `room_invitation` |
| **Reglas** | **Responderla la borra, se acepte o se rechace**, y eso es lo que la hace de un solo uso · **cerrar la sala borra todas las suyas** · **eliminar cualquiera de las dos cuentas la borra** · **no hay historial de invitaciones**: nadie lo consulta · entrar tecleando el código **no la consume**: sigue pendiente hasta que se responda o la sala cierre |
| **Resultado** | La invitación deja de existir. Rechazarla no avisa a quien la envió |
| **Casos de uso** | `CU-17`, `CU-18` |

---

## `match` — la partida

### CR-18 · Crear la partida

| | |
|---|---|
| **Qué gestiona** | El registro de que una partida empezó, con cuántos y en qué sala |
| **Actor** | **Jugador** o **Invitado**, el que sea anfitrión de la sala |
| **Operación** | **Crear** |
| **Entrada** | Ninguna que se teclee: la partida se arma con quienes están activos en la sala · `match_id` **[auto]** · `room_id` **[auto]** · `status` = *en curso* **[auto]** · `player_count` **[auto]** · `started_at` **[auto]** |
| **Datos afectados** | **Se crea** una fila de `match`, en la misma transacción que las participaciones y el primer estado. **Se consulta** `sanction`, para comprobar que ninguno esté sancionado, y los catálogos, para los turnos y las construcciones |
| **Reglas** | **De 2 a 4 jugadores, y solo cuentan los activos**: un inactivo sigue en la sala pero no entra a la partida · **toda partida nace en una sala** · **nadie puede incorporarse una vez iniciada** · **ningún jugador activo puede tener una prohibición temporal vigente** |
| **Resultado** | La partida queda en curso y empieza la colocación inicial. Si alguien está sancionado o queda un solo activo, no se crea nada |
| **Caso de uso** | `CU-19` |

### CR-19 · Consultar el historial de partidas

| | |
|---|---|
| **Qué gestiona** | Las partidas terminadas en las que participó una cuenta |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: son las de la cuenta de la sesión |
| **Datos afectados** | **Se consulta** `match_participant` unida a `match`, ordenada por fecha de fin |
| **Reglas** | Solo aparecen las **terminadas**: las que están en curso no · las **interrumpidas** se muestran **sin puesto ni puntos**, porque no tienen resultado · **`player_count` dice cuántos jugaron aunque no todos dejen fila** · una partida en la que el jugador fue retirado o se rindió **sigue apareciendo y sigue contando** |
| **Resultado** | El jugador ve su fecha, contra quién jugó, su puesto, sus puntos y cómo terminó cada partida. Los invitados se cuentan pero no se nombran |
| **Caso de uso** | `CU-10` |

### CR-20 · Consultar el detalle de una partida

| | |
|---|---|
| **Qué gestiona** | El resultado de todos los participantes de una partida concreta |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | Partida elegida del historial **[O]** |
| **Datos afectados** | **Se consulta** `match` y todas sus `match_participant`, unidas a `player` para los nombres. La **duración** se calcula con `started_at` y `finished_at`: no se guarda |
| **Reglas** | **La tabla puede no incluir todos los puestos**: si ganó un invitado, ninguna fila tendrá el puesto 1, y eso es correcto · **no hay desglose por rondas ni secuencia de jugadas**, porque no se guardan |
| **Resultado** | El jugador ve el puesto y los puntos de cada participante del que el sistema conserva datos |
| **Caso de uso** | `CU-11` |

### CR-21 · Consultar las partidas en curso

| | |
|---|---|
| **Qué gestiona** | La búsqueda, al arrancar el servidor, de las partidas que quedaron a medias |
| **Actor** | **El sistema** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: la dispara el arranque |
| **Datos afectados** | **Se consulta** `match` con `status` = *en curso*, y para cada una su `match_state` y sus `match_participant` |
| **Reglas** | **Es la única prueba de que una partida quedó a medias**: el estado de memoria no sobrevive · la partida espera el **plazo de reanudación** contado desde el arranque · **se reanuda cuando han vuelto al menos dos**, y los invitados cuentan presentando su pase |
| **Resultado** | Las partidas quedan a la espera. Al vencer el plazo: con un jugador, termina como *abandonada*; sin ninguno, como *interrumpida* |
| **Caso de uso** | `CU-27` |

### CR-22 · Terminar la partida

| | |
|---|---|
| **Qué gestiona** | El cierre de la partida y el motivo por el que terminó |
| **Actor** | **El sistema** |
| **Operación** | **Actualizar** |
| **Entrada** | Ninguna: la dispara jugarse la tercera ronda, quedar menos de dos jugadores o vencer el plazo de reanudación · `status` = *terminada* **[auto]** · `finished_at` **[auto]** · `end_reason` **[auto]** |
| **Datos afectados** | **Se modifican** `match.status`, `match.finished_at` y `match.end_reason`. En la misma transacción **se escribe** el resultado de cada participante y **se elimina** el estado |
| **Reglas** | **Una partida en curso no tiene fin ni motivo; una terminada tiene los dos** · *completada* si se jugaron las tres rondas, *abandonada* si quedaron menos de dos, *interrumpida* si tras una caída no volvió nadie · **la interrumpida no tiene puestos ni puntuaciones** y no cuenta para ningún ranking |
| **Resultado** | La partida pasa al historial y al ranking global. El ranking de la sala se actualiza en memoria |
| **Casos de uso** | `CU-25`, `CU-27`, `CU-28`, y la historia `HU-04` |

### CR-23 · Eliminar una partida sin participaciones

| | |
|---|---|
| **Qué gestiona** | La partida que ya no puede consultar nadie |
| **Actor** | **El sistema** |
| **Operación** | **Eliminar** |
| **Entrada** | Ninguna: la dispara terminar una partida sin cuentas, o eliminar la última cuenta que participaba |
| **Datos afectados** | **Se elimina** la fila de `match` y, en cascada, su `match_state` si aún lo tuviera |
| **Reglas** | **Una partida jugada solo por invitados no deja ninguna participación**: existe mientras está en curso, para poder reanudarse, y se elimina al terminar · al eliminar una cuenta se eliminan también las partidas que se queden sin ninguna participación |
| **Resultado** | La partida deja de existir. Ninguna consulta la echa de menos, porque no había nadie que pudiera pedirla |
| **Casos de uso** | `CU-07`, `CU-19` FA04 |

---

## `match_participant` — el jugador con cuenta dentro de una partida

### CR-24 · Crear las participaciones

| | |
|---|---|
| **Qué gestiona** | El asiento de cada jugador **con cuenta** dentro de la partida |
| **Actor** | **Jugador** o **Invitado**, el que sea anfitrión: es parte de iniciar la partida |
| **Operación** | **Crear** |
| **Entrada** | Ninguna que se teclee · `seat_number` **[auto]**, según el orden en que están acomodados en la sala · `player_id` **[auto]** · `was_withdrawn` = falso **[auto]** |
| **Datos afectados** | **Se crea** una fila por cada jugador con cuenta que se sienta. **Los invitados no generan fila**: su alias y su pase van dentro de `match_state` |
| **Reglas** | **El puesto de mesa es el identificador del jugador dentro de la partida**, y el único que se escribe en el registro de eventos junto al de la cuenta · **una cuenta ocupa un solo puesto por partida** · **los puestos guardados pueden tener huecos**, porque los invitados no dejan fila |
| **Resultado** | Cada jugador con cuenta tiene su asiento registrado, sin resultado todavía. Una partida de solo invitados **no crea ninguna fila** |
| **Caso de uso** | `CU-19` |

### CR-25 · Escribir el resultado de cada participante

| | |
|---|---|
| **Qué gestiona** | La puntuación final y el puesto final, que son lo que alimenta el historial y el ranking |
| **Actor** | **El sistema** |
| **Operación** | **Actualizar** |
| **Entrada** | Ninguna: se calcula al terminar la partida · `final_score` **[auto]** · `final_position` **[auto]** |
| **Datos afectados** | **Se modifican** `final_score` y `final_position` de cada participación, en la misma transacción que cierra la partida |
| **Reglas** | La puntuación suma **las jardineras donde el jugador tiene una única oruga propia**, los puntos de la mariposa y **1 punto por cada carta obtenida y no usada** · **el puesto final se guarda, no se deriva**: el desempate usa el número de jardineras puntuadas, que **no se guarda** · **una participación tiene los dos resultados o ninguno** · **dos jugadores no pueden quedar en el mismo puesto** · una partida *interrumpida* **no escribe ninguno de los dos** |
| **Resultado** | El historial y el ranking pueden calcularse. Los invitados no reciben fila, así que su resultado se pierde al terminar |
| **Casos de uso** | `CU-25`, `CU-27`, y la historia `HU-04` |

### CR-26 · Marcar a un participante como retirado

| | |
|---|---|
| **Qué gestiona** | Que un jugador dejó la partida antes de que terminara |
| **Actor** | **El sistema** cuando vence la ventana de reconexión; **Jugador** o **Invitado** cuando se rinde |
| **Operación** | **Actualizar** |
| **Entrada** | Confirmación **[O]** cuando es una rendición; ninguna cuando lo dispara el vencimiento del plazo · `was_withdrawn` = verdadero **[auto]** |
| **Datos afectados** | **Se modifica** `was_withdrawn`. **En memoria** se retiran sus orugas del tablero y se dejan sus construcciones |
| **Reglas** | **Agotar la ventana y rendirse tienen el mismo efecto**, y por eso son **un solo booleano y no dos**: ningún caso de uso pregunta cuál de los dos fue · el retirado **deja de contar para el resultado y para «el de menor puntuación»** · **la partida sigue contando para su ranking** · si quedan menos de dos, la partida termina como *abandonada* · **un invitado que se rinde no deja rastro**, porque no tiene fila |
| **Resultado** | El jugador queda fuera de la partida sin poder volver. La partida continúa con la configuración del número inicial de jugadores |
| **Casos de uso** | `CU-26`, `CU-28` |

---

## `match_state` — el estado vivo de la partida

### CR-27 · Crear el primer estado de la partida

| | |
|---|---|
| **Qué gestiona** | El tablero en el instante en que la partida arranca, para que una caída durante el primer turno no la deje sin nada que reanudar |
| **Actor** | **Jugador** o **Invitado**, el que sea anfitrión |
| **Operación** | **Crear** |
| **Entrada** | Ninguna que se teclee · `round_number` = 1, `turn_number` = 1 y `active_seat_number` **[auto]** · `state_document` **[auto]**, con el tablero inicial y, **de cada invitado, su alias, su puesto y su pase de asiento** |
| **Datos afectados** | **Se crea** la fila de `match_state` |
| **Reglas** | **El pase de asiento del invitado es el mismo identificador temporal que el servidor le dio**, copiado aquí: es lo único que le permitirá recuperar su puesto tras una caída · **no se guarda el reloj ni los puntos de acción**: el estado es siempre una frontera de turno |
| **Resultado** | La partida puede reanudarse desde el principio si el servidor cae antes del primer cierre de turno |
| **Caso de uso** | `CU-19` |

### CR-28 · Actualizar el estado al cerrar el turno

| | |
|---|---|
| **Qué gestiona** | El avance de la partida: dónde está el tablero y a quién le toca |
| **Actor** | **Jugador** o **Invitado** al terminar su turno; **El sistema** cuando se le agota el tiempo |
| **Operación** | **Actualizar** |
| **Entrada** | Ninguna que se teclee: es el resultado de las jugadas del turno · `round_number`, `turn_number`, `active_seat_number`, `state_document` y `saved_at` **[auto]** |
| **Datos afectados** | **Se modifica** la fila de `match_state`, entera |
| **Reglas** | **Se escribe al cerrar cada turno, y solo entonces**: el estado guardado es siempre una **frontera de turno** · **no guarda el reloj, ni los puntos restantes, ni las construcciones pendientes**, porque el turno interrumpido se rejuega entero con noventa segundos completos · las jugadas del turno **no se guardan una a una**: no hay secuencia de jugadas |
| **Resultado** | Una caída del servidor pierde como mucho el turno en juego, que se repetirá completo |
| **Caso de uso** | `CU-25` |

### CR-29 · Consultar el estado para reanudar

| | |
|---|---|
| **Qué gestiona** | La recuperación del tablero tras una caída del servidor |
| **Actor** | **El sistema** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: la dispara el arranque · el **pase de asiento** que presenta cada invitado **[O]** para recuperar su puesto |
| **Datos afectados** | **Se consulta** `match_state` de cada partida que quedó en curso |
| **Reglas** | **El turno interrumpido se descarta entero**, incluidas las cartas obtenidas o usadas en él · se reanuda con **noventa segundos completos** · **el pase prueba que se ocupaba ese asiento, no quién es esa persona**: quien no presente uno válido no puede ocuparlo |
| **Resultado** | La partida continúa desde el último cierre de turno. Un invitado que cerró su cliente perdió el pase y no recupera su puesto |
| **Caso de uso** | `CU-27` |

### CR-30 · Eliminar el estado al terminar

| | |
|---|---|
| **Qué gestiona** | La retirada del tablero cuando ya no hace falta |
| **Actor** | **El sistema** |
| **Operación** | **Eliminar** |
| **Entrada** | Ninguna: la dispara el fin de la partida |
| **Datos afectados** | **Se elimina** la fila de `match_state`, en la misma transacción que cierra la partida |
| **Reglas** | **El estado existe solo mientras la partida está en curso.** Que sea así **lo garantiza la operación, no el motor**: es la misma transacción la que escribe el resultado y borra el estado |
| **Resultado** | De la partida solo queda su resultado. El tablero no se conserva en ninguna parte |
| **Casos de uso** | `CU-25`, `CU-27`, `CU-28` |

---

## `password_recovery` — el código de recuperación

### CR-31 · Crear un código de recuperación

| | |
|---|---|
| **Qué gestiona** | La prueba de que quien pide cambiar la contraseña es el titular del correo |
| **Actor** | **Jugador** |
| **Operación** | **Crear** — y **sustituye al anterior** si la cuenta ya tenía uno |
| **Entrada** | `email` **[O]** · `code` **[auto]** · `created_at` **[auto]** |
| **Datos afectados** | **Se crea** la fila de `password_recovery`. **Se consulta** `player` para localizar la cuenta del correo |
| **Reglas** | **Un jugador tiene como mucho un código vigente**: pedir otro sustituye al anterior · **el código no se registra nunca en el log**: es una credencial · si el correo no pertenece a ninguna cuenta, **no se crea nada y el sistema muestra lo mismo**, para no revelar qué correos existen |
| **Resultado** | El código llega al correo de la cuenta. Si el envío falla, se informa y no se reintenta |
| **Caso de uso** | `CU-03` |

### CR-32 · Comprobar un código de recuperación

| | |
|---|---|
| **Qué gestiona** | La validación del código que el jugador teclea |
| **Actor** | **Jugador** |
| **Operación** | **Consultar** |
| **Entrada** | `code` **[O]** |
| **Datos afectados** | **Se consulta** `password_recovery` de esa cuenta, y `created_at` para la caducidad |
| **Reglas** | **El código se comprueba contra la cuenta, no por sí solo**: no existe ninguna operación que reciba un código y averigüe de quién es · **caduca a los cinco minutos** de haberse generado · **la caducidad se calcula, no se guarda**: guardar el vencimiento sería guardar la suma de dos datos que ya están |
| **Resultado** | Se permite establecer la contraseña nueva. Si el código no es de esa cuenta, ya se usó o caducó, se rechaza y puede pedirse otro |
| **Caso de uso** | `CU-03` |

### CR-33 · Eliminar el código usado

| | |
|---|---|
| **Qué gestiona** | Que un código sirva **una sola vez** |
| **Actor** | **El sistema** |
| **Operación** | **Eliminar** |
| **Entrada** | Ninguna: la dispara el cambio de contraseña |
| **Datos afectados** | **Se elimina** la fila, en la misma transacción que actualiza `password_hash` |
| **Reglas** | **Se elimina al usarse** · se elimina también **con la cuenta**, en cascada |
| **Resultado** | El código deja de valer. Quien quiera cambiar otra vez la contraseña tendrá que pedir uno nuevo |
| **Caso de uso** | `CU-03` |

---

## `report` y `sanction` — la convivencia

### CR-34 · Crear un reporte

| | |
|---|---|
| **Qué gestiona** | La constancia de que alguien escribió un mensaje ofensivo, **con copia del mensaje** |
| **Actor** | **Jugador** — **un invitado no reporta y no puede ser reportado** |
| **Operación** | **Crear** |
| **Entrada** | Mensaje elegido del chat **[O]** · `reported_text` **[auto]**, copiado del mensaje · `room_id` **[auto]** · `match_id` **[auto]** si hay partida en curso, vacío si no · `created_at` **[auto]** |
| **Datos afectados** | **Se crea** la fila de `report` |
| **Reglas** | **Solo las cuentas reportan y solo se reporta a cuentas** · **el texto se copia porque el chat no se guarda**: sin esa copia el reporte se quedaría sin sustento en el mismo instante en que se hace · **nadie avisa al reportado**, ni se le dice quién fue · el texto cabe en **200 caracteres**, que es el límite del chat |
| **Resultado** | El reporte queda registrado y se recuenta cuántas ocasiones acumula el reportado |
| **Caso de uso** | `CU-23` |

### CR-35 · Contar las ocasiones acumuladas

| | |
|---|---|
| **Qué gestiona** | El recuento que decide si se cruza el umbral. **Es la consulta menos evidente del sistema** |
| **Actor** | **El sistema** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: se dispara al registrarse un reporte |
| **Datos afectados** | **Se consulta** `report` del jugador reportado y `sanction`, para saber desde cuándo contar |
| **Reglas** | **Lo que se acumula son ocasiones, no reportes sueltos**: cada partida distinta cuenta una, y **todos los reportes de una sala fuera de partida cuentan una entre todos** · la pareja `(room_id, match_id)` **es** la ocasión: contar ocasiones es **contar parejas distintas** · **el contador se reinicia tras cada prohibición**, y eso se implementa contando solo los reportes **posteriores al instante del umbral de la última sanción** · el umbral es **cinco** |
| **Resultado** | Si se llega a cinco, se crea la sanción. Si no, no ocurre nada visible para nadie |
| **Caso de uso** | `CU-23` |

### CR-36 · Crear una sanción

| | |
|---|---|
| **Qué gestiona** | La prohibición sobre una cuenta. **Automática: no hay quien la revise ni quien la levante** |
| **Actor** | **El sistema** |
| **Operación** | **Crear** |
| **Entrada** | Ninguna: la dispara cruzar el umbral · `level` **[auto]**, según cuántas prohibiciones previas tenga · `is_permanent` **[auto]** · `threshold_at` **[auto]** · `effective_from` **[auto]** · `effective_until` **[auto]**, calculado con la duración del catálogo |
| **Datos afectados** | **Se crea** la fila de `sanction`. **Se consulta** antes `sanction` para contar las previas y `sanction_level` para la duración |
| **Reglas** | Escalera **5 horas, 1 día y 3 días**; **el cuarto cruce es permanente** · **el contador de prohibiciones no se reinicia nunca**, y por eso las previas se cuentan sobre las filas de la cuenta en vez de guardarse aparte · **la temporal impide entrar a salas y jugar, pero no iniciar sesión; la permanente impide también iniciar sesión** · **si el umbral se cruza durante una partida, la sanción entra en vigor al terminarla, y su duración empieza a contar entonces** |
| **Resultado** | La cuenta queda sancionada. **Este es el punto donde el modelo tiene un hueco**: ver el apartado 13 |
| **Caso de uso** | `CU-23` |

### CR-37 · Comprobar la sanción vigente

| | |
|---|---|
| **Qué gestiona** | La comprobación que se hace en los tres sitios donde una prohibición tiene efecto |
| **Actor** | **El sistema** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna: se dispara al iniciar sesión, al entrar a una sala y al iniciar una partida |
| **Datos afectados** | **Se consulta** `sanction` de la cuenta |
| **Reglas** | Al **iniciar sesión** solo cuenta el baneo permanente · al **entrar a una sala** y al **iniciar una partida** cuentan también las temporales · **la temporal deja de aplicarse sola** al pasar su fecha de fin, sin que nadie haga nada, y **la fila se conserva** porque el contador de prohibiciones no se reinicia |
| **Resultado** | Se permite o se rechaza la operación, diciendo hasta cuándo dura la prohibición |
| **Casos de uso** | `CU-01`, `CU-13`, `CU-19` |

---

## Consultas sin entidad propia

### CR-38 · Consultar el ranking global

| | |
|---|---|
| **Qué gestiona** | La clasificación general. **No almacena nada: se calcula** |
| **Actor** | **Jugador** o **Invitado** — un invitado puede consultarlo, pero **nunca aparecerá en él** |
| **Operación** | **Consultar** |
| **Entrada** | Ninguna |
| **Datos afectados** | **Se consulta** la vista `player_ranking`, que agrega `match_participant` y `match` sobre `player` |
| **Reglas** | Ordena por **victorias**, después por **puntos acumulados** y después por **menos partidas jugadas** · entran las partidas jugadas con invitados y aquellas en las que el jugador fue retirado · **quedan fuera las interrumpidas y los invitados** |
| **Resultado** | Se muestra la clasificación vigente, con la fila propia resaltada si aparece |
| **Caso de uso** | `CU-12` |

### CR-39 · Consultar los catálogos de parámetros

| | |
|---|---|
| **Qué gestiona** | Los números que las reglas declaran variables y los valores que el proyecto fijó |
| **Actor** | **El sistema** |
| **Operación** | **Consultar** — **y nada más: no hay crear, actualizar ni eliminar** |
| **Entrada** | Ninguna |
| **Datos afectados** | **Se consultan** `turn_layout`, `build_allowance`, `action_cost`, `sanction_level` y `system_parameter` |
| **Reglas** | **Son de solo lectura y se cargan por script** · **no existe rol de administración**, así que ninguna pantalla los edita y ningún caso de uso los mantiene · existen para poder cambiar números **sin tocar el dominio ni recompilar** |
| **Resultado** | El dominio recibe los valores como argumentos desde la capa de servicios |
| **Casos de uso** | `CU-19`, `CU-23`, `CU-25` |

---

# 3. Matriz general CRUD

**C** crear · **R** consultar · **U** actualizar · **D** eliminar · **—** no existe, y la columna
«Por qué falta» dice el motivo.

| Entidad | C | R | U | D | Quién la gestiona | Por qué falta lo que falta |
|---|:-:|:-:|:-:|:-:|---|---|
| `player` | **C** | **R** | **U** | **D** | Usuario sin cuenta crea · Jugador consulta, actualiza y elimina | Es la única entidad con las cuatro |
| `friendship` | **C** | **R** | **U** | **D** | Jugador | La actualización es una sola: pasar de pendiente a aceptada |
| `room` | **C** | **R** | **U** | **—** | Jugador · Invitado · El sistema | **Nunca se borra.** Se cierra, porque sus partidas la referencian |
| `room_invitation` | **C** | **R** | **—** | **D** | Jugador · El sistema | **No se actualiza nunca:** una invitación no cambia, se responde y desaparece |
| `match` | **C** | **R** | **U** | **D** | Jugador · Invitado crean · El sistema actualiza y elimina | El borrado solo alcanza a la partida **sin ninguna participación** |
| `match_participant` | **C** | **R** | **U** | **—** | Jugador · Invitado crean · El sistema actualiza | **No se borra directamente:** solo en cascada con su partida o su jugador |
| `match_state` | **C** | **R** | **U** | **D** | Jugador · Invitado · El sistema | Las cuatro, pero ninguna la pide un actor como objetivo: son efectos de jugar |
| `password_recovery` | **C** | **R** | **—** | **D** | Jugador · El sistema | **No se actualiza:** pedir otro código sustituye la fila entera |
| `report` | **C** | **R** | **—** | **—** | Jugador crea · El sistema consulta | **No se corrige ni se retira.** Es constancia de un hecho; solo desaparece con una de las dos cuentas |
| `sanction` | **C** | **R** | **—** | **—** | El sistema | **No se levanta ni se modifica:** caduca sola. Solo desaparece con la cuenta |
| `turn_layout` | **—** | **R** | **—** | **—** | El sistema | **Solo lectura.** No hay rol de administración; se carga por script |
| `build_allowance` | **—** | **R** | **—** | **—** | El sistema | Igual |
| `action_cost` | **—** | **R** | **—** | **—** | El sistema | Igual |
| `sanction_level` | **—** | **R** | **—** | **—** | El sistema | Igual |
| `system_parameter` | **—** | **R** | **—** | **—** | El sistema | Igual |
| `player_ranking` | **—** | **R** | **—** | **—** | Jugador · Invitado | **Es una vista: no almacena nada.** Se calcula sobre partidas terminadas |

## 3.1 Lo que la matriz deja ver

**Entidades que solo se consultan:** los cinco catálogos y la vista del ranking. Seis de las
dieciséis. Ninguna tiene pantalla de mantenimiento, y no es un olvido: **no hay administrador**.

**Entidades que se crean como consecuencia de otra operación, no porque alguien las pida:**

| Entidad | La crea | A raíz de |
|---|---|---|
| `match_participant` | El anfitrión, sin elegir nada | Crear la partida |
| `match_state` | El anfitrión, sin elegir nada | Crear la partida |
| `sanction` | El sistema | Que un reporte cruce el umbral |

**Entidades con borrado físico y entidades que se conservan:**

| Entidad | Qué le pasa al final | Por qué |
|---|---|---|
| `player` | **Borrado físico en cascada** | La eliminación es irreversible por decisión del proyecto, y arrastra todo lo suyo |
| `friendship`, `room_invitation`, `password_recovery` | **Borrado físico** | Son estados pendientes: fuera del estado pendiente no tienen sentido, y nadie consulta su historia |
| `room` | **Se conserva, cerrada** | Sus partidas la referencian. El borrado está impedido por la clave ajena de `match` |
| `match`, `match_participant` | **Se conservan** | Son el historial y el ranking. Solo se borra la partida que se queda sin ningún participante con cuenta |
| `match_state` | **Borrado físico al terminar** | Es estado de ejecución: fuera de la partida en curso no significa nada |
| `report`, `sanction` | **Se conservan mientras exista la cuenta** | El contador de prohibiciones se cuenta sobre las filas, así que borrarlas falsearía la escalera |

**Información que debe persistirse:** cuentas, amistades, salas, partidas terminadas,
participaciones, reportes y sanciones. **Temporal en la base:** invitaciones pendientes,
solicitudes pendientes, códigos de recuperación y el estado de la partida en curso — tienen que
sobrevivir a una caída, pero se borran cuando dejan de tener sentido.

**Información que es estado de ejecución y no se almacena:** quién está en cada sala, quién es el
anfitrión, los colores, el chat, el ranking de la sala, el estado de conexión, el reloj del turno y
los puntos de acción restantes. **Todo lo de esta lista puede contener invitados, o muere con la
sala.** Y tres cosas más que tampoco se guardan aunque podrían: la secuencia de jugadas, el
desglose de puntos por ronda y las sesiones —nadie las consulta, y el estándar prohíbe registrar
las últimas—.

---

# 4. Listado definitivo de entidades

**Dieciséis: diez entidades, cinco catálogos y una vista.** Todas aparecen en la matriz anterior,
y ninguna existe sin una operación que la justifique.

| # | Entidad | Propósito | Clase |
|---|---|---|---|
| 1 | `player` | Identidad persistente del jugador. **Absorbe el perfil** | Permanente |
| 2 | `friendship` | Solicitud pendiente y amistad aceptada entre dos cuentas | Pendiente → permanente |
| 3 | `room` | La sala, como identidad y como código reservado | Permanente |
| 4 | `room_invitation` | Invitación a sala mientras está pendiente | Temporal |
| 5 | `match` | Una partida, en curso o archivada | Permanente al terminar |
| 6 | `match_participant` | Un jugador **con cuenta** dentro de una partida, y su resultado | Permanente |
| 7 | `match_state` | Estado vivo de la partida en curso, para reanudarla | Temporal |
| 8 | `password_recovery` | Código de un solo uso enviado al correo | Temporal |
| 9 | `report` | Mensaje ofensivo reportado, con copia del texto | Permanente |
| 10 | `sanction` | Prohibición sobre una cuenta, temporal o permanente | Permanente |
| 11 | `turn_layout` | Turnos por ronda según cuántos juegan | Catálogo |
| 12 | `build_allowance` | Construcciones por jugador en cada turno | Catálogo |
| 13 | `action_cost` | Coste en puntos de acción de cada acción | Catálogo |
| 14 | `sanction_level` | Duración de cada escalón de la escalera de prohibiciones | Catálogo |
| 15 | `system_parameter` | Umbrales, límites y plazos que deben cambiarse sin recompilar | Catálogo |
| 16 | `player_ranking` | Ranking global | Vista derivada |

## 4.1 ¿Falta alguna entidad? La comprobación

Se recorrieron **las 44 funcionalidades** buscando información que el modelo no pueda representar.
**No falta ninguna entidad**, y estas son las cinco que más lo parecían:

| Lo que parecía faltar | Dónde está representado |
|---|---|
| **Los miembros de una sala** | En memoria. Una tabla solo podría contener a los que tienen cuenta y **nunca sabría si la sala está vacía**, que es la condición que decide si la sala existe |
| **El anfitrión** | En memoria, por lo mismo: puede ser un invitado, y la condición cambia de persona |
| **Los mensajes del chat** | En ninguna parte, a propósito. Lo único que sobrevive de un mensaje es **la copia dentro de un reporte** |
| **El ranking de la sala** | En memoria. **Incluye invitados**, así que ninguna consulta podría producirlo |
| **El pase de asiento del invitado** | Dentro de `match_state.state_document`, junto a su alias y su puesto. Pertenece a alguien que no tiene fila en ninguna tabla |

**Lo que sí falta no es una entidad, es un estado:** el modelo no puede representar una sanción
**diferida**, cuyo umbral se cruzó durante una partida y todavía no ha entrado en vigor. Está en el
apartado 13.

---

# 5. Atributos, llaves primarias y llaves foráneas

Cada entidad con sus atributos, su tipo en PostgreSQL, y **por qué su llave primaria identifica
bien la fila**. `O` marca obligatorio.

## 5.1 `player`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `player_id` | `integer` generado | ✔ | **PK** |
| `username` | `varchar(20)` | ✔ | Único sobre `lower(username)` |
| `email` | `varchar(254)` | ✔ | Único sobre `lower(email)` |
| `password_hash` | `varchar(255)` | ✔ | BCrypt |
| `avatar_reference` | `varchar(255)` | | Ruta en el disco del servidor |
| `created_at` | `timestamptz` | ✔ | |

**PK `player_id`, y no `username`.** El nombre de usuario **es único y podría ser la llave**, pero
**se puede cambiar** en el perfil: usarlo como llave obligaría a propagar el cambio a las
participaciones, los reportes, las sanciones, las amistades y las invitaciones. Un identificador
generado no cambia nunca, y además **es el único identificador de persona que puede aparecer en el
registro de eventos**, cosa que un nombre de usuario no podría ser sin exponer un dato personal.

**FK:** ninguna. Es la raíz del modelo.

## 5.2 `friendship`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `requester_id` | `integer` | ✔ | **PK** · FK → `player` |
| `addressee_id` | `integer` | ✔ | **PK** · FK → `player` |
| `status` | `varchar(8)` | ✔ | *pending* · *accepted* |
| `requested_at` | `timestamptz` | ✔ | |
| `responded_at` | `timestamptz` | | Vacío mientras está pendiente |

**PK compuesta `(requester_id, addressee_id)`, y aquí sí procede.** La pareja **es** la identidad:
no hay dos relaciones entre los mismos dos jugadores, y un identificador generado no añadiría nada
—nada referencia a una amistad—. Es el caso de libro en el que la clave compuesta natural gana a
la artificial.

**Las dos FK apuntan a la misma tabla**: es una relación **reflexiva**.

**Integridad:** un índice único sobre `(menor, mayor)` de los dos identificadores impide que A→B y
B→A coexistan, que es lo que evita que dos jugadores acaben «dos veces amigos» · una comprobación
impide ser amigo de sí mismo · otra ata `status` con `responded_at`.

## 5.3 `room`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `room_id` | `integer` generado | ✔ | **PK** |
| `code` | `varchar(4)` | ✔ | Único **solo entre las abiertas** |
| `visibility` | `varchar(7)` | ✔ | *public* · *private* |
| `created_at` | `timestamptz` | ✔ | |
| `closed_at` | `timestamptz` | | Vacío mientras está abierta |

**PK `room_id`, y el código no puede serlo.** El código **se reutiliza**: es único entre las salas
abiertas, así que dos salas cerradas pueden compartirlo. No identifica una fila, identifica **una
sala viva**, y por eso la unicidad se implementa con un **índice único parcial** restringido a las
filas con `closed_at` vacío. Sin el identificador generado no habría forma de que `match` y
`report` apuntaran a la sala correcta.

**FK:** ninguna.

## 5.4 `room_invitation`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `room_id` | `integer` | ✔ | **PK** · FK → `room` |
| `invited_player_id` | `integer` | ✔ | **PK** · FK → `player` |
| `inviter_player_id` | `integer` | ✔ | FK → `player` |
| `channel` | `varchar(8)` | ✔ | *in_game* · *email* |
| `created_at` | `timestamptz` | ✔ | |

**PK compuesta `(room_id, invited_player_id)`.** La regla del sistema es **una sola invitación
pendiente por sala y destinatario**, así que ese par ya es la identidad de la fila. **Este análisis
retiró el identificador generado que tenía antes**: no lo referencia nadie, y mantenerlo obligaba a
llevar además un índice único sobre el par para hacer cumplir la regla. La clave compuesta hace las
dos cosas a la vez.

**Las tres FK** en cascada. **Ojo con una sutileza:** la cascada hacia `room` **nunca se dispara**,
porque las salas no se borran; el borrado de las invitaciones al cerrar la sala **lo hace la
operación**, en la misma transacción. La cascada es una red de seguridad, no el mecanismo.

## 5.5 `match`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `match_id` | `integer` generado | ✔ | **PK** |
| `room_id` | `integer` | ✔ | FK → `room`, **con borrado restringido** |
| `status` | `varchar(12)` | ✔ | *in_progress* · *finished* |
| `player_count` | `smallint` | ✔ | 2 a 4 |
| `started_at` | `timestamptz` | ✔ | |
| `finished_at` | `timestamptz` | | Vacío mientras está en curso |
| `end_reason` | `varchar(12)` | | *completed* · *abandoned* · *interrupted* |

**PK `match_id`.** No hay ninguna combinación de atributos que identifique una partida: la misma
sala juega varias, y dos podrían empezar en el mismo instante en salas distintas.

**`player_count` se guarda y no se deriva**, aunque lo parezca: **los invitados no dejan fila** y
una cuenta eliminada se lleva la suya, así que contar participaciones daría 2 en una partida de 4
con dos invitados, y 0 en una que jugaron solo invitados.

**La FK a `room` restringe el borrado**, y esa restricción es lo que garantiza por motor que una
sala con partidas no pueda borrarse.

## 5.6 `match_participant`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `match_id` | `integer` | ✔ | **PK** · FK → `match` |
| `seat_number` | `smallint` | ✔ | **PK** · 1 a 4 |
| `player_id` | `integer` | ✔ | FK → `player` |
| `final_score` | `integer` | | Vacío hasta que termina |
| `final_position` | `smallint` | | Vacío hasta que termina |
| `was_withdrawn` | `boolean` | ✔ | |

**PK compuesta `(match_id, seat_number)`, y es la que mejor describe el dominio.** El puesto de
mesa **es** el identificador del jugador dentro de la partida: es lo que dice de quién es cada
oruga, quién coloca la mariposa en la primera ronda y a quién le toca. Un identificador generado
existiría sin significar nada, y habría que añadir igualmente la unicidad del puesto.

**Unicidad adicional:** `(match_id, player_id)` —una cuenta ocupa un solo puesto— y
`(match_id, final_position)` —dos jugadores no quedan en el mismo puesto—, que admite varios vacíos
porque en una partida en curso nadie tiene puesto final todavía.

**`final_position` se guarda y no se deriva** de la puntuación: el desempate usa el número de
jardineras en las que se puntuó, **y ese número no se guarda**. Se guarda el resultado, no los
ingredientes.

## 5.7 `match_state`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `match_id` | `integer` | ✔ | **PK y FK a la vez** → `match` |
| `round_number` | `smallint` | ✔ | 1 a 3 |
| `turn_number` | `smallint` | ✔ | |
| `active_seat_number` | `smallint` | ✔ | 1 a 4 |
| `state_document` | `jsonb` | ✔ | Tablero, y **alias, puesto y pase de cada invitado** |
| `saved_at` | `timestamptz` | ✔ | |

**PK `match_id`, que es también la FK.** Es una **extensión opcional** de la partida: comparte su
llave, y así el motor garantiza por construcción que no puede haber dos estados de la misma partida
ni un estado sin partida.

## 5.8 `password_recovery`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `player_id` | `integer` | ✔ | **PK y FK a la vez** → `player` |
| `code` | `varchar(16)` | ✔ | |
| `created_at` | `timestamptz` | ✔ | La caducidad se calcula sobre él |

**PK `player_id`.** Un jugador tiene **como mucho un código vigente**, y poner la llave en el
jugador es lo que lo garantiza: pedir otro sustituye la fila. Con un identificador propio podrían
acumularse códigos vivos de la misma cuenta y habría que decidir cuál vale.

**`code` no lleva índice único global**: se comprueba **contra la cuenta**, y no existe ninguna
operación que reciba un código suelto y averigüe de quién es.

## 5.9 `report`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `report_id` | `integer` generado | ✔ | **PK** |
| `reporter_player_id` | `integer` | ✔ | FK → `player` |
| `reported_player_id` | `integer` | ✔ | FK → `player` |
| `room_id` | `integer` | ✔ | FK → `room` |
| `match_id` | `integer` | | FK → `match`. Vacío si fue fuera de partida |
| `reported_text` | `varchar(200)` | ✔ | Copia del mensaje |
| `created_at` | `timestamptz` | ✔ | |

**PK generada, y aquí sí hace falta.** No hay clave natural: **el mismo jugador puede reportar al
mismo jugador dos veces en la misma sala y en la misma partida** —el sistema lo registra, aunque no
cuente como ocasión nueva—, así que ninguna combinación de sus atributos es única.

**`room_id` obligatorio y `match_id` opcional es el corazón de la tabla**: la pareja
`(room_id, match_id)` **es la ocasión**, y contar ocasiones es contar parejas distintas. Si la
partida fuera obligatoria, los mensajes de sala no tendrían dónde ir; si la sala fuera opcional,
las ocasiones fuera de partida no podrían agruparse.

## 5.10 `sanction`

| Atributo | Tipo | O | Notas |
|---|---|:-:|---|
| `sanction_id` | `integer` generado | ✔ | **PK** |
| `player_id` | `integer` | ✔ | FK → `player` |
| `level` | `smallint` | | FK → `sanction_level`. Vacío si es permanente |
| `is_permanent` | `boolean` | ✔ | |
| `threshold_at` | `timestamptz` | ✔ | Cuándo se alcanzó la quinta ocasión |
| `effective_from` | `timestamptz` | ✔ | Cuándo entra en vigor **y empieza a contar** |
| `effective_until` | `timestamptz` | | Vacío si es permanente |

**PK generada.** `(player_id, threshold_at)` sería única en la práctica, pero una marca de tiempo
es una llave frágil y nada obliga a que dos cruces no compartan instante.

**`threshold_at` y `effective_from` son dos columnas y no una** porque, si el umbral se cruza
durante una partida, la sanción entra en vigor al terminarla y **su duración empieza a contar
entonces**. Con una sola marca, **una partida larga acortaría el castigo**.

**El número de prohibiciones previas no se guarda:** se cuenta sobre las filas de la cuenta.
Guardarlo crearía una segunda fuente para un dato que las propias filas ya dan.

## 5.11 Catálogos

| Catálogo | PK | Por qué esa llave | FK |
|---|---|---|---|
| `turn_layout` | `(player_count, round_number)` | Los turnos dependen de **las dos cosas a la vez**: cuántos juegan y qué ronda es | — |
| `build_allowance` | `(player_count, round_number, turn_number)` | Las construcciones dependen de **las tres**: las reglas las dan por ronda **y turno** | `(player_count, round_number)` → `turn_layout` |
| `action_cost` | `action_code` | El código de la acción es su identidad | — |
| `sanction_level` | `level` | El escalón es su identidad | — |
| `system_parameter` | `parameter_code` | El código del parámetro es su identidad | — |

**Las tres PK compuestas de este bloque son naturales**: la clave del catálogo es exactamente la
combinación por la que se consulta.

---

# 6. Relaciones y cardinalidades

| # | Origen | Destino | Tipo | Cardinalidad | Participación | ¿Entidad asociativa? |
|---|---|---|---|---|---|---|
| 1 | `player` | `friendship` | Reflexiva, como solicitante | 1 : 0..N | Opcional en los dos lados | **Sí, `friendship` lo es** |
| 2 | `player` | `friendship` | Reflexiva, como destinataria | 1 : 0..N | Opcional | La misma |
| 3 | `player` | `room_invitation` | Como quien invita | 1 : 0..N | Opcional | **Sí, `room_invitation` lo es** |
| 4 | `player` | `room_invitation` | Como invitada | 1 : 0..N | Opcional | La misma |
| 5 | `room` | `room_invitation` | Composición | 1 : 0..N | Opcional; **ninguna sobrevive a su sala** | La misma |
| 6 | `room` | `match` | Asociación | 1 : 0..N | **Obligatoria del lado de la partida**: toda partida nace en una sala | No |
| 7 | `match` | `match_participant` | Composición | 1 : **0..4** | **Opcional**, y el mínimo es cero | **Sí, es la asociativa** |
| 8 | `player` | `match_participant` | Asociación | 1 : 0..N | Opcional | La misma |
| 9 | `match` | `match_state` | Extensión 1:1 | 1 : **0..1** | Opcional: solo mientras está en curso | No |
| 10 | `player` | `password_recovery` | Extensión 1:1 | 1 : **0..1** | Opcional | No |
| 11 | `player` | `report` | Como autora | 1 : 0..N | Opcional | No |
| 12 | `player` | `report` | Como reportada | 1 : 0..N | Opcional | No |
| 13 | `room` | `report` | Asociación | 1 : 0..N | **Obligatoria del lado del reporte** | No |
| 14 | `match` | `report` | Asociación | 1 : 0..N | **Opcional**: vacía si fue fuera de partida | No |
| 15 | `player` | `sanction` | Asociación | 1 : 0..N | Opcional | No |
| 16 | `sanction_level` | `sanction` | Asociación | 1 : 0..N | **Opcional**: la permanente no tiene nivel | No |
| 17 | `turn_layout` | `build_allowance` | Composición | 1 : **1..N** | **Obligatoria**: cada ronda tiene al menos un turno con reparto | No |

## 6.1 Las relaciones que el enunciado pedía mirar de cerca

**Jugadores y cuentas: no son dos cosas.** Esta es la relación que más fácil se modela mal. En este
sistema **el jugador es la cuenta**: no hay una entidad «cuenta» y otra «jugador» unidas 1:1, ni
falta hace, porque ninguna de las dos tendría atributos propios. **El perfil tampoco es una
entidad**: son dos atributos de `player`, y uno de ellos, el nombre de usuario, **es además la
credencial de acceso y el único criterio de búsqueda**, así que no podría vivir fuera.

**Jugadores y salas: N:M, y deliberadamente sin tabla puente.** Es la única N:M del sistema que
**no se resuelve con una entidad asociativa**, y el motivo es que uno de los extremos puede ser un
invitado, del que no hay fila. Vive en la memoria del servidor. Lo único que sí se guarda de la
relación jugador–sala es la **invitación**, y se guarda porque tiene que sobrevivir a que el
destinatario esté desconectado.

**Salas y partidas: 1:N, y la sala sobrevive a la partida.** Una sala juega **varias** partidas, y
por eso al terminar una los jugadores vuelven a la sala y no al menú. La partida no puede existir
sin sala, y la sala no puede borrarse mientras tenga partidas.

**Jugadores y partidas: N:M resuelta con `match_participant`**, que **no es una tabla puente
vacía**: lleva el puesto de mesa, la puntuación, el puesto final y la marca de retirada. Es una
entidad asociativa con atributos propios, y **su mínimo es cero**, no dos, porque los invitados no
dejan fila.

**Jugadores y amistades: N:M reflexiva.** Las dos claves ajenas apuntan a `player`. Su
particularidad es que **la pareja no está ordenada** —A con B es lo mismo que B con A—, y por eso
necesita el índice único sobre el par normalizado, que no es una restricción habitual.

**Jugadores e historial: no hay entidad «historial».** El historial **es una consulta** sobre
`match_participant` unida a `match`. Crear una tabla de historial duplicaría lo que ya está.

**Jugadores y ranking: no hay entidad «ranking» tampoco.** El global es una **vista** que no
almacena nada. El de la sala **no existe en la base**: incluye invitados, así que ninguna consulta
podría producirlo, y muere con la sala.

---

# 7. Modelo Entidad-Relación

```
player (player_id) ──< friendship >── player          N:M reflexiva, con estado
player (player_id) ──< room_invitation >── room       N:M con atributos, muere con la sala
player (player_id) ──< match_participant >── match    N:M con atributos: puesto y resultado
room   (room_id)   ──< match                          1:N, la sala juega varias partidas
match  (match_id)  ──o match_state                    1:0..1, solo mientras está en curso
player (player_id) ──o password_recovery              1:0..1, un código vigente como mucho
player (player_id) ──< report >── player              N:M reflexiva, situada en room y match
player (player_id) ──< sanction >── sanction_level    1:N, y el nivel es opcional
turn_layout        ──< build_allowance                1:1..N
match_participant + match ──> player_ranking          vista: no almacena nada
```

El diagrama completo, con todos los atributos, las llaves y las cardinalidades, está en
[`code/database/Modelo-BD-Torres.puml`](code/database/Modelo-BD-Torres.puml), listo para generar.

---

# 8. Modelo relacional

```
player (
    PK  player_id            integer generado
        username             varchar(20)   NOT NULL, único sobre lower(username)
        email                varchar(254)  NOT NULL, único sobre lower(email)
        password_hash        varchar(255)  NOT NULL
        avatar_reference     varchar(255)  NULL
        created_at           timestamptz   NOT NULL
)

friendship (
    PK, FK  requester_id     integer  → player(player_id)
    PK, FK  addressee_id     integer  → player(player_id)
            status           varchar(8)   NOT NULL  { pending | accepted }
            requested_at     timestamptz  NOT NULL
            responded_at     timestamptz  NULL
)

room (
    PK  room_id              integer generado
        code                 varchar(4)   NOT NULL, único solo entre las abiertas
        visibility           varchar(7)   NOT NULL  { public | private }
        created_at           timestamptz  NOT NULL
        closed_at            timestamptz  NULL
)

room_invitation (
    PK, FK  room_id             integer  → room(room_id)
    PK, FK  invited_player_id   integer  → player(player_id)
        FK  inviter_player_id   integer  → player(player_id)
            channel             varchar(8)   NOT NULL  { in_game | email }
            created_at          timestamptz  NOT NULL
)

match (
    PK  match_id             integer generado
    FK  room_id              integer      → room(room_id), ON DELETE RESTRICT
        status               varchar(12)  NOT NULL  { in_progress | finished }
        player_count         smallint     NOT NULL, entre 2 y 4
        started_at           timestamptz  NOT NULL
        finished_at          timestamptz  NULL
        end_reason           varchar(12)  NULL  { completed | abandoned | interrupted }
)

match_participant (
    PK, FK  match_id         integer  → match(match_id)
    PK      seat_number      smallint entre 1 y 4
        FK  player_id        integer  → player(player_id), único dentro de la partida
            final_score      integer   NULL
            final_position   smallint  NULL, único dentro de la partida
            was_withdrawn    boolean   NOT NULL
)

match_state (
    PK, FK  match_id         integer  → match(match_id)
            round_number       smallint  NOT NULL, entre 1 y 3
            turn_number        smallint  NOT NULL
            active_seat_number smallint  NOT NULL, entre 1 y 4
            state_document     jsonb     NOT NULL
            saved_at           timestamptz NOT NULL
)

password_recovery (
    PK, FK  player_id        integer  → player(player_id)
            code             varchar(16)  NOT NULL
            created_at       timestamptz  NOT NULL
)

report (
    PK  report_id                integer generado
    FK  reporter_player_id       integer  → player(player_id)
    FK  reported_player_id       integer  → player(player_id)
    FK  room_id                  integer  → room(room_id)          NOT NULL
    FK  match_id                 integer  → match(match_id)        NULL
        reported_text            varchar(200) NOT NULL
        created_at               timestamptz  NOT NULL
)

sanction (
    PK  sanction_id          integer generado
    FK  player_id            integer   → player(player_id)
    FK  level                smallint  → sanction_level(level)     NULL
        is_permanent         boolean      NOT NULL
        threshold_at         timestamptz  NOT NULL
        effective_from       timestamptz  NOT NULL
        effective_until      timestamptz  NULL
)

turn_layout (
    PK  player_count         smallint entre 2 y 4
    PK  round_number         smallint entre 1 y 3
        turn_count           smallint NOT NULL
)

build_allowance (
    PK, FK  player_count     smallint  ┐
    PK, FK  round_number     smallint  ┘ → turn_layout(player_count, round_number)
    PK      turn_number      smallint
            building_count   smallint NOT NULL
)

action_cost (
    PK  action_code          varchar(32)
        action_point_cost    smallint NOT NULL
)

sanction_level (
    PK  level                smallint entre 1 y 3
        ban_duration         interval NOT NULL
)

system_parameter (
    PK  parameter_code       varchar(48)
        parameter_value      varchar(32) NOT NULL
)

player_ranking = VISTA sobre player ⋈ match_participant ⋈ match
                 WHERE match.status = 'finished'
                   AND match.end_reason <> 'interrupted'
                   AND match_participant.final_position IS NOT NULL
```

## 8.1 Cómo se transformaron las relaciones N:M

**Las tres N:M del modelo se resolvieron de tres maneras distintas**, y esa diferencia es
significativa:

| N:M | Cómo se transformó | Por qué así |
|---|---|---|
| **Jugador ↔ Jugador (amistad)** | Tabla `friendship`, con la PK compuesta por las dos claves ajenas **más** `status` y las dos fechas | La relación **tiene atributos propios** —en qué estado está y cuándo— |
| **Jugador ↔ Partida** | Tabla `match_participant`, con PK `(match_id, seat_number)` | La relación tiene **cuatro atributos propios**, y el puesto de mesa es tan identificador que sustituye al jugador en la llave |
| **Jugador ↔ Sala** | **No se transformó: no está en la base** | Uno de los extremos puede ser un invitado, **del que no se guarda nada**. La tabla puente quedaría siempre incompleta |

**Atributos multivaluados: ninguno**, con una excepción deliberada. Ningún atributo guarda listas
separadas por comas ni grupos repetidos. La excepción es `match_state.state_document`, que es un
documento JSON, y está justificada en el apartado siguiente.

---

# 9. Tercera Forma Normal

## 9.1 Primera Forma Normal

**Qué hay que comprobar:** que todos los atributos sean atómicos, que no haya grupos repetitivos y
que ningún atributo guarde varios valores.

**Catorce de las quince tablas están en 1FN sin discusión.** No hay ninguna lista separada por
comas, ninguna columna del tipo `jugador_1`, `jugador_2`, `jugador_3`, y ningún atributo que
guarde más de un dato. Conviene señalar dos sitios donde la tentación existía y se evitó:

- **Los cuatro jugadores de una partida** no son cuatro columnas de `match`: son cuatro filas de
  `match_participant`. Si fueran columnas, el modelo tendría un grupo repetitivo y no podría
  representar una partida de dos sin dejar columnas vacías.
- **Las prohibiciones previas de una cuenta** no son un contador en `player`: son las filas de
  `sanction`. El contador sería un dato derivado guardado aparte.

**La excepción, consciente: `match_state.state_document` es un documento JSON**, y un documento
no es un valor atómico. Esto **es una desviación de 1FN**, no un tecnicismo, y se toma a propósito
por tres razones:

1. **Nadie consulta dentro.** Ninguna operación filtra, ordena ni une por el contenido del tablero.
   El único consumidor es el servidor, y **lo lee entero**.
2. **Normalizarlo repartiría las reglas del juego por el esquema.** Haría falta una tabla de
   flores, otra de orugas, otra de cartas en mano, otra de jardineras, con sus restricciones de
   niveles y adyacencias; y reconstruir el tablero exigiría varias consultas para hacer exactamente
   lo mismo que hoy hace una.
3. **El tablero pertenece a la capa de dominio**, que es quien tiene las reglas. La base solo tiene
   que devolverlo tal como se lo dieron.

**Qué se pierde al no normalizarlo:** la posibilidad de preguntarle a SQL cosas sobre el tablero
—«en cuántas partidas hay una oruga en el nivel 3»—. **Ninguna funcionalidad del sistema pregunta
eso**, y si algún día se pidiera, la respuesta correcta sería calcularla en el dominio, no
normalizar el estado vivo de la partida.

## 9.2 Segunda Forma Normal

**Qué hay que comprobar:** que estando en 1FN, ningún atributo que no sea llave dependa **solo de
una parte** de una llave primaria compuesta.

**En nueve de las quince tablas la comprobación es inmediata**, porque **su llave primaria es
simple**: `player`, `room`, `match`, `match_state`, `password_recovery`, `report`, `sanction`,
`action_cost`, `sanction_level` y `system_parameter`. Con una llave de un solo atributo **no puede
existir una dependencia parcial**, porque no hay «parte» de la llave de la que depender. La 2FN se
cumple por construcción.

**Las cinco con llave compuesta sí hay que mirarlas una a una:**

| Tabla | Llave | ¿Depende algo de solo una parte? |
|---|---|---|
| `friendship` | `(requester_id, addressee_id)` | **No.** `status`, `requested_at` y `responded_at` describen **la relación entre los dos**, no a ninguno de ellos por separado. El estado de A no existe sin B |
| `room_invitation` | `(room_id, invited_player_id)` | **No.** `channel` y `created_at` describen **esa invitación concreta**. Y `inviter_player_id` tampoco: quién invita depende de la invitación, no de la sala —en la misma sala pueden invitar dos personas distintas— ni del invitado |
| `match_participant` | `(match_id, seat_number)` | **No.** `player_id`, `final_score`, `final_position` y `was_withdrawn` describen **a ese jugador en esa partida**. El puesto 2 no significa nada fuera de su partida, y el resultado de un jugador cambia de una partida a otra |
| `turn_layout` | `(player_count, round_number)` | **No.** `turn_count` depende de las dos: con 2 jugadores son 4 turnos en las tres rondas, y con 3 y 4 son 4/3/3 |
| `build_allowance` | `(player_count, round_number, turn_number)` | **No.** `building_count` depende de las tres. Con 3 jugadores la ronda 1 da 3 construcciones en los dos primeros turnos y 2 en los siguientes: **cambiar solo el turno cambia el valor** |

**`room_invitation` merece un comentario**, porque es donde una dependencia parcial habría sido
fácil. Si la tabla guardara, por ejemplo, el **código de la sala**, ese atributo dependería solo de
`room_id` —una parte de la llave— y rompería la 2FN. No lo guarda: el código se obtiene de `room`
cuando hace falta.

## 9.3 Tercera Forma Normal

**Qué hay que comprobar:** que estando en 2FN, ningún atributo que no sea llave dependa de otro
atributo que tampoco lo sea.

| Tabla | ¿Cumple? | Por qué |
|---|:-:|---|
| `player` | ✔ | Los cinco atributos dependen de `player_id` y **de nada más entre ellos**: el correo no determina el avatar, ni el nombre de usuario la fecha de alta. `username` y `email` son **claves candidatas**, y depender de una clave candidata no viola la 3FN |
| `friendship` | ✔ | `status`, `requested_at` y `responded_at` dependen de la pareja. **`status` no determina `responded_at`**: lo restringe —si está pendiente, la fecha está vacía—, pero dos amistades aceptadas tienen fechas distintas. Una restricción no es una dependencia funcional |
| `room` | ✔ | Código, visibilidad y las dos fechas dependen de `room_id`. **`closed_at` no determina nada**: solo dice si la sala está abierta |
| `room_invitation` | ✔ | Canal, fecha y quién invita dependen de la pareja, y ninguno determina a otro |
| `match` | ✔ | **Aquí sí había que mirar:** `status` parece determinar si `finished_at` y `end_reason` están vacíos. Otra vez, **restringe pero no determina**: dos partidas terminadas tienen fechas y motivos distintos. `player_count` **no se deriva** de las participaciones, por las razones ya dichas |
| `match_participant` | ✔ | **El punto delicado del modelo, y se sostiene.** `final_position` **parece** derivarse de `final_score`, pero **no es así**: el desempate usa el número de jardineras en las que se puntuó, que no se guarda, y luego el puesto de mesa. Dos jugadores con la misma puntuación **pueden quedar en puestos distintos**, así que la puntuación no determina el puesto |
| `match_state` | ✔ | Ronda, turno, puesto activo y fecha dependen de `match_id`. **Los tres números no se repiten dentro del documento**: son la frontera que identifica el estado, y tenerlos fuera evita dos fuentes para el mismo dato |
| `password_recovery` | ✔ | `code` y `created_at` dependen del jugador. **La caducidad no es un atributo**: se calcula sobre `created_at`. Guardarla sería guardar la suma de dos datos que ya están, y dejaría códigos con caducidades distintas si el plazo cambiara |
| `report` | ✔ | **Aquí hay una dependencia que había que comprobar:** `match_id` determina `room_id`, porque toda partida pertenece a una sala. **No es transitiva indebida**: `match_id` está **vacío** en los reportes hechos fuera de partida, y en esos la sala es el único sitio donde está la información. Un atributo que puede ser nulo no puede determinar a otro que es obligatorio. Ver el comentario de abajo |
| `sanction` | ✔ | **`level` determina la duración del castigo, pero la duración no está en esta tabla**: está en `sanction_level`, que es exactamente la descomposición que evita la transitividad. Si `ban_duration` estuviera aquí, `sanction_id → level → ban_duration` **sí sería** una dependencia transitiva y la tabla estaría en 2FN y no en 3FN. **`effective_until` no se deriva de `effective_from` + nivel** de forma fiable: se calcula una vez, al entrar en vigor, y se guarda para que un cambio del catálogo no altere castigos ya impuestos |
| `turn_layout`, `build_allowance`, `action_cost`, `sanction_level` | ✔ | Un solo atributo no clave en cada una: no hay entre qué haya dependencia |
| `system_parameter` | ✔ | `parameter_value` depende de `parameter_code`. Es una tabla clave-valor, que es una decisión consciente: agrupa números de naturaleza distinta —cuentas, tamaños y plazos— para poder cambiarlos sin recompilar, a costa de que el tipo se resuelva en la capa de servicios |

### El caso de `report`, explicado del todo

Es la única tabla donde una dependencia entre atributos no clave existe de verdad: **si
`match_id` tiene valor, entonces `room_id` queda determinado**, porque la partida sabe en qué sala
se jugó.

**Por qué no procede descomponer:** `match_id` es **opcional**. Los reportes hechos en la sala
fuera de toda partida —que son un caso corriente— **no tienen partida**, y en ellos `room_id` es el
único sitio donde consta dónde ocurrió. Quitar `room_id` y obtenerlo de la partida dejaría esos
reportes sin ubicación, y **rompería el recuento de ocasiones**, que necesita distinguir
«esta sala, fuera de partida» como una ocasión propia.

**Qué se hace en su lugar:** la coherencia entre los dos —que la partida referenciada pertenezca a
la sala referenciada— **se garantiza en la operación que inserta el reporte**, que es la misma que
sabe dónde está el jugador. Es una redundancia de un solo atributo, controlada, y la alternativa
—perder la ubicación de la mitad de los reportes— es peor.

## 9.4 Conclusión

**Las quince tablas están en 3FN**, con dos observaciones explícitas: `match_state.state_document`
se aparta de 1FN a propósito, y `report` conserva una redundancia controlada de un atributo. Las
dos están justificadas por el dominio, no por comodidad, y las dos dicen qué se pierde a cambio.

**Ninguna tabla necesitó descomponerse durante este análisis.** La que habría estado en 2FN y no en
3FN —`sanction` con la duración dentro— ya venía descompuesta en `sanction` y `sanction_level`.

---

# 10. Revisión de consistencia

## 10.1 Cada caso de uso contra las entidades que necesita

| Caso de uso | Operaciones CRUD | Entidades | ¿Tiene todo lo que necesita? |
|---|---|---|---|
| `CU-01` Iniciar sesión | CR-02, CR-37 | `player`, `sanction` | ✔ |
| `CU-02` Crear cuenta | CR-01 | `player` | ✔ |
| `CU-03` Recuperar acceso | CR-31, CR-32, CR-06, CR-33 | `player`, `password_recovery` | ✔ |
| `CU-04` Jugar como invitado | **ninguna** | — | ✔ **De un invitado no se guarda nada**, a propósito |
| `CU-05` Cambiar el idioma | **ninguna** | — | ✔ Preferencia del equipo, no de la cuenta |
| `CU-06` Modificar el perfil | CR-04, CR-05 | `player` | ✔ |
| `CU-07` Eliminar la cuenta | CR-07, CR-23 | `player` y todas sus dependientes | ✔ |
| `CU-08` Enviar solicitud | CR-03, CR-08, CR-09 | `player`, `friendship` | ✔ |
| `CU-09` Responder solicitud | CR-09, CR-10, CR-11 | `friendship` | ✔ |
| `CU-10` Historial | CR-19 | `match`, `match_participant` | ✔ |
| `CU-11` Detalle de una partida | CR-20 | `match`, `match_participant`, `player` | ✔ |
| `CU-12` Ranking global | CR-38 | `player_ranking` | ✔ |
| `CU-13` Entrar a la sala | CR-37 | `sanction`; lo demás es memoria | ✔ |
| `CU-14` Unirse a una sala | CR-13 | `room` | ✔ La ocupación sale de memoria, y debe |
| `CU-15` Crear una sala | CR-12 | `room` | ✔ |
| `CU-16` Invitar jugadores | CR-03, CR-09, CR-15 | `player`, `friendship`, `room_invitation` | ✔ |
| `CU-17` Responder una invitación | CR-16, CR-13, CR-17 | `room_invitation`, `room` | ✔ |
| `CU-18` Salir de la sala | CR-14, CR-17 | `room`, `room_invitation` | ✔ |
| `CU-19` Iniciar la partida | CR-37, CR-18, CR-24, CR-27, CR-39 | `sanction`, `match`, `match_participant`, `match_state`, catálogos | ✔ |
| `CU-20` Eliminar un amigo | CR-11 | `friendship` | ✔ |
| `CU-21` Expulsar a un jugador | **ninguna** | — | ✔ Toca solo la memoria de la sala |
| `CU-22` Enviar un mensaje | **ninguna** | — | ✔ El chat no se guarda |
| `CU-23` Reportar a un jugador | CR-34, CR-35, CR-36, CR-39 | `report`, `sanction`, `sanction_level`, `system_parameter` | **Con una salvedad**: la sanción diferida. Ver apartado 13 |
| `CU-24` Preparar la partida | CR-28 | `match_state` | ✔ |
| `CU-25` Jugar un turno | CR-28, CR-39, CR-22, CR-25, CR-30 | `match_state`, catálogos, `match`, `match_participant` | ✔ |
| `CU-26` Reconectar | CR-26 | `match_participant` | ✔ |
| `CU-27` Volver a una partida reanudada | CR-21, CR-29, CR-22, CR-25, CR-30 | `match`, `match_state`, `match_participant` | ✔ El pase del invitado va en el documento de estado |
| `CU-28` Abandonar la partida | CR-26, CR-22, CR-25, CR-30 | `match_participant`, `match`, `match_state` | ✔ |

**Las ocho historias de usuario** también están cubiertas: `HU-01` cerrar salas al arrancar → CR-14
· `HU-02` y `HU-03` inactividad → memoria, y CR-14 al quedar vacía · `HU-04` fin de partida →
CR-22, CR-25, CR-30 · `HU-05` ranking de sala → memoria · `HU-06` caducidad de la sanción → no
necesita escritura: **la sanción deja de aplicarse sola** · `HU-07` registro de eventos → log4net,
en archivo · `HU-08` salir del juego → cliente.

## 10.2 Las nueve comprobaciones pedidas

| # | Comprobación | Resultado |
|---|---|---|
| 1 | **Todo lo que debe persistirse tiene dónde almacenarse** | ✔ salvo el estado «sanción diferida», apartado 13 |
| 2 | **No hay funcionalidades que pidan datos no representados** | ✔ Las 26 operaciones sobre base de datos tienen sus columnas |
| 3 | **No hay entidades sin razón funcional** | ✔ Las dieciséis aparecen en la matriz CRUD con al menos una operación |
| 4 | **Los atributos son necesarios** | ✔ Se revisaron uno a uno. Dos que parecían redundantes —`player_count` y `final_position`— **no lo son**, y el apartado 5 explica por qué |
| 5 | **Los actores coinciden con los casos de uso** | ✔ Solo aparecen Jugador, Invitado, Usuario sin cuenta y El sistema. **No hay Administrador**, porque no existe en el proyecto |
| 6 | **Las reglas pueden representarse** | ✔ La mayoría como restricciones del motor; tres **tienen que ser lógica de aplicación**, y están abajo |
| 7 | **Las cardinalidades coinciden con el comportamiento** | ✔ Con dos que se salen de lo esperable y son correctas: `match` 1:**0..4** y `sanction_level` 1:**0..N** opcional |
| 8 | **No hay información duplicada** | ✔ Con una redundancia controlada de un atributo en `report`, justificada en 9.3 |
| 9 | **Compatible con PostgreSQL** | ✔ Ver abajo |

## 10.3 Reglas que el motor no puede hacer cumplir

Tres, y conviene que estén escritas porque son las que se escapan si nadie las vigila:

| Regla | Por qué no puede ser una restricción | Dónde vive |
|---|---|---|
| **No se puede eliminar una cuenta que participa en una partida en curso** | Una restricción de borrado no sabe distinguir una partida en curso de una terminada | La operación de CR-07, que consulta antes |
| **El estado existe solo mientras la partida está en curso** | Ninguna restricción puede obligar a borrar una fila de otra tabla | La transacción que termina la partida escribe el resultado y borra el estado a la vez |
| **La partida de un reporte pertenece a su sala** | Es la redundancia controlada de 9.3 | La operación que inserta el reporte, que sabe dónde está el jugador |

## 10.4 Compatibilidad con PostgreSQL

| Lo que usa el modelo | Cómo se resuelve |
|---|---|
| Identificadores generados | `integer GENERATED ALWAYS AS IDENTITY` |
| Unicidad sin distinguir mayúsculas | **Índice único sobre `lower(...)`**, que PostgreSQL permite por ser índice sobre expresión |
| Código único **solo entre salas abiertas** | **Índice único parcial** con `WHERE closed_at IS NULL` |
| Pareja no ordenada en `friendship` | **Índice único sobre `(least(...), greatest(...))`** |
| Documento del tablero | Tipo `jsonb` |
| Duración de las prohibiciones | Tipo `interval` |
| Marcas de tiempo | `timestamptz`, con zona |
| Coherencia entre columnas | Restricciones `CHECK` con varias columnas |

**Las cuatro de en medio son específicas de PostgreSQL** y no existirían en otros motores: el
modelo **aprovecha el motor elegido**, no se limita al mínimo común.

---

# 11. Decisiones justificadas

| # | Decisión | En qué se apoya |
|---|---|---|
| 1 | **El jugador, la cuenta y el perfil son una sola entidad** | El perfil tiene dos atributos, y uno de ellos es la credencial de acceso. Separarlos sería una relación 1:1 sin atributos que la justifiquen |
| 2 | **Los invitados no tienen tabla, ni siquiera con una marca** | Es la promesa del sistema: de un invitado no queda ninguna fila. Lo contrario obligaría a que tres tablas admitieran identidades temporales |
| 3 | **Los miembros de una sala no son una tabla** | Una tabla que solo contuviera cuentas **nunca sabría si la sala está vacía**, que es lo que decide si la sala existe |
| 4 | **La sala se cierra, no se borra** | Sus partidas la referencian. La clave ajena con borrado restringido lo garantiza |
| 5 | **`player_count` se guarda aunque parezca derivable** | Los invitados no dejan fila y una cuenta borrada se lleva la suya |
| 6 | **`final_position` se guarda aunque parezca derivable** | El desempate usa un dato que no se guarda: el número de jardineras puntuadas |
| 7 | **`was_withdrawn` es un booleano y no dos columnas** | Agotar la ventana y rendirse tienen **el mismo efecto**, y ningún caso de uso pregunta cuál fue |
| 8 | **La duración de las prohibiciones vive en un catálogo aparte** | Es justo la descomposición que mantiene `sanction` en 3FN |
| 9 | **El tablero es un documento JSON** | Nadie consulta dentro, y normalizarlo repartiría las reglas del juego por el esquema |
| 10 | **`room_invitation` pasa a llave compuesta `(room_id, invited_player_id)`** | **Cambio de este análisis.** El par ya era la identidad y estaba impuesto con un índice único; el identificador generado no lo referenciaba nadie |
| 11 | **Los catálogos no tienen CRUD de mantenimiento** | No hay rol de administración. Se cargan por script |
| 12 | **El ranking global es una vista y el de la sala no existe en la base** | El global se puede calcular; el de la sala incluye invitados, así que ninguna consulta podría producirlo |

---

# 12. Puntos pendientes de definición

Solo **tres**, y ninguno impide implementar el resto del modelo.

## 12.1 El estado «sanción diferida» no se puede representar

**El problema.** Cuando el umbral se cruza **durante una partida**, la regla dice que la sanción
entra en vigor **al terminarla** y que su duración empieza a contar entonces. Pero en el instante
en que se cruza el umbral **todavía no se sabe cuándo terminará la partida**, y el modelo exige
que `effective_from` y `effective_until` tengan valor. **No hay forma de escribir la fila en ese
momento.**

**Qué está definido:** que la sanción no interrumpe la partida, que entra en vigor al terminarla y
que la duración cuenta desde entonces. **Qué no está definido:** en qué momento se escribe la fila.

**Las dos salidas:**

| Opción | Qué implica | Coste |
|---|---|---|
| **A. Escribir la fila al entrar en vigor**, cuando la partida termina, conservando en `threshold_at` el instante anterior | **No cambia el esquema.** El estado diferido vive en memoria mientras dura la partida | Si el servidor cae entre el umbral y el fin de la partida, **la sanción no se escribe**. No se pierde para siempre: los reportes siguen ahí y el contador sigue en cinco, así que el siguiente reporte la vuelve a disparar. Pero entre medias el jugador podría entrar a salas |
| **B. Escribir la fila al cruzar el umbral**, con `effective_from` vacío mientras está diferida | El estado diferido **sobrevive a una caída** | **Cambia el esquema**: `effective_from` y `effective_until` pasan a admitir vacío, y la restricción que hoy obliga a que una sanción temporal tenga las dos fechas hay que rehacerla para admitir un tercer caso |

**Recomendación: la opción A**, porque no cambia el esquema y porque lo que se pierde en el caso
malo —una caída justo en esa ventana— **se recupera solo** en cuanto llegue el siguiente reporte.
Pero **es una decisión del equipo**, no una deducción: si se considera que un jugador no debe poder
colarse en una sala tras un reinicio oportuno, la opción B es la correcta y el esquema hay que
tocarlo.

## 12.2 El periodo del latido y los latidos perdidos

**Qué falta:** los dos números que deciden cuándo se da una conexión por perdida.

**Qué está definido:** que la detección se hace **por latido**, con un periodo y un número de
latidos perdidos **declarados**. La tabla `system_parameter` tiene sitio para los dos.

**A qué afecta:** al marcado de inactividad, a la apertura de la ventana de reconexión y al
traspaso del anfitrión. **No afecta al esquema**, solo a dos filas de configuración, y por eso el
script de carga **las deja fuera a propósito** en vez de inventar los valores.

## 12.3 La longitud y el alfabeto del código de recuperación

**Qué falta:** cuántos caracteres tiene y de qué alfabeto salen.

**Qué está definido:** que es un código, que llega por correo, que se teclea dentro de la
aplicación y que **caduca a los cinco minutos**. El prototipo dibuja cuatro dígitos, pero **ninguna
fuente lo fija**.

**A qué afecta:** al tamaño de `password_recovery.code`, hoy holgado a propósito, y a la
comprobación de formato de CU-03. **No afecta a ninguna relación ni a ninguna otra tabla.**

---

# 13. Lo que este análisis cambió

| Qué | Dónde |
|---|---|
| **`room_invitation` pierde su identificador generado** y pasa a llave compuesta `(room_id, invited_player_id)`, con la que el índice único que había deja de hacer falta | `schema.sql`, `Modelo-BD-Torres.puml` |
| **Queda documentado el hueco de la sanción diferida**, con las dos salidas y una recomendación | Apartado 12.1 |
| **Queda documentado qué reglas no puede hacer cumplir el motor** y dónde viven | Apartado 10.3 |

**Lo que no cambió, y se comprobó:** las dieciséis entidades siguen siendo las necesarias, ninguna
sobra, y no falta ninguna. Las cinco que más parecían faltar —miembros de la sala, anfitrión, chat,
ranking de sala y pase del invitado— tienen su sitio, y el apartado 4.1 dice cuál.
