# 0001 · Diagrammbibliothek

## Entscheidung
Wir verwenden OxyPlot (`OxyPlot.Wpf`) für alle Diagramme der App, beginnend mit dem Jahresfortschritt auf der Karteikarte „Aktuell“ und später für das Höhenprofil.

## Warum
Alles, was das Jahresdiagramm braucht, ist eingebaut: Linie mit ausgefüllter Fläche, Punkte, gestrichelte Linien, Markierungen und der Hinweis beim Überfahren eines Punkts. Das Diagramm wird als Datenmodell im ViewModel beschrieben und passt damit zu MVVM. Die Bibliothek ist seit vielen Jahren bewährt, kostenlos (MIT-Lizenz) und schafft auch Höhenprofile mit einigen tausend Punkten. Dass sie nur noch langsam weiterentwickelt wird und optisch eher schlicht ist, nehmen wir in Kauf.

## Alternativen
- ScottPlot (`ScottPlot.WPF`): sehr aktiv gepflegt und schnell bei vielen Punkten, aber den Hinweis beim Überfahren eines Punkts müssten wir selbst programmieren, und Zoomen/Verschieben müsste abgeschaltet werden.
- LiveCharts2 (`LiveChartsCore.SkiaSharpView.WPF`): modern mit Animationen, aber lange nur als Vorabversion mit häufig geänderten Schnittstellen.
