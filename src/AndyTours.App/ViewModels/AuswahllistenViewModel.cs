using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Bereichs „Auswahllisten“ im Einstellungsfenster: Trainingsbereiche, Aktivitäten,
/// Kategorien und Tätigkeiten hinzufügen, umbenennen, löschen, verschieben, kennzeichnen und zusammenführen; Equipments
/// anlegen, bearbeiten und löschen.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ein Klick auf einen Eintrag übernimmt ihn in das Eingabefeld. „Hinzufügen“ hängt den Feldinhalt
/// an, „Umbenennen“ ersetzt den markierten Eintrag durch den Feldinhalt. Ungültige Eingaben (leer, schon
/// vorhanden) werden mit einem Hinweis abgewiesen.</item>
/// <item>Trainingsbereiche stehen als Text in den Einheiten, Tätigkeiten als Text in den Wartungen: Wird ein
/// verwendeter Eintrag umbenannt, fragt das ViewModel mit der Anzahl nach. Aktivitäten und Kategorien sind über ihre Nummer zugeordnet; ihr
/// Umbenennen braucht keine Rückfrage.</item>
/// <item>Löschen ist gesperrt, solange der Eintrag verwendet wird (bei Aktivitäten auch für die
/// Hauptaktivität und die letzte Aktivität); ein Hinweis nennt den Grund. Ein unbenutzter Eintrag
/// verschwindet ohne Rückfrage.</item>
/// <item>„Nach oben“/„Nach unten“ gibt es für Trainingsbereiche, Aktivitäten und Tätigkeiten; Kategorien stehen
/// alphabetisch. „Als Hauptaktivität“ gilt nur für Aktivitäten, „Standard“ nur für Kategorien,
/// „Zusammenführen mit …“ für beide.</item>
/// <item>Equipments haben mehrere Felder: „Hinzufügen“ und „Bearbeiten…“ öffnen dafür das Fenster
/// „Equipment“ (<see cref="IAuswahllistenSpeicher.BearbeiteEquipment"/>); Eingabefeld und „Umbenennen“
/// sind bei dieser Liste ausgeblendet. Equipments stehen alphabetisch, die ausgemusterten am Ende.</item>
/// <item>Jede Änderung wird sofort über <see cref="IAuswahllistenSpeicher"/> gespeichert. Erst nach Erfolg
/// zeigt die Liste den neuen Stand; schlägt das Speichern fehl, bleibt sie, wie sie war (die Fehlermeldung
/// zeigt das Hauptfenster).</item>
/// </list>
/// </remarks>
public partial class AuswahllistenViewModel : ObservableObject
{
    /// <summary>Zahlenformat der Rückfragen (Tausenderpunkt).</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Bezeichnungen der Listen in der Auswahl, in Anzeigereihenfolge.</summary>
    private static readonly Dictionary<string, Auswahlliste> Bezeichnungen = new()
    {
        ["Trainingsbereich"] = Auswahlliste.Trainingsbereich,
        ["Aktivität"] = Auswahlliste.Aktivitaet,
        ["Kategorie"] = Auswahlliste.Kategorie,
        ["Equipment"] = Auswahlliste.Equipment,
        ["Tätigkeit"] = Auswahlliste.Taetigkeit,
    };

    /// <summary>Liest, prüft und speichert die Listen (umgesetzt vom Hauptfenster).</summary>
    private readonly IAuswahllistenSpeicher _speicher;

    /// <summary>Öffnet das Fenster „Zusammenführen mit …“: (Ursprung, übrige Einträge) → Ziel oder <c>null</c>.</summary>
    private readonly Func<string, IReadOnlyList<string>, string?> _waehleZiel;

    /// <summary>
    /// Erzeugt das ViewModel und zeigt die erste Liste.
    /// </summary>
    /// <param name="speicher">Liest, prüft und speichert die Listen.</param>
    /// <param name="waehleZiel">Öffnet das Fenster zur Wahl des Ziels beim Zusammenführen.</param>
    public AuswahllistenViewModel(IAuswahllistenSpeicher speicher, Func<string, IReadOnlyList<string>, string?> waehleZiel)
    {
        _speicher = speicher;
        _waehleZiel = waehleZiel;
        Listen = Bezeichnungen.Keys.ToList();
        GewaehlteListe = Listen[0];
    }

    /// <summary>Die Bezeichnungen der pflegbaren Listen.</summary>
    public IReadOnlyList<string> Listen { get; }

    /// <summary>Die Zeilen der gewählten Liste in der Reihenfolge der Anzeige.</summary>
    public ObservableCollection<Auswahllistenzeile> Eintraege { get; } = new();

    /// <summary>Bezeichnung der gewählten Liste.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstAktivitaetsliste))]
    [NotifyPropertyChangedFor(nameof(IstKategorienliste))]
    [NotifyPropertyChangedFor(nameof(KannZusammenfuehrenAnzeigen))]
    [NotifyPropertyChangedFor(nameof(IstEquipmentliste))]
    [NotifyPropertyChangedFor(nameof(HatEingabefeld))]
    private string _gewaehlteListe = string.Empty;

    /// <summary>Stelle des markierten Eintrags; -1 = keiner markiert.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UmbenennenCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoeschenCommand))]
    [NotifyCanExecuteChangedFor(nameof(NachObenCommand))]
    [NotifyCanExecuteChangedFor(nameof(NachUntenCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlsHauptaktivitaetCommand))]
    [NotifyCanExecuteChangedFor(nameof(StandardCommand))]
    [NotifyCanExecuteChangedFor(nameof(ZusammenfuehrenCommand))]
    [NotifyCanExecuteChangedFor(nameof(BearbeitenCommand))]
    private int _gewaehlterIndex = -1;

    /// <summary>Inhalt des Eingabefelds „Eintrag“.</summary>
    [ObservableProperty]
    private string _eingabe = string.Empty;

    /// <summary>Hinweis unter dem Eingabefeld (z. B. warum ein Eintrag abgewiesen wurde); leer = kein Hinweis.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary><c>true</c>, wenn die Liste „Aktivität“ gewählt ist (Schaltfläche „Als Hauptaktivität“ sichtbar).</summary>
    public bool IstAktivitaetsliste => Liste == Auswahlliste.Aktivitaet;

    /// <summary><c>true</c>, wenn die Liste „Kategorie“ gewählt ist (Schaltfläche „Standard“ sichtbar).</summary>
    public bool IstKategorienliste => Liste == Auswahlliste.Kategorie;

    /// <summary><c>true</c> bei Aktivitäten und Kategorien (Schaltfläche „Zusammenführen mit …“ sichtbar).</summary>
    public bool KannZusammenfuehrenAnzeigen => IstAktivitaetsliste || IstKategorienliste;

    /// <summary><c>true</c> bei Equipments (Schaltfläche „Bearbeiten…“ sichtbar).</summary>
    public bool IstEquipmentliste => Liste == Auswahlliste.Equipment;

    /// <summary><c>true</c>, wenn Eingabefeld und „Umbenennen“ sichtbar sind (nicht bei Equipments).</summary>
    public bool HatEingabefeld => !IstEquipmentliste;

    /// <summary><c>true</c> bei Listen, deren Reihenfolge der Benutzer festlegt (Trainingsbereiche, Aktivitäten, Tätigkeiten).</summary>
    private bool IstSortierbar => Liste is Auswahlliste.Trainingsbereich or Auswahlliste.Aktivitaet or Auswahlliste.Taetigkeit;

    /// <summary>Die gewählte Liste.</summary>
    private Auswahlliste Liste => Bezeichnungen.TryGetValue(GewaehlteListe, out Auswahlliste liste) ? liste : Auswahlliste.Trainingsbereich;

    /// <summary>Name des markierten Eintrags oder <c>null</c>.</summary>
    private string? Markiert => GewaehlterIndex >= 0 && GewaehlterIndex < Eintraege.Count ? Eintraege[GewaehlterIndex].Name : null;

    /// <summary>Die Namen aller Einträge der gewählten Liste.</summary>
    private List<string> Namen => Eintraege.Select(z => z.Name).ToList();

    /// <summary>
    /// Befehl „Hinzufügen“: legt den Inhalt des Eingabefelds als neuen Eintrag an; bei Equipments öffnet
    /// er das Fenster „Equipment“ für ein neues.
    /// </summary>
    [RelayCommand]
    private void Hinzufuegen()
    {
        if (IstEquipmentliste)
        {
            if (_speicher.BearbeiteEquipment(null) is string angelegt)
            {
                ZeigeListe(angelegt);
            }

            return;
        }

        if (PruefeAuswahleintrag.Ausfuehren(Namen, Eingabe) is string grund)
        {
            Hinweis = grund;
            return;
        }

        string neu = Eingabe.Trim();
        if (_speicher.Hinzufuegen(Liste, neu))
        {
            ZeigeListe(neu);
        }
    }

    /// <summary>
    /// Befehl „Umbenennen“: ersetzt den markierten Eintrag durch den Inhalt des Eingabefelds; ein
    /// verwendeter Trainingsbereich bzw. eine verwendete Tätigkeit erst nach Rückfrage, weil die Einheiten bzw.
    /// Wartungen mit umbenannt werden.
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannUmbenennen))]
    private void Umbenennen()
    {
        if (Markiert is not string alt)
        {
            return;
        }

        if (PruefeAuswahleintrag.Ausfuehren(Namen, Eingabe, GewaehlterIndex) is string grund)
        {
            Hinweis = grund;
            return;
        }

        string neuerName = Eingabe.Trim();
        if (neuerName == alt)
        {
            return; // nichts geändert
        }

        if (Liste is Auswahlliste.Trainingsbereich or Auswahlliste.Taetigkeit)
        {
            // Bei Tätigkeiten zählt der erste Wert die Wartungen
            int anzahl = _speicher.ZaehleVerwendung(Liste, alt).Einheiten;
            if (anzahl > 0)
            {
                string verwendung = Liste == Auswahlliste.Taetigkeit
                    ? (anzahl == 1 ? "Wartung" : "Wartungen")
                    : (anzahl == 1 ? "Einheit" : "Einheiten");
                string frage = string.Format(Deutsch, "In {0:N0} {1} ‚{2}‘ durch ‚{3}‘ ersetzen?",
                    anzahl, verwendung, alt, neuerName);
                if (!Frage(frage))
                {
                    return;
                }
            }
        }

        if (_speicher.Umbenennen(Liste, alt, neuerName))
        {
            ZeigeListe(neuerName);
        }
    }

    /// <summary>
    /// Befehl „Löschen“: entfernt den markierten Eintrag ohne Rückfrage – sofern nichts dagegen spricht;
    /// sonst nennt ein Hinweis den Grund.
    /// </summary>
    [RelayCommand(CanExecute = nameof(EinerMarkiert))]
    private void Loeschen()
    {
        if (Markiert is not string eintrag)
        {
            return;
        }

        if (_speicher.ErmittleLoeschsperre(Liste, eintrag) is string sperre)
        {
            Hinweis = sperre;
            return;
        }

        int stelle = GewaehlterIndex;
        if (_speicher.Loeschen(Liste, eintrag))
        {
            ZeigeListe(stelle);
        }
    }

    /// <summary>Befehl „Bearbeiten…“: öffnet das Fenster „Equipment“ für das markierte Equipment.</summary>
    [RelayCommand(CanExecute = nameof(EquipmentMarkiert))]
    private void Bearbeiten()
    {
        if (Markiert is string name && _speicher.BearbeiteEquipment(name) is string gespeichert)
        {
            ZeigeListe(gespeichert);
        }
    }

    /// <summary>Befehl „Nach oben“: verschiebt den markierten Eintrag eine Stelle nach oben.</summary>
    [RelayCommand(CanExecute = nameof(KannNachOben))]
    private void NachOben() => Verschiebe(nachOben: true);

    /// <summary>Befehl „Nach unten“: verschiebt den markierten Eintrag eine Stelle nach unten.</summary>
    [RelayCommand(CanExecute = nameof(KannNachUnten))]
    private void NachUnten() => Verschiebe(nachOben: false);

    /// <summary>Befehl „Als Hauptaktivität“: macht die markierte Aktivität zur Hauptaktivität.</summary>
    [RelayCommand(CanExecute = nameof(AktivitaetMarkiert))]
    private void AlsHauptaktivitaet()
    {
        if (Markiert is string name && _speicher.SetzeHauptaktivitaet(name))
        {
            ZeigeListe(name);
        }
    }

    /// <summary>Befehl „Standard“: schaltet das Standard-Kennzeichen der markierten Kategorie um.</summary>
    [RelayCommand(CanExecute = nameof(KategorieMarkiert))]
    private void Standard()
    {
        if (Markiert is string name && _speicher.SchalteStandardUm(name))
        {
            ZeigeListe(name);
        }
    }

    /// <summary>
    /// Befehl „Zusammenführen mit …“: Ziel im kleinen Fenster wählen, mit der Anzahl nachfragen und den
    /// markierten Eintrag im Ziel aufgehen lassen.
    /// </summary>
    [RelayCommand(CanExecute = nameof(KannZusammenfuehren))]
    private void Zusammenfuehren()
    {
        if (Markiert is not string ursprung)
        {
            return;
        }

        List<string> ziele = Namen.Where(n => n != ursprung).ToList();
        if (_waehleZiel(ursprung, ziele) is not string ziel)
        {
            return;
        }

        (int einheiten, int equipments) = _speicher.ZaehleVerwendung(Liste, ursprung);
        string anzahl = string.Format(Deutsch, "{0:N0} {1}", einheiten, einheiten == 1 ? "Einheit" : "Einheiten");
        if (equipments > 0)
        {
            anzahl += string.Format(Deutsch, " und {0:N0} {1}", equipments, equipments == 1 ? "Equipment" : "Equipments");
        }

        if (!Frage($"{anzahl} von ‚{ursprung}‘ nach ‚{ziel}‘ übernehmen und ‚{ursprung}‘ entfernen?"))
        {
            return;
        }

        if (_speicher.Zusammenfuehren(Liste, ursprung, ziel))
        {
            ZeigeListe(ziel);
        }
    }

    /// <summary>Wählbarkeit von „Löschen“: ein Eintrag ist markiert.</summary>
    private bool EinerMarkiert() => Markiert is not null;

    /// <summary>Wählbarkeit von „Umbenennen“: ein Eintrag ist markiert, und die Liste hat ein Eingabefeld.</summary>
    private bool KannUmbenennen() => Markiert is not null && HatEingabefeld;

    /// <summary>Wählbarkeit von „Bearbeiten…“: ein Equipment ist markiert.</summary>
    private bool EquipmentMarkiert() => Markiert is not null && IstEquipmentliste;

    /// <summary>Wählbarkeit von „Nach oben“: markiert, nicht ganz oben, und die Liste ist nicht alphabetisch.</summary>
    private bool KannNachOben() => Markiert is not null && IstSortierbar && GewaehlterIndex > 0;

    /// <summary>Wählbarkeit von „Nach unten“: markiert, nicht ganz unten, und die Liste ist nicht alphabetisch.</summary>
    private bool KannNachUnten() => Markiert is not null && IstSortierbar && GewaehlterIndex < Eintraege.Count - 1;

    /// <summary>Wählbarkeit von „Als Hauptaktivität“: eine Aktivität ist markiert.</summary>
    private bool AktivitaetMarkiert() => Markiert is not null && IstAktivitaetsliste;

    /// <summary>Wählbarkeit von „Standard“: eine Kategorie ist markiert.</summary>
    private bool KategorieMarkiert() => Markiert is not null && IstKategorienliste;

    /// <summary>Wählbarkeit von „Zusammenführen mit …“: markiert, und es gibt einen anderen Eintrag als Ziel.</summary>
    private bool KannZusammenfuehren() => Markiert is not null && KannZusammenfuehrenAnzeigen && Eintraege.Count > 1;

    /// <summary>Stellt eine Ja/Nein-Frage; vorbelegt ist „Nein“.</summary>
    private static bool Frage(string text) =>
        MessageBox.Show(text, "Auswahllisten", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;

    /// <summary>Verschiebt den markierten Eintrag und speichert die neue Reihenfolge.</summary>
    private void Verschiebe(bool nachOben)
    {
        int stelle = GewaehlterIndex;
        List<string> neu = VerschiebeEintrag.Ausfuehren(Namen, stelle, nachOben);
        if (_speicher.Verschieben(Liste, neu))
        {
            ZeigeListe(nachOben ? stelle - 1 : stelle + 1);
        }
    }

    /// <summary>Zeigt die Liste neu und markiert den Eintrag mit diesem Namen.</summary>
    private void ZeigeListe(string markieren)
    {
        LadeEintraege();
        Markiere(Eintraege.ToList().FindIndex(z => z.Name == markieren));
    }

    /// <summary>Zeigt die Liste neu und markiert die angegebene Stelle (begrenzt auf die Liste; -1 = keine).</summary>
    private void ZeigeListe(int markieren)
    {
        LadeEintraege();
        Markiere(Math.Min(markieren, Eintraege.Count - 1));
    }

    /// <summary>Liest die Zeilen der gewählten Liste neu ein.</summary>
    private void LadeEintraege()
    {
        Eintraege.Clear();
        foreach (Auswahllistenzeile zeile in _speicher.Lesen(Liste))
        {
            Eintraege.Add(zeile);
        }

        Hinweis = string.Empty;
    }

    /// <summary>Markiert eine Stelle; ohne Markierung wird das Eingabefeld geleert.</summary>
    private void Markiere(int stelle)
    {
        GewaehlterIndex = stelle;
        if (GewaehlterIndex < 0)
        {
            Eingabe = string.Empty;
        }

        // Die Wählbarkeit hängt auch von Liste und Anzahl der Einträge ab, nicht nur von der Markierung
        ZusammenfuehrenCommand.NotifyCanExecuteChanged();
        BearbeitenCommand.NotifyCanExecuteChanged();
        UmbenennenCommand.NotifyCanExecuteChanged();
        NachObenCommand.NotifyCanExecuteChanged();
        NachUntenCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Wird vom CommunityToolkit aufgerufen, sobald eine andere Liste gewählt wird.</summary>
    partial void OnGewaehlteListeChanged(string value) => ZeigeListe(-1);

    /// <summary>Übernimmt den markierten Eintrag in das Eingabefeld.</summary>
    partial void OnGewaehlterIndexChanged(int value)
    {
        if (Markiert is string eintrag)
        {
            Eingabe = eintrag;
            Hinweis = string.Empty;
        }
    }
}
