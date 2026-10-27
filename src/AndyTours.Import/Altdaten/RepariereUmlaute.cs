namespace AndyTours.Import.Altdaten;

/// <summary>
/// Repariert beschädigte Umlaute in einem Text anhand der <see cref="UmlautKorrekturliste"/>.
/// </summary>
public static class RepariereUmlaute
{
    /// <summary>
    /// Das Ersatzzeichen „�" (U+FFFD), das für ein verlorenes Zeichen steht.
    /// </summary>
    public const char Ersatzzeichen = '�';

    /// <summary>
    /// Ersetzt alle bekannten beschädigten Wortteile durch die korrekte Schreibweise.
    /// </summary>
    /// <remarks>
    /// Längere Einträge werden zuerst angewendet, damit z. B. „Br�ndling" vor einem allgemeineren
    /// Eintrag greift. Was danach noch „�" enthält, konnte nicht repariert werden; das prüft der
    /// Aufrufer mit <c>text.Contains(RepariereUmlaute.Ersatzzeichen)</c> und meldet es im Bericht.
    /// </remarks>
    /// <param name="text">Zu reparierender Text.</param>
    /// <param name="korrekturliste">
    /// Optional eine eigene Liste (für Tests); ohne Angabe wird <see cref="UmlautKorrekturliste.Eintraege"/> verwendet.
    /// </param>
    /// <returns>Der reparierte Text (unverändert, wenn nichts zu tun war).</returns>
    public static string Ausfuehren(string text, IReadOnlyList<(string Beschaedigt, string Korrigiert)>? korrekturliste = null)
    {
        korrekturliste ??= UmlautKorrekturliste.Eintraege;

        foreach (var (beschaedigt, korrigiert) in korrekturliste.OrderByDescending(eintrag => eintrag.Beschaedigt.Length))
        {
            // StringComparison.Ordinal: exakter Zeichenvergleich, unabhängig von Spracheinstellungen.
            text = text.Replace(beschaedigt, korrigiert, StringComparison.Ordinal);
        }

        return text;
    }
}
