namespace AndyTours.Import.Altdaten;

/// <summary>
/// Bringt einen Anhangpfad aus den Altdaten in die einheitliche relative Form „JJJJ/datei.fit".
/// </summary>
public static class NormalisiereAnhangPfad
{
    /// <summary>
    /// Normalisiert den Pfad.
    /// </summary>
    /// <remarks>
    /// Das Original speichert Anhänge normalerweise relativ zum Datenordner („2016/…fit").
    /// Vereinzelt steht aber ein absoluter Pfad aus der alten Installation darin
    /// (z. B. „D:\BikeXperience\2026\2026-10-05-06-28-55.fit"). Damit die Datenbank nicht an
    /// einen Laufwerksbuchstaben gebunden ist, wird bei absoluten Pfaden alles ab dem letzten
    /// Jahresordner (vierstellige Zahl) übernommen. Gibt es keinen Jahresordner, bleibt der Pfad
    /// unverändert (nur die Trennzeichen werden vereinheitlicht) und erscheint im Migrationsbericht.
    /// </remarks>
    /// <param name="pfad">Pfad aus der Altdatei.</param>
    /// <returns>Relativer Pfad mit „/" als Trennzeichen, oder leer, wenn kein Pfad angegeben war.</returns>
    public static string Ausfuehren(string pfad)
    {
        // Einheitlich „/" verwenden – das funktioniert unter Windows ebenso wie unter Linux.
        string vereinheitlicht = pfad.Trim().Replace('\\', '/');
        if (vereinheitlicht.Length == 0)
        {
            return string.Empty;
        }

        // Absolut ist ein Pfad mit Laufwerksbuchstaben („D:/…") oder mit führendem „/" (Linux, Netzwerk).
        bool istAbsolut = vereinheitlicht.Contains(':') || vereinheitlicht.StartsWith('/');
        if (!istAbsolut)
        {
            return vereinheitlicht;
        }

        string[] teile = vereinheitlicht.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Von hinten den letzten Ordner suchen, der wie eine Jahreszahl aussieht.
        // Der letzte Teil ist der Dateiname und wird deshalb nicht geprüft.
        for (int i = teile.Length - 2; i >= 0; i--)
        {
            if (teile[i].Length == 4 && teile[i].All(char.IsAsciiDigit))
            {
                return string.Join('/', teile[i..]);
            }
        }

        return vereinheitlicht;
    }
}
