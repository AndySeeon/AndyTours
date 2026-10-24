namespace AndyTours.Daten.Datensicherung;

/// <summary>
/// Ergebnis von <see cref="PruefeSicherung"/>.
/// </summary>
public enum Sicherungspruefung
{
    /// <summary>Die Datei ist eine unversehrte AndyTours-Datenbank dieser oder einer älteren Version.</summary>
    Brauchbar,

    /// <summary>Die Datei ist beschädigt oder enthält keine AndyTours-Daten.</summary>
    Unbrauchbar,

    /// <summary>Die Datei stammt von einer neueren AndyTours-Version, deren Daten diese Version nicht kennt.</summary>
    NeuereVersion,
}
