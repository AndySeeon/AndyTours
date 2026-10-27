namespace AndyTours.Core.Modell;

/// <summary>
/// Die Kennzahlen einer geplanten Tour, wie das Planungsfenster sie anzeigt.
/// </summary>
/// <param name="LaengeKm">Länge in Kilometern.</param>
/// <param name="HoehenmeterBergauf">Summe der Anstiege in Metern.</param>
/// <param name="HoehenmeterBergab">Summe der Abstiege in Metern.</param>
public record Planungswerte(double LaengeKm, int HoehenmeterBergauf, int HoehenmeterBergab);
