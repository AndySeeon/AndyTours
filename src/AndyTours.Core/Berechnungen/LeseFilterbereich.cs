using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Liest die beiden Eingabefelder „von“ und „bis“ eines Zahlenfilters (Distanz oder Höhenmeter).
/// </summary>
public static class LeseFilterbereich
{
    /// <summary>
    /// Wandelt die Texte der Felder in einen <see cref="Filterbereich"/> um.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Ein leeres Feld (auch nur Leerzeichen) bedeutet „keine Grenze“.</item>
    /// <item>Komma und Punkt gelten beide als Dezimalzeichen: „12,5“ und „12.5“ ergeben 12,5.
    /// Tausendertrennzeichen werden nicht unterstützt, „1.234,5“ ist daher ungültig.</item>
    /// <item>Ein Text, der keine endliche Zahl ist, markiert nur sein eigenes Feld als fehlerhaft.</item>
    /// <item>Ist „von“ größer als „bis“, sind beide Felder fehlerhaft.</item>
    /// </list>
    /// Ein fehlerhafter Bereich wird beim Filtern nicht angewendet (siehe <see cref="Filterbereich.IstAktiv"/>).
    /// </remarks>
    /// <param name="vonText">Inhalt des Feldes „von“.</param>
    /// <param name="bisText">Inhalt des Feldes „bis“.</param>
    /// <returns>Der gelesene Bereich mit Fehlermerkern.</returns>
    public static Filterbereich Ausfuehren(string? vonText, string? bisText)
    {
        bool vonOk = LeseZahl(vonText, out double? von);
        bool bisOk = LeseZahl(bisText, out double? bis);

        if (vonOk && bisOk && von > bis)
        {
            // Beide Zahlen für sich gültig, aber vertauscht → beide Felder markieren
            return new Filterbereich(von, bis, VonFehlerhaft: true, BisFehlerhaft: true);
        }

        return new Filterbereich(von, bis, VonFehlerhaft: !vonOk, BisFehlerhaft: !bisOk);
    }

    /// <summary>
    /// Liest eine Zahl aus einem Feld.
    /// </summary>
    /// <param name="text">Inhalt des Feldes.</param>
    /// <param name="zahl">Die gelesene Zahl; <c>null</c> bei leerem oder ungültigem Feld.</param>
    /// <returns><c>false</c>, wenn das Feld Text enthält, der keine endliche Zahl ist.</returns>
    private static bool LeseZahl(string? text, out double? zahl)
    {
        zahl = null;
        string bereinigt = (text ?? string.Empty).Trim();
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
}
