# Cómo implementar las GUI internacionalizadas de Torres

**Para qué es esta guía.** El código de `code/src/Torres.Client/` no lleva comentarios, por acuerdo del equipo y conforme a la regla 7.3 del estándar. El *porqué* de cada decisión vive aquí. Sigue los pasos en orden y acabas con lo mismo que ya está construido, sabiendo por qué está así.

**Antes de empezar necesitas:** .NET SDK 10, el diccionario `Diccionario-i18n-Torres.xlsx` y Python 3 con `openpyxl` (solo para el generador).

---

## El mapa: qué construyes y en qué orden

```
1. El proyecto           →  dotnet new mgdesktopgl
2. El .csproj            →  cultura base y satélites
3. Los recursos          →  Strings.resx y Strings.en.resx, generados del Excel
4. TextKeys              →  las claves como constantes, generadas del Excel
5. LocalizedText         →  quien lee el recurso
6. La fuente             →  para que se vean ñ, á y ·
7. LanguageService       →  cambiar de idioma y recordarlo
8. Los controles         →  botones y campos que releen su texto
9. Una pantalla          →  la primera, de principio a fin
10. El juego             →  conectar las pantallas
```

Cada paso funciona con el anterior; ninguno necesita el siguiente. Si te atascas, el paso anterior ya se podía probar.

---

## Paso 1. Crear el proyecto

```bash
dotnet new install MonoGame.Templates.CSharp
cd "code/src"
dotnet new mgdesktopgl -n Torres.Client -o Torres.Client
```

**Por qué DesktopGL y no WindowsDX:** DesktopGL usa OpenGL y SDL2, y corre en macOS, Linux y Windows. WindowsDX usa DirectX y deja fuera a medio equipo. Es la regla 1 de `STACK.md`.

La plantilla apunta a `net9.0`. **Cámbialo a `net10.0`** en el `.csproj`: la regla 3 de `STACK.md` exige que todos los proyectos vayan al mismo target o se rompen las referencias entre ellos.

El proyecto necesita además la herramienta que compila el contenido gráfico:

```bash
dotnet tool restore
```

Si no existe `.config/dotnet-tools.json`, cópialo de otro proyecto MonoGame del repositorio. Sin esa herramienta, `dotnet build` falla con `MSB3073` al llegar a `Content.mgcb`.

---

## Paso 2. Declarar las culturas en el `.csproj`

Dos líneas, y las dos importan:

```xml
<NeutralLanguage>es</NeutralLanguage>
<SatelliteResourceLanguages>en</SatelliteResourceLanguages>
```

| Línea | Qué hace | Qué pasa si falta |
|---|---|---|
| `NeutralLanguage` | Declara que `Strings.resx`, el archivo **sin sufijo**, es español | .NET buscaría un satélite `es` que no existe y haría trabajo de más en cada lectura |
| `SatelliteResourceLanguages` | Copia al `bin` solo la carpeta `en/` | Se copiarían también las traducciones de las bibliotecas de terceros, decenas de carpetas que no usas |

**Lo que NO debes poner** son los metadatos `StronglyTypedFileName`, `StronglyTypedClassName` y compañía, aunque los verás en casi todos los tutoriales. Explicado en el paso 4.

---

## Paso 3. Entender un `.resx` antes de generarlo

Un `.resx` es XML. Quitando la cabecera de esquema, que siempre es igual, cada texto es esto:

```xml
<data name="MainMenu.RoomsButton" xml:space="preserve">
  <value>Salas</value>
</data>
```

- `name` es **la Clave del recurso de tu Excel**, copiada literal, con su punto.
- `value` es la columna **Texto base** en `Strings.resx` y la columna **Traducción** en `Strings.en.resx`.
- `xml:space="preserve"` conserva los espacios; sin él, un texto que termina en espacio lo pierde.

**El sufijo del nombre del archivo ES la cultura.** `Strings.en.resx` es inglés porque se llama `.en.`, no por nada de dentro. Ese es todo el mecanismo.

**Los dos archivos llevan exactamente las mismas claves.** Si una clave falta en el inglés, .NET cae al español para esa clave y nadie se entera: en pantalla sale una frase en español en medio del inglés.

---

## Paso 4. Generar los recursos desde el Excel

No escribas los `.resx` a mano. Son 300 claves × 2 archivos; a mano se desincronizan con el diccionario en la primera corrección.

```bash
python3 code/tools/generar-recursos.py
```

Produce tres archivos: los dos `.resx` y `TextKeys.cs`. **El diccionario manda sobre el `.resx`, nunca al revés.** Si corriges un texto, lo corriges en el Excel y vuelves a ejecutar esto.

El generador hace tres cosas antes de escribir nada, y aborta si alguna falla:

1. Que no haya claves duplicadas ni fuera del patrón `Pantalla.Control`.
2. Que ninguna clave se quede sin texto en alguna de las dos culturas.
3. Que los marcadores `{0}` del texto base y de la traducción coincidan. Un `{0}` perdido no es un texto feo: es una excepción en tiempo de ejecución.

**Por qué el `.csproj` no genera la clase tipada.** Si pones los metadatos `StronglyTyped*`, el compilador genera una clase con una propiedad por clave, y convierte el punto en guion bajo:

```csharp
internal static string MainMenu_RoomsButton { ... }
```

La regla 3.9 del estándar prohíbe el guion bajo dentro de un identificador. Por eso esa clase no se genera y se usa `TextKeys` en su lugar.

---

## Paso 5. `TextKeys` y `LocalizedText`

Son las dos piezas que sustituyen a `{x:Static p:Resources.Title}` del XAML de WPF.

`TextKeys` es solo constantes, agrupadas en una clase anidada por pantalla:

```csharp
internal static class TextKeys
{
    internal static class MainMenu
    {
        internal const string RoomsButton = "MainMenu.RoomsButton";
    }
}
```

**Por qué anidada:** así la ruta de la constante *es* la clave (`TextKeys.MainMenu.RoomsButton` → `"MainMenu.RoomsButton"`), no se repite nada y ningún identificador lleva guion bajo.

**Por qué existe:** para que el compilador te corrija. Si escribes la clave a mano y te equivocas en una letra, compila igual y el fallo aparece cuando alguien abre esa pantalla. Con la constante, el error sale al compilar.

`LocalizedText` es quien va a buscar el texto:

```csharp
internal static class LocalizedText
{
    private static readonly ResourceManager Manager =
        new ResourceManager("Torres.Client.Resources.Strings", typeof(LocalizedText).Assembly);

    internal static string Get(string key)
    {
        return Manager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }
}
```

Tres detalles que tienes que poder explicar:

| Detalle | Por qué |
|---|---|
| `"Torres.Client.Resources.Strings"` | Es `RootNamespace` + carpeta + nombre del archivo. Si mueves el `.resx` de carpeta, esta cadena cambia o deja de encontrar nada |
| `CultureInfo.CurrentUICulture` | Es la propiedad que elige el archivo de recursos. No `CurrentCulture`, que es la de los formatos |
| `?? key` | `GetString` devuelve `null` cuando la clave no existe, **sin lanzar excepción**. Sin el `??`, un texto que falta sale como un hueco en blanco; con él, sale la clave y lo ves de inmediato |

---

## Paso 6. La fuente, o por qué se cierra el juego con una `ñ`

MonoGame convierte la fuente en una imagen **al compilar**, y solo con los caracteres que le digas. Un carácter que no esté se dibuja como `DefaultCharacter`; si no hay `DefaultCharacter`, el juego lanza una excepción al dibujar.

En `Content/Ui.spritefont`:

```xml
<DefaultCharacter>?</DefaultCharacter>
<CharacterRegions>
  <CharacterRegion><Start>&#32;</Start><End>&#255;</End></CharacterRegion>
  <CharacterRegion><Start>&#8212;</Start><End>&#8212;</End></CharacterRegion>
  <CharacterRegion><Start>&#8230;</Start><End>&#8230;</End></CharacterRegion>
</CharacterRegions>
```

- **32–255** es Latín-1: cubre `á é í ó ú ü ñ ¿ ¡ ·`.
- **8212** es `—` y **8230** es `…`. Están fuera de Latín-1 y los usan `Common.WindowTitle` y `Startup.ConnectingStatus`.

**Cómo saber qué te falta**, en vez de descubrirlo cuando se cierre el juego:

```bash
python3 - <<'EOF'
import xml.etree.ElementTree as ET
permitidos = set(range(32, 256)) | {0x2014, 0x2026}
for p in ['Resources/Strings.resx', 'Resources/Strings.en.resx']:
    for d in ET.parse(p).getroot().findall('data'):
        for ch in (d.find('value').text or ''):
            if ord(ch) not in permitidos:
                print(f"U+{ord(ch):04X} {ch!r} en {d.get('name')}")
EOF
```

Cada línea que imprima es una región que te falta añadir. Ejecútalo cada vez que cambien los textos.

**Y la `.ttf` va dentro de `Content/`.** Si pones el nombre de una fuente del sistema, compila en tu máquina y falla en la del otro.

---

## Paso 7. `LanguageService`: cambiar de idioma

El corazón son cuatro líneas:

```csharp
CultureInfo.CurrentUICulture = uiCulture;
CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
CultureInfo.CurrentCulture = formatCulture;
CultureInfo.DefaultThreadCurrentCulture = formatCulture;
```

| Línea | Para qué |
|---|---|
| `CurrentUICulture` | Elige el `.resx`: las **palabras** |
| `CurrentCulture` | Elige el formato: **fechas, horas y números** |
| Las dos `DefaultThread…` | Los hilos que crean las continuaciones `async` de WCF nacen con la cultura del sistema; sin estas dos, una respuesta del servidor se formatearía en el idioma del sistema operativo |

**El error clásico es poner solo las dos primeras.** El resultado no es "las fechas no cambian": es un híbrido. Con el patrón inglés `MMM dd, yyyy` y los nombres de mes españoles sale **`sep 05, 2026`**, que no es ningún idioma.

Fíjate además en qué cultura recibe cada par:

- Los **textos** usan la cultura neutra (`es`, `en`): la región no cambia ninguna palabra, y así `en-GB` o `es-AR` también encuentran su archivo.
- Los **formatos** usan la cultura específica (`es-MX`, `en-US`): ahí la región sí manda. `es` neutra formatea `1.480,5`, a la española, no `1,480.5`.

La preferencia se guarda en `ApplicationData/Torres/language.txt` porque `CU-05 POST-2` dice que **es del equipo, no de la cuenta**: no va a la base de datos ni viaja al servidor.

---

## Paso 8. Controles que releen su texto

Esta es la parte que hace que el cambio se vea **sin reiniciar**, y donde casi todo el mundo se equivoca.

```csharp
internal sealed class TextButton
{
    private readonly string _textKey;

    internal void Draw(Painter painter)
    {
        string text = LocalizedText.Get(_textKey);
        ...
    }
}
```

**El botón guarda la clave, nunca el texto.** Como `Draw` se ejecuta en cada cuadro, al cambiar la cultura el botón ya se dibuja traducido en el siguiente. No hay que avisar a nadie ni recrear la pantalla.

Lo que **no** debes hacer:

```csharp
private readonly string _text = LocalizedText.Get(TextKeys.MainMenu.RoomsButton);   // ✗
```

Ese texto se lee una vez, al construir el botón, y se queda congelado en el idioma que hubiera entonces. Es el mismo defecto que tiene `{x:Static}` en WPF. **Regla: nunca guardes en un campo un texto ya traducido; guarda la clave.**

Y el ancho se mide, no se fija:

```csharp
int textWidth = (int)font.MeasureString(LocalizedText.Get(_textKey)).X;
```

«Idioma de la interfaz» e «Interface language» no ocupan lo mismo. Un ancho fijo funciona en un idioma y corta el otro.

---

## Paso 9. Una pantalla completa, de principio a fin

La más corta es `AccountSettingsScreen` (PT-08). Tiene las cuatro partes que tienen todas:

```csharp
internal sealed class AccountSettingsScreen : Screen
{
    private const int CardHeight = 220;

    private readonly TextButton _deleteAccountButton =
        new TextButton(TextKeys.AccountSettings.DeleteAccountButton, ButtonStyle.Destructive);
    private readonly TextButton _backButton =
        new TextButton(TextKeys.AccountSettings.BackToProfileButton, ButtonStyle.Secondary);

    internal AccountSettingsScreen()
        : base(TextKeys.AccountSettings.HeaderLabel, true)
    {
    }

    internal override void Update(GameTime gameTime, InputState input, SpriteFont font)
    {
        Rectangle card = CardBounds(CardHeight);
        _deleteAccountButton.PlaceAt(card.X + Theme.Margin, card.Bottom - Theme.Margin - Theme.ButtonHeight);
        _backButton.PlaceAt(card.X + Theme.Margin, card.Bottom + 20);

        _deleteAccountButton.Update(input, font);
        _backButton.Update(input, font);

        if (_backButton.WasClicked)
        {
            RequestedScreen = ScreenId.Profile;
        }
    }

    internal override void Draw(Painter painter)
    {
        Vector2 content = DrawCard(painter, CardHeight);
        content.Y += DrawTitle(painter, TextKeys.AccountSettings.DeleteAccountTitle, content);

        painter.DrawWrappedText(
            LocalizedText.Get(TextKeys.AccountSettings.DeleteAccountHint),
            content,
            Theme.CardWidth - (2 * Theme.Margin),
            Theme.MutedText);

        _deleteAccountButton.Draw(painter);
        _backButton.Draw(painter);
    }
}
```

Léelo así:

1. **Los controles son campos `readonly`** y se construyen con su clave. Nada más: ni texto, ni posición.
2. **El constructor pasa a la base la clave del subtítulo** de la barra superior y si esa barra se dibuja.
3. **`Update` coloca, actualiza y navega**, en ese orden. Colocar antes de actualizar importa: el botón calcula su área con el texto del idioma activo, y el clic se comprueba contra esa área.
4. **`Draw` solo dibuja.** Ninguna decisión, ningún texto literal: todo sale de `TextKeys`.

**Por qué la navegación es `RequestedScreen` y no un `Action`:** la regla 6.30 del estándar prohíbe guardar lambdas en campos para invocarlas después. La pantalla apunta a dónde quiere ir y el juego lo lee al final del cuadro. Lo mismo con los clics: el botón expone `WasClicked` en vez de recibir un delegado.

---

## Paso 10. Conectar la pantalla al juego

En `TorresGame`, tres sitios:

```csharp
_screensById.Add(ScreenId.AccountSettings, new AccountSettingsScreen());
```

```csharp
screen.Update(gameTime, _input, _font!);
```

```csharp
if (screen.RequestedScreen is ScreenId requested)
{
    screen.RequestedScreen = null;
    _openScreen = requested;
}
```

El cambio de pantalla ocurre **al final del cuadro**, después de `Update`. Así la pantalla que se abre empieza con el ratón ya soltado y no hereda el clic que la abrió.

Y el título de la ventana también es un texto del diccionario:

```csharp
string title = LocalizedText.Get(TextKeys.Common.WindowTitle);
if (Window.Title != title)
{
    Window.Title = title;
}
```

Se relee cada cuadro, como todo lo demás, y solo se asigna si cambió.

---

## La receta: añadir una pantalla nueva tú mismo

Seis pasos, siempre los mismos:

1. **Busca sus claves en el Excel**, filtrando la columna Pantalla. No inventes textos: si falta uno, se añade primero al diccionario y se regenera.
2. **Añade su valor a `ScreenId`.**
3. **Crea la clase** que hereda de `Screen`, con un campo `readonly` por control, cada uno con su clave.
4. **Escribe `Update`**: colocar, actualizar, navegar.
5. **Escribe `Draw`**: solo dibujo, todo desde `TextKeys`.
6. **Regístrala en `TorresGame`** y dale una entrada desde otra pantalla, o no se podrá abrir.

---

## Cómo comprobar que quedó bien

Cuatro comprobaciones. Las tres primeras no necesitan abrir el juego:

**1. Compila sin advertencias.**
```bash
dotnet build
```

**2. Ningún texto de interfaz está escrito en el código.** Busca literales entre comillas en las pantallas: solo deben aparecer códigos de cultura, nombres de contenido (`"Ui"`, `"Content"`) y símbolos (`"»"`, `"|"`).
```bash
grep -rn '"' --include="*.cs" Screens/ | grep -v TextKeys
```

**3. Todas las claves que usa el código existen en los recursos.** Extrae los `TextKeys.X.Y` del código y compáralos con los `name` de los `.resx`. Si alguna no está, en pantalla saldría la clave en crudo.

**4. Ábrelo y cambia de idioma.** Menú principal → `Language · English` → la otra opción. Tiene que cambiar todo a la vez: la tarjeta, el menú de detrás, la barra superior y el título de la ventana. Si algo se queda en el idioma anterior, ese control guardó el texto en lugar de la clave.

---

## Lo que tienes que poder defender

Si te preguntan, estas son las cinco decisiones y su razón en una línea:

| Decisión | Razón |
|---|---|
| MonoGame y .NET 10 en vez de WPF y .NET Framework | Los dos son solo Windows y el equipo trabaja en macOS y Linux (`STACK.md` §4). El mecanismo que se evalúa, `.resx` + `ResourceManager`, es de .NET y no de WPF, así que se conserva entero |
| Claves con punto en el `.resx` y `TextKeys` en el código | La clase que genera Visual Studio convertiría el punto en guion bajo, prohibido por la regla 3.9 |
| Recursos neutros para el texto, culturas específicas para el formato | La región no cambia ninguna palabra, pero sí cambia la fecha y el separador decimal |
| Los controles guardan la clave y no el texto | `CU-05` paso 3 exige que el cambio se vea en la pantalla ya abierta |
| Las 9 pantallas PT-01…PT-08 y PT-20 | Contienen el mecanismo (CU-05), forman el CRUD completo de `Player` y funcionan sin servidor |
