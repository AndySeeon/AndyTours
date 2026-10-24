using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erzeugt die Texte der Karteikarte „Aktuell“: Überschrift des Diagramms und Kennzahlen des laufenden Jahres.
/// </summary>
/// <remarks>
/// Die Texte stehen zusammen, weil sie dieselben Zahlenformate teilen: Kilometer und Wochenleistung
/// mit einer Nachkommastelle, Ziele, Höhenmeter, Anzahl und Prozent ganzzahlig, Fahrzeit als „h:mm h“.
/// </remarks>
public static class BeschreibeJahresfortschritt
{
    /// <summary>Hinweis, wenn für Kilometer, Anzahl oder Höhenmeter kein Ziel festgelegt ist.</summary>
    public const string KeinZiel = "kein Saisonziel – unter Extras → Saisonziele… festlegen";

    /// <summary>Deutsche Kultur für Tausenderpunkt und Dezimalkomma.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Überschrift des Diagramms.
    /// </summary>
    /// <param name="wert">Der gezeigte Wert.</param>
    /// <returns>z. B. „Kilometer (Summe Entfernung)“.</returns>
    public static string Ueberschrift(Fortschrittswert wert) => wert switch
    {
        Fortschrittswert.Kilometer => "Kilometer (Summe Entfernung)",
        Fortschrittswert.Anzahl => "Anzahl Einheiten",
        Fortschrittswert.Hoehenmeter => "Höhenmeter (Summe Höhenmeter)",
        _ => "Fahrzeit",
    };

    /// <summary>
    /// Die zwei Zeilen über dem Diagramm.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Mit Ziel: „2026: 4.321,5 km von 6.000 km (72 %) · Soll heute 4.900,3 km · 578,8 km Rückstand“ und
    /// darunter „noch 1.678,5 km – 167,9 km pro Woche“ bzw. „Ziel erreicht – 120,0 km darüber“.</item>
    /// <item>Ohne Ziel: „2026: 4.321,5 km“ und darunter <see cref="KeinZiel"/>; bei der Fahrzeit bleibt die
    /// zweite Zeile leer, weil es dafür kein Saisonziel gibt.</item>
    /// </list>
    /// </remarks>
    /// <param name="fortschritt">Der Jahresfortschritt.</param>
    /// <returns>Erste und zweite Zeile.</returns>
    public static (string Stand, string Ausblick) Kennzahlen(Jahresfortschritt fortschritt)
    {
        Fortschrittswert wert = fortschritt.Wert;
        Jahreskennzahlen k = fortschritt.Kennzahlen;
        string stand = $"{k.Jahr}: {Text(k.Stand, wert)}";

        if (k.Ziel is not double ziel)
        {
            return (stand, wert == Fortschrittswert.Fahrzeit ? string.Empty : KeinZiel);
        }

        // Erste Zeile: Stand gegenüber Ziel und Soll-Stand
        double differenz = k.Differenz ?? 0;
        stand += string.Format(Deutsch, " von {0} ({1:N0} %) · Soll heute {2} · {3} {4}",
            Text(ziel, wert, ganz: true), (k.Anteil ?? 0) * 100, Text(k.Soll ?? 0, wert),
            Text(Math.Abs(differenz), wert), differenz >= 0 ? "Vorsprung" : "Rückstand");

        // Zweite Zeile: Wochenleistung bzw. Überschuss
        string ausblick = k.ProWoche is double proWoche
            ? $"noch {Text(k.Rest ?? 0, wert)} – {Text(proWoche, wert, proWoche: true)} pro Woche"
            : $"Ziel erreicht – {Text(k.Stand - ziel, wert)} darüber";
        return (stand, ausblick);
    }

    /// <summary>
    /// Formatiert eine Zahl mit Einheit.
    /// </summary>
    /// <param name="zahl">Die Zahl in der Einheit des Werts (Fahrzeit in Stunden).</param>
    /// <param name="wert">Der Wert (bestimmt Einheit und Nachkommastellen).</param>
    /// <param name="ganz"><c>true</c> für Ziele: ohne Nachkommastelle.</param>
    /// <param name="proWoche"><c>true</c> für die Wochenleistung: auch Anzahl und Höhenmeter mit einer Nachkommastelle.</param>
    private static string Text(double zahl, Fortschrittswert wert, bool ganz = false, bool proWoche = false)
    {
        string format = ganz ? "N0" : proWoche || wert == Fortschrittswert.Kilometer ? "N1" : "N0";
        return wert switch
        {
            Fortschrittswert.Kilometer => zahl.ToString(format, Deutsch) + " km",
            Fortschrittswert.Anzahl => zahl.ToString(format, Deutsch) + " Einheiten",
            Fortschrittswert.Hoehenmeter => zahl.ToString(format, Deutsch) + " hm",
            _ => Stunden(zahl),
        };
    }

    /// <summary>Fahrzeit in Stunden als „h:mm h“, z. B. „123:45 h“.</summary>
    private static string Stunden(double stunden)
    {
        long minuten = (long)Math.Round(stunden * 60);
        return string.Format(Deutsch, "{0:N0}:{1:00} h", minuten / 60, minuten % 60);
    }
}
