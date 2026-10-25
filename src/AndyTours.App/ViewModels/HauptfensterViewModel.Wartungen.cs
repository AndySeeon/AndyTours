using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die Karteikarte „Wartung“ – Tabelle der Wartungen mit km-Stand,
/// Radauswahl, Markierung und die Befehle „Neue Wartung…“, „Bearbeiten…“ und „Löschen“.
/// </summary>
/// <remarks>
/// Grundsatz wie bei den Einheiten: Erst wird in die Datenbank geschrieben, erst danach der Datenbestand im
/// Speicher geändert und die Tabelle neu aufgebaut. Schlägt das Schreiben fehl, erscheint eine Fehlermeldung
/// (über <see cref="Schreibe"/>), und die Tabelle bleibt beim vorherigen Stand.
/// <para>
/// Die Zeilen werden nach jeder Änderung an Einheiten, Rädern, Wartungen oder der Liste „Tätigkeit“ neu
/// aufgebaut (<see cref="AktualisiereWartungen"/>); so bleibt der km-Stand immer aktuell. Die Alarmliste
/// (<c>HauptfensterViewModel.Alarme.cs</c>) folgt derselben Radauswahl und wird jedes Mal mit aufgebaut.
/// </para>
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Eintrag der Radauswahl für „keine Einschränkung“.</summary>
    private static readonly Auswahleintrag AlleRaeder = new(null, "Alle Räder");

    /// <summary>Titel der Meldungsfenster der Karteikarte „Wartung“.</summary>
    private const string Wartungstitel = "Wartung";

    /// <summary>
    /// Solange <c>true</c>, löst ein Wechsel des gewählten Rads keinen Neuaufbau aus
    /// (während die Radauswahl neu gefüllt wird).
    /// </summary>
    private bool _wartungsraederWerdenGefuellt;

    /// <summary>
    /// Wird ausgelöst, wenn die Wartungstabelle eine Wartung markieren und in den sichtbaren Bereich holen soll
    /// (nach dem Speichern aus dem Wartungsfenster).
    /// </summary>
    public event Action<long>? MarkiereWartung;

    /// <summary>Die Zeilen der Wartungstabelle (gefiltert nach dem gewählten Rad, die neueste oben).</summary>
    public ObservableCollection<Wartungszeile> Wartungszeilen { get; } = new();

    /// <summary>
    /// Einträge der Radauswahl über der Tabelle: „Alle Räder“, dann die Räder im Einsatz alphabetisch und am Ende
    /// die ausgemusterten mit dem Zusatz „(ausgemustert)“.
    /// </summary>
    public ObservableCollection<Auswahleintrag> Wartungsraeder { get; } = new();

    /// <summary>
    /// Das oben gewählte Rad; <see cref="AlleRaeder"/> zeigt alle Wartungen. <c>null</c> nur kurz, während die
    /// Radauswahl neu gefüllt wird (die Auswahlliste der Oberfläche verliert dabei ihre Wahl); es gilt wie „Alle Räder“.
    /// </summary>
    [ObservableProperty]
    private Auswahleintrag? _gewaehltesWartungsrad = AlleRaeder;

    /// <summary><c>true</c>, wenn die Tabelle Zeilen hat; sonst steht dort <see cref="Wartungshinweis"/>.</summary>
    [ObservableProperty]
    private bool _hatWartungszeilen;

    /// <summary>Hinweis statt der leeren Tabelle, z. B. „Noch keine Wartungen erfasst.“</summary>
    [ObservableProperty]
    private string _wartungshinweis = string.Empty;

    /// <summary>
    /// Nummern der in der Wartungstabelle markierten Wartungen; die Tabelle setzt sie bei jeder Änderung der Markierung.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(WartungBearbeitenCommand))]
    [NotifyCanExecuteChangedFor(nameof(WartungenLoeschenCommand))]
    private IReadOnlyList<long> _markierteWartungen = [];

    /// <summary>
    /// <c>true</c>, solange die Karteikarte „Wartung“ gewählt ist (gebunden an deren <c>IsSelected</c>);
    /// die Statusleiste nennt dann die Anzahl der Wartungen statt der Einheiten.
    /// </summary>
    [ObservableProperty]
    private bool _istWartungGewaehlt;

    /// <summary>Karteikarte gewechselt: Statusleiste anpassen.</summary>
    partial void OnIstWartungGewaehltChanged(bool value) => AktualisiereStatustext();

    /// <summary>Anzahl aller Wartungen im Datenbestand (ohne Alarme, wie in der Tabelle).</summary>
    private int ZaehleWartungen() => _bestand.Wartungen.Count(w => w.Typ == WartungsTyp.Wartung);

    /// <summary>Ein anderes Rad gewählt: Tabelle neu füllen.</summary>
    partial void OnGewaehltesWartungsradChanged(Auswahleintrag? value)
    {
        if (!_wartungsraederWerdenGefuellt)
        {
            FuelleWartungszeilen();
        }
    }

    /// <summary>
    /// Befehl „Neue Wartung…“: öffnet das Wartungsfenster mit dem heutigen Datum; ist oben ein Rad gewählt,
    /// ist es vorgewählt.
    /// </summary>
    [RelayCommand]
    private void NeueWartung()
    {
        var neu = new Wartung
        {
            Datum = DateOnly.FromDateTime(DateTime.Today),
            EquipmentId = GewaehltesWartungsrad?.Id,
            Typ = WartungsTyp.Wartung,
        };
        OeffneWartungsfenster(neu, "Neue Wartung");
    }

    /// <summary>Befehl „Bearbeiten…“ (auch Doppelklick): öffnet das Wartungsfenster der markierten Wartung.</summary>
    [RelayCommand(CanExecute = nameof(GenauEineWartungMarkiert))]
    private void WartungBearbeiten()
    {
        if (FindeWartung(MarkierteWartungen[0]) is Wartung wartung)
        {
            OeffneWartungsfenster(wartung, "Wartung bearbeiten");
        }
    }

    /// <summary>
    /// Befehl „Löschen“ (auch Entf-Taste): löscht die markierten Wartungen nach einer Rückfrage mit der Anzahl.
    /// </summary>
    [RelayCommand(CanExecute = nameof(MindestensEineWartungMarkiert))]
    private void WartungenLoeschen()
    {
        List<Wartung> wartungen = MarkierteWartungen.Select(FindeWartung).OfType<Wartung>().ToList();
        if (wartungen.Count == 0)
        {
            return;
        }

        string frage = string.Format(CultureInfo.GetCultureInfo("de-DE"), "{0:N0} {1} wirklich löschen?",
            wartungen.Count, wartungen.Count == 1 ? "Wartung" : "Wartungen");
        if (MessageBox.Show(frage, Wartungstitel, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
        {
            return;
        }

        var ids = wartungen.Select(w => w.Id).ToHashSet();
        if (!Schreibe(verbindung => WartungenTabelle.Loeschen(verbindung, ids), "Die Wartungen konnten nicht gelöscht werden"))
        {
            return;
        }

        _bestand.Wartungen.RemoveAll(w => ids.Contains(w.Id));
        FuelleWartungszeilen();
    }

    /// <summary>Wählbarkeit von „Bearbeiten…“: genau eine Wartung markiert.</summary>
    private bool GenauEineWartungMarkiert() => MarkierteWartungen.Count == 1;

    /// <summary>Wählbarkeit von „Löschen“: mindestens eine Wartung markiert.</summary>
    private bool MindestensEineWartungMarkiert() => MarkierteWartungen.Count > 0;

    /// <summary>
    /// Öffnet das Wartungsfenster für eine neue oder vorhandene Wartung; die Liste „Tätigkeit“ und die Räder
    /// werden dabei jedes Mal neu ermittelt, damit das Fenster den aktuellen Stand anbietet.
    /// </summary>
    /// <param name="wartung">Die zu bearbeitende Wartung (bei „Neu“ mit der Nummer 0).</param>
    /// <param name="titel">Fenstertitel.</param>
    private void OeffneWartungsfenster(Wartung wartung, string titel)
    {
        // Bei „Neu“ nur Räder im Einsatz; beim Bearbeiten bleibt das bisherige (ggf. ausgemusterte) Rad wählbar
        long? bisheriges = wartung.Id == 0 ? null : wartung.EquipmentId;
        var viewModel = new WartungBearbeitenViewModel(
            wartung,
            ErmittleWartungsraeder.Ausfuehren(_bestand, bisheriges),
            [.. _taetigkeiten],
            titel,
            SpeichereWartung,
            DateOnly.FromDateTime(DateTime.Today));
        var fenster = new WartungBearbeitenFenster(viewModel) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }

    /// <summary>
    /// Speichert eine Wartung aus dem Wartungsfenster (neu oder geändert) und zeigt sie in der Tabelle.
    /// </summary>
    /// <remarks>
    /// Ist oben ein anderes Rad gewählt als das der Wartung, wechselt die Auswahl auf „Alle Räder“, damit die
    /// Wartung sofort sichtbar ist. Danach wird sie markiert.
    /// </remarks>
    /// <param name="wartung">Die fertige Wartung; bei einer neuen ist die Nummer 0.</param>
    /// <returns><c>true</c> bei Erfolg.</returns>
    private bool SpeichereWartung(Wartung wartung)
    {
        bool istNeu = wartung.Id == 0;
        if (!Schreibe(verbindung =>
            {
                if (istNeu)
                {
                    wartung.Id = WartungenTabelle.Einfuegen(verbindung, wartung);
                }
                else
                {
                    WartungenTabelle.Aendern(verbindung, wartung);
                }
            }, "Die Wartung konnte nicht gespeichert werden"))
        {
            if (istNeu)
            {
                wartung.Id = 0; // nichts gespeichert: beim nächsten Versuch wieder als neue Wartung
            }

            return false;
        }

        if (istNeu)
        {
            _bestand.Wartungen.Add(wartung);
        }
        else
        {
            int stelle = _bestand.Wartungen.FindIndex(w => w.Id == wartung.Id);
            if (stelle >= 0)
            {
                _bestand.Wartungen[stelle] = wartung;
            }
        }

        if (GewaehltesWartungsrad?.Id is long radId && radId != wartung.EquipmentId)
        {
            GewaehltesWartungsrad = AlleRaeder; // füllt die Tabelle neu
        }
        else
        {
            FuelleWartungszeilen();
        }

        MarkiereWartung?.Invoke(wartung.Id);
        return true;
    }

    /// <summary>
    /// Baut Radauswahl und Tabelle der Karteikarte „Wartung“ neu auf; das gewählte Rad bleibt (über seine
    /// Nummer) erhalten, solange es das Rad noch gibt.
    /// </summary>
    /// <remarks>
    /// Läuft nach jedem Laden des Datenbestands und nach Änderungen an Einheiten, Rädern und Tätigkeiten
    /// (siehe <see cref="AktualisiereSummen"/>).
    /// </remarks>
    private void AktualisiereWartungen()
    {
        long? gewaehlt = GewaehltesWartungsrad?.Id;
        _wartungsraederWerdenGefuellt = true;
        try
        {
            Wartungsraeder.Clear();
            Wartungsraeder.Add(AlleRaeder);
            // Räder im Einsatz alphabetisch, danach die ausgemusterten mit Zusatz „(ausgemustert)“
            foreach (Auswahleintrag rad in ErmittleWartungsraeder.Ausfuehren(_bestand, null, alleRaeder: true))
            {
                Wartungsraeder.Add(rad);
            }

            // Die Auswahlliste der Oberfläche hat die Wahl beim Leeren verloren – auch bei gleichem Wert neu melden
            GewaehltesWartungsrad = Wartungsraeder.FirstOrDefault(r => r.Id == gewaehlt) ?? AlleRaeder;
            OnPropertyChanged(nameof(GewaehltesWartungsrad));
        }
        finally
        {
            _wartungsraederWerdenGefuellt = false;
        }

        FuelleWartungszeilen();
    }

    /// <summary>
    /// Füllt die Tabelle mit den Wartungen des gewählten Rads (dieselbe Sammlung, damit die gewählte
    /// Sortierung erhalten bleibt) und setzt den Hinweis für eine leere Tabelle; die Alarmliste wird mit
    /// aufgebaut, weil jede Wartung einen Alarm zurücksetzen kann.
    /// </summary>
    private void FuelleWartungszeilen()
    {
        List<Wartungszeile> zeilen = ErstelleWartungszeilen.Ausfuehren(_bestand, GewaehltesWartungsrad?.Id);
        Wartungszeilen.Clear();
        foreach (Wartungszeile zeile in zeilen)
        {
            Wartungszeilen.Add(zeile);
        }

        HatWartungszeilen = zeilen.Count > 0;
        AktualisiereStatustext(); // die Anzahl der Wartungen kann sich geändert haben
        Wartungshinweis = ZaehleWartungen() > 0
            ? "Keine Wartungen für dieses Rad."
            : "Noch keine Wartungen erfasst.";

        FuelleAlarmzeilen();
    }

    /// <summary>Sucht eine Wartung im Datenbestand.</summary>
    private Wartung? FindeWartung(long id) => _bestand.Wartungen.FirstOrDefault(w => w.Id == id);
}
