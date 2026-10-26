using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="PruefeOrdnername"/>.
/// </summary>
public class PruefeOrdnernameTests
{
    /// <summary>„Alpen“ (1) mit Unterordner „Tirol“ (2), dazu „Chiemgau“ (3) auf oberster Ebene.</summary>
    private static readonly Overlayordner[] Ordner =
    [
        new(1, null, "Alpen"),
        new(2, 1, "Tirol"),
        new(3, null, "Chiemgau"),
    ];

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Leerer_Name_ergibt_Hinweis(string name)
    {
        Assert.Equal("Bitte einen Namen für den Ordner eingeben.", PruefeOrdnername.Ausfuehren(name, null, Ordner));
    }

    [Theory]
    [InlineData("Alpen")]
    [InlineData("alpen")]
    [InlineData(" ALPEN ")]
    public void Vorhandener_Name_im_selben_Ordner_ergibt_Hinweis(string name)
    {
        Assert.Equal("Einen Ordner „" + name.Trim() + "“ gibt es hier schon.", PruefeOrdnername.Ausfuehren(name, null, Ordner));
    }

    [Fact]
    public void Gleicher_Name_in_einem_anderen_Ordner_ist_erlaubt()
    {
        Assert.Null(PruefeOrdnername.Ausfuehren("Tirol", null, Ordner));
        Assert.Null(PruefeOrdnername.Ausfuehren("Chiemgau", 1, Ordner));
    }

    [Fact]
    public void Neuer_Name_ist_erlaubt()
    {
        Assert.Null(PruefeOrdnername.Ausfuehren("Allgäu", null, Ordner));
    }

    [Fact]
    public void Beim_Umbenennen_ist_der_eigene_Name_erlaubt()
    {
        Assert.Null(PruefeOrdnername.Ausfuehren("ALPEN", null, Ordner, eigeneId: 1));
    }

    [Fact]
    public void Beim_Umbenennen_auf_den_Namen_eines_Nachbarn_kommt_ein_Hinweis()
    {
        Assert.NotNull(PruefeOrdnername.Ausfuehren("Chiemgau", null, Ordner, eigeneId: 1));
    }
}
