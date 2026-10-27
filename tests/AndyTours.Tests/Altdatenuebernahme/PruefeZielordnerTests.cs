using AndyTours.Altdatenuebernahme;
using AndyTours.Daten.Datenordner;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests für <see cref="PruefeZielordner"/>.
/// </summary>
public class PruefeZielordnerTests
{
    [Fact]
    public void Fehlender_Ordner_ist_frei()
    {
        using var temp = new TempOrdner();

        Assert.Equal(ZielordnerZustand.FehltOderLeer, PruefeZielordner.Ausfuehren(Path.Combine(temp.Pfad, "neu")));
    }

    [Fact]
    public void Leerer_Ordner_ist_frei()
    {
        using var temp = new TempOrdner();

        Assert.Equal(ZielordnerZustand.FehltOderLeer, PruefeZielordner.Ausfuehren(temp.Pfad));
    }

    [Fact]
    public void Ordner_mit_Datenbank_hat_Daten()
    {
        using var temp = new TempOrdner();
        File.WriteAllText(Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei), "");

        Assert.Equal(ZielordnerZustand.MitDaten, PruefeZielordner.Ausfuehren(temp.Pfad));
    }

    [Fact]
    public void Ordner_mit_anderen_Dateien_hat_fremden_Inhalt()
    {
        using var temp = new TempOrdner();
        File.WriteAllText(Path.Combine(temp.Pfad, "Urlaubsfotos.txt"), "");

        Assert.Equal(ZielordnerZustand.FremderInhalt, PruefeZielordner.Ausfuehren(temp.Pfad));
    }
}
