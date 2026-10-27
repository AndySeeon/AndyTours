using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleAuswahljahre"/>.
/// </summary>
public class ErmittleAuswahljahreTests
{
    /// <summary>Erzeugt eine Einheit an einem Tag des angegebenen Jahres.</summary>
    private static Trainingseinheit Einheit(int jahr) => new() { Datum = new DateOnly(jahr, 6, 1) };

    [Fact]
    public void Jahre_stehen_ohne_Doppelte_und_das_neueste_zuerst()
    {
        List<int> jahre = ErmittleAuswahljahre.Ausfuehren([Einheit(2017), Einheit(2019), Einheit(2017), Einheit(2018)], 2019);

        Assert.Equal([2019, 2018, 2017], jahre);
    }

    [Fact]
    public void Aktuelles_Jahr_ohne_Einheiten_wird_ergaenzt_und_steht_vorne()
    {
        List<int> jahre = ErmittleAuswahljahre.Ausfuehren([Einheit(2024), Einheit(2025)], 2026);

        Assert.Equal([2026, 2025, 2024], jahre);
    }

    [Fact]
    public void Ohne_Einheiten_gibt_es_nur_das_aktuelle_Jahr()
    {
        Assert.Equal([2026], ErmittleAuswahljahre.Ausfuehren([], 2026));
    }
}
