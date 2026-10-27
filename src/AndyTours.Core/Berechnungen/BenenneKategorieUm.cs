using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Benennt eine Kategorie um – samt der Equipments, bei denen sie als Standardkategorie eingetragen ist.
/// </summary>
/// <remarks>
/// Die Einheiten verweisen per Id auf die Kategorie und brauchen keine Änderung. Die Standardkategorie
/// eines Equipments ist dagegen als Name gespeichert und muss mitgezogen werden; verglichen wird ohne
/// Unterschied zwischen Groß- und Kleinschreibung und ohne Leerzeichen am Rand.
/// </remarks>
public static class BenenneKategorieUm
{
    /// <summary>
    /// Gibt der Kategorie den neuen Namen (ohne Randleerzeichen) und ändert die passenden Equipments.
    /// </summary>
    /// <param name="bestand">Der Datenbestand; er wird geändert.</param>
    /// <param name="kategorieId">Id der Kategorie.</param>
    /// <param name="neuerName">Der neue Name.</param>
    /// <returns>Anzahl der geänderten Equipments.</returns>
    public static int Ausfuehren(Datenbestand bestand, long kategorieId, string neuerName)
    {
        Kategorie? kategorie = bestand.Kategorien.FirstOrDefault(k => k.Id == kategorieId);
        if (kategorie is null)
        {
            return 0;
        }

        string alt = kategorie.Name.Trim();
        string neu = neuerName.Trim();
        kategorie.Name = neu;

        int anzahl = 0;
        foreach (Equipment equipment in bestand.Equipments)
        {
            if (string.Equals(equipment.StandardKategorie.Trim(), alt, StringComparison.OrdinalIgnoreCase))
            {
                equipment.StandardKategorie = neu;
                anzahl++;
            }
        }

        return anzahl;
    }
}
