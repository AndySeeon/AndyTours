using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleLoeschsperre"/> mit dem Bestand aus <see cref="Auswahlbestand"/>.
/// </summary>
public class ErmittleLoeschsperreTests
{
    [Fact]
    public void Hauptaktivitaet_ist_gesperrt()
    {
        Assert.Equal(
            "‚Bike‘ ist die Hauptaktivität und kann nicht gelöscht werden. Bitte zuerst eine andere Aktivität als Hauptaktivität festlegen.",
            ErmittleLoeschsperre.Aktivitaet(Auswahlbestand.Erzeugen(), 1));
    }

    [Fact]
    public void Verwendete_Aktivitaet_nennt_Einheiten_und_Equipments()
    {
        Assert.Equal("‚Laufen‘ wird von 1 Einheit und 1 Equipment verwendet und kann nicht gelöscht werden.",
            ErmittleLoeschsperre.Aktivitaet(Auswahlbestand.Erzeugen(), 2));
    }

    [Fact]
    public void Aktivitaet_nur_mit_Equipments_nennt_nur_die_Equipments()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();
        bestand.Trainingseinheiten.RemoveAll(e => e.AktivitaetId == 2);
        bestand.Equipments.Add(new Equipment { Id = 4, Name = "Laufband", AktivitaetId = 2 });

        Assert.Equal("‚Laufen‘ wird von 2 Equipments verwendet und kann nicht gelöscht werden.",
            ErmittleLoeschsperre.Aktivitaet(bestand, 2));
    }

    [Fact]
    public void Unbenutzte_Aktivitaet_darf_geloescht_werden()
    {
        Assert.Null(ErmittleLoeschsperre.Aktivitaet(Auswahlbestand.Erzeugen(), 3));
    }

    [Fact]
    public void Letzte_Aktivitaet_ist_gesperrt()
    {
        var bestand = new Datenbestand { Aktivitaeten = [new() { Id = 5, Name = "Gravel" }] };

        Assert.Equal("‚Gravel‘ ist die letzte Aktivität und kann nicht gelöscht werden.",
            ErmittleLoeschsperre.Aktivitaet(bestand, 5));
    }

    [Fact]
    public void Kategorie_mit_Einheiten_und_Equipment_nennt_beides()
    {
        Assert.Equal(
            "‚Trainieren‘ wird in 2 Einheiten verwendet und ist beim Equipment ‚Genius‘ als Standardkategorie eingetragen; sie kann daher nicht gelöscht werden.",
            ErmittleLoeschsperre.Kategorie(Auswahlbestand.Erzeugen(), 2));
    }

    [Fact]
    public void Kategorie_nur_mit_Einheiten_nennt_die_Anzahl()
    {
        Assert.Equal("‚Training‘ wird in 2 Einheiten verwendet; sie kann daher nicht gelöscht werden.",
            ErmittleLoeschsperre.Kategorie(Auswahlbestand.Erzeugen(), 3));
    }

    [Fact]
    public void Kategorie_nur_als_Standardkategorie_nennt_das_Equipment()
    {
        Datenbestand bestand = Auswahlbestand.Erzeugen();
        bestand.Trainingseinheiten.RemoveAll(e => e.KategorieIds.Contains(1));

        Assert.Equal("‚Urlaub‘ ist beim Equipment ‚Cube‘ als Standardkategorie eingetragen; sie kann daher nicht gelöscht werden.",
            ErmittleLoeschsperre.Kategorie(bestand, 1));
    }

    [Fact]
    public void Unbenutzte_Kategorie_darf_geloescht_werden()
    {
        Assert.Null(ErmittleLoeschsperre.Kategorie(Auswahlbestand.Erzeugen(), 4));
    }

    [Fact]
    public void Equipment_mit_Einheiten_und_Wartungen_nennt_beides_und_empfiehlt_Ausmustern()
    {
        Assert.Equal(
            "‚Genius‘ wird in 1 Einheit verwendet und hat 1 Wartung; es kann daher nicht gelöscht werden. Bitte stattdessen als ausgemustert kennzeichnen.",
            ErmittleLoeschsperre.Equipment(Auswahlbestand.MitEquipments(), 1));
    }

    [Fact]
    public void Equipment_nur_mit_Wartungen_ist_gesperrt()
    {
        Assert.Equal(
            "‚Rennrad‘ hat 2 Wartungen; es kann daher nicht gelöscht werden. Bitte stattdessen als ausgemustert kennzeichnen.",
            ErmittleLoeschsperre.Equipment(Auswahlbestand.MitEquipments(), 5));
    }

    [Fact]
    public void Ausgemustertes_Equipment_mit_Einheiten_ist_gesperrt_ohne_Empfehlung()
    {
        Assert.Equal(
            "‚Altrad‘ wird in 1 Einheit verwendet; es kann daher nicht gelöscht werden.",
            ErmittleLoeschsperre.Equipment(Auswahlbestand.MitEquipments(), 4));
    }

    [Fact]
    public void Unbenutztes_Equipment_ohne_Wartungen_darf_geloescht_werden()
    {
        Assert.Null(ErmittleLoeschsperre.Equipment(Auswahlbestand.MitEquipments(), 7));
    }
}
