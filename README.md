# ZarzadzanieWojewodztwami
Projekt na potrzeby uczelniane. ASP.NET minimal Api, z relacyjną bazą danych, EF Core

Przykładowy scenariusz użycia, którym testowałem program.
Podczas używania losow sprawdzałem faktyczne wartości z bazy danych
poprzez używanie Getów.

Można było jeszcze rozbić program na Kontrolery oraz dodać Dto,
jednak myślę że główny zamysł zadania został już spełniony.

DODAWANIE DANYCH
1. Najpierw dodajemy województwa.
przykładowe Json
{
  "name": "Wielkopolska"
}

{
  "name": "Śląskie"
}

2. Później dodajemy Powiaty z Id Województwa
przykładowy Json
{
  "name": "Powiat Koniński",
  "wojewodztwoId": 1
}

{
  "name": "Powiat Legnicki",
  "wojewodztwoId": 2
}

3. Później dodajemy Gminy z Id Powiatu
przykładowy Json
{
  "name": "Gmina Konin",
  "powiatId": 1
}

{
  "name": "Gmina Legnica",
  "powiatId": 2
}


4. Później dodajemy miasto z Populacją i Id Gminy
przykładowy Json
{
  "name": "Konin",
  "population": 50000,
  "gminaId": 1
}

{
  "name": "Legnica",
  "population": 70000,
  "gminaId": 2
}

USTAWIANIE STOLIC
1. Ustawiamy stolice

/api/wojewodztwo/setcapital
wojewodztwoId = 1
miastoId = 1

/api/gmina/setcapital
gminaId = 2
miastoId = 2

USUWANIE
/api/wojewodztwo
idWojewodztwa = 2

/api/gmina
idGminy = 1