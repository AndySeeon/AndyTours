using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt das Equipment, das in der Importliste für eine Tour vorbelegt wird.
/// </summary>
public static class ErmittleImportEquipment
{
    /// <summary>
    /// Liefert das Equipment, mit dem die Tour vermutlich gefahren wurde.
    /// </summary>
    /// <remarks>
    /// Reihenfolge:
    /// <list type="number">
    /// <item>Das Equipment, bei dem die Rad-Kennung der Aufzeichnung gemerkt ist.</item>
    /// <item>Das Equipment, bei dem einer der Sensoren gemerkt ist (der erste Sensor der Aufzeichnung mit Treffer).</item>
    /// <item>Das Standard-Equipment der Hauptaktivität (siehe <see cref="ErmittleStandardEquipment"/>).</item>
    /// </list>
    /// Ausgemusterte Equipments werden nicht vorgeschlagen.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aufzeichnung">Die gelesene Aufzeichnung.</param>
    /// <returns>Das Equipment oder <c>null</c>, wenn nichts passt und es kein Standard-Equipment gibt.</returns>
    public static Equipment? Ausfuehren(Datenbestand bestand, Aufzeichnung aufzeichnung)
    {
        List<(Equipment Equipment, List<Radkennung> Kennungen)> gemerkt = bestand.Equipments
            .Where(e => !e.Ausgemustert)
            .Select(e => (e, GeraetezuordnungText.Lesen(e.Geraetezuordnung)))
            .ToList();

        if (aufzeichnung.RadKennung.Length > 0
            && FindeMit(gemerkt, new Radkennung(Radkennungsart.Rad, aufzeichnung.RadKennung)) is Equipment perRad)
        {
            return perRad;
        }

        foreach (string sensor in aufzeichnung.Sensoren)
        {
            if (FindeMit(gemerkt, new Radkennung(Radkennungsart.Sensor, sensor)) is Equipment perSensor)
            {
                return perSensor;
            }
        }

        long? hauptaktivitaet = bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Id;
        return ErmittleStandardEquipment.Ausfuehren(bestand, hauptaktivitaet);
    }

    /// <summary>Sucht das erste Equipment, bei dem die Kennung gemerkt ist.</summary>
    private static Equipment? FindeMit(List<(Equipment Equipment, List<Radkennung> Kennungen)> gemerkt, Radkennung kennung) =>
        gemerkt.FirstOrDefault(g => g.Kennungen.Contains(kennung)).Equipment;
}
