using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Zeile im Fenster „Saisonziele“: die Ziele einer Aktivität für das gewählte Jahr.
/// </summary>
/// <remarks>
/// Die Werte sind Text, weil der Benutzer auch Ungültiges tippen kann. Die Zeile merkt sich die Texte beim
/// Anzeigen: Erst wenn einer davon geändert wird, wird sie zum eigenen Ziel des Jahres, und die Kennzeichnung
/// „übernommen aus …“ verschwindet.
/// </remarks>
public partial class Saisonzielzeile : ObservableObject
{
    /// <summary>Kennzeichnung beim Anzeigen, z. B. „übernommen aus 2026“.</summary>
    private readonly string _herkunft;

    /// <summary>Texte beim Anzeigen (zum Erkennen einer Änderung).</summary>
    private readonly (string Km, string Hm, string Anzahl) _anfang;

    /// <summary>
    /// Erzeugt die Zeile aus dem wirksamen Ziel.
    /// </summary>
    /// <param name="aktivitaet">Die Aktivität.</param>
    /// <param name="ziel">Das wirksame Ziel im gewählten Jahr.</param>
    public Saisonzielzeile(Aktivitaet aktivitaet, WirksamesSaisonziel ziel)
    {
        AktivitaetId = aktivitaet.Id;
        Name = aktivitaet.Name;
        IstEigenesZiel = ziel.IstEigenes;
        _herkunft = ziel.Herkunft;
        _km = SaisonzielEingaben.AlsText(ziel.ZielKm);
        _hm = SaisonzielEingaben.AlsText(ziel.ZielHm);
        _anzahl = SaisonzielEingaben.AlsText(ziel.ZielAnzahl);
        _anfang = (_km, _hm, _anzahl);
    }

    /// <summary>Nummer der Aktivität.</summary>
    public long AktivitaetId { get; }

    /// <summary>Name der Aktivität.</summary>
    public string Name { get; }

    /// <summary><c>true</c>, wenn für das Jahr schon beim Anzeigen ein eigenes Ziel eingetragen war.</summary>
    public bool IstEigenesZiel { get; }

    /// <summary>Ziel Kilometer als Text.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Herkunft))]
    private string _km;

    /// <summary>Ziel Höhenmeter als Text.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Herkunft))]
    private string _hm;

    /// <summary>Ziel Anzahl als Text.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Herkunft))]
    private string _anzahl;

    /// <summary><c>true</c>, wenn mindestens ein Wert gegenüber der Anzeige geändert wurde.</summary>
    public bool IstGeaendert => (Km, Hm, Anzahl) != _anfang;

    /// <summary><c>true</c>, wenn die Zeile beim Speichern ein eigenes Ziel des Jahres ergibt.</summary>
    public bool WirdEigenesZiel => IstEigenesZiel || IstGeaendert;

    /// <summary>Kennzeichnung „übernommen aus …“; leer bei einem eigenen oder geänderten Ziel.</summary>
    public string Herkunft => WirdEigenesZiel ? string.Empty : _herkunft;
}
