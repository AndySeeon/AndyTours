using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die Auskunft „Equipment-Einsatz“ öffnen.
/// </summary>
public partial class HauptfensterViewModel
{
    /// <summary>
    /// Befehl „Extras → Equipment-Einsatz…“: zeigt km und hm je Equipment über alle Einheiten,
    /// unabhängig von der Jahres- und Aktivitätsauswahl.
    /// </summary>
    [RelayCommand]
    private void EquipmentEinsatzOeffnen()
    {
        var fenster = new EquipmentEinsatzFenster(new EquipmentEinsatzViewModel(_bestand)) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }
}
