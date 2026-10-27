using CommunityToolkit.Mvvm.ComponentModel;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Kategorie mit Häkchen in der Kategorienliste des Bearbeitungsfensters.
/// </summary>
public partial class Kategorieauswahl : ObservableObject
{
    /// <summary>Erzeugt den Eintrag.</summary>
    /// <param name="id">Nummer der Kategorie.</param>
    /// <param name="name">Name der Kategorie.</param>
    /// <param name="istGewaehlt">Anfangszustand des Häkchens.</param>
    public Kategorieauswahl(long id, string name, bool istGewaehlt)
    {
        Id = id;
        Name = name;
        _istGewaehlt = istGewaehlt;
    }

    /// <summary>Nummer der Kategorie.</summary>
    public long Id { get; }

    /// <summary>Name der Kategorie.</summary>
    public string Name { get; }

    /// <summary><c>true</c>, wenn die Kategorie der Einheit zugeordnet ist.</summary>
    [ObservableProperty]
    private bool _istGewaehlt;
}
