using System.Windows;

// Legt fest, wo WPF nach Design-Ressourcen (Themes) sucht:
// - Erster Wert: Ressourcen für bestimmte Windows-Designs – werden nicht verwendet (None).
// - Zweiter Wert: allgemeine Ressourcen (Themes/Generic.xaml) liegen in dieser Assembly.
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]
