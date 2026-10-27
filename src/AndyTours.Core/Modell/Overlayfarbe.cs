namespace AndyTours.Core.Modell;

/// <summary>
/// Die wählbaren Farben eines Overlays (gemeinsame Overlay-Farbe in den Einstellungen und eigene Farbe je Overlay).
/// </summary>
/// <remarks>
/// Eine feste Liste gut unterscheidbarer Farben statt eines freien Farbdialogs. Die Farbwerte und Anzeigenamen legt
/// die Oberfläche fest; in der Datenbank steht der Name des Eintrags im Klartext (z. B. „Violett“).
/// </remarks>
public enum Overlayfarbe
{
    /// <summary>Violett – die Voreinstellung der gemeinsamen Overlay-Farbe.</summary>
    Violett,

    /// <summary>Blau.</summary>
    Blau,

    /// <summary>Hellblau.</summary>
    Hellblau,

    /// <summary>Türkis.</summary>
    Tuerkis,

    /// <summary>Grün.</summary>
    Gruen,

    /// <summary>Gelb.</summary>
    Gelb,

    /// <summary>Orange.</summary>
    Orange,

    /// <summary>Rot.</summary>
    Rot,

    /// <summary>Pink.</summary>
    Pink,

    /// <summary>Braun.</summary>
    Braun,

    /// <summary>Grau.</summary>
    Grau,

    /// <summary>Schwarz.</summary>
    Schwarz,
}
