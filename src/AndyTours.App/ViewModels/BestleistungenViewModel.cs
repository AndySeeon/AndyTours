using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Bereichs „Bestleistungen“ auf der Karteikarte „Aktuell“: Jahres- und Einzelrekorde.
/// </summary>
/// <remarks>
/// Die Rekorde berechnet <see cref="ErmittleBestleistungen"/>, die Texte <see cref="BeschreibeBestleistungen"/>.
/// Bei jeder Aktualisierung werden neue Listen zugewiesen, damit die Oberfläche die Änderung bemerkt.
/// </remarks>
public partial class BestleistungenViewModel : ObservableObject
{
    /// <summary>Zeilen der Jahresrekorde (km, Anzahl, hm, Fahrzeit) mit Rekordjahr.</summary>
    [ObservableProperty]
    private IReadOnlyList<Bestleistungszeile> _jahresrekorde = [];

    /// <summary>Zeilen der Einzelrekorde mit Datum und Name der Einheit.</summary>
    [ObservableProperty]
    private IReadOnlyList<Bestleistungszeile> _einzelrekorde = [];

    /// <summary><c>true</c>, wenn die Auswahl Einheiten enthält; sonst entfällt der Bereich.</summary>
    [ObservableProperty]
    private bool _hatEinheiten;

    /// <summary>
    /// Berechnet die Bestleistungen für eine neue Auswahl.
    /// </summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Aktivität der Werkzeugleiste; <c>null</c> = alle Aktivitäten.</param>
    /// <param name="kategorie">Kategorie der Filterleiste; <c>null</c> = alle.</param>
    /// <param name="heute">Das heutige Datum (für die Hervorhebung des laufenden Jahres).</param>
    public void Aktualisiere(Datenbestand bestand, long? aktivitaetId, string? kategorie, DateOnly heute)
    {
        (Jahresrekorde, Einzelrekorde) = BeschreibeBestleistungen.Ausfuehren(ErmittleBestleistungen.Ausfuehren(bestand, aktivitaetId, kategorie), heute);

        // Gibt es Einheiten, hat mindestens ein Jahr eine Anzahl größer 0 und damit einen Jahresrekord
        HatEinheiten = Jahresrekorde.Count > 0;
    }
}
