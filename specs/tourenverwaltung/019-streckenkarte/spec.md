# 019 – Streckenkarte

## Was und warum
Im Tourdetail einer Einheit lässt sich die gefahrene Strecke auf einer Landkarte ansehen. Das Tourdetail bekommt dafür neben „Höhenprofil“ die Karteikarte „Karte“. Wahlweise erscheint die OpenStreetMap-Standardkarte oder OpenTopoMap mit Höhenlinien und Wegen. So lässt sich wie in BikeXperience nachvollziehen, wo eine Tour entlangführte. Bereits angesehene Gegenden bleiben auch ohne Internet sichtbar.

## Kriterien

### Anzeigen
- Wenn das Tourdetail einer Einheit geöffnet ist, dann gibt es neben „Höhenprofil“ die Karteikarte „Karte“.
- Wenn der Benutzer die Karteikarte „Karte“ wählt, dann zeigt sie die Strecke der Aufzeichnung als einfarbige Linie auf einer Karte, so vergrößert, dass die ganze Strecke zu sehen ist.
- Wenn die Strecke angezeigt wird, dann sind Start und Ziel jeweils mit einem eigenen Zeichen markiert.
- Wenn der Benutzer zwischen „OpenStreetMap“ und „OpenTopoMap“ umschaltet, dann erscheint die gewählte Karte mit derselben Strecke und demselben Ausschnitt.
- Wenn der Benutzer das Mausrad dreht, dann wird die Karte an der Mausposition vergrößert oder verkleinert; Ziehen mit gedrückter linker Maustaste verschiebt sie.
- Wenn der Benutzer auf „Ganze Strecke“ klickt, dann zeigt die Karte wieder die ganze Strecke.
- Wenn die Karte angezeigt wird, dann steht in einer Ecke der Hinweis „© OpenStreetMap-Mitwirkende“, bei OpenTopoMap ergänzt um „Kartendarstellung: © OpenTopoMap (CC-BY-SA)“.

### Ohne Internet
- Wenn ein Kartenausschnitt einmal geladen wurde, dann erscheint er später auch ohne Internetverbindung.
- Wenn keine Internetverbindung besteht und ein Ausschnitt noch nie geladen wurde, dann wird die Strecke trotzdem gezeichnet, und es erscheint der Hinweis „Die Karte konnte nicht geladen werden. Besteht eine Internetverbindung?“

### Grenz- und Fehlerfälle
- Wenn die Aufzeichnung keine GPS-Daten enthält (z. B. eine Fahrt auf der Rolle), dann erscheint in der Karteikarte „Die Aufzeichnung enthält keine GPS-Daten.“
- Wenn einzelne Messpunkte keine Position haben (z. B. im Tunnel), dann verbindet die Linie die Punkte davor und danach.
