namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Zeile der Alarmliste auf der Karteikarte „Wartung“.
/// </summary>
/// <param name="Id">Nummer des Alarms.</param>
/// <param name="Rad">Name des Rads; leer, wenn der Alarm keinem Rad zugeordnet ist.</param>
/// <param name="Taetigkeit">Die Tätigkeit, z. B. „Kettenwechsel“.</param>
/// <param name="IntervallKm">Nach wie vielen km der Alarm fällig wird.</param>
/// <param name="GefahrenKm">Seit dem Bezugstag gefahrene km.</param>
/// <param name="VerbleibendKm">Bis zur Fälligkeit verbleibende km; negativ = Überschreitung.</param>
/// <param name="Zustand">Fällig, bald fällig, normal oder ausgeschaltet.</param>
public record Alarmzeile(long Id, string Rad, string Taetigkeit, double IntervallKm, double GefahrenKm, double VerbleibendKm, Alarmzustand Zustand);
