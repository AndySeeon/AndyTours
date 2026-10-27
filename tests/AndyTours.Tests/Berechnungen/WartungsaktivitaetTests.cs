using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="Wartungsaktivitaet"/>.
/// </summary>
public class WartungsaktivitaetTests
{
    /// <summary>„Bike“ (Hauptaktivität) und „Laufen“.</summary>
    private static Datenbestand Bestand() => new()
    {
        Aktivitaeten =
        {
            new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true },
            new Aktivitaet { Id = 2, Name = "Laufen" },
        },
    };

    [Fact]
    public void Es_gilt_die_oben_gewaehlte_Aktivitaet()
    {
        Assert.Equal(2, Wartungsaktivitaet.Bestimmen(Bestand(), 2));
    }

    [Fact]
    public void Bei_allen_Aktivitaeten_gilt_die_Hauptaktivitaet()
    {
        Assert.Equal(1, Wartungsaktivitaet.Bestimmen(Bestand(), null));
    }

    [Fact]
    public void Ohne_Hauptaktivitaet_keine_Einschraenkung()
    {
        Datenbestand bestand = Bestand();
        bestand.Aktivitaeten[0].IstHaupt = false;

        Assert.Null(Wartungsaktivitaet.Bestimmen(bestand, null));
    }

    [Theory]
    [InlineData(1L, 1L, true)]      // gleiche Aktivität
    [InlineData(2L, 1L, false)]     // Schuhe bei „Bike“
    [InlineData(null, 1L, true)]    // Equipment ohne Aktivität passt immer
    [InlineData(2L, null, true)]    // keine Einschränkung
    public void Passt_nur_zur_eigenen_Aktivitaet(long? equipmentAktivitaet, long? aktivitaet, bool erwartet)
    {
        var equipment = new Equipment { Id = 7, AktivitaetId = equipmentAktivitaet };

        Assert.Equal(erwartet, Wartungsaktivitaet.Passt(equipment, aktivitaet));
    }

    [Fact]
    public void Ohne_Equipment_passt_es_immer()
    {
        Assert.True(Wartungsaktivitaet.Passt(null, 1));
    }
}
