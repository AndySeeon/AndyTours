using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Bereichs „Auswahllisten“ im Einstellungsfenster: Einträge einer Liste hinzufügen,
/// umbenennen, löschen und verschieben.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ein Klick auf einen Eintrag übernimmt ihn in das Eingabefeld. „Hinzufügen“ hängt den Feldinhalt
/// an, „Umbenennen“ ersetzt den markierten Eintrag durch den Feldinhalt. Ungültige Eingaben (leer, schon
/// vorhanden) werden mit einem Hinweis abgewiesen.</item>
/// <item>Wird ein Eintrag umbenannt, den Einheiten verwenden, fragt das ViewModel mit der Anzahl nach;
/// nach „Ja“ werden auch die Einheiten umbenannt. Einen verwendeten Eintrag zu löschen ist gesperrt,
/// ein Hinweis nennt die Anzahl; ein unbenutzter verschwindet ohne Rückfrage.</item>
/// <item>Jede Änderung wird sofort über die Funktion des Hauptfensters gespeichert. Erst nach Erfolg zeigt
/// die Liste den neuen Stand; schlägt das Speichern fehl, bleibt sie, wie sie war (die Fehlermeldung zeigt
/// das Hauptfenster).</item>
/// </list>
/// </remarks>
public partial class AuswahllistenViewModel : ObservableObject
{
    /// <summary>Bezeichnungen der Listen in der Auswahl, in Anzeigereihenfolge.</summary>
    private static readonly Dictionary<string, Auswahlliste> Bezeichnungen = new()
    {
        ["Trainingsbereich"] = Auswahlliste.Trainingsbereich,
    };

    /// <summary>Liest die Einträge einer Liste.</summary>
    private readonly Func<Auswahlliste, IReadOnlyList<string>> _lesen;

    /// <summary>Zählt die Einheiten, die einen Eintrag verwenden.</summary>
    private readonly Func<Auswahlliste, string, int> _zaehlen;

    /// <summary>Speichert eine geänderte Liste (bei Umbenennung samt Einheiten); <c>true</c> bei Erfolg.</summary>
    private readonly Func<Auswahlliste, IReadOnlyList<string>, (string Alt, string Neu)?, bool> _speichern;

    /// <summary>
    /// Erzeugt das ViewModel und zeigt die erste Liste.
    /// </summary>
    /// <param name="lesen">Liest die Einträge einer Liste.</param>
    /// <param name="zaehlen">Zählt die Einheiten, die einen Eintrag verwenden.</param>
    /// <param name="speichern">Speichert eine geänderte Liste; <c>true</c> bei Erfolg.</param>
    public AuswahllistenViewModel(
        Func<Auswahlliste, IReadOnlyList<string>> lesen,
        Func<Auswahlliste, string, int> zaehlen,
        Func<Auswahlliste, IReadOnlyList<string>, (string Alt, string Neu)?, bool> speichern)
    {
        _lesen = lesen;
        _zaehlen = zaehlen;
        _speichern = speichern;
        Listen = Bezeichnungen.Keys.ToList();
        GewaehlteListe = Listen[0];
    }

    /// <summary>Die Bezeichnungen der pflegbaren Listen (zunächst nur „Trainingsbereich“).</summary>
    public IReadOnlyList<string> Listen { get; }

    /// <summary>Die Einträge der gewählten Liste in ihrer Reihenfolge.</summary>
    public ObservableCollection<string> Eintraege { get; } = new();

    /// <summary>Bezeichnung der gewählten Liste.</summary>
    [ObservableProperty]
    private string _gewaehlteListe = string.Empty;

    /// <summary>Stelle des markierten Eintrags; -1 = keiner markiert.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UmbenennenCommand))]
    [NotifyCanExecuteChangedFor(nameof(LoeschenCommand))]
    [NotifyCanExecuteChangedFor(nameof(NachObenCommand))]
    [NotifyCanExecuteChangedFor(nameof(NachUntenCommand))]
    private int _gewaehlterIndex = -1;

    /// <summary>Inhalt des Eingabefelds „Eintrag“.</summary>
    [ObservableProperty]
    private string _eingabe = string.Empty;

    /// <summary>Hinweis unter dem Eingabefeld (z. B. warum ein Eintrag abgewiesen wurde); leer = kein Hinweis.</summary>
    [ObservableProperty]
    private string _hinweis = string.Empty;

    /// <summary>Die gewählte Liste.</summary>
    private Auswahlliste Liste => Bezeichnungen[GewaehlteListe];

    /// <summary>Der markierte Eintrag oder <c>null</c>.</summary>
    private string? Markiert => GewaehlterIndex >= 0 && GewaehlterIndex < Eintraege.Count ? Eintraege[GewaehlterIndex] : null;

    /// <summary>Befehl „Hinzufügen“: hängt den Inhalt des Eingabefelds an die Liste an.</summary>
    [RelayCommand]
    private void Hinzufuegen()
    {
        if (PruefeAuswahleintrag.Ausfuehren(Eintraege, Eingabe) is string grund)
        {
            Hinweis = grund;
            return;
        }

        List<string> neu = [.. Eintraege, Eingabe.Trim()];
        Speichere(neu, null, neu.Count - 1);
    }

    /// <summary>
    /// Befehl „Umbenennen“: ersetzt den markierten Eintrag durch den Inhalt des Eingabefelds; verwenden
    /// Einheiten den Eintrag, erst nach Rückfrage und dann auch in diesen Einheiten.
    /// </summary>
    [RelayCommand(CanExecute = nameof(EinerMarkiert))]
    private void Umbenennen()
    {
        if (Markiert is not string alt)
        {
            return;
        }

        if (PruefeAuswahleintrag.Ausfuehren(Eintraege, Eingabe, GewaehlterIndex) is string grund)
        {
            Hinweis = grund;
            return;
        }

        string neuerName = Eingabe.Trim();
        if (neuerName == alt)
        {
            return; // nichts geändert
        }

        int anzahl = _zaehlen(Liste, alt);
        if (anzahl > 0)
        {
            string frage = string.Format(CultureInfo.GetCultureInfo("de-DE"), "In {0:N0} {1} ‚{2}‘ durch ‚{3}‘ ersetzen?",
                anzahl, anzahl == 1 ? "Einheit" : "Einheiten", alt, neuerName);
            if (MessageBox.Show(frage, "Auswahllisten", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
            {
                return;
            }
        }

        List<string> neu = Eintraege.ToList();
        neu[GewaehlterIndex] = neuerName;
        Speichere(neu, anzahl > 0 ? (alt, neuerName) : null, GewaehlterIndex);
    }

    /// <summary>
    /// Befehl „Löschen“: entfernt den markierten Eintrag ohne Rückfrage – außer Einheiten verwenden ihn,
    /// dann ist das Löschen gesperrt und ein Hinweis nennt die Anzahl.
    /// </summary>
    [RelayCommand(CanExecute = nameof(EinerMarkiert))]
    private void Loeschen()
    {
        if (Markiert is not string eintrag)
        {
            return;
        }

        int anzahl = _zaehlen(Liste, eintrag);
        if (anzahl > 0)
        {
            Hinweis = string.Format(CultureInfo.GetCultureInfo("de-DE"), "‚{0}‘ wird in {1:N0} {2} verwendet und kann nicht gelöscht werden.",
                eintrag, anzahl, anzahl == 1 ? "Einheit" : "Einheiten");
            return;
        }

        int stelle = GewaehlterIndex;
        List<string> neu = Eintraege.ToList();
        neu.RemoveAt(stelle);
        Speichere(neu, null, Math.Min(stelle, neu.Count - 1));
    }

    /// <summary>Befehl „Nach oben“: verschiebt den markierten Eintrag um eine Stelle nach oben.</summary>
    [RelayCommand(CanExecute = nameof(KannNachOben))]
    private void NachOben() => Verschiebe(nachOben: true);

    /// <summary>Befehl „Nach unten“: verschiebt den markierten Eintrag um eine Stelle nach unten.</summary>
    [RelayCommand(CanExecute = nameof(KannNachUnten))]
    private void NachUnten() => Verschiebe(nachOben: false);

    /// <summary>Wählbarkeit von „Umbenennen“ und „Löschen“: ein Eintrag ist markiert.</summary>
    private bool EinerMarkiert() => Markiert is not null;

    /// <summary>Wählbarkeit von „Nach oben“: markiert und nicht der erste Eintrag.</summary>
    private bool KannNachOben() => Markiert is not null && GewaehlterIndex > 0;

    /// <summary>Wählbarkeit von „Nach unten“: markiert und nicht der letzte Eintrag.</summary>
    private bool KannNachUnten() => Markiert is not null && GewaehlterIndex < Eintraege.Count - 1;

    /// <summary>Verschiebt den markierten Eintrag und speichert die neue Reihenfolge.</summary>
    private void Verschiebe(bool nachOben)
    {
        int stelle = GewaehlterIndex;
        List<string> neu = VerschiebeEintrag.Ausfuehren(Eintraege, stelle, nachOben);
        Speichere(neu, null, nachOben ? stelle - 1 : stelle + 1);
    }

    /// <summary>
    /// Speichert die neue Liste; nur bei Erfolg zeigt die Liste den neuen Stand und markiert den angegebenen Eintrag.
    /// </summary>
    /// <param name="neu">Die neuen Einträge.</param>
    /// <param name="umbenennung">Alter und neuer Name, wenn Einheiten mit umbenannt werden.</param>
    /// <param name="markieren">Stelle, die danach markiert sein soll (-1 = keine).</param>
    private void Speichere(List<string> neu, (string Alt, string Neu)? umbenennung, int markieren)
    {
        if (!_speichern(Liste, neu, umbenennung))
        {
            return;
        }

        ZeigeListe(markieren);
        Hinweis = string.Empty;
    }

    /// <summary>Füllt die Einträge aus der gespeicherten Liste und markiert die angegebene Stelle.</summary>
    private void ZeigeListe(int markieren)
    {
        Eintraege.Clear();
        foreach (string eintrag in _lesen(Liste))
        {
            Eintraege.Add(eintrag);
        }

        GewaehlterIndex = markieren < Eintraege.Count ? markieren : -1;
        if (GewaehlterIndex < 0)
        {
            Eingabe = string.Empty;
        }
    }

    /// <summary>Eine andere Liste gewählt: deren Einträge zeigen.</summary>
    partial void OnGewaehlteListeChanged(string value)
    {
        Hinweis = string.Empty;
        ZeigeListe(-1);
    }

    /// <summary>Ein Eintrag markiert: ihn ins Eingabefeld übernehmen.</summary>
    partial void OnGewaehlterIndexChanged(int value)
    {
        if (Markiert is string eintrag)
        {
            Eingabe = eintrag;
            Hinweis = string.Empty;
        }
    }
}
