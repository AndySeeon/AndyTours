using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleLuftlinie"/>.
/// </summary>
public class ErstelleLuftlinieTests
{
    [Fact]
    public void Gerade_Linie_ohne_Abbiegepunkte_und_Hoehen()
    {
        var von = new Geoposition(48.0, 11.0);
        var nach = new Geoposition(48.1, 11.2);

        Teilstueck luftlinie = ErstelleLuftlinie.Ausfuehren(von, nach);

        Assert.True(luftlinie.IstLuftlinie);
        Assert.Equal([von, nach], luftlinie.Punkte.Select(p => p.Position));
        Assert.All(luftlinie.Punkte, p => Assert.Null(p.Hoehe));
        Assert.Empty(luftlinie.Abbiegepunkte);
    }
}
