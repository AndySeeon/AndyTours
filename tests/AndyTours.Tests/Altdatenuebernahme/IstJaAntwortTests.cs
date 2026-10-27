using AndyTours.Altdatenuebernahme;

namespace AndyTours.Tests.Altdatenuebernahme;

/// <summary>
/// Tests für <see cref="IstJaAntwort"/>.
/// </summary>
public class IstJaAntwortTests
{
    [Theory]
    [InlineData("j")]
    [InlineData("J")]
    [InlineData("ja")]
    [InlineData(" Ja ")]
    [InlineData("﻿j")]
    public void Ausdrueckliches_Ja_wird_erkannt(string eingabe)
    {
        Assert.True(IstJaAntwort.Ausfuehren(eingabe));
    }

    [Theory]
    [InlineData("")]
    [InlineData("n")]
    [InlineData("nein")]
    [InlineData("y")]
    [InlineData("jaaa")]
    [InlineData(null)]
    public void Alles_andere_gilt_als_Nein(string? eingabe)
    {
        Assert.False(IstJaAntwort.Ausfuehren(eingabe));
    }
}
