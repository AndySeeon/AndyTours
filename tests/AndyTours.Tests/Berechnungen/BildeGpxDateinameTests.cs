using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BildeGpxDateiname"/>.
/// </summary>
public class BildeGpxDateinameTests
{
    private static Etappe Etappe(int tag, string name) =>
        new(new Trainingseinheit { Datum = new DateOnly(2026, 5, tag), Name = name }, null);

    [Fact]
    public void Einzelne_Einheit_Datum_und_Name()
    {
        Assert.Equal("2026-05-01 Bründlingalm.gpx", BildeGpxDateiname.Ausfuehren([Etappe(1, "Bründlingalm")]));
    }

    [Fact]
    public void Mehrtagestour_erstes_und_letztes_Datum_und_Name_der_ersten_Etappe()
    {
        Etappe[] etappen = [Etappe(1, "Alpencross"), Etappe(2, "Tag 2"), Etappe(4, "Tag 4")];

        Assert.Equal("2026-05-01 bis 2026-05-04 Alpencross.gpx", BildeGpxDateiname.Ausfuehren(etappen));
    }

    [Fact]
    public void Mehrere_Etappen_am_selben_Tag_nur_ein_Datum()
    {
        Assert.Equal("2026-05-03 Vormittag.gpx", BildeGpxDateiname.Ausfuehren([Etappe(3, "Vormittag"), Etappe(3, "Nachmittag")]));
    }

    [Fact]
    public void Bearbeitete_Strecke_traegt_den_Zusatz()
    {
        Assert.Equal("2026-05-01 Bründlingalm (bearbeitet).gpx", BildeGpxDateiname.Ausfuehren([Etappe(1, "Bründlingalm")], bearbeitet: true));
        Assert.Equal("2026-05-01 (bearbeitet).gpx", BildeGpxDateiname.Ausfuehren([Etappe(1, "")], bearbeitet: true));
    }

    [Theory]
    [InlineData("Hin/Zurück", "2026-05-01 Hin-Zurück.gpx")]
    [InlineData("a\\b:c*d?e\"f<g>h|i", "2026-05-01 a-b-c-d-e-f-g-h-i.gpx")]
    [InlineData("  Runde  ", "2026-05-01 Runde.gpx")]
    [InlineData("", "2026-05-01.gpx")]
    public void Unzulaessige_Zeichen_werden_ersetzt(string name, string erwartet)
    {
        Assert.Equal(erwartet, BildeGpxDateiname.Ausfuehren([Etappe(1, name)]));
    }
}
