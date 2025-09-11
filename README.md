# 🚀 Projekat iz Sistemskog programiranja  
## SpaceX Web Server – Pretraga letova

### 🎓 Fakultet
Elektronski fakultet u Nišu  
Katedra za Računarstvo  

**Predmet:** Sistemsko programiranje  
**Školska godina:** 2024/2025  

👤 Autori:  
- Bogdan Jovanović - 19153  
- Lazar Krstić - 19190  

---

## 📌 Tekst zadatka
> **Zadatak 27 (prvi i drugi projekat):**  
> Kreirati Web server koji klijentu omogućava **pretragu letova korišćenjem SpaceX API-a**.  
> Pretraga se može vršiti pomoću filtera koji se zadaju u okviru query-a.  
> Spisak letova koji zadovoljavaju uslov vraća se kao odgovor klijentu.  
> Svi zahtevi serveru šalju se preko browsera korišćenjem **GET metode**.  
> Ukoliko navedene informacije ne postoje, prikazati grešku korisniku.  
>
> Način funkcionisanja SpaceX API-a je moguće proučiti na sledećem linku:  
> [https://github.com/r-spacex/SpaceX-API](https://github.com/r-spacex/SpaceX-API)  
>
> **Primer poziva API-ju:**  
> [https://api.spacexdata.com/v5/launches/past](https://api.spacexdata.com/v5/launches/past)

---

## ⚙️ Funkcionisanje programa
Naš server radi kao **mali HTTP server** koji prima GET zahteve iz browsera.  
Korisnik u adresi ukuca upit (sa filterima), a server interno šalje zahtev prema SpaceX API-ju i vraća JSON odgovor.

**Glavne karakteristike:**
- Server osluškuje na: `http://localhost:5055/`
- Rute:
  - `/health` – health-check (status servera)  
  - `/launches` – pretraga letova (sa query parametrima)  

**Podržani query parametri:**
- `success` → `true` | `false`  
- `upcoming` → `true` | `false`  
- `from` → početni datum (npr. `2020-01-01`)  
- `to` → krajnji datum (npr. `2020-12-31`)  
- `name` → deo naziva leta (case-insensitive)  
- `limit` → broj rezultata (1–50)  
- `sort` → `asc` | `desc` (po datumu lansiranja)  

**Primeri poziva:**
- `http://localhost:5055/health`  
- `http://localhost:5055/launches?limit=5`  
- `http://localhost:5055/launches?success=true&limit=3`  
- `http://localhost:5055/launches?from=2020-01-01&to=2020-12-31&limit=3`  
- `http://localhost:5055/launches?name=Starlink&limit=2`  

Ako nijedan let ne odgovara filterima → korisnik dobija grešku u JSON formatu.