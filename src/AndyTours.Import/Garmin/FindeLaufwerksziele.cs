namespace AndyTours.Import.Garmin;

/// <summary>
/// Findet Garmin-Geräte, die als Laufwerk angeschlossen sind (Edge 800, Edge 1000), als Ziel für Courses (Spec 032).
/// </summary>
public static class FindeLaufwerksziele
{
    /// <summary>Ordner, aus dem der Edge neue Dateien übernimmt.</summary>
    public static readonly string NeueDateien = Path.Combine("Garmin", "NewFiles");

    /// <summary>
    /// Durchsucht alle bereiten Wechsellaufwerke.
    /// </summary>
    /// <returns>Die gefundenen Geräte; leer, wenn keines angeschlossen ist.</returns>
    public static List<Courseziel> Ausfuehren() =>
        Ausfuehren(DriveInfo.GetDrives()
            .Where(l => l.IsReady && l.DriveType == DriveType.Removable)
            .Select(l => l.RootDirectory.FullName));

    /// <summary>
    /// Durchsucht die angegebenen Laufwerkswurzeln.
    /// </summary>
    /// <remarks>
    /// Ein Laufwerk gilt wie beim Import als Garmin-Gerät, wenn es den Ordner <c>Garmin/Activities</c> (oder
    /// <c>Garmin/Activity</c>) enthält; den Namen liefert <c>Garmin/GarminDevice.xml</c>, sonst heißt es
    /// „Garmin (E:\)“. Fehlt <c>Garmin/NewFiles</c>, wird der Ordner beim Schreiben angelegt.
    /// </remarks>
    /// <param name="wurzeln">Wurzelordner der Laufwerke, z. B. „E:\“.</param>
    /// <returns>Die gefundenen Geräte in der Reihenfolge der Laufwerke.</returns>
    public static List<Courseziel> Ausfuehren(IEnumerable<string> wurzeln) =>
        [.. wurzeln
            .Where(wurzel => FindeLaufwerksgeraete.Aufzeichnungsordner.Any(o => Directory.Exists(Path.Combine(wurzel, o))))
            .Select(wurzel =>
            {
                string ordner = Path.Combine(wurzel, NeueDateien);
                return new Courseziel(
                    FindeLaufwerksgeraete.LeseGeraetename(wurzel) ?? $"Garmin ({wurzel})",
                    datei => File.Exists(Path.Combine(ordner, datei)),
                    (datei, inhalt) =>
                    {
                        Directory.CreateDirectory(ordner);
                        File.WriteAllBytes(Path.Combine(ordner, datei), inhalt);
                    });
            })];
}
