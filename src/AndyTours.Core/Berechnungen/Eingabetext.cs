using System.Globalization;
using System.Text.RegularExpressions;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Liest und schreibt Werte in der Form, in der der Benutzer sie in Eingabefelder tippt
/// (Bearbeitungsfenster, Tabellenzellen, Filterleiste).
/// </summary>
/// <remarks>
/// Lesen und Schreiben stehen zusammen, weil beide dasselbe Format festlegen: Was <c>AlsText</c>
/// schreibt, muss <c>Lese…</c> wieder genauso lesen. Ein leeres Feld (auch nur Leerzeichen) ist
/// immer gültig und ergibt <c>null</c> („nicht angegeben“); ob ein Feld Pflicht ist, entscheidet der
/// Aufrufer.
/// </remarks>
public static partial class Eingabetext
{
    /// <summary>Deutsche Kultur für Datum und Dezimalkomma bei der Ausgabe.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Erlaubte Datumsformen: „24.10.2026“ und „24.10.26“ (Tag und Monat auch einstellig).</summary>
    private static readonly string[] Datumsformate = ["d.M.yyyy", "d.M.yy"];

    /// <summary>
    /// Liest eine Dezimalzahl. Komma und Punkt gelten beide als Dezimalzeichen („12,5“ = „12.5“);
    /// Tausendertrennzeichen werden nicht unterstützt.
    /// </summary>
    /// <param name="text">Inhalt des Feldes.</param>
    /// <param name="zahl">Die Zahl; <c>null</c> bei leerem oder ungültigem Feld.</param>
    /// <returns><c>false</c>, wenn das Feld Text enthält, der keine endliche Zahl ist.</returns>
    public static bool LeseZahl(string? text, out double? zahl)
    {
        zahl = null;
        string bereinigt = Bereinige(text);
        if (bereinigt.Length == 0)
        {
            return true;
        }

        // Komma wie Punkt behandeln, dann unabhängig von der Spracheinstellung lesen
        if (double.TryParse(bereinigt.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double wert)
            && double.IsFinite(wert))
        {
            zahl = wert;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Liest eine ganze Zahl (z. B. Höhenmeter, Puls), auch mit Minuszeichen.
    /// </summary>
    /// <param name="text">Inhalt des Feldes.</param>
    /// <param name="zahl">Die Zahl; <c>null</c> bei leerem oder ungültigem Feld.</param>
    /// <returns><c>false</c>, wenn das Feld Text enthält, der keine ganze Zahl ist.</returns>
    public static bool LeseGanzzahl(string? text, out int? zahl)
    {
        zahl = null;
        string bereinigt = Bereinige(text);
        if (bereinigt.Length == 0)
        {
            return true;
        }

        if (int.TryParse(bereinigt, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int wert))
        {
            zahl = wert;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Liest ein Datum in der Form „24.10.2026“ oder „24.10.26“.
    /// </summary>
    /// <param name="text">Inhalt des Feldes.</param>
    /// <param name="datum">Das Datum; <c>null</c> bei leerem oder ungültigem Feld.</param>
    /// <returns><c>false</c>, wenn das Feld Text enthält, der kein gültiges Datum ist.</returns>
    public static bool LeseDatum(string? text, out DateOnly? datum)
    {
        datum = null;
        string bereinigt = Bereinige(text);
        if (bereinigt.Length == 0)
        {
            return true;
        }

        if (DateOnly.TryParseExact(bereinigt, Datumsformate, Deutsch, DateTimeStyles.None, out DateOnly wert))
        {
            datum = wert;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Liest eine Uhrzeit in der Form „9:30“ oder „09:30“.
    /// </summary>
    /// <param name="text">Inhalt des Feldes.</param>
    /// <param name="uhrzeit">Die Uhrzeit; <c>null</c> bei leerem oder ungültigem Feld.</param>
    /// <returns><c>false</c>, wenn das Feld Text enthält, der keine gültige Uhrzeit ist.</returns>
    public static bool LeseUhrzeit(string? text, out TimeOnly? uhrzeit)
    {
        uhrzeit = null;
        string bereinigt = Bereinige(text);
        if (bereinigt.Length == 0)
        {
            return true;
        }

        if (TimeOnly.TryParseExact(bereinigt, ["H:mm", "HH:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly wert))
        {
            uhrzeit = wert;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Liest eine Dauer in der Form „h:mm“ oder „h:mm:ss“ (z. B. „3:45“ oder „3:45:12“); fehlen die
    /// Sekunden, gelten sie als 00. Die Stunden dürfen über 24 liegen, Minuten und Sekunden müssen
    /// zweistellig und kleiner als 60 sein.
    /// </summary>
    /// <param name="text">Inhalt des Feldes.</param>
    /// <param name="dauer">Die Dauer; <c>null</c> bei leerem oder ungültigem Feld.</param>
    /// <returns><c>false</c>, wenn das Feld Text enthält, der nicht diesem Format entspricht.</returns>
    public static bool LeseDauer(string? text, out TimeSpan? dauer)
    {
        dauer = null;
        string bereinigt = Bereinige(text);
        if (bereinigt.Length == 0)
        {
            return true;
        }

        Match treffer = DauerMuster().Match(bereinigt);
        if (!treffer.Success
            || !int.TryParse(treffer.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int stunden))
        {
            return false;
        }

        int minuten = int.Parse(treffer.Groups[2].Value, CultureInfo.InvariantCulture);
        // Sekunden sind freiwillig; ohne Angabe 00
        int sekunden = treffer.Groups[3].Success ? int.Parse(treffer.Groups[3].Value, CultureInfo.InvariantCulture) : 0;
        dauer = new TimeSpan(stunden, minuten, sekunden);
        return true;
    }

    /// <summary>
    /// Schreibt eine Dezimalzahl mit Dezimalkomma und ohne Tausenderpunkt, ohne Stellen zu verlieren
    /// (z. B. „1234,5“); <c>null</c> ergibt ein leeres Feld.
    /// </summary>
    /// <param name="zahl">Die Zahl.</param>
    /// <returns>Der Eingabetext.</returns>
    public static string AlsText(double? zahl) => zahl?.ToString(Deutsch) ?? string.Empty;

    /// <summary>Schreibt eine ganze Zahl ohne Tausenderpunkt; <c>null</c> ergibt ein leeres Feld.</summary>
    /// <param name="zahl">Die Zahl.</param>
    /// <returns>Der Eingabetext.</returns>
    public static string AlsText(int? zahl) => zahl?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Schreibt ein Datum als „24.10.2026“; <c>null</c> ergibt ein leeres Feld.</summary>
    /// <param name="datum">Das Datum.</param>
    /// <returns>Der Eingabetext.</returns>
    public static string AlsText(DateOnly? datum) => datum?.ToString("dd.MM.yyyy", Deutsch) ?? string.Empty;

    /// <summary>Schreibt eine Uhrzeit als „9:30“; <c>null</c> ergibt ein leeres Feld.</summary>
    /// <param name="uhrzeit">Die Uhrzeit.</param>
    /// <returns>Der Eingabetext.</returns>
    public static string AlsText(TimeOnly? uhrzeit) => uhrzeit?.ToString("H:mm", CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Schreibt eine Dauer als „h:mm:ss“ (auch über 24 Stunden); <c>null</c> ergibt ein leeres Feld.</summary>
    /// <param name="dauer">Die Dauer.</param>
    /// <returns>Der Eingabetext.</returns>
    public static string AlsText(TimeSpan? dauer) => dauer is TimeSpan d ? FormatiereDauer.Ausfuehren(d) : string.Empty;

    /// <summary>Entfernt Leerzeichen am Anfang und Ende; <c>null</c> wird zum leeren Text.</summary>
    private static string Bereinige(string? text) => (text ?? string.Empty).Trim();

    /// <summary>Muster „h:mm:ss“: beliebig viele Stundenziffern, Minuten und Sekunden 00–59.</summary>
    [GeneratedRegex(@"^(\d{1,6}):([0-5]\d)(?::([0-5]\d))?$")]
    private static partial Regex DauerMuster();
}
