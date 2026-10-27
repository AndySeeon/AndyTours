using AndyTours.Import.Altdaten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Altdaten;

/// <summary>
/// Tests für <see cref="ErkenneAltdaten"/>.
/// </summary>
public class ErkenneAltdatenTests
{
    /// <summary>Legt eine leere Datei an, samt fehlender Ordner.</summary>
    private static string LegeAn(string ordner, string relativerPfad)
    {
        string pfad = Path.Combine(ordner, relativerPfad);
        Directory.CreateDirectory(Path.GetDirectoryName(pfad)!);
        File.WriteAllBytes(pfad, []);
        return pfad;
    }

    [Fact]
    public void Ini_und_Workouts_ergeben_BikeXperience()
    {
        using var temp = new TempOrdner();
        LegeAn(temp.Pfad, "bikexperience.ini");
        LegeAn(temp.Pfad, "workouts.csv");
        LegeAn(temp.Pfad, Path.Combine("2016", "a.fit"));

        Altdatenfund fund = ErkenneAltdaten.Ausfuehren(temp.Pfad);

        Assert.Equal(Altdatenart.BikeXperience, fund.Art);
        Assert.Empty(fund.Aufzeichnungen);
    }

    [Fact]
    public void Nur_Ini_ohne_Workouts_ist_kein_BikeXperience_Ordner()
    {
        using var temp = new TempOrdner();
        LegeAn(temp.Pfad, "bikexperience.ini");
        LegeAn(temp.Pfad, "a.fit");

        Assert.Equal(Altdatenart.NurAufzeichnungen, ErkenneAltdaten.Ausfuehren(temp.Pfad).Art);
    }

    [Fact]
    public void Aufzeichnungen_werden_auch_in_Unterordnern_gefunden_und_gezaehlt()
    {
        using var temp = new TempOrdner();
        string a = LegeAn(temp.Pfad, "a.fit");
        string b = LegeAn(temp.Pfad, Path.Combine("2016", "b.FIT"));
        string c = LegeAn(temp.Pfad, Path.Combine("2017", "Juni", "c.fit"));
        LegeAn(temp.Pfad, "notizen.txt");
        LegeAn(temp.Pfad, "d.fitx");

        Altdatenfund fund = ErkenneAltdaten.Ausfuehren(temp.Pfad);

        Assert.Equal(Altdatenart.NurAufzeichnungen, fund.Art);
        Assert.Equal(new[] { a, b, c }.Order(StringComparer.OrdinalIgnoreCase), fund.Aufzeichnungen);
    }

    [Fact]
    public void Ordner_ohne_Aufzeichnungen_ergibt_Nichts()
    {
        using var temp = new TempOrdner();
        LegeAn(temp.Pfad, Path.Combine("Fotos", "bild.jpg"));

        Altdatenfund fund = ErkenneAltdaten.Ausfuehren(temp.Pfad);

        Assert.Equal(Altdatenart.Nichts, fund.Art);
        Assert.Empty(fund.Aufzeichnungen);
    }

    [Fact]
    public void Fehlender_Ordner_ergibt_Nichts()
    {
        using var temp = new TempOrdner();

        Assert.Equal(Altdatenart.Nichts, ErkenneAltdaten.Ausfuehren(Path.Combine(temp.Pfad, "gibt-es-nicht")).Art);
    }

    [Fact]
    public void Der_Ordner_bleibt_unveraendert()
    {
        using var temp = new TempOrdner();
        LegeAn(temp.Pfad, Path.Combine("2016", "a.fit"));
        string[] vorher = Directory.GetFileSystemEntries(temp.Pfad, "*", SearchOption.AllDirectories);

        ErkenneAltdaten.Ausfuehren(temp.Pfad);

        Assert.Equal(vorher, Directory.GetFileSystemEntries(temp.Pfad, "*", SearchOption.AllDirectories));
    }
}
