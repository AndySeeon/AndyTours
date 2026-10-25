using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erstellt die Zeilen der Alarmliste auf der Karteikarte „Wartung“.
/// </summary>
public static class ErstelleAlarmzeilen
{
    /// <summary>
    /// Liefert je Alarm eine Zeile mit Rad, Tätigkeit, Intervall, gefahrenen und verbleibenden km und Zustand.
    /// </summary>
    /// <remarks>
    /// Reihenfolge: fällige zuerst, dann bald fällige, dann die übrigen; innerhalb davon nach verbleibenden km
    /// aufsteigend (der dringendste oben). Ausgeschaltete Alarme und die ausgemusterter Räder stehen am Ende.
    /// Wartungen (Typ „Wartung“) erscheinen hier nicht.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipmentId">Nur die Alarme dieses Rads; <c>null</c> = alle Alarme.</param>
    /// <returns>Die Zeilen.</returns>
    public static List<Alarmzeile> Ausfuehren(Datenbestand bestand, long? equipmentId)
    {
        Dictionary<long, string> namen = bestand.Equipments.ToDictionary(e => e.Id, e => e.Name);
        return bestand.Wartungen
            .Where(w => w.Typ == WartungsTyp.Alarm && (equipmentId is null || w.EquipmentId == equipmentId))
            .Select(alarm =>
            {
                Alarmstand stand = BerechneAlarmstand.Ausfuehren(bestand, alarm);
                string rad = alarm.EquipmentId is long id ? namen.GetValueOrDefault(id, string.Empty) : string.Empty;
                return new Alarmzeile(alarm.Id, rad, alarm.Taetigkeit, alarm.AlarmKm, stand.GefahrenKm, stand.VerbleibendKm, stand.Zustand);
            })
            .OrderBy(z => z.Zustand)
            .ThenBy(z => z.VerbleibendKm)
            .ThenBy(z => z.Rad, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
