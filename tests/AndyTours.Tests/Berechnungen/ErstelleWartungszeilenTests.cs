using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErstelleWartungszeilen"/>.
/// </summary>
public class ErstelleWartungszeilenTests
{
    /// <summary>Zwei Räder (eines ausgemustert), Einheiten und Wartungen samt einem Alarm.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 7, Name = "Genius", AnfangsdistanzKm = 100 },
            new Equipment { Id = 8, Name = "Stevens", Ausgemustert = true },
        },
        Trainingseinheiten =
        {
            new Trainingseinheit { Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, DistanzKm = 40 },
            new Trainingseinheit { Datum = new DateOnly(2026, 6, 1), EquipmentId = 8, DistanzKm = 70 },
        },
        Wartungen =
        {
            new Wartung { Id = 1, Datum = new DateOnly(2026, 5, 1), EquipmentId = 7, Taetigkeit = "Kettenwechsel", Kommentar = "0,5 %" },
            new Wartung { Id = 2, Datum = new DateOnly(2026, 6, 1), EquipmentId = 8, Taetigkeit = "Reifen HR" },
            new Wartung { Id = 3, Datum = new DateOnly(2026, 6, 1), EquipmentId = 7, Taetigkeit = "Inspektion" },
            new Wartung { Id = 4, Datum = new DateOnly(2026, 7, 1), EquipmentId = 7, Taetigkeit = "Kette prüfen", Typ = WartungsTyp.Alarm },
        },
    };

    [Fact]
    public void Alle_Wartungen_die_neueste_oben_ohne_Alarme()
    {
        List<Wartungszeile> zeilen = ErstelleWartungszeilen.Ausfuehren(Bestand(), null);

        // gleiches Datum: die zuletzt erfasste (Nr. 3) oben; die Wartung des ausgemusterten Rads erscheint
        Assert.Equal([3, 2, 1], zeilen.Select(z => z.Id));
        Assert.Equal(new Wartungszeile(1, new DateOnly(2026, 5, 1), "Genius", 140, "Kettenwechsel", "0,5 %"), zeilen[2]);
        Assert.Equal("Stevens", zeilen[1].Rad);
        Assert.Equal(70, zeilen[1].KmStand);
    }

    [Fact]
    public void Nur_die_Wartungen_eines_Rads()
    {
        Assert.Equal([3, 1], ErstelleWartungszeilen.Ausfuehren(Bestand(), 7).Select(z => z.Id));
    }

    [Fact]
    public void Ohne_Rad_bleiben_Name_und_km_Stand_leer()
    {
        Datenbestand bestand = Bestand();
        bestand.Wartungen.Add(new Wartung { Id = 5, Datum = new DateOnly(2026, 8, 1), Taetigkeit = "Inspektion" });

        Wartungszeile zeile = ErstelleWartungszeilen.Ausfuehren(bestand, null)[0];

        Assert.Equal((5L, "", (double?)null), (zeile.Id, zeile.Rad, zeile.KmStand));
    }
}
