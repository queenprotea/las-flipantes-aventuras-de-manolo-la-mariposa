#!/usr/bin/env python3
"""Exporta el último análisis de SonarQube de Torres a Markdown y JSON.

Uso:
    python3 tools/exportar-sonar.py              # proyecto torres-client
    python3 tools/exportar-sonar.py servidor     # proyecto torres-server
    python3 tools/exportar-sonar.py todo         # proyecto torres
    python3 tools/exportar-sonar.py <clave>      # cualquier otra clave de proyecto

Deja en reportes-sonar/ (en la raíz del repo):
    <clave>-ultimo.md     reporte legible: métricas, problemas por archivo y por regla, hotspots
                          y cobertura por archivo (total y de código nuevo)
    <clave>-ultimo.json   los mismos datos sin procesar
    <clave>-AAAAMMDD-HHMM.md   copia con fecha, para comparar con reportes anteriores

El token se toma de la variable SONAR_TOKEN o de ~/.config/sonar/torres-token,
el mismo archivo que usa tools/analizar-sonar.sh. Si no hay ninguno, lo pide.
"""

import base64
import datetime
import getpass
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
from collections import Counter, defaultdict

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CARPETA_REPORTES = os.path.join(RAIZ, "reportes-sonar")
DIRECCION = os.environ.get("SONAR_URL", "http://localhost:9000").rstrip("/")
ARCHIVO_TOKEN = os.environ.get(
    "SONAR_TOKEN_FILE", os.path.expanduser("~/.config/sonar/torres-token")
)
PROYECTOS = {"cliente": "torres-client", "servidor": "torres-server", "todo": "torres"}
TAMANO_PAGINA = 500
LIMITE_API = 10000

METRICAS = [
    ("alert_status", "Quality gate"),
    ("ncloc", "Líneas de código"),
    ("bugs", "Bugs"),
    ("vulnerabilities", "Vulnerabilidades"),
    ("security_hotspots", "Security hotspots"),
    ("code_smells", "Code smells"),
    ("sqale_index", "Deuda técnica (min)"),
    ("reliability_rating", "Calificación de confiabilidad"),
    ("security_rating", "Calificación de seguridad"),
    ("sqale_rating", "Calificación de mantenibilidad"),
    ("duplicated_lines_density", "Líneas duplicadas (%)"),
    ("duplicated_blocks", "Bloques duplicados"),
    ("coverage", "Cobertura (%)"),
    ("complexity", "Complejidad ciclomática"),
    ("cognitive_complexity", "Complejidad cognitiva"),
    ("software_quality_maintainability_issues", "Problemas de mantenibilidad"),
    ("software_quality_reliability_issues", "Problemas de confiabilidad"),
    ("software_quality_security_issues", "Problemas de seguridad"),
]
CALIFICACIONES = {"1.0": "A", "2.0": "B", "3.0": "C", "4.0": "D", "5.0": "E"}
METRICAS_COBERTURA = [
    "coverage", "lines_to_cover", "uncovered_lines",
    "new_coverage", "new_lines_to_cover", "new_uncovered_lines",
]
ARCHIVOS_EN_TABLA_TOTAL = 40
ORDEN_SEVERIDAD = ["BLOCKER", "HIGH", "CRITICAL", "MEDIUM", "MAJOR", "LOW", "MINOR", "INFO"]


class ErrorSonar(Exception):
    pass


def leer_token():
    token = os.environ.get("SONAR_TOKEN", "").strip()
    if not token and os.path.isfile(ARCHIVO_TOKEN):
        with open(ARCHIVO_TOKEN, encoding="utf-8") as archivo:
            token = archivo.read().strip()
    if not token:
        token = getpass.getpass("Token de SonarQube: ").strip()
    return token


def pedir(ruta, parametros, token):
    url = f"{DIRECCION}{ruta}?{urllib.parse.urlencode(parametros)}"
    credencial = base64.b64encode(f"{token}:".encode()).decode()
    peticion = urllib.request.Request(url, headers={"Authorization": f"Basic {credencial}"})
    try:
        with urllib.request.urlopen(peticion, timeout=30) as respuesta:
            return respuesta.read().decode("utf-8")
    except urllib.error.HTTPError as error:
        detalle = error.read().decode(errors="replace")[:300]
        raise ErrorSonar(f"{ruta} respondió {error.code}: {detalle}") from error
    except urllib.error.URLError as error:
        raise ErrorSonar(f"no se pudo conectar con {DIRECCION} ({error.reason})") from error


def consultar(ruta, parametros, token):
    return json.loads(pedir(ruta, parametros, token))


def leer_version(token):
    try:
        return pedir("/api/server/version", {}, token).strip()
    except ErrorSonar:
        return None


def revisar_servidor(token):
    try:
        estado = consultar("/api/system/status", {}, token).get("status")
    except ErrorSonar as error:
        raise ErrorSonar(f"SonarQube no responde: {error}. ¿Está arrancado?") from error
    if estado != "UP":
        raise ErrorSonar(f"SonarQube está en estado {estado}, todavía no está listo.")
    validacion = consultar("/api/authentication/validate", {}, token)
    if not validacion.get("valid"):
        raise ErrorSonar("SonarQube rechazó el token.")


def leer_metricas(clave, token):
    existentes = {
        metrica["key"]
        for metrica in consultar("/api/metrics/search", {"ps": 500}, token).get("metrics", [])
    }
    pedidas = [codigo for codigo, _ in METRICAS if codigo in existentes]
    respuesta = consultar(
        "/api/measures/component",
        {"component": clave, "metricKeys": ",".join(pedidas)},
        token,
    )
    return {medida["metric"]: medida.get("value") for medida in respuesta["component"].get("measures", [])}


def leer_paginas(ruta, parametros, campo, token):
    elementos = []
    extras = defaultdict(list)
    pagina = 1
    while True:
        respuesta = consultar(ruta, {**parametros, "ps": TAMANO_PAGINA, "p": pagina}, token)
        elementos.extend(respuesta.get(campo, []))
        for nombre in ("rules", "components"):
            extras[nombre].extend(respuesta.get(nombre, []))
        total = respuesta.get("paging", {}).get("total", respuesta.get("total", 0))
        if len(elementos) >= total or pagina * TAMANO_PAGINA >= LIMITE_API or not respuesta.get(campo):
            return elementos, extras, total
        pagina += 1


def leer_problemas(clave, token):
    parametros = {"resolved": "false", "additionalFields": "rules"}
    try:
        return leer_paginas("/api/issues/search", {**parametros, "components": clave}, "issues", token)
    except ErrorSonar:
        return leer_paginas("/api/issues/search", {**parametros, "componentKeys": clave}, "issues", token)


def leer_hotspots(clave, token):
    try:
        hotspots, _, total = leer_paginas("/api/hotspots/search", {"project": clave}, "hotspots", token)
        return hotspots, total
    except ErrorSonar:
        return [], 0


def valor_de_medida(medida):
    if "value" in medida:
        return medida["value"]
    periodo = medida.get("period") or (medida.get("periods") or [{}])[0]
    return periodo.get("value")


def leer_cobertura(clave, token):
    archivos = []
    pagina = 1
    while True:
        respuesta = consultar(
            "/api/measures/component_tree",
            {
                "component": clave,
                "qualifiers": "FIL",
                "metricKeys": ",".join(METRICAS_COBERTURA),
                "ps": TAMANO_PAGINA,
                "p": pagina,
            },
            token,
        )
        for componente in respuesta.get("components", []):
            medidas = {medida["metric"]: valor_de_medida(medida) for medida in componente.get("measures", [])}
            if entero(medidas.get("lines_to_cover")) > 0 or entero(medidas.get("new_lines_to_cover")) > 0:
                archivos.append({"archivo": componente.get("path") or componente.get("key"), **medidas})
        total = respuesta.get("paging", {}).get("total", 0)
        if pagina * TAMANO_PAGINA >= min(total, LIMITE_API) or not respuesta.get("components"):
            return archivos
        pagina += 1


def leer_opcional(ruta, parametros, token):
    try:
        return consultar(ruta, parametros, token)
    except ErrorSonar:
        return {}


def severidad_de(problema):
    impactos = problema.get("impacts") or []
    if impactos:
        severidades = [impacto.get("severity", "") for impacto in impactos]
        return min(severidades, key=lambda s: ORDEN_SEVERIDAD.index(s) if s in ORDEN_SEVERIDAD else 99)
    return problema.get("severity", "?")


def calidad_de(problema):
    impactos = problema.get("impacts") or []
    if impactos:
        return "/".join(sorted({impacto.get("softwareQuality", "?") for impacto in impactos}))
    return problema.get("type", "?")


def archivo_de(componente, clave):
    return componente.split(":", 1)[1] if componente.startswith(f"{clave}:") else componente


def peso(severidad):
    return ORDEN_SEVERIDAD.index(severidad) if severidad in ORDEN_SEVERIDAD else 99


def valor_metrica(codigo, valor):
    if valor is None:
        return "—"
    if codigo.endswith("_rating"):
        return CALIFICACIONES.get(valor, valor)
    if codigo == "alert_status":
        return {"OK": "Aprobado", "ERROR": "Reprobado"}.get(valor, valor)
    return valor


def escapar(texto):
    return (texto or "").replace("|", "\\|").replace("\n", " ")


def escribir_markdown(datos):
    clave = datos["proyecto"]
    nombres_regla = {regla["key"]: regla.get("name", "") for regla in datos["reglas"]}
    problemas = datos["problemas"]
    lineas = [
        f"# Reporte de SonarQube: {clave}",
        "",
        f"- Exportado: {datos['exportado']}",
        f"- Último análisis: {datos['ultimo_analisis'] or 'desconocido'}",
        f"- Servidor: {DIRECCION} (versión {datos['version_servidor'] or '?'})",
        f"- Problemas abiertos: {datos['total_problemas']}"
        + (f" (solo se exportaron {len(problemas)}; la API no da más de {LIMITE_API})"
           if datos["total_problemas"] > len(problemas) else ""),
        f"- Security hotspots: {datos['total_hotspots']}",
        "",
        "## Métricas",
        "",
        "| Métrica | Valor |",
        "|---|---|",
    ]
    for codigo, nombre in METRICAS:
        if codigo in datos["metricas"]:
            lineas.append(f"| {nombre} | {escapar(valor_metrica(codigo, datos['metricas'][codigo]))} |")

    condiciones = datos["quality_gate"].get("conditions", [])
    if condiciones:
        lineas += ["", "### Condiciones del quality gate", "", "| Métrica | Estado | Valor | Umbral |", "|---|---|---|---|"]
        for condicion in condiciones:
            lineas.append(
                f"| {condicion.get('metricKey')} | {condicion.get('status')} | "
                f"{condicion.get('actualValue', '—')} | {condicion.get('comparator', '')} {condicion.get('errorThreshold', '')} |"
            )

    por_severidad = Counter(severidad_de(problema) for problema in problemas)
    lineas += ["", "## Problemas por severidad", "", "| Severidad | Cantidad |", "|---|---|"]
    for severidad, cantidad in sorted(por_severidad.items(), key=lambda par: peso(par[0])):
        lineas.append(f"| {severidad} | {cantidad} |")

    por_regla = Counter(problema.get("rule") for problema in problemas)
    lineas += ["", "## Problemas por regla", "", "| Regla | Nombre | Cantidad |", "|---|---|---|"]
    for regla, cantidad in por_regla.most_common():
        lineas.append(f"| {regla} | {escapar(nombres_regla.get(regla, ''))} | {cantidad} |")

    por_archivo = defaultdict(list)
    for problema in problemas:
        por_archivo[archivo_de(problema.get("component", ""), clave)].append(problema)
    lineas += ["", "## Problemas por archivo", ""]
    for archivo in sorted(por_archivo, key=lambda nombre: (-len(por_archivo[nombre]), nombre)):
        lista = sorted(por_archivo[archivo], key=lambda problema: (problema.get("line") or 0))
        lineas += [f"### `{archivo}` ({len(lista)})", ""]
        for problema in lista:
            linea = problema.get("line")
            ubicacion = f"L{linea}" if linea else "archivo"
            esfuerzo = f" · {problema['effort']}" if problema.get("effort") else ""
            lineas.append(
                f"- **{ubicacion}** `{severidad_de(problema)}` `{calidad_de(problema)}` "
                f"`{problema.get('rule')}`{esfuerzo}: {escapar(problema.get('message'))}"
            )
        lineas.append("")

    lineas += escribir_cobertura(datos.get("cobertura") or [])

    if datos["hotspots"]:
        lineas += ["## Security hotspots", "", "| Archivo | Línea | Probabilidad | Categoría | Estado | Mensaje |", "|---|---|---|---|---|---|"]
        for hotspot in sorted(datos["hotspots"], key=lambda h: (h.get("component", ""), h.get("line") or 0)):
            lineas.append(
                f"| `{archivo_de(hotspot.get('component', ''), clave)}` | {hotspot.get('line', '—')} | "
                f"{hotspot.get('vulnerabilityProbability', '?')} | {hotspot.get('securityCategory', '?')} | "
                f"{hotspot.get('status', '?')} | {escapar(hotspot.get('message'))} |"
            )
        lineas.append("")

    return "\n".join(lineas) + "\n"


def entero(valor):
    try:
        return int(float(valor))
    except (TypeError, ValueError):
        return 0


def porcentaje(valor):
    return f"{float(valor):.1f} %" if valor not in (None, "") else "—"


def escribir_cobertura(archivos):
    if not archivos:
        return ["## Cobertura por archivo", "", "SonarQube no devolvió cobertura por archivo.", ""]

    lineas = ["## Cobertura por archivo", ""]
    nuevos = [archivo for archivo in archivos if entero(archivo.get("new_lines_to_cover")) > 0]
    nuevos.sort(key=lambda archivo: (-entero(archivo.get("new_uncovered_lines")), archivo["archivo"]))
    lineas += [
        "### Código nuevo (lo que mide el quality gate)",
        "",
        "Ordenado por líneas nuevas sin cubrir, de más a menos.",
        "",
        "| Archivo | Líneas nuevas a cubrir | Sin cubrir | Cobertura nueva |",
        "|---|---|---|---|",
    ]
    for archivo in nuevos:
        lineas.append(
            f"| `{archivo['archivo']}` | {entero(archivo.get('new_lines_to_cover'))} | "
            f"{entero(archivo.get('new_uncovered_lines'))} | {porcentaje(archivo.get('new_coverage'))} |"
        )
    if not nuevos:
        lineas.append("| — | 0 | 0 | — |")

    totales = [archivo for archivo in archivos if entero(archivo.get("uncovered_lines")) > 0]
    totales.sort(key=lambda archivo: (-entero(archivo.get("uncovered_lines")), archivo["archivo"]))
    lineas += [
        "",
        f"### Todo el código (los {ARCHIVOS_EN_TABLA_TOTAL} archivos con más líneas sin cubrir)",
        "",
        f"Archivos con líneas sin cubrir: {len(totales)} de {len(archivos)}.",
        "",
        "| Archivo | Líneas a cubrir | Sin cubrir | Cobertura |",
        "|---|---|---|---|",
    ]
    for archivo in totales[:ARCHIVOS_EN_TABLA_TOTAL]:
        lineas.append(
            f"| `{archivo['archivo']}` | {entero(archivo.get('lines_to_cover'))} | "
            f"{entero(archivo.get('uncovered_lines'))} | {porcentaje(archivo.get('coverage'))} |"
        )
    lineas.append("")
    return lineas


def guardar(nombre, contenido):
    ruta = os.path.join(CARPETA_REPORTES, nombre)
    with open(ruta, "w", encoding="utf-8") as archivo:
        archivo.write(contenido)
    return ruta


def main():
    argumento = sys.argv[1] if len(sys.argv) > 1 else "cliente"
    if argumento in ("-h", "--help"):
        print(__doc__)
        return 0
    clave = PROYECTOS.get(argumento, argumento)

    try:
        token = leer_token()
        if not token:
            raise ErrorSonar("no hay token.")
        revisar_servidor(token)
        print(f"Exportando '{clave}' desde {DIRECCION}…")
        metricas = leer_metricas(clave, token)
        problemas, extras, total_problemas = leer_problemas(clave, token)
        hotspots, total_hotspots = leer_hotspots(clave, token)
    except ErrorSonar as error:
        print(f"Error: {error}", file=sys.stderr)
        return 1

    try:
        cobertura = leer_cobertura(clave, token)
    except ErrorSonar as error:
        print(f"Aviso: no se pudo leer la cobertura por archivo ({error}).", file=sys.stderr)
        cobertura = []

    analisis = leer_opcional("/api/project_analyses/search", {"project": clave, "ps": 1}, token)
    lista_analisis = analisis.get("analyses") or [{}]
    version = leer_version(token)
    datos = {
        "proyecto": clave,
        "exportado": datetime.datetime.now().strftime("%Y-%m-%d %H:%M"),
        "ultimo_analisis": lista_analisis[0].get("date"),
        "version_servidor": version,
        "quality_gate": leer_opcional("/api/qualitygates/project_status", {"projectKey": clave}, token).get("projectStatus", {}),
        "metricas": metricas,
        "total_problemas": total_problemas,
        "problemas": problemas,
        "reglas": list({regla["key"]: regla for regla in extras["rules"]}.values()),
        "total_hotspots": total_hotspots,
        "hotspots": hotspots,
        "cobertura": cobertura,
    }

    os.makedirs(CARPETA_REPORTES, exist_ok=True)
    markdown = escribir_markdown(datos)
    marca = datetime.datetime.now().strftime("%Y%m%d-%H%M")
    ruta_md = guardar(f"{clave}-ultimo.md", markdown)
    guardar(f"{clave}-{marca}.md", markdown)
    ruta_json = guardar(f"{clave}-ultimo.json", json.dumps(datos, ensure_ascii=False, indent=2))

    print(f"Problemas abiertos: {total_problemas} · hotspots: {total_hotspots}")
    print(f"Reporte:  {os.path.relpath(ruta_md, RAIZ)}")
    print(f"Datos:    {os.path.relpath(ruta_json, RAIZ)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
