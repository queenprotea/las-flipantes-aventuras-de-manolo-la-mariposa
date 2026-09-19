# Análisis de referencia externa — «Documento de Requerimientos Arquitectónicos · Atari Go»

**4 sep 2026 · documento de trabajo. Nada de lo que hay aquí forma parte del proyecto.**

## 0. Qué se analizó y cómo

**Origen:** `posible referencia .pdf` — *Documento de Requerimientos Arquitectónicos, Proyecto: Atari Go (Multijugador)*, disciplina Diseño de Software, equipo de Gael Samei Amores Rivas y Emiliano Morales Baizabal. 13 páginas. **Trabajo de otro equipo, sobre otro dominio.**

**Cómo se trata.** Como **material de referencia, no como fuente del proyecto**. No se copia texto, no se importan identificadores y no se adopta nada por parecido. Todo lo que aquí se propone como candidato debe poder **re-derivarse de la evidencia propia de Torres**; donde no se puede, se descarta y se dice.

**Criterio de evaluación**, aplicado a cada elemento:

1. Qué plantea el trabajo original · 2. Qué concepto podría ser relevante · 3. Si tiene correspondencia real con el dominio de Torres · 4. Cómo se trasladaría · 5. Qué beneficio tendría · 6. Qué riesgos genera · 7. Si Torres tiene información suficiente para justificarlo · 8. Si debe descartarse por pertenecer al otro dominio.

**Diferencia de dominio que condiciona todo el análisis.** Atari Go es un juego de dos jugadores, con partidas públicas, chat, 2FA obligatorio por correo, tablero parametrizable y un adversario declarado. Torres es de 2 a 4 jugadores, sin partidas públicas listadas, sin chat, sin 2FA, con tablero declarado fijo y sin adversario declarado. **Coinciden en la forma —juego por turnos en red, cliente/servidor, C#, cuatro capas— y divergen en casi todo el contenido.** Esa coincidencia de forma es la que hace útil el ejercicio; la divergencia de contenido es la que obliga a filtrar.

---

## 1. Análisis elemento por elemento

### 1.1 Frontera del sistema (§1 del documento externo)

| | |
|---|---|
| **Qué plantea** | Una lista explícita de lo que está **dentro** del sistema bajo diseño y lo que está **fuera** (ahí colocan el servicio de correo) |
| **Concepto relevante** | Declarar la frontera antes que nada. Lo de dentro puede ser *artefacto* de un escenario; lo de fuera solo puede ser *fuente* o dependencia |
| **¿Correspondencia con Torres?** | **Sí, y hay un hueco.** Torres tiene material equivalente disperso —«Contexto inicial delimitado» del Análisis 2 y «Fuera de alcance» del Análisis 3— pero **la base oficial no tiene sección de frontera** |
| **Cómo se trasladaría** | Consolidar en la base una sección de frontera construida **solo** con material de Torres |
| **Beneficio** | Directo sobre los escenarios: hoy `DEP-E9` (correo) es dependencia, pero nadie ha declarado que esté fuera de la frontera. Sin eso, alguien podría escribir un escenario poniendo «el servicio de correo» como artefacto |
| **Riesgos** | Ninguno, si se construye con material propio |
| **¿Información suficiente?** | Sí, de sobra |
| **Veredicto** | **CANDIDATO — CAND-01** |

### 1.2 Stakeholders (§2)

Su lista: Jugador · Equipo de desarrollo · Administrador/Operaciones · Auditor de Seguridad · **Adversario** · Autoridad organizacional.

| Stakeholder externo | Análisis para Torres | Veredicto |
|---|---|---|
| Jugador, Equipo de desarrollo | Equivalen a STK-01 y STK-02. Nada nuevo | Sin cambio |
| **Administrador / Operaciones** | Torres ya lo evaluó y lo resolvió **con evidencia propia**: el anexo B del Análisis 3 dice que el operador del servidor *es el propio equipo*. Que otro proyecto lo separe no cambia el hecho de Torres | **DESCARTADO** |
| **Auditor de Seguridad** | Torres declara explícitamente **fuera de alcance el registro de auditoría**. Un auditor sin nada que auditar sería un stakeholder sin concerns | **DESCARTADO** |
| **Adversario** | **Es el único que merece discusión.** Torres tiene una cláusula condicional dormida en CON-03 —«si se declara que se juega entre desconocidos y la manipulación es un riesgo, pasa a tocar integridad»— y además ya adoptó CON-10, cuyas cuatro decisiones presuponen que *alguien podría intentar usar una cuenta ajena*. Es decir: **Torres ya tiene medio adversario implícito y no lo ha nombrado.** Pero el documento externo **no aporta evidencia sobre Torres**: no puede decidir por STK-02 | **CANDIDATO como decisión a tomar, no como stakeholder a añadir — CAND-11** |
| **Autoridad organizacional** | La función existe en Torres y ya está asignada: STK-03 «hace obligatorias, y no opcionales, las restricciones del estándar». Además el estándar de Torres lo firma el propio equipo, no una autoridad externa | **DESCARTADO como stakeholder.** Pero su concepto de *restricciones tecnológicas innegociables* sí sirve → CAND-02 |

### 1.3 Concerns (§3)

| Concern externo | Análisis para Torres | Veredicto |
|---|---|---|
| CON-01 integridad del estado ante cliente modificado | Es la cláusula condicional de nuestro CON-03. No hay evidencia nueva | Ver CAND-11 |
| **CON-02 performance / latencia** | Torres **evaluó y rechazó** rendimiento por falta de evidencia, y sigue sin haberla. **Pero el documento externo hace ver una pregunta que Torres nunca se hizo**, y que sí se deriva de hechos propios: `R 5.1` da 90 s al jugador y `E 2.1` obliga a que **todo** lo decida el servidor. Si una jugada tarda en registrarse, esos 90 s no son 90 s reales | **El concern se descarta. La pregunta es CANDIDATO — CAND-07** |
| CON-03 testabilidad | Es nuestro CON-08, mejor sustentado en Torres (`E 2.2` lo declara literalmente) | Sin cambio |
| CON-04 disponibilidad / reconexión | Es nuestro CON-01. Torres lo tiene más desarrollado: dos variantes y medidas propias | Sin cambio |
| CON-05 auditabilidad de accesos | Torres declara el registro de auditoría **fuera de alcance** | **DESCARTADO** |
| CON-06 el anfitrión configura parámetros de la sala | En Atari Go el **jugador** configura la condición de victoria. En Torres nada de eso existe: `R` declara fijos tablero, rondas, caballeros y cartas, y `D-30` acota lo configurable a tres tablas que cambia **el equipo**, no un jugador. Adoptarlo sería **inventar una funcionalidad** | **DESCARTADO** |
| **CON-07 una falla del correo no debe afectar a las partidas en curso** | **Torres tiene la dependencia y no tiene el concern.** `DEP-E9` existe en la base —invitación por correo `P-20` y recuperación de acceso `D-13`— y **ningún documento pregunta qué pasa si ese canal falla**. El radio de impacto en Torres es mucho menor que en Atari Go, precisamente porque Torres **no tiene 2FA**: una caída del correo bloquea invitaciones y recuperación, no el inicio de sesión ni la partida | **La pregunta es CANDIDATO — CAND-08** |
| CON-08 localidad del cambio | Ya cubierto: la medida `DR-17` de nuestro CON-02 —«sin tocar `Game.Domain` ni recompilar»— **es** una medida de localidad | Sin cambio |
| CON-09 usabilidad / prevención de errores | Torres descartó usabilidad con motivo explícito: «las reglas describen mecánicas, no interacción». *(Existe un caso análogo —`DEP-I4`: un jugador puede tener la carta 5 u 8 y no poder usarla— pero nadie ha pedido que la interfaz lo anticipe)* | **DESCARTADO** |
| CON-10 integridad referencial | En Torres esto ya está resuelto **como restricciones de esquema**, no como concern: `schema.sql` y `D-28`. Elevarlo a concern exigiría un stakeholder y una exigencia que nadie ha enunciado | **DESCARTADO** |

### 1.4 Dependencias (§4)

| | |
|---|---|
| **Qué plantea** | Tres dependencias: servicio de correo, **WCF** como canal exclusivo de transporte, Entity Framework sobre SQL Server |
| **¿Correspondencia?** | El registro de Torres es **más rico y mejor clasificado** (diez dependencias, separadas en externas, de dominio y de sistema). No hay nada que importar |
| **Lo que sí revela** | El otro equipo tiene **decidido su mecanismo de comunicación cliente-servidor**. **Torres no.** `E 2.1` dice que `Game.Services` «expone las operaciones que el servidor ofrece al cliente», pero ningún documento de Torres dice **cómo**. Y eso no es un detalle de implementación: el escenario D-1 que ya escribimos dice *«desde el instante en que el servidor detecta la pérdida de conexión»* — y **nada en Torres explica cómo la detecta** |
| **Veredicto** | Las dependencias concretas se descartan por tecnología ajena. **El hueco que destapan es CANDIDATO — CAND-06** |

### 1.5 Preguntas abiertas (§5)

| Pregunta externa | Estado en Torres |
|---|---|
| Q-01 tolerancia de desconexión | **Ya resuelta**: `R 5.2` + `DR-20` |
| Q-02 reintentos si el correo rechaza | Se integra en CAND-08 |
| Q-03 movimiento de suicidio | Regla exclusiva de Atari Go. **Descartada** |
| Q-04 persistir por movimiento o al final | **Ya resuelta**: `P-27`, frontera de turno |
| **Q-05 ¿pausar el temporizador mientras el servidor calcula?** | **Torres nunca se lo preguntó.** Decidimos que el reloj corre durante la desconexión (`DR-20`), pero no si el tiempo de proceso del servidor se le descuenta al jugador. Se integra en CAND-07 |
| Q-06 distintos tamaños de tablero | `R 1.2` declara el tablero fijo y `D-30` lo deja fuera de lo configurable. **Descartada** |
| Q-07 conexiones concurrentes sin degradar latencia | Cubierta a medias por `DR-10` y `DR-31`; la mitad de latencia va a CAND-07 |

### 1.6 Segunda capa: objetivos, restricciones, supuestos y condiciones (§6)

**Es el aporte más valioso del documento externo, y es estructural, no de contenido.** Separa en cuatro registros lo que Torres tiene mezclado o directamente no tiene:

| Registro externo | Situación en Torres | Veredicto |
|---|---|---|
| **Objetivos de negocio** (OBJ-01…03) | **Torres no tiene ninguno enunciado.** Es un hueco que ya está registrado —`Q-23`, cerrada con `DR-29`: «los umbrales los fija STK-02»—. Un registro de objetivos es exactamente lo que le daría fundamento a la letra de **Importancia** del árbol de utilidad. **Se propone la categoría vacía, no su contenido**: los objetivos de un proyecto los declara su equipo | **CANDIDATO — CAND-04** |
| **Restricciones tecnológicas** (RES-TEC-01…04) | Torres las tiene, pero **registradas como dependencias**, que no es lo mismo. Una dependencia puede sustituirse; una restricción **no se negocia**. Hoy `DEP-E5` (el estándar) es en realidad una restricción, y `DEP-E7` (PostgreSQL) habría que ver de cuál de las dos se trata | **CANDIDATO — CAND-02** |
| **Supuestos** (ASM-01) | Torres tiene supuestos marcados `[DEDUCCIÓN]` dispersos por tres análisis, y uno grande sin registrar: `DR-29`, que los umbrales los fija el equipo | **CANDIDATO — CAND-05** |
| **Condiciones organizacionales** (ORG-01…03: personal limitado, calendario escolar, roles cruzados) | **Torres las tiene y son reales, pero viven como prosa dentro de la ficha de STK-02.** «Cinco sombreros» *es* ORG-03. Explicitarlas importa por una razón concreta: la letra de **Dificultad** del árbol de utilidad se justifica contra ellas — «difícil» para dos personas con roles cruzados no es lo mismo que difícil en abstracto | **CANDIDATO — CAND-03** |

### 1.7 Tensiones (§7)

| Tensión externa | Análisis para Torres | Veredicto |
|---|---|---|
| TEN-01 seguridad vs rendimiento | Torres no tiene requisito de rendimiento. Se disuelve en CAND-07 | Descartada |
| **TEN-02 modificabilidad/testabilidad vs complejidad para un equipo pequeño** | **Torres la tiene y nunca la nombró.** `TEN-F` exige convertir el azar en un colaborador sustituible del dominio para poder probarlo; `CON-08` exige aislamiento; y STK-02 son **dos personas con cinco sombreros**. Es un trade-off real con evidencia propia | **CANDIDATO — CAND-10** |
| **TEN-03 disponibilidad vs uso de recursos** | **Torres la tiene y nunca la nombró, y es la contrapartida directa de una decisión reciente.** `DR-10` no limita el número de partidas; `D-26` y `D-29` ponen salas, anfitrión y ranking de sala **en memoria del servidor**; `P-31` mantiene partidas caídas esperando 3 min. Sin límite × estado en memoria × sesiones esperando = presión de memoria que nadie ha examinado | **CANDIDATO — CAND-09** |
| TEN-04 integridad vs dependencia externa (2FA subordina el acceso al correo) | Torres no tiene 2FA. La versión reducida —el correo es la única vía de recuperación `D-13`— se integra en CAND-08 | Descartada como tensión |

### 1.8 Escenarios (§8) y árbol de utilidad (§9)

**Formato de escenario.** Usan seis partes más una etiqueta de categoría. **El formato de Torres ya es más estricto**: añade procedencia `CON-`, las cuatro preguntas de control y la refutación. **Nada que importar.**

**Formato del árbol de utilidad.** Tabla *Categoría (Atributo) · Escenario · Prioridad · Justificación*, con una línea por letra y exactamente dos escenarios en (A,A). Coincide con lo que ya está registrado como Tema 01. Sirve como **confirmación del formato esperado**, no como contenido.

---

## 2. Lo que conviene NO copiar

Se anota porque son errores que el propio material de clase advierte, y verlos en un ejemplo real ayuda a no repetirlos.

1. **Medidas que mezclan dos cosas.** ESC-01 mide a la vez «el 100 % de los intentos inválidos son rechazados» y «la validación local no excede 2 segundos». Son dos medidas de dos atributos distintos en un solo renglón.
2. **Medidas no refutables.** ESC-07: «se evita el 100 % de las solicitudes innecesarias al servidor debidas a errores de clic». No hay forma de demostrar mañana que ayer no se cumplió.
3. **Dificultad confundida con desconocimiento.** Es el riesgo que la clase nombra explícitamente. En su árbol, varias dificultades altas se justifican por falta de dominio de la herramienta —«sintonizar WCF… es altamente complejo», «requiere un dominio profundo de abstracción»— y no por dureza del problema. Cuando hagamos el árbol de Torres, `TEN-E` y `TEN-F` son dureza del problema; no saber usar una biblioteca **no lo es**.
4. **Un adversario adoptado sin declararlo como riesgo.** Ellos incorporan STK-05 Adversario y con él toda una familia de concerns. Es defendible en su proyecto —tienen partidas públicas—; en Torres sería importar una preocupación sin evidencia. Ver CAND-11.

---

## 3. Propuestas — candidatos, no decisiones

**Ninguno forma parte del proyecto hasta que STK-02 lo apruebe.** Cada uno indica de qué evidencia **propia de Torres** se sostiene.

### Estructurales — cambian la forma de la base, no su contenido

| ID | Propuesta | Se sostiene en | Por qué |
|---|---|---|---|
| **CAND-01** | Añadir a la base una sección **Frontera del sistema**: qué está dentro y qué fuera | Análisis 2 §1 y Análisis 3 §12, ya escritos | Evita que un escenario ponga como *artefacto* algo que está fuera del sistema —el correo, PostgreSQL—. Cuesta media página y se construye con material que ya existe |
| **CAND-02** | Separar un registro **`RES-`** de **restricciones** frente a las dependencias | `E 1`, `E 2.1`, `E 9.1`, `DEP-E5`, `DEP-E7` | Una dependencia se puede sustituir; una restricción **no se negocia**. Hoy están mezcladas, y eso importa al evaluar la Dificultad: «difícil porque el estándar lo prohíbe» no es lo mismo que «difícil porque elegimos esta biblioteca» |
| **CAND-03** | Añadir un registro **`ORG-`** de **condiciones organizacionales**: dos integrantes, cinco sombreros cruzados, calendario de entrega, reglas aún en borrador | Ficha de STK-02, `TEN-E`, `R` portada | **Es lo que da fundamento a la letra de Dificultad** del árbol de utilidad. Sin ellas, «difícil» es una opinión |
| **CAND-04** | Abrir un registro **`OBJ-`** de **objetivos de negocio**, hoy vacío, para que STK-02 lo llene | `Q-23`, `DR-29`, hueco H-3 | **Es lo que da fundamento a la letra de Importancia.** Hoy `DR-29` dice que los umbrales los fija el equipo; unos objetivos escritos convierten eso en algo justificable en lugar de arbitrario. **No propongo ningún objetivo: el contenido es de STK-02** |
| **CAND-05** | Añadir un registro **`ASM-`** de **supuestos** | Deducciones marcadas en los tres análisis, `DR-29`, `DR-30` | Barato. Hace visible sobre qué se apoya lo que no está probado |

### Preguntas — huecos reales que el documento externo hizo visibles

| ID | Pregunta | Se sostiene en | Por qué importa |
|---|---|---|---|
| **CAND-06** | **¿Cuál es el mecanismo de comunicación cliente–servidor, y cómo detecta el servidor que un jugador se cayó?** | `E 2.1`, `DEP-E3`, y el escenario **D-1 ya escrito** | **Es el candidato más importante de todo el análisis.** D-1 mide una ventana que empieza «cuando el servidor detecta la pérdida de conexión», y **ningún documento de Torres dice cómo la detecta**. Si es por *heartbeat*, el instante de detección depende de su periodo, y la medida de D-1 cambia. Afecta también a CON-05 y a `DEP-E3`, que hoy es una dependencia sin mecanismo |
| **CAND-07** | **¿Hay una latencia máxima aceptable, y el tiempo de proceso del servidor se le descuenta al jugador de sus 90 s?** | `R 5.1`, `R 5.2`, `E 2.1`, `DR-20` | Torres rechazó *rendimiento* por falta de evidencia, y sigue sin haberla como atributo. Pero esta pregunta **no es de rendimiento, es de equidad**: si el servidor decide todo y la red tarda, los 90 s del jugador no son 90 s. Puede que la respuesta sea «no importa» — pero hoy nadie la ha hecho |
| **CAND-08** | **¿Qué ocurre si el canal de correo falla?** ¿Se reintenta, se informa, se degrada? | `DEP-E9`, `P-20`, `D-13`, `D-24` | La dependencia está registrada y su modo de fallo no. En Torres el impacto es **acotado** —bloquea invitaciones por correo y recuperación de acceso, **no** el inicio de sesión ni las partidas en curso, porque Torres no tiene 2FA—. Merece la pena escribir precisamente eso: que el radio es pequeño y por qué |

### Tensiones candidatas — trade-offs reales de Torres, nunca nombrados

| ID | Tensión | Se sostiene en | Por qué |
|---|---|---|---|
| **CAND-09** | **Disponibilidad frente a uso de recursos.** `DR-10` no limita las partidas; `D-26` y `D-29` mantienen salas, anfitrión y ranking **en memoria**; `P-31` retiene partidas caídas 3 min esperando jugadores | `DR-10`, `D-26`, `D-29`, `P-31` | Es la **contrapartida directa de `DR-10`**, tomada hace unas horas y sin examinar. No propone limitar nada: propone **nombrar** el trade-off para que la decisión de no limitar sea consciente |
| **CAND-10** | **Desacoplamiento frente a capacidad del equipo.** `CON-08` y `TEN-F` empujan a inyectar colaboradores sustituibles —la fuente de azar, la de tiempo—; STK-02 son dos personas con cinco sombreros | `CON-08`, `TEN-F`, ficha de STK-02, `E 2.2` | Hace explícito por qué `TEN-F` no se ha resuelto: no es que nadie sepa cómo, es que **cuesta**. Y separa dureza del problema de falta de tiempo, que es justo lo que la letra de Dificultad debe distinguir |

### Decisión pendiente

| ID | Asunto | Por qué se plantea |
|---|---|---|
| **CAND-11** | **¿Torres declara un adversario, sí o no?** | Torres arrastra una **cláusula condicional dormida** en CON-03 desde el primer análisis, y ya adoptó **CON-10**, cuyas cuatro decisiones (`D-03`, `D-13`, `D-24`, `P-42`) solo tienen sentido si alguien podría intentar usar una cuenta ajena. O se declara —y CON-03 pasa a tocar integridad, y aparece un stakeholder adversario acotado a la cuenta, **no** al tablero— o se declara explícitamente que **no** y la cláusula se cierra para siempre. **Lo que no conviene es dejarla dormida.** El documento externo no aporta evidencia: solo hace ver que Torres lleva semanas sin decidirlo |

---

## 4. Resumen

| | Elementos |
|---|---|
| **Descartados por pertenecer al otro dominio** | Administrador/Operaciones · Auditor de Seguridad · Autoridad organizacional como stakeholder · performance como concern · auditabilidad · configuración de la partida por el anfitrión · usabilidad · integridad referencial como concern · movimiento de suicidio · tamaños de tablero variables · 2FA · WCF · Entity Framework · SQL Server |
| **Ya cubierto, mejor, en Torres** | Formato de escenario · registro de dependencias · reconexión · testabilidad · localidad del cambio |
| **Candidatos estructurales** | CAND-01 frontera · CAND-02 restricciones · CAND-03 condiciones organizacionales · CAND-04 objetivos de negocio · CAND-05 supuestos |
| **Candidatos como pregunta** | CAND-06 mecanismo de comunicación · CAND-07 latencia y equidad de los 90 s · CAND-08 fallo del canal de correo |
| **Candidatos como tensión** | CAND-09 disponibilidad vs recursos · CAND-10 desacoplamiento vs capacidad del equipo |
| **Decisión pendiente** | CAND-11 declarar o cerrar el adversario |

**El más urgente es CAND-06**, porque afecta a un escenario **ya escrito**: D-1 mide desde un instante que Torres no sabe cómo se produce.
