using CommunityToolkit.Mvvm.ComponentModel;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Eine Zeile im Fenster „Kategorien zuordnen“: eine Kategorie mit dreistufigem Häkchen.
/// </summary>
/// <remarks>
/// Das Häkchen steht anfangs auf „an“ (alle markierten Einheiten haben die Kategorie), „aus“ (keine) oder
/// „teilweise“ (<c>null</c>, nur manche). Zurück auf „teilweise“ lässt es sich nur stellen, wenn es anfangs
/// so stand (<see cref="KannTeilweise"/>); sonst wechselt es nur zwischen an und aus.
/// </remarks>
public partial class Kategoriezustand : ObservableObject
{
    /// <summary>
    /// Erzeugt die Zeile mit dem Zustand über die markierten Einheiten.
    /// </summary>
    /// <param name="id">Nummer der Kategorie.</param>
    /// <param name="name">Name der Kategorie.</param>
    /// <param name="anfang"><c>true</c> = alle, <c>false</c> = keine, <c>null</c> = teilweise.</param>
    public Kategoriezustand(long id, string name, bool? anfang)
    {
        Id = id;
        Name = name;
        Anfang = anfang;
        _istGewaehlt = anfang;
    }

    /// <summary>Nummer der Kategorie.</summary>
    public long Id { get; }

    /// <summary>Name der Kategorie.</summary>
    public string Name { get; }

    /// <summary>Zustand beim Öffnen des Fensters.</summary>
    public bool? Anfang { get; }

    /// <summary><c>true</c>, wenn das Häkchen auch „teilweise“ annehmen kann (nur wenn es so begann).</summary>
    public bool KannTeilweise => Anfang is null;

    /// <summary>Aktueller Zustand des Häkchens.</summary>
    [ObservableProperty]
    private bool? _istGewaehlt;
}
