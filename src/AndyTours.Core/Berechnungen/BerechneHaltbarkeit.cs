using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet zu einer Wartung, wie viele Kilometer und Tage seit der vorherigen gleichartigen Wartung vergangen
/// sind – also wie lange z. B. die gewechselte Kette gehalten hat.
/// </summary>
public static class BerechneHaltbarkeit
{
    /// <summary>
    /// Liefert km und Tage seit der vorherigen Wartung mit derselben Tätigkeit an diesem Rad.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Gleichartig ist eine Wartung (kein Alarm) am selben Rad mit derselben Tätigkeit, ohne Unterschied der
    /// Groß- und Kleinschreibung. Die vorherige ist die mit dem spätesten Datum vor dieser; bei gleichem Datum gilt
    /// die zuvor erfasste (kleinere Nummer) als vorherige – das Wartungsfenster lässt solche Doppel nicht zu, die
    /// Berechnung bleibt aber eindeutig (0 km, 0 Tage).</item>
    /// <item>Die km sind die Differenz der beiden km-Stände (<see cref="BerechneKmStand"/>): Es zählen die Einheiten
    /// mit diesem Rad nach dem Tag der vorherigen Wartung bis einschließlich zum Tag dieser Wartung.</item>
    /// <item>Die Tage sind die Kalendertage zwischen den beiden Daten (01.05. bis 31.05. = 30 Tage).</item>
    /// <item>Gibt es keine vorherige, sind die km der km-Stand des Rads an diesem Tag (mit Anfangskilometern) und die
    /// Tage die seit dem Anschaffungsdatum. Fehlt dieses oder liegt es nach der Wartung, zählen die Tage seit der
    /// ersten Einheit mit dem Rad bis zum Tag der Wartung; gibt es auch keine, bleiben die Tage leer.</item>
    /// <item>Ohne Rad (oder bei einem unbekannten Rad) bleiben beide Werte leer.</item>
    /// </list>
    /// </remarks>
    /// <param name="bestand">Der Datenbestand (Räder, Einheiten, Wartungen).</param>
    /// <param name="wartung">Die Wartung (Typ <see cref="WartungsTyp.Wartung"/>).</param>
    /// <returns>km und Tage seit der vorherigen gleichartigen Wartung.</returns>
    public static Haltbarkeit Ausfuehren(Datenbestand bestand, Wartung wartung)
    {
        Equipment? rad = bestand.Equipments.FirstOrDefault(e => e.Id == wartung.EquipmentId);
        if (rad is null)
        {
            return new Haltbarkeit(null, null);
        }

        double kmStand = BerechneKmStand.Ausfuehren(bestand.Trainingseinheiten, rad, wartung.Datum);

        // Vorherige gleichartige Wartung: spätestes Datum davor, bei gleichem Datum die kleinere Nummer
        string taetigkeit = wartung.Taetigkeit.Trim();
        Wartung? vorherige = bestand.Wartungen
            .Where(w => w.Typ == WartungsTyp.Wartung
                && w.EquipmentId == rad.Id
                && string.Equals(w.Taetigkeit.Trim(), taetigkeit, StringComparison.OrdinalIgnoreCase)
                && (w.Datum < wartung.Datum || (w.Datum == wartung.Datum && w.Id < wartung.Id)))
            .OrderByDescending(w => w.Datum)
            .ThenByDescending(w => w.Id)
            .FirstOrDefault();

        if (vorherige is not null)
        {
            double kmVorher = BerechneKmStand.Ausfuehren(bestand.Trainingseinheiten, rad, vorherige.Datum);
            return new Haltbarkeit(kmStand - kmVorher, wartung.Datum.DayNumber - vorherige.Datum.DayNumber);
        }

        // Erste Wartung ihrer Art: ab der Anschaffung, sonst ab der ersten Einheit mit diesem Rad
        DateOnly? anfang = rad.Anschaffungsdatum is DateOnly angeschafft && angeschafft <= wartung.Datum
            ? angeschafft
            : bestand.Trainingseinheiten
                .Where(e => e.EquipmentId == rad.Id && e.Datum <= wartung.Datum)
                .Select(e => (DateOnly?)e.Datum)
                .Min();

        return new Haltbarkeit(kmStand, anfang is DateOnly tag ? wartung.Datum.DayNumber - tag.DayNumber : null);
    }
}
