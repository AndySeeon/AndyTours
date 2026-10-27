namespace AndyTours.Import.Altdaten;

/// <summary>
/// Entfernt die einfachen Anführungszeichen, mit denen <c>workouts.csv</c> Textfelder umschließt.
/// </summary>
public static class EntferneHochkommas
{
    /// <summary>
    /// Entfernt je ein Hochkomma am Anfang und am Ende, falls beide vorhanden sind.
    /// </summary>
    /// <remarks>
    /// Hochkommas im Inneren (z. B. „Bernie's Runde") bleiben erhalten. Felder ohne
    /// umschließende Hochkommas werden nur von Leerzeichen am Rand befreit.
    /// </remarks>
    /// <param name="text">Feldinhalt, z. B. <c>'mit Sabine'</c>.</param>
    /// <returns>Der Text ohne umschließende Hochkommas, z. B. <c>mit Sabine</c>.</returns>
    public static string Ausfuehren(string text)
    {
        string bereinigt = text.Trim();
        if (bereinigt.Length >= 2 && bereinigt[0] == '\'' && bereinigt[^1] == '\'')
        {
            return bereinigt[1..^1];
        }

        return bereinigt;
    }
}
