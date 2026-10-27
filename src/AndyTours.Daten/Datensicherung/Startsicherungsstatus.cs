namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ausgang der Datensicherung beim Programmstart.
/// </summary>
public enum Startsicherungsstatus
{
    /// <summary>Die Sicherung war fällig und wurde angelegt.</summary>
    Gesichert,

    /// <summary>Keine Sicherung nötig: Intervall noch nicht erreicht oder Datenbank ohne Touren.</summary>
    NichtFaellig,

    /// <summary>Die Sicherung war fällig, ist aber fehlgeschlagen; beim nächsten Start wird es erneut versucht.</summary>
    Fehlgeschlagen,
}
