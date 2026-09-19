# Prueba técnica: Myra como capa de interfaz de Torres

**Fecha:** 18 de septiembre de 2026
**Proyecto de la prueba:** `code/examples/Torres.MyraSpike/` — aislado, no toca `code/src/Torres.Client/`
**Objeto:** decidir con datos, y no por intuición, si Myra sustituye a la carpeta `Ui/` escrita a mano.
**Alcance:** se probó el mecanismo, no el acabado. La restricción 4 del proyecto queda fuera de este análisis por indicación expresa.

---

## 1. Qué se montó

Un proyecto `net10.0` con MonoGame DesktopGL 3.8.5.1 y Myra 1.6.6, que usa **los recursos reales de Torres** (`Strings.resx` y `Strings.en.resx`, 300 claves) y **las claves reales** (`TextKeys`, copiado del cliente cambiando el espacio de nombres).

La pantalla de prueba contiene, a propósito, lo que más duele: etiquetas, un párrafo largo con ajuste de línea, dos campos de texto —uno de contraseña—, dos botones, una lista con barra de desplazamiento y una línea de diagnóstico con la fecha y el número formateados por la cultura activa.

Se capturó a PNG en las dos culturas desde el propio programa, con `RenderTarget2D`, para poder comparar sin depender de lo que vea nadie a ojo.

---

## 2. Las cuatro preguntas, respondidas

### 2.1 ¿Compila y corre con nuestro stack? — **Sí**

Compila sin advertencias y arranca. Y hay un detalle que conviene tener por escrito:

**Myra 1.6.6 está compilado contra MonoGame 3.8.0.1641 y nosotros usamos 3.8.5.1.** El desajuste no rompe nada, porque `MonoGame.Framework` no lleva nombre seguro y el runtime carga la que haya. Verificado en el `deps.json` del binario:

```
MonoGame.Framework.DesktopGL/3.8.5.1   MonoGame.Framework.dll   assembly=3.8.5.1
Myra/1.6.6                             Myra.dll                 assembly=1.6.6.0
```

Es decir: **no arrastra una segunda copia de MonoGame**. Funciona sobre la nuestra.

### 2.2 ¿Se ven `ñ`, `á`, `—` y `…` sin configurar rangos? — **Sí, y sin tocar nada**

Es el resultado más claro de la prueba. Con la hoja de estilos por defecto, **sin declarar un solo rango de caracteres**, se dibujan correctamente:

| | |
|---|---|
| `ñ` y acentos | «Contraseña», «tendrás» ✓ |
| `—` (U+2014) | «Torres — cliente de escritorio» ✓ |
| `…` (U+2026) | «Conectando…» ✓ |

Compáralo con lo que cuesta hoy: en `Ui.spritefont` hay que declarar 32–255 y **dos regiones más**, una por cada carácter suelto que se salga de Latín-1, y un carácter olvidado sale como `?`. Con Myra ese problema **desaparece**, porque FontStashSharp rasteriza cada glifo bajo demanda desde la `.ttf` en vez de precompilar un atlas.

**Matiz honesto:** desaparece el problema del *rango*, no el de la *cobertura*. La fuente sigue teniendo que contener el glifo. La diferencia es que para añadir japonés bastaría cambiar la `.ttf`, sin tocar ninguna configuración de rangos.

### 2.3 ¿Cuánto cuesta el refresco de idioma? — **88 líneas**

Myra es de modo retenido: `Text` se asigna una vez y ahí se queda. Hubo que escribir:

| Archivo | Líneas | Qué hace |
|---|---|---|
| `ILocalizedWidget.cs` | 7 | El contrato: «sé releer mi texto» |
| `LocalizedLabel.cs` | 20 | `Label` que recuerda su clave |
| `LocalizedButton.cs` | 22 | `Button` que recuerda su clave |
| `LanguageRefresher.cs` | 39 | Recorre el árbol de widgets y refresca los que sepan |
| **Total atribuible a Myra** | **88** | |

(`LocalizedText.cs`, 16 líneas, no cuenta: existe igual en las dos versiones.)

**Funciona:** el diagnóstico de la captura en inglés dice `widgets refrescados=28`. Un clic, 28 widgets al día.

88 líneas es poco. Pero la lectura importante es otra: **es el mecanismo que la actividad evalúa, y con Myra hay que escribirlo, mientras que con el dibujo en modo inmediato sale gratis del bucle.**

### 2.4 ¿Cuánto cuesta un `ScrollViewer` como el del prototipo? — **A medias**

`new ScrollViewer { Content = rows, Height = 120, Width = 620 }` y ya hay lista con barra de desplazamiento funcionando. Eso, a mano, son bastantes horas.

**Lo que no medí:** darle el aspecto del prototipo. La hoja de estilos por defecto es sobria, de herramienta, como se ve en las capturas. Adaptarla exige aprender su formato XML (`.xmms`) y no lo probé. **Esta pregunta queda contestada a medias y así hay que tomarla.**

---

## 3. Lo que apareció sin buscarlo

### 3.1 La API no es la que cuentan los tutoriales

**`TextButton` no existe en Myra 1.6.6.** Casi toda la documentación y los ejemplos que circulan lo usan. La forma real es:

```csharp
var button = new Button { Content = new Label { Text = "Entrar" } };
```

`Button` deriva de `ContentControl` y acepta **cualquier widget** dentro, exactamente igual que en WPF. Es más potente —un botón con icono y texto es un `HorizontalStackPanel` dentro— pero significa que **casi todo el material de aprendizaje que encuentres estará desactualizado**. Hubo que sacar la API por reflexión sobre el ensamblado.

Los eventos tampoco son los de .NET: usan `MyraEventHandler(object sender, MyraEventArgs e)`, no `EventHandler`.

### 3.2 El orden de inicialización choca con cómo están escritas nuestras pantallas

`MyraEnvironment.Game = this;` **tiene que ejecutarse antes de construir el primer widget**, o revienta al crear el primer `Label`, dentro de la hoja de estilos por defecto. Me pasó en el primer intento.

Consecuencia directa para una migración: hoy las nueve pantallas declaran sus controles así:

```csharp
private readonly TextButton _backButton = new TextButton(TextKeys.Common.BackButton, ButtonStyle.Secondary);
```

Un inicializador de campo corre en el constructor, que es demasiado pronto. **Con Myra, los controles no pueden inicializarse en línea**: hay que moverlos a un método de construcción y dejan de ser `readonly` con valor. Es un cambio estructural en las nueve, no un reemplazo de tipos.

### 3.3 El auto-layout reacomoda solo, y se ve

Compara las dos capturas. El párrafo de limitaciones del invitado ocupa **tres líneas en español y dos en inglés**, y todo lo que hay debajo —la lista, el diagnóstico— **sube solo**.

Hoy eso no pasa: las posiciones son fijas y calculadas a mano, así que el bloque de abajo se quedaría donde estaba, dejando un hueco en inglés. **Esto ataca justo el riesgo de la etapa 6**, el de los textos que crecen al traducirse.

### 3.4 Los nombres de los idiomas no se tradujeron, y eso está bien

Los botones «Español» y «English» siguen igual en las dos capturas. Es el comportamiento correcto de `CU-05` paso 1, y sale solo porque las dos claves tienen el mismo valor en los dos `.resx`. Confirma de paso que la corrección H-01 de la auditoría quedó bien hecha.

---

## 4. Coste real de migrar

| | |
|---|---|
| **Se borra** | `Painter`, `TextWrapper`, `InputState`, `TextButton`, `TextField`, `ButtonStyle`, `Theme` y toda la aritmética de posiciones de las 9 pantallas |
| **Se escribe** | 88 líneas de refresco de idioma, más una hoja de estilos para parecerse al prototipo (sin medir) |
| **Se reestructura** | Las 9 pantallas: los controles pasan de campo inicializado en línea a construcción en método |
| **Se decide antes** | Cómo llegan los callbacks duplex de WCF a los widgets (ver abajo) |

### El punto que sigue sin resolver: los hilos

No lo probé porque la prueba no tiene servidor, y es el riesgo de verdad. Torres es **WCF duplex**: el servidor llama de vuelta desde un hilo del canal. Los widgets de Myra se tocan desde el hilo del juego.

- **Hoy (modo inmediato):** el callback escribe en un dato, y el `Draw` lo lee en el cuadro siguiente. No hay problema de hilos en la interfaz.
- **Con Myra (modo retenido):** el callback querría hacer `label.Text = ...` desde el hilo equivocado. Hace falta una cola de acciones que el `Update` vacíe en el hilo del juego.

Son unas 30 líneas, pero **condiciona los contratos compartidos**, así que se decide antes de escribirlos, junto a lo de los códigos de error del §4.7 del documento de internacionalización.

---

## 5. Veredicto

**Myra es viable para Torres.** Compila, corre sobre nuestra versión de MonoGame, resuelve el problema de la fuente de raíz y trae hechos el scroll, las pestañas, los diálogos y las tablas que aún nos faltan.

Con dos reservas y una recomendación:

1. **No migrar ahora.** Las nueve pantallas funcionan. Migrar a mitad de la actividad significa reestructurarlas todas para reintroducir a mano el mecanismo que se está evaluando.
2. **Antes de adoptarlo para el juego completo, decidir lo de los hilos**, porque toca los contratos.
3. **Recomendación:** adoptarlo al empezar las pantallas de sala y partida, que es donde la caja de herramientas casera se queda corta de verdad. Lo que ya está hecho puede quedarse como está o migrarse después, sin prisa.

Y dos avisos prácticos para quien lo retome: **la documentación que encuentres estará desactualizada** —`TextButton` ya no existe—, y la referencia fiable es el ensamblado mismo, leído por reflexión, que es como se sacó la API para esta prueba.

---

## 6. Cómo repetir la prueba

```bash
cd "code/examples/Torres.MyraSpike"
dotnet run                 # ventana interactiva; los botones cambian el idioma
dotnet run -- --capture    # genera myra-es.png y myra-en.png y termina
```
