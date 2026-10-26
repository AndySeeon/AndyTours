using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wählt aus einer gefahrenen Spur die Stützpunkte, über die der Routingdienst die Strecke nachrechnet, um ihre
/// Abbiegungen zu ermitteln (Spec 032).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Die Spur wird ausgedünnt (Douglas-Peucker): Ein Punkt bleibt, wenn die Spur ohne ihn um mehr als
/// <see cref="ToleranzMeter"/> abwiche. So bleiben Kurven und Abzweigungen erhalten, gerade Stücke schrumpfen auf ihre
/// Enden.</item>
/// <item>Liegen danach zwei Stützpunkte entlang der Spur mehr als <see cref="HoechstabstandMeter"/> auseinander, kommen
/// Punkte der Spur dazwischen. Sonst könnte der Dienst auf einem langen geraden Stück einen anderen Weg wählen.</item>
/// <item>Die Stützpunkte werden in Pakete zu höchstens <see cref="Paketgroesse"/> Punkten geteilt, je eines für eine
/// Anfrage. Aufeinanderfolgende Pakete teilen sich einen Punkt, damit keine Lücke entsteht.</item>
/// </list>
/// </remarks>
public static class WaehleStuetzpunkte
{
    /// <summary>Erlaubte Abweichung beim Ausdünnen in Metern.</summary>
    public const double ToleranzMeter = 20;

    /// <summary>Größter Abstand zweier Stützpunkte entlang der Spur in Metern.</summary>
    public const double HoechstabstandMeter = 1000;

    /// <summary>Höchstzahl der Punkte je Anfrage.</summary>
    public const int Paketgroesse = 25;

    /// <summary>
    /// Wählt die Stützpunkte und teilt sie in Pakete.
    /// </summary>
    /// <param name="spur">Die Positionen der gefahrenen Strecke in Fahrtrichtung.</param>
    /// <returns>Die Pakete in Fahrtrichtung; leer bei weniger als zwei Positionen.</returns>
    public static IReadOnlyList<IReadOnlyList<Geoposition>> Ausfuehren(IReadOnlyList<Geoposition> spur)
    {
        if (spur.Count < 2)
        {
            return [];
        }

        List<int> stuetzen = Verdichte(spur, Duenne(spur));
        List<IReadOnlyList<Geoposition>> pakete = [];
        for (int anfang = 0; anfang < stuetzen.Count - 1; anfang += Paketgroesse - 1)
        {
            int ende = Math.Min(anfang + Paketgroesse, stuetzen.Count);
            pakete.Add([.. stuetzen[anfang..ende].Select(i => spur[i])]);
        }

        return pakete;
    }

    /// <summary>Nummern der Punkte, die beim Ausdünnen bleiben, aufsteigend; Anfang und Ende immer.</summary>
    private static SortedSet<int> Duenne(IReadOnlyList<Geoposition> spur)
    {
        SortedSet<int> bleibt = [0, spur.Count - 1];

        // Ohne Rekursion, weil eine lange Aufzeichnung viele tausend Punkte hat
        var offen = new Stack<(int Von, int Bis)>();
        offen.Push((0, spur.Count - 1));
        while (offen.Count > 0)
        {
            (int von, int bis) = offen.Pop();
            int weitester = -1;
            double groesster = ToleranzMeter;
            for (int i = von + 1; i < bis; i++)
            {
                double abstand = Geoabstand.ZurStrecke(spur[i], spur[von], spur[bis]);
                if (abstand > groesster)
                {
                    groesster = abstand;
                    weitester = i;
                }
            }

            if (weitester >= 0)
            {
                bleibt.Add(weitester);
                offen.Push((von, weitester));
                offen.Push((weitester, bis));
            }
        }

        return bleibt;
    }

    /// <summary>Ergänzt Punkte der Spur, sodass zwei Stützpunkte höchstens <see cref="HoechstabstandMeter"/> auseinanderliegen.</summary>
    private static List<int> Verdichte(IReadOnlyList<Geoposition> spur, SortedSet<int> bleibt)
    {
        List<int> stuetzen = [0];
        double seitLetztem = 0;
        for (int i = 1; i < spur.Count; i++)
        {
            double schritt = Geoabstand.Meter(spur[i - 1], spur[i]);

            // Der vorige Punkt wird Stützpunkt, wenn es mit diesem Schritt zu weit würde
            if (seitLetztem + schritt > HoechstabstandMeter && stuetzen[^1] != i - 1)
            {
                stuetzen.Add(i - 1);
                seitLetztem = 0;
            }

            seitLetztem += schritt;
            if (bleibt.Contains(i))
            {
                stuetzen.Add(i);
                seitLetztem = 0;
            }
        }

        return stuetzen;
    }
}
