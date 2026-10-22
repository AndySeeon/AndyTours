using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fortschrittsfensters: bereitet einen <see cref="KopierFortschritt"/> für die Anzeige auf.
/// </summary>
/// <remarks>
/// Allgemein gehalten, damit das Fenster für jeden länger dauernden Kopiervorgang dient
/// (Datenordner wechseln, später auch die Datensicherung).
/// </remarks>
public partial class FortschrittsViewModel : ObservableObject
{
    /// <summary>Ein Megabyte in Byte (1024 × 1024), für die Mengenangabe.</summary>
    private const double BytesJeMegabyte = 1024 * 1024;

    /// <summary>
    /// Erzeugt das ViewModel.
    /// </summary>
    /// <param name="titel">Fenstertitel, z. B. „Daten werden kopiert …“.</param>
    public FortschrittsViewModel(string titel)
    {
        Titel = titel;
    }

    /// <summary>Fenstertitel.</summary>
    public string Titel { get; }

    /// <summary>Fortschritt in Prozent (0–100) für den Balken.</summary>
    [ObservableProperty]
    private double _prozent;

    /// <summary>Text „Dateien: x von y“.</summary>
    [ObservableProperty]
    private string _dateienText = "Dateien werden gezählt …";

    /// <summary>Text „Datenmenge: x von y MB“.</summary>
    [ObservableProperty]
    private string _mengeText = string.Empty;

    /// <summary>
    /// Übernimmt einen neuen Zwischenstand. Wird über <see cref="Progress{T}"/> im Oberflächen-Thread aufgerufen.
    /// </summary>
    /// <param name="stand">Der gemeldete Zwischenstand.</param>
    public void Aktualisiere(KopierFortschritt stand)
    {
        // Anteil nach Datenmenge, weil große Dateien länger dauern; ohne Datenmenge (nur leere Dateien) nach Anzahl
        Prozent = stand.GesamtBytes > 0
            ? 100.0 * stand.KopierteBytes / stand.GesamtBytes
            : 100.0 * stand.KopierteDateien / Math.Max(1, stand.GesamtDateien);

        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        DateienText = string.Format(de, "Dateien: {0:N0} von {1:N0}", stand.KopierteDateien, stand.GesamtDateien);
        MengeText = string.Format(de, "Datenmenge: {0:N1} von {1:N1} MB",
            stand.KopierteBytes / BytesJeMegabyte, stand.GesamtBytes / BytesJeMegabyte);
    }
}
