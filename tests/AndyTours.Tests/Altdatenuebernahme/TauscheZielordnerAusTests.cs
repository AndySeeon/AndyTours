using AndyTours.Altdatenuebernahme;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests für <see cref="TauscheZielordnerAus"/>.
/// </summary>
public class TauscheZielordnerAusTests
{
    /// <summary>Legt einen Ordner mit einer Datei an.</summary>
    private static string OrdnerMitDatei(string pfad, string dateiname, string inhalt)
    {
        Directory.CreateDirectory(pfad);
        File.WriteAllText(Path.Combine(pfad, dateiname), inhalt);
        return pfad;
    }

    [Fact]
    public void Fehlender_Zielordner_wird_durch_den_Zwischenordner_ersetzt()
    {
        using var temp = new TempOrdner();
        string zwischen = OrdnerMitDatei(Path.Combine(temp.Pfad, ".Ziel.umwandlung"), "neu.txt", "neu");
        string ziel = Path.Combine(temp.Pfad, "Ziel");

        Assert.Null(TauscheZielordnerAus.Ausfuehren(zwischen, ziel));

        Assert.Equal("neu", File.ReadAllText(Path.Combine(ziel, "neu.txt")));
        Assert.False(Directory.Exists(zwischen));
    }

    [Fact]
    public void Vorhandener_Zielordner_enthaelt_danach_nur_die_neuen_Daten()
    {
        using var temp = new TempOrdner();
        string ziel = OrdnerMitDatei(Path.Combine(temp.Pfad, "Ziel"), "alt.txt", "alt");
        string zwischen = OrdnerMitDatei(Path.Combine(temp.Pfad, ".Ziel.umwandlung"), "neu.txt", "neu");

        Assert.Null(TauscheZielordnerAus.Ausfuehren(zwischen, ziel));

        Assert.Equal(["neu.txt"], Directory.GetFiles(ziel).Select(Path.GetFileName));
        // Neben dem Ziel bleibt nichts zurück (kein Zwischenordner, kein beiseitegelegter alter Ordner)
        Assert.Equal([ziel], Directory.GetDirectories(temp.Pfad));
    }

    [Fact]
    public void Geoeffnete_Datei_im_Ziel_laesst_den_Zielordner_unveraendert()
    {
        using var temp = new TempOrdner();
        string ziel = OrdnerMitDatei(Path.Combine(temp.Pfad, "Ziel"), "alt.txt", "alt");
        string zwischen = OrdnerMitDatei(Path.Combine(temp.Pfad, ".Ziel.umwandlung"), "neu.txt", "neu");

        // Eine geöffnete Datei verhindert unter Windows das Umbenennen des Ordners (wie ein laufendes AndyTours)
        using (File.Open(Path.Combine(ziel, "alt.txt"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.ThrowsAny<IOException>(() => TauscheZielordnerAus.Ausfuehren(zwischen, ziel));
        }

        Assert.Equal("alt", File.ReadAllText(Path.Combine(ziel, "alt.txt")));
        Assert.Equal([ziel], Directory.GetDirectories(temp.Pfad));
    }
}
