namespace AndyTours.Core.Modell;

/// <summary>
/// Höchstwerte der angezeigten Einheiten; diese Werte werden in der Tabelle rot dargestellt.
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="Berechnungen.ErmittleBestwerte"/>. Ein Wert ist <c>null</c>, wenn keine
/// der Einheiten einen Wert in dieser Spalte hat.
/// </remarks>
/// <param name="DistanzKm">Größte Distanz.</param>
/// <param name="Hoehenmeter">Meiste Höhenmeter.</param>
/// <param name="Fahrzeit">Längste Fahrzeit.</param>
/// <param name="DurchschnittKmh">Höchste Durchschnittsgeschwindigkeit.</param>
/// <param name="MaxKmh">Höchste Höchstgeschwindigkeit.</param>
public record Bestwerte(double? DistanzKm, int? Hoehenmeter, TimeSpan? Fahrzeit, double? DurchschnittKmh, double? MaxKmh);
