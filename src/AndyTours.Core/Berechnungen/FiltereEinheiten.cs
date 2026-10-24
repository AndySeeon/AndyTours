using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wendet die Filter der Filterleiste auf die Einheiten der Jahres- und Aktivitätsauswahl an.
/// </summary>
public static class FiltereEinheiten
{
    /// <summary>
    /// Liefert die Einheiten, die alle gesetzten Filter gleichzeitig erfüllen.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Kategorie:</b> Die Einheit passt, wenn eine ihrer Kategorien den gewählten Namen trägt
    /// (weitere Kategorien stören nicht).</item>
    /// <item><b>Equipment, Trainingsbereich:</b> gleicher Name bzw. gleiches Kürzel.</item>
    /// <item><b>Distanz:</b> Grenzen eingeschlossen. 0 km gilt wie in der Tabelle als „nicht erfasst“; eine
    /// solche Einheit fällt bei aktivem Distanzfilter heraus.</item>
    /// <item><b>Höhenmeter:</b> Grenzen eingeschlossen. 0 hm ist ein echter Wert (Flachtour).</item>
    /// <item><b>Name:</b> Der Name muss den Text enthalten, ohne Rücksicht auf Groß- und Kleinschreibung;
    /// Leerzeichen am Anfang und Ende der Eingabe zählen nicht.</item>
    /// </list>
    /// Fehlerhafte Zahlenbereiche schränken nicht ein (<see cref="Filterbereich.IstAktiv"/>).
    /// Kategorie und Equipment werden über den Datenbestand von Nummern in Namen übersetzt.
    /// </remarks>
    /// <param name="einheiten">Die Einheiten der Jahres- und Aktivitätsauswahl.</param>
    /// <param name="filter">Die gesetzten Filter.</param>
    /// <param name="bestand">Der Datenbestand mit Kategorien und Equipments.</param>
    /// <returns>Die passenden Einheiten in unveränderter Reihenfolge.</returns>
    public static List<Trainingseinheit> Ausfuehren(IEnumerable<Trainingseinheit> einheiten, Einheitenfilter filter, Datenbestand bestand)
    {
        // Nummern der Kategorien bzw. Equipments mit dem gewählten Namen (bei gleichen Namen mehrere)
        HashSet<long>? kategorieIds = filter.Kategorie is null
            ? null
            : bestand.Kategorien.Where(k => k.Name == filter.Kategorie).Select(k => k.Id).ToHashSet();
        HashSet<long>? equipmentIds = filter.Equipment is null
            ? null
            : bestand.Equipments.Where(e => e.Name == filter.Equipment).Select(e => e.Id).ToHashSet();
        string name = filter.Name.Trim();

        return einheiten
            .Where(e => kategorieIds is null || e.KategorieIds.Any(kategorieIds.Contains))
            .Where(e => equipmentIds is null || (e.EquipmentId is long id && equipmentIds.Contains(id)))
            .Where(e => filter.Trainingsbereich is null || e.Trainingsbereich == filter.Trainingsbereich)
            .Where(e => !filter.Distanz.IstAktiv || (e.DistanzKm > 0 && filter.Distanz.Enthaelt(e.DistanzKm)))
            .Where(e => !filter.Hoehenmeter.IstAktiv || filter.Hoehenmeter.Enthaelt(e.Hoehenmeter))
            .Where(e => name.Length == 0 || e.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
