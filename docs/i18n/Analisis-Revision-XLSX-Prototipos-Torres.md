# Revisión del diccionario i18n contra los prototipos · Torres

**Qué se revisa:** `Diccionario-i18n-Torres.xlsx` (hojas Portada, Culturas, Diccionario con 268 filas y Resumen) contra `Prototipo-Pantallas-Torres.html` (27 pantallas, `PT-01` a `PT-27`).
**Referencia de navegación:** `Documento-Casos-de-Uso-Torres.md` y `Analisis-Pantallas-Prototipo-Torres.md` §7–8.
**Estado:** revisión **resuelta** el 16 sep 2026 (ver §9). No se implementó ninguna ventana.
**Fecha:** 16 sep 2026.

---

## 0. Cómo se hizo la revisión

1. Se leyeron las cuatro hojas del XLSX completas.
2. Se abrió el prototipo en el navegador y se dibujó **cada pantalla con todas las combinaciones de conmutadores** (identidad, conexión, rol, datos, pestaña de amigos, turno, tipo de sala, ocupación). De cada combinación se tomó el texto visible y los atributos `title`, `placeholder` y `aria-label`.
3. Cada una de las 268 filas se buscó en la pantalla que le corresponde. Los marcadores `{jugador}`, `{codigo}`, `{n}`, etc. se trataron como comodines.
4. En sentido inverso, se listaron los textos del prototipo que no tienen fila en el XLSX.
5. Cada diferencia se revisó a mano. **Se descartaron como falsos positivos** los casos en que el prototipo muestra el texto en mayúsculas solo por estilo (`USERNAME`, `CORREO`, `FECHA`, `JUGADORES`, `TUS ORUGAS`, `COLOCA LA MARIPOSA`…), así como los datos de ejemplo (`ana_torres`, `K7QM`, puntos, relojes y mensajes de chat).
6. La navegación y el botón «Volver» se contrastaron con los flujos de los casos de uso.

### Correspondencia de pantallas

Las 27 pantallas del prototipo tienen su grupo en el XLSX, y todos los grupos de ventana del XLSX tienen su prototipo. **No sobra ni falta ninguna ventana.** `Common`, `Chat` y `Formats` no son ventanas: `Chat` es el panel que aparece dentro de `PT-15`, `PT-21` y `PT-22`.

| PT | Título en el prototipo | Pantalla en el XLSX | Ventana en los casos de uso | ¿Nombres consistentes? |
|---|---|---|---|---|
| PT-01 | Arranque y conexión | Startup | — | Sin nombre `GUI` |
| PT-02 | Menú principal | MainMenu | GUIMainMenu | ✔ |
| PT-20 | Idioma | Language | GUIChangeLanguage | ✘ |
| PT-03 | Iniciar sesión | Login | GUILogIn | ≈ (`Login` / `LogIn`) |
| PT-04 | Crear cuenta | Register | **GUISignIn** | ✘ (ver §7.4) |
| PT-05 | Recuperar acceso | PasswordRecovery | GUIRecoverAccess | ✘ |
| PT-06 | Entrar como invitado | GuestAccess | GUIGuestAccess | ✔ |
| PT-07 | Perfil | Profile | GUIProfile | ✔ |
| PT-08 | Configurar cuenta | AccountSettings | GUIAccountConfiguration | ✘ |
| PT-09 | Amigos | Friends | GUIFriends | ✔ |
| PT-10 | Invitaciones recibidas | Invitations | GUIInvitationsReceived | ✘ |
| PT-16 | Invitar jugadores | InvitePlayers | GUIInvitePlayers | ✔ |
| PT-11 | Historial | History | GUIHistory | ✔ |
| PT-12 | Detalle de una partida | MatchDetail | GUIGameDetails | ✘ (`Match` / `Game`) |
| PT-13 | Ranking global | GlobalRanking | GUIGlobalRanking | ✔ |
| PT-14 | Salas | Rooms | GUISearchRooms | ✘ |
| PT-19 | Crear sala | CreateRoom | GUICreateRoom | ✔ |
| PT-15 | Dentro de una sala | Room | GUIRoom | ✔ |
| PT-17 | Avisos y confirmaciones | Alerts | — (diálogos) | Sin nombre `GUI` |
| PT-18 | Preparando la partida *(menú lateral: «Iniciar partida»)* | MatchPreparation | GUILoadingGame | ✘ |
| PT-21 | Colocación inicial | InitialPlacement | «la ventana de la partida» | Sin nombre `GUI` |
| PT-22 | Partida *(menú lateral: «Tablero y turno»)* | Match | «la ventana de la partida» | Sin nombre `GUI` |
| PT-23 | Cierre de ronda | RoundSummary | «la ventana de la partida» | Sin nombre `GUI` |
| PT-24 | Resultado | Result | «la ventana de la partida» | Sin nombre `GUI` |
| PT-25 | Desconexión | Disconnection | «la ventana de la partida» | Sin nombre `GUI` |
| PT-26 | Reanudación | Resume | «la ventana de la partida» | Sin nombre `GUI` |
| PT-27 | Partida en curso al volver | MatchInProgress | — | Sin nombre `GUI` |

Hay **tres nomenclaturas en paralelo** y ocho ventanas con nombres que no coinciden entre el XLSX y los casos de uso. Además, dentro del propio prototipo, `PT-18` y `PT-22` se titulan de una forma y aparecen con otra en el menú lateral.

---

## 1. Ventanas que coinciden correctamente

Todos los textos del XLSX están en el prototipo y el prototipo no muestra textos propios que falten en el XLSX. La navegación también es coherente.

| PT | Pantalla | Nota |
|---|---|---|
| PT-01 | Startup | Sus textos propios coinciden. El aviso de conexión caída depende de `Common` (ver §2) |
| PT-06 | GuestAccess | Incluye «Volver» |
| PT-15 | Room | Incluye el chat y los estados de anfitrión y miembro |
| PT-16 | InvitePlayers | El canal de correo sigue pendiente (§7.9) |
| PT-18 | MatchPreparation | Solo cambia el nombre de la ventana (§0) |
| PT-19 | CreateRoom | Incluye «Volver» |
| PT-20 | Language | Incluye «Volver». Ver la nota interna del prototipo en §6.5 |
| PT-21 | InitialPlacement | Usa las etiquetas «tú» e «invitado», que en el XLSX solo existen bajo `Disconnection` y `Result` (§2.14) |
| PT-22 | Match | Falta el texto accesible del tablero (§4) |
| PT-25 | Disconnection | — |
| PT-26 | Resume | — |
| PT-27 | MatchInProgress | — |

---

## 2. Ventanas que requieren correcciones

### 2.1 `Common` (afecta a PT-01 y PT-02) — el XLSX está desactualizado
| Clave | XLSX | Prototipo y CU-01/CU-02 |
|---|---|---|
| `Common.ServerErrorTitle` | «No se pudo conectar con el servidor.» | «No se pudo establecer una conexión con el servidor.» |

Este texto ya se unificó el 11 sep a favor de los casos de uso. **Hay que corregir el XLSX** y su traducción.

### 2.2 PT-02 `MainMenu` — falta en los dos lados
CU-06 se inicia así: el jugador «selecciona su avatar […] y elige la opción "Perfil"». El prototipo no dibuja ese menú y el XLSX no tiene el texto «Perfil». **Hoy no hay forma visible de llegar a `PT-07`.**

### 2.3 PT-03 `Login` — posible falta de «Volver»
Ver §5 y §7.2.

### 2.4 PT-04 `Register` — falta una fila en el XLSX
- El prototipo muestra «No disponible, ya existe una cuenta con ese nombre.» (estado de username ocupado). **El XLSX no tiene esa fila en `Register`.**
- El texto equivalente de `Profile.UsernameUnavailable` usa **dos puntos** («No disponible: ya existe…») y el de `PT-04` usa **coma**. Hay que unificarlos, y si es el mismo mensaje, puede ser una sola clave.

### 2.5 PT-05 `PasswordRecovery`
- **Falta en el XLSX:** «Ingresa un correo electrónico válido.» (validación del paso 1, que sí está dibujada).
- **Texto que no coincide:** el XLSX tiene `BackToLoginButton` = «Volver a iniciar sesión». El prototipo y CU-03 dicen solo «Volver».
- **Posición de «Volver» distinta:** el prototipo lo pone únicamente en el paso 3. CU-03 lo pide en el **paso 1** («las opciones "Enviar código" y "Volver"») y en el paso 3. Ver §5.

### 2.6 PT-07 `Profile`
- **Falta «Volver»** en el prototipo. CU-06 FA01 lo exige y el XLSX lo tiene como «Volver al menú».
- **Formato numérico incoherente:** el texto base es «El archivo pesa 7.4 MB.», pero la hoja Culturas dice que es-MX usa coma decimal (`7,4`). Ver §7.5.

### 2.7 PT-08 `AccountSettings` — el XLSX está desactualizado
- **Sobran** `AccountSettings.LanguageSetting` («Idioma») y `AccountSettings.LanguageValue` («Español»). El idioma **salió de la configuración de la cuenta** (CU-05, CU-06 y CU-07; análisis de pantallas, `PT-08`: «Ya no contiene el idioma»). Hay que quitar esas dos filas.
- **Texto que no coincide:** el XLSX dice «Volver al perfil» y el prototipo y CU-07 dicen «Volver».

### 2.8 PT-09 `Friends` — la ventana con más diferencias
**Sobra en el XLSX:** `Friends.InviteToRoomButton` («Invitar a mi sala»). Se retiró a propósito: invitar exige estar dentro de una sala y se hace en `PT-16`.

**Faltan en el XLSX** (todos están dibujados en el prototipo):

| Pestaña | Textos |
|---|---|
| Amigos | Botón «Eliminar» (el XLSX solo tiene el tooltip «Eliminar de mis amigos») |
| Recibidas | «quiere ser tu amigo» · «Aceptar» · «Rechazar» · vacío: «No tienes solicitudes» / «Aquí verás las solicitudes de amistad que te envíen.» |
| Enviadas | «esperando respuesta» · «Pendiente» · vacío: «No has enviado solicitudes» / «Las que envíes aparecerán aquí hasta que las respondan.» |
| Buscar | Placeholder «USERNAME» · botón «Buscar» (existe `Friends.SearchTab`, pero es la pestaña) · «Enviar solicitud» · «Ya es tu amigo» · «Solicitud enviada» · «Te envió una solicitud» · «Tu propio username no aparece en los resultados: no puedes agregarte a ti mismo.» · sin resultado: «No encontramos a nadie con ese username» / «La búsqueda es exacta: revisa cómo se escribe.» |

**Falta en el prototipo:** «Volver» (§5).

### 2.9 PT-10 `Invitations`, PT-11 `History`, PT-13 `GlobalRanking`
Sus textos coinciden, pero **al prototipo le falta «Volver»**, que sí está en el XLSX y en los casos de uso (§5).

### 2.10 PT-12 `MatchDetail`
- El encabezado usa la **fecha larga** «05 de septiembre de 2026». La hoja `Formats` solo define la corta («05 sep 2026»). **Falta ese formato.**
- Coincide en todo lo demás, incluido «Volver al historial».

### 2.11 PT-14 `Rooms`
El XLSX tiene «Volver al menú», pero **el prototipo no lo dibuja y CU-14 no lo menciona**. Ver §5 y §7.2.

### 2.12 PT-17 `Alerts` — falta un diálogo en el XLSX
El prototipo tiene el diálogo **«No pudimos enviar el correo»** / «No pudimos enviar el correo en este momento. Inténtalo más tarde.» (CU-03 y CU-16 EX02). **No está en el XLSX.**

### 2.13 PT-23 `RoundSummary` y PT-24 `Result` — terminología retirada
`RoundSummary.FlowerbedsColumn` y `Result.FlowerbedsColumn` dicen **«Macizos»**. El prototipo dice **«Jardineras»**: el término *macizo* se descartó el 11 sep. Hay que corregir el XLSX; la traducción «Flowerbeds» puede quedarse.

Además, en PT-24 la columna de puesto muestra **1, 2, 3**, mientras que `Formats.OrdinalRankFormat` dice que las «tablas de resultados» usan ordinales (1.º, 2.º…). Hay que decidir cuál vale.

### 2.14 Etiquetas «tú», «invitado» y «sin conexión» (PT-15, PT-21, PT-22, PT-24, PT-25)
Aparecen en varias ventanas, pero en el XLSX solo están bajo `Disconnection` (`YouTag`, `DisconnectedTag`) y `Result` (`GuestTag`). No falta ningún texto, pero conviene pasarlas a `Common` para no repetir la traducción en cada ventana.

---

## 3. Elementos faltantes en los prototipos

Son elementos que el XLSX o los casos de uso exigen y que el prototipo no dibuja.

| # | Pantalla | Qué falta | Respaldo |
|---|---|---|---|
| F-1 | PT-07 | Botón «Volver» | CU-06 FA01 · `Profile.BackToMenuButton` |
| F-2 | PT-09 | Botón «Volver» | CU-08 FA08 · CU-09 FA03 · CU-20 FA02 · `Friends.BackToMenuButton` |
| F-3 | PT-10 | Botón «Volver» | CU-17 FA04 · `Invitations.BackToMenuButton` |
| F-4 | PT-11 | Botón «Volver» | CU-10 paso 6 · `History.BackToMenuButton` |
| F-5 | PT-13 | Botón «Volver» | CU-12 paso 6 · `GlobalRanking.BackToMenuButton` |
| F-6 | PT-05 | «Volver» en el paso 1 | CU-03 paso 1 y FA01 |
| F-7 | PT-02 | Menú del avatar con la opción «Perfil» | CU-06, disparador (**tampoco está en el XLSX**) |
| F-8 | PT-14 | Botón «Volver» | Solo `Rooms.BackToMenuButton`; **CU-14 no lo menciona** → confirmar |
| F-9 | PT-03 | Control para salir sin iniciar sesión | CU-01 FA07 describe la salida, pero **no nombra el control** y el XLSX tampoco lo tiene → confirmar |
| F-10 | PT-05 | Mensaje de confirmación al volver a Login tras guardar | CU-03 paso 14; **no hay texto literal** en ninguna fuente → confirmar |
| F-11 | PT-16 | Opción para invitar por correo | CU-16 FA05 y EX02; no está en el prototipo ni en el XLSX → confirmar |

---

## 4. Elementos presentes en los prototipos que no aparecen en el XLSX

| # | Pantalla | Texto del prototipo | Tipo |
|---|---|---|---|
| E-1 | PT-04 | «No disponible, ya existe una cuenta con ese nombre.» | ErrorMessage |
| E-2 | PT-05 | «Ingresa un correo electrónico válido.» | ErrorMessage |
| E-3 | PT-09 | Los 19 textos de la tabla de §2.8 | Button · SystemMessage · HelpText · GameState |
| E-4 | PT-17 | «No pudimos enviar el correo» + cuerpo | ErrorMessage |
| E-5 | PT-12 | Fecha larga «05 de septiembre de 2026» | DateFormat |
| E-6 | Todas salvo PT-01 | **Subtítulo de la barra superior** de cada pantalla: «menú principal», «idioma», «iniciar sesión», «crear cuenta», «recuperar acceso», «jugar como invitado», «perfil», «configurar cuenta», «amigos», «invitaciones», «historial», «detalle de partida», «ranking», «salas», «crear sala», «sala», «avisos», «preparando la partida», «colocación inicial», «partida · sala {codigo}», «cierre de la ronda {n}», «resultado», «partida», «reanudando», «tu partida sigue en curso». **El XLSX solo recoge el de PT-16** (`InvitePlayers.HeaderLabel`) | Label |
| E-7 | PT-02, 03, 04, 06, 18, 19 | Texto accesible de la ilustración: «Ilustración de ambientación: plantas, una oruga y mariposas» | HelpText (accesibilidad) |
| E-8 | PT-21, PT-22 | Texto accesible «Tablero de la partida» | HelpText (accesibilidad) |
| E-9 | PT-22 | El coste de cada acción, «· 2» / «· 1», se compone aparte del rótulo | TextFormat — el XLSX lo menciona en una observación, pero no le da clave |

`Common.WindowTitle`, los tres estados de conexión y «Iniciar sesión» de la barra superior **sí** están en el XLSX y en el marco del prototipo.

---

## 5. Ventanas a las que debe agregarse «Volver»

### 5.1 Revisión de las 27 pantallas

| PT | ¿Tiene «Volver» hoy? | ¿Debe tenerlo? | A dónde regresa | Respaldo |
|---|---|---|---|---|
| PT-01 | No | **No aplica** (es la entrada) | — | — |
| PT-02 | No | **No aplica** (es la raíz; tiene «Salir») | — | — |
| PT-20 | ✔ «Volver» | ✔ | Ventana de origen (GUIMainMenu) | CU-05 FA01 |
| PT-03 | No | **Por confirmar** | GUIMainMenu | CU-01 FA07 (sin control nombrado) |
| PT-04 | ✔ «Volver» | ✔ | GUILogIn | CU-02 FA01 |
| PT-05 | Solo en el paso 3 | **Sí, en el paso 1** (y se mantiene en el 3) | GUILogIn | CU-03 paso 1, paso 10 y FA01 |
| PT-06 | ✔ «Volver» | ✔ | GUILogIn | CU-04 FA01 |
| PT-07 | **No** | **Sí** | GUIMainMenu | CU-06 FA01 |
| PT-08 | ✔ «Volver» | ✔ | GUIProfile | CU-07 FA01 |
| PT-09 | **No** | **Sí** | GUIMainMenu | CU-08 FA08, CU-09 FA03, CU-20 FA02 |
| PT-10 | **No** | **Sí** | GUIMainMenu | CU-17 FA04 |
| PT-16 | ✔ «Volver a la sala» | ✔ | GUIRoom | CU-16 |
| PT-11 | **No** | **Sí** | GUIMainMenu | CU-10 paso 6 |
| PT-12 | ✔ «Volver al historial» | ✔ | GUIHistory | CU-11 paso 5 |
| PT-13 | **No** | **Sí** | GUIMainMenu | CU-12 paso 6 |
| PT-14 | **No** | **Por confirmar** (sin él es un callejón sin salida) | GUIMainMenu | Solo el XLSX |
| PT-19 | ✔ «Volver» | ✔ | GUISearchRooms | CU-15 FA01 |
| PT-15 | No (tiene «Salir») | **No aplica**: «Salir» es la salida de la sala | GUIMainMenu | CU-18 |
| PT-17 | No | **No aplica** (son diálogos con Cancelar / Entendido) | — | — |
| PT-18 | No | **No aplica** (transición automática) | — | CU-24 |
| PT-21 a PT-23 | No | **No aplica** (flujo de partida; la salida es «Rendirse») | — | CU-25, CU-28 |
| PT-24 | ✔ «Volver a la sala» | ✔ | GUIRoom | CU-25 FA10 |
| PT-25 | No («Reintentar» / «Salir del juego») | **No aplica** | — | CU-26 |
| PT-26 | No | **No aplica** (se reanuda sin intervención) | — | CU-27 |
| PT-27 | No («Volver a la partida» / «Abandonarla») | **No aplica** | — | CU-27, CU-28 |

**Resumen:** hay que agregarlo con seguridad en **PT-07, PT-09, PT-10, PT-11 y PT-13**, y en el **paso 1 de PT-05**. Faltan por confirmar **PT-03 y PT-14**.

### 5.2 Coherencia de ubicación
El prototipo usa **cuatro posiciones distintas**:

| Patrón | Pantallas | Posición |
|---|---|---|
| Formulario | PT-04, PT-06, PT-19, PT-05 (paso 3) | Botón secundario **a la derecha** del principal, dentro de la tarjeta |
| Consulta en tarjeta | PT-08, PT-12, PT-20 | Solo, **al pie de la tarjeta** |
| Consulta en lista | PT-16 | **Debajo de la lista**, fuera de tarjeta |
| Cierre de partida | PT-24 | Botón **principal centrado** |

El XLSX describe dos posiciones: «Acción secundaria» para los formularios y «Pie de pantalla» para las listas. **Propuesta para confirmar:**
- En los formularios, el botón secundario va junto al principal, como ya está.
- En las pantallas de consulta y de lista (PT-07, 08, 09, 10, 11, 12, 13, 14, 16), va **al pie, alineado a la izquierda y fuera de las tarjetas**, siempre en el mismo sitio.
- PT-24 se queda como excepción, porque ahí regresar es la acción principal.

### 5.3 Coherencia de función y rótulo
- **Función:** en todas las ventanas, «Volver» **cierra la ventana sin aplicar cambios** y regresa a la ventana de origen, según el caso de uso. Coincide en todas las fuentes.
- **Rótulo: no coincide.**

| Fuente | Rótulos |
|---|---|
| Casos de uso y prototipo | «Volver» en todas, salvo «Volver al historial» y «Volver a la sala» |
| XLSX | «Volver al menú» (6 veces), «Volver al perfil», «Volver a iniciar sesión», y «Volver» en 4 ventanas |

Además, la observación de `Common.BackButton` dice que las claves «se mantienen genéricas», pero las claves del XLSX **no lo son** (`BackToMenuButton`, `BackToProfileButton`…). Ver §7.1.

---

## 6. Problemas de navegación detectados

1. **No se puede llegar a PT-07.** CU-06 pide un menú en el avatar con la opción «Perfil». Ni el prototipo ni el XLSX lo tienen.
2. **Callejones sin salida en el prototipo:** PT-07, PT-09, PT-10, PT-11, PT-13 y PT-14 no ofrecen forma de regresar al menú.
3. **PT-03 no tiene salida visible**, aunque CU-01 FA07 describe que el jugador «abandona la ventana». Un usuario que abrió Login por error no puede volver al menú sin identidad.
4. **PT-05 presenta los tres pasos a la vez**, pero la ventana real muestra uno cada vez. El paso 2 **no tiene salida** en CU-03: durante la espera del código, el usuario no puede cancelar. Hay que confirmarlo.
5. **Contradicción interna del prototipo en PT-20:** su metadato dice «Accesible desde el menú y desde la cuenta». CU-05 y el análisis dicen que **solo** se abre desde el menú. Las filas de idioma de `AccountSettings` en el XLSX siguen la versión anterior.
6. **PT-24 siempre dice «Volver a la sala»**, pero CU-25 FA10 lleva a GUIMainMenu **si la sala ya no existe**. Falta decidir qué rótulo se muestra en ese caso.
7. **PT-11 → PT-12:** nada en el prototipo indica que las filas del historial se pueden seleccionar, y CU-11 empieza así.
8. **PT-15:** sigue abierta `PU-08`, si se puede ir a otras secciones sin abandonar la sala. Por eso la única salida es «Salir».

---

## 7. Puntos que deben aclararse antes de desarrollar

1. **Rótulo de «Volver».** ¿«Volver» genérico, como en los casos de uso, o con destino («Volver al menú»), como en el XLSX? Y según la respuesta, ¿las claves son genéricas (`Common.BackButton`) o una por ventana? Hasta ahora la regla del proyecto era que **manda el texto de los casos de uso**.
2. **¿Llevan «Volver» PT-03 (Login) y PT-14 (Salas)?** Si la respuesta es sí, también hay que agregarlo en CU-01 y CU-14, que hoy no lo nombran.
3. **PT-05:** ¿el paso 2 permite cancelar? ¿El rótulo es «Volver» o «Volver a iniciar sesión»? ¿Qué texto tiene la confirmación del paso 14 de CU-03?
4. **Nombre oficial de cada ventana en el código.** Los nombres `GUIxxx` vienen de la captura de ventanas del sistema, y el XLSX usa otros. Casos delicados:
   - `GUISignIn` es la ventana de **Crear cuenta**, pero en inglés *sign in* significa *iniciar sesión*.
   - `GUIGameDetails` y `GUILoadingGame` dicen *Game*, mientras que el XLSX y la base (`MatchSetupService`) dicen *Match*.
   - Las ventanas de la partida (PT-21 a PT-27) todavía **no tienen nombre `GUI`**.
5. **Formatos de es-MX.** La hoja Culturas dice que es-MX usa **espacio** para los miles (`1 480`) y **coma** decimal (`7,4`). Pero .NET 10, con `CultureInfo("es-MX")`, produce **`1,480` y `7.4`** (verificado). Además, el prototipo muestra `1 480` en PT-13 y `7.4 MB` en PT-07, así que tampoco coincide consigo mismo. Hay que decidir si se usa el formato de la cultura (recomendado) y corregir la hoja y el prototipo, o si se fija un formato propio.
6. **Códigos de cultura.** El XLSX define `es-MX` y `en-US`. CU-05 habla de «español e inglés», y el ejemplo de localización ya hecho usa `es` y `en`. Hay que decidir los nombres de los `.resx`: `Strings.en-US.resx` o `Strings.en.resx`.
7. **Formato de las claves.** Con la clave `Pantalla.Control` (con punto), la clase generada convierte el punto en guion bajo (`Common_WindowTitle`), y la regla 3.9 del estándar v7 prohíbe el guion bajo dentro de un identificador. Opciones: claves sin punto (`CommonWindowTitle`), un `.resx` por ventana, o aceptar la excepción. Además, **el XLSX cita el «Estándar v5», reglas 2.2, 2.6 y 2.7**; en el v7 vigente esas reglas son **3.5, 3.9 y 3.10**, y la 2.2 es «Dirección de las dependencias».
8. **Marcadores de posición.** El XLSX usa nombres (`{jugador}`, `{codigo}`, `{tiempo}`), pero `string.Format` de .NET necesita índices (`{0}`). Hay que decidir la convención antes de escribir los `.resx`.
9. **Invitar por correo (CU-16 FA05).** No hay control en el prototipo ni texto en el XLSX. ¿Se dibuja o se retira del alcance?
10. **Cerrar sesión.** Ninguna fuente define cómo dejar de ser Jugador o Invitado sin salir de la aplicación. **No se propone nada**; solo se señala para que el equipo lo confirme.
11. **Menú del avatar (F-7).** Hace falta el texto «Perfil» y definir si ese menú tiene más opciones. **No se debe inventar ninguna**.
12. **Ordinales en PT-24** (§2.13): ¿1, 2, 3 o 1.º, 2.º, 3.º?
13. **Hoja Resumen.** Dice «Etiquetas: 80», pero hay 66 filas de tipo `Label` (80 sale de sumar los 14 `Title`), y «Títulos de ventanas: 1». Hay que revisar cómo se contó.
14. **Ejemplo de localización ya hecho** (`code/examples/Torres.LocalizationExample`). Sus claves (`MainMenu_LogIn`, `ChangeLanguage_Title`…) **no son las del XLSX** (`Common.LogInButton`, `Language.Title`…). Hay que alinearlas antes de usarlo como base.

---

## 8. Resumen de acciones propuestas (pendientes de aprobación)

| Dónde | Acción |
|---|---|
| XLSX | Corregir `Common.ServerErrorTitle` · quitar las dos filas de idioma de `AccountSettings` y `Friends.InviteToRoomButton` · «Macizos» → «Jardineras» (2 filas) · agregar E-1, E-2, E-3, E-4, E-5, E-6 y E-7/E-8 si se localiza la accesibilidad · corregir la hoja Culturas (§7.5) · actualizar las referencias al estándar v7 |
| Prototipo | Agregar «Volver» en PT-07, 09, 10, 11 y 13, y en el paso 1 de PT-05 · menú del avatar con «Perfil» · corregir el metadato de PT-20 · unificar `1 480` y `7.4` con la decisión de §7.5 · unificar los títulos de PT-18 y PT-22 con su rótulo del menú lateral |
| Casos de uso | Solo si se confirma §7.2 y §7.3: nombrar el control de salida en CU-01 y CU-14, y la cancelación en el paso 2 de CU-03 |

**Todas se aplicaron el 16 sep 2026; el resultado está en §9.**

---

## 9. Resolución (16 sep 2026)

El botón «Volver» se resolvió con **el criterio de la versión del prototipo que el equipo usó como referencia**. Esa versión se usó **solo** para este botón. Todo lo demás se resolvió con la información del proyecto: casos de uso, análisis de pantallas, base oficial y stack.

### 9.1 Archivos actualizados

| Archivo | Cambio |
|---|---|
| `Prototipo-Pantallas-Torres.html` | Botón de regreso, menú del avatar, invitación por correo, tooltip de rechazo, formato de miles, ordinales en el resultado, metadato de PT-20 y rótulos del menú lateral |
| `Diccionario-i18n-Torres.xlsx` (**copia corregida dentro del proyecto**; el archivo de Descargas no se tocó) | Pasa de 268 a **321 filas** · hoja nueva **Ventanas** · Culturas, Resumen y Portada al día |
| `Documento-Casos-de-Uso-Torres.md` | Rótulos de regreso con destino · CU-01 FA07 nombra su control · CU-14 gana FA07 · CU-16 nombra «Invitar por correo» · registro al final |
| `Analisis-Pantallas-Prototipo-Torres.md` | v4.1 y §14 · «Cambio de idioma» ya no apunta a PT-08 |

### 9.2 Botón de regreso: criterio aplicado

| Tipo de pantalla | Posición | Rótulo | Pantallas |
|---|---|---|---|
| Formulario | Botón secundario junto a la acción principal, dentro de la tarjeta | «Volver» (`Common.BackButton`) | PT-04, PT-06, PT-19, PT-20 |
| Consulta o lista | **Al pie de la pantalla**, fuera de las tarjetas | **Con destino** | PT-03, PT-07, PT-09, PT-10, PT-11, PT-13, PT-14 → «Volver al menú» · PT-08 → «Volver al perfil» · PT-16 → «Volver a la sala» |
| Flujo por pasos | Botón secundario de cada paso que lo permite | «Volver a iniciar sesión» | PT-05, pasos 1 y 3 (CU-03) |
| Sin cambio | — | — | PT-12 («Volver al historial», en la tarjeta) · PT-24 («Volver a la sala», acción principal) |
| Sin regreso | — | — | PT-01, PT-02, PT-15 («Salir»), PT-17, PT-18, PT-21 a PT-27 |

### 9.3 Cómo quedó cada punto de §7

| # | Punto | Resolución | Fuente |
|---|---|---|---|
| 1 | Rótulo de «Volver» | Criterio de §9.2. Los casos de uso se actualizaron para seguir coincidiendo con el prototipo | Prototipo de referencia |
| 2 | PT-03 y PT-14 | **Sí llevan regreso**, «Volver al menú». PT-14 ya lo tenía en la referencia y en el XLSX; PT-03 lo necesita para cumplir CU-01 FA07 | CU-01 FA07 · referencia |
| 3 | PT-05 | Regreso en los pasos 1 y 3, con el rótulo «Volver a iniciar sesión». El paso 2 no lo lleva porque ni CU-03 ni la referencia lo dan. El texto de confirmación del paso 14 **sigue sin definir**: no se inventó | CU-03 |
| 4 | Nombres de ventana | No se renombró nada. La hoja **Ventanas** del XLSX relaciona Pantalla ↔ PT ↔ `GUIxxx` y anota el caso de `GUISignIn` | Casos de uso · XLSX |
| 5 | Formatos es-MX | Se usa el formato de la cultura de .NET: **`1,480` y `7.4`**. Se corrigieron la hoja Culturas, `Formats` y PT-13. Se añadió la fecha larga | Stack .NET 10 (verificado) |
| 6 | Códigos de cultura | Se mantienen **es-MX y en-US**, como define el XLSX; son compatibles con «español e inglés» de CU-05. El ejemplo de localización se alineará al implementar | XLSX · CU-05 |
| 7 | Formato de las claves | **Se mantiene `Pantalla.Control`**, que es la convención documentada en la Portada. Las referencias al estándar pasan a v7 (3.5, 3.9 y 3.10). Cómo se accede a una clave con punto sin generar un identificador con guion bajo se decide al implementar | Portada · estándar v7 |
| 8 | Marcadores | **Formato compuesto de .NET, `{0}` y `{1}`**, en el texto base y en la traducción; Observaciones dice qué valor ocupa cada uno | Stack .NET |
| 9 | Invitar por correo | Se dibuja en PT-16 como «Invitar por correo» y se añade al XLSX. CU-16 FA05 ya definía el flujo | CU-16 FA05 |
| 10 | Cerrar sesión | **Sin cambio.** Ninguna fuente lo define y no se inventó | — |
| 11 | Menú del avatar | Solo con la opción «Perfil» y solo con cuenta, en todas las barras superiores. Clave `Common.ProfileMenuItem` | CU-06 |
| 12 | Ordinales en PT-24 | `1.º`, igual que PT-11, PT-12 y `Formats.OrdinalRankFormat` | XLSX · prototipo |
| 13 | Hoja Resumen | Recalculada. «Etiquetas» cuenta Label + Title y lo dice; «Formatos» suma los cuatro tipos de formato | — |
| 14 | Ejemplo de localización | **Pendiente para la implementación**: sus claves deben pasar a las del XLSX | — |

### 9.4 Cambios en el XLSX

- **Corregidas:** `Common.ServerErrorTitle` · «Macizos» → «Jardineras» (las claves pasan a `RoundSummary.PlantersColumn` y `Result.PlantersColumn`) · `Formats.ThousandsSeparatorFormat` y `Formats.DecimalSeparatorFormat` · las 19 plantillas con marcadores con nombre pasan a `{0}` · `MainMenu.LanguageButton` pasa a «Idioma · {0}».
- **Retiradas:** `AccountSettings.LanguageSetting`, `AccountSettings.LanguageValue` y `Friends.InviteToRoomButton`.
- **Movidas a `Common`:** «tú», «invitado», «sin conexión» y «retirado» (antes estaban en `Disconnection` y `Result`).
- **Reclasificadas:** `Language.Hint` y `Language.SavedHint` pasan a `ConfigurationText`. Al quitar las filas de idioma de `AccountSettings` era la única categoría que quedaba vacía, y estos dos textos describen justamente el ajuste de idioma.
- **Añadidas:**
  - 25 subtítulos de la barra superior.
  - 19 textos de Friends.
  - `Common.ProfileMenuItem`.
  - `Login.BackToMenuButton`.
  - `Register.UsernameUnavailable` y `PasswordRecovery.InvalidEmail`.
  - `Invitations.RejectToolTip` e `InvitePlayers.InviteByEmailButton`.
  - `Alerts.EmailFailedTitle` y `Alerts.EmailFailedBody`.
  - `History.GuestOpponentsMessage`, el plural de «y 1 invitado».
  - `Disconnection.PlayersLabel`, la etiqueta «Jugadores» de PT-25.
  - `Formats.MatchLongDateFormat` y `Formats.ActionCostFormat`.
- **No se añadieron:** los textos accesibles del HTML («Ilustración de ambientación…» y «Tablero de la partida»). Solo existen para el navegador del prototipo; el cliente MonoGame no los muestra.

### 9.5 Verificación

1. Se dibujaron las 27 pantallas con todas las combinaciones de conmutadores y se buscaron las **321 filas**. Solo quedan sin coincidencia exacta los textos que el prototipo compone dentro de una frase: «7.4 MB», «4 h 12 min», «38 minutos», «y 1 invitado», la lista de ordinales y «Ronda {0} de 3 · Turno {1} de 3». Los seis están dibujados dentro de su frase.
2. En sentido inverso, **ningún texto del prototipo queda sin fila**, salvo los datos de ejemplo (nombres, códigos, números y mensajes de chat).
3. De las **188 cadenas entre comillas** de los casos de uso, todas aparecen en el prototipo, salvo seis falsos positivos: plantillas con números concretos («3 de 4», «3 jugadores · 3 rondas»), un mensaje que el prototipo parte en título y detalle, el nombre de canal «dentro del juego» y los estados en minúscula «abandonada» e «interrumpida».

