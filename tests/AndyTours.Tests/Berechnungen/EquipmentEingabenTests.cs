using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="EquipmentEingaben"/>.
/// </summary>
public class EquipmentEingabenTests
{
    /// <summary>Gültige Eingaben für ein neues Equipment.</summary>
    private static readonly Equipmenteingaben Gueltig = new()
    {
        Name = " Gravel-Rad ",
        AktivitaetId = 1,
        StandardKategorie = "Urlaub",
        Anschaffungsdatum = "24.10.2026",
        Anfangskilometer = "12,5",
        IstStandard = true,
    };

    [Fact]
    public void Gueltige_Eingaben_ergeben_ein_Equipment()
    {
        (Equipment? equipment, string? hinweis) = EquipmentEingaben.AlsEquipment(Gueltig, new Equipment(), Auswahlbestand.MitEquipments());

        Assert.Null(hinweis);
        Assert.NotNull(equipment);
        Assert.Equal("Gravel-Rad", equipment.Name);
        Assert.Equal(1, equipment.AktivitaetId);
        Assert.Equal("Urlaub", equipment.StandardKategorie);
        Assert.Equal(new DateOnly(2026, 10, 24), equipment.Anschaffungsdatum);
        Assert.Equal(12.5, equipment.AnfangsdistanzKm);
        Assert.True(equipment.IstStandard);
        Assert.False(equipment.Ausgemustert);
    }

    [Fact]
    public void Leere_Felder_ergeben_kein_Datum_und_null_Kilometer()
    {
        (Equipment? equipment, _) = EquipmentEingaben.AlsEquipment(
            Gueltig with { Anschaffungsdatum = "", Anfangskilometer = " " }, new Equipment(), Auswahlbestand.MitEquipments());

        Assert.Null(equipment!.Anschaffungsdatum);
        Assert.Equal(0, equipment.AnfangsdistanzKm);
    }

    [Theory]
    [InlineData("", "Bitte einen Eintrag eingeben.")]
    [InlineData(" genius ", "‚Genius‘ steht bereits in der Liste.")]
    public void Leerer_oder_vorhandener_Name_wird_abgewiesen(string name, string erwartet)
    {
        (Equipment? equipment, string? hinweis) = EquipmentEingaben.AlsEquipment(
            Gueltig with { Name = name }, new Equipment(), Auswahlbestand.MitEquipments());

        Assert.Null(equipment);
        Assert.Equal(erwartet, hinweis);
    }

    [Fact]
    public void Eigener_Name_zaehlt_beim_Bearbeiten_nicht()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        (Equipment? equipment, string? hinweis) = EquipmentEingaben.AlsEquipment(
            Gueltig with { Name = "GENIUS" }, bestand.Equipments[0], bestand);

        Assert.Null(hinweis);
        Assert.Equal("GENIUS", equipment!.Name);
        Assert.Equal(1, equipment.Id);
    }

    [Theory]
    [InlineData("31.02.2020")]
    [InlineData("gestern")]
    public void Ungueltiges_Datum_wird_abgewiesen(string datum)
    {
        Assert.Equal("Das Anschaffungsdatum ist ungültig (Form: 24.10.2026).",
            EquipmentEingaben.AlsEquipment(Gueltig with { Anschaffungsdatum = datum }, new Equipment(), Auswahlbestand.MitEquipments()).Hinweis);
    }

    [Theory]
    [InlineData("-5")]
    [InlineData("viel")]
    public void Ungueltige_Kilometer_werden_abgewiesen(string kilometer)
    {
        Assert.Equal("Die Anfangskilometer müssen eine Zahl von 0 an sein.",
            EquipmentEingaben.AlsEquipment(Gueltig with { Anfangskilometer = kilometer }, new Equipment(), Auswahlbestand.MitEquipments()).Hinweis);
    }

    [Fact]
    public void Standard_entfaellt_ohne_Aktivitaet_oder_wenn_ausgemustert()
    {
        Datenbestand bestand = Auswahlbestand.MitEquipments();
        Assert.False(EquipmentEingaben.AlsEquipment(Gueltig with { AktivitaetId = null }, new Equipment(), bestand).Equipment!.IstStandard);
        Assert.False(EquipmentEingaben.AlsEquipment(Gueltig with { Ausgemustert = true }, new Equipment(), bestand).Equipment!.IstStandard);
    }

    [Fact]
    public void Nicht_gepflegte_Werte_bleiben_erhalten()
    {
        var basis = new Equipment
        {
            Id = 9, Name = "Alt", GewichtKg = 13, RadumfangMm = 2290, CrCoeff = 0.004, Kettenblaetter = [36, 52],
            Ritzel = [11, 28], Geraetezuordnung = "Bike1", Bildpfad = "rad.jpg",
        };

        Equipment equipment = EquipmentEingaben.AlsEquipment(Gueltig, basis, Auswahlbestand.MitEquipments()).Equipment!;

        Assert.Equal(9, equipment.Id);
        Assert.Equal(13, equipment.GewichtKg);
        Assert.Equal(2290, equipment.RadumfangMm);
        Assert.Equal(0.004, equipment.CrCoeff);
        Assert.Equal([36, 52], equipment.Kettenblaetter);
        Assert.Equal([11, 28], equipment.Ritzel);
        Assert.Equal("Bike1", equipment.Geraetezuordnung);
        Assert.Equal("rad.jpg", equipment.Bildpfad);
    }

    [Fact]
    public void Eingaben_eines_Equipments_mit_unwirksamem_Standard()
    {
        // Altrad ist als Standard gekennzeichnet, aber ausgemustert
        Equipmenteingaben eingaben = EquipmentEingaben.AlsEingaben(Auswahlbestand.MitEquipments().Equipments.Single(e => e.Name == "Altrad"));

        Assert.False(eingaben.IstStandard);
        Assert.True(eingaben.Ausgemustert);
        Assert.Equal("", eingaben.Anfangskilometer);
        Assert.Equal("", eingaben.Anschaffungsdatum);
    }

    [Fact]
    public void Hin_und_zurueck_ergibt_dieselben_Werte()
    {
        var original = new Equipment
        {
            Id = 3, Name = "Namur", AktivitaetId = 1, StandardKategorie = "Urlaub",
            Anschaffungsdatum = new DateOnly(2016, 4, 13), AnfangsdistanzKm = 1200.5, IstStandard = true,
        };

        Equipmenteingaben eingaben = EquipmentEingaben.AlsEingaben(original);
        Equipment zurueck = EquipmentEingaben.AlsEquipment(eingaben, original, new Datenbestand { Equipments = { original } }).Equipment!;

        Assert.Equal("13.04.2016", eingaben.Anschaffungsdatum);
        Assert.Equal("1200,5", eingaben.Anfangskilometer);
        Assert.Equal(EquipmentEingaben.AlsEingaben(original), EquipmentEingaben.AlsEingaben(zurueck));
    }
}
