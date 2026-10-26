using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="PlanungseinstellungenEinstellung"/>.
/// </summary>
public class PlanungseinstellungenTests
{
    [Fact]
    public void Ohne_Eintrag_Trekking_ohne_Ausschnitt_und_Ordner()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Equal(new Planungseinstellungen(Fahrprofil.Trekking, null, null, null), PlanungseinstellungenEinstellung.Laden(verbindung));
    }

    [Fact]
    public void Einstellungen_bleiben_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        var einstellungen = new Planungseinstellungen(
            Fahrprofil.Mountainbike, new Geoposition(47.123456789, 11.5), new Geoposition(48.25, 12.987654321), @"D:\Touren\Planungen");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            PlanungseinstellungenEinstellung.Speichern(verbindung, einstellungen);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(einstellungen, PlanungseinstellungenEinstellung.Laden(wieder));
    }

    [Fact]
    public void Ohne_Ausschnitt_und_Ordner_werden_die_Eintraege_geloescht()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        PlanungseinstellungenEinstellung.Speichern(verbindung,
            new Planungseinstellungen(Fahrprofil.Rennrad, new Geoposition(47, 11), new Geoposition(48, 12), @"C:\A"));

        PlanungseinstellungenEinstellung.Speichern(verbindung, new Planungseinstellungen(Fahrprofil.Rennrad, null, null, null));

        Assert.Equal(new Planungseinstellungen(Fahrprofil.Rennrad, null, null, null), PlanungseinstellungenEinstellung.Laden(verbindung));
    }

    [Theory]
    [InlineData("Tourenplanung.Profil", "Tandem")]
    [InlineData("Tourenplanung.Profil", "7")]
    [InlineData("Tourenplanung.Ausschnitt", "47;11;48")]
    [InlineData("Tourenplanung.Ausschnitt", "48;11;47;12")]
    [InlineData("Tourenplanung.Ausschnitt", "a;b;c;d")]
    public void Unlesbare_Eintraege_ergeben_den_Standard(string schluessel, string wert)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "INSERT INTO Einstellung (Schluessel, Wert) VALUES ($s, $w);";
            befehl.Parameters.AddWithValue("$s", schluessel);
            befehl.Parameters.AddWithValue("$w", wert);
            befehl.ExecuteNonQuery();
        }

        Assert.Equal(Planungseinstellungen.Standard, PlanungseinstellungenEinstellung.Laden(verbindung));
    }
}
