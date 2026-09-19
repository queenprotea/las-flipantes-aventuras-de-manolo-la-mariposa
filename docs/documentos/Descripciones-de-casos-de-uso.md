# Descripciones de casos de uso

> Convertido desde el PDF original.

descripciónes de casos de uso
Las flipantes aventuras de manolo la mariposa(Torres

| Nombre de cu |  |
|---|---|
| ID | CU-01 |
| Nombre | nombre de cu |
| Descripción | descripción de lo que el cu permitira realizar |
| Autor | Equipo de desarrollo |
| Actores | todos los usuarios identificados (Actores) que podran usar este caso de uso |
| Precondición | estado o condiciones que deben cumplirse dentro del sistema para poder llevar a cabo este caso de uso |
| Disparador | accion concreta a realizar y por quien para iniciar el caso de uso |
| Flujo Normal | descripcion paso a paso de lo que se realizara en este caso de uso , la descripción debe de ser detallada debe de incluier quien lo hace, sistema para el sistema , actores en caso de varios pueden usar este cu , o concreto en caso de que sea uno ej. el sistema muestra la ventana GUIInuciarSesion, con el formulario para ingresar las credenciales “username” y “Contraseña” que debe de cumplir con el formato de numero letras y caracter especiales , con las opciones “inicar sesion” , “recuperar acceso” nombre de la pantalla/vista donde se mostrara o realizara dicha accion |
| Flujo Alterno | se deben de desribi todos los casos que eudna salir de el flujo norma que no correspondan a este o no sea el caso base a seguir , deben de ser con el mismo nivel de detalle , aqui entran las copciones como cancelar o demas ya que el flujo normal solo debe de describir un camino , en caso de existir o derivar mas , deben de ser descritos aqui |
| Excepciones | aqui se edescriben todos los casos excepciones que no permitan seguir la operacion del flujo normal o alternos , deben describirse con el mismo detalle EX01 - Error de conexión con la base de datos 1. el sistema detecta que no es posible conectarse a la base de datos. |

|  | 2. el sistema muestra el mensaje "Error de conexión con la base de datos, intenta más tarde" y cierra GUIRegistrarCoordinador. |
|---|---|
| Postcondiciones | condiciones que se deben cumplir en el sistema despues de terminar el caso de uso estado en el que debe de estar el sistema despues de terminar el caso de uso El nuevo coordinador queda registrado en la base de datos con estado "Activo". Su matrícula es su identificador de acceso al sistema. |
| Extensiones | casos de uso que podrian ejercitarse durante el trscauros del caso de uso |
| Inclusiones | casos de uso que si o si deben de ejecutarse durante el transcutso del caso de uso |

| Registrar Coordinador |  |
|---|---|
| ID | CU-01 |
| Nombre | inicial sesion |
| Descripción | permite a un jugador con cuenta en el sistema iniciar sesion con sus credenciales validas |
| Autor | Equipo de desarrollo |
| Actores | Jugador |
| Precondición |  |
| Disparador | el jugador accion la opcion “iniciar sesion” en la ventana GUIMainMenu |
| Flujo Normal | 1. el sistema muestra la ventana GUILogin con el formulario para ingreso de credenciales con “Username” y “contraseña”, con las opciones “Entrar”, “Crear Cuenta “ y “¿Olvidaste tu acceso?” 2 . el jugador ingresa su username y contraseña con formato valido (minusculas, mayusculas, caracteres especiales, min 8 caracteres) validas, correspondientes en el formulario y acciona la opcion “Entrar” 3. el sistema valida que los campos “username” y “contraseña” no esten vacios.(FA-01) 4. el sistema valida en base de datos que las credenciales ingresadas correspondan a una cuenta registrada(FA-02 5. el sistema redirige a la ventana GUIMainMenu Fin de cu |
| Flujo Alterno | FA01 - Campos vacios 1. El sistema detecta un campo vacio. 2. el sistema muestra el mensaje “Campos vacios”. 3. el sistema regresa a paso 1 del flujo normal |
| Excepciones | EX01 - Error de conexión con la base de datos 1. el sistema detecta que no es posible conectarse a la base de datos. |

|  | 2. el sistema muestra el mensaje "No se pudo comprobar sus credenciales en este momento intente nuevamente" 3. el sistema regresa al paso 1 del flujo normal |
|---|---|
| Postcondiciones |  |
| Extensiones | CU-02 Crear cuenta |
| Inclusiones | Ninguna |

| Crear cuenta |  |
|---|---|
| ID | CU-02 |
| Nombre | Crear Cuenta |
| Descripción | Permite a un usuario sin cuenta registrarse en el sistema mediante un username, correo electrónico y una contraseña, con el fin de obtener una cuenta persistente que le permita jugar, agregar amigos, acumular historial de partidas y permanecer en el ranking |
| Autor | Equipo de desarrollo |
| Actores | Usuario sin cuenta |
| Precondición | PRE-1. El sistema debe estar conectado al servidor PRE-2. El usuario no debe tener una sesión iniciada |
| Disparador | El jugador selecciona la opción “Crear cuenta” desde la pantalla GUILogIn |
| Flujo Normal | 1. El sistema muestra la ventana GUISignIn con los campos “Nombre de Usuario”, “Correo” y “Contraseña”, además de las opciones “Crear cuenta” y “Volver”. (FA01) 2. El usuario ingresa un nombre de usuario en el campo correspondiente, que cumpla con el formato de 8 a 15 caracteres compuestos por letras, números y guión bajo. 3. El sistema valida que el nombre de usuario no esté registrado por otra cuenta y muestra el mensaje de confirmación “Disponible. De 3 a 20 caracteres: letras, números y guion bajo.”. (FA02) 4. El usuario ingresa un valor en el campo “Correo” con formato de correo electrónico válido. 5. El sistema valida que el correo no esté asociado a una cuenta existente. (FA03) 6. El usuario ingresa una contraseña válida (De 8 a 20 caracteres, incluyendo letras mayúsculas y minúsculas, números y caracteres especiales). 7. El usuario selecciona la opción “Crear cuenta”. 8. El sistema valida que los tres campos estén completos y en el formato correspondiente. (FA04) 9. El sistema registra la nueva cuenta en la base de datos con el nombre de usuario, correo y contraseña cifrada, asignándole un identificador único. (EX01) 10. El sistema inicia sesión con la cuenta recién creada y redirige al usuario a la ventana GUIMainMenu. |

| Flujo Alterno | FA01 - Cancelar el registro 1. El Usuario presiona el botón "Cancelar". 2. El sistema cierra GUISignIn y regresa a la ventana GUILogIn. 3. Termina el caso de uso. FA02 - Nombre de usuario no disponible 1. El Usuario ingresa un nombre de usuario que ya pertenece a otra cuenta registrada. 2. El sistema muestra el mensaje “No disponible, ya existe una cuenta con ese nombre”. 3. El sistema deshabilita la opción “Crear cuenta” mientras el campo de “Nombre de usuario” mantenga un valor no disponible. 4. El usuario modifica el valor del campo “Nombre de Usuario” 5. El flujo regresa al paso 3 del flujo normal para validar nuevamente la disponibilidad. FA03 - Correo ya registrado 1. El Usuario ingresa un correo que ya está registrado 2. El sistema muestra el mensaje “No disponible, ya existe una cuenta con ese correo”. 3. El sistema deshabilita la opción “Crear cuenta” mientras el campo de “Correo” mantenga un valor ya registrado. 4. El usuario modifica el valor del campo “Correo” 5. El flujo regresa al paso 5 del flujo normal para validar nuevamente la disponibilidad. FA04 - Formato de campo incorrecto 1. El Usuario deja vacíos algunos de los campos “Nombre de usuario”, “Correo” o “Contraseña”, o ingresa un valor que no cumple el formato requerido. 2. El sistema muestra junto al campo correspondiente un mensaje indicando el error específico de formato. |
|---|---|
| Excepciones | EX01 - Error de conexión con el servidor 1. El sistema detecta que no es posible conectarse al servidor. 2. El sistema muestra el mensaje “No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo” y cierra GUISignIn. |
| Postcondiciones | POST-1. La nueva cuenta queda registrada en la base de datos, con nombre de usuario y correo únicos. POST-2. La contraseña queda registrada en la base de datos únicamente de forma cifrada POST-3. El Usuario queda identificado con sesión iniciada |
| Extensiones | Ninguna |
| Inclusiones | Ninguna |

| Recuperar acceso |  |
|---|---|
| ID | CU-03 |
| Nombre | Recuperar acceso |
| Descripción | Permite a un usuario con cuenta, que no recuerda su contraseña o no puede iniciar sesión, solicitar el restablecimiento de su acceso a través del correo electrónico asociado a su cuenta. |
| Autor | Equipo de desarrollo |
| Actores | Usuario con cuenta |
| Precondición | PRE-1. El sistema debe estar conectado al servidor. PRE-2. El usuario no debe tener una sesión iniciada. |
| Disparador | El usuario selecciona la opción "¿Olvidaste tu acceso?" desde la ventana GUILogIn. |
| Flujo Normal | 1. El sistema muestra la ventana GUIRecoverAccess con el campo "Correo" y las opciones "Enviar" y "Volver". 2. El usuario ingresa en el campo "Correo" la dirección de correo electrónico asociada a su cuenta. 3. El usuario selecciona la opción "Enviar". (FA01) 4. El sistema valida que el campo "Correo" tenga un formato de correo electrónico válido. (FA02) 5. El sistema verifica internamente si el correo ingresado corresponde a una cuenta registrada. (FA03) 6. Si el correo corresponde a una cuenta registrada, el sistema genera un enlace o código de recuperación de un solo uso y lo envía a esa dirección de correo. 7. El sistema reemplaza el formulario de GUIRecoverAccess por el mensaje de confirmación "Revisa tu correo. Si ese correo pertenece a una cuenta, recibirás un mensaje para volver a entrar.". 8. El usuario abre el mensaje recibido en su correo y sigue el enlace o ingresa el código para establecer una nueva contraseña. 9. El sistema valida el enlace o código de recuperación y, de ser válido, muestra un formulario para ingresar una nueva contraseña. (FA04) 10. El usuario ingresa y confirma la nueva contraseña. 11. El sistema cifra y actualiza la contraseña de la cuenta, invalida el enlace o código utilizado y redirige al usuario a GUILogIn con un mensaje de confirmación. |

| Flujo Alterno | FA01 - Cancelar el proceso de recuperación 1. El usuario presiona el botón "Volver" antes de seleccionar "Enviar". 2. El sistema cierra GUIRecoverAccess y regresa a la ventana GUILogIn. 3. Termina el caso de uso. FA02 - Formato de correo incorrecto 1. El usuario deja vacío el campo "Correo" o ingresa un valor que no cumple el formato de correo electrónico. 2. El sistema muestra junto al campo el mensaje "Ingresa un correo electrónico válido.". 3. El sistema mantiene deshabilitada la opción "Enviar" mientras el campo no cumpla el formato. 4. El usuario corrige el valor del campo "Correo". 5. El flujo regresa al paso 5 del flujo normal para validar nuevamente el correo. FA03 - El correo ingresado no corresponde a ninguna cuenta registrada 1. El sistema verifica que el correo ingresado no está asociado a ninguna cuenta. 2. El sistema no genera ningún enlace ni envía correo alguno. 3. El sistema muestra, de igual manera que en el flujo normal, el mensaje "Revisa tu correo. Si ese correo pertenece a una cuenta, recibirás un mensaje para volver a entrar.". 4. Termina el caso de uso sin que se haya iniciado ningún proceso de recuperación real. FA04 - El enlace o código de recuperación caducó o ya fue utilizado 1. El usuario abre un enlace o ingresa un código de recuperación que ya expiró o que ya fue utilizado anteriormente. 2. El sistema muestra el mensaje "Este enlace ya no es válido. Solicita uno nuevo." con la opción "Solicitar de nuevo". 3. Si el usuario selecciona "Solicitar de nuevo", el flujo regresa al paso 2 del flujo normal. 4. Termina el caso de uso si el usuario no solicita un nuevo enlace. |
|---|---|
| Excepciones | EX01 - Error de conexión con el servidor 1. El sistema detecta que no es posible conectarse al servidor. |

|  | 2. El sistema muestra el mensaje "No se pudo establecer una conexión con el servidor. Revisa tu conexión e inténtalo de nuevo" y cierra GUIRecoverAccess. EX02 - Falla en el envío del correo de recuperación 1. El sistema detecta que el servicio de envío de correos no pudo entregar el mensaje de recuperación. 2. El sistema muestra el mensaje "No pudimos enviar el correo en este momento. Inténtalo más tarde" y permanece en GUIRecoverAccess. |
|---|---|
| Postcondiciones | POST-1. Si el correo pertenece a una cuenta registrada, queda generado un enlace o código de recuperación de un solo uso, vinculado a esa cuenta. POST-2. Si el usuario completa el restablecimiento, la contraseña de la cuenta queda actualizada y almacenada de forma cifrada, y el enlace o código utilizado queda invalidado. |
| Extensiones | Ninguna |
| Inclusiones | Ninguna |
