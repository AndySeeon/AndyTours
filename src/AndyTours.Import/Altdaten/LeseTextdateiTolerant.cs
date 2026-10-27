namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liest eine Textdatei des Originals zeilenweise und verkraftet dabei gemischte Zeichenkodierungen.
/// </summary>
public static class LeseTextdateiTolerant
{
    /// <summary>
    /// Liest alle Zeilen der Datei.
    /// </summary>
    /// <param name="pfad">Vollständiger Dateipfad.</param>
    /// <param name="anzahlLatin1Zeichen">Anzahl der als Latin-1 wiederhergestellten Zeichen (siehe <see cref="DekodiereGemischteKodierung"/>).</param>
    /// <returns>Die Zeilen ohne Zeilenendezeichen; eine leere letzte Zeile wird weggelassen.</returns>
    public static List<string> Ausfuehren(string pfad, out int anzahlLatin1Zeichen)
    {
        // Datei als Bytes lesen, damit die Kodierung selbst bestimmt werden kann
        // (File.ReadAllLines würde ungültiges UTF-8 sofort durch „�" ersetzen).
        byte[] bytes = File.ReadAllBytes(pfad);
        string text = DekodiereGemischteKodierung.Ausfuehren(bytes, out anzahlLatin1Zeichen);

        // Die Altdateien verwenden Windows-Zeilenenden (CR LF); zur Sicherheit wird auch reines LF akzeptiert.
        var zeilen = text.Replace("\r\n", "\n").Split('\n').ToList();

        // Endet die Datei mit einem Zeilenumbruch, entsteht beim Aufteilen eine leere letzte Zeile.
        if (zeilen.Count > 0 && zeilen[^1].Length == 0)
        {
            zeilen.RemoveAt(zeilen.Count - 1);
        }

        return zeilen;
    }
}
