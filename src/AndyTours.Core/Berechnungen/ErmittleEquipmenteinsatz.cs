using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet die Zeilen der Auskunft „Equipment-Einsatz“: Kilometer und Höhenmeter je Equipment insgesamt.
/// </summary>
public static class ErmittleEquipmenteinsatz
{
    /// <summary>Name der Zeile für Einheiten ohne Equipment.</summary>
    public const string OhneEquipment = "ohne Equipment";

    /// <summary>Name der Zeile mit den Spaltensummen.</summary>
    public const string Summe = "Summe";

    /// <summary>
    /// Liefert je Equipment eine Zeile, dazu ggf. „ohne Equipment“ und zuletzt „Summe“.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Es zählen alle übergebenen Einheiten, unabhängig von Jahr und Aktivität.</item>
    /// <item>„Gesamt (km)“ sind die Anfangskilometer plus die Distanzen der Einheiten, wie der km-Stand
    /// der Wartung (<see cref="BerechneKmStand"/>).</item>
    /// <item>Ein Equipment ohne Einheit und ohne Anfangskilometer fehlt.</item>
    /// <item>Einheiten ohne Equipment – oder mit einem Equipment, das es nicht mehr gibt – bilden die Zeile
    /// „ohne Equipment“; gibt es keine, fehlt die Zeile.</item>
    /// <item>Reihenfolge: Equipments im Einsatz, danach ausgemusterte, jeweils absteigend nach „Gesamt (km)“
    /// (bei Gleichstand alphabetisch), danach „ohne Equipment“, zuletzt „Summe“.</item>
    /// <item>Gibt es gar keine Zeile, ist die Liste leer – auch ohne „Summe“.</item>
    /// </list>
    /// </remarks>
    /// <param name="einheiten">Alle Trainingseinheiten.</param>
    /// <param name="equipments">Alle Equipments, auch ausgemusterte.</param>
    /// <returns>Die Zeilen in Anfangsreihenfolge.</returns>
    public static List<Einsatzzeile> Ausfuehren(IEnumerable<Trainingseinheit> einheiten, IEnumerable<Equipment> equipments)
    {
        List<Trainingseinheit> alle = einheiten.ToList();
        List<Equipment> equipmentListe = equipments.ToList();
        HashSet<long> bekannteIds = equipmentListe.Select(e => e.Id).ToHashSet();

        // Je Equipment: Anfangskilometer und Summen seiner Einheiten
        var raeder = equipmentListe
            .Select(equipment =>
            {
                List<Trainingseinheit> eigene = alle.Where(e => e.EquipmentId == equipment.Id).ToList();
                return (Equipment: equipment, Anzahl: eigene.Count, Km: equipment.AnfangsdistanzKm + eigene.Sum(e => e.DistanzKm),
                    Hm: eigene.Sum(e => e.Hoehenmeter));
            })
            .Where(r => r.Anzahl > 0 || r.Equipment.AnfangsdistanzKm != 0)
            .OrderBy(r => r.Equipment.Ausgemustert)
            .ThenByDescending(r => r.Km)
            .ThenBy(r => r.Equipment.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        List<Einsatzzeile> zeilen = raeder
            .Select(r => new Einsatzzeile(EquipmentAnzeigename.Bilden(r.Equipment), OhneNull(r.Equipment.AnfangsdistanzKm), OhneNull(r.Km), OhneNull(r.Hm)))
            .ToList();

        // Einheiten ohne (bekanntes) Equipment
        List<Trainingseinheit> ohne = alle.Where(e => e.EquipmentId is not long id || !bekannteIds.Contains(id)).ToList();
        if (ohne.Count > 0)
        {
            zeilen.Add(new Einsatzzeile(OhneEquipment, null, OhneNull(ohne.Sum(e => e.DistanzKm)), OhneNull(ohne.Sum(e => e.Hoehenmeter))));
        }

        if (zeilen.Count == 0)
        {
            return zeilen;
        }

        // Summe jeder Spalte, aus den ungerundeten Werten
        zeilen.Add(new Einsatzzeile(
            Summe,
            OhneNull(raeder.Sum(r => r.Equipment.AnfangsdistanzKm)),
            OhneNull(raeder.Sum(r => r.Km) + ohne.Sum(e => e.DistanzKm)),
            OhneNull(raeder.Sum(r => r.Hm) + ohne.Sum(e => e.Hoehenmeter)),
            IstSumme: true));
        return zeilen;
    }

    /// <summary>Liefert <c>null</c>, wenn der Wert gerundet 0 ergibt (leere Zelle).</summary>
    private static double? OhneNull(double wert) => Math.Round(wert) == 0 ? null : wert;

    /// <summary>Liefert <c>null</c> für 0 Höhenmeter (leere Zelle).</summary>
    private static int? OhneNull(int wert) => wert == 0 ? null : wert;
}
