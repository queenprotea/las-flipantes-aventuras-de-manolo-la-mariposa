# Torres

Juego de mesa por turnos en C#. Cliente de escritorio en MonoGame, servidor con CoreWCF
duplex sobre net.tcp.

## Estructura

| Carpeta | Contenido |
|---|---|
| `src/Torres.Client/` | Cliente de escritorio |
| `tools/` | Utilidades de desarrollo |
| `database/` | Modelo y scripts de la base de datos |
| `examples/` | Proyectos de prueba, fuera del producto |
| `docs/` | Documentacion del proyecto |

## Documentacion

| Ruta | Que es |
|---|---|
| `docs/STACK.md` | Stack, decisiones e instalacion del entorno |
| `docs/estandar/` | Estandar de codificacion del equipo |
| `docs/reglas/` | Reglas del juego |
| `docs/documentos/` | Casos de uso, historias de usuario, arquitectura y base de datos |
| `docs/analisis/` | Analisis previos a cada documento |
| `docs/i18n/` | Diccionario de internacionalizacion y sus guias |
| `docs/prototipo/` | Prototipo interactivo de pantallas |

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
