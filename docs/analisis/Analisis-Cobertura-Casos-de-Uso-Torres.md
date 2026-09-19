# Análisis de funcionalidades y revisión de cobertura de los casos de uso — Juego de Torres

**Documento:** `Analisis-Cobertura-Casos-de-Uso-Torres.md` · **Versión 5.3** · 9 sep 2026
**Alcance:** Actividad 1, instrucciones 1 y 2. La instrucción 1 —análisis de funcionalidades—
se resuelve en la **Parte 1**; la instrucción 2 —casos de uso— **no se describe aquí**: la
Parte 2 revisa **cobertura y completitud** de `CU-01`…`CU-19`. La **Parte 3** resuelve las
cuestiones que STK-02 encargó analizar.

**Regla aplicada.** Nada se afirma sin una fuente del proyecto detrás. `[NO DETERMINABLE]`
cuando la fuente no alcanza, `[AMBIGUO]` cuando dos fuentes chocan, `[PROPUESTA]` cuando lo
propone este análisis, **`[RESUELTO POR ENCARGO]`** cuando STK-02 delegó la decisión y este
análisis la cierra con su justificación. No se añade nada por ser habitual en otros juegos.

**Estructura de versiones.** El documento **se actualiza en su sitio**: no se apilan capas ni se
conservan estados obsoletos junto a los vigentes. La sección 0 dice qué cambió y qué
conclusiones quedan retiradas.

**Fuentes** — `R` reglas v3.0 · `B` base oficial (`DR-`, `RES-`, `ASM-`, frontera §4.1) ·
`PER` análisis de persistencia v4 (`D-`, `P-`) · `SQL` `schema.sql` y modelo ER · `PROT`
análisis de pantallas v2.0 (`PT-`, `PU-`) · `CU` documento de casos de uso · `OB` aclaraciones
de STK-02, **cuatro rondas del 8 sep 2026 y una del 9 sep**.

---

## 0. Qué cambia respecto a la versión anterior

### 0.1 Las tres primeras rondas, ya asentadas

**`OB-01`…`OB-17`** — el anfitrión **expulsa** e **inicia la partida**; **hay chat** de sala
conservado hacia sus partidas; **hay reportes y sanciones**; alias de invitado libre con
identificador temporal; idioma por equipo, inglés y español; **se eliminan amigos**;
pública/privada solo cambia la visibilidad en el listado; el anfitrión pasa al siguiente del
orden guardado; el invitado no invita; «Salir» sale del juego; al modo invitado se entra desde
la ventana de inicio de sesión; «Recuperar acceso» manda un código.

**`OB-18`…`OB-31`** — código de sala **alfanumérico de 4**; **color de jugador** para
desambiguar alias iguales; el expulsado **puede volver**; **no se expulsa con partida en
curso**; anfitrión desconectado → **inactivo** y traspaso; **el chat no se guarda**;
**escribir consume turno**; **el que entra tarde ve lo anterior**; **no se reporta a un
invitado**; la sanción temporal **prohíbe entrar a salas y jugar**; reportes y sanciones **se
borran con la cuenta**.

**`OB-32`…`OB-44`** — carta 7 redefinida; escalera de prohibiciones **5 h / 1 día / 3 días**;
los 5 reportes se acumulan **en partidas diferentes**; un reporte fuera de partida **cuenta como
uno**; los umbrales **viven en tablas de parámetros**; el **baneo permanente impide iniciar
sesión**; la sanción que salta jugando **se aplica al terminar la partida**; el invitado
**tampoco puede reportar**; el color **se asigna en la sala**; **el inactivo no cuenta ni entra
a la partida**; al expulsado **sí** se le avisa y al eliminado de amigos **no**.

### 0.2 Cuarta ronda — `OB-45`…`OB-50`

Cinco respuestas. **Dos cierran huecos, una ratifica una propuesta y dos delegan la decisión en
este análisis.**

| # | Aclaración | Efecto |
|---|---|---|
| `OB-45` | **El pase de invitado se queda.** Cómo se resuelve, delegado a este análisis | **Retira `DR-21`.** Los invitados **sí** vuelven tras una caída — §3.4 |
| `OB-46` | **El jugador inactivo no ocupa plaza en la partida**: **se queda en la sala**, pero no entra a jugar. La sala **nunca contiene más de cuatro personas** | Cierra el hueco 44 — §0.6 |
| `OB-47` | **Usar la carta 7 no cuesta nada**; lo que cuesta son **sus acciones**, que **siguen las reglas de coste de movimiento** | Cierra el hueco 39 — §0.4 |
| `OB-48` | **La carta 3 hace lo que dice; nada que ver con la 7** | Cierra el hueco 40 — §0.4 |
| `OB-49` | **Los baneos, las duraciones y su conteo**, delegados a este análisis; y **§3.3 se mantiene tal como se propuso** | Cierra los huecos 41, 42, 43 y 45 — §0.5 — y **ratifica el latido de `Q-32` y la regla del reloj de `Q-33`** |
| `OB-50` *(9 sep)* | **Pasado cierto tiempo inactivo, el jugador sí sale de la sala** | Segunda fase de la inactividad. **Cierra el punto de las salas que se quedaban abiertas** — §0.6 |

### 0.3 Conclusiones retiradas

Acumulado; la última es de esta versión.

| # | Conclusión retirada | Motivo |
|---|---|---|
| `R-1` | «Expulsar no está respaldado; no es un caso de uso que falte» | `OB-01` |
| `R-2` | «El chat está fuera de alcance y no debe aparecer en ningún caso de uso» | `OB-03` |
| `R-3` | FC-03 con resultado `[NO DETERMINABLE]` | `OB-17` |
| `R-4` | «No determinable dónde se guarda el idioma» | `OB-06` |
| `R-5` | «Eliminar una amistad aceptada: sin definir» | `OB-07` |
| `R-6` | «Qué significa cada tipo de sala sigue abierto» | `OB-08` |
| `R-7` | «Quién puede iniciar la partida: `[NO DETERMINABLE]`» | `OB-02` |
| `R-8` | «Criterio de sucesión del anfitrión: `[NO DETERMINABLE]`» | `OB-10` |
| `R-9` | «Si un invitado puede invitar: `[NO DETERMINABLE]`» | `OB-11` |
| `R-10` | CU-04 disparado desde `GUIMainMenu` | `OB-16` |
| `R-11` | «Lectura de memoria para el chat, a confirmar» | `OB-24` |
| `R-12` | «Sin decidir qué prohíbe la sanción, CU-01, CU-13 y CU-19 no pueden completarse» | `OB-30` |
| `R-13` | «Acotar el chat», sin cifra | `OB-26`; §3.2 la propone |
| `R-14` | «La carta 7 es la última pregunta de reglas abierta» | `OB-19`, `OB-32` |
| `R-15` | «El umbral de 5 reportes es inalcanzable con 4 jugadores» | `OB-34`: eran **5 partidas diferentes**, no 5 en una |
| `R-16` | «CU-01 vuelve a los correctos» | `OB-37`: el baneo permanente **sí** bloquea el inicio de sesión |
| `R-17` | «`R 2.4` y `OB-19` se contradicen sobre el nivel» | `OB-32` |
| **`R-18`** | **«La carta 3 podría quedar subsumida por la carta 7»** | `OB-48` lo descarta, y §0.4 encuentra **en las propias reglas** qué las separa: **la carta 3 es gratis y la 7 se paga** |
| **`R-19`** | **«El inactivo sigue en la lista de la sala sin contar para el máximo»** — lectura de la v5.0 | Permitiría una sala de **cinco personas**, y la sala solo contiene cuatro |
| **`R-20`** | **«Al marcarlo inactivo, el jugador sale de la sala»** — lectura de la v5.1 | **Se queda en la sala** al ser marcado. La plaza que no ocupa es **la de la partida**. Sale, pero **después**, al vencer el plazo de `OB-50` — §0.6 |

### 0.4 La carta 7 y la carta 3, cerradas — y por qué no se pisan

**`OB-47` cierra el coste.** Usar la carta no cuesta nada, que es además la regla general de
`R 2.4` —«utilizar una carta ya obtenida no tendrá ningún coste»—. Lo que se paga es **el
movimiento que la carta habilita**, con las tarifas de `R 3.2`:

| Movimiento con la carta 7 | Coste |
|---|---|
| Recorrer el castillo hasta otra torre **al mismo nivel** | **1 PA** — mover un caballero |
| Recorrer y **bajar** cualquier número de niveles | **1 PA** — bajar no se cobra `R 2.1`, `R 3.2` |
| Recorrer y **subir un nivel** | **2 PA** — 1 por mover, 1 por el nivel |
| Recorrer y **subir dos niveles** —de 1 a 3, el máximo por `DR-03`— | **3 PA** |
| **Subirse al castillo desde el tablero** y hacer lo anterior | Lo mismo: el movimiento y los niveles que suba |

**Corrección documental que esto deja en `R 2.4`.** El texto dice que la carta 7 «es la única
que señala un coste, de 1 punto de acción». Con `OB-47` ese 1 PA **no es un coste plano de la
carta**: es la tarifa de mover un caballero, y los niveles se cobran aparte. Hay que reescribir
esa frase, junto con «subiendo un nivel», que `OB-32` ya había superado.

**`OB-48` separa las dos cartas, y las reglas dicen por qué.** No hay que dar la separación por
supuesta: está escrita en `R 2.4`. **«La acción que permite una carta costará únicamente los
puntos de acción que la propia carta indique»**, y **la carta 3 no indica ninguno**. Entonces:

| | Carta 3 | Carta 7 |
|---|---|---|
| Qué hace | Sube un caballero **del nivel 1 al 3 de una torre** | **Recorre el castillo** de una torre a otra, sin importar las casillas, y permite **subirse al castillo desde el tablero** |
| Qué regla rompe | La de **subir un solo nivel por movimiento** `R 2.1` | La de **recorrer casillas** para llegar a otra torre |
| Qué cuesta | **Nada.** No indica coste `R 2.4` | **Su movimiento y sus niveles** `OB-47` |

**La diferencia es económica y está en las reglas:** la carta 3 regala una subida de dos
niveles; la carta 7 regala **distancia**, y cobra la altura como cualquier movimiento. Se
parecen en el efecto y no en el precio, que es exactamente lo que `OB-48` afirma.

### 0.5 `OB-49` · El régimen de baneos, resuelto entero

`OB-49` delegó cuatro decisiones. Se cierran aquí, cada una con el motivo que la sostiene.
**`[RESUELTO POR ENCARGO]`** — bastan cuatro líneas de STK-02 para ratificarlas.

**1 · La escalera y el permanente.** `OB-29` habla de **tres** baneos temporales antes del
permanente y `OB-33` da **tres** duraciones. Se leen como la misma escalera: **1.º → 5 horas ·
2.º → 1 día · 3.º → 3 días**, y **el cuarto cruce del umbral es el baneo permanente**. Es la
única composición que usa las dos respuestas sin sobrar ni faltar nada.

**2 · El contador de reportes se reinicia después de cada baneo temporal; el contador de baneos
no.** Son **dos contadores distintos**, y esa es la clave:

- Si el de reportes **no** se reiniciara, el sexto reporte dispararía el segundo baneo, el
  séptimo el tercero y el octavo el permanente: **la cuenta sería 5, 6, 7, 8** en vez de 5, 10,
  15, 20. Una escalera cuyos peldaños suben de 5 h a 3 días **existe para dar margen a
  corregirse**; sin reinicio no hay margen, y el permanente llegaría casi de golpe.
- El de **baneos sí se acumula**, porque es lo que `OB-29` llama reincidencia. No hace falta
  guardarlo: **se cuenta sobre las sanciones ya registradas** de esa cuenta.

**3 · Varios reportes en la misma sala fuera de partida cuentan como uno.** `OB-34` fijó el
principio —lo que se acumula es **la ocasión**, no el reporte suelto: cinco **partidas**
diferentes, no cinco reportes—. `OB-35` dice que el reporte de sala «cuenta como uno». Aplicar
el mismo principio al único otro sitio donde vive el chat da: **la ocasión, fuera de partida, es
la sala**. Si no fuera así, la sala sería un camino más barato hacia la sanción que la partida,
y nada justifica esa diferencia.

*Simplificación que se asume y conviene decir en voz alta:* con este criterio, dos reportes en
**la misma sala** pero en momentos distintos —antes de la primera partida y entre dos partidas—
cuentan **una sola** ocasión. Distinguirlos exigiría un concepto de «sesión de sala» que el
proyecto no tiene.

**4 · Una sanción diferida empieza a contar cuando entra en vigor, no cuando se cruza el
umbral.** `OB-38` dice que el jugador **termina la partida** y después queda bloqueado. Si las
5 horas empezaran en el umbral, una partida de 4 jugadores —hasta **60 minutos** `B §5`— se
comería una quinta parte del castigo mientras el sancionado sigue jugando, y **cuanto más
durase la partida, menos duraría la sanción**. Además `OB-30` define la prohibición como *no
entrar a salas y no jugar*: no puede empezar mientras está jugando. Por eso **el instante de
inicio se guarda aparte**; no es deducible del instante del umbral.

**El régimen queda así, completo**

| Elemento | Valor | Fuente |
|---|---|---|
| Qué se cuenta | Ocasiones con reporte: **partidas diferentes**, y **cada sala** para lo dicho fuera de partida | `OB-34`, `OB-35`, §0.5-3 |
| Umbral | **5 ocasiones** | `OB-29` |
| Al cruzarlo | Baneo temporal, y **el contador de reportes vuelve a cero** | §0.5-2 |
| Escalera | **5 h → 1 día → 3 días**, y el **cuarto** es permanente | `OB-33`, §0.5-1 |
| Inicio de la sanción | Al entrar en vigor: inmediato, o **al terminar la partida** si se cruzó jugando | `OB-38`, §0.5-4 |
| Qué prohíbe la temporal | **Entrar a salas y jugar**; no iniciar sesión | `OB-30` |
| Qué prohíbe la permanente | **También iniciar sesión** | `OB-37` |
| Quién reporta y a quién | **Solo cuentas, y solo contra cuentas** | `OB-28`, `OB-39` |
| Dónde viven los números | **Tablas de parámetros** | `OB-36` — §3.5 |
| Al borrar la cuenta | Reportes y sanciones **se eliminan** | `OB-31` |

### 0.6 `OB-46` y `OB-50` · La inactividad tiene dos fases

**De dónde viene la segunda.** «Si se le saca de la sala al cabo de un tiempo» era una de las
tres partes del **hueco 34** que la v3.0 dejó abierto; `OB-46` respondió a otra de ellas y esa
se quedó sin cerrar. **No estaba acordada: estaba planteada.** `OB-50` la cierra ahora, y con
ella cae el punto que la v5.2 dejaba señalado.

**Fase 1 — inactivo, dentro de la sala.** El servidor detecta la desconexión **por el latido**
`OB-49` y marca al jugador **inactivo**. **Se queda en la sala y sigue ocupando una de sus cuatro
plazas** `OB-46`, `P-34`; lo que **no** ocupa es **plaza en la partida**, a la que no entra
`OB-41`. Si era el anfitrión, la condición pasa al siguiente del orden `OB-23`, `OB-10`. Si
reconecta dentro de esta fase, **vuelve a activo sin nada que recuperar**.

**Fase 2 — pasado el plazo, fuera de la sala** `OB-50`. Si sigue inactivo cuando vence el plazo,
**se le retira de la sala**: su plaza queda libre, otro puede entrar, y **si era el último la
sala se queda vacía y se cierra** con su fecha de cierre, su código liberado y sus invitaciones
eliminadas `D-23`, `FC-15`. Para volver, entra como cualquiera.

**Dos lecturas anteriores quedan retiradas** (`R-19`, `R-20`): ni el inactivo deja libre su plaza
al ser marcado —eso daría salas de cinco—, ni se le saca en ese mismo instante. **Sale, pero
después.**

**Lo que esto cierra.** La v5.2 señalaba que una sala con solo inactivos **no está vacía por la
letra** de `D-23` y seguiría abierta hasta que `DR-18a` la cerrase al arrancar el servidor.
Con `OB-50` **se cierra sola**: los inactivos se van venciendo el plazo, el último deja la sala
vacía y `D-23` la cierra. **No hace falta ninguna regla nueva.**

**El plazo, que es lo único que falta** `[PROPUESTA]`. Es un umbral, así que por `DR-29` lo fija
STK-02 y por `OB-36` vive en la tabla de parámetros (§3.5). Propuesta: **3 minutos**, el mismo
valor que `P-31` usa para esperar a que vuelvan los jugadores de una partida interrumpida. Dos
razones: es la única espera comparable que el proyecto ya tiene escrita —«cuánto se aguanta a
alguien que no está»—, y en la sala **no hay reloj de por medio**, así que no procede algo tan
corto como los 90 s de `R 5.2`. Como `P-31`, se declara **valor de pruebas** y se ajusta sin
tocar datos.

**Una precisión necesaria para no chocar con `P-35`.** El plazo de `OB-50` **no puede sacar de
la sala a quien está dentro de una partida**: a ese jugador lo gobierna `R 5.2` —su ventana de
90 s y, si la agota, el retiro de la partida `DR-25`, `DR-32`— y `P-35` prohíbe salir de la sala
mientras se juega. La retirada por inactividad se aplica, por tanto, **a quien no está en una
partida en curso**; para el que sí lo está, cuenta cuando la partida termine.

**Al retirado no hace falta avisarle** en el momento, porque por definición está desconectado; lo
descubre al volver, cuando ya no está en la sala. Es una deducción, no una regla dada.

### 0.7 Correcciones que estas aclaraciones obligan a hacer fuera de este documento

1. **`DR-21` queda retirado** —«los invitados no vuelven tras una caída»— por `OB-45`. Por la
   regla de gobernanza del proyecto, **su identificador no se reutiliza**: hace falta un `DR-`
   nuevo que diga lo contrario. §3.4.
2. **`B §4.1` y `PER §12`**: retirar «chat» y «salas públicas listadas» de lo que está «fuera de
   alcance, y no volverá»; revisar «registro de auditoría» a la luz de `OB-04`.
3. **`ASM-05`** queda muerto → se activa la cláusula condicional de `CON-03` y **`CAND-11` deja
   de ser opcional**.
4. **`D-30`**: `OB-36` lo amplía con los parámetros de sanción, de chat y —desde `OB-49`— **los
   del latido** `P` y `N`. §3.5.
5. **`schema.sql`**: `room.code` a **cuatro caracteres exactos** `OB-18` — `HC-17`.
6. **`R 2.4`**: reescribir la carta 7 —el «subiendo un nivel» y el «coste de 1 punto de
   acción»— según `OB-32` y `OB-47` (§0.4), junto a la corrección de `R 2.1` pendiente por
   `DR-22`.
7. **`PER §4.1`**: la justificación de `D-20` ya no puede apoyarse en preguntas de reglas
   abiertas, porque **no queda ninguna**. `D-20` **se conserva** por su otro motivo: solo el
   servidor lee el tablero, y lo lee entero.
8. **`PER §6.2` y `P-38`**: reescribir la reanudación con el pase adoptado; **la alternativa de
   reclamar el puesto solo por el alias queda descartada**. §3.4.
9. **`Q-32` y `Q-33` pasan a cerradas** por `OB-49`, con la salvedad de comprobar `STACK.md`.

**Impacto de seguridad acumulado.** `RES-09` fija `SecurityMode.None` y `C-2` sigue abierto por
la credencial sin cifrar. Con `OB-03` **los mensajes viajan sin cifrar**; con `OB-04` el sistema
**guarda texto escrito por personas**; con `OB-27` **quien entra tarde lee lo anterior**; y
desde `OB-45` **circula un pase que da acceso a un puesto de partida**. Agrava `TEN-I` y toca
`CON-06`.

---
---

# Parte 1 · Análisis de las funcionalidades, comportamientos, interacciones y situaciones que sí pertenecen a Torres

## 1.1 Frontera aplicada

`B §4.1`, **corregida por `OB-03` y `OB-04`**. Dentro: la partida (tablero, castillos,
caballeros, construcciones, cartas, rey, turnos, puntuación), cuentas, perfil con avatar,
amistades, salas, código e invitaciones, rankings global y de sala, reconexión y reanudación,
registro de eventos, **chat**, y **reportes y sanciones**.

**La partida está dentro de la frontera desde el primer renglón de la base.** Que la fase de
prototipos parara en «Iniciar partida» fue una decisión de *esa fase*, no un recorte del sistema.

## 1.2 Cómo se lee «CRUD» en Torres

| Marca | Significado | Fuente |
|---|---|---|
| `[BD]` | Datos permanentes | `PER §2` |
| `[BD-T]` | Datos temporales de la base | `PER §2` |
| `[MEM]` | Estado en memoria: salas, miembros, anfitrión, ranking de sala, chat, colores, reloj, PA | `D-26`, `D-29`, `OB-24`, `OB-40` |
| `[DER]` | Dato calculado | vista `player_ranking` |
| `[CLI]` | Solo cliente | — |
| `[ARCH]` | Archivo en el disco del servidor | `P-43`, `DEP-E10` |

## 1.3 Inventario de funcionalidades

Con actor, entrada, datos, reglas y resultado, como pide la actividad. **No son descripciones
de casos de uso.** Los identificadores nunca se reasignan.

### Bloque A · Identidad, cuenta y perfil

**FC-01 · Crear cuenta** `[BD]` · *Create* — username `^[A-Za-z0-9_]{3,20}$` único sin
distinguir mayúsculas, correo único, hash BCrypt con coste 11 configurable `D-03`, `P-46`;
BCrypt ignora lo que exceda 72 bytes; prohibido registrar la contraseña `RES-06`.

> **Consecuencia de `OB-31` + `OB-37`.** El baneo permanente impide iniciar sesión, pero borrar
> la cuenta se lleva sus reportes y sanciones `OB-31`, y crearse otra solo exige username y
> correo libres. **El baneo permanente es evitable.** Se sigue de `OB-31`, `D-28` y FC-01.

**FC-02 · Iniciar sesión** `[BD]` · *Read* — verifica contra el hash `P-04`; no guarda sesiones
ni tokens `PER §2`. **Comprueba y rechaza el baneo permanente** `OB-37`; la sanción temporal no
bloquea aquí `OB-30`.

**FC-03 · Recuperar el acceso** `[BD]` `[DEP-E9]` · *Read* + *Update* — **envía un código**, el
jugador lo introduce y **pone otra contraseña** `OB-17`, guardada como hash `D-03`. **El código
no existe en el modelo** — `HC-15`. Depende del canal de correo `DEP-E9`, `Q-34`.

**FC-04 · Consultar y modificar el perfil** `[BD]` `[ARCH]` — username y avatar `D-22`; PNG o
JPG, máximo 5 MB, validado en servicios `P-43`; la base guarda solo la referencia; si el archivo
falta, avatar por defecto.

**FC-05 · Eliminar la cuenta** `[BD]` `[ARCH]` · *Delete* — cascada sobre `account`,
`friendship`, `room_invitation`, `match_participant` **y sus reportes y sanciones** `OB-31`;
borra **el archivo del avatar** y **la partida que quede sin participaciones** `PER §7.5`.
Irreversible `D-28`; **prohibida con una partida en curso** `P-42`.

**FC-06 · Obtener identidad de invitado** `[MEM]` · *Create* solo en memoria — alias **libre**
`OB-05`, con **identificador temporal** del servidor que, desde `OB-45`, **es también su pase de
asiento** (§3.4); en pantalla, **marca de invitado y color** `OB-20`, `OB-40`. No escribe
ninguna fila `P-08`. Juega, crea salas y puede ser anfitrión `P-19`, `P-41`; **no tiene
historial, ranking global, amigos ni invitaciones, no invita** `OB-11`, **no reporta** `OB-39`
**y no puede ser reportado** `OB-28`. Se entra desde la ventana de inicio de sesión `OB-16`.

**FC-07 · Cambiar el idioma** `[CLI]` — **inglés y español**, preferencia **por equipo** `OB-06`.

### Bloque B · Amistades

**FC-08 · Buscar por username** `[BD]` · *Read* — **solo por username** `P-16`.
**FC-09 · Enviar solicitud** `[BD]` · *Create* — una fila por pareja; sin bloqueo `P-15`.
**FC-10 · Aceptar** · *Update* · **FC-11 · Rechazar** · *Delete*, **sin dejar registro** `P-15`.
**FC-34 · Eliminar una amistad aceptada** `[BD]` · *Delete* `OB-07` — **borrado simétrico**, y
**al eliminado no se le avisa** `OB-43`.

### Bloque C · Salas e invitaciones

**FC-12 · Crear una sala** `[BD]` `[MEM]` · *Create* — actor con cuenta **o invitado** `P-41`.
**Crea** la fila `room` `D-23` y, en memoria, su **lista ordenada** de miembros, anfitrión,
ranking, **chat** y **colores**. Código **alfanumérico de cuatro** `OB-18`, **único solo entre
salas abiertas** —con reintento ante colisión—; **máximo cuatro personas, nunca más, contando a
los inactivos** `P-34`, `OB-46`; una
sala por jugador `P-33`; **pública o privada solo cambia la visibilidad en el listado** `OB-08`.

> **Deducción de `OB-08`:** a una sala privada solo se entra **por código o por invitación**,
> las únicas vías documentadas una vez retirado el listado.

**FC-13 · Consultar salas abiertas y buscar por código** `[MEM]` · *Read* — del estado en
memoria `D-26`, **solo las públicas** `OB-08`. Sigue contradiciendo `B §4.1` y `PER §12` —
`HC-09`.

**FC-14 · Entrar a una sala** `[MEM]` · *Update* — agrega a la lista ordenada y avisa. Sala
abierta, **con menos de cuatro jugadores contando a los inactivos** `P-34`, `OB-46`, y el
jugador no puede estar en otra `P-33`.
**Comprueba la sanción temporal** `OB-30`. **Asigna el color** `OB-40`.

**FC-15 · Salir de una sala** `[MEM]` (+ `[BD]`) — si era el último, **cierra la sala** con su
fecha `D-23` y elimina sus invitaciones; ranking y chat se descartan `P-24`, `OB-24`. **No se
sale jugando** `P-35`; el anfitrión pasa **al siguiente del orden** `OB-10`, lo que exige
**orden explícito**. **La sala vacía se cierra, no se borra.**

**FC-35 · Expulsar a un jugador** `[MEM]` — actor **el anfitrión** `OB-01`; **no con partida en
curso** `OB-22`; **se le avisa** `OB-42` y **puede volver con el código** `OB-21`. Nunca vacía
la sala. Su efecto real: una vez iniciada la partida **nadie se incorpora** `R 1.1`, así que
expulsar y arrancar deja fuera al expulsado de esa partida.

**FC-16 · Invitar dentro del juego** `[BD-T]` · *Create* — actor **con cuenta** `OB-11`;
siempre **dirigida a una cuenta** `P-22`; una pendiente por sala y destinatario; se conserva
aunque esté desconectado `P-21`; no caduca `P-39`.
**FC-17 · Invitar por correo** `[BD-T]` `[DEP-E9]` — uso único porque **se borra al responderse**
`D-24`; canje resuelto en §3.1.
**FC-18 · Responder una invitación** `[BD-T]` · *Delete* (+ FC-14 si acepta).

### Bloque D · Consultas de resultado

**FC-19 · Historial** `[BD]` — fecha, cuántos jugaron, rivales conservados, puesto y puntos
`P-05`, motivo de fin; la lista de rivales puede estar incompleta `P-08` y `match.player_count`
es el único dato fiable `D-25`.
**FC-20 · Detalle de una partida** `[BD]`. **FC-21 · Ranking global** `[DER]` — vista
`player_ranking`, victorias → puntos → menos partidas `DR-15`.
**FC-22 · Ranking de la sala** `[MEM]` — por puntos, con victorias `P-44`, `P-23`; **incluye
invitados**, por eso ninguna consulta puede producirlo `D-29`.
**FC-23 · Actualizarlo al terminar una partida** `[MEM]` `D-29`.

### Bloque E · La partida

**FC-24 · Iniciar la partida** `[BD]` `[BD-T]` `[MEM]` · *Create* — actor **el anfitrión**
`OB-02`. **Crea** `match` *en curso*, una `match_participant` por jugador **con cuenta** con su
puesto `D-17`, y la primera fila de `match_state` `DR-18b`; asigna puestos `DR-24` y arrastra
los colores `OB-40`; **copia en el documento del estado el pase de cada invitado** §3.4. **De 2
a 4 jugadores** `R 1.1`, `OB-41`; **nadie se incorpora después** `R 1.1`; **comprueba
la sanción** `OB-30`; 3 rondas `DR-01`; turnos y construcciones desde `turn_layout` y
`build_allowance` `R 1.3`.

**FC-25 · Colocar el caballero inicial** `[MEM]` — uno por jugador sobre una torre `DR-05`, en
orden de puesto `DR-24`.
**FC-26 · Colocar el rey** `[MEM]` — ronda 1, el último puesto `R 2.3`; después, **el de menor
puntuación**, al azar entre empatados `R 2.3`. Siempre sobre una torre.

**FC-27 · Jugar un turno** `[MEM]` — 5 PA que **se reinician y no se acumulan** `R 3.1`; máximo
90 s `R 5.1`; como mucho las construcciones de la carta resumen `R 1.3`. **Escribir en el chat
consume ese tiempo** `OB-25`.

| Acción | Coste | Reglas propias |
|---|---|---|
| Colocar un nuevo caballero | 2 PA | Máximo 5 por jugador `R 2.1` |
| Mover un caballero | 1 PA | Nunca a torre ocupada; **siempre dentro del mismo castillo** `DR-22` |
| Subir un nivel | 1 PA por nivel | Un nivel por movimiento salvo cartas 3 y 7; bajar no cuesta; altura máxima 3 `DR-03` |
| Colocar una construcción | 1 PA | Sobre castillo existente, ortogonal; **no se crean ni se unen castillos** `R 2.2`, `DR-08` |
| Obtener una carta | 1 PA | **Al azar** `DR-06`; mismo mazo de 8 |
| Utilizar una carta obtenida | **0 PA** `R 2.4`, `OB-47` | **Carta 3:** sube del nivel 1 al 3 de una torre, **gratis** `R 2.4`. **Carta 7:** recorre el castillo hasta cualquier torre sin importar las casillas, y permite subirse al castillo desde el tablero, **pagando el movimiento y los niveles** `OB-32`, `OB-47` — §0.4. Cartas 1 y 2 en cualquier momento del turno pero **nunca las dos** `DR-26`. Cartas 5 y 8: no parten el castillo, no retiran el nivel 1 de una torre de altura 1 y **no unen castillos** `DR-07` |

**FC-28 · Cerrar el turno** `[BD-T]` — por decisión o **por agotarse los 90 s** `R 5.1`; se
**escribe el estado** `P-27`. Reloj, PA restantes y construcciones pendientes **no se guardan**
`P-28`.

**FC-29 · Cerrar la ronda** `[MEM]` — **puntuación parcial**, designación del colocador del rey
y reparto según las cartas resumen. **No se retira nada del tablero** `DR-23`. Puntuación:
superficie × nivel, **solo con un único caballero propio en el castillo** `R 4.1`; más el rey
—5, 10, 15— para quien comparta castillo **y nivel** `R 4.2`.

**FC-30 · Terminar la partida y registrar el resultado** `[BD]` `[BD-T]` — `match` a
*terminada*; puntuación y puesto en cada `match_participant`; **elimina** `match_state` `D-21`;
**actualiza** el ranking de sala `D-29`. Final = castillos + rey + **1 punto por carta no
usada** `R 4.3`; ganador por puntos → castillos puntuados → puesto `DR-15`; motivos
*completada*, *abandonada*, *interrumpida* `P-37`; una partida **solo de invitados** se elimina
al terminar `PER §7.4`. **Aquí entran en vigor las sanciones diferidas** `OB-38`, §0.5-4.

**FC-31 · Perder la conexión y reconectar** `[MEM]` — en su turno, el tiempo restante; en el de
otro, **90 s** `R 5.2`; **el reloj sigue corriendo** `DR-20`; al volver **retoma su turno en el
estado exacto** `DR-30`. **La pérdida se detecta por el latido** ratificado en `OB-49`, §3.3.

**FC-32 · Retirar al jugador que agotó la ventana** `[BD]` `[MEM]` — marca `was_withdrawn`;
**deja de contar** `DR-25`; **sus construcciones se quedan y sus caballeros se retiran**
`DR-32`; la partida sigue con la configuración inicial `DR-09` y **cuenta para el ranking**
`P-07`.

**FC-33 · Reanudar tras la caída** `[BD]` `[BD-T]` `[MEM]` — con **al menos 2 jugadores**
`P-30`, y **desde `OB-45` los invitados cuentan**, porque vuelven presentando su pase (§3.4).
El turno interrumpido **se rejuega con 90 s completos** `P-28`; los 90 s **no corren con el
servidor caído** `DR-13`; **3 minutos** configurables `D-27`, `P-31`; uno solo → *abandonada*,
nadie → *interrumpida* `P-37`; **las salas no se reanudan** `P-32` y **se pierde el chat**
`OB-24`.

### Bloque F · Chat, reportes y sanciones

**FC-36 · Enviar un mensaje de chat** `[MEM]` · *Create* — actor: cualquiera dentro de la sala,
**con cuenta o invitado**. **Un solo chat, el de la sala**, que la partida muestra `OB-03`. **No
se guarda en la base** `OB-24`; muere con la sala y **una caída se lo lleva** `P-32`.
**Escribir consume el turno** `OB-25`. Límites en §3.2, guardados en la tabla de parámetros
`OB-36`.

**FC-37 · Consultar el chat** `[MEM]` · *Read* — **el que entra tarde ve lo anterior** `OB-27`,
hasta donde llegue la ventana de §3.2.

**FC-38 · Reportar a un jugador** `[BD]` · *Create* — **solo una cuenta reporta** `OB-39` **y
solo contra una cuenta** `OB-28`; motivo, **mensajes ofensivos** `OB-04`; **se borra con la
cuenta** `OB-31`. **La entidad no existe en el modelo** — `HC-16`. Guarda **una copia del
mensaje**, porque `OB-24` no conserva el chat y la ventana de §3.2 descarta lo antiguo, y **la
ocasión** —la partida, o la sala si fue fuera de partida— porque es lo que se acumula
(§0.5-3).

> **Consecuencia de `OB-28` + `OB-39`:** el régimen **solo funciona entre cuentas**. Un invitado
> ni reporta ni puede ser reportado, aunque escriba en el mismo chat.

**FC-39 · Aplicar una sanción** `[BD]` · *Create* + *Update* — la dispara el sistema a las **5
ocasiones** `OB-29`, `OB-34`, `OB-35`. Escalera **5 h → 1 día → 3 días** y, al cuarto cruce,
**permanente** `OB-33`, §0.5-1. **Al aplicarla, el contador de reportes vuelve a cero**
§0.5-2. **Si se cruzó jugando, entra en vigor al terminar la partida**, y **desde ahí** corre su
duración `OB-38`, §0.5-4. Los números viven en tablas de parámetros `OB-36` — §3.5.

**FC-40 · Comprobar la sanción vigente** `[BD]` · *Read* — **dos puntos, ambos decididos**: el
**baneo permanente** al iniciar sesión `OB-37`, y la **sanción temporal** al entrar a la sala y
al jugar `OB-30`.

### Bloque G · Presencia e identidad visible

**FC-41 · Asignar y mostrar el color** `[MEM]` — **se asigna en la sala** `OB-40` y distingue a
dos invitados con el mismo alias `OB-20`. Con cuatro jugadores como máximo, cuatro colores
bastan.

> **Deducción de `OB-40` × `P-34`:** el color de quien sale **vuelve al conjunto disponible**, y
> el que cada jugador tiene al iniciar la partida es el que **arrastra a la partida**, porque el
> color identifica también las piezas.

**FC-42 · Marcar inactivo y traspasar el anfitrión** `[MEM]` — si un jugador **se desconecta sin
salir**, el servidor lo detecta **por el latido** `OB-49` y lo marca **inactivo**; si era el
anfitrión, **pasa la condición al siguiente del orden** `OB-23`, `OB-10`. **El inactivo se queda
en la sala y sigue ocupando su plaza** `OB-46`, `P-34`; lo que no ocupa es **plaza en la
partida**, a la que no entra `OB-41`. Al reconectar dentro del plazo **vuelve a activo**, porque
no dejó la sala. Nadie la pide: la dispara el servidor.

**FC-43 · Retirar de la sala al inactivo que agota el plazo** `[MEM]` (+ `[BD]` si cierra la
sala) — vencido el plazo de `OB-50`, el servidor **lo saca de la sala** y libera su plaza; **si
era el último, la sala se queda vacía y se cierra** `D-23`. **No se aplica a quien está dentro de
una partida en curso**, que se rige por `R 5.2` y `P-35` (§0.6). El plazo vive en la tabla de
parámetros §3.5. Tampoco la pide nadie: la dispara el tiempo.

## 1.4 Situaciones del sistema que no las pide ningún actor

| # | Situación | Fuente |
|---|---|---|
| S-1 | Al arrancar el servidor **se cierran las salas** abiertas | `DR-18a` |
| S-2 | Fin de turno **por agotarse los 90 s** | `R 5.1` — FC-28 |
| S-3 | Retiro por agotarse la ventana de reconexión | `R 5.2` — FC-32 |
| S-4 | Cierre de la partida a la que nadie volvió | `P-31`, `P-37` — FC-33 |
| S-5 | Actualización del ranking de sala | `D-29` — FC-23 |
| S-6 | **Registro de eventos**, solo log4net, sin contraseñas, tokens ni datos personales | `RES-06` |
| S-7 | **Aplicación de la sanción**, **diferida al fin de la partida si se cruza jugando** | `OB-29`, `OB-38` — FC-39 |
| S-8 | **Caducidad de la prohibición**, a las 5 h, 1 día o 3 días **desde su entrada en vigor** | `OB-33`, §0.5-4 |
| S-9 | **Marcado de inactividad y traspaso del anfitrión**, disparado por el latido. El inactivo **se queda en la sala** | `OB-23`, `OB-46`, `OB-49` — FC-42 |
| **S-10** | **Retirada de la sala del inactivo que agota el plazo**, y **cierre de la sala si era el último** | `OB-50`, `D-23` — FC-43 |

## 1.5 Lo que **no** pertenece al sistema

Siguen fuera: **espectadores, notificaciones más allá de las invitaciones, logros, temporadas,
bloqueo de jugadores, historial de invitaciones y rol de administración** `D-31`; la **secuencia
de jugadas** y el desglose por ronda; las **cartas maestras** `DR-27`.

**Salieron de la lista** el chat `OB-03` y la expulsión `OB-01` (`R-1`, `R-2`). El **registro de
auditoría** hay que reescribirlo: `OB-04` obliga a conservar reportes y sanciones.

**Dos ausencias correctas, que no son huecos.** No hay mantenimiento de `turn_layout`,
`build_allowance`, `action_cost` **ni de las tablas de parámetros nuevas**: se cargan por
script, como `seed-configuration.sql`, mientras `D-31` descarte el rol de administración. Y **no
hay caso de uso de moderación**: las sanciones son automáticas por umbral.

## 1.6 Lo que no se puede determinar con la información actual

**No queda ninguno.** De los siete de la v4.0, cuatro los resolvió este análisis por encargo
(§0.5), dos los cerró STK-02 (`OB-47`, `OB-48`) y el último lo cierra `OB-46` (§0.6). **Lo que
queda no es información: es ratificación y trabajo de escritura.**

| # | Sin determinar | Origen |
|---|---|---|
| — | **Ninguno.** El hueco 44 se cierra con `OB-46`: el inactivo se queda en la sala, así que al reconectar vuelve a activo sin nada que recuperar (§0.6) |

**Pendientes de ratificación, no de información.** Estas ya tienen respuesta razonada; solo
falta que STK-02 diga sí: la escalera y el reinicio del contador (§0.5-1 y §0.5-2), la ocasión
fuera de partida (§0.5-3), el instante de inicio de una sanción diferida (§0.5-4), el pase de
invitado y la retirada de `DR-21` (§3.4), la longitud del código de correo (§3.1) y los límites
de chat (§3.2).

**Deducciones que conviene ratificar de una línea:** a una sala privada solo se entra por código
o invitación `OB-08`; salir del juego `OB-15` en mitad de una partida solo puede tratarse como
desconexión, porque `R 5.2` es lo único definido para un jugador que desaparece; el color
liberado vuelve al conjunto `OB-40` × `P-34`.

---
---

# Parte 2 · Revisión de la cobertura de los casos de uso `CU-01`…`CU-19`

**No se describe ningún caso de uso.** Se revisa si el conjunto representa de forma suficiente
el comportamiento de la Parte 1.

## 2.1 Mapa funcionalidad → caso de uso

| Funcionalidad | Caso de uso | Situación |
|---|---|---|
| FC-01 Crear cuenta | CU-02 | Cubierta |
| FC-02 Iniciar sesión | CU-01 | **Incompleta** — `HC-14` |
| FC-03 Recuperar acceso | CU-03 | **Incompleta** — `HC-15` |
| FC-04 Perfil | CU-06 | Cubierta |
| FC-05 Eliminar cuenta | CU-07 | **Incompleta** — `HC-04` |
| FC-06 Invitado | CU-04 | **Incompleta** — `HC-13` |
| FC-07 Idioma | CU-05 | Cubierta y cerrada |
| FC-08 … FC-11 Amistades | CU-08, CU-09 | Cubiertas |
| FC-34 Eliminar un amigo | — | **Sin caso de uso** |
| FC-12 Crear sala | CU-15 | **Incompleta** — `HC-05`, `HC-17` |
| FC-13 Consultar salas | CU-14 | Cubierta; ver `HC-09` |
| FC-14 Entrar a la sala | CU-13 | **Incompleta** — `HC-18` |
| FC-15 Salir de la sala | CU-18 | **Incompleta** — `HC-06` |
| FC-35 Expulsar | — | **Sin caso de uso** |
| FC-16 Invitar en el juego | CU-16 | Cubierta |
| FC-17 Invitar por correo | — | **Sin caso de uso** — `HC-03`; §3.1 |
| FC-18 Responder invitación | CU-17 | Cubierta |
| FC-19 / FC-20 / FC-21 Consultas | CU-10, CU-11, CU-12 | Cubiertas |
| FC-22 Ranking de sala | CU-13 §5 | Cubierta como consulta |
| FC-23 Actualizar ranking de sala | — | **Sin caso de uso** — `HC-07` |
| FC-24 Iniciar la partida | CU-19 | **Incompleta** — `HC-01` |
| FC-25 … FC-33 · La partida entera | — | **Sin caso de uso** (nueve funcionalidades) |
| FC-36 / FC-37 Chat | — | **Sin caso de uso** |
| FC-38 Reportar | — | **Sin caso de uso** |
| FC-39 Aplicar sanción | — | **Sin caso de uso** |
| FC-40 Comprobar la sanción | — | **Sin caso de uso**; es **inclusión** de CU-01, CU-13 y CU-19 |
| FC-41 Color del jugador | CU-13, parcialmente | **Obliga a complementar CU-13** — `HC-18` |
| FC-42 Inactividad y traspaso | — | **Sin caso de uso** |
| FC-43 Retirada de la sala por inactividad | — | **Sin caso de uso** |

**De 43 funcionalidades, 20 siguen sin caso de uso.** v1.0: 11 de 33 · v2.0: 18 de 40 · v3.0,
v4.0 y v5.0: 19 de 42; **v5.3: 20 de 43**, por la retirada de sala de `OB-50`.

*El mapa cuenta funcionalidades y §2.6 cuenta huecos de caso de uso; no hay correspondencia uno
a uno.*

## 2.2 Cobertura por tabla del modelo de datos

| Tabla | C | R | U | D | Hueco |
|---|---|---|---|---|---|
| `account` | CU-02 | CU-01, CU-06, CU-08 | CU-06 | CU-07 | CU-03 debe modificar `password_hash` `OB-17`; **nadie comprueba el baneo permanente** `OB-37` |
| `friendship` | CU-08 | CU-08, CU-09 | CU-09 | CU-09 | **Nadie borra una amistad aceptada** `OB-07` |
| `room` | CU-15 | CU-14 | CU-18 (cierre) | nunca | La sitúan en memoria; el modelo la persiste `HC-05`, `HC-06`. Falta la **columna de tipo** `OB-08`; el `CHECK` del código está mal `HC-17` |
| `room_invitation` | CU-16 | CU-17 | — | CU-17, CU-18, CU-07 | El canal `email` no lo crea nadie `HC-03`; §3.1 propone **retirar `invitation_token`** |
| `match` | CU-19, parcial | CU-10, CU-11 | **nadie** | **nadie** | Nadie la pasa a *terminada* |
| `match_participant` | **nadie** | CU-10…CU-12 | **nadie** | CU-07 (cascada) | Nadie la crea ni escribe resultado |
| `match_state` | **nadie** | **nadie** | **nadie** | **nadie** | **Ninguna operación tiene caso de uso.** Su comentario **ya menciona el pase de asiento**: adoptarlo no cambia el esquema `OB-45` |
| `turn_layout`, `build_allowance`, `action_cost` | — | — | — | — | Correcto: solo lectura `D-30` |
| **Reporte** *(no existe)* | — | — | — | — | `OB-04`, con **copia del mensaje**, **la ocasión** —partida o sala— `OB-34`, `OB-35`, y cascada `OB-31` — `HC-16` |
| **Sanción** *(no existe)* | — | — | — | — | `OB-04`, con nivel, **instante de entrada en vigor**, fin y marca de permanente §0.5 — `HC-16` |
| **Parámetros de sanción, chat y latido** *(no existen)* | — | — | — | — | `OB-36`, `OB-49` — dos tablas, §3.5 |
| **Código de recuperación** *(no existe)* | — | — | — | — | `OB-17` — `HC-15` |
| ~~Mensaje de chat~~ | — | — | — | — | **No hace falta**: `OB-24` |

## 2.3 Casos de uso correctamente contemplados

**Once:** CU-02, CU-05, CU-06, CU-08, CU-09, CU-10, CU-11, CU-12, CU-14, CU-16, CU-17. Sin
cambios en esta ronda: las cinco aclaraciones nuevas afectan a casos de uso que ya estaban en la
lista de incompletos, o a comportamientos que no tienen ninguno.

Aciertos que siguen mereciendo mención: **CU-10 FA02** y **CU-11 FA02** con los huecos de `P-08`
y `P-02`; **CU-12 paso 4**, alineado con la vista; **CU-07 FA03**, que aplica `P-42` en la
operación y no en el motor.

## 2.4 Casos de uso incompletos

**Ocho**, que con los once correctos son los diecinueve.

**`HC-01` · CU-19 Iniciar la partida.** Faltan: crear `match`, **una participación por jugador
con cuenta** `D-17`, **el primer estado** `DR-18b`, los **puestos de mesa** `DR-24`, el arrastre
de **colores** `OB-40` y **la copia del pase de cada invitado en el documento del estado**
`OB-45`, §3.4. Actor: **el anfitrión** `OB-02`. **Comprueba la sanción** `OB-30` y **cuenta solo
jugadores **activos**: los inactivos se quedan en la sala y no entran** `R 1.1`, `OB-41`,
`OB-46`, §0.6.

**`HC-02` · CU-19 no cubre lo que sigue:** caballero inicial `DR-05` y colocación del rey
`R 2.3`.

**`HC-03` · CU-16 cubre un solo canal.** El canje por correo sigue sin caso de uso; §3.1 lo
desbloquea.

**`HC-04` · CU-07 omite tres efectos:** el **archivo del avatar**, la **partida sin
participaciones** `PER §7.5` y **reportes y sanciones** `OB-31`.

**`HC-05` · CU-15 contradice el modelo:** sitúa la sala «únicamente» en memoria cuando
`schema.sql` la persiste y `D-23` la **cierra, no la borra**. Le falta dónde vive el tipo y el
**código de cuatro con reintento**.

**`HC-06` · CU-18 arrastra el mismo error** y le falta el criterio de sucesión `OB-10`.

**`HC-13` · CU-04 desfasado en tres puntos:** disparador en la ventana de inicio de sesión
`OB-16` —corrigiendo `PT-03`—, **identificador temporal** `OB-05` —que ahora es además **el
pase** `OB-45`— y **marca de invitado más color** `OB-20`, `OB-40`.

**`HC-14` · CU-01 no comprueba el baneo permanente** `OB-37`.

**`HC-15` · CU-03 se queda corto ante `OB-17`:** tres pasos y **un dato temporal que no existe
en el modelo**.

**`HC-16` · Reportes y sanciones no tienen ni caso de uso ni modelo, pero ya tienen todos sus
números.** Con §0.5, lo que queda es escribirlo: no falta ninguna decisión.

**`HC-17` · `schema.sql` no cumple `OB-18`:** `room.code` a `varchar(4)` con
`CHECK '^[A-Z0-9]{4}$'` —o el alfabeto reducido de §3.1—, **más el reintento ante colisión**.

**`HC-18` · CU-13 se queda corto en tres frentes, los tres decididos:** **comprobar la sanción
temporal** `OB-30`, **mostrar el chat** `OB-03` y **mostrar marca de invitado y color** `OB-20`,
`OB-40`. **Su PRE-4 se queda tal cual**: «menos de cuatro jugadores», **contando a los
inactivos**, que siguen en la sala y siguen ocupando plaza `OB-46`, §0.6. Era el único caso de
uso bloqueado por un hueco, y deja de estarlo.

**`HC-07` · Nadie alimenta el ranking de sala** `D-29`.

## 2.5 Casos de uso que deberían dividirse o complementarse

**Dividir — `CU-19`.** Único con fundamento: mezcla **crear la partida** (FC-24) con **preparar
el tablero** (FC-25, FC-26). `OB-02` lo refuerza: el arranque lo hace el anfitrión; la
colocación inicial, todos por turno.

**Complementar sin dividir — `CU-09` y `CU-17`:** cada rama debe decir su operación.
**`CU-06`:** tolerancia al avatar ausente `P-43`. **`CU-13`:** las tres cosas de `HC-18` más «no
se entra a una sala con partida en curso» `R 1.1`, `P-35`. **`CU-03`:** flujo entero por
`OB-17`. **`CU-01`:** baneo permanente `OB-37`.

**Complementar `CU-18` y factorizar el traspaso del anfitrión.** El mismo efecto tiene **dos
disparos** con **la misma regla** `OB-10`: salir de la sala (CU-18 FA02) y quedar inactivo (S-9,
`OB-23`). Es el fragmento común más claro. El de expulsar/salir es más discutible —difieren en
actor, disparo, cierre y herencia—, aunque con `OB-42` comparten también **el aviso**.

## 2.6 Casos de uso que faltan

**De la partida — sin cambios en cinco versiones**

| # | Falta | Justificación |
|---|---|---|
| 1 | **Preparar la partida**: caballero inicial y rey | `DR-05`, `DR-24`, `R 2.3` |
| 2 | **Jugar un turno** | `R 3.1`, `R 3.2`, `R 5.1` |
| 3 | **Obtener** y **utilizar** una carta | `R 2.4`, `DR-06`, `DR-26`, `OB-32`, `OB-47`, `OB-48` |
| 4 | **Cerrar el turno y guardar el estado** | `R 5.1`, `P-27`, `P-28` |
| 5 | **Cerrar la ronda** | `DR-02`, `DR-23`, `R 4.1`, `R 4.2` |
| 6 | **Terminar la partida y registrar el resultado** | `DR-15`, `R 4.3`, `D-21`, `P-37`, `OB-38` |
| 7 | **Reconectar** | `R 5.2`, `DR-20`, `DR-30`, `OB-49` |
| 8 | **Retirar al jugador que agotó la ventana** | `R 5.2`, `DR-25`, `DR-32` |
| 9 | **Reanudar tras la caída**, **con los invitados presentando su pase** | `PER §6`, `P-30`, `P-37`, `OB-45` |
| 10 | **Actualizar el ranking de la sala** | `D-29` |

**Traídos por las aclaraciones**

| # | Falta | Justificación |
|---|---|---|
| 11 | **Expulsar a un jugador** | `OB-01`, `OB-21`, `OB-22`, `OB-42` |
| 12 | **Eliminar un amigo** | `OB-07`, `OB-43` |
| 13 | **Enviar y leer mensajes de chat** | `OB-03`, `OB-25`, `OB-27` |
| 14 | **Reportar a un jugador** | `OB-04`, `OB-28`, `OB-39` |
| 15 | **Aplicar la sanción** y **el baneo permanente** | `OB-29`, `OB-33`, `OB-37`, `OB-38`, §0.5 |
| 16 | **Canjear una invitación recibida por correo** | Requisito 6 de `[N2]`, §3.1 |
| 17 | **Marcar inactivo y traspasar el anfitrión** — *situación sin actor* | `OB-23`, `OB-41`, `OB-46` |
| 18 | **Retirar de la sala al inactivo que agota el plazo**, cerrándola si era el último — *situación sin actor* | `OB-50`, `D-23` |

Como **decisión del equipo**, si `S-1`, `S-7`, `S-8`, `S-9` y `S-10` se modelan como casos de
uso o quedan como comportamiento del sistema.

**Advertencia de granularidad.** Dieciocho comportamientos **no son dieciocho casos de uso**.
El corte lo decide el equipo; lo que este análisis sostiene es que **ninguno está hoy en ninguna
parte**.

## 2.7 Relaciones `include` y `extend`

**Lo que ya está y se sostiene:** `CU-13` incluido por `CU-14`, `CU-15` y `CU-17`.

**Lo que conviene revisar:** `CU-13` es fragmento incluido **y** base de extensiones que no
paran de crecer —`CU-16`, `CU-18`, `CU-19`, expulsar, chat—; las heredan los tres que lo
incluyen. Y siguen sin declararse `CU-01` ↔ `CU-03` y **`CU-04` extiende a `CU-01`** `OB-16`.

**De la partida, si se modela**

| Relación | Tipo | Por qué |
|---|---|---|
| Cerrar el turno → Guardar el estado | `include` | En **cada** cierre `P-27` |
| Terminar la partida → Calcular la puntuación | `include` | Siempre `DR-02` |
| Jugar un turno → cada acción con coste en PA | `extend` | Todas opcionales `R 3.2` |
| Jugar un turno → Obtener / Utilizar una carta | `extend` | Opcionales `R 2.4` |
| Reconectar → Retirar al jugador | `extend` | Solo si vence la ventana `R 5.2` |
| **Reanudar → Recuperar el puesto con el pase** | `extend` | Solo invitados. **Ya no está en el aire:** `OB-45` lo adopta, §3.4 |
| Iniciar la partida → Preparar la partida | `include` si se separa | Ocurre siempre `DR-05` |

**Traídas por las aclaraciones**

| Relación | Tipo | Por qué |
|---|---|---|
| Estar en la sala → **Expulsar** | `extend` | Opcional, solo el anfitrión, **solo sin partida en curso** `OB-22` |
| Estar en la sala / Jugar → **Enviar un mensaje** | `extend` | Nadie está obligado a escribir |
| Entrar a la sala → **Mostrar chat y color** | **Ninguna: es parte de CU-13** | Apartados de la ventana, como el ranking |
| Chat → **Reportar** | `extend` | Opcional, **solo entre cuentas** `OB-28`, `OB-39` |
| Reportar → **Aplicar la sanción** | `extend` | Solo a la quinta ocasión `OB-29`, `OB-34` |
| Aplicar la sanción → **Banear permanentemente** | `extend` | Solo al cuarto cruce `OB-33`, §0.5-1 |
| Terminar la partida → **Aplicar la sanción diferida** | `extend` | Solo si se cruzó el umbral durante esa partida `OB-38` |
| Iniciar sesión → **Comprobar el baneo permanente** | `include` | Siempre `OB-37` |
| Entrar a la sala → **Comprobar la sanción temporal** | `include` | Siempre `OB-30` |
| Iniciar la partida → **Comprobar la sanción temporal** | `include` | `OB-30`, «y jugar» |
| **Salir de la sala → Traspasar el anfitrión** e **Inactividad → Traspasar el anfitrión** | `include` de un fragmento común | Mismo efecto, **misma regla** `OB-10`, dos disparos `OB-23`. El candidato más claro |
| **Expulsar** ↔ **Salir de la sala** | `include` de un fragmento común, si el equipo lo decide | Comparten retirar de la lista **y avisar** `OB-42`; difieren en actor, disparo, cierre y herencia. **La inactividad no entra aquí**: el inactivo no deja la sala `OB-46` |
| Consultar amigos → **Eliminar un amigo** | `extend` | Opcional. **No existe** un caso de uso «consultar amigos» |
| Recuperar el acceso → el código `OB-17` | Ninguna | Son **pasos** del mismo caso de uso |

**Lo que sigue sin deber declararse:** `CU-07` no incluye el borrado del archivo del avatar;
FC-06 no incluye la generación del identificador temporal; FC-14 no incluye la asignación del
color. Son pasos de una operación, no objetivos de un actor.

## 2.8 Casos de uso mal planteados o apoyados en algo no vigente

**`HC-05` y `HC-06`** siguen siendo los dos errores de fondo: contradicen `schema.sql`.
**`HC-08`** · el significado del tipo de sala está cerrado `OB-08`; falta **dónde se guarda**
(§3.2). **`HC-09`** · CU-14 contradice `B §4.1` y `PER §12`, que siguen listando «salas públicas
listadas» como fuera de alcance; `OB-03` obliga a la misma corrección para el chat: **una sola
pasada**. **`HC-10`** · CU-05 no aporta nada al modelo y ya se sabe que nunca lo hará `OB-06`.

**Ninguno de los diecinueve casos de uso es ajeno al sistema.** No hay que retirar ninguno.

## 2.9 Veredicto

**La identificación ya está sustancialmente completa; lo que falta es escribirla.** Es el primer
veredicto de las cinco versiones que puede decir eso.

- **No queda ningún hueco de información.** Con `OB-46` se cierra el último. Todo lo que falta
  está decidido, o resuelto por encargo a falta de una ratificación.
- **El régimen de sanciones está cerrado de punta a punta** —umbral, ocasión, escalera,
  reinicio del contador, instante de inicio, puntos de comprobación y dónde viven los números—.
  Ya no falta ninguna decisión: falta la tabla y el caso de uso.
- **Las reglas del juego no tienen ninguna pregunta abierta.** Con `OB-47` y `OB-48`, la carta 7
  y la carta 3 quedan definidas y separadas; era lo último. A cambio, `R 2.4` **necesita dos
  correcciones** y `PER §4.1` una.
- **La partida sigue sin cubrirse en absoluto.** Nueve funcionalidades, cero casos de uso, sin
  cambios desde la v1.0. **Es lo único que no se ha movido en cuatro rondas de aclaraciones**, y
  ahora es lo único grande que queda.
- **Once casos de uso correctos y ocho incompletos.** De los ocho, **siete saben exactamente qué
  les falta**; el octavo, CU-19, es el que hay que dividir.
- **La prueba objetiva sigue siendo el modelo:** `match_state` sin ninguna operación,
  `match_participant` sin quien la cree, nadie pasa `match` a *terminada*, y **cuatro datos sin
  tabla** —reporte, sanción, parámetros y código de recuperación—.
- **Cuatro correcciones son contra entregables ya hechos:** `HC-05`, `HC-06`, `HC-17` y `R 2.4`.

**Orden sugerido, sin describir todavía nada:**

1. **Ratificar de una vez** lo resuelto por encargo: §0.5 (los cuatro puntos del régimen de
   baneos), §3.1 (el código del correo), §3.2 (los límites de chat) y §3.4 (el pase).
2. **Retirar `DR-21`** y escribir el `DR-` nuevo que lo sustituye. Fijar de paso **el plazo de
   `OB-50`** — se propone 3 minutos (§0.6).
3. Corregir `HC-05`, `HC-06`, `HC-17`, `R 2.4` y `PER §4.1`.
4. **Fijar el corte de los casos de uso de la partida**: es lo único grande que queda.
5. Escribir las dos tablas de parámetros de §3.5 y las dos entidades de `HC-16`.
6. Actualizar `B §4.1`, `PER §12` y `D-30` en una sola pasada.

---
---

# Parte 3 · Las cuestiones que STK-02 encargó analizar

## 3.1 `OB-12` y `OB-44` · El canje por correo, y la longitud de su código `[PROPUESTA]`

`RES-03` prohíbe el stack web; `RES-02` fija WCF duplex sobre net.tcp; un cliente MonoGame no
recibe clics de navegador; un esquema `torres://` exigiría registro distinto en macOS, Linux y
Windows, contra `ORG-03` y `ORG-04`. **Queda un código que el jugador escribe en la
aplicación**, y `OB-44` deja su longitud aquí.

**Propuesta: que el correo lleve el propio código de sala de cuatro caracteres, y que
`invitation_token` desaparezca.** Cuatro razones apoyadas en decisiones ya tomadas:

1. **La seguridad no descansa en el secreto del código.** Por `P-22` la invitación va **siempre
   dirigida a una cuenta**: el canje comprueba que **quien lo presenta es la destinataria**. Un
   token largo sería un segundo cerrojo en una puerta que ya se abre con la identidad.
2. **El código de sala ya circula:** cualquiera con él entra por CU-14, y a una sala privada se
   entra precisamente por código `OB-08`. Un token distinto no protegería la sala.
3. **Cuatro caracteres se teclean desde un correo; sesenta y cuatro, no.** `OB-18` eligió cuatro
   porque se escribe a mano, y el del correo se escribe igual.
4. **Simplifica el esquema:** desaparecen la columna, su `UNIQUE` global y el `CHECK` que la
   exige para `email`. Los dos canales pasan a diferenciarse **solo en `channel`**, que es lo que
   son: el mismo hecho avisado por dos vías.

**Sobre el alfabeto:** como el código se copia de un correo, conviene **excluir los caracteres
que se confunden** —`0`/`O`, `1`/`I`/`L`—. Quedan 31 símbolos y 31⁴ ≈ 923 000 combinaciones, de
sobra para una unicidad que **solo aplica entre salas abiertas**. Es una línea del `CHECK` de
`HC-17`.

**Lo honesto:** con esto **el canal de correo no aporta capacidad**, porque la invitación ya
está en la bandeja del destinatario `P-22`, CU-17. Aporta **un aviso**. Es bueno para `Q-34` —si
el correo falla no se pierde nada— y obliga a decidir si la dependencia `DEP-E9` compensa.

## 3.2 `OB-09`, `OB-13` y `OB-26` · Recursos en memoria y límites del chat

**`D-26` y `D-29` no se tomaron por rendimiento sino por corrección.** Una tabla de miembros
solo tendría a los jugadores con cuenta `P-08` y **nunca sabría si la sala está vacía**, que es
la condición que decide si la sala existe. El ranking de sala **incluye invitados** `P-40`.
Llevarlos a la base **los haría incorrectos**, no más baratos.

**Orden de magnitud.** Una sala son cuatro jugadores `P-34`: cuatro referencias ordenadas, un
anfitrión, cuatro acumuladores, un código de cuatro, un tipo y cuatro colores. Despreciable
frente a lo que **tiene que estar en memoria igualmente**: el estado vivo de la partida, porque
`E 2.1` y `RES-05` obligan al servidor a decidir cada acción con 90 s de turno `R 5.1`. **El
gasto lo domina la partida, no la sala.** Donde sí hay riesgo es en `DR-10` —sin límite de
partidas simultáneas—, ya anotado como **`TEN-G`**.

**El chat es lo único sin cota `[PROPUESTA]`**

| Límite | Valor | Razonamiento |
|---|---|---|
| Longitud de un mensaje | **200 caracteres** | **Escribir consume el turno** `OB-25` y un turno son 90 s: el límite acompaña a una regla existente |
| Mensajes conservados por sala | **200, en ventana deslizante** | La partida más larga son **60 min** con 4 jugadores `B §5`; doscientos cubren una entera, que es lo que `OB-27` necesita |
| Cota resultante | **~100 KB por sala** | Ruido frente al documento del tablero |

Con ventana deslizante **un mensaje puede desaparecer antes de que su reporte sirva de algo**;
por eso FC-38 guarda **una copia del texto**.

**Recomendación:** mantener `D-26` y `D-29`; acotar el chat con esas cifras, en la tabla de
parámetros; llevar `TEN-G` a la mesa si preocupa el consumo; y **bajar el tipo de sala a la
base**, porque `room` ya es fila permanente `D-23` y el tipo **se fija al crear y no cambia**.
Cierra `HC-08`.

## 3.3 `Q-32` y `Q-33` — **cerradas por `OB-49`**

**`Q-33`.** El reloj del turno **es del servidor** y **la latencia va dentro de los 90 s**. Se
sigue de `DR-20` —el reloj corre con el jugador desconectado, y un reloj así solo puede estar en
el servidor—, lo confirma `E 2.1` y lo vuelve a confirmar `OB-25`: si escribir en el chat
consume turno, el tiempo lo lleva quien ve el mensaje y el reloj a la vez. Si se quiere acotar la
injusticia, queda fijar una **latencia máxima tolerada** como umbral `DR-29`.

**`Q-32`.** Se adopta el **latido con periodo declarado**: el cliente envía uno cada `P`; el
servidor da la conexión por perdida tras `N` sin recibir; **la ventana de `R 5.2`, la medida del
escenario D-1 y el marcado de inactividad de `OB-23` se cuentan desde ese instante**. `P` y `N`
van a la tabla de parámetros (§3.5).

**Por qué el latido y no el canal fallido:** la medida de D-1 empieza «cuando el servidor detecta
la pérdida», así que el instante tiene que ser **calculable**; detectar por canal fallido lo
ataría a tiempos de transporte no documentados y posiblemente distintos entre macOS, Linux y
Windows `ORG-03`, rompiendo la comparabilidad entre las máquinas de las dos personas `ORG-04`; y
`R 5.2` cuenta 90 s **desde la pérdida**, que quedarían anclados a un instante indeterminado,
con `DR-20` amplificándolo.

**Salvedad que la ratificación no elimina:** `STACK.md` se cita en la base pero **no está en el
directorio**, así que no he podido comprobar si el binding elegido ya ofrece un equivalente con
tiempos declarados. **Si lo ofrece y son iguales en las tres plataformas, sirve igual y ahorra
código.** Conviene mirarlo antes de implementar, no antes de decidir.

## 3.4 `OB-45` · El pase de invitado se queda: cómo se resuelve

**La resolución, en una frase:** el **identificador temporal** que el servidor da al invitado al
entrar `OB-05` **es también su pase**, y lo único que hay que añadir es **copiarlo dentro del
documento de `match_state` al iniciar la partida**.

**Por qué eso basta, y por qué es imprescindible.** El identificador nace en la memoria del
servidor, y **la memoria es justo lo que se pierde en la caída** para la que existe el pase. La
escritura del estado ya ocurre al empezar la partida `DR-18b`, así que la copia no añade ninguna
operación nueva. Y **el esquema ya lo contempla**: el comentario de `match_state.state_document`
en `schema.sql` dice que el documento guarda «los puestos de los invitados que juegan, con su
alias y su pase de asiento». **Adoptar `OB-45` no cambia el esquema: lo pone en uso.**

**Dónde lo guarda el cliente: en memoria, y no hace falta más.** El pase existe para sobrevivir
a **la caída del servidor**, y una caída del servidor **no cierra el cliente**. Escribirlo en un
archivo local le daría al invitado una identidad persistente en disco, que es exactamente lo que
`CON-07` niega —el invitado es una identidad cerrada que no deja rastro— y lo que `P-08`
sostiene. **Se asume el coste que `PER §6.5` ya registraba:** si el jugador cierra la
aplicación, pierde el pase y con él su puesto.

**Debe ser aleatorio e impredecible**, porque quien lo presenta ocupa el puesto; `PER §6.5` ya lo
exige, y esa propiedad **no se relaja** por reutilizarlo como identidad de sesión. Con
`SecurityMode.None` `RES-09` viaja sin cifrar, lo que lo suma a `TEN-I` (§0.7).

**Qué se desbloquea.** El problema que `P-38` planteaba —una partida de un jugador con cuenta y
tres invitados **nunca podría reanudarse**, porque hacen falta 2 y los invitados no podían
volver— **queda resuelto**: los invitados cuentan para los 2 de `P-30`.

**Qué hay que retirar, y con qué cuidado.** `DR-21` —«los invitados no vuelven tras una caída»—
**queda sin efecto**. Por la regla de gobernanza del proyecto, **un identificador que muere no
se reutiliza**: `DR-21` se marca retirado y se escribe un `DR-` **nuevo** que diga que el
invitado recupera su puesto presentando su pase. También hay que reescribir `PER §6.2` y
**descartar la alternativa de `P-38`** de reclamar el puesto solo por el alias, que dejaba el
sitio a quien conociera el nombre.

**El único límite del pase, dicho claro:** no protege de un cliente cerrado, no protege de un
invitado que pierda la máquina, y no es una identidad —no sirve para historial, ranking ni nada
permanente—. Es exactamente lo que `PER §6.5` describía, ni más ni menos.

## 3.5 `OB-36` y `OB-49` · Dónde viven los umbrales, los límites y el latido

`OB-36` decide que vayan a **tablas de parámetros, como las que ya existen**, lo que **amplía
`D-30`**. El proyecto **ya tiene la forma**: `action_cost` es exactamente una tabla
`código → número` con su `CHECK`. Dos tablas con ese patrón bastan y no introducen ningún estilo
ajeno al esquema.

| Tabla propuesta | Forma | Contenido |
|---|---|---|
| **`sanction_level`** | `(level, ban_duration)`, clave `level`, `CHECK level BETWEEN 1 AND 3` | 1 → **5 horas** · 2 → **1 día** · 3 → **3 días** `OB-33`. El cuarto cruce no es una fila: es el **baneo permanente** §0.5-1 |
| **`system_parameter`** | `(parameter_code, numeric_value)`, misma forma que `action_cost` | Umbral de ocasiones = **5** `OB-29`; longitud de mensaje = **200**; mensajes por sala = **200** §3.2; **periodo `P` del latido** y **número `N` de latidos perdidos** `OB-49`, §3.3; **plazo de inactividad en sala** = **3 minutos** `OB-50`, §0.6 |

**Tres precisiones para no romper decisiones vigentes**

- **Son de solo lectura, como las otras tres.** `D-31` sigue descartando el rol de
  administración: **no hay caso de uso de mantenimiento**; se cargan por script, como
  `seed-configuration.sql`.
- **`D-27` no cambia.** El plazo de reanudación de 3 minutos está declarado expresamente como
  **configuración de aplicación y no dato de la base**. `OB-36` habla de sanciones, chat y
  latido, no de aquel; moverlo sería reabrir `D-27` sin que nadie lo pida.
- **La duración se guarda como duración.** PostgreSQL tiene `interval`, y «5 horas», «1 día» y
  «3 días» son duraciones; guardarlas como enteros obligaría a fijar la unidad en otro sitio.

**Lo que estas dos tablas cierran:** el último obstáculo para escribir `HC-16`. Con §0.5 los
números están decididos y con §3.5 tienen dónde vivir; **la entidad de sanción solo necesita
además el instante de entrada en vigor**, porque §0.5-4 lo separa del instante del umbral.
