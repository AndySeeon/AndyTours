using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Zerlegt eine Zeile aus <c>maintanence.dat</c> in ihre Felder.
/// </summary>
/// <remarks>
/// Aufbau: <c>Datum;Equipment;Tätigkeit;Kommentar;Distanz;aktiv;Typ</c>
/// (Typ 0 = Wartung, 1 = Alarm nach <i>Distanz</i> km).
/// </remarks>
public static class WandleWartungszeile
{
    /// <summary>Anzahl der Felder je Zeile im Original.</summary>
    public const int Feldanzahl = 7;

    /// <summary>
    /// Wandelt die Zeile um.
    /// </summary>
    /// <param name="zeile">Eine Zeile der Datei.</param>
    /// <param name="zeilennummer">Zeilennummer in der Datei, wird im Ergebnis vermerkt.</param>
    /// <returns>Der gelesene Eintrag mit dem Equipment-Namen.</returns>
    /// <exception cref="FormatException">Die Zeile hat nicht 7 Felder oder ein Feld ist ungültig.</exception>
    public static AltWartung Ausfuehren(string zeile, int zeilennummer)
    {
        string[] f = zeile.Split(';');
        if (f.Length != Feldanzahl)
        {
            throw new FormatException($"{f.Length} statt {Feldanzahl} Felder.");
        }

        int typ = ParseGanzzahl.Ausfuehren(f[6]);
        if (!Enum.IsDefined(typeof(WartungsTyp), typ))
        {
            throw new FormatException($"Unbekannter Wartungstyp {typ}.");
        }

        return new AltWartung
        {
            Zeilennummer = zeilennummer,
            EquipmentName = f[1].Trim(),
            Wartung = new Wartung
            {
                Datum = ParseAltesDatum.Ausfuehren(f[0]) ?? throw new FormatException("Datum fehlt."),
                Taetigkeit = f[2].Trim(),
                // Das Original schreibt für „kein Kommentar" ein einzelnes Leerzeichen; Trim macht daraus leer.
                Kommentar = f[3].Trim(),
                AlarmKm = ParseZahl.Ausfuehren(f[4]),
                Aktiv = f[5].Trim().Equals("true", StringComparison.OrdinalIgnoreCase),
                Typ = (WartungsTyp)typ,
            },
        };
    }
}
