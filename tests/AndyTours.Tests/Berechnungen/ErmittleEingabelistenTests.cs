using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleEingabelisten"/>.
/// </summary>
public class ErmittleEingabelistenTests
{
    /// <summary>Datenbestand mit einem ausgemusterten Equipment und mehreren Trainingsbereichen.</summary>
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten = { new Aktivitaet { Id = 2, Name = "Wandern" }, new Aktivitaet { Id = 1, Name = "Bike" } },
        Equipments =
        {
            new Equipment { Id = 8, Name = "Rennrad" }, new Equipment { Id = 7, Name = "Genius" },
            new Equipment { Id = 9, Name = "Altes Rad", Ausgemustert = true },
        },
        Trainingseinheiten =
        {
            new Trainingseinheit { Trainingsbereich = "GA2" }, new Trainingseinheit { Trainingsbereich = "GA1" },
            new Trainingseinheit { Trainingsbereich = "" }, new Trainingseinheit { Trainingsbereich = "GA1" },
            new Trainingseinheit { Trainingsbereich = "WSA" },
        },
    };

    [Fact]
    public void Aktivitaeten_in_der_Reihenfolge_des_Datenbestands()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit());

        Assert.Equal(["Wandern", "Bike"], listen.Aktivitaeten.Select(a => a.Name));
        Assert.Equal([2L, 1L], listen.Aktivitaeten.Select(a => a.Id!.Value));
    }

    [Fact]
    public void Equipments_mit_kein_vorne_und_ohne_ausgemusterte()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit { EquipmentId = 7 });

        Assert.Equal(["(kein)", "Genius", "Rennrad"], listen.Equipments.Select(e => e.Name));
        Assert.Null(listen.Equipments[0].Id);
    }

    [Fact]
    public void Ausgemustertes_Equipment_erscheint_wenn_es_zugeordnet_ist()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit { EquipmentId = 9 });

        Assert.Equal(["(kein)", "Altes Rad", "Genius", "Rennrad"], listen.Equipments.Select(e => e.Name));
    }

    [Fact]
    public void Trainingsbereiche_leer_vorne_dann_vorkommende_Kuerzel_alphabetisch()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit());

        Assert.Equal(["", "GA1", "GA2", "WSA"], listen.Trainingsbereiche);
    }

    [Fact]
    public void Leerer_Datenbestand_ergibt_nur_die_Leereintraege()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(new Datenbestand(), new Trainingseinheit());

        Assert.Empty(listen.Aktivitaeten);
        Assert.Equal(["(kein)"], listen.Equipments.Select(e => e.Name));
        Assert.Equal([""], listen.Trainingsbereiche);
    }
}
