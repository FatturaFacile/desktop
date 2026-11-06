# Trasmissione SDI — configurazione del canale

FatturaFacile invia le fatture elettroniche e riceve notifiche e fatture fornitori tramite un **adattatore** (`ISdiTransmitter`).
Il canale si sceglie in `appsettings.json`:

```json
"Sdi": {
  "Channel": "Folder",
  "BaseUrl": "",
  "ApiKey": ""
}
```

## Canale "Folder" (predefinito)

Le cartelle sono nella cartella dati dell'applicazione (`%LOCALAPPDATA%\FatturaFacile` o quella configurata):

| Cartella | Uso |
|---|---|
| `SDI\Uscita` | Fatture FatturaPA da trasmettere (pulsante *Invia allo SDI*) |
| `SDI\Entrata` | Notifiche SDI (`*_RC_*`, `*_NS_*`, `*_MC_*`, `*_NE_*`, `*_DT_*`, `*_AT_*`) e fatture fornitori (`.xml` / `.xml.p7m`) |
| `SDI\Elaborati` | File già elaborati (pulsante *Ricevute SDI*) |

Le cartelle si possono usare con il portale *Fatture e Corrispettivi* dell'Agenzia delle Entrate (caricamento e scaricamento manuali),
con la PEC, oppure con un intermediario che sincronizza le cartelle (SFTP, client desktop).

## Canale "Http" (intermediario generico)

Con `"Channel": "Http"` l'applicazione chiama un servizio REST dell'intermediario accreditato (o un piccolo adattatore
scritto per il suo API). `ApiKey` viene inviata come `Authorization: Bearer <chiave>`.

| Operazione | Richiesta | Risposta |
|---|---|---|
| Invio fattura | `PUT {BaseUrl}/outbox/{NomeFile}` con il file XML (`application/xml`) | `2xx`; il corpo, se presente, è l'identificativo restituito |
| Elenco file ricevuti | `GET {BaseUrl}/inbox` | `[{ "fileName": "...", "content": "<Base64>" }]` |
| Conferma elaborazione | `DELETE {BaseUrl}/inbox/{NomeFile}` | `2xx` |

## Comportamento

- Si possono inviare solo fatture italiane **emesse** con file FatturaPA valido.
- Una fattura scartata (NS) riceve un nuovo progressivo nel nome file e può essere reinviata; gli errori di scarto sono salvati.
- RC/MC impostano la data di consegna; NE (EC01/EC02) e DT aggiornano lo stato delle fatture verso la PA.
- Le fatture fornitori ricevute diventano bozze in *Finanza → Fatture fornitori*; il fornitore viene creato se non esiste
  (ricerca per partita IVA o codice fiscale). Le fatture già importate (stesso fornitore, numero e data) vengono saltate.
- I file P7M vengono aperti senza verificare la firma, che lo SDI ha già controllato.
