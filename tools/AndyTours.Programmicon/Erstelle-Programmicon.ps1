<#
.SYNOPSIS
    Erzeugt das Programmicon AndyTours.ico aus der Vektorzeichnung Programmicon.xaml (Spezifikation 034).

.DESCRIPTION
    Die Zeichnung src/AndyTours.App/Ressourcen/Programmicon.xaml ist die einzige Quelle des Motivs.
    Das Skript rendert sie mit WPF in alle Größen, die Windows für Titelleiste, Taskleiste, Explorer und
    Startmenü braucht, und schreibt sie als eine Icon-Datei:

      1. Zeichnung mit dem XAML-Leser laden (reines ResourceDictionary, daher ohne Übersetzen nutzbar).
      2. Je Größe die passende Fassung wählen: bis 24 px „ProgrammiconKlein“, darüber „ProgrammiconGross“.
      3. Fassung in ein Bild der Größe zeichnen und als PNG kodieren.
      4. Alle PNG-Bilder in den ICO-Container schreiben (Kopf, Verzeichnis, Bilddaten).

    Die fertige src/AndyTours.App/Ressourcen/AndyTours.ico liegt im Repository. Das Skript muss nur laufen,
    wenn sich das Motiv ändert; Bauen und Installationsprogramm brauchen es nicht.

    Läuft mit Windows PowerShell 5.1 (powershell.exe), weil dort WPF aus .NET Framework ohne Weiteres
    geladen werden kann; WPF braucht den STA-Modus, den powershell.exe standardmäßig hat.

.PARAMETER Vorschau
    Optionaler Ordner, in den zusätzlich jede Größe als PNG geschrieben wird (zum Ansehen).

.EXAMPLE
    powershell tools/AndyTours.Programmicon/Erstelle-Programmicon.ps1
#>
param(
    [string]$Vorschau
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore, PresentationFramework, WindowsBase

$Projektordner = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$Quelle = Join-Path $Projektordner 'src\AndyTours.App\Ressourcen\Programmicon.xaml'
$Ziel = Join-Path $Projektordner 'src\AndyTours.App\Ressourcen\AndyTours.ico'

# Größen in Pixeln: 16–24 für Titelleiste und kleine Symbole, 32–64 für Taskleiste und Explorer
# (auch bei 125–200 % Windows-Skalierung), 256 für große Symbole und das Startmenü.
$Groessen = 16, 20, 24, 32, 40, 48, 64, 256

# 1. Zeichnung laden
$Strom = [System.IO.File]::OpenRead($Quelle)
try {
    $Woerterbuch = [System.Windows.Markup.XamlReader]::Load($Strom)
}
finally {
    $Strom.Dispose()
}

if ($Vorschau) {
    New-Item -ItemType Directory -Force $Vorschau | Out-Null
}

# 2. und 3. Jede Größe rendern und als PNG-Bytes merken
$Bilder = foreach ($Groesse in $Groessen) {
    $Schluessel = if ($Groesse -le 24) { 'ProgrammiconKlein' } else { 'ProgrammiconGross' }
    $Zeichnung = $Woerterbuch[$Schluessel]

    $Sicht = New-Object System.Windows.Media.DrawingVisual
    $Kontext = $Sicht.RenderOpen()
    $Kontext.DrawImage($Zeichnung, (New-Object System.Windows.Rect 0, 0, $Groesse, $Groesse))
    $Kontext.Close()

    # 96 dpi: ein WPF-Punkt entspricht genau einem Pixel des Icons
    $Bitmap = New-Object System.Windows.Media.Imaging.RenderTargetBitmap $Groesse, $Groesse, 96, 96, ([System.Windows.Media.PixelFormats]::Pbgra32)
    $Bitmap.Render($Sicht)

    $Kodierer = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $Kodierer.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($Bitmap))
    $Speicher = New-Object System.IO.MemoryStream
    $Kodierer.Save($Speicher)
    $Bytes = $Speicher.ToArray()
    $Speicher.Dispose()

    if ($Vorschau) {
        [System.IO.File]::WriteAllBytes((Join-Path $Vorschau "Programmicon-$Groesse.png"), $Bytes)
    }
    [pscustomobject]@{ Groesse = $Groesse; Bytes = $Bytes }
}

# 4. ICO-Container schreiben (alle Zahlen Little Endian):
#    Kopf (6 Byte): reserviert 0, Typ 1 = Icon, Anzahl der Bilder
#    Verzeichnis (16 Byte je Bild): Breite, Höhe (0 bedeutet 256), Farbanzahl 0, reserviert 0,
#    Ebenen 1, Bit je Pixel 32, Länge der Bilddaten, Versatz der Bilddaten ab Dateianfang
#    Bilddaten: die PNG-Bytes (seit Windows Vista in Icons zulässig)
$Datei = New-Object System.IO.MemoryStream
$Schreiber = New-Object System.IO.BinaryWriter $Datei
$Schreiber.Write([uint16]0)
$Schreiber.Write([uint16]1)
$Schreiber.Write([uint16]$Bilder.Count)

$Versatz = 6 + 16 * $Bilder.Count
foreach ($Bild in $Bilder) {
    $Kante = if ($Bild.Groesse -ge 256) { 0 } else { $Bild.Groesse }
    $Schreiber.Write([byte]$Kante)
    $Schreiber.Write([byte]$Kante)
    $Schreiber.Write([byte]0)
    $Schreiber.Write([byte]0)
    $Schreiber.Write([uint16]1)
    $Schreiber.Write([uint16]32)
    $Schreiber.Write([uint32]$Bild.Bytes.Length)
    $Schreiber.Write([uint32]$Versatz)
    $Versatz += $Bild.Bytes.Length
}
foreach ($Bild in $Bilder) {
    $Schreiber.Write($Bild.Bytes)
}
$Schreiber.Flush()
[System.IO.File]::WriteAllBytes($Ziel, $Datei.ToArray())
$Schreiber.Dispose()

Write-Host "Programmicon geschrieben: $Ziel ($($Bilder.Count) Größen: $($Groessen -join ', ') px)"
