# Iteración de diseño: estructura general del sistema — Juego de Torres

**Versión 1.0 · 21 sep 2026.**

---

## 1. Objetivo de la iteración

Crear la estructura general del sistema: decidir en qué elementos de grano grueso se divide Torres,
qué responsabilidad tiene cada uno y dónde vive el estado de una partida.

El objetivo procede del propósito de diseño. Torres es un sistema nuevo en un dominio maduro —un
juego de mesa por turnos cliente–servidor— **con parte del sistema ya construida**: el esquema de
base de datos está cargado, existe un cliente de escritorio con diez pantallas internacionalizadas,
hay un prototipo de veintisiete pantallas y la prueba de concepto del canal duplex está verificada
en macOS arm64 `STACK §6`. La iteración no inventa la estructura desde cero: la hace explícita,
comprueba que los drivers la sostienen y cierra lo que quedaba sin decidir.

---

## 2. Entradas consideradas

| Documento | Qué aporta |
|---|---|
| `Base-Oficial-STK-CON-DEP-Torres.md` v4.1 | Stakeholders, concerns, dependencias, restricciones `RES-`, condiciones `ORG-`, objetivos `OBJ-`, supuestos `ASM-` y tensiones |
| `Escenarios-Arquitectonicos-Torres.md` v1.0 | Los cinco escenarios de calidad: D-1, M-1, F-1, T-1, L-1 |
| `Arbol-de-Utilidad-Torres.md` v1.0 | Par (Importancia, Dificultad) de cada escenario |
| `Documento-Base-Consolidada-Torres.md` | Funcionalidades, casos de uso, entidades permanentes, entidades en memoria y reglas de conexión y persistencia |
| `Estandar-de-codificacion-v7.md` §2 | Las cuatro capas y la dirección de dependencias `E 2.1`, `E 2.2` |
| `STACK.md` | Marco técnico, alternativas descartadas y estado verificado |
| `Analisis-Prueba-Myra-Torres.md` | Comportamiento de los callbacks duplex frente al hilo del juego |
| `Documento-Internacionalizacion-Torres.md` | Efecto del idioma sobre los contratos compartidos |
| `code/database/`, `src/Torres.Client/` | Artefactos ya construidos |

### 2.1 Objetivos de negocio

Solo hay dos enunciados por alguna fuente. `OBJ-01`: se evalúa el código, no el acabado visual.
`OBJ-02`: las dos personas del equipo deben poder trabajar y probar de forma autónoma. **Ninguna
fuente enuncia un objetivo sobre el jugador** `DR-29`, de modo que la importancia de un driver se
justifica contra requisito escrito o regla del juego, no contra un objetivo de negocio.

### 2.2 Funcionalidad primaria

Criterio de selección: funcionalidad que atraviesa más elementos arquitectónicos, que tiene mayor
dificultad técnica y cuya ausencia dejaría al sistema sin razón de ser. Se seleccionan seis de los
veintiocho casos de uso.

| Caso de uso | Por qué es primaria |
|---|---|
| **CU-25 Jugar un turno** | Atraviesa las cuatro capas. Consume el presupuesto de 5 puntos de acción, las cartas, el reloj de 90 s y toda la validación geométrica |
| **CU-19 Iniciar la partida** y **CU-24 Preparar la partida** | Transición de sala a partida. Es donde `MatchSetupService` lee la configuración y la entrega al dominio `DR-11`, `D-30`, y donde el estado se escribe por primera vez `DR-18b` |
| **CU-26 Reconectar** y **CU-27 Volver a una partida reanudada** | Realizan CON-01 en sus dos variantes y son la funcionalidad que el escenario D-1 mide |
| **CU-13 Entrar a la sala** | Crea la sala viva, de la que cuelgan anfitrión, colores, chat y ranking de sala |
| **CU-01 Iniciar sesión** | Única puerta de entrada a la identidad persistente, de la que dependen historial, ranking global, amistades y sanciones |

Perfil, amistades, historial, ranking global, chat, reportes y sanciones están dentro del alcance y
**no son funcionalidad primaria**: se manejan sobre la misma estructura y no la modifican.

### 2.3 Escenarios de atributos de calidad

| Escenario | Atributo | Par | Qué le exige a la estructura |
|---|---|---|---|
| **F-1** | Corrección funcional | **(A, A)** | Un solo punto donde se determina el resultado; cálculo de puntuación en el dominio; azar reproducible |
| **T-1** | Testabilidad | **(A, A)** | Dominio ejercitable sin servidor, sin ventana y sin base de datos; proyecto de pruebas sin referencias a las otras capas |
| **D-1** | Disponibilidad | (A, M) | Custodia del estado del turno fuera del cliente; detección de la caída; reloj con autoridad |
| **M-1** | Modificabilidad | (A, B) | Parámetros fuera del código, leídos por servicios y pasados al dominio como argumentos |
| **L-1** | Analizabilidad | (M, B) | Registro por clase con operación e identificador de entidad. No cambia ninguna estructura |

F-1 y T-1 son las dos hojas (A, A) del árbol y son duras por la misma causa, `TEN-F`: las reglas
invocan el azar en dos sitios, el sorteo del empate `R 2.3` y la obtención de cartas `DR-06`.

### 2.4 Concerns

**Concerns con atributo de calidad:** CON-01 continuidad ante desconexión o caída, CON-02 ajuste de
reglas y parámetros, CON-03 correspondencia entre lo jugado y lo puntuado, CON-05 aislamiento entre
partidas simultáneas, CON-06 confidencialidad, CON-08 testabilidad, CON-09 analizabilidad, CON-10
autenticidad.

**Concerns sin atributo, que se comportan como funcionalidad:** CON-04 conservación del resultado y
CON-07 jugar con cuenta o como invitado. **CON-07 es el que más estructura decide**, porque de los
invitados no se guarda ninguna fila `P-08` y eso determina qué puede vivir en la base de datos.

**Concerns sobre la construcción y la operación:**

| Concern | Registro |
|---|---|
| Las dos personas deben poder correr servidor y clientes en su propia máquina | `ORG-04`, `OBJ-02` |
| Dos personas con cinco papeles cruzados, en catorce semanas | `ORG-01`, `ORG-02`, `ORG-05`, `TEN-H` |
| Se construye sobre un documento de reglas en estado Borrador | `ORG-06`, `TEN-E` |
| Equipo en macOS arm64 y Linux, con una sola máquina Windows | `ORG-03` |

### 2.5 Restricciones

| ID | Restricción | Impuesta por |
|---|---|---|
| `RES-01` | C# | STK-03 |
| `RES-02` | WCF con callbacks duplex sobre net.tcp, llamadas asíncronas | STK-03 |
| `RES-03` | Prohibido el stack web y los WebSockets; excluidos SignalR, gRPC y REST | STK-03 |
| `RES-04` | Se evalúa el código, no el acabado visual | STK-03 |
| `RES-05` | Cuatro capas `Game.Client → Game.Services → Game.Domain + Game.Persistence`; el dominio no depende de nadie y el cliente no decide resultados | STK-02, exigible por STK-03 |
| `RES-06` | Registro solo con log4net; prohibido registrar datos personales | STK-02, exigible por STK-03 |
| `RES-07` | Régimen de pruebas de `E 10` | STK-02, exigible por STK-03 |
| `RES-08` | `DesktopGL` nunca `WindowsDX`; todo en `net10.0`; nunca `PublishTrimmed` ni `PublishAot` | STK-02 |
| `RES-09` | `SecurityMode.None` en el `NetTcpBinding` | STK-02, con el conflicto `C-2` abierto |

`RES-01` a `RES-04` están enunciadas en `STACK §2` como impuestas por el receptor de la entrega.
`RES-05` a `RES-07` proceden del estándar de codificación, obligatorio por `E 1` y evaluable por
STK-03. `RES-08` y `RES-09` proceden de `ORG-03`: son verdaderas para este equipo y por ese motivo,
no por exigencia de STK-03.

### 2.6 Decisiones anteriores que restringen el espacio de soluciones

| Decisión | Registro |
|---|---|
| El motor de base de datos es PostgreSQL | `DR-33` |
| Los parámetros configurables viven en tablas de solo lectura; los lee `Game.Services` y los pasa al dominio como argumentos | `D-30` |
| El manejador de partidas es `MatchSetupService`, en `Game.Services` | `DR-11` |
| El estado de la partida se escribe al empezarla y al cerrar cada turno, y se elimina al terminar | `DR-18b`, `P-27`, `D-21` |
| La pérdida de conexión se detecta por latido | Base consolidada §4.9 |
| Lo que incluye invitados no se persiste | `P-08`, `D-26`, `D-29` |
| Diez entidades permanentes, cinco catálogos y seis entidades en memoria | Base consolidada §3, ya materializado en `schema.sql` |

---

## 3. Elementos que se refinan

**El sistema completo.** Es la primera iteración y el único elemento existente es Torres visto como
una caja. Lo que se descompone es esa caja.

Estado del código al iniciar la iteración: existe `src/Torres.Client` con diez pantallas y la
internacionalización. **No existe ningún proyecto de servidor, de dominio, de persistencia ni de
pruebas.**

---

## 4. Conceptos de diseño candidatos

### 4.1 Arquitecturas de referencia

Una arquitectura de referencia proporciona una estructura recurrente para una familia de
aplicaciones y debe adaptarse al problema concreto. Ninguna de las cuatro es, por sí sola, la
arquitectura de Torres.

| Candidato | Qué aporta | Qué deja abierto | Resultado |
|---|---|---|---|
| **Aplicación web** (servidor web y navegador, con MVC o cliente de página única sobre una API) | Estructura conocida para aplicaciones multiusuario | — | **Descartado.** `RES-03` prohíbe el stack web y nombra expresamente SignalR, gRPC y REST |
| **Cliente enriquecido de escritorio con servidor de aplicación** | Separación entre presentación y servicios, sesión por cliente, invocación remota | Dónde vive el estado, cuántos niveles físicos, cómo se notifica a los demás clientes | **Seleccionado como punto de partida.** Es la familia a la que Torres pertenece: cliente MonoGame `DEP-E12` contra un servidor de aplicación |
| **Servidor autoritativo de juego en red con clientes de representación** | El servidor mantiene el estado de la partida, aplica las reglas y empuja los cambios; el cliente muestra y envía intenciones | Si el estado vive en memoria, en base o en ambas; qué se empuja; cómo se detecta la caída | **Seleccionado y adaptado:** sin bucle de simulación continuo porque el juego es por turnos, sin predicción en el cliente ni reconciliación, y con el reloj del servidor como único reloj con autoridad |
| **Juego distribuido entre pares con paso determinista de jugadas** | Suprime el servidor | — | **Descartado.** `RES-05` dice que el cliente no decide el resultado de ninguna acción y `RES-02` fija cliente–servidor sobre WCF. Además haría imposibles CON-05, CON-04 y CON-10 |

La arquitectura de referencia seleccionada aporta la forma general —autoridad central y clientes de
representación— y **no determina** cuántos procesos hay, qué se escribe en base de datos, cuándo, ni
cómo se aísla una partida de otra.

### 4.2 Patrones arquitectónicos

| Candidato | Qué aporta | Resultado |
|---|---|---|
| **Capas** | Separación de responsabilidades con dirección de dependencias fija | **Impuesto** por `RES-05`, con la dirección de `E 2.2`. Es una descomposición de módulos: no determina el número de nodos |
| **Cliente–servidor** | Un servidor atiende a varios clientes por un canal | **Impuesto** por `RES-02` |
| **Publicar–suscribir** | Notificar a varios clientes un cambio que ninguno pidió: que otro jugador jugó, que el turno venció, que llegó un mensaje | **Seleccionado**, instanciado por los callbacks duplex que `RES-02` ya obliga a usar. El grupo de suscriptores es la sala o la partida |
| **Intermediario** | Desacoplar clientes de la ubicación de los servicios | **Descartado.** No hay más de un servidor previsto, no hay requisito de carga ni de latencia, y su coste no cabe en `ORG-01` y `ORG-05` |
| **Modelo de dominio con colaboradores inyectados** | El dominio expresa las reglas sobre entidades propias y recibe de fuera lo que no puede conocer: parámetros, tiempo y azar | **Seleccionado.** `E 2.1` ya coloca entidades y reglas en `Game.Domain` y `D-30` ya entrega los parámetros como argumentos; falta aplicar la misma forma al azar y al tiempo |

**Patrones de despliegue.** Se identifican como categoría pertinente y **no se selecciona ninguno en
esta iteración**, cuyo objetivo es la descomposición en módulos. El patrón de capas no determina el
número de nodos: `RES-05` obliga a cuatro capas y no obliga a ningún número de nodos.

### 4.3 Componentes externos

| Componente | Papel | Estado |
|---|---|---|
| **CoreWCF** `NetTcpBinding` duplex con `System.ServiceModel` | Todo el transporte `DEP-E11` | Elegido y verificado en macOS arm64. Descansa sobre `ASM-02`: STK-03 no lo ha aprobado |
| **Ensamblado de contratos compartido** sobre `netstandard2.0` | Un solo ensamblado con las interfaces, referenciado por cliente y servidor | Elegido `STACK §3` |
| **MonoGame DesktopGL** 3.8.5.1 | Es `Game.Client` `DEP-E12` | Elegido y en uso |
| **Myra** 1.6.6 | Biblioteca de interfaz retenida sobre MonoGame | Candidato. El análisis la declara viable, recomienda no migrar las pantallas existentes y adoptarla al empezar las de sala y partida |
| **PostgreSQL** | Todo lo permanente y lo temporal en base `DEP-E7` | Elegido `DR-33` |
| **log4net** | Registro de eventos `DEP-E1`, `RES-06` | Elegido, con la verificación sobre .NET 10 sin realizar `C-3` |
| **BCrypt** | Resguardo de la credencial `DEP-E8`, factor 11 | Elegido |
| **Canal de correo saliente** | Invitación y recuperación de acceso `DEP-E9` | Tecnología sin definir. El comportamiento ante fallo sí está decidido: informa y no reintenta |

---

## 5. De concern a decisión: la autoridad sobre el estado de la partida

### 5.1 Origen

| Elemento | Contenido |
|---|---|
| **Concern** | **CON-03 · Correspondencia entre lo jugado y lo puntuado.** Que el resultado refleje exactamente las acciones legales realizadas, y solo esas. Titular STK-01, comparte STK-02. Refuerza **CON-05**, aislamiento entre partidas simultáneas |
| **Driver — escenario** | **F-1**, par (A, A). Su medida 3: *el cierre produce un solo resultado, el del servidor, idéntico para todos los clientes* |
| **Driver — restricción** | **`RES-05`**, `E 2.1`: *el cliente no decide el resultado de ninguna acción del juego*. Y `DEP-S2`: que el resultado sea único y aceptado depende de que exista un solo punto donde se determina |
| **Driver — funcionalidad** | CU-25, CU-24 y los comportamientos sin actor: cierre de turno por tiempo, cierre de ronda y fin de partida |

### 5.2 Alternativas

| Alternativa | Qué implicaría |
|---|---|
| **A · Autoridad central en el servidor** | El cliente envía intenciones; el servidor valida, aplica y publica el estado resultante |
| **B · Autoridad en el cliente del jugador en turno** | El cliente activo calcula el resultado y el servidor lo retransmite |
| **C · Cálculo en ambos lados con comparación** | El cliente calcula para responder de inmediato y el servidor confirma |

### 5.3 Comparación

| Alternativa | Efecto sobre los drivers |
|---|---|
| **A** | Satisface `RES-05` literalmente, satisface la medida 3 de F-1 y hace posible el invariante de CON-05 |
| **B** | **Viola `RES-05`.** Hace indefendible F-1: dos clientes podrían producir dos resultados y el servidor no tendría criterio para elegir |
| **C** | No lo pide ningún driver: **no hay requisitos de latencia en ninguna fuente**. Duplica la implementación de las reglas, con el coste que `ORG-01` y `ORG-05` no admiten, y empeora T-1 porque habría dos lugares donde probar la misma regla |

### 5.4 Decisión

Se selecciona la alternativa A. **La decisión está impuesta, no elegida:** `RES-05` la cierra por
escrito y `E 1` hace el estándar obligatorio y exigible por STK-03. La alternativa B queda eliminada
por la restricción, no por este análisis.

**La justificación válida es la corrección**, un solo punto de determinación `DEP-S2`, **y el
aislamiento**, CON-05. **No es una defensa frente a un jugador tramposo:** la base evaluó esa
cláusula de CON-03 y no la activó, y no hay atacante activo declarado. La fase de casos de uso
incorporó chat abierto y un régimen de reportes, que es la situación que `ASM-05` negaba; si STK-02
declara que CON-03 pasa a tocar integridad, esta decisión no cambia, pero aparecen decisiones nuevas
sobre qué se valida y qué se registra del contenido del chat.

Lo que la restricción no cubre, y esta iteración sí decide, es en qué capa vive la autoridad, con
qué granularidad se conserva el estado y cómo llega el cambio a los demás clientes.

---

## 6. Decisiones de la iteración

| Decisión | Estado | Driver que la sostiene |
|---|---|---|
| **1. Autoridad del estado de la partida en el servidor** | Impuesta | `RES-05`, F-1, `DEP-S2` |
| **2. Descomposición en cuatro módulos y un ensamblado de contratos compartido** | Impuesta en las capas, elegida en el ensamblado | `RES-05`, `E 2.2`, `DEP-E11` |
| **3. La custodia del estado vivo corresponde a `Game.Services`** | Elegida | `E 2.2` y T-1 |
| **4. El estado vivo reside en memoria del proceso servidor; la persistencia guarda fronteras de turno** | Elegida, ya vigente | CON-01 V2, `DR-18b`, `P-27`, `P-28`, `D-21` |
| **5. El dominio recibe el azar y el tiempo como colaboradores sustituibles** | Elegida en esta iteración, pendiente de ratificación por STK-02 | F-1 R1, T-1 R1, `TEN-F`, `DEP-E6` |
| **6. Los cambios no solicitados se notifican por callbacks duplex** | Elegida | `RES-02`, CU-25, CU-13, respuesta de D-1 |

### Decisión 2 · Cuatro módulos y un ensamblado de contratos

Las cuatro capas y su dirección son impuestas. El quinto elemento no lo es: el ensamblado de
contratos sobre `netstandard2.0` se justifica por `DEP-E11` —*sin él, cada cambio de contrato
desincroniza los dos lados*— y por `ORG-01`: con dos personas trabajando en paralelo, la alternativa
es negociar a mano cada cambio de interfaz. Funciona porque CoreWCF reconoce por reflexión los
atributos de `System.ServiceModel`, lo que permite que el mismo ensamblado sirva a los dos lados.

### Decisión 3 · La autoridad vive en servicios, no en el dominio

Es lo que `RES-05` deja abierto, y lo resuelve T-1. Si el dominio custodiara el estado vivo tendría
que conocer la sesión, el reloj y el canal, y la medida 2 de T-1 —*el proyecto de pruebas del
dominio no referencia ningún tipo de `Game.Services`, `Game.Persistence` ni `Game.Client`*— dejaría
de ser satisfacible. La división resultante es: **`Game.Services` custodia y coordina; `Game.Domain`
decide qué es legal y cuánto puntúa.** Es coherente con `DR-11`, que colocó `MatchSetupService` en
servicios porque lee configuración.

### Decisión 4 · Memoria para el estado vivo, base para las fronteras de turno

Dos drivers empujan en direcciones opuestas y la decisión los concilia.

Hacia la memoria: CON-07 y `P-08` —de los invitados no se guarda nada—, que impiden que sala,
anfitrión, colores, chat y ranking de sala vivan en la base sin quedar incompletos; y `DR-30`, que
exige retomar el turno en el estado exacto, incluidos los puntos de acción no gastados, que es
información de grano más fino que la que se escribe.

Hacia la base: CON-01 en su variante de caída del servidor, que exige que la partida no se destruya.

La conciliación es la granularidad: **el punto de recuperación es la frontera de turno, no la
acción.** Por eso el estado guardado no incluye reloj, puntos restantes ni construcciones
pendientes, y por eso un turno interrumpido se rejuega entero con 90 s completos `P-28`. El coste de
escritura se acota a una vez por turno a cambio de aceptar perder, como máximo, el turno en curso.

### Decisión 5 · Azar y tiempo como colaboradores del dominio

Hay dos fuentes de azar en las reglas —el sorteo del empate `R 2.3` y la obtención de cartas
`DR-06`— y una fuente de tiempo con autoridad `DEP-E6`, de la que dependen el vencimiento del turno
y las ventanas. Sin esta decisión, las refutaciones R1 de F-1 y de T-1 quedan en pie: la prueba no
puede afirmar el resultado con una aserción simple, como exige `E 10.4`.

El mecanismo no es nuevo en el proyecto: `D-30` ya establece que lo que el dominio no puede conocer
se lo entrega `Game.Services` como argumento. Esta decisión aplica la misma forma a dos
colaboradores más. **Su coste está declarado en `TEN-H`:** desacoplar cuesta, y el equipo son dos
personas con cinco papeles en catorce semanas. Se asume porque el coste de no tomarla recae sobre
las dos hojas (A, A) del árbol de utilidad.

### Decisión 6 · Notificación por callbacks duplex

`RES-02` obliga a usar callbacks duplex; esta decisión fija para qué se usan: para todo cambio que
un cliente debe conocer sin haberlo pedido —que otro jugador jugó, que el turno venció por tiempo,
que se cerró la ronda, que llegó un mensaje, que alguien entró o fue expulsado de la sala—. El
escenario D-1 lo exige: *los demás jugadores siguen viendo la partida y el reloj avanzar, sin
interrupción*.

---

## 7. Responsabilidades asignadas

### `Game.Client` — interacción y representación

- Dibujar la pantalla, leer la entrada del jugador y enviar intenciones, nunca resultados `RES-05`.
- Representar el estado que recibe del servidor, incluido el reloj: el reloj mostrado es una
  representación del reloj del servidor, no una cuenta propia con autoridad.
- Traducir los textos visibles. El idioma existe solo en el cliente.
- Trasladar los callbacks al hilo del juego: llegan en un hilo del canal y MonoGame dibuja en el
  hilo del juego.
- No contiene ninguna regla del juego.

### `Game.Services` — autoridad, coordinación y ciclo de vida

- Exponer las operaciones del contrato y validar sus argumentos.
- Custodiar el estado vivo de salas y partidas.
- Llevar el reloj del turno y aplicar su vencimiento y las ventanas de reconexión. La latencia y el
  proceso van dentro de los 90 s del jugador.
- Vigilar el latido y declarar la pérdida de conexión: ese instante inicia la ventana de reconexión
  y el marcado de inactividad.
- Leer la configuración de los catálogos y entregarla al dominio como argumentos `D-30`;
  `MatchSetupService` configura las partidas que van a empezar `DR-11`.
- Invocar al dominio para decidir legalidad y puntuación, sin implementar reglas `E 2.1`.
- Ordenar la escritura del estado en cada frontera de turno y su eliminación al terminar.
- Publicar los cambios por los callbacks duplex.
- Aplicar lo que es regla del sistema y no del juego: sanciones, inactividad en sala, cierre de
  salas al arrancar `DR-18a` y borrado del archivo de avatar `D-28`.

### `Game.Domain` — reglas y estado de la partida como concepto

- Entidades del juego y reglas que las gobiernan: legalidad de una acción, coste en puntos de
  acción, geometría de castillos y torres, puntuación de ronda y final, designación del colocador
  del rey, ganador y desempates.
- Toda la validación geométrica vive aquí: el tablero se guarda serializado y el motor de base de
  datos no lo valida `D-20`.
- No depende de ninguna otra capa `E 2.2`. No lee de persistencia, no conoce el canal y no conoce el
  reloj del sistema ni la fuente de azar del proceso: los recibe.

### `Game.Persistence` — conservación

- Leer y escribir las diez entidades permanentes o temporales y los cinco catálogos.
- Escribir y leer el documento de estado de la partida en las fronteras de turno, y eliminarlo al
  terminar.
- Resolver la consulta derivada del ranking global.
- No contiene reglas del juego `E 2.1`. El desempate lo resuelve el dominio, y por eso el puesto
  final se guarda en lugar de derivarse.

### Ensamblado de contratos — frontera del diálogo

- Declarar las operaciones, el contrato de callback y los tipos de datos que cruzan la frontera.
- Ser el único lugar donde esos tipos se declaran, referenciado por cliente y servidor.
- No contiene textos traducibles: los recursos de idioma no van aquí.

---

## 8. Estado activo y estado durable

### Estado activo — memoria del proceso servidor

| Elemento | Por qué no puede estar en la base |
|---|---|
| Sala viva: lista ordenada de participantes, anfitrión, colores en uso, chat y ranking de sala | Incluye invitados, de los que no se guarda ninguna fila `P-08`. Una tabla que solo contuviera cuentas nunca sabría si una sala está vacía, que es la condición que decide si la sala sigue existiendo |
| Invitado: alias e identificador temporal, que es su pase de asiento | La misma razón. El cliente lo guarda en memoria y no en disco: un archivo local le daría identidad persistente, contra CON-07 |
| Mensajes de chat | Está decidido no persistirlos; ventana deslizante de 200 por sala |
| Estado de conexión, reloj del turno y puntos de acción restantes | Cambian dentro del turno. Guardarlos obligaría a escribir por acción y no por frontera de turno, y `P-28` decide lo contrario: el turno interrumpido se rejuega entero |
| Tablero, castillos, caballeros, rey, cartas obtenidas y usadas, puntuación acumulada | Es el estado autoritativo vivo. Se refleja en la base en cada frontera de turno, pero la autoridad es la memoria |

### Estado durable — base de datos

| Elemento | Por qué debe sobrevivir |
|---|---|
| Cuenta, amistades y sanciones | Identidad persistente: de ella dependen ranking global, historial, atribución y régimen de convivencia `DEP-S4` |
| Sala y sus invitaciones pendientes | La sala es la referencia de sus partidas; se cierra en lugar de borrarse `D-23` |
| Partida y participaciones con puesto y puntos | CON-04: el resultado debe seguir disponible para ranking e historial. El puesto final se guarda, no se deriva |
| Reportes con copia del mensaje | El chat no se conserva: sin la copia el reporte quedaría sin sustento |
| Código de recuperación | Vale cinco minutos y es la prueba de titularidad de CON-10 |
| Catálogos de parámetros | Son el mecanismo de M-1: cambiar una fila sin tocar el dominio ni recompilar `D-30`, `DR-17` |
| Documento de estado de la partida en curso | Es lo que hace posible la reanudación tras una caída del servidor |

### Coste registrado de esta separación

`TEN-G`, abierta. `DR-10` no limita el número de partidas simultáneas, el estado vivo está en
memoria y `P-31` retiene las partidas caídas tres minutos esperando jugadores. Sin límite, con
estado en memoria y con sesiones esperando, hay una presión de memoria que nadie ha examinado, y se
agrava con `ORG-04`, porque el servidor corre en el portátil de cada integrante.

---

## 9. Estructura resultante

### 9.1 Vista de módulos

```
                 ┌──────────────────────────────────────┐
                 │            Game.Client                │  intención y representación
                 │  MonoGame DesktopGL · Myra candidata  │
                 └───────────────┬──────────────────────┘
                                 │ usa
                 ┌───────────────▼──────────────────────┐
                 │   Ensamblado de contratos compartido  │  netstandard2.0
                 │   operaciones · callback · tipos      │
                 └───────────────▲──────────────────────┘
                                 │ implementa
                 ┌───────────────┴──────────────────────┐
                 │           Game.Services               │  autoridad del estado vivo
                 │  sesión · reloj · latido · publicación │  MatchSetupService
                 └──────┬────────────────────┬──────────┘
                        │ invoca             │ ordena leer y escribir
          ┌─────────────▼──────────┐  ┌──────▼──────────────────┐
          │      Game.Domain        │  │    Game.Persistence      │
          │  reglas · puntuación ·  │  │  entidades · catálogos · │
          │  geometría · desempate  │  │  documento de estado     │
          │  no depende de nadie    │  └──────┬──────────────────┘
          └─────────────────────────┘         │
                                              ▼
                                        PostgreSQL, fuera de la frontera
```

**`Game.Domain` no apunta a `Game.Persistence`** `E 2.2`. Cuando el dominio necesita un dato que
solo la persistencia sabe obtener, servicios lo lee y se lo entrega como argumento.

### 9.2 Correspondencia con la solución C#

| Módulo | Proyecto | Estado |
|---|---|---|
| `Game.Client` | `src/Torres.Client` | Existe. Diez pantallas, internacionalización, MonoGame DesktopGL 3.8.5.1 |
| `Game.Services` | — | Por crear. Aloja el host de CoreWCF |
| `Game.Domain` | — | Por crear. Sin referencias a ningún otro proyecto |
| `Game.Persistence` | — | Por crear |
| Contratos compartidos | — | Por crear, en `netstandard2.0`; el resto en `net10.0` `RES-08` |
| Proyecto de pruebas del dominio | — | Por crear. Es artefacto de T-1 |

El estándar nombra las capas `Game.*` y el proyecto existente usa el espacio de nombres
`Torres.Client`. Son dos fuentes del proyecto que no coinciden, y el criterio de nombrado lo cierra
STK-02.

WPF no aparece en esta estructura: `STACK §4` lo descarta por existir solo en Windows, contra
`ORG-03`. El cliente es MonoGame DesktopGL `DEP-E12`.

---

## 10. Revisión del resultado

| Driver | Cobertura |
|---|---|
| **F-1** corrección funcional (A, A) | **Parcial.** La decisión 1 da el punto único de determinación y la decisión 5 hace reproducible el azar. Falta el aislamiento entre partidas, que es CON-05 |
| **T-1** testabilidad (A, A) | **Cubierto estructuralmente.** La decisión 3 mantiene el dominio libre de sesión, reloj y canal; la decisión 5 retira las dos fuentes de azar |
| **D-1** disponibilidad (A, M) | **Parcial.** El estado del turno está fuera del cliente y el reloj es del servidor. El instante que la medida usa depende del latido, cuyas cifras no están fijadas |
| **M-1** modificabilidad (A, B) | **Cubierto.** Resuelto por `D-30`; la estructura lo sostiene |
| **L-1** analizabilidad (M, B) | **Cubierto, sin efecto estructural.** El registro por clase es disciplina de código |
| CON-01, caída del servidor | **Cubierto** por la decisión 4 |
| CON-05 aislamiento | **No cubierto** |
| CON-06 y CON-10, seguridad | **No cubierto.** Dependen del conflicto `C-2`, que STK-02 dejó sin definir |
| Funcionalidad primaria | Los siete casos de uso seleccionados tienen módulo y responsabilidad asignados; no tienen operaciones del contrato asignadas |
| `ORG-04`, correr todo en la propia máquina | **No cubierto.** Es una cuestión de despliegue |

El objetivo de la iteración se alcanza: existe una estructura general con responsabilidades
asignadas y con el estado clasificado en activo y durable.
