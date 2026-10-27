using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="OrdneEinheitenZu"/>.
/// </summary>
public class OrdneEinheitenZuTests
{
    /// <summary>Die Einheiten 10 (Bike, Trainieren, Genius) und 11 (Bike, Trainieren + Training, Altrad).</summary>
    private static List<Trainingseinheit> Markiert(Datenbestand bestand) => bestand.Trainingseinheiten.Where(e => e.Id is 10 or 11).ToList();

    [Fact]
    public void Gesetzte_und_entfernte_Kategorien_gelten_fuer_alle_teilweise_bleibt()
    {
        // Urlaub setzen, Trainieren entfernen, Training (teilweise) unberührt lassen
        List<Trainingseinheit> geaendert = OrdneEinheitenZu.Kategorien(Markiert(Auswahlbestand.MitEquipments()), [1], [2]);

        Assert.Equal([1L], geaendert[0].KategorieIds);
        Assert.Equal([3L, 1L], geaendert[1].KategorieIds);
    }

    [Fact]
    public void Vorhandene_Kategorie_kommt_nicht_doppelt()
    {
        List<Trainingseinheit> geaendert = OrdneEinheitenZu.Kategorien(Markiert(Auswahlbestand.MitEquipments()), [2, 3], []);

        Assert.Equal([2L, 3L], geaendert[0].KategorieIds);
        Assert.Equal([2L, 3L], geaendert[1].KategorieIds);
    }

    [Fact]
    public void Aktivitaet_wechselt_und_das_Equipment_bleibt()
    {
        List<Trainingseinheit> geaendert = OrdneEinheitenZu.Aktivitaet(Markiert(Auswahlbestand.MitEquipments()), 2);

        Assert.All(geaendert, e => Assert.Equal(2, e.AktivitaetId));
        Assert.Equal([1L, 4L], geaendert.Select(e => e.EquipmentId!.Value));
    }

    [Theory]
    [InlineData(5L)]
    [InlineData(null)]
    public void Equipment_wird_gesetzt_und_die_Kategorien_bleiben(long? equipmentId)
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        bestand.Equipments.Single(e => e.Id == 5).StandardKategorie = "Urlaub";

        List<Trainingseinheit> geaendert = OrdneEinheitenZu.Equipment(Markiert(bestand), equipmentId);

        Assert.All(geaendert, e => Assert.Equal(equipmentId, e.EquipmentId));
        Assert.Equal([2L], geaendert[0].KategorieIds);
        Assert.Equal([2L, 3L], geaendert[1].KategorieIds);
    }

    [Fact]
    public void Originale_bleiben_unveraendert_Nummer_Datum_und_Anhaenge_bleiben()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        List<Trainingseinheit> markiert = Markiert(bestand);
        markiert[0].Datum = new DateOnly(2020, 5, 1);
        markiert[0].Anhaenge = [new Anhang { Id = 3, RelativerPfad = "a.fit" }];

        List<Trainingseinheit> geaendert = OrdneEinheitenZu.Aktivitaet(markiert, 2);

        Assert.Equal(1, markiert[0].AktivitaetId);
        Assert.Equal(10, geaendert[0].Id);
        Assert.Equal(new DateOnly(2020, 5, 1), geaendert[0].Datum);
        Assert.Equal("a.fit", Assert.Single(geaendert[0].Anhaenge).RelativerPfad);
        Assert.NotSame(markiert[0].KategorieIds, geaendert[0].KategorieIds);
    }
}
