using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Ergebnis von <see cref="UebernehmeAltdaten"/>: der übernommene Datenbestand und der Bericht dazu.
/// </summary>
public class Migrationsergebnis
{
    /// <summary>Der vollständige, in AndyTours-Modelle umgewandelte Datenbestand.</summary>
    public Datenbestand Bestand { get; set; } = new();

    /// <summary>Alle Auffälligkeiten der Übernahme.</summary>
    public Migrationsbericht Bericht { get; set; } = new();
}
