using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt die Bestwerte (Höchstwerte) der angezeigten Trainingseinheiten.
/// </summary>
public static class ErmittleBestwerte
{
    /// <summary>
    /// Liefert je Bestwert-Spalte den höchsten vorhandenen Wert.
    /// </summary>
    /// <remarks>
    /// Bestwert-Spalten sind Distanz, Höhenmeter, Fahrzeit, Ø km/h und max km/h. Fehlende Werte
    /// (gespeichert als 0) zählen nicht; hat keine Einheit einen Wert, ist der Bestwert <c>null</c>
    /// und es wird nichts rot markiert. Ø km/h wird aus den Rohwerten berechnet
    /// (<see cref="BerechneDurchschnittsgeschwindigkeit"/>), nicht aus gerundeten Anzeigewerten.
    /// </remarks>
    /// <param name="einheiten">Die angezeigten Einheiten (aktuelle Auswahl).</param>
    /// <returns>Die Bestwerte.</returns>
    public static Bestwerte Ausfuehren(IReadOnlyCollection<Trainingseinheit> einheiten)
    {
        return new Bestwerte(
            DistanzKm: Hoechster(einheiten.Where(e => e.DistanzKm > 0).Select(e => e.DistanzKm)),
            Hoehenmeter: Hoechster(einheiten.Where(e => e.Hoehenmeter > 0).Select(e => e.Hoehenmeter)),
            Fahrzeit: Hoechster(einheiten.Where(e => e.Fahrzeit > TimeSpan.Zero).Select(e => e.Fahrzeit)),
            DurchschnittKmh: Hoechster(einheiten
                .Select(e => BerechneDurchschnittsgeschwindigkeit.Ausfuehren(e.DistanzKm, e.Fahrzeit))
                .OfType<double>()),
            MaxKmh: Hoechster(einheiten.Where(e => e.MaxGeschwindigkeit > 0).Select(e => e.MaxGeschwindigkeit)));
    }

    /// <summary>Größter Wert der Folge oder <c>null</c>, wenn sie leer ist.</summary>
    private static T? Hoechster<T>(IEnumerable<T> werte) where T : struct, IComparable<T>
    {
        T? hoechster = null;
        foreach (T wert in werte)
        {
            if (hoechster is null || wert.CompareTo(hoechster.Value) > 0)
            {
                hoechster = wert;
            }
        }

        return hoechster;
    }
}
