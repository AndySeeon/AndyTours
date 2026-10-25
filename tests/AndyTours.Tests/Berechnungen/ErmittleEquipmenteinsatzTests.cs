using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleEquipmenteinsatz"/>.
/// </summary>
public class ErmittleEquipmenteinsatzTests
{
    /// <summary>Einheit mit Equipment, Distanz und Höhenmetern.</summary>
    private static Trainingseinheit Einheit(long? equipmentId, double km, int hm, int jahr = 2026, long aktivitaetId = 1) =>
        new() { Datum = new DateOnly(jahr, 5, 1), EquipmentId = equipmentId, DistanzKm = km, Hoehenmeter = hm, AktivitaetId = aktivitaetId };

    [Fact]
    public void Summen_je_Equipment_ueber_alle_Jahre_und_Aktivitaeten_mit_Anfangskilometern()
    {
        var genius = new Equipment { Id = 7, Name = "Genius", AnfangsdistanzKm = 1000 };
        var einheiten = new[]
        {
            Einheit(7, 40.4, 500, 2024, aktivitaetId: 1),
            Einheit(7, 60.2, 700, 2026, aktivitaetId: 2),
        };

        var zeile = ErmittleEquipmenteinsatz.Ausfuehren(einheiten, [genius])[0];

        Assert.Equal("Genius", zeile.Name);
        Assert.Equal(1000, zeile.VorherKm);
        Assert.Equal(1100.6, zeile.GesamtKm!.Value, 6);
        Assert.Equal(1200, zeile.GesamtHm);
        Assert.False(zeile.IstSumme);
    }

    [Fact]
    public void Gesamt_km_entspricht_dem_km_Stand()
    {
        var genius = new Equipment { Id = 7, Name = "Genius", AnfangsdistanzKm = 250 };
        var einheiten = new[] { Einheit(7, 40, 0), Einheit(7, 35, 0, 2027) };

        var zeile = ErmittleEquipmenteinsatz.Ausfuehren(einheiten, [genius])[0];

        Assert.Equal(BerechneKmStand.Ausfuehren(einheiten, genius, new DateOnly(2100, 1, 1)), zeile.GesamtKm);
    }

    [Fact]
    public void Werte_0_sind_leer()
    {
        var genius = new Equipment { Id = 7, Name = "Genius" };

        var zeile = ErmittleEquipmenteinsatz.Ausfuehren([Einheit(7, 0.3, 0)], [genius])[0];

        Assert.Null(zeile.VorherKm);
        Assert.Null(zeile.GesamtKm);   // 0,3 km ergibt gerundet 0
        Assert.Null(zeile.GesamtHm);
    }

    [Fact]
    public void Reihenfolge_im_Einsatz_dann_ausgemustert_dann_ohne_Equipment_dann_Summe()
    {
        var equipments = new[]
        {
            new Equipment { Id = 1, Name = "Genius" },
            new Equipment { Id = 2, Name = "Stevens" },
            new Equipment { Id = 3, Name = "Namur", Ausgemustert = true },
            new Equipment { Id = 4, Name = "Alt", Ausgemustert = true },
        };
        var einheiten = new[]
        {
            Einheit(1, 100, 0), Einheit(2, 300, 0), Einheit(3, 5000, 0), Einheit(4, 6000, 0), Einheit(null, 10, 0),
        };

        var namen = ErmittleEquipmenteinsatz.Ausfuehren(einheiten, equipments).Select(z => z.Name);

        Assert.Equal(["Stevens", "Genius", "Alt (ausgemustert)", "Namur (ausgemustert)", "ohne Equipment", "Summe"], namen);
    }

    [Fact]
    public void Equipment_ohne_Einheit_und_ohne_Anfangskilometer_fehlt()
    {
        var equipments = new[]
        {
            new Equipment { Id = 1, Name = "Genius" },
            new Equipment { Id = 2, Name = "Neu" },
            new Equipment { Id = 3, Name = "Nur Anfang", AnfangsdistanzKm = 500 },
        };

        var namen = ErmittleEquipmenteinsatz.Ausfuehren([Einheit(1, 10, 0)], equipments).Select(z => z.Name);

        Assert.Equal(["Nur Anfang", "Genius", "Summe"], namen);
    }

    [Fact]
    public void Einheiten_ohne_oder_mit_unbekanntem_Equipment_bilden_ohne_Equipment()
    {
        var genius = new Equipment { Id = 7, Name = "Genius" };
        var einheiten = new[] { Einheit(7, 50, 100), Einheit(null, 20, 200), Einheit(99, 30, 300) };

        var zeile = ErmittleEquipmenteinsatz.Ausfuehren(einheiten, [genius]).Single(z => z.Name == "ohne Equipment");

        Assert.Null(zeile.VorherKm);
        Assert.Equal(50, zeile.GesamtKm);
        Assert.Equal(500, zeile.GesamtHm);
    }

    [Fact]
    public void Ohne_solche_Einheiten_fehlt_die_Zeile_ohne_Equipment()
    {
        var genius = new Equipment { Id = 7, Name = "Genius" };

        var zeilen = ErmittleEquipmenteinsatz.Ausfuehren([Einheit(7, 50, 100)], [genius]);

        Assert.DoesNotContain(zeilen, z => z.Name == "ohne Equipment");
    }

    [Fact]
    public void Summe_bildet_die_Summe_jeder_Spalte()
    {
        var equipments = new[]
        {
            new Equipment { Id = 1, Name = "Genius", AnfangsdistanzKm = 1000 },
            new Equipment { Id = 2, Name = "Namur", AnfangsdistanzKm = 200, Ausgemustert = true },
        };
        var einheiten = new[] { Einheit(1, 50, 100), Einheit(2, 30, 300), Einheit(null, 20, 50) };

        var summe = ErmittleEquipmenteinsatz.Ausfuehren(einheiten, equipments)[^1];

        Assert.True(summe.IstSumme);
        Assert.Equal("Summe", summe.Name);
        Assert.Equal(1200, summe.VorherKm);
        Assert.Equal(1300, summe.GesamtKm);
        Assert.Equal(450, summe.GesamtHm);
    }

    [Fact]
    public void Leerer_Bestand_ergibt_keine_Zeilen()
    {
        Assert.Empty(ErmittleEquipmenteinsatz.Ausfuehren([], [new Equipment { Id = 1, Name = "Neu" }]));
        Assert.Empty(ErmittleEquipmenteinsatz.Ausfuehren([], []));
    }
}
