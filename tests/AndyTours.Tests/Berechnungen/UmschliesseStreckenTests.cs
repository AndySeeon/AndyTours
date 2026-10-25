using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="UmschliesseStrecken"/>.
/// </summary>
public class UmschliesseStreckenTests
{
    /// <summary>Eine Strecke, von der hier nur das Rechteck zählt.</summary>
    private static Strecke Strecke(Geoposition suedWest, Geoposition nordOst) => new([suedWest, nordOst], suedWest, nordOst);

    [Fact]
    public void Rechteck_um_alle_Etappen()
    {
        Strecke[] strecken =
        [
            Strecke(new Geoposition(47.80, 12.10), new Geoposition(47.90, 12.40)),
            Strecke(new Geoposition(47.20, 11.30), new Geoposition(47.85, 12.00)),
            Strecke(new Geoposition(46.70, 11.40), new Geoposition(47.20, 11.60)),
        ];

        (Geoposition suedWest, Geoposition nordOst) = UmschliesseStrecken.Ausfuehren(strecken);

        Assert.Equal(new Geoposition(46.70, 11.30), suedWest);
        Assert.Equal(new Geoposition(47.90, 12.40), nordOst);
    }

    [Fact]
    public void Eine_Strecke_behaelt_ihr_Rechteck()
    {
        Strecke strecke = Strecke(new Geoposition(47.80, 12.10), new Geoposition(47.90, 12.40));

        Assert.Equal((strecke.SuedWest, strecke.NordOst), UmschliesseStrecken.Ausfuehren([strecke]));
    }
}
