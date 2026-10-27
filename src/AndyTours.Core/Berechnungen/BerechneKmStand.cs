using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet den Kilometerstand eines Rads an einem Tag (Spalte „km-Stand“ der Wartungstabelle).
/// </summary>
public static class BerechneKmStand
{
    /// <summary>
    /// Liefert die Anfangskilometer des Rads plus die Distanzen aller seiner Einheiten bis einschließlich zum Tag.
    /// </summary>
    /// <remarks>
    /// Die Anfangskilometer sind die Kilometer, die das Rad schon vor der Erfassung in AndyTours gefahren ist;
    /// so entspricht der km-Stand dem Tacho. Einheiten am selben Tag zählen mit, auch wenn sie nach der Wartung
    /// gefahren wurden – die Wartung hat keine Uhrzeit.
    /// </remarks>
    /// <param name="einheiten">Alle Trainingseinheiten.</param>
    /// <param name="rad">Das Rad.</param>
    /// <param name="tag">Der Tag, z. B. der Tag einer Wartung.</param>
    /// <returns>Der km-Stand.</returns>
    public static double Ausfuehren(IEnumerable<Trainingseinheit> einheiten, Equipment rad, DateOnly tag) =>
        rad.AnfangsdistanzKm + einheiten.Where(e => e.EquipmentId == rad.Id && e.Datum <= tag).Sum(e => e.DistanzKm);
}
