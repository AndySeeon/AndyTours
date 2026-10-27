using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="FahrerTabelle.SpeicherePersoenlicheDaten"/>.
/// </summary>
public class FahrerTests
{
    /// <summary>Neue persönliche Daten mit allen Feldern des Fensters.</summary>
    private static Fahrer Neu() => new()
    {
        Vorname = "Andy", Nachname = "Muster", Geburtstag = new DateOnly(1963, 5, 28), GroesseCm = 182,
        GewichtKg = 78.5, Ruhepuls = 48, Maxpuls = 178, Heimatort = "Ingolstadt",
    };

    [Fact]
    public void Vorhandener_Sportler_wird_geaendert_und_behaelt_die_uebrigen_Werte()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Fahrer =
            {
                new Fahrer
                {
                    Id = 3, Vorname = "Alt", GewichtKg = 90, TrainingspulsVon = 120, TrainingspulsBis = 150,
                    Heimatort = "Alt", HeimatBreitengrad = 47.97, HeimatLaengengrad = 11.1,
                },
            },
        });

        long id = FahrerTabelle.SpeicherePersoenlicheDaten(verbindung, Neu());

        Fahrer gelesen = Assert.Single(FahrerTabelle.Laden(verbindung));
        Assert.Equal(3, id);
        Assert.Equal(3, gelesen.Id);
        Assert.Equal("Andy", gelesen.Vorname);
        Assert.Equal("Muster", gelesen.Nachname);
        Assert.Equal(new DateOnly(1963, 5, 28), gelesen.Geburtstag);
        Assert.Equal(182, gelesen.GroesseCm);
        Assert.Equal(78.5, gelesen.GewichtKg);
        Assert.Equal(48, gelesen.Ruhepuls);
        Assert.Equal(178, gelesen.Maxpuls);
        Assert.Equal("Ingolstadt", gelesen.Heimatort);
        Assert.Equal(120, gelesen.TrainingspulsVon);
        Assert.Equal(150, gelesen.TrainingspulsBis);
        Assert.Equal(47.97, gelesen.HeimatBreitengrad);
        Assert.Equal(11.1, gelesen.HeimatLaengengrad);
    }

    [Fact]
    public void In_leerer_Datenbank_wird_der_Sportler_angelegt()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        long id = FahrerTabelle.SpeicherePersoenlicheDaten(verbindung, Neu());

        Fahrer gelesen = Assert.Single(FahrerTabelle.Laden(verbindung));
        Assert.Equal(id, gelesen.Id);
        Assert.Equal("Andy", gelesen.Vorname);
        Assert.Equal(178, gelesen.Maxpuls);
    }

    [Fact]
    public void Unbekannte_Werte_werden_als_leer_gespeichert()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        FahrerTabelle.SpeicherePersoenlicheDaten(verbindung, Neu());

        FahrerTabelle.SpeicherePersoenlicheDaten(verbindung, new Fahrer());

        Fahrer gelesen = Assert.Single(FahrerTabelle.Laden(verbindung));
        Assert.Equal("", gelesen.Vorname);
        Assert.Null(gelesen.Geburtstag);
        Assert.Equal(0, gelesen.GroesseCm);
        Assert.Equal(0, gelesen.GewichtKg);
        Assert.Equal(0, gelesen.Ruhepuls);
        Assert.Equal(0, gelesen.Maxpuls);
        Assert.Equal("", gelesen.Heimatort);
    }
}
