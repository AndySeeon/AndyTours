namespace AndyTours.Core.Modell;

/// <summary>
/// Die gefahrene Strecke einer Tour für die Streckenkarte.
/// </summary>
/// <remarks>
/// Enthält nur Messpunkte mit Position, in ihrer Reihenfolge; Punkte ohne Position (z. B. im Tunnel) fehlen,
/// sodass die Linie die Punkte davor und danach verbindet. Der erste Punkt ist der Start, der letzte das Ziel.
/// Das umschließende Rechteck dient für „Ganze Strecke“.
/// </remarks>
/// <param name="Punkte">Die Positionen der Strecke; mindestens eine.</param>
/// <param name="SuedWest">Süd-West-Ecke des umschließenden Rechtecks (kleinste Breite und Länge).</param>
/// <param name="NordOst">Nord-Ost-Ecke des umschließenden Rechtecks (größte Breite und Länge).</param>
public record Strecke(IReadOnlyList<Geoposition> Punkte, Geoposition SuedWest, Geoposition NordOst);
