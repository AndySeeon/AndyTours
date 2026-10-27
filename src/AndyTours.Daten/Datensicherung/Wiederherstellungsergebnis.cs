namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ergebnis von <see cref="SpieleSicherungZurueck"/>.
/// </summary>
/// <param name="Status">Ausgang des Wiederherstellens.</param>
/// <param name="ZusaetzlicheSicherung">Pfad der Sicherung des vorherigen Stands; vorhanden, sobald sie angelegt wurde
/// (bei <see cref="Wiederherstellungsstatus.Wiederhergestellt"/> und ggf. bei <see cref="Wiederherstellungsstatus.Fehlgeschlagen"/>).</param>
/// <param name="Fehlergrund">Verständliche Beschreibung des Fehlers bei <see cref="Wiederherstellungsstatus.SicherungFehlgeschlagen"/>
/// und <see cref="Wiederherstellungsstatus.Fehlgeschlagen"/>.</param>
public record Wiederherstellungsergebnis(
    Wiederherstellungsstatus Status,
    string? ZusaetzlicheSicherung = null,
    string? Fehlergrund = null);
