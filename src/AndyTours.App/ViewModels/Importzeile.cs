using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Zeile der Liste im Fenster „Garmin-Import“: eine neue Tour mit Häkchen und änderbarem Equipment.
/// </summary>
/// <remarks>
/// Die angezeigten Werte kommen unverändert aus der gelesenen <see cref="Aufzeichnung"/>. Änderbar sind nur das
/// Häkchen und das Equipment; beides wertet der Aufrufer nach „Importieren“ aus.
/// </remarks>
public partial class Importzeile : ObservableObject
{
    /// <summary>
    /// Erzeugt die Zeile; sie ist angehakt.
    /// </summary>
    /// <param name="aufzeichnung">Die gelesene Aufzeichnung.</param>
    /// <param name="quelldatei">Vollständiger Pfad der gelesenen Datei (Zwischenkopie oder gewählte Datei).</param>
    /// <param name="equipment">Das vorbelegte Equipment („(keines)“ hat die Nummer <c>null</c>).</param>
    public Importzeile(Aufzeichnung aufzeichnung, string quelldatei, Auswahleintrag equipment)
    {
        Aufzeichnung = aufzeichnung;
        Quelldatei = quelldatei;
        _equipment = equipment;
    }

    /// <summary>Die gelesene Aufzeichnung.</summary>
    public Aufzeichnung Aufzeichnung { get; }

    /// <summary>Vollständiger Pfad der gelesenen Datei; von dort wird sie im Datenordner abgelegt.</summary>
    public string Quelldatei { get; }

    /// <summary>Datum der Tour (Ortszeit).</summary>
    public DateOnly Datum => Aufzeichnung.Datum;

    /// <summary>Startzeit der Tour (Ortszeit).</summary>
    public TimeOnly Startzeit => Aufzeichnung.Startzeit;

    /// <summary>Distanz in km.</summary>
    public double DistanzKm => Aufzeichnung.DistanzKm;

    /// <summary>Gestoppte Fahrzeit ohne Pausen.</summary>
    public TimeSpan Fahrzeit => Aufzeichnung.Fahrzeit;

    /// <summary>Fahrzeit wie in der Einheitentabelle, z. B. „5:40:07“.</summary>
    public string FahrzeitText => FormatiereDauer.Ausfuehren(Fahrzeit);

    /// <summary>Profil des Edge, z. B. „MTB“.</summary>
    public string Profil => Aufzeichnung.Profil;

    /// <summary>Name des Geräts, z. B. „Edge 1040“.</summary>
    public string Geraet => Aufzeichnung.Geraet;

    /// <summary>Angehakt = wird importiert.</summary>
    [ObservableProperty]
    private bool _importieren = true;

    /// <summary>Gewähltes Equipment; „(keines)“ hat die Nummer <c>null</c>.</summary>
    [ObservableProperty]
    private Auswahleintrag _equipment;
}
