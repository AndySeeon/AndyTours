using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="MerkeRadkennungen"/>.
/// </summary>
public class MerkeRadkennungenTests
{
    /// <summary>
    /// Equipments: SL Road (10, Altwert „Bike1“), JAM2 (11, Rad-Kennung „jam“, Sensor 300), Raven (12, ohne Kennung).
    /// </summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        [
            new() { Id = 10, Name = "SL Road", Geraetezuordnung = "Bike1" },
            new() { Id = 11, Name = "JAM2", Geraetezuordnung = "Rad:jam; Sensor:300" },
            new() { Id = 12, Name = "Raven" },
        ],
    };

    [Fact]
    public void Neue_Kennungen_werden_beim_gewaehlten_Equipment_ergaenzt()
    {
        Aufzeichnung aufzeichnung = new() { RadKennung = "raven", Sensoren = ["400", "401"] };

        List<Equipment> geaendert = MerkeRadkennungen.Ausfuehren(Bestand(), aufzeichnung, 12);

        Equipment raven = Assert.Single(geaendert);
        Assert.Equal(12, raven.Id);
        Assert.Equal("Rad:raven; Sensor:400; Sensor:401", raven.Geraetezuordnung);
    }

    [Fact]
    public void Kennungen_wandern_vom_bisherigen_zum_gewaehlten_Equipment()
    {
        Aufzeichnung aufzeichnung = new() { RadKennung = "jam", Sensoren = ["300"] };

        List<Equipment> geaendert = MerkeRadkennungen.Ausfuehren(Bestand(), aufzeichnung, 10);

        Assert.Equal(
            new[] { (10L, "Bike1; Rad:jam; Sensor:300"), (11L, "") },
            geaendert.Select(e => (e.Id, e.Geraetezuordnung)));
    }

    [Fact]
    public void Bereits_gemerkte_Kennungen_aendern_nichts()
    {
        Aufzeichnung aufzeichnung = new() { RadKennung = "jam", Sensoren = ["300"] };

        Assert.Empty(MerkeRadkennungen.Ausfuehren(Bestand(), aufzeichnung, 11));
    }

    [Fact]
    public void Ohne_Kennungen_aendert_sich_nichts()
    {
        Assert.Empty(MerkeRadkennungen.Ausfuehren(Bestand(), new Aufzeichnung(), 12));
    }

    [Fact]
    public void Ohne_Equipment_werden_die_Kennungen_nur_entfernt()
    {
        Aufzeichnung aufzeichnung = new() { Sensoren = ["300"] };

        Equipment jam = Assert.Single(MerkeRadkennungen.Ausfuehren(Bestand(), aufzeichnung, null));
        Assert.Equal("Rad:jam", jam.Geraetezuordnung);
    }

    [Fact]
    public void Der_Bestand_bleibt_unveraendert()
    {
        Datenbestand bestand = Bestand();

        MerkeRadkennungen.Ausfuehren(bestand, new Aufzeichnung { RadKennung = "jam" }, 10);

        Assert.Equal("Bike1", bestand.Equipments[0].Geraetezuordnung);
        Assert.Equal("Rad:jam; Sensor:300", bestand.Equipments[1].Geraetezuordnung);
    }
}
