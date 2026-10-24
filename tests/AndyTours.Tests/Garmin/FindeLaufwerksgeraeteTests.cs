using AndyTours.Import.Garmin;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Garmin;

/// <summary>
/// Tests für <see cref="FindeLaufwerksgeraete"/> mit Testordnern als Laufwerkswurzeln.
/// </summary>
public class FindeLaufwerksgeraeteTests
{
    /// <summary>Legt in der Wurzel ein Gerät mit Aufzeichnungsordner und den angegebenen Dateien an.</summary>
    private static void LegeGeraetAn(string wurzel, string? modell, params string[] dateien)
    {
        string ordner = Path.Combine(wurzel, "Garmin", "Activities");
        Directory.CreateDirectory(ordner);
        foreach (string datei in dateien)
        {
            File.WriteAllText(Path.Combine(ordner, datei), datei);
        }

        if (modell is not null)
        {
            File.WriteAllText(Path.Combine(wurzel, "Garmin", "GarminDevice.xml"),
                $"""<?xml version="1.0"?><Device xmlns="http://www.garmin.com/xmlschemas/GarminDevice/v2"><Model><PartNumber>006-B1836-00</PartNumber><Description>{modell}</Description></Model></Device>""");
        }
    }

    [Fact]
    public void Aufzeichnungen_aller_Geraete_mit_Geraetenamen()
    {
        using var temp = new TempOrdner();
        string edge1000 = Path.Combine(temp.Pfad, "E");
        string edge800 = Path.Combine(temp.Pfad, "F");
        LegeGeraetAn(edge1000, "Edge 1000", "2026-09-28-06-20-11.fit", "2026-09-21-06-16-20.fit", "notiz.txt");
        LegeGeraetAn(edge800, "Edge 800", "2014-05-01-10-00-00.fit");

        List<Geraeteaufzeichnung> aufzeichnungen = FindeLaufwerksgeraete.Ausfuehren([edge1000, edge800]);

        Assert.Equal(
            new[]
            {
                ("2026-09-21-06-16-20.fit", "Edge 1000"),
                ("2026-09-28-06-20-11.fit", "Edge 1000"),
                ("2014-05-01-10-00-00.fit", "Edge 800"),
            },
            aufzeichnungen.Select(a => (a.Dateiname, a.Geraet)));
    }

    [Fact]
    public void Laufwerk_ohne_Garmin_Ordner_wird_uebergangen()
    {
        using var temp = new TempOrdner();
        Directory.CreateDirectory(Path.Combine(temp.Pfad, "USB-Stick", "Fotos"));

        Assert.Empty(FindeLaufwerksgeraete.Ausfuehren([Path.Combine(temp.Pfad, "USB-Stick")]));
    }

    [Fact]
    public void Ohne_Geraetebeschreibung_heisst_das_Geraet_nach_dem_Laufwerk()
    {
        using var temp = new TempOrdner();
        string wurzel = Path.Combine(temp.Pfad, "G");
        LegeGeraetAn(wurzel, null, "2026-01-01-10-00-00.fit");

        Geraeteaufzeichnung aufzeichnung = Assert.Single(FindeLaufwerksgeraete.Ausfuehren([wurzel]));
        Assert.Equal($"Garmin ({wurzel})", aufzeichnung.Geraet);
    }

    [Fact]
    public void Kopieren_legt_die_Datei_am_Ziel_ab()
    {
        using var temp = new TempOrdner();
        string wurzel = Path.Combine(temp.Pfad, "E");
        LegeGeraetAn(wurzel, "Edge 1000", "2026-09-28-06-20-11.fit");
        string ziel = Path.Combine(temp.Pfad, "kopie.fit");

        Assert.Single(FindeLaufwerksgeraete.Ausfuehren([wurzel])).KopiereNach(ziel);

        Assert.Equal("2026-09-28-06-20-11.fit", File.ReadAllText(ziel));
    }
}
