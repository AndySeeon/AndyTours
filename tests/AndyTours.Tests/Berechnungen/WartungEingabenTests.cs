using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="WartungEingaben"/>.
/// </summary>
public class WartungEingabenTests
{
    /// <summary>Der „heutige“ Tag der Tests.</summary>
    private static readonly DateOnly Heute = new(2026, 10, 25);

    /// <summary>Gültige Eingaben.</summary>
    private static Wartungseingaben Gueltig() => new()
    {
        Datum = "25.10.2026", EquipmentId = 7, Taetigkeit = " Kettenwechsel ", Kommentar = " 0,5 % ",
    };

    [Fact]
    public void Gueltige_Eingaben_ergeben_die_Wartung()
    {
        var basis = new Wartung { Id = 12, Typ = WartungsTyp.Wartung, Aktiv = true };

        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(Gueltig(), basis, new Datenbestand(), Heute);

        Assert.True(ergebnis.IstGueltig);
        Assert.Empty(ergebnis.Fehler);
        Wartung wartung = ergebnis.Wartung!;
        Assert.Equal((12L, Heute, 7L, "Kettenwechsel", "0,5 %"),
            (wartung.Id, wartung.Datum, wartung.EquipmentId!.Value, wartung.Taetigkeit, wartung.Kommentar));
        Assert.True(wartung.Aktiv);
    }

    [Fact]
    public void Hin_und_zurueck_bleiben_die_Werte()
    {
        var wartung = new Wartung { Id = 3, Datum = new DateOnly(2026, 9, 6), EquipmentId = 7, Taetigkeit = "Reifen HR", Kommentar = "Schwalbe" };

        Wartungseingaben eingaben = WartungEingaben.AlsEingaben(wartung);
        Wartung zurueck = WartungEingaben.AlsWartung(eingaben, wartung, new Datenbestand(), Heute).Wartung!;

        Assert.Equal("06.09.2026", eingaben.Datum);
        Assert.Equal((wartung.Datum, wartung.EquipmentId, wartung.Taetigkeit, wartung.Kommentar),
            (zurueck.Datum, zurueck.EquipmentId, zurueck.Taetigkeit, zurueck.Kommentar));
    }

    [Fact]
    public void Ohne_Rad_und_Taetigkeit_sind_beide_Felder_markiert()
    {
        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(Gueltig() with { EquipmentId = null, Taetigkeit = "" }, new Wartung(), new Datenbestand(), Heute);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal([Wartungsfeld.Rad, Wartungsfeld.Taetigkeit], ergebnis.Fehler.Order());
        Assert.Null(ergebnis.Hinweis);
    }

    [Theory]
    [InlineData("")]
    [InlineData("31.02.2026")]
    [InlineData("gestern")]
    public void Leeres_oder_ungueltiges_Datum_ist_markiert(string datum)
    {
        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(Gueltig() with { Datum = datum }, new Wartung(), new Datenbestand(), Heute);

        Assert.Equal([Wartungsfeld.Datum], ergebnis.Fehler);
        Assert.Null(ergebnis.Hinweis);
    }

    [Fact]
    public void Datum_in_der_Zukunft_ist_nicht_erlaubt_heute_schon()
    {
        Wartungsergebnis morgen = WartungEingaben.AlsWartung(Gueltig() with { Datum = "26.10.2026" }, new Wartung(), new Datenbestand(), Heute);

        Assert.Equal([Wartungsfeld.Datum], morgen.Fehler);
        Assert.Equal("Das Datum liegt in der Zukunft.", morgen.Hinweis);
        Assert.True(WartungEingaben.AlsWartung(Gueltig(), new Wartung(), new Datenbestand(), Heute).IstGueltig);
    }

    /// <summary>Rad 7 „Genius“ mit einem Kettenwechsel am 01.05.2026 und einem gleichnamigen Alarm am 02.05.2026.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments = { new Equipment { Id = 7, Name = "Genius" }, new Equipment { Id = 8, Name = "Stevens" } },
        Wartungen =
        {
            new Wartung { Id = 1, Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel" },
            new Wartung { Id = 2, Datum = new DateOnly(2026, 5, 2), EquipmentId = 7, Taetigkeit = "Kettenwechsel", Typ = WartungsTyp.Alarm },
        },
    };

    [Theory]
    [InlineData(" Kettenwechsel ")]
    [InlineData("KETTENWECHSEL")]
    public void Gleichartige_Wartung_am_selben_Tag_am_selben_Rad_ist_nicht_erlaubt(string taetigkeit)
    {
        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(
            Gueltig() with { Datum = "01.05.2026", Taetigkeit = taetigkeit }, new Wartung(), Bestand(), Heute);

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal([Wartungsfeld.Taetigkeit], ergebnis.Fehler);
        Assert.Equal("Für ‚Genius‘ gibt es am 01.05.2026 schon die Wartung ‚Kettenwechsel‘.", ergebnis.Hinweis);
    }

    [Theory]
    [InlineData("02.05.2026", 7, "Kettenwechsel")] // anderer Tag (an dem nur der Alarm liegt)
    [InlineData("01.05.2026", 8, "Kettenwechsel")] // anderes Rad
    [InlineData("01.05.2026", 7, "Inspektion")]    // andere Tätigkeit
    public void Anderer_Tag_anderes_Rad_oder_andere_Taetigkeit_ist_erlaubt(string datum, long rad, string taetigkeit)
    {
        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(
            Gueltig() with { Datum = datum, EquipmentId = rad, Taetigkeit = taetigkeit }, new Wartung(), Bestand(), Heute);

        Assert.True(ergebnis.IstGueltig);
    }

    [Fact]
    public void Beim_Bearbeiten_zaehlt_die_Wartung_selbst_nicht()
    {
        Datenbestand bestand = Bestand();
        Wartung vorhanden = bestand.Wartungen[0];

        Wartungsergebnis ergebnis = WartungEingaben.AlsWartung(
            WartungEingaben.AlsEingaben(vorhanden) with { Kommentar = "neu" }, vorhanden, bestand, Heute);

        Assert.True(ergebnis.IstGueltig);
    }
}
