using System.Globalization;
using AndyTours.Daten.Datensicherung;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="ErmittleSicherungen"/>.
/// </summary>
public class ErmittleSicherungenTests
{
    /// <summary>Legt eine leere Datei mit dem angegebenen Namen an.</summary>
    private static void Lege(string ordner, string name) => File.WriteAllText(Path.Combine(ordner, name), "");

    /// <summary>Zeitpunkt der Sicherung vom 1. Oktober 2026 um 8 Uhr plus <paramref name="tage"/> Tage.</summary>
    private static DateTime Zeitpunkt(int tage) => new DateTime(2026, 10, 1, 8, 0, 0).AddDays(tage);

    /// <summary>Dateiname der Sicherung zu <see cref="Zeitpunkt"/>.</summary>
    private static string Sicherung(int tage) =>
        "andytours_" + Zeitpunkt(tage).ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture) + ".db";

    [Fact]
    public void Sicherungen_werden_neueste_zuerst_mit_Zeitpunkt_aus_dem_Namen_geliefert()
    {
        using var temp = new TempOrdner();
        Lege(temp.Pfad, Sicherung(1));
        Lege(temp.Pfad, Sicherung(5));
        Lege(temp.Pfad, Sicherung(3));

        List<Sicherungseintrag> sicherungen = ErmittleSicherungen.Ausfuehren(temp.Pfad);

        Assert.Equal([Zeitpunkt(5), Zeitpunkt(3), Zeitpunkt(1)], sicherungen.Select(s => s.Zeitpunkt));
        Assert.Equal(Path.Combine(temp.Pfad, Sicherung(5)), sicherungen[0].Datei);
    }

    [Fact]
    public void Fremde_und_aehnlich_benannte_Dateien_fehlen_in_der_Liste()
    {
        using var temp = new TempOrdner();
        foreach (string name in new[]
                 {
                     "andytours.db", "andytours_alt.db", "andytours_2026-01-01.db", "andytours_2020-01-01_08-00-00.db.tmp",
                     "andytours_2020-01-01_08-00-00.dbx", "Kopie andytours_2020-01-01_08-00-00.db", "notizen.txt",
                 })
        {
            Lege(temp.Pfad, name);
        }

        Lege(temp.Pfad, Sicherung(0));

        Sicherungseintrag einzige = Assert.Single(ErmittleSicherungen.Ausfuehren(temp.Pfad));
        Assert.Equal(Zeitpunkt(0), einzige.Zeitpunkt);
    }

    [Fact]
    public void Leerer_Ordner_ergibt_leere_Liste()
    {
        using var temp = new TempOrdner();

        Assert.Empty(ErmittleSicherungen.Ausfuehren(temp.Pfad));
    }

    [Fact]
    public void Fehlender_Ordner_ergibt_leere_Liste()
    {
        using var temp = new TempOrdner();

        Assert.Empty(ErmittleSicherungen.Ausfuehren(Path.Combine(temp.Pfad, "gibt es nicht")));
    }
}
