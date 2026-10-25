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
/// Teil des Hauptfenster-ViewModels: die Wartungsalarme der Karteikarte „Wartung“ – Alarmliste, Markierung,
/// die Befehle „Neuer Alarm…“, „Bearbeiten…“, „Löschen“ und „Ein-/Ausschalten“, der Titel der Karteikarte und
/// der Hinweis auf fällige Alarme beim Programmstart.
/// </summary>
/// <remarks>
/// Alarme sind Einträge vom Typ „Alarm“ im Bestand der Wartungen. Grundsatz wie bei den Wartungen: Erst wird
/// in die Datenbank geschrieben, erst danach der Bestand im Speicher geändert und die Liste neu aufgebaut;
/// schlägt das Schreiben fehl, erscheint eine Fehlermeldung (über <see cref="Schreibe"/>), und die Liste
/// bleibt beim vorherigen Stand.
/// <para>
/// Die Alarmliste folgt der Radauswahl über der Wartungstabelle und wird mit jeder Wartungstabelle neu
/// aufgebaut (<see cref="FuelleWartungszeilen"/>), also auch nach Änderungen an Einheiten, Rädern, Wartungen
/// und Tätigkeiten.
/// </para>
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Titel der Meldungsfenster der Alarme.</summary>
    private const string Alarmtitel = "Wartungsalarme";

    /// <summary>
    /// Wird ausgelöst, wenn die Alarmliste Alarme markieren und den ersten in den sichtbaren Bereich holen soll
    /// (nach dem Speichern aus dem Alarmfenster und nach dem Ein-/Ausschalten).
    /// </summary>
    public event Action<IReadOnlyCollection<long>>? MarkiereAlarme;

    /// <summary>Die Zeilen der Alarmliste (gefiltert nach dem gewählten Rad, fällige zuerst).</summary>
    public ObservableCollection<Alarmzeile> Alarmzeilen { get; } = new();

    /// <summary><c>true</c>, wenn die Alarmliste Zeilen hat; sonst steht dort <see cref="Alarmhinweis"/>.</summary>
    [ObservableProperty]
    private bool _hatAlarmzeilen;

    /// <summary>Hinweis statt der leeren Alarmliste, z. B. „Keine Alarme eingerichtet.“</summary>
    [ObservableProperty]
    private string _alarmhinweis = string.Empty;

    /// <summary>Titel der Karteikarte: „Wartung“, mit fälligen Alarmen z. B. „Wartung (2)“.</summary>
    [ObservableProperty]
    private string _wartungTitel = "Wartung";

    /// <summary>
    /// Nummern der in der Alarmliste markierten Alarme; die Liste setzt sie bei jeder Änderung der Markierung.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AlarmBearbeitenCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlarmeLoeschenCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlarmeEinAusschaltenCommand))]
    private IReadOnlyList<long> _markierteAlarme = [];

    /// <summary>
    /// Zeigt einen Hinweis mit allen fälligen Alarmen, sofern es welche gibt; vom Hauptfenster einmal nach dem
    /// ersten Anzeigen aufgerufen (nur beim Programmstart).
    /// </summary>
    public void MeldeFaelligeAlarme()
    {
        if (BeschreibeFaelligeAlarme.Ausfuehren(_bestand) is string text)
        {
            MessageBox.Show(Application.Current.MainWindow, text, Alarmtitel, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Befehl „Neuer Alarm…“: öffnet das Alarmfenster; ist oben ein Rad gewählt, ist es vorgewählt. Der Tag des
    /// Anlegens ist heute.
    /// </summary>
    [RelayCommand]
    private void NeuerAlarm()
    {
        var neu = new Wartung
        {
            Datum = DateOnly.FromDateTime(DateTime.Today),
            EquipmentId = GewaehltesWartungsrad?.Id,
            Typ = WartungsTyp.Alarm,
            Aktiv = true,
        };
        OeffneAlarmfenster(neu, "Neuer Alarm");
    }

    /// <summary>Befehl „Bearbeiten…“ (auch Doppelklick): öffnet das Alarmfenster des markierten Alarms.</summary>
    [RelayCommand(CanExecute = nameof(GenauEinAlarmMarkiert))]
    private void AlarmBearbeiten()
    {
        if (FindeWartung(MarkierteAlarme[0]) is Wartung alarm)
        {
            OeffneAlarmfenster(alarm, "Alarm bearbeiten");
        }
    }

    /// <summary>Befehl „Löschen“ (auch Entf-Taste): löscht die markierten Alarme nach einer Rückfrage mit der Anzahl.</summary>
    [RelayCommand(CanExecute = nameof(MindestensEinAlarmMarkiert))]
    private void AlarmeLoeschen()
    {
        List<Wartung> alarme = MarkierteAlarme.Select(FindeWartung).OfType<Wartung>().ToList();
        if (alarme.Count == 0)
        {
            return;
        }

        string frage = string.Format(CultureInfo.GetCultureInfo("de-DE"), "{0:N0} {1} wirklich löschen?",
            alarme.Count, alarme.Count == 1 ? "Alarm" : "Alarme");
        if (MessageBox.Show(frage, Alarmtitel, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
        {
            return;
        }

        var ids = alarme.Select(a => a.Id).ToHashSet();
        if (!Schreibe(verbindung => WartungenTabelle.Loeschen(verbindung, ids), "Die Alarme konnten nicht gelöscht werden"))
        {
            return;
        }

        _bestand.Wartungen.RemoveAll(w => ids.Contains(w.Id));
        FuelleWartungszeilen();
    }

    /// <summary>
    /// Befehl „Ein-/Ausschalten“ (Kontextmenü): Ist einer der markierten Alarme eingeschaltet, werden alle
    /// markierten ausgeschaltet; sind alle aus, werden alle eingeschaltet.
    /// </summary>
    [RelayCommand(CanExecute = nameof(MindestensEinAlarmMarkiert))]
    private void AlarmeEinAusschalten()
    {
        List<Wartung> alarme = MarkierteAlarme.Select(FindeWartung).OfType<Wartung>().ToList();
        if (alarme.Count == 0)
        {
            return;
        }

        bool einschalten = alarme.All(a => !a.Aktiv);
        List<long> ids = alarme.Select(a => a.Id).ToList();

        // Geänderte Fassungen schreiben; der Bestand bleibt bis zum Erfolg unverändert
        List<Wartung> geaendert = alarme.Select(a => new Wartung
        {
            Id = a.Id, Datum = a.Datum, EquipmentId = a.EquipmentId, Taetigkeit = a.Taetigkeit, Kommentar = a.Kommentar,
            Typ = a.Typ, AlarmKm = a.AlarmKm, Aktiv = einschalten,
        }).ToList();
        if (!Schreibe(verbindung =>
            {
                foreach (Wartung alarm in geaendert)
                {
                    WartungenTabelle.Aendern(verbindung, alarm);
                }
            }, "Die Alarme konnten nicht gespeichert werden"))
        {
            return;
        }

        foreach (Wartung alarm in alarme)
        {
            alarm.Aktiv = einschalten;
        }

        FuelleWartungszeilen();
        MarkiereAlarme?.Invoke(ids);
    }

    /// <summary>Wählbarkeit von „Bearbeiten…“: genau ein Alarm markiert.</summary>
    private bool GenauEinAlarmMarkiert() => MarkierteAlarme.Count == 1;

    /// <summary>Wählbarkeit von „Löschen“ und „Ein-/Ausschalten“: mindestens ein Alarm markiert.</summary>
    private bool MindestensEinAlarmMarkiert() => MarkierteAlarme.Count > 0;

    /// <summary>
    /// Öffnet das Alarmfenster für einen neuen oder vorhandenen Alarm; Räder und Liste „Tätigkeit“ werden jedes
    /// Mal neu ermittelt, damit das Fenster den aktuellen Stand anbietet.
    /// </summary>
    /// <param name="alarm">Der zu bearbeitende Alarm (bei „Neu“ mit der Nummer 0).</param>
    /// <param name="titel">Fenstertitel.</param>
    private void OeffneAlarmfenster(Wartung alarm, string titel)
    {
        // Bei „Neu“ nur Räder im Einsatz; beim Bearbeiten bleibt das bisherige (ggf. ausgemusterte) Rad wählbar
        long? bisheriges = alarm.Id == 0 ? null : alarm.EquipmentId;
        var viewModel = new AlarmBearbeitenViewModel(
            alarm,
            ErmittleWartungsraeder.Ausfuehren(_bestand, bisheriges),
            [.. _taetigkeiten],
            titel,
            SpeichereAlarm,
            _bestand);
        var fenster = new AlarmBearbeitenFenster(viewModel) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }

    /// <summary>
    /// Speichert einen Alarm aus dem Alarmfenster (neu oder geändert) und zeigt ihn in der Liste.
    /// </summary>
    /// <remarks>
    /// Ist oben ein anderes Rad gewählt als das des Alarms, wechselt die Auswahl auf „Alle Räder“, damit der
    /// Alarm sofort sichtbar ist. Danach wird er markiert.
    /// </remarks>
    /// <param name="alarm">Der fertige Alarm; bei einem neuen ist die Nummer 0.</param>
    /// <returns><c>true</c> bei Erfolg.</returns>
    private bool SpeichereAlarm(Wartung alarm)
    {
        bool istNeu = alarm.Id == 0;
        if (!Schreibe(verbindung =>
            {
                if (istNeu)
                {
                    alarm.Id = WartungenTabelle.Einfuegen(verbindung, alarm);
                }
                else
                {
                    WartungenTabelle.Aendern(verbindung, alarm);
                }
            }, "Der Alarm konnte nicht gespeichert werden"))
        {
            if (istNeu)
            {
                alarm.Id = 0; // nichts gespeichert: beim nächsten Versuch wieder als neuer Alarm
            }

            return false;
        }

        if (istNeu)
        {
            _bestand.Wartungen.Add(alarm);
        }
        else
        {
            int stelle = _bestand.Wartungen.FindIndex(w => w.Id == alarm.Id);
            if (stelle >= 0)
            {
                _bestand.Wartungen[stelle] = alarm;
            }
        }

        if (GewaehltesWartungsrad?.Id is long radId && radId != alarm.EquipmentId)
        {
            GewaehltesWartungsrad = AlleRaeder; // füllt beide Listen neu
        }
        else
        {
            FuelleWartungszeilen();
        }

        MarkiereAlarme?.Invoke([alarm.Id]);
        return true;
    }

    /// <summary>
    /// Füllt die Alarmliste mit den Alarmen des gewählten Rads (dieselbe Sammlung, damit die gewählte Sortierung
    /// erhalten bleibt), setzt den Hinweis für eine leere Liste und den Titel der Karteikarte.
    /// </summary>
    /// <remarks>Der Titel zählt die fälligen Alarme aller Räder, unabhängig von der Radauswahl.</remarks>
    private void FuelleAlarmzeilen()
    {
        List<Alarmzeile> zeilen = ErstelleAlarmzeilen.Ausfuehren(_bestand, GewaehltesWartungsrad?.Id);
        Alarmzeilen.Clear();
        foreach (Alarmzeile zeile in zeilen)
        {
            Alarmzeilen.Add(zeile);
        }

        HatAlarmzeilen = zeilen.Count > 0;
        Alarmhinweis = _bestand.Wartungen.Any(w => w.Typ == WartungsTyp.Alarm)
            ? "Keine Alarme für dieses Rad."
            : "Keine Alarme eingerichtet.";

        int faellig = GewaehltesWartungsrad?.Id is null
            ? zeilen.Count(z => z.Zustand == Alarmzustand.Faellig)
            : ErstelleAlarmzeilen.Ausfuehren(_bestand, null).Count(z => z.Zustand == Alarmzustand.Faellig);
        WartungTitel = faellig > 0 ? $"Wartung ({faellig})" : "Wartung";
    }
}
