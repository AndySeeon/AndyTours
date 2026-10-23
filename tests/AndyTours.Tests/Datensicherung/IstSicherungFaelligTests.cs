using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="IstSicherungFaellig"/>.
/// </summary>
public class IstSicherungFaelligTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, true)] // z. B. nach einer fehlgeschlagenen Sicherung
    public void Faellig_ab_dem_eingestellten_Intervall(int starts, bool erwartet)
    {
        Assert.Equal(erwartet, IstSicherungFaellig.Ausfuehren(starts, intervall: 3, anzahlEinheiten: 932));
    }

    [Fact]
    public void Intervall_1_sichert_bei_jedem_Start()
    {
        Assert.True(IstSicherungFaellig.Ausfuehren(1, intervall: 1, anzahlEinheiten: 1));
    }

    [Fact]
    public void Ohne_Touren_ist_nie_eine_Sicherung_faellig()
    {
        Assert.False(IstSicherungFaellig.Ausfuehren(10, intervall: 3, anzahlEinheiten: 0));
    }
}
