using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="RoutingEinstellung"/>.
/// </summary>
public class RoutingEinstellungTests
{
    /// <summary>Erfundener Testschlüssel.</summary>
    private const string Schluessel = "test-schluessel";

    [Fact]
    public void Ohne_Eintrag_BRouter_mit_oeffentlichem_Server()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        Routingeinstellungen einstellungen = RoutingEinstellung.Laden(verbindung);

        Assert.Equal(new Routingeinstellungen(Routingdienst.BRouter, "https://brouter.de/brouter", null), einstellungen);
    }

    [Fact]
    public void Einstellungen_bleiben_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        var einstellungen = new Routingeinstellungen(Routingdienst.Openrouteservice, "http://localhost:17777/brouter", Schluessel);
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            RoutingEinstellung.Speichern(verbindung, einstellungen);
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal(einstellungen, RoutingEinstellung.Laden(wieder));
    }

    [Fact]
    public void Leere_Adresse_und_leerer_Schluessel_ergeben_den_Standard()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        RoutingEinstellung.Speichern(verbindung, new Routingeinstellungen(Routingdienst.Openrouteservice, "http://x/brouter", Schluessel));

        RoutingEinstellung.Speichern(verbindung, new Routingeinstellungen(Routingdienst.BRouter, "  ", " "));

        Assert.Equal(Routingeinstellungen.Standard, RoutingEinstellung.Laden(verbindung));
    }

    [Fact]
    public void Leerzeichen_am_Rand_werden_entfernt()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        RoutingEinstellung.Speichern(verbindung, new Routingeinstellungen(Routingdienst.Openrouteservice, " http://x/brouter ", $" {Schluessel} "));

        Assert.Equal(new Routingeinstellungen(Routingdienst.Openrouteservice, "http://x/brouter", Schluessel), RoutingEinstellung.Laden(verbindung));
    }

    [Fact]
    public void Unbekannter_Dienst_ergibt_BRouter()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = "INSERT INTO Einstellung (Schluessel, Wert) VALUES ('Routing.Dienst', 'Graphhopper');";
            befehl.ExecuteNonQuery();
        }

        Assert.Equal(Routingdienst.BRouter, RoutingEinstellung.Laden(verbindung).Dienst);
    }
}
