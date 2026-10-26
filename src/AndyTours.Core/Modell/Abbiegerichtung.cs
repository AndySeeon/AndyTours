namespace AndyTours.Core.Modell;

/// <summary>
/// Richtung, in die die geplante Strecke an einem Abbiegepunkt abbiegt.
/// </summary>
/// <remarks>
/// „Geradeaus“ gibt es bewusst nicht: An Abzweigungen, an denen es geradeaus weitergeht, entsteht kein Abbiegepunkt.
/// „Links halten“ und „rechts halten“ der Routingdienste zählen als „leicht links“ bzw. „leicht rechts“.
/// </remarks>
public enum Abbiegerichtung
{
    /// <summary>Links abbiegen.</summary>
    Links,

    /// <summary>Rechts abbiegen.</summary>
    Rechts,

    /// <summary>Leicht links abbiegen (auch „links halten“).</summary>
    LeichtLinks,

    /// <summary>Leicht rechts abbiegen (auch „rechts halten“).</summary>
    LeichtRechts,

    /// <summary>Scharf links abbiegen.</summary>
    ScharfLinks,

    /// <summary>Scharf rechts abbiegen.</summary>
    ScharfRechts,

    /// <summary>In den Kreisverkehr einfahren; die Ausfahrt steht im <see cref="Abbiegepunkt"/>.</summary>
    Kreisverkehr,

    /// <summary>Wenden.</summary>
    Wenden,
}
