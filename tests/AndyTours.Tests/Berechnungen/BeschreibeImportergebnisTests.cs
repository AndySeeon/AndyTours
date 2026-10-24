using AndyTours.Core.Berechnungen;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BeschreibeImportergebnis"/>.
/// </summary>
public class BeschreibeImportergebnisTests
{
    [Fact]
    public void Anzahl_in_der_Statusleiste()
    {
        Assert.Equal("3 Touren importiert.", BeschreibeImportergebnis.Ausfuehren(3, []).Statustext);
        Assert.Equal("1 Tour importiert.", BeschreibeImportergebnis.Ausfuehren(1, []).Statustext);
        Assert.Equal("0 Touren importiert.", BeschreibeImportergebnis.Ausfuehren(0, []).Statustext);
    }

    [Fact]
    public void Ohne_uebersprungene_Aufzeichnungen_keine_Meldung()
    {
        Assert.Null(BeschreibeImportergebnis.Ausfuehren(3, []).Meldung);
    }

    [Fact]
    public void Uebersprungene_Aufzeichnungen_werden_genannt()
    {
        (string statustext, string? meldung) = BeschreibeImportergebnis.Ausfuehren(2, ["a.fit – Datei beschädigt", "b.fit – Datei beschädigt"]);

        Assert.Equal("2 Touren importiert.", statustext);
        Assert.Equal("Diese Aufzeichnungen konnten nicht gelesen werden und wurden übersprungen:\n\na.fit – Datei beschädigt\nb.fit – Datei beschädigt", meldung);
    }
}
