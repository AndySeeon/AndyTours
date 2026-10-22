namespace AndyTours.Import.Altdaten;

/// <summary>
/// Sammelt alle Auffälligkeiten während der Übernahme der Altdaten.
/// </summary>
/// <remarks>
/// Der Bericht wird nach der Migration angezeigt und als Textdatei abgelegt
/// (siehe <see cref="ErstelleBerichtstext"/>), damit nichts stillschweigend verändert wird.
/// </remarks>
public class Migrationsbericht
{
    /// <summary>
    /// Informationen über automatische Anpassungen, z. B. ergänzte Stammdaten oder umgestellte Pfade.
    /// </summary>
    public List<string> Hinweise { get; } = new();

    /// <summary>
    /// Probleme, die eine Nachkontrolle erfordern, z. B. fehlerhafte Zeilen oder fehlende Dateien.
    /// </summary>
    public List<string> Warnungen { get; } = new();

    /// <summary>
    /// Texte, die nach der Umlaut-Reparatur noch das Ersatzzeichen „�" enthalten, mit Fundstelle.
    /// </summary>
    public List<string> NichtReparierteTexte { get; } = new();

    /// <summary>Anzahl der Texte, in denen beschädigte Umlaute über die Korrekturliste repariert wurden.</summary>
    public int AnzahlReparierteTexte { get; set; }

    /// <summary>Anzahl der Zeichen, die als Latin-1 erkannt und dadurch exakt wiederhergestellt wurden.</summary>
    public int AnzahlLatin1Zeichen { get; set; }

    /// <summary>
    /// Fügt einen Hinweis nur hinzu, wenn er nicht schon vorhanden ist
    /// (z. B. „Equipment X ergänzt" soll nicht für jede Einheit einzeln erscheinen).
    /// </summary>
    /// <param name="hinweis">Der Hinweistext.</param>
    public void HinweisEinmalig(string hinweis)
    {
        if (!Hinweise.Contains(hinweis))
        {
            Hinweise.Add(hinweis);
        }
    }
}
