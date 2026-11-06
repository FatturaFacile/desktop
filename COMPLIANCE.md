# Compliance Status — FatturaFacile

Ultimo aggiornamento: 2026-11-06. Questo documento descrive **lo stato reale** della conformità normativa.
Nessuna funzionalità va presentata come "certificata" o "conforme" oltre quanto riportato qui.

## Legenda
- ✅ Implementato e coperto da test
- 🟡 Implementato parzialmente / richiede configurazione
- ❌ Non implementato

## 🇨🇭 Svizzera

| Requisito | Stato | Note |
|---|---|---|
| QR-bill: payload SPC 0200 (Swiss Implementation Guidelines) | ✅ | `SwissQrBillPayloadBuilder`, solo indirizzi strutturati (tipo S) |
| QR-bill: IBAN/QR-IBAN, QRR (mod 10 ricorsivo), SCOR (ISO 11649) | ✅ | `IbanValidator`, `SwissPaymentReference` |
| QR-bill: codice QR reale + croce svizzera, sezione pagamento e ricevuta | 🟡 | SVG in mm (`SwissQrBillSvgRenderer`), disegnata su una pagina A4 aggiuntiva del PDF della fattura (`QrBillPdfPainter`); verificare la stampa con il validatore SIX |
| QR-bill multilingua (DE/FR/EN) | ❌ | Etichette solo in italiano |
| eBill | ❌ | |
| IVA (MWST): aliquote 8.1/2.6/3.8, rendiconto AFC | 🟡 | Calcolo del rendiconto con metodo effettivo (cifra d'affari, imposta dovuta, imposta precedente) e chiusura con registrazione contabile; non ancora l'invio elettronico ad AFC (ePortal/eMWST) né i metodi delle aliquote saldo/forfettarie |
| Paghe: AVS/AI/IPG, AD con massimale, LPP coordinato + accrediti età | 🟡 | `SwissPayrollEngine`: genere salario, massimali cumulati annui, età di riferimento AVS 21, elaborazioni mensili con chiusura e riapertura, buste paga stampabili da modello Word |
| Paghe: LAINF, IGM, CAF | 🟡 | Aliquote configurate per dipendente |
| Imposta alla fonte | 🟡 | Importazione dei file tariffari ESTV (record 06) e calcolo da tariffa/cantone/figli/chiesa; i file vanno scaricati dall'AFC ogni anno |
| Certificato di salario (Lohnausweis, modulo 11) | 🟡 | Generazione annuale dai cedolini chiusi e stampa da modello; non è il modulo ufficiale AFC con codice a barre |
| Swissdec ELM (trasmissione) | ❌ | Rinviato su richiesta; necessario per la certificazione |
| **Certificazione Swissdec** | ❌ | Processo formale con Swissdec dopo l'implementazione ELM |
| Tenuta libri (CO 957 ss., OLC/GeBüV): immutabilità e tracciabilità | 🟡 | Documenti emessi e registrazioni contabilizzate bloccati; cancellazione logica; audit trail completo solo per le timbrature |
| Banca: estratti ISO 20022 camt.053/054 e riconciliazione QRR/SCOR | ✅ | `CamtStatementParser`, `BankReconciler`, abbinamento manuale con registrazione |
| Fatture fornitori, cespiti e ammortamenti | 🟡 | IVA detraibile e registrazione in contabilità; ammortamento lineare e decrescente con piano annuale; piano dei conti standard minimo |

## 🇮🇹 Italia

| Requisito | Stato | Note |
|---|---|---|
| FatturaPA v1.2 (FPR12/FPA12) con namespace e blocchi obbligatori | ✅ | `FatturaPaXmlBuilder` |
| Riepiloghi IVA per aliquota/natura, Natura N1–N7 | ✅ | `FatturaPaRules` |
| Split payment, reverse charge, ritenuta, bollo virtuale, DatiPagamento | ✅ | |
| TipoDocumento | 🟡 | TD01, TD02, TD04, TD05; mancano TD16–TD29 (integrazioni/autofatture) |
| Validazione pre-invio | 🟡 | Regole principali; manca validazione XSD ufficiale (aggiungere lo schema delle specifiche tecniche vigenti, v1.9.1 dal 15/05/2026) |
| Trasmissione SDI (intermediario / web service / SFTP / PEC) | ❌ | |
| Ricevute SDI (RC, NS, MC, esiti PA) e ciclo passivo | ❌ | |
| Firma digitale (CAdES/XAdES) | ❌ | Obbligatoria solo per alcuni flussi PA |
| Conservazione a norma | ❌ | Tramite conservatore accreditato |
| Paghe: INPS, IRPEF 23/33/43, detrazioni art. 13, cuneo fiscale | 🟡 | Parametri 2026 in `ItalianPayrollParameters`; ritenuta ripartita sulle mensilità, senza conguaglio |
| CCNL, LUL, UniEmens, F24, CU, 770 | ❌ | |
| Liquidazione IVA periodica | 🟡 | Calcolo da vendite e acquisti con credito precedente e interessi trimestrali 1%, chiusura con registrazione; manca il file XML LIPE per l'invio |

## ⏱ Registrazione della durata del lavoro (LL, OLL1, nLPD)

Riferimenti: art. 46 LL, art. 73, 73a e 73b OLL1, indicazioni SECO.

| Requisito | Stato | Note |
|---|---|---|
| Durata giornaliera e settimanale del lavoro | ✅ | Timbrature entrata/uscita (`TimeClockEvent`), totali giornalieri e settimanali (`SwissWorkingTimeEngine`) |
| Lavoro straordinario e lavoro supplementare (art. 12–13 LL) | ✅ | Ore oltre l'orario contrattuale e oltre il massimo legale 45/50 h; tetto annuo 170/140 h |
| Pause di 30 minuti o più (art. 15 LL) | ✅ | Inizio e fine pausa registrati; controllo 15/30/60 minuti secondo la durata giornaliera |
| Periodi di lavoro e di riposo (art. 15a–20 LL) | ✅ | Riposo giornaliero 11 h, lavoro notturno e domenicale, giorni festivi cantonali (Ticino completo, altri cantoni da integrare) |
| Saldo ore | ✅ | Saldo per periodo rispetto all'orario contrattuale |
| Storico delle modifiche (audit log) | ✅ | Le correzioni richiedono un motivo; `TimeAuditEntry` non modificabile né cancellabile, le timbrature non si cancellano fisicamente |
| Registrazione semplificata (art. 73b OLL1) | ✅ | Modalità per dipendente: solo durata giornaliera, con data dell'accordo |
| Rinuncia alla registrazione (art. 73a OLL1) | 🟡 | Modalità disponibile con data dell'accordo; la verifica delle condizioni (salario minimo, autonomia, CCL) è a carico dell'azienda |
| Conservazione 5 anni (art. 73 cpv. 2 OLL1) | 🟡 | Dati inclusi nei backup; manca una politica automatica di conservazione e cancellazione |
| Accesso del dipendente ai propri dati (nLPD) | 🟡 | Permesso dedicato per la sola timbratura e la consultazione; manca l'export per il dipendente |

## 🔐 Sicurezza e multitenancy

| Requisito | Stato | Note |
|---|---|---|
| Filtro tenant su tutte le entità di fatturazione | ✅ | Inclusi `InvoiceQrBillData` e `InvoiceSdiMetadata` |
| Windows Hello fail-closed | ✅ | Fallback solo in build Debug |
| Utenti, ruoli e autorizzazione per tenant | 🟡 | Utenti globali con ruolo per azienda (Viewer, Sales, Accountant, Payroll, Administrator), permessi applicati nella pipeline MediatR, cambio azienda protetto, password PBKDF2 con blocco dopo 5 tentativi. Il primo account Windows diventa amministratore di sistema. Schermata di gestione utenti disponibile |
| Cifratura dei dati personali | 🟡 | AES-256-GCM sui campi sensibili (n. AVS/codice fiscale, email e telefono dei dipendenti, IBAN dei contatti), chiave protetta con DPAPI in `%LOCALAPPDATA%\FatturaFacile\field-encryption.key` (da includere nei backup) |
| Backup | ✅ | Archivio ZIP cifrato con password: database, chiave dei campi, modelli di stampa e documenti archiviati della cartella dati |
| Cifratura dell'intero file database | ❌ | Da SQLitePCLRaw 3.0 (usato da EF Core 11) le build SQLCipher/SQLite3MC gratuite non sono più distribuite: servono pacchetti commerciali (Zetetic o SourceGear) oppure BitLocker sul disco |
| Migrazioni EF Core | ✅ | Migrazione `InitialCreate`; un DB creato con `EnsureCreated` viene salvato come `*.legacy-<data>.bak` e ricreato. Nuove migrazioni: `dotnet tool restore` e `dotnet ef migrations add <Nome> --project src/FatturaFacile.Infrastructure --startup-project src/FatturaFacile.Wpf --output-dir Persistence/Migrations` |
| Protezione dati (nLPD / GDPR) | ❌ | Registro trattamenti, cancellazione, export |
