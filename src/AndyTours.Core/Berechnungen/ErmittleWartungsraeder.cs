using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Räder, die das Wartungsfenster und die Radauswahl der Karteikarte „Wartung“ anbieten.
/// </summary>
public static class ErmittleWartungsraeder
{
    /// <summary>Zusatz hinter dem Namen eines ausgemusterten Rads.</summary>
    public const string ZusatzAusgemustert = " (ausgemustert)";

    /// <summary>
    /// Liefert die Räder im Einsatz alphabetisch, danach die ausgemusterten – ebenfalls alphabetisch und mit dem
    /// Zusatz „(ausgemustert)“ hinter dem Namen.
    /// </summary>
    /// <remarks>
    /// Für eine neue Wartung erscheinen nur Räder im Einsatz; beim Bearbeiten zusätzlich das bisherige Rad,
    /// auch wenn es inzwischen ausgemustert ist. Die Radauswahl über der Tabelle zeigt alle Räder, weil die
    /// Wartungen ausgemusterter Räder sichtbar bleiben. Einen Eintrag „kein Rad“ gibt es nicht: Jede Wartung
    /// gehört zu einem Rad.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="bisherigesEquipmentId">Rad der bearbeiteten Wartung; <c>null</c> bei einer neuen Wartung.</param>
    /// <param name="alleRaeder"><c>true</c>: alle ausgemusterten Räder mitliefern (Radauswahl über der Tabelle).</param>
    /// <returns>Die Auswahleinträge (Nummer und angezeigter Name).</returns>
    public static List<Auswahleintrag> Ausfuehren(Datenbestand bestand, long? bisherigesEquipmentId, bool alleRaeder = false) =>
        bestand.Equipments
            .Where(e => alleRaeder || !e.Ausgemustert || e.Id == bisherigesEquipmentId)
            .OrderBy(e => e.Ausgemustert)
            .ThenBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, e.Ausgemustert ? e.Name + ZusatzAusgemustert : e.Name))
            .ToList();
}
