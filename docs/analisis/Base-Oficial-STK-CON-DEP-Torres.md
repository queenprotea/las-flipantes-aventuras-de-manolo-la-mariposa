# Base oficial — Stakeholders, Concerns y Dependencias · Juego de Torres

**Versión 4.1 · 4 sep 2026.** Documento normativo del proyecto. **Estos identificadores y estas definiciones son los únicos válidos.** Los análisis previos siguen valiendo como argumentación y evidencia, pero no como registro de identificadores.

**Cómo leer este documento.** Las secciones 1 a 10 describen **el estado vigente**: lo que dicen es lo que es hoy. No hay estados antiguos mezclados. El historial —de dónde salió cada cosa y qué se corrigió— está en los anexos A y B, al final.

**Qué NO contiene.** Escenarios ni árbol de utilidad. Los escenarios se construyen sobre esta base.

---

## 0. Fuentes y regla de precedencia

| Documento | Fecha | Papel |
|---|---|---|
| `Reglas_del_Juego_de_Torres.docx` v3.0, **Borrador** | 29 ago 2026 | Fuente normativa del dominio · `R` |
| `Estandar-de-codificacion-v7.docx` | 20 ago 2026 | Fuente normativa de la forma del código · `E` |
| `Analisis-Arquitectonico-Torres.md` | 1 sep | Evidencia · origen de `Q-xx` |
| `Analisis-Stakeholders-Concerns-Torres.md` | 1 sep | Evidencia · origen de `CON-xx`, `DEP-xx`, `TEN-x`, `DA-xx` |
| `Analisis-Persistencia-Torres.md` v4 | 2 sep | Evidencia · origen de `P-xx` y `D-xx` |
| `code/database/` | 2 sep | Artefactos vigentes: `schema.sql`, `seed-configuration.sql`, `Modelo-ER-Torres.md` |
| `STACK.md` | 4 sep | **Fuente normativa del marco técnico** · origen de `RES-`, `ORG-` y de `DEP-E11`…`DEP-E13` |
| **Respuestas de STK-02** | 4 sep | Origen de `DR-xx` |

**Regla de precedencia:** el esquema de identificadores en vigor es el que ya cita el documento más reciente. Es la única elección que no invalida los artefactos ya ejecutados.

**Prefijos en vigor**

| Prefijo | Qué designa |
|---|---|
| `STK-` | Stakeholder |
| `CON-` | Concern |
| `DEP-E/I/S/D/P` | Dependencia externa · dominio · sistema · entre decisiones · del proyecto |
| `TEN-` (letras) | Tensión |
| `DR-` | Decisión ratificada por STK-02 el 4 sep |
| `RES-` | Restricción: no se negocia |
| `ORG-` | Condición organizacional |
| `OBJ-` | Objetivo de negocio |
| `ASM-` | Supuesto |
| `D-` | Decisión de persistencia (Análisis 3) |
| `DA-` | Decisión arquitectónica (Análisis 2) |
| `P-` | Pregunta al equipo, con respuesta registrada |
| `Q-` | Cuestión de análisis |

**Un identificador nunca se reutiliza con otro significado.** Si un elemento muere, su ID queda retirado, no reasignado. La traducción desde los documentos antiguos está en el **anexo A**.

---

## 1. Stakeholders

**Criterio.** Un stakeholder **introduce concerns** sobre el sistema, su construcción, operación o evaluación. Interactuar no basta (eso es un *actor*); ser nombrado por las reglas tampoco (eso puede ser un objeto de dominio). Se distingue **relevancia** de **autoridad**.

### STK-01 · Jugador

| Campo | Contenido |
|---|---|
| **Qué representa** | La persona que participa en una partida. Único sujeto cuyas decisiones alteran el estado del juego. De 2 a 4 por partida `R 1.1` |
| **Relevancia** | Alta. Todas las interacciones del sistema existen para él |
| **Autoridad** | Ninguna |
| **Concerns que introduce** | CON-01, CON-03, CON-04, CON-06, CON-07, CON-10 |

**Estados y roles** — un solo stakeholder, agrupados por eje para que un escenario cite exactamente uno:

| Eje | Valores |
|---|---|
| Identidad | Con cuenta · Invitado |
| Conexión | Conectado · Desconectado dentro de la ventana · Retirado *(terminal)* |
| Posición en la partida | Jugador activo *(turno propio)* · En espera |
| Rol temporal | Colocador del rey en ronda 1 · Colocador del rey en rondas 2–3 |
| Fuera de partida | En sala · Anfitrión *(puede ser invitado)* · Con invitación pendiente · Con amistad aceptada o solicitud pendiente |

Los roles temporales **no son stakeholders distintos** y los estados de conexión **no son roles**.

**Diferencias funcionales entre cuenta e invitado** — cerradas:

| | Con cuenta | Invitado |
|---|---|---|
| Se guarda algo de él | Sí | **Nada** `P-08` |
| Ranking global | Sí | **No** `P-40` |
| Ranking de sala | Sí | Sí `P-40` |
| Crea sala y es anfitrión | Sí | Sí `P-41` |
| Puede ser invitado a una sala | Sí | **No** — la invitación va siempre a una cuenta `P-22` |
| Vuelve tras una caída del servidor | Sí | **No** `DR-21` |

### STK-02 · Equipo de desarrollo

| Campo | Contenido |
|---|---|
| **Qué representa** | Los dos integrantes nombrados en el estándar. `C` le atribuye además las reglas, el diseño, las pruebas y la operación del servidor |
| **Relevancia** | Alta y transversal |
| **Autoridad** | **Máxima dentro del proyecto.** Único que puede cerrar cuestiones y ratificar propuestas |
| **Concerns que introduce** | CON-02, CON-05, CON-08, CON-09; comparte CON-03, CON-04, CON-06, CON-10 |

**Cinco sombreros, un solo stakeholder:** autor y revisor de las reglas · diseñador y desarrollador · probador · operador del servidor · responsable ante la entrega.

**Consecuencia arquitectónica.** Un cambio de regla **no requiere negociación externa** — abarata la variante «cambio de regla» de CON-02. El precio: **no hay contraparte que valide una regla ambigua desde fuera**, y por eso el proyecto acumula preguntas que solo puede cerrar él mismo.

### STK-03 · Receptor de la entrega

| Campo | Contenido |
|---|---|
| **Qué representa** | Quien recibe y evalúa el resultado. `R` portada exige completar los campos «antes de la entrega» |
| **Relevancia** | Media documentada; **posiblemente alta** — ver Q-23 |
| **Autoridad** | Sobre la **aceptación**, no sobre el diseño |
| **Concerns que introduce** | Conformidad con `E` y con `R`. Ningún otro puede atribuírsele |
| **Restricciones que impone** | `RES-01` C# · `RES-02` WCF duplex sobre net.tcp · `RES-03` prohibición del stack web · `RES-04` se evalúa el código, no el acabado visual — **confirmado por `STACK §2`** |

**Por qué se conserva pese a no estar identificado.** Es lo que hace **obligatorias, y no opcionales**, las restricciones de capas, nombrado, registro y pruebas. Sin él, `E` sería una guía de estilo y `DEP-E5` no sería una dependencia.

**Lo que cambió con `STACK.md`.** STK-03 **sí emite exigencias**: cuatro restricciones tecnológicas impuestas y un criterio de evaluación —«se evalúa el código, no el acabado visual», `OBJ-01`—. Su relevancia sube de *media documentada* a **real y verificable**.

**Lo que sigue igual:** impone restricciones pero **no ha dado umbrales**. Ningún número aceptable procede de él, así que `DR-29` se mantiene: los umbrales los fija STK-02 (`DEP-P4`).

### Descartados

| Considerado | Motivo |
|---|---|
| Rey, caballeros, cartas, castillos, tablero | Objetos de dominio. No tienen intereses |
| Servidor, cliente, manejador de partidas | Artefactos del sistema. Aparecen como *artefacto* en los escenarios; duplicarlos sería un error de categoría |
| Espectador | Ninguna mención en ninguna fuente; declarado fuera de alcance |
| Árbitro o administrador de partida | Las reglas resuelven automáticamente todos los casos, incluido el empate y el fin de turno |
| Operador de infraestructura | Es el propio equipo (anexo B del Análisis 3) → STK-02 |
| **Administrador del juego** | `D-31`: **no se modela** mientras no se defina qué puede hacer. Si se define, entra como **STK-04** |

---

## 2. Concerns

**Criterio.** Un concern nombra **aquello sobre lo que hay que poder responder**, sin fijar la respuesta de diseño. La clasificación en atributo de calidad **no se fuerza**: dos concerns siguen sin ella a propósito.

**Catálogo de referencia:** **ISO/IEC 25010**, adoptado por `DR-16`. Los siete nombres de atributo en uso pertenecen a su vocabulario, así que la clasificación de cada concern —y la afirmación de que dos **no** tienen atributo— es verificable contra una lista acordada.

### 2.1 Cuadro vigente

| ID | Concern | Atributo | Stakeholder titular | ¿Listo para escenario? |
|---|---|---|---|---|
| **CON-01** | Continuidad ante desconexión del jugador **o** caída del servidor | Disponibilidad | STK-01 | **Sí** — las dos variantes |
| **CON-02** | Ajuste de reglas y parámetros sin rehacer el sistema | Modificabilidad | STK-02 | **Sí** |
| **CON-03** | Correspondencia entre lo jugado y lo puntuado | Corrección funcional | STK-01 | **Sí** |
| **CON-04** | Conservación del resultado de las partidas | **— sin atributo** | STK-01 con cuenta | No aplica: es funcional |
| **CON-05** | Aislamiento entre partidas simultáneas | Corrección funcional bajo concurrencia | STK-02 | **Sí**, con medida de invariante |
| **CON-06** | Qué información personal se guarda y se registra | Seguridad — confidencialidad | STK-01 | **Sí** |
| **CON-07** | Jugar con cuenta o como invitado sin perder atribución | **— sin atributo** | STK-01 | No aplica: es funcional |
| **CON-08** | Poder comprobar que una regla hace lo que dice | Testabilidad | STK-02 | **Sí** |
| **CON-09** | Poder reconstruir qué ocurrió cuando algo falla | Analizabilidad | STK-02 | **Sí** |
| **CON-10** | Que solo el titular pueda actuar como su cuenta | Seguridad — autenticidad | STK-01 | **Sí** |

**Observación estructural.** CON-02, CON-08 y CON-09 son los tres atributos de la mantenibilidad y los tres provienen del **mismo stakeholder**, STK-02. Por eso se refuerzan en lugar de competir.

### 2.2 Fichas

#### CON-01 · Continuidad ante una desconexión o una caída

| | |
|---|---|
| **Qué preocupa** | Que un jugador que pierde la conexión pueda retomar la partida, que la partida siga siendo jugable para los demás, y que una caída del servidor no destruya la partida |
| **Stakeholder** | STK-01 · STK-02 (autor de reglas) |
| **Atributo** | **Disponibilidad** |
| **Evidencia** | `R 5.1`, `R 5.2`, `C`, `P-27`…`P-32`, `P-37`, `DR-13`, `DR-20`, `DR-21` |

**V1 · Cae el jugador.** Ventana de reconexión: el **tiempo restante de su turno** si era su turno, **90 s** si era turno ajeno. **El reloj del turno sigue corriendo** durante la desconexión `DR-20`. Si vuelve dentro de la ventana, **retoma su turno en el estado exacto en que lo dejó** —PA no gastados y acciones ya confirmadas— porque el servidor es la única fuente de autoridad del estado `DR-30`. Al vencer la ventana, **retiro** de la partida. Si el retiro deja menos de 2 jugadores, la partida termina y gana el que queda. La partida continúa con la **configuración del número inicial** de jugadores `DR-09`.

**V2 · Cae el servidor.** El estado se escribe en **cada frontera de turno** `P-27`. Al arrancar, el servidor busca las partidas en curso y las reanuda cuando han vuelto **al menos 2 jugadores con cuenta** `P-30`+`DR-21`. El turno interrumpido **se repite con 90 s completos** `P-28`. Los 90 s de `R 5.2` **no corren** mientras el servidor está caído `DR-13`. Plazo de espera: **3 min desde el arranque** `P-31`, configuración de aplicación `D-27`. Vuelve uno solo → *abandonada*, gana él; no vuelve nadie → *interrumpida*, sin puestos ni puntuaciones `P-37`. **Las salas no se reanudan** `P-32`.

**Cota derivada de `DR-01`:** una partida dura como máximo **60 min** (4 jugadores). Acota qué puede significar «mientras la partida siga activa» en cualquier medida.

**Residual:** ninguno bloqueante.

#### CON-02 · Ajuste de reglas y parámetros

| | |
|---|---|
| **Qué preocupa** | Que los valores que las reglas declaran variables, y los cambios de versión del documento, se apliquen con un coste acotado |
| **Stakeholder** | STK-02 (autor de reglas, mantenedor) |
| **Atributo** | **Modificabilidad** — el asunto es el **coste del cambio**, no que el sistema funcione |
| **Evidencia** | `R 1.3`, `R 3.2`, `R` portada v3.0 borrador, `D-30`, `seed-configuration.sql` |

**Delimitación oficial de lo configurable:**

| Configurable | No configurable |
|---|---|
| Turnos por jugador y ronda `R 1.3` | Tablero 8×8 y su distribución `R 1.2` |
| Construcciones por jugador y turno `R 1.3` | 3 rondas `R 1.3` |
| Costes de acción en PA `R 3.2` | Catálogo de 8 cartas `R 2.4` |
| Plazo de reanudación `D-27` | 5 caballeros por jugador `R 2.1` |
| Factor de coste de BCrypt `P-46` | Los 90 s de turno `R 5.1` |
| | Altura máxima de torre: nivel 3 `DR-03` |

**Dónde vive** `D-30`: tres tablas de solo lectura en la base de datos, **leídas por `Game.Services` y pasadas al dominio como argumentos**, porque el dominio no lee de persistencia `E 2.2`.

**Artefacto** `DR-11`: el «manejador de partidas» es el componente que **configura las partidas que van a empezar**. Como lee configuración, pertenece a **`Game.Services`**. Nombre adoptado: **`MatchSetupService`** — en inglés, según `E 1`.

**Medida** `DR-17`: cambiar un valor de `R 1.3` o `R 3.2` **no debe requerir tocar `Game.Domain` ni recompilar**: se cambia una fila y la siguiente partida la usa.

#### CON-03 · Correspondencia entre lo jugado y lo puntuado

| | |
|---|---|
| **Qué preocupa** | Que el resultado refleje exactamente las acciones legales realizadas, y solo esas |
| **Stakeholder** | STK-01 · STK-02 |
| **Atributo** | **Corrección funcional** — es corrección, no seguridad |
| **Evidencia** | `E 2.1`, `R 4.1`–`R 4.3`, `R 2.3`, `DR-02`, `DR-08`, `DR-22` |

**Cláusula condicional — evaluada y no activada.** Pasaría a tocar *integridad* si el equipo declarase que se juega entre desconocidos y que la manipulación es un riesgo a mitigar. Hoy: las invitaciones solo van a cuentas `P-22`, no hay salas públicas listadas, y ninguna fuente menciona jugadores adversarios.

**Nota de alcance.** En este dominio **no existe un concern de protección (*safety*)**: ninguna decisión del sistema puede causar daño a una persona.

**Reglas que lo sostienen, ya cerradas.** El corte de ronda no retira nada del tablero `DR-23`; la puntuación se calcula al final de cada ronda y al final de la partida `DR-02`; el orden de colocación inicial es el puesto de mesa `DR-24`; el jugador retirado deja de contar para la puntuación pero sus construcciones permanecen `DR-25`; el ganador y los desempates están definidos `DR-15`.

**Sin residuales.** `DR-32` cierra el último: los caballeros del retirado **se quitan del tablero**, y las torres que ocupaban quedan libres.

#### CON-04 · Conservación del resultado

| | |
|---|---|
| **Qué preocupa** | Que el resultado de una partida terminada siga disponible para el ranking y el historial |
| **Stakeholder** | STK-01 con cuenta · STK-02 |
| **Atributo** | **Ninguno** |
| **Evidencia** | `P-05`, `D-05`, `D-16`, `D-17`, `D-21`, `D-25`, `P-37` |

**Qué se conserva:** de cada partida terminada, **puesto y puntos** de cada jugador con cuenta, y cuántos jugaron. **Qué no:** secuencia de jugadas, desglose por ronda, sesiones y tokens, chat, espectadores.

**Por qué no se clasifica.** El único candidato era una tolerancia declarada a la pérdida del resultado, y el proyecto declaró lo contrario: `P-37` acepta que una partida *interrumpida* **no produzca resultado alguno**, sin exigir nada a cambio. Clasificarlo sería inventar una exigencia.

#### CON-05 · Aislamiento entre partidas simultáneas

| | |
|---|---|
| **Qué preocupa** | Que lo que ocurre en una partida no altere el estado ni el resultado de otra |
| **Stakeholder** | STK-02 (operación) · STK-01 |
| **Atributo** | **Corrección funcional bajo concurrencia** |
| **Evidencia** | `C`, `R 5.1`, `DR-10`, `D-26`, `D-29` |

**No es escalabilidad ni rendimiento.** `DR-10` decide **no limitar** el número de partidas. «Sin límite» es una aspiración de diseño, **no una medida**.

**Forma de la medida:** un **invariante**, no una cifra de capacidad — *«con **5** partidas en curso, ninguna operación de una altera el estado, el reloj ni el resultado de otra»* `DR-31`. **Las 5 no son un límite del sistema**: `DR-10` no limita nada. Son la cifra con la que se verifica, elegida para no saturar al equipo en pruebas.

**Superficie sobre la que hay que demostrarlo:** no solo el estado en base de datos, también el estado **en memoria del servidor** — miembros de sala, anfitrión y ranking de sala viven ahí `D-26`, `D-29`.

#### CON-06 · Qué información personal se guarda y se registra

| | |
|---|---|
| **Qué preocupa** | Que la información personal que el sistema conserva y escribe en sus registros se mantenga en el mínimo necesario |
| **Stakeholder** | STK-01 · STK-02 |
| **Atributo** | **Seguridad — confidencialidad** |
| **Evidencia** | `E 9.4`, `D-03`, `D-13`, `D-22`, `P-22`, `P-43` |

| Dato | Decisión |
|---|---|
| Correo | **Único dato personal obligatorio**; recuperación de acceso y destino de invitaciones `D-13` |
| Contraseña | **Nunca se guarda**; solo su hash `D-03` |
| Avatar | Archivo en disco del servidor, máx. 5 MB, PNG/JPG; la base guarda solo la referencia `P-43` |
| Identificador interno | **Lo único que puede aparecer en el registro de eventos** |
| Del invitado | **Nada** `P-08` |
| Correo de terceros | Nunca: la invitación va siempre a una cuenta existente `P-22` |

**Atención — `RES-09`.** El transporte usa `SecurityMode.None`, luego **nada de lo que viaja entre cliente y servidor va cifrado**: ni la credencial, ni el correo, ni el estado de la partida. La confidencialidad está garantizada **en reposo y en el registro**, no en tránsito. Ver **C-2** en §8.

**Residual, no bloqueante:** dónde deja el servidor los archivos de avatar y con qué nombre. Y `D-28` — borrar la cuenta borra sus filas pero **no** borra el archivo: obligación de `Game.Services`, no garantía de la base.

#### CON-07 · Jugar con cuenta o como invitado

| | |
|---|---|
| **Qué preocupa** | Que la forma de identificarse no impida jugar y determine con claridad qué resultados se atribuyen a quién |
| **Stakeholder** | STK-01 |
| **Atributo** | **Ninguno** |
| **Evidencia** | `C`, `P-08`, `P-22`, `P-40`, `P-41`, `DR-21` |

**Por qué no se clasifica.** Es una decisión sobre qué hace el sistema y para quién. Forzarlo hacia usabilidad exigiría que STK-02 declarase «jugar sin registro» como **objetivo**; ninguna fuente lo hace.

**Consecuencia sobre CON-01:** con `DR-21`, un invitado **no vuelve** tras una caída, luego «2 jugadores para reanudar» significa **2 con cuenta**. Una partida con menos de 2 jugadores con cuenta termina siempre como *interrumpida*.

#### CON-08 · Poder comprobar que una regla hace lo que dice

| | |
|---|---|
| **Qué preocupa** | Que el comportamiento de las reglas pueda verificarse sin depender del servidor ni de la interfaz |
| **Stakeholder** | STK-02 (probador) |
| **Atributo** | **Testabilidad** |
| **Evidencia** | `E 2.2` propósito declarado · `E 10.1`–`E 10.5` régimen de pruebas |

**Es el caso mejor sustentado del proyecto: no hay que inferirlo.** `E 2.2` declara literalmente que la dirección de dependencias existe «para que las reglas puedan probarse sin levantar el servidor ni abrir la ventana del juego».

**Refuerzo nuevo:** `D-20` guarda el tablero serializado y el motor no lo valida, luego **toda la validación geométrica vive en `Game.Domain`** — justo la capa probable.

**Atención a TEN-F** si el escenario toca el azar: con `DR-06` hay ya dos fuentes de aleatoriedad.

#### CON-09 · Poder reconstruir qué ocurrió cuando algo falla

| | |
|---|---|
| **Qué preocupa** | Que ante un fallo se pueda saber qué operación falló, sobre qué entidad y en qué punto |
| **Stakeholder** | STK-02 (operación) |
| **Atributo** | **Analizabilidad** |
| **Evidencia** | `E 9.1`, `E 9.3`, `E 9.5`, `C` |

Registro por clase, excepción pasada como argumento para conservar tipo y traza, y un solo registro por fallo. **Evitar tener que reproducir el fallo es la definición operativa del atributo.**

**Limitación declarada:** **no hay registro de auditoría** (fuera de alcance). La analizabilidad se apoya solo en el registro de eventos de `E 9`.

#### CON-10 · Que solo el titular pueda actuar como su cuenta

| | |
|---|---|
| **Qué preocupa** | Que solo el titular pueda actuar como su cuenta, y que la vía de recuperación de acceso no se convierta en la vía de suplantación |
| **Stakeholder** | STK-01 (titular) · STK-02 |
| **Atributo** | **Seguridad — autenticidad** |
| **Por qué corresponde** | El proyecto ya tomó **cuatro decisiones cuyo único motivo es esta cualidad**. Una decisión de estructura tomada en nombre de una cualidad es un atributo de calidad en juego — el mismo criterio que sostuvo CON-08 |
| **Evidencia** | `D-03` hash BCrypt · `P-46` factor configurable · `D-13` recuperación por correo · `D-24` enlace de un solo uso · `P-42` no se borra una cuenta con partida en curso |
| **Sí cubre** | La credencial en reposo, la recuperación de acceso por correo y el uso único del enlace de invitación |
| **No cubre** | No hay atacante activo declarado y **no hay control de acceso por roles** (`D-31`). **Sobre el cifrado en tránsito sí hay decisión, y es que no lo hay**: `RES-09` fija `SecurityMode.None`, de modo que la credencial viaja sin protección aunque se guarde con BCrypt. Ver **C-2** en §8 |
| **Relación** | Descarga a CON-06 de la mitad «credenciales», que queda limpio como confidencialidad de datos personales |
| **Residual** | Si el enlace de recuperación caduca, y en cuánto. `D-24` fija el uso único de la invitación; de la recuperación no dice nada |

---

## 3. Dependencias

**Criterio.** Hay dependencia cuando un elemento **no puede determinarse, calcularse o existir sin el otro**: cambiar el primero cambia el segundo. Interactuar no basta. Las relaciones que no cumplen el criterio están en §3.6 para que no vuelvan a proponerse.

### 3.1 DEP-E · Externas

| ID | Qué depende | De qué depende | Por qué | Evidencia |
|---|---|---|---|---|
| **DEP-E1** | El registro de eventos de todo el sistema | **log4net** e `ILog` | Único medio permitido: «no se escribe en la consola ni en un archivo por otros medios» | `E 9.1` |
| **DEP-E2** | El manejo de errores | Los **tipos de excepción del framework** | Obliga a usar los predefinidos «siempre que apliquen» | `E 8.7` |
| **DEP-E3** | La continuidad de la participación | La **conexión de red** cliente–servidor | Es la fuente del fallo que las reglas de reconexión atienden | `R 5.2` |
| **DEP-E4** | Las reglas implementadas | El **documento de reglas**, v3.0 **Borrador** | Cada versión suya redefine el comportamiento correcto del dominio | `R` portada |
| **DEP-E5** | Capas, nombrado, registro y pruebas | El **estándar de codificación** | Obligatorio en construcción, revisión y mantenimiento | `E 1` |
| **DEP-E6** | El vencimiento del turno y las ventanas | Una **fuente de tiempo con autoridad** | Una consecuencia irreversible depende de un plazo medido | `R 5.1`, `R 5.2` |
| **DEP-E7** | Todo lo permanente y lo temporal en base | **PostgreSQL** — confirmado por STK-02 el 4 sep `DR-33`. `schema.sql` sigue vigente sin cambios | Análisis 3 §13, `DR-33` |
| **DEP-E8** | El resguardo de la credencial | **BCrypt** | Cambiarlo obliga a rehacer todos los hashes | `D-03`, `P-46` |
| **DEP-E9** | Invitación por correo y recuperación de acceso | Un **canal de correo saliente** | Sin canal, ambas funciones no existen | `N2`, `P-20`, `D-13` |
| **DEP-E10** | El avatar del jugador | El **sistema de archivos del servidor** | La base **no puede tocar el disco**: solo guarda la referencia | `P-43` |
| **DEP-E11** | **Todo el transporte cliente–servidor** | **CoreWCF** con `NetTcpBinding` duplex y `System.ServiceModel` | Materializa `RES-02`. Cliente y servidor comparten **un solo ensamblado de contratos** sobre `netstandard2.0`; sin él, cada cambio de contrato desincroniza los dos lados | `STACK §1`, `§3` |
| **DEP-E12** | La presentación del cliente | **MonoGame DesktopGL** (OpenGL + SDL2) | Es `Game.Client`. `DesktopGL` es la única variante que corre en los tres sistemas `RES-08` | `STACK §1`, `§3` |
| **DEP-E13** | La compilación y ejecución de todo el sistema | **.NET 10** (LTS) | `.NET Framework` solo existe en Windows y excluiría a todo el equipo `ORG-03` | `STACK §3` |

**DEP-E4 es la más consecuente del proyecto:** el elemento del que depende el núcleo **todavía no está aprobado**. Sostiene CON-02 y hace de TEN-E una tensión real.

**Marco de pruebas — no registrado.** Los ejemplos del estándar usan `[Fact]`, pero el documento **no nombra ningún marco** y sus ejemplos pertenecen a otro juego. Pendiente de confirmar con STK-02.

### 3.2 DEP-I · Internas del dominio

| ID | Qué depende | De qué depende | Evidencia |
|---|---|---|---|
| **DEP-I1** | Turnos de cada jugador por ronda y construcciones que recibe | Cantidad de jugadores, ronda y turno | `R 1.3` — **la única dependencia que el documento nombra con esa palabra** |
| **DEP-I2** | Puntuación que un castillo otorga | Superficie del castillo, nivel de la torre del caballero, y que sea **el único** caballero propio en él | `R 4.1` |
| **DEP-I3** | Superficie de un castillo | Qué torres lo forman en ese momento — **crece a lo largo de toda la partida**, porque el corte de ronda no retira nada `DR-23` | `R 2.2`, `DR-08`, `DR-23` |
| **DEP-I4** | Legalidad de usar las cartas 5 y 8 | Estado geométrico del castillo afectado; la operación no puede partirlo **ni unir dos castillos** | `R 2.4`, `DR-07` |
| **DEP-I5** | Puntos de acción disponibles en un turno | Si usó la carta 1 o la 2 en ese turno — **no pueden usarse las dos a la vez**, y pueden usarse **en cualquier momento del turno** `DR-26` | `R 3.1`, `R 2.4`, `C`, `DR-26` |
| **DEP-I6** | Quién coloca el rey en rondas 2 y 3 | Puntuación parcial de **todos** los jugadores | `R 2.3` |
| **DEP-I7** | Que exista esa puntuación parcial | El corte de fin de ronda | `DR-02` |
| **DEP-I8** | Puntos que el rey otorga | Caballero en el mismo castillo y mismo nivel que el rey, y la ronda en curso (5/10/15) | `R 4.2` |
| **DEP-I9** | Puntuación final de un jugador | Cartas obtenidas y **no** utilizadas | `R 4.3` |
| **DEP-I10** | Duración de la ventana de reconexión | De quién era el turno en el instante de la caída | `R 5.2`, `DR-20` |
| **DEP-I11** | Que la partida termine anticipadamente | Cuántos jugadores quedan, que depende de los retiros | `C`, `R 5.2` |
| **DEP-I12** | El movimiento de un caballero | Que origen y destino estén **en el mismo castillo** | `DR-22` |

**Cadena más larga:** `DEP-I6 → DEP-I7 → DEP-I2 → DEP-I3`. Quién coloca el rey depende de la puntuación parcial, que depende del corte de ronda, que depende de la puntuación de cada castillo, que depende de qué torres lo componen. **Un cambio en cualquier eslabón llega hasta la designación del colocador del rey.**

**Dependencia secundaria de DEP-I6:** el empate se resuelve **al azar** `R 2.3` → depende de una fuente de aleatoriedad única y autoritativa. Con `DR-06` (cartas al azar) son **dos** fuentes. Ver TEN-F.

**Efecto de `DR-22` sobre `DEP-I2`:** al no migrar los caballeros entre castillos, la condición de «un único caballero propio» deja de ser una coincidencia geométrica y pasa a ser estructural. La identidad del castillo es estable durante la ronda y la puntuación es calculable sin ambigüedad.

### 3.3 DEP-S · Internas del sistema

| ID | Qué depende | De qué depende | Evidencia |
|---|---|---|---|
| **DEP-S1** | Dónde puede leerse la configuración de partida | La dirección de dependencias entre capas | `E 2.2` — resuelta por `D-30` y `DR-11` |
| **DEP-S2** | Que el resultado de una acción sea único y aceptado | Que exista un solo punto donde se determina | `E 2.1` |
| **DEP-S3** | El ranking histórico | Que la partida termine **con puestos y puntuaciones asignados** | `P-37` |
| **DEP-S4** | Que un resultado pueda atribuirse en el ranking global | Que el jugador tenga identidad persistente | `C`, `P-40` |
| **DEP-S5** | Poder diagnosticar un fallo sin reproducirlo | Que el mensaje lleve la operación y el identificador de la entidad | `E 9.3` |
| **DEP-S6** | Poder ejercitar las reglas sin levantar el servidor | Que el dominio no dependa de ninguna otra capa | `E 2.2` |
| **DEP-S8** | Que la sala y su ranking existan | Que el **servidor siga en pie** | `D-26`, `D-29`, `P-32` |

**`DEP-S3` — atención al enunciado.** No es «toda partida produce un resultado». Una partida *interrumpida* se cierra **sin puestos ni puntuaciones** `P-37` y no cuenta para ningún ranking. El ranking depende de que se alcance un desenlace **con puestos**: fin normal o fin *abandonada*.

**`DEP-S8` — dos regímenes de durabilidad.** La partida sobrevive a la caída; la sala y su ranking, no. Es diseño decidido, no tensión, pero condiciona cualquier escenario que hable de salas.

**`DEP-S7` — retirada.** Registraba *reanudación ← identidad del invitado*. `DR-21` decide que los invitados no vuelven: no hay mecanismo y no lo habrá. **El ID queda retirado, no se reasigna.**

### 3.4 DEP-D · Entre decisiones ya tomadas

| ID | Decisión | Depende de | Por qué |
|---|---|---|---|
| **DEP-D1** | Fin de partida con menos de 2 jugadores | Ventanas de reconexión y retiro `R 5.2` | El retiro es el mecanismo que puede llevarla por debajo de 2 |
| **DEP-D2** | Persistencia, ranking e histórico | Cuentas y modo invitado | El histórico atribuye a una identidad; qué identidades existen lo fija la otra decisión |
| **DEP-D3** | Partidas simultáneas | El cliente no decide resultados `E 2.1` | El estado no queda repartido entre clientes. No se tomó por esto, pero se apoya en ello |
| **DEP-D4** | Cualquier decisión sobre CON-02 | El dominio no depende de nadie `E 2.2` | Ya materializada en `D-30` y `DR-11` |
| **DEP-D5** | `D-30` parámetros en base de datos | `DEP-E7` y `DEP-S1` | Se justifica en que **ya existe una base**; si esa premisa cayera, habría que revisarla |
| **DEP-D6** | `D-28` borrado en cascada | `P-42` prohibición con partida en curso | Sin ella, el borrado dejaría una partida en curso sin uno de sus participantes |

### 3.5 DEP-P · Del proyecto, no del sistema

| ID | Qué depende | De qué depende |
|---|---|---|
| **DEP-P1** | El cierre de **todas** las cuestiones abiertas | **STK-02** — no hay contraparte externa que pueda cerrarlas |
| **DEP-P2** | La cifra **de prueba** de CON-05 | STK-02 (`DR-10` decidió no limitar; para verificar hace falta un N) |
| **DEP-P3** | La aceptación del trabajo | STK-03 y sus criterios, desconocidos |
| **DEP-P4** | Que las medidas de los escenarios tengan **umbral** | STK-03 (Q-23) o, en su defecto, una decisión de STK-02 |

### 3.6 Relaciones descartadas — no volver a proponerlas

| Relación | Por qué no es dependencia |
|---|---|
| Caballero ↔ caballero | No poder colocarse sobre una torre ocupada es una **restricción de validación** contra el estado del tablero |
| Rey ↔ cartas resumen | Contexto compartido; ninguno determina al otro |
| Jugador ↔ jugador | La dependencia real está entre la puntuación agregada y `DEP-I6` |
| Cliente ↔ servidor | Declarada en un solo sentido: «una capa no conoce a la que la usa» `E 2.2` |
| Partida ↔ partida simultánea | **CON-05 es la exigencia de que esta dependencia no exista.** Registrarla invertiría el concern |

---

## 4. Marco del proyecto: frontera, restricciones, condiciones, objetivos y supuestos

Incorporado el 4 sep 2026 a partir de `CAND-01`…`CAND-05`, aprobados por STK-02. **Ningún contenido procede del documento de referencia externo:** solo la forma de organizarlo. Todo lo de abajo se sostiene en fuentes propias de Torres, incluido `STACK.md`.

### 4.1 Frontera del sistema

**Dentro del sistema bajo diseño**

- La partida: tablero, castillos, caballeros, construcciones, cartas, rey, turnos, puntuación.
- Cuentas, perfil con avatar y amistades.
- Salas, código de entrada e invitaciones.
- Rankings global y de sala.
- Reconexión del jugador y reanudación tras la caída del servidor.
- El registro de eventos.

**Fuera de la frontera** — pueden ser *fuente* de un escenario o dependencia, **nunca artefacto**

| Elemento | Registro |
|---|---|
| El canal de correo saliente | `DEP-E9` |
| El motor de base de datos | `DEP-E7` |
| El sistema de archivos donde viven los avatares | `DEP-E10` |
| La red entre cliente y servidor | `DEP-E3` |
| El documento de reglas y el estándar de codificación | `DEP-E4`, `DEP-E5` |
| Bibliotecas de terceros: registro, cifrado de credencial, transporte | `DEP-E1`, `DEP-E8`, `DEP-E11` |

**Fuera de alcance, y no volverá** — chat, espectadores, notificaciones más allá de las invitaciones, logros, temporadas, bloqueo de jugadores, salas públicas listadas, historial de invitaciones, registro de auditoría, rol de administración `D-31`.

### 4.2 `RES-` · Restricciones

**Una restricción no se negocia; una dependencia se puede sustituir.** Por eso se registran aparte. Las impuestas por STK-03 son el marco dentro del cual se diseña.

| ID | Restricción | Origen | Impuesta por |
|---|---|---|---|
| **RES-01** | El sistema se escribe en **C#** | `STACK §2.1`, `E 1` | **STK-03** |
| **RES-02** | La comunicación cliente–servidor usa **WCF con callbacks duplex sobre net.tcp**, con llamadas asíncronas | `STACK §2.2` | **STK-03** |
| **RES-03** | **Prohibido el stack web y los WebSockets** — quedan excluidos SignalR, gRPC y REST | `STACK §2.3` | **STK-03** |
| **RES-04** | **Se evalúa el código, no el acabado visual** | `STACK §2.4` | **STK-03** |
| **RES-05** | Cuatro capas `Game.Client` → `Game.Services` → `Game.Domain` + `Game.Persistence`; **el dominio no depende de nadie** y el cliente no decide resultados | `E 2.1`, `E 2.2` | STK-02 (autor del estándar), exigible por STK-03 |
| **RES-06** | El registro de eventos se hace **solo con log4net**; prohibido registrar contraseñas, tokens y datos personales | `E 9.1`, `E 9.4` | STK-02, exigible por STK-03 |
| **RES-07** | Régimen de pruebas de `E 10`: nombrado, tres bloques, una sola ejecución, sin lógica en la prueba | `E 10` | STK-02, exigible por STK-03 |
| **RES-08** | **`DesktopGL`, nunca `WindowsDX`**; **todos los proyectos en `net10.0`**; **nunca `PublishTrimmed` ni `PublishAot`** —WCF funciona por reflexión y el recortador elimina código necesario— | `STACK §9` | STK-02 |
| **RES-09** | **`SecurityMode.None` en el `NetTcpBinding`** — el modo por defecto usa autenticación de Windows y no funciona entre macOS y Linux | `STACK §9.2` | STK-02 · **ver C-2 en §8** |
| **RES-10** | Mayúsculas exactas en nombres de archivo: Linux distingue, macOS no | `STACK §9.4`, `E 6.14` | STK-02 |

### 4.3 `ORG-` · Condiciones organizacionales

Son el marco contra el que se justifica la letra de **Dificultad** cuando se construya el árbol de utilidad. «Difícil» para este equipo no es lo mismo que difícil en abstracto.

| ID | Condición | Origen |
|---|---|---|
| **ORG-01** | El equipo son **dos personas** | `E` portada, `STACK §2.5` |
| **ORG-02** | Esas dos personas asumen **cinco sombreros cruzados**: autor de reglas, desarrollador, probador, operador y responsable de la entrega | Ficha de STK-02 |
| **ORG-03** | Trabajan en **macOS arm64 y Linux**; **solo una tiene acceso real a Windows** | `STACK §2.5` |
| **ORG-04** | **Las dos deben poder correr servidor y clientes en su propia máquina**, sin depender de la otra. *«La restricción que más decisiones fuerza, y la que suele pasarse por alto»* | `STACK §2.6` |
| **ORG-05** | El desarrollo cabe en el **calendario escolar** — el propio equipo razona sobre un horizonte de **14 semanas** | `STACK §3` |
| **ORG-06** | Se construye sobre un **documento de reglas en estado Borrador**, sin elaborador ni revisor asignados | `R` portada, `TEN-E` |

**ORG-03 y ORG-04 explican decisiones que de otro modo parecerían preferencias:** `.NET 10` en lugar de `.NET Framework`, CoreWCF en lugar de WCF clásico y `DesktopGL` en lugar de `WindowsDX` **no son gustos técnicos**: son lo único que permite que las dos personas trabajen.

### 4.4 `OBJ-` · Objetivos de negocio

**Registro deliberadamente corto.** Solo se anota lo que alguna fuente enuncia. **STK-02 puede ampliarlo; el análisis no inventa objetivos.**

| ID | Objetivo | Origen | Estado |
|---|---|---|---|
| **OBJ-01** | Que **el código** sea lo que demuestre el trabajo: se evalúa el código, no el acabado visual | `STACK §2.4`, `RES-04` | Enunciado por STK-03 |
| **OBJ-02** | Que **las dos personas puedan trabajar y probar de forma autónoma** durante todo el proyecto | `STACK §2.6`, `ORG-04` | Enunciado por STK-02 |
| — | *Cualquier objetivo sobre el jugador, la experiencia de juego o la adopción* | — | **No enunciado por nadie.** Ver `DR-29` |

**Consecuencia para el árbol de utilidad.** Con solo estos dos objetivos, la letra de **Importancia** puede justificarse frente a la evaluación y frente al trabajo del equipo, **pero no frente a ningún interés del jugador**. Si STK-02 quiere que la Importancia refleje algo del jugador, tiene que enunciarlo primero.

### 4.5 `ASM-` · Supuestos

Lo que se da por cierto sin que ninguna fuente lo pruebe. Si un supuesto cae, cae lo que se apoya en él.

| ID | Supuesto | Si cae |
|---|---|---|
| **ASM-01** | Los clientes se ejecutan en máquinas de escritorio con .NET 10 disponible en macOS, Linux o Windows | Cambia el empaquetado y `RES-08` |
| **ASM-02** | **STK-03 aceptará CoreWCF** como cumplimiento de `RES-02`. `STACK §10.1` lo declara *«lo más urgente: de esa respuesta depende toda la arquitectura»* | **Cae toda la arquitectura de transporte** y, con `ORG-03`, el proyecto vuelve a depender de una sola máquina Windows |
| **ASM-03** | Los umbrales que fije STK-02 serán aceptables para STK-03 | Las medidas de todos los escenarios quedan sin validez `DR-29` |
| **ASM-04** | Al reconectar, el jugador retoma su turno en el estado exacto, por autoridad del servidor | `DR-30` dejaría de valer y la respuesta de D-1 cambia |
| **ASM-05** | El juego se juega entre conocidos: no hay salas públicas listadas y las invitaciones van siempre a una cuenta | Se activa la cláusula condicional de CON-03 y `CAND-11` deja de ser opcional |

---

## 5. Reglas del dominio cerradas por STK-02

Decisiones vigentes sobre el juego. **`R` sigue siendo la fuente normativa**, pero estas la completan o la corrigen donde estaba incompleta o ambigua.

| ID | Regla vigente | Sustituye o completa |
|---|---|---|
| **DR-01** | La partida son **siempre 3 rondas**. En cada ronda, **cada jugador** tiene los turnos que dicte la carta resumen. **Un turno es el periodo de un solo jugador**, con sus 90 s | Aclara `R 1.3` y `R 5.1` |
| **DR-02** | La puntuación se calcula **al final de cada ronda** —dicta quién coloca el rey en la siguiente— y **otra vez al final de la partida** | Completa `R 4.1`, habilita `R 2.3` |
| **DR-03** | **Altura máxima de torre: nivel 3** | Acota `R 4.1`, explica la carta 3 |
| **DR-23** | **El corte de ronda no retira nada del tablero.** Lo que ocurre al terminar una ronda es: se toma la **puntuación parcial**, se designa al **colocador del rey** de la ronda siguiente, y se reparte lo que dicten las cartas resumen. *(Retira y sustituye a `DR-04`)* | Completa `R 1.3`, habilita `R 2.3` |
| **DR-05** | Antes del primer turno, **cada jugador coloca un caballero sobre una torre** | Habilita `R 2.3` en la ronda 1 |
| **DR-06** | Las cartas **se obtienen al azar** del mazo | Completa `R 2.4` |
| **DR-07** | Mover un nivel **no puede unir dos castillos** | Completa `R 2.2`, `R 2.4` |
| **DR-08** | **Castillo** = conjunto de torres contiguas; cada construcción ocupa una casilla y llega hasta nivel 3. Los castillos **nacen y quedan delineados por las construcciones iniciales**. No se crean nuevos ni se unen | **Define lo que `R` nunca definió** |
| **DR-09** | Tras un retiro, la partida **continúa con la configuración del número inicial de jugadores** | Resuelve el choque `R 1.1` × `R 5.2` |
| **DR-22** | La condición **«dentro del mismo castillo» aplica a todo movimiento** de caballeros, no solo a la carta 3 | **`R 2.1` está incompleta**: falta enunciarla |
| **DR-24** | El **orden de colocación inicial es el puesto de mesa**, asignado según el orden en que los jugadores se acomodan al empezar la partida. El **último puesto** es «el último jugador que colocó un caballero» y coloca el rey en la ronda 1 | Hace determinable `R 2.3` |
| **DR-25** | Un jugador **retirado** deja de contar: su puntuación ya no entra en «el de menor puntuación» ni en el resultado. **Sus construcciones se quedan en el tablero y sus caballeros se retiran de él** `DR-32` | Completa `R 5.2` |
| **DR-26** | Las cartas 1 y 2 pueden usarse **en cualquier momento del turno**, incluso después de haber gastado PA: suman la diferencia sobre los 5 PA base (+1 y +2). Siguen sin poder usarse las dos a la vez | Aclara `R 3.1`, `R 2.4` |
| **DR-15** | **Ganador:** mayor puntuación total → mayor número de castillos en los que puntuó → puesto de mesa más bajo. **Ranking global:** victorias → puntos acumulados → menos partidas jugadas | **`R` nunca declaraba** que gane quien más puntos tenga |

**Repartos derivados de `DR-01`**

| Jugadores | Turnos **por jugador** R1/R2/R3 | Turnos de mesa por partida | Duración máxima |
|---|---|---|---|
| 2 | 4 / 4 / 4 | 24 | 36 min |
| 3 | 4 / 3 / 3 | 30 | 45 min |
| 4 | 4 / 3 / 3 | 40 | **60 min** |

**Decisiones de proyecto**

| ID | Decisión |
|---|---|
| **DR-10** | El sistema **no limita** el número de partidas simultáneas |
| **DR-11** | El «manejador de partidas» configura las partidas al iniciarlas → **`Game.Services`** |
| **DR-13** | Los 90 s **no corren** mientras el servidor está caído |
| **DR-20** | **El reloj del turno sigue corriendo** durante la desconexión del jugador |
| **DR-21** | Los **invitados no vuelven** tras una caída del servidor |
| **DR-16** | Catálogo de atributos de calidad del proyecto: **ISO/IEC 25010** |
| **DR-18a** | Al arrancar el servidor se **cierran las salas** que quedaran abiertas |
| **DR-18b** | El estado de la partida se escribe **también al empezarla**, no solo al cerrar cada turno |
| **DR-27** | Una **carta maestra** es una carta que modifica las reglas y la distribución. **Mencionada pero nunca documentada.** Sigue fuera del alcance actual — ver el aviso de §8 |
| **DR-28** | La sección «6. Puntos por aclarar» de `R` está vacía porque **no había más puntos que aclarar** en ese momento. No es una omisión |
| **DR-29** | Mientras no se identifique una fuente externa de criterios de aceptación, **los umbrales de todas las medidas los fija STK-02**, y cada escenario lo declara |
| **DR-30** | Al reconectar dentro de la ventana, el jugador **retoma su turno en el estado exacto en que lo dejó** —PA no gastados y acciones ya confirmadas—. Se sigue de que **el servidor es la única fuente de autoridad del estado** `E 2.1` y de que el turno no ha terminado |
| **DR-31** | La cifra de prueba del invariante de aislamiento de CON-05 es **5 partidas simultáneas**. No es un límite del sistema; es el número con el que se verifica |
| **DR-32** | Los **caballeros del jugador retirado se quitan del tablero**. Sus construcciones se quedan. Las torres que ocupaban quedan libres para los demás `R 2.1`, y la comprobación de «un único caballero propio» de `DEP-I2` deja de contarlos |
| **DR-33** | El motor de base de datos es **PostgreSQL**. Se descarta el entregable de doble clic que ofrecía SQLite |

**Acciones documentales que estas decisiones exigen** `[PENDIENTES]`

| Dónde | Qué corregir |
|---|---|
| `R 2.1` | Añadir la condición de «mismo castillo» que la carta 3 cita como existente `DR-22` |
| `R` | Añadir el mapa inicial de castillos: cuántos hay y qué forma tienen. `R 1.2` los declara fijos y nunca los dibuja |
| `schema.sql:308` y `seed-configuration.sql:4` | El comentario dice «turns per round»; debe decir **turnos por jugador y ronda**. **Los datos no cambian**; cambia lo que significan |

---

## 6. Relaciones

### 5.1 Stakeholder → concern

| Concern | STK-01 Jugador | STK-02 Equipo | STK-03 Receptor |
|---|---|---|---|
| CON-01 | **Titular** | Comparte | — |
| CON-02 | — | **Titular** | Indirecto vía `E` |
| CON-03 | **Titular** | Comparte | Indirecto vía `R` |
| CON-04 | **Titular** | Comparte | — |
| CON-05 | Comparte | **Titular** | — |
| CON-06 | **Titular** | Comparte | Indirecto vía `E 9.4` |
| CON-07 | **Titular** | — | — |
| CON-08 | — | **Titular** | **Sí** — hace obligatorio `E 10` |
| CON-09 | — | **Titular** | **Sí** — hace obligatorio `E 9` |
| CON-10 | **Titular** | Comparte | — |

STK-02 es titular o copartícipe de **nueve de diez**. STK-01 no es titular de ningún concern de mantenibilidad; STK-02, de ninguno de continuidad. **STK-03 no es titular de nada**, pero convierte tres concerns en obligaciones.

### 5.2 Concern → dependencias que lo condicionan

| Concern | Dependencias |
|---|---|
| CON-01 | DEP-E3, DEP-E6, DEP-I10, DEP-I11, DEP-S8, DEP-D1 |
| CON-02 | DEP-E4, DEP-E7, DEP-I1, DEP-S1, DEP-D4, DEP-D5 |
| CON-03 | DEP-I2, DEP-I3, DEP-I6, DEP-I7, DEP-I8, DEP-I9, DEP-I12, DEP-S2 |
| CON-04 | DEP-S3, DEP-S4, DEP-D2 |
| CON-05 | DEP-S8, DEP-P2 |
| CON-06 | DEP-E1, DEP-E8, DEP-E9, DEP-E10, DEP-S5 |
| CON-07 | DEP-S4 |
| CON-08 | DEP-E5, DEP-S6 |
| CON-09 | DEP-E1, DEP-S5 |
| CON-10 | DEP-E8, DEP-E9, DEP-D6 |

### 5.3 Entre concerns

**Se refuerzan**
- **CON-08 → CON-02.** Comprobar una regla barato abarata cambiarla. Propósito declarado en `E 2.2`.
- **CON-09 → CON-03.** El registro con operación y entidad permite **detectar** que un resultado no corresponde a lo jugado. Ayuda a detectarlo, no a impedirlo.
- **CON-01 → CON-04.** Guardar el estado en cada frontera de turno es lo que permite que una partida caída llegue a producir resultado.

**Condiciona**
- **CON-07 → CON-01.** Con `DR-21`, la identidad decide quién cuenta para los 2 jugadores que exige la reanudación.
- **CON-07 → CON-04.** Un invitado no tiene identidad a la que atribuir un resultado.
- **CON-10 → CON-04.** Si la cuenta es suplantable, el historial atribuye a quien no jugó.

**Aproximadamente independientes**
- **CON-05 y CON-08.** Una regla del dominio se comprueba igual haya una partida o veinte.

**Presión sin ser tensión**
- **CON-06 sobre CON-04 y CON-07.** No es un atributo central del juego, pero presiona sobre qué se persiste.

**Sin fundamento suficiente** — se mantiene sin clasificar
- Relación entre **CON-05 y CON-01**. `D-26` y `D-29` los acercan, pero ninguna fuente describe qué comparten entre sí dos partidas.

---

## 7. Tensiones

| ID | Tensión | Estado |
|---|---|---|
| **TEN-A** | CON-09 frente a CON-06 · el registro debe identificar la entidad y no debe llevar datos personales | **CERRADA.** El identificador interno de la cuenta es el único que puede aparecer en el registro |
| **TEN-B** | CON-04 frente a CON-06 · el histórico exige conservar datos de personas; la confidencialidad empuja al mínimo | **CERRADA.** Se guarda puesto y puntos, no la secuencia de jugadas; el correo es el único dato obligatorio |
| **TEN-C** | CON-01 frente al ritmo de la partida · si el reloj se detiene durante la desconexión | **CERRADA.** `DR-20` para el jugador, `DR-13` para el servidor |
| **TEN-D** | CON-02 frente a la ausencia de evidencia sobre qué cambiará | **CERRADA por `D-30`.** Se externaliza lo que las reglas declaran variable y nada más |
| **TEN-E** | Construir sobre reglas en estado de borrador | **ABIERTA.** `D-30` y las `DR-` mitigan mucho, pero `R` sigue en v3.0 Borrador, sin elaborador ni revisor. Es el argumento más sólido a favor de CON-02 |
| **TEN-F** | Aleatoriedad frente a pruebas deterministas y sin lógica `E 10.3`, `E 10.4` | **ABIERTA y ampliada.** Con `DR-06` hay **dos** fuentes de azar: el sorteo del empate `R 2.3` y la obtención de cartas. Conciliarlas exige que el azar sea un **colaborador sustituible** del dominio: decisión de diseño con coste. Afecta CON-03 y CON-08 |

| **TEN-G** | **Disponibilidad frente a uso de recursos** · `DR-10` no limita el número de partidas; `D-26` y `D-29` mantienen salas, anfitrión y ranking de sala **en memoria del servidor**; `P-31` retiene partidas caídas 3 min esperando jugadores | **ABIERTA.** Sin límite × estado en memoria × sesiones esperando = presión de memoria que nadie ha examinado. No exige limitar nada: exige **saber** que no limitar tiene coste. Agravada por `ORG-04`: el servidor corre en el portátil de cada integrante |
| **TEN-H** | **Desacoplamiento frente a capacidad del equipo** · `CON-08` y `TEN-F` empujan a inyectar colaboradores sustituibles —la fuente de azar, la de tiempo—; STK-02 son **dos personas con cinco sombreros** `ORG-01`, `ORG-02` en 14 semanas `ORG-05` | **ABIERTA.** Explica por qué `TEN-F` sigue sin resolverse: no es que nadie sepa cómo, es que **cuesta**. Es la tensión que separa *el problema es duro* de *no tenemos tiempo* |
| **TEN-I** | **Seguridad del transporte frente a portabilidad del equipo** · `RES-09` fija `SecurityMode.None` porque el modo por defecto usa autenticación de Windows y **no funciona entre macOS y Linux** `ORG-03` | **ABIERTA.** Proteger el canal exigiría certificados o un modo que rompe `ORG-03` y `ORG-04`. Dejarlo abierto expone la credencial en tránsito pese a `D-03`. Afecta CON-06 y CON-10. Ver **C-2** |

**Consideradas y no sostenidas** — se mantienen fuera:
- *Disponibilidad frente a consistencia.* `E 2.1` ya decidió que el cliente no determina resultados: no existe la opción de seguir jugando desconectado.
- *Rendimiento frente a modificabilidad.* El único dato de rendimiento en las fuentes es una justificación de estilo sobre el coste de las excepciones `E 8.6`. No hay requisitos de latencia ni de carga.

---

## 8. Cuestiones abiertas

### 8.1 Conflictos entre fuentes vigentes `[REQUIEREN DECISIÓN]`

Aparecieron al incorporar `STACK.md`. **No son preguntas sin respuesta: son dos fuentes del proyecto que dicen cosas distintas.**

#### C-1 · El motor de base de datos — **RESUELTO**

`DR-33` — **PostgreSQL se queda.** `schema.sql` y `seed-configuration.sql` siguen vigentes sin cambios y `DEP-E7` queda cerrada. La opción de SQLite, y con ella el entregable de doble clic que `STACK §5` señalaba, **se descarta a sabiendas**: STK-03 tendrá que levantar el motor para ejecutar la entrega.

#### C-2 · La credencial viaja sin cifrar `[REQUIERE DECISIÓN EXPLÍCITA]`

`D-03` guarda la contraseña con BCrypt y nunca en claro. `RES-09` fija **`SecurityMode.None`** en el `NetTcpBinding`, con un motivo legítimo: el modo por defecto usa autenticación de Windows y **no funciona entre macOS y Linux** `ORG-03`.

**Consecuencia que ninguna fuente había puesto por escrito:** si el cliente envía la contraseña al servidor para que este la verifique contra el hash —que es lo normal—, **esa contraseña viaja por la red sin protección**. El cuidado de `D-03` protege la base de datos, no el cable.

Alcanza a **CON-10** (autenticidad) y a **CON-06** (confidencialidad), y es la tensión `TEN-I`.

**Salidas, ninguna gratis:**

| Salida | Coste |
|---|---|
| **Asumirlo y declararlo** — proyecto académico, red local, sin datos sensibles más allá del correo | Ninguno técnico. Exige que CON-06 y CON-10 digan **explícitamente** que su alcance excluye el tránsito |
| Activar seguridad de transporte con certificados | Rompe `ORG-03` y `ORG-04`: complica que cada integrante levante servidor y clientes en su máquina |
| Derivar la credencial en el cliente antes de enviarla | No resuelve nada por sí solo: lo derivado pasa a ser la credencial |

**Recomendación del análisis:** la primera, **declarada**. Lo que no conviene es que quede implícita, porque hoy la ficha de CON-10 y la de CON-06 dan a entender una protección que el transporte no da.

#### C-3 · `log4net` sobre .NET 10 `[VERIFICAR]`

`RES-06` obliga a registrar **solo con log4net** `E 9.1`, y `DEP-E13` fija .NET 10. **No afirmo que sean incompatibles** — hay versiones de log4net para .NET moderno. Pero `STACK §6` no lista esta comprobación entre lo verificado, y `DEP-E1` es una dependencia dura de todo el sistema. **Verificar antes de escribir el escenario de CON-09.**

#### C-4 · CoreWCF sin aprobar `[RIESGO ABIERTO]`

`ASM-02`. `STACK §10.1` lo declara *«lo más urgente: de esa respuesta depende toda la arquitectura»*. Si STK-03 exige WCF clásico sobre .NET Framework, caen `DEP-E11`, `DEP-E13` y con ellos `ORG-04`: el proyecto vuelve a depender de la única máquina Windows del equipo durante todo el semestre. **Es el mayor riesgo abierto del proyecto y no depende de STK-02.**

### 8.2 Preguntas abiertas

| # | Pregunta | Origen | Por qué importa |
|---|---|---|---|
| **Q-32** | Con transporte duplex sobre net.tcp, **¿cómo detecta el servidor que un jugador se cayó?** ¿Por el canal en estado fallido, o con un latido propio y qué periodo? | `CAND-06`, `DEP-E11` | **Afecta a un escenario ya escrito.** D-1 mide una ventana que empieza «cuando el servidor detecta la pérdida de conexión». Si la detección es por latido, el instante depende de su periodo y **la medida de D-1 cambia**. `RES-02` ya decidió el *mecanismo*; falta el *criterio de detección* |
| **Q-33** | ¿Hay una latencia máxima aceptable, y **el tiempo de proceso del servidor se le descuenta al jugador de sus 90 s?** | `CAND-07` | No es rendimiento, es **equidad**: `E 2.1` obliga a que todo lo decida el servidor y `R 5.1` da 90 s. Si la red tarda, los 90 s no son 90 s. La respuesta puede ser «no importa» — pero nadie la ha dado |
| **Q-34** | **¿Qué ocurre si el canal de correo falla?** ¿Se reintenta, se informa, se degrada? Y además: **`STACK.md` no elige ninguna tecnología para enviarlo** | `CAND-08`, `DEP-E9` | La dependencia está registrada y su modo de fallo no. En Torres el radio es **acotado** —bloquea invitaciones por correo y recuperación de acceso, **no** el inicio de sesión ni las partidas en curso, porque Torres no tiene 2FA— y merece la pena escribir exactamente eso |
| **Q-03** | Las **cartas maestras** | — | **Aplazada.** Son cartas que modifican reglas y distribución, mencionadas y nunca documentadas `DR-27`. Fuera de alcance. Ver el aviso de abajo |

**Cerradas:** Q-25 → `DR-23` · Q-26 → `DR-24` · Q-27 → `DR-25` · Q-30 → `DR-32` · Q-07 → `DR-26` · Q-20 → `DR-28` · Q-23 → `DR-29` **y `RES-01`…`RES-04`** · Q-31 → `DR-31`.

**Pendientes de aplicar:** las tres correcciones documentales de §5.

### Aviso sobre las cartas maestras `DR-27`

Una carta que modifica las reglas y la distribución **rompe la delimitación de lo configurable de CON-02**. Hoy `R 1.2` declara la distribución fija y `D-30` deja configurables solo tres tablas de parámetros. Una carta maestra sería **variabilidad en tiempo de ejecución** —mecanismo distinto del que `D-30` construye— y, si además altera la distribución, una contradicción directa con `R 1.2`. Mientras sigan fuera de alcance no hay problema. **Si se incorporan, CON-02 debe reabrirse, no ampliarse.**

### Q-23 — resuelta como supuesto declarado `DR-29`

`N1` y `N2` son etiquetas de `Analisis-Persistencia-Torres.md`, definidas ahí como «requisitos de las fases 1 y 2»: `N1` agrupa cuentas, historial y ranking global; `N2`, salas, invitaciones, amigos, perfil, ranking de sala y reconexión. **Nada de eso viene de las reglas del juego.**

`STACK.md` confirma que **STK-03 sí emite exigencias** —`RES-01`…`RES-04`— pero **ninguna es un umbral**. Por eso el supuesto sigue en pie:

> `DR-29` — Mientras no se identifique una fuente externa de **criterios de aceptación**, los umbrales de todas las medidas los fija STK-02, y cada escenario lo declara.

---

## 9. Propuestas — todas ratificadas

STK-02 ratificó las siete el 4 sep 2026. **Ya no son propuestas: son parte de la base.**

| ID | Adoptado |
|---|---|
| **DR-11** | El manejador de partidas es **`MatchSetupService`**, en `Game.Services` |
| **DR-15** | Ganador y desempates *(ver §5)* |
| **DR-16** | **ISO/IEC 25010** como catálogo de atributos |
| **DR-17** | Medida de CON-02 = coste de cambio |
| **DR-18a** | Cerrar al arrancar las salas que quedaran abiertas |
| **DR-18b** | Escribir el estado también al empezar la partida |
| **DR-19** | Redacción de CON-10 |

**Pendiente:** los conflictos **C-2**, **C-3** y **C-4** de §8.1 quedan **deliberadamente sin definir** por decisión de STK-02 el 4 sep. Los escenarios que los toquen deben declararlo.

**Aprobadas el 4 sep 2026 en la ronda de la referencia externa** — incorporadas en §4, §7 y §8:

| ID | Adoptado como |
|---|---|
| CAND-01 | §4.1 Frontera del sistema |
| CAND-02 | §4.2 registro `RES-` |
| CAND-03 | §4.3 registro `ORG-` |
| CAND-04 | §4.4 registro `OBJ-` |
| CAND-05 | §4.5 registro `ASM-` |
| CAND-06 | `Q-32` — el mecanismo lo cierra `RES-02`; queda el criterio de detección |
| CAND-07 | `Q-33` |
| CAND-08 | `Q-34` |
| CAND-09 | `TEN-G` |
| CAND-10 | `TEN-H` |

**`CAND-11` sigue sin resolverse:** declarar o cerrar el adversario. Registrado como `ASM-05`.

**Pendiente además:** los cuatro conflictos de §8.1 y las tres correcciones documentales de §5.

---

## 10. Base para construir los escenarios

### 10.1 Qué está listo

**Los diez concerns están listos.** No queda ninguno bloqueado.

| Concern | ¿Listo? | Nota |
|---|---|---|
| CON-01 V1 · cae el jugador | **Sí**, con reserva | `Q-32` puede afinar el instante desde el que se cuenta la ventana |
| CON-01 V2 · cae el servidor | **Sí** | `DR-18a` y `DR-18b` ya ratificadas |
| CON-02 | **Sí** | Artefacto `MatchSetupService`, medida de coste de cambio |
| CON-03 | **Sí** | — |
| CON-04 | Funcional | Sin atributo: producirá escenario solo si se le exige una cualidad |
| CON-05 | **Sí** | Invariante verificable con **5 partidas** `DR-31` |
| CON-06 | **Sí** | Debe declarar que su alcance **excluye el tránsito** — ver C-2 |
| CON-07 | Funcional | Sin atributo. Su parte con cualidad vive en CON-01 |
| CON-08 | **Sí** | Atención a **TEN-F** si el escenario toca el azar: hay dos fuentes |
| CON-09 | **Sí** | Verificar C-3 (log4net sobre .NET 10) antes de fijar la medida |
| CON-10 | **Sí** | Igual que CON-06: declarar el alcance frente a C-2 |

**Ocho concerns con atributo de calidad** producirán escenarios. CON-04 y CON-07 son funcionales y no lo harán mientras no se les exija una cualidad.

### 10.2 Materia prima por parte del escenario

| Parte | Valores que esta base respalda |
|---|---|
| **Fuente** | STK-01 (con cuenta / invitado), STK-02 (autor de reglas, operador), la red `DEP-E3`, el servidor, el reloj `DEP-E6` |
| **Artefacto** | `Game.Client` (MonoGame DesktopGL), `Game.Services`, `Game.Domain`, `Game.Persistence`; el **canal duplex CoreWCF** y el ensamblado de contratos; las siete entidades del modelo; las tres tablas de parámetros; el registro de eventos; `MatchSetupService` `DR-11`. **Nunca** algo de fuera de la frontera §4.1 |
| **Ambiente** | Partida en curso · turno propio · turno ajeno · arranque del servidor · reanudación dentro de los 3 min · sala abierta sin partida. **No existe** «modo degradado» definido |
| **Medida** | 90 s `R 5.1` · tiempo restante del turno `R 5.2` · frontera de turno `P-27` · 2 jugadores con cuenta `P-30`+`DR-21` · 3 min `P-31` · **duración máxima de partida: 36 / 45 / 60 min** `DR-01` · 5 MB `P-43` · sin cifra de concurrencia ni de latencia |
| **Procedencia** | CON-01…CON-10. Todo escenario cita uno |

### 10.3 Los dos escenarios ya redactados

El equipo redactó uno de Modificabilidad y otro de Disponibilidad. **No forman parte de esta base y deben rehacerse sobre ella.** Sus defectos y qué los corrige:

| Defecto | Escenario | Qué lo corrige |
|---|---|---|
| **DEF-1** · exige configurar «la distribución inicial de los castillos», que `R 1.2` declara fija | Modificabilidad | `D-30` — retirar la mención a la geometría |
| **DEF-2** · se apoya en «cartas maestras», que no existen en `R` | Modificabilidad | Q-03 aplazada — no usarlas como estímulo |
| **DEF-3** · promete reconexión «mientras la partida continúe activa», sin plazo | Disponibilidad | `R 5.2` + `DR-20`; incluir el desenlace del retiro |
| **DEF-4** · el artefacto es el «manejador de partidas», sin capa | Modificabilidad | `DR-11` |
| **DEF-5** · la medida describe que la configuración se aplique al arrancar: es funcional | Modificabilidad | `DR-17` |
| **DEF-6** · el ambiente «servidor en estado normal» excluye la caída del servidor | Disponibilidad | Partirlo en dos: CON-01 V1 y V2 |

**El texto literal de ambos no está en el repositorio.** Los tres análisis los critican, ninguno los transcribe. Para rehacerlos hace falta su redacción exacta.

---

## 11. Gobernanza

1. Un identificador **nunca se reutiliza** con otro significado. Si un elemento muere, su ID queda retirado.
2. **Añadir un concern** exige: stakeholder que lo introduce, evidencia con marca de origen, y decir si tiene atributo o por qué no. Sin las tres cosas, entra como candidato.
3. **Añadir una dependencia** exige demostrar que cambiar un elemento cambia el otro. Si no, va a §3.6.
4. **No se clasifica un concern en un atributo para poder puntuarlo después.** Dos siguen sin atributo a propósito.
5. Las propuestas de §9 **no son decisiones** hasta que STK-02 las ratifique.
6. **Este documento mantiene un solo estado vigente.** Las secciones 1–10 dicen lo que es hoy; el historial vive en los anexos. Al cambiar algo se actualiza la sección, no se añade una capa nueva.

---

# Anexo A · Traducción desde los documentos anteriores

Para leer los tres análisis previos sin equivocarse.

**Stakeholders**

| Análisis 1 | Análisis 2 | **Vigente** |
|---|---|---|
| STK-01 Jugador | STK-01 | **STK-01** |
| STK-02 Equipo | STK-02 | **STK-02** |
| STK-03 Autor y revisor de reglas | *(rol del equipo)* | **STK-02**, sombrero «autor de reglas» |
| STK-04 Receptor de la entrega | STK-03 | **STK-03** |
| STK-05 Operador `[candidato]` | *(rol del equipo)* | **STK-02**, sombrero «operador» |

**Concerns** — ⚠ estaban **invertidos** entre los dos análisis

| Análisis 1 | **Vigente** |
|---|---|
| CON-01 Modificabilidad | **CON-02** |
| CON-02 Disponibilidad | **CON-01** |
| CON-C1 Testabilidad | **CON-08** *(era duplicado)* |
| CON-C2 Integridad del estado | **CON-03** *(era duplicado)* |
| CON-C3 Sincronía temporal | Sin ID: absorbido por `DEP-E6` y `DR-20` |
| CON-C4 Persistencia de la partida | **CON-01 V2** y **CON-04** *(estaba respondido)* |
| CON-C5 Escalabilidad, usabilidad, portabilidad | Se mantienen descartados |

**Dependencias**

| Análisis 1 | **Vigente** |
|---|---|
| DEP-01 | DEP-I1 |
| DEP-02 | DEP-I2 + DEP-I3 |
| DEP-03 | DEP-I4 |
| DEP-04 | DEP-I5 |
| DEP-05 | DEP-I6 (+I7, +I8) |
| DEP-06 | DEP-I10 |
| DEP-07 | DEP-I11 |
| DEP-08 | DEP-S1 |

**Tensiones**

| Análisis 1 | **Vigente** |
|---|---|
| TEN-01, TEN-02, TEN-03 `[BLOQUEANTE]` | **No eran tensiones** → DEF-1, DEF-2, DEF-3 en §10.3 |
| TEN-04 | TEN-C *(cerrada)* |
| TEN-05 | TEN-F |
| TEN-06 | Absorbida en TEN-D *(cerrada)* |
| TEN-07 | TEN-E |

**Decisiones** — ⚠ `D-01`…`D-10` significaban **dos cosas distintas**

| Documento | Prefijo vigente | Ejemplo del choque |
|---|---|---|
| Análisis 2 §7, decisiones arquitectónicas | **`DA-01`…`DA-10`** | `DA-03` = registro con log4net |
| Análisis 3 §11, decisiones de persistencia | **`D-03`…`D-31`** *(sin cambio)* | `D-03` = hash con BCrypt |

**Cuestiones del Análisis 2** — sus «cuestiones 1–18» equivalen a: 1→Q-04, 2→Q-10, 3→Q-05, 4→Q-11, 5→Q-12, 6→Q-13, 7→Q-06, 8→Q-21, 9→Q-14, 10 y 11→Q-08, 12/13/14→cerradas por el Análisis 3, 15→Q-19, 16→Q-15, 17→Q-01, 18→Q-22.

---

# Anexo B · Historial de reconciliación

Por qué esta base existe. Los tres análisis previos eran correctos por separado y **mutuamente inconsistentes** en sus identificadores.

| # | Hallazgo | Resolución |
|---|---|---|
| **H-A** | `CON-01` y `CON-02` **invertidos** entre los dos análisis | Rige el esquema del Análisis 2, que es el que ya cita el Análisis 3 |
| **H-B** | Stakeholders: 5 contra 3, con `STK-03` designando a dos personas distintas | Tres stakeholders. Los otros dos son sombreros de STK-02, confirmado de forma independiente por el anexo B del Análisis 3 |
| **H-C** | Tres registros de dependencias para los mismos hechos; 8 de 11 eran duplicados | Esquema `E`/`I`/`S`/`D`/`P` |
| **H-D** | Dos registros de tensiones solapados | Esquema alfabético; `TEN-01`…`03` reclasificadas como defectos de escenario |
| **H-E** | El Análisis 3 respondía cuestiones que los otros dos daban por abiertas — es posterior y ninguno pudo verlo | Q-17, Q-18 y las cuestiones 12, 13 y 14 cerradas |
| **H-F** | `DEP-S3` era **falsa** desde `P-37`: una partida *interrumpida* no produce ganador | Enunciado corregido |
| **H-G** | Cuatro de los cinco candidatos a concern del Análisis 1 ya no procedían | `CON-C1`, `CON-C2` duplicados; `CON-C4` respondido |
| **H-H** | Cuatro dependencias externas reales que ningún registro recogía | `DEP-E7`…`DEP-E10` |
| **H-I** | El papel de STK-03 puede haber cambiado de naturaleza | Registrado como Q-23, no afirmado |
| **H-J** | `D-01`…`D-10` también colisionaban entre el Análisis 2 y el 3 | Las arquitectónicas pasan a `DA-` |
| **H-K** | Yo leía `turn_count` como turnos totales de la ronda; es **turnos por jugador** | Corregido por STK-02. `DR-01`. Deja como efecto la cota de duración de partida |
| **H-L** | Leí «las piezas se restablecen» como que el tablero se vacía entre rondas. **No se retira nada**: lo que se reparte de nuevo cada ronda es la asignación de las cartas resumen | `DR-04` retirada, sustituida por `DR-23`. Efecto: la superficie de los castillos **crece durante toda la partida** |

**Elementos retirados** — sus identificadores no se reasignan: `CON-C1`, `CON-C2`, `CON-C4`, `DEP-01`…`DEP-08`, `DEP-S7`, `DR-04`, `DR-12`, `STK-03/04/05` del Análisis 1, el atributo *hubo invitados* de la partida, `P-38` (vale de asiento).

---

# Registro de cambios

| Versión | Fecha | Cambio |
|---|---|---|
| **4.1** | 4 sep 2026 | `DR-33`: **PostgreSQL confirmado**, C-1 resuelto. C-2, C-3 y C-4 quedan sin definir por decisión de STK-02 |
| 4.0 | 4 sep 2026 | **Se incorpora el marco del proyecto.** Nueva §4 con frontera, `RES-01`…`RES-10`, `ORG-01`…`ORG-06`, `OBJ-01`/`OBJ-02` y `ASM-01`…`ASM-05`, a partir de `STACK.md`. Nuevas `DEP-E11`…`DEP-E13` (CoreWCF, MonoGame, .NET 10) y nuevas tensiones `TEN-G`, `TEN-H`, `TEN-I`. Nuevas preguntas `Q-32`…`Q-34`. **Se registran cuatro conflictos entre fuentes vigentes: C-1 motor de base de datos, C-2 credencial sin cifrar en tránsito, C-3 log4net sobre .NET 10, C-4 CoreWCF sin aprobar.** STK-03 pasa de relevancia media a real: impone `RES-01`…`RES-04` |
| 3.2 | 4 sep 2026 | `DR-32`: los caballeros del jugador retirado se quitan del tablero. **Q-30 cerrada — no queda ninguna cuestión abierta que afecte a la base** |
| 3.1 | 4 sep 2026 | `DR-30` (el jugador retoma su turno en el estado exacto, por autoridad del servidor) y `DR-31` (cifra de prueba de CON-05 = 5 partidas). Q-31 cerrada |
| 3.0 | 4 sep 2026 | STK-02 ratifica las siete propuestas y cierra Q-25, Q-26, Q-27, Q-07, Q-20 y Q-23. **`DR-04` retirada y sustituida por `DR-23`**: el corte de ronda no retira nada del tablero. Nuevas: `DR-23`…`DR-29`. ISO/IEC 25010 adoptado. Los diez concerns quedan listos para escenarios |
| 2.0 | 4 sep 2026 | **Reescritura consolidada.** Un solo estado vigente en las secciones 1–9; historial movido a los anexos. Se eliminan los estados obsoletos que quedaban de las versiones anteriores. Se añade §4 con las reglas del dominio cerradas por STK-02, `DEP-I12`, y las acciones documentales pendientes |
| 1.2 | 4 sep 2026 | Correcciones de STK-02: `DR-01′`, `DR-20` retira `DR-12`, `DR-21`, `DR-22` |
| 1.1 | 4 sep 2026 | Ronda de respuestas de STK-02: `DR-01`…`DR-14`, propuestas `DR-15`…`DR-19`, hallazgo H-J |
| 1.0 | 4 sep 2026 | Versión inicial. Reconciliación de los tres análisis previos |
