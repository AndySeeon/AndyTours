namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Punkt der Linie eines <see cref="Course"/>.
/// </summary>
/// <remarks>
/// Der Edge braucht zu jedem Punkt die Distanz seit dem Start und eine Zeit: daraus berechnet er den Fortschritt, die
/// voraussichtliche Ankunft und den virtuellen Partner.
/// </remarks>
/// <param name="Position">Position des Punkts.</param>
/// <param name="Hoehe">Höhe in Metern, falls bekannt (für das Höhenprofil auf dem Edge).</param>
/// <param name="DistanzKm">Zurückgelegte Distanz seit dem Start in Kilometern.</param>
/// <param name="Zeit">Zeit seit dem Start, zu der der virtuelle Partner hier ist.</param>
public record Coursepunkt(Geoposition Position, double? Hoehe, double DistanzKm, TimeSpan Zeit);
