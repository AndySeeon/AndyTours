using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Merkt sich beim Import Rad-Kennung und Sensoren einer Tour bei dem Equipment, mit dem sie gefahren wurde.
/// </summary>
/// <remarks>
/// So lernt AndyTours die Gerätezuordnung selbst: Beim nächsten Import mit derselben Kennung oder demselben
/// Sensor schlägt <see cref="ErmittleImportEquipment"/> dieses Equipment vor.
/// </remarks>
public static class MerkeRadkennungen
{
    /// <summary>
    /// Ermittelt die Equipments, deren gemerkte Kennungen sich ändern.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Beim gewählten Equipment werden Rad-Kennung und Sensoren der Aufzeichnung ergänzt, soweit sie fehlen.</item>
    /// <item>Bei allen anderen Equipments werden genau diese Kennungen entfernt – eine Kennung gehört zu einem Rad.</item>
    /// <item>Ohne gewähltes Equipment werden die Kennungen nur entfernt.</item>
    /// </list>
    /// Die gelieferten Equipments sind Kopien; der Bestand bleibt unverändert (Übernahme danach mit
    /// <see cref="UebernimmEquipment.Einsetzen"/>).
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aufzeichnung">Die importierte Aufzeichnung.</param>
    /// <param name="equipmentId">Das Equipment der neuen Einheit oder <c>null</c>.</param>
    /// <returns>Die geänderten Equipments; leer, wenn sich nichts ändert.</returns>
    public static List<Equipment> Ausfuehren(Datenbestand bestand, Aufzeichnung aufzeichnung, long? equipmentId)
    {
        List<Radkennung> neue = aufzeichnung.Sensoren.Select(s => new Radkennung(Radkennungsart.Sensor, s)).ToList();
        if (aufzeichnung.RadKennung.Length > 0)
        {
            neue.Insert(0, new Radkennung(Radkennungsart.Rad, aufzeichnung.RadKennung));
        }

        List<Equipment> geaendert = [];
        if (neue.Count == 0)
        {
            return geaendert;
        }

        foreach (Equipment equipment in bestand.Equipments)
        {
            List<Radkennung> bisher = GeraetezuordnungText.Lesen(equipment.Geraetezuordnung);
            List<Radkennung> kuenftig = equipment.Id == equipmentId
                ? [.. bisher, .. neue.Where(n => !bisher.Contains(n))]
                : bisher.Where(k => !neue.Contains(k)).ToList();

            if (!kuenftig.SequenceEqual(bisher))
            {
                Equipment kopie = UebernimmEquipment.Kopie(equipment);
                kopie.Geraetezuordnung = GeraetezuordnungText.Schreiben(kuenftig);
                geaendert.Add(kopie);
            }
        }

        return geaendert;
    }
}
