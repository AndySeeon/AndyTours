using System.Diagnostics;
using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="FuehreSchreibprobeDurch"/>.
/// </summary>
public class FuehreSchreibprobeDurchTests
{
    [Fact]
    public async Task Beschreibbarer_Ordner_besteht_die_Probe_und_bleibt_leer()
    {
        using var temp = new TempOrdner();

        bool ergebnis = await FuehreSchreibprobeDurch.Ausfuehren(temp.Pfad, TimeSpan.Zero, 1, TimeSpan.Zero);

        Assert.True(ergebnis);
        Assert.Empty(Directory.GetFileSystemEntries(temp.Pfad)); // Probedatei wieder entfernt
    }

    [Fact]
    public async Task Pufferzeit_wird_eingehalten()
    {
        using var temp = new TempOrdner();
        var uhr = Stopwatch.StartNew();

        bool ergebnis = await FuehreSchreibprobeDurch.Ausfuehren(temp.Pfad, TimeSpan.FromMilliseconds(200), 1, TimeSpan.Zero);

        Assert.True(ergebnis);
        Assert.True(uhr.Elapsed >= TimeSpan.FromMilliseconds(190), $"nur {uhr.ElapsedMilliseconds} ms gewartet");
    }

    [Fact]
    public async Task Nicht_vorhandener_Ordner_besteht_die_Probe_nicht()
    {
        using var temp = new TempOrdner();

        bool ergebnis = await FuehreSchreibprobeDurch.Ausfuehren(Path.Combine(temp.Pfad, "fehlt"), TimeSpan.Zero, 1, TimeSpan.Zero);

        Assert.False(ergebnis);
    }

    [Fact]
    public async Task Fehlschlag_wird_mit_Pausen_wiederholt()
    {
        using var temp = new TempOrdner();
        var uhr = Stopwatch.StartNew();

        // 3 Versuche → 2 Pausen à 150 ms zwischen den Versuchen
        bool ergebnis = await FuehreSchreibprobeDurch.Ausfuehren(Path.Combine(temp.Pfad, "fehlt"),
            TimeSpan.Zero, 3, TimeSpan.FromMilliseconds(150));

        Assert.False(ergebnis);
        Assert.True(uhr.Elapsed >= TimeSpan.FromMilliseconds(290), $"nur {uhr.ElapsedMilliseconds} ms – zu wenige Versuche");
    }
}
