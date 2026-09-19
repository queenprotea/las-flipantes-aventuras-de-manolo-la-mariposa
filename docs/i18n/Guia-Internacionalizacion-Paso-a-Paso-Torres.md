# Guía paso a paso: internacionalización en Torres

Esta guía reproduce el ejemplo *WPF – Localization* de Tutorialspoint con las herramientas de Torres: **C#, .NET 10 y MonoGame DesktopGL**. La haces tú, desde una carpeta vacía.

Tiene dos partes:

- **Parte 1:** el mismo ejemplo del tutorial. El idioma se fija en el código y la pantalla muestra los textos.
- **Parte 2:** lo que pide `CU-05`. El idioma se cambia desde el juego, el cambio se ve al momento y la preferencia se guarda en el equipo.

Al terminar cada paso hay un **✔ Comprobación** para saber que vas bien.

> El proyecto terminado está en `code/examples/Torres.LocalizationExample/` por si te atoras. Intenta no mirarlo hasta que termines.

---

## Antes de empezar

```bash
dotnet --version
```

✔ Debe imprimir `10.0.x`.

```bash
dotnet new list mgdesktopgl
```

✔ Debe aparecer *MonoGame Cross-Platform OpenGL Desktop Application*. Si no aparece:

```bash
dotnet new install MonoGame.Templates.CSharp
```

---

# PARTE 1 — El ejemplo del tutorial

## Paso 1. Crear el proyecto

En el tutorial se crea un proyecto WPF. Aquí se crea uno de MonoGame, porque WPF solo funciona en Windows.

```bash
mkdir -p "/Users/qp/torres proyecto/code/practica"
cd "/Users/qp/torres proyecto/code/practica"
dotnet new mgdesktopgl -n TorresIdiomas -o TorresIdiomas
cd TorresIdiomas
```

Abre `TorresIdiomas.csproj` y cambia `net9.0` por `net10.0` (regla 3 de `STACK.md`).

✔ `dotnet run` abre una ventana azul. Ciérrala.

## Paso 2. Decir cuál es el idioma por defecto

En el tutorial, el idioma por defecto es el de `Resources.resx`. Aquí será el español. Dentro del primer `<PropertyGroup>` del `.csproj`, debajo de `<TargetFramework>`, agrega:

```xml
    <NeutralLanguage>es</NeutralLanguage>
    <SatelliteResourceLanguages>en</SatelliteResourceLanguages>
```

- `NeutralLanguage` avisa a .NET que el archivo sin sufijo está en español.
- `SatelliteResourceLanguages` hace que solo se copie la carpeta del inglés y no las de otros idiomas que traen los paquetes.

## Paso 3. Crear el archivo de recursos en español

Equivale a `Resources.resx` del tutorial. Crea la carpeta `Resources` y dentro el archivo **`Strings.resx`** con este contenido:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <data name="MainMenu_LogIn" xml:space="preserve"><value>Iniciar sesión</value></data>
  <data name="MainMenu_Rooms" xml:space="preserve"><value>Salas</value></data>
  <data name="MainMenu_Ranking" xml:space="preserve"><value>Ranking</value></data>
  <data name="MainMenu_Exit" xml:space="preserve"><value>Salir</value></data>
  <data name="MainMenu_Language" xml:space="preserve"><value>Idioma · {0}</value></data>
  <data name="ChangeLanguage_Title" xml:space="preserve"><value>Idioma de la interfaz</value></data>
  <data name="ChangeLanguage_Hint" xml:space="preserve"><value>Se aplica de inmediato a todas las pantallas.</value></data>
  <data name="ChangeLanguage_Note" xml:space="preserve"><value>La preferencia se guarda en este equipo, no en tu cuenta.</value></data>
  <data name="Common_Back" xml:space="preserve"><value>Volver</value></data>
</root>
```

Cada `<data>` es una clave (`name`) con su texto (`value`). Los nombres de las claves van en inglés, como pide el estándar de codificación para los identificadores. Los textos son los de `CU-05` y `CU-01`.

> En Rider puedes abrir el `.resx` con su editor de tabla en lugar de escribir el XML a mano.

## Paso 4. Crear el archivo en inglés

Equivale a `Resources.en.resx`. En la misma carpeta, crea **`Strings.en.resx`**. Debe tener **las mismas claves** y los textos traducidos:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader>
  <data name="MainMenu_LogIn" xml:space="preserve"><value>Log in</value></data>
  <data name="MainMenu_Rooms" xml:space="preserve"><value>Rooms</value></data>
  <data name="MainMenu_Ranking" xml:space="preserve"><value>Ranking</value></data>
  <data name="MainMenu_Exit" xml:space="preserve"><value>Exit</value></data>
  <data name="MainMenu_Language" xml:space="preserve"><value>Language · {0}</value></data>
  <data name="ChangeLanguage_Title" xml:space="preserve"><value>Interface language</value></data>
  <data name="ChangeLanguage_Hint" xml:space="preserve"><value>Applies immediately to every screen.</value></data>
  <data name="ChangeLanguage_Note" xml:space="preserve"><value>This preference is saved on this computer, not in your account.</value></data>
  <data name="Common_Back" xml:space="preserve"><value>Back</value></data>
</root>
```

**El sufijo `.en` es lo que importa**: así sabe .NET que ese archivo es para el inglés. Usa `en` y no `en-US`, así cualquier variante del inglés lo encuentra.

⚠ Escribe el nombre con las mayúsculas exactas (`Strings`, no `strings`). En Mac funciona de las dos formas, pero en Linux no (regla 4 de `STACK.md`).

## Paso 5. Hacer que se genere la clase `Strings`

En el tutorial, Visual Studio genera sola la clase `Properties.Resources`. Con `dotnet build` en Mac eso **no pasa**, así que se pide en el `.csproj`. Agrega este bloque antes del `<ItemGroup>` que tiene los `PackageReference`:

```xml
  <ItemGroup>
    <EmbeddedResource Update="Resources\Strings.resx">
      <StronglyTypedFileName>$(IntermediateOutputPath)Strings.Designer.cs</StronglyTypedFileName>
      <StronglyTypedLanguage>CSharp</StronglyTypedLanguage>
      <StronglyTypedNamespace>TorresIdiomas.Resources</StronglyTypedNamespace>
      <StronglyTypedClassName>Strings</StronglyTypedClassName>
    </EmbeddedResource>
  </ItemGroup>
```

```bash
dotnet build
ls obj/Debug/net10.0/Strings.Designer.cs
ls bin/Debug/net10.0/en
```

✔ Existe `Strings.Designer.cs` (tu clase generada) y la carpeta `en/` con `TorresIdiomas.resources.dll` (las traducciones al inglés).

## Paso 6. Crear la fuente con los caracteres del español

**Este paso no está en el tutorial** porque WPF usa las fuentes de Windows. MonoGame no: convierte la fuente en una imagen al compilar, y **solo con los caracteres que le indiques**. Si falta la `ñ`, el juego se cierra al intentar dibujarla.

**6.1** Crea la plantilla de fuente:

```bash
dotnet new mgsf -n Ui -o Content
```

**6.2** Copia una fuente con licencia libre dentro de `Content/`. DejaVu Sans ya está en tu Mac:

```bash
cp /Users/qp/Library/Python/3.9/lib/python/site-packages/matplotlib/mpl-data/fonts/ttf/DejaVuSans.ttf Content/
```

**6.3** Abre `Content/Ui.spritefont` y cambia tres cosas:

```xml
<FontName>DejaVuSans.ttf</FontName>
```
Pones el archivo y no el nombre de una fuente instalada, porque la otra persona del equipo quizá no la tenga.

```xml
<Size>20</Size>
```

```xml
<DefaultCharacter>?</DefaultCharacter>
```
Esta línea viene comentada; quítale el `<!--` y el `-->`. Así, un carácter que falte sale como `?` en vez de cerrar el juego.

Y deja `<CharacterRegions>` **solo** con este rango (borra los de japonés y los demás):

```xml
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#255;</End>
      </CharacterRegion>
    </CharacterRegions>
```

El rango 32–255 incluye `á é í ó ú ü ñ ¿ ¡ ·`.

**6.4** La plantilla **no registra** la fuente en el contenido. Agrega esto al final de `Content/Content.mgcb`:

```
#begin Ui.spritefont
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/processorParam:PremultiplyAlpha=True
/processorParam:TextureFormat=Compressed
/build:Ui.spritefont
```

✔ `dotnet build` muestra `Building Font …DejaVuSans.ttf` y termina sin errores.

## Paso 7. Mostrar los textos y fijar el idioma

Aquí se juntan los dos últimos pasos del tutorial:

- `Title="{x:Static p:Resources.Title}"` en el XAML pasa a ser `Strings.ChangeLanguage_Title` dentro de `DrawString`.
- `Thread.CurrentThread.CurrentUICulture = …` en `App.xaml.cs` pasa al método `Initialize`.

Reemplaza todo `Game1.cs` por:

```csharp
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TorresIdiomas.Resources;

namespace TorresIdiomas;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Equivale a la línea del App.xaml.cs del tutorial.
        CultureInfo.CurrentUICulture = new CultureInfo("en");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Ui");
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkSlateGray);
        _spriteBatch.Begin();
        // Equivalen a los {x:Static p:Resources.…} del XAML.
        _spriteBatch.DrawString(_font, Strings.ChangeLanguage_Title, new Vector2(40, 40), Color.White);
        _spriteBatch.DrawString(_font, Strings.ChangeLanguage_Hint, new Vector2(40, 90), Color.White);
        _spriteBatch.DrawString(_font, Strings.ChangeLanguage_Note, new Vector2(40, 140), Color.White);
        _spriteBatch.DrawString(_font, Strings.Common_Back, new Vector2(40, 190), Color.White);
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
```

```bash
dotnet run
```

✔ Salen los cuatro textos **en inglés**.

Ahora cambia `"en"` por `"es"` en `Initialize` y vuelve a ejecutar.

✔ Salen **en español**, con tildes. Si ves `?` en lugar de `ó`, revisa el rango del paso 6.3.

Prueba también con `"fr"`.

✔ Sale en español. No hay archivo para el francés, así que .NET usa el idioma por defecto.

**Aquí termina el ejemplo del tutorial.** Si llegaste, ya lo tienes trasladado a Torres.

---

# PARTE 2 — Cambiar el idioma desde el juego (`CU-05`)

El tutorial fija el idioma en el código. Torres necesita tres cosas más:

1. Cambiarlo desde una pantalla, y que el cambio se vea al momento (paso 3 de `CU-05`).
2. Guardar la preferencia en el equipo, no en la cuenta (`POST-2`).
3. Pantallas con opciones en las que se puede hacer clic.

## Paso 8. Activar la comprobación de nulos

En el `.csproj`, dentro del primer `<PropertyGroup>`:

```xml
    <Nullable>enable</Nullable>
```

## Paso 9. El servicio de idioma

Crea `Localization/LanguageService.cs`. Toma el lugar de la línea de `Initialize`, pero además cambia el idioma y lo recuerda:

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TorresIdiomas.Localization
{
    /// <summary>
    /// Idioma de la interfaz del cliente. Sustituye a la línea
    /// Thread.CurrentThread.CurrentUICulture = ... del App.xaml.cs de WPF.
    /// La preferencia es del equipo, no de la cuenta (CU-05, POST-2): se guarda en un
    /// archivo local y nunca viaja al servidor.
    /// </summary>
    public sealed class LanguageService
    {
        private const string PreferenceFileName = "language.txt";

        // El nombre de cada idioma se muestra siempre en su propio idioma ("Español", "English"),
        // por eso no vive en los .resx.
        public static readonly IReadOnlyList<LanguageOption> Available = new[]
        {
            new LanguageOption("es", "Español"),
            new LanguageOption("en", "English"),
        };

        private readonly string preferencePath;

        public LanguageService()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Torres");
            preferencePath = Path.Combine(folder, PreferenceFileName);
        }

        public LanguageOption Current { get; private set; } = Available[0];

        public void LoadSavedPreference()
        {
            string code = Available[0].CultureCode;
            if (File.Exists(preferencePath))
            {
                code = File.ReadAllText(preferencePath).Trim();
            }

            Apply(Find(code) ?? Available[0]);
        }

        /// <returns>false si el idioma ya estaba en uso (CU-05, FA02).</returns>
        public bool Change(LanguageOption language)
        {
            if (language.CultureCode == Current.CultureCode)
            {
                return false;
            }

            Apply(language);
            Directory.CreateDirectory(Path.GetDirectoryName(preferencePath)!);
            File.WriteAllText(preferencePath, language.CultureCode);
            return true;
        }

        private void Apply(LanguageOption language)
        {
            var culture = CultureInfo.GetCultureInfo(language.CultureCode);

            // Hilo actual y también los que crean las continuaciones async de WCF.
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Current = language;
        }

        private static LanguageOption? Find(string cultureCode)
        {
            foreach (LanguageOption option in Available)
            {
                if (option.CultureCode == cultureCode)
                {
                    return option;
                }
            }

            return null;
        }
    }

    public sealed record LanguageOption(string CultureCode, string NativeName);
}
```

Fíjate en tres cosas:

- **`DefaultThreadCurrentUICulture`**: las llamadas asíncronas de WCF pueden continuar en otro hilo. Sin esta línea, ese hilo usaría el idioma del sistema.
- **«Español» y «English» no están en los `.resx`**: cada idioma se muestra siempre con su propio nombre, sin importar el idioma activo.
- **El archivo se guarda en la carpeta de datos del usuario**: en Mac es `~/Library/Application Support/Torres/`, en Linux `~/.config/Torres/`. Nunca va al servidor.

## Paso 10. Un botón que se traduce solo

Crea la carpeta `Screens` y dentro `Screens/IScreen.cs`:

```csharp
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TorresIdiomas.Screens
{
    public interface IScreen
    {
        void Update(MouseState mouse, bool clicked);

        void Draw(SpriteBatch spriteBatch);
    }
}
```

Y `Screens/MenuButton.cs`:

```csharp
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TorresIdiomas.Screens
{
    /// <summary>
    /// Botón de texto. Recibe una función que devuelve el texto, no el texto: así se relee
    /// del .resx en cada cuadro y el cambio de idioma se ve al instante, incluso en la
    /// pantalla abierta (CU-05, paso 3). Es lo que en WPF haría un binding dinámico;
    /// {x:Static} solo lee el valor una vez.
    /// </summary>
    public sealed class MenuButton
    {
        private readonly Func<string> text;
        private readonly Action onClick;
        private readonly Vector2 position;
        private Rectangle bounds;
        private bool hovered;

        public MenuButton(Func<string> text, Vector2 position, Action onClick)
        {
            this.text = text;
            this.position = position;
            this.onClick = onClick;
        }

        public bool IsMarked { get; set; }

        public void Update(SpriteFont font, Point mouse, bool clicked)
        {
            // El ancho se mide con el texto traducido: "Idioma de la interfaz" e
            // "Interface language" no ocupan lo mismo, así que nada se fija a mano.
            Vector2 size = font.MeasureString(Label());
            bounds = new Rectangle(position.ToPoint(), size.ToPoint());
            hovered = bounds.Contains(mouse);
            if (hovered && clicked)
            {
                onClick();
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            Color color = hovered ? Color.Gold : Color.White;
            spriteBatch.DrawString(font, Label(), position, color);
        }

        private string Label()
        {
            // La marca es "»" (U+00BB) porque la fuente solo incluye Latín-1: un "✓" saldría como "?".
            return IsMarked ? "» " + text() : "   " + text();
        }
    }
}
```

**La clave de todo el cambio inmediato:** el botón recibe `Func<string>`, es decir, **una función que devuelve el texto**, no el texto ya leído. `Draw` se ejecuta unas 60 veces por segundo, así que cada vez vuelve a pedir el texto y, si cambió el idioma, sale el nuevo.

Si hubieras escrito `string text = Strings.MainMenu_Rooms;` en el constructor, el botón se quedaría en el idioma de cuando se creó. **Regla: nunca guardes en un campo un texto ya traducido.**

## Paso 11. El menú principal

`Screens/MainMenuScreen.cs`. Solo tiene las opciones que citan los casos de uso:

```csharp
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TorresIdiomas.Localization;
using TorresIdiomas.Resources;

namespace TorresIdiomas.Screens
{
    /// <summary>GUIMainMenu reducida a las opciones que citan los casos de uso.</summary>
    public sealed class MainMenuScreen : IScreen
    {
        private readonly Game1 game;
        private readonly List<MenuButton> buttons;

        public MainMenuScreen(Game1 game, LanguageService languages)
        {
            this.game = game;
            buttons = new List<MenuButton>
            {
                new MenuButton(() => Strings.MainMenu_LogIn, new Vector2(60, 120), () => { }),
                new MenuButton(() => Strings.MainMenu_Rooms, new Vector2(60, 170), () => { }),
                new MenuButton(() => Strings.MainMenu_Ranking, new Vector2(60, 220), () => { }),
                // "Idioma · Español": texto con hueco {0}, igual que harían los mensajes con datos.
                new MenuButton(
                    () => string.Format(Strings.MainMenu_Language, languages.Current.NativeName),
                    new Vector2(60, 270),
                    () => game.Show(new ChangeLanguageScreen(game, languages, this))),
                new MenuButton(() => Strings.MainMenu_Exit, new Vector2(60, 320), game.Exit),
            };
        }

        public void Update(MouseState mouse, bool clicked)
        {
            foreach (MenuButton button in buttons)
            {
                button.Update(game.Font, mouse.Position, clicked);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(game.Font, "Torres", new Vector2(60, 40), Color.LightGreen);
            foreach (MenuButton button in buttons)
            {
                button.Draw(spriteBatch, game.Font);
            }
        }
    }
}
```

`MainMenu_Language` vale `Idioma · {0}`, y `string.Format` pone el nombre del idioma en el `{0}`. **No se concatena** (`"Idioma · " + nombre`) porque el orden de las palabras no es igual en todos los idiomas.

## Paso 12. La pantalla de idioma

`Screens/ChangeLanguageScreen.cs`. Es `GUIChangeLanguage` de `CU-05`:

```csharp
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TorresIdiomas.Localization;
using TorresIdiomas.Resources;

namespace TorresIdiomas.Screens
{
    /// <summary>GUIChangeLanguage (CU-05).</summary>
    public sealed class ChangeLanguageScreen : IScreen
    {
        private readonly Game1 game;
        private readonly LanguageService languages;
        private readonly List<(MenuButton Button, LanguageOption Language)> options = new();
        private readonly MenuButton backButton;

        public ChangeLanguageScreen(Game1 game, LanguageService languages, IScreen returnTo)
        {
            this.game = game;
            this.languages = languages;

            float y = 180;
            foreach (LanguageOption language in LanguageService.Available)
            {
                LanguageOption selected = language;
                var button = new MenuButton(
                    () => selected.NativeName, new Vector2(60, y), () => languages.Change(selected));
                options.Add((button, language));
                y += 50;
            }

            // FA01: "Volver" regresa a la ventana de origen.
            backButton = new MenuButton(() => Strings.Common_Back, new Vector2(60, 400), () => game.Show(returnTo));
        }

        public void Update(MouseState mouse, bool clicked)
        {
            foreach ((MenuButton button, LanguageOption language) in options)
            {
                button.Update(game.Font, mouse.Position, clicked);
            }

            // La marca se recalcula después de procesar el clic (paso 4 del flujo normal).
            foreach ((MenuButton button, LanguageOption language) in options)
            {
                button.IsMarked = language.CultureCode == languages.Current.CultureCode;
            }

            backButton.Update(game.Font, mouse.Position, clicked);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(game.Font, Strings.ChangeLanguage_Title, new Vector2(60, 40), Color.LightGreen);
            spriteBatch.DrawString(game.Font, Strings.ChangeLanguage_Hint, new Vector2(60, 90), Color.LightGray);
            foreach ((MenuButton button, LanguageOption language) in options)
            {
                button.Draw(spriteBatch, game.Font);
            }

            spriteBatch.DrawString(game.Font, Strings.ChangeLanguage_Note, new Vector2(60, 320), Color.LightGray);
            backButton.Draw(spriteBatch, game.Font);
        }
    }
}
```

Compara con el flujo de `CU-05`:

| `CU-05` | Dónde está |
|---|---|
| Paso 1: título, indicación, lista, nota y «Volver» | `Draw` |
| Paso 3: aplica el idioma de inmediato | `languages.Change(selected)` + `MenuButton` releyendo el texto |
| Paso 4: mueve la marca | `IsMarked`, recalculado en cada `Update` |
| FA01: «Volver» sin cambiar | `backButton` |
| FA02: elegir el idioma que ya está | `Change` devuelve `false` y no hace nada |

## Paso 13. Juntar todo en el juego

Reemplaza `Game1.cs` completo:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TorresIdiomas.Localization;
using TorresIdiomas.Screens;

namespace TorresIdiomas
{
    public sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly LanguageService languages = new();
        private SpriteBatch spriteBatch = null!;
        private IScreen screen = null!;
        private ButtonState previousLeftButton = ButtonState.Released;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 960,
                PreferredBackBufferHeight = 540,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public SpriteFont Font { get; private set; } = null!;

        public void Show(IScreen next)
        {
            screen = next;
        }

        protected override void Initialize()
        {
            // Antes de crear cualquier pantalla, como en el constructor de App.xaml.cs.
            languages.LoadSavedPreference();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Ui");
            screen = new MainMenuScreen(this, languages);
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState mouse = Mouse.GetState();
            bool clicked = IsActive
                && mouse.LeftButton == ButtonState.Released
                && previousLeftButton == ButtonState.Pressed;
            previousLeftButton = mouse.LeftButton;

            screen.Update(mouse, clicked);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(24, 40, 32));
            spriteBatch.Begin();
            screen.Draw(spriteBatch);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
```

Cambió respecto a la parte 1:

- `Initialize` ya no fija `"en"`: carga la preferencia guardada.
- `Update` detecta el clic cuando **se suelta** el botón del ratón; si no, un clic contaría en varios cuadros seguidos.
- `Draw` delega en la pantalla activa.

## Paso 14. Probar

```bash
dotnet run
```

✔ **Prueba A.** El menú sale en español y dice «Idioma · Español».

✔ **Prueba B.** Clic en «Idioma · Español» → sale la pantalla de idioma con «» Español» marcado.

✔ **Prueba C.** Clic en «English» → **la misma pantalla** cambia al inglés sin cerrarse, y la marca pasa a «English».

✔ **Prueba D.** Clic en «English» otra vez → no pasa nada (FA02).

✔ **Prueba E.** «Back» → el menú ya está en inglés.

✔ **Prueba F.** Cierra con «Exit» y vuelve a ejecutar → arranca en inglés.

```bash
cat ~/Library/Application\ Support/Torres/language.txt
```

✔ Imprime `en`.

✔ **Prueba G.** Borra la preferencia y ejecuta → vuelve a arrancar en español.

```bash
rm ~/Library/Application\ Support/Torres/language.txt
```

---

## Si algo falla

| Síntoma | Causa probable |
|---|---|
| `The name 'Strings' does not exist` | Falta el bloque del paso 5, o el `namespace` no coincide con `TorresIdiomas.Resources` |
| Siempre sale en español aunque pongas `"en"` | El archivo no se llama exactamente `Strings.en.resx`, o no existe `bin/Debug/net10.0/en/` |
| `ContentLoadException: Ui` | Falta el bloque del paso 6.4 en `Content.mgcb` |
| El juego se cierra con `Text contains characters that cannot be resolved` | El rango del paso 6.3 no cubre ese carácter y `DefaultCharacter` sigue comentado |
| Sale `?` en lugar de un carácter | Ese carácter está fuera del rango 32–255 (por ejemplo, `✓`) |
| La pantalla abierta no cambia de idioma | Guardaste el texto en un campo en vez de pasar `() => Strings…` |
| Un clic activa dos opciones | El clic se detecta con el botón presionado y no al soltarlo |

---

## Lo que queda por decidir en el equipo

1. **Los textos en inglés**: los de esta guía son una propuesta; ningún documento del proyecto los define.
2. **Los errores del servidor**: el servidor no sabe en qué idioma está cada cliente, así que debería mandar un código (un `enum`) y que el cliente lo traduzca.
3. **El idioma de la primera vez**: la guía arranca en español. `CU-05` no dice nada.
