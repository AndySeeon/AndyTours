using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="SicherungEinstellungen.Laden"/> und <see cref="SicherungEinstellungen.Speichern"/>.
/// </summary>
public class SicherungseinstellungenTests
{
    [Fact]
    public void Neue_Datenbank_liefert_die_Standardwerte()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Sicherungseinstellungen geladen = SicherungEinstellungen.Laden(verbindung);

        Assert.Null(geladen.Sicherungsordner);
        Assert.Equal(3, geladen.Intervall);
        Assert.Equal(10, geladen.Hoechstzahl);
        Assert.Equal(0, geladen.StartsSeitSicherung);
    }

    [Fact]
    public void Gespeicherte_Werte_werden_unveraendert_geladen()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        var einstellungen = new Sicherungseinstellungen
        {
            Sicherungsordner = @"\\NAS\Sicherung\Böhmerwald", Intervall = 1, Hoechstzahl = 99, StartsSeitSicherung = 2,
        };

        SicherungEinstellungen.Speichern(verbindung, einstellungen);

        Assert.Equal(einstellungen, SicherungEinstellungen.Laden(verbindung));
    }

    [Fact]
    public void Werte_bleiben_nach_erneutem_Oeffnen_der_Datei_erhalten()
    {
        using var temp = new Datenordner.TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        var einstellungen = new Sicherungseinstellungen { Sicherungsordner = @"D:\Sicherung", Intervall = 5, Hoechstzahl = 2 };

        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            SicherungEinstellungen.Speichern(verbindung, einstellungen);
        }

        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            Assert.Equal(einstellungen, SicherungEinstellungen.Laden(verbindung));
        }
    }

    [Fact]
    public void Zurueck_auf_Standardordner_entfernt_den_gewaehlten_Ordner()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        SicherungEinstellungen.Speichern(verbindung, new Sicherungseinstellungen { Sicherungsordner = @"D:\Sicherung" });

        SicherungEinstellungen.Speichern(verbindung, new Sicherungseinstellungen { Sicherungsordner = null });

        Assert.Null(SicherungEinstellungen.Laden(verbindung).Sicherungsordner);
    }

    [Fact]
    public void Unbrauchbare_Eintraege_werden_durch_Standardwerte_ersetzt()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = """
                INSERT INTO Einstellung VALUES ('Sicherung.Intervall', '0');
                INSERT INTO Einstellung VALUES ('Sicherung.Hoechstzahl', 'viele');
                INSERT INTO Einstellung VALUES ('Sicherung.StartsSeitSicherung', '-4');
                """;
            befehl.ExecuteNonQuery();
        }

        Assert.Equal(new Sicherungseinstellungen(), SicherungEinstellungen.Laden(verbindung));
    }
}
