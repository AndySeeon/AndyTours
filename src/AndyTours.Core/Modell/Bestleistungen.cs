namespace AndyTours.Core.Modell;

/// <summary>
/// Die Bestleistungen der Übersicht: Jahresrekorde und Einzelrekorde.
/// </summary>
/// <remarks>Arten ohne einen vorhandenen Wert fehlen; ohne Einheiten sind beide Listen leer.</remarks>
/// <param name="Jahresrekorde">Je Wert höchstens ein Rekord, in der Reihenfolge km, Anzahl, hm, Fahrzeit.</param>
/// <param name="Einzelrekorde">Je Art höchstens ein Rekord, in der Reihenfolge von <see cref="Einzelrekordart"/>.</param>
public record Bestleistungen(IReadOnlyList<Jahresrekord> Jahresrekorde, IReadOnlyList<Einzelrekord> Einzelrekorde);
