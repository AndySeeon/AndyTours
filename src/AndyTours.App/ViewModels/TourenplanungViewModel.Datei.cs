using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des ViewModels „Tour planen“: Speichern, Öffnen und die Rückfrage bei ungespeicherten Änderungen.
/// </summary>
/// <remarks>
/// Eine Planung wird als GPX-Datei gespeichert (<see cref="PlanungsGpx"/>). Vorgeschlagen werden beim ersten
/// Speichern der zuletzt verwendete Ordner und „Planung JJJJ-MM-TT.gpx“, nach dem Öffnen oder Speichern Ordner und
/// Name dieser Datei. Die Rückfrage vor dem Überschreiben stellt der Speicherdialog von Windows.
/// </remarks>
public partial class TourenplanungViewModel
{
    /// <summary>Dateifilter für die Dialoge.</summary>
    private const string Dateifilter = "GPX-Dateien (*.gpx)|*.gpx";

    /// <summary>Hinweis, wenn eine gewählte Datei keine Planung von AndyTours ist.</summary>
    public const string KeinePlanung =
        "Die Datei ist keine von AndyTours gespeicherte Planung oder konnte nicht gelesen werden. Die aktuelle Planung " +
        "bleibt unverändert.";

    /// <summary>Pfad der zuletzt geöffneten oder gespeicherten Datei; <c>null</c> bei einer neuen Planung.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Fenstertitel))]
    private string? _dateipfad;

    /// <summary>Titel des Fensters, mit dem Namen der Datei, sobald es eine gibt.</summary>
    public string Fenstertitel => Dateipfad is null ? Titel : $"{Titel} – {Path.GetFileNameWithoutExtension(Dateipfad)}";

    /// <summary><c>true</c>, wenn die Planung gespeichert werden kann (mindestens zwei Wegpunkte).</summary>
    public bool KannSpeichern => Planung.Wegpunkte.Count >= 2;

    /// <summary>
    /// Fragt bei ungespeicherten Änderungen, ob sie verworfen werden sollen (vor dem Schließen des Fensters).
    /// </summary>
    /// <returns><c>true</c>, wenn es nichts zu verlieren gibt oder der Benutzer das Verwerfen bestätigt.</returns>
    public bool DarfVerwerfen()
    {
        if (!IstGeaendert)
        {
            return true;
        }

        MessageBoxResult antwort = MessageBox.Show("Die Planung hat ungespeicherte Änderungen. Änderungen verwerfen?",
            Titel, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
        return antwort == MessageBoxResult.Yes;
    }

    /// <summary>
    /// Bricht eine laufende Ermittlung ab (beim Schließen des Fensters).
    /// </summary>
    public void BeendeErmittlung()
    {
        _ermittlung?.Cancel();
        _ermittlung?.Dispose();
        _ermittlung = null;
    }

    /// <summary>Befehl „Speichern…“: Datei wählen und die Planung als GPX schreiben.</summary>
    [RelayCommand(CanExecute = nameof(KannSpeichern))]
    private void Speichern()
    {
        var dialog = new SaveFileDialog
        {
            Title = "Planung speichern",
            Filter = Dateifilter,
            DefaultExt = ".gpx",
            OverwritePrompt = true,
            FileName = Dateipfad is null ? BildePlanungsdateiname.Ausfuehren(DateOnly.FromDateTime(DateTime.Today)) : Path.GetFileName(Dateipfad),
            InitialDirectory = Startordner(),
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            string inhalt = PlanungsGpx.Schreiben(Planung, Path.GetFileNameWithoutExtension(dialog.FileName));
            File.WriteAllText(dialog.FileName, inhalt, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Planung konnte nicht gespeichert werden: {fehler.Message}", Titel,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Dateipfad = dialog.FileName;
        IstGeaendert = false;
        MerkeOrdner(dialog.FileName);
    }

    /// <summary>Befehl „Öffnen…“: eine gespeicherte Planung laden und weiterbearbeiten.</summary>
    [RelayCommand]
    private void Oeffnen()
    {
        if (!DarfVerwerfen())
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = "Planung öffnen",
            Filter = Dateifilter,
            InitialDirectory = Startordner(),
        };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        Tourenplanung? gelesen;
        try
        {
            gelesen = PlanungsGpx.Lesen(File.ReadAllText(dialog.FileName));
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            gelesen = null;
        }

        if (gelesen is null)
        {
            MessageBox.Show(KeinePlanung, Titel, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        BeendeErmittlung();
        _wirdGeladen = true;
        try
        {
            Profil = gelesen.Profil;
        }
        finally
        {
            _wirdGeladen = false;
        }

        Hinweis = string.Empty;
        ErmitteltGerade = false;
        Uebernimm(gelesen);
        Dateipfad = dialog.FileName;
        IstGeaendert = false;
        MerkeOrdner(dialog.FileName);
        GanzePlanungAngefordert?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Ordner, in dem die Dialoge beginnen: der der geöffneten Datei, sonst der gemerkte, sonst „Dokumente“.</summary>
    private string Startordner()
    {
        string? ordner = Path.GetDirectoryName(Dateipfad) ?? _einstellungen.Ordner;
        return ordner is not null && Directory.Exists(ordner) ? ordner : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    /// <summary>Merkt den Ordner einer gespeicherten oder geöffneten Datei.</summary>
    private void MerkeOrdner(string datei)
    {
        if (Path.GetDirectoryName(datei) is string ordner)
        {
            Merke(_einstellungen with { Ordner = ordner });
        }
    }
}
