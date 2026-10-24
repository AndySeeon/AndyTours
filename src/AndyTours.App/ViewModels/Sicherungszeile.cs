using AndyTours.Daten.Datensicherung;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Zeile in der Liste des Fensters „Sicherung wiederherstellen“.
/// </summary>
/// <param name="Eintrag">Die Sicherung (Datei und Zeitpunkt).</param>
/// <param name="Anzeigetext">Wochentag, Datum und Uhrzeit, z. B. „Freitag, 23.10.2026, 18:58:22 Uhr“.</param>
public record Sicherungszeile(Sicherungseintrag Eintrag, string Anzeigetext);
