namespace AndyTours.Daten.Datenordner;

/// <summary>
/// Legt eine importierte Aufzeichnung im Datenordner ab, im Unterordner ihres Jahres.
/// </summary>
public static class LegeAufzeichnungAb
{
    /// <summary>
    /// Kopiert die Aufzeichnung nach <c>&lt;Datenordner&gt;/&lt;Jahr&gt;/&lt;Dateiname&gt;</c> und liefert
    /// den Pfad relativ zum Datenordner, so wie er im Anhang der Einheit gespeichert wird.
    /// </summary>
    /// <remarks>
    /// Die Ablage entspricht der der übernommenen Altdaten (z. B. <c>2026/2026-09-26-08-02-49.fit</c>);
    /// der relative Pfad verwendet deshalb immer „/“ als Trenner.
    /// <para>
    /// Liegt dort schon eine Datei gleichen Namens, wird sie nie überschrieben: Hat sie denselben Inhalt,
    /// wird sie verwendet; sonst erhält die neue Datei einen Zusatz (<c>…_2.fit</c>, <c>…_3.fit</c> …).
    /// </para>
    /// </remarks>
    /// <param name="datenordner">Der Datenordner von AndyTours.</param>
    /// <param name="quelldatei">Die gelesene Aufzeichnung (z. B. eine Zwischenkopie vom Gerät).</param>
    /// <param name="dateiname">Name, unter dem die Aufzeichnung abgelegt wird (der Name auf dem Gerät).</param>
    /// <param name="jahr">Jahr der Tour; bestimmt den Unterordner.</param>
    /// <returns>Pfad relativ zum Datenordner, z. B. <c>2026/2026-09-26-08-02-49.fit</c>.</returns>
    public static string Ausfuehren(string datenordner, string quelldatei, string dateiname, int jahr)
    {
        string jahrordner = jahr.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string zielordner = Path.Combine(datenordner, jahrordner);
        Directory.CreateDirectory(zielordner);

        string name = Path.GetFileNameWithoutExtension(dateiname);
        string endung = Path.GetExtension(dateiname);
        string ablagename = dateiname;
        for (int nummer = 2; ; nummer++)
        {
            string ziel = Path.Combine(zielordner, ablagename);
            if (!File.Exists(ziel))
            {
                File.Copy(quelldatei, ziel);
                break;
            }

            if (GleicherInhalt(quelldatei, ziel))
            {
                // dieselbe Aufzeichnung liegt schon dort (z. B. nach einem abgebrochenen Import)
                break;
            }

            ablagename = $"{name}_{nummer}{endung}";
        }

        return jahrordner + "/" + ablagename;
    }

    /// <summary>Vergleicht zwei Dateien Byte für Byte.</summary>
    private static bool GleicherInhalt(string datei1, string datei2)
    {
        if (new FileInfo(datei1).Length != new FileInfo(datei2).Length)
        {
            return false;
        }

        return File.ReadAllBytes(datei1).AsSpan().SequenceEqual(File.ReadAllBytes(datei2));
    }
}
