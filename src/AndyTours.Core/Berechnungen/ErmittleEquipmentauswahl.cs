using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Stellt die Equipments zusammen, die für eine Einheit angeboten werden.
/// </summary>
public static class ErmittleEquipmentauswahl
{
    /// <summary>
    /// Liefert die Einträge der Equipment-Auswahl für eine Einheit mit der gewählten Aktivität.
    /// </summary>
    /// <remarks>
    /// Zuerst „(kein)“, dann alphabetisch:
    /// <list type="bullet">
    /// <item>die nicht ausgemusterten Equipments der gewählten Aktivität,</item>
    /// <item>die nicht ausgemusterten Equipments ohne Aktivität (sie passen zu jeder Aktivität),</item>
    /// <item>das Equipment, das die Einheit bereits hat – auch wenn es ausgemustert ist oder zu einer
    /// anderen Aktivität gehört, damit die Zuordnung alter Touren nicht verloren geht.</item>
    /// </list>
    /// Ohne gewählte Aktivität (<c>null</c> oder 0) werden nur die Equipments ohne Aktivität angeboten.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Die gewählte Aktivität der Einheit.</param>
    /// <param name="bisherigesEquipmentId">Das Equipment, das die Einheit bereits hat, oder <c>null</c>.</param>
    /// <returns>Die Einträge mit „(kein)“ an erster Stelle.</returns>
    public static List<Auswahleintrag> Ausfuehren(Datenbestand bestand, long? aktivitaetId, long? bisherigesEquipmentId) =>
        bestand.Equipments
            .Where(e => e.Id == bisherigesEquipmentId
                || (!e.Ausgemustert && (e.AktivitaetId is null || (aktivitaetId is long id && id != 0 && e.AktivitaetId == id))))
            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, e.Name))
            .Prepend(new Auswahleintrag(null, Eingabelisten.KeinEquipment))
            .ToList();
}
