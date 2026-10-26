# 0004 · Routingdienst

## Entscheidung
Die Tourenplanung ermittelt Strecken über einen Online-Routingdienst, den der Benutzer in den Einstellungen wählt.
Zur Wahl stehen zwei Dienste:

- **BRouter (voreingestellt):** ohne Zugangsschlüssel, Serveradresse änderbar, anfangs `https://brouter.de/brouter`.
  Die Fahrprofile gehen an die BRouter-Profile: Rennrad → `fastbike`, Trekking/Tourenrad → `trekking`,
  Mountainbike → `mtb`. Höhen kommen je Streckenpunkt, Abbiegehinweise über `timode`.
- **openrouteservice:** mit Zugangsschlüssel des Benutzers. Die Fahrprofile gehen an `cycling-road`,
  `cycling-regular` und `cycling-mountain`, Höhen und Fahranweisungen werden mit angefordert.

Fähren werden bei beiden Diensten gemieden (BRouter `profile:allow_ferries=0`, openrouteservice `avoid_features`),
sonst führt die Strecke z. B. mit der Fähre quer über den Chiemsee. Jede Anfrage trägt die Kennung „AndyTours“. Die Antwort jedes Dienstes wandelt eine eigene Fachfunktion im Core in
dasselbe Teilstück-Modell um, sodass die übrige Planung vom gewählten Dienst nichts weiß. Dienst, Serveradresse und
Zugangsschlüssel liegen in der Tabelle `Einstellung` des Datenordners. Der Schlüssel erscheint im
Einstellungsfenster nur maskiert und wird nirgends protokolliert. Erreicht die App den Dienst nicht, wird das
Teilstück eine Luftlinie (Spec 030).

## Warum
BRouter ist kostenlos, braucht kein Konto, ist auf Fahrräder ausgelegt und liefert alles, was die Planung braucht:
Fahrprofile, Höhen und Abbiegehinweise. Eine Testabfrage gegen `brouter.de` hat das für alle drei Profile
bestätigt. Der öffentliche Server wird privat betrieben, eine Verfügbarkeit ist nicht zugesichert. openrouteservice
dient deshalb als wählbarer Ausweichdienst mit eigenem Schlüssel. Die änderbare BRouter-Adresse erlaubt später
einen anderen oder eigenen BRouter-Server.

## Alternativen
- Nur BRouter: am einfachsten, aber ohne Ausweichmöglichkeit, wenn der öffentliche Server ausfällt.
- Zusätzlich GraphHopper: gute Fahranweisungen, aber ein weiterer Dienst mit Schlüssel und eigener Umwandlung. Ob das
  freie Kontingent Rennrad und Mountainbike enthält, ist ungeprüft.
- Nur openrouteservice: von einem Institut betrieben, aber jeder Benutzer bräuchte von Anfang an Konto und Schlüssel.
