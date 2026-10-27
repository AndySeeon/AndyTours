namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ergebnis von <see cref="FuehreStartsicherungDurch"/>.
/// </summary>
/// <param name="Status">Ausgang der Sicherung.</param>
/// <param name="Sicherungsordner">Der verwendete Sicherungsordner (für Meldungen); <c>null</c>, wenn er nicht ermittelt werden konnte.</param>
/// <param name="Sicherungsdatei">Pfad der angelegten Sicherung; nur bei <see cref="Startsicherungsstatus.Gesichert"/>.</param>
/// <param name="Fehlergrund">Verständliche Beschreibung des Fehlers; nur bei <see cref="Startsicherungsstatus.Fehlgeschlagen"/>.</param>
public record Startsicherungsergebnis(
    Startsicherungsstatus Status,
    string? Sicherungsordner = null,
    string? Sicherungsdatei = null,
    string? Fehlergrund = null);
