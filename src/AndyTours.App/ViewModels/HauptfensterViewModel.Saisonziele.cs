using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die Saisonziele im Fenster „Saisonziele“ pflegen.
/// </summary>
/// <remarks>
/// Wie überall wird zuerst in die Datenbank geschrieben und erst danach der Datenbestand im Speicher geändert.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Befehl „Extras → Saisonziele…“: öffnet das Fenster mit den Zielen des laufenden Jahres.</summary>
    [RelayCommand]
    private void SaisonzieleOeffnen()
    {
        var viewModel = new SaisonzieleViewModel(_bestand.Aktivitaeten, _bestand.Saisonziele, DateTime.Today.Year, SpeichereSaisonziele);
        var fenster = new SaisonzieleFenster(viewModel) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }

    /// <summary>
    /// Ersetzt die Ziele der gepflegten Jahre in der Datenbank und danach im Datenbestand.
    /// </summary>
    /// <param name="jahre">Die gepflegten Jahre (laufendes und folgendes).</param>
    /// <param name="ziele">Die eigenen Ziele dieser Jahre.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde und das Fenster schließen darf.</returns>
    private bool SpeichereSaisonziele(IReadOnlyList<int> jahre, List<Saisonziel> ziele)
    {
        if (!Schreibe(verbindung => SaisonzieleTabelle.SpeichereJahre(verbindung, jahre, ziele), "Die Saisonziele konnten nicht gespeichert werden"))
        {
            return false;
        }

        _bestand.Saisonziele.RemoveAll(z => jahre.Contains(z.AbJahr));
        _bestand.Saisonziele.AddRange(ziele);
        return true;
    }
}
