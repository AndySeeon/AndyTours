namespace AndyTours.Core.Modell;

/// <summary>
/// Ein rechteckiger Bereich auf dem Bildschirm, z. B. der Arbeitsbereich des Hauptbildschirms.
/// </summary>
/// <param name="Links">Linker Rand.</param>
/// <param name="Oben">Oberer Rand.</param>
/// <param name="Breite">Breite.</param>
/// <param name="Hoehe">Höhe.</param>
public record Bildschirmbereich(double Links, double Oben, double Breite, double Hoehe);
