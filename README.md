VS code extensions:
C# dev kit
MySQL
Gitlens

Core tools:
XAMPP
wordpress kolun serveri
.net sdk
github desktop
XAMPP


## Koulussa käynnistäminen

Tämä projekti toimii niin, että WordPress-sivu pyörii koulun palvelimella ja ASP.NET API + MySQL-tietokanta omalla koneella.

### 1. Käynnistä MySQL

1. Avaa XAMPP Control Panel.
2. Käynnistä `Apache` ja `MySQL`.
3. Avaa phpMyAdmin ja varmista, että tietokanta on nimeltä `animal_shelter`.
4. Suorita tarvittaessa taulujen luonti- ja testidataskripti uudelleen.

### 2. Tarkista tietokantayhteys

Projektin API käyttää tiedostossa `appsettings.json` yhteyttä:

```json
"server=localhost;user=root;database=animal_shelter;password=;"
```

Jos MySQL:llä on eri käyttäjä tai salasana, päivitä tuo rivi vastaamaan omaa konettasi.

### 3. Käynnistä API

Avaa terminaali projektin juuressa ja aja:

```bash
dotnet run
```

API käynnistyy oletuksena osoitteeseen:

- `http://localhost:5144`

Jos käytössä on HTTPS-profiili, se löytyy myös osoitteesta:

- `https://localhost:7171`

### 4. Varmista että WordPress käyttää oikeaa API-osoitetta

WordPress-sivun JavaScriptissä API-osoite on kovakoodattu näin:

```js
const API = 'http://localhost:5144';
```

Jos sivu ajetaan koulun palvelimelta eikä samalla koneella kuin API, tuo osoite ei toimi koulun käyttäjille. Silloin API:n pitää olla julkisesti saavutettava tai WordPressin koodissa pitää käyttää koneen oikeaa verkko-osoitetta.

### 5. Käynnistä testi käytännössä

1. Avaa WordPressin sivu koulun palvelimella.
2. Kirjaudu sisään tai rekisteröidy.
3. Tee varauslomake loppuun.
4. Jos varaus ei mene läpi, tarkista selaimen Console ja API:n terminaali.

### 6. Yleisimmät ongelmat

- `Unknown column 'user_id'` tarkoittaa, että tietokannan taulut eivät vastaa nykyistä koodia.
- `Cannot add or update a child row` tarkoittaa, että `userId` puuttuu tai käyttäjää ei ole olemassa `users`-taulussa.
- Jos kirjautuminen toimii mutta varaus ei, tarkista että `localStorage` sisältää oikean `userId`-arvon.

### 7. Nopea tarkistuslista ennen esitystä

- MySQL käynnissä
- Tietokanta `animal_shelter` olemassa
- Taulut luotu oikein
- `dotnet run` päällä
- WordPress-sivulla oikea API-osoite
- Käyttäjä kirjautunut sisään ennen varausta


-------------------------------
Työn jako API
- Sanna anto pohja koodin apin 
- Sami lisäsi ja muokkasin api koodia

- käynii yhdessä läpi

- Sanna poisti ylimääräsiä tiedostoja ja teki cleanup työtä
- Sami paransi sala avainta Ja lisäsi kommentteja

- käytiin yhdessä läpi 16.5

-
-------------------------------

