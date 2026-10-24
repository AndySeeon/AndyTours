using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FuehreAktivitaetenZusammen"/> und <see cref="FuehreKategorienZusammen"/>.
/// </summary>
public class FuehreZusammenTests
{
    [Fact]
    public void Kategorien_Einheiten_erhalten_das_Ziel_nur_einmal()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        int anzahl = FuehreKategorienZusammen.Ausfuehren(bestand, 2, 3);

        Assert.Equal(2, anzahl);
        Assert.Equal([3L], bestand.Trainingseinheiten[0].KategorieIds);
        Assert.Equal([3L], bestand.Trainingseinheiten[1].KategorieIds);
        Assert.Equal([3L], bestand.Trainingseinheiten[2].KategorieIds);
        Assert.Equal([1L], bestand.Trainingseinheiten[3].KategorieIds);
        Assert.DoesNotContain(bestand.Kategorien, k => k.Id == 2);
    }

    [Fact]
    public void Kategorien_Standard_und_Equipment_Standardkategorie_gehen_auf_das_Ziel_ueber()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        FuehreKategorienZusammen.Ausfuehren(bestand, 2, 3);

        Assert.True(bestand.Kategorien.Single(k => k.Id == 3).IstStandard);
        Assert.Equal("Training", bestand.Equipments[0].StandardKategorie);
        Assert.Equal("Urlaub", bestand.Equipments[2].StandardKategorie);
    }

    [Fact]
    public void Kategorien_Ziel_ohne_Standard_bleibt_ohne_Standard_wenn_Ursprung_keiner_war()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        FuehreKategorienZusammen.Ausfuehren(bestand, 1, 3);

        Assert.False(bestand.Kategorien.Single(k => k.Id == 3).IstStandard);
        Assert.Equal("Training", bestand.Equipments[2].StandardKategorie);
    }

    [Fact]
    public void Aktivitaeten_Einheiten_und_Equipments_gehen_auf_das_Ziel_ueber()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        int anzahl = FuehreAktivitaetenZusammen.Ausfuehren(bestand, 2, 3);

        Assert.Equal(1, anzahl);
        Assert.Equal([1L, 1L, 3L, 1L], bestand.Trainingseinheiten.Select(e => e.AktivitaetId));
        Assert.Equal([1L, 3L, null], bestand.Equipments.Select(e => e.AktivitaetId));
        Assert.Equal(["Bike", "Gravel"], bestand.Aktivitaeten.Select(a => a.Name));
        Assert.True(bestand.Aktivitaeten[0].IstHaupt);
    }

    [Fact]
    public void Aktivitaeten_Hauptaktivitaet_geht_auf_das_Ziel_ueber()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        FuehreAktivitaetenZusammen.Ausfuehren(bestand, 1, 2);

        Assert.Equal(["Laufen", "Gravel"], bestand.Aktivitaeten.Select(a => a.Name));
        Assert.True(bestand.Aktivitaeten[0].IstHaupt);
        Assert.All(bestand.Trainingseinheiten, e => Assert.NotEqual(1, e.AktivitaetId));
    }
}
