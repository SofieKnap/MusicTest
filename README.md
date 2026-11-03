# Selenium-test af FrontMusic

Testene i `MusicTest/UnitTest1.cs` tester den nye frontend i Chrome:

- Et kendt nummer og den rigtige kunstner vises på samme kort.
- Søgning efter titel, kunstner og begge dele (tre testkørsler).
- En søgning uden match viser en besked og ingen kort.
- En favorit kan gemmes, overleve genindlæsning og fjernes igen.
- Musikken kan sorteres med de nyeste udgivelser først.
- Detaljevinduet viser titel, kunstner, år og varighed og kan lukkes.

Det giver otte testkørsler i alt.

## Kør testene

1. Start FrontMusic med Live Server på `http://127.0.0.1:5500/index.html`.
2. Kontrollér, at siden viser musikken. Frontend bruger det publicerede Azure-API; det lokale RESTmusic behøver ikke køre.
3. Åbn MusicTest i Visual Studio, vælg **Test → Test Explorer**, og kør testene.

Alternativt: Kør `dotnet test MusicTest.slnx` fra denne mappe.

Chrome og internetadgang skal være tilgængelige. Testene bruger de eksisterende musikdata fra Azure, herunder lowkey af NIKI og Gabriela af KATSEYE. Ændres de data, skal de relevante forventninger også opdateres.

## Sådan er testene opbygget

Hver test bruger **Arrange, Act og Assert**. `WebDriverWait` venter på, at Vue har vist det forventede indhold; der bruges ingen faste pauser. Søgning bruger `[Theory]` og `[InlineData]`, så samme test kan køres med tre forskellige filtre.

Hver test starter sin egen Chrome-session uden gemte favoritter. `using` lukker browseren efter testen, også hvis den fejler. Favorittestene påvirker kun den midlertidige testbrowser og ændrer ikke API'ets musikdata.

Chrome kører uden et synligt vindue (`--headless=new`). Fjern den linje i `OpretBrowser`, hvis I vil se browseren udføre trinnene. Hjælpemetoderne nederst samler browseropsætning og ventetid, så det ikke gentages i hver test.

## Hvis en test fejler

- **Siden kan ikke åbnes:** Start Live Server på port 5500.
- **Timeout, før musikken vises:** Kontrollér internetforbindelsen, Azure-API'et og browserens Console. Vue og Axios hentes også fra internettet.
- **Chrome starter ikke:** Læs fejlen i Test Explorer. Selenium kan have brug for at hente en passende ChromeDriver ved første kørsel.
