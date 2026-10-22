using AndyTours.Altdatenuebernahme;
using AndyTours.Daten.Datenordner;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests für <see cref="LeseAufrufparameter"/>.
/// </summary>
public class LeseAufrufparameterTests
{
    /// <summary>Der echte Projektordner, gefunden vom Ausgabeordner der Tests aus.</summary>
    private static string Projektordner => FindeProjektordner.Ausfuehren(AppContext.BaseDirectory)!;

    [Fact]
    public void Beide_Angaben_werden_als_vollstaendige_Pfade_uebernommen()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren([@"C:\Alt\BikeXperience", @"\\NAS\AndyTours"], AppContext.BaseDirectory);

        Assert.Equal(@"C:\Alt\BikeXperience", p.Quellordner);
        Assert.Equal(@"\\NAS\AndyTours", p.Zielordner);
    }

    [Fact]
    public void Ohne_Zielordner_wird_AppDaten_im_Projektordner_verwendet()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren([@"C:\Alt\BikeXperience"], AppContext.BaseDirectory);

        Assert.Equal(@"C:\Alt\BikeXperience", p.Quellordner);
        Assert.Equal(Path.Combine(Projektordner, "AppDaten"), p.Zielordner);
    }

    [Fact]
    public void Ohne_Angaben_wird_von_Daten_nach_AppDaten_umgewandelt()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren([], AppContext.BaseDirectory);

        Assert.Equal(Path.Combine(Projektordner, "Daten"), p.Quellordner);
        Assert.Equal(Path.Combine(Projektordner, "AppDaten"), p.Zielordner);
    }

    [Fact]
    public void Mehr_als_zwei_Angaben_werden_abgelehnt()
    {
        Assert.Throws<ArgumentException>(() => LeseAufrufparameter.Ausfuehren(["a", "b", "c"], AppContext.BaseDirectory));
    }

    [Fact]
    public void Vorgabe_ohne_Projektordner_meldet_einen_Fehler()
    {
        using var temp = new TempOrdner();

        Assert.Throws<InvalidOperationException>(() => LeseAufrufparameter.Ausfuehren([], temp.Pfad));
    }
}
