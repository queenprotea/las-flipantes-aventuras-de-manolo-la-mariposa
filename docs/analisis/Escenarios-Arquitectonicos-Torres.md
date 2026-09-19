# Escenarios arquitectónicos — Juego de Torres

**Versión 1.0 · 4 sep 2026 · cinco escenarios.**

Se construyen sobre `Base-Oficial-STK-CON-DEP-Torres.md` v3.0. Todo lo que aparece aquí tiene respaldo en esa base; nada se inventa.

## Formato

Seis partes: **Fuente · Estímulo · Artefacto · Ambiente · Respuesta · Medida de la respuesta**. Cada escenario lleva además su **procedencia** (`CON-` de origen y stakeholder) y pasa las cuatro preguntas de control.

**Todavía no llevan el par (Importancia, Dificultad).** Ese par pertenece al árbol de utilidad, que va después.

**Cada escenario se somete a refutación**: buscar una situación en la que se cumpla al pie de la letra y el sistema siga siendo inservible. Lo que la refutación encuentra o bien corrige el escenario, o bien queda declarado como coste asumido.

---

# D-1 · Disponibilidad — el jugador pierde la conexión en su propio turno

**Procedencia:** CON-01, variante V1 · **Stakeholder:** STK-01 Jugador · **Atributo:** Disponibilidad (ISO/IEC 25010, `DR-16`)

| Parte | Contenido |
|---|---|
| **Fuente** | El jugador activo (STK-01), a través de su conexión de red `DEP-E3` |
| **Estímulo** | Pierde la conexión con el servidor mientras es su turno y le quedan **T** segundos de sus 90 |
| **Artefacto** | El estado de la partida en curso, custodiado por `Game.Services`, y el reloj del turno `DEP-E6` |
| **Ambiente** | Partida en curso, **servidor en operación normal**, turno propio del jugador, mesa de 2 a 4 jugadores |
| **Respuesta** | El servidor conserva el puesto del jugador y el estado de su turno. **El reloj del turno sigue corriendo** `DR-20`. Los demás jugadores siguen viendo la partida y el reloj avanzar, sin interrupción. Si el jugador vuelve dentro de esos T segundos, **retoma su turno en el estado exacto en que lo dejó** —PA no gastados y acciones ya confirmadas—, porque el servidor es la única fuente de autoridad del estado `DR-30`, `E 2.1`. Si no vuelve, al agotarse T **es retirado de la partida** `R 5.2`, sus construcciones permanecen en el tablero y su puntuación deja de contar `DR-25`. Si el retiro deja menos de 2 jugadores, la partida termina y gana el que queda. El desenlace queda registrado con la operación y el identificador interno de la entidad afectada `E 9.3` |
| **Medida** | **1.** La ventana para volver es exactamente **T**, contada desde el instante en que el servidor detecta la pérdida de conexión, donde T es el tiempo restante del turno. **2.** El turno **no se prolonga**: la espera de los otros jugadores no aumenta ni un segundo por causa de la desconexión, y ningún turno pasa de 90 s `R 5.1`. **3.** Al vencer T, el retiro es efectivo antes de que empiece el turno siguiente. **4.** Tanto la reconexión como el retiro dejan una entrada en el registro de eventos |

### Las cuatro preguntas de control

| Pregunta | Respuesta |
|---|---|
| ¿Qué se mide y desde qué instante? | La ventana de reconexión, desde que el **servidor** detecta la pérdida de conexión — no desde que el cliente la nota |
| ¿En qué ventana y con qué frecuencia? | En cada desconexión de un jugador activo. La ventana es variable: entre 0 y 90 s |
| ¿Quién lo nota si no se cumple? | Los otros 1 a 3 jugadores de la mesa, que esperarían más de 90 s; y el jugador desconectado, que perdería el puesto antes de tiempo |
| ¿De qué `CON-` salió y de quién era el interés? | CON-01, interés de STK-01 en sus dos papeles opuestos: el que se cae y los que esperan |

**¿Podría alguien demostrar mañana que ayer no se cumplió?** Sí. `E 9.3` obliga a registrar la operación y el identificador de la entidad afectada, y `DR-25` hace que el retiro sea un hecho observable en el estado de la partida.

### Refutación

**R1 · Se cumple y el jugador pierde igualmente.** Si T vale 3 s, el jugador reconecta en 2 s y le queda 1 s para jugar: el escenario se cumple al pie de la letra y su turno está perdido de hecho.
→ **No es un defecto: es el coste declarado de `DR-20`.** Pierde el turno, no la partida — la siguiente ventana vuelve a ser de 90 s y con 4 jugadores le quedan hasta 10 turnos propios `DR-01`. Queda constancia de que se aceptó a sabiendas.

**R2 · Se cumple y la mesa se queda congelada.** El escenario original del equipo solo hablaba del jugador que se cae. Si mientras tanto los demás ven la interfaz detenida, todo se cumple y la partida es injugable.
→ **La refutación corrigió el escenario:** por eso la respuesta dice explícitamente que *los demás siguen viendo la partida y el reloj avanzar*. Sin esa frase, el escenario era refutable.

### Nota de trazabilidad

Sustituye al escenario de Disponibilidad redactado por el equipo, corrigiendo **DEF-3** (prometía reconexión «mientras la partida continúe activa», sin plazo) y **DEF-6** (su ambiente excluía por construcción la caída del servidor, que ahora es un escenario aparte, D-2).

**Sin residuales.** Lo que en la v0.1 era una deducción —que el jugador retome su turno en el estado exacto— **quedó ratificado por STK-02 como `DR-30`**, con el argumento del servidor como única fuente de autoridad `E 2.1`.

---

# M-1 · Modificabilidad — cambia el coste en PA de una acción

**Procedencia:** CON-02 · **Stakeholder:** STK-02, sombrero autor de las reglas · **Atributo:** Modificabilidad (ISO/IEC 25010, `DR-16`)

| Parte | Contenido |
|---|---|
| **Fuente** | STK-02 en su sombrero de **autor y revisor de las reglas** |
| **Estímulo** | Publica una versión del documento de reglas en la que **colocar un caballero pasa de costar 2 PA a costar 3 PA** `R 3.2` |
| **Artefacto** | La tabla `action_cost` de la base de datos, y **`MatchSetupService`** en `Game.Services`, que la lee y entrega los valores al dominio como argumentos `D-30`, `DR-11` |
| **Ambiente** | Sistema desplegado y en operación, con partidas en curso, **fuera de cualquier ventana de despliegue** |
| **Respuesta** | El cambio se aplica **editando una fila** de `action_cost`. No se modifica ningún archivo de `Game.Domain`, no se recompila y no se redespliega la aplicación. Las partidas **ya en curso** conservan el coste con el que empezaron; la **siguiente partida que se inicie** toma el valor nuevo, porque la configuración se lee al configurar la partida `DR-11` |
| **Medida** | **1.** Artefactos tocados: **exactamente uno**, una fila de una tabla. **Cero** archivos de código. **2.** El cambio **no requiere recompilar `Game.Domain` ni redesplegar** ningún componente. **3.** Queda efectivo en la primera partida que se inicie tras el cambio; ninguna partida en curso altera su comportamiento a mitad. **4.** El cambio lo puede realizar STK-02 en su sombrero de operador, sin intervención del sombrero de desarrollador |

### Las cuatro preguntas de control

| Pregunta | Respuesta |
|---|---|
| ¿Qué se mide y desde qué instante? | El **coste del cambio** —artefactos tocados y necesidad de recompilar—, desde que se decide el nuevo valor hasta que una partida lo usa |
| ¿En qué ventana y con qué frecuencia? | En cada versión del documento de reglas que toque `R 1.3` o `R 3.2`. El documento va por la v3.0 y sigue en borrador `DEP-E4`: ya ocurrió al menos dos veces |
| ¿Quién lo nota si no se cumple? | STK-02: si hay que tocar el dominio y recompilar, cada ajuste de balance cuesta un ciclo de desarrollo completo |
| ¿De qué `CON-` salió y de quién era el interés? | CON-02, interés de STK-02 como autor de reglas y como mantenedor |

**¿Podría alguien demostrar mañana que ayer no se cumplió?** Sí, y de forma trivial: si el cambio exigió un *commit* en `Game.Domain` o un redespliegue, no se cumplió.

### Refutación

**R1 · Se cumple y el juego queda roto.** Se edita la fila y se pone `place_knight = 0`. El escenario se cumple perfectamente —un artefacto tocado, sin recompilar— y la partida queda desequilibrada.
→ **No es un defecto del escenario: es su alcance.** Mide *coste de cambio*, no *validez del valor*. Hoy `schema.sql` solo garantiza `action_point_cost >= 0`. Si STK-02 quiere rangos válidos, es una restricción a añadir, y **no pertenece a este escenario**.

**R2 · Se cumple y el cambio caro sigue siendo caro.** El escenario cubre los costes de `R 3.2` y las tablas de `R 1.3`. Si la nueva versión de las reglas cambia lo que el rey otorga por ronda `R 4.2` o la fórmula de puntuación `R 4.1`, hay que tocar `Game.Domain` igualmente: el escenario se cumple y la modificabilidad que importaba no está cubierta.
→ **Es cierto, y está declarado.** La delimitación de CON-02 lo dice: configurable son solo esas tres tablas. La **variante cara —cambio del documento de reglas que toca el dominio— sigue sin escenario**. Es una decisión consciente, no un olvido; si se quiere cubrir, hace falta un segundo escenario de CON-02 y una decisión de diseño que hoy no existe.

### Nota de trazabilidad

Sustituye al escenario de Modificabilidad redactado por el equipo, corrigiendo los cinco defectos registrados: **DEF-1** (ya no habla de configurar la distribución de castillos, que `R 1.2` declara fija), **DEF-2** (no se apoya en cartas maestras, que siguen fuera de alcance `DR-27`), **DEF-4** (el artefacto existe y tiene capa: `MatchSetupService` en `Game.Services`) y **DEF-5** (la medida es coste de cambio, no comportamiento de arranque).

---

# F-1 · Corrección funcional — el cierre de puntuación de una ronda

**Procedencia:** CON-03 · **Stakeholder:** STK-01 Jugador, STK-02 Equipo · **Atributo:** Corrección funcional (ISO/IEC 25010, `DR-16`)

| Parte | Contenido |
|---|---|
| **Fuente** | El jugador que ocupa el **último turno de la ronda** (STK-01) |
| **Estímulo** | Confirma su última acción, con lo que la ronda termina y se dispara el corte de puntuación `DR-02` |
| **Artefacto** | `Game.Domain`: el cálculo de puntuación de castillos `DEP-I2`, `DEP-I3`, el efecto del rey `DEP-I8` y la designación del colocador del rey `DEP-I6` |
| **Ambiente** | Partida en curso, servidor en operación normal, **fin de la ronda 1 o 2**, mesa de 2 a 4 jugadores **con al menos un jugador retirado** |
| **Respuesta** | El servidor —único que determina el resultado `E 2.1`— calcula la puntuación parcial de cada jugador **activo**: por cada castillo en el que tenga **exactamente un** caballero propio, superficie del castillo × nivel de la torre de ese caballero `R 4.1`; más los puntos del rey si tiene un caballero en el mismo castillo y nivel, según la ronda `R 4.2`. **Nada se retira del tablero** `DR-23`. Los caballeros del retirado **ya no están** y no cuentan para la exclusividad `DR-32`; **sus construcciones sí siguen formando parte de los castillos** `DR-25`. Designa colocador del rey de la ronda siguiente al **jugador activo** con menor puntuación parcial; si hay empate, sorteo `R 2.3`. El cierre queda registrado con la operación y el identificador de la partida `E 9.3` |
| **Medida** | **1.** La puntuación de cada jugador es **exactamente** la que se deriva de `R 4.1` y `R 4.2` sobre el estado del tablero en el instante del cierre: comparable contra un cálculo independiente sobre el mismo estado. **2.** Ningún jugador retirado aparece en la designación del colocador del rey, ni con puntuación ni como candidato. **3.** El cierre produce **un solo** resultado, el del servidor, idéntico para todos los clientes. **4.** El cierre queda registrado, de modo que puede comprobarse después qué puntuación se asignó y a quién |

### Las cuatro preguntas de control

| Pregunta | Respuesta |
|---|---|
| ¿Qué se mide y desde qué instante? | La correspondencia entre el estado del tablero y la puntuación asignada, en el instante exacto del cierre de ronda |
| ¿En qué ventana y con qué frecuencia? | Dos veces por partida —fin de la ronda 1 y de la 2— más el cálculo final `DR-02` |
| ¿Quién lo nota si no se cumple? | Todos los jugadores: la puntuación parcial decide **quién coloca el rey** en la ronda siguiente `DEP-I6`, y eso cambia el desarrollo de la partida |
| ¿De qué `CON-` salió y de quién era el interés? | CON-03, interés de STK-01 —que su juego se refleje en su puntuación— y de STK-02 |

**¿Podría alguien demostrar mañana que ayer no se cumplió?** Sí, mientras la partida siga en curso: el estado vivo está guardado y la puntuación es recalculable sobre él. **Después de terminar, no**: `D-21` elimina el estado vivo. Ver la refutación R2.

### Refutación

**R1 · Se cumple y no es reproducible.** El cálculo es correcto, pero si dos jugadores empatan en la menor puntuación el colocador del rey **se elige al azar** `R 2.3`. Una prueba no puede afirmar el resultado con una aserción simple, como exige `E 10.4`.
→ **Es `TEN-F`, y este escenario la destapa.** No es defecto del escenario: es la tensión abierta. Conciliarlas exige que la fuente de azar sea un colaborador sustituible del dominio, que es exactamente el coste que `TEN-H` describe.

**R2 · Se cumple y no puede auditarse después.** La medida 4 dice que puede comprobarse qué puntuación se asignó. Pero `D-21` **elimina el estado vivo al terminar la partida**: de una partida terminada queda el puesto y los puntos `P-05`, no el tablero.
→ **La refutación acotó la medida:** la verificación es posible **durante la partida**, no después. Queda declarado, no escondido.

---

# T-1 · Testabilidad — probar una regla sin levantar nada

**Procedencia:** CON-08 · **Stakeholder:** STK-02, sombrero probador · **Atributo:** Testabilidad (ISO/IEC 25010, `DR-16`)

| Parte | Contenido |
|---|---|
| **Fuente** | STK-02 en su sombrero de **probador** |
| **Estímulo** | Ejecuta la batería de pruebas de una regla del dominio: que un castillo **solo puntúa** si el jugador tiene exactamente un caballero propio en él `R 4.1`, `DEP-I2` |
| **Artefacto** | `Game.Domain` y el proyecto de pruebas |
| **Ambiente** | Entorno local de desarrollo de cualquiera de los dos integrantes, en **macOS o Linux** `ORG-03`: **sin servidor levantado, sin ventana de MonoGame y sin PostgreSQL corriendo** |
| **Respuesta** | La prueba construye el estado del tablero en memoria, invoca la regla y comprueba el resultado con una aserción directa. Los parámetros de `R 1.3` y `R 3.2` **llegan como argumentos** `D-30`, `E 2.2`, no se leen de la base. No se arranca `Game.Services`, no se abre `Game.Client` y no se toca `Game.Persistence` |
| **Medida** | **1.** La ejecución **no levanta ningún proceso externo**: ni servidor, ni ventana, ni motor de base de datos. **2.** El proyecto de pruebas del dominio **no referencia** ningún tipo de `Game.Services`, `Game.Persistence` ni `Game.Client` — comprobable inspeccionando las referencias del `.csproj`. **3.** Cada prueba hace **una sola ejecución** del método bajo prueba y aserciones directas, **sin lógica en la prueba** `E 10.3`, `E 10.4`. **4.** La batería corre con **un solo comando** en la máquina de cualquiera de los dos integrantes, en los tres sistemas `ORG-03`, `ORG-04` |

### Las cuatro preguntas de control

| Pregunta | Respuesta |
|---|---|
| ¿Qué se mide y desde qué instante? | El **aislamiento**: cuántas cosas hay que levantar para ejercitar una regla. Se mide sobre la ejecución completa de la batería |
| ¿En qué ventana y con qué frecuencia? | En cada cambio del dominio, y en cada versión del documento de reglas `DEP-E4`, que va por la v3.0 en borrador |
| ¿Quién lo nota si no se cumple? | STK-02: si probar una regla exige levantar servidor y base, cada cambio de regla cuesta un ciclo completo, y con `ORG-01` y `ORG-05` —dos personas, 14 semanas— eso se paga en funcionalidad no entregada |
| ¿De qué `CON-` salió y de quién era el interés? | CON-08. **Es el único concern cuyo propósito está declarado literalmente en la fuente**: `E 2.2` dice que la dirección de dependencias existe «para que las reglas del juego puedan probarse sin levantar el servidor ni abrir la ventana del juego» |

**¿Podría alguien demostrar mañana que ayer no se cumplió?** Sí, y de forma mecánica: basta mirar las referencias del proyecto de pruebas.

**Nota sobre el umbral.** Este escenario **no fija un tiempo máximo de ejecución** a propósito. Lo que `E 2.2` declara es aislamiento, no velocidad, y ninguna fuente enuncia un umbral de tiempo. Añadir uno sería inventarlo `DR-29`.

### Refutación

**R1 · Se cumple y la regla que importa sigue sin poder probarse.** El aislamiento es perfecto, pero si la regla bajo prueba invoca el sorteo del empate `R 2.3` o la obtención de cartas al azar `DR-06`, **la prueba no puede afirmar el resultado con una aserción simple**.
→ **Es `TEN-F`, y era previsible: hay dos fuentes de azar en el dominio.** El escenario se cumple y la testabilidad no alcanza a las dos reglas donde más falta hace. Resolverlo exige convertir el azar en un colaborador sustituible: decisión de diseño con coste, hoy no tomada.

**R2 · Se cumple y no sirve al otro integrante.** Si la batería solo corre en una de las dos máquinas, el aislamiento es real pero el beneficio es de una persona.
→ **La refutación añadió la medida 4**: correr en la máquina de cualquiera de los dos, en los tres sistemas. Sin ella, `ORG-04` quedaba fuera del escenario.

---

# L-1 · Analizabilidad — diagnosticar sin reproducir el fallo

**Procedencia:** CON-09 · **Stakeholder:** STK-02, sombrero operador · **Atributo:** Analizabilidad (ISO/IEC 25010, `DR-16`)

| Parte | Contenido |
|---|---|
| **Fuente** | `Game.Services`, al aplicar la acción de un jugador |
| **Estímulo** | Una operación **falla con una excepción** mientras se aplica esa acción en una partida concreta |
| **Artefacto** | El registro de eventos —log4net, `DEP-E1`— y las clases de `Game.Services` y `Game.Domain` que registran |
| **Ambiente** | Servidor en operación normal, con **varias partidas en curso** `DR-31`, y la partida **ya terminada o no reproducible** cuando alguien investiga |
| **Respuesta** | Se escribe **un solo registro** por fallo `E 9.5`, desde el registrador **de la clase donde ocurre** `E 9.1`, con: la **operación** que se estaba ejecutando, el **identificador interno** de la entidad afectada y el de la **partida** `E 9.3`, y la **excepción pasada como argumento** para conservar su tipo y su traza. **No** aparece username, correo, token ni ningún otro dato personal `E 9.4`, CON-06 |
| **Medida** | **1.** A partir del registro, y **sin volver a ejecutar la partida**, puede determinarse qué operación falló, sobre qué partida y sobre qué jugador —por su identificador interno—. **2.** **Exactamente una** entrada por fallo: ni duplicados por relanzar la excepción, ni fallos silenciosos. **3.** **Cero** datos personales en el registro: buscar un correo o un username en él no devuelve nada. **4.** La entrada permite identificar la clase de origen sin recorrer el código |

### Las cuatro preguntas de control

| Pregunta | Respuesta |
|---|---|
| ¿Qué se mide y desde qué instante? | Si el registro basta **por sí solo** para el diagnóstico, medido en el momento en que alguien lo lee, que es siempre **después** del fallo |
| ¿En qué ventana y con qué frecuencia? | En cada fallo. La ventana es indefinida: el registro se lee días después, cuando la partida ya no existe |
| ¿Quién lo nota si no se cumple? | STK-02 en su sombrero de **operador**, que es quien lee los registros — y lo nota justo cuando ya no puede reproducir el fallo |
| ¿De qué `CON-` salió y de quién era el interés? | CON-09. El estándar enuncia el criterio con la forma exacta del atributo: un mensaje sin ese contexto **obliga a reproducir el fallo** para saber a qué se refería |

**¿Podría alguien demostrar mañana que ayer no se cumplió?** Sí: si para entender una entrada del registro hubo que reproducir el fallo, no se cumplió.

### Refutación

**R1 · Se cumple y el diagnóstico se queda a medias.** El registro identifica la operación, la partida y el jugador. Pero `D-21` **elimina el estado vivo al terminar la partida**: si el fallo se investiga después, **el tablero sobre el que ocurrió ya no existe**. Se sabe *qué* falló y *sobre qué*, no *con qué estado*.
→ **La refutación acota el alcance del escenario, y queda declarado.** CON-09 promete identificar la operación y la entidad, **no reconstruir el estado**. Reconstruirlo exigiría conservar el estado o la secuencia de jugadas, y ambas cosas están **fuera de alcance** por decisión ya tomada.

**R2 · Se cumple y el artefacto podría no existir.** El escenario descansa sobre log4net `RES-06`, `DEP-E1`, y `STACK §6` **no lista entre lo verificado** que log4net funcione sobre .NET 10 `DEP-E13`.
→ **Es el conflicto `C-3` de la base, hoy sin resolver por decisión de STK-02.** El escenario es válido; su artefacto está pendiente de verificar.

---

## Cobertura de los cinco escenarios

| Escenario | Concern | Atributo | Tensión que destapa |
|---|---|---|---|
| **D-1** | CON-01 V1 | Disponibilidad | — |
| **M-1** | CON-02 | Modificabilidad | — |
| **F-1** | CON-03 | Corrección funcional | `TEN-F` |
| **T-1** | CON-08 | Testabilidad | `TEN-F`, `TEN-H` |
| **L-1** | CON-09 | Analizabilidad | — |

**Cinco atributos distintos.** Se evita el riesgo que advierte el material de clase —«ocho escenarios de disponibilidad, ¿y las otras propiedades?»— y el árbol de utilidad tendrá cinco categorías con hoja, no una sola con cinco.

### Qué queda sin escenario, y por qué

| Concern | Motivo |
|---|---|
| **CON-01 V2** · caída del servidor | Está listo y tiene las mejores medidas del proyecto. Se dejó fuera **solo** porque repetiría categoría con D-1 y el encargo eran cinco |
| **CON-05** · aislamiento | Listo, con invariante de 5 partidas `DR-31`. Repetiría categoría con F-1 |
| **CON-06** y **CON-10** · confidencialidad y autenticidad | **Se dejaron fuera a propósito.** Su escenario tendría que declarar que el alcance **excluye el tránsito**, y eso depende del conflicto **C-2**, que STK-02 decidió dejar sin definir. Escribirlos ahora obligaría a rehacerlos |
| **CON-04**, **CON-07** | Son funcionales y no tienen atributo de calidad. No producen escenario |
