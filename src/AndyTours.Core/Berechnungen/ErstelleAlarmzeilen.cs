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
    /// aufsteigend (der dringendste oben). Ausgeschaltete Alarme und die ausgemusterter Räder stehen am Ende;
    /// der Name eines ausgemusterten Rads trägt den Zusatz „(ausgemustert)“.
    /// Wartungen (Typ „Wartung“) erscheinen hier nicht. Ohne gewähltes Rad („Alle Equipments“) erscheinen nur die
    /// Alarme der Equipments, die zur Aktivität passen (siehe <see cref="Wartungsaktivitaet.Passt"/>), auch die
    /// ausgemusterter; ohne Aktivität alle Alarme – so zählen Titel und Hinweis beim Programmstart alle fälligen.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipmentId">Nur die Alarme dieses Rads; <c>null</c> = alle Alarme der Aktivität.</param>
    /// <param name="aktivitaetId">Die gültige Aktivität (siehe <see cref="Wartungsaktivitaet.Bestimmen"/>); <c>null</c> = alle.</param>
    /// <returns>Die Zeilen.</returns>
    public static List<Alarmzeile> Ausfuehren(Datenbestand bestand, long? equipmentId, long? aktivitaetId = null)
    {
        Dictionary<long, Equipment> equipments = bestand.Equipments.ToDictionary(e => e.Id);
        return bestand.Wartungen
            .Where(w => w.Typ == WartungsTyp.Alarm
                && (equipmentId is null
                    ? Wartungsaktivitaet.Passt(w.EquipmentId is long id ? equipments.GetValueOrDefault(id) : null, aktivitaetId)
                    : w.EquipmentId == equipmentId))
            .Select(alarm =>
            {
                Alarmstand stand = BerechneAlarmstand.Ausfuehren(bestand, alarm);
                string rad = alarm.EquipmentId is long id ? equipments.GetValueOrDefault(id) is Equipment e ? EquipmentAnzeigename.Bilden(e) : string.Empty : string.Empty;
                return new Alarmzeile(alarm.Id, rad, alarm.Taetigkeit, alarm.AlarmKm, stand.GefahrenKm, stand.VerbleibendKm, stand.Zustand);
            })
            .OrderBy(z => z.Zustand)
            .ThenBy(z => z.VerbleibendKm)
            .ThenBy(z => z.Rad, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
