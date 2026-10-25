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
    /// anderen Aktivität gehört, damit die Zuordnung alter Touren nicht verloren geht. Ist es ausgemustert,
    /// steht es am Ende und trägt den Zusatz „(ausgemustert)“ (siehe <see cref="EquipmentAnzeigename"/>).</item>
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
            .OrderBy(e => e.Ausgemustert)
            .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, EquipmentAnzeigename.Bilden(e)))
            .Prepend(new Auswahleintrag(null, Eingabelisten.KeinEquipment))
            .ToList();

    /// <summary>
    /// Liefert die Einträge der Equipment-Auswahl für mehrere Einheiten mit verschiedenen Aktivitäten
    /// („Equipment ändern…“ bei einer Mehrfachauswahl).
    /// </summary>
    /// <remarks>
    /// Zuerst „(kein)“, dann alphabetisch die nicht ausgemusterten Equipments, die zu einer der Aktivitäten
    /// passen, und die ohne Aktivität. Ausgemusterte werden nicht angeboten, auch wenn Einheiten sie haben.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetIds">Die Aktivitäten der markierten Einheiten.</param>
    /// <returns>Die Einträge mit „(kein)“ an erster Stelle.</returns>
    public static List<Auswahleintrag> Ausfuehren(Datenbestand bestand, IReadOnlyCollection<long> aktivitaetIds) =>
        bestand.Equipments
            .Where(e => !e.Ausgemustert && (e.AktivitaetId is null || aktivitaetIds.Contains(e.AktivitaetId.Value)))
            .OrderBy(e => e.Ausgemustert)
            .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, EquipmentAnzeigename.Bilden(e)))
            .Prepend(new Auswahleintrag(null, Eingabelisten.KeinEquipment))
            .ToList();
}
