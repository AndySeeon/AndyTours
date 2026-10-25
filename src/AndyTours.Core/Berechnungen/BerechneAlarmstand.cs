using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet den Stand eines Wartungsalarms: ab wann gezählt wird, wie viele km seitdem gefahren wurden und ob
/// er fällig ist.
/// </summary>
public static class BerechneAlarmstand
{
    /// <summary>
    /// Liefert den Stand eines Alarms.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Bezugstag ist der Tag der letzten Wartung (nicht eines Alarms) mit derselben Tätigkeit an diesem
    /// Rad – ohne Unterschied der Schreibweise und auch, wenn sie vor dem Anlegen des Alarms liegt. Gibt es
    /// keine, zählt der Alarm ab dem Tag, an dem er angelegt wurde (sein Datum).</item>
    /// <item>Gezählt werden die Einheiten mit diesem Rad <em>nach</em> dem Bezugstag; eine Fahrt am Tag der
    /// Wartung gehört noch zum alten Teil (wie beim km-Stand, der den Tag einschließt).</item>
    /// <item>Fällig ist der Alarm, sobald die gefahrenen km das Intervall erreichen; bald fällig, wenn höchstens
    /// 10 % des Intervalls übrig sind. Ein ausgeschalteter Alarm, ein Alarm ohne Rad und der Alarm eines
    /// ausgemusterten Rads gelten als ausgeschaltet und werden nie fällig.</item>
    /// </list>
    /// </remarks>
    /// <param name="bestand">Der Datenbestand (Räder, Einheiten, Wartungen).</param>
    /// <param name="alarm">Der Alarm (Typ <see cref="WartungsTyp.Alarm"/>).</param>
    /// <returns>Der Stand.</returns>
    public static Alarmstand Ausfuehren(Datenbestand bestand, Wartung alarm)
    {
        string taetigkeit = alarm.Taetigkeit.Trim();
        DateOnly bezugstag = bestand.Wartungen
            .Where(w => w.Typ == WartungsTyp.Wartung
                && w.EquipmentId == alarm.EquipmentId
                && string.Equals(w.Taetigkeit.Trim(), taetigkeit, StringComparison.OrdinalIgnoreCase))
            .Select(w => (DateOnly?)w.Datum)
            .Max() ?? alarm.Datum;

        double gefahren = alarm.EquipmentId is long radId
            ? bestand.Trainingseinheiten.Where(e => e.EquipmentId == radId && e.Datum > bezugstag).Sum(e => e.DistanzKm)
            : 0;
        double verbleibend = alarm.AlarmKm - gefahren;

        Equipment? rad = bestand.Equipments.FirstOrDefault(e => e.Id == alarm.EquipmentId);
        Alarmzustand zustand =
            !alarm.Aktiv || rad is null || rad.Ausgemustert ? Alarmzustand.Ausgeschaltet
            : verbleibend <= 0 ? Alarmzustand.Faellig
            : verbleibend * 10 <= alarm.AlarmKm ? Alarmzustand.BaldFaellig // höchstens 10 % übrig, ohne Rundungsfehler verglichen
            : Alarmzustand.Normal;

        return new Alarmstand(bezugstag, gefahren, verbleibend, zustand);
    }
}
