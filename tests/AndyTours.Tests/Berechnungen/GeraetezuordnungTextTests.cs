using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="GeraetezuordnungText"/>.
/// </summary>
public class GeraetezuordnungTextTests
{
    [Fact]
    public void Lesen_erkennt_Rad_Sensor_und_Altwert()
    {
        List<Radkennung> kennungen = GeraetezuordnungText.Lesen(" Rad:7af22db6 ;sensor: 3452708077; Bike1;; Sensor:3452708077 ");

        Assert.Equal(
            new[]
            {
                new Radkennung(Radkennungsart.Rad, "7af22db6"),
                new Radkennung(Radkennungsart.Sensor, "3452708077"),
                new Radkennung(Radkennungsart.Altwert, "Bike1"),
            },
            kennungen);
    }

    [Fact]
    public void Lesen_leerer_Text_liefert_keine_Kennungen()
    {
        Assert.Empty(GeraetezuordnungText.Lesen(""));
        Assert.Empty(GeraetezuordnungText.Lesen("  ;  "));
    }

    [Fact]
    public void Schreiben_und_wieder_Lesen_ergibt_dieselben_Kennungen()
    {
        List<Radkennung> kennungen =
        [
            new(Radkennungsart.Rad, "7af22db6-d829-4395-9e42-a200867c4abf"),
            new(Radkennungsart.Sensor, "3452708077"),
            new(Radkennungsart.Altwert, "Run"),
        ];

        string text = GeraetezuordnungText.Schreiben(kennungen);

        Assert.Equal("Rad:7af22db6-d829-4395-9e42-a200867c4abf; Sensor:3452708077; Run", text);
        Assert.Equal(kennungen, GeraetezuordnungText.Lesen(text));
    }

    [Fact]
    public void Schreiben_ohne_Kennungen_ist_leer()
    {
        Assert.Equal("", GeraetezuordnungText.Schreiben([]));
    }
}
