using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="LegeAufzeichnungAb"/>.
/// </summary>
public class LegeAufzeichnungAbTests
{
    /// <summary>Schreibt eine Quelldatei mit dem angegebenen Inhalt außerhalb des Datenordners.</summary>
    private static string Quelle(TempOrdner temp, string inhalt)
    {
        string pfad = Path.Combine(temp.Pfad, "quelle_" + Guid.NewGuid().ToString("N") + ".fit");
        File.WriteAllText(pfad, inhalt);
        return pfad;
    }

    [Fact]
    public void Aufzeichnung_landet_im_Ordner_ihres_Jahres()
    {
        using var temp = new TempOrdner();
        string datenordner = Path.Combine(temp.Pfad, "Daten");

        string relativ = LegeAufzeichnungAb.Ausfuehren(datenordner, Quelle(temp, "Tour"), "2026-09-26-08-02-49.fit", 2026);

        Assert.Equal("2026/2026-09-26-08-02-49.fit", relativ);
        Assert.Equal("Tour", File.ReadAllText(Path.Combine(datenordner, "2026", "2026-09-26-08-02-49.fit")));
    }

    [Fact]
    public void Andere_Datei_gleichen_Namens_wird_nicht_ueberschrieben()
    {
        using var temp = new TempOrdner();
        string datenordner = Path.Combine(temp.Pfad, "Daten");
        LegeAufzeichnungAb.Ausfuehren(datenordner, Quelle(temp, "alt"), "tour.fit", 2026);

        string relativ = LegeAufzeichnungAb.Ausfuehren(datenordner, Quelle(temp, "neu"), "tour.fit", 2026);

        Assert.Equal("2026/tour_2.fit", relativ);
        Assert.Equal("alt", File.ReadAllText(Path.Combine(datenordner, "2026", "tour.fit")));
        Assert.Equal("neu", File.ReadAllText(Path.Combine(datenordner, "2026", "tour_2.fit")));
    }

    [Fact]
    public void Gleiche_Datei_wird_wiederverwendet()
    {
        using var temp = new TempOrdner();
        string datenordner = Path.Combine(temp.Pfad, "Daten");
        LegeAufzeichnungAb.Ausfuehren(datenordner, Quelle(temp, "Tour"), "tour.fit", 2026);

        string relativ = LegeAufzeichnungAb.Ausfuehren(datenordner, Quelle(temp, "Tour"), "tour.fit", 2026);

        Assert.Equal("2026/tour.fit", relativ);
        Assert.Single(Directory.GetFiles(Path.Combine(datenordner, "2026")));
    }
}
