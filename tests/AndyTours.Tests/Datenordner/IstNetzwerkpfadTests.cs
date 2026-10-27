using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="IstNetzwerkpfad"/>.
/// </summary>
public class IstNetzwerkpfadTests
{
    [Theory]
    [InlineData(@"\\AndyNAS-2\Backfast\AndyTours")]
    [InlineData(@"\\?\UNC\AndyNAS-2\Backfast\AndyTours")]
    public void UNC_Pfade_sind_Netzwerkpfade(string pfad)
    {
        Assert.True(IstNetzwerkpfad.Ausfuehren(pfad));
    }

    [Fact]
    public void Temp_Ordner_ist_kein_Netzwerkpfad()
    {
        Assert.False(IstNetzwerkpfad.Ausfuehren(Path.GetTempPath()));
    }

    [Fact]
    public void Fehlendes_Laufwerk_ist_kein_Netzwerkpfad()
    {
        string? frei = TestLaufwerke.FreierLaufwerksbuchstabe();
        if (frei is null)
        {
            return; // alle Buchstaben belegt – Test hier nicht durchführbar
        }

        Assert.False(IstNetzwerkpfad.Ausfuehren($@"{frei}:\AndyTours"));
    }

    [Fact]
    public void Verbundenes_Netzlaufwerk_wird_erkannt()
    {
        // Nur prüfbar, wenn auf dem Rechner ein Netzlaufwerk verbunden ist (z. B. Z: → NAS)
        DriveInfo? netzlaufwerk = DriveInfo.GetDrives().FirstOrDefault(l => l.DriveType == DriveType.Network);
        if (netzlaufwerk is null)
        {
            return;
        }

        Assert.True(IstNetzwerkpfad.Ausfuehren(Path.Combine(netzlaufwerk.RootDirectory.FullName, "AndyTours")));
    }
}
