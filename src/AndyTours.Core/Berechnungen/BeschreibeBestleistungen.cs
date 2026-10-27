using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erzeugt die angezeigten Zeilen des Bereichs „Bestleistungen“ auf der Karteikarte „Aktuell“.
/// </summary>
public static class BeschreibeBestleistungen
{
    /// <summary>Deutsche Kultur für Tausenderpunkt, Dezimalkomma und Datum.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Macht aus den Bestleistungen die Zeilen der beiden Spalten.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><b>Jahresrekorde</b> wie im Jahresfortschritt: km mit einer Nachkommastelle, Anzahl und hm ganzzahlig,
    /// Fahrzeit als „h:mm h“; Herkunft ist das Jahr, z. B. „Kilometer | 8.123,4 km | 2019“.</item>
    /// <item><b>Einzelrekorde</b> wie in der Tabelle: km mit zwei Nachkommastellen, Fahrzeit „h:mm:ss“, km/h mit
    /// einer Nachkommastelle, hm und Höhe ganzzahlig; Herkunft sind Datum und Name, z. B.
    /// „Längste Tour | 212,35 km | 14.06.2021 Alpenrunde“.</item>
    /// <item>Stammt ein Rekord aus dem laufenden Jahr, ist die Zeile als solche gekennzeichnet.</item>
    /// <item>Zeilen der Einzelrekorde tragen die Nummer ihrer Einheit, damit ein Doppelklick sie öffnen kann.</item>
    /// </list>
    /// </remarks>
    /// <param name="bestleistungen">Die Bestleistungen.</param>
    /// <param name="heute">Das heutige Datum (bestimmt das laufende Jahr).</param>
    /// <returns>Zeilen der Jahresrekorde und Zeilen der Einzelrekorde.</returns>
    public static (List<Bestleistungszeile> Jahresrekorde, List<Bestleistungszeile> Einzelrekorde) Ausfuehren(Bestleistungen bestleistungen, DateOnly heute)
    {
        List<Bestleistungszeile> jahresrekorde = bestleistungen.Jahresrekorde
            .Select(r => new Bestleistungszeile(
                Bezeichnung(r.Wert),
                Jahreswert(r.Summe, r.Wert),
                r.Jahr.ToString(CultureInfo.InvariantCulture),
                r.Jahr == heute.Year))
            .ToList();

        List<Bestleistungszeile> einzelrekorde = bestleistungen.Einzelrekorde
            .Select(r => new Bestleistungszeile(
                Bezeichnung(r.Art),
                Einzelwert(r),
                $"{r.Einheit.Datum.ToString("dd.MM.yyyy", Deutsch)} {r.Einheit.Name}".TrimEnd(),
                r.Einheit.Datum.Year == heute.Year,
                r.Einheit.Id))
            .ToList();

        return (jahresrekorde, einzelrekorde);
    }

    /// <summary>Bezeichnung eines Jahresrekords.</summary>
    private static string Bezeichnung(Fortschrittswert wert) => wert switch
    {
        Fortschrittswert.Kilometer => "Kilometer",
        Fortschrittswert.Anzahl => "Anzahl Einheiten",
        Fortschrittswert.Hoehenmeter => "Höhenmeter",
        _ => "Fahrzeit",
    };

    /// <summary>Bezeichnung eines Einzelrekords.</summary>
    private static string Bezeichnung(Einzelrekordart art) => art switch
    {
        Einzelrekordart.Distanz => "Längste Tour",
        Einzelrekordart.Hoehenmeter => "Meiste Höhenmeter",
        Einzelrekordart.Fahrzeit => "Längste Fahrzeit",
        Einzelrekordart.Durchschnittsgeschwindigkeit => "Höchste Ø-Geschwindigkeit",
        Einzelrekordart.Hoechstgeschwindigkeit => "Höchste Geschwindigkeit",
        _ => "Höchster Punkt",
    };

    /// <summary>Jahressumme mit Einheit.</summary>
    private static string Jahreswert(double summe, Fortschrittswert wert) => wert switch
    {
        Fortschrittswert.Kilometer => summe.ToString("N1", Deutsch) + " km",
        Fortschrittswert.Anzahl => summe.ToString("N0", Deutsch) + " Einheiten",
        Fortschrittswert.Hoehenmeter => summe.ToString("N0", Deutsch) + " hm",
        _ => FormatiereStunden.Ausfuehren(summe),
    };

    /// <summary>Wert eines Einzelrekords mit Einheit; die Fahrzeit direkt aus der Einheit.</summary>
    private static string Einzelwert(Einzelrekord rekord) => rekord.Art switch
    {
        Einzelrekordart.Distanz => rekord.Wert.ToString("N2", Deutsch) + " km",
        Einzelrekordart.Hoehenmeter => rekord.Wert.ToString("N0", Deutsch) + " hm",
        Einzelrekordart.Fahrzeit => FormatiereDauer.Ausfuehren(rekord.Einheit.Fahrzeit) + " h",
        Einzelrekordart.Durchschnittsgeschwindigkeit or Einzelrekordart.Hoechstgeschwindigkeit => rekord.Wert.ToString("N1", Deutsch) + " km/h",
        _ => rekord.Wert.ToString("N0", Deutsch) + " m",
    };
}
