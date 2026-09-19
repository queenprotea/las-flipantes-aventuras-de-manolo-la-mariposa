# Análisis Arquitectónico — Juego de Torres

**Fase:** identificación de Stakeholders, Concerns, Dependencias, Tensiones y Preguntas.

**Fuentes usadas:**
- **A** — `Reglas_del_Juego_de_Torres.docx` — v3.0, estado **Borrador**, 29 ago 2026
- **B** — `Estandar-de-codificacion-v7.docx` — 20 ago 2026

**Marcas de origen usadas en todo el documento:**

| Marca | Significado |
|---|---|
| `[REGLA §x]` | Afirmado literalmente en el documento de reglas |
| `[ESTÁNDAR §x]` | Afirmado en el estándar de codificación |
| `[CONTEXTO]` | Aportado por el equipo en el enunciado, no por los documentos |
| `[DEDUCCIÓN]` | Se sigue razonablemente de lo anterior, pero **no está escrito**. No es regla |
| `[PENDIENTE]` | No hay información suficiente. Requiere decisión o aclaración |

---

## 0. Hallazgos que condicionan todo el análisis

**1. La sección «6. Puntos por aclarar» de las reglas está vacía.**
Aparece en el índice con número de página asignado, pero el cuerpo del documento termina en 5.2. El propio documento reserva un espacio para lo no resuelto y no lo llena: no existe una lista oficial de pendientes contra la cual contrastar.

**2. Las reglas no contienen «cartas maestras».**
Los únicos tipos de carta definidos son las *cartas de acción* (8, `[REGLA §2.4]`) y las *cartas resumen* (`[REGLA §1.3]`). El escenario de Modificabilidad se apoya en un elemento que la fuente principal no define.

**3. Las reglas declaran la distribución del tablero como fija.**
`[REGLA §1.2]`: «la distribución del tablero será fija en todas las partidas», mientras que el escenario de Modificabilidad plantea configurar la distribución inicial de los castillos. Ambas afirmaciones no pueden ser ciertas a la vez sin definir antes qué comprende «distribución del tablero».

---

## 1. Base documental verificada

Conjunto de hechos comprobables sobre los que se apoya el resto. Cualquier afirmación posterior que no derive de aquí, del contexto declarado por el equipo, o de una deducción marcada como tal, es inválida.

| Ref. | Hecho verificado | Consecuencia arquitectónica directa |
|---|---|---|
| Reglas 1.1 | 2 a 4 jugadores. Mínimo 2 para comenzar. No se incorporan jugadores adicionales a los que empezaron. | El conjunto de participantes queda cerrado al inicio: no hay caso «unirse a partida en curso». |
| Reglas 1.2 | Tablero 8×8. Distribución fija en todas las partidas. | El tablero no es un parámetro de configuración según las reglas. |
| Reglas 1.3 | 3 rondas. Las cartas resumen definen turnos por ronda y construcciones por jugador; ambos valores **dependen de** cantidad de jugadores, ronda y turno. | Dependencia funcional explícita, con tabla de valores conocida. |
| Reglas 2.3 | Ronda 1: coloca el rey el último jugador que colocó un caballero. Rondas siguientes: el jugador con menor puntuación; empate resuelto **al azar**. | Requiere puntuación parcial calculada durante la partida y una fuente de aleatoriedad con autoridad única. |
| Reglas 2.4 | 8 cartas de acción, mazo igual para todos, no disponibles al inicio, 1 PA obtenerlas, 0 PA usarlas. Cartas 5 y 8 exigen no partir el castillo. | Las cartas alteran *reglas de movimiento y construcción*, no la configuración de la partida. |
| Reglas 3.1 | 5 PA por turno, reinicio en cada turno, sin acumulación. Cartas 1 y 2 sustituyen el total por 6 y 7. | El presupuesto de acción es estado de turno, no estado de partida. |
| Reglas 4.1 | Un castillo puntúa a un jugador solo si tiene **un único** caballero propio en él: superficie × nivel de la torre del caballero. | La puntuación depende de la identidad y conectividad del castillo. |
| Reglas 5.1 | Turno de 90 s máximo. Al agotarse, el turno finaliza y se pasa al siguiente. | Existe un reloj con autoridad para cerrar turnos sin acción del jugador. |
| Reglas 5.2 | Desconexión en turno propio: queda el tiempo restante del turno. En turno ajeno: 90 s. Agotado el plazo, **el jugador es retirado de la partida**. | La reconexión tiene ventana acotada y consecuencia definida. |
| Estándar 2.1 | Cuatro capas: `Game.Client`, `Game.Services`, `Game.Domain`, `Game.Persistence`. El cliente **no decide el resultado de ninguna acción del juego**. | Autoridad del servidor ya decidida. El cliente es presentación e intención. |
| Estándar 2.2 | Cliente → Servicios → Dominio + Persistencia. El dominio no depende de nadie. Propósito declarado: probar las reglas sin levantar el servidor ni abrir la ventana. | Restricción dura sobre dónde puede vivir la configuración de reglas. |
| Estándar 9.4 | No se registran contraseñas, tokens de sesión ni datos personales del jugador. | Indicio de que existen credenciales y sesiones; su diseño no está definido en ninguna fuente. |

### OBS-01 — El estándar usa ejemplos de otro juego

- **Observación.** El estándar afirma que «el proyecto es un videojuego desarrollado en C#» y que «todos los ejemplos de este documento pertenecen a ese contexto». Sin embargo sus ejemplos son `DamageCalculator`, `EnemySpawner`, cálculo de daño, progresión de oleadas y `SaveGameRepository`. Ninguno de esos conceptos existe en las reglas de Torres.
- **Por qué importa.** Determina si el estándar sirve como evidencia sobre el *dominio* de Torres o solo sobre la *forma* del código. En este análisis se ha optado por lo segundo: se citan sus reglas de estructura, capas, dependencias, registro y pruebas, y se descarta toda inferencia de dominio a partir de sus ejemplos.
- **Interpretaciones** `[PENDIENTE]`
  - (a) El estándar se redactó con ejemplos genéricos o heredados de otro proyecto; su contenido de dominio no aplica.
  - (b) El proyecto abarca más de un videojuego y Torres es solo uno.
  - Para decidir basta confirmar con los autores si `Game.Domain` se refiere exclusivamente a Torres.

---

## 2. Stakeholders

Solo actores que interactúan con el sistema o tienen interés comprobable. Los elementos del juego (rey, caballeros, cartas, tablero) **no son stakeholders**: no tienen intereses, son objetos gobernados por reglas. El servidor y el manejador de partidas tampoco: son artefactos, y así aparecen correctamente en los escenarios ya redactados.

### STK-01 — Jugador
`[REGLA §1.1]` `[REGLA §5.2]`

**1. Qué representa.** La persona que participa en una partida y ejecuta acciones con puntos de acción. Único actor cuyas decisiones alteran el estado del juego. Entre 2 y 4 por partida `[REGLA §1.1]`.

**2 y 3. Estados, y situación que justifica cada uno.**

| Estado | Situación que lo justifica | Origen |
|---|---|---|
| **Sin sesión** | No identificado ante el sistema. Ninguna fuente define autenticación; único indicio: el estándar prohíbe registrar contraseñas y tokens de sesión `[ESTÁNDAR §9.4]` | `[CONTEXTO]` `[PENDIENTE]` |
| **Con sesión, fuera de partida** | Una partida requiere reunir un mínimo de 2 jugadores antes de comenzar `[REGLA §1.1]`, luego existe un momento previo con jugador identificado y sin partida | `[DEDUCCIÓN]` |
| **En partida, en turno propio** | Dispone de 5 PA y hasta 90 s. Estado distinguido explícitamente por las reglas de reconexión `[REGLA §5.1, §5.2]` | `[REGLA]` |
| **En partida, esperando turno** | Sin PA asignados; las reglas le dan un plazo de reconexión distinto, 90 s completos `[REGLA §5.2]` | `[REGLA]` |
| **Desconectado dentro de la ventana** | La partida continúa y conserva su lugar; la ventana depende de si era su turno `[REGLA §5.2]` | `[REGLA]` |
| **Retirado de la partida** | Estado terminal declarado: «si el tiempo de reconexión termina, el jugador será retirado de la partida» `[REGLA §5.2]` | `[REGLA]` |

**Roles temporales — no son estados.** Dentro de una partida el mismo jugador puede asumir roles que las reglas le asignan y le retiran: *colocador del rey en ronda 1* (el último que colocó un caballero), *colocador del rey en rondas 2 y 3* (el de menor puntuación) y *jugador activo* (el del turno en curso) `[REGLA §2.3, §5.1]`. Son atribuciones derivadas del estado de la partida, no stakeholders distintos ni estados de conexión.

**4. Relaciones.** Con CON-02 Disponibilidad (sus estados de conexión son el estímulo del escenario), con DEP-05 (su puntuación determina quién coloca el rey) y con DEP-07 (su retiro altera la composición de la partida).

**5. Justificación de su identificación.** Es el único actor nombrado en las reglas con acciones, recursos y consecuencias propias.

**6. Falta definir.**
- Si existe identidad persistente (cuenta) o el jugador es anónimo por partida.
- Qué ocurre con torres, caballeros, cartas y puntos de un jugador retirado.
- Si un jugador retirado puede volver a entrar, y si su retiro puede dejar la partida por debajo del mínimo de 2.

**7. No determinable.** El orden de juego dentro de una ronda. Las reglas nombran «el siguiente turno» `[REGLA §5.1]` pero nunca establecen cómo se ordena a los jugadores ni quién empieza. Sin esto no puede modelarse la transición de turno.

### STK-02 — Equipo de desarrollo
`[ESTÁNDAR portada, §1]`

**1. Qué representa.** Los dos integrantes nombrados en el estándar, responsables de construcción, revisión y mantenimiento del código. El estándar existe para ellos y declara que las tareas del equipo se redactan en español y el código en inglés `[ESTÁNDAR §1]`.

**2 y 3. Estados / roles.**
- **Autor** — escribe código que debe cumplir el estándar.
- **Revisor** — el estándar declara el propósito de «mantener un código consistente, legible y verificable durante la revisión» `[ESTÁNDAR §1]`.
- **Mantenedor** — aplica cambios de reglas posteriores. Este rol es el que da a la Modificabilidad un beneficiario concreto.

**4. Relaciones.** Con CON-01 Modificabilidad y con TEN-06 (dónde puede vivir la configuración sin romper la dirección de dependencias).

**5. Justificación.** Es el actor cuyo coste sube o baja cuando cambia una regla del juego. Sin él, Modificabilidad no tiene beneficiario identificable.

**6. Falta definir.** Si el equipo es también quien decide las reglas o solo quien las implementa. De ello depende si un cambio de regla es una decisión interna barata o una negociación externa.

**7. No determinable.** Su capacidad de decisión sobre el alcance del sistema.

### STK-03 — Autor y revisor del documento de reglas
`[REGLA portada]` `[PENDIENTE: rol sin asignar]`

**1. Qué representa.** El rol que fija y aprueba las reglas del juego. Es la **fuente** del escenario de Modificabilidad ya redactado: si las reglas son la fuente del estímulo, alguien tiene la potestad de cambiarlas.

**2 y 3. Estado actual.** Los campos *Elaborado por* y *Revisado por* de la portada **están vacíos**, el estado es **Borrador** y el documento va por la versión 3.0. El propio documento advierte que esos campos «deben completarse antes de la entrega».

**4. Relaciones.** Con todas las preguntas Q-01 a Q-20, y con TEN-07.

**5. Justificación.** Es el único actor que puede resolver las preguntas de la sección 6. Mientras el rol esté vacante, ninguna ambigüedad de reglas tiene responsable de cierre.

**6. Falta definir.** Quién ocupa el rol y cuál es el procedimiento para versionar las reglas.

**7. No determinable.** Si es una persona del equipo o externa. La respuesta cambia la naturaleza de la Modificabilidad: cambio de requisito interno frente a requisito impuesto desde fuera.

### STK-04 — Receptor de la entrega
`[REGLA portada]` `[PENDIENTE: identidad no definida]`

**1. Qué representa.** Existe una «entrega» a la que ambos documentos apuntan: las reglas exigen completar la portada «antes de la entrega» y el estándar está fechado y firmado por integrantes. Hay alguien que recibe y evalúa el resultado.

**2 y 3. Interés comprobable.** El cumplimiento del estándar de codificación y la conformidad del juego con el documento de reglas. Es el actor que hace obligatorias, y no opcionales, las restricciones de capas, nombrado, registro y pruebas.

**4. Relaciones.** Con STK-02 y con las medidas de ambos concerns.

**5. Justificación.** Se deriva de la mención explícita a la entrega en la portada de las reglas.

**6. Falta definir.** Quién es, qué criterios de aceptación aplica y si evalúa atributos de calidad o solo funcionalidad. Sin criterios, las *medidas* de los escenarios no tienen umbral contra el que compararse.

**7. No determinable.** Si coincide con STK-03. Los documentos no lo indican.

### STK-05 — Operador de la infraestructura del servidor
`[DEDUCCIÓN: candidato]` `[PENDIENTE: sin evidencia suficiente]`

**Por qué se plantea.** El escenario de Disponibilidad nombra un «servidor del juego» y el estándar describe una capa de servicios que «expone las operaciones que el servidor ofrece al cliente» `[ESTÁNDAR §2.1]`. Un servidor implica alguien que lo despliega, lo vigila y lo reinicia. El estándar refuerza el indicio al exigir registro con log4net y distinguir niveles «que deciden qué se ve en producción» `[ESTÁNDAR §9.1, §9.2]` — la palabra *producción* presupone un entorno operado.

**Por qué no se confirma.** Ninguna fuente nombra a ese actor, ni describe despliegue, monitoreo o soporte. Se registra como **candidato pendiente de validación**, no como stakeholder identificado. Si se confirma, su interés es la caída y recuperación del servidor, un caso de Disponibilidad que el escenario actual **no** cubre (véase TEN-04 y Q-17).

### Descartados y por qué

| Actor considerado | Motivo del descarte |
|---|---|
| Espectador | No hay ninguna mención a observadores en las reglas. Incluirlo sería inventar un requisito. `[PENDIENTE de preguntar]` |
| Administrador de partida / árbitro | Las reglas resuelven automáticamente todos los casos que plantean, incluido el empate (al azar) y el fin de turno (por reloj). No aparece intervención humana externa. |
| Rey, caballeros, cartas | Elementos del dominio gobernados por reglas, sin intereses propios. El rey es un objeto puntuable, no un participante. |
| Servidor, manejador de partidas | Artefactos del sistema. Aparecen correctamente como *artefacto* en los escenarios y no deben duplicarse como stakeholders. |

---

## 3. Concerns

### CON-01 — Modificabilidad: qué se está pidiendo modificar, exactamente
`[CONTEXTO]` `[REGLA §1.3, §2.4]`

**1. Qué representa.** El escenario redactado mezcla, bajo una sola palabra, tres cosas de coste y mecanismo muy distintos. Separarlas es el primer paso para que el escenario sea verificable.

**2 y 3. Variantes, y situación del juego que justifica cada una.**

**Variante A — Configuración parametrizada**
- *Qué es:* valores que las reglas ya declaran variables y cuya tabla es conocida: turnos por ronda y construcciones por jugador, ambos función de (jugadores, ronda, turno) `[REGLA §1.3]`.
- *Situación que la justifica:* una partida de 3 jugadores recibe 4 turnos en la ronda 1 y 3 en la ronda 2, con 3 construcciones en los turnos 1–2 y 2 en los turnos 3–4. Es un dato tabulado, no una regla programada.
- *Coste del cambio:* editar datos. No requiere recompilar el dominio si la tabla se externaliza. **Es la única variante que el escenario actual mide correctamente.**

**Variante B — Reglas alteradas en ejecución**
- *Qué es:* las cartas de acción cambian el comportamiento legal del juego mientras la partida transcurre. La carta 3 es «la única excepción a la regla de subir un solo nivel por movimiento» `[REGLA §2.1, §2.4]`; la carta 4 permite saltar sobre una casilla ocupada; las cartas 1 y 2 sustituyen el presupuesto de 5 PA por 6 y 7 `[REGLA §3.1]`.
- *Situación que la justifica:* un caballero en nivel 1 no puede llegar al nivel 3 en un movimiento, salvo que el jugador haya obtenido y use la carta 3. La misma acción es ilegal o legal según el estado de cartas del jugador.
- *Consecuencia:* la validación de una acción no puede ser función de la jugada aislada; necesita el contexto de cartas activas del jugador en ese turno. Esto es **variabilidad en tiempo de ejecución**, no modificabilidad del código.

**Variante C — Cambio del documento de reglas**
- *Qué es:* el documento va por la versión 3.0 y sigue en Borrador. Cada nueva versión puede alterar costes de PA, cantidad de caballeros, efectos de cartas o fórmula de puntuación.
- *Situación que la justifica:* la existencia misma de una versión 3.0 en borrador prueba que las reglas cambiaron al menos dos veces antes de que exista el sistema.
- *Coste del cambio:* toca `Game.Domain`, donde el estándar sitúa «las reglas que gobiernan las entidades» `[ESTÁNDAR §2.1]`. Es la variante más cara y **la que ningún escenario cubre hoy**.

**4. Relaciones.** Con STK-02 (beneficiario), DEP-01 (la dependencia que da sentido a la variante A), DEP-08 y TEN-06 (dónde puede vivir la configuración), TEN-01 y TEN-02 (contradicciones con las reglas).

**5. Justificación de su identificación como concern.** Sostenida: las reglas declaran explícitamente valores dependientes de parámetros `[REGLA §1.3]`, existe un mecanismo de excepción de reglas en ejecución `[REGLA §2.4]` y el documento fuente es un borrador versionado.

**6. Falta definir.**
- Qué es «manejador de partidas»: no aparece en reglas ni en el estándar, y no corresponde por nombre a ninguna de las cuatro capas.
- Si las cartas resumen son datos externos o constantes del dominio.
- Qué elementos son configurables por partida y cuáles son invariantes del juego.

**7. No determinable — problema de la medida actual.** La medida redactada («el manejador de partidas debe recuperar y aplicar dichas configuraciones al iniciar la partida») describe *que la configuración funcione*, no *cuánto cuesta cambiarla*. Tal como está es un requisito funcional del arranque, comprobable con una prueba de aceptación, no una medida de modificabilidad. Una medida de modificabilidad se expresa en coste de cambio: número de artefactos tocados, si requiere recompilación o redespliegue, o tiempo de la modificación. Falta decidir cuál de esas formas adopta el proyecto (Q-16).

**Contradicción a resolver.** El escenario nombra «la distribución inicial de los castillos» como configurable y «la distribución de las torres en sus casillas iniciales» como afectable por una carta. Las reglas afirman que la distribución del tablero es fija `[REGLA §1.2]`, no definen en ningún apartado cuál es la distribución inicial de castillos, y ninguna de las 8 cartas modifica la disposición inicial. Véase TEN-01 y Q-01.

### CON-02 — Disponibilidad: alcance real de lo exigido
`[REGLA §5.1, §5.2]` `[CONTEXTO]`

**1. Qué representa.** La capacidad de que una partida siga siendo jugable y de que un jugador recupere su lugar tras un fallo. A diferencia de Modificabilidad, aquí las reglas **sí** aportan medidas numéricas, y son más estrictas que las del escenario redactado.

**2 y 3. Variantes de fallo, y situación que justifica cada una.**

| Variante | Situación concreta | Origen |
|---|---|---|
| **Desconexión del cliente en su propio turno** | La ventana es «el tiempo restante de ese turno». Si le quedaban 12 s, tiene 12 s. Ventana variable y potencialmente casi nula `[REGLA §5.2]` | `[REGLA]` |
| **Desconexión del cliente en turno ajeno** | Ventana fija de 90 s `[REGLA §5.2]` | `[REGLA]` |
| **Vencimiento de la ventana** | El jugador es retirado de la partida. Estado terminal, no degradación temporal `[REGLA §5.2]` | `[REGLA]` |
| **Caída del servidor** | Ninguna fuente la contempla. El escenario actual asume «servidor en estado normal», por lo que este caso queda fuera de cobertura | `[PENDIENTE]` |
| **Cliente activo pero jugador sin responder** | No es un fallo: el turno vence a los 90 s y se pasa al siguiente `[REGLA §5.1]`. Conviene no confundirlo con la desconexión, porque el sistema los detecta de forma distinta | `[REGLA]` |

**4. Relaciones.** Con STK-01 (sus estados de conexión), DEP-06 (la ventana depende del estado del turno), DEP-07 (el retiro altera la partida), TEN-03 y TEN-04.

**5. Justificación.** Sostenida directamente: las reglas dedican el apartado 5.2 completo a la reconexión, con plazos y consecuencia definidos.

**6. Falta definir.**
- Qué ocurre con el **reloj del turno** durante la desconexión del jugador activo: «dispondrá del tiempo restante» implica que el reloj *no* se detiene, pero no lo afirma.
- Si al reconectar recupera los PA no gastados y las acciones ya confirmadas.
- Qué pasa con la partida si el retiro deja menos de 2 jugadores. El mínimo de 2 está declarado solo para *comenzar* `[REGLA §1.1]`, no para continuar.
- Qué pasa con torres, caballeros y puntos del jugador retirado, y si su rey colocado sigue en pie.

**7. No determinable.** Si la disponibilidad exigida cubre solo la sesión del cliente o también la continuidad del servidor. El escenario fija el ambiente en «servidor en estado normal», lo que excluye por construcción la caída del servidor; si la exclusión es deliberada debe declararse, y si no lo es hace falta un segundo escenario.

**Contradicción en la medida.** La medida redactada dice que el servidor permitirá reconectarse «mientras la partida continúe activa». La regla 5.2 dice lo contrario: la ventana es acotada y su vencimiento retira al jugador. Una partida puede seguir activa mucho después de que el jugador haya perdido el derecho a reconectarse. Véase TEN-03.

### Candidatos a concern — evaluados, no adoptados

#### CON-C1 — Testabilidad `[ESTÁNDAR §2.2, §10]` `[DEDUCCIÓN: evidencia fuerte]`
- **A favor.** El estándar no solo pide pruebas: declara un **propósito arquitectónico explícito** para la dirección de dependencias — «que las reglas del juego puedan probarse sin levantar el servidor ni abrir la ventana del juego» `[ESTÁNDAR §2.2]`. Eso es una decisión de estructura tomada en nombre de un atributo de calidad, que es la definición de concern. La sección 10 completa la exigencia: nombrado, estructura en tres bloques, una sola ejecución por prueba, prohibición de lógica en la prueba.
- **Relación con los otros dos.** Es el habilitador de Modificabilidad: sin pruebas del dominio, cada cambio de regla obliga a verificar a mano y el coste del cambio se dispara. También choca con la aleatoriedad de las reglas (TEN-05).
- **Qué falta para adoptarlo.** Confirmar con el equipo si se tratará como concern arquitectónico con escenario propio o como restricción de proceso ya satisfecha por el estándar. La evidencia basta para proponerlo; elevarlo es decisión del equipo.

#### CON-C2 — Integridad del estado de la partida `[ESTÁNDAR §2.1]` `[DEDUCCIÓN: evidencia parcial]`
- **A favor.** El estándar decide que el cliente «no decide el resultado de ninguna acción del juego» `[ESTÁNDAR §2.1]` y que la capa de servicios «valida los argumentos». Es una decisión de autoridad del servidor ya tomada. Las reglas la refuerzan indirectamente: hay información oculta y estado compartido —el mazo, la puntuación parcial que decide quién coloca el rey, el sorteo del empate— que ningún cliente puede resolver por su cuenta.
- **Por qué no se adopta aún.** La evidencia sostiene que *la decisión de autoridad ya está tomada*, no que el proyecto trate la trampa o la manipulación como riesgo a mitigar. Ninguna fuente menciona jugadores adversarios, validación anti-trampa ni cifrado. Adoptarlo sería importar una preocupación típica de otros sistemas sin evidencia en este.
- **Qué lo decidiría.** Saber si el juego se ejecutará entre desconocidos en red abierta o entre miembros del equipo en una demostración controlada.

#### CON-C3 — Sincronía temporal cliente/servidor `[REGLA §5.1, §5.2]` `[DEDUCCIÓN: evidencia parcial]`
- **A favor.** Las reglas imponen dos plazos duros de 90 s y hacen que una consecuencia irreversible —pérdida del turno y retiro del jugador— dependa de un reloj. Si el reloj del cliente y el del servidor discrepan, un jugador puede ver tiempo restante y que su acción sea rechazada por vencida. El problema deriva de dos reglas escritas, no de una analogía.
- **Por qué no se adopta aún.** Puede ser un concern propio (precisión temporal) o un corolario de la autoridad del servidor: si el servidor es la única autoridad del reloj, el cliente solo muestra una estimación y el problema desaparece por diseño. No hay información para decidir. Véase Q-11 y Q-09.

#### CON-C4 — Persistencia de la partida `[PENDIENTE]`
- El estándar define `Game.Persistence` para «los datos que sobreviven al cierre de la partida, como las partidas guardadas y las puntuaciones» `[ESTÁNDAR §2.1]`. Pero las reglas de Torres **no mencionan guardar partidas ni conservar puntuaciones históricas**, y la mención del estándar aparece junto a sus ejemplos de otro juego (OBS-01).
- **Conclusión.** No puede afirmarse que sea un concern de Torres. Tampoco descartarse: si la reconexión debe sobrevivir a un reinicio del servidor, el estado de partida tendría que persistirse, y eso cambiaría el diseño de CON-02. Queda como pregunta Q-17, no como concern.

#### CON-C5 — Descartados por falta total de evidencia
- **Escalabilidad.** Ninguna fuente indica cuántas partidas simultáneas debe soportar el sistema. Una partida tiene como máximo 4 jugadores `[REGLA §1.1]`; el número de partidas concurrentes no está acotado ni mencionado. Sin ese dato no hay concern, hay una pregunta (Q-19).
- **Usabilidad.** Las reglas describen mecánicas, no interacción. La única exigencia con implicación de interfaz es actuar dentro de 90 s, pero no se define nada de presentación.
- **Interoperabilidad / portabilidad.** No hay mención de plataformas, dispositivos ni sistemas externos en ninguna fuente.

---

## 4. Dependencias

Se registra una dependencia solo cuando un elemento **no puede determinarse ni existir** sin el otro: cambiar el primero cambia el segundo. Al final se listan relaciones descartadas por no cumplir ese criterio, aunque los elementos interactúen.

### DEP-01 — Configuración del turno ← cantidad de jugadores, ronda y turno
`[REGLA §1.3]` — dependencia comprobada

- **Naturaleza.** Declarada literalmente: «ambos valores dependen de la cantidad de jugadores, la ronda y el turno». Es la única dependencia del proyecto que el documento nombra con esa palabra.
- **Prueba.** Con 3 jugadores, la ronda 1 tiene 4 turnos y las rondas 2 y 3 tienen 3; con 2 jugadores, las tres rondas tienen 4. Las construcciones caen de 3 a 2 en los turnos finales con 3 jugadores, y son siempre 1 con 4 jugadores. Cambiar cualquiera de las tres entradas cambia la salida.
- **Consecuencia.** La cantidad de jugadores debe fijarse *antes* de que exista configuración de turnos, coherente con que no se admitan incorporaciones posteriores `[REGLA §1.1]`. Es la dependencia que da sentido al escenario de Modificabilidad.
- **Falta definir.** Si «turno» significa el turno de un jugador o una vuelta completa de todos (Q-04): la misma tabla admite dos lecturas y ambas son consistentes con el texto.

### DEP-02 — Puntuación de castillo ← definición e integridad del castillo
`[REGLA §2.2, §4.1]` — dependencia comprobada

- **Naturaleza.** La puntuación es «superficie que el castillo ocupa sobre el tablero × nivel de la torre del caballero» `[REGLA §4.1]`. Ambos factores presuponen que el sistema sabe *qué torres forman un castillo*. Sin esa noción no hay superficie que medir ni castillo al que atribuir un caballero.
- **Cadena completa.** Puntuación → superficie del castillo → conjunto de torres conectadas ortogonalmente → reglas de colocación que prohíben unir ortogonalmente torres de castillos distintos y permiten el contacto diagonal `[REGLA §2.2]`. La condición «un único caballero propio» `[REGLA §4.1]` añade dependencia sobre la ocupación del castillo completo, no de una torre.
- **Por qué es dependencia y no relación.** Una modificación del criterio de conectividad cambia el número de castillos del tablero y con ello la puntuación de todos los jugadores, sin que ninguno haya movido una pieza.
- **Falta definir.** Las reglas **nunca definen qué es un castillo**. Solo dicen dónde no puede colocarse una construcción y que no pueden crearse castillos nuevos. La identidad del castillo se presupone existente desde el inicio sin decir cuántos hay ni qué forma tienen (Q-02).

### DEP-03 — Cartas 5 y 8 ← comprobación de que el castillo no se parte
`[REGLA §2.4]` — dependencia comprobada

- **Naturaleza.** Mover o eliminar un nivel exige que «la operación no debe partir el castillo de origen ni separar sus torres» y que no se retire el nivel 1 de una torre de una sola altura. Es una precondición que solo puede evaluarse sobre el modelo de castillo de DEP-02.
- **Consecuencia.** La legalidad de usar una carta no depende solo de la carta ni del jugador: depende del estado geométrico del tablero en ese instante. Un jugador puede tener la carta 8 y no poder usarla.
- **Falta definir.** Si al mover un nivel con la carta 5 el castillo de destino puede ser cualquiera, y si esa operación puede unir dos castillos, lo que contradiría la prohibición de unir torres de castillos distintos `[REGLA §2.2]`.

### DEP-04 — Presupuesto de acción del turno ← cartas 1 y 2 usadas en ese turno
`[REGLA §2.4, §3.1]` — dependencia comprobada

- **Naturaleza.** Los PA disponibles no son constantes: 5 por defecto, 6 con la carta 1, 7 con la carta 2, «en sustitución» y «solo en el turno en que se utilizan». El presupuesto depende de una acción del propio jugador dentro del turno.
- **Consecuencia.** La validación «¿alcanzan los PA?» no puede resolverse al abrir el turno: el máximo puede subir a mitad de turno. Y como los PA se reinician cada turno y no se acumulan `[REGLA §3.1]`, el presupuesto es estado de turno de vida corta.
- **Falta definir.** Qué ocurre si se usan las cartas 1 y 2 en el mismo turno, y si pueden usarse después de haber gastado PA. Ambas situaciones son alcanzables con las reglas actuales y ninguna está resuelta (Q-07).

### DEP-05 — Colocación del rey ← puntuación parcial de todos los jugadores
`[REGLA §2.3, §4.1, §4.2]` — dependencia comprobada

- **Naturaleza.** En rondas 2 y 3 coloca el rey «el jugador con menor puntuación hasta ese momento». Eso obliga a que exista una puntuación calculada *durante* la partida, dependiente a su vez de DEP-02 y de los puntos que el rey otorgó en la ronda anterior `[REGLA §4.2]`.
- **Cadena.** Quién coloca el rey ← puntuación parcial ← castillos y niveles de caballeros ← colocaciones de todos los jugadores en la ronda anterior. Es la cadena más larga de las reglas y atraviesa a todos los jugadores.
- **Dependencia secundaria.** El empate se resuelve **al azar** `[REGLA §2.3]`. Introduce dependencia de una fuente de aleatoriedad única y autoritativa, porque ningún cliente puede sortear por su cuenta sin que los demás lo acepten. Enlaza con CON-C2 y TEN-05.
- **No determinable.** El momento exacto en que se calcula la puntuación. Las reglas no dicen si se puntúa al final de cada ronda o de forma continua. Que el rey otorgue puntos distintos por ronda (5/10/15) sugiere un corte por ronda, pero es `[DEDUCCIÓN]`, no regla (Q-05).

### DEP-06 — Ventana de reconexión ← estado del turno en el instante de la caída
`[REGLA §5.1, §5.2]` — dependencia comprobada

- **Naturaleza.** La duración de la ventana no es parámetro del jugador ni de la red: se deriva de si era su turno («el tiempo restante de ese turno») o el de otro (90 s). Cambiar la duración del turno en 5.1 cambia automáticamente ambas ventanas.
- **Consecuencia para el diseño.** El componente que gestiona la reconexión no puede ser independiente del que gestiona el turno: necesita leer de quién es el turno y cuánto le queda. Es dependencia real entre lógica de partida y de conexión, no simple coordinación.
- **Falta definir.** Si el reloj del turno sigue corriendo mientras el jugador activo está desconectado. El texto lo insinúa pero no lo afirma. Es la diferencia entre una ventana que se agota sola y una que se congela (Q-09).

### DEP-07 — Continuidad de la partida ← permanencia de los jugadores
`[REGLA §1.1, §5.2]` — `[PENDIENTE: cadena incompleta]`

- **Naturaleza.** Dos reglas se tocan sin cerrarse: el retiro de un jugador es posible en cualquier momento `[REGLA §5.2]`, y el mínimo de 2 jugadores se exige solo «para comenzar» `[REGLA §1.1]`. Además, «no podrán incorporarse jugadores adicionales», por lo que la pérdida es irreversible en composición.
- **Qué sí puede afirmarse.** La configuración de turnos y construcciones depende de la cantidad de jugadores (DEP-01). Por tanto, si un jugador es retirado, o bien la configuración deja de corresponder a la cantidad real de jugadores, o bien debe recalcularse a mitad de partida. Las reglas no eligen ninguna de las dos.
- **No determinable.** Si una partida de 2 jugadores que pierde a uno termina, continúa en solitario o se anula. Es la laguna más consecuente del documento: afecta a la puntuación, a la colocación del rey y al fin de la partida (Q-08).

### DEP-08 — Ubicación de la configuración ← dirección de dependencias entre capas
`[ESTÁNDAR §2.1, §2.2]` — dependencia comprobada

- **Naturaleza.** El estándar fija: cliente → servicios → dominio + persistencia, «el dominio no depende de ninguna otra capa», y «cuando el dominio necesita un dato que solo la persistencia sabe obtener, la capa de servicios lo lee y se lo entrega como argumento».
- **Consecuencia directa.** Si las cartas resumen y la configuración inicial se almacenan fuera del código, **el dominio no puede ir a buscarlas**: tienen que llegarle como argumento desde servicios. Eso ubica el «manejador de partidas» del escenario en `Game.Services` si lee configuración, o en `Game.Domain` si solo aplica reglas — pero no en ambos.
- **Falta definir.** De dónde se leen las configuraciones: archivo, base de datos, constantes compiladas. La respuesta decide si interviene `Game.Persistence` y con ella toda la cadena de capas (Q-15).

### Relaciones descartadas como dependencia

| Relación | Por qué no es dependencia |
|---|---|
| Caballero ↔ caballero | Un caballero no puede colocarse sobre una torre ocupada por otro `[REGLA §2.1]`. Es una **restricción de validación** contra el estado del tablero: ninguno de los dos caballeros cambia porque el otro cambie. |
| Rey ↔ cartas resumen | Ambos pertenecen a la misma partida y ronda, pero ninguno determina al otro. Contexto compartido, no dependencia. |
| Jugador ↔ jugador | Compiten por el mismo tablero y sus puntuaciones se comparan, pero la dependencia real está entre la puntuación agregada y la colocación del rey (DEP-05), no entre los jugadores como actores. |
| Cliente ↔ servidor | Se comunican constantemente, pero la dependencia arquitectónica está declarada en un solo sentido: el cliente depende de los servicios y «una capa no conoce a la que la usa» `[ESTÁNDAR §2.2]`. Tratarla como bidireccional contradiría el estándar. |

---

## 5. Tensiones

Cada tensión se apoya en dos afirmaciones concretas y localizables que no pueden satisfacerse a la vez sin una decisión. Las tres primeras son contradicciones entre las premisas del análisis y la fuente principal, y deben resolverse antes que las demás.

### TEN-01 — Distribución configurable frente a distribución fija `[BLOQUEANTE]`
- **Lado A** `[CONTEXTO]`. El escenario de Modificabilidad exige ajustar «la distribución inicial de los castillos» y admite que una carta afecte «la distribución de las torres en sus casillas iniciales».
- **Lado B** `[REGLA §1.2]`. «La distribución del tablero será fija en todas las partidas». Además, ninguna de las 8 cartas altera la disposición inicial: las cartas 5 y 8 mueven y eliminan niveles *durante* la partida, con restricciones que impiden crear o partir castillos `[REGLA §2.2, §2.4]`.
- **Interpretaciones posibles.**
  - (a) «Distribución del tablero» incluye la posición de los castillos iniciales, que sería fija; el escenario de Modificabilidad sería inválido en ese punto.
  - (b) Se refiere solo a la geometría del tablero, y la colocación inicial de castillos es un asunto distinto no regulado; el escenario sería válido y las reglas incompletas.
  - (c) La regla 1.2 describe el comportamiento por defecto y la configurabilidad es una extensión deliberada por encima de las reglas.
- **Qué la resuelve.** Una definición explícita de qué comprende «distribución del tablero» y un apartado que describa la disposición inicial de castillos, que hoy no existe en ningún lugar del documento.

### TEN-02 — Cartas maestras frente al catálogo cerrado de cartas `[BLOQUEANTE]`
- **Lado A** `[CONTEXTO]`. «El juego cuenta con opciones que permiten modificar las reglas y el comportamiento de la partida mediante las cartas maestras».
- **Lado B** `[REGLA §2.4]`. Catálogo cerrado: «existirán 8 cartas de acción», con efectos tabulados. El otro tipo de carta, la carta resumen, solo define turnos y construcciones `[REGLA §1.3]`. El término «carta maestra» no aparece.
- **Interpretaciones posibles.**
  - (a) Es otro nombre para la *carta resumen*, que sí determina configuración inicial. Es la lectura más compatible con el escenario, porque las cartas resumen actúan «al inicio de cada partida».
  - (b) Es otro nombre para la *carta de acción*, que sí altera reglas, pero durante la partida y no en su configuración inicial.
  - (c) Es un elemento nuevo, aún no incorporado al documento de reglas.
- **Impacto si no se resuelve.** Las tres lecturas producen arquitecturas distintas: (a) datos de configuración leídos al arrancar; (b) motor de reglas con excepciones activas en ejecución; (c) mecanismo de extensión sin especificar. No es una diferencia de nombre, es una diferencia de diseño.

### TEN-03 — Reconexión «mientras la partida siga activa» frente a ventana acotada `[BLOQUEANTE]`
- **Lado A** `[CONTEXTO]`. «El servidor permitirá al jugador reconectarse a la partida mientras esta continúe activa».
- **Lado B** `[REGLA §5.2]`. La ventana se acota al tiempo restante del turno propio o a 90 s en turno ajeno, y al vencer «el jugador será retirado de la partida».
- **Por qué es tensión real.** Una partida de 3 rondas dura bastante más que 90 s: hay escenarios concretos en los que la partida sigue activa y el jugador ya fue retirado. La medida A es más laxa que la regla B, ambas no pueden implementarse a la vez, y A no es verificable porque no fija ningún plazo.
- **Recomendación del análisis.** Adoptar la medida de la regla 5.2, que es concreta y comprobable, y reformular la respuesta del escenario para incluir el desenlace del retiro, hoy fuera del escenario.

### TEN-04 — Duración fija del turno frente a equidad del jugador desconectado
- **El conflicto.** El turno dura como máximo 90 s y vence solo `[REGLA §5.1]`. Si el jugador activo se desconecta, su ventana es exactamente lo que quede de ese turno `[REGLA §5.2]`. Un jugador que cae con 8 s restantes tiene 8 s para reconectar y, aun logrando reconectar, no dispone de tiempo útil para jugar.
- **Los dos lados.** *Ritmo de la partida:* los demás no deben esperar indefinidamente; congelar el reloj es lo que ocurriría si se favorece al desconectado. *Continuidad del jugador:* el objetivo declarado del concern es que el jugador vuelva a la partida, y una ventana de 8 s lo hace inalcanzable en el peor caso.
- **Por qué no se resuelve solo.** Las reglas dan la duración pero no dicen si el reloj se pausa. Cualquiera de las dos decisiones es defendible y cada una satisface un lado a costa del otro. Es una tensión genuina, no una omisión trivial.

### TEN-05 — Sorteo del empate frente a pruebas deterministas y sin lógica
- **El conflicto.** Ante empate en la menor puntuación, el colocador del rey «se elegirá al azar» `[REGLA §2.3]`. El estándar exige que las reglas puedan probarse sin levantar el servidor `[ESTÁNDAR §2.2]` y que las pruebas no contengan lógica, con una sola ejecución y aserciones directas `[ESTÁNDAR §10.3, §10.4]`.
- **Por qué es tensión.** Una regla de dominio que invoca aleatoriedad directamente no es determinista y no puede afirmarse con una aserción simple. Para conciliarlas, la fuente de azar debe ser un colaborador sustituible del dominio: decisión de diseño con coste, no detalle de implementación.
- **Alcance.** Puede afectar también a la obtención de cartas del mazo, si resulta ser aleatoria. Las reglas dicen que el jugador «deberá obtenerlas del mazo» por 1 PA `[REGLA §2.4]` pero no si elige la carta o se le entrega al azar (Q-06).

### TEN-06 — Reglas configurables frente a un dominio sin dependencias
- **El conflicto.** El estándar sitúa las reglas del juego en `Game.Domain` y prohíbe que esa capa dependa de cualquier otra, en particular de `Game.Persistence` `[ESTÁNDAR §2.1, §2.2]`. El concern de Modificabilidad quiere ajustar reglas y configuraciones sin afectar el funcionamiento de la partida, lo que empuja a leerlas desde fuera del código.
- **Cómo se manifiesta.** Cuanto más configurable se hace el dominio, más parámetros debe recibir por argumento desde servicios, según la vía que el propio estándar prescribe. La modificabilidad se paga en superficie de interfaz del dominio y en responsabilidad acumulada en servicios.
- **Lo que no está en conflicto.** Conviene señalarlo: la separación en capas del estándar *favorece* la modificabilidad en el caso general, porque aísla las reglas del cliente y de la persistencia. La tensión no es entre el estándar y el concern, sino sobre **dónde** queda la frontera entre regla y dato configurable, que es justo lo no decidido.

### TEN-07 — Construir sobre reglas en estado de borrador
- **El conflicto.** El documento de reglas es la fuente principal y está en versión 3.0, estado **Borrador**, sin elaborador ni revisor asignados, con su sección de puntos por aclarar vacía. El estándar, en cambio, sitúa las reglas del juego en el núcleo de la arquitectura `[ESTÁNDAR §2.1]`.
- **Por qué importa arquitectónicamente.** Es el argumento más sólido a favor de tratar la Modificabilidad como concern real: no es una hipótesis sobre cambios futuros, es la constatación de que la especificación ya cambió al menos dos veces y sigue sin aprobarse. A la vez, obliga a decidir cuánta flexibilidad construir antes de saber qué cambiará, que es exactamente el riesgo de sobre-diseñar.
- **Cómo se gestiona.** Priorizar la flexibilidad donde las reglas *ya* declaran variabilidad —tablas de cartas resumen `[REGLA §1.3]` y costes de PA `[REGLA §3.2]`— y no anticipar flexibilidad en lo que las reglas declaran fijo, como el tablero `[REGLA §1.2]` o las 3 rondas.

### Tensiones consideradas y no sostenidas
- **Disponibilidad frente a consistencia.** Sería la tensión clásica de permitir jugar sin conexión frente a mantener un estado único. **No aplica con la información actual:** el estándar ya decidió que el cliente no determina resultados `[ESTÁNDAR §2.1]`, así que no existe la opción de continuar jugando desconectado. La tensión desaparece por una decisión ya tomada, y registrarla sería importar un problema ajeno.
- **Rendimiento frente a modificabilidad.** El único dato de rendimiento en las fuentes es una justificación de estilo sobre el coste de las excepciones en «un bucle de juego que se ejecuta muchas veces por segundo» `[ESTÁNDAR §8.6]`. No hay requisitos de latencia, carga ni tiempo de respuesta. Insuficiente para sostener una tensión.

---

## 6. Preguntas

Ordenadas por lo que bloquean. Las **bloqueantes** impiden avanzar al diseño porque cualquier decisión tomada sin ellas tendría que rehacerse.

### Bloqueantes — impiden cerrar los escenarios ya redactados

**Q-01. ¿Qué comprende exactamente «la distribución del tablero» que la regla 1.2 declara fija?**
¿Incluye la posición y forma de los castillos iniciales, o solo la cuadrícula 8×8? El documento nunca describe cuál es esa distribución inicial, de modo que hoy no se sabe ni qué es fijo ni cuál es el valor fijo.
*Desbloquea:* TEN-01 y la validez completa del escenario de Modificabilidad.

**Q-02. ¿Qué es un castillo y cuántos hay al inicio de la partida?**
Las reglas dicen dónde no se puede construir y que no se pueden crear castillos nuevos `[REGLA §2.2]`, pero nunca definen la entidad. De ella depende toda la puntuación `[REGLA §4.1, DEP-02]` y la legalidad de las cartas 5 y 8 (DEP-03).
*Desbloquea:* DEP-02, DEP-03 y el modelo de dominio entero.

**Q-03. ¿Qué es una «carta maestra» y con cuál de los dos tipos de carta definidos se corresponde?**
Si es la carta resumen, la modificabilidad es de configuración inicial. Si es la carta de acción, es variabilidad en ejecución. Si es algo nuevo, hay que especificarla antes de diseñar.
*Desbloquea:* TEN-02 y la elección entre las variantes A y B de CON-01.

**Q-04. ¿Un «turno» pertenece a un solo jugador o es una vuelta completa de todos?**
Ambas lecturas son consistentes con el texto. A favor de «un jugador»: «el turno del jugador finalizará y se pasará al siguiente turno» `[REGLA §5.1]`. A favor de «vuelta completa»: «cada jugador recibirá 5 puntos de acción en cada turno» y «construcciones que recibe *cada jugador* en cada turno» `[REGLA §1.3, §3.1]`. Con 4 jugadores y 4 turnos en la ronda 1, la primera lectura da un turno por jugador y la segunda, cuatro.
*Desbloquea:* DEP-01, la duración total de la partida y el dimensionamiento del reloj.

**Q-05. ¿En qué momento se calcula la puntuación y con qué periodicidad?**
La regla 2.3 exige conocer «la menor puntuación hasta ese momento» al comenzar las rondas 2 y 3, y el rey otorga puntos distintos por ronda `[REGLA §4.2]`. Eso sugiere un corte por ronda, pero ninguna regla lo establece. Tampoco se dice si los puntos de rondas anteriores se acumulan o se recalculan.
*Desbloquea:* DEP-05 y el ciclo de vida de la ronda.

**Q-06. Al obtener una carta del mazo por 1 PA, ¿el jugador la elige o se le entrega al azar?**
Las reglas dicen que todos reciben un mazo igual y que deben obtenerlas del mazo `[REGLA §2.4]`, sin especificar el mecanismo. Determina si hace falta una segunda fuente de aleatoriedad y si el cliente puede mostrar el contenido del mazo.
*Desbloquea:* TEN-05 y el modelo de información del cliente.

### Reglas incompletas — situaciones alcanzables y sin respuesta

**Q-07. ¿Pueden usarse las cartas 1 y 2 en el mismo turno, y en qué momento del turno?**
Ambas «sustituyen» los 5 PA por 6 y 7 `[REGLA §2.4, §3.1]`. Si se usan juntas no está definido si el resultado es 7, 13, o si la combinación es ilegal. Tampoco si pueden usarse tras haber gastado PA. *Afecta:* DEP-04.

**Q-08. ¿Qué ocurre con la partida cuando un jugador es retirado por no reconectar?**
¿Continúa la partida? ¿Qué pasa si quedan menos de 2 jugadores, si el mínimo solo se exige para comenzar `[REGLA §1.1]`? ¿Qué sucede con sus caballeros, torres, cartas y puntuación? ¿Y si era el jugador a quien correspondía colocar el rey? ¿Se recalcula la configuración de turnos, que depende de la cantidad de jugadores `[REGLA §1.3]`? *Afecta:* DEP-07, CON-02 y la definición del fin de partida.

**Q-09. ¿El reloj del turno sigue corriendo mientras el jugador activo está desconectado?**
«Dispondrá del tiempo restante de ese turno» `[REGLA §5.2]` lo insinúa, pero no lo afirma. Y si reconecta con tiempo restante, ¿conserva los PA no gastados y las acciones ya confirmadas? *Afecta:* TEN-04, DEP-06, respuesta del escenario de Disponibilidad.

**Q-10. ¿Cómo se determina el ganador y cómo se resuelve un empate final?**
Las reglas describen cómo se obtienen puntos `[REGLA §4.1, §4.2, §4.3]` pero **nunca declaran que gane quien más puntos tenga**, ni definen el desempate final. El único desempate definido es el de la colocación del rey `[REGLA §2.3]`. *Afecta:* la condición de término del sistema.

**Q-11. ¿Existe un límite de altura para las torres?**
La carta 3 menciona un movimiento «del nivel 1 al nivel 3» `[REGLA §2.4]` y la puntuación multiplica por el nivel de la torre `[REGLA §4.1]`, pero ninguna regla acota la altura máxima. Sin límite, la puntuación no tiene cota superior conocida. *Afecta:* el modelo de torre y la validación de construcción.

**Q-12. ¿Los caballeros y las construcciones permanecen en el tablero entre rondas?**
Las reglas describen el reinicio de los PA en cada turno `[REGLA §3.1]` pero no dicen nada sobre qué se conserva o retira al terminar una ronda. Cada jugador tiene 5 caballeros en total `[REGLA §2.1]`, no por ronda. *Afecta:* el ciclo de vida de la partida y la interpretación de la puntuación por ronda.

**Q-13. ¿Cómo entran los caballeros al tablero por primera vez y en qué orden juegan los jugadores?**
Colocar un caballero cuesta 2 PA `[REGLA §3.2]`, pero no se dice dónde puede colocarse uno nuevo. Además, la regla 2.3 hace depender la colocación del rey en la ronda 1 del «último jugador que haya colocado un caballero», lo que presupone un orden de colocación inicial que el documento no define en ninguna parte. *Afecta:* STK-01, DEP-05 y el arranque de la partida.

**Q-14. ¿La condición de «mismo castillo» aplica a todo movimiento entre niveles o solo a la carta 3?**
La carta 3 dice mantener «las demás condiciones de movimiento de los caballeros, incluida la de realizar el movimiento dentro del mismo castillo» `[REGLA §2.4]`. Esa condición se cita como existente, pero el apartado 2.1 sobre movimiento **no la enuncia**. O falta una regla en 2.1, o la frase de la carta 3 sobra. *Afecta:* la validación de todo movimiento vertical de caballeros.

### Decisiones de proyecto — no son reglas, son acuerdos pendientes

**Q-15. ¿Qué es el «manejador de partidas» y a qué capa pertenece?**
Es el artefacto del escenario de Modificabilidad, pero no aparece ni en las reglas ni en el estándar. Según el estándar, si lee configuración externa pertenece a `Game.Services`; si contiene reglas, a `Game.Domain`; y el dominio no puede leer de persistencia `[ESTÁNDAR §2.1, §2.2]`. No puede ser ambas cosas. *Afecta:* DEP-08, TEN-06 y la validez del escenario tal como está redactado.

**Q-16. ¿Cómo se medirá la Modificabilidad: en coste de cambio o en comportamiento del arranque?**
La medida actual describe que la configuración se aplique al iniciar la partida, lo cual es un requisito funcional verificable con una prueba. Una medida de modificabilidad se expresa en coste: artefactos tocados, necesidad de recompilar o redesplegar, o tiempo de la modificación. Hay que elegir y, si se elige la segunda, fijar el umbral. *Afecta:* CON-01 y la posibilidad de evaluar el escenario.

**Q-17. ¿La Disponibilidad cubre la caída del servidor o solo la del cliente?**
El escenario actual fija el ambiente en «servidor en estado normal», lo que excluye por construcción el fallo del servidor. Si la exclusión es deliberada debe declararse; si no, falta un segundo escenario y con él la decisión de si el estado de partida se persiste. *Afecta:* CON-02, CON-C4, STK-05.

**Q-18. ¿Existe autenticación de jugadores y qué persiste entre partidas?**
El único indicio es la prohibición del estándar de registrar «contraseñas, tokens de sesión y datos personales del jugador» `[ESTÁNDAR §9.4]`. Las reglas no mencionan cuentas. Sin esta respuesta, los estados «con sesión» y «sin sesión» de STK-01 no pueden confirmarse ni descartarse. *Afecta:* STK-01 y el alcance del sistema fuera de la partida.

**Q-19. ¿Cuántas partidas concurrentes debe soportar el sistema?**
Ninguna fuente lo indica. Sin ese dato no puede evaluarse si la escalabilidad es un concern ni dimensionarse el servidor. Basta un número aproximado para cerrar la cuestión. *Afecta:* CON-C5.

**Q-20. ¿El contenido de la sección «6. Puntos por aclarar» se omitió o aún no se ha escrito?**
La sección figura en el índice del documento de reglas con número de página asignado, pero el cuerpo termina en 5.2. Si existe una versión con esa sección redactada, contiene precisamente las respuestas que este análisis está reclamando. *Afecta:* todo el conjunto de preguntas anteriores.

---

*Análisis elaborado exclusivamente a partir de `Reglas_del_Juego_de_Torres.docx` (v3.0, borrador) y `Estandar-de-codificacion-v7.docx`, más el contexto aportado por el equipo sobre los escenarios de Modificabilidad y Disponibilidad. Las referencias entre corchetes remiten al apartado exacto de la fuente citada. Ninguna afirmación sin marca de origen debe tomarse como regla del juego.*
