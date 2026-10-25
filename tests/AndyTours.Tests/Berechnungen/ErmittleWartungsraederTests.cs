using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleWartungsraeder"/>.
/// </summary>
public class ErmittleWartungsraederTests
{
    /// <summary>Vier Räder („Bike“, zwei davon ausgemustert), zwei Paar Schuhe („Laufen“) und ein Equipment ohne Aktivität.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 1, Name = "Stevens", AktivitaetId = 1, Ausgemustert = true },
            new Equipment { Id = 2, Name = "Genius", AktivitaetId = 1 },
            new Equipment { Id = 3, Name = "bergamont", AktivitaetId = 1 },
            new Equipment { Id = 4, Name = "Arc8", AktivitaetId = 1, Ausgemustert = true },
            new Equipment { Id = 5, Name = "Brooks", AktivitaetId = 2 },
            new Equipment { Id = 6, Name = "Salomon", AktivitaetId = 2, Ausgemustert = true },
            new Equipment { Id = 7, Name = "Werkzeug" },
        },
    };

    [Fact]
    public void Nur_Equipments_im_Einsatz_der_Aktivitaet_alphabetisch()
    {
        Assert.Equal(["bergamont", "Genius", "Werkzeug"], ErmittleWartungsraeder.Ausfuehren(Bestand(), 1, null).Select(e => e.Name));
        Assert.Equal(["Brooks", "Werkzeug"], ErmittleWartungsraeder.Ausfuehren(Bestand(), 2, null).Select(e => e.Name));
    }

    [Fact]
    public void Ohne_Aktivitaet_alle_Equipments_im_Einsatz()
    {
        Assert.Equal(["bergamont", "Brooks", "Genius", "Werkzeug"],
            ErmittleWartungsraeder.Ausfuehren(Bestand(), null, null).Select(e => e.Name));
    }

    [Fact]
    public void Beim_Bearbeiten_bleibt_das_ausgemusterte_Rad_waehlbar_gekennzeichnet_am_Ende()
    {
        Assert.Equal(
            [new Auswahleintrag(3, "bergamont"), new Auswahleintrag(2, "Genius"), new Auswahleintrag(7, "Werkzeug"),
                new Auswahleintrag(1, "Stevens (ausgemustert)")],
            ErmittleWartungsraeder.Ausfuehren(Bestand(), 1, 1));
    }

    [Fact]
    public void Beim_Bearbeiten_bleibt_ein_Equipment_einer_anderen_Aktivitaet_waehlbar()
    {
        Assert.Equal(["bergamont", "Brooks", "Genius", "Werkzeug"],
            ErmittleWartungsraeder.Ausfuehren(Bestand(), 1, 5).Select(e => e.Name));
    }
}
