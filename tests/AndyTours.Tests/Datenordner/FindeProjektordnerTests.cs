using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="FindeProjektordner"/>.
/// </summary>
public class FindeProjektordnerTests
{
    [Fact]
    public void Projektmappe_wird_mehrere_Ebenen_hoeher_gefunden()
    {
        using var temp = new TempOrdner();
        File.WriteAllText(Path.Combine(temp.Pfad, DatenordnerInhalt.Projektmappe), "<Solution />");
        string programmordner = Path.Combine(temp.Pfad, "src", "AndyTours.App", "bin", "Debug");
        Directory.CreateDirectory(programmordner);

        Assert.Equal(temp.Pfad, FindeProjektordner.Ausfuehren(programmordner));
    }

    [Fact]
    public void Ohne_Projektmappe_wird_null_geliefert()
    {
        using var temp = new TempOrdner();

        Assert.Null(FindeProjektordner.Ausfuehren(temp.Pfad));
    }

    [Fact]
    public void Echter_Projektordner_wird_vom_Testprogramm_aus_gefunden()
    {
        // Die Tests laufen in tests\AndyTours.Tests\bin\… – darüber liegt die echte Projektmappe.
        string? projektordner = FindeProjektordner.Ausfuehren(AppContext.BaseDirectory);

        Assert.NotNull(projektordner);
        Assert.True(File.Exists(Path.Combine(projektordner, DatenordnerInhalt.Projektmappe)));
    }
}
