using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Findet die Kategorie, die bei einem Equipment als Standardkategorie eingetragen ist.
/// </summary>
public static class FindeStandardkategorie
{
    /// <summary>
    /// Liefert die Id der Standardkategorie eines Equipments.
    /// </summary>
    /// <remarks>
    /// Die Standardkategorie ist beim Equipment als Name gespeichert; verglichen wird ohne Unterschied
    /// zwischen Groß- und Kleinschreibung und ohne Leerzeichen am Rand. Wird sie bei einer Einheit gewählt,
    /// hakt das Bearbeitungsfenster diese Kategorie zusätzlich an.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipmentId">Id des Equipments oder <c>null</c> für „(kein)“.</param>
    /// <returns>Die Id der Kategorie oder <c>null</c>, wenn keine eingetragen ist oder es sie nicht gibt.</returns>
    public static long? Ausfuehren(Datenbestand bestand, long? equipmentId)
    {
        string name = bestand.Equipments.FirstOrDefault(e => e.Id == equipmentId)?.StandardKategorie.Trim() ?? string.Empty;
        return name.Length == 0
            ? null
            : bestand.Kategorien.FirstOrDefault(k => string.Equals(k.Name.Trim(), name, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}
