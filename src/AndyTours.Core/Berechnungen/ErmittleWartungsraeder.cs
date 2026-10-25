using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Equipments, die das Wartungsfenster, das Alarmfenster und die Auswahl der Karteikarte „Wartung“
/// anbieten.
/// </summary>
public static class ErmittleWartungsraeder
{
    /// <summary>Zusatz hinter dem Namen eines ausgemusterten Equipments.</summary>
    public const string ZusatzAusgemustert = " (ausgemustert)";

    /// <summary>
    /// Liefert den angezeigten Namen eines Equipments: bei einem ausgemusterten mit dem Zusatz „(ausgemustert)“
    /// (in der Auswahl und in den Listen „Wartungen“ und „Alarme“).
    /// </summary>
    /// <param name="equipment">Das Equipment.</param>
    /// <returns>Der Name, ggf. mit Zusatz.</returns>
    public static string Anzeigename(Equipment equipment) =>
        equipment.Ausgemustert ? equipment.Name + ZusatzAusgemustert : equipment.Name;

    /// <summary>
    /// Liefert die Equipments im Einsatz, die zur gültigen Aktivität gehören, alphabetisch.
    /// </summary>
    /// <remarks>
    /// Ausgemusterte Equipments und die anderer Aktivitäten erscheinen nicht (siehe <see cref="Wartungsaktivitaet"/>).
    /// Beim Bearbeiten bleibt das bisherige Equipment zusätzlich wählbar, damit sich der Eintrag ohne Umhängen
    /// speichern lässt – auch wenn es inzwischen ausgemustert ist (dann am Ende mit dem Zusatz „(ausgemustert)“)
    /// oder zu einer anderen Aktivität gehört. Einen Eintrag „kein Rad“ gibt es nicht: Jede Wartung gehört zu
    /// einem Equipment.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Die gültige Aktivität (siehe <see cref="Wartungsaktivitaet.Bestimmen"/>); <c>null</c> = alle.</param>
    /// <param name="bisherigesEquipmentId">Equipment des bearbeiteten Eintrags; <c>null</c> bei einem neuen und für die Auswahl über den Listen.</param>
    /// <returns>Die Auswahleinträge (Nummer und angezeigter Name).</returns>
    public static List<Auswahleintrag> Ausfuehren(Datenbestand bestand, long? aktivitaetId, long? bisherigesEquipmentId) =>
        bestand.Equipments
            .Where(e => (!e.Ausgemustert && Wartungsaktivitaet.Passt(e, aktivitaetId)) || e.Id == bisherigesEquipmentId)
            .OrderBy(e => e.Ausgemustert)
            .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, Anzeigename(e)))
            .ToList();
}
