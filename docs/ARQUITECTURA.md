# Arquitectura de Torres: proyectos, comunicación y cómo ampliarla

Documento de referencia del equipo, junto a `STACK.md`. Explica cómo está organizado el código, qué
hace cada proyecto, cómo se comunican entre sí, por qué se eligió esta forma y no otra, y dónde va
cada cosa nueva.

Estado al 14 de octubre de 2026. La estructura se verificó primero en
`examples/Torres.ArchitectureSpike` y después se aplicó al proyecto. Lo que ya existía no cambió de
comportamiento: solo cambió de sitio.

---

## 1. La idea en una frase

**El cliente solo dibuja y pide; el servidor decide. Dentro del servidor, cada proyecto tiene un solo
trabajo, y las reglas del juego solo se alcanzan por un camino.**

```
        CLIENTE (un proceso por jugador)                  SERVIDOR (un solo proceso)
 ┌───────────────────────────────┐   net.tcp    ┌──────────────────────────────────────────┐
 │ Torres.Client                 │  CoreWCF     │ Game.Server        arranca y configura   │
 │  pantallas, Myra, idiomas     │ ───────────► │   └ Game.Services  atiende las llamadas  │
 │  envía intenciones            │ ◄─────────── │       ├ Game.Match   partida en curso    │
 │  muestra lo que recibe        │  respuestas  │       │   └ Game.Domain  reglas          │
 └──────────────┬────────────────┘  (y avisos)  │       └ Game.Persistence  base de datos ─┼──► PostgreSQL
                │                               └──────────────────────┬───────────────────┘
                └──────────────► Game.Contracts ◄──────────────────────┘
                                 el idioma común: qué se puede pedir y qué datos viajan
```

---

## 2. Las tres soluciones

| Solución | Para qué | Proyectos |
|---|---|---|
| `Torres.Server.sln` | Trabajar en el servidor | `Game.Server`, `Game.Services`, `Game.Match`, `Game.Domain`, `Game.Persistence`, `Game.Contracts` |
| `Torres.Client.sln` | Trabajar en el cliente | `Torres.Client`, `Game.Contracts` |
| `Torres.sln` | Tenerlo todo abierto, incluidos los ejemplos. Es la que usa la configuración de Rider «Servidor y cliente», que arranca los dos a la vez | Todos |

**`Game.Contracts` aparece en las dos soluciones, pero es el mismo archivo `.csproj`, no una copia.**
Si se cambia un contrato desde una solución, la otra ve el cambio.

**Lo que no significa tener dos soluciones:** no hay dos repositorios, no se copian los contratos y
no se genera ningún proxy. Los procesos ya eran dos antes: el cliente de cada jugador y el servidor.

---

## 3. Qué hace cada proyecto

### `Game.Contracts`: el idioma común

| | |
|---|---|
| **Trabajo** | Declarar qué le puede pedir el cliente al servidor (interfaces `[ServiceContract]`) y qué datos viajan (`[DataContract]` y enumerados de resultado) |
| **Contiene hoy** | Una carpeta por área, con su interfaz y sus tipos juntos: `Account/` (`IAccountService`, `LoginResult`, `LoginStatus`, `RegistrationResult`, `PlayerIdentity`), `Ranking/` (`IRankingService`, `GlobalRankingResponseContract`, `RankingEntryContract`) y `Status/` (`IServerStatusService`, `ServerStatus`). En la raíz, `ServiceEndpointPaths`, con las rutas `/status`, `/account` y `/ranking`, porque lo usan todas las áreas |
| **Carpetas y espacio de nombres** | Las carpetas solo ordenan: todo está en `Game.Contracts`. Aquí importa especialmente, porque WCF identifica los tipos que viajan por la red por su espacio de nombres; mantenerlo garantiza que cliente y servidor siguen hablando el mismo idioma. Un área nueva (sesión, salas, chat, partida…) es una carpeta nueva |
| **Destino** | `netstandard2.0`, para que lo usen los dos lados |
| **No contiene** | Lógica, textos traducibles ni tipos del dominio. Los errores viajan como **código** (un valor de enumerado) y el cliente los traduce a su idioma |

### `Game.Server`: el host

| | |
|---|---|
| **Trabajo** | Arrancar el proceso: cargar `.env`, configurar log4net, leer `appsettings.json`, registrar todo en la inyección de dependencias y abrir net.tcp con un extremo por servicio |
| **Contiene hoy** | `Program.cs`, `ServerSettings.cs`, `appsettings.json` (puerto 8000), `log4net.config` |
| **Es el único que** | Conoce todos los proyectos del servidor, porque los registra. Por eso puede referenciarlos a todos sin que eso cree acoplamiento entre los demás |
| **No contiene** | Validaciones, reglas ni consultas |

### `Game.Services`: atiende las llamadas

| | |
|---|---|
| **Trabajo** | Implementar los contratos: recibir la intención del cliente, validar los argumentos, coordinar los pasos y devolver el resultado como código. Es la capa de servicios del estándar (`E 2.1`) |
| **Contiene hoy** | `ServerStatusService`, `AccountService` (registro e inicio de sesión con BCrypt) y `RankingService` |
| **Contendrá** | La sesión dúplex de cada jugador (conexión, latido, cola de avisos), salas, chat, amistades, invitaciones, perfil, historial y sanciones |
| **No contiene** | Reglas del juego. Ni siquiera **puede** verlas: no referencia `Game.Domain` (§5) |
| **Carpetas** | Hoy no tiene: son tres archivos. **Cuando llegue la sesión dúplex, se divide por las mismas áreas que `Game.Contracts`**, de modo que `Account/IAccountService` tenga su implementación en `Account/AccountService`. El espacio de nombres sigue siendo `Game.Services` |

### `Game.Match`: la partida en curso (vacío por ahora)

| | |
|---|---|
| **Trabajo** | Mantener viva cada partida mientras se juega: el turno de una operación a la vez, el reloj de 90 s, las ventanas de reconexión, guardar el estado al cerrar cada turno, la reanudación tras una caída, el resultado y la vista de cada puesto |
| **Es la única puerta a las reglas** | Recibe la intención, pide a `Game.Domain` que la valide y aplique, y devuelve el resultado **ya convertido a tipos de `Game.Contracts`** |
| **Por qué está vacío** | Todavía no hay código de partida, y las decisiones que fijan su contrato siguen abiertas (§9). El proyecto existe para que el primer código caiga en su sitio y con las referencias correctas |

### `Game.Domain`: las reglas (vacío por ahora)

| | |
|---|---|
| **Trabajo** | Decir qué es legal, cuánto cuesta y cuánto puntúa: tablero, jardineras, orugas, mariposa, cartas, puntos de acción, turnos y rondas, puntuación, ganador y retiro |
| **No referencia nada** | Ni base, ni red, ni reloj, ni azar del sistema. Recibe los parámetros y la fuente de azar desde fuera. Así se puede probar sin levantar nada (escenario T-1) |
| **Por qué está vacío** | Hay reglas por cerrar (el mapa, Q-A; la carta 6, Q-E) y escribirlas ahora sería inventarlas |

### `Game.Persistence`: la base de datos

| | |
|---|---|
| **Trabajo** | Leer y escribir en PostgreSQL con EF Core |
| **Contiene hoy** | **El mapeo completo del esquema:** 16 entidades en `Entities/` y sus 16 mapeos en `Mappings/`, una por cada tabla y por la vista de `database/schema.sql`. Son `Player`, `Friendship`, `Room`, `RoomInvitation`, `MatchRecord` (tabla `match`), `MatchParticipant`, `MatchState`, `PasswordRecovery`, `Report`, `Sanction`, los cinco catálogos (`TurnLayout`, `BuildAllowance`, `ActionCost`, `SanctionLevel`, `SystemParameter`) y la vista `PlayerRanking`. En la raíz están `TorresDbContext` (un `DbSet` por entidad y una línea que aplica los mapeos) y `DatabaseSettings`. En `Repositories/` están `PlayerRepository`, con su resultado `PlayerCreationOutcome`, y `RankingRepository`: solo existen repositorios para lo que ya se usa |
| **Cómo están mapeadas** | Una propiedad por columna, con los tipos del esquema: `smallint` → `short`, `timestamptz` → `DateTime` (UTC), `interval` → `TimeSpan`, `jsonb` → `string` con `HasColumnType("jsonb")`. Las columnas de estado restringidas por `CHECK` (`'pending'`, `'finished'`…) son `string`, igual que en la base. Las claves foráneas son propiedades normales (`RoomId`, `PlayerId`…), **sin propiedades de navegación**; las relaciones y los borrados en cascada los gobierna PostgreSQL. Las identidades y los `DEFAULT now()` llevan `ValueGeneratedOnAdd`, y las claves que no genera la base, `ValueGeneratedNever` |
| **Por qué `MatchRecord` y no `Match`** | Una clase `Match` chocaría con el proyecto y espacio de nombres `Game.Match` y con `System.Text.RegularExpressions.Match`, que `AccountService` importa |
| **Carpetas y espacio de nombres** | Las carpetas solo ordenan: todo está en el espacio `Game.Persistence`, porque el estándar ata el espacio de nombres a la capa. `src/.editorconfig` desactiva la sugerencia de hacerlos coincidir con la carpeta (IDE0130 y la inspección de Rider) |
| **Reglas que no se rompen** | El esquema lo manda `database/schema.sql`: nada de migraciones ni `EnsureCreated`, porque `torres_app` no tiene permisos de DDL. No contiene reglas del juego |

### `Torres.Client`: el juego

| | |
|---|---|
| **Trabajo** | Dibujar, leer la entrada, enviar intenciones y mostrar lo que el servidor contesta. El idioma solo existe aquí |
| **Conoce del servidor** | Únicamente `Game.Contracts` y la dirección del servidor (`appsettings.json` → `ServerAddress`) |
| **No contiene** | Ninguna regla ni parámetro del juego. Hoy `Screens/MatchScreen.cs` todavía tiene los costes escritos a mano. Es una deuda conocida: tendrán que llegar del servidor |

---

## 4. Cómo se comunican

### 4.1 Entre procesos: cliente y servidor

- **Transporte:** CoreWCF sobre net.tcp con `SecurityMode.None` (`RES-02`, `RES-09`).
- **Hoy hay tres extremos sin sesión**, uno por contrato: `net.tcp://servidor:8000/status`, `/account` y
  `/ranking`. Cada petición abre un canal, llama, espera la respuesta y lo cierra.
- **El cliente nunca bloquea el hilo del juego.** La pantalla guarda el `Task` de la llamada y en cada
  `Update` mira si ya terminó. `LoginScreen`, `RegisterScreen`, `StartupScreen` y
  `GlobalRankingScreen` siguen ese patrón.
- **El cliente no recibe los contratos del servidor en ejecución.** Los conoce al compilar, porque
  referencia `Game.Contracts`. En ejecución solo necesita la dirección.

### 4.2 Dentro del servidor: entre proyectos

```
Game.Server ──registra──► Game.Services ──llama──► Game.Match ──llama──► Game.Domain
     │                          │                        │
     │                          └──────► Game.Persistence ◄──────┘
     └── lee la configuración y la pasa
```

**Hoy** el recorrido es corto, porque no hay partida: `Game.Services` → `Game.Persistence`.

### 4.3 Recorrido real del inicio de sesión

| Paso | Dónde | Qué pasa |
|---|---|---|
| 1 | `Torres.Client` · `LoginScreen` | El jugador pulsa «Iniciar sesión». La pantalla crea un canal con la fábrica de `IAccountService`, llama a `LoginAsync` y guarda el `Task` |
| 2 | Red | La petición viaja por net.tcp a `/account` |
| 3 | `Game.Server` | CoreWCF la entrega a `AccountService`, registrado al arrancar |
| 4 | `Game.Services` · `AccountService` | Valida que haya nombre y contraseña, y pide la cuenta |
| 5 | `Game.Persistence` · `PlayerRepository` | Busca sin distinguir mayúsculas y devuelve la entidad `Player` |
| 6 | `Game.Services` · `AccountService` | Verifica el hash con BCrypt y convierte `Player` en `PlayerIdentity`, **sin el hash** |
| 7 | Red | `LoginResult` vuelve con un código: `LoggedIn`, `InvalidCredentials` o `DatabaseUnavailable` |
| 8 | `Torres.Client` · `LoginScreen.Update` | Ve que el `Task` terminó, libera el canal (`Abort`), guarda la identidad en `PlayerSession` y traduce el código a texto |

**Observa:** la entidad de base de datos (`Player`) nunca sale del servidor. Lo que viaja es un
contrato (`PlayerIdentity`), y la conversión la hace `Game.Services`.

### 4.4 Cómo será cuando haya partida (diseñado y probado, no construido)

Verificado en `examples/Torres.ArchitectureSpike`:

- **Una sesión dúplex por jugador** en un extremo nuevo, con su propia ruta en `ServiceEndpointPaths`. El
  nombre de esa ruta no está decidido; `/session` es solo el que usa la prueba técnica. Se abre al identificarse, porque
  las invitaciones llegan aunque el jugador esté en el menú.
- **Todas las operaciones del cliente son de petición–respuesta**, incluido el latido. Así el cliente
  sabe si su jugada se aceptó y detecta si el servidor se cayó.
- **Todos los avisos del servidor son callbacks one-way.** El servidor nunca espera al cliente. Cada
  jugador tiene su propia cola de salida, de modo que uno lento no frena a los demás.
- **Cuando un jugador mueve:** `Game.Services` sabe por la sesión qué puesto tiene → `Game.Match`
  toma esa partida y llama al dominio → `Game.Domain` valida y aplica → `Game.Match` devuelve el
  resultado en tipos del contrato → `Game.Services` responde «aceptada» y pone el estado nuevo en la
  cola de cada participante.
- **Cuando se acaba el tiempo y nadie pidió nada:** el reloj de `Game.Match` vence el turno, llama al
  dominio y avisa por una interfaz que declara el propio `Game.Match` (el «receptor de avisos») y que
  implementa `Game.Services`. Con esa interfaz, `Game.Match` no necesita conocer `Game.Services`, y
  no se forma un ciclo.

---

## 5. Reglas de dependencia

| Proyecto | Puede referenciar | Nunca | Por qué |
|---|---|---|---|
| `Game.Contracts` | Solo `System.ServiceModel.Primitives` | Nada nuestro | Lo usa el cliente: todo lo que referencie viajaría con él |
| `Game.Domain` | Nada | Todo | Para probar las reglas sin servidor, ventana ni base (`E 2.2`, T-1) |
| `Game.Persistence` | Nada nuestro (EF Core, Npgsql) | Domain, Match, Services | Solo lee y escribe; no decide nada |
| `Game.Match` | Domain (**privada**), Persistence, Contracts | Services, Server | Si conociera Services se formaría un ciclo; los avisos van por interfaz |
| `Game.Services` | Match, Persistence, Contracts | **Domain**, Server | Una sola entrada a las reglas: por Match |
| `Game.Server` | Services, Persistence, Contracts | — | Es quien registra todo |
| `Torres.Client` | Contracts | Todo lo del servidor | El cliente no decide resultados (`RES-05`) |

**Sin ciclos.** Si se ordenan los proyectos por nivel (0: Contracts, Domain, Persistence · 1: Match · 2:
Services · 3: Server), toda referencia baja de nivel.

**La referencia privada.** En .NET las referencias entre proyectos son transitivas: si
`Game.Services` referencia `Game.Match` y este a `Game.Domain`, `Game.Services` vería las reglas sin
haberlo pedido. Por eso `Game.Match.csproj` marca su referencia a `Game.Domain` con
`PrivateAssets="all"`. **Está comprobado en la prueba técnica:**

- con la marca, un proyecto que solo conoce Match no compila si usa un tipo del dominio;
- sin la marca, sí compila;
- y aun con la marca, `Game.Domain.dll` llega al servidor y se ejecuta.

**Si alguna vez hace falta romper una de estas reglas, la estructura está avisando de algo:** lo más
probable es que el código esté en el proyecto equivocado.

---

## 6. Por qué esta forma y no otras

| Alternativa | Por qué no |
|---|---|
| **Todo el servidor en un proyecto** (lo que había) | Las pruebas de las reglas tendrían que cargar todo el servidor, en contra de la medida 2 de T-1, y nada impediría que una regla llamara a la base |
| **Partida y Reglas en dos proyectos de dominio** | Cada regla se evalúa sobre el estado de la partida, y aplicar una acción es validar, cobrar y cambiar el estado en un paso. Separarlas obliga a un ciclo o a un estado sin comportamiento |
| **La partida en curso dentro del dominio** | El dominio necesitaría el reloj, la base y las sesiones, y dejaría de poder probarse solo |
| **Services conociendo Domain** | Habría dos entradas a las reglas, y la segunda se saltaría el control de turno de Match |
| **Compartir el dominio con el cliente** (para resaltar jugadas sin preguntar) | Rompe `E 2.2`. El cliente no tiene el estado completo (mazo, manos ajenas), los parámetros están en la base, y cada cambio de regla obligaría a redistribuir el cliente. Los destinos legales los calculará el servidor |
| **Generar los contratos en el cliente** con `dotnet-svcutil` | CoreWCF publica los metadatos por HTTP, que choca con `RES-03`, y duplicaría los tipos |
| **Copiar los contratos o empaquetarlos en NuGet** | Dos copias se desincronizan, y NuGet solo tiene sentido con repositorios separados |
| **Varios extremos dúplex** (uno para salas, otro para partida…) | Varias sesiones por jugador que habría que correlacionar. Puede pasar que el chat siga vivo y la partida no |

El razonamiento completo está en los análisis del 13 y 14 de octubre: contratos y soluciones,
organización de proyectos (versión 2) y resultados de la prueba técnica.

---

## 7. Cómo agregar cosas

### 7.1 Una operación nueva sin sesión

Ejemplo: recuperar la contraseña (CU-03), que se usa sin estar identificado.

1. **`Game.Contracts`:** en la carpeta de su área (aquí `Account/`), añade el método al contrato (`IAccountService`) con
   `[OperationContract(Name = "…")]` y devolviendo `Task<…>`. Si el resultado tiene varios casos,
   crea un enumerado `[DataContract]` con `[EnumMember]`. **Nunca un texto.**
2. **`Game.Persistence`:** si necesita datos nuevos, entidad + mapeo en `TorresDbContext` +
   repositorio (§7.3).
3. **`Game.Services`:** impleméntalo en la clase del servicio. Valida los argumentos, llama al
   repositorio, captura `NpgsqlException` y devuelve el código correspondiente.
4. **`Game.Server`:** si es un **servicio nuevo**, registra la clase y su repositorio en
   `Program.cs` (`AddSingleton`), añade el extremo en `AddEndpoints` y la ruta en
   `Game.Contracts/ServiceEndpointPaths`. Si es una operación de un servicio existente, no hay que
   tocar nada.
5. **`Torres.Client`:** usa la fábrica de canales de ese contrato. Crea el canal, llama, guarda el
   `Task` y míralo en `Update`. Traduce el código con `TextKeys` y el `.resx`.

### 7.2 Algo que necesita saber quién es el jugador

Ejemplos: amistades, perfil, salas.

**Todavía no se puede hacer bien:** necesita la sesión dúplex y la ficha de identidad, y esas
decisiones están abiertas (§9). No hay que poner el identificador del jugador como argumento: un
cliente modificado podría hacerse pasar por otro (`RES-05`). La referencia de cómo hacerlo está en
`examples/Torres.ArchitectureSpike` (`GameSessionService`, `SessionRegistry`).

### 7.3 Una tabla o entidad nueva

1. La tabla ya debe existir en `database/schema.sql`, que es quien manda.
2. **`Game.Persistence`:**
   - la clase de entidad, en `Entities/`;
   - su mapeo en `Mappings/`: una clase `…Mapping` que implementa `IEntityTypeConfiguration<…>`
     con `ToTable` y `HasColumnName` en `snake_case`. `TorresDbContext` la descubre sola con
     `ApplyConfigurationsFromAssembly`, así que no hay que registrarla;
   - una propiedad `DbSet<…>` en `TorresDbContext`;
   - un repositorio en `Repositories/`, que use `IDbContextFactory` porque los servicios son
     singleton. Si una operación puede salir de varias formas esperables, devuelve un enumerado de
     resultado junto al repositorio, como `PlayerCreationOutcome`, en lugar de lanzar una excepción.
   - El espacio de nombres sigue siendo `Game.Persistence`.
3. **`Game.Server`:** registra el repositorio en `Program.cs`.
4. **No** se usan migraciones ni `EnsureCreated`.

### 7.4 Una regla del juego

- **Va en `Game.Domain`**, sin referencias.
- Si necesita un parámetro (un coste, turnos por ronda), **lo recibe como argumento**. Lo lee
  `Game.Match` de la base y se lo pasa (`D-30`).
- Si necesita azar, lo recibe como colaborador, para que las pruebas sean reproducibles.
- Antes de escribirla, comprueba que la regla está cerrada en `docs/reglas/` y en la base consolidada.

### 7.5 Algo de la partida en curso

Ejemplos: el reloj, la reconexión, guardar el turno.

- **Va en `Game.Match`.**
- Su parte pública solo usa tipos de `Game.Contracts` o tipos propios de `Game.Match`, **nunca del
  dominio**. Si no, `Game.Services` necesitaría el dominio.
- Lo que tenga que avisar sin que nadie lo pida sale por la interfaz del receptor de avisos.

### 7.6 Configuración

- Del servidor: `src/Game.Server/appsettings.json`, leída con una clase `…Settings`.
- Del cliente: `src/Torres.Client/appsettings.json`.
- Secretos: `.env`, que no se versiona.
- **Ningún literal de configuración en el código** (criterio del profesor).
- Si un valor debe coincidir en los dos lados (tiempos, tamaño máximo de mensaje), se escribe con el
  mismo nombre en los dos `appsettings.json`. El binding no se puede compartir: el servidor usa el
  `NetTcpBinding` de CoreWCF y el cliente el de `System.ServiceModel`, que son tipos distintos.

### 7.7 Carpetas

En el servidor, **las carpetas ordenan y el espacio de nombres indica la capa**: todo lo de
`Game.Persistence` está en `Game.Persistence`, todo lo de `Game.Contracts` en `Game.Contracts`, y
así con cada proyecto. `src/.editorconfig` desactiva la sugerencia de .NET y de Rider de hacerlos
coincidir con la carpeta.

| Proyecto | Carpetas |
|---|---|
| `Game.Contracts` | Por área: `Account/`, `Ranking/`, `Status/` y las que vengan |
| `Game.Persistence` | Por tipo de pieza: `Entities/`, `Mappings/`, `Repositories/` |
| `Game.Services` | Ninguna por ahora; por área, igual que los contratos, al llegar la sesión |
| `Game.Server` | Ninguna: si necesita carpetas, algo está en el proyecto equivocado |
| `Game.Match`, `Game.Domain` | Se decidirán con su primer código; no se crean carpetas vacías |
| `Torres.Client` | Ya tiene las suyas, y ahí el espacio de nombres **sí** sigue a la carpeta (`Torres.Client.Screens`…). Dividir `Screens/` por área, si se hace, debe acordarse con quien trabaja en las pantallas, para no provocar conflictos al integrar ramas |

### 7.8 Antes de dar algo por terminado

- `dotnet build Torres.Server.sln` y `dotnet build Torres.Client.sln` sin errores ni advertencias. El
  estilo se comprueba en la compilación (`EnforceCodeStyleInBuild`).
- Comprueba que no apareció ninguna referencia prohibida (§5) en los `.csproj`.
- El código sigue el estándar v7 con los criterios del profesor:
  - `return` solo de variables;
  - sin comentarios;
  - nombres descriptivos;
  - nada de SQL a mano si EF Core puede hacerlo.

---

## 8. Comandos

```bash
dotnet build Torres.Server.sln
```

```bash
dotnet build Torres.Client.sln
```

```bash
dotnet run --project src/Game.Server
```

```bash
dotnet run --project src/Torres.Client
```

```bash
examples/Torres.ArchitectureSpike/verificar.sh
```

En macOS, si PostgreSQL no arranca y el registro dice «postmaster became multithreaded during
startup», arráncalo con la variable `LC_ALL` definida, por ejemplo `LC_ALL=en_US.UTF-8`.

---

## 9. Lo que falta y por qué no se añadió

No se inventó nada para rellenar. Esto es lo que falta:

| Falta | Por qué no está | Qué lo desbloquea |
|---|---|---|
| Código en `Game.Domain` | Hay reglas abiertas: el mapa (Q-A) y la carta 6 (Q-E) | Cerrarlas |
| Código en `Game.Match` y el extremo de la sesión dúplex | La forma del contrato depende de decisiones abiertas: la ficha del invitado frente a su pase, si el callback lleva el suceso o el estado, cómo se reidentifica una cuenta tras una caída del servidor, qué pasa con un segundo inicio de sesión de la misma cuenta y si las manos ajenas son visibles | Decidir A-1 a A-5 del análisis de contratos |
| Proyecto de pruebas `Game.Domain.Tests` | Ninguna fuente fija el marco de pruebas (xUnit, NUnit o MSTest) | Elegirlo |
| Costes y límites que llegan al cliente por contrato | Dependen del contrato de partida | El contrato de partida |
| Paquetes criptográficos del cliente | `Torres.Client` fija `System.Security.Cryptography.Xml` 10.0.11, que el 13 de octubre quedó afectada por tres CVE de denegación de servicio; `Pkcs` 10.0.11, por otras dos | Subir los dos a 10.0.13 (propuesto como tarea aparte). `Game.Services` y `Game.Match` ya fijan la 10.0.13 |
| Probar un corte de red entre dos máquinas | Necesita dos equipos | Hacerlo en red local |

`STACK.md §6` y §8 siguen citando un `DuplexSmokeTest` que no está en el repositorio. La
verificación del dúplex que sí está es `examples/Torres.ArchitectureSpike`.
