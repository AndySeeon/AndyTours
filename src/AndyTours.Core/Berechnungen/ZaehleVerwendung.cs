using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Zählt, wo eine Aktivität oder Kategorie verwendet wird – Grundlage für die Löschsperre und die
/// Rückfrage beim Zusammenführen.
/// </summary>
public static class ZaehleVerwendung
{
    /// <summary>
    /// Zählt die Einheiten und Equipments, die einer Aktivität zugeordnet sind.
    /// </summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Id der Aktivität.</param>
    /// <returns>Anzahl der Einheiten und Anzahl der Equipments.</returns>
    public static (int Einheiten, int Equipments) Aktivitaet(Datenbestand bestand, long aktivitaetId) =>
        (bestand.Trainingseinheiten.Count(e => e.AktivitaetId == aktivitaetId),
         bestand.Equipments.Count(e => e.AktivitaetId == aktivitaetId));

    /// <summary>
    /// Zählt die Einheiten mit einer Kategorie und nennt die Equipments, bei denen sie als
    /// Standardkategorie eingetragen ist.
    /// </summary>
    /// <remarks>
    /// Die Standardkategorie eines Equipments ist als Name gespeichert; verglichen wird ohne Unterschied
    /// zwischen Groß- und Kleinschreibung und ohne Leerzeichen am Rand.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="kategorieId">Id der Kategorie.</param>
    /// <returns>Anzahl der Einheiten und die Namen der Equipments (leer, wenn keines).</returns>
    public static (int Einheiten, List<string> Equipments) Kategorie(Datenbestand bestand, long kategorieId)
    {
        string name = bestand.Kategorien.FirstOrDefault(k => k.Id == kategorieId)?.Name.Trim() ?? string.Empty;
        int einheiten = bestand.Trainingseinheiten.Count(e => e.KategorieIds.Contains(kategorieId));
        List<string> equipments = name.Length == 0
            ? []
            : bestand.Equipments
                .Where(e => string.Equals(e.StandardKategorie.Trim(), name, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Name)
                .ToList();
        return (einheiten, equipments);
    }
}
