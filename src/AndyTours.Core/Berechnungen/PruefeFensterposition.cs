using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Sorgt dafür, dass ein gemerktes Fenster beim Start sichtbar ist.
/// </summary>
public static class PruefeFensterposition
{
    /// <summary>Breite des Titelleisten-Stücks, das mindestens auf einem Bildschirm liegen muss.</summary>
    public const double MindestbreiteSichtbar = 100;

    /// <summary>Höhe des Titelleisten-Stücks, das mindestens auf einem Bildschirm liegen muss.</summary>
    public const double MindesthoeheSichtbar = 30;

    /// <summary>
    /// Liefert die gemerkte Position, wenn das Fenster damit greifbar bleibt, sonst eine Position auf dem Hauptbildschirm.
    /// </summary>
    /// <remarks>
    /// Greifbar heißt: Ein Stück der Titelleiste (oben im Fenster, mindestens
    /// <see cref="MindestbreiteSichtbar"/> × <see cref="MindesthoeheSichtbar"/>) liegt im Bereich aller
    /// Bildschirme – dann kann der Benutzer das Fenster anfassen und verschieben. Das ist z. B. nicht
    /// mehr der Fall, wenn es zuletzt auf einem inzwischen abgesteckten zweiten Bildschirm lag.
    /// Dann wird es mittig auf den Arbeitsbereich des Hauptbildschirms gesetzt und höchstens so groß
    /// wie dieser; der Zustand „maximiert“ bleibt erhalten.
    /// </remarks>
    /// <param name="gemerkt">Die gemerkte Position.</param>
    /// <param name="alleBildschirme">Umgebendes Rechteck aller Bildschirme.</param>
    /// <param name="hauptbildschirm">Arbeitsbereich des Hauptbildschirms (ohne Taskleiste).</param>
    /// <returns>Die zu verwendende Position.</returns>
    public static Fensterposition Ausfuehren(Fensterposition gemerkt, Bildschirmbereich alleBildschirme, Bildschirmbereich hauptbildschirm)
    {
        // Überlappung der Titelleiste (obere Kante des Fensters) mit dem Bereich aller Bildschirme
        double ueberlappungBreite = Math.Min(gemerkt.Links + gemerkt.Breite, alleBildschirme.Links + alleBildschirme.Breite)
            - Math.Max(gemerkt.Links, alleBildschirme.Links);
        double ueberlappungHoehe = Math.Min(gemerkt.Oben + MindesthoeheSichtbar, alleBildschirme.Oben + alleBildschirme.Hoehe)
            - Math.Max(gemerkt.Oben, alleBildschirme.Oben);

        if (ueberlappungBreite >= MindestbreiteSichtbar && ueberlappungHoehe >= MindesthoeheSichtbar)
        {
            return gemerkt;
        }

        double breite = Math.Min(gemerkt.Breite, hauptbildschirm.Breite);
        double hoehe = Math.Min(gemerkt.Hoehe, hauptbildschirm.Hoehe);
        return gemerkt with
        {
            Links = hauptbildschirm.Links + (hauptbildschirm.Breite - breite) / 2,
            Oben = hauptbildschirm.Oben + (hauptbildschirm.Hoehe - hoehe) / 2,
            Breite = breite,
            Hoehe = hoehe,
        };
    }
}
