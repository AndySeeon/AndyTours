using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FindeEinfuegestelle"/>.
/// </summary>
public class FindeEinfuegestelleTests
{
    // Drei Wegpunkte entlang des Breitengrads 48°: A, B und C je etwa 7,4 km auseinander
    private static readonly Geoposition A = new(48.0, 11.0);
    private static readonly Geoposition B = new(48.0, 11.1);
    private static readonly Geoposition C = new(48.0, 11.2);

    [Fact]
    public void Ohne_Teilstueck_gibt_es_keine_Einfuegestelle()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A], []);

        Assert.Null(FindeEinfuegestelle.Ausfuehren(planung, A));
    }

    [Fact]
    public void Stelle_auf_dem_zweiten_offenen_Teilstueck()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B, C], [null, null]);

        (int teilstueck, double abstand) = FindeEinfuegestelle.Ausfuehren(planung, new Geoposition(48.0, 11.15))!.Value;

        Assert.Equal(1, teilstueck);
        Assert.True(abstand < 1, $"Abstand {abstand} m");
    }

    [Fact]
    public void Abstand_neben_der_Linie_in_Metern()
    {
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B], [null]);

        // 0,0009° Breite sind rund 100 m
        (_, double abstand) = FindeEinfuegestelle.Ausfuehren(planung, new Geoposition(48.0009, 11.05))!.Value;

        Assert.InRange(abstand, 98, 103);
    }

    [Fact]
    public void Ermitteltes_Teilstueck_zaehlt_mit_seinem_Umweg()
    {
        // Teilstück 0 macht einen Bogen nach Norden über (48.05, 11.05); Teilstück 1 ist noch offen
        var umweg = new Teilstueck(
            [
                new Verlaufspunkt { Position = A },
                new Verlaufspunkt { Position = new Geoposition(48.05, 11.05) },
                new Verlaufspunkt { Position = B },
            ],
            [], false);
        var planung = new Tourenplanung(Fahrprofil.Trekking, [A, B, C], [umweg, null]);

        (int teilstueck, double abstand) = FindeEinfuegestelle.Ausfuehren(planung, new Geoposition(48.05, 11.05))!.Value;

        Assert.Equal(0, teilstueck);
        Assert.True(abstand < 1, $"Abstand {abstand} m");
    }
}
