# Torres — Stakeholders, Concerns y Contexto

**Qué es este documento.** Aplicación de la metodología *decisión → propiedad → stakeholder → concern* (hacia atrás) y *stakeholder → concern → driver → atributo → decisión → evidencia* (hacia adelante) al proyecto del juego de Torres.

**Qué NO se hace aquí, por ser riesgo propio de esta fase:** no se resuelven trade-offs, no se eligen tecnologías, no se formalizan escenarios completos nuevos, no se priorizan ASRs finales y no se diseña la arquitectura. Donde una decisión aún no existe, se dice que no existe.

**Fuentes.**
- `R` — `Reglas_del_Juego_de_Torres.docx`, v3.0
- `E` — `Estandar-de-codificacion-v7.docx`
- `C` — contexto aportado por el equipo (13 aclaraciones)
- `?` — no definido

---

## 1. Contexto inicial delimitado

Delimitar el contexto es lo que permite empezar la trazabilidad: fija sobre qué se puede razonar hoy.

### Dentro del contexto

| Elemento | Origen |
|---|---|
| Partida de 2 a 4 jugadores, 3 rondas, tablero 8×8 con distribución fija | `R 1.1, 1.2, 1.3` |
| Castillo: construcción inicial fijada del tablero, extensible durante la partida según las reglas | `C` |
| 8 cartas de acción, mazo igual por jugador, obtención con coste de 1 PA | `R 2.4` |
| Un solo rey por partida; su efecto se aplica al final de la ronda | `C` |
| Turno de 90 s; ventanas de reconexión de 5.2; retiro del jugador al vencer | `R 5.1, 5.2` |
| Partida con menos de 2 jugadores: termina y gana el que queda | `C` |
| Cuentas de jugador **y** modo invitado | `C` |
| Persistencia de partidas, ranking histórico y amigos | `C` |
| Partidas simultáneas | `C` |
| Cuatro capas, dominio sin dependencias, cliente que no decide resultados | `E 2.1, 2.2` |
| Registro con log4net; prohibición de registrar datos sensibles | `E 9.1, 9.4` |

### Fuera del contexto por ahora

| Elemento | Origen |
|---|---|
| Cartas maestras — extra sujeto a disponibilidad de tiempo, no funcionalidad comprometida | `C` |
| Ejemplos del estándar (daño, oleadas, enemigos) — se dejan como están y no se usan como evidencia de dominio | `C` |

### Dentro del contexto pero sin valor definido

Cuántas partidas simultáneas; qué datos se guardan de una cuenta; qué alcance tiene «amigos»; qué ocurre con el resultado de un invitado frente al ranking.

---

## 2. Actores y stakeholders: no son lo mismo

Un **actor** es un rol que participa en una interacción que modelamos. Un **stakeholder** es una parte que introduce concerns sobre el sistema, su construcción, operación o evaluación. Un mismo sujeto puede ser ambos, uno solo, o ninguno.

| Sujeto | ¿Actor? | ¿Stakeholder? | Justificación |
|---|---|---|---|
| Jugador | Sí — interactúa con el sistema en toda interacción de partida | Sí — introduce concerns de continuidad, resultado y datos personales | `R` completo; `C` cuentas/invitado |
| Equipo de desarrollo | Sí, cuando opera el servidor | Sí — es quien define reglas, desarrolla, diseña, prueba y opera | `C`; `E` portada y §1 |
| Receptor de la entrega | No — no participa en ninguna interacción del sistema | Sí — evalúa el resultado | `R` portada: «deben completarse antes de la entrega» |
| Rey, caballeros, cartas, castillos | No | No | Elementos de dominio gobernados por reglas; no interactúan ni tienen intereses |
| Servidor, cliente, manejador de partidas | No — son partes del sistema, no roles frente a él | No | `E 2.1` los describe como capas y componentes |

**Consecuencia.** Solo hay tres stakeholders sostenibles hoy. Lo que en un análisis anterior parecían stakeholders distintos —autor de las reglas, operador del servidor, probador— son **roles del equipo de desarrollo**, según `C`.

---

## 3. Stakeholders

Se separa **relevancia** (tiene concerns que afectan al sistema) de **autoridad** (puede decidir sobre ellos). No coinciden.

### STK-01 — Jugador

- **Relevancia.** Alta. Todas las interacciones del sistema existen para él.
- **Autoridad.** Ninguna sobre las reglas ni sobre el sistema. Es relevante sin ser decisor.
- **Roles y estados** — un solo stakeholder, no varios:
  - Por identidad: **con cuenta** / **invitado** `C`.
  - Por conexión: **conectado**, **desconectado dentro de la ventana**, **retirado** `R 5.2`.
  - Por posición en la partida: **jugador activo** (turno propio) / **en espera** `R 5.1`; **colocador del rey** en ronda 1 o en rondas 2–3 `R 2.3`.
- **Concerns que introduce.** CON-01, CON-03, CON-04, CON-06, CON-07.
- **No definido.** Si un invitado puede figurar en el ranking o tener amigos; qué distingue funcionalmente a una cuenta de un invitado más allá de la identidad.

### STK-02 — Equipo de desarrollo

- **Relevancia.** Alta y transversal: `C` le atribuye reglas, desarrollo, diseño, pruebas y operación del servidor.
- **Autoridad.** Máxima dentro del proyecto. Es el único stakeholder que puede cerrar las cuestiones abiertas de la sección 10.
- **Roles** — un solo stakeholder, cinco sombreros:
  - **Autor de las reglas** — explica por qué el documento de reglas es una entrada volátil y a la vez interna.
  - **Diseñador y desarrollador** — sujeto al estándar `E`.
  - **Probador** — `E 10` y el propósito declarado en `E 2.2`.
  - **Operador del servidor** — `C`; es aquí donde es también actor.
  - **Responsable ante la entrega** — frente a STK-03.
- **Concerns que introduce.** CON-02, CON-05, CON-08, CON-09; comparte CON-03 y CON-04.
- **Efecto de concentrar los cinco roles.** Un cambio de reglas no requiere negociación externa, lo que abarata la variante «cambio de regla». A cambio, no hay contraparte que valide una regla ambigua desde fuera: las ambigüedades de la sección 10 solo se cierran por decisión propia.

### STK-03 — Receptor de la entrega

- **Relevancia.** Media, indirecta pero real: es quien hace obligatorias, y no opcionales, las restricciones del estándar.
- **Autoridad.** Sobre la aceptación del resultado, no sobre el diseño.
- **Concerns que introduce.** Conformidad con `E` y con `R`. No se le atribuyen otros: no hay información sobre sus criterios.
- **No definido.** Su identidad y sus criterios de aceptación. Sin ellos no puede saberse si evalúa atributos de calidad o solo funcionalidad.

---

## 4. Concerns

Un concern nombra **aquello sobre lo que hay que poder responder**, sin fijar todavía la respuesta de diseño.

Cada ficha indica, además, si el concern se corresponde con un **atributo de calidad** y por qué. **No todos lo tienen:** tres de los nueve no admiten hoy una clasificación sostenible y se dejan deliberadamente sin ella. Forzarlas produciría atributos que nadie ha pedido y que no se podrían medir.

*Terminología:* se usan los nombres habituales de atributos de calidad. El proyecto **no ha fijado un catálogo de referencia** — cuestión abierta 18.

### 4.1 Refinamiento de las frases recibidas

| Frase recibida | ¿Por qué importa? | ¿A quién afecta? | Asunto a preservar → **concern** | Solución prescrita (no decidida) |
|---|---|---|---|---|
| «Necesitamos persistencia» | Habrá ranking histórico y amigos `C` | Jugador con cuenta, equipo | Que el resultado de una partida terminada siga disponible después de terminar → **CON-04** | Base de datos, archivos — sin decidir |
| «Necesitamos que sea modificable» | El documento de reglas va por v3.0 en borrador y las cartas maestras podrían añadirse `R` portada, `C` | Equipo (rol reglas y mantenimiento) | Que ajustar reglas y parámetros no obligue a rehacer el sistema → **CON-02** | Configuración externa, motor de reglas — sin decidir |
| «Seguridad» (vago) | Hay cuentas, amigos y un ranking que atribuye resultados `C` | Jugador, equipo | Se precisa en dos concerns distintos: **CON-03** correspondencia entre lo jugado y lo puntuado, y **CON-06** qué datos personales se guardan y registran | Autenticación, cifrado — sin decidir |
| «Que aguante varias partidas» | Habrá partidas simultáneas `C` | Jugador, equipo (rol operación) | Que lo que ocurre en una partida no altere otra → **CON-05** | Sin decidir |

### 4.2 Resumen: concern → atributo de calidad

| Concern | Atributo de calidad | Estado de la clasificación |
|---|---|---|
| CON-01 · Continuidad ante una desconexión | **Disponibilidad** | Confirmado |
| CON-02 · Ajuste de reglas y parámetros | **Modificabilidad** | Confirmado |
| CON-03 · Correspondencia entre lo jugado y lo puntuado | **Corrección funcional** | Confirmado. Tocaría *seguridad / integridad* solo si se declara el juego entre desconocidos |
| CON-04 · Conservación del resultado de las partidas | — | **Sin atributo.** Requisito funcional mientras no se exija tolerancia a la pérdida |
| CON-05 · Aislamiento entre partidas simultáneas | **Corrección funcional bajo concurrencia** | Parcial. **No** es escalabilidad: falta la cifra objetivo |
| CON-06 · Qué información personal se guarda y registra | **Seguridad — confidencialidad** | Confirmado, con alcance limitado al registro de eventos |
| CON-07 · Jugar con cuenta o como invitado | — | **Sin atributo.** Decisión funcional de alcance |
| CON-08 · Poder comprobar que una regla hace lo que dice | **Testabilidad** | Confirmado |
| CON-09 · Poder reconstruir qué ocurrió cuando algo falla | **Analizabilidad (diagnóstico)** | Confirmado |

**Observación estructural.** CON-02, CON-08 y CON-09 corresponden a los tres atributos que componen la mantenibilidad —modificabilidad, testabilidad y analizabilidad— y los tres provienen del **mismo stakeholder**, STK-02, que aquí reúne los roles de autor de reglas, probador y operador `C`. Eso explica por qué se refuerzan entre sí y no compiten: sirven al mismo interesado.

### 4.3 Fichas

#### CON-01 — Continuidad de la partida ante una desconexión

| | |
|---|---|
| **Qué preocupa** | Que un jugador que pierde la conexión pueda retomar la partida en el punto en que estaba, y que mientras tanto la partida siga siendo jugable para los demás |
| **Stakeholder** | STK-01 Jugador · STK-02 Equipo (rol autor de reglas) |
| **Situación concreta** | Un jugador cae en mitad de su turno; los otros dos siguen en la mesa con sus 90 s corriendo. El sistema tiene que decidir si lo espera, cuánto, y qué hace si no vuelve |
| **Atributo de calidad** | **Disponibilidad** |
| **Por qué corresponde** | El concern trata de que el servicio siga prestándose ante un fallo y de que el afectado se recupere en un plazo. Eso es exactamente disponibilidad: detección del fallo, ventana de recuperación y desenlace definido |
| **Cómo se refleja** | Las reglas ya fijan los tres elementos: la ventana según de quién sea el turno, el plazo de 90 s, y el retiro al vencer. `C` añade el desenlace de la partida cuando quedan menos de 2 jugadores |
| **Evidencia** | `R 5.2` ventanas y consecuencia · `R 5.1` turno de 90 s · `C` fin de partida con menos de 2 jugadores |
| **No definido** | Si el reloj del turno corre durante la desconexión · qué ocurre con piezas y puntos del retirado cuando quedan 2 o más jugadores |

#### CON-02 — Ajuste de reglas y parámetros sin rehacer el sistema

| | |
|---|---|
| **Qué preocupa** | Que los valores que las reglas ya declaran variables, y los cambios de versión del documento de reglas, puedan aplicarse con un coste acotado |
| **Stakeholder** | STK-02 Equipo (rol autor de reglas y rol mantenimiento) |
| **Situación concreta** | El documento de reglas va por la v3.0 y sigue en borrador: ya cambió al menos dos veces antes de que exista el sistema. Y las cartas maestras podrían añadirse si hay tiempo |
| **Atributo de calidad** | **Modificabilidad** |
| **Por qué corresponde** | El asunto es el **coste de un cambio**, no que el sistema funcione. Ese es el objeto de la modificabilidad: cuántos artefactos hay que tocar y qué hay que rehacer para incorporar una variación |
| **Cómo se refleja** | Parte del comportamiento ya está expresado como datos y no como enunciado de regla: `R 1.3` declara literalmente que turnos y construcciones «dependen de la cantidad de jugadores, la ronda y el turno». La variabilidad no es una hipótesis, está escrita |
| **Evidencia** | `R 1.3` tablas paramétricas · `R` portada v3.0 en borrador · `C` cartas maestras como extra posible |
| **No definido** | Cómo se medirá el coste de cambio · qué frontera separa una regla de un dato configurable |

#### CON-03 — Correspondencia entre lo jugado y lo puntuado

| | |
|---|---|
| **Qué preocupa** | Que el resultado de la partida refleje exactamente las acciones legales realizadas por cada jugador, y solo esas |
| **Stakeholder** | STK-01 Jugador · STK-02 Equipo |
| **Situación concreta** | La puntuación depende del estado geométrico del tablero —superficie del castillo por nivel del caballero, y solo si hay un único caballero propio— y de un sorteo cuando hay empate. Ningún cliente puede calcular eso por su cuenta y que los demás lo acepten |
| **Atributo de calidad** | **Corrección funcional** |
| **Por qué corresponde** | Lo que se pide es que el cómputo del resultado sea el correcto según las reglas, no que esté protegido de un atacante. Es corrección, no seguridad |
| **Clasificación condicional** | Si el equipo declara que se jugará entre desconocidos y que la manipulación es un riesgo a mitigar, **el mismo concern pasa a tocar seguridad en su dimensión de integridad**. Hoy ninguna fuente menciona jugadores adversarios, así que esa mitad no se afirma |
| **Cómo se refleja** | `E 2.1` ya decidió que el cliente «no decide el resultado de ninguna acción del juego»: hay un único punto donde se determina el resultado |
| **Evidencia** | `E 2.1` autoridad del servidor · `R 4.1`–`R 4.3` cálculo de puntos · `R 2.3` sorteo del empate · `C` un solo rey, efecto al final de la ronda |
| **Nota de alcance** | En este dominio **no existe un concern de protección (safety)**: ninguna decisión del sistema puede causar daño a una persona. Se hace explícito para no arrastrar una categoría que aquí no aplica |

#### CON-04 — Conservación del resultado de las partidas

| | |
|---|---|
| **Qué preocupa** | Que el resultado de una partida terminada quede disponible después para el ranking histórico |
| **Stakeholder** | STK-01 Jugador con cuenta · STK-02 Equipo |
| **Situación concreta** | Habrá ranking histórico y amigos, y toda partida produce un resultado —incluso la que termina antes de tiempo por quedarse con un solo jugador |
| **Atributo de calidad** | **Ninguno, por ahora** |
| **Por qué no se clasifica** | «El sistema guarda el resultado» es **lo que el sistema hace**, no *con qué cualidad* lo hace. Un atributo de calidad aparecería en cuanto se exigiera algo sobre esa función: por ejemplo, que ningún resultado se pierda ante una caída del servidor —fiabilidad—, o que el histórico se consulte en un tiempo dado —rendimiento—. Ninguna fuente exige nada de eso |
| **Qué lo convertiría en atributo** | Que el equipo declare una tolerancia a la pérdida de resultados. Mientras no la haya, clasificarlo sería inventar una exigencia |
| **Evidencia** | `C` persistencia, ranking histórico y amigos · `C` la partida con menos de 2 jugadores termina con ganador |
| **No definido** | Qué se guarda exactamente · qué ocurre con las partidas jugadas por invitados |

#### CON-05 — Aislamiento entre partidas simultáneas

| | |
|---|---|
| **Qué preocupa** | Que lo que ocurre en una partida no altere el estado ni el resultado de otra |
| **Stakeholder** | STK-02 Equipo (rol operación) · STK-01 Jugador |
| **Situación concreta** | Varias partidas corren a la vez, cada una con sus propios turnos de 90 s venciendo de forma independiente y su propio rey, tablero y marcador |
| **Atributo de calidad** | **Corrección funcional bajo concurrencia** — clasificación parcial |
| **Por qué corresponde** | El asunto es que el resultado de cada partida siga siendo el correcto cuando hay varias en curso. Es una exigencia sobre la corrección del cómputo en presencia de concurrencia |
| **Por qué NO es escalabilidad ni rendimiento** | Ambos atributos requieren una cifra objetivo —cuántas partidas, con qué tiempo de respuesta— y esa cifra no existe (cuestión 15). Clasificarlo así sería fijar un requisito que nadie ha enunciado |
| **Evidencia** | `C` deben poder existir partidas simultáneas · `R 5.1` cada partida lleva sus propios plazos |
| **No definido** | Cuántas partidas simultáneas |

#### CON-06 — Qué información personal se guarda y se registra

| | |
|---|---|
| **Qué preocupa** | Que la información personal del jugador que el sistema conserva y escribe en sus registros se mantenga en el mínimo necesario |
| **Stakeholder** | STK-01 Jugador · STK-02 Equipo |
| **Situación concreta** | Existirán cuentas y relaciones de amistad, es decir, hay datos de personas que el sistema va a manejar y a escribir en sus registros de diagnóstico |
| **Atributo de calidad** | **Seguridad, en su dimensión de confidencialidad** |
| **Por qué corresponde** | La exigencia es que cierta información no quede expuesta a quien no debe verla. El propio estándar lo argumenta así: «el registro se conserva y se comparte para diagnosticar, así que todo lo que se escribe en él deja de ser privado» |
| **Alcance real de la evidencia** | La regla documentada cubre **solo el registro de eventos**. Sobre qué se almacena en la persistencia no hay ninguna fuente. La clasificación vale para la mitad del concern que está sustentada |
| **Evidencia** | `E 9.4` prohibición de registrar contraseñas, tokens y datos personales · `C` existirán cuentas y amigos |
| **No definido** | Qué datos de una cuenta se conservan · qué alcance tiene «amigos» |

#### CON-07 — Jugar con cuenta o como invitado sin perder atribución

| | |
|---|---|
| **Qué preocupa** | Que la forma de identificarse no impida jugar y que, a la vez, determine con claridad qué resultados pueden atribuirse a un jugador después de la partida |
| **Stakeholder** | STK-01 Jugador |
| **Situación concreta** | Un invitado puede sentarse a jugar sin registrarse, pero el ranking histórico atribuye resultados a una identidad que un invitado no tiene |
| **Atributo de calidad** | **Ninguno, por ahora** |
| **Por qué no se clasifica** | Es una decisión sobre **qué hace el sistema y para quién** —modos de identificación y qué se puede atribuir a cada uno—, no sobre con qué cualidad lo hace. Se podría forzar hacia usabilidad, alegando que jugar sin registro reduce la fricción de entrada, pero ninguna fuente enuncia esa intención y sería atribuir un propósito no declarado |
| **Qué lo convertiría en atributo** | Que el equipo declare explícitamente que un jugador debe poder empezar una partida sin registrarse, como objetivo y no como opción. Entonces sería usabilidad |
| **Evidencia** | `C` existirán cuentas y también modo invitado · `C` habrá ranking histórico |
| **No definido** | Si el invitado aparece en el ranking, si puede tener amigos, si puede convertirse en cuenta |

#### CON-08 — Poder comprobar que una regla hace lo que dice

| | |
|---|---|
| **Qué preocupa** | Que el comportamiento de las reglas del juego pueda verificarse sin depender del servidor ni de la interfaz |
| **Stakeholder** | STK-02 Equipo (rol probador) |
| **Situación concreta** | Las reglas son el núcleo del sistema y cambian entre versiones del documento. Comprobar que la puntuación de un castillo o el límite de subida de un caballero siguen siendo correctos no debería requerir levantar una partida completa |
| **Atributo de calidad** | **Testabilidad** |
| **Por qué corresponde** | Es el caso más sólido de todo el análisis, porque **no hay que inferirlo**: el estándar declara ese propósito de forma literal para justificar una decisión de estructura. Una decisión arquitectónica tomada en nombre de una cualidad es, por definición, un atributo de calidad en juego |
| **Cómo se refleja** | La dirección de las dependencias existe por este motivo: «el propósito es que las reglas del juego puedan probarse sin levantar el servidor ni abrir la ventana del juego». Y `E 10` fija nombrado, estructura en tres bloques y prohibición de lógica en las pruebas |
| **Evidencia** | `E 2.2` propósito declarado · `E 10.1`–`E 10.5` régimen de pruebas |
| **Relación** | Refuerza CON-02: comprobar una regla barato abarata cambiarla |

#### CON-09 — Poder reconstruir qué ocurrió cuando algo falla

| | |
|---|---|
| **Qué preocupa** | Que ante un fallo en una partida se pueda saber qué operación falló, sobre qué entidad y en qué punto |
| **Stakeholder** | STK-02 Equipo (rol operación) |
| **Situación concreta** | El propio equipo opera el servidor, así que es quien lee esos registros cuando una partida falla en producción y ya no puede reproducirse |
| **Atributo de calidad** | **Analizabilidad** — la capacidad de diagnosticar una deficiencia o localizar la causa de un fallo |
| **Por qué corresponde** | El estándar enuncia el criterio con la forma exacta de este atributo: exige que el mensaje lleve la operación y el identificador de la entidad afectada porque «un mensaje sin ese contexto obliga a reproducir el fallo para saber a qué se refería». Evitar tener que reproducir el fallo es la definición operativa de analizabilidad |
| **Cómo se refleja** | Registro por clase, la excepción pasada como argumento para conservar tipo y traza, y un solo registro por fallo |
| **Evidencia** | `E 9.1` registrador por clase · `E 9.3` contenido del mensaje · `E 9.5` un solo registro por fallo · `C` el equipo opera el servidor |
| **Tensión** | Con CON-06: el mismo estándar pide identificar la entidad afectada y prohíbe registrar datos personales del jugador |

## 5. Relaciones entre concerns

Solo se afirman relaciones con fundamento. Las demás se dejan sin clasificar.

### Se refuerzan

Los tres atributos de mantenibilidad —modificabilidad, testabilidad y analizabilidad— provienen del mismo stakeholder, lo que explica que se refuercen en lugar de competir (véase 4.2).

- **CON-08 → CON-02.** Poder comprobar una regla sin levantar el servidor reduce el riesgo de cambiarla. El estándar lo declara como propósito de la dirección de dependencias `E 2.2`, y el propio equipo es a la vez autor de las reglas y probador `C`.
- **CON-09 → CON-03.** Un registro que identifica operación y entidad afectada `E 9.3` es lo que permite detectar que un resultado no corresponde a lo jugado. Refuerzo parcial: el registro ayuda a detectarlo, no a impedirlo.

### Aproximadamente independientes

- **CON-05 y CON-08.** Aislar partidas simultáneas y poder probar las reglas aisladas se atienden en planos distintos: una regla del dominio se comprueba igual haya una partida o veinte. Atender uno apenas modifica al otro.

### Tensiones

- **TEN-A · CON-09 frente a CON-06.** El estándar exige que el mensaje de registro lleve «el identificador de la entidad afectada, como el jugador» `E 9.3` y a la vez prohíbe registrar datos personales del jugador `E 9.4`. Ambas reglas son del mismo documento y se tocan directamente. Cuánta identificación es diagnóstico y cuánta es dato personal no está resuelto.
- **TEN-B · CON-04 frente a CON-06.** Un ranking histórico y una lista de amigos `C` exigen conservar información asociada a personas a lo largo del tiempo; CON-06 empuja a conservar el mínimo. La tensión aparece en cuanto se decida qué guardar, y hoy no hay ninguna fuente que lo delimite.
- **TEN-C · CON-01 frente al ritmo de la partida.** El turno dura como máximo 90 s y vence solo `R 5.1`; la ventana de reconexión del jugador activo es exactamente lo que quede de ese turno `R 5.2`. Favorecer al desconectado alarga la espera de los demás; favorecer el ritmo puede dejar una ventana de pocos segundos. Las reglas fijan las duraciones pero no dicen si el reloj se detiene.
- **TEN-D · CON-02 frente a la ausencia de evidencia sobre qué cambiará.** Las cartas maestras son un extra sujeto a tiempo disponible `C`, y las reglas declaran fijos el tablero `R 1.2` y las 3 rondas `R 1.3`. Construir flexibilidad donde las reglas declaran fijeza es coste sin demanda; no construirla donde el documento sigue en borrador es riesgo. No se resuelve aquí.

### Presión sin ser tensión

**CON-06 sobre CON-04 y CON-07.** Igual que la privacidad en el ejemplo clínico de los apuntes, aquí la protección de datos personales no es uno de los atributos centrales del juego, pero presiona sobre qué se persiste y sobre cómo se distingue una cuenta de un invitado.

### Sin fundamento suficiente

No hay evidencia para afirmar relación entre **CON-05** y **CON-01**. Que varias partidas corran a la vez y que un jugador se reconecte parecen tocarse, pero ninguna fuente describe cómo se localiza una partida ni qué comparten entre sí. Queda sin clasificar.

---

## 6. Dependencias

**Criterio.** Se registra una dependencia cuando un elemento **no puede determinarse, calcularse o existir sin el otro**: cambiar el primero cambia el segundo. Que dos elementos interactúen no basta. Al final se listan las relaciones descartadas por no cumplir el criterio.

Se separan en **externas** —el elemento del que se depende está fuera del sistema o fuera del control del equipo— e **internas**.

### 6.1 Dependencias externas

| ID | Qué depende | De qué depende | Por qué existe | Evidencia |
|---|---|---|---|---|
| **DEP-E1** | El registro de eventos de todo el sistema | La biblioteca **log4net** y su interfaz `ILog` | El estándar la impone como único medio: «no se escribe en la consola ni en un archivo por otros medios» | `E 9.1` |
| **DEP-E2** | El manejo de errores del código | Los **tipos de excepción del framework** (`ArgumentException` y derivadas, `InvalidOperationException`) | El estándar obliga a usar los tipos predefinidos «siempre que apliquen», para que el llamador los capture sin conocer tipos propios | `E 8.7` |
| **DEP-E3** | La continuidad de la participación de un jugador | La **conexión de red** entre su cliente y el servidor | Es la fuente del fallo que las reglas de reconexión existen para atender | `R 5.2` |
| **DEP-E4** | Las reglas implementadas en el sistema | El **documento de reglas**, hoy en v3.0 y estado borrador | El documento es la fuente normativa; cada versión suya redefine el comportamiento correcto del dominio | `R` portada |
| **DEP-E5** | La forma del código: capas, nombrado, registro, pruebas | El **estándar de codificación** | Es obligatorio para todos los integrantes en construcción, revisión y mantenimiento | `E 1` |
| **DEP-E6** | El vencimiento del turno y el cómputo de las ventanas de reconexión | Una **fuente de tiempo** con autoridad | Las reglas hacen que una consecuencia irreversible —perder el turno, ser retirado— dependa de un plazo medido. Sin una referencia temporal única no puede determinarse cuándo vence | `R 5.1`, `R 5.2` |

- **Sobre DEP-E4.** Es la dependencia externa más consecuente del proyecto: el elemento del que depende el núcleo del sistema todavía no está aprobado. Es también lo que sostiene CON-02.
- **Marco de pruebas — no confirmado.** Los ejemplos del estándar usan el atributo `[Fact]`, pero el documento **no nombra ningún marco de pruebas en su texto**. Dado que sus ejemplos pertenecen a otro juego y se ha decidido no moverlos `C`, no se afirma esta dependencia. Queda pendiente de confirmar con el equipo.

### 6.2 Dependencias internas del dominio

| ID | Qué depende | De qué depende | Por qué existe | Evidencia |
|---|---|---|---|---|
| **DEP-I1** | Turnos de cada ronda y construcciones que recibe cada jugador | Cantidad de jugadores, ronda y turno | Declarado literalmente: «ambos valores dependen de la cantidad de jugadores, la ronda y el turno». Es la única dependencia que el documento nombra con esa palabra | `R 1.3` |
| **DEP-I2** | Puntuación que un castillo otorga a un jugador | Superficie del castillo, nivel de la torre donde está su caballero, y que sea **el único** caballero propio en ese castillo | La fórmula multiplica superficie por nivel, y la condición de exclusividad decide si hay puntos o no | `R 4.1` |
| **DEP-I3** | Superficie de un castillo | Qué torres forman ese castillo en ese momento | El castillo es una construcción inicial fijada que **se extiende durante la partida**, luego su superficie cambia con las construcciones colocadas | `C`, `R 2.2` |
| **DEP-I4** | Legalidad de usar las cartas 5 y 8 | Estado geométrico del castillo afectado | La operación no debe partir el castillo ni separar sus torres, y no puede retirarse el nivel 1 de una torre de una sola altura. Un jugador puede tener la carta y no poder usarla | `R 2.4` |
| **DEP-I5** | Puntos de acción disponibles en un turno | Si el jugador usó la carta 1 o la carta 2 en ese turno | El presupuesto no es constante: se suma la diferencia sobre los 5 PA base. Las dos cartas no pueden usarse a la vez | `R 3.1`, `R 2.4`, `C` |
| **DEP-I6** | Quién coloca el rey en las rondas 2 y 3 | Puntuación parcial de todos los jugadores hasta ese momento | La regla designa al jugador con menor puntuación, luego exige una puntuación calculada durante la partida y no solo al final | `R 2.3` |
| **DEP-I7** | Que exista esa puntuación parcial | El corte de fin de ronda | El rey aplica su efecto al final de la ronda, y ese efecto forma parte de la puntuación con la que se decide quién coloca el rey en la siguiente | `C`, `R 4.2` |
| **DEP-I8** | Puntos que el rey otorga a un jugador | Que tenga un caballero en el mismo castillo **y** el mismo nivel que el rey, y la ronda en curso | La condición es doble y el valor cambia por ronda: 5, 10 y 15 | `R 4.2` |
| **DEP-I9** | Puntuación final de un jugador | Cartas de acción obtenidas y **no** utilizadas | Cada carta no usada equivale a 1 punto al final de la partida, lo que hace que gastar una carta tenga un coste diferido | `R 4.3` |
| **DEP-I10** | Duración de la ventana de reconexión | De quién era el turno en el instante de la caída | Turno propio: el tiempo restante de ese turno. Turno ajeno: 90 s. La ventana no es un parámetro fijo, se deriva del estado de la partida | `R 5.2` |
| **DEP-I11** | Que la partida termine anticipadamente | Cantidad de jugadores que quedan, que a su vez depende de los retiros por reconexión vencida | Con menos de 2 jugadores la partida termina y gana el que queda. El retiro es lo que puede provocarlo | `C`, `R 5.2` |

- **Cadena más larga del dominio.** DEP-I6 → DEP-I7 → DEP-I2 → DEP-I3: quién coloca el rey depende de la puntuación parcial, que depende del corte de ronda, que depende de la puntuación de cada castillo, que depende de qué torres lo componen en ese momento. Un cambio en cualquier eslabón se propaga hasta la designación del colocador del rey.
- **Dependencia secundaria de DEP-I6.** El empate en la menor puntuación se resuelve al azar `R 2.3`, luego esa designación depende además de una fuente de aleatoriedad única: ningún cliente puede sortear por su cuenta y que los demás lo acepten.
- **Punto abierto en DEP-I4.** `C` confirma que un nivel puede moverse de un castillo a otro, mientras que `R 2.2` prohíbe unir ortogonalmente torres de castillos distintos. Si esa unión es posible, la comprobación de DEP-I4 tiene que cubrir también el castillo de destino, no solo el de origen. Sin resolver (cuestión 8).

### 6.3 Dependencias internas del sistema

| ID | Qué depende | De qué depende | Por qué existe | Evidencia |
|---|---|---|---|---|
| **DEP-S1** | Dónde puede leerse la configuración de una partida | La dirección de dependencias entre capas | El dominio no puede depender de la persistencia: si la configuración se guarda fuera del código, tiene que llegarle como argumento desde la capa de servicios | `E 2.2` |
| **DEP-S2** | Que el resultado de una acción sea único y aceptado por todos | Que exista un solo punto donde se determina | El cliente no decide el resultado de ninguna acción del juego, luego la determinación depende enteramente del servidor | `E 2.1` |
| **DEP-S3** | El ranking histórico | Que toda partida produzca un resultado registrable | Sin desenlace no hay nada que archivar. La regla de fin anticipado garantiza que incluso una partida rota produzca un ganador | `C` |
| **DEP-S4** | Que un resultado pueda atribuirse en el ranking | Que el jugador tenga identidad persistente | El ranking asocia resultados a alguien; un invitado no tiene esa identidad. Es la dependencia que hace que CON-07 condicione a CON-04 | `C` |
| **DEP-S5** | Poder diagnosticar un fallo sin reproducirlo | Que el mensaje de registro lleve la operación y el identificador de la entidad afectada | El estándar lo argumenta explícitamente: un mensaje sin ese contexto obliga a reproducir el fallo | `E 9.3` |
| **DEP-S6** | Poder ejercitar las reglas sin levantar el servidor ni abrir la ventana | Que el dominio no dependa de ninguna otra capa | Es el propósito declarado de la regla de dependencias, no una consecuencia inferida | `E 2.2` |

### 6.4 Dependencias entre decisiones ya tomadas

| Decisión | Depende de | Por qué |
|---|---|---|
| **D-09** fin de partida con menos de 2 jugadores | **D-04** ventanas de reconexión y retiro | El retiro por reconexión vencida es el mecanismo que puede llevar la partida por debajo de 2 jugadores. Sin D-04, D-09 no tendría caso que atender |
| **D-06** persistencia, ranking e histórico | **D-05** cuentas y modo invitado | El histórico atribuye resultados a una identidad; qué identidades existen lo fija D-05 |
| **D-07** partidas simultáneas | **D-02** el cliente no decide resultados | Con la determinación concentrada en el servidor, el estado de cada partida no queda repartido entre clientes. D-02 no fue tomada por esto, pero D-07 se apoya en ella |
| Cualquier decisión futura sobre **CON-02** | **D-01** dominio sin dependencias | D-01 acota de antemano dónde puede vivir la configuración, antes de que se decida cómo se almacena |

### 6.5 Dependencias del proyecto, no del sistema

| Qué depende | De qué depende | Por qué |
|---|---|---|
| La resolución de las 18 cuestiones abiertas | **STK-02** | Es el único stakeholder con autoridad: `C` le atribuye las reglas, el desarrollo, el diseño, las pruebas y el servidor. No hay contraparte externa que pueda cerrarlas |
| Poder delimitar **CON-05** | La cifra de partidas simultáneas | Sin ella no puede distinguirse aislamiento de escalabilidad (cuestión 15) |
| Poder delimitar **CON-04** y **CON-06** | Qué datos de una cuenta se conservan y qué alcance tiene «amigos» | Es lo que hoy impide avanzar en la tensión TEN-B (cuestiones 13 y 14) |
| La aceptación del trabajo | **STK-03** y sus criterios | Los criterios no están definidos, luego la dependencia existe pero su contenido se desconoce |

### 6.6 Relaciones descartadas como dependencia

| Relación | Por qué no es dependencia |
|---|---|
| Caballero ↔ caballero | Un caballero no puede colocarse sobre una torre ocupada por otro `R 2.1`. Es una **restricción de validación** contra el estado del tablero: ninguno de los dos caballeros cambia porque el otro cambie |
| Rey ↔ cartas resumen | Coexisten en la misma partida y la misma ronda, pero ninguno determina al otro. Contexto compartido, no dependencia |
| Jugador ↔ jugador | Compiten y sus puntuaciones se comparan, pero la dependencia real está entre la puntuación agregada y la designación del colocador del rey (DEP-I6), no entre los jugadores como actores |
| Cliente ↔ servidor | La dependencia está declarada en un solo sentido: el cliente depende de los servicios y «una capa no conoce a la que la usa» `E 2.2`. Tratarla como bidireccional contradiría el estándar |
| Partida ↔ partida simultánea | **CON-05 es precisamente la exigencia de que esta dependencia no exista.** Registrarla como dependencia invertiría el sentido del concern |

## 7. Cadenas hacia atrás: decisión → propiedad → stakeholder → concern

Partimos de decisiones que **ya existen** y preguntamos qué propiedad hizo que importaran, a quién le importaba y qué concern había detrás.

**D-01 · «El dominio no depende de ninguna otra capa» `E 2.2`**
→ *Propiedad:* las reglas del juego pueden ejercitarse aisladas del servidor y de la interfaz.
→ *Stakeholder:* STK-02, rol probador y rol mantenimiento.
→ *Concern:* CON-08, y por refuerzo CON-02.
→ *Evidencia:* el estándar enuncia el propósito de forma literal, no inferida.

**D-02 · «El cliente no decide el resultado de ninguna acción del juego» `E 2.1`**
→ *Propiedad:* existe un único punto donde se determina el resultado de una acción.
→ *Stakeholder:* STK-01 y STK-02.
→ *Concern:* CON-03. Habilita CON-05, porque el resultado no queda repartido entre clientes.

**D-03 · «Registro con log4net, un logger por clase, sin datos sensibles» `E 9.1, 9.3, 9.4`**
→ *Propiedad:* los fallos quedan atribuibles a una clase y a una entidad concreta, sin exponer datos del jugador.
→ *Stakeholder:* STK-02 rol operación, y STK-01.
→ *Concerns:* CON-09 y CON-06 a la vez. Es la decisión donde nace TEN-A.

**D-04 · «Ventana de reconexión acotada y retiro al vencer» `R 5.2`**
→ *Propiedad:* la partida avanza sin quedar bloqueada por un ausente, y el ausente conserva una oportunidad acotada.
→ *Stakeholder:* STK-01 en dos roles opuestos —el desconectado y los que esperan— y STK-02 como autor de la regla.
→ *Concern:* CON-01. La tensión TEN-C es interna a esta decisión.

**D-05 · «Cuentas de jugador y modo invitado» `C`**
→ *Propiedad:* se puede entrar a jugar sin registro previo y, con registro, atribuir resultados a una identidad.
→ *Stakeholder:* STK-01.
→ *Concerns:* CON-07 y CON-04. Lo que la decisión aún no fija es qué ocurre con el resultado de un invitado.

**D-06 · «Persistencia de partidas, ranking histórico y amigos» `C`**
→ *Propiedad:* el resultado de una partida sobrevive al fin de la partida.
→ *Stakeholder:* STK-01 con cuenta, STK-02.
→ *Concern:* CON-04. Presiona CON-06 y origina TEN-B.

**D-07 · «Deben poder existir partidas simultáneas» `C`**
→ *Propiedad:* varias partidas coexisten sin que el estado de una alcance a otra.
→ *Stakeholder:* STK-02 rol operación, STK-01.
→ *Concern:* CON-05.

**D-08 · «Turnos y construcciones definidos por cartas resumen según jugadores, ronda y turno» `R 1.3`**
→ *Propiedad:* parte del comportamiento de la partida está expresado como datos tabulados y no como enunciado de regla.
→ *Stakeholder:* STK-02 rol autor de reglas.
→ *Concern:* CON-02. Es la evidencia más sólida de que la modificabilidad no es una hipótesis: la variabilidad ya está declarada en el documento.

**D-09 · «Con menos de 2 jugadores la partida termina y gana el que queda» `C`**
→ *Propiedad:* la partida siempre alcanza un desenlace definido aunque pierda jugadores por desconexión.
→ *Stakeholder:* STK-01, STK-02.
→ *Concerns:* CON-01 y CON-04 —el histórico necesita que toda partida produzca un resultado.

**D-10 · «El rey aplica su efecto al final de la ronda» `C`**
→ *Propiedad:* existe un corte por ronda en el que la puntuación queda establecida.
→ *Stakeholder:* STK-02 rol autor de reglas.
→ *Concern:* CON-03. Es lo que hace determinable «el jugador con menor puntuación hasta ese momento» de `R 2.3`.
→ *Sigue sin definirse:* si la puntuación de los castillos `R 4.1` se calcula también en ese corte o de forma continua.

---

## 8. Cadenas hacia adelante: stakeholder → concern → driver → atributo → decisión → evidencia

Partimos de las presiones identificadas. Donde la decisión aún no se ha tomado, se dice.

**FW-01** · STK-02 (probador) → **CON-08** → *driver:* las reglas son el núcleo del sistema y cambian entre versiones del documento → *atributo:* verificabilidad de las reglas → *decisión:* dominio sin dependencias y proyecto de pruebas con nombrado propio → *evidencia:* `E 2.2` declara el propósito; `E 10.1` desactiva en el proyecto de pruebas la regla que prohíbe el guion bajo.

**FW-02** · STK-01 → **CON-01** → *driver:* la conexión puede caerse en cualquier momento y el turno dura 90 s → *atributo:* continuidad de la participación → *decisión:* ventanas diferenciadas según de quién sea el turno, con retiro al vencer → *evidencia:* `R 5.2`, complementada por `C` para el caso de quedar menos de 2 jugadores.

**FW-03** · STK-01 con cuenta → **CON-04** → *driver:* habrá ranking histórico y amigos → *atributo:* permanencia del resultado más allá de la partida → *decisión:* existirá persistencia de partidas → *evidencia:* `C`. **El mecanismo no está decidido** y no se propone ninguno aquí.

**FW-04** · STK-01 y STK-02 → **CON-06** → *driver:* existen cuentas y relaciones entre jugadores → *atributo:* mínimo de información personal conservada → *decisión parcial:* prohibición de escribir datos sensibles en el registro → *evidencia:* `E 9.4`. **Cubre solo el registro**; sobre qué se almacena en la persistencia no hay decisión ni fuente.

**FW-05** · STK-02 (rol autor de reglas) → **CON-02** → *driver:* documento en v3.0 borrador, valores ya tabulados por número de jugadores `R 1.3`, y cartas maestras previstas como extra `C` → *atributo:* coste de cambio de reglas y parámetros → *decisión:* **no tomada**. Hoy existe la intención expresada en el escenario del equipo, pero ninguna decisión de proyecto la respalda todavía.

**FW-06** · STK-02 (rol operación) y STK-01 → **CON-05** → *driver:* deben poder existir partidas simultáneas, cada una con sus propios plazos corriendo `C`, `R 5.1` → *atributo:* aislamiento entre partidas → *decisión:* **no tomada**. Falta además el dato de cuántas.

---

## 9. Trazabilidad

| Stakeholder | Concern | Atributo de calidad | Decisión existente | Origen |
|---|---|---|---|---|
| STK-02 | CON-08 | Testabilidad | Dominio sin dependencias; pruebas con nombrado propio | `E 2.2`, `E 10` |
| STK-01, STK-02 | CON-03 | Corrección funcional | El cliente no decide resultados; rey único; efecto al final de ronda | `E 2.1`, `C` |
| STK-02 | CON-09 | Analizabilidad | Registro por clase con operación y entidad | `E 9.1`, `E 9.3` |
| STK-01, STK-02 | CON-06 | Seguridad — confidencialidad | Prohibición de registrar datos sensibles (solo registro) | `E 9.4` |
| STK-01 | CON-01 | Disponibilidad | Ventanas de reconexión; retiro; fin con menos de 2 jugadores | `R 5.2`, `C` |
| STK-01 | CON-07 | — sin atributo | Cuentas y modo invitado | `C` |
| STK-01, STK-02 | CON-04 | — sin atributo | Persistencia de partidas, ranking, amigos | `C` |
| STK-02, STK-01 | CON-05 | Corrección funcional bajo concurrencia | Partidas simultáneas (sin decisión de cómo) | `C` |
| STK-02 | CON-02 | Modificabilidad | **Ninguna** | — |
| STK-03 | Conformidad con `E` y `R` | — sin atributo | **Ninguna documentada** | — |

---

## 10. Cuestiones abiertas

Solo las que siguen sin respuesta después de las 13 aclaraciones. Cada una indica qué parte del análisis bloquea.

**Sobre las reglas**

1. ¿Un «turno» pertenece a un jugador o es una vuelta completa de todos? `R 5.1` sugiere lo primero, `R 1.3` y `R 3.1` admiten lo segundo. Bloquea CON-02 y el dimensionamiento del reloj.
2. ¿Cómo se determina el ganador en una partida que termina normalmente, y cómo se desempata? `C` resolvió el caso de quedar menos de 2 jugadores; el caso normal sigue sin definirse. Bloquea CON-04.
3. ¿En qué momento se calcula la puntuación de los castillos `R 4.1`? `C` fijó que el rey aplica al final de la ronda, pero no si los castillos se puntúan en ese mismo corte. Bloquea CON-03.
4. ¿Hay altura máxima de torre? `R 4.1` multiplica por el nivel y ninguna regla lo acota.
5. ¿Los caballeros y las construcciones permanecen en el tablero entre rondas? Ninguna fuente lo dice.
6. ¿Cómo entran los caballeros por primera vez y en qué orden juegan los jugadores? `R 2.3` presupone un orden de colocación inicial que no está definido en ninguna parte.
7. Al obtener una carta del mazo por 1 PA `R 2.4`, ¿el jugador la elige o se le entrega al azar?
8. `C` confirma que un nivel puede moverse de un castillo a otro. ¿Puede esa operación unir dos castillos, si `R 2.2` prohíbe unir ortogonalmente torres de castillos distintos?
9. La carta 3 cita «la condición de realizar el movimiento dentro del mismo castillo» como existente, pero `R 2.1` no la enuncia. ¿Falta en 2.1 o sobra en la carta 3?

**Sobre el contexto del proyecto**

10. ¿Qué ocurre con las piezas, cartas y puntos de un jugador retirado cuando quedan 2 o más jugadores? Bloquea CON-01 y CON-03.
11. Si un jugador es retirado, ¿se recalcula la configuración de turnos y construcciones, que depende del número de jugadores `R 1.3`? Bloquea CON-01 y CON-02.
12. ¿El resultado de una partida jugada por invitados entra al ranking? Bloquea CON-07 y CON-04.
13. ¿Qué datos de una cuenta se conservan y cuáles quedan fuera? Es lo que hoy impide avanzar en TEN-B.
14. ¿Qué alcance tiene «amigos»? Sin él, CON-04 y CON-06 no pueden delimitarse.
15. ¿Cuántas partidas simultáneas? Bloquea CON-05.
16. ¿Qué es el «manejador de partidas» del escenario de Modificabilidad y a qué capa pertenece? No aparece en `R` ni en `E`.
17. La premisa de configurar «la distribución inicial de los castillos» convive con `R 1.2` —distribución fija— y con `C` —el castillo es una construcción inicial fijada del tablero—. Falta explicitar qué es entonces lo configurable.

18. ¿Qué catálogo de atributos de calidad adopta el proyecto? Los nombres usados en la sección 4 son los habituales, pero ninguna fuente fija un marco de referencia. Sin él, las clasificaciones de 4.2 son sostenibles pero no verificables contra un catálogo acordado. Bloquea poder decir que un concern «no tiene atributo» de forma definitiva.

---

## 11. Correcciones respecto al análisis previo

Las aclaraciones cierran tres puntos que el documento anterior señalaba como abiertos:

- **La sección 6 vacía no es una omisión.** Está vacía porque las observaciones hasta ese punto ya estaban atendidas `C`. Queda retirada como hallazgo.
- **Las cartas maestras no son una contradicción con las reglas.** Son un extra sujeto a disponibilidad de tiempo `C`, es decir, funcionalidad no comprometida. Se reclasifican como *fuera del contexto actual* y como driver de CON-02, no como conflicto.
- **Qué es un castillo queda definido** `C`: construcción inicial fijada del tablero, extensible durante la partida. Con ello, `R 1.2` y la noción de castillo dejan de estar en conflicto; lo que sigue abierto es qué significa entonces «configurar la distribución inicial de los castillos» (cuestión 17).

Se mantienen abiertas las demás cuestiones de la sección 10.

---

## 12. Recomendaciones

Todo lo de abajo es **propuesta del análisis**, no hecho establecido ni regla del juego. Cada una dice en qué se apoya y qué se pierde al tomarla. La decisión es del equipo (STK-02), que tiene la autoridad.

### Sobre las tensiones

**TEN-A · Registro frente a datos personales → registrar solo el identificador interno.**
`E 9.3` pide el identificador de la entidad afectada y `E 9.4` prohíbe los datos personales. Ambas se satisfacen si el registro lleva el identificador interno del jugador dentro de la partida y el identificador de la partida, y nunca nombre de usuario, correo, token ni nada de la cuenta. El diagnóstico sigue siendo posible porque el equipo opera el servidor `C` y puede cruzar ese identificador con la partida.
*Coste:* diagnosticar un caso reportado por un jugador exige un paso extra para llegar de la persona al identificador.

**TEN-B · Histórico frente a privacidad → guardar el resultado, no la partida entera.**
El ranking necesita quién jugó, qué puntuación obtuvo, cuándo y contra cuántos. No necesita la secuencia de jugadas ni datos de perfil. Guardar el mínimo que el ranking consume cierra la tensión antes de que aparezca.
*Coste:* si más adelante se quiere repetir o revisar una partida, ese dato ya no existirá. Conviene decidirlo ahora y no después.

**TEN-C · Reloj durante la desconexión → que siga corriendo.**
Es la lectura que sostiene el texto de `R 5.2` («dispondrá del tiempo restante de ese turno») y la única compatible con `R 5.1`, que garantiza a los demás que ningún turno pasa de 90 s. Pausar el reloj rompería esa garantía para los otros jugadores, que son mayoría en la mesa.
*Coste:* un jugador que cae con pocos segundos restantes pierde el turno de hecho. Es aceptable porque pierde el turno, no la partida: la siguiente ventana vuelve a ser de 90 s completos.
*Acción:* enunciarlo explícitamente en `R 5.2`, que hoy solo lo insinúa.

**TEN-D · Cuánta flexibilidad construir → externalizar dos cosas y nada más.**
Externalizar las tablas de cartas resumen `R 1.3` y los costes de puntos de acción `R 3.2`: son los únicos puntos donde el propio documento ya declara variabilidad. Dejar en código lo que las reglas declaran fijo —tablero 8×8, 3 rondas, 5 caballeros, catálogo de 8 cartas—.
*Por qué aquí:* es flexibilidad con demanda demostrada, no anticipada. Las cartas maestras, al ser un extra sujeto a tiempo `C`, no justifican construir hoy un mecanismo de extensión.

### Sobre las cuestiones abiertas de reglas

**Cuestión 1 · «Turno» → turno de un jugador.**
Es la única lectura donde `R 5.1` tiene sentido: los 90 s y la frase «el turno del jugador finalizará y se pasará al siguiente turno» describen a una sola persona actuando. Bajo la otra lectura, 90 s tendrían que repartirse entre 4 jugadores.
*Consecuencia a verificar antes de adoptarla:* con 4 jugadores y 4 turnos en la ronda 1, cada jugador jugaría una sola vez esa ronda. Si eso no es lo que se quiere, la tabla de `R 1.3` necesita revisión, no la interpretación.

**Cuestión 2 · Ganador → mayor puntuación total; desempate por número de castillos puntuados.**
La primera mitad es lo que `R 4.1`–`R 4.3` presuponen sin decirlo y solo falta escribirlo. Para el desempate propongo el número de castillos en los que el jugador puntuó, porque usa información que el sistema ya calcula y no introduce azar en el resultado final —a diferencia de `R 2.3`, donde el azar solo decide quién coloca una pieza, no quién gana—.

**Cuestión 3 · Momento de la puntuación → puntuar castillos en el mismo corte de fin de ronda que el rey.**
`C` ya fijó que el rey aplica al final de la ronda, y `R 2.3` necesita «la menor puntuación hasta ese momento» al empezar la siguiente. Un único corte por ronda cubre ambas y evita mantener una puntuación viva durante el turno.

**Cuestión 8 · Mover un nivel entre castillos → prohibir el movimiento si uniría dos castillos.**
`C` permite mover un nivel de un castillo a otro, pero `R 2.2` prohíbe unir ortogonalmente torres de castillos distintos. Tratar la unión como movimiento ilegal conserva las dos reglas sin tocar ninguna.

**Cuestión 9 · «Mismo castillo» → añadir la condición a `R 2.1`.**
La carta 3 la cita como existente. O se enuncia en el apartado de movimiento, o se borra de la carta 3. Recomiendo enunciarla: es la lectura que hace que la carta 3 sea una excepción a algo, que es como está redactada.

### Sobre las cuestiones abiertas del proyecto

**Cuestión 10 · Jugador retirado → sus piezas se quedan en el tablero y dejan de puntuar.**
Retirarlas alteraría la superficie de los castillos `R 4.1` y cambiaría la puntuación de los demás por un hecho ajeno al juego. Dejarlas quietas mantiene estable el tablero de todos.

**Cuestión 11 · Configuración tras un retiro → no recalcular.**
La configuración se fija al inicio con el número de jugadores que empezaron `R 1.3`. Recalcularla a mitad de partida cambiaría el número de turnos restantes y las construcciones ya repartidas.

**Cuestión 12 · Invitados → juegan y aparecen en el marcador de la partida, pero no entran al ranking histórico.**
El ranking atribuye resultados a una identidad persistente `C`, y un invitado no la tiene. Así CON-07 se sostiene sin forzar CON-04.

**Cuestión 16 · «Manejador de partidas» → en `Game.Services`.**
Si su trabajo es recuperar configuración y aplicarla al arrancar la partida, `E 2.2` lo ubica ahí: el dominio no puede leer de persistencia y debe recibir esos valores como argumento. Con esta ubicación, el escenario de Modificabilidad deja de contradecir al estándar.

**Cuestión 17 · Qué es configurable → los parámetros, no la geometría.**
Dado que el castillo es una construcción inicial fijada del tablero `C` y que `R 1.2` declara fija la distribución, recomiendo reformular el escenario de Modificabilidad para que hable de las tablas de `R 1.3` y los costes de `R 3.2`, que es lo que sí varía, y retirar la mención a configurar la distribución inicial de castillos.

### Sobre los escenarios ya redactados

**Medida de Modificabilidad → expresarla en coste de cambio.**
La medida actual describe que la configuración se aplique al iniciar la partida, lo cual es un requisito funcional. Propongo sustituirla por algo como: cambiar un valor de las tablas de `R 1.3` o de los costes de `R 3.2` no debe requerir tocar `Game.Domain` ni recompilar. Es comprobable y sí mide coste de cambio.

**Medida de Disponibilidad → adoptar los plazos de `R 5.2`.**
«Mientras la partida continúe activa» es más laxo que la regla y no fija plazo. Los valores de `R 5.2` —tiempo restante del turno, o 90 s— son concretos y verificables. Añadir al escenario el desenlace del retiro, que hoy queda fuera.

### Lo que no recomiendo decidir todavía

- **Cuántas partidas simultáneas.** Es un número que depende de dónde se vaya a ejecutar el servidor, y eso no está decidido. Fijarlo ahora sería inventar un requisito.
- **Mecanismo de persistencia.** CON-04 está claro; la forma de resolverlo depende de la cuestión 13, que sigue abierta.
- **Alcance de «amigos».** Sin saber qué hace un amigo dentro del sistema, cualquier recomendación sería relleno.
