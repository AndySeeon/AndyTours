using AndyTours.Import.Altdaten;

namespace AndyTours.Tests.Altdaten;

/// <summary>
/// Tests für <see cref="RepariereUmlaute"/> mit der echten <see cref="UmlautKorrekturliste"/>.
/// </summary>
public class RepariereUmlauteTests
{
    [Theory]
    [InlineData("Br�ndling �ber Blickner", "Bründling über Blickner")]
    [InlineData("Burgeralm- Runde �ber K�ssen", "Burgeralm- Runde über Kössen")]
    [InlineData("Priener-/Burgerh�tte", "Priener-/Burgerhütte")]
    [InlineData("Hartseest�berl", "Hartseestüberl")]
    [InlineData("saukalt, immer um die 0�C", "saukalt, immer um die 0°C")]
    [InlineData("hei� wars, 40� in der Sonne", "heiß wars, 40° in der Sonne")]
    [InlineData("285 psi f�r 28% SAG", "285 psi für 28% SAG")]
    [InlineData("H gel", "Hügel")]
    [InlineData("Stoisser B�ckeralm", "Stoisser Bäckeralm")]
    [InlineData("Arbeit KW 12 (�PD)", "Arbeit KW 12 (ÜPD)")]
    public void Bekannte_Woerter_werden_repariert(string beschaedigt, string erwartet)
    {
        Assert.Equal(erwartet, RepariereUmlaute.Ausfuehren(beschaedigt));
    }

    [Fact]
    public void Unbekannte_Stellen_bleiben_mit_Ersatzzeichen_stehen()
    {
        // Ein erfundener Name, der in keiner Korrekturliste steht
        string ergebnis = RepariereUmlaute.Ausfuehren("Gipfelkreuz am Gr�nstein");

        Assert.Contains(RepariereUmlaute.Ersatzzeichen, ergebnis);
    }

    [Fact]
    public void Laengere_Eintraege_haben_Vorrang()
    {
        // Ohne Sortierung nach Länge würde „ab" zuerst greifen und „abc" nie mehr passen.
        var liste = new List<(string, string)> { ("ab", "X"), ("abc", "Y") };

        Assert.Equal("Y", RepariereUmlaute.Ausfuehren("abc", liste));
    }
}
