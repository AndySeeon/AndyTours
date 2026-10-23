namespace AndyTours.Core.Modell;

/// <summary>
/// Einstellungen der automatischen Datensicherung eines Datenordners.
/// </summary>
/// <remarks>
/// Die Werte gehören zum Datenbestand und liegen in der Datenbank des Datenordners
/// (Tabelle <c>Einstellung</c>). Nach einem Wechsel des Datenordners gelten deshalb die
/// Einstellungen und der Startzähler des neuen Ordners.
/// <para>
/// Ein neu erzeugtes Objekt enthält die Standardwerte, die gelten, solange der Benutzer in den
/// Einstellungen nichts gewählt hat: alle 3 Programmstarts sichern, höchstens 10 Sicherungen
/// aufbewahren, Sicherungsordner <c>Sicherungen</c> im Datenordner.
/// </para>
/// </remarks>
public record Sicherungseinstellungen
{
    /// <summary>Standard-Intervall: nach so vielen Programmstarts wird gesichert.</summary>
    public const int StandardIntervall = 3;

    /// <summary>Standard-Höchstzahl der aufbewahrten Sicherungen.</summary>
    public const int StandardHoechstzahl = 10;

    /// <summary>Name des Standard-Sicherungsordners; er liegt im Datenordner.</summary>
    public const string StandardUnterordner = "Sicherungen";

    /// <summary>Kleinster erlaubter Wert für Intervall und Höchstzahl.</summary>
    public const int Minimalwert = 1;

    /// <summary>Größter erlaubter Wert für Intervall und Höchstzahl.</summary>
    public const int Maximalwert = 99;

    /// <summary>
    /// Vom Benutzer gewählter Sicherungsordner (vollständiger Pfad) oder <c>null</c>, wenn
    /// der Standardordner <see cref="StandardUnterordner"/> im Datenordner gilt.
    /// </summary>
    public string? Sicherungsordner { get; init; }

    /// <summary>Nach wie vielen Programmstarts gesichert wird (1 bis 99).</summary>
    public int Intervall { get; init; } = StandardIntervall;

    /// <summary>Wie viele Sicherungen höchstens aufbewahrt werden (1 bis 99).</summary>
    public int Hoechstzahl { get; init; } = StandardHoechstzahl;

    /// <summary>
    /// Anzahl der Programmstarts seit der letzten erfolgreichen Sicherung (Startzähler).
    /// </summary>
    public int StartsSeitSicherung { get; init; }
}
