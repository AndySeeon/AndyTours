using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BenenneKategorieUm"/>.
/// </summary>
public class BenenneKategorieUmTests
{
    [Fact]
    public void Benennt_Kategorie_und_Standardkategorie_des_Equipments_um()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        int anzahl = BenenneKategorieUm.Ausfuehren(bestand, 2, " Intervalle ");

        Assert.Equal(1, anzahl);
        Assert.Equal("Intervalle", bestand.Kategorien.Single(k => k.Id == 2).Name);
        Assert.Equal(["Intervalle", "", "Urlaub"], bestand.Equipments.Select(e => e.StandardKategorie));
    }

    [Fact]
    public void Zuordnung_der_Einheiten_bleibt_erhalten()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();

        BenenneKategorieUm.Ausfuehren(bestand, 2, "Intervalle");

        Assert.Equal([2L], bestand.Trainingseinheiten[0].KategorieIds);
    }
}
