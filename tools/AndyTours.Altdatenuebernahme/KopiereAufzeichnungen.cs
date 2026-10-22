namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Kopiert die Aufzeichnungen und Anhänge aus dem BikeXperience-Ordner.
/// </summary>
public static class KopiereAufzeichnungen
{
    /// <summary>
    /// Kopiert alle Unterordner des BikeXperience-Ordners samt Inhalt in den Zielordner.
    /// </summary>
    /// <remarks>
    /// Die Aufzeichnungen (FIT-Dateien) und Anhänge liegen in BikeXperience in Jahresordnern
    /// (<c>2016</c>, <c>2017</c>, …). Die Anhangpfade in der Datenbank sind relativ dazu
    /// gespeichert (z. B. <c>2016/2016-03-05-08-40-41.fit</c>); deshalb wird die Ordnerstruktur
    /// unverändert übernommen. Die Dateien direkt im BikeXperience-Ordner (INI, CSV, …) werden
    /// nicht kopiert – ihr Inhalt steckt nach der Umwandlung in der Datenbank.
    /// </remarks>
    /// <param name="quellordner">BikeXperience-Ordner (wird nur gelesen).</param>
    /// <param name="zielordner">Ordner, in den kopiert wird (muss existieren).</param>
    /// <returns>Anzahl der kopierten Dateien.</returns>
    public static int Ausfuehren(string quellordner, string zielordner)
    {
        int anzahl = 0;
        foreach (string unterordner in Directory.EnumerateDirectories(quellordner))
        {
            foreach (string datei in Directory.EnumerateFiles(unterordner, "*", SearchOption.AllDirectories))
            {
                string zielDatei = Path.Combine(zielordner, Path.GetRelativePath(quellordner, datei));
                Directory.CreateDirectory(Path.GetDirectoryName(zielDatei)!);
                File.Copy(datei, zielDatei);
                anzahl++;
            }
        }

        return anzahl;
    }
}
