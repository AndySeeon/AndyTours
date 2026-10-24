using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleStandardEquipment"/>, <see cref="ErmittleEquipmentauswahl"/> und
/// <see cref="FindeStandardkategorie"/>.
/// </summary>
public class EquipmentauswahlTests
{
    [Fact]
    public void Standard_Equipment_ist_das_nicht_ausgemusterte()
    {
        // Altrad ist ebenfalls als Standard gekennzeichnet, aber ausgemustert
        Assert.Equal("Rennrad", ErmittleStandardEquipment.Ausfuehren(Auswahlbestand.MitEquipments(), 1)?.Name);
    }

    [Theory]
    [InlineData(2L)]
    [InlineData(0L)]
    [InlineData(null)]
    public void Ohne_Standard_oder_ohne_Aktivitaet_gibt_es_kein_Standard_Equipment(long? aktivitaetId)
    {
        Assert.Null(ErmittleStandardEquipment.Ausfuehren(Auswahlbestand.MitEquipments(), aktivitaetId));
    }

    [Fact]
    public void Ausgemustertes_Standard_Equipment_ist_nicht_wirksam()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        Assert.False(ErmittleStandardEquipment.IstWirksam(bestand.Equipments.Single(e => e.Name == "Altrad")));
        Assert.True(ErmittleStandardEquipment.IstWirksam(bestand.Equipments.Single(e => e.Name == "Rennrad")));
    }

    [Fact]
    public void Auswahl_zeigt_Equipments_der_Aktivitaet_und_ohne_Aktivitaet()
    {
        List<Auswahleintrag> auswahl = ErmittleEquipmentauswahl.Ausfuehren(Auswahlbestand.MitEquipments(), 1, null);
        Assert.Equal(["(kein)", "Cube", "Genius", "Rennrad"], auswahl.Select(e => e.Name));
        Assert.Null(auswahl[0].Id);
    }

    [Fact]
    public void Auswahl_einer_anderen_Aktivitaet_ohne_Ausgemusterte()
    {
        List<Auswahleintrag> auswahl = ErmittleEquipmentauswahl.Ausfuehren(Auswahlbestand.MitEquipments(), 2, null);
        Assert.Equal(["(kein)", "Cube", "Neu", "Schuhe"], auswahl.Select(e => e.Name));
    }

    [Fact]
    public void Bisheriges_Equipment_bleibt_auch_ausgemustert_oder_fremd_erhalten()
    {
        Assert.Equal(["(kein)", "Altrad", "Cube", "Neu", "Schuhe"],
            ErmittleEquipmentauswahl.Ausfuehren(Auswahlbestand.MitEquipments(), 2, 4).Select(e => e.Name));
    }

    [Fact]
    public void Ohne_Aktivitaet_nur_Equipments_ohne_Aktivitaet()
    {
        Assert.Equal(["(kein)", "Cube"], ErmittleEquipmentauswahl.Ausfuehren(Auswahlbestand.MitEquipments(), null, null).Select(e => e.Name));
    }

    [Fact]
    public void Wieder_aktiviertes_Equipment_wird_angeboten()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        bestand.Equipments.Single(e => e.Name == "Spikes").Ausgemustert = false;
        Assert.Contains("Spikes", ErmittleEquipmentauswahl.Ausfuehren(bestand, 2, null).Select(e => e.Name));
    }

    [Fact]
    public void Standardkategorie_wird_ohne_Unterschied_der_Schreibweise_gefunden()
    {
        // Genius hat „trainieren “ eingetragen, die Kategorie heißt „Trainieren“ (Id 2)
        Assert.Equal(2, FindeStandardkategorie.Ausfuehren(Auswahlbestand.MitEquipments(), 1));
    }

    [Theory]
    [InlineData(2L)]
    [InlineData(99L)]
    [InlineData(null)]
    public void Ohne_Standardkategorie_oder_Equipment_gibt_es_keine(long? equipmentId)
    {
        Assert.Null(FindeStandardkategorie.Ausfuehren(Auswahlbestand.MitEquipments(), equipmentId));
    }

    [Fact]
    public void Eingetragene_aber_fehlende_Kategorie_ergibt_keine()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        bestand.Equipments[0].StandardKategorie = "Gibt es nicht";
        Assert.Null(FindeStandardkategorie.Ausfuehren(bestand, 1));
    }
}
