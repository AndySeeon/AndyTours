# 0003 · Kartenbibliothek

## Entscheidung
AndyTours zeigt Karten mit dem NuGet-Paket `Mapsui.Wpf` (Version 5.1.0, MIT-Lizenz), einer nativen WPF-Kartenansicht. Kartenausschnitte von OpenStreetMap und OpenTopoMap werden mit der Kennung „AndyTours“ geladen und auf dem Rechner zwischengespeichert; Strecke sowie Start- und Zielzeichen liegen als Ebenen auf der Karte. Das Paket kommt nur ins App-Projekt (`net10.0-windows`); Core, Import und Tests bleiben ohne Kartenbibliothek.

## Warum
Mapsui ist die einzige aktiv gepflegte native WPF-Karte, die unter .NET 10 läuft (gebaut für .NET 8/9, von .NET 10 nutzbar). Kartenausschnitte mit Zwischenspeicher, eigene Kennung, Linien und Zeichen sowie Mausrad und Ziehen sind eingebaut, ohne Umweg über einen eingebetteten Browser.

## Alternativen
- `GMap.NET.WinPresentation` 2.1.7: bewährt und mit fertigem Zwischenspeicher, aber seit Juni 2022 nicht mehr aktualisiert und nur bis .NET 6 gebaut – ein Risiko unter .NET 10.
- Browser-Steuerelement (WebView2) mit Leaflet: viele Beispiele und moderne Darstellung, aber eingebetteter Browser, Kartenlogik in JavaScript, Datenaustausch über Skripte und ein mühsamer Zwischenspeicher für die Offline-Nutzung.
