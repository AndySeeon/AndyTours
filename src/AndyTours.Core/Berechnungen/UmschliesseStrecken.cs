using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt das Rechteck, das mehrere Strecken umschließt (für „Ganze Strecke“ einer Mehrtagestour).
/// </summary>
public static class UmschliesseStrecken
{
    /// <summary>
    /// Liefert die Süd-West- und die Nord-Ost-Ecke des Rechtecks um alle Strecken.
    /// </summary>
    /// <param name="strecken">Die Strecken der Etappen; mindestens eine.</param>
    /// <returns>Kleinste Breite und Länge sowie größte Breite und Länge aller Strecken.</returns>
    public static (Geoposition SuedWest, Geoposition NordOst) Ausfuehren(IReadOnlyList<Strecke> strecken) =>
    (
        new Geoposition(strecken.Min(s => s.SuedWest.Breite), strecken.Min(s => s.SuedWest.Laenge)),
        new Geoposition(strecken.Max(s => s.NordOst.Breite), strecken.Max(s => s.NordOst.Laenge))
    );
}
