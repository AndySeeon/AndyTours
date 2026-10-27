namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Eine vorhandene Sicherung im Sicherungsordner.
/// </summary>
/// <param name="Datei">Vollständiger Pfad der Sicherungsdatei.</param>
/// <param name="Zeitpunkt">Zeitpunkt der Sicherung, gelesen aus dem Zeitstempel im Dateinamen.</param>
public record Sicherungseintrag(string Datei, DateTime Zeitpunkt);
