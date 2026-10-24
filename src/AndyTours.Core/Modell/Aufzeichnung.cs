namespace AndyTours.Core.Modell;

/// <summary>
/// Die gelesenen Werte einer Aufzeichnung (FIT-Datei) eines Garmin-Radcomputers.
/// </summary>
/// <remarks>
/// Aus einer Aufzeichnung wird beim Import eine <see cref="Trainingseinheit"/>. Wie bei der Einheit bedeutet
/// 0 bei Puls, Trittfrequenz und Leistung „nicht erfasst“. Rad-Kennung und Sensoren dienen der Gerätezuordnung:
/// Über sie erkennt AndyTours, mit welchem Equipment die Tour gefahren wurde.
/// </remarks>
public record Aufzeichnung
{
    /// <summary>Dateiname auf dem Gerät, z. B. „2026-09-26-08-02-49.fit“.</summary>
    public string Dateiname { get; init; } = string.Empty;

    /// <summary>Name des Geräts, mit dem aufgezeichnet wurde, z. B. „Edge 1040“.</summary>
    public string Geraet { get; init; } = string.Empty;

    /// <summary>Datum des Starts (Ortszeit).</summary>
    public DateOnly Datum { get; init; }

    /// <summary>Uhrzeit des Starts (Ortszeit), auf die Sekunde.</summary>
    public TimeOnly Startzeit { get; init; }

    /// <summary>Gestoppte Zeit ohne Pausen, wie sie der Edge als „Zeit“ anzeigt.</summary>
    public TimeSpan Fahrzeit { get; init; }

    /// <summary>Distanz in Kilometern.</summary>
    public double DistanzKm { get; init; }

    /// <summary>Summe der bergauf gefahrenen Höhenmeter.</summary>
    public int Hoehenmeter { get; init; }

    /// <summary>Höchster Punkt in Metern.</summary>
    public int MaxHoehe { get; init; }

    /// <summary>Höhe des Startpunkts in Metern.</summary>
    public int StartHoehe { get; init; }

    /// <summary>Höchstgeschwindigkeit in km/h.</summary>
    public double MaxGeschwindigkeit { get; init; }

    /// <summary>Durchschnittliche Herzfrequenz (0 = nicht erfasst).</summary>
    public int HfDurchschnitt { get; init; }

    /// <summary>Maximale Herzfrequenz (0 = nicht erfasst).</summary>
    public int HfMax { get; init; }

    /// <summary>Minimale Herzfrequenz (0 = nicht erfasst).</summary>
    public int HfMin { get; init; }

    /// <summary>Durchschnittliche Trittfrequenz (0 = nicht erfasst).</summary>
    public int Trittfrequenz { get; init; }

    /// <summary>Durchschnittstemperatur in °C.</summary>
    public int Temperatur { get; init; }

    /// <summary>Durchschnittliche Leistung in Watt (0 = nicht erfasst).</summary>
    public double LeistungWatt { get; init; }

    /// <summary>Name des Aktivitätsprofils am Gerät, z. B. „MTB“.</summary>
    public string Profil { get; init; } = string.Empty;

    /// <summary>
    /// Kennung des Rads, das am Edge nach der Tour gewählt wurde (leer = keine, z. B. bei älteren Geräten).
    /// </summary>
    public string RadKennung { get; init; } = string.Empty;

    /// <summary>Seriennummern der Geschwindigkeits- und Trittfrequenzsensoren, als Text.</summary>
    public IReadOnlyList<string> Sensoren { get; init; } = [];
}
