using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="VerschiebeEintrag"/>.
/// </summary>
public class VerschiebeEintragTests
{
    /// <summary>Beispielliste.</summary>
    private static readonly string[] Liste = ["GA1", "GA2", "K0"];

    [Fact]
    public void Nach_oben_tauscht_mit_dem_Vorgaenger()
    {
        Assert.Equal(["K0", "GA1", "GA2"], VerschiebeEintrag.Ausfuehren(VerschiebeEintrag.Ausfuehren(Liste, 2, nachOben: true), 1, nachOben: true));
    }

    [Fact]
    public void Nach_unten_tauscht_mit_dem_Nachfolger()
    {
        Assert.Equal(["GA2", "GA1", "K0"], VerschiebeEintrag.Ausfuehren(Liste, 0, nachOben: false));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, false)]
    [InlineData(-1, true)]
    [InlineData(3, false)]
    public void Am_Rand_oder_bei_ungueltiger_Stelle_bleibt_die_Liste_unveraendert(int index, bool nachOben)
    {
        Assert.Equal(Liste, VerschiebeEintrag.Ausfuehren(Liste, index, nachOben));
    }

    [Fact]
    public void Die_uebergebene_Liste_bleibt_unveraendert()
    {
        string[] liste = ["A", "B"];
        VerschiebeEintrag.Ausfuehren(liste, 1, nachOben: true);

        Assert.Equal(["A", "B"], liste);
    }
}
