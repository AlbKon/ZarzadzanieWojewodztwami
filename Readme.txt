1. Najpierw dodajemy województwa.
przyk³adowy Json
{
  "name": "Wielkopolska"
}

2. PóŸniej dodajemy Powiaty z Id Województwa
przyk³adowy Json
{
  "name": "Powiat Koniñski",
  "wojewodztwoId": 1
}

3. PóŸniej dodajemy Gminy z Id Powiatu
przyk³adowy Json
{
  "name": "Gmina Konin",
  "powiatId": 1
}

4. PóŸniej dodajemy miasto z Populacj¹ i Id Gminy
przyk³adowy Json
{
  "name": "Konin",
  "population": 50000,
  "gminaId": 1
}