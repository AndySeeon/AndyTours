using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="ErmittleStartDatenordner"/>.
/// </summary>
public class ErmittleStartDatenordnerTests
{
    private const string Standardordner = @"C:\Users\Test\Documents\AndyTours";

    [Fact]
    public void Erster_Start_verwendet_den_Standardordner()
    {
        string ordner = ErmittleStartDatenordner.Ausfuehren(false, @"C:\Programme\AndyTours", null, Standardordner);

        Assert.Equal(Standardordner, ordner);
    }

    [Fact]
    public void Gemerkter_Ordner_hat_Vorrang_vor_dem_Standardordner()
    {
        string ordner = ErmittleStartDatenordner.Ausfuehren(false, @"C:\Programme\AndyTours", @"\\NAS\AndyTours", Standardordner);

        Assert.Equal(@"\\NAS\AndyTours", ordner);
    }

    [Fact]
    public void Entwicklung_verwendet_immer_AppDaten_im_Projektordner()
    {
        using var temp = new TempOrdner();
        File.WriteAllText(Path.Combine(temp.Pfad, DatenordnerInhalt.Projektmappe), "<Solution />");
        string programmordner = Path.Combine(temp.Pfad, "src", "AndyTours.App", "bin", "Debug");
        Directory.CreateDirectory(programmordner);

        // Auch ein gemerkter Ordner der fertigen App ändert daran nichts
        string ordner = ErmittleStartDatenordner.Ausfuehren(true, programmordner, @"\\NAS\AndyTours", Standardordner);

        Assert.Equal(Path.Combine(temp.Pfad, "AppDaten"), ordner);
    }

    [Fact]
    public void Entwicklung_ohne_Projektordner_meldet_einen_Fehler()
    {
        using var temp = new TempOrdner();

        Assert.Throws<InvalidOperationException>(
            () => ErmittleStartDatenordner.Ausfuehren(true, temp.Pfad, null, Standardordner));
    }
}
