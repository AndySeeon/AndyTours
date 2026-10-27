using Microsoft.Data.Sqlite;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="GpxOrdnerEinstellung.Laden"/> und <see cref="GpxOrdnerEinstellung.Speichern"/>.
/// </summary>
public class GpxOrdnerTests
{
    [Fact]
    public void Ohne_Eintrag_gibt_es_keinen_Ordner()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Assert.Null(GpxOrdnerEinstellung.Laden(verbindung));
    }

    [Fact]
    public void Ordner_bleibt_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            GpxOrdnerEinstellung.Speichern(verbindung, @"D:\Touren\GPX");
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(@"D:\Touren\GPX", GpxOrdnerEinstellung.Laden(wieder));
    }

    [Fact]
    public void Erneutes_Speichern_ueberschreibt_den_Ordner()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        GpxOrdnerEinstellung.Speichern(verbindung, @"C:\A");
        GpxOrdnerEinstellung.Speichern(verbindung, @"C:\B");

        Assert.Equal(@"C:\B", GpxOrdnerEinstellung.Laden(verbindung));
    }
}
