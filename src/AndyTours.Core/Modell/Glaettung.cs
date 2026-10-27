namespace AndyTours.Core.Modell;

/// <summary>
/// Stufe, mit der „Track bearbeiten“ die Positionen einer Strecke glättet.
/// </summary>
public enum Glaettung
{
    /// <summary>Nicht glätten: die Punkte der Aufzeichnung, wie sie sind.</summary>
    Aus,

    /// <summary>Schwach glätten: kleines Zittern verschwindet, Kurven bleiben genau.</summary>
    Schwach,

    /// <summary>Stark glätten: ruhigere Linie mit deutlich weniger Punkten.</summary>
    Stark,
}
