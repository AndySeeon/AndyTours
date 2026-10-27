using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="UebernimmEquipment"/>.
/// </summary>
public class UebernimmEquipmentTests
{
    [Fact]
    public void Neuer_Standard_nimmt_den_anderen_Equipments_der_Aktivitaet_die_Kennzeichnung()
    {
        // Genius (Bike) wird Standard; Rennrad und das ausgemusterte Altrad sind es bisher
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        Equipment genius = new() { Id = 1, Name = "Genius", AktivitaetId = 1, IstStandard = true };

        List<Equipment> aenderungen = UebernimmEquipment.ErmittleAenderungen(bestand, genius);

        Assert.Equal(["Genius", "Altrad", "Rennrad"], aenderungen.Select(e => e.Name));
        Assert.True(aenderungen[0].IstStandard);
        Assert.All(aenderungen.Skip(1), e => Assert.False(e.IstStandard));
    }

    [Fact]
    public void Ermitteln_laesst_den_Bestand_unveraendert()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        UebernimmEquipment.ErmittleAenderungen(bestand, new Equipment { Id = 1, Name = "Genius", AktivitaetId = 1, IstStandard = true });

        Assert.True(bestand.Equipments.Single(e => e.Name == "Rennrad").IstStandard);
        Assert.False(bestand.Equipments.Single(e => e.Name == "Genius").IstStandard);
    }

    [Fact]
    public void Standard_einer_anderen_Aktivitaet_bleibt()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        List<Equipment> aenderungen = UebernimmEquipment.ErmittleAenderungen(bestand,
            new Equipment { Id = 7, Name = "Neu", AktivitaetId = 2, IstStandard = true });

        Assert.Equal(["Neu"], aenderungen.Select(e => e.Name));
    }

    [Fact]
    public void Ohne_Standard_aendert_sich_nur_das_Equipment_selbst()
    {
        List<Equipment> aenderungen = UebernimmEquipment.ErmittleAenderungen(Auswahlbestand.MitEquipments(),
            new Equipment { Id = 1, Name = "Genius", AktivitaetId = 1 });

        Assert.Single(aenderungen);
    }

    [Fact]
    public void Beim_Ausmustern_faellt_der_Standard_weg()
    {
        List<Equipment> aenderungen = UebernimmEquipment.ErmittleAenderungen(Auswahlbestand.MitEquipments(),
            new Equipment { Id = 5, Name = "Rennrad", AktivitaetId = 1, IstStandard = true, Ausgemustert = true });

        Assert.Single(aenderungen);
        Assert.False(aenderungen[0].IstStandard);
        Assert.True(aenderungen[0].Ausgemustert);
    }

    [Fact]
    public void Einsetzen_ersetzt_vorhandene_und_haengt_neue_an()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        int anzahl = bestand.Equipments.Count;

        UebernimmEquipment.Einsetzen(bestand,
        [
            new Equipment { Id = 1, Name = "Genius 2" },
            new Equipment { Id = 20, Name = "Gravel-Rad", AktivitaetId = 1 },
        ]);

        Assert.Equal(anzahl + 1, bestand.Equipments.Count);
        Assert.Equal("Genius 2", bestand.Equipments[0].Name);
        Assert.Equal("Gravel-Rad", bestand.Equipments[^1].Name);
    }
}
