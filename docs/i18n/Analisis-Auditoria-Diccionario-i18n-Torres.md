# Auditoría del diccionario de internacionalización de Torres

**Archivo auditado:** `Diccionario-i18n-Torres.xlsx` (copia del proyecto, 321 filas)
**Fecha:** 17 de septiembre de 2026
**Método:** verificación programática de las 321 filas de la hoja Diccionario, cruzada con las hojas Culturas, Ventanas y Resumen, con `Prototipo-Pantallas-Torres.html`, con `Documento-Casos-de-Uso-Torres.md`, con `Documento-Internacionalizacion-Torres.md` y con los `.resx` de `Act inter/TorresIdiomas` y de `code/examples/Torres.LocalizationExample`.
**Alcance:** integridad estructural, reglas de no duplicación, coherencia con las fuentes del proyecto y riesgo de distribución. No se modificó el XLSX.

---

## 1. Resultado en una línea

El diccionario está **estructuralmente impecable** —cero defectos en las ocho comprobaciones mecánicas— y tiene **dos defectos de contenido que sí hay que corregir**, uno de ellos porque contradice un caso de uso aprobado. Lo demás son consolidaciones recomendables y anotaciones pendientes.

---

## 2. Comprobaciones superadas

| # | Comprobación | Resultado |
|---|---|---|
| A | Claves duplicadas | **0** — las 321 claves son únicas |
| B | Celdas obligatorias vacías (Clave, Texto base, Traducción, Cultura, Contexto, Tipo, Control) | **0** en las siete columnas |
| C | Marcadores `{0}` descuadrados entre base y traducción | **0** sobre 24 filas con marcador; las 24 lo explican en Observaciones |
| D | Primer segmento de la clave ≠ su Pantalla | **0** |
| E | Claves fuera del patrón `PascalCase.PascalCase` | **0** |
| F | Controles fuera de patrón (`_camelCase`; `PascalCase` en Formats) | **0** |
| J | Capitalización incoherente entre base y traducción | **0** |
| N | Pantallas cruzadas con la hoja Ventanas | **30 de 30**, sin sobrantes ni faltantes |
| O | Conteos de la hoja Resumen contra el conteo real | **0 descuadres** |

**La regla 2 está bien aplicada** (mismo texto español, distinto significado → claves distintas), que es el error más caro y más frecuente:

| Texto base | Traducciones distintas, correctamente separadas |
|---|---|
| Salir | `Exit` (aplicación) · `Leave` (sala) |
| Entrar | `Log in` · `Enter` · `Join` |
| Eliminar | `Remove` (amigo) · `Delete` (cuenta) |
| Invitado | `Guest` (rol) · `Invited` (ya invitado) |

---

## 3. Hallazgos

### H-01 · ALTO · `Language.SpanishOption` traduce un nombre de idioma

| | |
|---|---|
| **Fila** | `Language.SpanishOption` — base `Español` → traducción **`Spanish`** |
| **Contradice** | `CU-05`, paso 1: *"la lista con los dos idiomas disponibles, «Español» e «English»"* — literal, sin traducir |
| **Contradice también** | `Documento-Internacionalizacion-Torres.md` §4.6: *"Los nombres de los idiomas no se traducen"* |
| **Es incoherente con** | Su fila hermana `Language.EnglishOption` (`English` → `English`, observación *"Se muestra igual en ambas culturas"*) |
| **Y con el código** | `LanguageService.Available` mantiene los nombres **fuera** de los `.resx` precisamente para que no se traduzcan |

**Por qué importa:** con la interfaz en inglés, quien solo hable español vería la opción escrita como `Spanish` y no reconocería su propio idioma. Es la razón por la que el estándar de la industria muestra cada idioma en su propia lengua.

**Corrección:** traducción = `Español`; Observaciones = *"El nombre del idioma se muestra siempre en su propia lengua (CU-05 paso 1); no se traduce."*

---

### H-02 · ALTO · Cuatro filas `*.BackButton` redundantes, y el propio diccionario lo dice

| Clave | Base | Traducción |
|---|---|---|
| `Common.BackButton` | Volver | Back |
| `Language.BackButton` | Volver | Back |
| `Register.BackButton` | Volver | Back |
| `GuestAccess.BackButton` | Volver | Back |
| `CreateRoom.BackButton` | Volver | Back |

La observación de `Common.BackButton` dice: *"Botón secundario de los formularios (**Idioma, Crear cuenta, Jugar como invitado, Crear sala**)"* — exactamente las cuatro pantallas que además tienen su propia clave. **El archivo se contradice a sí mismo**: declara que esas cuatro pantallas reutilizan la clave compartida y acto seguido les da una propia.

**Consecuencia si no se corrige:** cinco recursos en el `.resx` para un solo botón; cambiar "Volver" obliga a tocar cinco lugares y basta olvidar uno para que la interfaz quede inconsistente (regla 3: una clave = un texto = un lugar donde cambiarlo).

**Corrección:** eliminar las cuatro filas de pantalla. 321 → **317 filas**.

---

### H-03 · MEDIO · «Volver al menú» repetido en siete pantallas y «Volver a la sala» en dos

Las 17 filas de la familia `Back*` se reparten así:

| Texto | Nº de claves | Veredicto |
|---|---|---|
| Volver | 5 | **H-02**: consolidar en `Common.BackButton` |
| Volver al menú | **7** (`Login`, `Profile`, `Friends`, `Invitations`, `History`, `GlobalRanking`, `Rooms`) | Consolidar en `Common.BackToMenuButton` |
| Volver a la sala | **2** (`InvitePlayers`, `Result`) | Consolidar en `Common.BackToRoomButton` |
| Volver a iniciar sesión · Volver al perfil · Volver al historial | 1 cada una | **Correctas**: destino único, se quedan por pantalla |

Texto idéntico, traducción idéntica, significado idéntico: es el caso de manual de la regla 1. El criterio del equipo —*"el destino va en el rótulo"*— **se conserva intacto**, porque el destino sigue siendo el mismo en las siete.

**Contrapartida honesta:** el diccionario también sirve de lista de cobertura por pantalla, y al consolidar, esas siete pantallas pierden una fila en la hoja Resumen. Se compensa registrando la reutilización en Observaciones, que es justo lo que ya hace `Common.RetryButton`.

**Corrección:** 317 → **310 filas**.

---

### H-04 · MEDIO · Dos duplicados semánticos contra claves de `Common` ya existentes

| Clave de pantalla | Clave `Common` equivalente | Diferencia real |
|---|---|---|
| `Room.GuestBadge` (`Invitado` → `Guest`) | `Common.GuestRole` (`Invitado` → `Guest`) | **Ninguna**: texto y traducción idénticos |
| `Room.DisconnectedStatus` (`Sin conexión` → `Disconnected`) | `Common.DisconnectedTag` (`sin conexión` → `disconnected`) | Solo la capitalización |

`Common.OfflineStatus` (`Sin conexión` → `No connection`) **no** entra aquí: se refiere a la conexión propia con el servidor, no al estado de otro jugador. Su traducción distinta es correcta y es un acierto.

**Decisión pendiente del equipo:** unificar, o documentar en Observaciones por qué la sala necesita una clave propia.

---

### H-05 · MEDIO · Las claves del código no corresponden a ninguna clave del diccionario

Los dos `.resx` del repositorio (`Act inter/TorresIdiomas` y el ejemplo) usan **9 claves, y ninguna de las 9 existe en el XLSX**. Ni siquiera coincide el nombre de la pantalla: el código la llama `ChangeLanguage` y el diccionario `Language`.

| Clave en el código | Clave del diccionario | Estado |
|---|---|---|
| `MainMenu_LogIn` | `Common.LogInButton` | Distinta clave **y distinta pantalla**: en el diccionario el acceso vive en la barra superior, no en el menú |
| `MainMenu_Rooms` | `MainMenu.RoomsButton` | Falta el sufijo del tipo |
| `MainMenu_Ranking` | `MainMenu.RankingButton` | Falta el sufijo del tipo |
| `MainMenu_Exit` | `MainMenu.ExitButton` | Falta el sufijo del tipo |
| `MainMenu_Language` | `MainMenu.LanguageButton` | Falta el sufijo del tipo |
| `ChangeLanguage_Title` | `Language.Title` | Pantalla distinta |
| `ChangeLanguage_Hint` | `Language.Hint` | Pantalla distinta |
| `ChangeLanguage_Note` | `Language.SavedHint` | Pantalla **y** elemento distintos |
| `Common_Back` | `Common.BackButton` | Falta el sufijo del tipo |

**Causa:** el ejemplo se escribió a partir del tutorial de WPF, antes de que existiera el diccionario. Era conocido como pendiente; queda aquí cuantificado y con el mapeo resuelto.

**Corrección:** al implementar, las claves salen del XLSX. El diccionario manda sobre el `.resx`, nunca al revés (criterio 2 de mantenibilidad).

---

### H-06 · BAJO · Cinco filas no traducidas sin justificación escrita

De las 16 filas cuya traducción es idéntica al texto base, 11 explican por qué. Estas no:

| Clave | Texto | Observación actual |
|---|---|---|
| `Register.UsernameLabel` | Username | *(vacía)* — su hermana `Login.UsernameLabel` sí dice *"Término técnico, se mantiene igual"* |
| `Profile.UsernameLabel` | Username | *(vacía)* |
| `GlobalRanking.RowNumberColumn` | # | *(vacía)* |
| `Result.TotalColumn` | Total | *(vacía)* — coincide en ambos idiomas, pero no está dicho |
| `MainMenu.RankingButton` | Ranking | Habla de otra cosa (*"Deshabilitada sin identidad"*) |

**Por qué importa:** una traducción vacía de contenido y una traducción deliberadamente igual se ven idénticas en la hoja. Sin la nota, la siguiente revisión no puede distinguir "ya está resuelto" de "se nos olvidó".

---

### H-07 · BAJO · El inglés destapa una inconsistencia del **prototipo**, no del diccionario

El prototipo usa **«Entrar» 6 veces y «Unirse» 1 vez** para acciones equivalentes, y «Entrar» acaba traducido de tres formas según la pantalla:

| Clave | Base | Traducción |
|---|---|---|
| `Login.LogInButton` | Entrar | `Log in` |
| `GuestAccess.EnterButton` | Entrar | `Enter` |
| `Invitations.JoinButton` | Entrar | `Join` |
| `Rooms.JoinButton` | **Unirse** | `Join` |

Las cuatro traducciones son correctas para su contexto: el diccionario es **fiel al prototipo**. El problema está río arriba, en la terminología del diseño: dos verbos distintos para entrar a una sala. Traducir obligó a mirar cada texto de cerca y por eso apareció — es un efecto secundario habitual y valioso de la internacionalización.

**Corrección sugerida (fuera del alcance de esta actividad):** unificar en el prototipo, y propagar al diccionario y a los casos de uso.

---

### H-08 · INFORMATIVO · Riesgo de corte por expansión del inglés

Para las pruebas de la etapa 6 y para elegir el 30 % de GUI. El inglés suele ser más corto que el español, así que los pocos casos en que crece son los que hay que vigilar:

| Clave | Base | Traducción | Factor |
|---|---|---|---|
| `Match.CaterpillarActionButton` | Oruga | Caterpillar | **2.20×** ← el peor botón del juego |
| `Match.YourCaterpillarsLabel` | Tus orugas | Your caterpillars | 1.70× |
| `Alerts.RemovedFromRoomTitle` | Te han sacado de la sala | You've been removed from the room | 1.38× |
| `Friends.EmptyStateTitle` | Todavía no tienes amigos | You don't have any friends yet | 1.26× |
| `Register.EmailAlreadyExists` | Ya hay una cuenta con este correo. | An account with this email already exists. | 1.24× |

`Match.CaterpillarActionButton` es un botón de acción de la partida, junto a otros de coste fijo: **es la prueba de corte obligatoria**. En MonoGame no hay layout automático, así que el ancho debe salir de `font.MeasureString` sobre el texto ya traducido (§4.4 del documento de i18n), nunca de un número fijo.

---

## 4. Hallazgos anteriores que siguen abiertos

De la sesión de análisis de culturas, no resueltos por esta auditoría:

| | Hallazgo |
|---|---|
| **HC-01** | El XLSX declara `es-MX`/`en-US` (culturas específicas) y el código usa `es`/`en` (neutras). Verificado en .NET: `es` formatea `1.480,5`, no `1,480.5`. Recomendación: conservar `es-MX`/`en-US` como culturas objetivo, mantener los `.resx` neutros para los **textos** y usar la cultura específica para los **formatos** |
| **HC-02** | `LanguageService.Apply()` fija solo `CurrentUICulture`. Sin `CurrentCulture`, **las 10 filas `Formats` del diccionario no funcionarían**: la fecha no pasaría de `05 sep 2026` a `Sep 05, 2026` |
| **HC-03** | `Act inter/TorresIdiomas` apunta a `net9.0`; la regla 3 de `STACK.md` exige `net10.0` |

---

## 5. Plan de corrección propuesto

| Orden | Acción | Filas | Requiere decisión |
|---|---|---|---|
| 1 | H-01: `Language.SpanishOption` → `Español` | 321 | No: lo fija `CU-05` |
| 2 | H-02: eliminar las 4 filas `*.BackButton` | → 317 | No: lo fija el propio archivo |
| 3 | H-03: consolidar `Volver al menú` (7→1) y `Volver a la sala` (2→1) | → 310 | **Sí** |
| 4 | H-04: unificar o justificar `Room.GuestBadge` y `Room.DisconnectedStatus` | → 308 o 310 | **Sí** |
| 5 | H-06: escribir las 5 observaciones que faltan | 310 | No |
| 6 | HC-01: fijar el criterio de culturas específicas para formatos | — | **Sí** |
| 7 | H-05: usar las claves del XLSX al implementar | — | No |
| 8 | H-07: unificar «Entrar»/«Unirse» en el prototipo | — | **Sí**, y fuera de esta actividad |

---

## 6. Conclusión

El diccionario es sólido: las comprobaciones que suelen fallar —claves duplicadas, marcadores rotos, huecos de traducción, desalineación con el prototipo— salieron **todas limpias**, y la separación por significado está bien hecha. Los dos hallazgos altos son puntuales y su corrección está determinada por fuentes ya aprobadas del proyecto, no por criterio de nadie.

Tras aplicar el plan, el diccionario queda en **310 filas** y puede servir directamente de fuente para generar `Strings.resx` y `Strings.en.resx`.

---

## 7. Aplicación de las correcciones · 17 de septiembre de 2026

Aplicadas sobre `Diccionario-i18n-Torres.xlsx`. **321 → 310 filas.**

| Hallazgo | Acción aplicada | Efecto |
|---|---|---|
| H-01 | `Language.SpanishOption`: traducción `Spanish` → **`Español`**, con la observación que cita `CU-05` paso 1. La misma observación se puso en `Language.EnglishOption` | 2 filas |
| H-02 | Eliminadas `Language.BackButton`, `Register.BackButton`, `GuestAccess.BackButton` y `CreateRoom.BackButton` | −4 filas |
| H-03 | Las 7 `*.BackToMenuButton` (Login, Profile, Friends, Invitations, History, GlobalRanking, Rooms) → **`Common.BackToMenuButton`**; las 2 `*.BackToRoomButton` (InvitePlayers, Result) → **`Common.BackToRoomButton`**. Las pantallas reutilizadoras quedan registradas en Observaciones | −7 filas |
| H-04 | **Documentado, no unificado** — ver abajo | 4 filas |
| H-06 | Escritas las observaciones de `Register.UsernameLabel`, `Profile.UsernameLabel`, `GlobalRanking.RowNumberColumn`, `Result.TotalColumn` y `MainMenu.RankingButton` | 5 filas |
| HC-01 | Añadido a la hoja Culturas el criterio de implementación: textos en recursos neutros, formatos con la cultura específica, y la advertencia de fijar `CurrentUICulture` **y** `CurrentCulture` | — |

### Por qué H-04 cambió de veredicto

La recomendación inicial era unificar. **La revisión del prototipo la desmintió** y se siguió al prototipo, que es la fuente de verdad:

```html
<span class="chip guest">Invitado</span>      ← sala (PT-16), mayúscula
<span class="chip bad">Sin conexión</span>    ← sala (PT-16), mayúscula
<span class="tag g">invitado</span>           ← otras pantallas, minúscula
<span class="tag y">sin conexión</span>       ← otras pantallas, minúscula
```

Son dos elementos distintos del diseño (`chip` y `tag`) con capitalización distinta, no una duplicación. Unificarlos habría roto la fidelidad con el prototipo. Se escribieron en cambio cuatro observaciones cruzadas —en `Room.GuestBadge`, `Room.DisconnectedStatus`, `Common.GuestRole` y `Common.OfflineStatus`— para que la próxima revisión no vuelva a abrir el caso.

### Consistencia del libro tras la edición

| Ajuste | Detalle |
|---|---|
| Tabla `DiccionarioI18n` | Rango `A1:I322` → `A1:I311`, con su autofiltro |
| Bandeado y altura de fila | Rehechos sobre las 310 filas |
| Hoja Resumen, por tipo | `Button` 71 → **60** |
| Hoja Resumen, por pantalla | Reordenada y recontada: Common 20 → **22**, Friends 29 → 28, Login 13 → 12, Profile 13 → 12, History 15 → 14, Rooms 14 → 13, GlobalRanking 9 → 8, Invitations 8 → 7, GuestAccess 8 → 7, Language 7 → 6, InvitePlayers 7 → 6, Result 10 → 9, CreateRoom 10 → 9, Register 11 → 10 |
| Checklist de la actividad | «Botones: 71 elementos» → «60 elementos». Las once categorías pedidas siguen cubiertas |
| Portada | «los 321 elementos» → «los 310 elementos» |

### Verificación posterior

Se repitió la batería completa sobre el archivo ya corregido. **16 de 16 comprobaciones superadas**: 310 filas, claves únicas, sin celdas obligatorias vacías, marcadores cuadrados, claves y controles en su patrón, clave alineada con su pantalla, 30/30 pantallas cruzadas con Ventanas, hoja Resumen cuadrada por tipo y por pantalla, rango de la tabla correcto, sin `*.BackButton` de pantalla, sin textos «Volver…» repetidos, y **ninguna traducción idéntica al texto base sin justificación escrita**.

### Sigue abierto

| | |
|---|---|
| **HC-02** | `LanguageService.Apply()` no fija `CurrentCulture`. Se corrige en el código, en la etapa de implementación |
| **HC-03** | `Act inter/TorresIdiomas` apunta a `net9.0`; debe pasar a `net10.0` |
| **H-05** | Las 9 claves de los `.resx` se sustituyen por las del diccionario al implementar. El mapeo está en §3 |
| **H-07** | «Entrar» / «Unirse» en el prototipo: decisión de diseño, fuera del alcance de esta actividad |
