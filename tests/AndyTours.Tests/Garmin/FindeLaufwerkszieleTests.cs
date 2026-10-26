using AndyTours.Import.Garmin;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Tests für <see cref="FindeLaufwerksziele"/> mit Testordnern als Laufwerkswurzeln.
/// </summary>
public class FindeLaufwerkszieleTests
{
    /// <summary>Legt in der Wurzel ein Gerät mit Aufzeichnungsordner und optional Gerätebeschreibung an.</summary>
    private static string LegeGeraetAn(string wurzel, string? modell)
    {
        Directory.CreateDirectory(Path.Combine(wurzel, "Garmin", "Activities"));
        if (modell is not null)
        {
            File.WriteAllText(Path.Combine(wurzel, "Garmin", "GarminDevice.xml"),
                $"""<?xml version="1.0"?><Device xmlns="http://www.garmin.com/xmlschemas/GarminDevice/v2"><Model><Description>{modell}</Description></Model></Device>""");
        }

        return wurzel;
    }

    [Fact]
    public void Alle_Garmin_Laufwerke_mit_Geraetenamen()
    {
        using var temp = new TempOrdner();
        string edge1000 = LegeGeraetAn(Path.Combine(temp.Pfad, "E"), "Edge 1000");
        string edge800 = LegeGeraetAn(Path.Combine(temp.Pfad, "F"), null);
        string stick = Path.Combine(temp.Pfad, "G");
        Directory.CreateDirectory(Path.Combine(stick, "Fotos"));

        List<Courseziel> ziele = FindeLaufwerksziele.Ausfuehren([edge1000, edge800, stick]);

        Assert.Equal(["Edge 1000", $"Garmin ({edge800})"], ziele.Select(z => z.Geraet));
    }

    [Fact]
    public void Schreiben_legt_die_Datei_in_NewFiles_an()
    {
        using var temp = new TempOrdner();
        string wurzel = LegeGeraetAn(Path.Combine(temp.Pfad, "E"), "Edge 800");
        Courseziel ziel = Assert.Single(FindeLaufwerksziele.Ausfuehren([wurzel]));

        ziel.Schreibe("Runde.fit", [1, 2, 3]);

        Assert.Equal([1, 2, 3], File.ReadAllBytes(Path.Combine(wurzel, "Garmin", "NewFiles", "Runde.fit")));
    }

    [Fact]
    public void Wartende_Datei_gleichen_Namens_wird_erkannt_und_ersetzt()
    {
        using var temp = new TempOrdner();
        string wurzel = LegeGeraetAn(Path.Combine(temp.Pfad, "E"), "Edge 1000");
        Courseziel ziel = Assert.Single(FindeLaufwerksziele.Ausfuehren([wurzel]));

        Assert.False(ziel.DateiVorhanden("Runde.fit"));
        ziel.Schreibe("Runde.fit", [1]);
        Assert.True(ziel.DateiVorhanden("Runde.fit"));
        Assert.False(ziel.DateiVorhanden("Andere.fit"));

        ziel.Schreibe("Runde.fit", [2]);

        Assert.Equal([2], File.ReadAllBytes(Path.Combine(wurzel, "Garmin", "NewFiles", "Runde.fit")));
    }
}
