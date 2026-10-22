namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Eintrag in der Wartungsliste: eine durchgeführte Wartung oder ein Kilometer-Alarm.
/// </summary>
/// <remarks>
/// Entspricht im Original einer Zeile der Datei <c>maintanence.dat</c>
/// (Java-Klasse <c>datamodel.Maintenance</c>).
/// </remarks>
public class Wartung
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Datum der Wartung bzw. ab dem der Alarm zählt.</summary>
    public DateOnly Datum { get; set; }

    /// <summary>
    /// Betroffenes Equipment (Verweis auf <see cref="Equipment.Id"/>);
    /// <c>null</c> bei Einträgen, die für kein bestimmtes Rad gelten.
    /// </summary>
    public long? EquipmentId { get; set; }

    /// <summary>Durchgeführte Tätigkeit, z. B. „Kettenwechsel".</summary>
    public string Taetigkeit { get; set; } = string.Empty;

    /// <summary>Freier Kommentar, z. B. der gemessene Kettenverschleiß.</summary>
    public string Kommentar { get; set; } = string.Empty;

    /// <summary>Wartung oder Alarm.</summary>
    public WartungsTyp Typ { get; set; }

    /// <summary>Bei Alarmen: nach wie vielen Kilometern der Alarm ausgelöst wird.</summary>
    public double AlarmKm { get; set; }

    /// <summary><c>true</c>, solange der Eintrag aktiv ist (ein erledigter Alarm wird inaktiv).</summary>
    public bool Aktiv { get; set; } = true;
}
