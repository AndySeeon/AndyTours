using System.Security.AccessControl;
using System.Security.Principal;
using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datenordner;

/// <summary>
/// Tests für <see cref="PruefeDatenordner"/>.
/// </summary>
public class PruefeDatenordnerTests
{
    [Fact]
    public async Task Fehlender_Ordner_wird_angelegt_und_ist_leer()
    {
        using var temp = new TempOrdner();
        string ordner = Path.Combine(temp.Pfad, "Dokumente", "AndyTours");

        DatenordnerZustand zustand = await PruefeDatenordner.Ausfuehren(ordner);

        Assert.Equal(DatenordnerZustand.Leer, zustand);
        Assert.True(Directory.Exists(ordner));
    }

    [Fact]
    public async Task Ordner_mit_Datenbank_hat_Daten()
    {
        using var temp = new TempOrdner();
        File.WriteAllText(Path.Combine(temp.Pfad, DatenordnerInhalt.Datenbankdatei), "");

        Assert.Equal(DatenordnerZustand.MitDaten, await PruefeDatenordner.Ausfuehren(temp.Pfad));
    }

    [Fact]
    public async Task Ordner_auf_fehlendem_Laufwerk_ist_nicht_erreichbar()
    {
        string? frei = TestLaufwerke.FreierLaufwerksbuchstabe();
        if (frei is null)
        {
            return;
        }

        Assert.Equal(DatenordnerZustand.NichtErreichbar, await PruefeDatenordner.Ausfuehren($@"{frei}:\AndyTours"));
    }

    [Fact]
    public async Task Ordner_ohne_Schreibrecht_ist_nicht_beschreibbar()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var temp = new TempOrdner();
        string ordner = Path.Combine(temp.Pfad, "Gesperrt");
        var info = Directory.CreateDirectory(ordner);

        // Dem aktuellen Benutzer das Anlegen und Schreiben von Dateien in diesem Ordner verweigern
        var benutzer = WindowsIdentity.GetCurrent().User!;
        var verbot = new FileSystemAccessRule(benutzer,
            FileSystemRights.CreateFiles | FileSystemRights.WriteData | FileSystemRights.AppendData,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
        DirectorySecurity rechte = info.GetAccessControl();
        rechte.AddAccessRule(verbot);
        info.SetAccessControl(rechte);

        try
        {
            Assert.Equal(DatenordnerZustand.NichtBeschreibbar, await PruefeDatenordner.Ausfuehren(ordner));
        }
        finally
        {
            // Verbot wieder entfernen, damit der Temp-Ordner aufgeräumt werden kann
            rechte.RemoveAccessRule(verbot);
            info.SetAccessControl(rechte);
        }
    }

    [Fact]
    public async Task Zeitueberschreitung_gilt_als_nicht_erreichbar()
    {
        using var temp = new TempOrdner();

        // Ein Zeitlimit von 0 ist sofort überschritten – simuliert ein hängendes NAS
        Assert.Equal(DatenordnerZustand.NichtErreichbar, await PruefeDatenordner.Ausfuehren(temp.Pfad, TimeSpan.Zero));
    }
}
