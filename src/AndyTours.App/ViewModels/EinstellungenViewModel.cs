using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Daten.Datensicherung;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Einstellungsfensters.
/// </summary>
/// <remarks>
/// Es enthält zwei Bereiche:
/// <list type="bullet">
/// <item><b>Datenordner:</b> Der eigentliche Wechsel (Ordnerwahl, Rückfragen, Kopieren, Laden, Merken)
/// wird vom Hauptfenster-ViewModel als Funktion übergeben, weil dieses den Datenbestand hält und neu
/// laden muss.</item>
/// <item><b>Datensicherung:</b> Sicherungsordner, Intervall und Höchstzahl. Sie liegen in der Datenbank
/// des aktuellen Datenordners und werden sofort gespeichert, sobald ein gültiger Wert eingegeben bzw.
/// ein Ordner gewählt ist. Nach einem Wechsel des Datenordners werden die Werte des neuen Ordners
/// angezeigt.</item>
/// </list>
/// </remarks>
public partial class EinstellungenViewModel : ObservableObject
{
    /// <summary>Erklärung, warum der Datenordner in der Entwicklungsversion nicht änderbar ist.</summary>
    public const string Entwicklungshinweis =
        "Diese Entwicklungsversion verwendet immer den Ordner „AppDaten“ im Projektordner, damit Ihre " +
        "echten Daten unberührt bleiben. Der Datenordner lässt sich nur in der fertigen Programmversion ändern.";

    /// <summary>Hinweis bei einer ungültigen Eingabe für Intervall oder Höchstzahl.</summary>
    public static readonly string Bereichshinweis = string.Create(CultureInfo.InvariantCulture,
        $"Bitte eine ganze Zahl von {Sicherungseinstellungen.Minimalwert} bis {Sicherungseinstellungen.Maximalwert} eingeben.");

    /// <summary>Führt den Ordnerwechsel durch; liefert den neuen Ordner oder <c>null</c>.</summary>
    private readonly Func<Task<string?>> _ordnerWechseln;

    /// <summary><c>true</c>, solange gespeicherte Werte in die Felder geschrieben werden (dann nicht speichern).</summary>
    private bool _wirdAngezeigt;

    /// <summary>
    /// Erzeugt das ViewModel und liest die Sicherungseinstellungen des Datenordners.
    /// </summary>
    /// <param name="datenordner">Der aktuelle Datenordner.</param>
    /// <param name="aendernMoeglich"><c>false</c> in der Entwicklungsversion.</param>
    /// <param name="ordnerWechseln">Ablauf des Ordnerwechsels (vom Hauptfenster-ViewModel).</param>
    public EinstellungenViewModel(string datenordner, bool aendernMoeglich, Func<Task<string?>> ordnerWechseln)
    {
        _datenordner = datenordner;
        AendernMoeglich = aendernMoeglich;
        _ordnerWechseln = ordnerWechseln;
        ZeigeSicherungseinstellungen();
    }

    /// <summary>Vollständiger Pfad des aktuellen Datenordners.</summary>
    [ObservableProperty]
    private string _datenordner;

    /// <summary><c>true</c>, wenn der Datenordner geändert werden darf (nur in der fertigen Programmversion).</summary>
    public bool AendernMoeglich { get; }

    /// <summary><c>true</c>, wenn der <see cref="Entwicklungshinweis"/> angezeigt werden soll.</summary>
    public bool HinweisSichtbar => !AendernMoeglich;

    /// <summary>Text des Hinweises (für die Datenbindung).</summary>
    public string Hinweis => Entwicklungshinweis;

    /// <summary>Vollständiger Pfad des Sicherungsordners (gewählt oder Standard).</summary>
    [ObservableProperty]
    private string _sicherungsordner = string.Empty;

    /// <summary>Eingabefeld „Sichern nach … Programmstarts“.</summary>
    [ObservableProperty]
    private string _intervallText = string.Empty;

    /// <summary>Eingabefeld „Höchstens … Sicherungen aufbewahren“.</summary>
    [ObservableProperty]
    private string _hoechstzahlText = string.Empty;

    /// <summary>Hinweis unter dem Intervall-Feld; leer, solange die Eingabe gültig ist.</summary>
    [ObservableProperty]
    private string _intervallHinweis = string.Empty;

    /// <summary>Hinweis unter dem Höchstzahl-Feld; leer, solange die Eingabe gültig ist.</summary>
    [ObservableProperty]
    private string _hoechstzahlHinweis = string.Empty;

    /// <summary>
    /// Befehl „Ändern…“ beim Datenordner: startet den Ordnerwechsel und zeigt danach den neuen Ordner
    /// und dessen Sicherungseinstellungen an.
    /// </summary>
    /// <remarks>
    /// Nur ausführbar, wenn <see cref="AendernMoeglich"/> gilt – dadurch ist die Schaltfläche in
    /// der Entwicklungsversion gesperrt.
    /// </remarks>
    [RelayCommand(CanExecute = nameof(AendernMoeglich))]
    private async Task Aendern()
    {
        string? neu = await _ordnerWechseln();
        if (neu is not null)
        {
            Datenordner = neu;
            ZeigeSicherungseinstellungen();
        }
    }

    /// <summary>
    /// Befehl „Ändern…“ beim Sicherungsordner: Ordner wählen und sofort speichern.
    /// </summary>
    /// <remarks>
    /// Der Ordner wird hier nicht geprüft; ist er später nicht erreichbar oder nicht beschreibbar,
    /// meldet das die Sicherung beim Programmstart. Anders als der Datenordner ist der
    /// Sicherungsordner auch in der Entwicklungsversion änderbar.
    /// </remarks>
    [RelayCommand]
    private void SicherungsordnerAendern()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Ordner für die Datensicherung wählen",
            InitialDirectory = Directory.Exists(Sicherungsordner) ? Sicherungsordner : string.Empty,
        };
        Window? besitzer = Application.Current?.Windows.OfType<Window>().FirstOrDefault(f => f.IsActive);
        bool? gewaehlt = besitzer is null ? dialog.ShowDialog() : dialog.ShowDialog(besitzer);
        if (gewaehlt != true)
        {
            return; // Auswahl abgebrochen – bisheriger Ordner bleibt
        }

        string ordner = Path.GetFullPath(dialog.FolderName);
        if (Speichere(e => e with { Sicherungsordner = ordner }))
        {
            Sicherungsordner = ordner;
        }
    }

    /// <summary>Wird bei jeder Änderung im Intervall-Feld aufgerufen: gültige Werte sofort speichern.</summary>
    partial void OnIntervallTextChanged(string value)
    {
        int? wert = PruefeSicherungswert.Ausfuehren(value);
        IntervallHinweis = wert is null ? Bereichshinweis : string.Empty;
        if (!_wirdAngezeigt && wert is int intervall)
        {
            Speichere(e => e with { Intervall = intervall });
        }
    }

    /// <summary>Wird bei jeder Änderung im Höchstzahl-Feld aufgerufen: gültige Werte sofort speichern.</summary>
    partial void OnHoechstzahlTextChanged(string value)
    {
        int? wert = PruefeSicherungswert.Ausfuehren(value);
        HoechstzahlHinweis = wert is null ? Bereichshinweis : string.Empty;
        if (!_wirdAngezeigt && wert is int hoechstzahl)
        {
            Speichere(e => e with { Hoechstzahl = hoechstzahl });
        }
    }

    /// <summary>
    /// Liest die Sicherungseinstellungen des aktuellen Datenordners und zeigt sie an.
    /// </summary>
    /// <remarks>
    /// Während des Anzeigens ist <see cref="_wirdAngezeigt"/> gesetzt, damit das Füllen der Felder
    /// nicht gleich wieder ein Speichern auslöst. Ist die Datenbank nicht lesbar, werden die
    /// Standardwerte angezeigt.
    /// </remarks>
    private void ZeigeSicherungseinstellungen()
    {
        Sicherungseinstellungen einstellungen;
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            einstellungen = SicherungEinstellungen.Laden(verbindung);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            einstellungen = new Sicherungseinstellungen();
        }

        _wirdAngezeigt = true;
        try
        {
            Sicherungsordner = ErmittleSicherungsordner.Ausfuehren(Datenordner, einstellungen);
            IntervallText = einstellungen.Intervall.ToString(CultureInfo.InvariantCulture);
            HoechstzahlText = einstellungen.Hoechstzahl.ToString(CultureInfo.InvariantCulture);
            IntervallHinweis = string.Empty;
            HoechstzahlHinweis = string.Empty;
        }
        finally
        {
            _wirdAngezeigt = false;
        }
    }

    /// <summary>
    /// Liest die aktuellen Einstellungen, ändert sie und speichert sie wieder.
    /// </summary>
    /// <remarks>
    /// Es wird jedes Mal frisch gelesen, damit der Startzähler unverändert bleibt.
    /// </remarks>
    /// <param name="aenderung">Die Änderung, z. B. <c>e =&gt; e with { Intervall = 5 }</c>.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde.</returns>
    private bool Speichere(Func<Sicherungseinstellungen, Sicherungseinstellungen> aenderung)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Datenbankdatei);
            SicherungEinstellungen.Speichern(verbindung, aenderung(SicherungEinstellungen.Laden(verbindung)));
            return true;
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Einstellung konnte nicht gespeichert werden:\n\n{fehler.Message}",
                "Einstellungen", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>Vollständiger Pfad der Datenbank im aktuellen Datenordner.</summary>
    private string Datenbankdatei => Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei);
}
