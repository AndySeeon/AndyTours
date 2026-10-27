using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="OverlayfarbeEinstellung"/>.
/// </summary>
public class OverlayfarbeTests
{
    [Fact]
    public void Ohne_Eintrag_ist_die_gemeinsame_Farbe_Violett()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Equal(Overlayfarbe.Violett, OverlayfarbeEinstellung.Laden(verbindung));
    }

    [Fact]
    public void Gespeicherte_Farbe_bleibt_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            OverlayfarbeEinstellung.Speichern(verbindung, Overlayfarbe.Tuerkis);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(Overlayfarbe.Tuerkis, OverlayfarbeEinstellung.Laden(wieder));
    }

    [Fact]
    public void Unbekannter_Wert_ergibt_Violett()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "INSERT INTO Einstellung (Schluessel, Wert) VALUES ('Overlay.Farbe', 'Lila');";
            befehl.ExecuteNonQuery();
        }

        Assert.Equal(Overlayfarbe.Violett, OverlayfarbeEinstellung.Laden(verbindung));
    }
}
