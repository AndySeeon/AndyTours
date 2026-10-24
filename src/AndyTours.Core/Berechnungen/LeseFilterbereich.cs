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
    /// Tausendertrennzeichen werden nicht unterstützt, „1.234,5“ ist daher ungültig
    /// (gelesen über <see cref="Eingabetext.LeseZahl"/>).</item>
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
        bool vonOk = Eingabetext.LeseZahl(vonText, out double? von);
        bool bisOk = Eingabetext.LeseZahl(bisText, out double? bis);

        if (vonOk && bisOk && von > bis)
        {
            // Beide Zahlen für sich gültig, aber vertauscht → beide Felder markieren
            return new Filterbereich(von, bis, VonFehlerhaft: true, BisFehlerhaft: true);
        }

        return new Filterbereich(von, bis, VonFehlerhaft: !vonOk, BisFehlerhaft: !bisOk);
    }
}
