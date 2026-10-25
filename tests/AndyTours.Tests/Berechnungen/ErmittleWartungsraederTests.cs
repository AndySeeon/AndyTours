using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleWartungsraeder"/>.
/// </summary>
public class ErmittleWartungsraederTests
{
    /// <summary>Vier Räder, zwei davon ausgemustert.</summary>
    private static Datenbestand Bestand() => new()
    {
        Equipments =
        {
            new Equipment { Id = 1, Name = "Stevens", Ausgemustert = true },
            new Equipment { Id = 2, Name = "Genius" },
            new Equipment { Id = 3, Name = "bergamont" },
            new Equipment { Id = 4, Name = "Arc8", Ausgemustert = true },
        },
    };

    [Fact]
    public void Neue_Wartung_ohne_ausgemusterte_Raeder_alphabetisch()
    {
        Assert.Equal(["bergamont", "Genius"], ErmittleWartungsraeder.Ausfuehren(Bestand(), null).Select(e => e.Name));
    }

    [Fact]
    public void Beim_Bearbeiten_bleibt_das_ausgemusterte_Rad_waehlbar_gekennzeichnet_am_Ende()
    {
        Assert.Equal([new Auswahleintrag(3, "bergamont"), new Auswahleintrag(2, "Genius"), new Auswahleintrag(1, "Stevens (ausgemustert)")],
            ErmittleWartungsraeder.Ausfuehren(Bestand(), 1));
    }

    [Fact]
    public void Radauswahl_zeigt_alle_ausgemusterten_am_Ende_alphabetisch()
    {
        Assert.Equal(["bergamont", "Genius", "Arc8 (ausgemustert)", "Stevens (ausgemustert)"],
            ErmittleWartungsraeder.Ausfuehren(Bestand(), null, alleRaeder: true).Select(e => e.Name));
    }
}
