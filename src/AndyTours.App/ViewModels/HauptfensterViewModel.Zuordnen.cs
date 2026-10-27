using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: Kategorien, Aktivität oder Equipment allen markierten Einheiten
/// auf einmal zuordnen.
/// </summary>
/// <remarks>
/// Ablauf je Befehl: Fenster zur Auswahl öffnen, bei mehr als einer Einheit mit der Anzahl nachfragen,
/// alle geänderten Einheiten in einer Transaktion schreiben, erst danach den Datenbestand ersetzen und
/// die Anzeige neu aufbauen. Die geänderten Einheiten bleiben markiert, soweit sie noch angezeigt werden.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Befehl „Einheit → Kategorien zuordnen…“: setzt oder entfernt Kategorien bei allen markierten Einheiten.</summary>
    [RelayCommand(CanExecute = nameof(MindestensEineMarkiert))]
    private void KategorienZuordnen()
    {
        List<Trainingseinheit> einheiten = LeseMarkierteEinheiten();
        if (einheiten.Count == 0)
        {
            return;
        }

        var viewModel = new KategorienZuordnenViewModel(ErmittleKategoriezustaende.Ausfuehren(_bestand.Kategorien, einheiten), einheiten.Count);
        var fenster = new KategorienZuordnenFenster(viewModel) { Owner = Application.Current.MainWindow };

        // Abbrechen oder kein Häkchen geändert: nichts zu tun, auch keine Rückfrage
        if (fenster.ShowDialog() != true || !viewModel.AendertEtwas)
        {
            return;
        }

        Uebernimm(einheiten, OrdneEinheitenZu.Kategorien(einheiten, viewModel.Setzen, viewModel.Entfernen));
    }

    /// <summary>Befehl „Einheit → Aktivität ändern…“: ordnet allen markierten Einheiten eine Aktivität zu.</summary>
    /// <remarks>Das Equipment der Einheiten bleibt dabei unverändert, auch wenn es nicht zur neuen Aktivität passt.</remarks>
    [RelayCommand(CanExecute = nameof(MindestensEineMarkiert))]
    private void AktivitaetAendern()
    {
        List<Trainingseinheit> einheiten = LeseMarkierteEinheiten();
        if (einheiten.Count == 0)
        {
            return;
        }

        List<Auswahleintrag> eintraege = _bestand.Aktivitaeten.Select(a => new Auswahleintrag(a.Id, a.Name)).ToList();
        if (WaehleEintrag("Aktivität ändern", "Neue Aktivität", einheiten, eintraege, e => e.AktivitaetId) is { Id: long aktivitaetId })
        {
            Uebernimm(einheiten, OrdneEinheitenZu.Aktivitaet(einheiten, aktivitaetId));
        }
    }

    /// <summary>Befehl „Einheit → Equipment ändern…“: ordnet allen markierten Einheiten ein Equipment (oder keines) zu.</summary>
    /// <remarks>Die Kategorien der Einheiten bleiben dabei unverändert.</remarks>
    [RelayCommand(CanExecute = nameof(MindestensEineMarkiert))]
    private void EquipmentAendern()
    {
        List<Trainingseinheit> einheiten = LeseMarkierteEinheiten();
        if (einheiten.Count == 0)
        {
            return;
        }

        // Angeboten werden „(kein)“ und die Equipments, die zu einer der Aktivitäten passen
        List<long> aktivitaetIds = einheiten.Select(e => e.AktivitaetId).Distinct().ToList();
        List<Auswahleintrag> eintraege = ErmittleEquipmentauswahl.Ausfuehren(_bestand, aktivitaetIds);
        if (WaehleEintrag("Equipment ändern", "Neues Equipment", einheiten, eintraege, e => e.EquipmentId) is Auswahleintrag gewaehlt)
        {
            Uebernimm(einheiten, OrdneEinheitenZu.Equipment(einheiten, gewaehlt.Id));
        }
    }

    /// <summary>Liefert die markierten Einheiten aus dem Datenbestand.</summary>
    private List<Trainingseinheit> LeseMarkierteEinheiten() =>
        MarkierteEinheiten.Select(FindeEinheit).OfType<Trainingseinheit>().ToList();

    /// <summary>
    /// Öffnet das Fenster „Eintrag wählen“. Haben alle Einheiten denselben Wert, ist dieser vorgewählt.
    /// </summary>
    /// <param name="titel">Titel des Fensters.</param>
    /// <param name="was">Anfang der Erklärung, z. B. „Neue Aktivität“.</param>
    /// <param name="einheiten">Die markierten Einheiten.</param>
    /// <param name="eintraege">Die wählbaren Einträge.</param>
    /// <param name="wert">Liest den bisherigen Wert (Nummer) einer Einheit.</param>
    /// <returns>Der gewählte Eintrag oder <c>null</c> bei „Abbrechen“.</returns>
    private static Auswahleintrag? WaehleEintrag(string titel, string was, List<Trainingseinheit> einheiten,
        List<Auswahleintrag> eintraege, Func<Trainingseinheit, long?> wert)
    {
        List<long?> werte = einheiten.Select(wert).Distinct().ToList();
        Auswahleintrag? vorgewaehlt = werte.Count == 1 ? eintraege.FirstOrDefault(e => e.Id == werte[0]) : null;
        string erklaerung = einheiten.Count == 1
            ? $"{was} für die markierte Einheit:"
            : string.Format(CultureInfo.GetCultureInfo("de-DE"), "{0} für {1:N0} Einheiten:", was, einheiten.Count);

        var viewModel = new EintragWaehlenViewModel(titel, erklaerung, eintraege, vorgewaehlt);
        var fenster = new EintragWaehlenFenster(viewModel) { Owner = Application.Current.MainWindow };
        return fenster.ShowDialog() == true ? viewModel.Ergebnis : null;
    }

    /// <summary>
    /// Fragt bei mehr als einer Einheit nach, schreibt die geänderten Einheiten und zeigt den neuen Stand an.
    /// </summary>
    /// <param name="einheiten">Die markierten Einheiten (für die Anzahl der Rückfrage).</param>
    /// <param name="geaendert">Die geänderten Kopien; der Datenbestand wird erst nach dem Schreiben ersetzt.</param>
    private void Uebernimm(List<Trainingseinheit> einheiten, List<Trainingseinheit> geaendert)
    {
        CultureInfo deutsch = CultureInfo.GetCultureInfo("de-DE");
        if (einheiten.Count > 1
            && MessageBox.Show(string.Format(deutsch, "{0:N0} Einheiten ändern?", einheiten.Count), Meldungstitel,
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
        {
            return;
        }

        if (!Schreibe(verbindung => TrainingseinheitenTabelle.AendereMehrere(verbindung, geaendert), "Die Einheiten konnten nicht geändert werden"))
        {
            return;
        }

        foreach (Trainingseinheit einheit in geaendert)
        {
            ErsetzeEinheit(einheit);
        }

        // Anzeige neu aufbauen; danach die noch angezeigten Einheiten wieder markieren
        AktualisiereJahre();
        AktualisiereSummen();
        var ids = geaendert.Select(e => e.Id).ToHashSet();
        List<long> sichtbar = Einheitszeilen.Where(z => ids.Contains(z.Id)).Select(z => z.Id).ToList();
        MarkiereEinheiten?.Invoke(sichtbar);

        // Herausgefallene Einheiten in der Statuszeile melden
        int ausserhalb = ids.Count - sichtbar.Count;
        if (ausserhalb > 0)
        {
            Statusmeldung = ausserhalb == 1
                ? "1 geänderte Einheit liegt außerhalb der aktuellen Auswahl."
                : string.Format(deutsch, "{0:N0} geänderte Einheiten liegen außerhalb der aktuellen Auswahl.", ausserhalb);
        }
    }
}
