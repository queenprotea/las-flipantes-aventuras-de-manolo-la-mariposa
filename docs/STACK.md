# Stack del proyecto: decisiones, justificación e instalación

Documento de referencia del equipo. Recoge qué tecnologías se usan, por qué se
eligieron frente a las alternativas, qué está verificado y cómo montar el
entorno en macOS, Windows y Linux.

---

## 1. El stack

| Capa | Tecnología | Versión |
|---|---|---|
| Lenguaje | C# | del SDK |
| Runtime y SDK | .NET | 10.0.400 |
| Servidor de red | CoreWCF con NetTcpBinding duplex | 1.9.1 |
| Cliente de red | System.ServiceModel.NetTcp | 10.0.652802 |
| Contratos compartidos | System.ServiceModel.Primitives sobre netstandard2.0 | 10.0.652802 |
| Motor 2D | MonoGame DesktopGL | 3.8.5.1 |
| Base de datos | pendiente de decidir, ver sección 5 | |
| Editor | JetBrains Rider o VS Code con C# Dev Kit | |
| Control de versiones | git | |

---

## 2. Las restricciones que mandan

Toda decisión de abajo sale de estas seis. No son preferencias, son el marco.

1. **C# obligatorio.** Impuesto por el profesor. Lo demás es libre.
2. **WCF con callbacks duplex sobre net.tcp y llamadas asíncronas.** Impuesto.
3. **Prohibido el stack web y los WebSockets.** Impuesto.
4. **Se evalúa el código**, no el acabado visual.
5. **Equipo de dos, en macOS arm64 y en Linux.** Solo uno tiene acceso real a
   Windows.
6. **Los dos deben poder correr servidor y clientes en su propia máquina**, para
   trabajar y probar sin depender del otro.

La restricción 6 es la que más decisiones fuerza, y la que suele pasarse por
alto.

---

## 3. Por qué cada pieza

### .NET 10 en lugar de .NET Framework

.NET Framework **solo existe en Windows**. Con el equipo en macOS y Linux, nadie
podría compilar ni ejecutar. No es una preferencia: es que no arranca.

.NET 10 es LTS con soporte hasta noviembre de 2028 y corre nativo en los tres
sistemas.

### CoreWCF en lugar de WCF clásico

Cumple el requisito de WCF duplex sobre net.tcp, pero **corre en macOS y Linux**.
Es el port oficial de WCF, está bajo la .NET Foundation y Microsoft publica una
política de soporte para él.

Qué es idéntico a WCF clásico:

- Los contratos: mismos `[ServiceContract]`, `[OperationContract]` y callbacks.
- El cliente: mismo `DuplexChannelFactory` y `InstanceContext`.
- El protocolo en el cable: interoperan entre sí.

Lo único que cambia es la clase que abre el host: `ServiceHost` pasa a ser el
host genérico de ASP.NET Core.

**Alternativa descartada:** WCF clásico con el servidor en la única máquina
Windows del equipo. Funcionaría, pero rompe la restricción 6 y convierte a esa
persona en punto único de fallo durante 14 semanas.

### MonoGame DesktopGL en lugar de Godot o Unity

- Es **C# puro**: cada clase, herencia y decisión de diseño es del equipo y se
  ve al calificar. Con la restricción 4, eso pesa más que la comodidad.
- **DesktopGL** usa OpenGL y SDL2, y corre en los tres sistemas. La variante
  `mgwindowsdx` usa DirectX y solo funciona en Windows.
- Es un proyecto `.csproj` estándar: se abre en cualquier editor, en cualquier
  sistema, sin instalar el motor.

**Godot** fue la segunda opción y se descartó por poco: su editor visual haría
parte del trabajo que precisamente se evalúa, su soporte de C# va por detrás de
GDScript en documentación de la comunidad, y con 14 semanas no se necesita la
velocidad de desarrollo que era su ventaja.

**El contraargumento de Godot era el editor visual.** Quedó neutralizado al
comprobar que `dotnet watch` aplica cambios en caliente en 343 ms sin cerrar la
ventana.

### El proyecto de contratos aparte, en netstandard2.0

Servidor y cliente comparten **un solo ensamblado** con las interfaces. Sin esto,
cada cambio de contrato es una negociación manual entre las dos personas y los
dos lados se desincronizan.

Funciona porque CoreWCF reconoce por reflexión los atributos del namespace
`System.ServiceModel`, lo que permite que el mismo ensamblado sirva a los dos
lados.

---

## 4. Lo que se descartó y por qué

| Descartado | Motivo |
|---|---|
| .NET Framework | Solo Windows. Excluye a todo el equipo |
| WCF clásico | Solo Windows. Rompe la restricción 6 |
| WinForms, WPF | Solo Windows |
| SignalR, gRPC, REST | Prohibidos por el requisito 3 |
| MonoGame WindowsDX | DirectX, solo Windows |
| Godot | El editor hace parte del trabajo evaluado |
| Unity | Sobredimensionado: 10 GB y curva empinada para un 2D pequeño |
| Stride | Su editor es solo Windows |
| Avalonia | Es framework de interfaces, no motor de juego |
| LÖVE | Es Lua, incumple el requisito de C# |
| Azure SQL Edge | Microsoft lo retiró el 30 de septiembre de 2025 |
| Mono, Wine, Docker para Windows | Innecesarios con este stack |

---

## 5. Base de datos, decisión pendiente

Depende de si el profesor exige un motor concreto.

| Opción | En Mac arm64 | Entregable | Cuándo elegirla |
|---|---|---|---|
| **SQLite** | Nativo | Un archivo junto al ejecutable | Si solo guarda usuarios, partidas y puntajes |
| **PostgreSQL** | Nativo en Docker | Requiere levantar contenedor | Si se pide servidor de base de datos y hay libertad |
| **SQL Server** | x86_64 bajo emulación Rosetta | Requiere levantar contenedor | Si el profesor lo exige |

SQL Server no tiene imagen ARM64. En Apple Silicon corre emulado con Docker
Desktop: es la vía recomendada hoy y funciona, con penalización de rendimiento
irrelevante para este proyecto.

**SQLite es la única que produce un entregable que se ejecuta con doble clic**,
sin que quien lo reciba tenga que instalar ni levantar nada.

---

## 6. Estado verificado

Todo lo siguiente se ejecutó en macOS arm64 (Apple M4) con .NET SDK 10.0.400.

| Qué | Resultado |
|---|---|
| CoreWCF duplex sobre net.tcp | Dos clientes intercambiaron callbacks. Sin el `InvalidCastException` reportado en CoreWCF |
| Conexión por IP de red local | Funcionó por `net.tcp://10.50.5.14:8000`, no solo por localhost |
| Ventana de MonoGame | 960x540, contexto OpenGL sobre Apple M4, game loop activo |
| Hot reload con `dotnet watch` | Cambio aplicado en 343 ms sin cerrar la ventana |
| Publicación cruzada | `win-x64`, `linux-x64` y `osx-arm64` generados desde el Mac, unos 85 MB cada uno |
| Vulnerabilidad CVE-2026-50648 | Corregida fijando System.Security.Cryptography.Xml 10.0.11 |

**No verificado todavía:** ejecución en Linux, ejecución en Windows, y conexión
real entre dos máquinas distintas.

---

## 7. Instalación

### 7.1 macOS (Apple Silicon)

```bash
brew install dotnet
```

Agrega esto a `~/.zshrc`:

```bash
export DOTNET_ROOT="/opt/homebrew/opt/dotnet/libexec"
```

Se usa la fórmula `dotnet`, no el cask `dotnet-sdk`: la fórmula viene
precompilada y no pide contraseña de administrador.

### 7.2 Windows

```powershell
winget install Microsoft.DotNet.SDK.10
```

Si no hay winget, el instalador está en `https://dotnet.microsoft.com/download`,
eligiendo el binario **x64** o **Arm64** según el equipo.

### 7.3 Linux

**Ubuntu o Debian**

```bash
sudo apt update && sudo apt install -y dotnet-sdk-10.0
```

**Fedora**

```bash
sudo dnf install -y dotnet-sdk-10.0
```

**Arch**

```bash
sudo pacman -S dotnet-sdk
```

**Cualquier distribución, sin permisos de root**

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
```

Con el script, agrega a `~/.bashrc`:

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet"
```

MonoGame en Linux puede necesitar las bibliotecas nativas de SDL2 y OpenAL. Solo
instálalas si la ventana no abre:

```bash
sudo apt install -y libsdl2-2.0-0 libopenal1
```

En Fedora son `SDL2` y `openal-soft`, en Arch `sdl2` y `openal`.

### 7.4 Plantillas de MonoGame, en los tres sistemas

Solo hacen falta para **crear** proyectos nuevos. Para compilar y correr lo que
ya existe en el repositorio, no.

```bash
dotnet new install MonoGame.Templates.CSharp
```

```bash
dotnet new mgdesktopgl -n NombreDelProyecto -o src/NombreDelProyecto
```

La plantilla apunta a `net9.0`. **Cámbialo a `net10.0`** en el `.csproj`.

---

## 8. Verificar que quedó bien

En los tres sistemas, lo mismo:

```bash
dotnet --version
```

Debe imprimir `10.0.x`. Una versión menor no compilará el proyecto.

```bash
dotnet build src/DuplexSmokeTest.Server/DuplexSmokeTest.Server.csproj
```

```bash
dotnet run --project src/DuplexSmokeTest.Desktop
```

Debe abrirse una ventana de 960x540 con un tablero y un recuadro naranja que se
mueve. Si el recuadro se mueve, el game loop corre.

En Linux hay instrucciones detalladas en `INSTALACION-LINUX.md`.

---

## 9. Reglas que no se negocian

**1. DesktopGL, nunca WindowsDX.** La segunda es DirectX y deja fuera a Mac y
Linux.

**2. `SecurityMode.None` en el NetTcpBinding.** El modo por defecto usa
autenticación de Windows y no funciona entre macOS y Linux. Los dos lados deben
coincidir.

**3. Todos los proyectos en `net10.0`.** Un target distinto rompe las
referencias entre proyectos.

**4. Mayúsculas exactas en nombres de archivo.** Linux distingue mayúsculas y
macOS no. Es el fallo más común en equipos mixtos y solo se manifiesta en la
máquina del otro. La sección 6.14 del estándar del equipo lo cubre.

**5. Nunca activar `PublishTrimmed` ni `PublishAot`.** WCF funciona por
reflexión y el recortador elimina código que sí hace falta. El fallo aparece en
tiempo de ejecución, no al compilar.

---

## 10. Pendientes

1. **Preguntar al profesor si acepta CoreWCF** o exige WCF sobre .NET Framework.
   Es lo más urgente: de esa respuesta depende toda la arquitectura.
2. **Preguntarle también si exige un motor de base de datos concreto.**
3. **Definir qué juego es.** Debe ser por turnos, de tablero o de cartas: WCF
   duplex no está pensado para tiempo real.
4. **Probar el prototipo en Linux y entre dos máquinas.**
5. **Resolver la inconsistencia del estándar** entre la sección 6 y la 10 sobre
   la posición de la llave de apertura, antes de generar el `.editorconfig`.
