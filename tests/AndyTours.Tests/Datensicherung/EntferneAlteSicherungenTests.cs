using System.Globalization;
using AndyTours.Daten.Datensicherung;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="EntferneAlteSicherungen"/>.
/// </summary>
public class EntferneAlteSicherungenTests
{
    /// <summary>Legt eine leere Datei mit dem angegebenen Namen an.</summary>
    private static void Lege(string ordner, string name) => File.WriteAllText(Path.Combine(ordner, name), "");

    /// <summary>Name einer Sicherung vom 1. Oktober 2026 um 8 Uhr plus <paramref name="tage"/> Tage.</summary>
    private static string Sicherung(int tage) => "andytours_" + new DateTime(2026, 10, 1, 8, 0, 0).AddDays(tage).ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture) + ".db";

    [Fact]
    public void Die_aeltesten_ueber_der_Hoechstzahl_werden_geloescht()
    {
        using var temp = new TempOrdner();
        for (int tag = 0; tag < 12; tag++)
        {
            Lege(temp.Pfad, Sicherung(tag));
        }

        int geloescht = EntferneAlteSicherungen.Ausfuehren(temp.Pfad, 10);

        Assert.Equal(2, geloescht);
        Assert.False(File.Exists(Path.Combine(temp.Pfad, Sicherung(0))));
        Assert.False(File.Exists(Path.Combine(temp.Pfad, Sicherung(1))));
        Assert.Equal(10, Directory.GetFiles(temp.Pfad).Length);
    }

    [Fact]
    public void Andere_und_aehnlich_benannte_Dateien_bleiben_unberuehrt()
    {
        using var temp = new TempOrdner();
        string[] fremde =
        [
            "andytours.db", "andytours_alt.db", "andytours_2026-01-01.db", "andytours_2020-01-01_08-00-00.db.tmp",
            "andytours_2020-01-01_08-00-00.dbx", "Kopie andytours_2020-01-01_08-00-00.db", "notizen.txt",
        ];
        foreach (string name in fremde)
        {
            Lege(temp.Pfad, name);
        }

        Lege(temp.Pfad, Sicherung(0));
        Lege(temp.Pfad, Sicherung(1));

        EntferneAlteSicherungen.Ausfuehren(temp.Pfad, 1);

        Assert.All(fremde, name => Assert.True(File.Exists(Path.Combine(temp.Pfad, name)), name));
        Assert.True(File.Exists(Path.Combine(temp.Pfad, Sicherung(1))));
        Assert.False(File.Exists(Path.Combine(temp.Pfad, Sicherung(0))));
    }

    [Fact]
    public void Alter_ergibt_sich_aus_dem_Namen_nicht_aus_dem_Dateidatum()
    {
        using var temp = new TempOrdner();
        Lege(temp.Pfad, Sicherung(5));
        Lege(temp.Pfad, Sicherung(0));
        // Die ältere Sicherung wurde zuletzt geschrieben (z. B. beim Kopieren des Ordners)
        File.SetLastWriteTime(Path.Combine(temp.Pfad, Sicherung(0)), DateTime.Now.AddDays(1));

        EntferneAlteSicherungen.Ausfuehren(temp.Pfad, 1);

        Assert.True(File.Exists(Path.Combine(temp.Pfad, Sicherung(5))));
        Assert.False(File.Exists(Path.Combine(temp.Pfad, Sicherung(0))));
    }

    [Fact]
    public void Unter_der_Hoechstzahl_wird_nichts_geloescht()
    {
        using var temp = new TempOrdner();
        Lege(temp.Pfad, Sicherung(0));
        Lege(temp.Pfad, Sicherung(1));

        Assert.Equal(0, EntferneAlteSicherungen.Ausfuehren(temp.Pfad, 10));
        Assert.Equal(2, Directory.GetFiles(temp.Pfad).Length);
    }
}
