# 0005 · Installationsprogramm

## Entscheidung
Wir bauen das Installations- und Deinstallationsprogramm von AndyTours mit Inno Setup.

## Warum
- Kostenlos.
- Installiert nur für den Benutzer, ohne Administratorrechte.
- Erlaubt eigene Seiten mit Rückfragen, z. B. für den Datenordner und die Übernahme der bisherigen Daten.
- Bringt schon mit: den Eintrag unter „Apps“, ein Deinstallationsprogramm und das Erkennen einer laufenden App.

## Alternativen
- WiX/MSI: lässt sich in `dotnet build` einbinden, aber eigene Seiten mit Rückfragen sind deutlich aufwendiger.
- Eigenes C#-Programm: volle Kontrolle, aber Eintrag unter „Apps“, Selbstlöschen des Deinstallationsprogramms und Erkennen einer laufenden App müssten selbst gebaut werden.
