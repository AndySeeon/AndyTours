using AndyTours.Altdatenuebernahme;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests für <see cref="ErstelleAufzeichnungsbericht"/>.
/// </summary>
public class ErstelleAufzeichnungsberichtTests
{
    private static readonly Aufzeichnungsergebnis Ergebnis = new(
        Gefunden: 1250,
        Uebernommen: 1247,
        Uebersprungen: [(@"2016\kaputt.fit", "FIT decode error"), ("doppelt.fit", "Dieselbe Tour gibt es schon.")]);

    [Fact]
    public void Bericht_nennt_Herkunft_Mengen_und_jede_uebersprungene_Datei_mit_Grund()
    {
        string text = ErstelleAufzeichnungsbericht.Bericht(Ergebnis, @"C:\Fit", new DateTime(2026, 10, 26, 9, 5, 0));

        Assert.Contains("Erstellt: 26.10.2026 09:05", text);
        Assert.Contains(@"C:\Fit", text);
        Assert.Contains("Gefundene Aufzeichnungen: 1.250", text);
        Assert.Contains("Übernommene Touren:       1.247", text);
        Assert.Contains("Übersprungene Dateien:    2", text);
        Assert.Contains(@"2016\kaputt.fit: FIT decode error", text);
        Assert.Contains("doppelt.fit: Dieselbe Tour gibt es schon.", text);
    }

    [Fact]
    public void Bericht_ohne_uebersprungene_Dateien_hat_keine_Liste()
    {
        string text = ErstelleAufzeichnungsbericht.Bericht(new Aufzeichnungsergebnis(3, 3, []), @"C:\Fit", DateTime.Now);

        Assert.Contains("Übersprungene Dateien:    0", text);
        Assert.DoesNotContain("Übersprungene Dateien:\r", text);
        Assert.DoesNotContain("Übersprungene Dateien:\n", text);
    }

    [Fact]
    public void Zusammenfassung_nennt_Anzahl_uebernommen_uebersprungen_und_den_Bericht()
    {
        string text = ErstelleAufzeichnungsbericht.Zusammenfassung(Ergebnis, @"C:\AndyTours");

        Assert.Contains("Übernommene Touren:    1.247", text);
        Assert.Contains("Übersprungene Dateien: 2", text);
        Assert.Contains(@"Bericht: C:\AndyTours\aufzeichnungsbericht.txt", text);
    }
}
