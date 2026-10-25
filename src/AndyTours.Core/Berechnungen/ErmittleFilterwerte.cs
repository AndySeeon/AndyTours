using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Einträge der Auswahllisten Kategorie, Equipment und Trainingsbereich der Filterleiste.
/// </summary>
public static class ErmittleFilterwerte
{
    /// <summary>
    /// Liefert je Liste die Werte, die in der Jahres- und Aktivitätsauswahl vorkommen.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Jede Liste beginnt mit <see cref="Filterwerte.Alle"/>; danach folgen die Werte alphabetisch
    /// (ohne Rücksicht auf Groß- und Kleinschreibung) und ohne Doppelte.</item>
    /// <item>Leere Werte (Einheit ohne Equipment, ohne Trainingsbereich, unbekannte Nummer) erscheinen nicht.</item>
    /// <item>Ausgemustertes Equipment trägt den Zusatz „(ausgemustert)“ (siehe <see cref="EquipmentAnzeigename"/>)
    /// und steht nach den Equipments im Einsatz, ebenfalls alphabetisch.</item>
    /// <item>Ein bereits gewählter Wert, der in der Auswahl nicht mehr vorkommt (z. B. nach einem Wechsel des
    /// Jahres), wird trotzdem aufgenommen. So bleibt er sichtbar gewählt, statt dass der Filter unbemerkt
    /// verschwindet; die Tabelle zeigt dann den Hinweis, dass keine Einheit passt.</item>
    /// </list>
    /// </remarks>
    /// <param name="auswahl">Die Einheiten der Jahres- und Aktivitätsauswahl (noch ungefiltert).</param>
    /// <param name="bestand">Der Datenbestand mit Kategorien und Equipments.</param>
    /// <param name="aktuelleWahl">Der zurzeit gesetzte Filter.</param>
    /// <returns>Die Einträge der drei Auswahllisten.</returns>
    public static Filterwerte Ausfuehren(IReadOnlyCollection<Trainingseinheit> auswahl, Datenbestand bestand, Einheitenfilter aktuelleWahl)
    {
        Dictionary<long, string> kategorien = bestand.Kategorien.ToDictionary(k => k.Id, k => k.Name);
        Dictionary<long, string> equipments = bestand.Equipments.ToDictionary(e => e.Id, EquipmentAnzeigename.Bilden);

        return new Filterwerte(
            Kategorien: Liste(auswahl.SelectMany(e => e.KategorieIds).Select(id => kategorien.GetValueOrDefault(id, string.Empty)),
                aktuelleWahl.Kategorie),
            Equipments: Liste(auswahl.Select(e => e.EquipmentId is long id ? equipments.GetValueOrDefault(id, string.Empty) : string.Empty),
                aktuelleWahl.Equipment, ausgemusterteAmEnde: true),
            Trainingsbereiche: Liste(auswahl.Select(e => e.Trainingsbereich), aktuelleWahl.Trainingsbereich));
    }

    /// <summary>
    /// Baut eine Auswahlliste: „(alle)“, dann die nicht leeren Werte und die gewählte Eintragung, alphabetisch und ohne Doppelte.
    /// </summary>
    /// <param name="werte">Die vorkommenden Werte (auch leere und doppelte).</param>
    /// <param name="gewaehlt">Der gewählte Wert oder <c>null</c>.</param>
    /// <param name="ausgemusterteAmEnde"><c>true</c> (Equipments): Werte mit dem Zusatz „(ausgemustert)“ stehen am Ende.</param>
    /// <returns>Die fertige Liste.</returns>
    private static List<string> Liste(IEnumerable<string> werte, string? gewaehlt, bool ausgemusterteAmEnde = false)
    {
        IEnumerable<string> alle = gewaehlt is null ? werte : werte.Append(gewaehlt);
        return alle
            .Select(w => w.Trim())
            .Where(w => w.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(w => ausgemusterteAmEnde && w.EndsWith(EquipmentAnzeigename.ZusatzAusgemustert, StringComparison.Ordinal))
            .ThenBy(w => w, StringComparer.CurrentCultureIgnoreCase)
            .Prepend(Filterwerte.Alle)
            .ToList();
    }
}
