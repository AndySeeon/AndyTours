using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Equipment-Einsatz“: Kilometer und Höhenmeter je Equipment insgesamt.
/// </summary>
/// <remarks>
/// Die Zeilen berechnet <see cref="ErmittleEquipmenteinsatz"/> einmal beim Öffnen aus allen Einheiten und
/// Equipments – unabhängig von der Auswahl im Hauptfenster. Das Fenster ändert nichts am Datenbestand.
/// Sortiert wird über <see cref="Sortiere"/>, damit die Zeile „Summe“ unten bleibt.
/// </remarks>
public partial class EquipmentEinsatzViewModel : ObservableObject
{
    /// <summary>Die Zeilen der Tabelle, zuletzt „Summe“.</summary>
    [ObservableProperty]
    private List<Einsatzzeile> _zeilen;

    /// <summary>
    /// Erzeugt das ViewModel und berechnet die Zeilen.
    /// </summary>
    /// <param name="bestand">Der aktuelle Datenbestand.</param>
    public EquipmentEinsatzViewModel(Datenbestand bestand)
    {
        _zeilen = ErmittleEquipmenteinsatz.Ausfuehren(bestand.Trainingseinheiten, bestand.Equipments);
        HatZeilen = _zeilen.Count > 0;
    }

    /// <summary><c>true</c>, wenn es Zeilen gibt; sonst zeigt das Fenster den Hinweis „Keine Einheiten vorhanden.“.</summary>
    public bool HatZeilen { get; }

    /// <summary>
    /// Sortiert die Zeilen nach einer Spalte; die Zeile „Summe“ bleibt unten.
    /// </summary>
    /// <param name="spalte">Die angeklickte Spalte.</param>
    /// <param name="aufsteigend"><c>true</c> = aufsteigend, <c>false</c> = absteigend.</param>
    public void Sortiere(Einsatzspalte spalte, bool aufsteigend) =>
        Zeilen = SortiereEinsatzzeilen.Ausfuehren(Zeilen, spalte, aufsteigend);
}
