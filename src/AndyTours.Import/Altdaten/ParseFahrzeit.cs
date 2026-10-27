namespace AndyTours.Import.Altdaten;

/// <summary>
/// Wandelt eine Zeitdauer „hh:mm:ss" aus den Altdaten in eine <see cref="TimeSpan"/> um.
/// </summary>
public static class ParseFahrzeit
{
    /// <summary>
    /// Liest die Zeitdauer.
    /// </summary>
    /// <remarks>
    /// Bewusst ohne <c>TimeSpan.Parse</c>: Dieses würde „25:00:00" als 25 Tage deuten.
    /// Hier werden die Teile einzeln gelesen, sodass auch Mehrtagestouren mit mehr als
    /// 24 Stunden Fahrzeit korrekt übernommen werden. Fehlen die Sekunden („hh:mm"), gelten sie als 0.
    /// </remarks>
    /// <param name="text">Zeitdauer, z. B. „04:33:23".</param>
    /// <returns>Die Dauer; ein leerer Text ergibt <see cref="TimeSpan.Zero"/>.</returns>
    /// <exception cref="FormatException">Der Text hat nicht das Format hh:mm[:ss].</exception>
    public static TimeSpan Ausfuehren(string text)
    {
        string bereinigt = text.Trim();
        if (bereinigt.Length == 0)
        {
            return TimeSpan.Zero;
        }

        string[] teile = bereinigt.Split(':');
        int stunden = 0;
        int minuten = 0;
        int sekunden = 0;

        // Zwei oder drei Teile, jeweils eine ganze Zahl; der Sekundenteil ist optional.
        bool gueltig = teile.Length is 2 or 3
            && int.TryParse(teile[0], out stunden)
            && int.TryParse(teile[1], out minuten)
            && (teile.Length == 2 || int.TryParse(teile[2], out sekunden));

        if (!gueltig)
        {
            throw new FormatException($"„{text}“ ist keine gültige Zeitdauer (erwartet hh:mm:ss).");
        }

        return new TimeSpan(stunden, minuten, sekunden);
    }
}
