# Análisis de pantallas para el prototipo — Juego de Torres

**Versión 4.1** · 16 sep 2026 · **Alcance: el sistema completo, de la conexión al resultado de
la partida.**

**Lo que la versión 4.1 añade.** Alineación con `Diccionario-i18n-Torres.xlsx`; detalle en
`Analisis-Revision-XLSX-Prototipos-Torres.md` y resumen en la §14.

**Lo que la versión 4.0 añade.** Las nueve aclaraciones del equipo del 10 sep 2026 cierran
`PU-26` a `PU-29` y obligan a cuatro cambios de pantalla: `PT-21` gana su **reloj de noventa
segundos**, `PT-22` gana la opción **"Rendirse"**, `PT-24` pasa de "Volver al menú" a **volver a
la sala** —la invitación es a la sala, y en ella pueden jugarse varias partidas— y nace **`PT-27`
Partida en curso al volver**, para el jugador que salió mientras el servidor estaba caído.
`PT-17` recoge dos avisos nuevos, el de eliminar un amigo y el de rendirse.

El prototipo ya no se detiene en «Iniciar partida». A las veinte pantallas del flujo previo se
suman **siete del tablero** —`PT-21` a `PT-27`—, y las anteriores quedan alineadas con las
decisiones vigentes: chat de sala, expulsión, colores de jugador, inactividad, reportes y
sanciones, código de sala de cuatro caracteres, recuperación de acceso con código e idioma en
dos catálogos. Las secciones afectadas están reescritas, no anotadas: no quedan estados
obsoletos mezclados con los vigentes.

Este documento **revisa** la propuesta inicial de pantallas; no la da por buena. Cada punto
se contrasta contra lo que el proyecto ya tiene cerrado y se clasifica en: respaldado,
modificable, ausente o **pendiente de definir**. Donde falta información **no se inventa**.

## Fuentes usadas y su autoridad

| Fuente | Qué aporta |
|---|---|
| `Base-Oficial-STK-CON-DEP-Torres.md` | Registro normativo: `STK-`, `CON-`, `DEP-`, `TEN-`, `DR-`, `ASM-` |
| `Analisis-Persistencia-Torres.md` v4 | Requisitos `[N1]`/`[N2]` 1–12, respuestas `P-xx`, decisiones `D-xx`, §12 fuera de alcance |
| `code/database/schema.sql` | Qué dato existe realmente y con qué forma |
| `Reglas_del_Juego_de_Torres.docx` v3.0 | `R 1.1` (2–4 jugadores, nadie se incorpora empezada la partida) |
| `Documento-Arquitectura-Torres.md` | `DEP-E12` (cliente MonoGame DesktopGL), restricciones de STK-03 |

## Marcas

| Marca | Significado |
|---|---|
| `[DEFINIDO]` | Hay requisito, regla o decisión cerrada que lo respalda. Se cita |
| `[DERIVADO]` | No está escrito como tal, pero se deduce sin añadir reglas nuevas. Se explica la deducción |
| `[PROPUESTA]` | Elección de diseño de interfaz que este documento propone y que el equipo debe ratificar |
| `[PENDIENTE PU-xx]` | Falta una decisión. **Se señala, no se asume** |

**Prefijo nuevo `PU-`** (pregunta de interfaz de usuario). No reutiliza `P-` ni `Q-`, que ya
existen con otro significado, conforme a la regla de gobernanza de la base: *un identificador
nunca se reutiliza*.

---

## 0. Criterio con el que se revisó cada punto

Cada pantalla y cada funcionalidad de la propuesta pasó por tres pruebas:

1. **¿Hay respaldo?** Un requisito `[N1]`/`[N2]`, una regla `R`, una respuesta `P-xx` o una
   decisión `D-xx`/`DR-xx`. Sin respaldo no se elimina automáticamente: se marca `PU-`.
2. **¿El sistema puede producir el dato?** El modelo de persistencia está cerrado (siete
   entidades) y lo que no está ahí ni en el estado en memoria del servidor **no se puede
   mostrar**. Una pantalla que pida un dato inexistente está mal planteada, no incompleta.
3. **¿Contradice algo ya cerrado?** §12 del análisis de persistencia y `ASM-05` declaran
   cosas fuera de alcance. Proponerlas otra vez no es añadir: es reabrir, y hay que decirlo.

**Restricción transversal que condiciona todo el prototipo.** El cliente es una aplicación
de escritorio en **MonoGame DesktopGL** `DEP-E12`, comunicada por **WCF duplex sobre
net.tcp**, y STK-03 **prohíbe el stack web**. Consecuencias para el prototipo:

- No hay navegador, ni URL, ni botón «atrás» del sistema. La navegación es **una ventana con
  pantallas**, y cada pantalla debe llevar su propio retorno explícito.
- El canal **duplex** permite que el servidor empuje cambios al cliente sin que este
  pregunte. Eso hace **viable** —no obligatorio— que la sala se actualice sola cuando alguien
  entra o sale. Es la única razón técnica que respalda una vista «viva»; no se inventa nada.
- **El prototipo HTML no es la implementación.** Es un instrumento de diseño previo a Figma.
  Conviene decirlo por escrito para que no se lea como una violación de la restricción de
  STK-03.
- `E 9.4` no guarda sesiones ni tokens → **no puede existir «mantener la sesión iniciada»**
  sin una decisión nueva.
- `SecurityMode.None` (`C-2`, `TEN-I` abierta): la credencial **viaja sin cifrar**. La
  pantalla de inicio de sesión no debe prometer seguridad que hoy no existe.

---

## 1. Pantallas propuestas que sí son necesarias

| Pantalla propuesta | Veredicto | Qué la justifica |
|---|---|---|
| **Menú principal** | Necesaria | `[DERIVADO]` Un cliente de escritorio necesita un punto de entrada único; todas las secciones cuelgan de él |
| **Inicio de sesión** | Necesaria | `[DEFINIDO]` Requisito 1 «Cuentas» `[N1]`; la cuenta tiene credencial y `CON-10` trata de que solo el titular actúe como ella |
| **Perfil** | Necesaria, **reducida** | `[DEFINIDO]` Requisito 10 «Perfil con avatar» `[N2]`. `D-22`: el perfil **es** username + avatar. Nada más está definido |
| **Salas** | Necesaria, **modificada** | `[DEFINIDO]` Requisitos 4 y 5 «Salas y entrada por código» `[N2]`. Ver §2.4: la lista de salas no procede |
| **Dentro de una sala** | Necesaria — es el núcleo de esta fase | `[DEFINIDO]` Requisitos 7 y 11 `[N2]`, `P-33`…`P-36`, `P-41`, `D-26`, `D-29` |
| **Amigos** | Necesaria | `[DEFINIDO]` Requisitos 8 y 9 `[N2]`, `P-15`, `P-16`, `P-21` |
| **Historial** | Necesaria | `[DEFINIDO]` Requisito 2 `[N1]`, `P-05` |
| **Ranking (global)** | Necesaria | `[DEFINIDO]` Requisito 3 `[N1]`, `P-01`, `DR-15`. Existe como vista `player_ranking` |
| **Salir** | Necesaria **como acción, no como pantalla** | `[DERIVADO]` Es una aplicación de escritorio: tiene que poder cerrarse. Ver §2.9 |

**Ninguna pantalla propuesta sobra por completo.** Lo que sobra son **funcionalidades dentro
de ellas**, y lo que más pesa es lo que falta (§3).

---

## 2. Pantallas que deberían modificarse

### 2.1 Menú principal

**Lo que se conserva:** nombre del juego, avatar arriba a la derecha, y las entradas Salas,
Amigos, Historial, Ranking, Salir.

**Cambio 1 — el menú tiene tres versiones, no dos.** La propuesta contempla «con sesión» y
«sin sesión». El proyecto tiene **tres identidades** y están cerradas en `CON-07`:

| | Con cuenta | Invitado | Sin identidad todavía |
|---|---|---|---|
| Historial | Sí | **No existe** `P-08` | No |
| Ranking global | Aparece | **No aparece nunca** `P-40` | No |
| Amigos | Sí | **No** — la amistad es entre cuentas `P-16` | No |
| Crear sala / ser anfitrión | Sí | **Sí** `P-41` | No |
| Recibir invitación a sala | Sí | **No** — siempre va a una cuenta `P-22` | No |

Es decir: **el invitado no es «un usuario sin sesión con menos permisos»; es una identidad
definida con una tabla de diferencias cerrada**. `[DEFINIDO]` Se entra en ese modo **desde la
ventana de inicio de sesión** `OB-16`, el **alias es libre** —sin formato ni unicidad— y el
servidor le asigna un **identificador temporal** que además le sirve de pase para recuperar su
puesto en una partida reanudada `OB-05`, `OB-45`; dos invitados con el mismo alias se
distinguen por su **color** `OB-20`. Sigue sin definirse si las entradas que no le sirven se
ocultan o se muestran deshabilitadas → `[PENDIENTE PU-02]`.

**Cambio 2 — falta una entrada: invitaciones a sala.** El requisito 9 `[N2]` y `P-21` dicen
que **una invitación a un amigo desconectado se conserva hasta que entre**. Si se conserva
para que la vea al entrar, **tiene que haber dónde verla**. Hoy la propuesta no tiene ninguna
superficie para eso. Es la omisión más grave del menú.

**Cambio 3 — el avatar del menú necesita su estado vacío.** Está definido que la referencia
del avatar puede estar vacía y que, **si el archivo desaparece, la aplicación debe tolerarlo
mostrando el avatar por defecto** (§3.1 del análisis de persistencia). Ese avatar por defecto
existe por decisión, pero **su aspecto no está definido** → `[PENDIENTE PU-22]`.

**Cambio 4 — indicador de conexión.** `DEP-E3` registra que la participación depende de la
red y `CON-01 V2` contempla la caída del servidor. Un cliente de escritorio que no puede
hablar con el servidor **no puede ofrecer ninguna de las cinco entradas**. El menú necesita
un estado «sin servidor» `[PROPUESTA]`. `[DEFINIDO]` Si el servidor cae estando el jugador en
una sala, **la sala no se reanuda** `P-32`; si estaba en una partida, **la partida sí**, y el
jugador vuelve a ella —el invitado, presentando su pase— `OB-45`. Es lo que representa `PT-26`.

### 2.2 Inicio de sesión

**Se confirma la pantalla** y se confirma que **crear cuenta es una pantalla aparte**, no un
enlace que abre un formulario ampliado: los datos del alta están definidos y son más que los
del acceso — username `^[A-Za-z0-9_]{3,20}$` único sin distinguir mayúsculas, correo único
(**único dato personal obligatorio**, `D-13`) y contraseña.

**Lo que falta y sí está justificado:** **recuperar el acceso**. No es un añadido de
cortesía: `D-13` declara que **el correo sirve para recuperar el acceso**, y `CON-10` exige
que esa vía no se convierta en vía de suplantación. `[DEFINIDO]` El mecanismo es un **código
enviado al correo** que el jugador introduce para poder establecer una contraseña nueva
`OB-17`; son tres pasos y así se dibuja. Su modo de fallo sigue abierto en la base `Q-34`, y la
el código **caduca a los cinco minutos** de haberse generado.

**Lo que no debe aparecer:** «mantener la sesión iniciada». `E 9.4` no guarda sesiones ni
tokens; ponerlo sería inventar un mecanismo.

**Validaciones que sí se pueden mostrar porque el modelo las garantiza:** username ocupado
(índice único sobre `lower(username)`), correo ya registrado, formato de username. Y un
detalle real que conviene validar en el alta: **BCrypt ignora lo que exceda de 72 bytes** de
contraseña (§3.1).

### 2.3 Perfil y configuración de cuenta

La propuesta mezcla dos cosas que el proyecto ya separó. `D-22` es explícito: **el perfil se
integra en la cuenta y el perfil es username + avatar**.

**Perfil — solo esto está respaldado:**

| Elemento | Estado |
|---|---|
| Cambiar username si está disponible | `[DEFINIDO]` `P-13`; unicidad garantizada por el índice |
| Cambiar avatar, proporcionado por el jugador | `[DEFINIDO]` `P-43`: **lo sube el jugador**, lo guarda el servidor, **máximo 5 MB**, **PNG o JPG**, la base solo guarda la referencia |
| Ver el correo | `[DERIVADO]` Es dato de la cuenta y la única vía de recuperación; mostrarlo no añade regla |

La pantalla de avatar **debe** mostrar el límite de 5 MB y los formatos: no son adorno, son
la validación que hace la capa de servicios **antes** de guardar el archivo.

**«Configurar cuenta» — hoy tiene exactamente un contenido respaldado:**

| Opción | Estado |
|---|---|
| **Eliminar la cuenta** | `[DEFINIDO]` `P-02`, `D-28`. Borra en cascada participaciones, amistades e invitaciones; **se rechaza si el jugador está en una partida en curso** `P-42`. Es irreversible: exige confirmación |
| Cambiar contraseña | `[PENDIENTE PU-15]` Ninguna fuente la menciona |
| Cambiar correo | `[PENDIENTE PU-15]` Ninguna fuente la menciona, y toca la vía de recuperación |
| Sonido, idioma, gráficos | **No incluir.** Ninguna fuente los menciona. Añadirlos sería inventar |

Es decir: **la sección existe, pero con un solo elemento**. Eso es un resultado del análisis,
no un hueco que haya que rellenar. Si el equipo quiere más, es una decisión `PU-15`, no una
deducción.

### 2.4 Salas — búsqueda arriba, salas disponibles debajo

La pantalla se organiza en dos bloques, en este orden:

1. **Búsqueda por código, en la parte superior.** Es la vía de entrada definida (requisitos 4
   y 5 `[N2]`), y el código es único solo entre las salas abiertas `D-23`. `[DEFINIDO]` Es
   **alfanumérico de cuatro caracteres** `OB-18`; como se teclea también desde un correo,
   conviene excluir del alfabeto los caracteres que se confunden `[PROPUESTA]`.
2. **Lista de salas disponibles, debajo.** `[DECIDIDO 6 sep]` Cada fila muestra **solo lo que
   permite identificar la sala y decidir si entrar**: código, anfitrión y ocupación sobre el
   máximo de cuatro `P-34`, con la acción **Unirse** cuando queda lugar y deshabilitada cuando
   la sala está llena. No se añade ningún dato más porque ningún otro está definido.
3. **Crear sala**, junto a la búsqueda. Ya no es una acción sin pantalla: con las salas
   públicas y privadas hace falta elegir el tipo (§2.4.1).

**Consecuencia que hay que registrar en la base, no en el prototipo.** La v1.0 de este
documento objetaba la lista porque §12 del análisis de persistencia declara «salas públicas
listadas» fuera de alcance y `ASM-05` supone que *se juega entre conocidos*. STK-02 decide
incorporarla, así que **`ASM-05` deja de ser cierto tal como está escrito** y con él se activa
la **cláusula condicional de `CON-03`** —el sistema pasa a admitir partidas entre
desconocidos— y `CAND-11` **deja de ser opcional**: hay que declarar o cerrar la figura del
adversario. Además la lista **no puede salir de una consulta a la base**: quién está dentro de
cada sala vive en memoria del servidor `D-26`, de modo que es una operación nueva sobre el
estado en memoria, con la presión de recursos que `TEN-G` ya tiene abierta. Nada de esto
impide la decisión; sí obliga a actualizar §12, `ASM-05` y `CON-03` en
`Base-Oficial-STK-CON-DEP-Torres.md`.

**Estados de la pantalla:** con salas, sin salas disponibles, sala llena en la lista, y los
rechazos al buscar por código —código inexistente, sala cerrada, sala llena, o estar ya en
otra sala `P-33`—. El texto exacto de esos rechazos → `[PENDIENTE PU-07]`.

#### 2.4.1 Crear sala — pública o privada

`[DECIDIDO 6 sep]` Al crear la sala, el anfitrión elige entre **pública** y **privada**. El
prototipo presenta las dos opciones como una elección única. `[DEFINIDO]` **La única diferencia
es si la sala aparece en el listado** `OB-08`: en lo demás son iguales. De ahí se sigue, sin
añadir ninguna regla, que a una sala privada solo se entra **por su código o por invitación**,
que son las dos vías que quedan. Dentro de la sala, el tipo se muestra como una etiqueta.

Lo demás sigue sin ser configurable y por eso no aparece en el formulario: el máximo son 4
jugadores `P-34`, la partida son siempre 3 rondas `DR-01` y el número de jugadores lo
determina quién esté dentro al empezar.

### 2.5 Dentro de una sala — la pantalla que más hay que corregir

**Lo que se confirma:**

| Elemento propuesto | Veredicto |
|---|---|
| Código de la sala | `[DEFINIDO]` Es la única forma de que otro entre. Debe poder copiarse |
| Lista de jugadores | `[DEFINIDO]` Es estado en memoria del servidor `D-26`; con duplex el servidor puede empujar los cambios |
| Salir de la sala | `[DEFINIDO]` La sala **muere vacía** `[N2]`; el anfitrión **se hereda** al salir el anterior `P-36` |
| Ranking de la sala | `[DEFINIDO]` **y sí corresponde a esta pantalla** — ver abajo |
| Iniciar la partida | `[DEFINIDO]` **La pulsa el anfitrión** `OB-02`, con 2 a 4 jugadores **activos** `OB-41` |
| Expulsar jugadores | `[DEFINIDO]` **Solo el anfitrión, y solo sin partida en curso**; al expulsado se le avisa y puede volver `OB-01`, `OB-22`, `OB-42`, `OB-21` |
| Chat de la sala | `[DEFINIDO]` Es uno solo, se conserva hacia las partidas de esa sala y no se guarda en la base `OB-03`, `OB-24` |
| Color de cada jugador | `[DEFINIDO]` Se asigna en la sala y distingue a dos invitados con el mismo alias `OB-40`, `OB-20` |

**Corrección 1 — «dueño de la sala» no es la figura definida; el anfitrión sí, y ya tiene
atribuciones.** El proyecto define que **existe un anfitrión**, que **se hereda** cuando el
anterior se va —pasa **al siguiente en el orden en que están guardados** `OB-10`— y que **puede
ser un invitado** `P-41`. Por lo tanto:

- «Dueño» sugiere permanencia y propiedad; el término correcto es **anfitrión**, y **cambia
  de persona**. La interfaz tiene que contemplar que **el rol te llegue mientras estás
  dentro**, y también que se traspase solo cuando el anfitrión pierde la conexión `OB-23`.
- Sus dos poderes están decididos: **expulsar** `OB-01` e **iniciar la partida** `OB-02`. Los
  controles correspondientes se muestran **solo a él**.
- La expulsión **no veta el acceso**: el expulsado recibe un aviso y puede volver a entrar con
  el código `OB-42`, `OB-21`. Su efecto real es dejarlo fuera de la partida que se inicie
  después, porque una vez empezada nadie se incorpora `R 1.1`.

**Corrección 2 — el ranking de sala sí va aquí, pero con estado vacío.** Está definido que
**vive en memoria, incluye a los invitados, se ordena por puntos totales, muestra también las
victorias y muere con la sala** `P-40`, `P-44`, `P-23`, `P-24`, `D-29`. Y una sala **juega
varias partidas** `P-26`. Luego su lugar natural es la sala, entre una partida y la
siguiente. Pero **en una sala recién creada está vacío**: solo se llena cuando termina una
partida jugada en ella. El prototipo muestra el estado vacío, que es el que verá cualquiera que
acabe de crear una sala.

**Corrección 3 — «iniciar la partida» tiene una condición definida y varias sin definir.**
Definido: **mínimo 2 jugadores, máximo 4** `R 1.1`, y **nadie se incorpora una vez empezada**.
Luego la acción está deshabilitada con 1 jugador activo y habilitada de 2 a 4. `[DEFINIDO]`
**La pulsa el anfitrión** `OB-02`, y **los jugadores inactivos no cuentan ni entran a la
partida** `OB-41`. Sin definir: si hay preparación previa (¿todos listos?) —nadie lo ha pedido,
no se inventa—, y si la sala sigue admitiendo gente mientras su partida está en curso `PU-06`.

**Corrección 4 — el puesto de mesa.** `DR-24` dice que el puesto se asigna «según el orden en
que los jugadores se acomodan al empezar la partida», y de él dependen el orden de colocación
inicial y quién coloca el rey en la ronda 1. `[DEFINIDO]` El puesto se fija **al iniciar la
partida**, siguiendo el orden en que los jugadores están acomodados en la sala; el prototipo lo
muestra en `PT-18` y en la columna de jugadores de `PT-21` y `PT-22`.

**Corrección 5 — falta invitar desde la sala.** Es el requisito 9 `[N2]` y no aparece en la
propuesta. Ver §3.

**Qué ocurre si el anfitrión abandona** (pregunta explícita del encargo): **está definido** —
se hereda `P-36`, la sala **no** se cierra. Solo se cierra cuando **se queda vacía**. Lo que
falta es qué se le muestra a quien recibe el rol.

### 2.6 Amigos

La estructura propuesta —amigos, recibidas, enviadas, buscar por username, enviar solicitud—
**coincide con lo definido** `P-15`, `P-16`, y es de las pocas partes que no necesita
corrección estructural. Lo que hay que completar:

| Acción | Estado |
|---|---|
| Aceptar / rechazar una solicitud recibida | `[DEFINIDO]` `P-15`. **Rechazar borra la fila**: es irreversible, no queda historial |
| **Invitar a un amigo a mi sala** | `[DEFINIDO]` Requisito 9 `[N2]`, `P-21`. **Falta por completo en la propuesta** |
| Cancelar una solicitud enviada | `[PENDIENTE PU-11]` |
| Eliminar una amistad aceptada | `[PENDIENTE PU-11]` |
| Bloquear | **No existe** `P-15`. No se prototipa |
| Ver si un amigo está conectado | `[PENDIENTE PU-12]` — y **no hace falta para el flujo**: la invitación se conserva hasta que el amigo entre `P-21` |

La búsqueda es **solo por username** `P-16`. No por correo: el correo es dato personal
protegido por `CON-06` y el sistema **nunca guarda el correo de un tercero** `P-22`.

### 2.7 Historial

Lo propuesto —partidas, puesto y puntos— es **lo mínimo definido** `P-05`. Es demasiado poco
para que la pantalla sea útil, y **todo lo que falta ya existe en el modelo**, así que
añadirlo no inventa nada:

| Dato | Justificación |
|---|---|
| **Fecha de fin** | `match.finished_at`; el índice del historial ya ordena por ella |
| **Cuántos jugaron** | `match.player_count`, guardado **precisamente para el historial** `D-25`: los invitados no dejan fila y una cuenta borrada se lleva la suya |
| **Contra quién** (con cuenta) | `match_participant` + `account.username`. Puede haber **huecos**: si ganó un invitado, ninguna fila tendrá el puesto 1. La pantalla debe tolerarlo |
| **Motivo de fin** | *completada* / *abandonada* / *interrumpida* `P-37`. Está definido que **una partida interrumpida aparece en el historial como interrumpida**, sin puesto ni puntos: **es un estado de fila obligatorio** |
| **Marca de retirado** | `was_withdrawn`; el jugador retirado **sigue contando para el ranking** `P-07`, y el historial debe explicar por qué su fila se ve distinta |

**Detalle de una partida:** cabe, pero es delgado a propósito. **Lo único que puede mostrar**
es lo de arriba más los resultados de cada participante. **No puede haber repetición ni
resumen por rondas**: la secuencia de jugadas no se guarda nunca y el estado vivo se elimina
al terminar `D-21`. Si el detalle no aporta más que la fila, quizá no merezca pantalla propia
→ `[PENDIENTE PU-19]`.

### 2.8 Ranking

Está definido más de lo que la propuesta supone:

- **Orden: victorias → puntos acumulados → menos partidas jugadas** `DR-15`. Ratificado.
- **Datos disponibles** en la vista `player_ranking`: partidas jugadas, victorias, puntos
  totales, mejor puntuación, fecha de la última partida.
- **Quedan fuera**: los invitados (no existen en la base) y las partidas *interrumpidas* (no
  tienen resultado).
- **Solo hay un ranking global.** El de sala es otra cosa, vive en otro sitio y se ordena por
  otro criterio (puntos, no victorias). **No deben presentarse como dos pestañas del mismo
  ranking**: eso induciría a pensar que el de sala persiste, y muere con la sala.

Sin definir: cuántas filas se muestran, si hay paginación y si se resalta la posición propia
→ `[PENDIENTE PU-18]`. Y para un invitado la pantalla es puramente informativa: nunca
aparecerá en ella `P-40`.

### 2.9 Salir

**No es una pantalla; es una acción del menú.** `[DEFINIDO]` **«Salir» cierra la aplicación**
`OB-15`. Lo que sigue importando es qué arrastra según dónde esté el jugador:

- Fuera de una sala: cierra la aplicación y no hay más consecuencias.
- **Dentro de una sala**: salir del cliente es salir de la sala, y **si era el último, la sala
  se cierra** `D-23` — con sus invitaciones pendientes `7.2`. Eso es una consecuencia real, no
  cosmética, y es lo que justificaría una confirmación.
- Durante una partida: `P-35` prohíbe salir de la sala mientras se juega, así que cerrar la
  aplicación **solo puede tratarse como una desconexión**, que es lo único que el proyecto
  define para un jugador que desaparece `R 5.2`. Es una deducción, no una regla escrita, y
  conviene que STK-02 la ratifique.

---

## 3. Funcionalidades que faltan

Ordenadas por gravedad. **Todas están respaldadas por un requisito ya aceptado**; ninguna es
un añadido de oficio.

### 3.1 Invitaciones a sala — el hueco mayor

Los requisitos 6 y 9 `[N2]` están **enteros sin pantalla**. Lo definido:

- La invitación **va siempre a una cuenta** `P-22`. No se puede invitar a un correo ajeno ni a
  un invitado.
- Dos canales `D-19`: **dentro del juego** y **por correo con un enlace único** `P-20`.
- **Solo existen invitaciones pendientes** `D-24`: responderla la borra, y por eso **el enlace
  del correo sirve una sola vez**.
- **No caduca** `P-39`: vive mientras viva la sala.
- **Se conserva aunque el destinatario esté desconectado** `P-21`.
- **Una sola invitación pendiente por sala y destinatario** (restricción 7 del modelo).

De ahí salen **dos superficies que no existen en la propuesta**:

1. **Enviar invitación**, desde la sala: elegir amigo, o buscar por username, y elegir canal.
   Debe impedir invitar dos veces a la misma persona a la misma sala.
2. **Bandeja de invitaciones recibidas**, accesible desde el menú, con las que llegaron
   estando el jugador desconectado. Aceptar lleva a la sala; rechazar borra la invitación.

**El canal correo, resuelto sin tocar la restricción de STK-03:** el correo **no lleva un
enlace, lleva el código de la sala**, que el jugador escribe en la aplicación como cualquier
otro código. La seguridad no descansa en que el código sea secreto sino en que **la invitación
va siempre dirigida a una cuenta** `P-22`: al canjearlo, el sistema comprueba que quien lo
presenta es la destinataria. Por eso **no hace falta ninguna pantalla de canje**: se entra por
`PT-14` con el código, o se acepta la invitación desde `PT-10`.

### 3.2 Entrada como invitado

`CON-07` cierra **qué puede y qué no puede** hacer un invitado, y el documento de estado de la
partida guarda «el alias de los invitados». `[DEFINIDO]` El alias **se pide desde la ventana de
inicio de sesión** `OB-16`, es **libre** —sin formato ni unicidad— y el servidor le entrega un
**identificador temporal** con el que lo distingue y con el que el invitado podrá **recuperar
su puesto si la partida se reanuda** `OB-05`, `OB-45`. En la sala, dos alias iguales se
distinguen por el **color** `OB-20`.

Conviene además que la interfaz sea explícita con el precio de jugar como invitado, porque está
definido: **no aparece en el ranking global, no tiene historial, no puede tener amigos ni
enviar invitaciones, y no puede reportar ni ser reportado** `CON-07`, `OB-11`, `OB-28`,
`OB-39`.

### 3.3 Crear cuenta y recuperar acceso

La propuesta las menciona como «opción para crear una cuenta» dentro del inicio de sesión.
Son **dos pantallas** (§2.2), y recuperar acceso está respaldada por `D-13`.

### 3.4 Estados de fallo del cliente

`DEP-E3` y `CON-01 V2` hacen que **la caída del servidor sea un caso previsto, no un
imprevisto**. Hacen falta «conectando» y «servidor no disponible» —`PT-01`—, y el caso del
jugador que estaba dentro cuando el servidor cayó: **las salas no se reanudan** `P-32`, luego
su sala ya no existe, pero **su partida sí se reanuda** y vuelve a ella `OB-45`. Eso es
`PT-26`. Lo único que sigue sin decidirse es **cómo se le indica que esa partida le espera**
→ resuelto: mientras el servidor está caído se indica en la interfaz; si el jugador no salió, la partida se reanuda sin intervención suya, y si salió, al volver se le indica que sigue en curso y elige volver o abandonarla (`PT-27`).

### 3.5 Confirmaciones de lo irreversible

Tres acciones del flujo previo son irreversibles por decisión ya tomada, y ninguna aparece
confirmada en la propuesta:

| Acción | Por qué es irreversible |
|---|---|
| Eliminar cuenta | Borrado en cascada `D-28`; además se rechaza si hay partida en curso `P-42` |
| Rechazar solicitud de amistad | **Se borra**, no queda rastro `P-15` |
| Salir siendo el último de la sala | La sala **se cierra**, con sus invitaciones `D-23`, `7.2` |

---

## 4. Estados de pantalla que deben contemplarse

Se organizan por **ejes**, igual que la base organiza los estados de `STK-01`, para que cada
pantalla del prototipo declare exactamente en qué combinación está.

| Eje | Valores | Origen |
|---|---|---|
| **Identidad** | Sin identidad · Con cuenta · Invitado | `CON-07` |
| **Conexión con el servidor** | Conectado · Conectando · Servidor no disponible | `DEP-E3`, `CON-01 V2` |
| **Situación de sala** | Fuera de sala · Dentro como anfitrión · Dentro sin ser anfitrión | `P-36`, `P-41`, `P-33` |
| **Ocupación de la sala** | 1 jugador (no se puede iniciar) · 2–3 (se puede) · 4 llena | `R 1.1`, `P-34` |
| **Datos** | Cargando · Con contenido · **Vacío** · Error | Consecuencia de que todo dato viene del servidor |
| **Invitaciones pendientes** | Ninguna · Una o varias | `P-21` |

**Estados vacíos que el prototipo debe dibujar** (no son adorno: son el estado normal de un
sistema recién estrenado):

| Pantalla | Estado vacío | Por qué existe |
|---|---|---|
| Amigos | Sin amigos, sin recibidas, sin enviadas | Cuenta nueva |
| Historial | Sin partidas terminadas | La primera partida ocurre después de esta fase |
| Ranking global | Sin jugadores con partidas terminadas | Ídem |
| **Ranking de sala** | **Vacío siempre en esta fase** | Se llena solo al terminar una partida en esa sala `D-29` |
| Bandeja de invitaciones | Sin invitaciones | Normal |
| Sala | Un solo jugador dentro, «Iniciar» deshabilitado | `R 1.1` exige 2 |

**Estados que la propuesta pedía y que hay que precisar:**

- «Sala creada» y «sala a la que me acabo de unir» **son la misma pantalla**; lo que cambia es
  quién es el anfitrión y si hay alguien más dentro. No hacen falta dos diseños.
- «Partida iniciada / no iniciada»: dentro de esta fase **solo existe «no iniciada»** más un
  estado de transición al pulsar «Iniciar». Que la sala admita o no gente **mientras** su
  partida está en curso está sin definir `PU-06`; si se decidiera que sí, aparecería un cuarto
  estado de sala («partida en curso, esperando la siguiente») que hoy **no** se prototipa.

---

## 5. Dependencias entre pantallas y flujos

**Dependencias duras** (sin A no puede existir B):

| B depende de A | Por qué |
|---|---|
| Todo → **conexión con el servidor** | El cliente no decide nada por su cuenta; el servidor es la única autoridad del estado `DR-30` |
| Amigos, Historial, Perfil, Invitaciones → **cuenta iniciada** | Los invitados no tienen ninguna de esas cosas `P-08`, `P-16`, `P-22` |
| Invitar dentro del juego → **Amigos** o búsqueda por username | La invitación va siempre a una cuenta `P-22` |
| Sala → **código propio, invitación aceptada, o creación** | No hay lista de salas `ASM-05` |
| Ranking de sala → **una partida terminada en esa sala** | `D-29` |
| Historial y Ranking global → **partidas terminadas** | Se producen fuera de esta fase |
| Iniciar partida → **2 a 4 jugadores en la sala** | `R 1.1` |

**Dependencia de navegación que el prototipo debe resolver:** con **una sala por jugador**
`P-33`, estar dentro de una sala es un estado que acompaña al jugador. Si puede irse a
Amigos o al Ranking **sin salir de la sala**, hace falta una forma visible de volver
(una barra persistente de «estás en la sala XXXX»). Si no puede, la sala es modal. **No está
definido** → `[PENDIENTE PU-08]`. Es una de las decisiones que más cambia el prototipo, y
conviene cerrarla antes de Figma.

---

## 6. Decisiones pendientes de definir

Registro `PU-`. Ninguna se resuelve en este documento. **De las veinticinco preguntas abiertas en
la versión anterior quedan once**; el resto las cerraron las decisiones de STK-02. Las cuatro que
abrieron las pantallas de la partida, `PU-26` a `PU-29`, **quedaron cerradas el 10 sep 2026** y
aparecen más abajo con lo que obligaron a dibujar; en su lugar queda abierta `PU-30`.

| # | Decisión | A qué pantalla afecta | Qué se sabe ya |
|---|---|---|---|
| **PU-02** | Si a un invitado se le ocultan o se le deshabilitan Historial, Amigos y Perfil | `PT-02` | Está definido que no tiene ninguna de las tres |
| **PU-06** | Si la sala admite entradas mientras su partida está en curso | `PT-14`, `PT-15` | Nadie se incorpora a una partida empezada `R 1.1`; una sala juega varias partidas `P-26` |
| **PU-07** | Texto exacto del rechazo por código inexistente, sala cerrada o sala llena | `PT-17` | `D-23`, `P-33`, `P-34` |
| **PU-08** | Si se puede navegar fuera de la sala sin salir de ella | Toda la navegación | Una sala por jugador `P-33`; no se sale jugando `P-35` |
| **PU-11** | Si se puede cancelar una solicitud ya enviada | `PT-09` | El rechazo borra `P-15`; eliminar una amistad aceptada ya está decidido `OB-07` |
| **PU-12** | Si se muestra el estado de conexión de los amigos | `PT-09` | No hace falta para el flujo: la invitación espera al amigo `P-21` |
| **PU-15** | Si «Configurar cuenta» tendrá algo más que el idioma y el borrado | `PT-08` | Hoy solo esos dos están respaldados `D-28`, `P-42`, `OB-06` |
| **PU-18** | Longitud, paginación y resaltado de la posición propia en el ranking | `PT-13` | El orden está definido `DR-15` |
| **PU-19** | Si el historial necesita detalle de partida propio | `PT-11`, `PT-12` | Solo puede mostrar resultados; no hay jugadas ni desglose por rondas `D-21` |
| **PU-21** | Nombre del juego en pantalla y hasta dónde llega la temática | Todas | La temática no debe alterar las reglas |
| **PU-22** | Cuál es el avatar por defecto, y si existe un catálogo además de la subida | `PT-07`, `PT-02`, `PT-15` | Está definido que debe existir uno cuando falta el archivo `P-43` |

**Cerradas el 10 sep 2026, y lo que cada una obligó a dibujar**

| # | Decisión | Cómo quedó | Qué se dibujó |
|---|---|---|---|
| **PU-26** | Desconexión y tiempo de la colocación inicial | **El mismo tiempo y las mismas reglas que un turno**: noventa segundos y la misma ventana de reconexión | Reloj en `PT-21` |
| **PU-27** | Cómo se avisa de una partida reanudada | Mientras el servidor está caído **se indica en la interfaz**; si el jugador no salió, **la partida se reanuda sola**; si salió, al volver **se le indica que sigue en curso** y elige volver o abandonarla | Nota en `PT-26` y pantalla nueva **`PT-27`** |
| **PU-28** | Caducidad del código de recuperación | **Cinco minutos** | `PT-05`, sin cambio de forma |
| **PU-29** | Si eliminar un amigo pide confirmación | **Sí** | Aviso "Eliminar amigo" en `PT-17` |

**Abierta por las pantallas nuevas**

| # | Decisión | A qué pantalla afecta | Qué se sabe ya |
|---|---|---|---|
| **PU-30** | Dónde vive exactamente el menú de la partida que contiene "Rendirse", y si agrupa algo más | `PT-22` | Debe existir una opción de menú para rendirse, disponible sea o no el turno del jugador |
**Cerrada también, sin abrir ninguna nueva:** si la ventana de amigos debía ofrecer un atajo para
invitar a una sala. **No lo ofrece.** Para invitar hay que estar dentro de una sala, y la ventana
de la sala no tiene ninguna salida hacia el menú principal: para llegar a la lista de amigos
habría que salir de la sala, y entonces ya no queda sala a la que invitar. Se retiró de `PT-09` la
opción "Invitar a mi sala" que tenía dibujada, y la invitación se queda donde funciona, en la
pestaña "Mis amigos" de `PT-16`. Solo cambiaría si el equipo decidiera que desde dentro de una
sala se puede abrir otra ventana sin abandonarla, que es la cuestión `PU-08`.

**Cerradas desde la versión anterior:** `PU-01` alias libre con identificador temporal y color
`OB-05`, `OB-20` · `PU-03`, `PU-04`, `PU-05` atribuciones del anfitrión: expulsa e inicia la
partida `OB-01`, `OB-02` · `PU-09` el correo lleva el código de la sala · `PU-10` acotada a
`Q-34`, que sigue abierta en la base · `PU-13` código alfanumérico de cuatro `OB-18` · `PU-14`
el puesto de mesa se fija al iniciar y se muestra `DR-24` · `PU-16` recuperación con código
`OB-17` · `PU-17` las salas no se reanudan y
la partida sí, con el pase del invitado `OB-45` · `PU-20` «Salir» cierra la aplicación `OB-15`
· `PU-23` sí se listan salas · `PU-24` español e inglés, por equipo `OB-06` · `PU-25` la
visibilidad en el listado es la única diferencia `OB-08`.

**Regla de trabajo del prototipo.** Lo pendiente **no se dibuja ni se anota dentro del
prototipo**: vive en este documento. El archivo del prototipo muestra **solo la interfaz que
vería el jugador**, sin comentarios, justificaciones ni referencias a decisiones. Donde una
funcionalidad no está definida, **la pantalla simplemente no la ofrece**.

---

## 7. Estructura de navegación propuesta

```
[Arranque / conexión]
   └── Menú principal ─── lista de opciones + ambientación + idioma
        ├── (avatar, esquina superior derecha)
        │     ├── sin identidad → Iniciar sesión ── Crear cuenta
        │     │                                  └─ Recuperar acceso (PU-16)
        │     │                   └── Entrar como invitado (PU-01)
        │     └── con cuenta   → Perfil ─── Cambiar username
        │                        │          Cambiar avatar (5 MB, PNG/JPG)
        │                        └── Cuenta ── Idioma (PU-24)
        │                                      Eliminar cuenta (confirmación)
        ├── Idioma (también desde el menú)
        ├── Invitaciones recibidas → aceptar → Sala | rechazar
        ├── Salas
        │     ├── Buscar por código ─────────→ Sala
        │     ├── Salas disponibles ── Unirse → Sala
        │     └── Crear sala (pública / privada) → Sala
        ├── Amigos ── Lista | Recibidas | Enviadas | Buscar por username
        ├── Historial ── (detalle de partida: PU-19)
        └── Ranking global

Sala (código · tipo · jugadores con su color · ranking de sala · chat · acciones)
   ├── Invitar jugadores → amigos / buscar por username · dentro del juego o por correo
   ├── Chat de la sala ── reportar un mensaje
   ├── Salir de la sala → Menú   (si era el último, la sala se cierra)
   ├── Expulsar (solo anfitrión, y solo sin partida en curso)
   └── Iniciar partida (solo anfitrión · 2–4 jugadores activos)
         └── Preparando la partida
               └── Colocación inicial ── un caballero por jugador + el rey
                     └── Tablero y turno ── 5 PA · 90 s · chat
                           ├── Desconexión ── reconectar | retiro
                           ├── Cierre de ronda ── puntuación parcial + nuevo rey
                           └── Resultado ── puestos, puntos y motivo de fin

Caída del servidor
   └── Reanudación ── espera de jugadores · el invitado presenta su pase
         ├── vuelven dos o más → Tablero y turno
         ├── vuelve uno → Resultado (abandonada)
         └── no vuelve nadie → Resultado (interrumpida)
```

**Dos advertencias sobre el diagrama:**

1. La rama `Sala → otras secciones` **no está dibujada a propósito**: depende de `PU-08`.
2. Las salas **no se reanudan** tras una caída `P-32`: la reanudación devuelve al jugador a la
   partida, no a una sala.

## 8. Lista consolidada de pantallas

Veintisiete pantallas. Las siete últimas son de la partida.

| # | Pantalla | Estados a dibujar | Prioridad |
|---|---|---|---|
| **PT-01** | Arranque / conexión | Conectando · Sin conexión | Media |
| **PT-02** | Menú principal | Sin identidad · Con cuenta · Invitado · Con invitaciones · Sin conexión | **Alta** |
| **PT-03** | Iniciar sesión | Vacío · Error de credenciales · **Cuenta con baneo permanente** · **Entrada de invitado** | **Alta** |
| **PT-04** | Crear cuenta | Username disponible · **Username ocupado** · Correo ya registrado | **Alta** |
| **PT-05** | Recuperar acceso | **Tres pasos: correo · código · contraseña nueva** · **Correo con formato inválido** · Código incorrecto | Media |
| **PT-06** | Entrar como invitado | Alias vacío · Con alias · **Alias repetido, admitido** | Media |
| **PT-07** | Perfil | Con avatar · **Archivo ausente, avatar por defecto** · Username no disponible · Archivo demasiado grande | **Alta** |
| **PT-08** | Configurar cuenta | Eliminar cuenta · Rechazo por partida en curso. **Ya no contiene el idioma**: no es un dato de la cuenta | Media |
| **PT-09** | Amigos | **Cuatro pestañas, una cada vez, con su estado lleno y su estado vacío:** "Amigos" · "Recibidas" · "Enviadas" · "Buscar". La búsqueda tiene sus cuatro desenlaces —se puede enviar solicitud · ya es amigo · solicitud ya enviada · te envió una solicitud— y sin resultado. **Eliminar amigo, con confirmación.** **No lleva opción de invitar a una sala**: invitar exige estar dentro de una sala y ocurre en `PT-16` | **Alta** |
| **PT-10** | Invitaciones recibidas | Con invitaciones · Vacío | **Alta** |
| **PT-11** | Historial | Con partidas · Vacío · Partida interrumpida | **Alta** |
| **PT-12** | Detalle de partida | Solo si se cierra `PU-19` | Baja |
| **PT-13** | Ranking global | Con jugadores · Vacío | Media |
| **PT-14** | Salas | Búsqueda arriba · **Solo salas públicas** · Lista vacía · Sala llena | **Crítica** |
| **PT-19** | Crear sala | Pública seleccionada · Privada seleccionada | **Alta** |
| **PT-15** | Sala | 1 jugador · 2–3 · Llena 4/4 · Anfitrión / miembro · Pública / privada · Ranking vacío · **Chat** · **Colores** · **Jugador inactivo** · **Expulsar** | **Crítica** |
| **PT-16** | Invitar jugadores | Desde amigos · Buscar por username · Ya invitado · **Canal de correo** | **Alta** |
| **PT-17** | Avisos y confirmaciones | Salir de la sala · Eliminar cuenta · Código no encontrado · Sala llena · **Confirmar expulsión** · **Aviso al expulsado** · **Sanción vigente** · **Eliminar amigo** · **Rendirse** · **Fallo del envío de correo** | Media |
| **PT-20** | Idioma | Español · English · Preferencia del equipo. **Se alcanza solo desde el menú principal**, no desde la configuración de la cuenta | Media |
| **PT-18** | Preparando la partida | Puestos de mesa · Pases entregados | Media |
| **PT-21** | **Partida · colocación inicial** | Te toca colocar · Le toca a otro · Torre ocupada · Rey fuera de torre · **Reloj de noventa segundos** | **Crítica** |
| **PT-22** | **Partida · el tablero y el turno** | Tu turno · Turno de otro · Chat con mensajes · Chat vacío · 2, 3 o 4 jugadores · **Opción "Rendirse"** | **Crítica** |
| **PT-23** | **Partida · cierre de ronda** | Puntuación parcial · Designación del rey · Empate al azar | **Alta** |
| **PT-24** | **Partida · resultado** | Completada · Con invitados · Abandonada · Interrumpida · **Se vuelve a la sala, no al menú** | **Alta** |
| **PT-25** | **Partida · desconexión** | Tu ventana corriendo · Ventana agotada · Rival sin conexión · Rival retirado | **Alta** |
| **PT-26** | **Reanudación tras la caída** | Esperando · Pase presentado · Vuelve uno · No vuelve nadie · **Se reanuda sin intervención** | Media |
| **PT-27** | **Partida en curso al volver** | El jugador salió mientras el servidor estaba caído y elige volver a la partida o abandonarla | **Alta** |

**No se prototipa:** configuración de cuenta más allá del idioma y el borrado `PU-15`, y
cualquier pantalla de moderación, porque las sanciones son automáticas y `D-31` descarta el rol
de administración.

## 9. La temática visual

La correspondencia **flores = torres, orugas = caballeros, mariposa = rey** es la del tablero, y
el tablero ya está prototipado con ella: el jugador ve un jardín, no una cuadrícula. Los
castillos son **jardineras de obra** sobre la hierba, cada torre es **una flor** cuya altura es
su nivel, los caballeros son **orugas del color de su jugador** posadas sobre la flor, y el rey
es **la mariposa**.

En el flujo previo a la partida la ambientación sigue donde estaba: la ilustración del menú, los
avatares y el nombre del juego. En la partida, la identidad de cada jugador se lleva con **su
oruga de color**, que es la misma marca que aparece en la sala, en las listas y en los
resultados.

---

## 10. El tablero

**Vista en perspectiva.** El tablero se dibuja en escorzo, con las filas del fondo más
estrechas y las del frente más anchas, de modo que **las alturas se ven**: una flor de nivel 1
es baja, una de nivel 3 se alza al triple y tapa parcialmente lo que tiene detrás. Cada nivel
añade un par de hojas al tallo, así que la altura se puede contar además de verse. **No hay
ningún número sobre el tablero.**

- **Los castillos son jardineras**: una superficie de tierra clara, con su borde, que agrupa las
  flores contiguas. Es lo que hace legible la regla que gobierna todo el movimiento —«dentro
  del mismo castillo»— y la puntuación por superficie.
- **Las orugas se posan sobre la flor** en la que está el caballero, con el color de su jugador.
- **La mariposa vuela sobre la flor** donde está el rey, y no usa ninguno de los cuatro colores
  de jugador para que no se confunda con una pieza más.
- **La casilla elegida** se marca con un halo en la base de su flor, sin recuadros ni etiquetas.
- Las piezas se dibujan **de fondo hacia el frente**, de modo que las flores cercanas se
  superponen a las lejanas y la profundidad se lee sola.

**El resto de la pantalla acompaña al tablero, no compite con él.** El reloj va **centrado y
grande** sobre el tablero, con la ronda y el turno debajo en una sola línea, y las acciones son
**pastillas con su coste en PA** justo bajo el tablero, no una tabla de precios.

**Reparto del espacio alrededor del tablero.** A la izquierda, bajo la lista de jugadores, van
las **orugas que le quedan por colocar** y las **flores que puede plantar este turno**. A la
derecha, el **chat**, el **mazo** con su acción de robar y el botón de terminar el turno. En la
franja inferior, los **PA** en la esquina izquierda y las **cartas ya obtenidas centradas**, con
sitio para las siete a tamaño legible. El nombre del jugador no se repite abajo: ya está en la
lista y en la cabecera. No hay tarjetas ni marcos alrededor de cada dato: solo una línea que
separa la franja del tablero.

**La inclinación es moderada a propósito.** Basta para que las alturas se vean y para que el
tablero parezca una mesa, pero no tanto como para deformar las casillas del fondo: las
posiciones se siguen identificando de un vistazo.

> **Lo que el prototipo no puede acertar: el mapa inicial de castillos.**
> `R 1.2` declara que la distribución del tablero es **fija en todas las partidas** y **nunca la
> dibuja**. La base del proyecto ya tiene anotada esa corrección documental como pendiente. El
> prototipo usa **una distribución de ejemplo** —cinco jardineras de tres y cuatro flores, con
> alturas variadas—. **Cuando se defina el mapa real, se sustituye: no cambia nada más de la
> pantalla.**

---

## 11. Qué aporta cada pantalla de la partida

| Pantalla | Qué resuelve | Reglas que hace visibles |
|---|---|---|
| **PT-21** Colocación inicial | El orden de colocación por puesto de mesa y las dos condiciones de colocación | `DR-05`, `DR-24`, `R 2.3`, `R 2.1` |
| **PT-22** Tablero y turno | El presupuesto del turno: 5 puntos de acción, 90 segundos y el coste de cada acción; las construcciones de ese turno; el mazo y las cartas obtenidas; el chat | `R 3.1`, `R 3.2`, `R 5.1`, `R 1.3`, `R 2.4`, `DR-06`, `OB-25` |
| **PT-23** Cierre de ronda | Que la puntuación se toma por rondas y no en tiempo real, y que el rey lo coloca el de menor puntuación | `DR-02`, `R 4.1`, `R 4.2`, `DR-23`, `R 2.3` |
| **PT-24** Resultado | La suma final con las cartas no usadas, el desempate y el motivo de fin | `R 4.3`, `DR-15`, `P-37` |
| **PT-25** Desconexión | La ventana de reconexión corriendo, el retomar el turno intacto y el efecto del retiro sobre el tablero | `R 5.2`, `DR-20`, `DR-30`, `DR-25`, `DR-32` |
| **PT-26** Reanudación | El plazo desde el arranque, el pase del invitado y los tres desenlaces | `P-30`, `P-28`, `P-31`, `P-37`, `OB-45` |

**Una corrección sobre la propuesta de distribución.** El boceto de referencia mostraba un
marcador de puntos vivo junto a cada rival y **dos tipos** de construcción disponible. Ninguna
de las dos cosas procede: la puntuación se calcula **al cerrar cada ronda** `DR-02`, así que la
ficha muestra la de la última ronda cerrada; y `R 1.3` da **una sola cantidad** de
construcciones por jugador y turno, no dos clases distintas.

---

## 12. Trazabilidad pantalla ↔ caso de uso

| Caso de uso | Pantallas |
|---|---|
| CU-01 Iniciar sesión · CU-02 Crear cuenta | PT-03, PT-04 |
| CU-03 Recuperar acceso | PT-05 |
| CU-04 Jugar como invitado | PT-03, PT-06 |
| CU-05 Cambiar el idioma | PT-20, PT-02. **Ya no PT-08**: el idioma salió de la configuración de la cuenta |
| CU-06 Modificar el perfil · CU-07 Eliminar la cuenta | PT-07, PT-08, PT-17 |
| CU-08 Enviar solicitud · CU-09 Responder solicitud · CU-20 Eliminar un amigo | PT-09, en sus cuatro pestañas, y PT-17 para la confirmación de eliminar |
| CU-10 Historial · CU-11 Detalle de una partida | PT-11, PT-12 |
| CU-12 Ranking global | PT-13 |
| CU-13 Entrar a la sala | PT-15 |
| CU-14 Unirse a una sala · CU-15 Crear una sala | PT-14, PT-19, PT-17 |
| CU-16 Invitar jugadores | PT-16 |
| CU-17 Responder una invitación | PT-10 |
| CU-18 Salir de la sala | PT-15, PT-17 |
| CU-19 Iniciar la partida | PT-15, PT-18 |
| CU-21 Expulsar a un jugador | PT-15, PT-17 |
| CU-22 Enviar un mensaje al chat | PT-15, PT-22 |
| CU-23 Reportar a un jugador | PT-15, PT-22, PT-17 |
| CU-24 Preparar la partida | PT-21, con su reloj, y PT-23 para el rey de las rondas siguientes |
| CU-25 Jugar un turno | PT-22 |
| CU-26 Reconectar a una partida en curso | PT-25 |
| CU-27 Volver a una partida reanudada | PT-26, PT-27 |
| CU-28 Abandonar la partida | PT-22, PT-17, PT-27 |
| Cierre de ronda y fin de partida *(sin actor)* | PT-23, PT-24 |
| Inactividad y retirada de la sala *(sin actor)* | PT-15 |

**Los veintiocho casos de uso tienen al menos una pantalla.** Los dos únicos que no la tienen
propia —CU-24 en su variante de rondas posteriores y CU-23— se representan dentro de la pantalla
en la que realmente ocurren, que es donde el jugador los vive.

---

## 13. Trazabilidad pantalla ↔ requisito

| Requisito | Pantallas |
|---|---|
| 1 · Cuentas `[N1]` | PT-03, PT-04, PT-05, PT-07, PT-08 |
| 2 · Historial `[N1]` | PT-11, PT-12 |
| 3 · Ranking global `[N1]` | PT-13 |
| 4 y 5 · Salas y entrada por código `[N2]` | PT-14, PT-15 |
| 6 · Invitación por correo `[N2]` | PT-16, PT-10 |
| 7 · La sala sobrevive al creador; muere vacía `[N2]` | PT-15, PT-17 |
| 8 · Amigos `[N2]` | PT-09 |
| 9 · Invitar a un amigo `[N2]` | PT-16, PT-10 |
| 10 · Perfil con avatar `[N2]` | PT-07 |
| 11 · Ranking de sala `[N2]` | PT-15 |
| 12 · Reconexión tras caída `[N2]` | PT-25, PT-26, PT-01 |
| La partida `[R]` | PT-18, PT-21, PT-22, PT-23, PT-24 |
| Chat de sala `[STK-02]` | PT-15, PT-22 |
| Reportes y sanciones `[STK-02]` | PT-22, PT-17, PT-03 |
| Colores de jugador `[STK-02]` | PT-06, PT-15, PT-21, PT-22 |
| Expulsión `[STK-02]` | PT-15, PT-17 |
| Salas públicas y privadas · listado `[STK-02]` | PT-19, PT-14, PT-15 |
| Cambio de idioma `[STK-02]` | PT-02, PT-20 |

**Todos los requisitos quedan cubiertos. Ninguno queda sin pantalla por olvido.**

---

## 14. Revisión contra el diccionario i18n (v4.1, 16 sep 2026)

**Botón de regreso.** El criterio se tomó de la versión del prototipo que usó el equipo para el
diccionario:

- **Formularios** (`PT-04`, `PT-06`, `PT-19`, `PT-20`): «Volver» es el botón secundario de la
  tarjeta, junto a la acción principal.
- **Resto de pantallas:** un botón **al pie de la pantalla** cuyo rótulo dice el destino.
  - «Volver al menú»: `PT-03`, `PT-07`, `PT-09` (en las cuatro pestañas), `PT-10`, `PT-11`,
    `PT-13` y `PT-14`.
  - «Volver al perfil»: `PT-08`.
  - «Volver a la sala»: `PT-16`.
- **`PT-05`:** «Volver a iniciar sesión» en los pasos 1 y 3, como pide CU-03.
- **Sin cambios:** `PT-12` («Volver al historial») y `PT-24` («Volver a la sala»).
- **Sin regreso, porque su salida es otra:** `PT-01`, `PT-02`, `PT-15`, `PT-17`, `PT-18` y
  `PT-21` a `PT-27`.

**Otros cambios del prototipo:**

- **`PT-02`:** el avatar abre un menú con la opción «Perfil» (disparador de CU-06), solo con
  cuenta. Hay un conmutador nuevo, «Menú del avatar».
- **`PT-16`:** cada jugador ofrece «Invitar por correo» junto a «Invitar» (CU-16 FA05).
- **`PT-10`:** el botón de rechazo tiene el tooltip «Rechazar».
- **`PT-13`:** los miles se escriben con coma (`1,480`), que es lo que produce es-MX en .NET.
- **`PT-24`:** el puesto se muestra como ordinal (`1.º`), igual que en `PT-11` y `PT-12`.
- **`PT-20`:** el metadato ya no dice que el idioma se abra desde la cuenta.
- **Menú lateral:** `PT-18` y `PT-22` se rotulan igual que su título.

