namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet die Durchschnittsgeschwindigkeit einer Einheit.
/// </summary>
public static class BerechneDurchschnittsgeschwindigkeit
{
    /// <summary>
    /// Teilt die Distanz durch die Fahrzeit: km ÷ Stunden = km/h.
    /// </summary>
    /// <remarks>
    /// Fehlt die Distanz oder die Fahrzeit (0), lässt sich keine Geschwindigkeit angeben; dann wird
    /// <c>null</c> geliefert und die Zelle bleibt leer (statt „0“ bzw. einer Division durch 0).
    /// Die Fahrzeit ist die reine Bewegungszeit ohne Pausen, wie bei BikeXperience.
    /// </remarks>
    /// <param name="distanzKm">Distanz in Kilometern.</param>
    /// <param name="fahrzeit">Fahrzeit.</param>
    /// <returns>Die Geschwindigkeit in km/h oder <c>null</c>.</returns>
    public static double? Ausfuehren(double distanzKm, TimeSpan fahrzeit)
    {
        if (distanzKm <= 0 || fahrzeit <= TimeSpan.Zero)
        {
            return null;
        }

        return distanzKm / fahrzeit.TotalHours;
    }
}
