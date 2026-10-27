namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Zeile der Liste im Bereich „Auswahllisten“ der Einstellungen.
/// </summary>
/// <param name="Name">Der Eintrag selbst, z. B. „Bike“; je Liste eindeutig und Schlüssel aller Befehle.</param>
/// <param name="Anzeige">Der angezeigte Text mit Zusatz, z. B. „Bike (Hauptaktivität)“ oder „Trainieren (Standard)“.</param>
public sealed record Auswahllistenzeile(string Name, string Anzeige);
