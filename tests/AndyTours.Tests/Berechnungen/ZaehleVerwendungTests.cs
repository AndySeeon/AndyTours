using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ZaehleVerwendung"/> mit dem Bestand aus <see cref="Auswahlbestand"/>.
/// </summary>
public class ZaehleVerwendungTests
{
    [Theory]
    [InlineData(1, 3, 1)]
    [InlineData(2, 1, 1)]
    [InlineData(3, 0, 0)]
    public void Zaehlt_Einheiten_und_Equipments_einer_Aktivitaet(long id, int einheiten, int equipments)
    {
        Assert.Equal((einheiten, equipments), ZaehleVerwendung.Aktivitaet(Auswahlbestand.Erzeugen(), id));
    }

    [Fact]
    public void Zaehlt_Einheiten_einer_Kategorie_und_nennt_Equipments_ohne_Unterschied_der_Schreibweise()
    {
        (int einheiten, List<string> equipments) = ZaehleVerwendung.Kategorie(Auswahlbestand.Erzeugen(), 2);

        Assert.Equal(2, einheiten);
        Assert.Equal(["Genius"], equipments);
    }

    [Fact]
    public void Unbenutzte_Kategorie_hat_keine_Verwendung()
    {
        (int einheiten, List<string> equipments) = ZaehleVerwendung.Kategorie(Auswahlbestand.Erzeugen(), 4);

        Assert.Equal(0, einheiten);
        Assert.Empty(equipments);
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(4, 1, 0)]
    [InlineData(5, 0, 2)]
    [InlineData(7, 0, 0)]
    public void Zaehlt_Einheiten_und_Wartungen_eines_Equipments(long id, int einheiten, int wartungen)
    {
        Assert.Equal((einheiten, wartungen), ZaehleVerwendung.Equipment(Auswahlbestand.MitEquipments(), id));
    }
}
