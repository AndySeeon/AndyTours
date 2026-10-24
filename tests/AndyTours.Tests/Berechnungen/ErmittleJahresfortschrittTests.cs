using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleJahresfortschritt"/>.
/// </summary>
public class ErmittleJahresfortschrittTests
{
    /// <summary>Heute: 2. Juli 2026, der 183. Tag eines Jahres mit 365 Tagen.</summary>
    private static readonly DateOnly Heute = new(2026, 7, 2);

    /// <summary>
    /// Erzeugt den Testbestand.
    /// </summary>
    /// <remarks>
    /// Aktivitäten: Bike (1, Grundziel 6.000 km, 100 Einheiten, keine hm), Laufen (2, eigenes Ziel ab 2025:
    /// 500 km). Kategorien: Training (10), Urlaub (11).
    /// Einheiten: 2024 Bike 100 km/1.000 hm/2 h Training und 50 km/500 hm/1,5 h Urlaub; 2025 Laufen 10 km;
    /// 2026 Bike 200 km/2.000 hm/8 h Training.
    /// </remarks>
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten =
        [
            new() { Id = 1, Name = "Bike", ZielKm = 6000, ZielAnzahl = 100 },
            new() { Id = 2, Name = "Laufen" },
        ],
        Saisonziele = [new() { AktivitaetId = 2, AbJahr = 2025, ZielKm = 500 }],
        Kategorien =
        [
            new() { Id = 10, Name = "Training" },
            new() { Id = 11, Name = "Urlaub" },
        ],
        Trainingseinheiten =
        [
            new() { Id = 1, AktivitaetId = 1, Datum = new(2024, 5, 1), DistanzKm = 100, Hoehenmeter = 1000, Fahrzeit = TimeSpan.FromHours(2), KategorieIds = [10] },
            new() { Id = 2, AktivitaetId = 1, Datum = new(2024, 6, 1), DistanzKm = 50, Hoehenmeter = 500, Fahrzeit = TimeSpan.FromHours(1.5), KategorieIds = [11] },
            new() { Id = 3, AktivitaetId = 2, Datum = new(2025, 3, 1), DistanzKm = 10, Hoehenmeter = 100, Fahrzeit = TimeSpan.FromHours(1) },
            new() { Id = 4, AktivitaetId = 1, Datum = new(2026, 2, 1), DistanzKm = 200, Hoehenmeter = 2000, Fahrzeit = TimeSpan.FromHours(8), KategorieIds = [10] },
        ],
    };

    [Fact]
    public void Alle_Jahre_der_Aktivitaet_erscheinen_aufsteigend_mit_ihrem_Ziel()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, null, Fortschrittswert.Kilometer, Heute);

        Assert.Equal(new[] { new Jahrespunkt(2024, 150, 6000), new Jahrespunkt(2026, 200, 6000) }, ergebnis.Punkte);
    }

    [Fact]
    public void Laufendes_Jahr_ohne_Einheiten_erscheint_mit_null()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 2, null, Fortschrittswert.Kilometer, Heute);

        Assert.Equal(new[] { new Jahrespunkt(2025, 10, 500), new Jahrespunkt(2026, 0, 500) }, ergebnis.Punkte);
    }

    [Fact]
    public void Ohne_Einheiten_gibt_es_keine_Punkte()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 2, "Training", Fortschrittswert.Kilometer, Heute);

        Assert.Empty(ergebnis.Punkte);
        Assert.Equal(0, ergebnis.Kennzahlen.Stand);
        Assert.Null(ergebnis.Kennzahlen.Ziel);
    }

    [Fact]
    public void Alle_Aktivitaeten_summieren_Einheiten_und_Ziele()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), null, null, Fortschrittswert.Kilometer, Heute);

        Assert.Equal(
            new[] { new Jahrespunkt(2024, 150, 6000), new Jahrespunkt(2025, 10, 6500), new Jahrespunkt(2026, 200, 6500) },
            ergebnis.Punkte);
    }

    [Fact]
    public void Kategorie_der_Filterleiste_schraenkt_ein()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, "Urlaub", Fortschrittswert.Kilometer, Heute);

        Assert.Equal(new[] { new Jahrespunkt(2024, 50, 6000), new Jahrespunkt(2026, 0, 6000) }, ergebnis.Punkte);
    }

    [Fact]
    public void Anzahl_zaehlt_die_Einheiten()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, null, Fortschrittswert.Anzahl, Heute);

        Assert.Equal(new[] { new Jahrespunkt(2024, 2, 100), new Jahrespunkt(2026, 1, 100) }, ergebnis.Punkte);
    }

    [Fact]
    public void Hoehenmeter_ohne_Ziel_haben_kein_Ziel()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, null, Fortschrittswert.Hoehenmeter, Heute);

        Assert.Equal(new[] { new Jahrespunkt(2024, 1500, null), new Jahrespunkt(2026, 2000, null) }, ergebnis.Punkte);
        Assert.Null(ergebnis.Kennzahlen.Ziel);
        Assert.Equal(2000, ergebnis.Kennzahlen.Stand);
    }

    [Fact]
    public void Fahrzeit_in_Stunden_und_nie_mit_Ziel()
    {
        Jahresfortschritt ergebnis = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, null, Fortschrittswert.Fahrzeit, Heute);

        Assert.Equal(new[] { new Jahrespunkt(2024, 3.5, null), new Jahrespunkt(2026, 8, null) }, ergebnis.Punkte);
        Assert.Null(ergebnis.Kennzahlen.Ziel);
    }

    [Fact]
    public void Kennzahlen_berechnen_Soll_Rueckstand_und_Wochenleistung()
    {
        Jahreskennzahlen k = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, null, Fortschrittswert.Kilometer, Heute).Kennzahlen;

        // Soll: 6.000 × 183 / 365; Restwochen: (365 − 183 + 1) / 7 = 183 / 7
        double soll = 6000.0 * 183 / 365;
        Assert.Equal(2026, k.Jahr);
        Assert.Equal(200, k.Stand);
        Assert.Equal(6000, k.Ziel);
        Assert.Equal(200.0 / 6000, k.Anteil!.Value, 9);
        Assert.Equal(soll, k.Soll!.Value, 9);
        Assert.Equal(200 - soll, k.Differenz!.Value, 9);
        Assert.Equal(5800, k.Rest);
        Assert.Equal(5800 / (183 / 7.0), k.ProWoche!.Value, 9);
    }

    [Fact]
    public void Am_31_Dezember_ist_das_Soll_das_ganze_Ziel_und_es_bleibt_eine_Siebtelwoche()
    {
        Jahreskennzahlen k = ErmittleJahresfortschritt.Ausfuehren(Bestand(), 1, null, Fortschrittswert.Kilometer, new(2026, 12, 31)).Kennzahlen;

        Assert.Equal(6000, k.Soll!.Value, 9);
        Assert.Equal(5800 * 7, k.ProWoche!.Value, 9);
    }

    [Fact]
    public void Erreichtes_Ziel_hat_keinen_Rest_und_keine_Wochenleistung()
    {
        Datenbestand bestand = Bestand();
        bestand.Saisonziele.Add(new() { AktivitaetId = 1, AbJahr = 2026, ZielKm = 150 });

        Jahreskennzahlen k = ErmittleJahresfortschritt.Ausfuehren(bestand, 1, null, Fortschrittswert.Kilometer, Heute).Kennzahlen;

        Assert.Equal(150, k.Ziel);
        Assert.Equal(0, k.Rest);
        Assert.Null(k.ProWoche);
    }
}
