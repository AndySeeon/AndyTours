using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="UmschliesseOverlay"/>.
/// </summary>
public class UmschliesseOverlayTests
{
    [Fact]
    public void Rechteck_um_alle_Abschnitte()
    {
        var overlay = new Overlay(1, null, "Test", null, true,
        [
            [new Geoposition(48.1, 11.5), new Geoposition(48.3, 11.4)],
            [new Geoposition(47.9, 11.8)],
        ]);

        (Geoposition suedWest, Geoposition nordOst) = UmschliesseOverlay.Ausfuehren(overlay);

        Assert.Equal(new Geoposition(47.9, 11.4), suedWest);
        Assert.Equal(new Geoposition(48.3, 11.8), nordOst);
    }

    [Fact]
    public void Ein_einzelner_Punkt_ergibt_ein_Rechteck_ohne_Ausdehnung()
    {
        var overlay = new Overlay(1, null, "Test", null, true, [[new Geoposition(48, 11)]]);

        Assert.Equal((new Geoposition(48, 11), new Geoposition(48, 11)), UmschliesseOverlay.Ausfuehren(overlay));
    }
}
