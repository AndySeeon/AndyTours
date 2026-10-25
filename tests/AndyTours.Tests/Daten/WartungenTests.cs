using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="WartungenTabelle.Einfuegen"/>, <see cref="WartungenTabelle.Aendern"/>,
/// <see cref="WartungenTabelle.Loeschen"/>, <see cref="WartungenTabelle.ErsetzeTaetigkeit"/> und die Liste
/// „Tätigkeit“ in <see cref="AuswahllisteEinstellung"/>.
/// </summary>
public class WartungenTests
{
    /// <summary>Öffnet eine Arbeitsspeicher-Datenbank mit einem Rad und drei Wartungen.</summary>
    private static SqliteConnection DatenbankMitWartungen()
    {
        SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        DatenbestandTabellen.Speichern(verbindung, new Datenbestand
        {
            Equipments = { new Equipment { Id = 7, Name = "Genius" } },
            Wartungen =
            {
                new Wartung { Id = 1, Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel" },
                new Wartung { Id = 2, Datum = new DateOnly(2026, 6, 1), EquipmentId = 7, Taetigkeit = "Dämpfer eingestellt" },
                new Wartung { Id = 3, Datum = new DateOnly(2026, 7, 1), EquipmentId = 7, Taetigkeit = "kettenwechsel " },
            },
        });
        return verbindung;
    }

    [Fact]
    public void Neue_Wartung_bekommt_eine_neue_Nummer_und_bleibt_erhalten()
    {
        using SqliteConnection verbindung = DatenbankMitWartungen();
        var neu = new Wartung { Datum = new DateOnly(2026, 9, 6), EquipmentId = 7, Taetigkeit = "Reifen HR", Kommentar = "Schwalbe" };

        long id = WartungenTabelle.Einfuegen(verbindung, neu);

        Assert.True(id > 3);
        Wartung geladen = WartungenTabelle.Laden(verbindung).Single(w => w.Id == id);
        Assert.Equal(new DateOnly(2026, 9, 6), geladen.Datum);
        Assert.Equal(7, geladen.EquipmentId);
        Assert.Equal("Reifen HR", geladen.Taetigkeit);
        Assert.Equal("Schwalbe", geladen.Kommentar);
        Assert.Equal(WartungsTyp.Wartung, geladen.Typ);
    }

    [Fact]
    public void Aendern_speichert_die_neuen_Werte()
    {
        using SqliteConnection verbindung = DatenbankMitWartungen();

        WartungenTabelle.Aendern(verbindung, new Wartung
        {
            Id = 2, Datum = new DateOnly(2026, 6, 2), EquipmentId = null, Taetigkeit = "Inspektion", Kommentar = "neu",
        });

        Wartung geladen = WartungenTabelle.Laden(verbindung).Single(w => w.Id == 2);
        Assert.Equal(new DateOnly(2026, 6, 2), geladen.Datum);
        Assert.Null(geladen.EquipmentId);
        Assert.Equal("Inspektion", geladen.Taetigkeit);
        Assert.Equal("neu", geladen.Kommentar);
    }

    [Fact]
    public void Aendern_einer_unbekannten_Wartung_meldet_einen_Fehler()
    {
        using SqliteConnection verbindung = DatenbankMitWartungen();

        Assert.Throws<InvalidOperationException>(() => WartungenTabelle.Aendern(verbindung, new Wartung { Id = 99, Taetigkeit = "x" }));
    }

    [Fact]
    public void Loeschen_entfernt_mehrere_Wartungen()
    {
        using SqliteConnection verbindung = DatenbankMitWartungen();

        WartungenTabelle.Loeschen(verbindung, [1, 3]);

        Assert.Equal([2], WartungenTabelle.Laden(verbindung).Select(w => w.Id));
    }

    [Fact]
    public void Taetigkeit_ersetzen_ohne_Unterschied_der_Schreibweise()
    {
        using SqliteConnection verbindung = DatenbankMitWartungen();
        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        int anzahl = WartungenTabelle.ErsetzeTaetigkeit(verbindung, transaktion, "Kettenwechsel", " Kette gewechselt ");
        int umlaut = WartungenTabelle.ErsetzeTaetigkeit(verbindung, transaktion, "DÄMPFER eingestellt", "Dämpfer");
        transaktion.Commit();

        Assert.Equal(2, anzahl);
        Assert.Equal(1, umlaut);
        Assert.Equal(["Kette gewechselt", "Dämpfer", "Kette gewechselt"],
            WartungenTabelle.Laden(verbindung).OrderBy(w => w.Id).Select(w => w.Taetigkeit));
    }

    [Fact]
    public void Liste_Taetigkeit_wird_gespeichert_und_geladen()
    {
        using SqliteConnection verbindung = DatenbankMitWartungen();

        Assert.Null(AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Taetigkeit));
        AuswahllisteEinstellung.Speichern(verbindung, null, Auswahlliste.Taetigkeit, ["Kettenwechsel", "Dämpfer eingestellt"]);

        Assert.Equal(["Kettenwechsel", "Dämpfer eingestellt"], AuswahllisteEinstellung.Laden(verbindung, Auswahlliste.Taetigkeit));
    }
}
