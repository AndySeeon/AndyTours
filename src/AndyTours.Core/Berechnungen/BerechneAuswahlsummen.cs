using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Berechnet die Summen der im Hauptfenster gewählten Trainingseinheiten.
/// </summary>
public static class BerechneAuswahlsummen
{
    /// <summary>
    /// Addiert Anzahl, Kilometer, Höhenmeter und Fahrzeit der übergebenen Einheiten.
    /// </summary>
    /// <remarks>
    /// Eine leere Auswahl ergibt überall 0. Die Distanz wird wie bei
    /// <see cref="BerechneJahressummen"/> auf 2 Nachkommastellen gerundet, damit die Werte genau
    /// mit den Jahressummen im Umwandlungsbericht übereinstimmen.
    /// </remarks>
    /// <param name="einheiten">Die gewählten Einheiten (siehe <see cref="FiltereAuswahl"/>).</param>
    /// <returns>Die Summen der Auswahl.</returns>
    public static Auswahlsumme Ausfuehren(IReadOnlyCollection<Trainingseinheit> einheiten)
    {
        return new Auswahlsumme(
            Anzahl: einheiten.Count,
            DistanzKm: Math.Round(einheiten.Sum(einheit => einheit.DistanzKm), 2),
            Hoehenmeter: einheiten.Sum(einheit => einheit.Hoehenmeter),
            Fahrzeit: TimeSpan.FromTicks(einheiten.Sum(einheit => einheit.Fahrzeit.Ticks)));
    }
}
