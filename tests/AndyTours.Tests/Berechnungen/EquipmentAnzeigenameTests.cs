using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="EquipmentAnzeigename"/>.
/// </summary>
public class EquipmentAnzeigenameTests
{
    [Fact]
    public void Ausgemustertes_Equipment_traegt_den_Zusatz_der_Name_bleibt()
    {
        var equipment = new Equipment { Name = "Namur", Ausgemustert = true };

        Assert.Equal("Namur (ausgemustert)", EquipmentAnzeigename.Bilden(equipment));
        Assert.Equal("Namur", equipment.Name);
    }

    [Fact]
    public void Equipment_im_Einsatz_ohne_Zusatz()
    {
        Assert.Equal("Genius", EquipmentAnzeigename.Bilden(new Equipment { Name = "Genius" }));
    }

    [Theory]
    [InlineData("Namur (ausgemustert)", true, true)]  // angezeigter Name
    [InlineData("Namur", true, true)]                 // gespeicherter Name (z. B. Filter von vorher)
    [InlineData("Namur (ausgemustert)", false, true)]  // wieder im Einsatz: Wert von vorher passt weiter
    [InlineData("Genius", true, false)]
    [InlineData(null, true, false)]
    public void Passt_zum_angezeigten_und_zum_gespeicherten_Namen(string? name, bool ausgemustert, bool erwartet)
    {
        var equipment = new Equipment { Name = "Namur", Ausgemustert = ausgemustert };

        Assert.Equal(erwartet, EquipmentAnzeigename.Passt(equipment, name));
    }
}
