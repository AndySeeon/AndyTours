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
    /// <item><b>Trainingsbereiche:</b> zuerst ein leerer Eintrag (kein Bereich), dann die in den
    /// Einstellungen gepflegte Liste in deren Reihenfolge. Hat die Einheit einen Bereich, der nicht in der
    /// Liste steht, kommt er am Ende dazu, damit er beim Speichern nicht verloren geht. Neue Kürzel lassen
    /// sich hier nicht anlegen; das geschieht in den Einstellungen.</item>
    /// </list>
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="einheit">Die bearbeitete Einheit.</param>
    /// <param name="trainingsbereiche">Die gepflegte Liste der Trainingsbereiche.</param>
    /// <returns>Die drei Listen.</returns>
    public static Eingabelisten Ausfuehren(Datenbestand bestand, Trainingseinheit einheit, IReadOnlyList<string> trainingsbereiche)
    {
        List<Auswahleintrag> aktivitaeten = bestand.Aktivitaeten.Select(a => new Auswahleintrag(a.Id, a.Name)).ToList();

        List<Auswahleintrag> equipments = bestand.Equipments
            .Where(e => !e.Ausgemustert || e.Id == einheit.EquipmentId)
            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, e.Name))
            .Prepend(new Auswahleintrag(null, Eingabelisten.KeinEquipment))
            .ToList();

        List<string> bereiche = [string.Empty, .. trainingsbereiche];
        string eigener = einheit.Trainingsbereich.Trim();
        if (eigener.Length > 0 && !trainingsbereiche.Contains(eigener, StringComparer.Ordinal))
        {
            bereiche.Add(eigener);
        }

        return new Eingabelisten(aktivitaeten, equipments, bereiche);
    }
}
