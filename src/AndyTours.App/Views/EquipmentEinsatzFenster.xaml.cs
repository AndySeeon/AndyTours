using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AndyTours.App.ViewModels;
using AndyTours.Core.Modell;

namespace AndyTours.App.Views;

/// <summary>
/// Code-Behind des Fensters „Equipment-Einsatz“.
/// </summary>
/// <remarks>
/// Die Zeilen liegen im <see cref="EquipmentEinsatzViewModel"/>. Hier wird nur das Sortieren der Tabelle
/// abgefangen: Die eingebaute Sortierung würde die Zeile „Summe“ mitsortieren, deshalb sortiert das ViewModel,
/// und der Sortierpfeil wird hier gesetzt. „Schließen“ ist als Abbrechen-Schaltfläche (<c>IsCancel</c>) markiert
/// und schließt ohne eigenen Code.
/// </remarks>
public partial class EquipmentEinsatzFenster : Window
{
    /// <summary>
    /// Erzeugt das Fenster und verbindet es mit dem ViewModel.
    /// </summary>
    /// <param name="viewModel">ViewModel mit den Zeilen der Auskunft.</param>
    public EquipmentEinsatzFenster(EquipmentEinsatzViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    /// <summary>
    /// Sortiert nach der angeklickten Spalte: beim ersten Klick bei Zahlen absteigend, beim Namen aufsteigend;
    /// jeder weitere Klick kehrt die Reihenfolge um. Die Zeile „Summe“ bleibt unten.
    /// </summary>
    private void Tabelle_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        Einsatzspalte spalte = e.Column == SpalteEquipment ? Einsatzspalte.Equipment
            : e.Column == SpalteVorherKm ? Einsatzspalte.VorherKm
            : e.Column == SpalteGesamtKm ? Einsatzspalte.GesamtKm
            : Einsatzspalte.GesamtHm;

        // Erster Klick: Namen A–Z, Zahlen vom größten Wert an; danach jeweils umkehren
        bool aufsteigend = e.Column.SortDirection switch
        {
            ListSortDirection.Ascending => false,
            ListSortDirection.Descending => true,
            _ => spalte == Einsatzspalte.Equipment,
        };

        ((EquipmentEinsatzViewModel)DataContext).Sortiere(spalte, aufsteigend);

        // Sortierpfeil nur an der angeklickten Spalte
        foreach (DataGridColumn spalteDerTabelle in Tabelle.Columns)
        {
            spalteDerTabelle.SortDirection = null;
        }
        e.Column.SortDirection = aufsteigend ? ListSortDirection.Ascending : ListSortDirection.Descending;
    }
}
