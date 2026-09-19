# Analisis de Torres en formato APA

> Convertido desde el .docx original, que sigue siendo el documento normativo del equipo.

**Análisis de Stakeholders, Concerns, Dependencias, Tensiones y Preguntas del Proyecto Juego de Torres**

Jesús Bautista Hernández y Valentín Benavides Martínez

[Nombre de la institución]

[Clave y nombre del curso]

[Nombre del profesor]

2 de septiembre de 2026

**Análisis de Stakeholders, Concerns, Dependencias, Tensiones y Preguntas del Proyecto Juego de Torres**

**Introducción**

Este documento reúne los stakeholders, los concerns, las dependencias, las tensiones y las preguntas identificadas para el desarrollo del juego de Torres. Su objetivo es dejar por escrito qué le preocupa a cada parte interesada, qué elementos dependen de otros y qué asuntos siguen sin definirse. Cuando no existe información suficiente para determinar algo, se señala como pregunta pendiente en lugar de suponer una respuesta.

**Tabla 1**

*Fuentes utilizadas en el análisis*

| Fuente | Versión y fecha | Uso en este documento |
|---|---|---|
| Reglas del Juego de Torres | Versión 3.0, en estado de borrador, 29 de agosto de 2026 | Fuente normativa de las reglas del juego |
| Estándar de codificación | Versión 7, 20 de agosto de 2026 | Reglas de construcción del código: capas, nombrado, registro de eventos y pruebas |
| Comunicación con el equipo de desarrollo | 2 de septiembre de 2026 | Aclaraciones sobre puntos que los documentos no cubren |

**Stakeholders**

Un stakeholder es una parte que introduce preocupaciones relevantes sobre el sistema, su construcción, su operación o su evaluación. No debe confundirse con un actor, que es el rol que participa en una interacción del sistema. Un mismo sujeto puede ser las dos cosas, una sola o ninguna.

**Tabla 2**

*Stakeholders identificados*

| Stakeholder | Qué representa | ¿Es también actor? | Autoridad dentro del proyecto |
|---|---|---|---|
| Jugador | La persona que participa en una partida y ejecuta acciones mediante puntos de acción | Sí | Ninguna sobre las reglas ni sobre el sistema |
| Equipo de desarrollo | Los dos integrantes encargados de las reglas, el desarrollo, el diseño, las pruebas y el manejo del servidor | Sí, cuando opera el servidor | Máxima; es el único que puede cerrar las preguntas pendientes |
| Receptor de la entrega | Quien recibe y evalúa el resultado del proyecto | No | Sobre la aceptación del trabajo, no sobre el diseño |

**Jugador**

Una partida admite de dos a cuatro jugadores y no pueden incorporarse jugadores adicionales una vez iniciada (Reglas del Juego de Torres, 2026, apartado 1.1). Los siguientes estados y roles corresponden al mismo stakeholder y no constituyen stakeholders distintos.

**Tabla 3**

*Estados y roles del jugador*

| Criterio | Estado o rol | Situación que lo justifica |
|---|---|---|
| Identidad | Jugador con cuenta registrada | El equipo confirmó que existirán cuentas de jugador |
| Identidad | Jugador invitado | El equipo confirmó que también se podrá jugar como invitado |
| Conexión | Conectado | Estado normal durante la partida |
| Conexión | Desconectado dentro del plazo | Conserva su lugar mientras no venza el plazo de reconexión (apartado 5.2) |
| Conexión | Retirado de la partida | Al vencer el plazo de reconexión el jugador es retirado (apartado 5.2) |
| Posición en la partida | Jugador activo | Es quien tiene el turno, con una duración máxima de noventa segundos (apartado 5.1) |
| Posición en la partida | Jugador en espera | Su plazo de reconexión es de noventa segundos completos (apartado 5.2) |
| Rol temporal | Quien coloca al rey en la ronda 1 | Corresponde al último jugador que colocó un caballero (apartado 2.3) |
| Rol temporal | Quien coloca al rey en las rondas 2 y 3 | Corresponde al jugador con menor puntuación hasta ese momento (apartado 2.3) |

**Equipo de desarrollo**

El equipo confirmó que concentra cinco roles. Esto tiene una consecuencia concreta: un cambio en las reglas no requiere negociarse con nadie externo, pero tampoco existe una contraparte que valide desde fuera una regla ambigua.

**Tabla 4**

*Roles que concentra el equipo de desarrollo*

| Rol | Responsabilidad asociada |
|---|---|
| Autor de las reglas | Define y modifica el documento de reglas, que se encuentra en su versión 3.0 y en estado de borrador |
| Diseñador y desarrollador | Construye el sistema conforme al Estándar de codificación |
| Responsable de las pruebas | Aplica el régimen de pruebas de la sección 10 del Estándar de codificación |
| Operador del servidor | Maneja el servidor y consume los registros de eventos; en este rol es también actor |
| Responsable ante la entrega | Responde por el resultado frente al receptor de la entrega |

**Receptor de la entrega**

- Interés: que el juego corresponda con el documento de reglas y que el código cumpla el Estándar de codificación.
- Efecto sobre el proyecto: convierte en obligatorias, y no opcionales, las restricciones sobre capas, nombrado, registro de eventos y pruebas.
- Pendiente por definir: su identidad y sus criterios de aceptación, por lo que no se le atribuyen otras preocupaciones.
**Concerns**

Un concern es un asunto de importancia para uno o más stakeholders. Nombra aquello sobre lo que se necesita poder responder, sin fijar todavía una solución de diseño. Se identifican nueve concerns; seis se corresponden con un atributo de calidad justificable con las fuentes y tres no.

**Tabla 5**

*Concerns identificados*

| Concern | Qué preocupa | Stakeholder | Atributo de calidad |
|---|---|---|---|
| Continuidad de la partida ante una desconexión | Que un jugador que pierde la conexión pueda retomar la partida y que mientras tanto siga siendo jugable para los demás | Jugador; equipo de desarrollo | Disponibilidad |
| Ajuste de reglas y parámetros | Que modificar reglas y valores de configuración no obligue a rehacer el sistema | Equipo de desarrollo | Modificabilidad |
| Correspondencia entre lo jugado y lo puntuado | Que el resultado refleje exactamente las acciones legales realizadas | Jugador; equipo de desarrollo | Corrección funcional |
| Conservación del resultado de las partidas | Que el resultado de una partida terminada siga disponible para el ranking histórico | Jugador con cuenta; equipo de desarrollo | No se asigna |
| Aislamiento entre partidas simultáneas | Que lo que ocurre en una partida no altere el estado ni el resultado de otra | Equipo de desarrollo; jugador | Corrección funcional en presencia de concurrencia |
| Información personal que se guarda y se registra | Que los datos personales conservados y escritos en los registros se mantengan en el mínimo necesario | Jugador; equipo de desarrollo | Seguridad, en su dimensión de confidencialidad |
| Juego con cuenta o como invitado | Que la forma de identificarse no impida jugar y que determine qué resultados pueden atribuirse después | Jugador | No se asigna |
| Verificación de las reglas del juego | Que el comportamiento de las reglas pueda comprobarse sin depender del servidor ni de la interfaz | Equipo de desarrollo | Testabilidad |
| Reconstrucción de lo ocurrido ante un fallo | Que ante un fallo se pueda saber qué operación falló y sobre qué elemento | Equipo de desarrollo | Analizabilidad |

**Tabla 6**

*Justificación de la clasificación de cada concern*

| Concern | Por qué corresponde el atributo indicado o por qué no se asigna ninguno |
|---|---|
| Continuidad de la partida ante una desconexión | El asunto es que el servicio siga prestándose ante una falla y que el jugador afectado se recupere dentro de un plazo. Las reglas ya fijan el plazo, su variación según de quién sea el turno y el retiro al vencer (apartado 5.2) |
| Ajuste de reglas y parámetros | El asunto es el costo de un cambio y no que el sistema funcione. El apartado 1.3 ya expresa como datos la cantidad de turnos y de construcciones, y el documento continúa en borrador en su versión 3.0 |
| Correspondencia entre lo jugado y lo puntuado | Se pide que el cálculo del resultado sea correcto según las reglas, no que esté protegido frente a un atacante. Pasaría a tocar la seguridad solo si el equipo declarara que la manipulación es un riesgo por atender, y ninguna fuente lo menciona |
| Conservación del resultado de las partidas | Describe lo que el sistema hace y no la cualidad con que lo hace. Se convertiría en atributo si se exigiera, por ejemplo, que ningún resultado se pierda ante una caída del servidor |
| Aislamiento entre partidas simultáneas | El asunto es que el resultado de cada partida siga siendo correcto con varias en curso. No se clasifica como escalabilidad ni como rendimiento porque no existe una cifra objetivo de partidas concurrentes |
| Información personal que se guarda y se registra | La exigencia es que cierta información no quede expuesta. El estándar lo argumenta al señalar que el registro se conserva y se comparte, por lo que deja de ser privado (sección 9.4). La evidencia cubre solo el registro y no lo que se almacena |
| Juego con cuenta o como invitado | Es una decisión sobre el alcance del sistema. Se vincularía con la usabilidad solo si el equipo declarara que jugar sin registrarse es un objetivo, y ninguna fuente lo enuncia |
| Verificación de las reglas del juego | No requiere inferencia: el estándar declara que el propósito de la dirección de las dependencias entre capas es que las reglas puedan probarse sin levantar el servidor ni abrir la ventana del juego (sección 2.2) |
| Reconstrucción de lo ocurrido ante un fallo | El estándar exige que el mensaje de registro incluya la operación y el identificador del elemento afectado, precisamente para no tener que reproducir el fallo (sección 9.3) |

**Tabla 7**

*Relaciones entre concerns*

| Concerns relacionados | Tipo de relación | Fundamento |
|---|---|---|
| Ajuste de reglas y parámetros, verificación de las reglas y reconstrucción de lo ocurrido ante un fallo | Se refuerzan | Corresponden a los tres atributos que componen la mantenibilidad y provienen del mismo stakeholder, por lo que poder comprobar una regla reduce el riesgo de modificarla |
| Aislamiento entre partidas simultáneas y verificación de las reglas | Aproximadamente independientes | Una regla del juego se comprueba igual haya una partida o varias en curso, por lo que atender uno apenas modifica al otro |

**Dependencias**

Se registra una dependencia cuando un elemento no puede determinarse, calcularse o existir sin otro, de modo que un cambio en el primero produce un cambio en el segundo. Que dos elementos interactúen no basta para considerarlos dependientes.

**Dependencias externas**

**Tabla 8**

*Dependencias respecto de elementos externos al sistema*

| Elemento dependiente | Elemento del que depende | Razón de la dependencia |
|---|---|---|
| Registro de eventos de todo el sistema | Biblioteca log4net y su interfaz ILog | El Estándar de codificación la impone como único medio y prohíbe escribir en la consola o en un archivo por otros medios (sección 9.1) |
| Manejo de errores del código | Tipos de excepción del marco de trabajo | El estándar obliga a usar los tipos ya definidos siempre que apliquen, para que quien llama pueda capturarlos sin conocer tipos propios del proyecto (sección 8.7) |
| Continuidad de la participación de un jugador | Conexión de red entre el cliente y el servidor | Es la falla que las reglas de reconexión existen para atender (apartado 5.2) |
| Reglas implementadas en el sistema | Documento de reglas, en versión 3.0 y en borrador | El documento es la fuente normativa del juego, por lo que cada versión suya redefine cuál es el comportamiento correcto |
| Forma del código, capas, nombrado y pruebas | Estándar de codificación | Es obligatorio para todos los integrantes en la construcción, la revisión y el mantenimiento (sección 1) |
| Vencimiento del turno y cálculo de los plazos de reconexión | Una fuente de tiempo con autoridad | Perder el turno o ser retirado depende de un plazo medido, y sin una referencia temporal única no puede determinarse cuándo vence (apartados 5.1 y 5.2) |

De estas dependencias, la del documento de reglas es la de mayor consecuencia, porque el elemento del que depende el centro del sistema todavía no está aprobado.

**Dependencias internas del juego**

**Tabla 9**

*Dependencias entre elementos de las reglas del juego*

| Elemento dependiente | Elemento del que depende | Razón de la dependencia |
|---|---|---|
| Turnos de cada ronda y construcciones por jugador | Cantidad de jugadores, ronda y turno | El documento de reglas lo declara de forma literal en el apartado 1.3 |
| Puntuación que otorga un castillo | Superficie del castillo, nivel de la torre del caballero y que sea el único caballero propio en ese castillo | La fórmula multiplica superficie por nivel, y la condición de exclusividad decide si hay puntos o no (apartado 4.1) |
| Superficie de un castillo | Torres que lo componen en ese momento | El castillo es una construcción inicial fijada del tablero que se extiende durante la partida, por lo que su superficie cambia |
| Legalidad de usar las cartas de acción 5 y 8 | Estado del castillo afectado | La operación no debe partir el castillo ni separar sus torres, y no puede retirarse el primer nivel de una torre de una sola altura (apartado 2.4) |
| Puntos de acción disponibles en un turno | Uso de la carta de acción 1 o de la carta 2 en ese turno | Ambas cartas no pueden usarse a la vez y suman la diferencia sobre los cinco puntos base (apartados 2.4 y 3.1) |
| Jugador que coloca al rey en las rondas 2 y 3 | Puntuación parcial de todos los jugadores hasta ese momento | La regla designa al jugador con menor puntuación, lo que exige una puntuación calculada durante la partida (apartado 2.3) |
| Existencia de esa puntuación parcial | Cierre de la ronda | El rey aplica su efecto al final de la ronda, y ese efecto forma parte de la puntuación con la que se decide quién lo coloca después |
| Puntos que otorga el rey | Que el jugador tenga un caballero en el mismo castillo y en el mismo nivel que el rey, y la ronda en curso | La condición es doble y el valor cambia por ronda, con cinco, diez y quince puntos (apartado 4.2) |
| Puntuación final de un jugador | Cartas de acción obtenidas y no utilizadas | Cada carta no utilizada equivale a un punto al final de la partida (apartado 4.3) |
| Duración del plazo de reconexión | De quién era el turno al momento de la desconexión | En turno propio es el tiempo restante de ese turno y en turno ajeno son noventa segundos (apartado 5.2) |
| Fin anticipado de la partida | Cantidad de jugadores que quedan, y esta a su vez de los retiros por vencimiento del plazo | Con menos de dos jugadores la partida termina y gana quien queda |

Cuatro de estas dependencias forman la cadena más larga del juego, en la que un cambio en el último eslabón se propaga hasta el primero:

- Quién coloca al rey depende de la puntuación parcial.
- La puntuación parcial depende del cierre de la ronda.
- El cierre de la ronda depende de la puntuación de cada castillo.
- La puntuación de cada castillo depende de qué torres lo componen en ese momento.
A esto se suma que el empate en la menor puntuación se resuelve al azar, por lo que esa designación depende además de una fuente de aleatoriedad única que todos los jugadores acepten.

**Dependencias internas del sistema**

**Tabla 10**

*Dependencias entre elementos del sistema en construcción*

| Elemento dependiente | Elemento del que depende | Razón de la dependencia |
|---|---|---|
| Lugar desde el que puede leerse la configuración de una partida | Dirección de las dependencias entre capas | La capa de dominio no puede depender de la de persistencia, por lo que la configuración debe llegarle como argumento desde la capa de servicios (sección 2.2) |
| Que el resultado de una acción sea único y aceptado por todos | Que exista un solo punto donde se determina | El estándar establece que el cliente no decide el resultado de ninguna acción del juego (sección 2.1) |
| Ranking histórico | Que toda partida produzca un resultado registrable | Sin desenlace no hay nada que archivar; la regla de fin anticipado asegura que incluso una partida interrumpida produzca un ganador |
| Atribución de un resultado en el ranking | Que el jugador tenga una identidad que persista | El ranking asocia resultados a alguien, y un invitado no cuenta con esa identidad |
| Poder diagnosticar un fallo sin reproducirlo | Que el mensaje de registro incluya la operación y el identificador del elemento afectado | El estándar lo argumenta de forma explícita en la sección 9.3 |
| Poder ejercitar las reglas sin levantar el servidor | Que la capa de dominio no dependa de ninguna otra | Es el propósito declarado de la regla de dependencias entre capas (sección 2.2) |

**Tensiones**

Una tensión existe cuando atender un concern dificulta, encarece o impide atender otro. En esta etapa se identifican, pero no se resuelven, porque su resolución corresponde a decisiones posteriores del equipo.

**Tabla 11**

*Tensiones identificadas*

| Tensión | Elementos en conflicto | Fundamento | Qué impide resolverla |
|---|---|---|---|
| Registro de eventos frente a información personal | La sección 9.3 del estándar exige el identificador del elemento afectado; la sección 9.4 prohíbe registrar datos personales del jugador | Ambas reglas provienen del mismo documento y se tocan de forma directa | No está definido cuánta identificación corresponde al diagnóstico y cuánta constituye un dato personal |
| Ranking histórico frente a información personal | El ranking y la función de amigos frente al concern de información personal | El ranking exige conservar información asociada a personas a lo largo del tiempo, mientras el concern empuja a conservar el mínimo | No está definido qué datos de una cuenta se conservan |
| Continuidad del jugador frente al ritmo de la partida | El turno de noventa segundos que vence por sí solo frente al plazo del jugador activo, que es lo que reste de ese turno | Favorecer al jugador desconectado alarga la espera de los demás; favorecer el ritmo puede dejar un plazo de pocos segundos | Las reglas no indican si el reloj se detiene durante la desconexión |
| Flexibilidad frente a la falta de evidencia sobre qué cambiará | El concern de ajuste de reglas frente a los elementos que las reglas declaran fijos | El tablero y la cantidad de rondas están declarados fijos, y las cartas maestras son un añadido sujeto a la disponibilidad de tiempo | No se sabe qué cambiará, por lo que no puede estimarse el costo de anticipar flexibilidad |

**Preguntas**

Las siguientes preguntas surgen de información faltante, de ambigüedades o de decisiones que aún no se han tomado. Todas debe resolverlas el equipo de desarrollo, que es el único stakeholder con autoridad sobre las reglas y sobre el sistema.

**Tabla 12**

*Preguntas sobre las reglas del juego*

| N.º | Pregunta | Elemento que no puede determinarse sin la respuesta |
|---|---|---|
| 1 | ¿Un turno pertenece a un solo jugador o corresponde a una vuelta completa de todos? El apartado 5.1 sugiere lo primero, mientras que los apartados 1.3 y 3.1 admiten la segunda lectura | La duración total de la partida y la configuración de turnos |
| 2 | ¿Cómo se determina el ganador en una partida que termina de forma normal y cómo se resuelve un empate final? | La condición de término de la partida |
| 3 | ¿En qué momento se calcula la puntuación de los castillos? Se aclaró que el rey aplica su efecto al final de la ronda, pero no si los castillos se puntúan en ese mismo momento | La puntuación parcial con la que se designa a quien coloca al rey |
| 4 | ¿Existe una altura máxima para las torres? La puntuación multiplica por el nivel de la torre y ninguna regla lo limita | El límite de la puntuación que puede otorgar un castillo |
| 5 | ¿Los caballeros y las construcciones permanecen en el tablero entre rondas? | El estado con el que inicia cada ronda |
| 6 | ¿Cómo entran los caballeros al tablero por primera vez y en qué orden juegan los jugadores? El apartado 2.3 presupone un orden de colocación inicial que no aparece definido | El inicio de la partida y la designación de quien coloca al rey en la primera ronda |
| 7 | Al obtener una carta del mazo con un punto de acción, ¿el jugador la elige o se le entrega al azar? | Si el sistema requiere una segunda fuente de aleatoriedad |
| 8 | Si un nivel puede moverse de un castillo a otro, ¿puede esa operación unir dos castillos? El apartado 2.2 prohíbe unir de forma ortogonal torres de castillos distintos | El alcance de la comprobación al usar las cartas de acción 5 y 8 |
| 9 | La carta de acción 3 menciona la condición de realizar el movimiento dentro del mismo castillo como si ya existiera, pero el apartado 2.1 no la enuncia. ¿Falta esa regla o sobra la mención? | La validación de los movimientos entre niveles |

**Tabla 13**

*Preguntas sobre el proyecto*

| N.º | Pregunta | Elemento que no puede determinarse sin la respuesta |
|---|---|---|
| 10 | ¿Qué ocurre con las piezas, las cartas y los puntos de un jugador retirado cuando quedan dos o más jugadores en la partida? | El cálculo de la puntuación de los demás jugadores |
| 11 | Si un jugador es retirado, ¿se recalcula la configuración de turnos y construcciones, que depende del número de jugadores? | La cantidad de turnos restantes después de un retiro |
| 12 | ¿El resultado de una partida jugada por invitados se incorpora al ranking histórico? | El alcance del ranking histórico |
| 13 | ¿Qué datos de una cuenta se conservan y cuáles quedan fuera? | La tensión entre el ranking histórico y la información personal |
| 14 | ¿Qué alcance tiene la función de amigos dentro del sistema? | El alcance de la información que se conserva |
| 15 | ¿Cuántas partidas simultáneas debe soportar el sistema? | Si el aislamiento entre partidas constituye además un requisito de escalabilidad |
| 16 | ¿Qué es el manejador de partidas y a qué capa pertenece? No aparece en las reglas ni en el Estándar de codificación | La capa responsable de recuperar y aplicar la configuración de la partida |
| 17 | La configuración de la distribución inicial de los castillos convive con la regla que declara fija la distribución del tablero y con la aclaración de que el castillo es una construcción inicial fijada. ¿Qué es entonces lo configurable? | El alcance del concern de ajuste de reglas y parámetros |
| 18 | ¿Qué catálogo de atributos de calidad adopta el proyecto? | La verificación de las clasificaciones presentadas en la sección de concerns |

**Referencias**

Bautista Hernández, J., y Benavides Martínez, V. (2026). *Estándar de codificación* (versión 7) [Documento interno no publicado]. Proyecto Juego de Torres.

*Reglas del Juego de Torres* (versión 3.0). (2026). [Documento interno no publicado, en estado de borrador]. Proyecto Juego de Torres.

*Nota.* La información proporcionada por el equipo de desarrollo el 2 de septiembre de 2026 constituye una comunicación personal. Conforme al Manual de la American Psychological Association, este tipo de fuente se cita únicamente en el texto y no se incluye en la lista de referencias.
