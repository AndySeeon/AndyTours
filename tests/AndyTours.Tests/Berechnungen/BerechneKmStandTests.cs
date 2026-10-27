using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BerechneKmStand"/>.
/// </summary>
public class BerechneKmStandTests
{
    /// <summary>Einheiten zweier Räder an verschiedenen Tagen.</summary>
    private static List<Trainingseinheit> Einheiten() =>
    [
        new() { Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, DistanzKm = 40 },
        new() { Datum = new DateOnly(2026, 6, 1), EquipmentId = 7, DistanzKm = 60.5 },
        new() { Datum = new DateOnly(2026, 6, 1), EquipmentId = 8, DistanzKm = 100 },
        new() { Datum = new DateOnly(2026, 6, 2), EquipmentId = 7, DistanzKm = 30 },
        new() { Datum = new DateOnly(2026, 5, 15), EquipmentId = null, DistanzKm = 25 },
    ];

    [Fact]
    public void Einheiten_bis_einschliesslich_zum_Tag_nur_dieses_Rads()
    {
        Assert.Equal(100.5, BerechneKmStand.Ausfuehren(Einheiten(), new Equipment { Id = 7 }, new DateOnly(2026, 6, 1)));
    }

    [Fact]
    public void Anfangskilometer_zaehlen_dazu()
    {
        var rad = new Equipment { Id = 7, AnfangsdistanzKm = 1200 };

        Assert.Equal(1240, BerechneKmStand.Ausfuehren(Einheiten(), rad, new DateOnly(2026, 5, 31)));
        Assert.Equal(1200, BerechneKmStand.Ausfuehren(Einheiten(), rad, new DateOnly(2026, 4, 30)));
    }
}
