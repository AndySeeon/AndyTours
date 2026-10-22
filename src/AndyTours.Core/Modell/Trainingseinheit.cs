namespace AndyTours.Core.Modell;

/// <summary>
/// Eine einzelne Trainingseinheit (Tour, Lauf) mit ihren Kennwerten.
/// </summary>
/// <remarks>
/// Entspricht im Original einer Zeile der Datei <c>workouts.csv</c> (Java-Klasse <c>TourEntry</c>).
/// Wie im Original bedeutet der Wert 0 bei Puls, Trittfrequenz, Leistung und Erholungspuls
/// „nicht erfasst". Bei der Temperatur ist 0 °C dagegen ein gültiger Messwert.
/// </remarks>
public class Trainingseinheit
{
    /// <summary>Eindeutige Nummer in der Datenbank.</summary>
    public long Id { get; set; }

    /// <summary>Sportler, der die Einheit absolviert hat (Verweis auf <see cref="Fahrer.Id"/>).</summary>
    public long FahrerId { get; set; }

    /// <summary>Datum der Einheit.</summary>
    public DateOnly Datum { get; set; }

    /// <summary>Startzeit (Uhrzeit), falls bekannt.</summary>
    public TimeOnly? Startzeit { get; set; }

    /// <summary>Name der Tour, z. B. „Bründlingalm" oder der Zeitstempel der Aufzeichnung.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Reine Fahr- bzw. Laufzeit (ohne Pausen).</summary>
    public TimeSpan Fahrzeit { get; set; }

    /// <summary>Zurückgelegte Distanz in Kilometern.</summary>
    public double DistanzKm { get; set; }

    /// <summary>Summe der bergauf gefahrenen Höhenmeter.</summary>
    public int Hoehenmeter { get; set; }

    /// <summary>Höchster Punkt der Tour in Metern über dem Meeresspiegel.</summary>
    public int MaxHoehe { get; set; }

    /// <summary>Höhe des Startpunkts in Metern über dem Meeresspiegel.</summary>
    public int StartHoehe { get; set; }

    /// <summary>Höchstgeschwindigkeit in km/h.</summary>
    public double MaxGeschwindigkeit { get; set; }

    /// <summary>Durchschnittliche Herzfrequenz in Schlägen pro Minute (0 = nicht erfasst).</summary>
    public int HfDurchschnitt { get; set; }

    /// <summary>Maximale Herzfrequenz (0 = nicht erfasst).</summary>
    public int HfMax { get; set; }

    /// <summary>Minimale Herzfrequenz (0 = nicht erfasst).</summary>
    public int HfMin { get; set; }

    /// <summary>Durchschnittliche Trittfrequenz in Umdrehungen pro Minute (0 = nicht erfasst).</summary>
    public int Trittfrequenz { get; set; }

    /// <summary>Durchschnittstemperatur in °C.</summary>
    public int Temperatur { get; set; }

    /// <summary>Durchschnittliche Leistung in Watt (0 = nicht erfasst).</summary>
    public double LeistungWatt { get; set; }

    /// <summary>Körpergewicht des Sportlers zum Zeitpunkt der Einheit in Kilogramm.</summary>
    public double KoerpergewichtKg { get; set; }

    /// <summary>Erholungspuls (0 = nicht erfasst).</summary>
    public int Erholungspuls { get; set; }

    /// <summary>Kürzel des Trainingsbereichs, z. B. „GA1", „GA2", „WSA".</summary>
    public string Trainingsbereich { get; set; } = string.Empty;

    /// <summary>Freier Kommentar.</summary>
    public string Kommentar { get; set; } = string.Empty;

    /// <summary>Aktivität der Einheit (Verweis auf <see cref="Aktivitaet.Id"/>).</summary>
    public long AktivitaetId { get; set; }

    /// <summary>Verwendetes Equipment (Verweis auf <see cref="Equipment.Id"/>), falls angegeben.</summary>
    public long? EquipmentId { get; set; }

    /// <summary>Zugeordnete Kategorien (Verweise auf <see cref="Kategorie.Id"/>).</summary>
    public List<long> KategorieIds { get; set; } = new();

    /// <summary>Angehängte Dateien (FIT-Aufzeichnung, Dokumentation …).</summary>
    public List<Anhang> Anhaenge { get; set; } = new();
}
