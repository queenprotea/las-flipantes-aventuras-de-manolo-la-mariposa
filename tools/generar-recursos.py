#!/usr/bin/env python3

import os
import re
import sys
from xml.sax.saxutils import escape

import openpyxl

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
XLSX = os.path.join(RAIZ, "docs", "i18n", "Diccionario-i18n-Torres.xlsx")
PROYECTO = os.path.join(RAIZ, "src", "Torres.Client")
ESPACIO_NOMBRES = "Torres.Client.Localization"

PANTALLAS_EXCLUIDAS = {"Formats"}

CABECERA_RESX = """<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
"""


def leer_diccionario():
    hoja = openpyxl.load_workbook(XLSX, data_only=True)["Diccionario"]
    columnas = ["pantalla", "control", "tipo", "clave", "base", "traduccion",
                "cultura", "contexto", "observaciones"]
    filas = []
    for fila in hoja.iter_rows(min_row=2, values_only=True):
        if not fila or not fila[3]:
            continue
        f = {c: ("" if v is None else str(v)) for c, v in zip(columnas, fila)}
        if f["pantalla"] in PANTALLAS_EXCLUIDAS:
            continue
        filas.append(f)
    return filas


def validar(filas):
    errores = []
    vistas = set()
    identificadores = {}
    patron = re.compile(r"^[A-Z][A-Za-z0-9]*\.[A-Z][A-Za-z0-9]*$")
    for f in filas:
        clave = f["clave"]
        if clave in vistas:
            errores.append("clave duplicada: " + clave)
        vistas.add(clave)
        if not patron.match(clave):
            errores.append("clave fuera del patron Pantalla.Control: " + clave)
        if not f["base"] or not f["traduccion"]:
            errores.append("clave sin texto en alguna cultura: " + clave)
        if sorted(re.findall(r"\{(\d+)", f["base"])) != sorted(re.findall(r"\{(\d+)", f["traduccion"])):
            errores.append("marcadores descuadrados: " + clave)
        identificador = clave.replace(".", "")
        if identificador in identificadores:
            errores.append("dos claves producen la misma constante: %s y %s"
                           % (identificadores[identificador], clave))
        identificadores[identificador] = clave
    return errores


def escribir_resx(ruta, filas, columna):
    partes = [CABECERA_RESX]
    for f in filas:
        partes.append('  <data name="%s" xml:space="preserve">\n    <value>%s</value>\n  </data>\n'
                      % (escape(f["clave"]), escape(f[columna])))
    partes.append("</root>\n")
    with open(ruta, "w", encoding="utf-8") as archivo:
        archivo.write("".join(partes))


def escribir_claves(ruta, filas):
    pantallas = []
    for f in filas:
        if not pantallas or pantallas[-1][0] != f["pantalla"]:
            pantallas.append((f["pantalla"], []))
        pantallas[-1][1].append(f)

    lineas = [
        "namespace %s" % ESPACIO_NOMBRES,
        "{",
        "    internal static class TextKeys",
        "    {",
    ]
    for pantalla, grupo in pantallas:
        lineas.append("        internal static class %s" % pantalla)
        lineas.append("        {")
        for f in grupo:
            nombre = f["clave"].split(".", 1)[1]
            lineas.append('            internal const string %s = "%s";' % (nombre, f["clave"]))
        lineas.append("        }")
        lineas.append("")
    if lineas[-1] == "":
        lineas.pop()
    lineas.append("    }")
    lineas.append("}")
    with open(ruta, "w", encoding="utf-8") as archivo:
        archivo.write("\n".join(lineas) + "\n")


def main():
    filas = leer_diccionario()
    errores = validar(filas)
    if errores:
        for e in errores:
            print("ERROR: " + e, file=sys.stderr)
        return 1

    escribir_resx(os.path.join(PROYECTO, "Resources", "Strings.resx"), filas, "base")
    escribir_resx(os.path.join(PROYECTO, "Resources", "Strings.en.resx"), filas, "traduccion")
    escribir_claves(os.path.join(PROYECTO, "Localization", "TextKeys.cs"), filas)

    pantallas = len({f["pantalla"] for f in filas})
    print("Generado desde %d filas del diccionario, %d pantallas (excluidas: %s):"
          % (len(filas), pantallas, ", ".join(sorted(PANTALLAS_EXCLUIDAS))))
    print("  Resources/Strings.resx")
    print("  Resources/Strings.en.resx")
    print("  Localization/TextKeys.cs")
    return 0


if __name__ == "__main__":
    sys.exit(main())
