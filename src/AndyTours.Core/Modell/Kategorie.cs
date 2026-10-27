namespace AndyTours.Core.Modell;

/// <summary>
/// Frei vergebbares Stichwort zur Einordnung von Trainingseinheiten (z. B. „Arbeit", „Urlaub").
/// </summary>
/// <remarks>
/// Eine Trainingseinheit kann mehreren Kategorien zugeordnet sein (n:m-Beziehung,
/// siehe <see cref="Trainingseinheit.KategorieIds"/>).
/// Entspricht im Original dem INI-Abschnitt <c>[Category]</c> mit Zeilen <c>Name=Standard</c>.
/// </remarks>
public class Kategorie
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Name der Kategorie.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// <c>true</c>, wenn die Kategorie bei neuen Trainingseinheiten automatisch vorgeschlagen wird.
    /// </summary>
    public bool IstStandard { get; set; }
}
