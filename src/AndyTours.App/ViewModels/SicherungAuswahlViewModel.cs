using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Daten.Datensicherung;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Sicherung wiederherstellen“: Liste der Sicherungen und die Auswahl.
/// </summary>
/// <remarks>
/// Das ViewModel stellt nur die Auswahl bereit. Rückfrage und Zurückspielen übernimmt der Aufrufer
/// (<c>FuehreWiederherstellungDurch</c>), nachdem das Fenster mit einer Auswahl geschlossen wurde.
/// </remarks>
public partial class SicherungAuswahlViewModel : ObservableObject
{
    /// <summary>Deutsche Kultur für Wochentag und Datum.</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Erzeugt das ViewModel; die neueste Sicherung ist vorausgewählt.
    /// </summary>
    /// <param name="sicherungen">Die Sicherungen, neueste zuerst (siehe <see cref="ErmittleSicherungen"/>).</param>
    /// <param name="sicherungsordner">Der Sicherungsordner (wird zur Orientierung angezeigt).</param>
    public SicherungAuswahlViewModel(IReadOnlyList<Sicherungseintrag> sicherungen, string sicherungsordner)
    {
        Zeilen = sicherungen.Select(s => new Sicherungszeile(s, FormatiereZeitpunkt(s.Zeitpunkt))).ToList();
        Sicherungsordner = sicherungsordner;
        Ausgewaehlt = Zeilen.Count > 0 ? Zeilen[0] : null;
    }

    /// <summary>Wird ausgelöst, sobald eine Sicherung gewählt wurde; das Fenster schließt sich daraufhin.</summary>
    public event EventHandler? Gewaehlt;

    /// <summary>Die anzuzeigenden Sicherungen, neueste zuerst.</summary>
    public IReadOnlyList<Sicherungszeile> Zeilen { get; }

    /// <summary>Der Sicherungsordner, aus dem die Liste stammt.</summary>
    public string Sicherungsordner { get; }

    /// <summary>Die in der Liste markierte Sicherung.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(WiederherstellenCommand))]
    private Sicherungszeile? _ausgewaehlt;

    /// <summary>Die gewählte Sicherung nach „Wiederherstellen…“; <c>null</c> bei Abbruch.</summary>
    public Sicherungseintrag? Ergebnis { get; private set; }

    /// <summary>
    /// Formatiert den Zeitpunkt einer Sicherung für Liste und Rückfrage,
    /// z. B. „Freitag, 23.10.2026, 18:58:22 Uhr“.
    /// </summary>
    /// <param name="zeitpunkt">Zeitpunkt der Sicherung.</param>
    /// <returns>Der Text auf Deutsch.</returns>
    public static string FormatiereZeitpunkt(DateTime zeitpunkt) =>
        zeitpunkt.ToString("dddd, dd.MM.yyyy, HH:mm:ss 'Uhr'", Deutsch);

    /// <summary>
    /// Befehl „Wiederherstellen…“ (auch Doppelklick): übernimmt die markierte Sicherung und schließt das Fenster.
    /// </summary>
    [RelayCommand(CanExecute = nameof(IstAusgewaehlt))]
    private void Wiederherstellen()
    {
        Ergebnis = Ausgewaehlt?.Eintrag;
        Gewaehlt?.Invoke(this, EventArgs.Empty);
    }

    /// <summary><c>true</c>, wenn eine Sicherung markiert ist.</summary>
    private bool IstAusgewaehlt() => Ausgewaehlt is not null;
}
