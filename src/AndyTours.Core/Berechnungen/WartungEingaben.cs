using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt zwischen einer Wartung und den Eingaben des Wartungsfensters um und prüft die Eingaben.
/// </summary>
public static class WartungEingaben
{
    /// <summary>Hinweis, wenn das Datum in der Zukunft liegt.</summary>
    public const string DatumInDerZukunft = "Das Datum liegt in der Zukunft.";

    /// <summary>
    /// Liefert die Eingaben für das Fenster aus einer vorhandenen oder neuen Wartung.
    /// </summary>
    /// <param name="wartung">Die Wartung.</param>
    /// <returns>Die Eingaben.</returns>
    public static Wartungseingaben AlsEingaben(Wartung wartung) => new()
    {
        Datum = Eingabetext.AlsText(wartung.Datum),
        EquipmentId = wartung.EquipmentId,
        Taetigkeit = wartung.Taetigkeit.Trim(),
        Kommentar = wartung.Kommentar,
    };

    /// <summary>
    /// Prüft die Eingaben und liefert die Wartung daraus.
    /// </summary>
    /// <remarks>
    /// Pflicht sind ein gültiges Datum, ein Rad und eine Tätigkeit. Das Datum darf nicht nach <paramref name="heute"/>
    /// liegen – eine Wartung ist etwas, das schon gemacht wurde. Nummer, Typ, Alarmkilometer und „aktiv“ kommen
    /// unverändert aus <paramref name="basis"/>.
    /// </remarks>
    /// <param name="eingaben">Die Eingaben des Fensters.</param>
    /// <param name="basis">Die bearbeitete Wartung bzw. eine neue.</param>
    /// <param name="heute">Der heutige Tag (übergeben, damit die Prüfung testbar ist).</param>
    /// <returns>Die Wartung oder die fehlerhaften Felder samt Hinweis.</returns>
    public static Wartungsergebnis AlsWartung(Wartungseingaben eingaben, Wartung basis, DateOnly heute)
    {
        HashSet<Wartungsfeld> fehler = [];
        string? hinweis = null;

        if (!Eingabetext.LeseDatum(eingaben.Datum, out DateOnly? datum) || datum is null)
        {
            fehler.Add(Wartungsfeld.Datum);
        }
        else if (datum > heute)
        {
            fehler.Add(Wartungsfeld.Datum);
            hinweis = DatumInDerZukunft;
        }

        if (eingaben.EquipmentId is null)
        {
            fehler.Add(Wartungsfeld.Rad);
        }

        if (string.IsNullOrWhiteSpace(eingaben.Taetigkeit))
        {
            fehler.Add(Wartungsfeld.Taetigkeit);
        }

        if (fehler.Count > 0)
        {
            return new Wartungsergebnis(null, fehler, hinweis);
        }

        var wartung = new Wartung
        {
            Id = basis.Id,
            Datum = datum!.Value,
            EquipmentId = eingaben.EquipmentId,
            Taetigkeit = eingaben.Taetigkeit.Trim(),
            Kommentar = eingaben.Kommentar.Trim(),
            Typ = basis.Typ,
            AlarmKm = basis.AlarmKm,
            Aktiv = basis.Aktiv,
        };
        return new Wartungsergebnis(wartung, fehler, null);
    }
}
