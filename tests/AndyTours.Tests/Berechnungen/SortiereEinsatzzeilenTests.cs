using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="SortiereEinsatzzeilen"/>.
/// </summary>
public class SortiereEinsatzzeilenTests
{
    /// <summary>Drei Equipments, „ohne Equipment“ und „Summe“ in Anfangsreihenfolge.</summary>
    private static List<Einsatzzeile> Zeilen() =>
    [
        new("Stevens", null, 3000, 20000),
        new("genius", 1000, 1500, 40000),
        new("Namur (ausgemustert)", 200, 8000, null),
        new("ohne Equipment", null, 50, 100),
        new("Summe", 1200, 12550, 60100, IstSumme: true),
    ];

    private static IEnumerable<string> Namen(Einsatzspalte spalte, bool aufsteigend) =>
        SortiereEinsatzzeilen.Ausfuehren(Zeilen(), spalte, aufsteigend).Select(z => z.Name);

    [Fact]
    public void Zahlenspalte_nach_Zahlenwert_Summe_bleibt_unten()
    {
        Assert.Equal(["ohne Equipment", "genius", "Stevens", "Namur (ausgemustert)", "Summe"], Namen(Einsatzspalte.GesamtKm, true));
        Assert.Equal(["Namur (ausgemustert)", "Stevens", "genius", "ohne Equipment", "Summe"], Namen(Einsatzspalte.GesamtKm, false));
    }

    [Fact]
    public void Leere_Zelle_zaehlt_als_0()
    {
        Assert.Equal(["Namur (ausgemustert)", "ohne Equipment", "Stevens", "genius", "Summe"], Namen(Einsatzspalte.GesamtHm, true));
        Assert.Equal(["genius", "Namur (ausgemustert)", "Stevens", "ohne Equipment", "Summe"], Namen(Einsatzspalte.VorherKm, false));
    }

    [Fact]
    public void Equipment_alphabetisch_ohne_Gross_und_Kleinschreibung()
    {
        Assert.Equal(["genius", "Namur (ausgemustert)", "ohne Equipment", "Stevens", "Summe"], Namen(Einsatzspalte.Equipment, true));
        Assert.Equal(["Stevens", "ohne Equipment", "Namur (ausgemustert)", "genius", "Summe"], Namen(Einsatzspalte.Equipment, false));
    }

    [Fact]
    public void Leere_Liste_bleibt_leer()
    {
        Assert.Empty(SortiereEinsatzzeilen.Ausfuehren([], Einsatzspalte.GesamtKm, false));
    }
}
