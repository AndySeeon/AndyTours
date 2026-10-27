using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erstellt ein Teilstück als gerade Linie – wenn der Routingdienst die Strecke nicht ermitteln konnte (z. B. ohne
/// Internet).
/// </summary>
public static class ErstelleLuftlinie
{
    /// <summary>
    /// Liefert die gerade Verbindung zweier Wegpunkte.
    /// </summary>
    /// <remarks>
    /// Die Luftlinie hat keine Höhen und keine Abbiegepunkte; ihre Länge zählt in der Planung mit, Höhenmeter nicht.
    /// </remarks>
    /// <param name="von">Anfangswegpunkt.</param>
    /// <param name="nach">Endwegpunkt.</param>
    /// <returns>Das Teilstück mit genau diesen beiden Punkten, als Luftlinie gekennzeichnet.</returns>
    public static Teilstueck Ausfuehren(Geoposition von, Geoposition nach) =>
        new([new Verlaufspunkt { Position = von }, new Verlaufspunkt { Position = nach }], [], IstLuftlinie: true);
}
