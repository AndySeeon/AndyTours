using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BildeCoursename"/>.
/// </summary>
public class BildeCoursenameTests
{
    private static readonly DateOnly Heute = new(2026, 10, 26);

    private static Etappe Etappe(int tag, string name) =>
        new(new Trainingseinheit { Datum = new DateOnly(2026, 5, tag), Name = name }, null);

    [Fact]
    public void Gespeicherte_Planung_heisst_wie_ihre_Datei()
    {
        Assert.Equal("Chiemsee-Runde", BildeCoursename.FuerPlanung(@"C:\Touren\Chiemsee-Runde.gpx", Heute));
    }

    [Fact]
    public void Ungespeicherte_Planung_heisst_Planung_mit_Datum()
    {
        Assert.Equal("Planung 2026-10-26", BildeCoursename.FuerPlanung(null, Heute));
    }

    [Fact]
    public void Etappe_heisst_nach_Datum_und_Name()
    {
        Assert.Equal("2026-05-01 Bründlingalm", BildeCoursename.FuerEtappe(Etappe(1, "Bründlingalm")));
        Assert.Equal("2026-05-02", BildeCoursename.FuerEtappe(Etappe(2, "")));
    }

    [Fact]
    public void Bearbeitete_Etappe_traegt_den_Zusatz()
    {
        Assert.Equal("2026-05-01 Bründlingalm (bearbeitet)", BildeCoursename.FuerEtappe(Etappe(1, "Bründlingalm"), bearbeitet: true));
    }

    [Theory]
    [InlineData("2026-05-01 Bründlingalm", "2026-05-01 Bründlingalm.fit")]
    [InlineData("  Hin/zurück: Alm?  ", "Hin-zurück- Alm-.fit")]
    [InlineData("Runde...", "Runde.fit")]
    public void Dateiname_auf_dem_Geraet(string name, string dateiname)
    {
        Assert.Equal(dateiname, BildeCoursename.Dateiname(name));
    }
}
