using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="UebernimmZellwert"/>.
/// </summary>
public class UebernimmZellwertTests
{
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike" }, new Aktivitaet { Id = 2, Name = "Wandern" } },
        Equipments = { new Equipment { Id = 7, Name = "Genius" }, new Equipment { Id = 8, Name = "Rennrad" } },
    };

    private static Trainingseinheit Einheit() => new()
    {
        Id = 42, Datum = new DateOnly(2026, 8, 15), Name = "Alm", AktivitaetId = 1, EquipmentId = 7,
        DistanzKm = 1234.5, Hoehenmeter = 900, Fahrzeit = TimeSpan.FromHours(3), Temperatur = 18,
        Anhaenge = [new Anhang { Id = 5, RelativerPfad = "a.fit" }],
    };

    private static Trainingseinheit Gueltig(Einheitenspalte spalte, string text)
    {
        Eingabeergebnis ergebnis = UebernimmZellwert.Ausfuehren(Einheit(), spalte, text, Bestand());
        Assert.True(ergebnis.IstGueltig, $"{spalte} = „{text}“ sollte gültig sein");
        return ergebnis.Einheit!;
    }

    [Fact]
    public void Jede_bearbeitbare_Spalte_wird_uebernommen()
    {
        Assert.Equal(new DateOnly(2026, 8, 16), Gueltig(Einheitenspalte.Datum, "16.08.2026").Datum);
        Assert.Equal(new TimeOnly(10, 15), Gueltig(Einheitenspalte.Startzeit, "10:15").Startzeit);
        Assert.Equal("Kampenwand", Gueltig(Einheitenspalte.Name, "Kampenwand").Name);
        Assert.Equal(2, Gueltig(Einheitenspalte.Aktivitaet, "Wandern").AktivitaetId);
        Assert.Equal(8, Gueltig(Einheitenspalte.Equipment, "Rennrad").EquipmentId);
        Assert.Equal(55.5, Gueltig(Einheitenspalte.Distanz, "55,5").DistanzKm);
        Assert.Equal(1000, Gueltig(Einheitenspalte.Hoehenmeter, "1000").Hoehenmeter);
        Assert.Equal(new TimeSpan(2, 30, 0), Gueltig(Einheitenspalte.Fahrzeit, "2:30:00").Fahrzeit);
        Assert.Equal(65.2, Gueltig(Einheitenspalte.MaxKmh, "65.2").MaxGeschwindigkeit);
        Assert.Equal(131, Gueltig(Einheitenspalte.PulsDurchschnitt, "131").HfDurchschnitt);
        Assert.Equal(172, Gueltig(Einheitenspalte.PulsMax, "172").HfMax);
        Assert.Equal(85, Gueltig(Einheitenspalte.Trittfrequenz, "85").Trittfrequenz);
        Assert.Equal(-3, Gueltig(Einheitenspalte.Temperatur, "-3").Temperatur);
        Assert.Equal(1800, Gueltig(Einheitenspalte.MaxHoehe, "1800").MaxHoehe);
        Assert.Equal("GA2", Gueltig(Einheitenspalte.Trainingsbereich, "GA2").Trainingsbereich);
    }

    [Fact]
    public void Uebrige_Werte_und_Anhaenge_bleiben()
    {
        Trainingseinheit e = Gueltig(Einheitenspalte.Name, "Neu");

        Assert.Equal(42, e.Id);
        Assert.Equal(1234.5, e.DistanzKm);
        Assert.Equal(7, e.EquipmentId);
        Assert.Single(e.Anhaenge);
    }

    [Theory]
    [InlineData("(kein)")]
    [InlineData("")]
    public void Equipment_kein_entfernt_die_Zuordnung(string text)
    {
        Assert.Null(Gueltig(Einheitenspalte.Equipment, text).EquipmentId);
    }

    [Theory]
    [InlineData(Einheitenspalte.Distanz, "abc", Eingabefeld.Distanz)]
    [InlineData(Einheitenspalte.Distanz, "-5", Eingabefeld.Distanz)]
    [InlineData(Einheitenspalte.Datum, "", Eingabefeld.Datum)]
    [InlineData(Einheitenspalte.Datum, "32.01.2026", Eingabefeld.Datum)]
    [InlineData(Einheitenspalte.Hoehenmeter, "-1", Eingabefeld.Hoehenmeter)]
    [InlineData(Einheitenspalte.Fahrzeit, "3:60", Eingabefeld.Fahrzeit)]
    [InlineData(Einheitenspalte.Aktivitaet, "Schwimmen", Eingabefeld.Aktivitaet)]
    [InlineData(Einheitenspalte.Equipment, "Gibt es nicht", Eingabefeld.Equipment)]
    public void Ungueltiger_Wert_meldet_das_Feld(Einheitenspalte spalte, string text, Eingabefeld feld)
    {
        Eingabeergebnis ergebnis = UebernimmZellwert.Ausfuehren(Einheit(), spalte, text, Bestand());

        Assert.False(ergebnis.IstGueltig);
        Assert.Equal([feld], ergebnis.Fehler);
    }

    [Theory]
    [InlineData(Einheitenspalte.DurchschnittKmh)]
    [InlineData(Einheitenspalte.Kategorien)]
    public void Berechnete_und_mehrfache_Spalten_sind_nicht_bearbeitbar(Einheitenspalte spalte)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => UebernimmZellwert.Ausfuehren(Einheit(), spalte, "1", Bestand()));
    }

    [Fact]
    public void Ausgemustertes_Equipment_mit_Zusatz_bleibt_zugeordnet()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments[0].Ausgemustert = true;

        string rohtext = UebernimmZellwert.Rohtext(Einheit(), Einheitenspalte.Equipment, bestand);
        Eingabeergebnis ergebnis = UebernimmZellwert.Ausfuehren(Einheit(), Einheitenspalte.Equipment, rohtext, bestand);

        Assert.Equal("Genius (ausgemustert)", rohtext);
        Assert.True(ergebnis.IstGueltig);
        Assert.Equal(7, ergebnis.Einheit!.EquipmentId);
    }

    [Fact]
    public void Rohtext_ohne_Tausenderpunkt_und_mit_Namen()
    {
        Assert.Equal("1234,5", UebernimmZellwert.Rohtext(Einheit(), Einheitenspalte.Distanz, Bestand()));
        Assert.Equal("Bike", UebernimmZellwert.Rohtext(Einheit(), Einheitenspalte.Aktivitaet, Bestand()));
        Assert.Equal("Genius", UebernimmZellwert.Rohtext(Einheit(), Einheitenspalte.Equipment, Bestand()));
        Assert.Equal("(kein)", UebernimmZellwert.Rohtext(new Trainingseinheit(), Einheitenspalte.Equipment, Bestand()));
        Assert.Equal("3:00:00", UebernimmZellwert.Rohtext(Einheit(), Einheitenspalte.Fahrzeit, Bestand()));
    }
}
