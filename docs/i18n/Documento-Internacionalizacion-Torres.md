# Internacionalización en Torres: del ejemplo WPF al stack del proyecto

**Fuente:** tutorial *WPF – Localization* de Tutorialspoint.
**Destino:** el cliente de Torres (C# · .NET 10 · MonoGame DesktopGL), `CU-05 Cambiar el idioma de la interfaz`.
**Ejemplo que compila y funciona:** `code/examples/Torres.LocalizationExample/`.

---

## 1. Qué hace el ejemplo original (concepto general)

1. Se crea un proyecto WPF. Visual Studio le pone un `Properties/Resources.resx`.
2. Se crean los archivos de recursos: `Resources.resx` (idioma por defecto), `Resources.en.resx` y `Resources.ru-RU.resx`. **El sufijo del nombre es la cultura**: así .NET sabe qué archivo corresponde a cada idioma.
3. En los tres se ponen **las mismas claves** (`Title`, `Name`, `Address`, `Age`, `OK_Button`, `Cancel_Button`, `Help_Button`), cada una con su traducción.
4. Visual Studio genera una clase, `Properties.Resources`, con una propiedad por clave.
5. El XAML muestra los textos con `{x:Static p:Resources.Title}` en lugar de escribirlos a mano.
6. En `App.xaml.cs` se fija el idioma antes de abrir la ventana: `Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU")`.
7. Al ejecutar, .NET busca el archivo de la cultura elegida. Si una clave no está ahí, usa el archivo por defecto.

**La idea que importa:** los textos salen del código y van a archivos de recursos; el código solo pide «la clave X» y .NET entrega la versión del idioma activo.

---

## 2. Por qué no se puede copiar tal cual

`STACK.md` descarta WPF porque solo funciona en Windows, y el equipo trabaja en macOS y Linux. **Lo que sí sirve es el mecanismo de `.resx`**: no es parte de WPF, es parte de .NET (`System.Resources.ResourceManager`) y funciona igual en .NET 10 y en los tres sistemas.

Lo que hay que cambiar es todo lo de alrededor: el XAML, el generador de Visual Studio y la manera de dibujar el texto.

---

## 3. Tabla de equivalencias

| Ejemplo WPF | En Torres | Por qué cambia |
|---|---|---|
| Proyecto WPF | Proyecto `mgdesktopgl` en `net10.0` | WPF solo existe en Windows |
| `Properties/Resources.resx` | `Resources/Strings.resx` (**español**) | El archivo sin sufijo es el idioma de respaldo; los casos de uso están escritos en español |
| `Resources.en.resx`, `Resources.ru-RU.resx` | `Resources/Strings.en.resx` | `CU-05` solo pide español e inglés. Se usa `en` y no `en-US` para que cualquier variante del inglés lo encuentre |
| Visual Studio genera `Resources.Designer.cs` | El `.csproj` lo genera al compilar con `StronglyTypedFileName` / `StronglyTypedClassName` | El generador de Visual Studio no corre con `dotnet build` en macOS ni en Linux. Así la clase `Strings` sale igual en Rider, VS Code o la terminal |
| `xmlns:p="clr-namespace:…Properties"` | `using Torres.LocalizationExample.Resources;` | MonoGame no tiene XAML |
| `Content="{x:Static p:Resources.Name}"` | `spriteBatch.DrawString(font, Strings.ChangeLanguage_Title, …)` | En MonoGame el texto se dibuja en cada cuadro |
| `Thread.CurrentThread.CurrentUICulture = …` en `App.xaml.cs` | `LanguageService.Apply()`: `CultureInfo.CurrentUICulture` **y** `CultureInfo.DefaultThreadCurrentUICulture` | Las llamadas asíncronas de WCF pueden continuar en otro hilo; sin la segunda línea, ese hilo usaría el idioma del sistema |
| El idioma está fijo en el código | Se lee de `language.txt` en la carpeta de datos del usuario | `CU-05 POST-2`: la preferencia es del equipo, no de la cuenta. No va a la base ni al servidor |
| El texto se ve porque Windows tiene las fuentes | `Content/Ui.spritefont` con el rango **32–255** y una `.ttf` incluida | MonoGame solo dibuja los caracteres que se compilaron en la fuente (ver §4) |
| Cambiar idioma = reiniciar | Se aplica al momento, sin reiniciar | `CU-05` paso 3 lo exige, incluida la pantalla abierta |

---

## 4. Lo que el tutorial no cuenta y en Torres sí importa

**4.1 La fuente tiene que traer los caracteres del español.**
MonoGame convierte la fuente en una imagen al compilar, y solo con los caracteres del rango indicado en `.spritefont`. Si falta `ñ`, `á` o `¿`, el juego **se cierra con una excepción** al dibujar el texto. El ejemplo usa el rango 32–255 (Latín-1), que cubre `á é í ó ú ü ñ ¿ ¡ ·`, y pone `DefaultCharacter` en `?` para que un carácter olvidado salga como `?` en vez de cerrar el juego. Por eso la marca del idioma en uso es `»` y no `✓`: el `✓` está fuera del rango.

**4.2 La fuente viaja con el juego.**
`FontName` apunta a `DejaVuSans.ttf`, que está dentro de `Content/`. Si se pone el nombre de una fuente del sistema, compila en una máquina y falla en otra (regla 4 de `STACK.md`). DejaVu tiene una licencia que permite distribuirla.

**4.3 El cambio inmediato sale de leer el texto en cada cuadro.**
`{x:Static}` lee el valor una sola vez. En el ejemplo, cada botón recibe **una función que devuelve el texto** (`() => Strings.MainMenu_Rooms`), no el texto ya leído. Como `Draw` se ejecuta en cada cuadro, al cambiar la cultura la pantalla abierta se actualiza sola. **Regla para el equipo: no guardar en un campo un texto ya traducido.**

**4.4 Los tamaños se miden, no se fijan.**
«Idioma de la interfaz» e «Interface language» no miden lo mismo. `MenuButton` calcula su área con `font.MeasureString` sobre el texto traducido.

**4.5 Textos con datos: se usa un hueco, no se concatena.**
`MainMenu_Language` vale `Idioma · {0}` y `Language · {0}`, y se completa con `string.Format`. Si se concatenara, el orden de las palabras quedaría fijo, y no en todos los idiomas va igual.

**4.6 Los nombres de los idiomas no se traducen.**
«Español» y «English» se muestran siempre así, sin importar el idioma activo (igual que en `CU-05` paso 1). Por eso están en `LanguageService.Available` y no en los `.resx`.

**4.7 El servidor no debe mandar textos.**
Como el idioma solo existe en el cliente, el servidor no sabe qué idioma usa cada jugador. Si un error del servidor viaja como texto, llegará en un solo idioma. **Lo correcto es que el contrato WCF devuelva un código** (por ejemplo, un `enum`) y que el cliente lo traduzca con su `.resx`. Esto toca los contratos compartidos, así que conviene decidirlo antes de escribirlos.

---

## 5. Estructura del ejemplo

```
code/examples/Torres.LocalizationExample/
├── Torres.LocalizationExample.csproj   net10.0, NeutralLanguage=es, genera la clase Strings
├── Program.cs
├── TorresGame.cs                       carga la preferencia antes de crear pantallas
├── Resources/
│   ├── Strings.resx                    español (respaldo)
│   └── Strings.en.resx                 inglés
├── Localization/
│   └── LanguageService.cs              idioma activo, cambio y archivo de preferencia
├── Screens/
│   ├── IScreen.cs
│   ├── MenuButton.cs                   texto como función, medida con MeasureString
│   ├── MainMenuScreen.cs               GUIMainMenu (opciones citadas en los CU)
│   └── ChangeLanguageScreen.cs         GUIChangeLanguage (CU-05)
└── Content/
    ├── Content.mgcb
    ├── Ui.spritefont                   rango 32–255, DefaultCharacter "?"
    └── DejaVuSans.ttf
```

**Claves de recurso** (nombres en inglés, como pide el estándar de codificación para los identificadores):

| Clave | `Strings.resx` | `Strings.en.resx` |
|---|---|---|
| `MainMenu_LogIn` | Iniciar sesión | Log in |
| `MainMenu_Rooms` | Salas | Rooms |
| `MainMenu_Ranking` | Ranking | Ranking |
| `MainMenu_Language` | Idioma · {0} | Language · {0} |
| `MainMenu_Exit` | Salir | Exit |
| `ChangeLanguage_Title` | Idioma de la interfaz | Interface language |
| `ChangeLanguage_Hint` | Se aplica de inmediato a todas las pantallas. | Applies immediately to every screen. |
| `ChangeLanguage_Note` | La preferencia se guarda en este equipo, no en tu cuenta. | This preference is saved on this computer, not in your account. |
| `Common_Back` | Volver | Back |

Los textos en español son los de `CU-05` y `CU-01`. **Los textos en inglés son una propuesta mía**: ningún documento del proyecto los define todavía.

Convención de nombres propuesta: `Pantalla_Elemento`, y `Common_` para lo que se repite en varias pantallas.

---

## 6. Cómo ejecutarlo

```bash
cd code/examples/Torres.LocalizationExample
dotnet run
```

En el menú, «Idioma · Español» abre la pantalla de idioma. Al elegir «English», la pantalla cambia al momento. «Back» regresa al menú, que ya aparece en inglés. Al cerrar y volver a abrir, el juego arranca en el último idioma elegido.

Para empezar de cero, borra el archivo de preferencia:
- macOS: `~/Library/Application Support/Torres/language.txt`
- Linux: `~/.config/Torres/language.txt`
- Windows: `%APPDATA%\Torres\language.txt`

---

## 7. Qué se comprobó

En macOS arm64 con .NET SDK 10.0.400 y MonoGame 3.8.5.1:

| Comprobación | Resultado |
|---|---|
| `dotnet build` sin Visual Studio | Compila; genera `Strings.Designer.cs` y la carpeta satélite `en/` |
| Búsqueda por cultura | `es` → español · `en` → inglés · `en-US` → inglés · `fr` → español (respaldo) |
| Dibujo de `ó`, `ñ`, `·` | Se ven bien en la ventana |
| Arranque con la preferencia `en` guardada | El menú aparece en inglés |

**No comprobado:** Linux y Windows (lo mismo que el resto del stack), y el cambio con clic grabado de principio a fin. El cambio se probó por partes: la búsqueda por cultura y el arranque con la preferencia guardada.

---

## 8. Pendiente de decidir por el equipo

1. **Textos en inglés definitivos.** Los del ejemplo son una propuesta.
2. **¿Los errores del servidor viajan como código?** (§4.7). Afecta los contratos compartidos.
3. **¿En qué idioma arranca la primera vez?** El ejemplo arranca en español. Otra opción es usar el idioma del sistema operativo si es inglés. `CU-05` no lo dice.
4. **¿Dónde vivirán los `.resx` en la solución real?** Si solo el cliente muestra textos, basta con el proyecto de cliente. No deben ir al ensamblado de contratos.
