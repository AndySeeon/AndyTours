using System.Reflection;

namespace AndyTours.App.ViewModels;

/// <summary>
/// ViewModel des Fensters „Über AndyTours“ (Spezifikation 034): Name, Version und Autor des Programms.
/// </summary>
/// <remarks>
/// Alle Angaben stammen aus den Dateieigenschaften der Programmdatei, die <c>Directory.Build.props</c> beim Bauen
/// setzt (<c>Product</c>, <c>Version</c>, <c>Copyright</c>). So stehen sie an genau einer Stelle und stimmen mit
/// dem überein, was der Explorer in den Eigenschaften der exe zeigt. Das Icon bindet das Fenster direkt aus den
/// Anwendungsressourcen (<c>ProgrammiconGross</c>).
/// </remarks>
public sealed class UeberViewModel
{
    /// <summary>Die Programmdatei der App, aus deren Angaben gelesen wird.</summary>
    private static readonly Assembly Programm = typeof(UeberViewModel).Assembly;

    /// <summary>Name des Programms, z. B. „AndyTours“.</summary>
    public string Programmname { get; } =
        Programm.GetCustomAttribute<AssemblyProductAttribute>()?.Product ?? "AndyTours";

    /// <summary>Anzeigetext der Version, z. B. „Version 1.0.0“.</summary>
    public string Version { get; } = $"Version {HauptfensterViewModel.Programmversion}";

    /// <summary>Autor laut Copyright-Angabe, z. B. „© Andreas Hensel“; leer, wenn keine gesetzt ist.</summary>
    public string Autor { get; } =
        Programm.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? string.Empty;

    /// <summary>Kurze Beschreibung unter dem Namen.</summary>
    public string Beschreibung { get; } = "Tourenverwaltung für Radltouren – Nachfolger von BikeXperience.";
}
