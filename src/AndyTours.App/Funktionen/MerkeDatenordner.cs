// WPF-Projekte binden System.IO nicht automatisch ein (Namenskonflikt Path ↔ System.Windows.Shapes.Path)
using System.IO;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Merkt sich den Datenordner für den nächsten Programmstart.
/// </summary>
public static class MerkeDatenordner
{
    /// <summary>
    /// Schreibt den Ordnerpfad in die Merkdatei (siehe <see cref="LeseGemerktenDatenordner.Merkdatei"/>).
    /// </summary>
    /// <param name="ordner">Vollständiger Pfad des Datenordners.</param>
    public static void Ausfuehren(string ordner)
    {
        // Den Unterordner %APPDATA%\AndyTours beim ersten Mal anlegen
        Directory.CreateDirectory(Path.GetDirectoryName(LeseGemerktenDatenordner.Merkdatei)!);
        File.WriteAllText(LeseGemerktenDatenordner.Merkdatei, ordner);
    }
}
