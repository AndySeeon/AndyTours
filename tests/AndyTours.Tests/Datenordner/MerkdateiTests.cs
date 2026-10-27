using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für das Zusammenspiel von <see cref="DatenordnerMerkdatei.Speichern"/> und <see cref="DatenordnerMerkdatei.Laden"/>.
/// </summary>
public class MerkdateiTests
{
    [Fact]
    public void Gemerkter_Ordner_wird_wieder_gelesen()
    {
        using var temp = new TempOrdner();
        // Merkdatei in einem noch nicht vorhandenen Unterordner – wie %APPDATA%\AndyTours beim ersten Start
        string merkdatei = Path.Combine(temp.Pfad, "AndyTours", "datenordner.txt");

        DatenordnerMerkdatei.Speichern(merkdatei, @"\\NAS\Daten\AndyTours");

        Assert.Equal(@"\\NAS\Daten\AndyTours", DatenordnerMerkdatei.Laden(merkdatei));
    }

    [Fact]
    public void Neuer_Ordner_ersetzt_den_alten()
    {
        using var temp = new TempOrdner();
        string merkdatei = Path.Combine(temp.Pfad, "datenordner.txt");

        DatenordnerMerkdatei.Speichern(merkdatei, @"C:\Alt");
        DatenordnerMerkdatei.Speichern(merkdatei, @"D:\Neu");

        Assert.Equal(@"D:\Neu", DatenordnerMerkdatei.Laden(merkdatei));
    }

    [Fact]
    public void Ohne_Merkdatei_wird_null_geliefert()
    {
        using var temp = new TempOrdner();

        Assert.Null(DatenordnerMerkdatei.Laden(Path.Combine(temp.Pfad, "fehlt.txt")));
    }

    [Fact]
    public void Leere_Merkdatei_gilt_als_nicht_gemerkt()
    {
        using var temp = new TempOrdner();
        string merkdatei = Path.Combine(temp.Pfad, "datenordner.txt");
        File.WriteAllText(merkdatei, "  \r\n");

        Assert.Null(DatenordnerMerkdatei.Laden(merkdatei));
    }

    [Fact]
    public void Nicht_vorhandener_Ordner_wird_trotzdem_geliefert()
    {
        // Wichtig für „NAS ausgeschaltet“: Der Pfad muss beim Start als nicht erreichbar gemeldet werden können.
        using var temp = new TempOrdner();
        string merkdatei = Path.Combine(temp.Pfad, "datenordner.txt");
        DatenordnerMerkdatei.Speichern(merkdatei, @"Q:\GibtEsNicht\AndyTours");

        Assert.Equal(@"Q:\GibtEsNicht\AndyTours", DatenordnerMerkdatei.Laden(merkdatei));
    }
}
