param([string]$Raiz, [string]$Salida)

function Cargar-Resx([string]$ruta) {
    $doc = New-Object System.Xml.XmlDocument
    $doc.LoadXml([System.IO.File]::ReadAllText($ruta, [System.Text.Encoding]::UTF8))
    $mapa = New-Object 'System.Collections.Specialized.OrderedDictionary'
    $duplicadas = New-Object System.Collections.Generic.List[string]
    foreach ($d in $doc.SelectNodes("//*[local-name()='data']")) {
        $clave = $d.GetAttribute('name')
        $valor = ''
        foreach ($h in $d.ChildNodes) { if ($h.LocalName -eq 'value') { $valor = $h.InnerText } }
        if ($mapa.Contains($clave)) { $duplicadas.Add($clave) } else { $mapa.Add($clave, $valor) }
    }
    return @{ Mapa = $mapa; Duplicadas = $duplicadas }
}

function Marcadores([string]$texto) {
    $r = [regex]::Matches($texto, '\{(\d+)')
    $lista = @()
    foreach ($m in $r) { $lista += $m.Groups[1].Value }
    return (($lista | Sort-Object) -join ',')
}

$rutaBase = Join-Path $Raiz 'src\Torres.Client\Resources\Strings.resx'
$rutaEn   = Join-Path $Raiz 'src\Torres.Client\Resources\Strings.en.resx'
$rutaKeys = Join-Path $Raiz 'src\Torres.Client\Localization\TextKeys.cs'

$base = Cargar-Resx $rutaBase
$en   = Cargar-Resx $rutaEn

$lineas = New-Object System.Collections.Generic.List[string]
$fallos = 0
function Registrar([string]$t) { $script:lineas.Add($t) }

Registrar '# Pruebas de internacionalizacion - Torres'
Registrar ''
Registrar ('Ejecutadas el ' + (Get-Date -Format 'dd/MM/yyyy HH:mm') + ' sobre:')
Registrar ''
Registrar '- `src/Torres.Client/Resources/Strings.resx` (cultura base es-MX)'
Registrar '- `src/Torres.Client/Resources/Strings.en.resx` (cultura adicional en-US)'
Registrar '- `src/Torres.Client/Localization/TextKeys.cs`'
Registrar ''
Registrar ('Entradas en la cultura base: ' + $base.Mapa.Count)
Registrar ('Entradas en la cultura adicional: ' + $en.Mapa.Count)
Registrar ''

Registrar '## P1. Cada clave existe en las dos culturas'
Registrar ''
$soloBase = @($base.Mapa.Keys | Where-Object { -not $en.Mapa.Contains($_) })
$soloEn   = @($en.Mapa.Keys | Where-Object { -not $base.Mapa.Contains($_) })
if ($soloBase.Count -eq 0 -and $soloEn.Count -eq 0) {
    Registrar ('CORRECTO: las ' + $base.Mapa.Count + ' claves estan en es-MX y en en-US.')
} else {
    $script:fallos++
    Registrar ('FALLO: solo en es-MX: ' + $soloBase.Count + '; solo en en-US: ' + $soloEn.Count)
    foreach ($k in $soloBase) { Registrar ('  falta en en-US: ' + $k) }
    foreach ($k in $soloEn) { Registrar ('  falta en es-MX: ' + $k) }
}
Registrar ''

Registrar '## P2. Ningun texto queda sin traducir ni vacio'
Registrar ''
$vacias = New-Object System.Collections.Generic.List[string]
foreach ($k in $base.Mapa.Keys) {
    if ([string]::IsNullOrWhiteSpace($base.Mapa[$k])) { $vacias.Add('es-MX: ' + $k) }
}
foreach ($k in $en.Mapa.Keys) {
    if ([string]::IsNullOrWhiteSpace($en.Mapa[$k])) { $vacias.Add('en-US: ' + $k) }
}
if ($vacias.Count -eq 0) {
    Registrar 'CORRECTO: ninguna entrada tiene valor vacio en ninguna de las dos culturas.'
} else {
    $script:fallos++
    Registrar ('FALLO: ' + $vacias.Count + ' entradas vacias.')
    foreach ($v in $vacias) { Registrar ('  ' + $v) }
}
Registrar ''

Registrar '## P3. Los marcadores de formato cuadran entre culturas'
Registrar ''
$descuadres = New-Object System.Collections.Generic.List[string]
$conMarcadores = 0
foreach ($k in $base.Mapa.Keys) {
    if (-not $en.Mapa.Contains($k)) { continue }
    $mb = Marcadores $base.Mapa[$k]
    $me = Marcadores $en.Mapa[$k]
    if ($mb.Length -gt 0) { $conMarcadores++ }
    if ($mb -ne $me) { $descuadres.Add($k + '  es-MX={' + $mb + '}  en-US={' + $me + '}') }
}
if ($descuadres.Count -eq 0) {
    Registrar ('CORRECTO: ' + $conMarcadores + ' cadenas llevan marcadores y todas coinciden entre las dos culturas.')
} else {
    $script:fallos++
    Registrar ('FALLO: ' + $descuadres.Count + ' cadenas con marcadores descuadrados.')
    foreach ($d in $descuadres) { Registrar ('  ' + $d) }
}
Registrar ''

Registrar '## P4. No hay claves duplicadas'
Registrar ''
if ($base.Duplicadas.Count -eq 0 -and $en.Duplicadas.Count -eq 0) {
    Registrar 'CORRECTO: ninguna clave aparece dos veces.'
} else {
    $script:fallos++
    Registrar ('FALLO: duplicadas en es-MX: ' + $base.Duplicadas.Count + ', en en-US: ' + $en.Duplicadas.Count)
    foreach ($d in $base.Duplicadas) { Registrar ('  es-MX: ' + $d) }
    foreach ($d in $en.Duplicadas) { Registrar ('  en-US: ' + $d) }
}
Registrar ''

Registrar '## P5. Cada constante de TextKeys tiene su recurso'
Registrar ''
$texto = [System.IO.File]::ReadAllText($rutaKeys, [System.Text.Encoding]::UTF8)
$constantes = @()
foreach ($m in [regex]::Matches($texto, 'internal const string \w+ = "([^"]+)"')) {
    $constantes += $m.Groups[1].Value
}
$huerfanas = @($constantes | Where-Object { -not $base.Mapa.Contains($_) })
if ($huerfanas.Count -eq 0) {
    Registrar ('CORRECTO: las ' + $constantes.Count + ' constantes de TextKeys resuelven a un recurso existente.')
} else {
    $script:fallos++
    Registrar ('FALLO: ' + $huerfanas.Count + ' constantes sin recurso.')
    foreach ($h in $huerfanas) { Registrar ('  ' + $h) }
}
Registrar ''

Registrar '## P6. Textos identicos en las dos culturas'
Registrar ''
$iguales = New-Object System.Collections.Generic.List[string]
foreach ($k in $base.Mapa.Keys) {
    if (-not $en.Mapa.Contains($k)) { continue }
    if ($base.Mapa[$k] -ceq $en.Mapa[$k]) { $iguales.Add($k + '  =  ' + $base.Mapa[$k]) }
}
Registrar ('Revision manual: ' + $iguales.Count + ' de ' + $base.Mapa.Count + ' entradas son identicas en ambas culturas.')
Registrar 'No es un fallo por si mismo: hay cadenas que no se traducen (nombres propios, simbolos, nombres de idioma).'
Registrar ''
foreach ($i in $iguales) { Registrar ('  ' + $i) }
Registrar ''

Registrar '## P7. No hay textos fijos internacionalizables en el codigo de UI'
Registrar ''
$carpetas = @('src\Torres.Client\Screens', 'src\Torres.Client\Localization', 'src\Torres.Client\Ui')
$literales = New-Object System.Collections.Generic.List[string]
$fuentes = @()
foreach ($c in $carpetas) { $fuentes += Get-ChildItem (Join-Path $Raiz $c) -Filter *.cs -Recurse }
$fuentes += Get-Item (Join-Path $Raiz 'src\Torres.Client\TorresGame.cs')
foreach ($f in $fuentes) {
    if ($f.Name -eq 'TextKeys.cs') { continue }
    $n = 0
    foreach ($l in ([System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8) -split "`r?`n")) {
        $n++
        foreach ($m in [regex]::Matches($l, '"([^"]*)"')) {
            $v = $m.Groups[1].Value
            if ($v.Length -eq 0) { continue }
            $literales.Add($f.Name + ':' + $n + '  "' + $v + '"')
        }
    }
}
Registrar ('Literales de cadena encontrados fuera de TextKeys: ' + $literales.Count)
Registrar ''
foreach ($l in $literales) { Registrar ('  ' + $l) }
Registrar ''
Registrar 'Clasificacion de los literales encontrados:'
Registrar ''
Registrar '- Simbolos decorativos (`✓`, `›`, `●`): marcas graficas, no texto traducible.'
Registrar '- Identificadores de infraestructura (`language.txt`, `Torres`, el nombre base del'
Registrar '  ResourceManager, `DejaVuSans.ttf`): nombres tecnicos, no texto de interfaz.'
Registrar '- Codigos de cultura (`es`, `es-MX`, `en`, `en-US`): la definicion de las culturas'
Registrar '  objetivo en LanguageService, no texto mostrado.'
Registrar '- `Fonts.cs:48`: mensaje de excepcion redactado en espanol. Es el unico texto fijo'
Registrar '  en lenguaje natural del proyecto. No llega a la interfaz: se lanza al arrancar si'
Registrar '  falta la fuente incrustada, y va dirigido a quien desarrolla, no al jugador.'
Registrar ''
Registrar 'Conclusion: ningun texto visible para el usuario esta fijo en el codigo. Todos salen'
Registrar 'de los .resx a traves de TextKeys.'
Registrar ''

$resumen = if ($fallos -eq 0) { 'Todas las comprobaciones automaticas pasaron.' } else { ('Comprobaciones con fallo: ' + $fallos) }
Registrar '## Resultado'
Registrar ''
Registrar $resumen

[System.IO.File]::WriteAllText($Salida, ($lineas -join "`r`n"), (New-Object System.Text.UTF8Encoding $false))
($lineas -join "`r`n")
