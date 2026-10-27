using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bestimmt das Rechteck um alle Punkte eines Overlays (für „ganze Strecke zeigen“ per Doppelklick in der Verwaltung).
/// </summary>
public static class UmschliesseOverlay
{
    /// <summary>
    /// Liefert die Süd-West- und die Nord-Ost-Ecke des Rechtecks um alle Abschnitte.
    /// </summary>
    /// <param name="overlay">Das Overlay; mindestens ein Abschnitt mit mindestens einem Punkt.</param>
    /// <returns>Kleinste Breite und Länge sowie größte Breite und Länge aller Punkte.</returns>
    public static (Geoposition SuedWest, Geoposition NordOst) Ausfuehren(Overlay overlay)
    {
        List<Geoposition> punkte = [.. overlay.Abschnitte.SelectMany(a => a)];
        return (
            new Geoposition(punkte.Min(p => p.Breite), punkte.Min(p => p.Laenge)),
            new Geoposition(punkte.Max(p => p.Breite), punkte.Max(p => p.Laenge)));
    }
}
