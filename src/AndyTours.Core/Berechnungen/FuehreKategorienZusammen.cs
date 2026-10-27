using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Führt zwei Kategorien zusammen: Die Ursprungskategorie geht in der Zielkategorie auf.
/// </summary>
/// <remarks>
/// Jede Einheit der Ursprungskategorie hat danach die Zielkategorie – eine Einheit, die beide hatte,
/// hat sie nur einmal. War die Ursprungskategorie Standardkategorie, wird es die Zielkategorie; ebenso
/// geht ihr Eintrag als Standardkategorie eines Equipments auf die Zielkategorie über. Zum Schluss wird
/// die Ursprungskategorie aus der Liste entfernt.
/// </remarks>
public static class FuehreKategorienZusammen
{
    /// <summary>
    /// Führt die Zusammenführung im Datenbestand aus.
    /// </summary>
    /// <param name="bestand">Der Datenbestand; er wird geändert.</param>
    /// <param name="ursprungId">Id der Kategorie, die entfernt wird.</param>
    /// <param name="zielId">Id der Kategorie, die bleibt.</param>
    /// <returns>Anzahl der Einheiten, die die Ursprungskategorie hatten.</returns>
    public static int Ausfuehren(Datenbestand bestand, long ursprungId, long zielId)
    {
        Kategorie? ursprung = bestand.Kategorien.FirstOrDefault(k => k.Id == ursprungId);
        Kategorie? ziel = bestand.Kategorien.FirstOrDefault(k => k.Id == zielId);
        if (ursprung is null || ziel is null || ursprungId == zielId)
        {
            return 0;
        }

        int anzahl = 0;
        foreach (Trainingseinheit einheit in bestand.Trainingseinheiten.Where(e => e.KategorieIds.Contains(ursprungId)))
        {
            // Ursprung durch Ziel ersetzen; hatte die Einheit das Ziel schon, bleibt es bei einem Eintrag
            einheit.KategorieIds.RemoveAll(id => id == ursprungId);
            if (!einheit.KategorieIds.Contains(zielId))
            {
                einheit.KategorieIds.Add(zielId);
            }

            anzahl++;
        }

        if (ursprung.IstStandard)
        {
            ziel.IstStandard = true;
        }

        string alterName = ursprung.Name.Trim();
        foreach (Equipment equipment in bestand.Equipments)
        {
            if (string.Equals(equipment.StandardKategorie.Trim(), alterName, StringComparison.OrdinalIgnoreCase))
            {
                equipment.StandardKategorie = ziel.Name;
            }
        }

        bestand.Kategorien.Remove(ursprung);
        return anzahl;
    }
}
