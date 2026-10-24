using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: Trainingseinheiten anlegen, bearbeiten, kopieren und löschen –
/// über das Bearbeitungsfenster oder direkt in einer Tabellenzelle.
/// </summary>
/// <remarks>
/// Grundsatz: Erst wird in die Datenbank geschrieben, erst danach der Datenbestand im Speicher geändert.
/// Schlägt das Schreiben fehl, bleibt die Anzeige so beim vorherigen Stand.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Titel der Meldungsfenster dieses Bereichs.</summary>
    private const string Meldungstitel = "Trainingseinheiten";

    /// <summary>
    /// Solange <c>true</c>, löst ein Wechsel des gewählten Jahres keinen Neuaufbau aus
    /// (während die Jahresliste neu gefüllt wird).
    /// </summary>
    private bool _jahreWerdenGefuellt;

    /// <summary>
    /// Wird ausgelöst, wenn die Tabelle eine Einheit markieren und in den sichtbaren Bereich holen soll
    /// (nach dem Speichern aus dem Bearbeitungsfenster).
    /// </summary>
    public event Action<long>? MarkiereEinheit;

    /// <summary>
    /// Wird ausgelöst, wenn die Tabelle mehrere Einheiten markieren soll (nach dem Zuordnen).
    /// </summary>
    public event Action<IReadOnlyCollection<long>>? MarkiereEinheiten;

    /// <summary>
    /// Nummern der in der Tabelle markierten Einheiten; die Tabelle setzt sie bei jeder Änderung der Markierung.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EinheitBearbeitenCommand))]
    [NotifyCanExecuteChangedFor(nameof(EinheitKopierenCommand))]
    [NotifyCanExecuteChangedFor(nameof(EinheitenLoeschenCommand))]
    [NotifyCanExecuteChangedFor(nameof(KategorienZuordnenCommand))]
    [NotifyCanExecuteChangedFor(nameof(AktivitaetAendernCommand))]
    [NotifyCanExecuteChangedFor(nameof(EquipmentAendernCommand))]
    private IReadOnlyList<long> _markierteEinheiten = [];

    /// <summary>
    /// Meldung in der Statusleiste, z. B. „Gespeichert – liegt außerhalb der aktuellen Auswahl.“;
    /// die nächste Änderung der Anzeige leert sie wieder.
    /// </summary>
    [ObservableProperty]
    private string _statusmeldung = string.Empty;

    /// <summary>Befehl „Einheit → Neue Einheit“: öffnet das Bearbeitungsfenster mit einer vorbelegten neuen Einheit.</summary>
    [RelayCommand]
    private void NeueEinheit()
    {
        long? aktivitaetId = _bestand.Aktivitaeten.FirstOrDefault(a => a.Name == AusgewaehlteAktivitaet)?.Id;
        Trainingseinheit neu = ErstelleNeueEinheit.Ausfuehren(_bestand, aktivitaetId, DateOnly.FromDateTime(DateTime.Today));
        OeffneBearbeitungsfenster(neu, istNeu: true, "Neue Einheit");
    }

    /// <summary>Befehl „Einheit → Bearbeiten“ (auch Doppelklick): öffnet das Bearbeitungsfenster der markierten Einheit.</summary>
    [RelayCommand(CanExecute = nameof(GenauEineMarkiert))]
    private void EinheitBearbeiten()
    {
        if (FindeEinheit(MarkierteEinheiten[0]) is Trainingseinheit einheit)
        {
            OeffneBearbeitungsfenster(einheit, istNeu: false, "Einheit bearbeiten");
        }
    }

    /// <summary>
    /// Doppelklick auf einen Einzelrekord der Karteikarte „Aktuell“: öffnet das Bearbeitungsfenster dieser Einheit.
    /// </summary>
    /// <param name="id">Nummer der Einheit; <c>null</c> (Zeile eines Jahresrekords) bewirkt nichts.</param>
    [RelayCommand]
    private void EinheitOeffnen(long? id)
    {
        if (id is long nummer && FindeEinheit(nummer) is Trainingseinheit einheit)
        {
            OeffneBearbeitungsfenster(einheit, istNeu: false, "Einheit bearbeiten");
        }
    }

    /// <summary>Befehl „Einheit → Kopieren“: öffnet das Bearbeitungsfenster mit einer Kopie der markierten Einheit.</summary>
    [RelayCommand(CanExecute = nameof(GenauEineMarkiert))]
    private void EinheitKopieren()
    {
        if (FindeEinheit(MarkierteEinheiten[0]) is Trainingseinheit einheit)
        {
            Trainingseinheit kopie = KopiereEinheit.Ausfuehren(einheit, DateOnly.FromDateTime(DateTime.Today));
            OeffneBearbeitungsfenster(kopie, istNeu: true, "Einheit kopieren");
        }
    }

    /// <summary>
    /// Befehl „Einheit → Löschen“ (auch Entf-Taste): löscht die markierten Einheiten nach einer Rückfrage.
    /// </summary>
    [RelayCommand(CanExecute = nameof(MindestensEineMarkiert))]
    private void EinheitenLoeschen()
    {
        List<Trainingseinheit> einheiten = MarkierteEinheiten.Select(FindeEinheit).OfType<Trainingseinheit>().ToList();
        if (einheiten.Count == 0)
        {
            return;
        }

        // Rückfrage: bei einer Einheit mit Name und Datum, bei mehreren mit der Anzahl
        string frage = einheiten.Count == 1
            ? string.Format(CultureInfo.GetCultureInfo("de-DE"), "Einheit ‚{0}‘ vom {1:dd.MM.yyyy} wirklich löschen?", einheiten[0].Name, einheiten[0].Datum)
            : $"{einheiten.Count} Einheiten wirklich löschen?";
        if (MessageBox.Show(frage, Meldungstitel, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
        {
            return;
        }

        var ids = einheiten.Select(e => e.Id).ToHashSet();
        if (!Schreibe(verbindung => TrainingseinheitenTabelle.Loeschen(verbindung, ids), "Die Einheiten konnten nicht gelöscht werden"))
        {
            return;
        }

        _bestand.Trainingseinheiten.RemoveAll(e => ids.Contains(e.Id));
        ZeigeAenderung(null);
    }

    /// <summary>Wählbarkeit von „Bearbeiten“ und „Kopieren“: genau eine Zeile markiert.</summary>
    private bool GenauEineMarkiert() => MarkierteEinheiten.Count == 1;

    /// <summary>Wählbarkeit von „Löschen“ und der Zuordnungsbefehle: mindestens eine Zeile markiert.</summary>
    private bool MindestensEineMarkiert() => MarkierteEinheiten.Count > 0;

    /// <summary>
    /// Liefert den Text, mit dem die Bearbeitung einer Zelle beginnt (ohne Tausenderpunkt, z. B. „1234,5“).
    /// </summary>
    /// <param name="id">Nummer der Einheit.</param>
    /// <param name="spalte">Die Spalte.</param>
    /// <returns>Der Eingabetext.</returns>
    public string RohtextFuer(long id, Einheitenspalte spalte) =>
        FindeEinheit(id) is Trainingseinheit einheit ? UebernimmZellwert.Rohtext(einheit, spalte, _bestand) : string.Empty;

    /// <summary>
    /// Liefert die Auswahllisten (Aktivität, Equipment, Trainingsbereich) für die Zelle einer Einheit.
    /// </summary>
    /// <param name="id">Nummer der Einheit.</param>
    /// <returns>Die Listen.</returns>
    public Eingabelisten EingabelistenFuer(long id) =>
        ErmittleEingabelisten.Ausfuehren(_bestand, FindeEinheit(id) ?? new Trainingseinheit(), _trainingsbereiche);

    /// <summary>
    /// Übernimmt den Wert einer bearbeiteten Tabellenzelle: prüfen und sofort dauerhaft speichern.
    /// </summary>
    /// <remarks>
    /// Die Tabelle wird hier noch nicht neu aufgebaut – das ist während der laufenden Zellbearbeitung nicht
    /// erlaubt. Nach dem Ende der Bearbeitung ruft die Tabelle <see cref="ZeigeAenderung"/> auf.
    /// </remarks>
    /// <param name="id">Nummer der Einheit.</param>
    /// <param name="spalte">Die bearbeitete Spalte.</param>
    /// <param name="text">Der eingetragene Text bzw. gewählte Listeneintrag.</param>
    /// <returns>Ob der Wert ungültig ist, gespeichert wurde oder nichts gespeichert wurde.</returns>
    public Zellaenderung AendereZelle(long id, Einheitenspalte spalte, string text)
    {
        if (FindeEinheit(id) is not Trainingseinheit einheit || text == UebernimmZellwert.Rohtext(einheit, spalte, _bestand))
        {
            return Zellaenderung.NichtGespeichert; // nichts geändert
        }

        Eingabeergebnis ergebnis = UebernimmZellwert.Ausfuehren(einheit, spalte, text, _bestand);
        if (ergebnis.Einheit is not Trainingseinheit geaendert)
        {
            return Zellaenderung.Ungueltig;
        }

        if (!Schreibe(verbindung => TrainingseinheitenTabelle.Aendern(verbindung, geaendert), "Die Änderung konnte nicht gespeichert werden"))
        {
            return Zellaenderung.NichtGespeichert;
        }

        ErsetzeEinheit(geaendert);
        return Zellaenderung.Gespeichert;
    }

    /// <summary>
    /// Zeigt den geänderten Datenbestand an: Jahresliste, Statusleiste, Tabelle, Summen und Bestwerte.
    /// </summary>
    /// <param name="geaenderteId">
    /// Nummer der gespeicherten Einheit: Sie wird markiert bzw. – liegt sie außerhalb von Auswahl und
    /// Filter – in der Statusleiste gemeldet. <c>null</c> nach dem Löschen.
    /// </param>
    public void ZeigeAenderung(long? geaenderteId)
    {
        AktualisiereJahre();
        AktualisiereStatustext();
        AktualisiereSummen();

        if (geaenderteId is long id)
        {
            if (Einheitszeilen.Any(z => z.Id == id))
            {
                MarkiereEinheit?.Invoke(id);
            }
            else
            {
                Statusmeldung = "Gespeichert – liegt außerhalb der aktuellen Auswahl.";
            }
        }
    }

    /// <summary>
    /// Öffnet das Bearbeitungsfenster; gespeichert wird über <see cref="SpeichereAusFenster"/>.
    /// </summary>
    private void OeffneBearbeitungsfenster(Trainingseinheit einheit, bool istNeu, string titel)
    {
        Statusmeldung = string.Empty;
        var viewModel = new EinheitBearbeitenViewModel(
            einheit,
            ErmittleEingabelisten.Ausfuehren(_bestand, einheit, _trainingsbereiche),
            _bestand.Kategorien,
            titel,
            gespeichert => SpeichereAusFenster(gespeichert, istNeu),
            _bestand,
            istNeu);
        var fenster = new EinheitBearbeitenFenster(viewModel) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }

    /// <summary>
    /// Speichert die Einheit aus dem Bearbeitungsfenster und zeigt den neuen Stand an.
    /// </summary>
    /// <returns><c>true</c>, wenn gespeichert wurde und das Fenster schließen darf.</returns>
    private bool SpeichereAusFenster(Trainingseinheit einheit, bool istNeu)
    {
        if (istNeu)
        {
            (long Id, long FahrerId) neu = default;
            if (!Schreibe(verbindung => neu = TrainingseinheitenTabelle.Einfuegen(verbindung, einheit), "Die Einheit konnte nicht gespeichert werden"))
            {
                return false;
            }

            einheit.Id = neu.Id;
            einheit.FahrerId = neu.FahrerId;
            if (_bestand.Fahrer.All(f => f.Id != neu.FahrerId))
            {
                // Der Standardsportler wurde eben erst angelegt (leerer Datenordner)
                _bestand.Fahrer.Add(new Fahrer { Id = neu.FahrerId });
            }

            _bestand.Trainingseinheiten.Add(einheit);
        }
        else
        {
            if (!Schreibe(verbindung => TrainingseinheitenTabelle.Aendern(verbindung, einheit), "Die Einheit konnte nicht gespeichert werden"))
            {
                return false;
            }

            ErsetzeEinheit(einheit);
        }

        ZeigeAenderung(einheit.Id);
        return true;
    }

    /// <summary>
    /// Führt einen Schreibvorgang auf der Datenbank aus; bei einem Fehler erscheint eine Meldung.
    /// </summary>
    /// <returns><c>true</c> bei Erfolg.</returns>
    private bool Schreibe(Action<SqliteConnection> schreiben, string fehlertext)
    {
        try
        {
            using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(Datenordner, DatenordnerInhalt.Datenbankdatei));
            schreiben(verbindung);
            return true;
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            MessageBox.Show($"{fehlertext}:\n\n{fehler.Message}", Meldungstitel, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>Sucht eine Einheit im Datenbestand.</summary>
    private Trainingseinheit? FindeEinheit(long id) => _bestand.Trainingseinheiten.FirstOrDefault(e => e.Id == id);

    /// <summary>Ersetzt eine Einheit im Datenbestand durch ihre geänderte Fassung (gleiche Nummer).</summary>
    private void ErsetzeEinheit(Trainingseinheit geaendert)
    {
        int stelle = _bestand.Trainingseinheiten.FindIndex(e => e.Id == geaendert.Id);
        if (stelle >= 0)
        {
            _bestand.Trainingseinheiten[stelle] = geaendert;
        }
    }

    /// <summary>
    /// Füllt die Jahresliste neu (ein neues Jahr kommt hinzu, ein leeres verschwindet); die Wahl bleibt erhalten.
    /// </summary>
    /// <remarks>
    /// Verschwindet das gewählte Jahr (letzte Einheit eines früheren Jahres gelöscht), wird das aktuelle Jahr
    /// gewählt, das immer in der Liste steht.
    /// </remarks>
    private void AktualisiereJahre()
    {
        int aktuellesJahr = DateTime.Today.Year;
        List<string> neu = [AlleJahre, .. ErmittleAuswahljahre.Ausfuehren(_bestand.Trainingseinheiten, aktuellesJahr)
            .Select(j => j.ToString(CultureInfo.InvariantCulture))];
        if (neu.SequenceEqual(Jahre))
        {
            return;
        }

        string gewaehlt = AusgewaehltesJahr;
        _jahreWerdenGefuellt = true;
        try
        {
            Jahre.Clear();
            foreach (string jahr in neu)
            {
                Jahre.Add(jahr);
            }
        }
        finally
        {
            _jahreWerdenGefuellt = false;
        }

        // Die Auswahlliste der Oberfläche hat die Wahl beim Leeren verloren – wieder setzen
        AusgewaehltesJahr = neu.Contains(gewaehlt) ? gewaehlt : aktuellesJahr.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>Setzt den Text links in der Statusleiste (Gesamtzahl der Einheiten).</summary>
    private void AktualisiereStatustext() =>
        Statustext = string.Format(CultureInfo.GetCultureInfo("de-DE"), "{0:N0} Trainingseinheiten in der Datenbank", _bestand.Trainingseinheiten.Count);
}
