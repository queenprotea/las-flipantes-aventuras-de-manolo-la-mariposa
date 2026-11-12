#!/usr/bin/env python3

import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import urllib.request

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PROYECTO = os.path.join(RAIZ, "src", "Torres.Client")
AJUSTES_CLIENTE = os.path.join(PROYECTO, "appsettings.json")
CARPETA_CONTRATOS = os.path.join(PROYECTO, "Contracts")
CARPETA_METADATOS = os.path.join(CARPETA_CONTRATOS, "Metadata")
ARCHIVO_GENERADO = "GameContracts.cs"
DIRECCION_POR_DEFECTO = "http://localhost:8001"
ESPACIO_NOMBRES = "Game.Contracts"
TIPO_COLECCION = "System.Collections.Generic.List`1"

UBICACION_ESQUEMA = re.compile(r'schemaLocation="([^"]+)"')

USO = """Uso: python3 tools/generar-contratos-cliente.py [direccion de metadatos]

Descarga la descripcion de los servicios que publica el servidor y genera
src/Torres.Client/Contracts/GameContracts.cs con dotnet-svcutil.

El servidor tiene que estar encendido con Server:PublishesServiceMetadata en true.
La direccion por defecto es {0}.""".format(DIRECCION_POR_DEFECTO)


def leer_rutas_de_servicios():
    with open(AJUSTES_CLIENTE, encoding="utf-8-sig") as archivo:
        ajustes = json.load(archivo)
    rutas = list(ajustes["ServicePaths"].values())
    return rutas


def descargar(url):
    with urllib.request.urlopen(url) as respuesta:
        contenido = respuesta.read().decode("utf-8")
    return contenido


def descargar_descripciones(direccion_base, rutas):
    nombres_por_url = {}
    nombres_por_huella = {}
    documentos = {}
    pendientes = []

    for ruta in rutas:
        nombre = ruta.strip("/") + ".wsdl"
        url = direccion_base.rstrip("/") + ruta + "?wsdl"
        nombres_por_url[url] = nombre
        pendientes.append(url)

    while pendientes:
        url = pendientes.pop(0)
        contenido = descargar(url)
        for url_esquema in UBICACION_ESQUEMA.findall(contenido):
            if url_esquema not in nombres_por_url:
                contenido_esquema = descargar(url_esquema)
                huella = hashlib.sha256(contenido_esquema.encode("utf-8")).hexdigest()
                if huella not in nombres_por_huella:
                    nombres_por_huella[huella] = "schema{0}.xsd".format(len(nombres_por_huella))
                    pendientes.append(url_esquema)
                nombres_por_url[url_esquema] = nombres_por_huella[huella]
        documentos[nombres_por_url[url]] = contenido

    for nombre in documentos:
        documentos[nombre] = UBICACION_ESQUEMA.sub(
            lambda coincidencia: 'schemaLocation="{0}"'.format(nombres_por_url[coincidencia.group(1)]),
            documentos[nombre])

    return documentos


def guardar_descripciones(documentos):
    if os.path.isdir(CARPETA_METADATOS):
        shutil.rmtree(CARPETA_METADATOS)
    os.makedirs(CARPETA_METADATOS)
    for nombre, contenido in documentos.items():
        with open(os.path.join(CARPETA_METADATOS, nombre), "w", encoding="utf-8", newline="\n") as archivo:
            archivo.write(contenido)


def preparar_entorno():
    entorno = dict(os.environ)
    dotnet = shutil.which("dotnet")
    if dotnet is None:
        sys.exit("No se encontro dotnet en el PATH.")
    entorno["DOTNET_ROOT"] = os.path.dirname(os.path.realpath(dotnet))
    entorno["DOTNET_ROLL_FORWARD"] = "Major"
    return entorno


def generar_codigo(documentos):
    entorno = preparar_entorno()
    subprocess.run(["dotnet", "tool", "restore"], cwd=RAIZ, env=entorno, check=True)
    archivos = sorted(os.path.join(CARPETA_METADATOS, nombre) for nombre in documentos)
    with tempfile.TemporaryDirectory() as carpeta_temporal:
        orden = ["dotnet", "tool", "run", "dotnet-svcutil"] + archivos + [
            "--namespace", "*," + ESPACIO_NOMBRES,
            "--collectionType", TIPO_COLECCION,
            "--sync",
            "--outputDir", carpeta_temporal,
            "--outputFile", ARCHIVO_GENERADO,
            "--noLogo",
        ]
        subprocess.run(orden, cwd=RAIZ, env=entorno, check=True)
        shutil.copyfile(os.path.join(carpeta_temporal, ARCHIVO_GENERADO), os.path.join(CARPETA_CONTRATOS, ARCHIVO_GENERADO))


def main():
    if len(sys.argv) > 2 or (len(sys.argv) == 2 and sys.argv[1] in ("-h", "--help")):
        print(USO)
        return
    direccion_base = sys.argv[1] if len(sys.argv) == 2 else DIRECCION_POR_DEFECTO
    documentos = descargar_descripciones(direccion_base, leer_rutas_de_servicios())
    guardar_descripciones(documentos)
    generar_codigo(documentos)
    print("Generado {0} con {1} archivos de descripcion.".format(os.path.join("src", "Torres.Client", "Contracts", ARCHIVO_GENERADO), len(documentos)))


if __name__ == "__main__":
    main()
