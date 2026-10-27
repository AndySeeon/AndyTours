namespace AndyTours.Import.Altdaten;

/// <summary>
/// Was in einem Ordner mit bisherigen Daten liegt (siehe <see cref="ErkenneAltdaten"/>).
/// </summary>
public enum Altdatenart
{
    /// <summary>Weder BikeXperience-Daten noch Aufzeichnungen – es gibt nichts zu übernehmen.</summary>
    Nichts,

    /// <summary>Ein BikeXperience-Datenordner – er wird vollständig umgewandelt.</summary>
    BikeXperience,

    /// <summary>Keine BikeXperience-Daten, aber Aufzeichnungen im FIT-Format – sie können als Touren übernommen werden.</summary>
    NurAufzeichnungen,
}
