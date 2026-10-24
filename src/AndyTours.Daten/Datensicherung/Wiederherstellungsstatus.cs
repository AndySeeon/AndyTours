namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ausgang von <see cref="SpieleSicherungZurueck"/>.
/// </summary>
public enum Wiederherstellungsstatus
{
    /// <summary>Der Datenbestand entspricht jetzt dem Stand der Sicherung.</summary>
    Wiederhergestellt,

    /// <summary>Die gewählte Sicherung ist beschädigt oder enthält keine AndyTours-Daten; nichts wurde verändert.</summary>
    Unbrauchbar,

    /// <summary>Die gewählte Sicherung stammt von einer neueren AndyTours-Version; nichts wurde verändert.</summary>
    NeuereVersion,

    /// <summary>Der aktuelle Stand konnte nicht gesichert werden; deshalb wurde nichts wiederhergestellt.</summary>
    SicherungFehlgeschlagen,

    /// <summary>Das Zurückspielen ist fehlgeschlagen; der bisherige Datenbestand ist unverändert erhalten.</summary>
    Fehlgeschlagen,
}
