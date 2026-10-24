using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Daten.Datensicherung;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Tests für <see cref="FuehreStartsicherungDurch"/>: Ablauf über mehrere Programmstarts.
/// </summary>
public class FuehreStartsicherungDurchTests
{
    /// <summary>Zeitpunkt des ersten simulierten Starts; jeder weitere Start liegt eine Minute später.</summary>
    private static readonly DateTime Beginn = new(2026, 10, 23, 7, 0, 0);

    /// <summary>Simuliert <paramref name="anzahl"/> Programmstarts und liefert die Ergebnisse.</summary>
    private static async Task<List<Startsicherungsergebnis>> Starte(string datenordner, int anzahl, int ersterStart = 1)
    {
        var ergebnisse = new List<Startsicherungsergebnis>();
        for (int start = ersterStart; start < ersterStart + anzahl; start++)
        {
            ergebnisse.Add(await FuehreStartsicherungDurch.Ausfuehren(datenordner, Beginn.AddMinutes(start)));
        }

        return ergebnisse;
    }

    /// <summary>Speichert Einstellungen in der Datenbank des Datenordners.</summary>
    private static void Stelle(string datenordner, Sicherungseinstellungen einstellungen)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei));
        SicherungEinstellungen.Speichern(verbindung, einstellungen);
    }

    /// <summary>Liest die Einstellungen aus der Datenbank des Datenordners.</summary>
    private static Sicherungseinstellungen Lies(string datenordner)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei));
        return SicherungEinstellungen.Laden(verbindung);
    }

    [Fact]
    public async Task Ohne_Einstellungen_wird_beim_dritten_Start_in_den_Ordner_Sicherungen_gesichert()
    {
        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));
        string standardordner = Path.Combine(temp.Pfad, "Sicherungen");

        List<Startsicherungsergebnis> ergebnisse = await Starte(temp.Pfad, 3);

        Assert.Equal([Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.Gesichert],
            ergebnisse.Select(e => e.Status));
        Assert.Equal(Path.Combine(standardordner, "andytours_2026-10-23_07-03-00.db"), ergebnisse[2].Sicherungsdatei);
        Assert.Single(Directory.GetFiles(standardordner));
    }

    [Fact]
    public async Task Nach_einer_Sicherung_beginnt_die_Zaehlung_von_vorn()
    {
        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));

        List<Startsicherungsergebnis> ergebnisse = await Starte(temp.Pfad, 6);

        Assert.Equal(
            [
                Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.Gesichert,
                Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.Gesichert,
            ],
            ergebnisse.Select(e => e.Status));
        Assert.Equal(0, Lies(temp.Pfad).StartsSeitSicherung);
    }

    [Fact]
    public async Task Gewaehlter_Ordner_Intervall_und_Hoechstzahl_werden_beachtet()
    {
        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));
        string ordner = Path.Combine(temp.Pfad, "Anderswo");
        Stelle(temp.Pfad, new Sicherungseinstellungen { Sicherungsordner = ordner, Intervall = 1, Hoechstzahl = 2 });

        List<Startsicherungsergebnis> ergebnisse = await Starte(temp.Pfad, 3);

        Assert.All(ergebnisse, e => Assert.Equal(Startsicherungsstatus.Gesichert, e.Status));
        // Nur die zwei neuesten bleiben; der Standardordner wird nicht angelegt
        Assert.Equal([ergebnisse[1].Sicherungsdatei, ergebnisse[2].Sicherungsdatei], Directory.GetFiles(ordner).Order());
        Assert.False(Directory.Exists(Path.Combine(temp.Pfad, "Sicherungen")));
    }

    [Fact]
    public async Task Ohne_Touren_wird_nie_gesichert()
    {
        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(0));

        List<Startsicherungsergebnis> ergebnisse = await Starte(temp.Pfad, 5);

        Assert.All(ergebnisse, e => Assert.Equal(Startsicherungsstatus.NichtFaellig, e.Status));
        Assert.False(Directory.Exists(Path.Combine(temp.Pfad, "Sicherungen")));
    }

    [Fact]
    public async Task Nicht_erreichbarer_Ordner_meldet_Fehler_und_der_naechste_Start_versucht_es_erneut()
    {
        string? frei = TestLaufwerke.FreierLaufwerksbuchstabe();
        if (frei is null)
        {
            return;
        }

        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));
        Stelle(temp.Pfad, new Sicherungseinstellungen { Sicherungsordner = $@"{frei}:\Sicherungen", Intervall = 2 });

        List<Startsicherungsergebnis> ergebnisse = await Starte(temp.Pfad, 3);

        Assert.Equal([Startsicherungsstatus.NichtFaellig, Startsicherungsstatus.Fehlgeschlagen, Startsicherungsstatus.Fehlgeschlagen],
            ergebnisse.Select(e => e.Status));
        Assert.Contains("nicht erreichbar", ergebnisse[1].Fehlergrund);
        Assert.Equal(3, Lies(temp.Pfad).StartsSeitSicherung);

        // Ordner wieder erreichbar (hier: auf den Standardordner zurückgestellt) → der nächste Start sichert sofort
        Stelle(temp.Pfad, Lies(temp.Pfad) with { Sicherungsordner = null });
        Assert.Equal(Startsicherungsstatus.Gesichert, (await Starte(temp.Pfad, 1, ersterStart: 4))[0].Status);
    }

    [Fact]
    public async Task Ordner_ohne_Schreibrecht_meldet_Fehler()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));
        string ordner = Path.Combine(temp.Pfad, "Gesperrt");
        DirectoryInfo info = Directory.CreateDirectory(ordner);
        Stelle(temp.Pfad, new Sicherungseinstellungen { Sicherungsordner = ordner, Intervall = 1 });

        // Dem aktuellen Benutzer das Anlegen von Dateien in diesem Ordner verweigern
        var benutzer = WindowsIdentity.GetCurrent().User!;
        var verbot = new FileSystemAccessRule(benutzer,
            FileSystemRights.CreateFiles | FileSystemRights.WriteData | FileSystemRights.AppendData,
            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
        DirectorySecurity rechte = info.GetAccessControl();
        rechte.AddAccessRule(verbot);
        info.SetAccessControl(rechte);

        try
        {
            Startsicherungsergebnis ergebnis = (await Starte(temp.Pfad, 1))[0];

            Assert.Equal(Startsicherungsstatus.Fehlgeschlagen, ergebnis.Status);
            Assert.Contains("nicht schreiben", ergebnis.Fehlergrund);
            Assert.Equal(1, Lies(temp.Pfad).StartsSeitSicherung);
        }
        finally
        {
            rechte.RemoveAccessRule(verbot);
            info.SetAccessControl(rechte);
        }
    }

    [Fact]
    public async Task Zeitueberschreitung_meldet_Fehler()
    {
        using var temp = new TempOrdner();
        TestDatenbestand.SpeichereIn(temp.Pfad, TestDatenbestand.Erzeuge(3));
        Stelle(temp.Pfad, new Sicherungseinstellungen { Intervall = 1 });

        // Zeitlimit 0 ist sofort überschritten – simuliert ein hängendes NAS
        Startsicherungsergebnis ergebnis = await FuehreStartsicherungDurch.Ausfuehren(temp.Pfad, Beginn, TimeSpan.Zero);

        Assert.Equal(Startsicherungsstatus.Fehlgeschlagen, ergebnis.Status);
        Assert.Contains("nicht innerhalb von 0 Sekunden", ergebnis.Fehlergrund);
    }

    [Fact]
    public async Task Gesichert_wird_die_Datenbank_des_uebergebenen_Datenordners()
    {
        using var alt = new TempOrdner();
        using var neu = new TempOrdner();
        TestDatenbestand.SpeichereIn(alt.Pfad, TestDatenbestand.Erzeuge(2, "Alter Ordner"));
        TestDatenbestand.SpeichereIn(neu.Pfad, TestDatenbestand.Erzeuge(4, "Neuer Ordner"));
        Stelle(alt.Pfad, new Sicherungseinstellungen { Intervall = 1 });
        Stelle(neu.Pfad, new Sicherungseinstellungen { Intervall = 1 });

        Startsicherungsergebnis vorher = (await Starte(alt.Pfad, 1))[0];
        Startsicherungsergebnis nachher = (await Starte(neu.Pfad, 1, ersterStart: 2))[0];

        Assert.Equal(Path.Combine(neu.Pfad, "Sicherungen"), nachher.Sicherungsordner);
        Assert.All(TestDatenbestand.Lade(vorher.Sicherungsdatei!).Trainingseinheiten, e => Assert.Equal("Alter Ordner", e.Name));
        Datenbestand gesichert = TestDatenbestand.Lade(nachher.Sicherungsdatei!);
        Assert.Equal(4, gesichert.Trainingseinheiten.Count);
        Assert.All(gesichert.Trainingseinheiten, e => Assert.Equal("Neuer Ordner", e.Name));
    }
}
