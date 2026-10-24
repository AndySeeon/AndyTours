// WPF-Projekte binden System.IO nicht automatisch ein (Namenskonflikt Path ↔ System.Windows.Shapes.Path)
using System.IO;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Feste Pfade und Kennzeichen der App, die vom Rechner und von der Programmversion abhängen.
/// </summary>
public static class Programmpfade
{
    /// <summary>
    /// <c>true</c>, wenn die App als Entwicklungsversion läuft (Debug-Build, z. B. Start aus
    /// Visual Studio oder mit <c>dotnet run</c>); dann gilt immer der Datenordner <c>AppDaten</c>
    /// im Projektordner.
    /// </summary>
    /// <remarks>
    /// <c>DEBUG</c> wird vom Compiler nur im Debug-Build gesetzt. Die fertige Programmversion
    /// (Release-Build) liefert hier <c>false</c>.
    /// </remarks>
    public static bool IstEntwicklung =>
#if DEBUG
        true;
#else
        false;
#endif

    /// <summary>
    /// Merkdatei für den zuletzt gewählten Datenordner der fertigen App:
    /// <c>%APPDATA%\AndyTours\datenordner.txt</c> (benutzerbezogen, unabhängig vom Programmordner).
    /// </summary>
    public static string Merkdatei => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AndyTours", "datenordner.txt");

    /// <summary>Standard-Datenordner beim ersten Start der fertigen App: <c>Dokumente\AndyTours</c>.</summary>
    public static string Standardordner => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AndyTours");

    /// <summary>
    /// Zwischenspeicher der Kartenausschnitte für die Streckenkarte: <c>%LOCALAPPDATA%\AndyTours\Karten</c>.
    /// </summary>
    /// <remarks>
    /// Bewusst nicht im Datenordner: Kartenausschnitte sind keine Tourdaten und sollen weder auf ein NAS noch in die
    /// Sicherung. Je Kartenart gibt es darin einen Unterordner.
    /// </remarks>
    public static string Kartenspeicher => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AndyTours", "Karten");

    /// <summary>Ordner, in dem das laufende Programm liegt; Ausgangspunkt für die Suche nach dem Projektordner.</summary>
    public static string Programmordner => AppContext.BaseDirectory;
}
