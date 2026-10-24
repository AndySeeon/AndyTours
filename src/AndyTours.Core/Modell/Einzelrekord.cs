namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Einzelrekord: die Einheit mit dem höchsten Wert einer Art.
/// </summary>
/// <param name="Art">Die Art des Rekords.</param>
/// <param name="Wert">Der Rekordwert (km, hm, Stunden, km/h bzw. m).</param>
/// <param name="Einheit">Die Einheit, in der der Rekord aufgestellt wurde.</param>
public record Einzelrekord(Einzelrekordart Art, double Wert, Trainingseinheit Einheit);
