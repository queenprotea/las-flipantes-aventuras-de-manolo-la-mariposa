# Iteración 3 de diseño arquitectónico (ADD) — Juego de Torres

**Versión 1.0 · 21 sep 2026.**

Tercera iteración del método ADD sobre Torres. **Parte del diseño obtenido en la iteración 2** y
toma como driver el pendiente que esa iteración dejó registrado: de los cinco escenarios, **D-1
disponibilidad es el único que su revisión cerró como «Parcial»**, y el árbol de utilidad ya había
marcado su dificultad en M por la misma causa —`Q-32`, el criterio de detección de la caída, sigue
abierto—.

**Lo que esta iteración no hace.** No reescribe D-1 ni ningún otro escenario, no reabre las
decisiones `IT1-D01`…`IT2-D07`, no fija umbrales —los fija STK-02 `DR-29`— y no toca reglas del
juego, casos de uso ni modelo de datos.

---

## 0. Entradas

| Fuente | Qué aporta a esta iteración |
|---|---|
| `Documento-Iteraciones-ADD-Torres.md` v1.0 | El diseño del que se parte: `IT1-D01`…`IT1-D06`, `IT2-D01`…`IT2-D07`, y el backlog §2.13 |
| `Escenarios-Arquitectonicos-Torres.md` v1.0 | **D-1**, el escenario de disponibilidad que es el driver |
| `Arbol-de-Utilidad-Torres.md` v1.0 | Par de D-1 **(A, M)** y el motivo de la M: `Q-32` sin decidir |
| `Base-Oficial-STK-CON-DEP-Torres.md` v4.1 | `Q-32`, `DR-20`, `DR-30`, `DEP-E3`, `DEP-E6`, `DEP-I10`, `TEN-G` |
| `Documento-Base-Consolidada-Torres.md` §4.4, §4.9 | La detección por latido y el efecto de la desconexión en sala y en partida |
| `database/seed-configuration.sql` | Los dos parámetros del latido, declarados y **deliberadamente no sembrados** |
| `STACK.md` §6, §10 | Qué está verificado del canal duplex y qué no |

**Identificadores nuevos.** Se continúan las series ya abiertas, sin reutilizar ninguna:
**`IT3-D01`…** decisiones, **`IT-C15`…** conceptos de diseño candidatos, **`IT-S06`…** supuestos,
**`IT-Q11`…** cuestiones abiertas.

---

## 1. Objetivo de la iteración

**Contener el efecto de una falla externa —la pérdida de la conexión de un jugador— de modo que el
servidor la detecte en un instante acotado y la mesa no se detenga por ella.**

El objetivo sale de dos huecos concretos que el proyecto ya tiene registrados:

1. **La medida 1 de D-1 no es verificable hoy.** Dice que la ventana de reconexión se cuenta
   *«desde el instante en que el servidor detecta la pérdida de conexión»*. Ninguna decisión de las
   iteraciones 1 y 2 establece cuándo ocurre ese instante. Es `Q-32`, y la base advierte que
   **afecta a un escenario ya escrito**.
2. **La respuesta de D-1 exige algo que el diseño actual no garantiza:** *«Los demás jugadores
   siguen viendo la partida y el reloj avanzar, sin interrupción»*. Con `IT2-D03`, las operaciones
   de una misma partida se aplican **de una en una**; si la publicación de un cambio espera a una
   sesión que ya no responde, esa espera retiene el turno de la partida entera.

---

## 2. El escenario que gobierna la iteración

**D-1 · Disponibilidad — el jugador pierde la conexión en su propio turno.** Se reproduce en sus
seis partes tal como está escrito; **no se modifica**.

| Parte | Contenido |
|---|---|
| **Fuente** | El jugador activo (STK-01), a través de su conexión de red `DEP-E3` |
| **Estímulo** | Pierde la conexión con el servidor mientras es su turno y le quedan **T** segundos de sus 90 |
| **Artefacto** | El estado de la partida en curso, custodiado por `Game.Services`, y el reloj del turno `DEP-E6` |
| **Ambiente** | Partida en curso, **servidor en operación normal**, turno propio del jugador, mesa de 2 a 4 jugadores |
| **Respuesta** | El servidor conserva el puesto y el estado del turno. **El reloj sigue corriendo** `DR-20`. **Los demás siguen viendo la partida y el reloj avanzar, sin interrupción.** Si el jugador vuelve dentro de T, retoma su turno en el estado exacto `DR-30`; si no vuelve, al agotarse T queda retirado `R 5.2`, `DR-25` |
| **Medida** | **1.** La ventana es exactamente **T**, contada **desde el instante en que el servidor detecta la pérdida de conexión**. **2.** El turno **no se prolonga**: la espera de los otros no aumenta ni un segundo, y ningún turno pasa de 90 s `R 5.1`. **3.** Al vencer T, el retiro es efectivo antes de que empiece el turno siguiente. **4.** Reconexión y retiro dejan entrada en el registro de eventos |

### Criterio de éxito de esta iteración

De las cuatro medidas, **esta iteración se comprueba contra dos**, que son las que el diseño actual
no realiza:

> **Medida A** — El instante de detección existe y está **acotado**: la pérdida se declara a más
> tardar dentro de un plazo conocido y observable en el registro, y a partir de él empieza a contar
> la ventana.
>
> **Medida B** — Mientras un jugador está caído, **la partida sigue avanzando para los demás**: ni
> el cierre del turno por tiempo ni la entrega de cambios a los clientes vivos dependen de la
> sesión que no responde.

Las medidas 3 y 4 de D-1 ya están sostenidas por decisiones anteriores —el reloj y el vencimiento
son del servidor `IT1-D03`, y el registro es disciplina de `E 9.3`— y aquí solo se comprueba que no
se rompen.

**Lo que no se fija aquí.** Ninguna fuente de Torres enuncia un **porcentaje de disponibilidad**
objetivo, ni un tiempo máximo de recuperación, ni requisitos de carga o de latencia. Fijar uno sería
inventarlo `DR-29`. La medida de D-1 es un plazo del juego —la ventana T— y es contra eso contra lo
que se comprueba.

---

## 3. Qué ocurre hoy con el diseño de la iteración 2

Vocabulario preciso, para separar la causa del efecto visible:

| | En Torres |
|---|---|
| **Defecto** (causa original) | La conexión de red entre el cliente de un jugador y el servidor se pierde `DEP-E3`. **Es externa al sistema**: ni el nodo cliente ni el nodo servidor la controlan. Tiene dos formas: cierre que el transporte reporta, y corte sin cierre, en el que la sesión sigue pareciendo abierta desde el servidor |
| **Error** (estado intermedio, interno) | `Game.Services` conserva una sesión duplex que ya no responde. Nadie ha declarado la pérdida, así que **la ventana de reconexión no ha empezado**; y si el servidor intenta publicar un cambio a esa sesión, la entrega queda esperando |
| **Falla** (desviación visible) | Como `IT2-D03` aplica las operaciones de una partida **de una en una**, la espera retiene el turno: los otros jugadores dejan de ver avanzar el reloj y el estado de la mesa, **aunque su participación no dependa del jugador caído**. Es exactamente lo que la respuesta de D-1 prohíbe |

**El diseño funciona, y una dependencia externa a un solo jugador puede detener toda su mesa.** Ese
es el problema que esta iteración ataca: impedir que el defecto llegue a producir la falla.

---

## 4. Tabla 0 — los elementos con los que se trabaja

Los elementos de la iteración 2, con el nombre que tienen en las fuentes del proyecto. La frontera
del sistema (**SUD**) contiene el nodo cliente y el nodo servidor `IT2-D01`.

| Elemento | Qué sabe o decide | Qué no hace | De quién depende | ¿Dentro del SUD? |
|---|---|---|---|---|
| **`Game.Client`** | Dibuja, lee la entrada del jugador, envía intenciones, **emite el latido**, traslada los callbacks al hilo del juego `IT2-D06` | No decide resultados, no contiene reglas, no lleva un reloj con autoridad | Ensamblado de contratos · **red `DEP-E3`** | **Dentro** (la red, **fuera**) |
| **Host de CoreWCF** | Abre el extremo net.tcp, acepta y cierra sesiones duplex, y ejecuta el trabajo de arranque `IT2-D07` | No conoce reglas ni estado de partida | `Game.Services` · **red `DEP-E3`** | **Dentro** |
| **`Game.Services`** | Custodia el estado vivo particionado por partida y sala `IT2-D03`, lleva el reloj del turno, **vigila el latido y declara la pérdida de conexión**, ordena escribir la frontera de turno y **publica los cambios por callback** `IT1-D06` | No decide legalidad ni puntuación, no dibuja, no traduce | `Game.Domain` · `Game.Persistence` · Host · **reloj `DEP-E6`** · **red `DEP-E3`** | **Dentro** |
| **`Game.Domain`** | Legalidad, coste en PA, geometría, puntuación, colocador del rey, ganador y desempate | No conoce sesión, canal, reloj del sistema ni fuente de azar: **los recibe** `IT1-D05` | De nadie `E 2.2` | **Dentro** |
| **`Game.Persistence`** | Lee y escribe entidades y catálogos, y el documento de estado en las fronteras de turno | No contiene reglas del juego | **PostgreSQL `DEP-E7`** | **Dentro** (el motor, **fuera**) |

### El elemento expuesto

**`Game.Services`**, en las dos responsabilidades que tocan la conexión del jugador: **vigilar el
latido** y **publicar por callback**.

- Es el único elemento que depende de algo que está **fuera del SUD y fuera del control del
  proyecto**: la red del jugador `DEP-E3`. `Game.Persistence` también depende de un elemento
  externo, PostgreSQL, pero ese fallo no es el que D-1 describe.
- Es donde el estado vivo de la partida está serializado por `IT2-D03`, de modo que una espera suya
  se convierte en espera de todos los jugadores de esa partida.
- Es el elemento cuyo artefacto nombra D-1: *«el estado de la partida en curso, custodiado por
  `Game.Services`, y el reloj del turno»*.

**Elemento a refinar: `Game.Services`.**

---

## 5. Alternativas comparadas

Tomadas del repertorio de tácticas de disponibilidad. Todas son viables en Torres: ninguna exige
protocolo prohibido por `RES-03` ni nodo adicional contra `IT2-D01`.

| ID | Alternativa | Tácticas que combina | Ventaja | Problema |
|---|---|---|---|---|
| **IT-C15** | **Solo el canal fallido.** El servidor declara la pérdida cuando el transporte reporta la sesión duplex en estado de fallo | Detectar: *Exception Detection* | No añade ningún elemento. Es una de las dos vías que `Q-32` nombra, y no cuesta nada mantener | **El instante no queda acotado.** Un corte sin cierre no se reporta: la sesión sigue pareciendo abierta y la pérdida solo se nota cuando el servidor intenta usar el canal. **Tampoco contiene nada:** el intento de publicar sigue reteniendo el turno de la partida |
| **IT-C16** | **Latido con umbral.** El cliente emite un latido con periodo fijo y el servidor declara la pérdida tras un número de latidos sin recibir | Detectar: *Heartbeat* + *Monitor* | **Acota el instante**: la detección ocurre a más tardar en `periodo × latidos perdidos`, y los dos valores viven en `system_parameter`, donde M-1 ya los hace configurables sin recompilar `D-30` | **Detecta, no contiene.** Mientras la pérdida no se declara —y también después, si nadie excluye la sesión— la publicación sigue dentro de la aplicación de la operación, y la mesa se detiene igual |
| **IT-C17** | **Latido con umbral + canal fallido + publicación fuera del turno de la partida + descarte de la sesión caída** | Detectar: *Heartbeat* + *Monitor* + *Exception Detection* + *Timeout* · Recuperar: *Ignore Faulty Behavior* | Acota el instante **y** corta la propagación: la aplicación de la operación sobre la partida deja de esperar a ningún cliente | Añade una salida por sesión dentro del proceso servidor, que es memoria en el mismo sitio que ya señala `TEN-G`. La notificación deja de ser simultánea al cambio de estado |

**Por qué no se consideran otras tácticas del repertorio.** *Redundant Spare*, *Shadow* y
*Escalating Restart* atienden la caída de un componente del sistema, no la de la red de un jugador;
la caída del servidor es CON-01 V2 y ya la atiende `IT2-D07`. *Retry* no aplica: el servidor no
puede reintentar la red del jugador, y la regla ya define qué hacer con el que no vuelve `R 5.2`.
*Voting* y *Predictive Model* exigirían duplicar el sistema o construir telemetría, sin ningún
driver que lo pida.

---

## 6. Decisión

> ### `IT3-D01` · Criterio de detección
> La pérdida de conexión de un jugador se declara por **la primera de dos vías**: el canal duplex
> en estado de fallo, cuando el transporte lo reporta, o el **latido del cliente con umbral de
> latidos perdidos**, como vía general. El instante de esa declaración es el que inicia la ventana
> de reconexión `R 5.2` y el marcado de inactividad en sala.
>
> **Estado: ELEGIDO en esta iteración.** Cierra `Q-32`, cuyo *mecanismo* ya fijaba `RES-02` y cuyo
> *criterio* faltaba. **Los dos valores siguen siendo de STK-02** `DR-29`.

> ### `IT3-D02` · La publicación sale del turno de la partida
> Entregar un cambio a los clientes **deja de formar parte de la aplicación de la operación sobre
> la partida**. La operación termina cuando el estado cambia y se ordena su escritura; la entrega a
> cada cliente ocurre después, por una salida propia de cada sesión, y **ninguna espera de entrega
> retiene el turno de la partida**.
>
> **Estado: ELEGIDO en esta iteración.**

> ### `IT3-D03` · La sesión declarada caída se descarta
> Desde que se declara la pérdida hasta que el jugador reconecta, su sesión **queda excluida de las
> publicaciones**. Ese hecho **no cambia su estado en la partida**: lo cambia el vencimiento de la
> ventana `R 5.2`, no la caída.
>
> **Estado: ELEGIDO en esta iteración.**

> ### `IT3-D04` · Dónde viven las tres
> Dentro de `Game.Services` se concentra en **un elemento propio** todo lo que concierne a la
> sesión de cada jugador: recibir su latido, declarar su caída y entregarle las publicaciones.
> **El resto de `Game.Services` no toca el canal**: entrega lo que hay que publicar y sigue.
>
> **Estado: ELEGIDO en esta iteración. Su nombre no se fija aquí**, por la misma razón que §2.9 de
> la iteración 2: salvo `MatchSetupService`, ninguna fuente del proyecto nombra componentes, e
> inventar un nombre crea un registro que después hay que defender.

### Por qué gana IT-C17 a las dos descartadas

- **Frente a IT-C15.** El canal fallido no se descarta: se **conserva** dentro de `IT3-D01` porque
  cuando el transporte reporta el fallo, esa vía es más rápida que esperar al umbral. Lo que no
  puede hacerse es depender **solo** de ella: el corte sin cierre deja el instante sin acotar y la
  medida A sin cumplir.
- **Frente a IT-C16.** El latido con umbral es el mecanismo que el proyecto **ya declaró**
  (`Base-Oficial` §4.9 de la consolidada, y los dos códigos de parámetro que `schema.sql` reserva).
  Esta iteración no lo cambia; añade lo que le falta. Por sí solo cumple la medida A y **deja la
  medida B sin cumplir**, que es la mitad del objetivo.
- El orden importa: **`IT3-D02` es lo que hace que el valor del plazo de espera de una entrega deje
  de ser crítico.** Sin ella habría que ajustar ese plazo muy corto para que la mesa no se
  detuviera, y un plazo corto cancela entregas a clientes vivos pero lentos. Con ella, la espera ya
  no le cuesta nada a la partida.

### Costo declarado

| Coste | Detalle |
|---|---|
| **Memoria** | Una salida por sesión dentro del único proceso servidor `IT2-D02`. Es memoria en el mismo lugar que `TEN-G` ya señala: sin límite de partidas `DR-10`, con salas, chat y ranking de sala en memoria. **La tensión no se cierra; se agrava un poco y queda dicho** |
| **Frescura de lo que ve el cliente** | La notificación deja de ser simultánea al cambio de estado. Como el reloj que el cliente muestra es **una representación del reloj del servidor** §1.7, un retraso de entrega se ve como un reloj ligeramente atrasado. No hay requisito de latencia en ninguna fuente, y `Q-33` ya decidió que la latencia y el proceso van dentro de los 90 s del jugador: el coste lo paga el jugador en turno, no la mesa |
| **Falsos positivos** | Es el trade-off propio del latido: con un umbral agresivo, un jugador vivo pero con red lenta se da por caído. Con `DR-20` el reloj sigue corriendo, así que el error le cuesta tiempo de su turno. **Es una razón más para que las dos cifras las fije STK-02 y no esta iteración** |
| **Lo que la decisión no elimina** | Si la red del jugador no vuelve, el jugador queda retirado `R 5.2` y la partida puede terminar por quedar con menos de dos. **La iteración contiene la propagación de la falla; no elimina el defecto**, que está fuera del SUD |

---

## 7. Antes y después

Misma geometría; lo único que cambia es cómo `Game.Services` llega a los clientes. La frontera del
SUD se mantiene donde la puso `IT2-D01`.

### Antes — diseño al cerrar la iteración 2

```
┌ SUD: Torres ─────────────────────────────────────────────────────────────┐
│                                                                          │
│  ┌──────────────────┐        net.tcp duplex        ┌──────────────────┐  │
│  │  Game.Client      │◄─────────────────────────── │  Host CoreWCF     │  │
│  │  (1..n)           │───────────────────────────► │                   │  │
│  └──────────────────┘   latido + intenciones       └────────┬─────────┘  │
│           ▲                                                 │            │
│           │  publicación dentro de la operación             ▼            │
│           │                                        ┌──────────────────┐  │
│           └────────────────────────────────────────│ Game.Services     │  │
│                                                    │ estado vivo por   │  │
│                                                    │ partida y sala    │  │
│                                                    │ (operaciones de   │  │
│                                                    │  una en una)      │  │
│                                                    └───┬──────────┬───┘  │
│                                                        ▼          ▼      │
│                                                 Game.Domain  Game.Persistence
└──────────────────────────────────────────────────────┼───────────────────┘
        red del jugador  DEP-E3  · FUERA               ▼   PostgreSQL · FUERA
```

**Lectura.** `Game.Services` publica dentro de la misma operación que aplica el cambio. Una sesión
que no responde retiene esa operación, y con ella el turno de la partida.

### Después — diseño al cerrar la iteración 3

```
┌ SUD: Torres ─────────────────────────────────────────────────────────────┐
│                                                                          │
│  ┌──────────────────┐        net.tcp duplex        ┌──────────────────┐  │
│  │  Game.Client      │◄─────────────────────────── │  Host CoreWCF     │  │
│  │  (1..n)           │───────────────────────────► │                   │  │
│  └──────────────────┘   latido + intenciones       └────────┬─────────┘  │
│           ▲                                                 │            │
│           │  entrega por sesión                             ▼            │
│           │                              ┌────────────────────────────┐  │
│           └──────────────────────────────│  CUSTODIA DE SESIONES      │  │
│                                          │  (elemento nuevo, dentro   │  │
│                                          │   de Game.Services)        │  │
│                                          │  · recibe el latido        │  │
│                                          │  · declara la caída        │  │
│                                          │  · salida por sesión       │  │
│                                          └──────────────▲─────────────┘  │
│                                            entrega lo   │ (no espera)    │
│                                            que hay que  │                │
│                                            publicar     │                │
│                                                    ┌────┴─────────────┐  │
│                                                    │ Game.Services     │  │
│                                                    │ estado vivo por   │  │
│                                                    │ partida y sala    │  │
│                                                    └───┬──────────┬───┘  │
│                                                        ▼          ▼      │
│                                                 Game.Domain  Game.Persistence
└──────────────────────────────────────────────────────┼───────────────────┘
        red del jugador  DEP-E3  · FUERA               ▼   PostgreSQL · FUERA
```

**Lectura.** La red del jugador sigue fuera del SUD y sigue pudiendo caerse. **Una sola caja
nueva**, dentro del nodo servidor: es el único punto que toca el canal, y es donde se declara la
caída y se acumula lo que hay que entregar. La partida ya no espera a ningún cliente. No cambian
los nodos, ni el número de procesos, ni las capas.

---

## 8. El elemento nuevo

| | Custodia de sesiones (nombre sin fijar) |
|---|---|
| **Qué sabe o decide** | Qué sesión duplex corresponde a cada jugador conectado; cuándo llegó su último latido; **si una sesión está viva o caída**, y el instante exacto en que se declaró caída; en qué orden hay que entregarle a cada sesión lo que se le debe |
| **Qué no hace** | No aplica reglas, no toca el estado de la partida, no decide el retiro —lo decide el vencimiento de la ventana, que lleva `Game.Services` con el reloj del servidor—, no traduce, y **no devuelve nada al que le entrega una publicación**: quien publica no espera |
| **De quién depende** | Del host de CoreWCF, que le da las sesiones; del reloj `DEP-E6`, para medir el silencio del latido; y de los dos parámetros del catálogo |

### Cómo cambia la interfaz

| Dónde | Cambio |
|---|---|
| **Ensamblado de contratos** | Aparece **la operación con la que el cliente emite el latido**. La base consolidada ya declara que el cliente lo envía; hasta ahora ninguna decisión lo había puesto en el contrato. Es un cambio de contrato, y por eso pertenece a esta iteración y no a la implementación |
| **Ensamblado de contratos** | **No aparece ningún código de fallo nuevo.** La caída de un jugador no es un fallo que se devuelva a quien invocó una operación: es un cambio de estado que el servidor observa. `IT2-D05` no se amplía |
| **Dentro de `Game.Services`** | La publicación pasa de ser una llamada que se espera a una entrega que se deja. Es la única parte de `Game.Services` que cambia de forma |
| **`Game.Client`** | **No cambia.** Ya emitía el latido `IT2-D06` §2.9 y ya traslada los callbacks al hilo del juego. Recibe lo mismo que antes |
| **`Game.Domain`, `Game.Persistence`** | **No cambian.** Ninguno conoce el canal `E 2.2` |
| **Catálogo de parámetros** | Se usan los dos códigos que `schema.sql` ya reserva y `seed-configuration.sql` deja sin sembrar a propósito: `heartbeat_period_seconds` y `heartbeat_misses_to_drop`. **Esta iteración no los siembra**: darles valor es de STK-02 `DR-29` |

---

## 9. Comprobación contra la medida

Se comprueba contra la medida del escenario, no contra la intención.

| Medida | ¿Se cumple? |
|---|---|
| **A · El instante de detección está acotado** | **Sí, estructuralmente.** `IT3-D01` fija que se declara por la primera de dos vías, y la vía general acota el plazo en `periodo × latidos perdidos`. Es verificable: se corta la red de un cliente y se lee en el registro `E 9.3` el instante en que se declaró la pérdida. **Con una reserva honesta: los dos valores no están fijados**, así que hoy el plazo es computable en cuanto STK-02 los fije, no antes |
| **B · La partida sigue avanzando para los demás** | **Sí.** Con `IT3-D02` la aplicación de la operación ya no espera a ninguna sesión, y con `IT3-D03` la sesión caída deja de recibir. El vencimiento del turno lo decide el reloj del servidor `IT1-D03`, no la entrega |
| **Medida 1 de D-1 (ventana = T desde la detección)** | **Sí**, una vez existe el instante. `DEP-I10` ya establecía que la duración depende de quién tenía el turno; lo que faltaba era el origen del conteo |
| **Medida 2 (el turno no se prolonga)** | **Sí, y ahora por construcción.** Antes dependía de que ninguna entrega se atascara |
| **Medida 3 (retiro efectivo antes del turno siguiente)** | **Sí.** No se altera: declarar la caída no retira a nadie `IT3-D03`; retira el vencimiento |
| **Medida 4 (queda registro)** | **Sí.** La declaración de pérdida y el retiro se registran con la operación y el identificador interno de la entidad `E 9.3`, sin datos personales `E 9.4` |

### Prueba de cierre

**¿La táctica previene, enmascara o mitiga el estímulo tal como lo exige la medida?**

- **No lo previene**, y no podría: el defecto es la red del jugador, que está fuera del SUD.
- **Lo mitiga** para el jugador caído: acota el instante en que su ausencia se reconoce, que es lo
  que hace realizable su ventana de reconexión.
- **Lo enmascara** frente a los demás jugadores: para ellos la partida no se interrumpe, de modo
  que **por su lado no hay falla observable**.
- **La respuesta no depende de que nadie haga nada.** Ningún jugador ni el operador tiene que
  intervenir para que la mesa siga. Lo único que queda en manos de una persona es que el jugador
  caído vuelva dentro de su ventana, y eso es la regla `R 5.2`, no un hueco de la táctica.

**Sobre el error habitual de nombrar tecnología en lugar de concepto.** La decisión de esta
iteración son conceptos de diseño —latido con umbral, detección por excepción del canal, descarte de
la fuente declarada caída y desacoplamiento de la publicación—. La tecnología que los realiza,
CoreWCF duplex sobre net.tcp, **no es la decisión**: la impone `RES-02` y estaba fijada desde antes
`DEP-E11`.

---

## 10. Backlog que deja esta iteración

Es el driver de la iteración siguiente.

| ID | Pendiente | Naturaleza |
|---|---|---|
| **IT-Q07** *(sigue abierta)* | Periodo del latido y número de latidos perdidos | **De decisión, de STK-02.** Ahora tiene consecuencia medible: fija el plazo de la medida A y el riesgo de falso positivo |
| **IT-Q11** | Plazo de espera de una entrega a una sesión antes de darla por no entregada | De decisión. `IT3-D02` quita urgencia al valor, pero el valor sigue sin existir |
| **IT-Q12** | **Si los demás jugadores se enteran de que alguien se cayó**, y no solo de que fue retirado al vencer su ventana | **De definición: ninguna fuente lo decide.** Afecta a qué publica la custodia de sesiones y a qué muestra el cliente |
| **IT-Q03** *(sigue abierta, con consecuencia nueva)* | Qué viaja en el callback: el suceso o el estado resultante | Con la entrega desacoplada, un cliente puede quedarse un momento sin recibir. Si viaja el suceso, tiene que reconstruir; si viaja el estado, no. **Esta iteración inclina la cuestión, no la cierra**: ninguna fuente la decide |
| **`TEN-G`** *(sigue abierta, agravada)* | Estado vivo sin límite de partidas en un solo proceso, ahora con una salida por sesión | De validación. Nadie la ha examinado |

---

## 11. Supuestos que esta iteración introduce

| ID | Supuesto | Si cae |
|---|---|---|
| **IT-S06** | Con net.tcp duplex, **un corte de red sin cierre limpio deja la sesión aparentemente abierta** del lado del servidor, y por eso hace falta el latido | Si el transporte reportara siempre el fallo con rapidez suficiente, `IT3-D01` se reduciría a su primera vía y el latido sería redundante. **No está verificado:** `STACK §6` no probó la conexión real entre dos máquinas |
| **IT-S07** | Entregar las publicaciones fuera de la operación **no rompe el orden que el cliente necesita** para representar la partida | Si el cliente necesitara ver los cambios en el mismo instante en que ocurren, habría que revisar `IT3-D02`. Hoy nada lo exige: no hay requisito de latencia y el reloj mostrado ya es una representación |
| **IT-S08** | Una salida por sesión **cabe en la memoria del proceso servidor** junto con todo el estado vivo | Es `TEN-G` otra vez. Si no cupiera, habría que limitar partidas —reabriendo `DR-10`— o revisar `IT2-D02` |

---

## 12. Trazabilidad: driver → decisión → estructura

| Driver | Decisión | Estructura resultante |
|---|---|---|
| D-1 medida 1 + `Q-32` + `DEP-E3` | `IT3-D01` detección por canal fallido o por latido con umbral | Instante de declaración acotado, con los dos valores en el catálogo de parámetros |
| D-1 respuesta («los demás siguen viendo la partida») + `IT2-D03` | `IT3-D02` publicación fuera del turno de la partida | La operación sobre la partida no espera a ningún cliente |
| `R 5.2` + `DR-20` + táctica de descarte | `IT3-D03` la sesión caída se excluye hasta reconectar | El descarte no altera el estado del jugador en la partida |
| `IT3-D01` + `IT3-D02` + `IT3-D03` | `IT3-D04` un elemento propio dentro de `Game.Services` | Un único punto que toca el canal; el resto de `Game.Services` entrega y sigue |
| Base consolidada §4.9 (el cliente emite el latido) | Consecuencia de `IT3-D01` | El ensamblado de contratos declara la operación del latido |

---

## 13. Pendientes de definir o validar que esta iteración deja

**De definición — nadie lo ha decidido:** los dos valores del latido `IT-Q07`; el plazo de espera de
una entrega `IT-Q11`; si los demás se enteran de la caída `IT-Q12`; qué viaja en el callback
`IT-Q03`; el nombre del elemento nuevo, que sigue el criterio de `IT-Q04`.

**De validación — se da por cierto y falta comprobarlo:** el comportamiento del canal net.tcp ante
un corte sin cierre `IT-S06`, que exige la prueba entre dos máquinas que `STACK §10.4` ya tenía
pendiente; que la entrega desacoplada no rompe el orden que el cliente necesita `IT-S07`; que la
salida por sesión cabe en memoria `IT-S08` / `TEN-G`; y la ratificación de `IT3-D01`…`IT3-D04` por
STK-02, que es el único que puede cerrar cuestiones.
