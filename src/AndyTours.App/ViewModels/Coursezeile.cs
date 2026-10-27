using CommunityToolkit.Mvvm.ComponentModel;
using AndyTours.Core.Modell;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Zeile im Fenster „Auf Garmin übertragen“: ein Course mit seinem änderbaren Namen.
/// </summary>
/// <remarks>
/// Ein Course entsteht entweder aus einer Planung (<see cref="Planung"/>) oder aus der gefahrenen Strecke einer Etappe
/// (<see cref="Verlauf"/>); genau eines von beiden ist gesetzt.
/// </remarks>
public sealed partial class Coursezeile : ObservableObject
{
    /// <summary>Erzeugt die Zeile einer Planung.</summary>
    /// <param name="planung">Die Planung.</param>
    /// <param name="name">Vorgeschlagener Name.</param>
    public Coursezeile(Tourenplanung planung, string name)
    {
        Planung = planung;
        _name = name;
        Bezeichnung = "Planung";
    }

    /// <summary>Erzeugt die Zeile einer gefahrenen Strecke.</summary>
    /// <param name="verlauf">Die Punkte der Aufzeichnung (bei bearbeitetem Track die bearbeitete Fassung).</param>
    /// <param name="name">Vorgeschlagener Name.</param>
    /// <param name="bezeichnung">Bezeichnung vor dem Namensfeld, z. B. „Etappe 2“.</param>
    public Coursezeile(IReadOnlyList<Verlaufspunkt> verlauf, string name, string bezeichnung)
    {
        Verlauf = verlauf;
        _name = name;
        Bezeichnung = bezeichnung;
    }

    /// <summary>Die Planung oder <c>null</c>.</summary>
    public Tourenplanung? Planung { get; }

    /// <summary>Die gefahrene Strecke oder <c>null</c>.</summary>
    public IReadOnlyList<Verlaufspunkt>? Verlauf { get; }

    /// <summary>Bezeichnung vor dem Namensfeld.</summary>
    public string Bezeichnung { get; }

    /// <summary>Name des Course, wie er auf dem Edge erscheint.</summary>
    [ObservableProperty]
    private string _name;
}
