using System.Windows;
using CommunityToolkit.Mvvm.Input;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: die persönlichen Daten des Sportlers im Fenster „Persönliche Daten“ pflegen.
/// </summary>
/// <remarks>
/// AndyTours kennt nur einen Sportler, den ersten im Datenbestand. In einem neuen, leeren Datenordner gibt
/// es noch keinen; dann zeigt das Fenster leere Felder, und beim Speichern wird er angelegt. Wie überall
/// wird zuerst in die Datenbank geschrieben und erst danach der Datenbestand im Speicher geändert.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Befehl „Extras → Persönliche Daten…“: öffnet das Fenster mit den gespeicherten Werten.</summary>
    [RelayCommand]
    private void PersoenlicheDatenOeffnen()
    {
        Fahrer basis = _bestand.Fahrer.FirstOrDefault() ?? new Fahrer();
        var viewModel = new PersoenlicheDatenViewModel(
            FahrerEingaben.AlsEingaben(basis),
            eingaben => FahrerEingaben.AlsFahrer(eingaben, basis, DateOnly.FromDateTime(DateTime.Today)).Hinweis,
            eingaben => SpeicherePersoenlicheDaten(eingaben, basis));
        var fenster = new PersoenlicheDatenFenster(viewModel) { Owner = Application.Current.MainWindow };
        fenster.ShowDialog();
    }

    /// <summary>
    /// Speichert die persönlichen Daten und übernimmt sie danach in den Datenbestand.
    /// </summary>
    /// <param name="eingaben">Die Eingaben des Fensters.</param>
    /// <param name="basis">Der bisherige Sportler bzw. ein leerer.</param>
    /// <returns><c>true</c>, wenn gespeichert wurde und das Fenster schließen darf.</returns>
    private bool SpeicherePersoenlicheDaten(Fahrereingaben eingaben, Fahrer basis)
    {
        if (FahrerEingaben.AlsFahrer(eingaben, basis, DateOnly.FromDateTime(DateTime.Today)).Fahrer is not Fahrer fahrer)
        {
            return false;
        }

        long id = 0;
        if (!Schreibe(verbindung => id = FahrerTabelle.SpeicherePersoenlicheDaten(verbindung, fahrer), "Die persönlichen Daten konnten nicht gespeichert werden"))
        {
            return false;
        }

        // Den Sportler im Bestand ersetzen bzw. (leerer Datenordner) hinzufügen
        fahrer.Id = id;
        int stelle = _bestand.Fahrer.FindIndex(f => f.Id == id);
        if (stelle >= 0)
        {
            _bestand.Fahrer[stelle] = fahrer;
        }
        else
        {
            _bestand.Fahrer.Add(fahrer);
        }

        return true;
    }
}
