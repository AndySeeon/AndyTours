# 0002 · Zugriff auf Mediengeräte

## Entscheidung
AndyTours greift über das NuGet-Paket `MediaDevices` (Version 2.0.0, MIT-Lizenz) auf den Garmin Edge 1040 zu, der sich per USB nur als Mediengerät (MTP) und nicht als Laufwerk meldet. Der Zugriff liegt im App-Projekt (`net10.0-windows`); Core, Import und Tests bleiben plattformneutral (`net10.0`).

## Warum
Wenig und gut lesbarer Code für Geräte auflisten, Ordner lesen und Dateien kopieren; das Paket ist aktuell, unterstützt .NET 10 unter Windows und steht unter MIT-Lizenz.

## Alternativen
- Windows-Schnittstelle für tragbare Geräte (WPD) selbst anbinden: keine fremde Abhängigkeit, aber viel fehleranfälliger Interop-Code, der selbst gepflegt und getestet werden müsste.
- Windows-Shell (Explorer-Funktionen): ohne Paket, aber Kopieren läuft im Hintergrund, kann Explorer-Dialoge zeigen, und die Ordnernamen sind übersetzt – für einen verlässlichen Import zu wackelig.
