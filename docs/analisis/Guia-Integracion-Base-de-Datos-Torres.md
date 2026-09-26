# Guía de integración con la base de datos de Torres

Versión 3, 24 de septiembre de 2026. La versión 1 proponía que el cliente accediera directamente
a la base; la 2 puso la base detrás del servidor (D1); esta 3 junta el servidor en **un solo
proyecto** y deja el entorno configurado para ejecutar y probar.

Etapa 1: conexión comprobada de extremo a extremo, desde la ventana del juego hasta PostgreSQL,
pasando por el contrato WCF y el servidor. Todavía **no** están el inicio de sesión, el registro ni
la consulta de puntuaciones.

---

## 1. Decisiones tomadas

| # | Decisión | Resultado | Dónde se aplicó |
|---|---|---|---|
| D1 | ¿Dónde se accede a la base? | **Contratos y servidor mínimos ya.** El cliente no conoce la base. **El servidor es un solo proyecto** | `src/Game.Contracts`, `src/Game.Services` |
| D2 | ORM | **Dapper sobre Npgsql.** Prisma no es viable (apartado 2) | `src/Game.Services/Persistence` |
| D3 | Nombres | **Los del estándar.** El proyecto del servidor es `Game.Services`; la persistencia vive dentro de él, en la carpeta `Persistence/`, con el espacio de nombres `Game.Persistence` que fija el estándar. El estándar no nombra el ensamblado de contratos; se propone `Game.Contracts` | Los dos proyectos |
| D4 | Datos de conexión | **Archivo `.env`**, leído solo por el servidor | `.env.example`, `.gitignore`, `Game.Services/Program.cs` |
| D5 | Claves de validación | **Añadidas** al diccionario (apartado 3) | `Diccionario-i18n-Torres.xlsx` y recursos regenerados |
| D6 | Biblioteca BCrypt | **BCrypt.Net-Next 4.2.1**, la última estable (la 5.0 solo existe como versión previa) | Se añadirá a `Game.Services` con el inicio de sesión; hoy nada la usa |
| D7 | `wip: rooms page` | **Completada** según PT-14 | `Screens/RoomsScreen.cs` |
| D8 | Dónde vive la sesión | **Por decidir**: opciones en el apartado 4 | Antes del inicio de sesión |

### Por qué el servidor es un proyecto y los contratos otro

El servidor (`Game.Services`) reúne el host de CoreWCF, los servicios y la persistencia. Las capas
del estándar siguen separadas, pero como **espacios de nombres y carpetas**, no como proyectos: la
regla 2.1 fija el espacio de nombres de cada capa, no un proyecto por capa.

`Game.Contracts` **no puede entrar** en el servidor: lo referencia también el cliente. Si los
contratos vivieran en el proyecto del servidor, el cliente arrastraría CoreWCF, Npgsql, Dapper y
ASP.NET Core, y tendría a su alcance las clases que hablan con la base. Además es una decisión ya
registrada (`STACK.md` §3 y la decisión 2 de la iteración 1 de ADD): un solo ensamblado en
`netstandard2.0` compartido por los dos lados.

---

## 2. D2: alternativas al papel de Entity Framework

| Opción | Qué es | Veredicto |
|---|---|---|
| **Prisma** | ORM de **TypeScript y Node.js**. Genera su cliente en TypeScript. No tiene cliente oficial para C# | **No viable.** Para usarlo habría que poner un proceso Node entre `Game.Services` y PostgreSQL: rompe la restricción 1 de `STACK.md` (C# obligatorio), añade un segundo proceso y una frontera más, y la persistencia dejaría de ser código C# dentro de `Game.Services` |
| Entity Framework Core | ORM completo de .NET, sucesor directo de EF | Viable técnicamente y multiplataforma, pero **excluido por tu indicación** |
| **Dapper** | Micro-ORM: tú escribes el SQL y Dapper convierte las filas en objetos C# | **Elegido** |
| linq2db | ORM ligero con consultas LINQ, sin seguimiento de cambios | Viable. Menos difundido; su compatibilidad con Npgsql 10 no se comprobó |
| NHibernate | ORM completo y veterano, con mapeos en XML o en código | Viable, pero pesado para doce tablas y con una curva que no cabe en la semana |
| Npgsql solo | Proveedor ADO.NET, sin mapeo | No es un ORM: no cumple el requisito |

**Por qué Dapper:**

- El esquema de la Actividad 1 ya está escrito en SQL, con una vista (`player_ranking`), índices
  únicos sobre `lower()`, identidades `GENERATED ALWAYS` y restricciones `CHECK`. Dapper lo usa tal
  cual; un ORM completo querría generar o gobernar ese esquema.
- El SQL queda a la vista en el código, que es lo que se califica (`STACK.md` §2, restricción 4).
- No depende de la versión del proveedor: funciona con cualquier conexión ADO.NET, y aquí se
  comprobó con Npgsql 10.0.3.

**Cómo se argumenta ante la actividad:** Dapper cumple el papel de EF, que es convertir entre filas
y objetos. Lo que no hace es generar el SQL ni seguir los cambios de los objetos. En Torres eso es
una ventaja, porque el SQL ya existe y se revisa.

---

## 3. D5: qué era y cómo quedó

**El problema.** Los casos de uso piden mensajes de validación que el diccionario no tenía:
`CU-01` FA01 («un mensaje indicando que debe completarlo»), `CU-02` FA04 (campos vacíos) y
`CU-02` FA05 («qué regla de formato incumple»). Sin esas claves la validación tendría que escribir
textos en el código, que es justo lo que la internacionalización prohíbe.

**Lo que se hizo.** Cuatro filas nuevas en `docs/i18n/Diccionario-i18n-Torres.xlsx`, y los
recursos se regeneraron con `python3 tools/generar-recursos.py`. Los `.resx` no se editaron a mano.

| Clave | es-MX | en-US | Caso |
|---|---|---|---|
| `Common.RequiredField` | Completa este campo. | Fill in this field. | CU-01 FA01, CU-02 FA04 |
| `Register.UsernameInvalidFormat` | Usa de 3 a 20 caracteres: solo letras, números y guion bajo. | Use 3 to 20 characters: only letters, numbers, and underscore. | CU-02 FA05 |
| `Register.EmailInvalidFormat` | Ingresa un correo electrónico válido. | Enter a valid email address. | CU-02 FA05 |
| `Register.PasswordInvalidFormat` | Usa de 8 a 20 caracteres, con mayúsculas, minúsculas, números y caracteres especiales. | Use 8 to 20 characters, including uppercase and lowercase letters, numbers, and special characters. | CU-02 paso 6 y FA05 |

| Criterio | Motivo |
|---|---|
| Una sola clave para «campo vacío» y en `Common` | Sirve a Login y a Register. Es el criterio de la auditoría del 17 sep: lo que se repite entre pantallas va a `Common` |
| El correo copia el texto de `PasswordRecovery.InvalidEmail` | Mismo mensaje en todo el juego. Se deja como clave aparte porque es de otra pantalla; si se prefiere consolidar en `Common`, hay que tocar `PasswordRecoveryScreen` |
| La regla del username es la de la base | La misma que `ck_player_username_format`: `^[A-Za-z0-9_]{3,20}$` |

**Qué más cambió en el XLSX:** 310 a **314 filas**; la tabla `DiccionarioI18n` pasa a
`A1:I315`; se conservó el bandeado de filas; en Resumen, `ErrorMessage` pasa de 30 a 34, `Common`
de 22 a 23 y `Register` de 10 a 13; la Portada cuenta 314 elementos y registra la revisión del 24
de septiembre. Los textos son propuesta: los casos de uso no fijan la redacción.

**Verificación:** el generador produjo exactamente 4 constantes nuevas en `TextKeys.cs` y 12
líneas en cada `.resx`, y nada más.

---

## 4. D8: dónde se guarda el jugador con sesión iniciada

### Lo que ya está fijado y restringe la respuesta

| Fuente | Qué impone |
|---|---|
| `RES-05` | El cliente no decide resultados. Si el cliente dijera «soy el jugador 7», el servidor no podría fiarse |
| `IT3-D04` | Todo lo de la sesión se concentra en un elemento propio dentro de `Game.Services`, todavía **sin nombre** |
| Modelo de datos | La entidad `session` está **descartada** con motivo: la sesión no vive en la base |
| `DR-30` | Al reconectar dentro de la ventana, el jugador retoma su turno: la identidad debe sobrevivir a un canal nuevo |
| WCF | Cada contrato tiene su propio punto de conexión y su propio canal. Si el login ocurre en un canal y las salas en otro, el servidor no puede relacionarlos por el canal |

### Opciones

| Opción | Cómo funciona | A favor | En contra |
|---|---|---|---|
| **A. Sesión ligada al canal** | El servicio usa `SessionMode.Required` y guarda al jugador en la instancia del canal | Sin parámetros extra; cuando el canal cae, la sesión cae | Obliga a un único contrato por cliente, o todo lo que no sea ese contrato queda sin identidad. Tras una reconexión el canal es otro, así que habría que volver a iniciar sesión, contra `DR-30` |
| **B. Ficha de sesión en la memoria del servidor** | El login devuelve un identificador opaco (`Guid`). El servidor guarda la relación ficha → `player_id` en el elemento de sesión de `IT3-D04`, y el cliente manda la ficha en cada operación que exige identidad | Funciona con varios contratos; sobrevive a la reconexión; la autoridad sigue en el servidor | Cada operación lleva un parámetro más. La ficha viaja sin cifrar por `SecurityMode.None` (conflicto `C-2`, ya registrado) |
| C. Solo en el cliente | El cliente recuerda su `player_id` y lo envía | Trivial | **Descartada**: cualquiera podría hacerse pasar por otro, contra `RES-05` |
| D. En la base | Tabla de sesiones | Sobrevive a un reinicio del servidor | **Descartada**: la entidad `session` ya se descartó en el modelo |

**Recomendación: B.** Es la única que cumple a la vez `RES-05`, `IT3-D04` y `DR-30` sin atar todo a
un solo contrato. En el cliente haría falta además un objeto que recuerde lo que se muestra
(username y si es cuenta o invitado; nunca la contraseña), creado en `TorresGame` y pasado a las
pantallas como hoy se pasa `LanguageService`. Lo necesitan la barra superior y `CU-12` FA02
(resaltar la fila propia). Su nombre y el del elemento del servidor están por decidir.

---

## 5. Cómo funciona ahora la conexión, aplicada a Torres

```
┌──────────────────────────────────────────────────────────────────────────┐
│ GUI           Torres.Client  ·  StartupScreen                            │
│               pide el estado, no espera, pregunta en cada Update         │
└───────────────┬──────────────────────────────────────────────────────────┘
                │ IServerStatusService.GetStatusAsync()  (Game.Contracts)
                │ WCF sobre net.tcp://localhost:8000/status, SecurityMode.None
┌───────────────▼──────────────────────────────────────────────────────────┐
│ Lógica        Game.Services  ·  ServerStatusService (host CoreWCF)       │
│               atrapa el fallo de la base y lo devuelve como código       │
└───────────────┬──────────────────────────────────────────────────────────┘
                │ PlayerRepository.CountAsync()
┌───────────────▼──────────────────────────────────────────────────────────┐
│ Acceso a datos Game.Services/Persistence  ·  Dapper sobre Npgsql         │
│               NpgsqlDataSource: reparto de conexiones                    │
└───────────────┬──────────────────────────────────────────────────────────┘
                │ TCP localhost:5432, usuario y contraseña del .env
┌───────────────▼──────────────────────────────────────────────────────────┐
│ PostgreSQL 18 base torres, esquema torres                                │
└───────────────┬──────────────────────────────────────────────────────────┘
                │ SELECT count(*) FROM player
┌───────────────▼──────────────────────────────────────────────────────────┐
│ Tabla         player                                                     │
└──────────────────────────────────────────────────────────────────────────┘
```

### Qué sabe cada pieza

| Pieza | Sabe | No sabe |
|---|---|---|
| `Torres.Client` | La dirección del servidor y el contrato | Que existe una base, dónde está, su contraseña |
| `Game.Contracts` | Qué operaciones hay y qué tipos cruzan la red | Cómo se implementan; ningún texto traducible |
| `Game.Services` | Cómo atender cada operación, qué hacer si la base falla | Cómo se dibuja nada; los idiomas |
| `Game.Persistence` (carpeta del servidor) | El SQL y cómo conectarse | Quién la llama y para qué |
| PostgreSQL | Tablas, permisos y restricciones | Que existe un juego |

**La consecuencia que más importa:** las credenciales de la base viven solo en la máquina del
servidor. El riesgo de la versión 1 (credenciales en cada cliente, `CON-06`) desaparece.

### El recorrido de la comprobación

1. `TorresGame` crea al arrancar un `ChannelFactory<IServerStatusService>` y le pasa a
   `StartupScreen` un canal.
2. En el primer `Update`, `StartupScreen` llama a `GetStatusAsync()`. La llamada sale por la red y
   la pantalla **no espera**: cada fotograma mira si el `Task` terminó.
3. CoreWCF recibe la llamada en `Game.Services` y la entrega a `ServerStatusService`.
4. El servicio pide a `PlayerRepository` el conteo de jugadores. Dapper abre una conexión del
   reparto, ejecuta el SQL y devuelve un `long`.
5. Si PostgreSQL responde, el servicio devuelve `ServerStatus.Ready`. Si no, registra el error con
   log4net y devuelve `ServerStatus.DatabaseUnavailable`. **El fallo cruza la red como código, no
   como excepción ni como texto** (lo fijó la iteración 2 de ADD).
6. El cliente traduce el código a una clave: `Startup.ReadyStatus` y pasa al menú, o
   `Common.ServerErrorTitle` en rojo. Si el servidor ni siquiera está arrancado, la llamada falla
   con `EndpointNotFoundException` y se muestra la misma clave.

---

## 6. Qué se implementó, archivo por archivo

### 6.1 `src/Game.Contracts/` (netstandard2.0)

| Archivo | Qué contiene | Por qué así |
|---|---|---|
| `Game.Contracts.csproj` | `netstandard2.0` y `System.ServiceModel.Primitives` 10.0.652802 | `RES-08` y `STACK.md` §3: un solo ensamblado que sirve a cliente y servidor. Copiado de la prueba de humo del equipo (`DuplexSmokeTest.Contracts`) |
| `IServerStatusService.cs` | `[ServiceContract]` con `Task<ServerStatus> GetStatusAsync()` y `[OperationContract(Name = "GetStatus")]` | El `Name` fija el nombre en la red para que no dependa de cómo cada lado trate el sufijo `Async` (lección de la prueba de humo). Lleva documentación XML porque la regla 7.3 la exige en las operaciones que consume el cliente |
| `ServerStatus.cs` | Enumerado `[DataContract]` con `Ready` y `DatabaseUnavailable` | Es el código de fallo que cruza la red. `[EnumMember]` hace que cada valor sea serializable |

Es un contrato **sin callback**. La comprobación de estado es pregunta y respuesta; el canal
duplex llegará con las salas y la partida.

### 6.2 `src/Game.Services/Persistence/` (espacio de nombres `Game.Persistence`)

| Archivo | Qué contiene | Por qué |
|---|---|---|
| `DatabaseSettings.cs` | Construye el `NpgsqlDataSource` desde `TORRES_DB_HOST`, `TORRES_DB_USER` y `TORRES_DB_PASSWORD` | Base `torres`, esquema `torres` y puerto 5432 salen de los scripts de la Actividad 1. `SearchPath = torres` es obligatorio: sin él, `relation "player" does not exist` (comprobado). Exige usuario y contraseña porque, sin ellos, Npgsql entra en silencio con el usuario del sistema operativo |
| `PlayerRepository.cs` | `CountAsync()`: abre conexión del reparto y ejecuta `SELECT count(*) FROM player` con `ExecuteScalarAsync<long>` de Dapper | Regla 4.3: un repositorio por entidad. Aquí crecerán la búsqueda por nombre y el alta |

```csharp
public async Task<long> CountAsync()
{
    await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

    return await connection.ExecuteScalarAsync<long>(CountSql);
}
```

| Línea | Qué hace |
|---|---|
| `OpenConnectionAsync()` | Toma una conexión del reparto; si no hay libre, abre una |
| `await using` | La devuelve al reparto al salir, aunque haya excepción (regla 8.11) |
| `ExecuteScalarAsync<long>` | Método de Dapper: ejecuta y convierte la primera columna al tipo pedido. Sin Dapper habría que crear el comando, ejecutarlo y convertir el `object` a mano |

### 6.3 `src/Game.Services/` (net10.0, SDK web): el resto del servidor

| Archivo | Qué contiene | Por qué |
|---|---|---|
| `Game.Services.csproj` | CoreWCF.NetTcp y CoreWCF.Primitives 1.9.1, Dapper 2.1.89, Npgsql 10.0.3, DotNetEnv 3.2.0 y log4net 3.4.0; referencia solo a `Game.Contracts` | La iteración 1 de ADD dice que `Game.Services` aloja el host de CoreWCF. El SDK web es el que usa el host genérico de ASP.NET Core, como en la prueba de humo |
| `Program.cs` | Carga `.env`, configura log4net, crea el reparto de conexiones, registra los servicios y abre el punto `net.tcp://localhost:8000/status` con `SecurityMode.None` | `SecurityMode.None` es la regla 2 de `STACK.md` §9 |
| `ServerStatusService.cs` | Implementa el contrato: llama a `CountAsync`, registra el conteo o el error y devuelve el código | Regla 4.2: el sufijo `Service` es de la capa de servicios e implementa una interfaz `I…Service` |
| `log4net.config` | Un apéndice de consola con fecha, nivel y clase | Regla 9.1: el registro se hace con log4net, nunca con `Console.WriteLine` |

```csharp
public async Task<ServerStatus> GetStatusAsync()
{
    long playerCount;
    try
    {
        playerCount = await _playerRepository.CountAsync();
    }
    catch (NpgsqlException exception)
    {
        _log.Error("The database could not be read during the status check.", exception);
        return ServerStatus.DatabaseUnavailable;
    }

    _log.InfoFormat("Status check answered; the player table has {0} rows.", playerCount);
    return ServerStatus.Ready;
}
```

| Parte | Por qué |
|---|---|
| `catch (NpgsqlException)` | Regla 8.9: solo lo que se sabe manejar. `PostgresException` (rol inexistente, permiso negado) hereda de ella |
| Registrar y devolver código | Regla 8.9: una excepción capturada se registra o se propaga, nunca se descarta |
| `InstanceContextMode.Single` y `ConcurrencyMode.Multiple` | Una instancia atiende a todos los clientes a la vez, como en la prueba de humo. Es seguro porque el servicio no guarda estado propio |

Dos detalles de `Program.cs`:

- El registro de los puntos de conexión va en un método con nombre (`AddEndpoints`) y no en una
  lambda, porque la regla 6.30 prohíbe lambdas de más de una expresión.
- El reparto de conexiones se crea **antes** de arrancar el host. Si falta una variable del
  `.env`, el servidor no arranca y dice cuál falta.

### 6.4 `src/Torres.Client/` (cambios)

| Archivo | Cambio | Por qué |
|---|---|---|
| `Torres.Client.csproj` | `System.ServiceModel.NetTcp` 10.0.652802, `System.Security.Cryptography.Xml` 10.0.11 y referencia a `Game.Contracts` | Las dos versiones son las de `STACK.md`; la segunda corrige CVE-2026-50648 |
| `TorresGame.cs` | Crea el `ChannelFactory<IServerStatusService>` y se lo pasa a `StartupScreen`; en `Dispose` lo aborta. Cierra el juego también cuando `StartupScreen.WasExitRequested` | Es donde el juego ya crea lo que comparten las pantallas. `Abort` cierra sin esperar respuesta: al salir no hay nada que esperar y no puede lanzar |
| `TorresGame.cs` | `: Game` pasa a `: Microsoft.Xna.Framework.Game` | Ver 8.1: el espacio de nombres `Game` del estándar choca con la clase de MonoGame |
| `Screens/StartupScreen.cs` | El temporizador falso de 1,6 y 2,4 s se sustituye por la llamada al servidor. Si falla, muestra el aviso de PT-01 con «Reintentar» y «Salir» | «Listo» ahora significa que el servidor y su base respondieron. «Reintentar» descarta el canal fallido (un canal WCF que falló queda inservible), pide uno nuevo a la fábrica y vuelve a preguntar |

```csharp
internal override void Update(GameTime gameTime)
{
    _statusCheck ??= StartStatusCheck();
    if ((_statusLabel is null) || (_failureNotice is null) || !_statusCheck.IsCompleted)
    {
        return;
    }

    bool isServerReady = _statusCheck.IsCompletedSuccessfully && (_statusCheck.Result == ServerStatus.Ready);
    if (isServerReady)
    {
        ShowReady(_statusLabel, gameTime);
        return;
    }

    _statusLabel.Visible = false;
    _failureNotice.Visible = true;
}
```

| Parte | Por qué |
|---|---|
| `??=` | Lanza la llamada en el primer fotograma. «Reintentar» pone `_statusCheck` en `null`, y el siguiente fotograma vuelve a lanzarla con un canal nuevo |
| `IsCompleted` antes que nada | `Update` corre sesenta veces por segundo; esperar congelaría la ventana. Además MonoGame no tiene contexto de sincronización y **los controles de Myra solo se tocan desde el hilo del juego**, que es donde corre `Update` |
| `IsCompletedSuccessfully` antes de `Result` | Si el `Task` falló (servidor apagado), leer `Result` lanzaría la excepción |
| Dos causas, un aviso | Servidor apagado y base caída muestran el mismo aviso (`Common.ServerErrorTitle` y `Common.ServerErrorDetail`, el texto de `EX01` de los casos de uso): el jugador no puede hacer nada distinto en un caso y en otro |

### 6.5 D7: `Screens/RoomsScreen.cs`

Completada según **PT-14** del prototipo: búsqueda por código con «Buscar» y «Crear sala»,
encabezado «Salas disponibles» con el distintivo «Solo públicas» y «Actualizar», estado vacío,
aviso de salas privadas y «Volver al menú» al pie, que regresa al menú.

| Qué había en el WIP | Qué se hizo |
|---|---|
| Un tipo `Room` que no existe en ningún proyecto | Se quitó. La lista de salas vendrá del servidor con una operación del contrato cuando se implemente el caso de uso de salas, que no es de esta actividad. Hasta entonces la pantalla muestra el estado vacío (`Rooms.EmptyStateTitle` y `Rooms.EmptyStateHint`) |
| Manejadores vacíos con `//TODO` | Se quitaron. «Buscar», «Crear sala» y «Actualizar» se muestran sin acción, igual que «Guardar» en `ProfileScreen`: no hay aún pantalla de crear sala ni operación de búsqueda |
| `var` con llamadas a métodos, `buildRoomRow` en minúscula, `Avaliable` | Corregidos según las reglas 6.13 y 3.11 |

---

## 7. Cómo ejecutarlo en tu máquina

### 7.1 Lo que ya quedó configurado

| Qué | Estado |
|---|---|
| Rol `torres_app` | Creado con `database/actividad-1/crear_usuario_permisos.sql`, sin editar el script, y con una contraseña aleatoria de 32 caracteres |
| `.env` | En la raíz del repositorio, con `TORRES_DB_HOST`, `TORRES_DB_USER` y `TORRES_DB_PASSWORD`. Excluido de git y con permisos `600` (solo tu usuario lo lee). La plantilla sin contraseña, `.env.example`, sí se versiona |
| `pg_hba.conf` | Dos líneas nuevas al principio que **exigen contraseña a `torres_app`** en `localhost` (`scram-sha-256`). Las demás conexiones siguen en `trust`, así que `postgres`, tu usuario y las fuentes de datos de Rider entran igual que antes. Respaldo del original: `/opt/homebrew/var/postgresql@18/pg_hba.conf.respaldo-2026-09-24` |
| Configuraciones de Rider | En `.run/`: **Servidor**, **Cliente** y **Servidor y cliente** (lanza los dos). Rider las lee del repositorio y aparecen en el selector de ejecución |

Comprobado después de configurar: `torres_app` entra con la contraseña del `.env`, es rechazado con
una incorrecta (`password authentication failed`), no puede borrar de `report` (`permission denied`,
como manda la matriz CRUD) y `postgres` sigue entrando sin contraseña.

**Para un compañero en otra máquina:** ejecutar el script de permisos, darle una contraseña a
`torres_app`, copiar `.env.example` a `.env` y escribirla ahí.

### 7.2 Ejecutar

PostgreSQL tiene que estar arrancado:

```bash
LC_ALL=C /opt/homebrew/opt/postgresql@18/bin/pg_ctl -D /opt/homebrew/var/postgresql@18 start
```

**Desde Rider:** elegir **Servidor y cliente** y ejecutar. Si el cliente abre antes de que el
servidor termine de arrancar, mostrará el aviso de conexión: basta con pulsar «Reintentar».

**Desde la terminal**, en la raíz del repositorio, primero el servidor:

```bash
dotnet run --project src/Game.Services
```

Debe decir `Server listening on net.tcp://localhost:8000/status`. CoreWCF añade la línea
`Now listening on: http://0.0.0.0:8000`: es un mensaje del host y no significa que atienda HTTP.
Y en otra terminal, el cliente:

```bash
dotnet run --project src/Torres.Client
```

### 7.3 Qué probar

| Prueba | Qué debe pasar |
|---|---|
| Servidor y base arrancados | «Conectando…», «Listo» y el menú. En el servidor: `Status check answered; the player table has 6 rows.` |
| Cliente sin servidor | El aviso con «Reintentar» y «Salir» |
| Encender el servidor y pulsar «Reintentar» | «Listo» y el menú |
| Contraseña equivocada en `.env` | Aviso en el cliente; en el servidor, `password authentication failed for user "torres_app"` |
| Cambiar el idioma a inglés y repetir | Los mismos estados, en inglés |
| Menú, «Salas» | La pantalla de salas completa |

---

## 8. Hallazgos al implementar

### 8.1 El espacio de nombres `Game` choca con MonoGame

Al referenciar `Game.Contracts` desde el cliente, `TorresGame : Game` dejó de compilar:
`'Game' es espacio de nombres pero se usa como tipo`. C# busca primero en los espacios de nombres
y encuentra `Game` (el del estándar) antes que la clase `Microsoft.Xna.Framework.Game`. Se resolvió
calificando la clase base.

**Consecuencia para D3:** si algún día el cliente pasa a llamarse `Game.Client`, dentro de él
`Game` significará siempre el espacio de nombres, y cada uso de la clase de MonoGame tendrá que ir
calificado. Además cambiaría el nombre del recurso que lee `LocalizedText`
(`Torres.Client.Resources.Strings`) y el espacio de nombres que escribe `generar-recursos.py`. Por
eso el cliente conserva su nombre; renombrarlo es decisión aparte.

### 8.2 El indicador de conexión de la barra superior no se entera

Con el servidor respondiendo, la barra superior sigue mostrando «Sin conexión»: `TopBarView`
pinta ese estado fijo. Hay que conectarlo al resultado de `StartupScreen`.

### 8.3 El canal no se reintentaba (resuelto)

En la versión 2, si el servidor no respondía al arrancar, el cliente se quedaba en el error. Ahora
`StartupScreen` muestra el aviso de PT-01 con «Reintentar» y «Salir».

### 8.4 La dirección del servidor es fija

`net.tcp://localhost:8000/status` es una constante de `TorresGame`. Para probar entre dos máquinas
habrá que llevarla a configuración.

### 8.5 log4net funciona sobre .NET 10

Era el conflicto `C-3` de la base oficial, «verificar log4net sobre .NET 10». Queda verificado para
el apéndice de consola.

---

## 9. Evidencia de la etapa 1

Todo se ejecutó el 24 de septiembre de 2026 en este Mac, con el juego real (el código de
`src/Torres.Client` compilado en un arnés que captura la pantalla) y el servidor real.

| Prueba | Cultura | Resultado |
|---|---|---|
| Servidor con base disponible | es-MX | «Listo» y, 0,8 s después, el menú principal |
| Servidor con base disponible | en-US | «Ready» y el menú principal |
| Servidor arrancado con un rol que no existe | es-MX | «No se pudo establecer una conexión con el servidor.» en rojo. El servidor registra `28000: role "torres_app" does not exist` |
| Lo mismo | en-US | «Couldn't establish a connection with the server.» |
| Servidor apagado | en-US | El mismo mensaje. La llamada falla con `EndpointNotFoundException` |
| Pantalla de salas | es-MX y en-US | Completa según PT-14 en los dos idiomas |
| Cliente arrancado sin servidor; después se enciende el servidor y se pulsa «Reintentar» | es-MX y en-US | Aviso de PT-01, luego «Listo» y el menú. El servidor leyó sus credenciales **solo del `.env`** y entró como `torres_app` con contraseña comprobada |
| Registro del servidor en cada comprobación | | `Status check answered; the player table has 6 rows.` |
| Compilación de la solución | | 0 advertencias, 0 errores |
| Paquetes vulnerables | | Ninguno en `Game.Services` ni en `Torres.Client` |

Las primeras pruebas se hicieron con tu usuario de sistema, antes de crear `torres_app`; la del
reintento, ya con `torres_app` y el `.env`. El botón «Salir» del aviso no se pulsó en las pruebas.
Tu preferencia de idioma se respaldó y se restauró (`en`).

---

## 10. Cómo se avanza después

Cada funcionalidad sigue el mismo camino que la comprobación: operación en el contrato, servicio
en `Game.Services`, método en el repositorio, pantalla que pregunta en `Update`.

### 10.1 Inicio de sesión (`CU-01`)

| Capa | Qué se añade |
|---|---|
| `Game.Contracts` | Una operación de inicio de sesión y un enumerado con los resultados: correcto, credenciales incorrectas, cuenta bloqueada. Su forma depende de D8 |
| `Game.Persistence` | `PlayerRepository`: buscar por `lower(username) = lower(@username)` con `QueryFirstOrDefaultAsync<T>` de Dapper, y consultar un baneo permanente en `sanction` |
| `Game.Services` | Comprobar la contraseña con BCrypt.Net-Next (D6). El hash no sale nunca del servidor |
| `Torres.Client` | `LoginScreen`: guardar los dos campos, validar vacíos con `Common.RequiredField` **sin llamar al servidor** (`CU-01` FA01), y mostrar `Login.InvalidCredentials`, `Login.AccountBannedTitle` o `Common.ServerErrorTitle` según el resultado |

Para mapear columnas como `password_hash` a propiedades como `PasswordHash`, Dapper necesita
`DefaultTypeMap.MatchNamesWithUnderscores = true` al arrancar el servidor.

Obstáculo para probarlo: las contraseñas de los seis jugadores de prueba no constan en ninguna
parte; primero hay que crear una cuenta con el registro.

### 10.2 Creación de usuarios (`CU-02`)

| Capa | Qué se añade |
|---|---|
| `Torres.Client` | Validar vacíos y formato con las claves de D5 antes de llamar (`CU-02` FA04 y FA05) |
| `Game.Services` | Volver a validar: el servidor no se fía del cliente. Calcular el hash BCrypt con el coste 11 de `system_parameter.password_hash_cost` |
| `Game.Persistence` | `INSERT INTO player (username, email, password_hash) VALUES (...) RETURNING player_id`. Si dos altas compiten, los índices `ux_player_username_lower` y `ux_player_email_lower` responden con el código `23505`, que se traduce a `Register.UsernameUnavailable` o `Register.EmailAlreadyExists` |

### 10.3 Consulta y visualización de puntuaciones (`CU-12`)

| Capa | Qué se añade |
|---|---|
| `Game.Contracts` | Una operación que devuelve la lista y un `[DataContract]` por fila: username, victorias, puntos, partidas |
| `Game.Persistence` | `SELECT username, wins, total_score, matches_played FROM player_ranking ORDER BY wins DESC, total_score DESC, matches_played ASC`, el orden literal de `CU-12` paso 2, con `QueryAsync<T>` |
| `Torres.Client` | Pantalla nueva con las 8 claves `GlobalRanking.*`, valor nuevo en `ScreenId`, registro en `TorresGame` y el botón «Ranking» del menú habilitado (hoy `MenuItem(TextKeys.MainMenu.RankingButton, false)`). Resaltar la fila propia depende de D8 |

### 10.4 Internacionalización en todo lo anterior

- Ninguna capa por debajo del cliente devuelve texto: devuelven códigos y datos.
- El cliente elige la clave; `LocalizedLabel` la traduce.
- Los números se formatean con la cultura activa, que `LanguageService` ya fija en `es-MX` o
  `en-US`.
- Los datos (usernames) no se traducen.

---

## 11. Qué queda pendiente

| Requisito de la actividad | Estado |
|---|---|
| Integración con la base de la Actividad 1 | Hecha y comprobada para una lectura, de extremo a extremo |
| ORM equivalente a EF | Elegido e integrado (Dapper) |
| GUI de inicio de sesión | Existe; falta conectarla (10.1) |
| GUI de creación de usuarios | Existe; falta conectarla (10.2) |
| GUI de consulta de puntuaciones | No existe (10.3) |
| Internacionalización | Mecanismo listo; claves de validación añadidas; falta la pantalla de ranking |
| Validación de campos | Claves listas; falta el código |
| Usuarios contra la base | Pendiente; necesita D8 y BCrypt |
| Puntuaciones | Pendiente |
| Evidencia en dos culturas | Hecha para el arranque y la pantalla de salas; faltan las tres GUI de la actividad |

Antes de seguir: decidir D8.
