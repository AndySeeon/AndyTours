using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="FahrerEingaben"/>.
/// </summary>
public class FahrerEingabenTests
{
    /// <summary>Fester „heutiger“ Tag der Tests.</summary>
    private static readonly DateOnly Heute = new(2026, 10, 24);

    /// <summary>Gültige Eingaben mit allen Feldern.</summary>
    private static readonly Fahrereingaben Gueltig = new()
    {
        Vorname = " Andy ",
        Nachname = "Muster",
        Geburtstag = "28.05.1963",
        Groesse = "182",
        Gewicht = "78,5",
        Ruhepuls = "48",
        Maxpuls = "178",
        Heimatort = " Ingolstadt ",
    };

    /// <summary>Bisheriger Sportler mit Werten, die das Fenster nicht pflegt.</summary>
    private static readonly Fahrer Basis = new()
    {
        Id = 7, TrainingspulsVon = 120, TrainingspulsBis = 150, HeimatBreitengrad = 48.76, HeimatLaengengrad = 11.42,
    };

    /// <summary>Prüft die Eingaben und liefert nur den Hinweis.</summary>
    private static string? Hinweis(Fahrereingaben eingaben) => FahrerEingaben.AlsFahrer(eingaben, Basis, Heute).Hinweis;

    [Fact]
    public void Gueltige_Eingaben_ergeben_den_Sportler_und_behalten_die_uebrigen_Werte()
    {
        (Fahrer? fahrer, string? hinweis) = FahrerEingaben.AlsFahrer(Gueltig, Basis, Heute);

        Assert.Null(hinweis);
        Assert.NotNull(fahrer);
        Assert.Equal(7, fahrer.Id);
        Assert.Equal("Andy", fahrer.Vorname);
        Assert.Equal("Muster", fahrer.Nachname);
        Assert.Equal(new DateOnly(1963, 5, 28), fahrer.Geburtstag);
        Assert.Equal(182, fahrer.GroesseCm);
        Assert.Equal(78.5, fahrer.GewichtKg);
        Assert.Equal(48, fahrer.Ruhepuls);
        Assert.Equal(178, fahrer.Maxpuls);
        Assert.Equal("Ingolstadt", fahrer.Heimatort);
        Assert.Equal(120, fahrer.TrainingspulsVon);
        Assert.Equal(150, fahrer.TrainingspulsBis);
        Assert.Equal(48.76, fahrer.HeimatBreitengrad);
        Assert.Equal(11.42, fahrer.HeimatLaengengrad);
    }

    [Fact]
    public void Leere_Felder_ergeben_unbekannte_Werte()
    {
        (Fahrer? fahrer, string? hinweis) = FahrerEingaben.AlsFahrer(new Fahrereingaben { Groesse = " " }, new Fahrer(), Heute);

        Assert.Null(hinweis);
        Assert.Equal("", fahrer!.Vorname);
        Assert.Null(fahrer.Geburtstag);
        Assert.Equal(0, fahrer.GroesseCm);
        Assert.Equal(0, fahrer.GewichtKg);
        Assert.Equal(0, fahrer.Ruhepuls);
        Assert.Equal(0, fahrer.Maxpuls);
        Assert.Equal("", fahrer.Heimatort);
    }

    [Fact]
    public void Unbekannte_Werte_erscheinen_als_leere_Felder()
    {
        Fahrereingaben eingaben = FahrerEingaben.AlsEingaben(new Fahrer());

        Assert.Equal(new Fahrereingaben(), eingaben);
    }

    [Fact]
    public void Gespeicherte_Werte_erscheinen_in_den_Feldern()
    {
        Fahrereingaben eingaben = FahrerEingaben.AlsEingaben(new Fahrer
        {
            Vorname = "Andy", Nachname = "Muster", Geburtstag = new DateOnly(1963, 5, 28), GroesseCm = 182,
            GewichtKg = 78.5, Ruhepuls = 48, Maxpuls = 178, Heimatort = "Ingolstadt",
        });

        Assert.Equal(Gueltig with { Vorname = "Andy", Heimatort = "Ingolstadt" }, eingaben);
    }

    [Theory]
    [InlineData("31.02.1990", "Der Geburtstag ist ungültig (Form: 28.05.1963).")]
    [InlineData("gestern", "Der Geburtstag ist ungültig (Form: 28.05.1963).")]
    [InlineData("25.10.2026", "Der Geburtstag darf nicht in der Zukunft liegen.")]
    [InlineData("24.10.2026", null)]
    public void Geburtstag_wird_geprueft(string text, string? erwartet) =>
        Assert.Equal(erwartet, Hinweis(Gueltig with { Geburtstag = text }));

    [Theory]
    [InlineData("99", false)]
    [InlineData("100", true)]
    [InlineData("250", true)]
    [InlineData("251", false)]
    [InlineData("1,5", false)]
    public void Groesse_wird_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Die Größe muss eine ganze Zahl von 100 bis 250 cm sein.", Hinweis(Gueltig with { Groesse = text }));

    [Theory]
    [InlineData("29,9", false)]
    [InlineData("30", true)]
    [InlineData("78,5", true)]
    [InlineData("78.5", true)]
    [InlineData("78,55", false)]
    [InlineData("250", true)]
    [InlineData("250,1", false)]
    [InlineData("abc", false)]
    public void Gewicht_wird_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Das Gewicht muss eine Zahl mit höchstens einer Nachkommastelle von 30 bis 250 kg sein.",
            Hinweis(Gueltig with { Gewicht = text }));

    [Theory]
    [InlineData("29", false)]
    [InlineData("30", true)]
    [InlineData("120", true)]
    [InlineData("121", false)]
    [InlineData("48,5", false)]
    public void Ruhepuls_wird_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Der Ruhepuls muss eine ganze Zahl von 30 bis 120 sein.",
            Hinweis(Gueltig with { Ruhepuls = text, Maxpuls = "200" }));

    [Theory]
    [InlineData("99", false)]
    [InlineData("100", true)]
    [InlineData("230", true)]
    [InlineData("231", false)]
    [InlineData("178,5", false)]
    public void Maximalpuls_wird_geprueft(string text, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Der Maximalpuls muss eine ganze Zahl von 100 bis 230 sein.",
            Hinweis(Gueltig with { Ruhepuls = "40", Maxpuls = text }));

    [Theory]
    [InlineData("110", "110", false)]
    [InlineData("120", "110", false)]
    [InlineData("109", "110", true)]
    [InlineData("", "110", true)]
    [InlineData("110", "", true)]
    public void Ruhepuls_muss_kleiner_als_Maximalpuls_sein(string ruhepuls, string maxpuls, bool gueltig) =>
        Assert.Equal(gueltig ? null : "Der Ruhepuls muss kleiner als der Maximalpuls sein.",
            Hinweis(Gueltig with { Ruhepuls = ruhepuls, Maxpuls = maxpuls }));
}
