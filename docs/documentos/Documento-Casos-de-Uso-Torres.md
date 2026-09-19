# Casos de uso — Las flipantes aventuras de Manolo la mariposa (Torres)

**Documento:** `Documento-Casos-de-Uso-Torres.md` · versión 4.0 · 11 sep 2026
**Qué contiene:** los **veintiocho casos de uso** del sistema, `CU-01` a `CU-28`, en un solo
documento y con la misma plantilla, el mismo nivel de detalle y los mismos apartados que la
primera ronda de descripciones. **`CU-01`, `CU-02` y `CU-03` se reproducen aquí íntegros y
corregidos**: ya no viven aparte.

**Versión 4.0 · 10 sep 2026.** Incorpora las nueve aclaraciones del equipo sobre los huecos que
la versión anterior había dejado señalados, y **añade `CU-28 Abandonar la partida`**, que nace de
una de ellas. Lo que cambió está al final, en «Integración de las aclaraciones del equipo».

---

## Criterio adoptado

### La plantilla

La de la primera ronda, con **trece apartados y en este orden**: ID · Nombre · Descripción ·
Autor · Actores · Precondición · Disparador · Flujo Normal · Flujo Alterno · Excepciones ·
Postcondiciones · Extensiones · Inclusiones. No se añade ninguno y no se omite ninguno.

### Cómo se escriben los pasos

Igual que en `CU-02` y `CU-03`, que son las descripciones más completas de la primera ronda:

- **Flujo Normal:** un solo camino, numerado, **alternando quién actúa** —«el sistema» o el
  actor—, **nombrando la ventana** donde ocurre cada paso y **entrecomillando** campos, opciones
  y mensajes tal como aparecen en pantalla. Las bifurcaciones se señalan al final del paso con
  `(FA0x)` y `(EX0x)`.
- **Flujo Alterno:** todo camino que se separa del normal —cancelaciones, validaciones que
  fallan, opciones equivalentes— con el **mismo nivel de detalle** que el flujo normal y con el
  **retorno explícito** al paso en el que se reanuda, o con «Termina el caso de uso».
- **Excepciones:** solo lo que **impide continuar** la operación, con el mensaje exacto que se
  muestra y qué queda sin hacer.
- **Precondiciones** `PRE-n` y **Postcondiciones** `POST-n`, numeradas.

### De dónde salen los textos de pantalla

De tres sitios, ninguno inventado: la **primera ronda de descripciones**, la **captura de
ventanas del sistema** —de donde vienen los nombres `GUIxxx`— y el **prototipo de pantallas**
`Prototipo-Pantallas-Torres.html`, que aporta los rótulos, los avisos y los mensajes literales.
Cuando una fuente no dice algo, **no se escribe**: queda anotado al final, en «Observaciones».

**Las ventanas del tablero no tienen nombre `GUIxxx`.** El prototipo las tiene dibujadas
—`PT-21` a `PT-26`— pero la captura de ventanas del sistema es anterior y no las incluye. Por eso
`CU-24` a `CU-28` dicen **«la ventana de la partida»** y nombran sus elementos tal como el
prototipo los rotula. Cuando el equipo les asigne nombre, basta sustituir esa expresión.

### Criterio sobre los actores

**Un actor es una identidad, no un rol, y no cambia de nombre según dónde esté esa persona
dentro del juego.** Quien tiene cuenta es un **Jugador** tanto si está eliminando su cuenta como
si está colocando una oruga; quien entró con un alias temporal es un **Invitado** en los mismos
términos. Solo hay tres actores:

| Actor | Quién es |
|---|---|
| **Jugador** | Quien tiene una cuenta y ha iniciado sesión. Es el equivalente de la entidad `Player` del modelo de datos |
| **Invitado** | Quien entró con un alias temporal, sin cuenta |
| **Usuario sin cuenta** | Quien todavía no se ha identificado de ninguna de las dos maneras. **Es el único actor cuyo nombre conserva la palabra «Usuario»**, y lo hace porque todavía no es ninguna de las otras dos cosas: llamarlo Jugador afirmaría una cuenta que aún no existe |

**Ser anfitrión, estar en turno o estar dentro de una partida no son actores: son condiciones**,
y por eso viven en las **precondiciones**, no en el apartado de actores. `CU-19` y `CU-21` tienen
como actores a Jugador e Invitado, y exigen en su precondición ser el anfitrión de la sala;
`CU-25` los tiene a los dos y exige en su precondición que sea su turno.

**Cuando los dos actores pueden hacer lo mismo, los pasos dicen «el jugador o el invitado».** Es
más largo que un término único, pero no esconde que son dos actores distintos con los mismos
permisos en ese caso concreto.

### Criterio sobre la temática

**Las descripciones usan los nombres de la temática del juego, no los de la carta de reglas.**
La equivalencia es esta, y el documento de reglas sigue siendo la fuente de cada norma:

| En las descripciones y en la pantalla | En el documento de reglas |
|---|---|
| **oruga** | caballero |
| **mariposa** | rey |
| **flor** | torre |
| **jardinera** | castillo |

Las reglas **no cambian** al cambiar el nombre: una oruga sigue subiendo un solo nivel por
movimiento, y una mariposa sigue situándose siempre sobre una flor y nunca sobre el tablero.

### Criterio de relaciones

- **`<<extend>>`** cuando el caso base **se completa sin el otro**, y el otro solo ocurre si el
  actor elige una opción concreta durante el caso base. **Se declara en los dos casos**: en el
  base, nombrando la extensión; en el que extiende, con la frase «Extiende a CU-xx».
- **`<<include>>`** solo cuando el comportamiento del otro caso de uso **se ejecuta siempre**
  como parte del flujo normal del base.

**Hay exactamente dos inclusiones en todo el conjunto:** `CU-13 Entrar a la sala`, ineludible en
las tres formas de llegar a una sala, y `CU-24 Preparar la partida`, ineludible antes del primer
turno.

**Comportamientos que ocurren siempre y aun así no son inclusiones.** La comprobación del baneo
permanente al iniciar sesión, la de la sanción temporal al entrar a la sala y al iniciar la
partida, el cierre del turno con la escritura del estado, el borrado del archivo del avatar, la
generación del identificador temporal del invitado y la asignación de color **no persiguen el
objetivo de ningún actor**: no son casos de uso, así que se describen como **pasos del flujo
normal** del caso que los ejecuta. Estar dentro de una sala, tener sesión iniciada o disponer de
conexión son **precondiciones**, no inclusiones; y la simple navegación entre ventanas no basta
para declarar una relación.

### Criterio sobre las precondiciones

Una precondición se **da por cumplida** al empezar el caso de uso; no es algo que el flujo vuelva
a comprobar. Por eso **una condición que el sistema verifica dentro del flujo y resuelve con un
flujo alterno no se declara además como precondición**. Quedan como precondiciones las que el
contexto garantiza —tener conexión, tener sesión iniciada, estar dentro de una sala— y las que el
caso llamante ya validó antes de invocar a un caso incluido.

---

## CU-01 Iniciar sesión

| | |
|---|---|
| **ID** | CU-01 |
| **Nombre** | Iniciar sesión |
| **Descripción** | Permite a un usuario que ya tiene una cuenta identificarse en el sistema con su nombre de usuario y su contraseña, para poder jugar con su identidad, conservar su historial, aparecer en el ranking global y disponer de sus amigos y de sus invitaciones. El sistema comprueba la contraseña contra el hash almacenado y rechaza el acceso a las cuentas con baneo permanente. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador no debe tener una sesión iniciada.<br>PRE-3. El jugador debe tener una cuenta registrada en el sistema. |
| **Disparador** | El jugador selecciona la opción "Iniciar sesión" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema muestra la ventana GUILogIn con el título "Iniciar sesión", la indicación "Entra con tu cuenta para jugar y guardar tus resultados.", los campos "Username" y "Contraseña", y las opciones "Entrar", "Crear cuenta", "¿Olvidaste tu acceso?", "Jugar como invitado" y "Volver al menú". (FA04) (FA05) (FA06) (FA07)
2. El jugador ingresa su nombre de usuario en el campo "Username".
3. El jugador ingresa su contraseña en el campo "Contraseña".
4. El jugador selecciona la opción "Entrar".
5. El sistema valida que los campos "Username" y "Contraseña" no estén vacíos. (FA01)
6. El sistema busca la cuenta cuyo nombre de usuario coincida con el valor ingresado, sin distinguir mayúsculas de minúsculas, y comprueba la contraseña contra el hash almacenado de esa cuenta. (FA02) (EX01)
7. El sistema comprueba que la cuenta no tenga un baneo permanente vigente. (FA03)
8. El sistema da por iniciada la sesión del jugador con su cuenta.
9. El sistema cierra GUILogIn y muestra la ventana GUIMainMenu con el nombre de usuario y el avatar de la cuenta en la esquina superior derecha, y con las opciones "Salas", "Invitaciones", "Amigos", "Historial", "Ranking" y "Salir" habilitadas.

**Flujo Alterno**

FA01 - Campos vacíos
1. El jugador selecciona la opción "Entrar" habiendo dejado vacío el campo "Username", el campo "Contraseña" o los dos.
2. El sistema muestra junto al campo vacío un mensaje indicando que debe completarlo.
3. El sistema no consulta la cuenta.
4. El flujo regresa al paso 2 del flujo normal.

FA02 - Credenciales incorrectas
1. El sistema no encuentra ninguna cuenta con ese nombre de usuario, o la contraseña ingresada no corresponde al hash almacenado de esa cuenta.
2. El sistema muestra bajo el campo "Contraseña" el mensaje "Username o contraseña incorrectos.", **sin indicar cuál de los dos falló**.
3. El sistema no inicia ninguna sesión.
4. El flujo regresa al paso 2 del flujo normal.

FA03 - La cuenta tiene un baneo permanente
1. El sistema detecta que la cuenta acumuló el cuarto cruce del umbral de reportes y quedó baneada de forma permanente.
2. El sistema no inicia la sesión.
3. El sistema muestra en GUILogIn el aviso "Esta cuenta no puede iniciar sesión." con el texto "Su acceso quedó bloqueado de forma permanente tras acumular tres prohibiciones temporales.".
4. Termina el caso de uso.

FA04 - El jugador no tiene cuenta y decide crear una
1. El jugador selecciona la opción "Crear cuenta".
2. Se ejercita el caso de uso CU-02 Crear cuenta.
3. Termina el caso de uso.

FA05 - El jugador no recuerda su contraseña
1. El jugador selecciona la opción "¿Olvidaste tu acceso?".
2. Se ejercita el caso de uso CU-03 Recuperar acceso.
3. Termina el caso de uso.

FA06 - El jugador prefiere jugar sin cuenta
1. El jugador selecciona la opción "Jugar como invitado".
2. Se ejercita el caso de uso CU-04 Jugar como invitado.
3. Termina el caso de uso.

FA07 - Volver sin iniciar sesión
1. El jugador presiona el botón "Volver al menú" sin completar el formulario.
2. El sistema cierra GUILogIn y regresa a la ventana GUIMainMenu sin identidad, con las opciones "Amigos", "Historial" e "Invitaciones" no disponibles.
3. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no inicia ninguna sesión.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador queda identificado con su cuenta durante la sesión, y el sistema lo reconoce como Jugador en todas las operaciones posteriores.<br>POST-2. La contraseña no se guarda ni se registra en ningún momento: solo se comprueba contra el hash almacenado.<br>POST-3. Una **prohibición temporal** vigente **no impide** iniciar sesión: se comprueba al entrar a una sala y al iniciar una partida. Solo el **baneo permanente** impide el acceso.<br>POST-4. Si el inicio de sesión no se completó, el jugador sigue sin identidad y ninguna cuenta queda abierta. |
| **Extensiones** | CU-02 Crear cuenta, CU-03 Recuperar acceso y CU-04 Jugar como invitado, las tres alcanzables como salidas opcionales de la ventana GUILogIn |
| **Inclusiones** | Ninguna |

---

## CU-02 Crear cuenta

| | |
|---|---|
| **ID** | CU-02 |
| **Nombre** | Crear cuenta |
| **Descripción** | Permite a un usuario sin cuenta registrarse en el sistema mediante un nombre de usuario, un correo electrónico y una contraseña, con el fin de obtener una cuenta persistente que le permita jugar, agregar amigos, acumular historial de partidas y permanecer en el ranking. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Usuario sin cuenta |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El usuario sin cuenta no debe tener una sesión iniciada. |
| **Disparador** | El usuario sin cuenta selecciona la opción "Crear cuenta" desde la ventana GUILogIn. |

**Flujo Normal**

1. El sistema muestra la ventana GUISignIn con el título "Crear cuenta", la indicación "Con esto podrás jugar, tener amigos y aparecer en el ranking.", los campos "Username", "Correo" y "Contraseña", y las opciones "Crear cuenta" y "Volver". (FA01)
2. El usuario sin cuenta ingresa un nombre de usuario en el campo "Username", que debe cumplir el formato de **3 a 20 caracteres** compuestos por letras, números y guion bajo.
3. El sistema valida que el nombre de usuario no esté registrado por otra cuenta, sin distinguir mayúsculas de minúsculas, y muestra el mensaje de confirmación "Disponible. De 3 a 20 caracteres: letras, números y guion bajo.". (FA02)
4. El usuario sin cuenta ingresa un valor en el campo "Correo" con formato de correo electrónico válido.
5. El sistema valida que el correo no esté asociado a una cuenta existente, sin distinguir mayúsculas de minúsculas. (FA03)
6. El usuario sin cuenta ingresa una contraseña válida en el campo "Contraseña", de 8 a 20 caracteres, incluyendo letras mayúsculas y minúsculas, números y caracteres especiales.
7. El usuario sin cuenta selecciona la opción "Crear cuenta".
8. El sistema valida que los tres campos estén completos. (FA04)
9. El sistema valida que los tres campos cumplan el formato correspondiente. (FA05)
10. El sistema registra la nueva cuenta en la base de datos con su nombre de usuario, su correo, **el hash BCrypt de la contraseña** y su fecha de alta, asignándole un identificador único. (EX01)
11. El sistema inicia sesión con la cuenta recién creada y muestra la ventana GUIMainMenu, donde quien acaba de registrarse **ya es un Jugador**.

**Flujo Alterno**

FA01 - Cancelar el registro
1. El usuario sin cuenta presiona el botón "Volver".
2. El sistema cierra GUISignIn y regresa a la ventana GUILogIn.
3. Termina el caso de uso.

FA02 - Nombre de usuario no disponible
1. El usuario sin cuenta ingresa un nombre de usuario que ya pertenece a otra cuenta registrada.
2. El sistema muestra el mensaje "No disponible, ya existe una cuenta con ese nombre.".
3. El sistema deshabilita la opción "Crear cuenta" mientras el campo "Username" mantenga un valor no disponible.
4. El usuario sin cuenta modifica el valor del campo "Username".
5. El flujo regresa al paso 3 del flujo normal para validar nuevamente la disponibilidad.

FA03 - Correo ya registrado
1. El usuario sin cuenta ingresa un correo que ya está registrado.
2. El sistema muestra el mensaje "Ya hay una cuenta con este correo.".
3. El sistema deshabilita la opción "Crear cuenta" mientras el campo "Correo" mantenga un valor ya registrado.
4. El usuario sin cuenta modifica el valor del campo "Correo".
5. El flujo regresa al paso 5 del flujo normal para validar nuevamente la disponibilidad.

FA04 - Campos vacíos
1. El usuario sin cuenta selecciona la opción "Crear cuenta" habiendo dejado vacío alguno de los campos "Username", "Correo" o "Contraseña".
2. El sistema muestra junto a cada campo vacío un mensaje indicando que debe completarlo, sin comprobar el formato de los que sí tienen valor.
3. El sistema mantiene deshabilitada la opción "Crear cuenta" y **no consulta la base de datos**: con un campo vacío no hay nada que validar ni que registrar.
4. El usuario sin cuenta completa los campos que faltan.
5. El flujo regresa al paso del flujo normal en el que se ingresa el primer campo que estaba vacío.

FA05 - Formato de campo incorrecto
1. El usuario sin cuenta ingresa en algún campo un valor que **sí tiene contenido** pero que no cumple el formato requerido: un "Username" fuera de 3 a 20 caracteres o con caracteres distintos de letras, números y guion bajo; un "Correo" sin forma de dirección electrónica; o una "Contraseña" que no llega a los 8 caracteres o no incluye mayúsculas, minúsculas, números y caracteres especiales.
2. El sistema muestra junto al campo un mensaje indicando **qué regla de formato incumple**, que es distinto de indicar que el campo está vacío.
3. El sistema no registra ninguna cuenta y no comprueba la disponibilidad de ese valor: un valor mal formado no se busca en la base de datos.
4. El usuario sin cuenta corrige el valor del campo.
5. El flujo regresa al paso del flujo normal en el que se ingresa ese campo.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y cierra GUISignIn.

| | |
|---|---|
| **Postcondiciones** | POST-1. La nueva cuenta queda registrada en la base de datos, con nombre de usuario y correo únicos.<br>POST-2. La contraseña queda registrada en la base de datos **únicamente como hash BCrypt**, y nunca se guarda ni se registra en claro.<br>POST-3. El usuario sin cuenta queda identificado con sesión iniciada.<br>POST-4. La cuenta recién creada no tiene amigos, ni historial, ni sanciones, y aún no aparece en el ranking global, que se calcula sobre partidas terminadas. |
| **Extensiones** | Extiende a CU-01 Iniciar sesión, como comportamiento opcional desde la opción "Crear cuenta" de la ventana GUILogIn |
| **Inclusiones** | Ninguna |

---

## CU-03 Recuperar acceso

| | |
|---|---|
| **ID** | CU-03 |
| **Nombre** | Recuperar acceso |
| **Descripción** | Permite a un jugador que no recuerda su contraseña o no puede iniciar sesión solicitar el restablecimiento de su acceso a través del correo electrónico asociado a su cuenta. El sistema envía un **código** a ese correo; el jugador lo introduce dentro de la aplicación y establece una contraseña nueva. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador no debe tener una sesión iniciada. |
| **Disparador** | El jugador selecciona la opción "¿Olvidaste tu acceso?" desde la ventana GUILogIn. |

**Flujo Normal**

1. El sistema muestra la ventana GUIRecoverAccess en su primer paso, "1 · Tu correo", con la indicación "Te enviaremos un código.", el campo "Correo" y las opciones "Enviar código" y "Volver a iniciar sesión". (FA01)
2. El jugador ingresa en el campo "Correo" la dirección de correo electrónico asociada a su cuenta.
3. El jugador selecciona la opción "Enviar código".
4. El sistema valida que el campo "Correo" tenga un formato de correo electrónico válido. (FA02)
5. El sistema verifica internamente si el correo ingresado corresponde a una cuenta registrada. (FA03)
6. El sistema genera un **código de recuperación de un solo uso**, lo asocia a esa cuenta con su fecha de creación y lo envía a esa dirección de correo. **El código caduca a los cinco minutos** de haberse generado. (EX02)
7. El sistema muestra el segundo paso, "2 · El código", con la indicación "Escribe el que has recibido.", el campo "Código" y la opción "Comprobar".
8. El jugador abre el mensaje recibido en su correo, ingresa el código en el campo "Código" y selecciona la opción "Comprobar".
9. El sistema comprueba que el código corresponda a la cuenta, que no se haya utilizado y que **no hayan pasado más de cinco minutos** desde que se generó. (FA04)
10. El sistema muestra el tercer paso, "3 · Contraseña nueva", con la indicación "Con esto vuelves a entrar.", los campos "Nueva contraseña" y "Repítela", y las opciones "Guardar" y "Volver a iniciar sesión".
11. El jugador ingresa la contraseña nueva y la repite, con el mismo formato exigido al crear la cuenta.
12. El jugador selecciona la opción "Guardar".
13. El sistema valida que las dos contraseñas coincidan y cumplan el formato. (FA05)
14. El sistema **guarda la contraseña nueva como hash BCrypt**, elimina el código de recuperación utilizado y regresa a la ventana GUILogIn con un mensaje de confirmación. (EX01)

**Flujo Alterno**

FA01 - Cancelar el proceso de recuperación
1. El jugador presiona el botón "Volver a iniciar sesión" antes de completar el proceso.
2. El sistema cierra GUIRecoverAccess y regresa a la ventana GUILogIn.
3. La contraseña de la cuenta no se modifica.
4. Termina el caso de uso.

FA02 - Formato de correo incorrecto
1. El jugador deja vacío el campo "Correo" o ingresa un valor que no cumple el formato de correo electrónico.
2. El sistema muestra junto al campo el mensaje "Ingresa un correo electrónico válido.".
3. El sistema mantiene deshabilitada la opción "Enviar código" mientras el campo no cumpla el formato.
4. El jugador corrige el valor del campo "Correo".
5. El flujo regresa al paso 4 del flujo normal para validar nuevamente el correo.

FA03 - El correo ingresado no corresponde a ninguna cuenta registrada
1. El sistema verifica que el correo ingresado no está asociado a ninguna cuenta.
2. El sistema no genera ningún código ni envía correo alguno.
3. El sistema muestra, **de igual manera que en el flujo normal**, el paso "2 · El código", para no revelar si ese correo pertenece o no a una cuenta.
4. Cualquier código que el jugador introduzca será rechazado por FA04.
5. Termina el caso de uso sin que se haya iniciado ningún proceso de recuperación real.

FA04 - El código es incorrecto, ya fue utilizado o caducó
1. El jugador ingresa un código que no corresponde a su cuenta, que ya se utilizó, o que se generó **hace más de cinco minutos**.
2. El sistema muestra junto al campo el mensaje "Ese código no es el de tu cuenta." y no permite continuar al tercer paso.
3. El jugador puede corregir el código, o volver al primer paso y solicitar uno nuevo.
4. El flujo regresa al paso 8 del flujo normal, o al paso 2 si el jugador solicita un código nuevo.

FA05 - Las contraseñas no coinciden o no cumplen el formato
1. El jugador ingresa dos contraseñas distintas en "Nueva contraseña" y "Repítela", o una que no cumple el formato.
2. El sistema muestra junto al campo un mensaje indicando el error.
3. El sistema no modifica la contraseña de la cuenta y mantiene el código sin consumir.
4. El flujo regresa al paso 11 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y cierra GUIRecoverAccess.

EX02 - Falla en el envío del correo de recuperación
1. El sistema detecta que el servicio de envío de correos no pudo entregar el mensaje de recuperación.
2. El sistema muestra el mensaje "No pudimos enviar el correo en este momento. Inténtalo más tarde" y permanece en GUIRecoverAccess.
3. El sistema **no reintenta** el envío por su cuenta: el jugador puede volver a solicitar el código.

| | |
|---|---|
| **Postcondiciones** | POST-1. Si el correo pertenece a una cuenta registrada, queda generado un código de recuperación de un solo uso vinculado a esa cuenta, **válido durante cinco minutos** desde que se generó.<br>POST-2. Si el jugador completó el restablecimiento, la contraseña de la cuenta queda actualizada y almacenada como hash BCrypt, y el código utilizado queda eliminado.<br>POST-3. El sistema no ha revelado en ningún momento si un correo pertenece o no a una cuenta.<br>POST-4. La sesión no se inicia por este caso de uso: el jugador vuelve a GUILogIn para entrar con su contraseña nueva. |
| **Extensiones** | Extiende a CU-01 Iniciar sesión, como comportamiento opcional desde la opción "¿Olvidaste tu acceso?" de la ventana GUILogIn |
| **Inclusiones** | Ninguna |

---

## CU-04 Jugar como invitado

| | |
|---|---|
| **ID** | CU-04 |
| **Nombre** | Jugar como invitado |
| **Descripción** | Permite a un usuario sin cuenta obtener una identidad temporal mediante un alias, con la que puede crear salas, unirse a ellas, ser anfitrión y jugar. El alias es libre y el sistema le asigna un identificador temporal con el que lo distingue internamente y que, si llega a jugar, será también su pase de asiento para recuperar su puesto en una partida reanudada. El sistema no conserva ningún dato del invitado: no tiene historial, no aparece en el ranking global, no tiene amigos, no envía ni recibe invitaciones a sala, y no puede reportar ni ser reportado. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Usuario sin cuenta |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El usuario sin cuenta no debe tener una sesión iniciada. |
| **Disparador** | El usuario sin cuenta selecciona la opción "Jugar como invitado" desde la ventana GUILogIn. |

**Flujo Normal**

1. El sistema muestra la ventana GUIGuestAccess con el título "Jugar como invitado", la indicación "Sin cuenta, solo para esta sesión.", el campo "Tu nombre en la partida", el aviso "Como invitado no se guarda tu progreso: no tendrás historial ni ranking, no podrás tener amigos ni enviar invitaciones, y no podrás reportar ni ser reportado." y las opciones "Entrar" y "Volver". (FA01)
2. El usuario sin cuenta ingresa un alias en el campo "Tu nombre en la partida".
3. El sistema muestra junto al campo la indicación "Puede repetirse: en la sala se te distinguirá por tu color.". (FA03)
4. El usuario sin cuenta selecciona la opción "Entrar".
5. El sistema valida que el campo "Tu nombre en la partida" no esté vacío. (FA02)
6. El sistema registra al invitado únicamente en la memoria del servidor, asociado a la sesión actual, **sin crear ninguna cuenta ni ninguna fila en la base de datos**. (EX01)
7. El sistema genera para el invitado un identificador temporal y lo entrega a su cliente, que lo conserva **en memoria** mientras la aplicación siga abierta.
8. El sistema cierra GUIGuestAccess y muestra la ventana GUIMainMenu con el alias del invitado y su marca de invitado, con las opciones "Amigos" e "Historial" no disponibles, sin ofrecer la opción "Invitaciones", y con "Salas", "Ranking" y "Salir" habilitadas.

**Flujo Alterno**

FA01 - Cancelar el acceso como invitado
1. El usuario sin cuenta presiona el botón "Volver".
2. El sistema cierra GUIGuestAccess y regresa a la ventana GUILogIn.
3. No se genera ninguna identidad temporal.
4. Termina el caso de uso.

FA02 - Alias vacío
1. El usuario sin cuenta selecciona la opción "Entrar" habiendo dejado vacío el campo "Tu nombre en la partida".
2. El sistema muestra junto al campo un mensaje indicando que debe ingresar un nombre.
3. El sistema no genera ninguna identidad temporal.
4. El flujo regresa al paso 2 del flujo normal.

FA03 - El alias elegido ya lo usa otro invitado
1. El usuario sin cuenta ingresa un alias que ya está usando otro invitado conectado.
2. El sistema **lo acepta igualmente**, porque el alias es libre y no tiene que ser único ni cumplir ningún formato.
3. El sistema distingue a los dos invitados por su identificador temporal, y dentro de una sala los distinguirá además por el color que asigne a cada uno.
4. El flujo continúa en el paso 4 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y cierra GUIGuestAccess.

| | |
|---|---|
| **Postcondiciones** | POST-1. El usuario sin cuenta queda identificado como invitado únicamente durante la sesión actual, con su alias y su identificador temporal.<br>POST-2. No queda registrado ningún dato del invitado en la base de datos.<br>POST-3. La identidad de invitado **no sobrevive al cierre del cliente**. Sí sobrevive a una caída del servidor mientras el cliente conserve su identificador temporal, que es lo que le permite recuperar su puesto en una partida reanudada.<br>POST-4. El invitado puede crear salas, entrar a ellas y ser anfitrión, pero queda fuera del régimen de amistades, invitaciones, reportes y ranking global. |
| **Extensiones** | Extiende a CU-01 Iniciar sesión, como comportamiento opcional desde la opción "Jugar como invitado" de la ventana GUILogIn |
| **Inclusiones** | Ninguna |

---

## CU-05 Cambiar el idioma de la interfaz

| | |
|---|---|
| **ID** | CU-05 |
| **Nombre** | Cambiar el idioma de la interfaz |
| **Descripción** | Permite a cualquiera que tenga la aplicación en ejecución cambiar el idioma en el que muestra sus textos, entre español e inglés, aplicándolo de inmediato a todas las pantallas. **La preferencia es del equipo, no de la cuenta**: no se guarda en la base de datos y no viaja con el jugador cuando inicia sesión en otro equipo. Por eso este caso de uso no forma parte de la configuración de la cuenta. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Usuario sin cuenta, Jugador, Invitado |
| **Precondición** | PRE-1. Quien usa la aplicación debe tener la aplicación en ejecución. |
| **Disparador** | Quien usa la aplicación selecciona la opción "Idioma · Español" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema muestra la ventana GUIChangeLanguage con el título "Idioma de la interfaz", la indicación "Se aplica de inmediato a todas las pantallas.", la lista con los dos idiomas disponibles, "Español" e "English", con una marca sobre el idioma en uso, la nota "La preferencia se guarda en este equipo, no en tu cuenta." y la opción "Volver". (FA01)
2. Quien usa la aplicación selecciona de la lista un idioma distinto del que está en uso. (FA02)
3. El sistema aplica el idioma seleccionado a los textos de todas las pantallas de la aplicación, incluida la que está abierta.
4. El sistema traslada la marca de idioma en uso al idioma seleccionado.
5. Quien usa la aplicación selecciona la opción "Volver".
6. El sistema cierra GUIChangeLanguage y regresa a la ventana GUIMainMenu, ya con los textos en el idioma elegido.

**Flujo Alterno**

FA01 - Salir sin cambiar el idioma
1. Quien usa la aplicación presiona el botón "Volver" sin seleccionar ningún idioma.
2. El sistema cierra GUIChangeLanguage y regresa a la ventana desde la que se abrió, conservando el idioma en uso.
3. Termina el caso de uso.

FA02 - Quien usa la aplicación selecciona el idioma que ya está en uso
1. Quien usa la aplicación selecciona de la lista el idioma que ya está marcado como en uso.
2. El sistema no modifica los textos de la interfaz ni traslada la marca.
3. El flujo continúa en el paso 5 del flujo normal.

**Excepciones**

Ninguna. El cambio de idioma **no requiere conexión con el servidor**: es una preferencia del cliente.

| | |
|---|---|
| **Postcondiciones** | POST-1. La interfaz queda mostrada en el idioma seleccionado en todas sus pantallas.<br>POST-2. La preferencia queda asociada al equipo en el que se ejecuta la aplicación y **no se guarda en ninguna cuenta**.<br>POST-3. El cambio no afecta a ningún otro jugador ni a lo que los demás ven de este. |
| **Extensiones** | Ninguna. **El idioma no cuelga de ningún otro caso de uso:** no es un dato de la cuenta ni del perfil, sino una preferencia del equipo en el que se ejecuta la aplicación, y se alcanza directamente desde la ventana GUIMainMenu |
| **Inclusiones** | Ninguna |

---

## CU-06 Modificar el perfil

| | |
|---|---|
| **ID** | CU-06 |
| **Nombre** | Modificar el perfil |
| **Descripción** | Permite a un jugador consultar su perfil y modificar los dos datos que lo componen, su nombre de usuario y su avatar, que son los que ven los demás jugadores. El correo se muestra únicamente para consulta y no puede modificarse. El archivo del avatar lo guarda el servidor; la base de datos solo conserva su referencia. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta. |
| **Disparador** | El jugador selecciona su avatar, en la esquina superior derecha de la ventana GUIMainMenu, y elige la opción "Perfil". |

**Flujo Normal**

1. El sistema muestra la ventana GUIProfile con el avatar actual de la cuenta, la opción "Cambiar avatar" acompañada de la indicación "PNG o JPG · hasta 5 MB", el título "Tu cuenta" con la indicación "Así te ven los demás jugadores.", el campo "Username" con el nombre de usuario actual, el campo "Correo" en modo de solo lectura, y las opciones "Guardar", "Configurar cuenta" y "Volver al menú". (FA01) (FA05) (FA06)
2. El jugador modifica el valor del campo "Username" con un valor que cumpla el formato de 3 a 20 caracteres compuestos por letras, números y guion bajo.
3. El sistema valida que el nombre de usuario no esté registrado por otra cuenta, sin distinguir mayúsculas de minúsculas. (FA02)
4. El jugador selecciona la opción "Cambiar avatar" y elige un archivo de imagen de su equipo. (FA03)
5. El sistema valida que el archivo elegido sea PNG o JPG y que no supere los 5 MB. (FA04)
6. El sistema muestra el archivo elegido como vista previa del avatar.
7. El jugador selecciona la opción "Guardar".
8. El sistema guarda el archivo del avatar en el disco del servidor y actualiza en la base de datos el nombre de usuario de la cuenta y la referencia a su avatar. (EX01)
9. El sistema muestra el perfil ya actualizado en GUIProfile y el nuevo avatar en la esquina superior derecha de GUIMainMenu.

**Flujo Alterno**

FA01 - Salir del perfil sin guardar
1. El jugador presiona el botón "Volver al menú".
2. El sistema cierra GUIProfile sin aplicar ningún cambio y regresa a la ventana GUIMainMenu.
3. Termina el caso de uso.

FA02 - Nombre de usuario no disponible
1. El jugador ingresa un nombre de usuario que ya pertenece a otra cuenta registrada.
2. El sistema muestra junto al campo el mensaje "No disponible: ya existe una cuenta con ese nombre.".
3. El sistema deshabilita la opción "Guardar" mientras el campo "Username" mantenga un valor no disponible.
4. El jugador modifica el valor del campo "Username".
5. El flujo regresa al paso 3 del flujo normal para validar nuevamente la disponibilidad.

FA03 - El jugador no cambia el avatar
1. El jugador cierra el selector de archivos sin elegir ninguno, o no selecciona la opción "Cambiar avatar".
2. El sistema conserva el avatar actual de la cuenta y su referencia.
3. El flujo continúa en el paso 7 del flujo normal.

FA04 - Archivo de avatar no admitido
1. El jugador elige un archivo que supera los 5 MB o que no es PNG ni JPG.
2. El sistema muestra junto a la opción "Cambiar avatar" un mensaje indicando el motivo del rechazo, como "El archivo pesa 7.4 MB.".
3. El sistema no sustituye el avatar actual ni guarda el archivo en el servidor.
4. El flujo regresa al paso 4 del flujo normal.

FA05 - El archivo del avatar ya no está en el servidor
1. El sistema detecta que la referencia del avatar guardada en la base de datos no corresponde a ningún archivo existente en el disco.
2. El sistema muestra el avatar por defecto en lugar del avatar de la cuenta, con el mensaje "Tu imagen ya no está en el servidor: se muestra el avatar por defecto.".
3. El sistema no modifica la referencia guardada ni impide continuar con el resto del caso de uso.
4. El flujo continúa en el paso 2 del flujo normal.

FA06 - El jugador entra a la configuración de la cuenta
1. El jugador selecciona la opción "Configurar cuenta".
2. El sistema muestra la ventana GUIAccountConfiguration con el apartado "Eliminar la cuenta", su advertencia "Se borran tu perfil, tus amistades y tus partidas. No se puede deshacer.", la opción "Eliminar cuenta" y la opción "Volver al perfil".
3. Si el jugador selecciona "Eliminar cuenta", se ejercita el caso de uso CU-07 Eliminar la cuenta.
4. Al terminar, el sistema regresa a la ventana GUIProfile y el flujo regresa al paso 1 del flujo normal, **salvo que la cuenta haya sido eliminada**, en cuyo caso termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no aplica ningún cambio al perfil.

| | |
|---|---|
| **Postcondiciones** | POST-1. El nombre de usuario de la cuenta queda actualizado y sigue siendo único entre todas las cuentas, sin distinguir mayúsculas.<br>POST-2. El archivo del avatar queda almacenado en el disco del servidor y la base de datos conserva únicamente su referencia.<br>POST-3. Los demás jugadores ven el nombre de usuario y el avatar actualizados, también en las salas en las que esté.<br>POST-4. El correo de la cuenta no ha cambiado. |
| **Extensiones** | CU-07 Eliminar la cuenta, alcanzable desde la opción "Configurar cuenta". **El idioma no es una extensión de este caso de uso:** no es un dato de la cuenta, sino una preferencia del equipo, y se cambia desde la ventana GUIMainMenu mediante CU-05 |
| **Inclusiones** | Ninguna |

---

## CU-07 Eliminar la cuenta

| | |
|---|---|
| **ID** | CU-07 |
| **Nombre** | Eliminar la cuenta |
| **Descripción** | Permite a un jugador eliminar definitivamente su cuenta del sistema, junto con su perfil, sus amistades, sus solicitudes, sus invitaciones pendientes, sus participaciones en partidas y sus reportes y sanciones. La acción es irreversible y no se permite mientras el jugador esté participando en una partida en curso. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta.<br>PRE-3. El jugador debe encontrarse en la ventana GUIAccountConfiguration. |
| **Disparador** | El jugador selecciona la opción "Eliminar cuenta" desde la ventana GUIAccountConfiguration. |

**Flujo Normal**

1. El sistema muestra la ventana GUIAccountConfiguration con el apartado "Eliminar la cuenta", la advertencia "Se borran tu perfil, tus amistades y tus partidas. No se puede deshacer.", la opción "Eliminar cuenta" y la opción "Volver al perfil". (FA01)
2. El jugador selecciona la opción "Eliminar cuenta".
3. El sistema muestra el aviso de confirmación "Eliminar la cuenta", con el texto "Se borrarán tu perfil, tus amistades y tus partidas. Esta acción no se puede deshacer." y las opciones "Eliminar" y "Cancelar". (FA02)
4. El jugador selecciona la opción "Eliminar".
5. El sistema verifica que el jugador no esté participando en una partida en curso. (FA03)
6. El sistema elimina de la base de datos la cuenta y, en cascada, su perfil, sus amistades y solicitudes, sus invitaciones enviadas y recibidas, sus participaciones en partidas, su código de recuperación si lo tuviera, y los reportes y las sanciones asociados a ella. (EX01)
7. El sistema elimina del disco del servidor el archivo del avatar de la cuenta, si lo tenía.
8. El sistema elimina las partidas que hayan quedado sin ninguna participación tras el borrado, junto con su estado si aún lo tuvieran. (FA04)
9. El sistema **saca al jugador de la sala** en la que estuviera, aplicando las mismas reglas que rigen cualquier salida de una sala. (FA05)
10. El sistema cierra la sesión del jugador y lo redirige a la ventana GUIMainMenu sin identidad.

**Flujo Alterno**

FA01 - Salir de la configuración sin eliminar la cuenta
1. El jugador presiona el botón "Volver al perfil".
2. El sistema cierra GUIAccountConfiguration y regresa a la ventana GUIProfile.
3. Termina el caso de uso.

FA02 - Cancelar la eliminación
1. El jugador selecciona la opción "Cancelar" en el aviso de confirmación.
2. El sistema cierra el aviso sin eliminar nada y permanece en GUIAccountConfiguration.
3. Termina el caso de uso.

FA03 - El jugador está participando en una partida en curso
1. El sistema detecta que el jugador participa en una partida que todavía no ha terminado.
2. El sistema no elimina la cuenta ni ninguno de sus datos.
3. El sistema muestra un mensaje indicando que no puede eliminarse la cuenta mientras haya una partida en curso.
4. El flujo regresa al paso 1 del flujo normal.

FA04 - Alguna partida queda sin ningún participante registrado
1. El sistema detecta que, tras el borrado, una partida terminada en la que participó el jugador ya no conserva ninguna participación, porque los demás jugadores eran invitados o sus cuentas ya se habían eliminado.
2. El sistema elimina esa partida, que ya no puede consultar nadie.
3. El flujo continúa en el paso 9 del flujo normal.

FA05 - El jugador está dentro de una sala sin partida en curso
1. El sistema detecta que el jugador forma parte de una sala en la que no se está jugando.
2. El sistema lo **retira de la lista de jugadores** de esa sala y libera el color que tenía asignado, que vuelve al conjunto de colores disponibles.
3. Si el jugador era el **anfitrión** y quedan jugadores dentro, la condición de anfitrión pasa al siguiente jugador en el orden en que están guardados.
4. Si el jugador era el **único** jugador de la sala, la sala queda vacía y **se cierra**: el sistema escribe su fecha de cierre, con lo que su código queda libre, elimina sus invitaciones pendientes y borra de la memoria su lista de jugadores, sus colores, su ranking y su chat.
5. El sistema actualiza la lista de jugadores en la ventana GUIRoom de los que permanecen dentro.
6. El flujo continúa en el paso 10 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no elimina la cuenta.

| | |
|---|---|
| **Postcondiciones** | POST-1. La cuenta y todos sus datos asociados quedan eliminados de la base de datos y no pueden recuperarse.<br>POST-2. El archivo del avatar queda eliminado del disco del servidor.<br>POST-3. El jugador queda sin sesión iniciada y **fuera de cualquier sala**, que sigue rigiéndose por sus propias reglas: hereda anfitrión si hacía falta, y se cierra si se quedó vacía.<br>POST-4. Las partidas ya terminadas conservan cuántos jugadores participaron en ellas, aunque el resultado del jugador eliminado desaparece y su puesto quede vacío.<br>POST-5. Las partidas que quedaron sin ninguna participación dejan de existir.<br>POST-6. Al eliminarse la cuenta se van con ella sus sanciones, incluido un baneo permanente. |
| **Extensiones** | Extiende a CU-06 Modificar el perfil, como comportamiento opcional desde la opción "Configurar cuenta" de la ventana GUIProfile |
| **Inclusiones** | Ninguna |

---

## CU-08 Enviar una solicitud de amistad

| | |
|---|---|
| **ID** | CU-08 |
| **Nombre** | Enviar una solicitud de amistad |
| **Descripción** | Permite a un jugador buscar a otro jugador por su nombre de usuario y enviarle una solicitud de amistad, que queda pendiente hasta que el destinatario la responda, aunque este no esté conectado. Entre dos cuentas solo puede existir una relación: no pueden coexistir una solicitud en un sentido y otra en el sentido contrario. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta. |
| **Disparador** | El jugador selecciona la opción "Amigos" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema muestra la ventana GUIFriends con las pestañas "Amigos", "Recibidas", "Enviadas" y "Buscar", abierta en la pestaña "Amigos" con la lista de sus amigos actuales, indicando de cada uno su nombre de usuario y su avatar, y la opción "Volver al menú", presente en las cuatro pestañas. (FA01) (FA06) (FA07) (FA08)
2. El jugador selecciona la pestaña "Buscar".
3. El jugador ingresa el nombre de usuario del jugador que busca y acciona la búsqueda.
4. El sistema busca en la base de datos la cuenta cuyo nombre de usuario coincida con el valor ingresado, sin distinguir mayúsculas de minúsculas. **El nombre de usuario es el único criterio de búsqueda**: no se busca por correo, y la coincidencia es exacta. (FA02)
5. El sistema muestra el jugador encontrado con su nombre de usuario, su avatar y la opción para enviarle una solicitud de amistad. (FA05)
6. El jugador acciona la opción de enviar la solicitud.
7. El sistema valida que no exista ya una amistad ni una solicitud pendiente entre ambos jugadores, en ninguno de los dos sentidos. (FA03) (FA04)
8. El sistema registra la solicitud en la base de datos con estado pendiente, dirigida a la cuenta encontrada, con la fecha de la solicitud y sin fecha de respuesta. (EX01)
9. El sistema muestra la solicitud en la pestaña "Enviadas" del jugador.

**Flujo Alterno**

FA01 - El jugador todavía no tiene amigos
1. El sistema detecta que la cuenta no tiene ninguna amistad aceptada.
2. El sistema muestra en la pestaña "Amigos" el mensaje "Todavía no tienes amigos" junto con la indicación "Busca a alguien por su username para enviarle una solicitud.".
3. El flujo continúa en el paso 2 del flujo normal.

FA02 - La búsqueda no encuentra ninguna cuenta con ese nombre de usuario
1. El jugador ingresa un nombre de usuario que no pertenece a ninguna cuenta registrada, porque lo escribió mal o porque ese jugador no existe.
2. El sistema no encuentra ninguna coincidencia. **La búsqueda es por coincidencia exacta del nombre de usuario, sin distinguir mayúsculas**, no por fragmentos: buscar "ana" no devuelve "ana_torres".
3. El sistema muestra la pestaña "Buscar" **sin ninguna fila de resultado**, de modo que el jugador no tiene sobre qué accionar la opción de enviar la solicitud.
4. El jugador corrige el valor de la búsqueda y vuelve a accionarla.
5. El flujo regresa al paso 3 del flujo normal.

FA03 - El jugador encontrado ya es su amigo
1. El jugador busca a alguien con quien **ya tiene una amistad aceptada**.
2. El sistema muestra la fila de ese jugador con su nombre de usuario y su avatar, pero **en lugar de la opción de enviar una solicitud indica que ya es su amigo**.
3. El sistema no registra ninguna solicitud: entre dos cuentas solo puede existir una relación, y esa relación ya existe.
4. El jugador puede buscar a otro jugador. El flujo regresa al paso 3 del flujo normal.

FA04 - Ya hay una solicitud pendiente entre los dos, en cualquiera de los dos sentidos
1. El jugador busca a alguien a quien **ya le envió una solicitud** que sigue sin responder, o que **le envió una solicitud a él** y todavía no ha respondido.
2. El sistema muestra la fila de ese jugador indicando que la solicitud ya está enviada, si la envió él, o que tiene una solicitud suya esperando respuesta, si fue el otro quien la envió; **en ninguno de los dos casos ofrece la opción de enviar una solicitud**.
3. El sistema no registra una segunda solicitud: si lo hiciera, los dos jugadores quedarían con una solicitud cruzada y al aceptarlas ambos aparecerían dos veces como amigos.
4. Si la solicitud pendiente es una que él recibió, puede responderla desde la pestaña "Recibidas", con lo que se ejercita el caso de uso CU-09 Responder una solicitud de amistad.
5. El flujo regresa al paso 3 del flujo normal.

FA05 - El jugador busca su propio nombre de usuario
1. El jugador ingresa en la búsqueda su propio nombre de usuario.
2. **El sistema no muestra ningún resultado para el propio jugador**, igual que si ese nombre no existiera, porque un jugador no puede agregarse a sí mismo como amigo.
3. Como no hay fila, no hay ninguna opción de enviar una solicitud que el jugador pueda accionar, y el sistema no llega a comprobar ni a registrar nada.
4. El flujo regresa al paso 3 del flujo normal.

FA06 - El jugador consulta las solicitudes que ya envió
1. El jugador selecciona la pestaña "Enviadas".
2. El sistema muestra las solicitudes que él envió y **siguen sin respuesta**, indicando de cada una el nombre de usuario y el avatar del destinatario.
3. El sistema **no ofrece ninguna acción sobre ellas**: cancelar una solicitud ya enviada no es una operación definida del sistema.
4. Las que ya fueron aceptadas no aparecen aquí, sino en la pestaña "Amigos"; las que fueron rechazadas **no aparecen en ninguna parte**, porque el rechazo las borra sin dejar registro y sin avisar a quien las envió.
5. El flujo regresa al paso 1 del flujo normal.

FA07 - El jugador responde una solicitud recibida
1. El jugador selecciona la pestaña "Recibidas".
2. Se ejercita el caso de uso CU-09 Responder una solicitud de amistad.
3. Al terminar, el flujo regresa al paso 1 del flujo normal.

FA08 - Salir de la ventana de amigos
1. El jugador presiona el botón "Volver al menú".
2. El sistema cierra GUIFriends y regresa a la ventana GUIMainMenu, conservando las solicitudes pendientes en los dos sentidos.
3. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no registra la solicitud.

| | |
|---|---|
| **Postcondiciones** | POST-1. La solicitud de amistad queda registrada en la base de datos con estado pendiente y aparece en la pestaña "Enviadas" del solicitante.<br>POST-2. La solicitud aparece en la pestaña "Recibidas" del destinatario, **aunque este no estuviera conectado** cuando se envió.<br>POST-3. Entre dos cuentas solo existe una amistad o una solicitud pendiente, nunca las dos ni una en cada sentido.<br>POST-4. La solicitud no caduca: se conserva hasta que el destinatario la responda o hasta que se elimine alguna de las dos cuentas. |
| **Extensiones** | CU-09 Responder una solicitud de amistad y CU-20 Eliminar un amigo, alcanzables desde las pestañas "Recibidas" y "Amigos" de la ventana GUIFriends |
| **Inclusiones** | Ninguna |

---

## CU-09 Responder una solicitud de amistad

| | |
|---|---|
| **ID** | CU-09 |
| **Nombre** | Responder una solicitud de amistad |
| **Descripción** | Permite a un jugador aceptar o rechazar las **solicitudes de amistad** que ha recibido. Al aceptarla queda registrada la amistad entre las dos cuentas y cada una aparece en la lista de amigos de la otra; al rechazarla la solicitud se elimina y no queda ningún registro de ella, de modo que el solicitante puede volver a enviarla más adelante. **No debe confundirse con la invitación a una sala**, que es otra cosa: vive en otra entidad, llega a otra ventana —GUIInvitationsReceived, desde la opción "Invitaciones" del menú principal— y se responde en el caso de uso CU-17. La pestaña "Recibidas" de la ventana de amigos contiene **solo solicitudes de amistad**. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta. |
| **Disparador** | El jugador selecciona la pestaña "Recibidas" de la ventana GUIFriends. |

**Flujo Normal**

1. El sistema muestra la ventana GUIFriends en la pestaña "Recibidas", bajo el título "Solicitudes recibidas", con las solicitudes de amistad pendientes dirigidas a la cuenta, indicando de cada una el nombre de usuario y el avatar de quien la envió, y las opciones de aceptar y de rechazar. (FA01) (FA03) (FA04)
2. El jugador acciona la opción de aceptar sobre la solicitud que quiere responder. (FA02)
3. El sistema registra la amistad entre las dos cuentas como aceptada y guarda la fecha de la respuesta. (EX01)
4. El sistema retira la solicitud de la pestaña "Recibidas".
5. El sistema muestra al nuevo amigo en la pestaña "Amigos", con la opción de eliminarlo de la lista.
6. El sistema muestra al solicitante, la próxima vez que consulte sus amigos, al jugador en su propia lista de amigos, porque la amistad es una sola para la pareja.
7. El sistema **no envía ninguna invitación a sala** al aceptar la amistad, ni crea ninguna: la amistad y la invitación a una sala son dos cosas distintas. (FA05)

**Flujo Alterno**

FA01 - El jugador no tiene solicitudes recibidas
1. El sistema detecta que no hay ninguna solicitud pendiente dirigida a la cuenta.
2. El sistema muestra la pestaña "Recibidas" sin ninguna solicitud.
3. Termina el caso de uso.

FA02 - Rechazar la solicitud
1. El jugador acciona la opción de rechazar sobre la solicitud.
2. El sistema **elimina** la solicitud de la base de datos, sin conservar ningún registro de ella: la solicitud rechazada no se guarda.
3. El sistema retira la solicitud de la pestaña "Recibidas" y no agrega ningún amigo.
4. El sistema no avisa al solicitante del rechazo, y nada le impide volver a enviar una solicitud más adelante, porque no existe el bloqueo.
5. El flujo regresa al paso 1 del flujo normal.

FA03 - Salir sin responder ninguna solicitud
1. El jugador presiona el botón "Volver al menú".
2. El sistema cierra GUIFriends y regresa a la ventana GUIMainMenu, **conservando** las solicitudes pendientes.
3. Termina el caso de uso.

FA04 - La cuenta que envió la solicitud ya no existe
1. El sistema detecta que la cuenta solicitante fue eliminada después de enviar la solicitud.
2. La solicitud desapareció con ella, en cascada, así que ya no figura en la lista.
3. El flujo continúa en el paso 1 del flujo normal.

FA05 - El jugador quiere invitar a su nuevo amigo a una sala
1. El jugador, ya con la amistad aceptada, quiere que ese amigo entre a una sala con él.
2. **La pestaña "Amigos" no es el sitio donde eso ocurre.** Para invitar hay que estar dentro de una sala: se ejercita el caso de uso CU-16 Invitar jugadores a la sala, que se dispara desde la ventana GUIRoom y ofrece la pestaña "Mis amigos" precisamente para elegir entre ellos.
3. **La amistad no es requisito para invitar**, ni la invitación requiere amistad: CU-16 también permite invitar buscando por nombre de usuario a quien no es amigo. Lo único que aporta la amistad es tener a ese jugador a mano en la primera pestaña.
4. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no registra la respuesta a la solicitud.

| | |
|---|---|
| **Postcondiciones** | POST-1. Si el jugador aceptó, la amistad queda registrada entre las dos cuentas, con su fecha de respuesta, y cada una aparece en la lista de amigos de la otra.<br>POST-2. Si el jugador rechazó, la solicitud queda eliminada y no queda ningún registro de ella.<br>POST-3. Las solicitudes que el jugador no respondió siguen pendientes.<br>POST-4. **No se ha creado ninguna invitación a sala.** Una amistad aceptada solo añade al jugador a la lista de amigos; lo único que cambia respecto de las salas es que ese amigo aparecerá en la pestaña "Mis amigos" de la ventana GUIInvitePlayers cuando el jugador esté dentro de una sala e invite desde ella. |
| **Extensiones** | Extiende a CU-08 Enviar una solicitud de amistad, como comportamiento opcional desde la pestaña "Recibidas" de la ventana GUIFriends |
| **Inclusiones** | Ninguna |

---

## CU-10 Consultar el historial de partidas

| | |
|---|---|
| **ID** | CU-10 |
| **Nombre** | Consultar el historial de partidas |
| **Descripción** | Permite a un jugador consultar las partidas que ya han terminado y en las que participó, con su fecha, cuántos jugaron, contra quién jugó, el puesto y los puntos que obtuvo y cómo terminó cada partida. Los invitados no dejan rastro, así que se cuentan pero no se nombran. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta. |
| **Disparador** | El jugador selecciona la opción "Historial" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema consulta en la base de datos las partidas terminadas en las que participó la cuenta. (EX01)
2. El sistema muestra la ventana GUIHistory con una tabla de columnas "Fecha", "Jugaron", "Rivales", "Puesto", "Puntos" y "Resultado", ordenada de la partida más reciente a la más antigua. (FA01)
3. El sistema muestra en la columna "Resultado" de cada fila cómo terminó la partida, con la marca "Terminada", "Abandonada" o "Interrumpida".
4. El sistema muestra en la columna "Jugaron" cuántos jugadores se sentaron en esa partida, y en la columna "Rivales" los nombres de los jugadores con cuenta de los que conserva el dato. (FA02)
5. El sistema muestra sin puesto ni puntos las partidas cuyo resultado es "Interrumpida", porque esas partidas no tienen resultado que mostrar. (FA03)
6. El jugador consulta la información y selecciona la opción "Volver al menú". (FA04)
7. El sistema cierra GUIHistory y regresa a la ventana GUIMainMenu.

**Flujo Alterno**

FA01 - El jugador no tiene ninguna partida terminada
1. El sistema no encuentra partidas terminadas en las que haya participado la cuenta.
2. El sistema muestra el mensaje "Todavía no has terminado ninguna partida" junto con la indicación "Aquí aparecerán tus resultados.".
3. El flujo continúa en el paso 6 del flujo normal.

FA02 - La partida incluyó jugadores de los que el sistema no conserva datos
1. En la partida participaron invitados, o participó una cuenta que después fue eliminada.
2. El sistema muestra en la columna "Rivales" únicamente a los jugadores con cuenta que conserva, indicando a los demás por su número, como "1 invitado".
3. La columna "Jugaron" sigue indicando cuántos jugaron realmente, porque ese dato se guardó al iniciar la partida y no se deduce de las participaciones.
4. El flujo continúa en el paso 5 del flujo normal.

FA03 - El jugador fue retirado de la partida
1. El sistema detecta que en esa partida el jugador quedó retirado por haber agotado su ventana de reconexión.
2. El sistema muestra igualmente su puesto y sus puntos, porque **la partida sigue contando** para él y para el ranking.
3. El flujo continúa en el paso 6 del flujo normal.

FA04 - El jugador consulta el detalle de una partida
1. El jugador selecciona una de las partidas de la tabla.
2. Se ejercita el caso de uso CU-11 Consultar el detalle de una partida.
3. Al terminar, el flujo regresa al paso 2 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y cierra GUIHistory.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador conoce el resultado de sus partidas terminadas.<br>POST-2. El historial no se modifica: la consulta no altera ningún dato del sistema.<br>POST-3. El historial no incluye partidas en curso: solo aparecen las que ya terminaron. |
| **Extensiones** | CU-11 Consultar el detalle de una partida, alcanzable al seleccionar una partida de la tabla |
| **Inclusiones** | Ninguna |

---

## CU-11 Consultar el detalle de una partida

| | |
|---|---|
| **ID** | CU-11 |
| **Nombre** | Consultar el detalle de una partida |
| **Descripción** | Permite a un jugador consultar el resultado completo de una de las partidas de su historial: cuándo se jugó, cuántos jugaron, cuánto duró y el puesto y los puntos de cada participante de los que el sistema conserva datos. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta.<br>PRE-3. El jugador debe estar consultando su historial de partidas en la ventana GUIHistory. |
| **Disparador** | El jugador selecciona una de las partidas de la tabla de la ventana GUIHistory. |

**Flujo Normal**

1. El sistema consulta en la base de datos el resultado de la partida seleccionada. (EX01)
2. El sistema muestra la ventana GUIGameDetails encabezada por la fecha de la partida y la indicación del número de jugadores y su duración, como "4 jugadores · 38 minutos". (FA01)
3. El sistema muestra una tabla de columnas "Puesto", "Jugador" y "Puntos" con el resultado de cada participante, ordenada por puesto. (FA02)
4. El sistema resalta en la tabla la fila del propio jugador.
5. El jugador selecciona la opción "Volver al historial".
6. El sistema cierra GUIGameDetails y regresa a la ventana GUIHistory.

**Flujo Alterno**

FA01 - La partida seleccionada quedó interrumpida
1. El sistema detecta que la partida terminó como "Interrumpida" y que, por tanto, no tiene puestos ni puntuaciones.
2. El sistema muestra la partida sin resultados, indicando únicamente su fecha y cuántos jugaron.
3. El flujo continúa en el paso 5 del flujo normal.

FA02 - La partida incluyó jugadores de los que el sistema no conserva datos
1. En la partida participaron invitados, o participó una cuenta que después fue eliminada.
2. El sistema muestra únicamente las filas de resultado que conserva, por lo que **la tabla puede no incluir todos los puestos** de la partida: si ganó un invitado, ninguna fila tendrá el puesto 1.
3. La duración y el número de jugadores se muestran igualmente, porque son datos de la partida y no de sus participantes.
4. El flujo continúa en el paso 5 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y regresa a la ventana GUIHistory.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador conoce el resultado de todos los participantes de esa partida de los que el sistema conserva datos.<br>POST-2. La consulta no altera ningún dato del sistema.<br>POST-3. El detalle no muestra la secuencia de jugadas ni el desglose de puntos por ronda, porque esos datos no se guardan. |
| **Extensiones** | Extiende a CU-10 Consultar el historial de partidas, como comportamiento opcional al seleccionar una partida de la tabla |
| **Inclusiones** | Ninguna |

---

## CU-12 Consultar el ranking global

| | |
|---|---|
| **ID** | CU-12 |
| **Nombre** | Consultar el ranking global |
| **Descripción** | Permite a un jugador consultar la clasificación general de las cuentas del sistema, ordenada por partidas ganadas, con las victorias, los puntos acumulados y las partidas jugadas de cada una. La clasificación se calcula sobre las partidas terminadas y no incluye a los invitados, de los que no se guarda ningún dato. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe estar identificado, con una cuenta o como invitado. |
| **Disparador** | El jugador o el invitado selecciona la opción "Ranking" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema calcula la clasificación a partir de los resultados de las partidas terminadas. (EX01)
2. El sistema ordena la clasificación por número de **victorias**; en caso de empate, por **puntos acumulados** y, si el empate persiste, por **menos partidas jugadas**.
3. El sistema muestra la ventana GUIGlobalRanking con una tabla de columnas "#", "Jugador", "Victorias", "Puntos" y "Partidas". (FA01)
4. El sistema deja fuera de la clasificación a los invitados y los resultados de las partidas interrumpidas; **sí incluye** las partidas jugadas con invitados y aquellas en las que el jugador o el invitado fue retirado.
5. El sistema resalta la fila del propio jugador cuando este aparece en la clasificación. (FA02)
6. El jugador o el invitado consulta la información y selecciona la opción "Volver al menú".
7. El sistema cierra GUIGlobalRanking y regresa a la ventana GUIMainMenu.

**Flujo Alterno**

FA01 - No hay partidas terminadas
1. El sistema no encuentra ninguna partida terminada con la que calcular la clasificación.
2. El sistema muestra el mensaje "Aún no hay partidas terminadas" junto con la indicación "El ranking se calcula con los resultados de las partidas jugadas.".
3. El flujo continúa en el paso 6 del flujo normal.

FA02 - El jugador o el invitado no aparece en la clasificación
1. El jugador o el invitado está identificado como invitado, o su cuenta no tiene ninguna partida terminada con resultado.
2. El sistema muestra la clasificación sin resaltar ninguna fila.
3. El flujo continúa en el paso 6 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y cierra GUIGlobalRanking.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador o el invitado conoce la clasificación general vigente.<br>POST-2. La consulta no altera ningún dato del sistema: el ranking global no se almacena, se calcula.<br>POST-3. Un invitado puede consultar el ranking, pero nunca aparecerá en él. |
| **Extensiones** | Ninguna |
| **Inclusiones** | Ninguna |

---

## CU-13 Entrar a la sala

| | |
|---|---|
| **ID** | CU-13 |
| **Nombre** | Entrar a la sala |
| **Descripción** | Comportamiento común por el que el sistema admite a un jugador en una sala: comprueba que no tenga una sanción vigente, le asigna un color, lo agrega a la lista de jugadores que mantiene en memoria, avisa a los jugadores que ya están dentro y le muestra la sala con su código, su tipo, sus jugadores, su ranking, su chat y las acciones disponibles. **Se ejecuta siempre** como parte de crear una sala, de unirse a una sala y de aceptar una invitación a una sala, y por eso se describe una vez y los tres casos la incluyen. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe estar identificado, con una cuenta o como invitado.<br>PRE-3. El jugador o el invitado no debe encontrarse dentro de otra sala.<br>PRE-4. La sala debe estar abierta y tener menos de cuatro jugadores, contando también a los que estén marcados como inactivos. |
| **Disparador** | El sistema ejecuta este caso de uso como parte de CU-14 Unirse a una sala, CU-15 Crear una sala o CU-17 Responder una invitación a una sala. |

**Flujo Normal**

1. El sistema comprueba que, si el jugador o el invitado tiene cuenta, no tenga una sanción temporal vigente. (FA04)
2. El sistema asigna al jugador o al invitado uno de los cuatro colores que no esté en uso en esa sala, que será el que identifique sus piezas si se juega una partida.
3. El sistema agrega al jugador o al invitado al final de la lista ordenada de jugadores de la sala, que mantiene en la memoria del servidor. (EX01)
4. El sistema muestra la ventana GUIRoom con el código de la sala y la opción "Copiar", la etiqueta del tipo de sala, "Pública" o "Privada", y las opciones "Invitar jugadores", "Salir" e "Iniciar partida".
5. El sistema muestra el apartado "Jugadores" con la lista de quienes están dentro y la indicación de cuántos son sobre el máximo, como "3 de 4", mostrando el color de cada uno y señalando con la marca "Anfitrión" a quien lo sea, con la marca "Invitado" a quienes no tengan cuenta y con la marca "Sin conexión" a quienes estén inactivos. (FA01) (FA05)
6. El sistema muestra la opción "Iniciar partida" **únicamente al anfitrión**, deshabilitada mientras haya un solo jugador activo y habilitada cuando haya de dos a cuatro; a los demás jugadores les muestra la nota "Solo el anfitrión puede iniciar la partida y expulsar jugadores.". (FA02)
7. El sistema muestra el apartado "Ranking de la sala" con los puntos y las victorias de cada jugador en las partidas jugadas en esa sala, ordenado por puntos, **incluidos los invitados**. (FA03)
8. El sistema muestra el apartado de chat de la sala con los mensajes que ya se hayan escrito en ella, **incluidos los anteriores a la llegada del jugador o del invitado**, y el recuadro de escritura con la indicación "Pulsa Enter para escribir". (FA06)
9. El sistema actualiza la lista de jugadores en la ventana GUIRoom de los demás jugadores de la sala, que ven aparecer al recién llegado con su color.

**Flujo Alterno**

FA01 - El jugador o el invitado entra como anfitrión de la sala
1. El jugador o el invitado es quien acaba de crear la sala.
2. El sistema lo registra como anfitrión y lo señala con la marca "Anfitrión" en la lista de jugadores.
3. El flujo continúa en el paso 6 del flujo normal.

FA02 - El jugador o el invitado es el único de la sala
1. El sistema detecta que el jugador o el invitado que entra es el único que hay dentro.
2. El sistema muestra la opción "Iniciar partida" **deshabilitada**, porque una partida necesita al menos dos jugadores activos.
3. El flujo continúa en el paso 7 del flujo normal.

FA03 - Todavía no ha terminado ninguna partida en esa sala
1. El sistema detecta que en la sala no ha terminado ninguna partida.
2. El sistema muestra el apartado "Ranking de la sala" con el mensaje "Aún sin partidas" y la indicación "Cuando terminen partidas en esta sala, verás aquí los puntos de cada jugador.".
3. El flujo continúa en el paso 8 del flujo normal.

FA04 - El jugador o el invitado tiene una sanción temporal vigente
1. El sistema detecta que la cuenta del jugador o el invitado tiene una prohibición temporal que todavía no ha vencido.
2. El sistema **no lo admite** en la sala y no le asigna ningún color.
3. El sistema muestra el aviso "No puedes entrar a salas" con el texto "Tienes una prohibición temporal por mensajes reportados." y el tiempo que le queda de prohibición, con la opción "Entendido".
4. Termina el caso de uso.

FA05 - Hay jugadores inactivos en la sala
1. El sistema detecta que alguno de los jugadores de la sala perdió la conexión y está marcado como inactivo.
2. El sistema lo muestra igualmente en la lista, con la marca "Sin conexión", porque **sigue ocupando su plaza**, y añade a la indicación de ocupación cuántos están activos, como "4 de 4 · 3 activos".
3. El flujo continúa en el paso 6 del flujo normal.

FA06 - El chat de la sala todavía no tiene mensajes
1. El sistema detecta que en la sala no se ha escrito ningún mensaje.
2. El sistema muestra el apartado de chat con la indicación "Todavía nadie ha escrito.".
3. El flujo continúa en el paso 9 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el jugador o el invitado no queda dentro de la sala.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador o el invitado forma parte de la sala, con su color asignado, y aparece en su lista de jugadores para todos los que están dentro.<br>POST-2. El jugador o el invitado no se encuentra en ninguna otra sala.<br>POST-3. La lista de jugadores, el anfitrión, los colores, el ranking y el chat de la sala existen **únicamente en la memoria del servidor**: ninguna de esas cosas se guarda en la base de datos.<br>POST-4. La sala tiene como máximo cuatro jugadores, contando a los inactivos.<br>POST-5. El jugador o el invitado ve el chat completo que la sala conserva, aunque los mensajes sean anteriores a su llegada. |
| **Extensiones** | CU-16 Invitar jugadores a la sala, CU-18 Salir de la sala, CU-19 Iniciar la partida, CU-21 Expulsar a un jugador de la sala y CU-22 Enviar un mensaje al chat, todos alcanzables desde las opciones de la ventana GUIRoom y ninguno obligatorio |
| **Inclusiones** | Ninguna. Este caso de uso **es incluido por** CU-14, CU-15 y CU-17 |

---

## CU-14 Unirse a una sala

| | |
|---|---|
| **ID** | CU-14 |
| **Nombre** | Unirse a una sala |
| **Descripción** | Permite a un jugador entrar a una sala ya abierta, ingresando su código de cuatro caracteres o eligiéndola de la lista de salas disponibles, que muestra únicamente las salas públicas. Una sala privada no aparece en esa lista: solo se entra a ella con su código o aceptando una invitación. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe estar identificado, con una cuenta o como invitado.<br>PRE-3. El jugador o el invitado no debe encontrarse dentro de otra sala. |
| **Disparador** | El jugador o el invitado selecciona la opción "Salas" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema muestra la ventana GUISearchRooms con el campo de búsqueda "CÓDIGO DE SALA" y las opciones "Buscar" y "Crear sala". (FA05)
2. El sistema muestra debajo el apartado "Salas disponibles" con la marca "Solo públicas", la opción "Actualizar" y la lista de las salas **públicas** abiertas, mostrando de cada una su código, su anfitrión y cuántos jugadores tiene sobre el máximo, como "2/4"; y al pie la nota "Las salas privadas no aparecen aquí: se entra a ellas con su código o aceptando una invitación.", seguida de la opción "Volver al menú". (FA01) (FA04) (FA06) (FA07)
3. El jugador o el invitado ingresa el código de la sala a la que quiere entrar en el campo "CÓDIGO DE SALA".
4. El jugador o el invitado selecciona la opción "Buscar".
5. El sistema busca entre las salas **abiertas** la que tenga ese código, sin distinguir mayúsculas de minúsculas. El campo admite únicamente los caracteres del alfabeto del código, que **excluye la O y el 0, y la I, la L y el 1** por ser los que se confunden al copiarlos de un correo. (FA02)
6. El sistema valida que la sala tenga menos de cuatro jugadores, contando también a los marcados como inactivos. (FA03)
7. Se ejecuta el caso de uso CU-13 Entrar a la sala. (EX01)

**Flujo Alterno**

FA01 - No hay salas disponibles
1. El sistema detecta que no hay ninguna sala pública abierta que mostrar en la lista.
2. El sistema muestra el mensaje "No hay salas disponibles ahora" junto con la indicación "Crea una sala e invita a tus amigos.".
3. El flujo continúa en el paso 3 del flujo normal, porque el jugador o el invitado todavía puede entrar por código a una sala privada.

FA02 - El código no corresponde a ninguna sala abierta
1. El sistema no encuentra ninguna sala abierta con el código ingresado, ya sea porque el código no existe, porque la sala se cerró o porque quedó libre y todavía no lo ha tomado ninguna otra.
2. El sistema muestra el aviso "No encontramos esa sala" con el texto "Revisa el código e inténtalo otra vez." y la opción "Entendido".
3. El jugador o el invitado selecciona la opción "Entendido".
4. El flujo regresa al paso 3 del flujo normal.

FA03 - La sala está llena
1. El sistema detecta que la sala ya tiene cuatro jugadores, **contando también a los marcados como inactivos**, que siguen ocupando su plaza.
2. El sistema muestra el aviso "La sala está llena" con el texto "Ya hay cuatro jugadores dentro. Puedes unirte a otra sala o crear la tuya." y las opciones "Ver salas" y "Cancelar".
3. Si el jugador o el invitado selecciona "Ver salas", el flujo regresa al paso 2 del flujo normal.
4. Si el jugador o el invitado selecciona "Cancelar", termina el caso de uso.

FA04 - El jugador o el invitado se une desde la lista de salas disponibles
1. El jugador o el invitado selecciona la opción "Unirse" de una de las salas de la lista, que se muestra habilitada solo cuando la sala tiene menos de cuatro jugadores contando a los inactivos; las salas con cuatro jugadores se muestran con la marca "Llena".
2. El sistema valida que la sala siga abierta y con lugar disponible, porque la lista pudo quedar desactualizada desde que se mostró. (FA02) (FA03)
3. El flujo continúa en el paso 7 del flujo normal.

FA05 - El jugador o el invitado crea una sala en lugar de unirse a una existente
1. El jugador o el invitado selecciona la opción "Crear sala".
2. Se ejercita el caso de uso CU-15 Crear una sala.
3. Termina el caso de uso.

FA06 - El jugador o el invitado actualiza la lista de salas disponibles
1. El jugador o el invitado selecciona la opción "Actualizar".
2. El sistema vuelve a consultar las salas públicas abiertas y muestra la lista con su ocupación al momento.
3. El flujo regresa al paso 2 del flujo normal.

FA07 - Volver al menú sin entrar a ninguna sala
1. El jugador o el invitado presiona el botón "Volver al menú".
2. El sistema cierra GUISearchRooms y regresa a la ventana GUIMainMenu.
3. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el jugador o el invitado no queda dentro de ninguna sala.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador o el invitado queda dentro de la sala elegida y aparece en su lista de jugadores.<br>POST-2. La sala tiene como máximo cuatro jugadores, contando a los inactivos.<br>POST-3. La lista de salas disponibles nunca ha mostrado salas privadas ni salas cerradas. |
| **Extensiones** | CU-15 Crear una sala, alcanzable desde la opción "Crear sala" de la ventana GUISearchRooms |
| **Inclusiones** | CU-13 Entrar a la sala |

---

## CU-15 Crear una sala

| | |
|---|---|
| **ID** | CU-15 |
| **Nombre** | Crear una sala |
| **Descripción** | Permite a un jugador o a un invitado abrir una sala nueva, eligiendo si es pública o privada, y quedar dentro de ella como su anfitrión. El sistema le asigna un **código de cuatro caracteres** con el que otros pueden entrar, tomado de un alfabeto **sin caracteres confundibles**. La sala nace **abierta** y con una visibilidad que ya no cambia. Un invitado también puede crear salas y ser anfitrión. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe estar identificado, con una cuenta o como invitado.<br>PRE-3. El jugador o el invitado no debe encontrarse dentro de otra sala. |
| **Disparador** | El jugador o el invitado selecciona la opción "Crear sala" desde la ventana GUISearchRooms. |

**Flujo Normal**

1. El sistema muestra la ventana GUICreateRoom con el título "Nueva sala", la indicación "Elige cómo quieres que sea tu sala.", las dos opciones de tipo de sala —"Pública", con la nota "Aparece en el listado de salas.", y "Privada", con la nota "No aparece: se entra con el código."—, la aclaración "En lo demás son iguales: mismo máximo de cuatro jugadores y mismo código de cuatro caracteres." y las opciones "Crear sala" y "Volver". (FA01)
2. El jugador o el invitado selecciona el tipo de sala, "Pública" o "Privada".
3. El jugador o el invitado selecciona la opción "Crear sala".
4. El sistema genera un código de **cuatro caracteres**, tomados de un alfabeto que **excluye cinco caracteres confundibles: la letra O y el dígito 0, y la letra I, la letra L y el dígito 1**. El alfabeto que queda es, por tanto, A B C D E F G H J K M N P Q R S T U V W X Y Z y los dígitos 2 a 9. **La exclusión se aplica al código de sala y solo a él**, porque es el único dato del sistema que un jugador copia a mano de un correo o de un mensaje, donde no puede preguntar si lo que ve es una O o un cero. (FA02)
5. El sistema registra la sala en la base de datos con su código, su tipo, su fecha de creación y **sin fecha de cierre, que es lo que la deja en estado abierta**. (EX01)
6. El sistema crea en la memoria del servidor el estado de la sala: su lista ordenada de jugadores, su anfitrión, sus cuatro colores disponibles, su ranking y su chat, todos vacíos.
7. El sistema registra al jugador o al invitado que la creó como **anfitrión** de la sala. (FA03)
8. Se ejecuta el caso de uso CU-13 Entrar a la sala.

**Flujo Alterno**

FA01 - Cancelar la creación de la sala
1. El jugador o el invitado presiona el botón "Volver".
2. El sistema cierra GUICreateRoom sin crear ninguna sala y regresa a la ventana GUISearchRooms.
3. Termina el caso de uso.

FA02 - El código generado ya lo usa una sala abierta
1. El sistema detecta que el código de cuatro caracteres que ha generado coincide con el de una sala que sigue abierta.
2. El sistema descarta ese código y genera otro, porque el código solo tiene que ser único **entre las salas abiertas**: el de una sala ya cerrada puede volver a usarse, y es lo que permite que el código sea tan corto.
3. El flujo regresa al paso 4 del flujo normal.

FA03 - El jugador o el invitado que crea la sala es un invitado
1. El sistema detecta que quien crea la sala no tiene cuenta.
2. El sistema lo registra igualmente como anfitrión, porque un invitado puede crear salas y mandar en ellas.
3. La sala queda registrada en la base de datos, pero **de su anfitrión no se guarda ninguna fila**: el anfitrión es estado de memoria.
4. El flujo continúa en el paso 8 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no crea la sala.

| | |
|---|---|
| **Postcondiciones** | POST-1. La sala queda registrada en la base de datos con su código y su tipo, **en estado abierta**. Una sala solo tiene dos estados —**abierta** y **cerrada**— y este caso de uso solo produce el primero: cerrarla corresponde a CU-18 cuando se queda vacía, a la retirada del último inactivo o al arranque del servidor.<br>POST-2. El código es único **entre las salas abiertas**; cuando la sala se cierre, quedará libre para que otra lo tome.<br>POST-3. El jugador o el invitado queda dentro de la sala como su anfitrión, con su color asignado.<br>POST-4. Los jugadores, el anfitrión, los colores, el ranking y el chat de la sala existen únicamente en la memoria del servidor.<br>POST-5. La sala queda con **una de sus dos visibilidades**: si es pública aparece en la lista de salas disponibles y si es privada no, y esa es la **única** diferencia entre las dos. **La visibilidad se fija al crearla y no cambia nunca**, así que no existe ninguna operación para convertir una sala pública en privada ni al revés. |
| **Extensiones** | Extiende a CU-14 Unirse a una sala, como comportamiento opcional desde la opción "Crear sala" de la ventana GUISearchRooms |
| **Inclusiones** | CU-13 Entrar a la sala |

---

## CU-16 Invitar jugadores a la sala

| | |
|---|---|
| **ID** | CU-16 |
| **Nombre** | Invitar jugadores a la sala |
| **Descripción** | Permite a un jugador que está dentro de una sala invitar a ella a sus amigos o a cualquier jugador con cuenta que encuentre por su nombre de usuario, avisándole dentro del juego o por correo. La invitación va siempre dirigida a una cuenta y queda pendiente hasta que el destinatario la responda, aunque no esté conectado. **Un invitado no puede invitar a nadie.** |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta.<br>PRE-3. El jugador debe encontrarse dentro de una sala abierta. |
| **Disparador** | El jugador selecciona la opción "Invitar jugadores" desde la ventana GUIRoom. |

**Flujo Normal**

1. El sistema muestra la ventana GUIInvitePlayers, encabezada por el código de la sala a la que se invita, con las pestañas "Mis amigos" y "Buscar por username", la lista de sus amigos con las opciones "Invitar" e "Invitar por correo" en cada uno, y la opción "Volver a la sala". (FA01)
2. El sistema muestra con la marca "Invitado", en lugar de la opción "Invitar", a los jugadores que ya tienen una invitación pendiente a esa sala.
3. El jugador selecciona la opción "Invitar" del amigo al que quiere invitar. (FA02)
4. El sistema valida que no exista ya una invitación pendiente de esa sala para ese jugador. (FA03)
5. El sistema registra la invitación en la base de datos como pendiente, asociada a la sala, dirigida a la cuenta del jugador elegido, con el canal "dentro del juego" y su fecha de creación. (FA05) (EX01)
6. El sistema sustituye la opción "Invitar" de ese jugador por la marca "Invitado".
7. El sistema muestra la invitación en la ventana GUIInvitationsReceived del destinatario, si está conectado; si no lo está, la encontrará ahí la próxima vez que entre. (FA06)
8. El jugador selecciona la opción "Volver a la sala".
9. El sistema cierra GUIInvitePlayers y regresa a la ventana GUIRoom.

**Flujo Alterno**

FA01 - Volver a la sala sin invitar a nadie
1. El jugador selecciona la opción "Volver a la sala" sin haber invitado a ningún jugador.
2. El sistema cierra GUIInvitePlayers y regresa a la ventana GUIRoom sin registrar ninguna invitación.
3. Termina el caso de uso.

FA02 - Invitar a un jugador que no es amigo
1. El jugador selecciona la pestaña "Buscar por username".
2. El jugador ingresa el nombre de usuario del jugador al que quiere invitar y acciona la búsqueda.
3. El sistema busca en la base de datos la cuenta cuyo nombre de usuario coincida con el valor ingresado, sin distinguir mayúsculas de minúsculas. (FA04)
4. El sistema muestra el jugador encontrado con la opción "Invitar".
5. El jugador selecciona la opción "Invitar".
6. El flujo continúa en el paso 4 del flujo normal. **No hace falta ser amigo para invitar**: la amistad solo sirve para tenerlo a mano en la primera pestaña.

FA03 - El jugador ya tiene una invitación pendiente a esa sala
1. El sistema detecta que ya existe una invitación pendiente de esa sala dirigida a ese jugador.
2. El sistema no registra una segunda invitación, porque solo puede haber una pendiente por sala y destinatario.
3. El sistema muestra a ese jugador con la marca "Invitado".
4. El flujo regresa al paso 3 del flujo normal.

FA04 - No existe ninguna cuenta con ese nombre de usuario
1. El sistema no encuentra ninguna cuenta cuyo nombre de usuario coincida con el valor ingresado.
2. El sistema muestra la búsqueda sin resultados.
3. El jugador modifica el valor de la búsqueda.
4. El flujo regresa al paso 3 del flujo alterno FA02.

FA05 - El jugador invita por correo
1. El jugador, que está dentro de la sala, elige enviar la invitación **por correo** en lugar de avisar solo dentro del juego, seleccionando la opción "Invitar por correo" del jugador. **Un invitado no llega nunca a este flujo**, porque no puede invitar.
2. El sistema registra la invitación exactamente igual que en el flujo normal —pendiente, asociada a la sala y dirigida a la cuenta elegida—, pero con el canal "correo".
3. El sistema envía un mensaje **al correo registrado en esa cuenta**, no a una dirección que escriba quien invita: no existe ninguna forma de invitar a alguien que no tenga cuenta en el sistema.
4. El mensaje lleva **el código de cuatro caracteres de la sala** y quién invita. No lleva ningún enlace ni ningún token: el cliente es una aplicación de escritorio sin navegador, así que no habría nada que canjeara un enlace. (EX02)
5. El destinatario, al abrir el correo, tiene **dos caminos y los dos existían ya**: responder la invitación desde la ventana GUIInvitationsReceived, que es CU-17, o teclear el código en la ventana GUISearchRooms, que es CU-14. El correo **no abre un tercer camino**: es un aviso de que la invitación está esperándole.
6. La invitación **sigue pendiente hasta que la responda desde su bandeja**. Si entra tecleando el código, entra en la sala pero su invitación no se consume: seguirá ahí hasta que la responda o hasta que la sala se cierre.
7. El flujo continúa en el paso 6 del flujo normal.

FA06 - La sala se llena mientras hay invitaciones pendientes
1. El sistema detecta que la sala alcanzó los cuatro jugadores y todavía hay invitaciones pendientes a ella.
2. El sistema **conserva** las invitaciones, que no caducan: quien las acepte encontrará la sala llena y lo sabrá en ese momento.
3. El flujo continúa en el paso 8 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no registra la invitación.

EX02 - Fallo del canal de correo
1. El sistema no consigue enviar el mensaje al correo de la cuenta destinataria.
2. La invitación **queda registrada igualmente**, porque el destinatario la verá en su bandeja de invitaciones al entrar: el aviso falla, la invitación no se pierde.
3. El sistema muestra al remitente el mensaje "No pudimos enviar el correo en este momento. Inténtalo más tarde" y permanece en GUIInvitePlayers.
4. El sistema **no reintenta** el envío por su cuenta, igual que en CU-03: el remitente puede volver a invitar por correo, y el destinatario ya tiene la invitación en su bandeja.
5. El flujo continúa en el paso 6 del flujo normal, porque la invitación existe aunque el aviso no llegara.

| | |
|---|---|
| **Postcondiciones** | POST-1. La invitación queda registrada como pendiente, asociada a la sala, a la cuenta destinataria y al canal por el que se avisó.<br>POST-2. La invitación se conserva aunque el destinatario no esté conectado, y **no caduca**: vive mientras viva la sala.<br>POST-3. Solo existe una invitación pendiente por sala y destinatario.<br>POST-4. Si el canal fue el correo y el envío funcionó, el destinatario ha recibido, en el correo de su cuenta, el código de la sala y quién le invita; si el envío falló, la invitación existe igualmente y el remitente ha sido informado.<br>POST-5. La invitación se borrará al responderse o al cerrarse la sala, y eso es lo que la hace de un solo uso.<br>POST-6. **La invitación por correo no concede ningún permiso que el código no conceda ya.** Entrar tecleando el código es una vía de acceso por sí misma, también en las salas privadas, así que quien tenga ese código puede entrar aunque no sea el destinatario de la invitación. Es el comportamiento buscado, no un descuido: ver «El código de sala es la credencial de la sala» en «Observaciones». |
| **Extensiones** | Extiende a CU-13 Entrar a la sala, como comportamiento opcional desde la opción "Invitar jugadores" de la ventana GUIRoom |
| **Inclusiones** | Ninguna |

---

## CU-17 Responder una invitación a una sala

| | |
|---|---|
| **ID** | CU-17 |
| **Nombre** | Responder una invitación a una sala |
| **Descripción** | Permite a un jugador consultar las invitaciones a sala que ha recibido, incluidas las que le llegaron mientras no estaba conectado, y aceptarlas para entrar a la sala o rechazarlas. Responder una invitación la elimina, tanto si se acepta como si se rechaza. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta.<br>PRE-3. El jugador no debe encontrarse dentro de otra sala. |
| **Disparador** | El jugador selecciona la opción "Invitaciones" desde la ventana GUIMainMenu. |

**Flujo Normal**

1. El sistema muestra la ventana GUIInvitationsReceived con las invitaciones pendientes dirigidas a su cuenta, indicando de cada una quién lo invitó —"luis_m te invitó a su sala"— y el código de la sala, con las opciones "Entrar" y de rechazo, y la opción "Volver al menú". (FA01) (FA04)
2. El jugador selecciona la opción "Entrar" de la invitación que quiere aceptar. (FA02)
3. El sistema valida que la sala de esa invitación siga abierta. (FA03)
4. El sistema valida que la sala tenga menos de cuatro jugadores, contando también a los marcados como inactivos. (FA05)
5. El sistema elimina la invitación de la base de datos, porque queda respondida.
6. Se ejecuta el caso de uso CU-13 Entrar a la sala. (EX01)

**Flujo Alterno**

FA01 - El jugador no tiene invitaciones
1. El sistema detecta que no hay ninguna invitación pendiente dirigida a la cuenta.
2. El sistema muestra el mensaje "No tienes invitaciones" junto con la indicación "Aquí verás las invitaciones a sala que te envíen tus amigos.".
3. Termina el caso de uso.

FA02 - Rechazar la invitación
1. El jugador acciona la opción de rechazo sobre la invitación.
2. El sistema elimina la invitación de la base de datos, sin conservar ningún registro de ella.
3. El sistema retira la invitación de la lista y el jugador no entra a ninguna sala.
4. El sistema no avisa al remitente del rechazo, porque no existe historial de invitaciones.
5. El flujo regresa al paso 1 del flujo normal.

FA03 - La sala ya no existe
1. El sistema detecta que la sala de la invitación se cerró, por haberse quedado vacía o porque el servidor se reinició.
2. El sistema muestra un mensaje indicando que esa sala ya no está disponible.
3. La invitación ya no figura en la lista, porque las invitaciones pendientes se eliminan al cerrarse su sala.
4. El flujo regresa al paso 1 del flujo normal.

FA04 - Salir sin responder ninguna invitación
1. El jugador presiona el botón "Volver al menú".
2. El sistema cierra GUIInvitationsReceived y regresa a la ventana GUIMainMenu, **conservando** las invitaciones pendientes.
3. Termina el caso de uso.

FA05 - La sala está llena
1. El sistema detecta que la sala ya tiene cuatro jugadores, contando también a los marcados como inactivos.
2. El sistema muestra el aviso "La sala está llena" con el texto "Ya hay cuatro jugadores dentro. Puedes unirte a otra sala o crear la tuya." y las opciones "Ver salas" y "Cancelar".
3. El sistema **conserva** la invitación, que sigue pendiente, porque no llegó a responderse.
4. Si el jugador selecciona "Ver salas", el sistema muestra la ventana GUISearchRooms y termina el caso de uso.
5. Si el jugador selecciona "Cancelar", el flujo regresa al paso 1 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el jugador no queda dentro de ninguna sala.

| | |
|---|---|
| **Postcondiciones** | POST-1. Si el jugador aceptó la invitación, queda dentro de la sala y la invitación deja de existir.<br>POST-2. Si el jugador rechazó la invitación, esta queda eliminada y no queda ningún registro de ella.<br>POST-3. Las invitaciones que el jugador no respondió siguen pendientes.<br>POST-4. Aceptar una invitación es la única vía de entrada a una sala privada además del código; quien recibió el aviso por correo puede entrar tecleando ese código, y entonces su invitación **sigue pendiente** hasta que la responda o hasta que la sala se cierre. |
| **Extensiones** | Ninguna |
| **Inclusiones** | CU-13 Entrar a la sala, que se ejecuta en el flujo normal al aceptar la invitación |

---

## CU-18 Salir de la sala

| | |
|---|---|
| **ID** | CU-18 |
| **Nombre** | Salir de la sala |
| **Descripción** | Permite a un jugador abandonar la sala en la que se encuentra. Si era el anfitrión, la condición de anfitrión pasa al siguiente jugador en el orden en que están guardados; si era el único que quedaba, la sala se cierra junto con sus invitaciones pendientes, su ranking y su chat. No se puede salir de la sala mientras se está jugando una partida. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe encontrarse dentro de una sala.<br>PRE-3. La sala no debe tener una partida en curso: mientras se juega, la opción "Salir" no se ofrece. |
| **Disparador** | El jugador o el invitado selecciona la opción "Salir" desde la ventana GUIRoom. |

**Flujo Normal**

1. El sistema comprueba cuántos jugadores hay dentro de la sala. (FA01)
2. El sistema retira al jugador o al invitado de la lista de jugadores de la sala y libera el color que tenía asignado, que vuelve al conjunto de colores disponibles. (EX01)
3. El sistema comprueba si el jugador o el invitado que sale era el anfitrión de la sala. (FA02)
4. El sistema actualiza la lista de jugadores en la ventana GUIRoom de los jugadores que permanecen dentro, que ven desaparecer al que salió.
5. El sistema muestra al jugador o al invitado que salió la ventana GUIMainMenu.
6. El sistema conserva las invitaciones pendientes de la sala, que sigue abierta. (FA03)

**Flujo Alterno**

FA01 - El jugador o el invitado es el único que queda en la sala
1. El sistema detecta que el jugador o el invitado es la única persona dentro de la sala.
2. El sistema muestra el aviso "Salir de la sala" con el texto "Eres la única persona dentro. Si sales, la sala se cerrará." y las opciones "Salir" y "Quedarme".
3. Si el jugador o el invitado selecciona "Quedarme", el sistema cierra el aviso, el jugador o el invitado permanece en GUIRoom y termina el caso de uso.
4. Si el jugador o el invitado selecciona "Salir", el sistema escribe la fecha de cierre de la sala en la base de datos, con lo que su código queda libre; elimina de la memoria del servidor su lista de jugadores, sus colores, su ranking y su chat; y elimina las invitaciones pendientes a esa sala.
5. El sistema muestra al jugador o al invitado la ventana GUIMainMenu.
6. Termina el caso de uso.

FA02 - El jugador o el invitado que sale era el anfitrión
1. El sistema detecta que el jugador o el invitado que sale era el anfitrión de la sala y que quedan jugadores dentro.
2. El sistema asigna la condición de anfitrión **al siguiente jugador en el orden en que están guardados** los que permanecen en la sala, sin más criterio y sin preguntar a nadie.
3. El sistema muestra la marca "Anfitrión" sobre el nuevo anfitrión en la ventana GUIRoom de todos los jugadores que permanecen dentro, y le habilita las opciones "Iniciar partida" y la de expulsar.
4. El nuevo anfitrión puede ser un invitado, porque la condición de anfitrión no exige tener cuenta.
5. El flujo regresa al paso 4 del flujo normal.

FA03 - La sala se queda con un solo jugador
1. El sistema detecta que, tras la salida, en la sala queda un único jugador.
2. La sala **sigue abierta**: solo se cierra cuando se queda vacía.
3. El sistema muestra a ese jugador la opción "Iniciar partida" deshabilitada, porque hacen falta al menos dos jugadores activos.
4. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo".
3. Si la pérdida de conexión persiste, el jugador o el invitado **no sale de la sala**: queda marcado como inactivo, que es un camino distinto y no equivale a salir.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador o el invitado deja de formar parte de la sala, no aparece en su lista de jugadores y su color vuelve a estar disponible.<br>POST-2. Si quedaron jugadores dentro, la sala sigue abierta y tiene anfitrión.<br>POST-3. Si el jugador o el invitado era el último, la sala queda cerrada con su fecha de cierre, su código queda libre, sus invitaciones pendientes quedan eliminadas y su ranking y su chat desaparecen. **La fila de la sala se conserva**, porque sus partidas terminadas la referencian.<br>POST-4. El jugador o el invitado que salió puede volver a entrar: salir no veta el acceso. |
| **Extensiones** | Extiende a CU-13 Entrar a la sala, como comportamiento opcional desde la opción "Salir" de la ventana GUIRoom |
| **Inclusiones** | Ninguna |

---

## CU-19 Iniciar la partida

| | |
|---|---|
| **ID** | CU-19 |
| **Nombre** | Iniciar la partida |
| **Descripción** | Permite al anfitrión iniciar una partida con los jugadores **activos** que se encuentran en la sala, siempre que sean entre dos y cuatro. El sistema registra la partida, asigna los puestos de mesa según el orden en que están acomodados, conserva el color de cada uno, copia el pase de asiento de los invitados al estado de la partida y guarda el primer estado. Una vez iniciada, ningún jugador puede incorporarse a ella. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe encontrarse dentro de una sala y ser su anfitrión.<br>PRE-3. La sala no debe tener una partida en curso.<br><br>*El número de jugadores activos y la ausencia de sanciones vigentes no son precondiciones: el sistema los comprueba dentro del flujo, en los pasos 3 y 4, y los resuelve con FA01 y FA03.* |
| **Disparador** | El anfitrión selecciona la opción "Iniciar partida" desde la ventana GUIRoom. |

**Flujo Normal**

1. El sistema muestra al anfitrión la opción "Iniciar partida" habilitada, porque la sala tiene entre dos y cuatro jugadores activos. (FA01)
2. El anfitrión selecciona la opción "Iniciar partida".
3. El sistema valida que la sala siga teniendo entre dos y cuatro jugadores activos en ese instante. (FA01) (FA02)
4. El sistema comprueba que ninguno de esos jugadores tenga una sanción temporal vigente. (FA03)
5. El sistema registra la partida en la base de datos como "en curso", asociada a la sala, con la cantidad de jugadores que se sientan y su fecha de inicio, sin fecha de fin ni motivo. (FA05) (EX01)
6. El sistema asigna a cada jugador su **puesto de mesa**, del 1 al 4, según el orden en que están acomodados en la sala, y conserva el color que cada uno tenía en ella.
7. El sistema registra una **participación** por cada jugador **con cuenta**, con su puesto de mesa y sin resultado. (FA04)
8. El sistema copia el identificador temporal de cada invitado —que es su **pase de asiento**, el mismo que el servidor le entregó al obtener su identidad— dentro del estado de la partida, junto con su alias y su puesto.
9. El sistema consulta los catálogos de parámetros para determinar cuántos turnos tiene cada ronda y cuántas construcciones recibe cada jugador en cada turno, **según la cantidad de jugadores**.
10. El sistema guarda el primer estado de la partida, con la ronda 1, el turno 1, el puesto al que le toca y el tablero inicial.
11. El sistema deja de admitir nuevos jugadores en esa partida y retira la opción "Salir" de la sala mientras dure.
12. El sistema muestra a todos los jugadores que entran a la partida la ventana GUILoadingGame con el mensaje "Preparando la partida", la indicación del número de jugadores y de rondas —"3 jugadores · 3 rondas"— y el apartado "Puestos de mesa" con el orden asignado.
13. Se ejecuta el caso de uso CU-24 Preparar la partida.

**Flujo Alterno**

FA01 - La sala no tiene los jugadores activos suficientes
1. El sistema detecta que en la sala hay un solo jugador activo, porque no ha entrado nadie más o porque los demás están marcados como inactivos.
2. El sistema muestra la opción "Iniciar partida" **deshabilitada**.
3. El anfitrión no puede iniciar la partida hasta que haya al menos dos jugadores activos.
4. Termina el caso de uso.

FA02 - Hay jugadores inactivos en la sala
1. El sistema detecta que uno o más jugadores de la sala están marcados como inactivos.
2. El sistema **no los cuenta** para el mínimo ni para el máximo y **no los incluye en la partida**.
3. Esos jugadores permanecen en la sala, ocupando su plaza, mientras los demás juegan: una sala de cuatro con un inactivo juega una partida de tres.
4. El flujo continúa en el paso 4 del flujo normal.

FA03 - Un jugador de la sala tiene una sanción temporal vigente
1. El sistema detecta que alguno de los jugadores activos tiene una prohibición temporal que todavía no ha vencido.
2. El sistema **no inicia** la partida.
3. El sistema muestra un mensaje indicando que ese jugador no puede jugar mientras dure la prohibición, y hasta cuándo dura.
4. El flujo regresa al paso 1 del flujo normal.

FA04 - En la partida solo hay invitados
1. El sistema detecta que ninguno de los jugadores que se sientan tiene cuenta.
2. El sistema **no registra ninguna participación**: solo la partida y su estado.
3. Esa partida existe mientras está en curso, para poder reanudarse, y **se eliminará al terminar**, porque no podría consultarla nadie.
4. El flujo continúa en el paso 8 del flujo normal.

FA05 - Un jugador pierde la conexión entre la comprobación y el arranque
1. El sistema detecta que un jugador que iba a sentarse ha perdido la conexión justo antes de registrarse la partida.
2. El sistema lo marca como inactivo y vuelve a contar los jugadores activos.
3. Si siguen siendo al menos dos, el flujo regresa al paso 5 del flujo normal con un jugador menos; si queda uno solo, el flujo regresa al paso 1 y la opción vuelve a deshabilitarse.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y la partida no se inicia.

| | |
|---|---|
| **Postcondiciones** | POST-1. La partida queda registrada como "en curso", con su sala, su cantidad de jugadores y su fecha de inicio.<br>POST-2. Cada jugador con cuenta tiene su participación registrada, con su puesto de mesa y sin resultado.<br>POST-3. El identificador temporal de cada invitado, que es su pase de asiento, queda guardado dentro del estado de la partida junto con su alias y su puesto, de modo que pueda recuperar su puesto si el servidor se cae.<br>POST-4. El primer estado de la partida está guardado, de modo que una caída durante el primer turno no deja la partida sin nada que reanudar.<br>POST-5. Ningún jugador puede incorporarse a esa partida.<br>POST-6. Los jugadores inactivos siguen en la sala, ocupando su plaza, y no forman parte de la partida.<br>POST-7. La sala sigue abierta y su chat sigue vivo: la partida lo muestra. |
| **Extensiones** | Extiende a CU-13 Entrar a la sala, como comportamiento opcional desde la opción "Iniciar partida" de la ventana GUIRoom |
| **Inclusiones** | CU-24 Preparar la partida, que se ejecuta siempre antes del primer turno |

---

## CU-20 Eliminar un amigo

| | |
|---|---|
| **ID** | CU-20 |
| **Nombre** | Eliminar un amigo |
| **Descripción** | Permite a un jugador deshacer una amistad ya aceptada. La amistad desaparece de las listas de los dos jugadores, no queda ningún registro de ella y el jugador eliminado no recibe ningún aviso. Como no existe el bloqueo, nada impide volver a enviarle una solicitud más adelante. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta.<br>PRE-3. El jugador debe encontrarse en la pestaña "Amigos" de la ventana GUIFriends. |
| **Disparador** | El jugador acciona la opción de eliminar, rotulada "Eliminar de mis amigos", sobre uno de los jugadores de la pestaña "Amigos" de la ventana GUIFriends. |

**Flujo Normal**

1. El sistema muestra la ventana GUIFriends en la pestaña "Amigos" con la lista de sus amigos, indicando de cada uno su nombre de usuario y su avatar, y ofreciendo sobre cada uno la opción "Eliminar". **Esta pestaña no ofrece ninguna opción de invitar a una sala**: invitar exige estar dentro de una sala y ocurre en CU-16. (FA01) (FA02)
2. El jugador acciona la opción de eliminar sobre el amigo que quiere quitar de su lista.
3. El sistema muestra el aviso de confirmación "Eliminar amigo", con el texto "Dejará de estar en tu lista de amigos y tú en la suya. No se le avisará." y las opciones "Eliminar" y "Cancelar". (FA04)
4. El jugador selecciona la opción "Eliminar".
5. El sistema elimina de la base de datos la amistad entre las dos cuentas, sin conservar ningún registro de ella. (EX01)
6. El sistema retira a ese jugador de la lista de amigos del jugador.
7. El sistema retira también al jugador de la lista de amigos del otro jugador, porque **la amistad era una sola para la pareja**. (FA03)
8. El sistema **no envía ningún aviso** al jugador eliminado.

**Flujo Alterno**

FA01 - El jugador no tiene amigos
1. El sistema detecta que la cuenta no tiene ninguna amistad aceptada.
2. El sistema muestra el mensaje "Todavía no tienes amigos" junto con la indicación "Busca a alguien por su username para enviarle una solicitud.", y no ofrece ninguna opción de eliminar ni ningún aviso de confirmación.
3. Termina el caso de uso.

FA02 - Salir sin eliminar a nadie
1. El jugador presiona el botón "Volver al menú".
2. El sistema cierra GUIFriends y regresa a la ventana GUIMainMenu, conservando la lista de amigos tal como estaba.
3. Termina el caso de uso.

FA03 - El jugador eliminado está conectado
1. El sistema detecta que el otro jugador tiene sesión iniciada en ese momento.
2. El sistema deja de mostrarle al jugador en su lista de amigos, **sin acompañarlo de ningún aviso**.
3. Si los dos estaban dentro de la misma sala, **ninguno sale de ella**: la amistad y la sala son cosas distintas.
4. El flujo continúa en el paso 8 del flujo normal.

FA04 - Cancelar la eliminación
1. El jugador selecciona la opción "Cancelar" en el aviso de confirmación.
2. El sistema cierra el aviso **sin eliminar la amistad** y permanece en la pestaña "Amigos" de GUIFriends.
3. El otro jugador sigue en la lista de amigos del jugador y el jugador en la suya.
4. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no elimina la amistad.

| | |
|---|---|
| **Postcondiciones** | POST-1. La amistad deja de existir y no queda ningún registro de ella.<br>POST-2. Ninguno de los dos jugadores aparece en la lista de amigos del otro.<br>POST-3. El jugador eliminado no ha recibido ningún aviso.<br>POST-4. Cualquiera de los dos puede volver a enviar una solicitud de amistad al otro, porque no existe el bloqueo.<br>POST-5. Las invitaciones a sala ya enviadas entre ambos, si las había, **no se ven afectadas**: dependen de la sala, no de la amistad. |
| **Extensiones** | Extiende a CU-08 Enviar una solicitud de amistad, como comportamiento opcional desde la lista de la pestaña "Amigos" |
| **Inclusiones** | Ninguna |

---

## CU-21 Expulsar a un jugador de la sala

| | |
|---|---|
| **ID** | CU-21 |
| **Nombre** | Expulsar a un jugador de la sala |
| **Descripción** | Permite al anfitrión sacar de la sala a otro de los jugadores que están dentro. El expulsado recibe un aviso y **puede volver a entrar** con el código de la sala, porque la expulsión no le veta el acceso; su efecto real es dejarlo fuera de la partida que se inicie después, ya que una vez iniciada nadie puede incorporarse. No puede expulsarse a nadie mientras haya una partida en curso. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe encontrarse dentro de una sala y ser su anfitrión.<br><br>*Que haya otro jugador dentro y que no haya partida en curso no son precondiciones: el sistema los comprueba dentro del flujo, en los pasos 1 y 4, y los resuelve con FA01 y FA02.* |
| **Disparador** | El anfitrión acciona la opción de expulsar, rotulada "Expulsar de la sala", sobre uno de los jugadores del apartado "Jugadores" de la ventana GUIRoom. |

**Flujo Normal**

1. El sistema muestra la ventana GUIRoom con el apartado "Jugadores" y, **únicamente al anfitrión**, la opción de expulsar junto a cada uno de los demás jugadores. (FA01)
2. El anfitrión acciona la opción de expulsar sobre el jugador o el invitado que quiere sacar de la sala.
3. El sistema muestra el aviso de confirmación "Expulsar a", seguido del nombre del jugador o del invitado, con el texto "Dejará de estar en la sala y se le avisará. Podrá volver a entrar con el código." y las opciones "Expulsar" y "Cancelar". (FA04)
4. El anfitrión selecciona la opción "Expulsar". (FA05)
5. El sistema comprueba que la sala no tenga una partida en curso. (FA02)
6. El sistema retira al jugador o al invitado de la lista de jugadores de la sala y libera el color que tenía asignado. (EX01)
7. El sistema muestra al jugador o al invitado expulsado el aviso "Te han sacado de la sala" con el texto "El anfitrión te ha expulsado de la sala", seguido del código, y la opción "Entendido", y lo devuelve a la ventana GUIMainMenu. (FA03)
8. El sistema actualiza la lista de jugadores en la ventana GUIRoom de los jugadores que permanecen dentro.

**Flujo Alterno**

FA01 - El anfitrión es el único jugador de la sala
1. El sistema detecta que no hay ningún otro jugador dentro.
2. El sistema no muestra ninguna opción de expulsar, porque **el anfitrión no puede expulsarse a sí mismo**.
3. Termina el caso de uso.

FA02 - La sala tiene una partida en curso
1. El sistema detecta que los jugadores de la sala están jugando una partida.
2. El sistema **no expulsa a nadie**.
3. El sistema muestra un mensaje indicando que no puede expulsarse a un jugador mientras haya una partida en curso.
4. El flujo regresa al paso 1 del flujo normal.

FA03 - El jugador o el invitado expulsado está marcado como inactivo
1. El sistema detecta que el jugador o el invitado al que se expulsa había perdido la conexión.
2. El sistema lo retira igualmente de la sala y libera su color, liberando también su plaza.
3. El aviso **no le llega en ese momento**; al recuperar la conexión encontrará que ya no está en la sala.
4. El flujo continúa en el paso 8 del flujo normal.

FA04 - Cancelar la expulsión
1. El anfitrión selecciona la opción "Cancelar" en el aviso de confirmación.
2. El sistema cierra el aviso sin expulsar a nadie y permanece en GUIRoom.
3. Termina el caso de uso.

FA05 - El anfitrión deja de serlo antes de confirmar
1. El sistema detecta que quien accionó la opción de expulsar ya no es el anfitrión, porque perdió la conexión y la condición pasó a otro jugador.
2. El sistema no ejecuta la expulsión, porque **solo el anfitrión expulsa**.
3. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no expulsa al jugador o al invitado.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador o el invitado expulsado deja de formar parte de la sala, su color vuelve a estar disponible y su plaza queda libre.<br>POST-2. El jugador o el invitado expulsado ha sido avisado, salvo que estuviera desconectado.<br>POST-3. La sala sigue abierta y conserva a su anfitrión: **una expulsión nunca deja la sala vacía**.<br>POST-4. El jugador o el invitado expulsado **puede volver a entrar** a la sala, con su código o desde el listado si es pública: no hay veto.<br>POST-5. La expulsión no genera ningún reporte ni ninguna sanción: son mecanismos distintos. |
| **Extensiones** | Extiende a CU-13 Entrar a la sala, como comportamiento opcional desde la lista de jugadores de la ventana GUIRoom |
| **Inclusiones** | Ninguna |

---

## CU-22 Enviar un mensaje al chat

| | |
|---|---|
| **ID** | CU-22 |
| **Nombre** | Enviar un mensaje al chat |
| **Descripción** | Permite a cualquier jugador que está dentro de una sala escribir un mensaje que ven todos los demás, tanto en la sala como durante las partidas que se juegan en ella. **El chat es uno solo, el de la sala**, y no se guarda en la base de datos: vive en la memoria del servidor mientras viva la sala. Escribir durante el propio turno consume el tiempo de ese turno, porque el reloj no se detiene. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe encontrarse dentro de una sala, o jugando una partida de esa sala. |
| **Disparador** | El jugador o el invitado escribe en el recuadro del apartado de chat, rotulado "Pulsa Enter para escribir", de la ventana GUIRoom o del mismo apartado mostrado en la ventana de la partida. |

**Flujo Normal**

1. El sistema muestra el apartado de chat con los mensajes que la sala conserva, cada uno con el nombre de su autor, **incluidos los escritos antes de que el jugador o el invitado llegara**. (FA01) (FA05)
2. El jugador o el invitado escribe un mensaje en el recuadro de escritura. (FA02)
3. El jugador o el invitado envía el mensaje. (FA03)
4. El sistema valida que el mensaje no supere la longitud máxima de doscientos caracteres. (FA02)
5. El sistema añade el mensaje al chat de la sala, con su autor y el instante en que se envió. (FA06) (EX01)
6. El sistema muestra el mensaje a todos los que están en la sala **y** a todos los que están jugando una partida de esa sala, sin distinguir entre unos y otros. (FA04)
7. Si el chat ya conservaba los doscientos mensajes que guarda, el sistema **descarta el más antiguo**, porque la ventana es deslizante.

**Flujo Alterno**

FA01 - El chat todavía no tiene mensajes
1. El sistema detecta que en la sala no se ha escrito ningún mensaje.
2. El sistema muestra el apartado de chat con la indicación "Todavía nadie ha escrito.".
3. El flujo continúa en el paso 2 del flujo normal.

FA02 - El mensaje supera la longitud máxima
1. El jugador o el invitado escribe más caracteres de los que admite un mensaje.
2. El sistema no admite los caracteres que exceden el máximo.
3. El flujo regresa al paso 2 del flujo normal.

FA03 - El jugador o el invitado envía un mensaje vacío
1. El jugador o el invitado acciona el envío sin haber escrito nada.
2. El sistema no añade ningún mensaje al chat y no avisa a nadie.
3. El flujo regresa al paso 2 del flujo normal.

FA04 - El jugador o el invitado escribe durante su propio turno
1. El jugador o el invitado está jugando su turno cuando escribe y envía el mensaje.
2. El reloj del turno **no se detiene**: el tiempo que emplea en escribir se descuenta de sus noventa segundos.
3. El sistema no descuenta ningún punto de acción, porque escribir no es una acción del juego.
4. El flujo continúa en el paso 5 del flujo normal.

FA05 - El jugador o el invitado reporta un mensaje ofensivo
1. El jugador o el invitado considera ofensivo un mensaje escrito por otro jugador **con cuenta**.
2. Se ejercita el caso de uso CU-23 Reportar a un jugador.
3. El flujo regresa al paso 1 del flujo normal.

FA06 - El jugador o el invitado que escribe es un invitado
1. El sistema detecta que el autor del mensaje no tiene cuenta.
2. El sistema publica el mensaje igualmente, porque **los invitados participan en el chat como cualquier otro jugador**.
3. El sistema no ofrece sobre ese mensaje la opción de reportar, porque a un invitado no se le puede reportar.
4. El flujo continúa en el paso 7 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el mensaje no se envía ni lo ve nadie.

| | |
|---|---|
| **Postcondiciones** | POST-1. El mensaje forma parte del chat de la sala y lo han visto todos los que estaban en ella o en su partida.<br>POST-2. El mensaje **no queda registrado en la base de datos** y desaparecerá cuando la sala se cierre o el servidor caiga.<br>POST-3. El chat conserva como mucho doscientos mensajes por sala.<br>POST-4. El chat se conserva de la sala a las partidas que se jueguen en ella: no hay un chat por partida. |
| **Extensiones** | CU-23 Reportar a un jugador. Y extiende a CU-13 Entrar a la sala, desde el apartado de chat de la ventana GUIRoom, y a CU-25 Jugar un turno, desde el mismo apartado mostrado en la ventana de la partida; en los dos casos como comportamiento opcional |
| **Inclusiones** | Ninguna |

---

## CU-23 Reportar a un jugador

| | |
|---|---|
| **ID** | CU-23 |
| **Nombre** | Reportar a un jugador |
| **Descripción** | Permite a un jugador dejar constancia de que otro jugador con cuenta ha escrito un mensaje ofensivo. El reporte guarda **una copia del mensaje**, porque el chat no se conserva. Lo que se acumula son **ocasiones**, no reportes sueltos, y al llegar a la quinta el sistema aplica una prohibición temporal. No se puede reportar a un invitado, ni un invitado puede reportar. No hay revisión humana: las sanciones son automáticas por umbral. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador debe tener una sesión iniciada con su cuenta.<br>PRE-3. El jugador debe encontrarse dentro de una sala, o jugando una partida de esa sala. |
| **Disparador** | El jugador acciona la opción de reportar, rotulada "Reportar", sobre un mensaje del apartado de chat. |

**Flujo Normal**

1. El sistema muestra la opción de reportar en cada mensaje del chat escrito por **otro jugador con cuenta**. (FA01) (FA05)
2. El jugador acciona la opción de reportar sobre el mensaje que considera ofensivo.
3. El sistema registra el reporte en la base de datos con la cuenta que reporta, la cuenta reportada, la sala, la partida en curso si la hay, la fecha, y **una copia del texto del mensaje**. (EX01)
4. El sistema cuenta cuántas **ocasiones distintas** con reporte acumula la cuenta reportada, entendiendo por ocasión cada partida distinta en la que se le reportó, y todos los reportes de una misma sala fuera de partida como una sola. (FA02) (FA03) (FA04)
5. El sistema confirma al jugador que el reporte ha quedado registrado.
6. El sistema **no avisa** al jugador reportado de que lo han reportado, ni le dice quién.

**Flujo Alterno**

FA01 - El mensaje lo escribió un invitado
1. El sistema detecta que el autor del mensaje no tiene cuenta.
2. El sistema no ofrece la opción de reportar sobre ese mensaje, porque **a un invitado no se le puede reportar**: de él no queda ninguna fila a la que asociar el reporte.
3. El flujo regresa al paso 1 del flujo normal, donde el jugador puede reportar otro mensaje; si no hay ninguno de una cuenta, termina el caso de uso.

FA02 - El jugador ya había reportado a ese jugador en la misma ocasión
1. El sistema detecta que ya existe un reporte de esa misma cuenta contra ese mismo jugador **en la misma partida**, o en la misma sala fuera de toda partida.
2. El sistema registra el reporte igualmente, pero **no incrementa el número de ocasiones**, porque lo que se acumula son ocasiones y no reportes sueltos.
3. El flujo continúa en el paso 5 del flujo normal.

FA03 - El mensaje se escribió en la sala fuera de toda partida
1. El sistema detecta que en la sala no hay ninguna partida en curso.
2. El sistema registra el reporte **sin partida asociada**.
3. El sistema cuenta **una sola ocasión** por todos los reportes hechos en esa sala fuera de partida, vengan de uno o de varios jugadores.
4. El flujo continúa en el paso 5 del flujo normal.

FA04 - La cuenta reportada alcanza la quinta ocasión
1. El sistema detecta que la cuenta reportada acumula **cinco ocasiones** con reporte.
2. El sistema le aplica una prohibición con la duración que corresponda a las que ya haya tenido: **cinco horas** la primera, **un día** la segunda y **tres días** la tercera; **al cuarto cruce del umbral, el baneo es permanente**.
3. El sistema registra la sanción con su nivel, su instante del umbral y su entrada en vigor, y **pone a cero el contador de ocasiones** de esa cuenta; el contador de prohibiciones **no** se reinicia.
4. Si la cuenta reportada está jugando esa misma partida, la prohibición **no la interrumpe**: entra en vigor cuando la partida termina, y su duración empieza a contar **desde ese momento**, no desde que se cruzó el umbral.
5. El flujo continúa en el paso 5 del flujo normal.

FA05 - No hay ningún mensaje reportable en el chat
1. El sistema detecta que el chat está vacío, o que todos sus mensajes son del propio jugador o de invitados.
2. El sistema no muestra ninguna opción de reportar.
3. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y no registra el reporte.

| | |
|---|---|
| **Postcondiciones** | POST-1. El reporte queda registrado con la copia del mensaje, la sala y, si la había, la partida.<br>POST-2. El número de ocasiones con reporte de la cuenta reportada queda actualizado.<br>POST-3. Si se alcanzó el umbral, la cuenta reportada tiene una sanción registrada, con su nivel y su instante de entrada en vigor.<br>POST-4. El reporte desaparecerá si se elimina cualquiera de las dos cuentas.<br>POST-5. Ni el reportado ni nadie más recibe aviso del reporte: no existe rol de administración que lo revise. |
| **Extensiones** | Extiende a CU-22 Enviar un mensaje al chat, como comportamiento opcional desde cualquier mensaje del chat escrito por otra cuenta |
| **Inclusiones** | Ninguna |

---

## CU-24 Preparar la partida

| | |
|---|---|
| **ID** | CU-24 |
| **Nombre** | Preparar la partida |
| **Descripción** | Comportamiento por el que cada jugador coloca sobre una flor una de sus orugas y un jugador designado por la regla coloca la mariposa. Tiene **dos momentos**: antes del primer turno, donde se ejecuta siempre como parte de iniciar la partida y comprende las dos colocaciones, en orden de puesto de mesa; y al empezar cada ronda posterior, donde comprende **solo la de la mariposa**, que pasa a colocar el jugador o el invitado con menor puntuación. **Cada colocación dispone del mismo tiempo que un turno, noventa segundos, y se rige por las mismas reglas**: si se agota, se pasa al siguiente, y si el jugador o el invitado pierde la conexión se le aplica la misma ventana de reconexión. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. La partida debe estar iniciada, con sus puestos de mesa asignados y su primer estado guardado.<br>PRE-3. Para la preparación previa al primer turno, ningún jugador debe haber colocado todavía ninguna pieza. Para la colocación de la mariposa de una ronda posterior, la ronda anterior debe estar cerrada y su puntuación parcial calculada. |
| **Disparador** | El sistema ejecuta este caso de uso **como parte de CU-19 Iniciar la partida**, para la preparación previa al primer turno; y **al cerrarse una ronda que no es la última**, para la nueva colocación de la mariposa (flujo alterno FA03). |

**Flujo Normal**

1. El sistema muestra a todos los jugadores la ventana de la partida con el tablero de ocho por ocho y su distribución fija de jardineras, el apartado "Jugadores" con el color de cada uno y el apartado "Orden" con los puestos de mesa.
2. El sistema indica que le corresponde colocar al jugador o al invitado del **primer puesto de mesa**, mostrándole la indicación "Coloca tu oruga sobre una flor" y, a los demás, a quién le toca, y **arranca su reloj de noventa segundos**, el mismo que rige un turno. (FA06)
3. El jugador o el invitado en turno coloca una de sus cinco orugas sobre una flor de una jardinera. (FA01) (EX01) (EX02)
4. El sistema registra la colocación, la muestra a todos los jugadores con el color de quien colocó, y repite los pasos 2 y 3 con cada jugador, **en orden de puesto de mesa**, hasta que todos hayan colocado una oruga.
5. El sistema indica al jugador o al invitado del **último puesto de mesa** que coloque la mariposa, con la indicación "el último puesto colocará la mariposa", y arranca de nuevo su reloj de noventa segundos. (FA06)
6. Ese jugador coloca la mariposa sobre una flor de una jardinera. (FA02)
7. El sistema da por preparada la partida y comienza el primer turno de la ronda 1, con lo que se ejercita el caso de uso CU-25 Jugar un turno. (FA03)

**Flujo Alterno**

FA01 - El jugador o el invitado elige una flor ya ocupada
1. El jugador o el invitado intenta colocar su oruga sobre una flor en la que ya hay una oruga, propia o de otro jugador.
2. El sistema **no admite** la colocación y lo indica, porque no puede haber dos orugas en la misma flor.
3. El flujo regresa al paso 3 del flujo normal.

FA02 - El jugador o el invitado intenta colocar la mariposa fuera de una flor
1. El jugador o el invitado intenta situar la mariposa directamente sobre el tablero, y no sobre una flor de una jardinera.
2. El sistema **no admite** la colocación y lo indica: la mariposa se sitúa siempre sobre una flor.
3. El flujo regresa al paso 6 del flujo normal.

FA03 - Colocación de la mariposa al empezar una ronda posterior
1. Termina una ronda que no es la tercera y el sistema calcula la puntuación parcial, que muestra en la ventana de cierre de ronda con el aviso "El jardín se queda como está. La ronda 3 empieza cuando se coloque la mariposa.".
2. El sistema designa al **jugador con menor puntuación** hasta ese momento para colocar la mariposa en la ronda siguiente, y lo muestra bajo el rótulo "Coloca la mariposa". (FA04) (FA05)
3. Ese jugador coloca la mariposa sobre una flor de una jardinera, con las mismas condiciones del paso 6.
4. En esta colocación **no se coloca ninguna oruga**: las orugas y las construcciones se quedan donde están, porque el corte de ronda no retira nada del tablero.
5. Termina el caso de uso, y comienza el primer turno de la ronda nueva.

FA04 - Empate en la menor puntuación
1. El sistema detecta que dos o más jugadores comparten la menor puntuación.
2. El sistema elige **al azar** entre los empatados a quien colocará la mariposa.
3. El flujo continúa en el paso 3 del flujo alterno FA03.

FA05 - Un jugador retirado tenía que colocar
1. El sistema detecta que el jugador o el invitado al que le correspondía colocar la mariposa ha sido retirado de la partida por haber agotado su ventana de reconexión, o la ha abandonado.
2. El jugador o el invitado retirado ya no cuenta para «el de menor puntuación», así que el sistema designa al de menor puntuación **entre los que siguen jugando**.
3. El flujo continúa en el paso 3 del flujo alterno FA03.

FA06 - Se agotan los noventa segundos de la colocación
1. El reloj llega a cero sin que el jugador o el invitado haya colocado su pieza.
2. El sistema cierra esa colocación **con lo que el jugador o el invitado hubiera confirmado**, igual que cierra un turno agotado, y pasa al siguiente jugador por orden de puesto de mesa.
3. El jugador o el invitado que no colocó su oruga **empieza la partida sin ninguno sobre el tablero**, y podrá colocar uno durante su turno pagando los 2 puntos de acción que cuesta esa acción.
4. Si a quien se le agotó el tiempo era el jugador o el invitado que debía colocar el **rey**, el sistema se lo indica al siguiente jugador según la misma regla de designación y le abre su reloj.
5. El flujo continúa en el paso 4 del flujo normal, o en el paso 7 si la colocación agotada era la de la mariposa.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y la colocación no se registra.

EX02 - El jugador o el invitado al que le toca colocar pierde la conexión
1. El sistema detecta que el jugador o el invitado que debe colocar su oruga o la mariposa ha perdido la conexión.
2. Se le aplican **las mismas reglas que a un turno**: dispone del tiempo que le quedaba de su colocación para volver, el reloj **sigue corriendo** mientras está desconectado, y se ejercita el caso de uso CU-26 Reconectar a una partida en curso.
3. Si vuelve dentro de la ventana, retoma su colocación en el punto exacto en que la dejó.
4. Si agota la ventana, queda **retirado de la partida**, con el mismo efecto que el retiro durante un turno.

| | |
|---|---|
| **Postcondiciones** | POST-1. Tras la preparación previa al primer turno, cada jugador tiene una oruga sobre una flor del tablero.<br>POST-2. La mariposa está sobre una flor de una jardinera, nunca sobre el tablero.<br>POST-3. La partida está lista para su primer turno.<br>POST-4. Al empezar cada ronda posterior, la mariposa ha vuelto a colocarse, lo ha hecho el jugador o el invitado con menor puntuación y **ninguna oruga ni ninguna construcción se ha retirado del tablero**.<br>POST-5. Cada jugador conserva las cuatro orugas que no ha colocado, para usarlas durante la partida.<br>POST-6. Ninguna colocación ha durado más de noventa segundos, y la partida nunca se ha detenido esperando a un jugador. |
| **Extensiones** | Extiende a CU-25 Jugar un turno: al cerrarse el último turno de una ronda que no es la tercera, se ejercita su flujo alterno FA03 para la nueva colocación de la mariposa |
| **Inclusiones** | Ninguna. Este caso de uso **es incluido por** CU-19 Iniciar la partida, para la preparación previa al primer turno |

---

## CU-25 Jugar un turno

| | |
|---|---|
| **ID** | CU-25 |
| **Nombre** | Jugar un turno |
| **Descripción** | Permite al jugador o al invitado al que le toca emplear sus **cinco puntos de acción** durante los **noventa segundos** de su turno: colocar y mover orugas, subirlas de nivel, colocar construcciones, obtener cartas de acción y utilizar las que ya tenga. Al cerrarse el turno, por decisión propia o por agotarse su tiempo, el sistema guarda el estado de la partida y pasa el turno al siguiente jugador. Los puntos no gastados se pierden. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. La partida debe estar en curso y ya preparada, con su colocación inicial hecha.<br>PRE-3. Debe ser el turno de ese jugador o de ese invitado, según el puesto que conste en el estado de la partida. |
| **Disparador** | El sistema indica al jugador o al invitado que comienza su turno, mostrando en la ventana de la partida la indicación de ronda y turno —"Ronda 2 de 3 · Turno 2 de 3 · juegas tú"— y arrancando su reloj. |

**Flujo Normal**

1. El sistema asigna al jugador o al invitado **5 puntos de acción**, que muestra en el indicador "PA", y arranca su reloj de noventa segundos. (FA01)
2. El sistema le muestra el estado del tablero, sus orugas disponibles en el apartado "Tus orugas", cuántas construcciones puede colocar en ese turno en el apartado "Construcciones" —según la ronda y el turno en que está—, su mano de cartas obtenidas y el apartado "Mazo" con la opción "Robar · 1".
3. El sistema habilita las acciones con su coste: "Oruga · 2" para colocar una oruga, "Mover · 1", "Subir · 1" y "Construir · 1"; a los demás las muestra deshabilitadas.
4. El jugador o el invitado realiza las acciones que quiera mientras le queden puntos de acción y tiempo: las cuatro acciones ordinarias, obtener una carta, utilizar una de las que ya tenga, escribir en el chat o rendirse. (FA02) (FA03) (FA04) (FA05) (FA06) (FA07) (FA08) (FA09) (FA10) (FA11) (FA12) (FA13) (FA16) (EX02)
5. El jugador o el invitado selecciona la opción "Terminar turno". (FA14)
6. El sistema cierra el turno: **escribe el estado de la partida** —la ronda, el turno, el puesto al que le toca y el tablero— y pasa el turno al siguiente jugador por puesto de mesa. (EX01) (FA15)
7. Los puntos de acción que el jugador o el invitado no haya gastado **se pierden**: no se acumulan al turno siguiente ni a la ronda siguiente.

**Flujo Alterno**

FA01 - El jugador o el invitado utiliza la carta 1 o la carta 2
1. El jugador o el invitado utiliza una carta 1 o una carta 2 que ya había obtenido, **en cualquier momento del turno**, incluso después de haber gastado puntos.
2. El sistema sustituye sus 5 puntos de acción por **6** con la carta 1, o por **7** con la carta 2, sumando la diferencia sobre lo que ya le quedaba.
3. El sistema **no permite utilizar las dos en el mismo turno**.
4. El sistema no descuenta ningún punto por utilizar la carta.
5. El flujo continúa en el paso 4 del flujo normal.

FA02 - El jugador o el invitado coloca una nueva oruga
1. El jugador o el invitado acciona "Oruga · 2" y sitúa una de sus orugas sobre una flor libre.
2. El sistema comprueba que no supere las **cinco orugas** que tiene y que la flor no esté ocupada por ninguna oruga, propia o ajena. (FA13)
3. El sistema descuenta **2 puntos de acción** y muestra la oruga en el tablero, con el color del jugador o del invitado.
4. El flujo continúa en el paso 4 del flujo normal.

FA03 - El jugador o el invitado mueve una oruga
1. El jugador o el invitado acciona "Mover · 1" y desplaza una oruga a otra casilla o a otro nivel, **dentro de la misma jardinera**.
2. El sistema comprueba que la flor de destino no esté ocupada y que **no suba más de un nivel** en ese movimiento; bajar cualquier cantidad de niveles sí está permitido. (FA13)
3. El sistema descuenta **1 punto de acción** por el movimiento y **1 punto más por cada nivel que suba**; bajar niveles no cuesta.
4. El flujo continúa en el paso 4 del flujo normal.

FA04 - El jugador o el invitado coloca una construcción
1. El jugador o el invitado acciona "Construir · 1" y coloca una construcción sobre una jardinera existente, **ortogonalmente y no en diagonal**.
2. El sistema comprueba que no cree una jardinera nueva, que no una ortogonalmente una flor de una jardinera con la de otra, que la flor no supere el **nivel 3** y que no rebase las construcciones que le corresponden en ese turno. (FA13)
3. El sistema descuenta **1 punto de acción** y descuenta también una de las construcciones disponibles del turno.
4. El flujo continúa en el paso 4 del flujo normal.

FA05 - El jugador o el invitado obtiene una carta de acción
1. El jugador o el invitado acciona la opción "Robar · 1" del apartado "Mazo".
2. El sistema le entrega una carta **al azar** del mazo, que es el mismo para todos los jugadores, y descuenta **1 punto de acción**.
3. El sistema muestra la carta en la mano del jugador o del invitado, disponible para este turno o para los siguientes.
4. El flujo continúa en el paso 4 del flujo normal.

**Las seis cartas que habilitan una jugada tienen cada una su propio flujo alterno**, de FA06 a
FA11, porque cada una impone condiciones distintas y cuesta cosas distintas. Lo común a todas es
que **utilizar la carta no cuesta nada**, y que **solo se paga la jugada cuando la propia carta
dice que se paga**: la única que lo dice es la 7. Las cartas 3, 4, 5, 6 y 8 son gratuitas de punta
a punta. Y en todas, si la comprobación falla, el sistema no aplica el efecto, **no retira la
carta de la mano** y no descuenta nada.

FA06 - El jugador o el invitado utiliza la carta 3
1. El jugador o el invitado selecciona de su mano la carta 3 y elige una de sus orugas que esté en el **nivel 1** de una flor.
2. El sistema sube esa oruga **directamente al nivel 3 de esa misma flor**. Es la única excepción a la regla de subir un solo nivel por movimiento. (FA13)
3. El sistema **no descuenta ningún punto de acción**: ni por la carta ni por la subida, porque la carta 3 no indica coste.
4. El sistema retira la carta de la mano.
5. El flujo continúa en el paso 4 del flujo normal.

FA07 - El jugador o el invitado utiliza la carta 4
1. El jugador o el invitado selecciona de su mano la carta 4 y mueve una oruga propia **por el nivel del tablero**.
2. El sistema permite que esa oruga **pase por encima de una casilla ocupada**, que es lo único que la carta cambia: el resto del movimiento se rige por las reglas ordinarias y la casilla de destino tiene que estar libre. (FA13)
3. El sistema **no descuenta ningún punto de acción**: ni por la carta ni por el movimiento que habilita, porque la carta 4 no indica coste.
4. El sistema retira la carta de la mano.
5. El flujo continúa en el paso 4 del flujo normal.

FA08 - El jugador o el invitado utiliza la carta 5
1. El jugador o el invitado selecciona de su mano la carta 5 y elige **un nivel de flor** para moverlo a otra flor o a otra jardinera.
2. El sistema comprueba que el nivel elegido **no esté ocupado por ninguna oruga**, que retirarlo **no parta la jardinera de origen ni separe sus flores**, que **no sea el nivel 1 de una flor de una sola altura** y que colocarlo **no una dos jardineras**. (FA13)
3. El sistema mueve el nivel y **no descuenta ningún punto de acción**, porque la carta 5 no indica coste.
4. El sistema retira la carta de la mano.
5. El flujo continúa en el paso 4 del flujo normal.

FA09 - El jugador o el invitado utiliza la carta 6
1. El jugador o el invitado selecciona de su mano la carta 6 y elige una de sus orugas y la casilla a la que quiere llevarla.
2. El sistema cambia la oruga de casilla, comprobando que la flor de destino no esté ocupada. (FA13)
3. El sistema **no descuenta ningún punto de acción**: ni por la carta ni por el cambio de casilla, porque la carta 6 no indica coste.
4. El sistema retira la carta de la mano.
5. El flujo continúa en el paso 4 del flujo normal.

FA10 - El jugador o el invitado utiliza la carta 7
1. El jugador o el invitado selecciona de su mano la carta 7 y elige una de sus orugas y la flor de destino.
2. El sistema le permite **recorrer la jardinera de una flor a otra sin importar las casillas ni el nivel**, y también **subir a la jardinera desde el tablero**, que es lo que ninguna otra carta ni la regla ordinaria permiten. (FA13)
3. El sistema descuenta **1 punto de acción por el movimiento y 1 más por cada nivel que la oruga gane**; bajar niveles no cuesta. **Esta es la única carta cuyo texto indica coste**, y por eso se cobra.
4. El sistema retira la carta de la mano.
5. El flujo continúa en el paso 4 del flujo normal.

FA11 - El jugador o el invitado utiliza la carta 8
1. El jugador o el invitado selecciona de su mano la carta 8 y elige **dos niveles de flor** para eliminarlos, **uno a uno**.
2. El sistema comprueba, **en cada uno de los dos**, que el nivel **no esté ocupado por ninguna oruga**, que retirarlo **no parta la jardinera ni separe sus flores** y que **no sea el nivel 1 de una flor de una sola altura**. (FA13)
3. El sistema elimina los niveles y **no descuenta ningún punto de acción**, porque la carta 8 no indica coste.
4. El sistema retira la carta de la mano.
5. El flujo continúa en el paso 4 del flujo normal.

FA12 - El jugador o el invitado escribe en el chat
1. El jugador o el invitado envía un mensaje al chat de la sala mientras juega su turno: se ejercita el caso de uso CU-22 Enviar un mensaje al chat.
2. El reloj del turno **no se detiene**: el tiempo empleado se descuenta de sus noventa segundos.
3. El sistema no descuenta ningún punto de acción.
4. El flujo continúa en el paso 4 del flujo normal.

FA13 - La acción no es válida o no tiene puntos suficientes
1. El jugador o el invitado intenta una acción que incumple alguna condición del tablero, o que cuesta más puntos de los que le quedan.
2. El sistema **no la realiza, no descuenta nada** y lo indica.
3. El estado del tablero queda exactamente como estaba.
4. El flujo regresa al paso 4 del flujo normal.

FA14 - Se agotan los noventa segundos
1. El reloj del turno llega a cero sin que el jugador o el invitado haya seleccionado "Terminar turno".
2. El sistema cierra el turno **con las acciones que el jugador o el invitado hubiera confirmado** hasta ese momento, que no se deshacen.
3. El flujo continúa en el paso 6 del flujo normal.

FA15 - El turno cerrado era el último de la ronda
1. El sistema detecta que ya se han jugado todos los turnos que la ronda tenía para cada jugador, según el catálogo y el número de jugadores.
2. El sistema calcula la **puntuación parcial** de todos los jugadores —las jardineras en las que cada uno tiene una única oruga propia, y los puntos de la mariposa— y **no retira nada del tablero**.
3. Si la ronda cerrada **no era la tercera**, el sistema designa al jugador o al invitado con menor puntuación para colocar la mariposa y se ejercita el caso de uso CU-24 Preparar la partida en su flujo alterno FA03.
4. Si la ronda cerrada **era la tercera**, el sistema termina la partida, registra su resultado y lo muestra a todos los jugadores.
5. Al cerrar el resultado, el sistema **devuelve a los jugadores a la ventana GUIRoom de la sala en la que se jugó**, que sigue abierta con sus jugadores, su chat y su ranking ya actualizado: la invitación era **a la sala**, no a la partida, y en una sala pueden jugarse varias partidas. Si la sala ya no existe —porque la partida venía de una reanudación y las salas no se reanudan—, el sistema los devuelve a la ventana GUIMainMenu.
6. Termina el caso de uso.

FA16 - El jugador o el invitado abandona la partida
1. El jugador o el invitado abre el menú de la ventana de la partida y selecciona la opción "Rendirse".
2. Se ejercita el caso de uso CU-28 Abandonar la partida.
3. La opción está disponible **también cuando no es su turno**, así que cualquier jugador de la partida puede accionarla en cualquier momento.
4. Termina el caso de uso para ese jugador; la partida continúa con los demás.

*Las construcciones **no** se reparten en el cierre de ronda: se reciben **en cada turno**, según la ronda y el turno en que está el jugador, y el sistema ya se las muestra en el paso 2 del flujo normal.*

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y las acciones del turno no se confirman.

EX02 - El jugador o el invitado en turno pierde la conexión
1. El servidor deja de recibir los latidos del jugador o el invitado durante el número de latidos fijado y da la conexión por perdida.
2. El jugador o el invitado dispone del **tiempo que le quedaba** de su turno para volver, y **el reloj sigue corriendo** mientras está desconectado.
3. Se ejercita el caso de uso CU-26 Reconectar a una partida en curso.

| | |
|---|---|
| **Postcondiciones** | POST-1. El estado de la partida queda escrito con la ronda, el turno, el puesto al que le toca y el tablero.<br>POST-2. Los puntos de acción no gastados se han perdido.<br>POST-3. El turno pasa al siguiente jugador, o la ronda se cierra si era el último turno.<br>POST-4. **Ni el reloj, ni los puntos restantes, ni las construcciones pendientes quedan guardados**: el estado guardado es siempre una frontera de turno.<br>POST-5. Las cartas obtenidas y no utilizadas siguen en la mano del jugador o el invitado y valdrán **1 punto cada una** al final de la partida.<br>POST-6. La secuencia de jugadas no se guarda en ningún sitio.<br>POST-7. Si el turno cerrado terminó la partida, los jugadores han vuelto a la sala en la que se jugó, que sigue abierta. |
| **Extensiones** | CU-22 Enviar un mensaje al chat; CU-24 Preparar la partida, que se ejercita solo cuando el turno cerrado era el último de una ronda que no es la tercera; y CU-28 Abandonar la partida, desde la opción "Rendirse" del menú de la ventana de la partida. **Las acciones con coste en puntos de acción** —colocar una oruga, moverla, subirla de nivel, colocar una construcción, obtener una carta y utilizar una carta— son puntos de extensión opcionales de este caso de uso y se describen como sus flujos alternos; ver «Observaciones» |
| **Inclusiones** | Ninguna. El cierre del turno y la escritura del estado **se ejecutan siempre**, pero no persiguen el objetivo de ningún actor: son los pasos 6 y 7 de su flujo normal |

---

## CU-26 Reconectar a una partida en curso

| | |
|---|---|
| **ID** | CU-26 |
| **Nombre** | Reconectar a una partida en curso |
| **Descripción** | Permite a un jugador que ha perdido la conexión durante una partida volver a ella dentro de su ventana de reconexión y retomar exactamente el estado en que estaba, con sus puntos no gastados y sus acciones ya confirmadas, porque el servidor es la única autoridad del estado. Si la ventana vence sin que vuelva, queda **retirado** de la partida. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El jugador o el invitado debe estar participando en una partida en curso.<br>PRE-2. El jugador o el invitado debe haber perdido la conexión con el servidor.<br>PRE-3. El servidor debe estar en funcionamiento: si el que cayó fue el servidor, no es este el caso de uso, sino CU-27. |
| **Disparador** | El jugador, que perdió la conexión durante una partida en curso, vuelve a conectarse al servidor. Los pasos 1 a 4 describen lo que el sistema ha hecho entretanto, desde que dejó de recibir sus latidos. |

**Flujo Normal**

1. El servidor da la conexión por perdida al no recibir los latidos del cliente durante el número de latidos fijado. **Ese instante** es el que inicia la ventana de reconexión.
2. El sistema abre la ventana de reconexión del jugador o del invitado: **el tiempo que le quedaba de su turno**, si la pérdida ocurrió durante su propio turno. (FA01)
3. El sistema muestra al jugador o al invitado desconectado, en cuanto su cliente pueda mostrárselo, el aviso "Has perdido la conexión" con el tiempo que le queda, la indicación "Si vuelves antes de que se agote, retomas tu turno tal como lo dejaste." y las opciones "Reintentar" y "Salir del juego"; y el reloj **sigue corriendo**. (FA06)
4. El sistema avisa a los demás jugadores marcando a ese jugador con "Sin conexión" en el apartado "Jugadores" de la ventana de la partida.
5. El jugador o el invitado recupera la conexión y se identifica. (FA02) (FA04) (EX02)
6. El sistema comprueba que la ventana de reconexión no haya vencido y que el jugador o el invitado siga participando en la partida. (EX01)
7. El sistema lo devuelve a la partida **en el estado exacto** en que estaba, con sus puntos de acción no gastados, sus acciones ya confirmadas y sus cartas. (FA03)
8. El jugador o el invitado continúa la partida, y los demás ven desaparecer su marca "Sin conexión".

**Flujo Alterno**

FA01 - La pérdida de conexión ocurre en el turno de otro jugador
1. El sistema detecta que no era el turno del jugador o el invitado desconectado.
2. El sistema le abre una ventana de reconexión de **noventa segundos**, que es la que fija la regla para ese caso.
3. El flujo continúa en el paso 3 del flujo normal.

FA02 - El que vuelve es un invitado
1. El jugador o el invitado que recupera la conexión no tiene cuenta.
2. El invitado se identifica con el **identificador temporal** que el servidor le entregó al entrar, que sigue vivo porque **el servidor no se ha caído**.
3. El flujo continúa en el paso 6 del flujo normal.

FA03 - El turno del jugador o el invitado terminó mientras estaba desconectado
1. El reloj del turno llegó a cero durante la desconexión y el turno pasó al siguiente jugador.
2. El jugador o el invitado vuelve **dentro de su ventana**, pero ya no es su turno.
3. El sistema lo devuelve a la partida y el jugador o el invitado espera a que le vuelva a tocar, conservando sus cartas y su posición en el tablero.
4. Termina el caso de uso.

FA04 - El jugador o el invitado no vuelve dentro de la ventana
1. El sistema detecta que la ventana de reconexión ha vencido sin que el jugador o el invitado se reconectara.
2. El sistema lo **retira de la partida**: deja de contar para el resultado y para «el de menor puntuación».
3. El sistema **retira sus orugas del tablero**, con lo que esas flores quedan libres para los demás, y **deja sus construcciones donde están**.
4. El sistema marca su participación como retirada, si tenía cuenta, y avisa a los demás jugadores.
5. La partida continúa con la configuración del **número inicial** de jugadores: los turnos por ronda y las construcciones por turno no cambian.
6. Si con ese retiro quedan **menos de dos jugadores**, la partida termina y gana el que queda.
7. Termina el caso de uso, salvo que el jugador o el invitado retirado vuelva más tarde. (FA05)

FA05 - El jugador o el invitado retirado vuelve más tarde
1. El jugador o el invitado recupera la conexión después de haber sido retirado.
2. El sistema **no lo devuelve** a la partida, porque ya no participa en ella.
3. La partida en la que fue retirado **sigue contando para su ranking**, con la puntuación y el puesto que le correspondan.
4. Termina el caso de uso.

FA06 - El jugador o el invitado decide no volver
1. El jugador o el invitado selecciona la opción "Salir del juego" del aviso de desconexión.
2. El sistema cierra la aplicación; la ventana de reconexión sigue corriendo en el servidor y, al vencer, el jugador o el invitado queda retirado por FA04.
3. Termina el caso de uso.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el jugador o el invitado no vuelve a la partida; su ventana sigue corriendo.

EX02 - El servidor cae durante la desconexión del jugador o del invitado
1. El servidor deja de funcionar mientras el jugador o el invitado estaba desconectado.
2. La ventana de reconexión **deja de correr** mientras el servidor está caído: con el servidor caído, los noventa segundos no corren.
3. El sistema **indica en la interfaz** que no hay conexión con el servidor, y el cliente sigue intentando restablecerla mientras el jugador o el invitado no salga del juego.
4. Al volver a arrancar el servidor, se ejercita el caso de uso CU-27 Volver a una partida reanudada.

| | |
|---|---|
| **Postcondiciones** | POST-1. Si el jugador o el invitado volvió dentro de su ventana, sigue participando en la partida con su estado intacto.<br>POST-2. Si la ventana venció, el jugador o el invitado queda marcado como retirado, sus orugas ya no están en el tablero y sus construcciones sí.<br>POST-3. La partida sigue contando para el ranking del jugador o del invitado, haya vuelto o no.<br>POST-4. La partida nunca se detiene por esperar a un jugador: el reloj corre durante toda la desconexión. |
| **Extensiones** | Ninguna. El **retiro del jugador o el invitado que agota su ventana** es un comportamiento del sistema, no un caso de uso, y se describe en el flujo alterno FA04 |
| **Inclusiones** | Ninguna |

---

## CU-27 Volver a una partida reanudada

| | |
|---|---|
| **ID** | CU-27 |
| **Nombre** | Volver a una partida reanudada |
| **Descripción** | Permite a un jugador volver a una partida que quedó en curso cuando el servidor se cayó. Mientras el servidor está caído, el sistema **se lo indica en la interfaz**; si el servidor vuelve y el jugador o el invitado **no ha salido del juego**, su cliente se reconecta y la partida se reanuda **sin ninguna intervención suya**. Si sí había salido, al volver a entrar el sistema **le indica que su partida sigue en curso** y le ofrece volver a ella o abandonarla. La partida se reanuda cuando han vuelto **al menos dos jugadores** dentro del plazo de reanudación. Los jugadores con cuenta se identifican con ella y los invitados presentan el pase de asiento que su cliente conserva. El turno que quedó a medias **se juega de nuevo entero**, con noventa segundos completos. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El servidor debe haber vuelto a arrancar tras una caída.<br>PRE-2. Debe existir una partida que quedó registrada como en curso, con su estado guardado.<br>PRE-3. El jugador o el invitado debe haber estado participando en esa partida. |
| **Disparador** | El servidor vuelve a arrancar y el cliente del jugador o el invitado restablece la conexión: **por sí solo**, si el jugador o el invitado no salió del juego, o cuando el jugador o el invitado vuelve a entrar, si sí había salido. |

**Flujo Normal**

1. Mientras el servidor está caído, el sistema **indica al jugador o al invitado en la interfaz que no hay conexión con el servidor**, y su cliente sigue intentando restablecerla. (FA07)
2. Al arrancar, el sistema busca las partidas que quedaron en curso y las mantiene a la espera durante el **plazo de reanudación**, contado desde el arranque.
3. El cliente del jugador o del invitado, que no se cerró, **restablece la conexión y lo identifica sin que el jugador o el invitado tenga que hacer nada**. (FA01)
4. El sistema comprueba, con las participaciones guardadas y con el estado de la partida, que ese jugador ocupaba un puesto en ella. (FA02) (FA05) (FA06)
5. El sistema lo devuelve a la partida y le muestra el aviso "Tu partida sigue en curso" con la indicación "Esperando a que vuelvan los demás", el tiempo que queda de plazo y la lista de jugadores con quién ha vuelto ya y quién no.
6. Cuando han vuelto **al menos dos jugadores**, el sistema reanuda la partida **sin ninguna intervención de los jugadores**. (FA03) (FA04)
7. El sistema **descarta por completo el turno que quedó interrumpido**, incluidas las cartas obtenidas o utilizadas en él, y se lo comunica a los jugadores.
8. El sistema reanuda desde el estado guardado, con la ronda, el turno y el puesto que constan en él, y **con noventa segundos completos**. (EX01)

*El cierre de las salas que hubieran quedado abiertas también ocurre al arrancar el servidor, pero no forma parte de este caso de uso: no lo persigue ningún actor y sucede **una sola vez por arranque**, no una vez por jugador que vuelve. Su efecto está en POST-5.*

**Flujo Alterno**

FA01 - El que vuelve es un invitado
1. El que se conecta es un invitado y, por tanto, **no figura en ninguna participación guardada**: de un invitado no se guarda ninguna fila.
2. El invitado presenta el **pase de asiento**, que es el mismo identificador temporal que el servidor le entregó al darle su identidad y que copió al estado de la partida junto a su alias y su puesto cuando la partida empezó.
3. El sistema compara el pase con el que consta en el estado de la partida y, si coincide, le devuelve su puesto con sus orugas, sus construcciones y sus cartas. **Quien no presente un pase válido no puede ocupar ese puesto.**
4. El pase **prueba que se ocupaba ese asiento, no quién es esa persona**: el invitado no tiene credencial y el sistema no puede distinguirlo de otro modo. Quien tenga el pase tiene el asiento.
4. El flujo continúa en el paso 5 del flujo normal.

FA02 - El invitado cerró la aplicación y perdió su pase
1. El que vuelve es un invitado cuyo cliente ya no conserva el pase, porque cerró la aplicación: **el pase vive en la memoria del cliente y no se escribe en disco**, para que el invitado no acabe teniendo una identidad que sobrevive a la sesión.
2. El sistema no puede comprobar que sea quien dice ser y **no le devuelve su puesto**.
3. Ese jugador no cuenta para los dos que hacen falta para reanudar.
4. Termina el caso de uso.

FA03 - Vuelve un solo jugador y vence el plazo
1. El plazo de reanudación vence habiendo vuelto un único jugador.
2. La partida **termina**, porque no puede jugarse con menos de dos, y **gana el jugador o el invitado que volvió**.
3. El sistema registra la partida como terminada con motivo "abandonada", con el puesto 1 para ese jugador si tiene cuenta, y elimina su estado.
4. Termina el caso de uso.

FA04 - No vuelve nadie y vence el plazo
1. El plazo de reanudación vence sin que haya vuelto ningún jugador.
2. El sistema registra la partida como terminada con motivo "interrumpida", **sin puestos ni puntuaciones**, y elimina su estado.
3. Esa partida no cuenta para ningún ranking y aparece en el historial como "Interrumpida".
4. Termina el caso de uso.

FA05 - La partida la jugaban solo invitados
1. El sistema detecta que la partida no tiene ninguna participación guardada, porque todos sus jugadores eran invitados.
2. El sistema comprueba el puesto de cada jugador que vuelve **únicamente con su pase de asiento**, guardado en el estado de la partida.
3. La partida se reanuda igualmente si vuelven al menos dos invitados con su pase; cuando termine, se eliminará junto con su estado, porque no podría consultarla nadie.
4. El flujo continúa en el paso 6 del flujo normal.

FA06 - El jugador o el invitado ya había sido retirado o había abandonado antes de la caída
1. El sistema detecta que ese jugador había agotado su ventana de reconexión, o había abandonado la partida, antes de que el servidor cayera, y que su participación está marcada como retirada.
2. El sistema **no le devuelve ningún puesto**: ya no participaba en la partida cuando esta se interrumpió.
3. Termina el caso de uso.

FA07 - El jugador o el invitado salió del juego mientras el servidor seguía caído
1. El jugador o el invitado cierra la aplicación, o sale de ella, mientras el servidor sigue sin responder.
2. Cuando vuelve a entrar y el servidor ya está en pie, el sistema **le indica que su partida sigue en curso** y le ofrece dos opciones: volver a ella o abandonarla.
3. Si el jugador o el invitado elige volver a la partida, el flujo continúa en el paso 4 del flujo normal.
4. Si el jugador o el invitado elige abandonarla, se ejercita el caso de uso CU-28 Abandonar la partida.
5. Si el jugador o el invitado es un **invitado** que cerró la aplicación, su pase de asiento se perdió con ella y no puede volver: se le aplica FA02.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el jugador o el invitado no vuelve a la partida.

| | |
|---|---|
| **Postcondiciones** | POST-1. Si volvieron al menos dos jugadores, la partida está reanudada desde su último estado guardado, con el turno interrumpido descartado.<br>POST-2. Cada invitado que volvió ocupa el puesto que tenía, comprobado con su pase de asiento.<br>POST-3. Si volvió uno solo, la partida está terminada como abandonada y ese jugador es el ganador.<br>POST-4. Si no volvió nadie, la partida está terminada como interrumpida, sin puestos ni puntuaciones.<br>POST-5. **Ninguna sala se ha reanudado**: las salas que quedaran abiertas se cerraron al arrancar el servidor, y con ellas se perdieron su chat y su ranking. Por eso, cuando la partida reanudada termine, sus jugadores vuelven a la ventana GUIMainMenu y no a una sala.<br>POST-6. La partida reanudada conserva su sala como referencia, aunque esa sala esté ya cerrada.<br>POST-7. El jugador o el invitado que no salió del juego ha vuelto a su partida **sin haber hecho nada**; el que había salido ha sido informado de que seguía en curso y ha podido elegir. |
| **Extensiones** | CU-28 Abandonar la partida, alcanzable desde el aviso que ofrece volver a la partida o abandonarla |
| **Inclusiones** | Ninguna |

---

## CU-28 Abandonar la partida

| | |
|---|---|
| **ID** | CU-28 |
| **Nombre** | Abandonar la partida |
| **Descripción** | Permite a un jugador dejar una partida en la que participa, rindiéndose desde el menú de la ventana de la partida, o abandonándola desde el aviso que le ofrece volver a una partida que siguió en curso tras una caída del servidor. El jugador o el invitado que abandona deja de participar, con el mismo efecto que quien agota su ventana de reconexión: sus orugas se retiran del tablero, sus construcciones se quedan en él y deja de contar para el resultado y para «el de menor puntuación». Si con su salida quedan menos de dos jugadores, la partida termina y gana el que queda. |
| **Autor** | Equipo de desarrollo |
| **Actores** | Jugador, Invitado |
| **Precondición** | PRE-1. El sistema debe estar conectado al servidor.<br>PRE-2. El jugador o el invitado debe estar participando en una partida en curso, o tener una partida en curso que lo espera tras una caída del servidor. |
| **Disparador** | El jugador o el invitado selecciona la opción "Rendirse" del menú de la ventana de la partida; o la opción de abandonar del aviso que le indica que su partida sigue en curso. |

**Flujo Normal**

1. El sistema muestra en el menú de la ventana de la partida la opción "Rendirse", disponible para cualquier jugador de la partida, **sea o no su turno**. (FA01)
2. El jugador o el invitado selecciona la opción "Rendirse".
3. El sistema muestra el aviso de confirmación "Rendirse", con el texto "Dejarás la partida y no podrás volver a ella." y las opciones "Rendirse" y "Cancelar". (FA02)
4. El jugador o el invitado selecciona la opción "Rendirse" en el aviso.
5. El sistema **retira del tablero las orugas** de ese jugador, con lo que esas flores quedan libres para los demás, y **deja sus construcciones donde están**. (EX01)
6. El sistema marca su participación como **retirada**, si el jugador o el invitado tiene cuenta. (FA06)
7. El sistema avisa a los demás jugadores de que ese jugador ha dejado la partida y actualiza su apartado "Jugadores".
8. La partida continúa con la configuración del **número inicial** de jugadores: los turnos por ronda y las construcciones por turno no cambian. (FA03) (FA04)
9. El sistema devuelve al jugador o al invitado que abandonó a la ventana GUIRoom de la sala, que sigue abierta. (FA05)

**Flujo Alterno**

FA01 - No es el turno del jugador o el invitado que quiere abandonar
1. El sistema detecta que en ese momento juega otro jugador.
2. El sistema muestra igualmente la opción "Rendirse", porque **abandonar no exige ser el jugador o el invitado al que le toca**.
3. El flujo continúa en el paso 2 del flujo normal.

FA02 - Cancelar el abandono
1. El jugador o el invitado selecciona la opción "Cancelar" en el aviso de confirmación.
2. El sistema cierra el aviso y el jugador o el invitado **sigue en la partida**, con sus orugas, sus cartas y su puesto intactos.
3. Termina el caso de uso.

FA03 - Con el abandono quedan menos de dos jugadores
1. El sistema detecta que, tras retirar a quien abandonó, queda un único jugador en la partida.
2. La partida **termina**, porque no puede jugarse con menos de dos, y **gana el jugador o el invitado que queda**.
3. El sistema registra la partida como terminada con motivo "abandonada", con el puesto 1 para ese jugador si tiene cuenta, y elimina el estado de la partida.
4. El flujo continúa en el paso 9 del flujo normal.

FA04 - El jugador o el invitado que abandona era quien debía colocar la mariposa
1. El sistema detecta que al jugador o al invitado que abandona le correspondía colocar la mariposa en la ronda siguiente, por ser el de menor puntuación.
2. Como ya no participa, el sistema designa al de menor puntuación **entre los que siguen jugando**, según el flujo alterno FA05 de CU-24 Preparar la partida.
3. El flujo continúa en el paso 8 del flujo normal.

FA05 - El jugador o el invitado abandona desde el aviso de partida en curso
1. El jugador o el invitado había salido del juego mientras el servidor estaba caído y, al volver a entrar, el sistema le indica que su partida sigue en curso y le ofrece volver a ella o abandonarla, según el flujo alterno FA07 de CU-27 Volver a una partida reanudada.
2. El jugador o el invitado elige abandonarla.
3. El sistema aplica los pasos 5 a 8 del flujo normal, sin mostrar la ventana de la partida.
4. El sistema devuelve al jugador o al invitado a la ventana GUIMainMenu, porque **la sala de esa partida ya no existe**: las salas no se reanudan.
5. Termina el caso de uso.

FA06 - El que abandona es un invitado
1. El sistema detecta que el jugador o el invitado que abandona no tiene cuenta.
2. El sistema lo retira igualmente de la partida, pero **no marca ninguna participación**, porque de los invitados no se guarda ninguna fila.
3. Su pase de asiento deja de valer para esa partida: no podrá volver a ocupar su puesto.
4. El flujo continúa en el paso 8 del flujo normal.

**Excepciones**

EX01 - Error de conexión con el servidor
1. El sistema detecta que no es posible conectarse al servidor.
2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y el abandono no se registra.
3. Si la pérdida de conexión persiste, el jugador o el invitado queda desconectado y se le aplica la ventana de reconexión de CU-26, que no es lo mismo que abandonar.

| | |
|---|---|
| **Postcondiciones** | POST-1. El jugador o el invitado deja de participar en la partida y no puede volver a ella.<br>POST-2. Sus orugas ya no están en el tablero y sus construcciones sí, igual que ocurre con el retiro por ventana agotada.<br>POST-3. Si tenía cuenta, su participación queda marcada como retirada, y **la partida sigue contando para su ranking**.<br>POST-4. Si quedaron menos de dos jugadores, la partida está terminada como abandonada y gana el que quedó.<br>POST-5. El jugador o el invitado que abandonó ha vuelto a la sala, si sigue abierta, o al menú principal si no. |
| **Extensiones** | Extiende a CU-25 Jugar un turno, desde la opción "Rendirse" del menú de la ventana de la partida; y a CU-27 Volver a una partida reanudada, desde la opción de abandonar de su aviso |
| **Inclusiones** | Ninguna |

---

## Resumen de relaciones

| Caso de uso | Extensiones | Inclusiones |
|---|---|---|
| CU-01 Iniciar sesión | CU-02, CU-03, CU-04 | Ninguna |
| CU-02 Crear cuenta | Extiende a CU-01 | Ninguna |
| CU-03 Recuperar acceso | Extiende a CU-01 | Ninguna |
| CU-04 Jugar como invitado | Extiende a CU-01 | Ninguna |
| CU-05 Cambiar el idioma de la interfaz | Ninguna | Ninguna |
| CU-06 Modificar el perfil | CU-07 | Ninguna |
| CU-07 Eliminar la cuenta | Extiende a CU-06 | Ninguna |
| CU-08 Enviar una solicitud de amistad | CU-09, CU-20 | Ninguna |
| CU-09 Responder una solicitud de amistad | Extiende a CU-08 | Ninguna |
| CU-10 Consultar el historial de partidas | CU-11 | Ninguna |
| CU-11 Consultar el detalle de una partida | Extiende a CU-10 | Ninguna |
| CU-12 Consultar el ranking global | Ninguna | Ninguna |
| CU-13 Entrar a la sala | CU-16, CU-18, CU-19, CU-21, CU-22 | Ninguna · **incluido por CU-14, CU-15 y CU-17** |
| CU-14 Unirse a una sala | CU-15 | **CU-13** |
| CU-15 Crear una sala | Extiende a CU-14 | **CU-13** |
| CU-16 Invitar jugadores a la sala | Extiende a CU-13 | Ninguna |
| CU-17 Responder una invitación a una sala | Ninguna | **CU-13** |
| CU-18 Salir de la sala | Extiende a CU-13 | Ninguna |
| CU-19 Iniciar la partida | Extiende a CU-13 | **CU-24** |
| CU-20 Eliminar un amigo | Extiende a CU-08 | Ninguna |
| CU-21 Expulsar a un jugador de la sala | Extiende a CU-13 | Ninguna |
| CU-22 Enviar un mensaje al chat | CU-23 · Extiende a CU-13 y a CU-25 | Ninguna |
| CU-23 Reportar a un jugador | Extiende a CU-22 | Ninguna |
| CU-24 Preparar la partida | Extiende a CU-25 | Ninguna · **incluido por CU-19** |
| CU-25 Jugar un turno | CU-22, CU-24, CU-28 | Ninguna |
| CU-26 Reconectar a una partida en curso | Ninguna | Ninguna |
| CU-27 Volver a una partida reanudada | CU-28 | Ninguna |
| CU-28 Abandonar la partida | Extiende a CU-25 y a CU-27 | Ninguna |

**Las dos únicas inclusiones del conjunto** son `CU-13 Entrar a la sala`, ineludible en las tres
formas de llegar a una sala, y `CU-24 Preparar la partida`, ineludible antes del primer turno.
Todo lo demás son extensiones, porque **el caso base se completa sin ellas**.

**`CU-24` aparece en las dos columnas y no es una contradicción:** `CU-19` lo **incluye**, porque
la colocación inicial ocurre siempre al empezar la partida; y **extiende** a `CU-25`, porque la
recolocación de la mariposa ocurre **solo** si el turno que se cierra era el último de una ronda que no
es la tercera.

**`CU-28` extiende a dos casos porque tiene dos entradas:** la opción "Rendirse" del menú de la
ventana de la partida, que es la ventana que gobierna `CU-25`, y la opción de abandonar del aviso
de partida en curso de `CU-27`. La primera está disponible para cualquier jugador de la partida,
sea o no su turno; si el equipo llega a definir un caso de uso propio para «estar en la partida»,
esa extensión debería colgar de él.

---

## Vocabulario de actores

**Tres actores, y ninguno cambia de nombre por estar en una sala, en una partida o en su cuenta.**

| Actor | Quién es | Qué puede hacer que el otro no |
|---|---|---|
| **Jugador** | Quien tiene una cuenta y ha iniciado sesión. Equivale a la entidad `Player` | Tiene perfil, historial, ranking global, amigos e invitaciones. **Invita, reporta y puede ser reportado y sancionado** |
| **Invitado** | Quien entró con un alias libre y un identificador temporal, sin cuenta. Es una **identidad cerrada**, no una cuenta recortada | Juega, crea salas, entra a ellas, escribe en el chat y **puede ser anfitrión**. No tiene historial, ranking global, amigos ni invitaciones; **no invita, no reporta y no puede ser reportado** |
| **Usuario sin cuenta** | Quien todavía no se ha identificado | Solo puede crear una cuenta, entrar como invitado y cambiar el idioma |

**Condiciones que no son actores.** Ser **anfitrión** de una sala, estar **en turno** dentro de
una partida o estar **dentro de una partida** son situaciones en las que se encuentra un Jugador
o un Invitado, no identidades distintas. Cada una vive en la precondición del caso de uso que la
exige:

| Condición | Dónde se exige |
|---|---|
| Ser el anfitrión de la sala | `CU-19` PRE-2 y `CU-21` PRE-2 |
| Ser quien tiene el turno | `CU-25` PRE-3 |
| Estar dentro de una partida en curso | `CU-24`, `CU-26`, `CU-27` y `CU-28` |
| Estar dentro de una sala | `CU-16`, `CU-18`, `CU-21` y `CU-22` |

**No existe el rol de administración.** Nadie revisa reportes, nadie levanta sanciones y nadie
mantiene los catálogos de parámetros, que son de solo lectura y se cargan por script. Por eso
ningún caso de uso de este documento tiene un actor administrador.

---

## Observaciones

Estas notas **no forman parte de las descripciones**. Recogen qué no se describió y por qué, sin
rellenar ningún caso de uso con supuestos.

### Datos que faltaban, y cómo quedaron

**Los nueve huecos que la versión anterior dejó señalados están cerrados.** El equipo los resolvió
y esta versión los integra en el flujo, la excepción o la postcondición que corresponde.

| # | Lo que faltaba | Cómo quedó | Dónde está |
|---|---|---|---|
| 1 | Desconexión durante la colocación inicial y tiempo máximo para colocar | **El mismo tiempo y las mismas reglas que un turno**: noventa segundos, y la misma ventana de reconexión | CU-24, paso 2, FA06 y EX02 |
| 2 | Cómo se avisa al jugador o al invitado de que hay una partida reanudada esperándole | Mientras el servidor está caído **se le indica en la interfaz**; si no salió del juego, **la partida se reanuda sin intervención suya**; si sí salió, al volver a entrar **se le indica que sigue en curso** y elige volver o abandonarla | CU-27, pasos 1, 3 y 6, y FA07; CU-26 EX02 |
| 3 | Qué muestra el sistema si falla el correo de una invitación | **El mismo criterio que CU-03**: informa con el mensaje "No pudimos enviar el correo en este momento. Inténtalo más tarde", no reintenta, y la invitación queda registrada | CU-16, EX02 |
| 4 | Con qué plazo caduca el código de recuperación | **Cinco minutos** desde que se genera | CU-03, pasos 6 y 9, FA04 y POST-1 |
| 5 | Si eliminar un amigo pide confirmación | **Sí.** Aviso "Eliminar amigo" con las opciones "Eliminar" y "Cancelar" | CU-20, paso 3 y FA04 |
| 6 | Qué pasa con la sala si se elimina la cuenta estando dentro de ella | **Sale de ella**, aplicando las reglas que rigen cualquier salida: libera su color, hereda el anfitrión el siguiente, y la sala se cierra si queda vacía | CU-07, paso 9 y FA05 |
| 7 | Si el reparto de construcciones es del cierre de ronda o del turno | **Por turno** | CU-25, pasos 2 y 3, y FA10 |
| 8 | A qué ventana vuelve el jugador o el invitado al terminar la partida | **A la sala**, porque la invitación es a la sala y en ella pueden jugarse varias partidas; al menú principal solo si la sala ya no existe | CU-25, FA10 paso 5; CU-28, paso 9 y FA05 |
| 9 | El formato de la contraseña | **El descrito en CU-02 y CU-03**: de 8 a 20 caracteres, con mayúsculas, minúsculas, números y caracteres especiales | CU-02, paso 6; CU-03, paso 11 |

**Lo que estas decisiones obligaron a cambiar en los demás documentos, ya aplicado:**

| Documento | Qué se corrigió |
|---|---|
| `Documento-Base-Consolidada-Torres.md` | `FC-29` y `S-3` dicen ahora que las construcciones se reciben **por turno** · §4.2 recoge el **formato de la contraseña** · la entidad `Código de recuperación` recoge sus **cinco minutos** · se añadieron **`FC-44`** y **`CU-28`**, las reglas de **abandono**, la **vuelta a la sala** al terminar y la **reanudación sin intervención** · §5.3 baja de tres pendientes a dos, los dos de proyecto |
| `Prototipo-Pantallas-Torres.html` y `Analisis-Pantallas-Prototipo-Torres.md` | `PT-24` ofrece **"Volver a la sala"** · `PT-22` gana la opción **"Rendirse"** · `PT-21` gana su **reloj de noventa segundos** · `PT-17` recoge los avisos de **"Eliminar amigo"** y **"Rendirse"** · nace **`PT-27` Partida en curso al volver** · `PU-26` a `PU-29` quedan cerradas y se abre `PU-30`, dónde vive exactamente el menú de la partida |
| `code/database/schema.sql`, `seed-configuration.sql` y `Modelo-ER-Torres.md` | Estaban en la versión 4 del análisis de persistencia, con **siete entidades**. Ahora tienen **diez**: se añadieron `password_recovery` —con sus cinco minutos—, `report` y `sanction`, y los catálogos `sanction_level` y `system_parameter` con sus valores · el código de sala pasa a **cuatro caracteres sin confundibles** · se retiró `invitation_token`, que el correo ya no necesita · `was_withdrawn` cubre también el **abandono voluntario** |

### El código de sala es la credencial de la sala

Hay una consecuencia del correo que conviene dejar escrita, porque parece un descuido y no lo es.
El correo de invitación lleva **el código de la sala**, y con ese código se entra a la sala, sea
pública o privada. Es decir: **quien reenvíe ese correo le está dando la entrada a otro**.

Se estudió si entrar por código a una sala **privada** debería exigir además que quien lo teclea
tenga una invitación pendiente a esa sala. **No procede**, y el motivo es concreto: **un invitado
no tiene cuenta, y las invitaciones van siempre dirigidas a una cuenta**. Si el código dejara de
bastar, **ningún invitado podría entrar nunca a una sala privada**, y los invitados sí pueden
entrar a las salas e incluso ser anfitriones. Exigir la invitación quitaría algo que ya está
definido para resolver un riesgo menor.

Queda entonces así, y es el comportamiento buscado:

- **El código es la credencial.** Quien lo tenga entra, tenga o no invitación, tenga o no cuenta.
- **La invitación no es una llave, es un aviso dirigido**: sirve para que el destinatario encuentre
  la sala sin que nadie le dicte el código, y para que la encuentre aunque estuviera desconectado.
- **Lo que acota el daño ya existe**: una sala admite cuatro personas como mucho, y el anfitrión
  puede expulsar a cualquiera que no debiera estar dentro.
- **Una sala privada no es una sala secreta.** Lo único que cambia respecto de una pública es que
  no aparece en el listado.

### Invitar a un amigo se hace desde la sala, no desde la lista de amigos

La ventana de amigos **no ofrece ninguna opción de invitar a una sala**, y esto tampoco es un
olvido.

Para invitar a alguien hay que **estar dentro de una sala**: la invitación se asocia a esa sala, y
sin sala no hay nada a lo que invitar. La ventana de la sala ofrece tres salidas —invitar
jugadores, salir e iniciar la partida— y **ninguna lleva de vuelta al menú principal**. Así que
para llegar a la lista de amigos habría que salir de la sala primero, y al salir ya no queda sala
a la que invitar: el atajo se quedaría sin sentido justo en el momento de usarlo.

Por eso invitar a un amigo ocurre donde siempre ocurrió: **dentro de la sala**, con la opción
"Invitar jugadores", que abre una ventana cuya primera pestaña es "Mis amigos" precisamente para
elegir entre ellos sin buscar a nadie. La lista de amigos sirve para tener amigos; la sala, para
invitarlos.

**Lo único que podría cambiar esto** es que el equipo decida que desde dentro de una sala se puede
abrir otra ventana sin abandonarla. Esa decisión no está tomada, y mientras no lo esté, un atajo
en la lista de amigos no puede describirse ni dibujarse.

### Ventanas de la partida

Los nombres `GUIxxx` que este documento usa son los de la **captura de ventanas del sistema**:
`GUIMainMenu`, `GUILogIn`, `GUISignIn`, `GUIRecoverAccess`, `GUIGuestAccess`, `GUIChangeLanguage`,
`GUIProfile`, `GUIAccountConfiguration`, `GUIFriends`, `GUIInvitationsReceived`, `GUIHistory`,
`GUIGameDetails`, `GUIGlobalRanking`, `GUISearchRooms`, `GUICreateRoom`, `GUIRoom`,
`GUIInvitePlayers` y `GUILoadingGame`.

**Las ventanas del tablero no están en esa captura**, que es anterior a que se prototipara la
partida. Por eso `CU-24`, `CU-25`, `CU-26` y `CU-27` dicen **«la ventana de la partida»** y
nombran sus elementos tal como los rotula el prototipo —el apartado "Jugadores", el apartado
"Mazo", las acciones "Oruga · 2", "Mover · 1", "Subir · 1" y "Construir · 1", el indicador "PA",
la opción "Terminar turno"—. Cuando el equipo les asigne nombre, basta sustituir esa expresión.

**La opción de expulsar, el apartado de chat y la opción de reportar sí están en el prototipo**,
que se actualizó después de que se tomaran esas decisiones: viven en la lista de jugadores y en
el panel de chat de la sala, sin añadir ninguna pantalla nueva.

**El canal de correo de `CU-16` está declarado en el análisis de pantallas como estado de
`PT-16`, pero su control no está dibujado** en el prototipo. Por eso `FA05` describe la elección
del canal sin citar el rótulo de ninguna opción.

**Tres elementos nacen de las aclaraciones y todavía no están en el prototipo:** el **menú con la
opción "Rendirse"** de la ventana de la partida, el **aviso que ofrece volver a la partida o
abandonarla** tras una caída del servidor, y los **avisos de confirmación** de `CU-20` y `CU-28`.
Sus textos se escribieron siguiendo el patrón de los avisos que el prototipo ya tiene en `PT-17`
—título, una frase que dice qué va a pasar, y dos opciones— y deberán confirmarse cuando esas
pantallas se dibujen.

### Decisiones de granularidad que corresponden al equipo

- **Las acciones dentro de un turno** —colocar una oruga, moverla, subirla de nivel, colocar
  una construcción, obtener una carta y utilizar una carta— se describen como **flujos alternos
  de CU-25**, no como casos de uso propios. El razonamiento está más abajo.
- **CU-18 y CU-21** comparten «retirar de la lista de jugadores, liberar el color y avisar a los
  demás». Puede factorizarse en un fragmento común incluido por los dos, o describirse por
  separado como está ahora. Difieren en actor, en disparo, en si la sala puede cerrarse, en si se
  hereda la condición de anfitrión y en si hay confirmación de por medio.

### Por qué CU-25 no se divide

Se examinó si «Jugar un turno» debía separarse en un caso de uso por acción.

- **Objetivo.** Es uno solo: gastar los puntos de acción del turno antes de que se agote el
  tiempo. Colocar, mover, construir u obtener cartas **no son objetivos**: son maneras de
  gastarlos.
- **Actor.** El mismo para todas: el jugador o el invitado al que le toca. Ninguna acción tiene actor propio.
- **Independencia.** Ninguna acción se sostiene fuera del turno: todas exigen ser el jugador o el invitado en
  turno y tener puntos disponibles, y ninguna deja una postcondición observable fuera de él.
- **El turno es un presupuesto.** Los 5 puntos, las cartas 1 y 2 que los suben a 6 o 7, el límite
  de construcciones de ese turno y el coste por nivel **solo tienen sentido juntos**. Separar las
  acciones obligaría a repetir ese presupuesto en cada una, o a perderlo.
- **Claridad.** La división produciría seis casos de uso de tres o cuatro pasos, todos con la
  misma precondición y el mismo disparador, más un séptimo que los coordinaría: el mismo caso con
  más papeleo.
- **Relaciones.** Si se separaran serían **`extend`**, nunca `include`, porque todas son
  opcionales. Y un caso base del que cuelgan seis extensiones y que por sí solo no hace nada es
  precisamente lo que conviene evitar.
- **Correspondencia con el sistema.** El sistema no ofrece «mover una oruga» como servicio
  independiente: ofrece jugar el turno.

**La otra división sí procede, y está aplicada:** `CU-19 Iniciar la partida` y `CU-24 Preparar la
partida` son casos distintos. El arranque lo ejecuta **el anfitrión** y registra la partida; la
colocación la ejecutan **todos los jugadores, por orden de puesto de mesa**, en otro momento y
con reglas propias.

### Comportamientos del sistema que este documento no describe como casos de uso

No tienen actor humano: los dispara el servidor o el vencimiento de un plazo. Unos quedan
recogidos en el flujo del caso de uso que los provoca y **otros no están en ninguna parte**; esos
se cubren con **historias de usuario**, en `Documento-Historias-de-Usuario-Torres.md`.

| Comportamiento | Dónde queda |
|---|---|
| `S-2` Cierre del turno por agotarse los 90 s | CU-25, FA09, y pasos 6 y 7 de su flujo normal |
| `S-3` Cierre de la ronda | CU-25, FA10, y CU-24, FA03 |
| `S-5` Retiro del jugador o el invitado que agota su ventana de reconexión | CU-26, FA04 |
| `S-6` Reanudación tras una caída | CU-27, flujo completo y FA03/FA04 |
| `S-8` Aplicación de la sanción al alcanzarse el umbral | CU-23, FA04 |
| `S-1` Cierre de las salas abiertas al arrancar el servidor | Historia de usuario `HU-01` |
| `S-10` Marcado de inactividad y traspaso del anfitrión | Historia de usuario `HU-02` |
| `S-11` Retirada de la sala del inactivo que agota el plazo | Historia de usuario `HU-03` |
| `S-4` Fin de la partida y registro del resultado | Historia de usuario `HU-04` |
| `S-7` Actualización del ranking de la sala | Historia de usuario `HU-05` |
| `S-9` Caducidad de la prohibición temporal | Historia de usuario `HU-06` |
| `S-12` Registro de los eventos del sistema | Historia de usuario `HU-07` |

---

## Correcciones aplicadas a CU-01, CU-02 y CU-03

Estos tres venían de la primera ronda y **no se han copiado tal cual**: se compararon con las
decisiones vigentes y se corrigieron. Esto es lo que cambió.

| # | Caso | Qué decía la primera ronda | Qué dice ahora, y por qué |
|---|---|---|---|
| 1 | **CU-01** | Actor «Jugador», sin definir qué era | **Jugador**, entendido como la identidad de quien tiene cuenta —la entidad `Player`—, no como un rol dentro de una partida. La versión 4.0 fijó ese criterio para todo el documento |
| 2 | **CU-01** | Precondición y postcondiciones **vacías** | Tres precondiciones y cuatro postcondiciones, incluida la que aclara que **la prohibición temporal no impide iniciar sesión** |
| 3 | **CU-01** | No comprobaba el baneo | **Paso 7 y FA03:** el baneo permanente **impide iniciar sesión**, con el aviso que el prototipo ya tiene dibujado |
| 4 | **CU-01** | Citaba `(FA02)` en el paso 4 pero **no la describía** | FA02 escrita: credenciales incorrectas, con el mensaje "Username o contraseña incorrectos." y **sin decir cuál de los dos falló** |
| 5 | **CU-01** | Extensiones: solo `CU-02` | **CU-02, CU-03 y CU-04**: las tres son salidas opcionales de la misma ventana |
| 6 | **CU-01** | Excepción «Error de conexión con **la base de datos**» | «Error de conexión con **el servidor**», como en CU-02 y CU-03. El cliente no habla con la base de datos: habla con el servidor |
| 7 | **CU-02** | Paso 2: username «de **8 a 15** caracteres», contra su propio paso 3, que muestra «De 3 a 20 caracteres» | **De 3 a 20**, que es la regla vigente. Se contradecía consigo mismo |
| 8 | **CU-02** | «Contraseña cifrada» | **Hash BCrypt.** Cifrar es reversible; aquí no se guarda nada reversible |
| 9 | **CU-03** | Pasos 6, 8 y 9: «un **enlace** o código» | **Solo el código**, que el jugador o el invitado introduce dentro de la aplicación. El enlace **no tiene canje posible** en un cliente MonoGame sin stack web |
| 10 | **CU-03** | Extensiones: «Ninguna» | **Extiende a CU-01**, desde la opción "¿Olvidaste tu acceso?" de su misma ventana |
| 11 | **CU-03** | Tres pasos sin separar en pantalla | Los **tres pasos del prototipo** —"1 · Tu correo", "2 · El código", "3 · Contraseña nueva"— con su validación de que las dos contraseñas coincidan, que antes no estaba |
| 12 | **CU-03** | FA03 terminaba «sin proceso real» sin decir qué ve el jugador o el invitado | Aclarado que el sistema **muestra lo mismo** que en el flujo normal, para no revelar si un correo pertenece a una cuenta, y añadida esa garantía como POST-3 |

**Lo que se conservó de la primera ronda, por ser decisión suya y no contradecir nada:** el
formato de la contraseña de CU-02, el texto de la excepción de conexión, la estructura de los
flujos alternos de CU-02 y la excepción EX02 de CU-03 para el fallo del correo, que es la que
fija el criterio que `CU-16` debe ratificar.

---

## Correcciones aplicadas al resto de los casos de uso

| # | Dónde | Qué cambió, y por qué |
|---|---|---|
| 1 | **CU-24**, disparador y PRE-3 | Se disparaba solo desde CU-19 y exigía que nadie hubiera colocado ninguna pieza, pero su FA03 describía la colocación de la mariposa de las rondas 2 y 3. Ahora tiene **dos disparos**, y PRE-3 se acota a cada uno |
| 2 | **CU-24 y CU-25**, relaciones | La recolocación de la mariposa se invocaba desde un paso sin declarar ninguna relación. Ahora **CU-24 extiende a CU-25**, y sigue siendo incluido por CU-19 |
| 3 | **CU-19**, paso 8 y POST-3 | Decía que el sistema «entrega» el pase de asiento al invitado. El pase **ya existe** desde CU-04: es su identificador temporal, y la partida solo lo **copia** a su estado |
| 4 | **CU-25**, FA10 | El cierre de ronda «repartía las construcciones». Las construcciones se reciben **en cada turno** según `R 1.3` y `R 2.2`; se quitó del cierre y quedó anotado como corrección pendiente de `FC-29` y `S-3` |
| 5 | **CU-13, CU-19, CU-21** | Declaraban como precondición lo que su propio flujo comprobaba y resolvía con un flujo alterno. Se retiraron de las precondiciones y quedaron como pasos con su alterno |
| 6 | **CU-21**, flujo normal | **No pedía confirmación.** El prototipo tiene dibujado el aviso "Dejará de estar en la sala y se le avisará. Podrá volver a entrar con el código." con las opciones "Expulsar" y "Cancelar": ahora son los pasos 3 y 4, con FA04 |
| 7 | **CU-07**, disparador | Era «selecciona "Configurar cuenta" desde GUIProfile», que es el disparador de abrir la configuración, no el de eliminar la cuenta |
| 8 | **CU-26**, disparador | El disparo era un evento del servidor, con lo que un caso de uso con actor humano empezaba sin que el actor hiciera nada. Ahora el disparo es **que el jugador o el invitado vuelve a conectarse** |
| 9 | **CU-27**, flujo normal | Metía el cierre de todas las salas abiertas dentro del flujo de un jugador que vuelve, cuando ocurre **una vez por arranque**. Se retiró del flujo; su efecto sigue en POST-5 |
| 10 | **CU-14 y CU-17** | «La sala ya tiene cuatro jugadores» → «**contando también a los marcados como inactivos**», que es lo que fija la regla: el inactivo sigue ocupando su plaza |
| 11 | **CU-09 y CU-05** | Declaraban «Extensiones: Ninguna» aunque los casos que las ofrecen sí las declaraban. Ahora **CU-09 extiende a CU-08** y **CU-05 extiende a CU-06**, en los dos sentidos |
| 12 | **CU-22**, extensiones | Declaraba que extiende a CU-13 pero no a CU-25, aunque CU-25 sí la declaraba. Se declara en los dos sentidos |
| 13 | **CU-23**, FA01 | Un mensaje escrito por un invitado **terminaba** el caso de uso. Solo significa que ese mensaje no ofrece la opción de reportar |
| 14 | **CU-17, CU-20 y CU-21** | Tenían flujos alternos que ocurrían **después** de terminar el caso de uso —volver a entrar por código, volver a agregar al amigo, volver a entrar el expulsado— e invocaban a otros casos invirtiendo la dirección de las relaciones declaradas. Se eliminaron; lo que decían ya está en sus postcondiciones |
| 15 | **Todo el documento** | Los mensajes, rótulos y avisos son ahora **los literales del prototipo**, no paráfrasis: "Aún sin partidas", "No hay salas disponibles ahora", "Te han sacado de la sala", "Todavía nadie ha escrito.", "Robar · 1", "Terminar turno" y los demás |

---

## Integración de las aclaraciones del equipo

Lo que la versión 4.0 cambió respecto de la 3.0. **Ninguna corrección añade nada que el equipo no
haya decidido**; donde una decisión no cubría un detalle, se aplicó la regla que ya existía para
el caso equivalente, y así queda dicho.

| # | Decisión | Qué se hizo |
|---|---|---|
| 1 | *El tiempo para colocar es el mismo y se aplican las mismas reglas* | **CU-24** pasa a tener reloj de **noventa segundos** en cada colocación (pasos 2 y 5), un flujo alterno **FA06** para el tiempo agotado y una excepción **EX02** que aplica la ventana de reconexión de un turno. Al agotarse el tiempo, la colocación se cierra como se cierra un turno y se pasa al siguiente; el jugador o el invitado que no colocó empieza sin oruga en el tablero y podrá colocar uno en su turno pagando los 2 PA que ya cuesta esa acción |
| 2 | *Indicar la caída, reanudar sin intervención si no salieron, avisar al que salió, y poder rendirse* | **CU-27** se rehízo: el paso 1 indica la caída en la interfaz, el paso 3 reconecta **sin que el jugador o el invitado haga nada**, el paso 6 reanuda sin intervención, y el nuevo **FA07** cubre al jugador o al invitado que sí salió, al que se le indica que su partida sigue en curso y elige volver o abandonarla. **CU-26 EX02** indica también la caída. Y se añade **`CU-28 Abandonar la partida`**, con la opción "Rendirse" del menú de la ventana de la partida |
| 3 | *Resolverlo según el contexto* | **CU-16 EX02** adopta el criterio que **CU-03 EX02 ya tenía** para el mismo canal: informar con su mismo mensaje, no reintentar, y la invitación queda registrada. No se introdujo ningún mecanismo nuevo |
| 4 | *Cinco minutos* | **CU-03** lo incorpora en el paso 6, en la comprobación del paso 9, en **FA04** y en **POST-1** |
| 5 | *Sí* | **CU-20** incorpora el aviso de confirmación en el paso 3 y el **FA04** de cancelación, y renumera su flujo |
| 6 | *Sale de ella si se cumplen los demás criterios que rigen las salas* | **CU-07** gana el paso 9 y reescribe **FA05**: se retira de la lista, libera su color, el anfitrión pasa al siguiente si lo era, y la sala se cierra si queda vacía |
| 7 | *El funcionamiento es por turno* | **CU-25** ya lo describía así; se retiró la marca de pendiente y queda anotada la corrección que hay que llevar a `FC-29` y `S-3` de la base consolidada |
| 8 | *La invitación es a la sala* | **CU-25 FA10** devuelve a los jugadores a **GUIRoom** al cerrar el resultado, y solo a GUIMainMenu si la sala ya no existe. **CU-28** hace lo mismo. Esto obliga a cambiar el botón "Volver al menú" de `PT-24` |
| 9 | *Seguir lo descrito en los casos de uso 2 y 3* | El formato de la contraseña se conserva tal como está en **CU-02** y **CU-03**, y queda anotado que hay que incorporarlo a la base consolidada |

### Qué se derivó, y de dónde

Tres detalles que las decisiones no nombraban y que **no se inventaron**, sino que se tomaron de
la regla que ya rige el caso equivalente:

1. **El efecto de rendirse.** La decisión dice que los jugadores deben poder abandonar la partida,
   pero no qué le pasa a su posición. Se aplicó **el mismo régimen del jugador o el invitado retirado** por
   agotar su ventana de reconexión, que es el único que el proyecto tiene definido para quien deja
   una partida a medias: orugas fuera del tablero, construcciones dentro, deja de contar para
   el resultado y para «el de menor puntuación», y la partida sigue contando para su ranking.
2. **Que rendirse pida confirmación.** Todas las acciones irreversibles del sistema confirman
   —salir de la sala siendo el último, eliminar la cuenta, expulsar—, así que esta también.
3. **Qué pasa si se agota el tiempo de una colocación.** La decisión dice «las mismas reglas», y
   la regla del turno es que se cierra con lo confirmado y se pasa al siguiente.

Los tres están señalados aquí para que el equipo pueda corregirlos si no era eso lo que quería.

---

## Correcciones de la versión 4.0

Once observaciones del equipo, y lo que cada una cambió. Ninguna añade funcionalidad.

| # | Observación | Qué se hizo, y por qué |
|---|---|---|
| 1 | **No usar «Usuario» donde hay un actor concreto** | **Un actor es una identidad, no un rol.** Quedan tres: **Jugador** —equivale a `Player`—, **Invitado** y **Usuario sin cuenta**. Desaparecen como actores «Usuario con cuenta», «Anfitrión» y «Jugador en turno»: ser anfitrión o tener el turno son **condiciones**, y pasan a las precondiciones de `CU-19`, `CU-21` y `CU-25`. Se corrigieron los 28 casos completos: actores, flujo normal, alternos, precondiciones y postcondiciones. Donde los dos actores pueden hacer lo mismo, los pasos dicen «el jugador o el invitado» |
| 2 | **CU-02: separar campos vacíos de formato incorrecto** | Eran un solo FA04. Ahora son **FA04 campos vacíos** —el sistema no consulta la base de datos, porque con un campo vacío no hay nada que validar— y **FA05 formato incorrecto**, que nombra qué regla incumple cada campo. El paso 8 del flujo normal se partió en dos validaciones |
| 3 | **CU-06: el idioma no pertenece a la cuenta** | Se retiró de `FA06` y de las extensiones. `GUIAccountConfiguration` ya no ofrece "Idioma" |
| 4 | **CU-07: lo mismo** | El paso 1 ya no nombra la opción "Idioma". Y **`CU-05` dejó de extender a `CU-06`**: el idioma no cuelga de ningún caso de uso, se alcanza desde `GUIMainMenu`, porque es preferencia del equipo y no se guarda en ninguna cuenta |
| 5 | **CU-08: flujos alternos de búsqueda ambiguos** | Reescritos. Lo que era un FA03 que mezclaba amistad y solicitud son ahora **FA03 ya es tu amigo** y **FA04 ya hay una solicitud pendiente, en cualquiera de los dos sentidos**, cada uno diciendo qué muestra la fila y por qué el sistema no registra nada. **FA05** dice que **buscarse a sí mismo no devuelve resultado**, no que «no se puede». Y se añadió **FA06**, la pestaña "Enviadas", que no estaba descrita |
| 6 | **CU-09: la invitación a sala no encaja** | Era una inconsistencia real. La opción "Invitar a mi sala" **se retiró** del caso y del prototipo, porque invitar exige estar dentro de una sala y desde la sala no se puede llegar a la lista de amigos sin salir de ella: el razonamiento completo está en «Invitar a un amigo se hace desde la sala». `CU-09` dice ahora explícitamente que **solicitud de amistad e invitación a sala son cosas distintas**, con entidad, ventana y caso de uso propios, y que aceptar una amistad **no crea ninguna invitación** |
| 7 | **CU-15: qué caracteres se excluyen** | Se nombran: **la O y el 0, y la I, la L y el 1**. Queda escrito el alfabeto completo, y **por qué** se excluyen —el código se copia a mano de un correo— y **dónde** aplica: solo al código de sala. `CU-14` lo recoge en el campo donde se teclea. Sobre los estados: la sala tiene **dos**, abierta y cerrada, y este caso solo produce el primero; y **dos visibilidades** que se fijan al crearla y no cambian nunca |
| 8 | **CU-16: la invitación por correo no está clara** | Estaba incompleta, no omitida. `FA05` pasa de cuatro pasos a siete: quién puede enviarla, que va **al correo registrado en la cuenta** y no a una dirección escrita, que lleva **el código y quién invita** y **ningún enlace**, los dos caminos que tiene el destinatario y que **entrar por código no consume la invitación**. Queda además escrito que **el código es la credencial de la sala**, con el porqué |
| 9 | **La temática** | Las descripciones pasan a usar **oruga**, **mariposa**, **flor** y **jardinera**. Las reglas no cambian: solo el nombre de las piezas. La equivalencia con el documento de reglas está en «Criterio sobre la temática» |
| 10 | **CU-25: las cartas de acción** | El caso **no se divide** —el razonamiento está en «Por qué CU-25 no se divide»—, pero **el flujo alterno que las describía sí**. Lo que era un solo `FA06` con las ocho cartas en un párrafo son ahora **seis flujos alternos, uno por carta**, de `FA06` a `FA11`, cada uno con su condición y sus comprobaciones. Sobre el coste: **utilizar una carta nunca cuesta, y la jugada solo se paga cuando la carta lo dice**. La única que lo dice es la 7; las cartas 3, 4, 5, 6 y 8 son gratuitas de punta a punta |
| 11 | **El invitado y la caída del servidor** | Confirmado que **sí puede volver con el pase**, que ya era lo decidido. `CU-27` `FA01` deja escrito qué es el pase —el mismo identificador temporal, copiado al estado de la partida— y qué **no** es: **prueba que se ocupaba ese asiento, no quién es esa persona** |


---

## Correcciones de la revisión del diccionario i18n (16 sep 2026)

Alineación con `Diccionario-i18n-Torres.xlsx` y con el prototipo. Detalle en `Analisis-Revision-XLSX-Prototipos-Torres.md`. **Solo cambian rótulos y la forma de salir de las ventanas; ninguna regla cambia.**

| # | Qué | Casos |
|---|---|---|
| 1 | **Rótulo del regreso con destino.** En las ventanas de consulta, el botón dice a dónde vuelve: "Volver al menú", "Volver al perfil", "Volver a iniciar sesión". En los formularios sigue siendo "Volver" (CU-02, CU-04, CU-05 y CU-15). "Volver al historial" y "Volver a la sala" no cambian | CU-03, CU-06, CU-07, CU-08, CU-09, CU-10, CU-12, CU-17, CU-20 |
| 2 | **CU-01 FA07 nombra su control.** Antes decía que el jugador «abandona la ventana», sin decir cómo. Ahora presiona "Volver al menú", que ya existía en el prototipo como forma de volver sin identidad | CU-01 |
| 3 | **CU-14 gana FA07.** GUISearchRooms no tenía forma de volver al menú sin entrar a una sala. Se añade "Volver al menú" | CU-14 |
| 4 | **CU-03: el regreso está en los pasos 1 y 3**, como ya decía el caso; solo cambia el rótulo | CU-03 |
| 5 | **CU-16 FA05 nombra su opción**, "Invitar por correo", junto a "Invitar" en cada jugador. El flujo ya existía | CU-16 |
