using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleEingabelisten"/>.
/// </summary>
public class ErmittleEingabelistenTests
{
    /// <summary>Gepflegte Liste der Trainingsbereiche in der Reihenfolge des Benutzers.</summary>
    private static readonly string[] Bereiche = ["K0", "GA1", "GA2"];

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
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit(), Bereiche);

        Assert.Equal(["Wandern", "Bike"], listen.Aktivitaeten.Select(a => a.Name));
        Assert.Equal([2L, 1L], listen.Aktivitaeten.Select(a => a.Id!.Value));
    }

    [Fact]
    public void Equipments_mit_kein_vorne_und_ohne_ausgemusterte()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit { EquipmentId = 7 }, Bereiche);

        Assert.Equal(["(kein)", "Genius", "Rennrad"], listen.Equipments.Select(e => e.Name));
        Assert.Null(listen.Equipments[0].Id);
    }

    [Fact]
    public void Ausgemustertes_Equipment_erscheint_wenn_es_zugeordnet_ist()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit { EquipmentId = 9 }, Bereiche);

        Assert.Equal(["(kein)", "Altes Rad", "Genius", "Rennrad"], listen.Equipments.Select(e => e.Name));
    }

    [Fact]
    public void Equipments_nur_der_Aktivitaet_der_Einheit_und_ohne_Aktivitaet()
    {
        Datenbestand bestand = Bestand();
        bestand.Equipments.Add(new Equipment { Id = 10, Name = "Wanderschuhe", AktivitaetId = 2 });
        bestand.Equipments.Add(new Equipment { Id = 11, Name = "Bergrad", AktivitaetId = 1 });

        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(bestand, new Trainingseinheit { AktivitaetId = 1 }, Bereiche);

        Assert.Equal(["(kein)", "Bergrad", "Genius", "Rennrad"], listen.Equipments.Select(e => e.Name));
    }

    [Fact]
    public void Trainingsbereiche_leer_vorne_dann_die_gepflegte_Liste_in_ihrer_Reihenfolge()
    {
        // „WSA“ kommt im Datenbestand vor, steht aber nicht in der Liste – es wird nicht angeboten
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit(), Bereiche);

        Assert.Equal(["", "K0", "GA1", "GA2"], listen.Trainingsbereiche);
    }

    [Fact]
    public void Bereich_der_Einheit_ausserhalb_der_Liste_kommt_am_Ende_dazu()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit { Trainingsbereich = "WSA" }, Bereiche);

        Assert.Equal(["", "K0", "GA1", "GA2", "WSA"], listen.Trainingsbereiche);
    }

    [Fact]
    public void Bereich_der_Einheit_aus_der_Liste_erscheint_nur_einmal()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(Bestand(), new Trainingseinheit { Trainingsbereich = "GA1" }, Bereiche);

        Assert.Equal(["", "K0", "GA1", "GA2"], listen.Trainingsbereiche);
    }

    [Fact]
    public void Leerer_Datenbestand_ergibt_nur_die_Leereintraege()
    {
        Eingabelisten listen = ErmittleEingabelisten.Ausfuehren(new Datenbestand(), new Trainingseinheit(), []);

        Assert.Empty(listen.Aktivitaeten);
        Assert.Equal(["(kein)"], listen.Equipments.Select(e => e.Name));
        Assert.Equal([""], listen.Trainingsbereiche);
    }
}
