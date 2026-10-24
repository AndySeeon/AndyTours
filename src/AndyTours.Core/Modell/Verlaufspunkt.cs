namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Messpunkt im Verlauf einer Tour, gelesen aus ihrer Aufzeichnung (Grundlage des Höhenprofils).
/// </summary>
/// <remarks>
/// Distanz und Fahrzeit sind immer bekannt; sie bilden die waagrechte Achse. Alle übrigen Werte fehlen
/// (<c>null</c>), wenn die Aufzeichnung sie an diesem Punkt nicht enthält, z. B. die Höhe bei einer Fahrt auf
/// der Rolle oder der Puls ohne Brustgurt.
/// </remarks>
public record Verlaufspunkt
{
    /// <summary>Zurückgelegte Distanz seit dem Start in Kilometern.</summary>
    public double DistanzKm { get; init; }

    /// <summary>Fahrzeit seit dem Start ohne Pausen.</summary>
    public TimeSpan Fahrzeit { get; init; }

    /// <summary>Höhe in Metern.</summary>
    public double? Hoehe { get; init; }

    /// <summary>Puls in Schlägen je Minute.</summary>
    public int? Puls { get; init; }

    /// <summary>Geschwindigkeit in km/h.</summary>
    public double? Geschwindigkeit { get; init; }

    /// <summary>Trittfrequenz in Umdrehungen je Minute.</summary>
    public int? Trittfrequenz { get; init; }

    /// <summary>Temperatur in °C.</summary>
    public int? Temperatur { get; init; }

    /// <summary>
    /// Liefert den Zusatzwert dieses Punkts.
    /// </summary>
    /// <param name="wert">Der gewünschte Zusatzwert.</param>
    /// <returns>Der Wert oder <c>null</c>, wenn er fehlt.</returns>
    public double? Wert(Verlaufswert wert) => wert switch
    {
        Verlaufswert.Puls => Puls,
        Verlaufswert.Geschwindigkeit => Geschwindigkeit,
        Verlaufswert.Trittfrequenz => Trittfrequenz,
        Verlaufswert.Temperatur => Temperatur,
        _ => null,
    };
}
