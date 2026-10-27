using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="SaisonzieleTabelle"/> und die Saisonziele im Gesamtbestand.
/// </summary>
public class SaisonzieleTests
{
    /// <summary>
    /// Arbeitsspeicher-Datenbank: Aktivitäten Bike (1), Laufen (2); Ziele Bike ab 2024 und 2026, Laufen ab 2026.
    /// </summary>
    private static SqliteConnection Datenbank()
    {
        SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true, ZielKm = 6000 }, new Aktivitaet { Id = 2, Name = "Laufen" } },
            Saisonziele =
            {
                new Saisonziel { AktivitaetId = 1, AbJahr = 2024, ZielKm = 7000, ZielHm = 90000, ZielAnzahl = 130 },
                new Saisonziel { AktivitaetId = 1, AbJahr = 2026, ZielKm = 7500 },
                new Saisonziel { AktivitaetId = 2, AbJahr = 2026, ZielKm = 500 },
            },
        });
        return verbindung;
    }

    [Fact]
    public void Gesamtbestand_enthaelt_die_Saisonziele()
    {
        using SqliteConnection verbindung = Datenbank();

        Datenbestand bestand = DatenbestandTabellen.Laden(verbindung);

        Assert.Equal(3, bestand.Saisonziele.Count);
        Saisonziel ziel = bestand.Saisonziele[0];
        Assert.Equal((1L, 2024, 7000, 90000, 130), (ziel.AktivitaetId, ziel.AbJahr, ziel.ZielKm, ziel.ZielHm, ziel.ZielAnzahl));
    }

    [Fact]
    public void Gespeicherte_Jahre_werden_ersetzt_und_andere_bleiben()
    {
        using SqliteConnection verbindung = Datenbank();

        SaisonzieleTabelle.SpeichereJahre(verbindung, [2026, 2027],
        [
            new Saisonziel { AktivitaetId = 1, AbJahr = 2026, ZielKm = 8000 },
            new Saisonziel { AktivitaetId = 2, AbJahr = 2027, ZielAnzahl = 50 },
            new Saisonziel { AktivitaetId = 2, AbJahr = 2020, ZielKm = 1 },
        ]);

        List<Saisonziel> ziele = SaisonzieleTabelle.Laden(verbindung);
        Assert.Equal([(1L, 2024, 7000), (1L, 2026, 8000), (2L, 2027, 0)],
            ziele.Select(z => (z.AktivitaetId, z.AbJahr, z.ZielKm)));
        Assert.Equal(50, ziele[2].ZielAnzahl);
    }

    [Fact]
    public void Mit_der_Aktivitaet_verschwinden_ihre_Ziele()
    {
        using SqliteConnection verbindung = Datenbank();

        using (SqliteTransaction transaktion = verbindung.BeginTransaction())
        {
            AktivitaetenTabelle.Loeschen(verbindung, transaktion, 2);
            transaktion.Commit();
        }

        Assert.All(SaisonzieleTabelle.Laden(verbindung), z => Assert.Equal(1, z.AktivitaetId));
    }

    [Fact]
    public void Beim_Zusammenfuehren_verschwinden_die_Ziele_des_Ursprungs()
    {
        using SqliteConnection verbindung = Datenbank();

        using (SqliteTransaction transaktion = verbindung.BeginTransaction())
        {
            AktivitaetenTabelle.FuehreZusammen(verbindung, transaktion, 2, 1);
            transaktion.Commit();
        }

        Assert.Equal(2, SaisonzieleTabelle.Laden(verbindung).Count(z => z.AktivitaetId == 1));
        Assert.DoesNotContain(SaisonzieleTabelle.Laden(verbindung), z => z.AktivitaetId == 2);
    }
}
