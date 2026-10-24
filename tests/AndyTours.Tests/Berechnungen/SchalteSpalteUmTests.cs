using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="SchalteSpalteUm"/>.
/// </summary>
public class SchalteSpalteUmTests
{
    [Fact]
    public void Sichtbare_Spalte_wird_ausgeblendet()
    {
        HashSet<Einheitenspalte> neu = SchalteSpalteUm.Ausfuehren(new HashSet<Einheitenspalte>(), Einheitenspalte.Trittfrequenz);

        Assert.Equal([Einheitenspalte.Trittfrequenz], neu);
    }

    [Fact]
    public void Ausgeblendete_Spalte_wird_wieder_sichtbar()
    {
        var ausgeblendet = new HashSet<Einheitenspalte> { Einheitenspalte.Trittfrequenz, Einheitenspalte.Temperatur };

        HashSet<Einheitenspalte> neu = SchalteSpalteUm.Ausfuehren(ausgeblendet, Einheitenspalte.Trittfrequenz);

        Assert.Equal([Einheitenspalte.Temperatur], neu);
        Assert.Equal(2, ausgeblendet.Count); // Eingabe unverändert
    }

    [Fact]
    public void Letzte_sichtbare_Spalte_laesst_sich_nicht_ausblenden()
    {
        var alleAusserName = Enum.GetValues<Einheitenspalte>().Where(s => s != Einheitenspalte.Name).ToHashSet();

        HashSet<Einheitenspalte> neu = SchalteSpalteUm.Ausfuehren(alleAusserName, Einheitenspalte.Name);

        Assert.Equal(alleAusserName, neu);
        Assert.DoesNotContain(Einheitenspalte.Name, neu);
    }

    [Fact]
    public void Vorletzte_sichtbare_Spalte_laesst_sich_ausblenden()
    {
        var ausgeblendet = Enum.GetValues<Einheitenspalte>()
            .Where(s => s is not Einheitenspalte.Name and not Einheitenspalte.Datum).ToHashSet();

        HashSet<Einheitenspalte> neu = SchalteSpalteUm.Ausfuehren(ausgeblendet, Einheitenspalte.Datum);

        Assert.Contains(Einheitenspalte.Datum, neu);
        Assert.Equal(16, neu.Count);
    }

    [Fact]
    public void Es_gibt_17_Spalten()
    {
        Assert.Equal(17, Enum.GetValues<Einheitenspalte>().Length);
    }
}
