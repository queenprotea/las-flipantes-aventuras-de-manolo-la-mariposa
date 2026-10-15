# Torres

Juego de mesa por turnos en C#. Cliente de escritorio en MonoGame, servidor con CoreWCF
duplex sobre net.tcp.

## Estructura

| Carpeta | Contenido |
|---|---|
| `src/Torres.Client/` | Cliente de escritorio |
| `src/Game.Contracts/` | Contratos que comparten cliente y servidor |
| `src/Game.Server/` | Host del servidor: arranca, configura y abre net.tcp |
| `src/Game.Services/` | Atiende las llamadas del cliente |
| `src/Game.Match/` | Partida en curso (por construir) |
| `src/Game.Domain/` | Reglas del juego (por construir) |
| `src/Game.Persistence/` | Acceso a PostgreSQL con EF Core |
| `tools/` | Utilidades de desarrollo |
| `database/` | Modelo y scripts de la base de datos |
| `examples/` | Proyectos de prueba, fuera del producto |
| `docs/` | Documentacion del proyecto |

## Documentacion

| Ruta | Que es |
|---|---|
| `docs/STACK.md` | Stack, decisiones e instalacion del entorno |
| `docs/ARQUITECTURA.md` | Como se organizan los proyectos, como se comunican y como se amplian |
| `docs/estandar/` | Estandar de codificacion del equipo |
| `docs/reglas/` | Reglas del juego |
| `docs/documentos/` | Casos de uso, historias de usuario, arquitectura y base de datos |
| `docs/analisis/` | Analisis previos a cada documento |
| `docs/i18n/` | Diccionario de internacionalizacion y sus guias |
| `docs/prototipo/` | Prototipo interactivo de pantallas |

## Soluciones

| Solucion | Proyectos |
|---|---|
| `Torres.Server.sln` | Servidor: Game.Server, Game.Services, Game.Match, Game.Domain, Game.Persistence, Game.Contracts |
| `Torres.Client.sln` | Cliente: Torres.Client, Game.Contracts |
| `Torres.sln` | Todo, incluidos los ejemplos; la usa la configuracion de Rider "Servidor y cliente" |

## Ejecutar el servidor

```
dotnet run --project src/Game.Server
```

## Ejecutar el cliente

```
dotnet run --project src/Torres.Client
```

## Regenerar los recursos de internacionalizacion

Los textos salen de `docs/i18n/Diccionario-i18n-Torres.xlsx`, que manda sobre los `.resx`.
Tras cambiar el diccionario:

```
python3 tools/generar-recursos.py
```

Requiere Python 3 con openpyxl. Los archivos que produce no se editan a mano.
