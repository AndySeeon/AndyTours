using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Einträge der Auswahllisten Aktivität, Equipment und Trainingsbereich für eine Einheit.
/// </summary>
public static class ErmittleEingabelisten
{
    /// <summary>
    /// Liefert die Auswahllisten für das Bearbeitungsfenster bzw. die Tabellenzelle dieser Einheit.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Aktivitäten:</b> alle, in der Reihenfolge des Datenbestands (wie in der Werkzeugleiste).</item>
    /// <item><b>Equipments:</b> zuerst „(kein)“, dann alle nicht ausgemusterten alphabetisch. Ein
    /// ausgemustertes erscheint nur, wenn es der Einheit bereits zugeordnet ist – so geht die Zuordnung
    /// alter Touren nicht verloren, ohne dass alte Räder die Liste füllen.</item>
    /// <item><b>Trainingsbereiche:</b> zuerst ein leerer Eintrag (kein Bereich), dann alle Kürzel, die im
    /// Datenbestand vorkommen, alphabetisch. Neue Kürzel lassen sich hier nicht anlegen; das Pflegen der
    /// Liste ist eine eigene Funktion.</item>
    /// </list>
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="einheit">Die bearbeitete Einheit.</param>
    /// <returns>Die drei Listen.</returns>
    public static Eingabelisten Ausfuehren(Datenbestand bestand, Trainingseinheit einheit)
    {
        List<Auswahleintrag> aktivitaeten = bestand.Aktivitaeten.Select(a => new Auswahleintrag(a.Id, a.Name)).ToList();

        List<Auswahleintrag> equipments = bestand.Equipments
            .Where(e => !e.Ausgemustert || e.Id == einheit.EquipmentId)
            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, e.Name))
            .Prepend(new Auswahleintrag(null, Eingabelisten.KeinEquipment))
            .ToList();

        List<string> bereiche = bestand.Trainingseinheiten
            .Select(e => e.Trainingsbereich.Trim())
            .Append(einheit.Trainingsbereich.Trim())
            .Where(b => b.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.CurrentCultureIgnoreCase)
            .Prepend(string.Empty)
            .ToList();

        return new Eingabelisten(aktivitaeten, equipments, bereiche);
    }
}
