namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Feste Namen rund um den Datenordner von AndyTours.
/// </summary>
/// <remarks>
/// Der Datenordner enthält alles, was zu einem Datenbestand gehört: die Datenbank und die
/// Jahresordner mit den Aufzeichnungen (FIT-Dateien) und Anhängen. Die Anhangpfade in der
/// Datenbank sind relativ zu diesem Ordner gespeichert – deshalb lässt sich der Ordner als
/// Ganzes kopieren oder verschieben.
/// </remarks>
public static class DatenordnerInhalt
{
    /// <summary>
    /// Dateiname der Datenbank im Datenordner. Liegt diese Datei im Ordner, gilt er als
    /// „Ordner mit AndyTours-Daten“.
    /// </summary>
    public const string Datenbankdatei = "andytours.db";

    /// <summary>
    /// Name des Datenordners während der Entwicklung; er liegt direkt im Projektordner
    /// und ist von der Versionsverwaltung ausgeschlossen (.gitignore).
    /// </summary>
    public const string Entwicklungsordner = "AppDaten";

    /// <summary>
    /// Dateiname der Projektmappe. Der Ordner, in dem sie liegt, ist der Projektordner
    /// (siehe <see cref="FindeProjektordner"/>).
    /// </summary>
    public const string Projektmappe = "AndyTours.slnx";
}
