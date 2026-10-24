using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FormatiereStunden"/>.
/// </summary>
public class FormatiereStundenTests
{
    [Theory]
    [InlineData(0, "0:00 h")]
    [InlineData(0.5, "0:30 h")]
    [InlineData(123.75, "123:45 h")]
    [InlineData(1234.5, "1.234:30 h")]
    public void Stunden_werden_als_Stunden_und_Minuten_formatiert(double stunden, string erwartet)
    {
        Assert.Equal(erwartet, FormatiereStunden.Ausfuehren(stunden));
    }

    [Fact]
    public void Auf_ganze_Minuten_gerundet()
    {
        // 59,6 Minuten werden zu einer Stunde, 59,4 Minuten bleiben 59 Minuten
        Assert.Equal("1:00 h", FormatiereStunden.Ausfuehren(59.6 / 60));
        Assert.Equal("0:59 h", FormatiereStunden.Ausfuehren(59.4 / 60));
    }
}
