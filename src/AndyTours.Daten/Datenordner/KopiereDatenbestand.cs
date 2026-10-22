namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Kopiert den gesamten Inhalt eines Datenordners in einen anderen Ordner.
/// </summary>
public static class KopiereDatenbestand
{
    /// <summary>
    /// Kopiert alle Dateien samt Unterordnern und meldet nach jeder Datei den Fortschritt.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Der Quellordner wird nur gelesen und bleibt unverändert.</item>
    /// <item>Vorhandene Dateien im Ziel werden nicht überschrieben; eine Namensgleichheit gilt als Fehler.</item>
    /// <item>Die Datenbankdatei wird als <b>letzte</b> kopiert. Bricht das Kopieren vorher ab (Fehler,
    /// Programm beendet), enthält das Ziel keine Datenbank und gilt damit nicht als
    /// „Ordner mit AndyTours-Daten“.</item>
    /// <item>Bei einem Fehler werden alle bereits kopierten Dateien und neu angelegten Unterordner
    /// wieder entfernt; danach wird der Fehler an den Aufrufer weitergegeben.</item>
    /// </list>
    /// Die Methode arbeitet synchron; die Oberfläche ruft sie im Hintergrund auf (<c>Task.Run</c>).
    /// </remarks>
    /// <param name="quellordner">Bisheriger Datenordner.</param>
    /// <param name="zielordner">Neuer Datenordner (muss existieren).</param>
    /// <param name="fortschritt">Empfänger der Fortschrittsmeldungen oder <c>null</c>.</param>
    /// <exception cref="IOException">Eine Datei konnte nicht gelesen oder geschrieben werden.</exception>
    /// <exception cref="UnauthorizedAccessException">Fehlendes Zugriffsrecht.</exception>
    public static void Ausfuehren(string quellordner, string zielordner, IProgress<KopierFortschritt>? fortschritt)
    {
        string datenbank = Path.GetFullPath(Path.Combine(quellordner, DatenordnerInhalt.Datenbankdatei));

        // Alle Dateien erfassen; die Datenbank ans Ende sortieren (false < true)
        List<FileInfo> dateien = new DirectoryInfo(quellordner)
            .EnumerateFiles("*", SearchOption.AllDirectories)
            .OrderBy(d => string.Equals(d.FullName, datenbank, StringComparison.OrdinalIgnoreCase))
            .ToList();
        long gesamtBytes = dateien.Sum(d => d.Length);

        // Merkliste für das Aufräumen im Fehlerfall
        var kopierteDateien = new List<string>();
        var angelegteOrdner = new List<string>();
        long kopierteBytes = 0;

        try
        {
            foreach (FileInfo datei in dateien)
            {
                string zielDatei = Path.Combine(zielordner, Path.GetRelativePath(quellordner, datei.FullName));
                LegeOrdnerAn(Path.GetDirectoryName(zielDatei)!, angelegteOrdner);

                datei.CopyTo(zielDatei, overwrite: false);
                kopierteDateien.Add(zielDatei);

                kopierteBytes += datei.Length;
                fortschritt?.Report(new KopierFortschritt(kopierteDateien.Count, dateien.Count, kopierteBytes, gesamtBytes));
            }
        }
        catch
        {
            RaeumeAuf(kopierteDateien, angelegteOrdner);
            throw;
        }
    }

    /// <summary>Legt einen Zielordner samt fehlender Elternordner an und merkt sich die neu angelegten.</summary>
    private static void LegeOrdnerAn(string ordner, List<string> angelegteOrdner)
    {
        // Von innen nach außen sammeln, welche Ordner noch fehlen …
        var fehlend = new Stack<string>();
        for (string? o = ordner; o is not null && !Directory.Exists(o); o = Path.GetDirectoryName(o))
        {
            fehlend.Push(o);
        }

        // … und von außen nach innen anlegen
        while (fehlend.Count > 0)
        {
            string neu = fehlend.Pop();
            Directory.CreateDirectory(neu);
            angelegteOrdner.Add(neu);
        }
    }

    /// <summary>Entfernt bereits kopierte Dateien und neu angelegte Ordner (innerste zuerst); Fehler dabei werden ignoriert.</summary>
    private static void RaeumeAuf(List<string> kopierteDateien, List<string> angelegteOrdner)
    {
        foreach (string datei in kopierteDateien)
        {
            try
            {
                File.Delete(datei);
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                // Rest bleibt liegen; die ursprüngliche Fehlermeldung ist wichtiger
            }
        }

        for (int i = angelegteOrdner.Count - 1; i >= 0; i--)
        {
            try
            {
                Directory.Delete(angelegteOrdner[i]);
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                // dto.
            }
        }
    }
}
