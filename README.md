# Torres

Juego de mesa por turnos en C#. Cliente de escritorio en MonoGame, servidor con CoreWCF
duplex sobre net.tcp.

## Estructura

| Carpeta | Contenido |
|---|---|
| `src/Torres.Client/` | Cliente de escritorio |
| `src/Game.Contracts/` | Contratos del servidor; el cliente no los referencia, genera los suyos a partir de ellos |
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
| `Torres.Client.sln` | Cliente: Torres.Client |

El cliente no depende del servidor para compilar ni para arrancar: no referencia ningun proyecto del
servidor y tiene sus propios `Directory.Build.props`, `.editorconfig` y `CodeMetricsConfig.txt`.
Los ejemplos de `examples/` se abren por su propio `.csproj`.

## Ejecutar el servidor

```
dotnet run --project src/Game.Server
```

## Ejecutar el cliente

```
dotnet run --project src/Torres.Client
```

## Jugar en red local

1. En el equipo servidor, ver su IP (en macOS: `ipconfig getifaddr en0`).
2. En cada cliente, crear `src/Torres.Client/appsettings.Local.json` con la direccion del servidor:

   ```
   {
     "ServerAddress": "net.tcp://192.168.1.79:8000"
   }
   ```

   Git lo ignora y, si existe, sustituye a la direccion de `appsettings.json`. Se copia junto al
   ejecutable al compilar; si se borra, hay que borrar tambien la copia de `bin/`.
3. Si el servidor corre en Windows, abrir el puerto 8000 de entrada en su firewall.

Las redes que aislan a los equipos entre si, como suele pasar en el Wi-Fi de la universidad, no
sirven; el punto de acceso de un celular si.

## Regenerar los contratos del cliente

El cliente usa `src/Torres.Client/Contracts/GameContracts.cs`, generado con `dotnet-svcutil` a partir
de la descripcion que publica el servidor. Se guarda en el repositorio junto con esa descripcion
(`Contracts/Metadata/`), para compilar con el servidor apagado. **Cada cambio de contrato se
regenera en el mismo commit**: si no, el error no sale al compilar, sale al jugar.

1. Arrancar el servidor con la descripcion publicada:

   ```
   Server__PublishesServiceMetadata=true dotnet run --project src/Game.Server
   ```

2. Generar:

   ```
   python3 tools/generar-contratos-cliente.py
   ```

La herramienta esta fijada en `dotnet-tools.json`. El puerto de la descripcion es
`Server:MetadataHttpPort` (8001) y solo escucha en `localhost`. Si un servicio es nuevo, su ruta se
anade tambien a `ServicePaths` en el `appsettings.json` del cliente.

## Regenerar los recursos de internacionalizacion

Los textos salen de `docs/i18n/Diccionario-i18n-Torres.xlsx`, que manda sobre los `.resx`.
Tras cambiar el diccionario:

```
python3 tools/generar-recursos.py
```

Requiere Python 3 con openpyxl. Los archivos que produce no se editan a mano.
