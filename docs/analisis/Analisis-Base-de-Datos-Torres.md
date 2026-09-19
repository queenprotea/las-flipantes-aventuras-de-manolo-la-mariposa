# Análisis de la base de datos — Juego de Torres

**Documento:** `Analisis-Base-de-Datos-Torres.md` · 10 sep 2026
**Implementación:** [`code/database/schema.sql`](code/database/schema.sql) ·
diagrama: [`code/database/Modelo-BD-Torres.puml`](code/database/Modelo-BD-Torres.puml)

---

## 1. Objetivo del modelo

El modelo tiene que sostener **veintiocho casos de uso** y las reglas del juego, y nada más. Su
objetivo no es representar el juego de Torres: es **guardar lo que tiene que sobrevivir**. Todo lo
demás —el tablero mientras se juega, quién está en cada sala, el chat, el reloj— vive donde le
corresponde, y este documento dice por qué en cada caso.

**Tres decisiones gobiernan el modelo entero.** No son preferencias de diseño: cada una viene de
una regla del sistema, y de ellas se deduce casi toda la forma que tiene el esquema.

1. **De los invitados no se guarda ninguna fila.** Un invitado es una identidad temporal con un
   alias libre; no tiene historial, ni ranking, ni amigos, y no deja rastro. En consecuencia,
   **todo lo que puede contener invitados no puede vivir en la base de datos**, porque la tabla
   estaría siempre incompleta y no podría responder la pregunta que importa. El caso extremo lo
   demuestra: si los miembros de una sala fueran una tabla y solo guardara a los que tienen
   cuenta, el sistema **nunca sabría si una sala está vacía**, que es exactamente la condición que
   decide si la sala sigue existiendo.
2. **El servidor es la única autoridad sobre el estado.** El cliente muestra; no decide. Por eso
   el estado vivo de una partida se guarda, pero **solo en la frontera de un turno**, y por eso no
   se guarda el reloj ni los puntos de acción restantes.
3. **Se guarda lo que alguien va a volver a consultar.** El historial, el ranking y la convivencia
   se consultan después. La secuencia de jugadas, las sesiones y el desglose de puntos por ronda
   no los consulta nadie, así que no existen.

---

## 2. Qué entra en la base y qué no

| Dónde vive | Qué | Por qué |
|---|---|---|
| **Permanente en la base** | Jugadores, amistades, salas, partidas terminadas, participaciones, reportes y sanciones | Sobreviven a la sala, a la partida y al reinicio del servidor |
| **Temporal en la base** | Invitaciones pendientes, solicitudes de amistad pendientes, códigos de recuperación y el estado de la partida en curso | Tienen que sobrevivir a una caída del servidor, pero se borran cuando dejan de tener sentido |
| **Memoria del servidor** | Quién está en cada sala, quién es el anfitrión, los colores, el chat, el ranking de la sala, el estado de conexión, el reloj del turno y los puntos de acción restantes | **Todo esto puede contener invitados**, o muere con la sala. Una tabla no podría representarlo entero |
| **Derivado** | El ranking global, el número de jugadores de una sala y la duración de una partida | Se calculan; almacenarlos sería duplicar |
| **Archivo en disco** | La imagen del avatar | La base guarda solo su referencia |
| **No se guarda en absoluto** | La secuencia de jugadas, las sesiones y los tokens, el desglose de puntos por ronda, los mensajes de chat y el registro de eventos | Nadie los consulta. El registro de eventos va a log4net, y tiene prohibido contener datos personales |

---

## 3. `Player`: una sola entidad para cuatro cosas que parecen distintas

Esta es la decisión más importante del modelo, porque es donde más fácil resulta multiplicar
tablas sin necesidad. **Cuenta, perfil, jugador de una sala y jugador de una partida no son cuatro
entidades: son una entidad, un conjunto de atributos y dos roles.**

| Concepto | Qué es en el modelo | Por qué |
|---|---|---|
| **Cuenta** | La entidad **`player`** | Es la única identidad persistente del sistema |
| **Perfil** | **Atributos de `player`**: `username` y `avatar_reference` | El perfil tiene exactamente dos datos, y los dos son 1:1 con la cuenta y obligatorios de consultar en el mismo momento que ella. Una tabla `profile` separada sería una relación 1:1 sin ningún atributo que la justifique, y obligaría a unir dos tablas para mostrar un nombre. Además el `username` **no es solo perfil**: es la credencial de acceso y el único criterio de búsqueda de jugadores, así que no puede vivir fuera de la cuenta |
| **Jugador dentro de una sala** | **No es una entidad de la base**: es un elemento del estado en memoria de la sala | Un invitado puede estar en una sala y **puede ser su anfitrión**. Una tabla de miembros solo podría contener a los que tienen cuenta, y entonces mentiría sobre la ocupación y sobre si la sala está vacía |
| **Jugador dentro de una partida** | La entidad **`match_participant`** | Aquí sí hace falta una entidad, porque el jugador dentro de una partida **tiene datos propios que no son suyos ni de la partida**: su puesto de mesa, su puntuación final, su puesto final y si abandonó. Esos datos solo existen en la intersección de los dos |

### Cómo se representa cada cosa que un `Player` puede hacer

| El jugador… | Cómo se representa | Entidad |
|---|---|---|
| **Crea una sala** | **No se guarda quién la creó.** El anfitrión es un rol que cambia de persona —se hereda al siguiente cuando el anfitrión sale o se marca inactivo— y **puede recaer en un invitado**. Una columna `created_by` estaría vacía en las salas creadas por invitados y sugeriría una propiedad sobre la sala que no existe | — |
| **Está en una sala** | Estado en memoria de la sala | — |
| **Participa en una partida** | Una fila de `match_participant` con su puesto de mesa | `match_participant` |
| **Tiene historial** | Sus filas de `match_participant` unidas a `match` | `match`, `match_participant` |
| **Aparece en el ranking global** | Una **vista** calculada sobre las partidas terminadas. No almacena nada | `player_ranking` |
| **Tiene amigos** | Una fila de `friendship` por pareja | `friendship` |
| **Recibe invitaciones a sala** | Una fila de `room_invitation` mientras está pendiente | `room_invitation` |
| **Tiene nombre de usuario y avatar** | Dos atributos de `player`; la imagen vive en disco | `player` |
| **Abandona o se rinde en una partida** | La marca `was_withdrawn` de su participación | `match_participant` |

**Por qué `was_withdrawn` es un solo booleano y no dos.** Un jugador puede dejar una partida a
medias de dos maneras: agotando su ventana de reconexión o rindiéndose. **El efecto es idéntico**
—sus caballeros salen del tablero, sus construcciones se quedan, deja de contar para el resultado
y para «el de menor puntuación», y la partida sigue contando para su ranking— y **ningún caso de
uso pregunta cuál de las dos fue**. Distinguirlas sería guardar un dato que nadie lee.

**Por qué el invitado no tiene tabla, ni siquiera una con `is_guest`.** Se estudió porque
simplificaría las claves ajenas, y no procede: obligaría a crear y borrar filas de identidades que
por definición no deben dejar rastro, y `match_participant`, `report` y `friendship` tendrían que
admitir referencias a jugadores que el sistema promete no guardar. El invitado existe **solo**
dentro del documento de estado de la partida que está jugando, con su alias y su pase de asiento,
y desaparece con ella.

---

## 4. Entidades definitivas

**Diez entidades**, cinco catálogos de parámetros y una vista derivada.

---

### 4.1 `player`

**Por qué es necesaria.** Es la identidad persistente del sistema: sin ella no hay inicio de
sesión, ni historial, ni ranking, ni amigos, ni invitaciones, ni reportes. Absorbe el perfil.

**Casos de uso que la requieren:** CU-01, CU-02, CU-03, CU-06, CU-07, CU-08, CU-09, CU-10, CU-11,
CU-12, CU-16, CU-17, CU-19, CU-20, CU-23.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `player_id` | entero generado | Sí | **Clave primaria.** Es el **único identificador de persona que puede aparecer en el registro de eventos** |
| `username` | texto de 3 a 20: letras, dígitos y guion bajo | Sí | **Único sin distinguir mayúsculas.** Credencial, nombre visible y único criterio de búsqueda |
| `email` | texto | Sí | **Único sin distinguir mayúsculas.** Recuperación de acceso y destino de las invitaciones |
| `password_hash` | texto | Sí | **Hash BCrypt.** La contraseña nunca se guarda ni se registra |
| `avatar_reference` | texto | **No** | Ruta del archivo en el servidor. Vacía mientras el jugador no suba ninguno; si el archivo desaparece del disco, la aplicación muestra el avatar por defecto y **la referencia no se toca** |
| `created_at` | marca de tiempo con zona | Sí | |

**Clave primaria:** `player_id`.
**Claves foráneas:** ninguna. Es la raíz del modelo.
**Unicidad:** dos índices únicos **sobre el valor en minúsculas** de `username` y de `email`.
«Ana» y «ana» no pueden ser dos cuentas, y el inicio de sesión y la búsqueda no distinguen
mayúsculas.

**Relaciones:** con `friendship` dos veces, con `room_invitation` dos veces, con
`match_participant`, con `report` dos veces, con `sanction` y con `password_recovery`.
**Al eliminarse un jugador, todo lo suyo se va en cascada**, incluidas sus sanciones: eso hace que
un baneo permanente sea evitable borrando la cuenta, y es una consecuencia conocida y aceptada.

---

### 4.2 `friendship`

**Por qué es necesaria.** Una solicitud pendiente y una amistad aceptada son **el mismo hecho en
dos momentos**, no dos entidades: la solicitud aceptada *se convierte* en la amistad. Separarlas
obligaría a mover filas de una tabla a otra al aceptar, y a comprobar dos tablas para saber si dos
jugadores ya tienen algo entre ellos.

**Casos de uso que la requieren:** CU-08, CU-09, CU-20, y CU-16 para listar amigos a los que
invitar.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `requester_id` | referencia a `player` | Sí | **Parte de la clave primaria.** Quien envió la solicitud |
| `addressee_id` | referencia a `player` | Sí | **Parte de la clave primaria.** Quien la responde |
| `status` | *pending* · *accepted* | Sí | **No existe *rejected*:** la solicitud rechazada **se borra** |
| `requested_at` | marca de tiempo | Sí | |
| `responded_at` | marca de tiempo | **No** | Vacía mientras está pendiente |

**Clave primaria:** `(requester_id, addressee_id)`.
**Claves foráneas:** las dos columnas, a `player`, en cascada.
**Cardinalidad:** `player` 1 : 0..N `friendship`, **dos veces**.

**Reglas de integridad que la sostienen:**
- **La pareja invertida es la misma pareja.** Un índice único sobre `(menor, mayor)` de los dos
  identificadores impide que A→B y B→A coexistan. Sin él, dos jugadores podrían enviarse
  solicitudes cruzadas y quedar «dos veces amigos».
- **Nadie es amigo de sí mismo**, por restricción.
- **La dirección solo importa mientras está pendiente**: una vez aceptada, la amistad es simétrica
  y las dos listas de amigos se construyen con la misma fila.
- **No existe el bloqueo.** Nada impide volver a enviar una solicitud tras un rechazo o tras
  eliminar la amistad.

---

### 4.3 `room`

**Por qué es necesaria.** Aunque casi todo lo que ocurre dentro de una sala vive en memoria, la
sala **tiene que existir en la base por dos motivos que no se pueden resolver de otra manera**:
sus partidas la referencian y deben seguir referenciándola cuando la sala ya no esté viva, y su
código tiene que ser único **entre las salas abiertas**, lo que exige un sitio donde comprobarlo.

**Casos de uso que la requieren:** CU-13, CU-14, CU-15, CU-16, CU-17, CU-18, CU-19, CU-23.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `room_id` | entero generado | Sí | **Clave primaria** |
| `code` | texto de 4 caracteres alfanuméricos | Sí | **Único solo entre las salas abiertas.** El alfabeto **excluye los caracteres que se confunden** —sin O, I ni L, sin 0 ni 1— porque se teclea desde un correo |
| `visibility` | *public* · *private* | Sí | **Decide una sola cosa: si la sala aparece en el listado.** Se fija al crearla y no cambia |
| `created_at` | marca de tiempo | Sí | |
| `closed_at` | marca de tiempo | **No** | Vacía mientras está abierta. **La sala vacía se cierra, no se borra** |

**Clave primaria:** `room_id`.
**Claves foráneas:** ninguna. **No guarda miembros, ni anfitrión, ni ranking, ni chat.**

**Por qué el código es único solo entre las abiertas.** Un código de cuatro caracteres se agota
enseguida si es único para siempre. Un índice único **parcial**, restringido a las salas sin fecha
de cierre, permite que el código sea corto y se reutilice cuando la sala muere, que es justo lo
que hace falta para poder teclearlo.

**Por qué se cierra y no se borra.** Sus partidas terminadas la referencian y siguen apareciendo
en el historial. Borrarla rompería el historial o exigiría desreferenciar las partidas, que es
peor. Cerrarla libera el código, elimina sus invitaciones pendientes y conserva la fila.

---

### 4.4 `room_invitation`

**Por qué es necesaria.** Es lo único de una sala que **tiene que sobrevivir a que el destinatario
esté desconectado**: la invitación se conserva y él la encuentra al entrar. Eso la saca de la
memoria y la trae a la base.

**Casos de uso que la requieren:** CU-16, CU-17, y CU-18 al cerrarse la sala.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `room_id` | referencia a `room` | Sí | **Parte de la clave primaria** |
| `inviter_player_id` | referencia a `player` | Sí | **Un invitado no puede invitar** |
| `invited_player_id` | referencia a `player` | Sí | **Parte de la clave primaria.** Siempre dirigida a una cuenta |
| `channel` | *in_game* · *email* | Sí | Cómo se avisó al destinatario |
| `created_at` | marca de tiempo | Sí | |

**Clave primaria:** **`(room_id, invited_player_id)`**, que es la identidad real de la fila: la
regla del sistema es **una sola invitación pendiente por sala y destinatario**. No lleva
identificador generado porque **nada referencia a una invitación**, y el par hace de llave y de
restricción a la vez.
**Claves foráneas:** las dos columnas de la llave, más `inviter_player_id` a `player`.
**Cardinalidad:** `room` 1 : 0..N · `player` 1 : 0..N **dos veces**.

**Por qué no hay token.** La invitación **va siempre dirigida a una cuenta**, así que la seguridad
la da el destinatario, no el secreto del enlace: al canjearla se comprueba que quien la presenta
es la cuenta invitada. El correo lleva **el propio código de la sala**, y por eso el canal de
correo es **un aviso**, no una vía de entrada distinta. Un token único sería un dato que no
protege nada y un índice que no sirve a ninguna consulta.

**Por qué solo existen las pendientes.** Responder la borra, y eso es exactamente lo que la hace
de un solo uso. No hay historial de invitaciones: nadie lo consulta. El **cierre de la sala
también las elimina**, y aquí hay una sutileza de integridad: la clave ajena a `room` está en
cascada, pero **las salas nunca se borran**, así que esa cascada nunca se dispara. **El borrado lo
hace la operación que cierra la sala**, en la misma transacción; la cascada es solo una red de
seguridad.

---

### 4.5 `match`

**Por qué es necesaria.** Es la unidad del historial y del ranking, y **la única prueba de que una
partida quedó a medias** cuando el servidor se cayó: al arrancar, el sistema busca precisamente
las partidas con estado *en curso*.

**Casos de uso que la requieren:** CU-10, CU-11, CU-12, CU-19, CU-25, CU-26, CU-27, CU-28.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `match_id` | entero generado | Sí | **Clave primaria** |
| `room_id` | referencia a `room` | Sí | **Toda partida nace en una sala** |
| `status` | *in_progress* · *finished* | Sí | |
| `player_count` | entero de 2 a 4 | Sí | Cuántos se sentaron |
| `started_at` | marca de tiempo | Sí | |
| `finished_at` | marca de tiempo | **No** | Vacía mientras está en curso |
| `end_reason` | *completed* · *abandoned* · *interrupted* | **No** | Vacío mientras está en curso |

**Clave primaria:** `match_id`.
**Claves foráneas:** `room_id` a `room`, **con borrado restringido**: no se puede borrar una sala
que tiene partidas, lo que es coherente con que las salas se cierren y nunca se borren.
**Cardinalidad:** `room` 1 : 0..N `match` — **una sala juega varias partidas**, y esa es la razón
de que la sala siga abierta cuando una partida termina.

**Por qué `player_count` se guarda y no se deriva.** Parece redundante —bastaría contar las
participaciones— y no lo es: **los invitados no dejan fila**, y una cuenta eliminada se lleva la
suya. Contar participaciones daría 2 en una partida de 4 con dos invitados, y daría 0 en una
partida que solo jugaron invitados. El dato se toma al empezar, cuando todavía se sabe.

**Por qué el estado y el motivo van juntos y no separados.** Una restricción obliga a que una
partida *en curso* no tenga ni fin ni motivo, y una *terminada* tenga los dos. Sin ella el modelo
admitiría una partida terminada sin decir cómo terminó, que es justo lo que el historial muestra.

---

### 4.6 `match_participant`

**Por qué es necesaria.** Es la entidad asociativa entre un jugador y una partida, y existe porque
**tiene datos propios que no pertenecen a ninguno de los dos**: el puesto de mesa, la puntuación
final, el puesto final y si dejó la partida.

**Casos de uso que la requieren:** CU-10, CU-11, CU-12, CU-19, CU-26, CU-27, CU-28.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `match_id` | referencia a `match` | Sí | **Parte de la clave primaria** |
| `seat_number` | entero de 1 a 4 | Sí | **Parte de la clave primaria.** Es **el identificador del jugador dentro de la partida**, y el único que se escribe en el registro de eventos junto al de la cuenta |
| `player_id` | referencia a `player` | Sí | Único dentro de la partida |
| `final_score` | entero ≥ 0 | **No** | Vacía hasta que la partida termina |
| `final_position` | entero de 1 a 4 | **No** | 1 es el ganador. Vacío hasta que termina |
| `was_withdrawn` | booleano | Sí | Dejó la partida antes de terminar: agotó su ventana de reconexión **o se rindió** |

**Clave primaria:** `(match_id, seat_number)`.
**Claves foráneas:** `match_id` a `match` en cascada; `player_id` a `player` en cascada.
**Unicidad:** `(match_id, player_id)` —una cuenta ocupa un solo puesto por partida— y
`(match_id, final_position)` —dos jugadores no pueden quedar en el mismo puesto—.
**Cardinalidad:** `match` 1 : **0..4** `match_participant` · `player` 1 : 0..N.

**Por qué la cardinalidad mínima es 0 y no 2.** Jugaron entre dos y cuatro, pero **solo los que
tienen cuenta dejan fila**. Una partida jugada solo por invitados tiene **cero** participaciones;
existe mientras está en curso, para poder reanudarse, y se elimina al terminar porque no podría
consultarla nadie.

**Por qué el puesto final se guarda en vez de derivarse de la puntuación.** El desempate lo
resuelve el dominio: a igual puntuación gana quien puntuó en más castillos, y si persiste, el
puesto de mesa más bajo. **El número de castillos en los que se puntuó no se guarda**, así que el
orden no se podría reconstruir con una consulta. Se guarda el resultado, no los ingredientes.

**Por qué los puestos guardados pueden tener huecos.** Si ganó un invitado, **ninguna fila tendrá
el puesto 1**. El detalle de una partida lo muestra tal cual: la tabla puede no incluir todos los
puestos, y eso es correcto, no un error de datos.

---

### 4.7 `match_state`

**Por qué es necesaria.** Existe por un solo motivo: **que una partida pueda reanudarse después de
que el servidor se caiga**. Sin ella, una caída perdería la partida entera.

**Casos de uso que la requieren:** CU-19, CU-24, CU-25, CU-26, CU-27.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `match_id` | referencia a `match` | Sí | **Clave primaria y clave ajena a la vez** |
| `round_number` | entero de 1 a 3 | Sí | Ronda que el estado guardado está a punto de jugar |
| `turn_number` | entero > 0 | Sí | |
| `active_seat_number` | entero de 1 a 4 | Sí | A qué puesto le toca |
| `state_document` | documento JSON | Sí | Tablero, castillos, caballeros, rey, cartas obtenidas y usadas, puntuación acumulada, **y los puestos de los invitados con su alias y su pase de asiento** |
| `saved_at` | marca de tiempo | Sí | Último cierre de turno |

**Clave primaria:** `match_id`, que es también la clave ajena. Es una **extensión opcional** de la
partida: comparte su clave y desaparece cuando la partida termina.
**Cardinalidad:** `match` 1 : **0..1** `match_state`.

**Por qué el tablero no está normalizado.** Sería la tentación obvia: una tabla de torres, otra de
caballeros. No procede, y no por comodidad: **ninguna consulta filtra ni une por el tablero**. Solo
lo lee el servidor, y lo lee entero. Normalizarlo repartiría las reglas del juego por el esquema y
obligaría a reconstruir el tablero con varias consultas para hacer exactamente lo mismo. El tablero
pertenece a la capa de dominio; la base solo tiene que devolvérselo tal como se lo dieron.

**Qué no guarda, y por qué.** No guarda el reloj, ni los puntos de acción restantes, ni las
construcciones pendientes del turno. **El estado guardado es siempre una frontera de turno**: se
escribe al empezar la partida y al cerrar cada turno. El turno que quedó a medias se descarta
entero y se juega de nuevo con noventa segundos completos, así que guardar su reloj no serviría
para nada.

**Los tres números no se repiten dentro del documento.** `round_number`, `turn_number` y
`active_seat_number` son columnas, no campos del JSON: son **la frontera** que identifica el
estado, y tenerlas fuera permite comprobar y ordenar sin abrir el documento. Repetirlas dentro
crearía dos fuentes para el mismo dato.

**El pase de asiento del invitado va dentro del documento y no en una columna.** Es el dato que
permite a un invitado recuperar su puesto tras una caída, y **pertenece a un jugador que no tiene
fila en ninguna parte**. Va donde está el resto de lo suyo.

---

### 4.8 `password_recovery`

**Por qué es necesaria.** El código enviado al correo tiene que sobrevivir al tiempo que el
usuario tarda en abrir su bandeja, y tiene que poder comprobarse contra la cuenta. No hay otro
sitio donde ponerlo.

**Casos de uso que la requieren:** CU-03.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `player_id` | referencia a `player` | Sí | **Clave primaria y clave ajena a la vez** |
| `code` | texto | Sí | Enviado al correo de la cuenta. **Nunca se escribe en el registro de eventos**: es una credencial |
| `created_at` | marca de tiempo | Sí | **El código vale cinco minutos desde este instante** |

**Clave primaria:** `player_id`.
**Cardinalidad:** `player` 1 : **0..1** `password_recovery`.

**Por qué la clave primaria es el jugador.** Un jugador tiene **como mucho un código vigente**:
pedir otro sustituye al anterior, que es el comportamiento que el caso de uso describe. Poner un
identificador propio permitiría acumular códigos vivos de la misma cuenta, y habría que decidir
cuál vale.

**Por qué el código no es único en toda la tabla.** Se comprueba **contra la cuenta**, no por sí
solo: no existe ninguna operación que reciba un código y tenga que averiguar de quién es. Un
índice único global sería una restricción que ninguna consulta aprovecha.

**Por qué la caducidad no es una columna.** Se calcula sobre `created_at` con el plazo que vive en
los parámetros del sistema. Guardar `expires_at` sería guardar la suma de dos datos que ya están,
y dejaría códigos con caducidades distintas si el plazo cambiara.

---

### 4.9 `report`

**Por qué es necesaria.** Sostiene la única capa de convivencia del sistema. Y tiene una razón de
existir muy concreta: **el chat no se guarda en ninguna parte**, así que sin el reporte —y sin la
copia del mensaje que lleva dentro— una denuncia se quedaría sin sustento en el instante en que se
hace.

**Casos de uso que la requieren:** CU-23, y CU-01, CU-13 y CU-19 a través de la sanción que
provoca.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `report_id` | entero generado | Sí | **Clave primaria** |
| `reporter_player_id` | referencia a `player` | Sí | **Solo las cuentas reportan** |
| `reported_player_id` | referencia a `player` | Sí | **Solo se reporta a cuentas**. Distinto del anterior |
| `room_id` | referencia a `room` | Sí | Dónde se dijo el mensaje |
| `match_id` | referencia a `match` | **No** | Vacío si el mensaje se dijo en la sala **fuera de toda partida** |
| `reported_text` | texto de hasta 200 caracteres | Sí | **Copia del mensaje.** 200 es el límite del chat |
| `created_at` | marca de tiempo | Sí | |

**Clave primaria:** `report_id`.
**Claves foráneas:** cuatro, todas en cascada.
**Cardinalidad:** `player` 1 : 0..N **dos veces** · `room` 1 : 0..N · `match` 1 : 0..N.

**Por qué la sala es obligatoria y la partida no, y por qué eso es el corazón de la tabla.** Lo que
se acumula hacia una sanción **no son reportes sueltos: son ocasiones**. Cada partida distinta en
la que a un jugador se le reporta cuenta **una**, y todos los reportes hechos en una sala fuera de
partida cuentan **una** entre todos. La pareja `(room_id, match_id)` **es** la ocasión: contar
ocasiones es contar parejas distintas. Si la partida fuera obligatoria, los mensajes de sala no
tendrían dónde ir; si la sala fuera opcional, las ocasiones fuera de partida no se podrían
agrupar.

**Por qué el texto se copia y no se referencia.** No hay nada a lo que referenciar. El chat vive en
memoria, en una ventana deslizante de doscientos mensajes por sala, y muere con la sala.

---

### 4.10 `sanction`

**Por qué es necesaria.** Las prohibiciones tienen que sobrevivir a la desconexión y al reinicio
—si no, bastaría cerrar el cliente para esquivarlas— y la escalera de duraciones exige saber
**cuántas prohibiciones previas** tiene la cuenta.

**Casos de uso que la requieren:** CU-01 para el baneo permanente, CU-13 y CU-19 para la
prohibición temporal, CU-23 para aplicarla.

| Atributo | Tipo | Obligatorio | Notas |
|---|---|---|---|
| `sanction_id` | entero generado | Sí | **Clave primaria** |
| `player_id` | referencia a `player` | Sí | |
| `level` | entero de 1 a 3 | **No** | Determina la duración según el catálogo. **Vacío si es permanente** |
| `is_permanent` | booleano | Sí | El cuarto cruce del umbral |
| `threshold_at` | marca de tiempo | Sí | Cuándo se alcanzó la quinta ocasión |
| `effective_from` | marca de tiempo | Sí | **Cuándo entra en vigor y cuándo empieza a contar su duración** |
| `effective_until` | marca de tiempo | **No** | Vacío si es permanente |

**Clave primaria:** `sanction_id`.
**Claves foráneas:** `player_id` a `player` en cascada; `level` al catálogo `sanction_level`.
**Cardinalidad:** `player` 1 : 0..N · `sanction_level` 1 : 0..N.

**Por qué `threshold_at` y `effective_from` son dos columnas y no una.** Si el umbral se cruza
**durante una partida**, la sanción no la interrumpe: entra en vigor cuando la partida termina, y
**su duración empieza a contar desde ese momento**. Con una sola marca, una partida larga acortaría
el castigo, que es exactamente lo contrario de lo que se decidió.

**Por qué el número de prohibiciones previas no se guarda.** Se cuenta sobre las filas de la
cuenta. Guardar un contador crearía una segunda fuente para un dato que las propias filas ya dan, y
tendría que mantenerse en el mismo sitio donde se insertan.

**Por qué la permanente no tiene nivel ni fin.** No tiene duración: es permanente. Una restricción
obliga a que una sanción sea **o** temporal con nivel y fin, **o** permanente sin ninguno de los
dos, y así el modelo no admite una permanente que caduque ni una temporal sin final.

**Por qué las sanciones caducan solas.** No hay rol de administración y nadie las levanta. La
prohibición deja de aplicarse cuando pasa `effective_until`, **sin que nadie haga nada**, y la fila
se conserva porque el contador de prohibiciones no se reinicia nunca.

---

## 5. Catálogos de parámetros

Son **de solo lectura** y se cargan por script. No hay rol de administración, así que **ninguno
tiene caso de uso de mantenimiento**: existen para poder cambiar números sin tocar el dominio ni
recompilar.

| Catálogo | Clave primaria | Contenido | Por qué es una tabla |
|---|---|---|---|
| `turn_layout` | `(player_count, round_number)` | Turnos **por jugador** en esa ronda | Las reglas la declaran variable y depende de cuántos juegan |
| `build_allowance` | `(player_count, round_number, turn_number)` | Construcciones que recibe cada jugador **en ese turno** | Igual, y con una clave ajena a `turn_layout`: no puede haber construcciones de un turno que esa ronda no tiene |
| `action_cost` | `action_code` | Puntos de acción que cuesta cada acción | Las reglas la declaran variable |
| `sanction_level` | `level` | Duración de cada escalón: 5 horas, 1 día, 3 días | Es un dato con clave —nivel → duración—, no un valor suelto |
| `system_parameter` | `parameter_code` | Umbral de reportes, límites del chat, latido, plazo de inactividad, plazo de reanudación, reloj del turno, caducidad del código y coste de BCrypt | Valores sueltos que deben cambiarse sin recompilar |

**Por qué `build_allowance` está indexada por turno y no solo por ronda.** Porque las reglas dicen
literalmente que cada jugador recibe sus construcciones **en cada turno**, según esa ronda y ese
turno. El reparto no pertenece al cierre de ronda.

---

## 6. Vista derivada: `player_ranking`

**No almacena nada.** Todos sus números salen de partidas ya archivadas.

| Qué entra | Qué no entra |
|---|---|
| Las partidas **terminadas con resultado** | Las **interrumpidas**, que no tienen puestos ni puntuaciones |
| Las partidas **jugadas con invitados** | Los **invitados**, que no existen en la base |
| Las partidas en las que el jugador fue **retirado o se rindió** | |

Ordena por **victorias**, después por **puntos acumulados** y después por **menos partidas
jugadas**.

**Por qué el ranking global es una vista y el ranking de la sala no existe en la base.** El global
se puede calcular porque todo lo que lo compone está guardado. El de la sala **incluye a los
invitados**, así que **ninguna consulta podría producirlo**: existe únicamente porque el servidor
lo acumula en memoria al terminar cada partida, y muere con la sala.

---

## 7. Relaciones y cardinalidades

| Relación | Cardinalidad | Lectura |
|---|---|---|
| `player` — `friendship` | 1 : 0..N **(dos veces)** | Como solicitante y como destinataria. La pareja invertida es la misma pareja |
| `player` — `room_invitation` | 1 : 0..N **(dos veces)** | Como quien invita y como invitada. Nunca se invita a quien no tiene cuenta |
| `room` — `room_invitation` | 1 : 0..N | Ninguna invitación sobrevive a su sala |
| `room` — `match` | 1 : 0..N | **Una sala juega varias partidas**; toda partida tiene sala |
| `match` — `match_participant` | 1 : **0..4** | Jugaron entre 2 y 4, pero solo los que tienen cuenta dejan fila |
| `player` — `match_participant` | 1 : 0..N | Una cuenta ocupa un solo puesto por partida |
| `match` — `match_state` | 1 : **0..1** | Existe solo mientras la partida está en curso |
| `player` — `password_recovery` | 1 : **0..1** | Un solo código vigente por cuenta |
| `player` — `report` | 1 : 0..N **(dos veces)** | Como quien reporta y como reportada |
| `room` — `report` | 1 : 0..N | Dónde se dijo el mensaje |
| `match` — `report` | 1 : 0..N | Vacío si se dijo fuera de partida |
| `player` — `sanction` | 1 : 0..N | Las previas se cuentan sobre estas filas |
| `sanction_level` — `sanction` | 1 : 0..N | La permanente no tiene nivel |
| `turn_layout` — `build_allowance` | 1 : 1..N | Una fila de construcciones por cada turno de esa ronda |

**Las dos únicas relaciones N:M del sistema, y cómo se resuelven.**

1. **Jugador ↔ partida.** Se resuelve con `match_participant`, que **no es una tabla puente
   vacía**: lleva el puesto de mesa, el resultado y la marca de abandono. Es una entidad
   asociativa con atributos propios.
2. **Jugador ↔ jugador (amistad).** Se resuelve con `friendship`, que es una N:M **reflexiva**:
   las dos claves ajenas apuntan a la misma tabla. Su particularidad es que **la pareja no está
   ordenada**, y por eso necesita el índice único sobre el par normalizado.

**Jugador ↔ sala es también N:M, y deliberadamente no tiene tabla puente**, porque uno de los dos
extremos puede ser un invitado, del que no hay fila. Vive en memoria del servidor. La única
relación jugador–sala que sí se guarda es la invitación, y se guarda porque tiene que sobrevivir a
que el destinatario esté desconectado.

---

## 8. Reglas de integridad

### Obligatorio frente a opcional

Un atributo es **opcional solo cuando su ausencia significa algo**, nunca por comodidad:

| Atributo opcional | Qué significa que esté vacío |
|---|---|
| `player.avatar_reference` | El jugador no ha subido ninguna imagen |
| `friendship.responded_at` | La solicitud sigue pendiente |
| `room.closed_at` | La sala sigue abierta |
| `match.finished_at` y `match.end_reason` | La partida sigue en curso |
| `match_participant.final_score` y `final_position` | La partida aún no ha terminado |
| `report.match_id` | El mensaje se dijo en la sala, fuera de toda partida |
| `sanction.level` y `effective_until` | La sanción es permanente |

### Coherencia entre columnas

Cuatro restricciones impiden estados que el dominio considera imposibles:

1. **Una partida en curso no tiene fin ni motivo; una terminada tiene los dos.**
2. **Una participación tiene los dos resultados o ninguno**, nunca puntuación sin puesto.
3. **Una sanción es temporal con nivel y fin, o permanente sin ninguno de los dos.**
4. **La entrada en vigor de una sanción nunca es anterior al cruce del umbral.**

### Unicidad y duplicados

| Qué se impide | Cómo |
|---|---|
| Dos cuentas que solo se distinguen por mayúsculas | Índices únicos sobre `lower(username)` y `lower(email)` |
| Dos salas abiertas con el mismo código | Índice único **parcial**, solo donde `closed_at` es nulo |
| Dos amistades para la misma pareja, una en cada sentido | Índice único sobre el par normalizado |
| Ser amigo de sí mismo | Restricción de comprobación |
| Dos invitaciones pendientes a la misma sala para el mismo jugador | Unicidad de `(room_id, invited_player_id)` |
| Invitarse a sí mismo | Restricción de comprobación |
| Que una cuenta ocupe dos puestos en la misma partida | Unicidad de `(match_id, player_id)` |
| Que dos jugadores queden en el mismo puesto final | Unicidad de `(match_id, final_position)`, que admite varios vacíos |
| Reportarse a sí mismo | Restricción de comprobación |

### Borrado

- **Eliminar un jugador** se lleva en cascada sus amistades, sus invitaciones, sus
  participaciones, sus reportes —los que hizo y los que recibió—, sus sanciones y su código de
  recuperación. También **su archivo de avatar**, que lo borra la operación porque vive fuera de
  la base, y **las partidas que queden sin ninguna participación**.
- **No se puede eliminar un jugador que participa en una partida en curso.** Lo comprueba la
  operación, no el motor: el motor no sabe distinguir una partida en curso de una terminada a
  efectos de borrado.
- **Las salas no se borran nunca.** La clave ajena de `match` a `room` está restringida
  precisamente para garantizarlo.

### Cómo se cuenta una ocasión, con este modelo

Es la consulta menos evidente del sistema y el modelo la soporta sin ninguna tabla adicional:

> Ocasiones de un jugador = número de **parejas distintas** `(room_id, match_id)` entre sus
> reportes recibidos **posteriores al `threshold_at` de su última sanción**, o entre todos si
> nunca ha tenido ninguna.

El filtro por la última sanción es lo que implementa **el reinicio del contador de ocasiones tras
cada prohibición**, y el `COUNT(DISTINCT ...)` es lo que implementa que **los reportes repetidos
en la misma ocasión no sumen**.

---

## 9. Trazabilidad: caso de uso → entidades

| Caso de uso | Entidades | Atributos y relaciones que usa |
|---|---|---|
| **CU-01** Iniciar sesión | `player`, `sanction` | `username` y `password_hash`; comprueba que no haya sanción con `is_permanent` |
| **CU-02** Crear cuenta | `player` | Inserta con los dos índices únicos en minúsculas |
| **CU-03** Recuperar acceso | `player`, `password_recovery` | Crea el código y comprueba `created_at` contra los cinco minutos; actualiza `password_hash` y borra la fila |
| **CU-04** Jugar como invitado | **ninguna** | El invitado no deja fila. Aparece en `match_state.state_document` si llega a jugar |
| **CU-05** Cambiar el idioma | **ninguna** | Preferencia del equipo, no de la cuenta |
| **CU-06** Modificar el perfil | `player` | `username` y `avatar_reference` |
| **CU-07** Eliminar la cuenta | `player` y todas sus dependientes | Cascada; y borra las partidas que queden sin participación |
| **CU-08** Enviar solicitud | `player`, `friendship` | Inserta *pending*; el índice del par normalizado impide la cruzada |
| **CU-09** Responder solicitud | `friendship` | Pasa a *accepted* con `responded_at`, o borra la fila |
| **CU-10** Historial | `match`, `match_participant` | Unión por `match_id`, ordenada por `finished_at` |
| **CU-11** Detalle de una partida | `match`, `match_participant`, `player` | Resultado de cada participante; puede no incluir todos los puestos |
| **CU-12** Ranking global | `player_ranking` | Vista sobre `match` y `match_participant` |
| **CU-13** Entrar a la sala | `room`, `sanction` | Comprueba que la sala esté abierta y que no haya sanción vigente. **Los jugadores y el color son memoria** |
| **CU-14** Unirse a una sala | `room` | Busca por `code` entre las abiertas. **El listado se sirve de memoria**, porque necesita la ocupación |
| **CU-15** Crear una sala | `room` | Inserta con `code` y `visibility`; el índice parcial garantiza el código |
| **CU-16** Invitar jugadores | `player`, `room_invitation` | Inserta pendiente con su `channel`; la unicidad impide la segunda |
| **CU-17** Responder invitación | `room_invitation`, `room` | Comprueba que la sala siga abierta y borra la fila |
| **CU-18** Salir de la sala | `room`, `room_invitation` | Al quedar vacía, escribe `closed_at` y borra sus invitaciones |
| **CU-19** Iniciar la partida | `match`, `match_participant`, `match_state`, `sanction`, catálogos | Crea la partida y una participación por cuenta; copia el pase de los invitados al documento; lee `turn_layout` y `build_allowance` |
| **CU-20** Eliminar un amigo | `friendship` | Borra la fila |
| **CU-21** Expulsar de la sala | **ninguna** | Solo toca el estado en memoria de la sala |
| **CU-22** Enviar un mensaje | **ninguna** | El chat no se guarda |
| **CU-23** Reportar a un jugador | `report`, `sanction`, `sanction_level`, `system_parameter` | Inserta el reporte con la copia; cuenta ocasiones; crea la sanción si cruza el umbral |
| **CU-24** Preparar la partida | `match_state` | Escribe la colocación en el documento |
| **CU-25** Jugar un turno | `match_state`, `action_cost`, `build_allowance` | Escribe la frontera del turno al cerrarlo |
| **CU-26** Reconectar | `match_state`, `match_participant` | Devuelve el estado; marca `was_withdrawn` si la ventana vence |
| **CU-27** Volver a una partida reanudada | `match`, `match_state`, `match_participant` | Busca las partidas *en curso*; el pase del invitado sale del documento |
| **CU-28** Abandonar la partida | `match_participant`, `match`, `match_state` | Marca `was_withdrawn`; si quedan menos de dos, cierra la partida como *abandoned* |

**Los veintiocho casos de uso se pueden implementar con este modelo.** Cuatro de ellos —CU-04,
CU-05, CU-21 y CU-22— **no tocan la base de datos en absoluto**, y eso no es una carencia: es la
consecuencia directa de que el invitado no se guarde, de que el idioma sea del equipo y de que la
sala y su chat vivan en memoria.

## 10. Trazabilidad inversa: entidad → qué la justifica

| Entidad | Qué la justifica | ¿Sobra? |
|---|---|---|
| `player` | Quince casos de uso | No |
| `friendship` | CU-08, CU-09, CU-20, CU-16 | No |
| `room` | El código único entre abiertas y la referencia de las partidas | No |
| `room_invitation` | Que la invitación sobreviva a que el destinatario esté desconectado | No |
| `match` | El historial, el ranking y la reanudación | No |
| `match_participant` | El resultado por jugador y el puesto de mesa | No |
| `match_state` | La reanudación tras una caída | No |
| `password_recovery` | CU-03 | No |
| `report` | CU-23, y que el chat no se guarde | No |
| `sanction` | Que las prohibiciones sobrevivan a la desconexión | No |
| `turn_layout`, `build_allowance`, `action_cost` | Las reglas los declaran variables | No |
| `sanction_level`, `system_parameter` | Los valores que deben cambiarse sin recompilar | No |
| `player_ranking` | CU-12 | No: no almacena nada |

**Ninguna entidad del modelo carece de justificación.** Y ninguna se conserva «por si acaso».

### Entidades que no deben existir, y por qué

Se revisaron una a una porque son las que cualquier sistema parecido tendría. Ninguna procede
aquí, y el motivo **no es el mismo en todas**:

| Entidad descartada | Por qué no procede |
|---|---|
| **`profile`** | Sus dos únicos datos son 1:1 con la cuenta, y uno de ellos, el `username`, **es también la credencial de acceso**: no puede vivir fuera de `player` |
| **`guest`** | Contradice la promesa del sistema: de un invitado no se guarda nada. Obligaría además a que `match_participant`, `report` y `friendship` admitieran referencias a identidades temporales |
| **`room_member`** | Puede contener invitados. Solo con las cuentas, **no sabría si la sala está vacía**, que es lo que decide si la sala existe |
| **`chat_message`** | El chat no se guarda: vive en memoria, muere con la sala y una caída se lo lleva. Lo único que sobrevive de un mensaje es la copia dentro de `report` |
| **`session`** | Está prohibido incluso registrar las sesiones y los tokens |
| **`match_move`** | Nadie consulta la secuencia de jugadas: reanudar necesita el estado, no la historia |
| **`round_score`** | El historial muestra puesto y puntos finales; el desglose por rondas no lo consulta nadie |
| **`room_ranking`** | Incluye invitados, así que ninguna consulta podría producirlo, y muere con la sala |
| **`color`** | Hay cuatro, fijos, y se asignan dentro de la sala. Es un valor del estado en memoria, no una entidad |
| **`administrator` / `moderation_case`** | **No existe el rol de administración**: las sanciones son automáticas por umbral y nadie las revisa ni las levanta |
| **`notification`** | Fuera de alcance salvo las invitaciones, que ya tienen su propia entidad |
| **`event_log`** | El registro va a log4net, en archivo, y tiene prohibido contener datos personales |

---

## 11. ¿Está completa la base de datos?

**Sí, con una corrección que este análisis encontró y una carencia que no es de modelo.**

**Lo que faltaba y se ha añadido:** la tabla `room` **no tenía la columna `visibility`**, aunque la
sala pública o privada es una decisión cerrada del proyecto y `CU-15` la fija al crear la sala.
Faltaba en el esquema, no en el análisis previo. Está añadida, con su restricción de valores.

**Lo que no es una carencia del modelo, aunque lo parezca:** no hay tablas para los miembros de la
sala, el anfitrión, el chat, los colores ni el ranking de la sala. Ninguna de las cinco puede
existir, y siempre por el mismo motivo: **todas pueden contener invitados**.

**Lo que sigue sin valor fijado**, y es configuración, no esquema: el **periodo del latido** y el
**número de latidos perdidos** que dan una conexión por caída. El proyecto declara el mecanismo y
la tabla `system_parameter` tiene sitio para los dos, pero **los números no están decididos**, así
que el script de carga los deja fuera en vez de inventarlos.

---

## 12. Puntos que todavía necesitan definición

Solo dos, y ninguno bloquea la implementación:

| # | Qué falta | A qué afecta |
|---|---|---|
| 1 | **Periodo del latido y número de latidos perdidos** para dar una conexión por caída | Dos filas de `system_parameter`. El mecanismo está decidido; los números no |
| 2 | **Longitud y alfabeto del código de recuperación.** El prototipo dibuja cuatro dígitos, pero ninguna fuente lo fija | El tamaño de `password_recovery.code`, hoy holgado a propósito |
