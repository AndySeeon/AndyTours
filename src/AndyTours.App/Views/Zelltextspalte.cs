using System.Windows.Controls;

namespace AndyTours.App.Views;

/// <summary>
/// Textspalte der Tabelle „Trainingseinheiten“, die trotz nur lesender Bindung bearbeitet werden kann.
/// </summary>
/// <remarks>
/// Die WPF-Textspalte sperrt sich selbst, sobald ihre Bindung nur lesend (<c>OneWay</c>) ist. In der
/// Tabelle soll die Bindung aber nur lesend bleiben – die Zeilen sind unveränderlich, der eingetippte Text
/// wird im Code-Behind gelesen und über das ViewModel gespeichert. Diese Spalte hebt deshalb die
/// automatische Sperre auf; gesperrt ist sie nur, wenn <c>IsReadOnly</c> gesetzt ist oder die ganze
/// Tabelle schreibgeschützt ist.
/// </remarks>
public class Zelltextspalte : DataGridTextColumn
{
    /// <summary>
    /// Ermittelt, ob die Spalte gesperrt ist – ohne Rücksicht auf die Art der Bindung.
    /// </summary>
    /// <param name="baseValue">Der an der Spalte gesetzte Wert von <c>IsReadOnly</c>.</param>
    /// <returns><c>true</c>, wenn die Spalte oder die ganze Tabelle schreibgeschützt ist.</returns>
    protected override bool OnCoerceIsReadOnly(bool baseValue) => baseValue || (DataGridOwner?.IsReadOnly ?? false);
}
