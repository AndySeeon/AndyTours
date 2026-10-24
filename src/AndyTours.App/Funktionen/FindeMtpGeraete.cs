// WPF-Projekte binden System.IO nicht automatisch ein (Namenskonflikt Path ↔ System.Windows.Shapes.Path)
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using AndyTours.Import.Garmin;
using MediaDevices;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Findet Garmin-Geräte, die nur als Mediengerät (MTP) angeschlossen sind (Edge 1040), und ihre Aufzeichnungen.
/// </summary>
/// <remarks>
/// Der Zugriff läuft über das Paket <c>MediaDevices</c> (siehe <c>docs/adr/0002-zugriff-auf-mediengeraete.md</c>).
/// Weil es ein Windows-Zielframework braucht, liegt diese Funktion im App-Projekt; sie liefert dieselbe Form
/// <see cref="Geraeteaufzeichnung"/> wie <see cref="FindeLaufwerksgeraete"/>. Einen automatischen Test gibt es
/// nicht, weil dafür ein echtes Gerät nötig ist.
/// </remarks>
public static partial class FindeMtpGeraete
{
    /// <summary>Ordner der Aufzeichnungen im Speicher des Geräts (Edge 1040: <c>Garmin\Activity</c>).</summary>
    private static readonly string[] Aufzeichnungsordner = [@"Garmin\Activity", @"Garmin\Activities"];

    /// <summary>
    /// Durchsucht alle angeschlossenen Mediengeräte von Garmin.
    /// </summary>
    /// <remarks>
    /// Windows meldet auch Laufwerke (USB-Sticks, Festplatten, Edge 1000) als Mediengerät; ihr Speicher heißt dann
    /// wie der Laufwerksbuchstabe (z. B. <c>\E:</c>). Solche Speicher werden übergangen, weil
    /// <see cref="FindeLaufwerksgeraete"/> sie schon liefert – sonst erschienen die Touren doppelt. Ein Gerät, das
    /// sich nicht lesen lässt (z. B. gerade getrennt), wird ebenfalls übergangen.
    /// </remarks>
    /// <returns>Die Aufzeichnungen (*.fit) aller gefundenen Geräte, je Gerät nach Dateiname sortiert.</returns>
    public static List<Geraeteaufzeichnung> Ausfuehren()
    {
        List<Geraeteaufzeichnung> aufzeichnungen = [];
        foreach (MediaDevice geraet in MediaDeviceManager.Instance?.GetDevices() ?? [])
        {
            if (!IstGarmin(geraet))
            {
                continue;
            }

            try
            {
                aufzeichnungen.AddRange(LeseAufzeichnungen(geraet));
            }
            catch (Exception fehler) when (IstGeraetefehler(fehler))
            {
                // Gerät nicht lesbar: übergehen, die übrigen Geräte werden trotzdem angezeigt
            }
            finally
            {
                if (geraet.IsConnected)
                {
                    geraet.Disconnect();
                }
            }
        }

        return aufzeichnungen;
    }

    /// <summary>Prüft am Hersteller (bzw. an der Beschreibung), ob es ein Garmin-Gerät ist.</summary>
    private static bool IstGarmin(MediaDevice geraet) =>
        (geraet.Manufacturer ?? "").Contains("Garmin", StringComparison.OrdinalIgnoreCase)
        || (geraet.Description ?? "").Contains("Garmin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Verbindet sich lesend mit dem Gerät und listet die Aufzeichnungen aller seiner Speicher.
    /// </summary>
    private static List<Geraeteaufzeichnung> LeseAufzeichnungen(MediaDevice geraet)
    {
        geraet.Connect(MediaDeviceAccess.GenericRead);
        string name = Geraetename(geraet);
        List<Geraeteaufzeichnung> aufzeichnungen = [];
        foreach (string speicher in geraet.GetDirectories(@"\"))
        {
            if (LaufwerksSpeicher().IsMatch(speicher))
            {
                continue;
            }

            string? ordner = Aufzeichnungsordner.Select(o => speicher + @"\" + o).FirstOrDefault(geraet.DirectoryExists);
            if (ordner is null)
            {
                continue;
            }

            aufzeichnungen.AddRange(geraet.GetFiles(ordner)
                .Where(datei => datei.EndsWith(".fit", StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(datei => new Geraeteaufzeichnung(Path.GetFileName(datei), name, ziel => Kopiere(geraet, datei, ziel))));
        }

        return aufzeichnungen;
    }

    /// <summary>
    /// Kopiert eine Aufzeichnung vom Gerät in eine Datei; eine vorhandene Zieldatei wird überschrieben.
    /// </summary>
    /// <remarks>
    /// Die Verbindung wird nur für das Kopieren hergestellt und danach wieder getrennt, damit das Gerät nicht
    /// länger als nötig belegt ist.
    /// </remarks>
    private static void Kopiere(MediaDevice geraet, string quelle, string ziel)
    {
        try
        {
            geraet.Connect(MediaDeviceAccess.GenericRead);
            using FileStream datei = File.Create(ziel);
            geraet.DownloadFile(quelle, datei);
        }
        catch (Exception fehler) when (fehler is COMException or InvalidOperationException or NotSupportedException)
        {
            // als Dateifehler weitergeben, damit der Import die Aufzeichnung wie eine unlesbare Datei überspringt
            throw new IOException($"Die Aufzeichnung konnte nicht vom Gerät kopiert werden: {fehler.Message}", fehler);
        }
        finally
        {
            if (geraet.IsConnected)
            {
                geraet.Disconnect();
            }
        }
    }

    /// <summary>
    /// Name des Geräts, z. B. „Edge 1040“: der Anzeigename, sonst das Modell, sonst „Garmin-Mediengerät“.
    /// </summary>
    private static string Geraetename(MediaDevice geraet) =>
        new[] { geraet.FriendlyName, geraet.Model, geraet.Description }
            .Select(n => n?.Trim())
            .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "Garmin-Mediengerät";

    /// <summary>Fehler, die beim Zugriff auf ein Mediengerät auftreten können.</summary>
    private static bool IstGeraetefehler(Exception fehler) =>
        fehler is COMException or IOException or UnauthorizedAccessException or InvalidOperationException or NotSupportedException;

    /// <summary>Speicher, der ein Laufwerk ist, z. B. <c>\E:</c>.</summary>
    [GeneratedRegex(@"^\\[A-Za-z]:\\?$")]
    private static partial Regex LaufwerksSpeicher();
}
