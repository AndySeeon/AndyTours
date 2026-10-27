using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Glättet die Positionen einer Strecke („Track bearbeiten“ → „Glätten“).
/// </summary>
public static class GlaetteTrack
{
    /// <summary>Meter je Grad geografischer Breite (Näherung, für Radtouren genau genug).</summary>
    private const double MeterJeGradBreite = 111_320;

    /// <summary>
    /// Liefert die geglätteten Messpunkte.
    /// </summary>
    /// <remarks>
    /// Zwei Schritte:
    /// <list type="number">
    /// <item>Gleitender Mittelwert der Positionen über benachbarte Punkte („schwach“ 5, „stark“ 15 Punkte; am Rand
    /// entsprechend weniger). Das GPS-Zittern verschwindet.</item>
    /// <item>Vereinfachung nach Douglas-Peucker: Punkte, die weniger als die Toleranz („schwach“ 3 m, „stark“ 10 m)
    /// neben der Linie ihrer Nachbarn liegen, fallen weg. Die Strecke hat danach weniger Punkte.</item>
    /// </list>
    /// Der erste und der letzte Punkt bleiben unverändert, damit Start und Ziel nicht wandern. Die übrigen Werte
    /// eines Punkts (Höhe, Distanz …) bleiben erhalten. Bei „aus“ oder weniger als drei Punkten kommen die Punkte
    /// unverändert zurück. Gerechnet wird in Metern auf einer örtlichen Ebene um den ersten Punkt.
    /// </remarks>
    /// <param name="punkte">Die Messpunkte in ihrer Reihenfolge; alle mit Position.</param>
    /// <param name="glaettung">Die Stufe.</param>
    /// <returns>Neue Liste der geglätteten Messpunkte; die übergebene bleibt unverändert.</returns>
    public static List<Verlaufspunkt> Ausfuehren(IReadOnlyList<Verlaufspunkt> punkte, Glaettung glaettung)
    {
        if (glaettung == Glaettung.Aus || punkte.Count < 3)
        {
            return [.. punkte];
        }

        (int fenster, double toleranzMeter) = glaettung == Glaettung.Stark ? (15, 10.0) : (5, 3.0);
        List<Verlaufspunkt> gemittelt = Mittele(punkte, fenster / 2);
        return Vereinfache(gemittelt, toleranzMeter);
    }

    /// <summary>Gleitender Mittelwert der Positionen; erster und letzter Punkt bleiben.</summary>
    private static List<Verlaufspunkt> Mittele(IReadOnlyList<Verlaufspunkt> punkte, int halbesFenster)
    {
        var ergebnis = new List<Verlaufspunkt>(punkte.Count) { punkte[0] };
        for (int i = 1; i < punkte.Count - 1; i++)
        {
            int von = Math.Max(0, i - halbesFenster);
            int bis = Math.Min(punkte.Count - 1, i + halbesFenster);
            double breite = 0, laenge = 0;
            for (int j = von; j <= bis; j++)
            {
                breite += punkte[j].Position!.Breite;
                laenge += punkte[j].Position!.Laenge;
            }

            int anzahl = bis - von + 1;
            ergebnis.Add(punkte[i] with { Position = new Geoposition(breite / anzahl, laenge / anzahl) });
        }

        ergebnis.Add(punkte[^1]);
        return ergebnis;
    }

    /// <summary>Vereinfachung nach Douglas-Peucker (ohne Rekursion, damit lange Strecken keinen Stapelüberlauf erzeugen).</summary>
    private static List<Verlaufspunkt> Vereinfache(List<Verlaufspunkt> punkte, double toleranzMeter)
    {
        Geoposition bezug = punkte[0].Position!;
        double meterJeGradLaenge = MeterJeGradBreite * Math.Cos(bezug.Breite * Math.PI / 180);
        (double X, double Y)[] eben = punkte
            .Select(p => ((p.Position!.Laenge - bezug.Laenge) * meterJeGradLaenge, (p.Position.Breite - bezug.Breite) * MeterJeGradBreite))
            .ToArray();

        bool[] behalten = new bool[punkte.Count];
        behalten[0] = behalten[^1] = true;
        var offen = new Stack<(int Von, int Bis)>();
        offen.Push((0, punkte.Count - 1));
        while (offen.Count > 0)
        {
            (int von, int bis) = offen.Pop();
            int weitester = -1;
            double groessterAbstand = toleranzMeter;
            for (int i = von + 1; i < bis; i++)
            {
                double abstand = AbstandZurStrecke(eben[i], eben[von], eben[bis]);
                if (abstand > groessterAbstand)
                {
                    groessterAbstand = abstand;
                    weitester = i;
                }
            }

            // Liegt ein Punkt weiter als die Toleranz neben der Linie, bleibt er, und beide Hälften werden geprüft
            if (weitester >= 0)
            {
                behalten[weitester] = true;
                offen.Push((von, weitester));
                offen.Push((weitester, bis));
            }
        }

        return punkte.Where((_, i) => behalten[i]).ToList();
    }

    /// <summary>Abstand eines Punkts von der Strecke zwischen zwei Punkten, in Metern.</summary>
    private static double AbstandZurStrecke((double X, double Y) p, (double X, double Y) a, (double X, double Y) b)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y;
        double laengeQuadrat = dx * dx + dy * dy;
        double t = laengeQuadrat == 0 ? 0 : Math.Clamp(((p.X - a.X) * dx + (p.Y - a.Y) * dy) / laengeQuadrat, 0, 1);
        double nx = a.X + t * dx - p.X, ny = a.Y + t * dy - p.Y;
        return Math.Sqrt(nx * nx + ny * ny);
    }
}
