using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="ErmittleTrainingsbereiche"/>.
/// </summary>
public class ErmittleTrainingsbereicheTests
{
    /// <summary>Erzeugt Einheiten mit den angegebenen Trainingsbereichen.</summary>
    private static List<Trainingseinheit> Einheiten(params string[] bereiche) =>
        bereiche.Select(b => new Trainingseinheit { Trainingsbereich = b }).ToList();

    [Fact]
    public void Vorkommende_Kuerzel_alphabetisch_ohne_leere_und_doppelte()
    {
        List<string> liste = ErmittleTrainingsbereiche.Ausfuehren(Einheiten("WSA", "GA2", "", "GA1", "GA2", " K0 ", "SK"));

        Assert.Equal(["GA1", "GA2", "K0", "SK", "WSA"], liste);
    }

    [Fact]
    public void Schreibvarianten_ergeben_einen_Eintrag_in_der_zuerst_gefundenen_Schreibweise()
    {
        List<string> liste = ErmittleTrainingsbereiche.Ausfuehren(Einheiten("GA1", "ga1", "Ga1"));

        Assert.Equal(["GA1"], liste);
    }

    [Fact]
    public void Ohne_Einheiten_ist_die_Liste_leer()
    {
        Assert.Empty(ErmittleTrainingsbereiche.Ausfuehren([]));
    }
}
