using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt eine Trainingseinheit in die Eingaben des Bearbeitungsfensters um und zurück.
/// </summary>
/// <remarks>
/// Beide Richtungen stehen zusammen, weil sie dieselben Regeln teilen: Ein Wert 0 bedeutet „nicht
/// erfasst“ und erscheint als leeres Feld, ein leeres Feld wird wieder als 0 gespeichert. So bleiben
/// die übernommenen Altdaten unverändert, wenn der Benutzer ein Feld nicht anfasst.
/// </remarks>
public static class EinheitEingaben
{
    /// <summary>
    /// Liefert die Eingaben, mit denen das Bearbeitungsfenster für diese Einheit gefüllt wird.
    /// </summary>
    /// <remarks>
    /// Zahlen werden ohne Tausenderpunkt und ohne Stellenverlust geschrieben (<see cref="Eingabetext"/>).
    /// Werte 0 werden zum leeren Feld – auch Höhenmeter und Temperatur, denn beim Speichern wird ein
    /// leeres Feld wieder zu 0.
    /// </remarks>
    /// <param name="einheit">Die Einheit.</param>
    /// <returns>Die Eingaben.</returns>
    public static Einheiteneingaben AlsEingaben(Trainingseinheit einheit) => new()
    {
        Datum = Eingabetext.AlsText(einheit.Datum),
        Startzeit = Eingabetext.AlsText(einheit.Startzeit),
        Name = einheit.Name,
        AktivitaetId = einheit.AktivitaetId > 0 ? einheit.AktivitaetId : null,
        KategorieIds = [.. einheit.KategorieIds],
        EquipmentId = einheit.EquipmentId,
        Distanz = Eingabetext.AlsText(OhneNull(einheit.DistanzKm)),
        Hoehenmeter = Eingabetext.AlsText(OhneNull(einheit.Hoehenmeter)),
        Fahrzeit = Eingabetext.AlsText(einheit.Fahrzeit > TimeSpan.Zero ? einheit.Fahrzeit : (TimeSpan?)null),
        MaxKmh = Eingabetext.AlsText(OhneNull(einheit.MaxGeschwindigkeit)),
        PulsDurchschnitt = Eingabetext.AlsText(OhneNull(einheit.HfDurchschnitt)),
        PulsMax = Eingabetext.AlsText(OhneNull(einheit.HfMax)),
        PulsMin = Eingabetext.AlsText(OhneNull(einheit.HfMin)),
        Trittfrequenz = Eingabetext.AlsText(OhneNull(einheit.Trittfrequenz)),
        Temperatur = Eingabetext.AlsText(OhneNull(einheit.Temperatur)),
        Leistung = Eingabetext.AlsText(OhneNull(einheit.LeistungWatt)),
        Koerpergewicht = Eingabetext.AlsText(OhneNull(einheit.KoerpergewichtKg)),
        Erholungspuls = Eingabetext.AlsText(OhneNull(einheit.Erholungspuls)),
        StartHoehe = Eingabetext.AlsText(OhneNull(einheit.StartHoehe)),
        MaxHoehe = Eingabetext.AlsText(OhneNull(einheit.MaxHoehe)),
        Trainingsbereich = einheit.Trainingsbereich,
        Kommentar = einheit.Kommentar,
    };

    /// <summary>
    /// Prüft die Eingaben und erzeugt daraus eine Trainingseinheit.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Pflicht sind nur Datum und Aktivität; alle anderen Felder dürfen leer sein (→ 0 bzw. leer).</item>
    /// <item>Zahlen müssen lesbar sein (<see cref="Eingabetext"/>) und dürfen nicht negativ sein – außer
    /// der Temperatur, bei der Minusgrade echte Werte sind.</item>
    /// <item>Die Fahrzeit muss die Form „h:mm“ oder „h:mm:ss“ haben (Sekunden freiwillig, sonst 00).</item>
    /// <item>Nummer, Fahrer und Anhänge werden aus <paramref name="basis"/> übernommen; so bleiben die
    /// Anhänge beim Bearbeiten erhalten.</item>
    /// </list>
    /// Alle fehlerhaften Felder werden gesammelt, damit die Oberfläche sie auf einmal markieren kann.
    /// </remarks>
    /// <param name="eingaben">Die Eingaben des Fensters bzw. der Zelle.</param>
    /// <param name="basis">Die bearbeitete Einheit (bei „Neu“ und „Kopieren“ die vorbereitete neue Einheit).</param>
    /// <returns>Die Einheit oder die Menge der fehlerhaften Felder.</returns>
    public static Eingabeergebnis AlsEinheit(Einheiteneingaben eingaben, Trainingseinheit basis)
    {
        var fehler = new HashSet<Eingabefeld>();

        // Pflichtfelder
        if (!Eingabetext.LeseDatum(eingaben.Datum, out DateOnly? datum) || datum is null)
        {
            fehler.Add(Eingabefeld.Datum);
        }

        if (eingaben.AktivitaetId is null)
        {
            fehler.Add(Eingabefeld.Aktivitaet);
        }

        // Übrige Felder: leer erlaubt, sonst lesbar und (außer Temperatur) nicht negativ
        if (!Eingabetext.LeseUhrzeit(eingaben.Startzeit, out TimeOnly? startzeit))
        {
            fehler.Add(Eingabefeld.Startzeit);
        }

        if (!Eingabetext.LeseDauer(eingaben.Fahrzeit, out TimeSpan? fahrzeit))
        {
            fehler.Add(Eingabefeld.Fahrzeit);
        }

        double distanz = Zahl(eingaben.Distanz, Eingabefeld.Distanz, fehler);
        double maxKmh = Zahl(eingaben.MaxKmh, Eingabefeld.MaxKmh, fehler);
        double leistung = Zahl(eingaben.Leistung, Eingabefeld.Leistung, fehler);
        double gewicht = Zahl(eingaben.Koerpergewicht, Eingabefeld.Koerpergewicht, fehler);
        int hoehenmeter = Ganzzahl(eingaben.Hoehenmeter, Eingabefeld.Hoehenmeter, fehler);
        int pulsDurchschnitt = Ganzzahl(eingaben.PulsDurchschnitt, Eingabefeld.PulsDurchschnitt, fehler);
        int pulsMax = Ganzzahl(eingaben.PulsMax, Eingabefeld.PulsMax, fehler);
        int pulsMin = Ganzzahl(eingaben.PulsMin, Eingabefeld.PulsMin, fehler);
        int trittfrequenz = Ganzzahl(eingaben.Trittfrequenz, Eingabefeld.Trittfrequenz, fehler);
        int erholungspuls = Ganzzahl(eingaben.Erholungspuls, Eingabefeld.Erholungspuls, fehler);
        int startHoehe = Ganzzahl(eingaben.StartHoehe, Eingabefeld.StartHoehe, fehler);
        int maxHoehe = Ganzzahl(eingaben.MaxHoehe, Eingabefeld.MaxHoehe, fehler);
        int temperatur = Ganzzahl(eingaben.Temperatur, Eingabefeld.Temperatur, fehler, negativErlaubt: true);

        if (fehler.Count > 0)
        {
            return new Eingabeergebnis(null, fehler);
        }

        var einheit = new Trainingseinheit
        {
            // Aus der Ausgangseinheit: Nummer, Fahrer, Anhänge
            Id = basis.Id,
            FahrerId = basis.FahrerId,
            Anhaenge = basis.Anhaenge.Select(a => new Anhang { Id = a.Id, Typ = a.Typ, RelativerPfad = a.RelativerPfad }).ToList(),

            // Aus den Eingaben
            Datum = datum!.Value,
            Startzeit = startzeit,
            Name = eingaben.Name.Trim(),
            AktivitaetId = eingaben.AktivitaetId!.Value,
            KategorieIds = eingaben.KategorieIds.Distinct().ToList(),
            EquipmentId = eingaben.EquipmentId,
            DistanzKm = distanz,
            Hoehenmeter = hoehenmeter,
            Fahrzeit = fahrzeit ?? TimeSpan.Zero,
            MaxGeschwindigkeit = maxKmh,
            HfDurchschnitt = pulsDurchschnitt,
            HfMax = pulsMax,
            HfMin = pulsMin,
            Trittfrequenz = trittfrequenz,
            Temperatur = temperatur,
            LeistungWatt = leistung,
            KoerpergewichtKg = gewicht,
            Erholungspuls = erholungspuls,
            StartHoehe = startHoehe,
            MaxHoehe = maxHoehe,
            Trainingsbereich = eingaben.Trainingsbereich.Trim(),
            Kommentar = eingaben.Kommentar,
        };
        return new Eingabeergebnis(einheit, fehler);
    }

    /// <summary>Liest eine Dezimalzahl ≥ 0; leer → 0. Bei Fehler wird das Feld vermerkt.</summary>
    private static double Zahl(string text, Eingabefeld feld, HashSet<Eingabefeld> fehler)
    {
        if (!Eingabetext.LeseZahl(text, out double? zahl) || zahl < 0)
        {
            fehler.Add(feld);
            return 0;
        }

        return zahl ?? 0;
    }

    /// <summary>Liest eine ganze Zahl (≥ 0, außer <paramref name="negativErlaubt"/>); leer → 0. Bei Fehler wird das Feld vermerkt.</summary>
    private static int Ganzzahl(string text, Eingabefeld feld, HashSet<Eingabefeld> fehler, bool negativErlaubt = false)
    {
        if (!Eingabetext.LeseGanzzahl(text, out int? zahl) || (!negativErlaubt && zahl < 0))
        {
            fehler.Add(feld);
            return 0;
        }

        return zahl ?? 0;
    }

    /// <summary>0 bedeutet „nicht erfasst“ → <c>null</c> (leeres Feld).</summary>
    private static double? OhneNull(double wert) => wert != 0 ? wert : null;

    /// <summary>0 bedeutet „nicht erfasst“ → <c>null</c> (leeres Feld).</summary>
    private static int? OhneNull(int wert) => wert != 0 ? wert : null;
}
