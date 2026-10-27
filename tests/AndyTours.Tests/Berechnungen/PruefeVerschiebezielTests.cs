using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="PruefeVerschiebeziel"/>.
/// </summary>
public class PruefeVerschiebezielTests
{
    /// <summary>Alpen (1) › Tirol (2) › Ötztal (3); Chiemgau (4) auf oberster Ebene.</summary>
    private static readonly Overlayordner[] Ordner =
    [
        new(1, null, "Alpen"),
        new(2, 1, "Tirol"),
        new(3, 2, "Ötztal"),
        new(4, null, "Chiemgau"),
    ];

    [Fact]
    public void In_sich_selbst_ist_nicht_erlaubt()
    {
        Assert.False(PruefeVerschiebeziel.Ausfuehren(1, 1, Ordner));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void In_einen_eigenen_Unterordner_ist_nicht_erlaubt(long ziel)
    {
        Assert.False(PruefeVerschiebeziel.Ausfuehren(1, ziel, Ordner));
    }

    [Fact]
    public void In_einen_anderen_Ordner_ist_erlaubt()
    {
        Assert.True(PruefeVerschiebeziel.Ausfuehren(2, 4, Ordner));
    }

    [Fact]
    public void In_den_eigenen_uebergeordneten_Ordner_ist_erlaubt()
    {
        Assert.True(PruefeVerschiebeziel.Ausfuehren(3, 1, Ordner));
    }

    [Fact]
    public void Auf_die_oberste_Ebene_ist_erlaubt()
    {
        Assert.True(PruefeVerschiebeziel.Ausfuehren(3, null, Ordner));
    }

    [Fact]
    public void Ein_Kreis_in_den_Daten_fuehrt_nicht_zur_Endlosschleife()
    {
        Overlayordner[] kreis = [new(1, 2, "A"), new(2, 1, "B"), new(3, null, "C")];

        Assert.True(PruefeVerschiebeziel.Ausfuehren(3, 1, kreis));
    }
}
