using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bearbeitet die Strecke einer Einheit im Tourdetail: kürzen, glätten, umkehren („Track bearbeiten“).
/// </summary>
public static class BearbeiteTrack
{
    /// <summary>
    /// Liefert die bearbeiteten Messpunkte.
    /// </summary>
    /// <remarks>
    /// Reihenfolge: zuerst das Stück von „Start“ bis „Ziel“ (beide einschließlich) ausschneiden, dann glätten
    /// (<see cref="GlaetteTrack"/>), zuletzt bei „Umkehren“ die Reihenfolge umdrehen. Höhe und Distanz der Punkte
    /// bleiben je Punkt erhalten. Es entsteht eine neue Liste; der Verlauf der Aufzeichnung bleibt unverändert.
    /// </remarks>
    /// <param name="punkte">Die Messpunkte der Aufzeichnung mit Position, in ihrer Reihenfolge.</param>
    /// <param name="start">Nummer (ab 0) des Startpunkts.</param>
    /// <param name="ziel">Nummer des Zielpunkts; mindestens <paramref name="start"/> + 1, damit zwei Punkte bleiben.</param>
    /// <param name="umkehren"><c>true</c>, wenn die Strecke rückwärts laufen soll.</param>
    /// <param name="glaettung">Stufe der Glättung.</param>
    /// <returns>Die bearbeiteten Messpunkte, mindestens zwei.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Start oder Ziel liegen außerhalb, oder Start liegt nicht vor Ziel.</exception>
    public static List<Verlaufspunkt> Ausfuehren(IReadOnlyList<Verlaufspunkt> punkte, int start, int ziel, bool umkehren, Glaettung glaettung)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(ziel, punkte.Count);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(ziel, start);

        List<Verlaufspunkt> stueck = punkte.Skip(start).Take(ziel - start + 1).ToList();
        List<Verlaufspunkt> ergebnis = GlaetteTrack.Ausfuehren(stueck, glaettung);
        if (umkehren)
        {
            ergebnis.Reverse();
        }

        return ergebnis;
    }
}
