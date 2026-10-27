# 0006 · Grundlage der Oberfläche

## Entscheidung
AndyTours verwendet das in WPF (.NET 10) eingebaute Fluent-Design. Es wird anwendungsweit an einer Stelle eingeschaltet (`ThemeMode="Light"` in `App.xaml`) und gilt für alle Fenster. Die App ist immer hell, einen dunklen Modus gibt es nicht. Künftige Fenster nutzen die Standardsteuerelemente ohne eigene Vorlagen; eigene Farben werden nur für fachliche Hervorhebungen gesetzt (Bestwerte, Alarmzustände, Fehler, Hinweise). Die Meldung `WPF0001` (experimentelle Schnittstelle) wird im App-Projekt gezielt abgeschaltet.

## Warum
Das Design gehört zu .NET 10: kein neues Paket, keine Lizenzfrage. Es bringt das Windows-11-Aussehen mit abgerundeten Steuerelementen und der Akzentfarbe von Windows, ohne eigene Stile pflegen zu müssen, und wird mit einer Zeile für alle Fenster eingeschaltet – auch neue Fenster sehen ohne Zutun passend aus. Nur hell, weil Diagramme, Karten und die fachlichen Farben (rote Bestwerte, rote und orange Alarmzeilen) für hellen Hintergrund gemacht sind; ein dunkler Modus hätte sie alle verdoppelt. Dass die Schnittstelle noch als experimentell gekennzeichnet ist und sich in einer späteren .NET-Version ändern kann, nehmen wir in Kauf; betroffen wäre nur die eine Stelle in `App.xaml`.

## Alternativen
- Eigene Stile auf dem klassischen WPF-Aussehen: volle Kontrolle und nichts Experimentelles, aber viel XAML, das für jedes Steuerelement selbst gepflegt werden muss.
- Fremde Bibliothek (z. B. WPF-UI): sehr modern und ausgereift, aber eine zusätzliche Abhängigkeit mit eigenen Steuerelementen und eigenem Versionswechsel.
- Das Fluent-Wörterbuch `Fluent.Light.xaml` direkt in `App.xaml` einbinden statt `ThemeMode`: ebenfalls hell und eingebaut, aber nicht der vorgesehene Weg; bleibt als Ausweg, falls sich `ThemeMode` ändert.
