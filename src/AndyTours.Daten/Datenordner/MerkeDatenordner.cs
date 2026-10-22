namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Merkt sich den gewählten Datenordner für den nächsten Programmstart.
/// </summary>
public static class MerkeDatenordner
{
    /// <summary>
    /// Schreibt den Ordnerpfad in die Merkdatei; ein vorhandener Eintrag wird ersetzt.
    /// </summary>
    /// <param name="merkdatei">Vollständiger Pfad der Merkdatei (in der App: <c>%APPDATA%\AndyTours\datenordner.txt</c>).</param>
    /// <param name="ordner">Vollständiger Pfad des Datenordners.</param>
    public static void Ausfuehren(string merkdatei, string ordner)
    {
        // Den Ordner der Merkdatei beim ersten Mal anlegen
        Directory.CreateDirectory(Path.GetDirectoryName(merkdatei)!);
        File.WriteAllText(merkdatei, ordner);
    }
}
