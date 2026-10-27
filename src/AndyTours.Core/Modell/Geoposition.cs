namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Position auf der Erde in Grad (WGS 84, wie GPS und OpenStreetMap sie verwenden).
/// </summary>
/// <param name="Breite">Geografische Breite in Grad, nördlich positiv (−90 bis 90).</param>
/// <param name="Laenge">Geografische Länge in Grad, östlich positiv (−180 bis 180).</param>
public record Geoposition(double Breite, double Laenge);
