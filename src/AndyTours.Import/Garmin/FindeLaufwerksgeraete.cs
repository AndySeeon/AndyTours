using System.Xml.Linq;

namespace AndyTours.Import.Garmin;

/// <summary>
/// Findet Garmin-Geräte, die als Laufwerk angeschlossen sind (Edge 800, Edge 1000), und ihre Aufzeichnungen.
/// </summary>
public static class FindeLaufwerksgeraete
{
    /// <summary>Ordner der Aufzeichnungen auf dem Laufwerk (Edge 800/1000).</summary>
    private static readonly string[] Aufzeichnungsordner = [Path.Combine("Garmin", "Activities"), Path.Combine("Garmin", "Activity")];

    /// <summary>
    /// Durchsucht alle bereiten Wechsellaufwerke.
    /// </summary>
    /// <returns>Die Aufzeichnungen aller gefundenen Geräte; leer, wenn keines angeschlossen ist.</returns>
    public static List<Geraeteaufzeichnung> Ausfuehren() =>
        Ausfuehren(DriveInfo.GetDrives()
            .Where(l => l.IsReady && l.DriveType == DriveType.Removable)
            .Select(l => l.RootDirectory.FullName));

    /// <summary>
    /// Durchsucht die angegebenen Laufwerkswurzeln.
    /// </summary>
    /// <remarks>
    /// Ein Laufwerk gilt als Garmin-Gerät, wenn es den Ordner <c>Garmin/Activities</c> (oder <c>Garmin/Activity</c>)
    /// enthält. Den Gerätenamen liefert <c>Garmin/GarminDevice.xml</c> (Modellbeschreibung, z. B. „Edge 1000“);
    /// fehlt sie, heißt das Gerät „Garmin (E:\)“.
    /// </remarks>
    /// <param name="wurzeln">Wurzelordner der Laufwerke, z. B. „E:\“.</param>
    /// <returns>Die Aufzeichnungen (*.fit) aller gefundenen Geräte, nach Dateiname sortiert.</returns>
    public static List<Geraeteaufzeichnung> Ausfuehren(IEnumerable<string> wurzeln)
    {
        List<Geraeteaufzeichnung> aufzeichnungen = [];
        foreach (string wurzel in wurzeln)
        {
            string? ordner = Aufzeichnungsordner.Select(o => Path.Combine(wurzel, o)).FirstOrDefault(Directory.Exists);
            if (ordner is null)
            {
                continue;
            }

            string geraet = LeseGeraetename(wurzel) ?? $"Garmin ({wurzel})";
            aufzeichnungen.AddRange(Directory.EnumerateFiles(ordner, "*.fit")
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(datei => new Geraeteaufzeichnung(Path.GetFileName(datei), geraet, ziel => File.Copy(datei, ziel, overwrite: true))));
        }

        return aufzeichnungen;
    }

    /// <summary>
    /// Liest die Modellbeschreibung aus <c>Garmin/GarminDevice.xml</c>.
    /// </summary>
    /// <returns>Der Name, z. B. „Edge 1000“, oder <c>null</c>, wenn die Datei fehlt oder unlesbar ist.</returns>
    private static string? LeseGeraetename(string wurzel)
    {
        string pfad = Path.Combine(wurzel, "Garmin", "GarminDevice.xml");
        try
        {
            if (!File.Exists(pfad))
            {
                return null;
            }

            string? name = XDocument.Load(pfad).Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "Model")?
                .Elements().FirstOrDefault(e => e.Name.LocalName == "Description")?.Value.Trim();
            return string.IsNullOrEmpty(name) ? null : name;
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            return null;
        }
    }
}
