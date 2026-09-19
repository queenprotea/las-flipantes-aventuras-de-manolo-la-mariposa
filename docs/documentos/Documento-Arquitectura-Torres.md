# Documento de Arquitectura — Juego de Torres

**Proyecto:** Juego de Torres (multijugador en línea)

**Disciplina:** Diseño de Software

**Fecha:** 5 de septiembre de 2026

---

## 1. Stakeholders

### STK-01 · Jugador

Es la persona que participa en una partida y la única cuyas decisiones cambian el estado del juego. Cada partida tiene entre dos y cuatro jugadores.

Todas las interacciones del sistema existen para él. Le importa poder retomar la partida si pierde la conexión, que el resultado refleje exactamente lo que jugó, que sus partidas terminadas queden guardadas para el ranking, que su información personal se mantenga en el mínimo necesario, poder jugar con cuenta o como invitado sin perder la atribución de sus resultados, y que nadie más pueda usar su cuenta. Esto orienta el diseño hacia la disponibilidad, la corrección funcional y la seguridad.

Un mismo jugador puede encontrarse en distintas situaciones dentro del sistema:

- Por su identidad, con cuenta o como invitado.
- Por su conexión, conectado, desconectado dentro de la ventana de reconexión, o retirado de la partida, que es un estado del que ya no se vuelve.
- Por su posición en la partida, jugador activo cuando es su turno, o en espera.
- Por lo que las reglas le asignan de manera temporal, colocador del rey en la primera ronda o colocador del rey en la segunda y la tercera.
- Fuera de la partida, dentro de una sala, anfitrión de una sala, con una invitación pendiente, o con una amistad aceptada o una solicitud sin responder.

Jugar con cuenta y jugar como invitado se distinguen así:

| | Con cuenta | Invitado |
|---|---|---|
| El sistema guarda información suya | Sí | No guarda nada |
| Aparece en el ranking global | Sí | No |
| Aparece en el ranking de la sala | Sí | Sí |
| Puede crear una sala y ser anfitrión | Sí | Sí |
| Puede recibir una invitación a una sala | Sí | No: las invitaciones solo se envían a cuentas |
| Puede volver a la partida si el servidor se cae | Sí | No |

### STK-02 · Equipo de desarrollo

Son los dos integrantes nombrados en el estándar de codificación. Dentro del proyecto cumplen varias funciones: escriben y revisan las reglas del juego, diseñan, desarrollan, prueban, operan el servidor y responden por la entrega. Es también el único con autoridad para tomar las decisiones del proyecto.

Le importa que ajustar una regla o un parámetro no obligue a rehacer el sistema, que lo que ocurre en una partida no altere otra, poder comprobar que una regla hace lo que dice sin levantar todo el sistema, y poder reconstruir qué ocurrió cuando algo falla. Comparte con el jugador el interés en la corrección del resultado, en la conservación de las partidas, en el manejo de la información personal y en la seguridad de las cuentas. Esto orienta el diseño hacia la modificabilidad, la testabilidad y la analizabilidad.

Que el equipo reúna la autoría de las reglas y el desarrollo hace que un cambio de regla sea una decisión interna y no una negociación externa, lo que abarata ese tipo de cambio.

### STK-03 · Receptor de la entrega

Es quien recibe y evalúa el resultado del proyecto. Su autoridad es sobre la aceptación del trabajo, no sobre el diseño.

Impone cuatro condiciones que no se negocian: que el sistema se escriba en C#, que la comunicación entre cliente y servidor use WCF con llamadas duplex sobre net.tcp, que quede prohibido el uso de tecnologías web y WebSockets, y que lo que se evalúe sea el código y no el acabado visual. Es también quien convierte las reglas del estándar de codificación —la separación en capas, el nombrado, el registro de eventos y el régimen de pruebas— en obligaciones y no en recomendaciones.

---

## 2. Concerns

Cada preocupación nombra un asunto sobre el que el sistema debe poder responder. La columna de atributo de calidad usa los nombres de la norma ISO/IEC 25010, que es el catálogo de referencia del proyecto.

| Identificador | Preocupación | Atributo de calidad | Quién la introduce |
|---|---|---|---|
| CON-01 | Continuidad de la partida ante una desconexión o una caída del servidor | Disponibilidad | Jugador |
| CON-02 | Ajuste de reglas y parámetros sin rehacer el sistema | Modificabilidad | Equipo |
| CON-03 | Correspondencia entre lo jugado y lo puntuado | Corrección funcional | Jugador |
| CON-04 | Conservación del resultado de las partidas | Ninguno | Jugador con cuenta |
| CON-05 | Aislamiento entre partidas simultáneas | Corrección funcional bajo concurrencia | Equipo |
| CON-06 | Qué información personal se guarda y se registra | Seguridad, en confidencialidad | Jugador |
| CON-07 | Jugar con cuenta o como invitado sin perder atribución | Ninguno | Jugador |
| CON-08 | Poder comprobar que una regla hace lo que dice | Testabilidad | Equipo |
| CON-09 | Poder reconstruir qué ocurrió cuando algo falla | Analizabilidad | Equipo |
| CON-10 | Que solo el titular pueda actuar como su cuenta | Seguridad, en autenticidad | Jugador |

### CON-01 · Continuidad de la partida

Un jugador que pierde la conexión debe poder retomar la partida donde estaba, la partida debe seguir siendo jugable para los demás, y una caída del servidor no debe destruirla.

**Cuando cae el jugador.** La ventana para volver es el tiempo que le quedara de su turno si era su turno, o noventa segundos si era el turno de otro. El reloj del turno sigue corriendo mientras está desconectado. Si vuelve dentro de la ventana, retoma su turno en el estado exacto en que lo dejó, con los puntos de acción no gastados y las acciones ya confirmadas. Si la ventana se agota, el jugador es retirado de la partida; si el retiro deja menos de dos jugadores, la partida termina y gana quien queda. La partida continúa con la configuración correspondiente al número inicial de jugadores.

**Cuando cae el servidor.** El estado de la partida se guarda al cerrar cada turno. Al arrancar de nuevo, el servidor busca las partidas en curso y las reanuda cuando han vuelto al menos dos jugadores con cuenta. El turno que quedó interrumpido se juega otra vez con noventa segundos completos, y los noventa segundos de la ventana de reconexión no corren mientras el servidor está caído. La espera es de tres minutos desde el arranque. Si vuelve un solo jugador, la partida termina como abandonada y él gana; si no vuelve nadie, termina como interrumpida, sin puestos ni puntuaciones, y no cuenta para ningún ranking. Las salas no se reanudan.

### CON-02 · Ajuste de reglas y parámetros

Los valores que las propias reglas declaran variables, y los cambios de versión del documento de reglas, deben poder aplicarse con un coste acotado. El asunto es el coste del cambio, no que el sistema funcione.

Lo que puede configurarse y lo que no está delimitado:

| Se puede configurar | No se puede configurar |
|---|---|
| Los turnos que tiene cada jugador en cada ronda | El tablero de ocho por ocho y su distribución |
| Las construcciones que recibe cada jugador en cada turno | Las tres rondas |
| El coste en puntos de acción de cada acción | El catálogo de ocho cartas de acción |
| El plazo de reanudación tras una caída | Los cinco caballeros por jugador |
| El factor de coste del cifrado de contraseñas | Los noventa segundos de turno |
| | La altura máxima de torre, que es de tres niveles |

Los tres conjuntos configurables viven en tres tablas de solo lectura dentro de la base de datos. Las lee la capa de servicios y se las entrega al dominio como argumentos, porque el dominio no puede leer de la persistencia. El componente que prepara las partidas antes de que empiecen se llama `MatchSetupService` y pertenece a la capa de servicios.

La medida de esta preocupación es el coste del cambio: modificar uno de esos valores no debe obligar a tocar el dominio ni a recompilar.

### CON-03 · Correspondencia entre lo jugado y lo puntuado

El resultado de la partida debe reflejar exactamente las acciones legales realizadas por cada jugador, y solo esas. El cliente no determina el resultado de ninguna acción, de modo que existe un único punto donde se decide.

### CON-04 · Conservación del resultado de las partidas

El resultado de una partida terminada debe seguir disponible después, para el ranking y el historial. De cada partida terminada se conservan el puesto y los puntos de cada jugador con cuenta, y cuántos jugaron.

### CON-05 · Aislamiento entre partidas simultáneas

Lo que ocurre en una partida no debe alterar el estado ni el resultado de otra. El sistema permite tantas partidas simultáneas como se soliciten, sin imponer un límite.

La medida es una condición que debe cumplirse siempre: con cinco partidas en curso, ninguna operación de una altera el estado, el reloj ni el resultado de otra. Esas cinco partidas son el número con el que se verifica el aislamiento durante las pruebas.

El aislamiento se demuestra también sobre lo que vive en memoria del servidor, porque los miembros de cada sala, el anfitrión y el ranking de sala están ahí.

### CON-06 · Qué información personal se guarda y se registra

La información personal que el sistema conserva y escribe en sus registros debe mantenerse en el mínimo necesario. El alcance de esta preocupación cubre lo que se guarda en la base de datos y lo que se escribe en el registro de eventos.

| Dato | Cómo se maneja |
|---|---|
| Correo electrónico | Es el único dato personal obligatorio. Sirve para recuperar el acceso y para enviar invitaciones |
| Contraseña | No se guarda nunca. Solo se guarda su resultado cifrado |
| Avatar | El archivo lo guarda el servidor, con un máximo de cinco megabytes y en formato PNG o JPG. La base de datos guarda solo la referencia |
| Identificador interno del jugador | Es lo único que puede aparecer en el registro de eventos |
| Datos del invitado | No se guarda nada |
| Correo de terceros | Una invitación solo puede dirigirse a una cuenta que ya existe |

### CON-07 · Jugar con cuenta o como invitado

La forma de identificarse no debe impedir jugar y, a la vez, debe determinar con claridad qué resultados pueden atribuirse a cada quien después de la partida. Una partida jugada con invitados cuenta para los jugadores con cuenta que participaron; el invitado no aparece en el ranking global porque de él no se guarda información.

Como los invitados no vuelven tras una caída del servidor, la exigencia de que regresen dos jugadores para reanudar significa dos jugadores con cuenta.

### CON-08 · Poder comprobar que una regla hace lo que dice

El comportamiento de las reglas del juego debe poder verificarse sin depender del servidor ni de la interfaz. El estándar declara ese propósito de forma literal para justificar la dirección de las dependencias entre capas: existe para que las reglas puedan probarse sin levantar el servidor ni abrir la ventana del juego.

El estándar completa la exigencia con el régimen de pruebas: nombrado, estructura en tres bloques, una sola ejecución por prueba y prohibición de poner lógica dentro de la prueba. Como el tablero se guarda serializado y la base de datos no lo valida, toda la validación geométrica vive en el dominio, que es la capa que puede probarse aislada.

### CON-09 · Poder reconstruir qué ocurrió cuando algo falla

Ante un fallo debe poder saberse qué operación falló, sobre qué entidad y en qué punto. El equipo opera el servidor, así que es quien lee esos registros cuando una partida falla y ya no puede reproducirse.

El estándar exige que el mensaje lleve la operación y el identificador de la entidad afectada, porque un mensaje sin ese contexto obliga a reproducir el fallo para saber a qué se refería. Se refleja en un registrador por clase, la excepción pasada como argumento para conservar su tipo y su traza, y un solo registro por fallo.

### CON-10 · Que solo el titular pueda actuar como su cuenta

Solo el titular de una cuenta debe poder actuar como ella, y la vía de recuperación de acceso no debe convertirse en una vía de suplantación. La cuenta es la identidad a la que el ranking global atribuye resultados y con la que se aceptan amistades e invitaciones.

Cubre la credencial guardada, la recuperación de acceso por correo y el uso único del enlace de invitación. Se apoya en cuatro decisiones: la contraseña se guarda cifrada y nunca en claro, con un factor de coste configurable; el correo es la vía de recuperación; el enlace de invitación sirve una sola vez; y no puede borrarse una cuenta que está en una partida en curso.

---

## 3. Dependencias

Existe una dependencia cuando un elemento no puede determinarse, calcularse o existir sin el otro: si cambia el primero, cambia el segundo.

### 3.1 Dependencias externas

| Identificador | Qué depende | De qué depende |
|---|---|---|
| DEP-E1 | El registro de eventos de todo el sistema | La biblioteca log4net, que el estándar impone como único medio: no se escribe en la consola ni en un archivo por otras vías |
| DEP-E2 | El manejo de errores del código | Los tipos de excepción del framework, que el estándar obliga a usar siempre que apliquen para que quien llama pueda capturarlos sin conocer tipos propios |
| DEP-E3 | La continuidad de la participación de un jugador | La conexión de red entre su cliente y el servidor |
| DEP-E4 | Las reglas implementadas en el sistema | El documento de reglas. Cada versión suya redefine qué comportamiento es el correcto |
| DEP-E5 | La forma del código: capas, nombrado, registro y pruebas | El estándar de codificación, obligatorio en construcción, revisión y mantenimiento |
| DEP-E6 | El vencimiento del turno y el cómputo de las ventanas de reconexión | Una fuente de tiempo con autoridad, porque una consecuencia irreversible depende de un plazo medido |
| DEP-E7 | Todo lo que se guarda de forma permanente o temporal | PostgreSQL. El esquema de la base está escrito y probado sobre él |
| DEP-E8 | El resguardo de la contraseña | La biblioteca de cifrado BCrypt. Cambiarla obligaría a rehacer todas las contraseñas guardadas |
| DEP-E9 | La invitación por correo y la recuperación de acceso | Un canal de correo saliente |
| DEP-E10 | El avatar del jugador | El sistema de archivos del servidor. La base de datos no puede tocar el disco: solo guarda la referencia |
| DEP-E11 | Toda la comunicación entre cliente y servidor | CoreWCF con canal duplex sobre net.tcp. Cliente y servidor comparten un único ensamblado con las interfaces |
| DEP-E12 | La presentación del cliente | MonoGame en su variante DesktopGL, que funciona en los tres sistemas operativos del equipo |
| DEP-E13 | La compilación y ejecución de todo el sistema | .NET 10 |

### 3.2 Dependencias del dominio del juego

| Identificador | Qué depende | De qué depende |
|---|---|---|
| DEP-I1 | Los turnos que tiene cada jugador en cada ronda, y las construcciones que recibe | La cantidad de jugadores, la ronda y el turno |
| DEP-I2 | La puntuación que un castillo otorga a un jugador | La superficie del castillo, el nivel de la torre donde está su caballero, y que sea el único caballero propio en ese castillo |
| DEP-I3 | La superficie de un castillo | Qué torres lo forman en ese momento. Como el cierre de ronda no retira nada del tablero, esa superficie crece a lo largo de la partida |
| DEP-I4 | La legalidad de usar las cartas cinco y ocho | El estado geométrico del castillo afectado: la operación no puede partirlo ni unir dos castillos |
| DEP-I5 | Los puntos de acción disponibles en un turno | Si el jugador usó la carta uno o la dos en ese turno. Pueden usarse en cualquier momento del turno, pero no las dos a la vez |
| DEP-I6 | Quién coloca el rey en la segunda y la tercera ronda | La puntuación parcial de todos los jugadores, y una fuente de aleatoriedad única cuando hay empate en la menor puntuación |
| DEP-I7 | Que exista esa puntuación parcial | El cierre de la ronda |
| DEP-I8 | Los puntos que el rey otorga a un jugador | Que tenga un caballero en el mismo castillo y en el mismo nivel que el rey, y la ronda en curso, porque el valor cambia entre cinco, diez y quince |
| DEP-I9 | La puntuación final de un jugador | Las cartas de acción que obtuvo y no utilizó |
| DEP-I10 | La duración de la ventana de reconexión | De quién era el turno en el instante de la caída |
| DEP-I11 | Que la partida termine antes de tiempo | Cuántos jugadores quedan, que a su vez depende de los retiros por reconexión vencida |
| DEP-I12 | El movimiento de un caballero | Que el origen y el destino estén en el mismo castillo |

Cuatro de estas dependencias forman la cadena más larga del dominio: quién coloca el rey depende de la puntuación parcial, que depende del cierre de ronda, que depende de la puntuación de cada castillo, que depende de qué torres lo componen en ese momento. Un cambio en cualquier eslabón llega hasta la designación del colocador del rey.

### 3.3 Dependencias internas del sistema

| Identificador | Qué depende | De qué depende |
|---|---|---|
| DEP-S1 | Dónde puede leerse la configuración de una partida | La dirección de las dependencias entre capas: si la configuración se guarda fuera del código, tiene que llegar al dominio como argumento desde la capa de servicios |
| DEP-S2 | Que el resultado de una acción sea único y aceptado por todos | Que exista un solo punto donde se determina, porque el cliente no decide resultados |
| DEP-S3 | El ranking histórico | Que la partida termine con puestos y puntuaciones asignados |
| DEP-S4 | Que un resultado pueda atribuirse en el ranking global | Que el jugador tenga una identidad que persista |
| DEP-S5 | Poder diagnosticar un fallo sin reproducirlo | Que el mensaje del registro lleve la operación y el identificador de la entidad afectada |
| DEP-S6 | Poder ejercitar las reglas sin levantar el servidor ni abrir la ventana | Que el dominio no dependa de ninguna otra capa |
| DEP-S8 | Que la sala y su ranking existan | Que el servidor siga en pie, porque ambos viven en su memoria |

### 3.4 Dependencias entre decisiones

| Identificador | Qué decisión | Depende de |
|---|---|---|
| DEP-D1 | Que la partida termine con menos de dos jugadores | Las ventanas de reconexión y el retiro, que son el mecanismo capaz de llevarla por debajo de dos |
| DEP-D2 | La persistencia de partidas, el ranking y el historial | La existencia de cuentas y de modo invitado, porque el historial atribuye resultados a una identidad |
| DEP-D3 | Que puedan existir partidas simultáneas | Que el cliente no decida resultados, porque así el estado no queda repartido entre clientes |
| DEP-D4 | Cualquier decisión sobre modificabilidad | Que el dominio no dependa de ninguna otra capa, lo que acota dónde puede vivir la configuración |
| DEP-D5 | Guardar los parámetros en la base de datos | Que ya exista una base de datos |
| DEP-D6 | El borrado de una cuenta en cascada | La prohibición de borrarla mientras esté en una partida en curso |

### 3.5 Dependencias del proyecto

| Identificador | Qué depende | De qué depende |
|---|---|---|
| DEP-P1 | El cierre de las cuestiones del proyecto | El equipo de desarrollo, que es el único con autoridad para decidirlas |
| DEP-P2 | La cifra con la que se verifica el aislamiento entre partidas | Una decisión del equipo, fijada en cinco partidas |
| DEP-P3 | La aceptación del trabajo | El receptor de la entrega y sus criterios |
| DEP-P4 | Que las medidas de los escenarios tengan un umbral | El receptor de la entrega o, en su defecto, una decisión del equipo |

---

## 4. Escenarios de calidad

Cada escenario describe una situación concreta y comprobable en seis partes: quién o qué la provoca, qué ocurre, sobre qué parte del sistema cae, en qué condición está el sistema, qué debe hacer y cómo se comprueba que lo hizo.

### D-1 · Disponibilidad: el jugador pierde la conexión en su propio turno

*Preocupación de origen: continuidad de la partida. Interesado: el jugador.*

- **Fuente.** El jugador que tiene el turno, a través de su conexión de red.
- **Estímulo.** Pierde la conexión con el servidor mientras es su turno y le quedan algunos segundos de los noventa.
- **Artefacto.** El estado de la partida en curso, que custodia la capa de servicios, y el reloj del turno.
- **Ambiente.** Partida en curso, servidor funcionando con normalidad, turno propio del jugador, mesa de dos a cuatro jugadores.
- **Respuesta.** El servidor conserva el puesto del jugador y el estado de su turno. El reloj del turno sigue corriendo. Los demás jugadores siguen viendo la partida y el reloj avanzar, sin interrupción. Si el jugador vuelve dentro del tiempo que le quedaba, retoma su turno en el estado exacto en que lo dejó, con los puntos de acción no gastados y las acciones ya confirmadas. Si no vuelve, al agotarse el tiempo es retirado de la partida: sus construcciones permanecen en el tablero, sus caballeros se retiran y su puntuación deja de contar. Si el retiro deja menos de dos jugadores, la partida termina y gana quien queda. El desenlace queda registrado.
- **Medida.** La ventana para volver es exactamente el tiempo que quedaba del turno, contado desde el instante en que el servidor detecta la pérdida de conexión. El turno no se prolonga: la espera de los otros jugadores no aumenta por causa de la desconexión, y ningún turno pasa de noventa segundos. Al agotarse la ventana, el retiro es efectivo antes de que empiece el turno siguiente. Tanto la reconexión como el retiro dejan una entrada en el registro de eventos.

### M-1 · Modificabilidad: cambia el coste en puntos de acción de una acción

*Preocupación de origen: ajuste de reglas y parámetros. Interesado: el equipo de desarrollo, en su función de autor de las reglas.*

- **Fuente.** El equipo de desarrollo cuando escribe y revisa las reglas.
- **Estímulo.** Publica una versión del documento de reglas en la que colocar un caballero pasa de costar dos puntos de acción a costar tres.
- **Artefacto.** La tabla de costes de acción en la base de datos, y el componente que prepara las partidas antes de que empiecen, que la lee y entrega los valores al dominio como argumentos.
- **Ambiente.** Sistema desplegado y en operación, con partidas en curso, fuera de cualquier ventana de despliegue.
- **Respuesta.** El cambio se aplica editando una fila de la tabla. No se modifica ningún archivo del dominio, no se recompila y no se vuelve a desplegar la aplicación. Las partidas que ya están en curso conservan el coste con el que empezaron; la siguiente partida que se inicie toma el valor nuevo, porque la configuración se lee al preparar la partida.
- **Medida.** Se toca exactamente un artefacto, una fila de una tabla, y ningún archivo de código. El cambio no obliga a recompilar el dominio ni a volver a desplegar ningún componente. Queda efectivo en la primera partida que se inicie después, y ninguna partida en curso cambia de comportamiento a mitad. Lo puede realizar el equipo desde su función de operación, sin intervenir el código.

### F-1 · Corrección funcional: el cierre de puntuación de una ronda

*Preocupación de origen: correspondencia entre lo jugado y lo puntuado. Interesados: el jugador y el equipo.*

- **Fuente.** El jugador que ocupa el último turno de la ronda.
- **Estímulo.** Confirma su última acción, con lo que la ronda termina y se dispara el cierre de puntuación.
- **Artefacto.** El dominio: el cálculo de puntuación de los castillos, el efecto del rey y la designación de quién colocará el rey en la ronda siguiente.
- **Ambiente.** Partida en curso, servidor funcionando con normalidad, fin de la primera o de la segunda ronda, mesa de dos a cuatro jugadores con al menos un jugador retirado.
- **Respuesta.** El servidor, que es el único que determina el resultado, calcula la puntuación parcial de cada jugador que sigue en la partida: por cada castillo en el que tenga exactamente un caballero propio, la superficie del castillo multiplicada por el nivel de la torre donde está ese caballero; y le suma los puntos del rey si tiene un caballero en el mismo castillo y en el mismo nivel, según la ronda. Nada se retira del tablero. Los caballeros del jugador retirado ya no están y no cuentan para la condición de exclusividad, pero sus construcciones siguen formando parte de los castillos. Designa como colocador del rey de la ronda siguiente al jugador con menor puntuación parcial de entre los que siguen en la partida, y si hay empate se sortea. El cierre queda registrado.
- **Medida.** La puntuación de cada jugador es exactamente la que se deriva de las reglas de puntuación sobre el estado del tablero en el instante del cierre, comparable contra un cálculo independiente sobre ese mismo estado. Ningún jugador retirado aparece en la designación, ni con puntuación ni como candidato. El cierre produce un solo resultado, el del servidor, idéntico para todos los clientes. Queda registrado, de modo que puede comprobarse qué puntuación se asignó y a quién mientras la partida sigue en curso.

### T-1 · Testabilidad: probar una regla sin levantar nada

*Preocupación de origen: poder comprobar que una regla hace lo que dice. Interesado: el equipo, en su función de pruebas.*

- **Fuente.** El equipo de desarrollo cuando prueba.
- **Estímulo.** Ejecuta la batería de pruebas de una regla del dominio, por ejemplo que un castillo solo puntúa si el jugador tiene exactamente un caballero propio en él.
- **Artefacto.** El dominio y el proyecto de pruebas.
- **Ambiente.** Entorno local de desarrollo de cualquiera de los dos integrantes, en macOS o en Linux, sin servidor levantado, sin la ventana del juego abierta y sin la base de datos corriendo.
- **Respuesta.** La prueba construye el estado del tablero en memoria, invoca la regla y comprueba el resultado con una aserción directa. Los parámetros configurables llegan como argumentos, no se leen de la base de datos. No se arranca la capa de servicios, no se abre el cliente y no se toca la persistencia.
- **Medida.** La ejecución no levanta ningún proceso externo: ni servidor, ni ventana, ni base de datos. El proyecto de pruebas del dominio no hace referencia a ningún tipo de las otras tres capas, lo que se comprueba inspeccionando las referencias del proyecto. Cada prueba realiza una sola ejecución del método que se prueba y comprobaciones directas, sin lógica dentro de la prueba. La batería corre con un solo comando en la máquina de cualquiera de los dos integrantes.

### L-1 · Analizabilidad: diagnosticar sin reproducir el fallo

*Preocupación de origen: poder reconstruir qué ocurrió cuando algo falla. Interesado: el equipo, en su función de operación del servidor.*

- **Fuente.** La capa de servicios, al aplicar la acción de un jugador.
- **Estímulo.** Una operación falla con una excepción mientras se aplica esa acción en una partida concreta.
- **Artefacto.** El registro de eventos y las clases de las capas de servicios y de dominio que escriben en él.
- **Ambiente.** Servidor funcionando con normalidad, con varias partidas en curso, y la partida ya terminada o imposible de reproducir cuando alguien investiga.
- **Respuesta.** Se escribe un solo registro por fallo, desde el registrador de la clase donde ocurre, con la operación que se estaba ejecutando, el identificador interno de la entidad afectada y el de la partida, y con la excepción pasada como argumento para conservar su tipo y su traza. No aparece el nombre de usuario, ni el correo, ni ningún otro dato personal.
- **Medida.** A partir del registro, y sin volver a ejecutar la partida, puede determinarse qué operación falló, sobre qué partida y sobre qué jugador, identificado por su clave interna. Hay exactamente una entrada por fallo, sin duplicados y sin fallos silenciosos. No hay ningún dato personal en el registro, lo que se comprueba buscando un correo o un nombre de usuario en él. La entrada permite identificar la clase de origen sin recorrer el código.

---

## 5. Árbol de utilidad

El árbol de utilidad organiza los escenarios en cuatro niveles. La raíz, llamada utilidad, permite mirar todo el sistema desde un solo lugar. Bajo ella van las categorías, que son los atributos de calidad; bajo cada categoría, los refinamientos, que acotan la categoría a un asunto concreto; y bajo cada refinamiento, las hojas, que son los escenarios.

Cada hoja lleva un par de letras. La primera es la importancia, es decir, qué se pierde y quién lo pierde si el escenario no se cumple. La segunda es la dificultad, es decir, cuánto cuesta lograrlo. Ambas se puntúan como alta, media o baja.

### 5.1 Estructura del árbol

| Categoría | Refinamiento | Escenario | Prioridad |
|---|---|---|---|
| Disponibilidad | Continuidad del jugador ante la pérdida de su conexión | D-1 | (A, M) |
| Modificabilidad | Coste de aplicar un cambio en un valor declarado variable | M-1 | (A, B) |
| Corrección funcional | Correspondencia entre el estado del tablero y la puntuación | F-1 | (A, A) |
| Testabilidad | Coste de ejercitar una regla del dominio | T-1 | (A, A) |
| Analizabilidad | Diagnóstico de un fallo que ya no puede reproducirse | L-1 | (M, B) |

### 5.2 Justificación de cada prioridad

**D-1 · Disponibilidad — (Alta, Media)**

*Importancia alta.* La reconexión es un requisito comprometido del proyecto y las reglas le dedican un apartado completo, con plazos y consecuencia. Si no se cumple, el jugador pierde su turno o queda retirado de la partida, y en una mesa de dos su retiro termina la partida también para el otro.

*Dificultad media.* El mecanismo de comunicación ya está definido y las reglas fijan los plazos, pero el componente que gestiona la reconexión necesita conocer de quién es el turno y cuánto le queda, así que no puede construirse de forma independiente del que gestiona el turno.

**M-1 · Modificabilidad — (Alta, Baja)**

*Importancia alta.* El documento de reglas ya cambió varias veces antes de que existiera el sistema. Si cada ajuste de un parámetro costara un ciclo de desarrollo, con dos personas y un solo semestre eso se pagaría en funcionalidad que no se entrega.

*Dificultad baja.* Los parámetros viven en tablas de solo lectura que la capa de servicios entrega al dominio como argumentos, de modo que el cambio consiste en editar un dato y no toca el código.

**F-1 · Corrección funcional — (Alta, Alta)**

*Importancia alta.* Si la puntuación no corresponde a lo jugado, el juego deja de ser el juego. Además el error no se queda quieto: la puntuación parcial decide quién coloca el rey en la ronda siguiente, así que se propaga al resto de la partida.

*Dificultad alta.* El cálculo atraviesa la cadena más larga del dominio, sobre castillos cuya superficie crece a lo largo de la partida, con una condición de exclusividad por castillo, con las piezas del jugador retirado saliendo del tablero mientras sus construcciones se quedan, y con un empate que se resuelve al azar y debe seguir siendo comprobable con pruebas deterministas.

**T-1 · Testabilidad — (Alta, Alta)**

*Importancia alta.* El estándar justifica con esta cualidad toda la dirección de las dependencias entre capas: si las reglas no pueden probarse aisladas, la estructura de cuatro capas pierde la razón que la sostiene. Además el régimen de pruebas es obligatorio y evaluable, justo en aquello que el receptor de la entrega evalúa, que es el código.

*Dificultad alta.* Hay dos fuentes de azar en el dominio, el sorteo del empate y la obtención de cartas, y el estándar exige comprobaciones directas sin lógica dentro de la prueba. Hacerlas comprobables obliga a cambiar la forma en que el dominio expresa esas reglas, y a hacerlo con un equipo de dos personas que además diseñan, prueban y operan.

**L-1 · Analizabilidad — (Media, Baja)**

*Importancia media.* Lo pierde el equipo en su función de operación, no el jugador, y no cambia ninguna estructura del sistema: es disciplina al escribir el mensaje. Cuando falla, el coste es tener que reproducir el fallo, que es molesto y a veces imposible, pero recuperable.

*Dificultad baja.* El estándar ya indica exactamente qué hacer: un registrador por clase, la operación y el identificador de la entidad en el mensaje, un solo registro por fallo y la excepción pasada como argumento. La biblioteca de registro que impone el estándar lo resuelve de forma directa.
