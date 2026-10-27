using AndyTours.Daten.Datenordner;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Wertet die Befehlszeile des Programms aus.
/// </summary>
public static class LeseAufrufparameter
{
    /// <summary>Name des Ordners mit den BikeXperience-Altdaten im Projektordner.</summary>
    public const string StandardQuellordner = "Daten";

    /// <summary>Schalter: nur feststellen, was im Ordner liegt.</summary>
    public const string SchalterErkennen = "--erkennen";

    /// <summary>Schalter: Aufzeichnungen als Touren übernehmen.</summary>
    public const string SchalterAufzeichnungen = "--aufzeichnungen";

    /// <summary>Schalter: keine Rückfrage auf der Konsole; vorhandene Daten im Ziel bleiben unverändert.</summary>
    public const string SchalterOhneRueckfrage = "--ohne-rueckfrage";

    /// <summary>
    /// Ermittelt Modus, Quell- und Zielordner aus den Argumenten.
    /// </summary>
    /// <remarks>
    /// Aufrufe:
    /// <list type="bullet">
    /// <item><c>AndyTours.Altdatenuebernahme [BikeXperience-Ordner] [Zielordner]</c> – Umwandlung wie bisher.
    /// Ohne Zielordner: <c>AppDaten</c> im Projektordner (der Entwicklungs-Datenordner von AndyTours);
    /// ohne beide Angaben zusätzlich <c>Daten</c> im Projektordner als BikeXperience-Ordner.</item>
    /// <item><c>AndyTours.Altdatenuebernahme --erkennen &lt;Ordner&gt;</c> – meldet, was im Ordner liegt.</item>
    /// <item><c>AndyTours.Altdatenuebernahme --aufzeichnungen &lt;Ordner&gt; &lt;Zielordner&gt;</c> – übernimmt die
    /// Aufzeichnungen des Ordners als Touren.</item>
    /// </list>
    /// <c>--ohne-rueckfrage</c> darf an beliebiger Stelle stehen und gilt für die Umwandlung und die Übernahme
    /// der Aufzeichnungen. Bei den neuen Modi gibt es keine Vorgaben; die Ordner müssen angegeben werden.
    /// Den Projektordner findet das Programm wie die App über die Projektmappe oberhalb des Programmordners.
    /// </remarks>
    /// <param name="argumente">Die Befehlszeilenargumente.</param>
    /// <param name="programmordner">Ordner des laufenden Programms; Ausgangspunkt für die Suche nach dem Projektordner.</param>
    /// <returns>Modus und die vollständigen Pfade der Ordner.</returns>
    /// <exception cref="ArgumentException">Falsche Anzahl Angaben oder unbekannter Schalter.</exception>
    /// <exception cref="InvalidOperationException">Eine Vorgabe wird gebraucht, aber kein Projektordner gefunden.</exception>
    public static Aufrufparameter Ausfuehren(IReadOnlyList<string> argumente, string programmordner)
    {
        bool ohneRueckfrage = argumente.Contains(SchalterOhneRueckfrage, StringComparer.OrdinalIgnoreCase);
        List<string> uebrige = argumente
            .Where(a => !string.Equals(a, SchalterOhneRueckfrage, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Neue Modi: der Schalter steht vorn, danach genau die nötigen Ordner
        if (uebrige.Count > 0 && string.Equals(uebrige[0], SchalterErkennen, StringComparison.OrdinalIgnoreCase))
        {
            if (uebrige.Count != 2)
            {
                throw new ArgumentException($"Nach {SchalterErkennen} muss genau ein Ordner folgen.");
            }

            return new Aufrufparameter(Path.GetFullPath(uebrige[1]), string.Empty, Programmmodus.Erkennen, ohneRueckfrage);
        }

        if (uebrige.Count > 0 && string.Equals(uebrige[0], SchalterAufzeichnungen, StringComparison.OrdinalIgnoreCase))
        {
            if (uebrige.Count != 3)
            {
                throw new ArgumentException($"Nach {SchalterAufzeichnungen} müssen der Ordner mit den Aufzeichnungen und der Zielordner folgen.");
            }

            return new Aufrufparameter(Path.GetFullPath(uebrige[1]), Path.GetFullPath(uebrige[2]), Programmmodus.Aufzeichnungen, ohneRueckfrage);
        }

        if (uebrige.FirstOrDefault(a => a.StartsWith("--", StringComparison.Ordinal)) is string unbekannt)
        {
            throw new ArgumentException($"Unbekannter Schalter {unbekannt}.");
        }

        // Bisheriger Aufruf: Umwandlung der BikeXperience-Daten
        if (uebrige.Count > 2)
        {
            throw new ArgumentException("Zu viele Angaben. Erlaubt sind höchstens BikeXperience-Ordner und Zielordner.");
        }

        // Projektordner nur suchen, wenn eine Vorgabe gebraucht wird
        string Projektordner() => FindeProjektordner.Ausfuehren(programmordner)
            ?? throw new InvalidOperationException(
                $"Kein Projektordner gefunden (keine {DatenordnerInhalt.Projektmappe} oberhalb von {programmordner}). " +
                "Bitte BikeXperience-Ordner und Zielordner angeben.");

        string quelle = uebrige.Count >= 1 ? uebrige[0] : Path.Combine(Projektordner(), StandardQuellordner);
        string ziel = uebrige.Count >= 2 ? uebrige[1] : Path.Combine(Projektordner(), DatenordnerInhalt.Entwicklungsordner);

        // Vollständige Pfade, damit Meldungen eindeutig sind und relative Angaben nicht vom aktuellen Ordner abhängen
        return new Aufrufparameter(Path.GetFullPath(quelle), Path.GetFullPath(ziel), Programmmodus.Umwandeln, ohneRueckfrage);
    }
}
