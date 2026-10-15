# Iteraciones 1 y 2 de diseño arquitectónico (ADD) — Juego de Torres

**Versión 1.0 · 21 sep 2026.**

Aplica las dos primeras iteraciones del método ADD al juego de Torres. **No se parte de una hoja
en blanco:** las entradas son los documentos ya producidos por el proyecto, y una parte de las
decisiones que aquí se citan **ya estaban tomadas** antes de esta iteración. Lo que este documento
añade es el paso explícito de *driver* a *decisión* y de *decisión* a *estructura*, con las
alternativas que se compararon y el motivo de la selección.

**Lo que este documento no hace.** No reabre reglas del juego, no reescribe los escenarios ni el
árbol de utilidad, no asigna toda la funcionalidad, no describe pantallas y no toca el modelo de
datos ya entregado. Tampoco fija umbrales: los fija STK-02 `DR-29`.

---

## 0. Entradas documentales y notación

### 0.1 Fuentes

| Documento | Papel en estas iteraciones |
|---|---|
| `Base-Oficial-STK-CON-DEP-Torres.md` v4.1 | **Registro normativo.** Stakeholders, concerns, dependencias, `RES-`, `ORG-`, `OBJ-`, `ASM-`, tensiones y cuestiones abiertas |
| `Escenarios-Arquitectonicos-Torres.md` v1.0 | Los cinco escenarios de atributos de calidad: D-1, M-1, F-1, T-1, L-1 |
| `Arbol-de-Utilidad-Torres.md` v1.0 | Prioridad (Importancia, Dificultad) de cada escenario |
| `Documento-Base-Consolidada-Torres.md` | Funcionalidades `FC-`, casos de uso `CU-`, entidades permanentes y **entidades en memoria**, reglas de conexión y persistencia |
| `Documento-Casos-de-Uso-Torres.md` v4.0 | Los 28 casos de uso descritos |
| `STACK.md` | Marco técnico, alternativas descartadas y estado verificado |
| `Estandar-de-codificacion-v7.md` §2 | Las cuatro capas y la dirección de dependencias (`E 2.1`, `E 2.2`) |
| `Analisis-Prueba-Myra-Torres.md` §4–§5 | Comportamiento de los callbacks duplex frente al hilo del juego |
| `Documento-Internacionalizacion-Torres.md` §4.7 | Efecto del idioma sobre los contratos compartidos |
| `code/database/schema.sql`, `seed-configuration.sql`, `src/Torres.Client/` | Artefactos ya construidos |

### 0.2 Notación de estado

Cada afirmación relevante se clasifica, y la clasificación se usa igual en las dos iteraciones:

| Estado | Significado | Consecuencia para el diseño |
|---|---|---|
| **Impuesto** | Una autoridad o una restricción registrada elimina las alternativas | No se compara: se comprueba que la restricción existe y se cita su fuente |
| **Elegido** | Ya hay decisión tomada y registrada en el proyecto | Se justifica, y se comprueba que sigue sostenida por sus drivers |
| **Candidato** | Parece útil y todavía compite con otras alternativas | Se compara y se selecciona, o se deja abierto declarándolo |
| **Supuesto** | Se da por cierto provisionalmente | Se registra qué cae si el supuesto cae |

### 0.3 Identificadores nuevos

El proyecto prohíbe reutilizar un identificador con otro significado. Se usan prefijos nuevos:
**`IT1-D`** y **`IT2-D`** para las decisiones de cada iteración, **`IT-C`** para los conceptos de
diseño candidatos, **`IT-S`** para los supuestos que estas iteraciones introducen y **`IT-Q`** para
las cuestiones que dejan abiertas. Ninguno colisiona con `DA-`, `D-`, `DR-`, `P-`, `Q-`, `S-`,
`CAND-` ni con los demás prefijos en vigor.

---

# ITERACIÓN 1 — Estructura general del sistema

## 1.1 Objetivo de la iteración

**Crear la estructura general del sistema:** decidir en qué elementos de grano grueso se divide
Torres, qué responsabilidad tiene cada uno y dónde vive el estado de una partida.

El objetivo procede del **propósito de diseño**, que es la entrada que gobierna la primera
iteración. En Torres el propósito es el desarrollo de un sistema nuevo en un dominio maduro —un
juego de mesa por turnos cliente–servidor—, **con parte del sistema ya construida**: existen el
esquema de base de datos cargado, un cliente de escritorio con diez pantallas internacionalizadas,
un prototipo de veintisiete pantallas y una prueba de concepto verificada del canal duplex
(`STACK §6`). Por eso la iteración no inventa la estructura desde cero: la **hace explícita,
comprueba que los drivers la sostienen y cierra lo que quedaba sin decidir**.

## 1.2 Inputs considerados (paso 1 de ADD)

### 1.2.1 Propósito de diseño

Sistema nuevo en dominio maduro, con artefactos previos. El único objetivo de negocio que habla
del resultado es `OBJ-01`: **se evalúa el código, no el acabado visual**. El segundo, `OBJ-02`, es
sobre el trabajo del equipo: las dos personas deben poder trabajar y probar de forma autónoma.
**Ninguna fuente enuncia un objetivo sobre el jugador** `DR-29`, y eso condiciona cómo se justifica
la importancia de cada driver: contra requisito escrito o regla del juego, no contra un objetivo
de negocio.

### 1.2.2 Funcionalidad primaria

Criterio de selección: funcionalidad que **atraviesa más elementos arquitectónicos**, que tiene
**mayor dificultad técnica** y cuya ausencia dejaría al sistema sin razón de ser. Se seleccionan
seis de los veintiocho casos de uso:

| Caso de uso | Por qué es primaria |
|---|---|
| **CU-25 Jugar un turno** | Es el sistema. Atraviesa las cuatro capas, consume el presupuesto de 5 PA, las cartas, el reloj de 90 s y toda la validación geométrica. La base consolidada declara que **la partida es la zona con menos cobertura** |
| **CU-19 Iniciar la partida** e **CU-24 Preparar la partida** | Transición sala → partida. Es donde `MatchSetupService` lee la configuración y la entrega al dominio `DR-11`, `D-30`, y donde se escribe el estado por primera vez `DR-18b` |
| **CU-26 Reconectar** y **CU-27 Volver a una partida reanudada** | Realizan CON-01 en sus dos variantes y son la funcionalidad que el escenario D-1 mide |
| **CU-13 Entrar a la sala** | Crea la **sala viva**, que es la entidad en memoria de la que cuelgan anfitrión, colores, chat y ranking de sala |
| **CU-01 Iniciar sesión** | Única puerta de entrada a la identidad persistente, de la que dependen historial, ranking global, amistades y sanciones |

**Funcionalidad no primaria y sin embargo dentro del alcance:** perfil, amistades, historial,
ranking global, chat, reportes y sanciones. Se manejan sobre la misma estructura y no la
modifican; el chat es la única con un efecto estructural propio —crece sin cota— y ya está acotado
por parámetro (200 mensajes en ventana deslizante).

### 1.2.3 Escenarios de atributos de calidad

Se consideran los cinco, con el par del árbol de utilidad:

| Escenario | Atributo | Par (I, D) | Qué le exige a la estructura |
|---|---|---|---|
| **F-1** | Corrección funcional | **(A, A)** | Un solo punto donde se determina el resultado; el cálculo de puntuación en el dominio; azar reproducible |
| **T-1** | Testabilidad | **(A, A)** | El dominio ejercitable sin servidor, sin ventana y sin base de datos; el proyecto de pruebas sin referencias a las otras capas |
| **D-1** | Disponibilidad | (A, M) | Custodia del estado del turno fuera del cliente; detección de la caída; reloj con autoridad |
| **M-1** | Modificabilidad | (A, B) | Parámetros fuera del código, leídos por servicios y pasados al dominio como argumentos |
| **L-1** | Analizabilidad | (M, B) | Registro por clase con operación e identificador de entidad. **No cambia ninguna estructura** |

**F-1 y T-1 son las dos hojas (A, A) y comparten causa: `TEN-F`**, el azar. El árbol ya señaló que
una sola decisión —convertir la fuente de azar en un colaborador sustituible del dominio—
desbloquea las dos. Esta iteración la toma.

### 1.2.4 Concerns arquitectónicos

Se distinguen dos clases, porque no entran igual en el diseño:

**a) Concerns del proyecto con atributo de calidad** — ya tienen escenario o lo tendrían:
CON-01, CON-02, CON-03, CON-05, CON-06, CON-08, CON-09, CON-10.

**b) Concerns sin atributo, que se comportan como funcionalidad:** CON-04 (conservación del
resultado) y CON-07 (cuenta o invitado). La base los declara funcionales a propósito. **CON-07 es
el que más estructura decide de los dos**, porque de los invitados no se guarda ninguna fila
`P-08` y eso determina qué puede vivir en la base de datos y qué no.

**c) Concerns generales del tipo que ADD trata en la primera iteración** —organización del trabajo
y entorno de construcción—, todos con registro propio en el proyecto:

| Concern general | Registro |
|---|---|
| Las dos personas deben poder correr **servidor y clientes en su propia máquina** | `ORG-04`, `OBJ-02` |
| Dos personas con cinco papeles cruzados, en catorce semanas | `ORG-01`, `ORG-02`, `ORG-05`, `TEN-H` |
| Se construye sobre un documento de reglas en estado **Borrador** | `ORG-06`, `TEN-E` |
| Equipo en macOS arm64 y Linux, con una sola máquina Windows | `ORG-03` |

### 1.2.5 Restricciones

Las diez del registro `RES-`. Las que gobiernan esta iteración:

| ID | Restricción | Impuesta por |
|---|---|---|
| `RES-01` | C# | **STK-03** |
| `RES-02` | WCF con callbacks **duplex sobre net.tcp**, llamadas asíncronas | **STK-03** |
| `RES-03` | **Prohibido el stack web y los WebSockets**; excluidos SignalR, gRPC y REST | **STK-03** |
| `RES-04` | Se evalúa el código, no el acabado visual | **STK-03** |
| `RES-05` | Cuatro capas `Game.Client → Game.Services → Game.Domain + Game.Persistence`; **el dominio no depende de nadie y el cliente no decide resultados** | STK-02, exigible por STK-03 |
| `RES-06` | Registro **solo con log4net**; prohibido registrar datos personales | STK-02, exigible por STK-03 |
| `RES-07` | Régimen de pruebas de `E 10` | STK-02, exigible por STK-03 |
| `RES-08` | `DesktopGL` nunca `WindowsDX`; todo en `net10.0`; nunca `PublishTrimmed` ni `PublishAot` | STK-02 |
| `RES-09` | `SecurityMode.None` en el `NetTcpBinding` | STK-02 · conflicto `C-2` abierto |

**Comprobación de que son restricciones y no preferencias.** `RES-01`…`RES-04` están enunciadas en
`STACK §2` como impuestas por el receptor de la entrega. `RES-05`…`RES-07` proceden del estándar de
codificación, que `E 1` declara obligatorio y cuya conformidad STK-03 evalúa: son exigibles, no
recomendaciones. `RES-08` y `RES-09` proceden de `ORG-03` y `ORG-04` —el equipo trabaja en macOS y
Linux— y **cerrarían el diseño indebidamente si se tomaran como universales**: son verdaderas para
este equipo y por ese motivo, no por una exigencia de STK-03.

### 1.2.6 Decisiones anteriores que entran como entrada

No se rediscuten en esta iteración; se citan porque restringen el espacio de soluciones.

| Decisión | Estado |
|---|---|
| El motor de base de datos es **PostgreSQL** `DR-33` | Elegido |
| Los parámetros configurables viven en **tablas de solo lectura**, los lee `Game.Services` y los pasa al dominio como argumentos `D-30` | Elegido |
| El manejador de partidas es **`MatchSetupService`**, en `Game.Services` `DR-11` | Elegido |
| El estado de la partida se escribe **al empezarla y al cerrar cada turno**, y se elimina al terminar `DR-18b`, `P-27`, `D-21` | Elegido |
| La pérdida de conexión se detecta **por latido**, con periodo y número de latidos perdidos declarados | Elegido; **sus dos cifras no están fijadas** |
| Lo que incluye invitados **no se persiste** `P-08`, `D-26`, `D-29` | Elegido |
| Diez entidades permanentes, cinco catálogos y **seis entidades en memoria** | Elegido, y ya materializado en `schema.sql` |

## 1.3 Elementos que se van a refinar (paso 3 de ADD)

**El sistema completo.** Es la primera iteración y el único elemento existente es Torres visto
como una caja. Lo que se descompone es esa caja.

Estado real del código en el momento de escribir: existe `src/Torres.Client` con diez pantallas y
la internacionalización; **no existe ningún proyecto de servidor, de dominio, de persistencia ni de
pruebas**. La estructura que esta iteración produce es, en su mayor parte, estructura por
construir, no estructura por documentar.

## 1.4 Conceptos de diseño candidatos (paso 4 de ADD)

**Un concepto de diseño es un candidato, no una respuesta.** Para cada uno: qué problema ayuda a
resolver, qué aporta, qué deja abierto y en qué acaba.

### 1.4.1 Arquitecturas de referencia

Una arquitectura de referencia **proporciona una estructura recurrente para una familia de
aplicaciones y todavía debe adaptarse al problema concreto**. Ninguna de las cuatro es, por sí
sola, la arquitectura de Torres.

| ID | Candidato | Qué aporta | Qué deja abierto | Resultado |
|---|---|---|---|---|
| **IT-C01** | **Aplicación web** (servidor web + navegador, MVC o SPA con API) | Estructura conocida para aplicaciones multiusuario | — | **Descartado. Impuesto por `RES-03`**, que prohíbe el stack web y nombra expresamente SignalR, gRPC y REST. No es preferencia del análisis: la restricción está escrita y viene de STK-03 |
| **IT-C02** | **Cliente enriquecido de escritorio con servidor de aplicación** | Separación entre presentación y servicios, sesión por cliente, canal de invocación remota | Dónde vive el estado, cuántos niveles físicos, cómo se notifica a los demás clientes | **Seleccionado como punto de partida.** Es la familia a la que Torres pertenece: cliente de escritorio MonoGame `DEP-E12` contra un servidor de aplicación |
| **IT-C03** | **Servidor autoritativo de juego en red con clientes de representación** | Describe el problema mejor que IT-C02: el servidor mantiene el estado de la partida, aplica las reglas y **empuja** los cambios; el cliente muestra y envía intenciones | Si el estado vive en memoria, en base o en ambas; qué se empuja —evento o estado completo—; cómo se detecta la caída | **Seleccionado y adaptado.** Se adapta a Torres así: **no hay bucle de simulación continuo** —el juego es por turnos—, **no hay predicción en el cliente** ni reconciliación, y el reloj del servidor es el único que cuenta |
| **IT-C04** | **Juego distribuido entre pares, con paso determinista de jugadas** | Usado en juegos por turnos; suprime el servidor | — | **Descartado. Impuesto por dos restricciones:** `RES-05` dice que el cliente no decide el resultado de ninguna acción, y `RES-02` fija cliente–servidor sobre WCF. Además haría imposibles CON-05, CON-04 y CON-10, que exigen una autoridad y una identidad persistente |

**Lo que IT-C03 no resuelve.** Aporta la forma general —autoridad central, clientes de
representación—, pero **no dice nada** sobre cuántos procesos hay, qué se escribe en base de datos,
cuándo, ni cómo se aísla una partida de otra. Todo eso hay que decidirlo, y parte se decide en la
iteración 2.

### 1.4.2 Patrones arquitectónicos

| ID | Candidato | Qué aporta | Estado |
|---|---|---|---|
| **IT-C05** | **Capas** | Separación de responsabilidades y dirección de dependencias fija | **Impuesto** por `RES-05`. Son cuatro capas y la dirección la fija `E 2.2`. **Es una descomposición de módulos, no de nodos**: no dice cómo se despliega |
| **IT-C06** | **Cliente–servidor** | Un servidor atiende a varios clientes por un canal | **Impuesto** por `RES-02` |
| **IT-C07** | **Publicar–suscribir** | Notificar a varios clientes un cambio que ninguno pidió: que otro jugador jugó, que el turno venció, que llegó un mensaje al chat | **Candidato seleccionado**, instanciado por los **callbacks duplex** que `RES-02` ya obliga a usar. El grupo de suscriptores es la sala o la partida. **Deja abierto qué viaja en la notificación** |
| **IT-C08** | **Intermediario (broker)** | Desacoplar clientes de la ubicación de los servicios | **Descartado.** No hay más de un servidor previsto, no hay requisito de carga ni de latencia, y añadirlo cuesta trabajo que `ORG-01` y `ORG-05` no tienen |
| **IT-C09** | **Modelo de dominio con colaboradores inyectados** | El dominio expresa las reglas sobre entidades propias y recibe de fuera lo que no puede conocer: parámetros, tiempo y azar | **Candidato seleccionado.** `E 2.1` ya coloca entidades y reglas en `Game.Domain`; `D-30` ya entrega los parámetros como argumentos. Lo que falta es aplicar el mismo mecanismo al **azar** y al **tiempo** |

### 1.4.3 Patrones de despliegue

Se identifican como candidatos y **su selección se aplaza a la iteración 2**, que es donde el
objetivo de la iteración lo exige. Candidatos: **dos niveles** (cliente / servidor con servicios,
acceso a datos y base) y **tres niveles** (cliente / servicios / persistencia y base como nivel
propio). Lo único que la iteración 1 fija es que el patrón de despliegue **no puede leerse del
patrón de capas**: `RES-05` obliga a cuatro capas y no obliga a ningún número de nodos.

### 1.4.4 Componentes externos

| Componente | Papel | Estado |
|---|---|---|
| **CoreWCF** `NetTcpBinding` duplex + `System.ServiceModel` | Todo el transporte `DEP-E11` | **Elegido**, verificado en macOS arm64 `STACK §6`. **Descansa sobre `ASM-02`**: STK-03 no lo ha aprobado — `C-4`, el mayor riesgo abierto |
| **Ensamblado de contratos compartido** sobre `netstandard2.0` | Un solo ensamblado con las interfaces, referenciado por cliente y servidor | **Elegido** `STACK §3`. **Su nombre no está fijado en ninguna fuente** |
| **MonoGame DesktopGL** 3.8.5.1 | Es `Game.Client` `DEP-E12` | **Elegido**, ya en uso |
| **Myra** 1.6.6 | Biblioteca de interfaz retenida sobre MonoGame | **Candidato.** El análisis la declara viable y recomienda **no migrar ahora** y adoptarla al empezar las pantallas de sala y partida. Referenciada ya por `Torres.Client.csproj` |
| **PostgreSQL** | Todo lo permanente y lo temporal en base `DEP-E7`, `DR-33` | **Elegido** |
| **Mecanismo de acceso a datos** (proveedor o mapeador) | Implementación de `Game.Persistence` | **Sin definir.** Ninguna fuente lo elige; Entity Framework figura en el análisis de referencia externa como **descartado por pertenecer a otro dominio** |
| **log4net** | Registro de eventos `DEP-E1`, `RES-06` | **Elegido**, con `C-3` pendiente: verificar sobre .NET 10 |
| **BCrypt** | Resguardo de la credencial `DEP-E8`, factor 11 | **Elegido** |
| **Canal de correo saliente** | Invitación y recuperación de acceso `DEP-E9` | **Tecnología sin definir** `Q-34`. El comportamiento ante fallo sí está decidido: informa y no reintenta |
| **Marco de pruebas** | Régimen de `E 10`, artefacto de T-1 | **Sin registrar.** El estándar usa `[Fact]` en ejemplos de otro juego, pero **no nombra ningún marco** |

## 1.5 Análisis formal: Concern → Driver → Decisión

Se desarrolla el caso que el objetivo de la iteración exige: **que ningún cliente pueda alterar por
su cuenta el estado reconocido de una partida**.

### 1.5.1 De dónde sale

| Paso | Contenido |
|---|---|
| **Concern** | **CON-03 · Correspondencia entre lo jugado y lo puntuado.** Que el resultado refleje exactamente las acciones legales realizadas, y solo esas. Titular: STK-01; comparte STK-02. Refuerza **CON-05**, aislamiento entre partidas simultáneas |
| **Driver 1 — escenario** | **F-1**, par **(A, A)**. Su medida 3 dice: *«El cierre produce un solo resultado, el del servidor, idéntico para todos los clientes»* |
| **Driver 2 — restricción** | **`RES-05`** (`E 2.1`): *«el cliente no decide el resultado de ninguna acción del juego»*, y `DEP-S2`: que el resultado sea único y aceptado depende de que **exista un solo punto donde se determina** |
| **Driver 3 — funcionalidad** | CU-25, CU-24 y los comportamientos sin actor: cierre de turno por tiempo, cierre de ronda y fin de partida |

### 1.5.2 Alternativas comparadas

| Alternativa | Qué implicaría | Efecto sobre los drivers |
|---|---|---|
| **A · Autoridad central en el servidor.** El cliente envía intenciones; el servidor valida, aplica y publica el estado resultante | Un solo punto de determinación; el cliente nunca tiene la última palabra | Satisface `RES-05` literalmente, satisface la medida 3 de F-1 y hace posible el invariante de CON-05 |
| **B · Autoridad en el cliente del jugador en turno**, con el servidor como retransmisor | El cliente activo calcula y el servidor difunde | **Viola `RES-05`.** Además haría indefendible F-1: dos clientes podrían producir dos resultados y el servidor no tendría criterio para elegir |
| **C · Cálculo en ambos lados y comparación** | El cliente calcula para responder rápido y el servidor confirma | No lo pide ningún driver: **no hay requisitos de latencia** en ninguna fuente. Duplica la implementación de las reglas, y con `ORG-01` y `ORG-05` el coste es prohibitivo. Empeora T-1: habría dos lugares donde probar la misma regla |

### 1.5.3 Decisión y clasificación honesta

> **`IT1-D01` · La autoridad sobre el estado de una partida es del servidor. El cliente envía
> intenciones y representa el estado que recibe; no determina ningún resultado.**
>
> **Estado: IMPUESTO.** No es una elección de esta iteración. `RES-05` la cierra por escrito, y
> `E 1` hace el estándar obligatorio, exigible por STK-03. La alternativa B queda eliminada por la
> restricción, no por el análisis.

**Lo que sí decide esta iteración** es el resto, que la restricción no cubre: en qué capa vive la
autoridad, con qué granularidad se conserva el estado y cómo llega el cambio a los demás clientes.

**Advertencia contra una justificación falsa.** Sería incorrecto justificar `IT1-D01` como defensa
frente a un jugador tramposo. La base evaluó esa cláusula y **no la activó**: hoy CON-03 es
corrección funcional, no integridad frente a un adversario, y **no hay atacante activo declarado**.
La justificación válida es la corrección —un solo punto de determinación `DEP-S2`— y el
aislamiento —CON-05—.

> **`IT-S01` · La cláusula condicional de CON-03 sigue sin activarse.** La fase de casos de uso
> incorporó **chat abierto entre jugadores y un régimen de reportes y sanciones**, que es
> exactamente la situación que `ASM-05` («se juega entre conocidos») negaba. La base oficial
> todavía registra `ASM-05` como supuesto vivo. **Pendiente de que STK-02 declare si CON-03 pasa a
> tocar integridad**; si lo hace, `IT1-D01` no cambia, pero aparecen decisiones nuevas —qué se
> valida del contenido del chat y qué se registra— y `CAND-11` deja de ser opcional.

## 1.6 Decisiones de la iteración 1

| ID | Decisión | Estado | Driver que la sostiene |
|---|---|---|---|
| **IT1-D01** | Autoridad del estado de la partida en el servidor | Impuesto | `RES-05`, F-1, `DEP-S2` |
| **IT1-D02** | Descomposición en **cuatro módulos** —`Game.Client`, `Game.Services`, `Game.Domain`, `Game.Persistence`— más un **ensamblado de contratos compartido** | Impuesto (las capas) + Elegido (el ensamblado) | `RES-05`, `E 2.2`, `DEP-E11`, `STACK §3` |
| **IT1-D03** | La autoridad de `IT1-D01` la ejerce **`Game.Services`**, no `Game.Domain` | Elegido en esta iteración | `E 2.2` + T-1 |
| **IT1-D04** | El **estado vivo de la partida reside en la memoria del proceso servidor**; la persistencia guarda **fronteras de turno**, no acciones | Elegido (ya vigente) | CON-01 V2, `DR-18b`, `P-27`, `P-28`, `D-21` |
| **IT1-D05** | El dominio recibe **azar y tiempo como colaboradores sustituibles**, igual que ya recibe los parámetros | Elegido en esta iteración, **pendiente de ratificación por STK-02** | F-1 R1, T-1 R1, `TEN-F`, `DEP-E6` |
| **IT1-D06** | Los cambios que el jugador no pidió se notifican por **callbacks duplex**, con la sala o la partida como grupo de suscriptores | Elegido en esta iteración | `RES-02`, IT-C07, CU-25, CU-13 |

### Justificación de cada decisión

**`IT1-D02` — cuatro módulos y un ensamblado de contratos.**
Las cuatro capas y su dirección son impuestas. El **quinto elemento** no lo es: es el ensamblado de
contratos sobre `netstandard2.0`, y se justifica por `DEP-E11` —*«sin él, cada cambio de contrato
desincroniza los dos lados»*— y por `ORG-01`: con dos personas trabajando en paralelo, la
alternativa es negociar a mano cada cambio de interfaz. Funciona porque CoreWCF reconoce por
reflexión los atributos de `System.ServiceModel`, lo que permite que el mismo ensamblado sirva a los
dos lados `STACK §3`. **Su nombre está sin fijar** — ver `IT-Q01`.

**`IT1-D03` — la autoridad vive en servicios, no en el dominio.**
Es la decisión que `RES-05` deja abierta, y la resuelve T-1. Si el dominio custodiara el estado
vivo tendría que conocer la sesión, el reloj y el canal, y la medida 2 de T-1 —*«el proyecto de
pruebas del dominio no referencia ningún tipo de `Game.Services`, `Game.Persistence` ni
`Game.Client`»*— dejaría de ser satisfacible. La división resultante es: **`Game.Services` custodia
y coordina; `Game.Domain` decide qué es legal y cuánto puntúa**. Es coherente con `DR-11`, que ya
colocó `MatchSetupService` en servicios porque lee configuración.

**`IT1-D04` — memoria para el estado vivo, base para las fronteras de turno.**
Dos drivers distintos empujan en direcciones opuestas y la decisión los concilia:

- **Hacia la memoria:** `CON-07` y `P-08` —de los invitados no se guarda nada—, que hacen
  imposible que sala, anfitrión, colores, chat y ranking de sala vivan en la base sin quedar
  incompletos; y `DR-30`, que exige retomar el turno **en el estado exacto**, incluidos puntos de
  acción no gastados, que es información de grano más fino que la que se escribe.
- **Hacia la base:** CON-01 V2, que exige que una caída del servidor no destruya la partida.

La conciliación es la **granularidad**: el punto de recuperación es la **frontera de turno**, no la
acción. Por eso el estado guardado no incluye reloj, puntos restantes ni construcciones pendientes,
y por eso un turno interrumpido **se rejuega entero con 90 s completos** `P-28`. La decisión ya
estaba tomada; lo que esta iteración añade es el argumento arquitectónico: **el coste de escritura
se acota a una vez por turno a cambio de aceptar perder, como máximo, el turno en curso.**

**`IT1-D05` — azar y tiempo como colaboradores del dominio.**
Es la única decisión que el árbol de utilidad señaló por su nombre: *«una sola decisión —convertir
la fuente de azar en un colaborador sustituible del dominio— desbloquea las dos hojas más caras del
árbol»*. Hay **dos fuentes de azar** en las reglas: el sorteo del empate `R 2.3` y la obtención de
cartas `DR-06`; y **una fuente de tiempo con autoridad** `DEP-E6`, de la que dependen el
vencimiento del turno y las ventanas. Sin esta decisión, F-1 R1 y T-1 R1 quedan refutados: la
prueba no puede afirmar el resultado con una aserción simple, como exige `E 10.4`.

El mecanismo **no es nuevo en el proyecto**: `D-30` ya establece que lo que el dominio no puede
conocer se lo entrega `Game.Services` como argumento. Esta decisión aplica la misma forma a dos
colaboradores más. **Coste declarado: `TEN-H`** — desacoplar cuesta, y el equipo son dos personas
con cinco papeles en catorce semanas. Se asume porque el coste de no hacerlo recae sobre las dos
hojas (A, A) del árbol.

**`IT1-D06` — notificación por callbacks duplex.**
`RES-02` ya obliga a callbacks duplex; lo que esta decisión fija es **para qué se usan**: para todo
cambio que un cliente debe conocer sin haberlo pedido —que otro jugador jugó, que el turno venció
por tiempo, que se cerró la ronda, que llegó un mensaje, que alguien entró o fue expulsado de la
sala—. El escenario D-1 lo exige explícitamente: *«los demás jugadores siguen viendo la partida y
el reloj avanzar, sin interrupción»*. **Queda abierto qué viaja en la notificación** — ver
`IT-Q03`.

## 1.7 Responsabilidades asignadas (paso 5 de ADD)

### `Game.Client` — interacción y representación

- Dibujar la pantalla, leer la entrada del jugador y **enviar intenciones**, nunca resultados
  `RES-05`.
- Representar el estado que recibe del servidor, incluido el reloj: **el reloj mostrado es una
  representación del reloj del servidor**, no una cuenta propia con autoridad.
- Traducir a idioma los textos visibles. El idioma **existe solo en el cliente**.
- **Trasladar los callbacks duplex al hilo del juego.** El callback llega en un hilo del canal y
  MonoGame dibuja en el hilo del juego; el análisis de Myra lo identifica como el riesgo real y
  como algo que **condiciona los contratos compartidos**.
- No contiene ninguna regla del juego.

### `Game.Services` — autoridad, coordinación y ciclo de vida

- **Exponer las operaciones del contrato** y validar sus argumentos.
- **Custodiar el estado vivo** de salas y partidas, que es la autoridad de `IT1-D01`.
- **Llevar el reloj del turno**, aplicar su vencimiento y las ventanas de reconexión. La latencia y
  el proceso van dentro de los 90 s del jugador.
- **Vigilar el latido** y declarar la pérdida de conexión: ese instante inicia la ventana de
  reconexión y el marcado de inactividad.
- **Leer la configuración** de los catálogos y **entregarla al dominio como argumentos** `D-30`;
  `MatchSetupService` configura las partidas que van a empezar `DR-11`.
- **Invocar al dominio** para decidir legalidad y puntuación, y **no implementar reglas** `E 2.1`.
- **Ordenar la escritura** del estado en cada frontera de turno y su eliminación al terminar.
- **Publicar** los cambios por los callbacks duplex.
- Aplicar lo que no es regla del juego pero sí regla del sistema: sanciones, inactividad en sala,
  cierre de salas al arrancar `DR-18a`, y el borrado del archivo de avatar que la base no hace
  `D-28`.

### `Game.Domain` — reglas y estado de la partida como concepto

- Entidades del juego y reglas que las gobiernan: legalidad de una acción, coste en puntos de
  acción, geometría de castillos y torres, puntuación de ronda y final, designación del colocador
  del rey, ganador y desempates.
- **Toda la validación geométrica vive aquí**, porque el estado del tablero se guarda serializado y
  el motor de base de datos no lo valida `D-20`.
- **No depende de ninguna otra capa** `E 2.2`: no lee de persistencia, no conoce el canal, no
  conoce el reloj del sistema ni la fuente de azar del proceso — **los recibe** `IT1-D05`.

### `Game.Persistence` — conservación

- Leer y escribir las diez entidades permanentes o temporales y los cinco catálogos.
- Escribir y leer el **documento de estado de la partida** en las fronteras de turno; eliminarlo al
  terminar.
- Resolver la consulta derivada del **ranking global**.
- **No contiene reglas del juego** `E 2.1`. El desempate lo resuelve el dominio, y por eso el
  puesto final **se guarda, no se deriva**.

### Ensamblado de contratos — frontera del diálogo

- Declarar las operaciones, el contrato de callback y los tipos de datos que cruzan la frontera.
- Ser **el único** lugar donde esos tipos se declaran, referenciado por cliente y servidor.
- **No contiene textos traducibles**: los recursos de idioma no deben ir aquí.

## 1.8 Estado activo frente a estado durable

La distinción no es de comodidad: cada lado responde a un driver distinto.

### Estado activo — memoria del proceso servidor

| Elemento | Por qué no puede estar en la base |
|---|---|
| Sala viva: lista **ordenada** de participantes, anfitrión, colores en uso, chat y ranking de sala | **Incluye invitados**, de los que no se guarda ninguna fila `P-08`. Una tabla que solo contuviera cuentas nunca sabría si una sala está vacía, que es la condición que decide si la sala sigue existiendo |
| Invitado: alias e identificador temporal, que es su **pase de asiento** | Misma razón. El cliente lo guarda **en memoria y no en disco**: un archivo local le daría identidad persistente, contra CON-07 |
| Mensajes de chat | Decidido no persistirlos; ventana deslizante de 200 por sala |
| Estado de conexión, **reloj del turno**, **puntos de acción restantes** | Cambian dentro del turno. Guardarlos obligaría a escribir por acción y no por frontera de turno, y `P-28` decide lo contrario: el turno interrumpido se rejuega entero |
| Tablero, castillos, caballeros, rey, cartas obtenidas y usadas, puntuación acumulada | Es el estado autoritativo vivo. Se **refleja** en la base en cada frontera de turno, pero la autoridad es la memoria |

### Estado durable — base de datos

| Elemento | Por qué debe sobrevivir |
|---|---|
| Cuenta, amistades, sanciones | Identidad persistente: de ella dependen ranking global, historial, atribución y el régimen de convivencia `DEP-S4` |
| Sala y sus invitaciones pendientes | La sala es la referencia de sus partidas; **se cierra, no se borra** `D-23` |
| Partida y participaciones con puesto y puntos | CON-04: el resultado debe seguir disponible para ranking e historial. El puesto final **se guarda**, no se deriva |
| Reportes con copia del mensaje | El chat no se conserva: sin la copia el reporte se quedaría sin sustento |
| Código de recuperación | Vale cinco minutos y es la prueba de titularidad de CON-10 |
| Catálogos de parámetros | Son el mecanismo de M-1: cambiar una fila, sin tocar el dominio ni recompilar `D-30`, `DR-17` |
| **Documento de estado de la partida en curso** | Es lo único que hace posible CON-01 V2. Se escribe al empezar y al cerrar cada turno, y **se elimina al terminar** |

### Lo que la separación cuesta, y está registrado

**`TEN-G`, abierta.** `DR-10` no limita el número de partidas simultáneas, el estado vivo está en
memoria y `P-31` retiene partidas caídas tres minutos esperando jugadores. Sin límite, por estado
en memoria, por sesiones esperando, es presión de memoria que nadie ha examinado, y se agrava con
`ORG-04`: el servidor corre en el portátil de cada integrante. **La tensión no exige limitar nada;
exige saber que no limitar tiene coste.** Queda abierta al terminar esta iteración.

## 1.9 Estructura arquitectónica resultante (paso 6 de ADD)

### 1.9.1 Vista de módulos

```
                 ┌──────────────────────────────────────┐
                 │            Game.Client                │  intención + representación
                 │  MonoGame DesktopGL · (Myra, candidato)│
                 └───────────────┬──────────────────────┘
                                 │ usa
                 ┌───────────────▼──────────────────────┐
                 │   Ensamblado de contratos compartido  │  netstandard2.0
                 │   operaciones · callback · tipos      │  (nombre sin fijar)
                 └───────────────▲──────────────────────┘
                                 │ implementa
                 ┌───────────────┴──────────────────────┐
                 │           Game.Services               │  AUTORIDAD del estado vivo
                 │  sesión · reloj · latido · publicación │  MatchSetupService (DR-11)
                 └──────┬────────────────────┬──────────┘
                        │ invoca             │ ordena leer y escribir
          ┌─────────────▼──────────┐  ┌──────▼──────────────────┐
          │      Game.Domain        │  │    Game.Persistence      │
          │  reglas · puntuación ·  │  │  entidades · catálogos · │
          │  geometría · desempate  │  │  documento de estado     │
          │  (no depende de nadie)  │  └──────┬──────────────────┘
          └─────────────────────────┘         │
                                              ▼
                                        PostgreSQL (fuera de la frontera)
```

La flecha que falta es deliberada: **`Game.Domain` no apunta a `Game.Persistence`** `E 2.2`. Cuando
el dominio necesita un dato que solo la persistencia sabe obtener, servicios lo lee y se lo entrega
como argumento.

### 1.9.2 Correspondencia con la solución C#

| Módulo | Proyecto | Estado hoy |
|---|---|---|
| `Game.Client` | `src/Torres.Client` | **Existe.** Diez pantallas, i18n, MonoGame DesktopGL 3.8.5.1 |
| `Game.Services` | — | **Por crear.** Aloja el host de CoreWCF |
| `Game.Domain` | — | **Por crear.** Sin referencias a ningún otro proyecto |
| `Game.Persistence` | — | **Por crear** |
| Contratos compartidos | — | **Por crear**, en `netstandard2.0`; el resto en `net10.0` `RES-08` |
| Proyecto de pruebas del dominio | — | **Por crear.** Es artefacto de T-1 y su marco no está registrado |

**Discrepancia detectada, no corregida aquí.** El estándar nombra las capas `Game.*` y el proyecto
existente usa el espacio de nombres `Torres.Client`. Los dos son fuentes del proyecto y no
coinciden. **Pendiente de que STK-02 unifique el criterio de nombrado**; no se toca en esta
iteración porque pertenece a otra entrega.

**Lo que el ejemplo de la clase no aplica.** El material de referencia ilustra la estructura con un
cliente WPF. En Torres **WPF está descartado por escrito** —`STACK §4`, solo Windows, contra
`ORG-03`— y el cliente es MonoGame DesktopGL `DEP-E12`. No se introduce ningún proyecto que las
fuentes no respalden.

## 1.10 Revisión de la iteración (paso 7 de ADD)

| Driver | ¿Cubierto por esta iteración? |
|---|---|
| **F-1** corrección funcional (A, A) | **Parcial.** `IT1-D01` da el punto único de determinación y `IT1-D05` hace reproducible el azar. Falta el aislamiento entre partidas, que es CON-05 |
| **T-1** testabilidad (A, A) | **Sí, estructuralmente.** `IT1-D03` mantiene el dominio libre de sesión, reloj y canal; `IT1-D05` retira las dos fuentes de azar. Queda pendiente el marco de pruebas |
| **D-1** disponibilidad (A, M) | **Parcial.** El estado del turno está fuera del cliente y el reloj es del servidor. **El instante que la medida usa depende del latido, cuyas dos cifras no están fijadas** |
| **M-1** modificabilidad (A, B) | **Sí.** Ya resuelto por `D-30`; esta iteración solo confirma que la estructura lo sostiene |
| **L-1** analizabilidad (M, B) | **Sí, sin efecto estructural.** El registro por clase es disciplina de código. Persiste `C-3` |
| CON-01 V2 caída del servidor | **Sí**, por `IT1-D04` |
| CON-05 aislamiento | **No.** Se traslada a la iteración 2 |
| CON-06 y CON-10 seguridad | **No.** Dependen de `C-2`, que STK-02 dejó sin definir |
| Funcionalidad primaria | CU-25, CU-24, CU-19, CU-26, CU-27, CU-13 y CU-01 tienen **módulo asignado y responsabilidad**; no tienen todavía asignación de operaciones al contrato |
| `ORG-04` correr todo en la propia máquina | **No.** Es cuestión de despliegue: iteración 2 |

**Conclusión: el objetivo de la iteración se alcanza.** Existe una estructura general con
responsabilidades asignadas y con el estado clasificado. **No** se ha decidido el despliegue, y
CON-05 y `TEN-G` siguen sin tratarse: los dos son la entrada de la iteración 2.

## 1.11 Supuestos que quedan por validar

| ID | Supuesto | Si cae |
|---|---|---|
| `ASM-02` | **STK-03 aceptará CoreWCF.** `STACK §10.1` lo declara lo más urgente | **Cae toda la arquitectura de transporte**, y con `ORG-03` el proyecto vuelve a depender de la única máquina Windows |
| `ASM-03` | Los umbrales que fije STK-02 serán aceptables para STK-03 | Las medidas de los cinco escenarios pierden validez |
| **`IT-S01`** | La cláusula condicional de CON-03 sigue sin activarse pese al chat y a los reportes | Aparecen decisiones nuevas sobre validación y registro del contenido del chat |
| **`IT-S02`** | `Game.Services` puede custodiar todo el estado vivo sin que la memoria del proceso sea un problema | Es `TEN-G`. Si cae, hay que limitar partidas o mover parte del estado, y `DR-10` tendría que reabrirse |
| **`IT-S03`** | Los parámetros del latido pueden fijarse sin cambiar la estructura | Si la detección exigiera un mecanismo distinto del latido, la medida de D-1 cambia `Q-32` |

## 1.12 Cuestiones abiertas para la iteración 2

| ID | Cuestión |
|---|---|
| **IT-Q01** | **Cómo se despliega el sistema**: dos niveles o tres, y cuántos procesos de servidor |
| **IT-Q02** | Cómo se garantiza el **aislamiento entre partidas simultáneas** dentro de un mismo proceso, y cómo se verifica el invariante de las 5 partidas `DR-31` |
| **IT-Q03** | **Qué viaja en el callback**: el suceso ocurrido o el estado resultante completo |
| **IT-Q04** | **Nombre del ensamblado de contratos** y unificación del criterio de nombrado frente a `Torres.Client` |
| **IT-Q05** | **Mecanismo de acceso a datos** de `Game.Persistence`: ninguna fuente lo elige |
| **IT-Q06** | **Marco de pruebas**, no registrado por ninguna fuente y necesario para T-1 |
| **IT-Q07** | Las **dos cifras del latido**, deliberadamente no sembradas en `seed-configuration.sql` porque nunca se fijaron |

---

# ITERACIÓN 2 — De los drivers a las estructuras: despliegue y refinamiento

## 2.1 Objetivo de la iteración

**Transformar la estructura general de la iteración 1 en decisiones de estructura y despliegue.**
En concreto: decidir en cuántos nodos se despliega el sistema, cuántos procesos de servidor hay, y
cómo la estructura sostiene el aislamiento entre partidas simultáneas.

**Alcance declarado.** Esta iteración **no** resuelve todo el árbol de utilidad, **no** asigna toda
la funcionalidad a operaciones del contrato y **no** toma todas las decisiones del sistema. Quedan
fuera a propósito: el diseño interno de cada módulo, el reparto de los 28 casos de uso en
operaciones, las pantallas y las decisiones que dependen de `C-2`.

## 2.2 Inputs considerados

**Del resultado de la iteración 1:** `IT1-D01`…`IT1-D06`, la vista de módulos, el reparto entre
estado activo y estado durable, `IT-S01`…`IT-S03` e `IT-Q01`…`IT-Q07`.

**De los drivers del proyecto:** los mismos de §1.2, con el peso que §2.4 justifica.

## 2.3 Clasificación de las afirmaciones relevantes

**No todo lo documentado tiene el mismo nivel de decisión.** Antes de defender nada, se comprueba
que exista evidencia para tratarlo como restricción: **una falsa restricción cierra el diseño sin
motivo.**

| Afirmación | Estado | Evidencia | Comprobación |
|---|---|---|---|
| La comunicación es WCF duplex sobre net.tcp | **Impuesto** | `RES-02`, `STACK §2.2` | STK-03 lo exige. Real |
| Está prohibido el stack web y los WebSockets | **Impuesto** | `RES-03`, `STACK §2.3` | Real, y nombra SignalR, gRPC y REST |
| Cuatro capas con dirección fija de dependencias | **Impuesto** | `RES-05`, `E 2.1`, `E 2.2` | Real y verificable por las referencias de los `.csproj` |
| El cliente no decide resultados | **Impuesto** | `RES-05` | Real |
| El motor de base de datos es PostgreSQL | **Elegido** | `DR-33` | Decisión de STK-02 del 4 sep; descarta a sabiendas el entregable de doble clic |
| `SecurityMode.None` | **Elegido con conflicto abierto** | `RES-09`, `C-2`, `TEN-I` | **Aparece como restricción y no lo es del todo:** procede de `ORG-03`, no de STK-03. Existen salidas registradas, todas con coste. **No cierra el despliegue**, pero sí afecta qué viaja sin cifrar |
| `DesktopGL` y no `WindowsDX` | **Impuesto por condición organizacional** | `RES-08`, `ORG-03` | Real **para este equipo**. No sería restricción para un equipo solo Windows; el motivo está escrito |
| El sistema **no limita** las partidas simultáneas | **Elegido** | `DR-10` | Decisión explícita. **No es una medida de capacidad**, es una aspiración de diseño |
| Con 5 partidas en curso, ninguna altera el estado, el reloj ni el resultado de otra | **Elegido como invariante de verificación** | `DR-31` | **Las 5 no son un límite del sistema**: son la cifra con la que se verifica |
| Cada integrante debe poder correr servidor y clientes en su máquina | **Impuesto por condición organizacional** | `ORG-04`, `OBJ-02`, `STACK §2.6` | Real. `STACK` la señala como *la restricción que más decisiones fuerza* |
| No hay requisitos de latencia ni de carga | **Elegido por omisión, declarado** | Base §7 | **Comprobado:** el único dato de rendimiento en las fuentes es una justificación de estilo sobre el coste de las excepciones. **No inventar un requisito de rendimiento** |
| La detección de la caída es por latido | **Elegido** | Base consolidada §4.9 | El mecanismo está; **las dos cifras no** |
| Un tercer nivel físico mejoraría la seguridad | **Falsa restricción** | — | **Ninguna fuente lo pide.** Se examina en §2.7 y se descarta con argumento, no por costumbre |
| STK-03 aceptará CoreWCF | **Supuesto** | `ASM-02`, `C-4` | El mayor riesgo abierto y **no depende de STK-02** |
| El estado vivo cabe en la memoria del proceso | **Supuesto** | `IT-S02`, `TEN-G` | Nadie lo ha examinado |

## 2.4 Drivers seleccionados para esta iteración, y los que no

**Todos los drivers se consideran, pero no todos pesan igual sobre una decisión de despliegue.**

| Peso | Drivers | Por qué |
|---|---|---|
| **Decisivo** | `RES-02`, `RES-03`, `RES-05`, `ORG-04`, `ORG-03`, `DR-33` | Son restricciones y condiciones que **eliminan alternativas de despliegue** |
| **Decisivo** | **CON-05** con su invariante `DR-31`, y **CON-01 V2** | Escenarios y concerns que afectan la **estructura global**: dónde vive el estado compartido y qué pasa al reiniciar |
| **Alto** | `ORG-01`, `ORG-05`, `TEN-H`, `TEN-G` | Fijan cuánta complejidad puede sostenerse y cuál es el riesgo del estado en memoria |
| **Medio** | **F-1**, **T-1** | Ya atendidos estructuralmente en la iteración 1. Aquí solo se comprueba que el despliegue no los rompa |
| **Bajo para esta decisión** | **M-1**, **L-1**, CON-06, CON-10 | M-1 se satisface con una fila de una tabla en cualquier despliegue; L-1 no cambia ninguna estructura; CON-06 y CON-10 dependen de `C-2`, sin definir |
| **Funcionalidad primaria** | CU-25, CU-19, CU-24, CU-26, CU-27, CU-13, CU-01 | Define qué sistema se construye: un servidor que sostiene partidas vivas con varios clientes conectados a la vez |

## 2.5 Elementos que se van a refinar

1. **El sistema como conjunto desplegable** — no estaba decidido y es `IT-Q01`.
2. **`Game.Services`**, que en la iteración 1 recibió la custodia del estado vivo sin que se dijera
   cómo lo organiza para sostener CON-05.
3. **El ensamblado de contratos**, del que se decide su forma —no su contenido completo—.

## 2.6 Conceptos de diseño candidatos de esta iteración

| ID | Concepto | Qué problema resuelve | Qué aporta | Qué deja abierto | Resultado |
|---|---|---|---|---|---|
| **IT-C10** | **Arquitectura de referencia de dos niveles**: cliente de presentación / servidor con servicios de negocio, acceso a datos y base | Cómo repartir físicamente los elementos | Un solo nodo de servidor que cada integrante puede levantar; una sola frontera remota | Cómo se aísla una partida de otra dentro del proceso | **Seleccionado** — §2.7 |
| **IT-C11** | **Arquitectura de referencia de tres niveles**: cliente / servicios de negocio / persistencia con base | Igual | Permitiría desplegar y administrar la persistencia por separado | El segundo salto remoto tendría que construirse con net.tcp, porque `RES-03` prohíbe lo demás | **Descartado** — §2.7 |
| **IT-C12** | **Un proceso de servidor por partida** | Aislamiento por proceso del sistema operativo | Aislamiento fuerte de CON-05 sin trabajo de diseño | Cómo se enruta el canal duplex a la partida correcta; cómo se comparten cuentas, salas y ranking | **Descartado** — §2.8 |
| **IT-C13** | **Un único proceso de servidor con el estado vivo particionado por partida y por sala** | CON-05 dentro de un proceso | Aislamiento por estructura de datos, verificable con el invariante de las 5 partidas | Qué se serializa y con qué grano | **Seleccionado** — §2.8 |
| **IT-C14** | **Componente externo: host genérico de ASP.NET Core** como alojamiento de CoreWCF | Cómo se abre y se cierra el servicio | Ciclo de vida del proceso, configuración y arranque. Es **lo único que cambia frente a WCF clásico**: `ServiceHost` pasa a ser el host genérico | Qué se ejecuta al arrancar, antes de aceptar clientes | **Seleccionado**, ya implícito en `DEP-E11` |

## 2.7 Decisión de despliegue: dos niveles frente a tres

### 2.7.1 Qué se está decidiendo realmente

En Torres **el motor de base de datos ya es un proceso aparte, y en macOS arm64 corre en un
contenedor** `STACK §5`, `DR-33`. Además, la frontera del sistema deja el motor **fuera**: es una
dependencia externa `DEP-E7`, no un artefacto del sistema.

Por tanto la pregunta no es si la base está separada —lo está—, sino:
**¿`Game.Persistence` se convierte en un elemento desplegable propio, con una frontera remota entre
ella y `Game.Services`?**

### 2.7.2 Las dos alternativas

**Alternativa 1 · Dos niveles**

```
Nodo Cliente (1..n)                 Nodo Servidor (1)                Motor de base (externo)
┌────────────────────┐   net.tcp   ┌───────────────────────────┐    ┌──────────────┐
│ Game.Client        │◄──duplex───►│ Host CoreWCF              │    │ PostgreSQL   │
│ Contratos          │             │ Game.Services             │───►│              │
│ System.ServiceModel│             │ Game.Domain               │    └──────────────┘
└────────────────────┘             │ Game.Persistence          │
                                   │ archivos de avatar        │
                                   └───────────────────────────┘
```

**Alternativa 2 · Tres niveles**

```
Nodo Cliente (1..n)      Nodo Servicios (1)          Nodo Persistencia (1)      Motor de base
┌──────────────┐ net.tcp ┌──────────────────┐ ???   ┌──────────────────┐       ┌───────────┐
│ Game.Client  │◄duplex─►│ Game.Services    │◄─────►│ Game.Persistence │──────►│PostgreSQL │
└──────────────┘         │ Game.Domain      │       └──────────────────┘       └───────────┘
                         └──────────────────┘
```

### 2.7.3 Comparación

| Criterio | Alternativa 1 · dos niveles | Alternativa 2 · tres niveles |
|---|---|---|
| **Aislamiento de responsabilidades** | **Ya lo da `RES-05`.** Es separación de módulos y se comprueba mirando las referencias de los `.csproj`, que es justamente la medida 2 de T-1 | No añade aislamiento que las capas no den ya. Separar procesos no hace más estricta una dependencia que el compilador ya impide |
| **Frontera de confianza** | La única frontera de confianza real es **cliente ↔ servidor**, porque el cliente corre en la máquina del jugador | **No añade frontera.** El cliente no alcanza la persistencia en ninguna de las dos alternativas. La afirmación de que un tercer nivel «protege la base» **no tiene respaldo en ningún driver de Torres** |
| **Despliegue independiente** | Suficiente: un solo operador, que es STK-02 `ORG-02` | Ventaja real solo si alguien despliega o administra la persistencia por separado. **Nadie lo hace**: el equipo son dos personas con cinco papeles |
| **Escalar o administrar por separado** | No hace falta: **no hay requisitos de latencia ni de carga** en ninguna fuente | La presión que el proyecto sí tiene registrada es `TEN-G`, **de memoria del proceso servidor**. Un nivel de persistencia aparte **no la alivia**: el estado que crece es el que está en memoria, no el que está en base |
| **Cumplir una restricción existente** | Ninguna `RES-` exige separación física | Tampoco. **No hay restricción que la imponga** |
| **Comunicación remota** | Una sola frontera remota, ya verificada `STACK §6` | **Un segundo salto remoto que habría que construir.** Con `RES-03` fuera de juego —nada de REST ni gRPC—, sería otro canal net.tcp con su propio contrato, su propio modo de fallo y su propia serialización |
| **Complejidad añadida** | La mínima compatible con las restricciones | Contra `ORG-01`, `ORG-05` y `TEN-H`: dos personas, catorce semanas, cinco papeles |
| **`ORG-04` — cada integrante levanta todo en su máquina** | Dos procesos propios más el contenedor de la base | **Tres procesos propios más el contenedor.** Un elemento más que levantar y coordinar cada vez que alguien quiere probar. Empuja contra `OBJ-02` |
| **Modos de fallo** | El cliente cae, o el servidor cae. Los dos están especificados: CON-01 V1 y V2 | Aparece un tercero —**la persistencia inalcanzable con el servidor en pie**— que **ninguna fuente del proyecto ha especificado**. Habría que definirlo desde cero |

### 2.7.4 Decisión

> **`IT2-D01` · El sistema se despliega en dos niveles:** un **nodo cliente** por jugador con
> `Game.Client` y los contratos, y un **nodo servidor** único que aloja el host de CoreWCF con
> `Game.Services`, `Game.Domain` y `Game.Persistence`. El **motor de PostgreSQL** es un elemento
> **externo a la frontera**, desplegado como proceso propio —en contenedor sobre macOS arm64—, y el
> **sistema de archivos donde viven los avatares** pertenece al nodo servidor `DEP-E10`.
>
> **Estado: ELEGIDO en esta iteración.**

**Justificación.** El tercer nivel **no aporta ninguno de los beneficios que justificarían su
coste**: el aislamiento ya lo da `RES-05` y es verificable por compilación; la única frontera de
confianza es cliente–servidor; no hay operación separada ni requisitos de carga; y ninguna
restricción lo exige. En cambio, su coste es real y medible contra drivers existentes: un segundo
salto remoto que habría que diseñar con net.tcp por exclusión de `RES-03`, un modo de fallo nuevo
sin especificar, y un proceso más que levantar en cada máquina, contra `ORG-04` y `OBJ-02`.

**La decisión no se toma porque «dos niveles» sea el patrón conocido**, sino porque el único
argumento habitual a favor del tercer nivel —proteger la base de datos— **no aplica aquí**: el
cliente no tiene acceso a la persistencia en ninguna de las dos alternativas.

**Lo que esta decisión no resuelve, y hay que declarar:** con `SecurityMode.None` `RES-09`, lo que
cruza la única frontera remota —credencial, mensajes de chat y pase de asiento del invitado— **viaja
sin cifrar**. El despliegue en dos niveles **no empeora ni mejora** ese punto: es `C-2`, y sigue sin
definir por decisión de STK-02.

## 2.8 Decisiones de estructura derivadas

### 2.8.1 Cuántos procesos de servidor

| Alternativa | Efecto sobre CON-05 | Efecto sobre el resto |
|---|---|---|
| **IT-C12 · un proceso por partida** | Aislamiento fuerte y gratuito | El canal duplex del cliente se establece contra **un** extremo: habría que enrutar o reconectar al proceso correcto al empezar la partida. Cuentas, salas, amistades y ranking son compartidos y tendrían que vivir fuera. **Complejidad muy superior**, sin ningún driver que la pida |
| **IT-C13 · un proceso con el estado particionado** | Aislamiento por estructura: **un grafo de estado vivo por partida y por sala, sin estado mutable compartido entre partidas** | El canal duplex ya establecido sirve para sala y para partida. `MatchSetupService` configura las partidas dentro del mismo proceso `DR-11` |

> **`IT2-D02` · Un único proceso de servidor.** Aloja todo el estado vivo y atiende todos los
> canales duplex. **Estado: ELEGIDO.**
>
> **`IT2-D03` · El estado vivo se particiona por partida y por sala.** No hay estado mutable
> compartido entre partidas, y las operaciones sobre una misma partida se aplican **de una en una**,
> de modo que dos acciones concurrentes nunca observen un estado intermedio. Lo único legítimamente
> común entre partidas es lo que ya es común por diseño: las cuentas, los catálogos de parámetros y
> el registro de eventos. **Estado: ELEGIDO.**

**Justificación de `IT2-D03`.** Es la respuesta estructural a CON-05, que la iteración 1 dejó sin
atender. El concern exige que **lo que ocurre en una partida no altere el estado, el reloj ni el
resultado de otra**, y su medida no es una cifra de capacidad sino un **invariante**: con 5 partidas
en curso `DR-31`. La superficie sobre la que hay que demostrarlo incluye el **estado en memoria**,
no solo el de la base `D-26`, `D-29` — y esta decisión es lo que lo hace demostrable: si no hay
estructura compartida entre partidas, el invariante se comprueba ejercitando cinco partidas a la
vez y observando que ninguna ve el estado de otra.

**Coste aceptado y declarado.** `IT2-D02` mantiene todo el estado vivo en un solo proceso, que es
exactamente la situación que describe **`TEN-G`**: sin límite de partidas `DR-10`, con salas, chat y
ranking de sala en memoria, y con partidas caídas retenidas tres minutos. **La tensión no se cierra
aquí**, y se agrava por `ORG-04`, porque ese proceso corre en el portátil de cada integrante.

### 2.8.2 El ensamblado de contratos

> **`IT2-D04` · Un único ensamblado de contratos sobre `netstandard2.0`**, referenciado por el nodo
> cliente y por el nodo servidor, que declara las operaciones, el contrato de callback y los tipos
> que cruzan la frontera. **Estado: ELEGIDO**, ya respaldado por `DEP-E11` y `STACK §3`.
>
> **`IT2-D05` · Los fallos que el servidor comunica al cliente viajan como código, no como texto.**
> **Estado: ELEGIDO en esta iteración, pendiente de ratificación por STK-02.**

**Justificación de `IT2-D05`.** El idioma **existe solo en el cliente**: el servidor no sabe qué
idioma usa cada jugador, de modo que un error que viajara como texto llegaría siempre en un solo
idioma. El documento de internacionalización lo enuncia y advierte que **toca los contratos
compartidos, por lo que conviene decidirlo antes de escribirlos**. Esta iteración es el momento, y
la consecuencia estructural es doble: el ensamblado de contratos declara un catálogo de códigos, y
**los recursos de idioma no entran en él**.

### 2.8.3 El cliente frente al callback duplex

> **`IT2-D06` · El cliente recibe los callbacks en un hilo del canal y los traslada al hilo del
> juego mediante una cola que el ciclo de actualización vacía.** Ninguna estructura de la interfaz
> se modifica desde el hilo del canal. **Estado: ELEGIDO en esta iteración.**

**Justificación.** Es una consecuencia directa de combinar dos componentes externos ya elegidos:
CoreWCF llama de vuelta desde un hilo del canal, y MonoGame dibuja en el hilo del juego. El análisis
de la prueba de Myra lo identifica como **el riesgo real** de adoptar una biblioteca de interfaz
retenida, y señala que **condiciona los contratos compartidos**, por lo que pertenece a esta
iteración y no a una posterior. La decisión **es independiente** de si finalmente se adopta Myra:
con la caja de herramientas actual el problema no se manifiesta, pero la cola es lo que permite
adoptarla más adelante sin rehacer el cliente.

### 2.8.4 Arranque del proceso servidor

> **`IT2-D07` · Al arrancar, y antes de aceptar clientes, el proceso servidor cierra las salas que
> quedaran abiertas y busca las partidas en curso para reanudarlas.** **Estado: ELEGIDO** — es
> `DR-18a` y CON-01 V2, que esta iteración **ubica**: son responsabilidad del ciclo de vida del
> host, no de una operación del contrato.

**Justificación.** Ninguna de las dos acciones tiene actor humano; las dos son comportamientos del
sistema y **ninguna puede ejecutarse a petición de un cliente**, porque ocurren cuando todavía no
hay clientes. Ubicarlas en el arranque del host es lo que hace realizable la respuesta de CON-01 V2:
*al arrancar, el servidor busca las partidas en curso y las reanuda cuando han vuelto al menos dos
jugadores*, con un plazo de espera de tres minutos.

## 2.9 Responsabilidades asignadas en esta iteración

**Nodo cliente** — las de `Game.Client` de §1.7, más: establecer el canal duplex, **emitir el
latido** con el periodo configurado, y trasladar los callbacks al hilo del juego `IT2-D06`.

**Nodo servidor**, repartidas en tres responsabilidades del alojamiento que la iteración 1 no había
ubicado:

| Responsabilidad | Dónde queda |
|---|---|
| Abrir el extremo net.tcp, aceptar sesiones duplex y cerrarlas | **Host de CoreWCF** `IT-C14` |
| Cerrar salas abiertas y reanudar partidas en curso al arrancar | **Host**, antes de aceptar clientes `IT2-D07` |
| Custodiar el estado vivo particionado y aplicar las operaciones de una en una por partida | **`Game.Services`** `IT2-D03` |
| Vigilar los latidos, declarar la pérdida de conexión, iniciar la ventana de reconexión y el marcado de inactividad | **`Game.Services`** |
| Configurar la partida que va a empezar leyendo los catálogos | **`MatchSetupService`**, en `Game.Services` `DR-11` — **único artefacto con nombre fijado por una fuente** |
| Decidir legalidad, puntuación, colocador del rey, ganador y desempate | **`Game.Domain`** |
| Escribir la frontera de turno y eliminar el estado al terminar | **`Game.Persistence`**, por orden de `Game.Services` |

**Nombres.** Salvo `MatchSetupService`, **ninguna fuente del proyecto fija el nombre de ningún otro
componente**. Esta iteración asigna responsabilidades, no nombres: inventarlos crearía un registro
que después habría que defender.

## 2.10 Estructura arquitectónica resultante

### 2.10.1 Vista de despliegue

```
   ┌──────────────────────────┐              ┌───────────────────────────────────────┐
   │  Nodo Cliente  (1..n)    │   net.tcp    │       Nodo Servidor  (1)              │
   │  proceso por jugador     │   duplex     │                                       │
   │                          │◄────────────►│  Host CoreWCF  (host genérico)        │
   │  Game.Client (MonoGame)  │ SecurityMode │   └─ arranque: cierra salas,          │
   │  Contratos compartidos   │    .None     │      reanuda partidas  (IT2-D07)      │
   │  latido + cola de        │   (C-2)      │                                       │
   │  callbacks (IT2-D06)     │              │  Game.Services — estado vivo          │
   └──────────────────────────┘              │    particionado por partida y sala    │
                                             │  Game.Domain                          │
                                             │  Game.Persistence                     │
                                             │  archivos de avatar   (DEP-E10)       │
                                             └────────────┬──────────────────────────┘
                                                          │
                          ┌───────────────────────────────┴───────┐
                          ▼                                       ▼
             ┌──────────────────────┐                ┌────────────────────────┐
             │ PostgreSQL           │                │ Canal de correo        │
             │ proceso propio,      │                │ EXTERNO · tecnología   │
             │ contenedor en macOS  │                │ sin definir  (Q-34)    │
             │ EXTERNO a la frontera│                └────────────────────────┘
             └──────────────────────┘
```

**En desarrollo, por `ORG-04`,** los tres se ejecutan en el portátil de cada integrante: uno o
varios clientes, el servidor y el contenedor de PostgreSQL. Es la configuración que `OBJ-02` exige y
la que `STACK §6` ya verificó parcialmente —queda sin verificar la ejecución en Linux, en Windows y
la conexión real entre dos máquinas—.

### 2.10.2 Vista de módulos refinada

Respecto de §1.9.1, lo que esta iteración añade:

- El **host de CoreWCF** aparece como elemento con responsabilidad propia dentro del nodo servidor,
  distinto de `Game.Services`: **el ciclo de vida del proceso es suyo**.
- `Game.Services` se refina en **una partición de estado vivo por partida y por sala**, con
  aplicación serializada de operaciones por partida.
- El ensamblado de contratos incorpora un **catálogo de códigos de fallo** y **no lleva recursos de
  idioma**.
- El cliente incorpora la **cola de traslado de callbacks** entre el hilo del canal y el del juego.

## 2.11 Revisión de la iteración

| Driver | ¿Cubierto? |
|---|---|
| **CON-05** aislamiento, invariante de 5 partidas | **Sí, estructuralmente**, por `IT2-D03`. Queda por escribir la prueba que lo ejercita |
| **CON-01 V2** caída del servidor | **Sí.** `IT2-D07` ubica la reanudación en el arranque del host |
| **D-1** disponibilidad | **Parcial.** El latido queda ubicado en cliente y servidor; **sus dos cifras siguen sin fijarse** `IT-Q07` |
| **F-1**, **T-1** | **No los rompe.** Un solo proceso mantiene el punto único de determinación, y el dominio sigue sin referencias a otras capas |
| **M-1**, **L-1** | Indiferentes al despliegue; se siguen cumpliendo |
| `ORG-04`, `OBJ-02` | **Sí.** Dos procesos propios y un contenedor por máquina, que es lo mínimo compatible con `DR-33` |
| `RES-02`, `RES-03`, `RES-05` | **Sí.** Una sola frontera remota, sobre net.tcp duplex, sin introducir ningún protocolo prohibido |
| CON-06, CON-10 | **No.** `C-2` sigue sin definir; el despliegue lo deja igual y lo declara |
| `TEN-G` | **No se cierra.** `IT2-D02` la asume conscientemente |
| Funcionalidad primaria | Los siete casos de uso primarios tienen nodo, módulo y responsabilidad. **No tienen todavía operaciones del contrato asignadas** |

**Conclusión: el objetivo de la iteración se alcanza.** Hay una decisión de despliegue justificada
contra drivers, una respuesta estructural a CON-05 y tres decisiones de contrato y de cliente que
estaban condicionando la escritura del ensamblado compartido.

## 2.12 Supuestos que todavía necesitan validación

| ID | Supuesto | Si cae |
|---|---|---|
| `ASM-02` | STK-03 acepta CoreWCF | **Cae `IT2-D01` entera**: con WCF clásico el nodo servidor solo existe en Windows, y `ORG-04` se rompe |
| `IT-S02` / `TEN-G` | El estado vivo de un número no limitado de partidas cabe en la memoria de un proceso | Habría que reabrir `DR-10` o revisar `IT2-D02` |
| **`IT-S04`** | Un solo proceso basta para todas las partidas simultáneas previstas | Si no bastara, `IT2-D02` se revisa; `IT2-D03` **sobrevive**, porque particionar el estado es el primer paso para poder repartirlo |
| **`IT-S05`** | La aplicación de operaciones de una en una por partida no compromete el reloj de 90 s | No hay requisito de latencia que lo mida, pero `Q-33` ya decidió que **la latencia y el proceso van dentro de los 90 s del jugador**: el coste lo paga el jugador en turno |
| `IT-S03` | El latido se puede parametrizar sin cambiar la estructura | La medida de D-1 cambiaría `Q-32` |

## 2.13 Decisiones que quedan abiertas para la iteración siguiente

| ID | Cuestión | Por qué no se resuelve aquí |
|---|---|---|
| **IT-Q03** | **Qué viaja en el callback**: el suceso ocurrido o el estado resultante completo | Es diseño del contrato. Afecta al tamaño de los mensajes y a cuánto estado duplica el cliente; ninguna fuente lo decide y no condiciona el despliegue |
| **IT-Q04** | Nombre del ensamblado de contratos y unificación del criterio `Game.*` frente a `Torres.Client` | Requiere decisión de STK-02 |
| **IT-Q05** | Mecanismo de acceso a datos de `Game.Persistence` | Ninguna fuente lo elige. Entity Framework consta como descartado por pertenecer a otro dominio |
| **IT-Q06** | Marco de pruebas | No registrado, y T-1 lo necesita como artefacto |
| **IT-Q07** | Periodo del latido y número de latidos perdidos | **Deliberadamente no sembrados**: el proyecto declara el mecanismo y nunca fijó los números. Es el único hueco de valor vivo |
| **IT-Q08** | Reparto de los 28 casos de uso en operaciones del contrato, y **corte de las acciones dentro del turno** | La base consolidada lo declara la única cuestión de granularidad abierta, y corresponde a STK-02 |
| **IT-Q09** | Qué hacer con `C-2`, seguridad del transporte | Decisión de proyecto pendiente, no hueco de análisis. Bloquea los escenarios de CON-06 y CON-10 |
| **IT-Q10** | Tecnología del canal de correo | `Q-34`. El comportamiento ante fallo ya está decidido: informa y no reintenta |

---

# Trazabilidad: driver → decisión → estructura

| Driver | Decisión | Estructura resultante |
|---|---|---|
| `RES-05` + F-1 + `DEP-S2` | `IT1-D01` autoridad en el servidor | El cliente no calcula resultados; un solo punto de determinación |
| `RES-05` + `E 2.2` + `DEP-E11` | `IT1-D02` cuatro módulos + contratos | `Game.Client`, `Game.Services`, `Game.Domain`, `Game.Persistence` + ensamblado `netstandard2.0` |
| T-1 medida 2 | `IT1-D03` autoridad en servicios, no en dominio | `Game.Domain` sin sesión, reloj ni canal; proyecto de pruebas sin referencias cruzadas |
| CON-01 V2 + `P-28` + CON-07/`P-08` | `IT1-D04` memoria autoritativa, base en fronteras de turno | Seis entidades en memoria; documento de estado escrito al empezar y al cerrar turno |
| F-1 R1 + T-1 R1 + `TEN-F` | `IT1-D05` azar y tiempo como colaboradores | El dominio los recibe como argumentos, igual que los parámetros `D-30` |
| `RES-02` + D-1 respuesta | `IT1-D06` notificación por callbacks duplex | Grupo de suscriptores = sala o partida |
| `ORG-04` + `ORG-01` + `RES-03` + ausencia de requisitos de carga | `IT2-D01` dos niveles | Nodo cliente + nodo servidor; PostgreSQL externo |
| CON-05 + `DR-31` + `DR-10` | `IT2-D02`, `IT2-D03` un proceso con estado particionado | Un grafo de estado vivo por partida y por sala; operaciones serializadas por partida |
| Internacionalización §4.7 | `IT2-D05` fallos como código | Catálogo de códigos en el ensamblado de contratos; sin recursos de idioma |
| MonoGame + CoreWCF | `IT2-D06` cola de traslado de callbacks | Estructura propia del cliente entre hilo de canal e hilo de juego |
| `DR-18a` + CON-01 V2 | `IT2-D07` trabajo de arranque | Responsabilidad del host, antes de aceptar clientes |

---

# Pendientes de definir o validar

**De definición — nadie lo ha decidido todavía**

1. Periodo del latido y número de latidos perdidos `IT-Q07`.
2. Mecanismo de acceso a datos de `Game.Persistence` `IT-Q05`.
3. Marco de pruebas `IT-Q06`.
4. Nombre del ensamblado de contratos y criterio de nombrado `Game.*` frente a `Torres.Client` `IT-Q04`.
5. Qué viaja en el callback `IT-Q03`.
6. Tecnología del canal de correo `IT-Q10`.
7. Corte de las acciones dentro del turno `IT-Q08`.
8. Seguridad del transporte, conflicto `C-2` `IT-Q09`.

**De validación — se ha dado por cierto y falta comprobarlo**

9. `ASM-02` — aceptación de CoreWCF por STK-03. **Riesgo mayor del proyecto.**
10. `C-3` — log4net sobre .NET 10.
11. `IT-S01` — si la cláusula condicional de CON-03 sigue inactiva con el chat y los reportes dentro del alcance.
12. `IT-S02` / `TEN-G` — que el estado vivo de un número no limitado de partidas quepa en la memoria de un proceso.
13. `IT1-D05` e `IT2-D05` — ratificación por STK-02, que es el único que puede cerrar cuestiones.
14. `STACK §6` — ejecución en Linux, en Windows y conexión real entre dos máquinas.
