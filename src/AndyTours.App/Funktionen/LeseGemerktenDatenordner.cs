// WPF-Projekte binden System.IO nicht automatisch ein (Namenskonflikt Path ↔ System.Windows.Shapes.Path)
using System.IO;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Liest den zuletzt verwendeten Datenordner, damit die Datenbank beim Start automatisch geöffnet wird.
/// </summary>
public static class LeseGemerktenDatenordner
{
    /// <summary>
    /// Pfad der Merkdatei: %APPDATA%\AndyTours\datenordner.txt (benutzerbezogen, unabhängig vom Programmordner).
    /// </summary>
    public static string Merkdatei => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AndyTours", "datenordner.txt");

    /// <summary>
    /// Liefert den gemerkten Ordner, sofern er noch existiert.
    /// </summary>
    /// <returns>Der Ordnerpfad oder <c>null</c>, wenn noch keiner gemerkt wurde oder er nicht mehr erreichbar ist (z. B. NAS getrennt).</returns>
    public static string? Ausfuehren()
    {
        if (!File.Exists(Merkdatei))
        {
            return null;
        }

        string ordner = File.ReadAllText(Merkdatei).Trim();
        return Directory.Exists(ordner) ? ordner : null;
    }
}
