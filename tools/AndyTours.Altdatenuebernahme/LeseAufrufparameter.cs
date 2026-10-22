using AndyTours.Daten.Datenordner;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Wertet die Befehlszeile des Programms aus.
/// </summary>
public static class LeseAufrufparameter
{
    /// <summary>Name des Ordners mit den BikeXperience-Altdaten im Projektordner.</summary>
    public const string StandardQuellordner = "Daten";

    /// <summary>
    /// Ermittelt Quell- und Zielordner aus den Argumenten.
    /// </summary>
    /// <remarks>
    /// Aufruf: <c>AndyTours.Altdatenuebernahme [BikeXperience-Ordner] [Zielordner]</c>
    /// <list type="bullet">
    /// <item>Ohne Zielordner: <c>AppDaten</c> im Projektordner (der Entwicklungs-Datenordner von AndyTours).</item>
    /// <item>Ohne beide Angaben zusätzlich: <c>Daten</c> im Projektordner als BikeXperience-Ordner.</item>
    /// </list>
    /// Den Projektordner findet das Programm wie die App über die Projektmappe oberhalb des Programmordners.
    /// </remarks>
    /// <param name="argumente">Die Befehlszeilenargumente.</param>
    /// <param name="programmordner">Ordner des laufenden Programms; Ausgangspunkt für die Suche nach dem Projektordner.</param>
    /// <returns>Die vollständigen Pfade beider Ordner.</returns>
    /// <exception cref="ArgumentException">Mehr als zwei Argumente.</exception>
    /// <exception cref="InvalidOperationException">Eine Vorgabe wird gebraucht, aber kein Projektordner gefunden.</exception>
    public static Aufrufparameter Ausfuehren(IReadOnlyList<string> argumente, string programmordner)
    {
        if (argumente.Count > 2)
        {
            throw new ArgumentException("Zu viele Angaben. Erlaubt sind höchstens BikeXperience-Ordner und Zielordner.");
        }

        // Projektordner nur suchen, wenn eine Vorgabe gebraucht wird
        string Projektordner() => FindeProjektordner.Ausfuehren(programmordner)
            ?? throw new InvalidOperationException(
                $"Kein Projektordner gefunden (keine {DatenordnerInhalt.Projektmappe} oberhalb von {programmordner}). " +
                "Bitte BikeXperience-Ordner und Zielordner angeben.");

        string quelle = argumente.Count >= 1 ? argumente[0] : Path.Combine(Projektordner(), StandardQuellordner);
        string ziel = argumente.Count >= 2 ? argumente[1] : Path.Combine(Projektordner(), DatenordnerInhalt.Entwicklungsordner);

        // Vollständige Pfade, damit Meldungen eindeutig sind und relative Angaben nicht vom aktuellen Ordner abhängen
        return new Aufrufparameter(Path.GetFullPath(quelle), Path.GetFullPath(ziel));
    }
}
