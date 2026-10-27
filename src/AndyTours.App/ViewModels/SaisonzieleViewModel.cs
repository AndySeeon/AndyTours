using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Saisonziele“: Ziele für Kilometer, Höhenmeter und Anzahl je Aktivität für das
/// laufende und das folgende Jahr.
/// </summary>
/// <remarks>
/// Das Fenster arbeitet auf einer Arbeitskopie der eigenen Ziele. Beim Wechsel des Jahres werden die
/// geänderten Zeilen in die Arbeitskopie übernommen; die Zeilen des anderen Jahres werden daraus neu
/// berechnet, sodass z. B. 2027 eine Änderung an 2026 sofort als „übernommen aus 2026“ zeigt. „OK“
/// speichert beide Jahre gemeinsam; „Abbrechen“ verwirft die Arbeitskopie. Jede Eingabe wird sofort
/// geprüft; solange ein Hinweis steht, sind „OK“ und der Jahreswechsel gesperrt.
/// </remarks>
public partial class SaisonzieleViewModel : ObservableObject
{
    /// <summary>Die Aktivitäten in Anzeigereihenfolge.</summary>
    private readonly IReadOnlyList<Aktivitaet> _aktivitaeten;

    /// <summary>Arbeitskopie aller eigenen Ziele (auch früherer Jahre, für die Übernahme).</summary>
    private readonly List<Saisonziel> _arbeitskopie;

    /// <summary>Speichert die Ziele der gepflegten Jahre; <c>false</c>, wenn das Speichern fehlgeschlagen ist.</summary>
    private readonly Func<IReadOnlyList<int>, List<Saisonziel>, bool> _speichern;

    /// <summary>Jahr, dessen Zeilen gerade angezeigt werden.</summary>
    private int _angezeigtesJahr;

    /// <summary>
    /// Erzeugt das ViewModel und zeigt die Ziele des laufenden Jahres.
    /// </summary>
    /// <param name="aktivitaeten">Die Aktivitäten (mit Grundziel).</param>
    /// <param name="ziele">Die gespeicherten eigenen Ziele; sie werden nicht verändert.</param>
    /// <param name="laufendesJahr">Das laufende Jahr.</param>
    /// <param name="speichern">Speichert die Ziele der übergebenen Jahre.</param>
    public SaisonzieleViewModel(IReadOnlyList<Aktivitaet> aktivitaeten, IEnumerable<Saisonziel> ziele, int laufendesJahr,
        Func<IReadOnlyList<int>, List<Saisonziel>, bool> speichern)
    {
        _aktivitaeten = aktivitaeten;
        _speichern = speichern;
        _arbeitskopie = ziele.Select(Kopie).ToList();
        Jahre = [laufendesJahr, laufendesJahr + 1];
        _gewaehltesJahr = laufendesJahr;
        ZeigeJahr(laufendesJahr);
    }

    /// <summary>Wird ausgelöst, sobald gespeichert wurde; das Fenster schließt sich dann.</summary>
    public event EventHandler? Gespeichert;

    /// <summary>Wählbare Jahre: das laufende und das folgende.</summary>
    public IReadOnlyList<int> Jahre { get; }

    /// <summary>Die Zeilen des angezeigten Jahres.</summary>
    public ObservableCollection<Saisonzielzeile> Zeilen { get; } = [];

    /// <summary>Gewähltes Jahr.</summary>
    [ObservableProperty]
    private int _gewaehltesJahr;

    /// <summary>Hinweis zur ersten ungültigen Eingabe; leer, wenn alles gültig ist.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IstGueltig))]
    private string _hinweis = string.Empty;

    /// <summary><c>true</c>, wenn alle Eingaben gültig sind (dann sind „OK“ und der Jahreswechsel möglich).</summary>
    public bool IstGueltig => Hinweis.Length == 0;

    /// <summary>Befehl „OK“: speichert beide Jahre und schließt das Fenster; bei einem Fehler bleibt es offen.</summary>
    [RelayCommand(CanExecute = nameof(IstGueltig))]
    private void Ok()
    {
        UebernimmZeilen();
        List<Saisonziel> ziele = _arbeitskopie.Where(z => Jahre.Contains(z.AbJahr)).Select(Kopie).ToList();
        if (_speichern(Jahre, ziele))
        {
            Gespeichert?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Jahreswechsel: Änderungen des bisherigen Jahres übernehmen, dann das neue Jahr anzeigen.</summary>
    partial void OnGewaehltesJahrChanged(int value)
    {
        if (value == _angezeigtesJahr)
        {
            return;
        }

        UebernimmZeilen();
        ZeigeJahr(value);
    }

    /// <summary>Füllt die Zeilen mit den wirksamen Zielen des Jahres aus der Arbeitskopie.</summary>
    private void ZeigeJahr(int jahr)
    {
        foreach (Saisonzielzeile zeile in Zeilen)
        {
            zeile.PropertyChanged -= Zeile_PropertyChanged;
        }

        Zeilen.Clear();
        foreach (Aktivitaet aktivitaet in _aktivitaeten)
        {
            var zeile = new Saisonzielzeile(aktivitaet, ErmittleSaisonziel.Ausfuehren(aktivitaet, _arbeitskopie, jahr));
            zeile.PropertyChanged += Zeile_PropertyChanged;
            Zeilen.Add(zeile);
        }

        _angezeigtesJahr = jahr;
        Pruefen();
    }

    /// <summary>
    /// Übernimmt die Zeilen des angezeigten Jahres in die Arbeitskopie: Jede Zeile, die ein eigenes Ziel
    /// ergibt, ersetzt das Ziel ihrer Aktivität für dieses Jahr. Nur bei gültigen Eingaben aufrufen.
    /// </summary>
    private void UebernimmZeilen()
    {
        foreach (Saisonzielzeile zeile in Zeilen.Where(z => z.WirdEigenesZiel))
        {
            if (SaisonzielEingaben.Lesen(zeile.Name, zeile.Km, zeile.Hm, zeile.Anzahl).Werte is not { } werte)
            {
                continue;
            }

            _arbeitskopie.RemoveAll(z => z.AktivitaetId == zeile.AktivitaetId && z.AbJahr == _angezeigtesJahr);
            _arbeitskopie.Add(new Saisonziel
            {
                AktivitaetId = zeile.AktivitaetId, AbJahr = _angezeigtesJahr,
                ZielKm = werte.Km, ZielHm = werte.Hm, ZielAnzahl = werte.Anzahl,
            });
        }
    }

    /// <summary>Prüft nach jeder Änderung eines Feldes neu.</summary>
    private void Zeile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Saisonzielzeile.Km) or nameof(Saisonzielzeile.Hm) or nameof(Saisonzielzeile.Anzahl))
        {
            Pruefen();
        }
    }

    /// <summary>Prüft alle Zeilen und aktualisiert Hinweis und „OK“; der Hinweis nennt den ersten Fehler.</summary>
    private void Pruefen()
    {
        Hinweis = Zeilen
            .Select(z => SaisonzielEingaben.Lesen(z.Name, z.Km, z.Hm, z.Anzahl).Hinweis)
            .FirstOrDefault(h => h is not null) ?? string.Empty;
        OkCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Unabhängige Kopie eines Ziels.</summary>
    private static Saisonziel Kopie(Saisonziel z) => new()
    {
        AktivitaetId = z.AktivitaetId, AbJahr = z.AbJahr, ZielKm = z.ZielKm, ZielHm = z.ZielHm, ZielAnzahl = z.ZielAnzahl,
    };
}
