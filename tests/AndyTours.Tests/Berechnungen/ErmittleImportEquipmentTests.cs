using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleImportEquipment"/>.
/// </summary>
public class ErmittleImportEquipmentTests
{
    /// <summary>
    /// Bike (1, Hauptaktivität). Equipments: SL Road (10, Standard, ohne Kennung), JAM2 (11, Rad-Kennung „jam“
    /// und Sensor 300), Raven (12, Sensor 400), Altrad (13, ausgemustert, Rad-Kennung „alt“).
    /// </summary>
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten = [new() { Id = 1, Name = "Bike", IstHaupt = true }],
        Equipments =
        [
            new() { Id = 10, Name = "SL Road", AktivitaetId = 1, IstStandard = true },
            new() { Id = 11, Name = "JAM2", AktivitaetId = 1, Geraetezuordnung = "Rad:jam; Sensor:300" },
            new() { Id = 12, Name = "Raven", AktivitaetId = 1, Geraetezuordnung = "Sensor:400" },
            new() { Id = 13, Name = "Altrad", AktivitaetId = 1, Ausgemustert = true, Geraetezuordnung = "Rad:alt" },
        ],
    };

    [Fact]
    public void Gemerkte_Rad_Kennung_gewinnt_vor_Sensor()
    {
        Aufzeichnung aufzeichnung = new() { RadKennung = "jam", Sensoren = ["400"] };

        Assert.Equal(11, ErmittleImportEquipment.Ausfuehren(Bestand(), aufzeichnung)?.Id);
    }

    [Fact]
    public void Unbekannte_Rad_Kennung_faellt_auf_Sensor_zurueck()
    {
        Aufzeichnung aufzeichnung = new() { RadKennung = "neu", Sensoren = ["999", "400"] };

        Assert.Equal(12, ErmittleImportEquipment.Ausfuehren(Bestand(), aufzeichnung)?.Id);
    }

    [Fact]
    public void Ohne_Treffer_Standard_Equipment_der_Hauptaktivitaet()
    {
        Assert.Equal(10, ErmittleImportEquipment.Ausfuehren(Bestand(), new Aufzeichnung())?.Id);
        Assert.Equal(10, ErmittleImportEquipment.Ausfuehren(Bestand(), new Aufzeichnung { Sensoren = ["999"] })?.Id);
    }

    [Fact]
    public void Ausgemustertes_Equipment_wird_nicht_vorgeschlagen()
    {
        Assert.Equal(10, ErmittleImportEquipment.Ausfuehren(Bestand(), new Aufzeichnung { RadKennung = "alt" })?.Id);
    }

    [Fact]
    public void Ohne_Hauptaktivitaet_und_ohne_Treffer_kein_Equipment()
    {
        Datenbestand bestand = Bestand();
        bestand.Aktivitaeten[0].IstHaupt = false;

        Assert.Null(ErmittleImportEquipment.Ausfuehren(bestand, new Aufzeichnung()));
    }
}
