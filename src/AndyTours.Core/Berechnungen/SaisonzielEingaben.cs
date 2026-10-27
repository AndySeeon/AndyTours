using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt Saisonziele in die Feldtexte des Fensters „Saisonziele“ um und zurück.
/// </summary>
/// <remarks>
/// Beide Richtungen stehen zusammen, weil sie dieselbe Regel teilen: 0 („kein Ziel“) erscheint als leeres
/// Feld, ein leeres Feld wird wieder als 0 gespeichert.
/// </remarks>
public static class SaisonzielEingaben
{
    /// <summary>Höchstes erlaubtes Ziel für Kilometer.</summary>
    public const int KmMax = 100_000;

    /// <summary>Höchstes erlaubtes Ziel für Höhenmeter.</summary>
    public const int HmMax = 2_000_000;

    /// <summary>Höchstes erlaubtes Ziel für die Anzahl der Einheiten.</summary>
    public const int AnzahlMax = 1_000;

    /// <summary>Deutsche Kultur für den Tausenderpunkt in den Hinweisen.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Liefert den Feldtext eines Zielwerts.
    /// </summary>
    /// <param name="wert">Der Zielwert.</param>
    /// <returns>Die Zahl ohne Tausenderpunkt, bei 0 ein leeres Feld.</returns>
    public static string AlsText(int wert) => wert > 0 ? wert.ToString(CultureInfo.InvariantCulture) : string.Empty;

    /// <summary>
    /// Prüft die drei Feldtexte einer Aktivität und wandelt sie in Zahlen um.
    /// </summary>
    /// <remarks>
    /// Jedes Feld darf leer sein (kein Ziel = 0). Sonst muss es eine ganze Zahl von 0 bis zur jeweiligen
    /// Grenze sein. Geprüft wird in der Reihenfolge Kilometer, Höhenmeter, Anzahl; der Hinweis nennt den
    /// ersten Fehler mit Aktivität und erlaubtem Bereich.
    /// </remarks>
    /// <param name="aktivitaet">Name der Aktivität (für den Hinweis).</param>
    /// <param name="km">Feldtext Kilometer.</param>
    /// <param name="hm">Feldtext Höhenmeter.</param>
    /// <param name="anzahl">Feldtext Anzahl.</param>
    /// <returns>Die drei Werte und <c>null</c>, oder <c>null</c> und der Hinweis.</returns>
    public static ((int Km, int Hm, int Anzahl)? Werte, string? Hinweis) Lesen(string aktivitaet, string km, string hm, string anzahl)
    {
        if (LeseWert(km, KmMax) is not int kilometer)
        {
            return (null, Hinweis("Kilometer", aktivitaet, KmMax));
        }

        if (LeseWert(hm, HmMax) is not int hoehenmeter)
        {
            return (null, Hinweis("Höhenmeter", aktivitaet, HmMax));
        }

        if (LeseWert(anzahl, AnzahlMax) is not int einheiten)
        {
            return (null, Hinweis("Anzahl", aktivitaet, AnzahlMax));
        }

        return ((kilometer, hoehenmeter, einheiten), null);
    }

    /// <summary>Liest einen Feldtext; leer ergibt 0, ungültig oder außerhalb von 0 bis <paramref name="max"/> ergibt <c>null</c>.</summary>
    private static int? LeseWert(string text, int max)
    {
        if (!Eingabetext.LeseGanzzahl(text, out int? zahl))
        {
            return null;
        }

        int wert = zahl ?? 0;
        return wert >= 0 && wert <= max ? wert : null;
    }

    /// <summary>Text des Hinweises, z. B. „Kilometer bei ‚Bike‘: bitte eine ganze Zahl von 0 bis 100.000 eingeben.“</summary>
    private static string Hinweis(string feld, string aktivitaet, int max) =>
        string.Format(Deutsch, "{0} bei ‚{1}‘: bitte eine ganze Zahl von 0 bis {2:N0} eingeben.", feld, aktivitaet, max);
}
