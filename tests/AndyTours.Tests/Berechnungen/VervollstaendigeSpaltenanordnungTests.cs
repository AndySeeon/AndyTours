using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="VervollstaendigeSpaltenanordnung"/>.
/// </summary>
public class VervollstaendigeSpaltenanordnungTests
{
    [Fact]
    public void Ohne_gespeicherte_Anordnung_gilt_die_Standardreihenfolge()
    {
        List<Spalteneinstellung> anordnung = VervollstaendigeSpaltenanordnung.Ausfuehren([]);

        Assert.Equal(Enum.GetValues<Einheitenspalte>(), anordnung.Select(a => a.Spalte));
        Assert.All(anordnung, a => Assert.Null(a.Breite));
    }

    [Fact]
    public void Gespeicherte_Reihenfolge_und_Breiten_bleiben_fehlende_kommen_hinten_dazu()
    {
        List<Spalteneinstellung> anordnung = VervollstaendigeSpaltenanordnung.Ausfuehren(
        [
            new(Einheitenspalte.Fahrzeit, 80), new(Einheitenspalte.Name, 250), new(Einheitenspalte.Datum, null),
        ]);

        Assert.Equal(17, anordnung.Count);
        Assert.Equal([new(Einheitenspalte.Fahrzeit, 80), new(Einheitenspalte.Name, 250), new Spalteneinstellung(Einheitenspalte.Datum, null)],
            anordnung.Take(3));
        Assert.Equal(Einheitenspalte.Startzeit, anordnung[3].Spalte); // erste fehlende in Standardreihenfolge
        Assert.Equal(Einheitenspalte.Trainingsbereich, anordnung[^1].Spalte);
    }

    [Fact]
    public void Doppelte_unbekannte_und_unbrauchbare_Eintraege_werden_bereinigt()
    {
        List<Spalteneinstellung> anordnung = VervollstaendigeSpaltenanordnung.Ausfuehren(
        [
            new(Einheitenspalte.Name, 200), new(Einheitenspalte.Name, 300), new((Einheitenspalte)99, 50),
            new(Einheitenspalte.Distanz, 0), new(Einheitenspalte.Fahrzeit, double.NaN),
        ]);

        Assert.Equal(17, anordnung.Select(a => a.Spalte).Distinct().Count());
        Assert.Equal(new Spalteneinstellung(Einheitenspalte.Name, 200), anordnung[0]);
        Assert.Equal(new Spalteneinstellung(Einheitenspalte.Distanz, null), anordnung[1]);
        Assert.Equal(new Spalteneinstellung(Einheitenspalte.Fahrzeit, null), anordnung[2]);
    }
}
