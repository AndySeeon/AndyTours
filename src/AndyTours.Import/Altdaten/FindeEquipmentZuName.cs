using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Sucht ein Equipment anhand des Namens, wie er in den Altdateien steht.
/// </summary>
public static class FindeEquipmentZuName
{
    /// <summary>
    /// Sucht zuerst nach exakt gleichem Namen, dann nach dem Namensteil vor dem Schrägstrich.
    /// </summary>
    /// <remarks>
    /// In <c>bikexperience.ini</c> und <c>workouts.csv</c> heißt ein Rad z. B. „Genius/Bike",
    /// in <c>maintanence.dat</c> dagegen nur „Genius". Deshalb wird im zweiten Schritt der Teil vor
    /// dem „/" verglichen. Passt dieser Teil auf mehrere Equipments, wird keines gewählt, um keine
    /// falsche Zuordnung zu erzeugen.
    /// </remarks>
    /// <param name="equipments">Alle bekannten Equipments.</param>
    /// <param name="name">Gesuchter Name (Groß-/Kleinschreibung wird ignoriert).</param>
    /// <returns>Das gefundene Equipment oder <c>null</c>.</returns>
    public static Equipment? Ausfuehren(IEnumerable<Equipment> equipments, string name)
    {
        var liste = equipments.ToList();
        string gesucht = name.Trim();

        // 1. Exakter Name
        Equipment? exakt = liste.FirstOrDefault(e => e.Name.Equals(gesucht, StringComparison.OrdinalIgnoreCase));
        if (exakt is not null)
        {
            return exakt;
        }

        // 2. Name ohne Zusatz nach „/" (beim gespeicherten Namen); nur bei eindeutigem Treffer
        var treffer = liste
            .Where(e => e.Name.Split('/')[0].Trim().Equals(gesucht, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return treffer.Count == 1 ? treffer[0] : null;
    }
}
