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

    [Fact]
    public void Bisheriger_Aufruf_wandelt_um_und_fragt_nach()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren([@"C:\Alt", @"C:\Neu"], AppContext.BaseDirectory);

        Assert.Equal(Programmmodus.Umwandeln, p.Modus);
        Assert.False(p.OhneRueckfrage);
    }

    [Fact]
    public void Erkennen_braucht_genau_einen_Ordner()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren(["--erkennen", @"C:\Alt"], AppContext.BaseDirectory);

        Assert.Equal(Programmmodus.Erkennen, p.Modus);
        Assert.Equal(@"C:\Alt", p.Quellordner);
        Assert.Equal(string.Empty, p.Zielordner);
        Assert.Throws<ArgumentException>(() => LeseAufrufparameter.Ausfuehren(["--erkennen"], AppContext.BaseDirectory));
        Assert.Throws<ArgumentException>(() => LeseAufrufparameter.Ausfuehren(["--erkennen", "a", "b"], AppContext.BaseDirectory));
    }

    [Fact]
    public void Aufzeichnungen_brauchen_Quelle_und_Ziel()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren(["--aufzeichnungen", @"C:\Fit", @"C:\AndyTours"], AppContext.BaseDirectory);

        Assert.Equal(Programmmodus.Aufzeichnungen, p.Modus);
        Assert.Equal(@"C:\Fit", p.Quellordner);
        Assert.Equal(@"C:\AndyTours", p.Zielordner);
        Assert.Throws<ArgumentException>(() => LeseAufrufparameter.Ausfuehren(["--aufzeichnungen", "a"], AppContext.BaseDirectory));
    }

    [Theory]
    [InlineData("--ohne-rueckfrage", "--aufzeichnungen", @"C:\Fit", @"C:\AndyTours")]
    [InlineData("--aufzeichnungen", @"C:\Fit", @"C:\AndyTours", "--ohne-rueckfrage")]
    public void Ohne_Rueckfrage_darf_an_beliebiger_Stelle_stehen(params string[] argumente)
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren(argumente, AppContext.BaseDirectory);

        Assert.Equal(Programmmodus.Aufzeichnungen, p.Modus);
        Assert.True(p.OhneRueckfrage);
        Assert.Equal(@"C:\AndyTours", p.Zielordner);
    }

    [Fact]
    public void Ohne_Rueckfrage_gilt_auch_fuer_die_Umwandlung()
    {
        Aufrufparameter p = LeseAufrufparameter.Ausfuehren([@"C:\Alt", @"C:\Neu", "--ohne-rueckfrage"], AppContext.BaseDirectory);

        Assert.Equal(Programmmodus.Umwandeln, p.Modus);
        Assert.True(p.OhneRueckfrage);
        Assert.Equal(@"C:\Neu", p.Zielordner);
    }

    [Fact]
    public void Unbekannter_Schalter_wird_abgelehnt()
    {
        Assert.Throws<ArgumentException>(() => LeseAufrufparameter.Ausfuehren(["--irgendwas", "a"], AppContext.BaseDirectory));
    }
}
