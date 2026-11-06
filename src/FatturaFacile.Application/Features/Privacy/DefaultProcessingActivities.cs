// <copyright file="DefaultProcessingActivities.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// The processing activities carried out with FatturaFacile.
/// </summary>
public static class DefaultProcessingActivities
{
    private const string Measures = "Accesso con utente e password, ruoli e permessi per azienda, cifratura AES-256 dei dati sensibili, backup cifrati, storico delle modifiche";

    /// <summary>
    /// Creates the default entries.
    /// </summary>
    /// <returns>The entries.</returns>
    public static IReadOnlyList<ProcessingActivity> Create() =>
    [
        new ProcessingActivity
        {
            Name = "Fatturazione, acquisti e contabilità",
            Purpose = "Emissione e ricezione delle fatture, incassi e pagamenti, tenuta della contabilità, adempimenti IVA",
            LegalBasis = "Contratto con il cliente o fornitore; obblighi legali (art. 957 ss. CO, LIVA; DPR 633/1972, art. 2214 ss. c.c.)",
            DataSubjects = "Clienti, fornitori e loro persone di contatto",
            DataCategories = "Anagrafica, dati fiscali (partita IVA, codice fiscale, IDI), indirizzi, e-mail/PEC, coordinate bancarie, documenti contabili",
            Recipients = "Sistema di Interscambio (SDI) e Agenzia delle Entrate, AFC (IVA), banche, fiduciario o commercialista, revisore",
            ForeignTransfers = "Nessuno: i dati sono conservati sul computer dell'azienda; fatture verso l'estero secondo il contratto",
            RetentionPeriod = "10 anni dalla fine dell'esercizio (art. 958f CO, art. 2220 c.c.)",
            SecurityMeasures = Measures,
        },
        new ProcessingActivity
        {
            Name = "Gestione del personale e paghe",
            Purpose = "Calcolo e pagamento dei salari, contributi sociali, imposte alla fonte e ritenute, certificati e dichiarazioni",
            LegalBasis = "Contratto di lavoro; obblighi legali (LAVS, LADI, LAINF, LPP, LIFD; leggi previdenziali e fiscali italiane)",
            DataSubjects = "Dipendenti, ex dipendenti e loro familiari",
            DataCategories = "Anagrafica, n. AVS / codice fiscale, permesso di soggiorno, stato civile, figli, salario, conto bancario, assenze (senza diagnosi)",
            Recipients = "Casse AVS e assegni familiari, assicuratori LAINF/IGM, istituto LPP, autorità fiscali, INPS, INAIL, Agenzia delle Entrate, fiduciario",
            ForeignTransfers = "Solo verso le autorità e gli enti competenti del paese di residenza o di lavoro",
            RetentionPeriod = "10 anni (documenti contabili e salariali); dati non più necessari cancellati alla fine del rapporto",
            SecurityMeasures = Measures,
        },
        new ProcessingActivity
        {
            Name = "Registrazione della durata del lavoro",
            Purpose = "Rispetto delle norme sulla durata del lavoro e del riposo, calcolo di saldo ore e straordinari",
            LegalBasis = "Obbligo legale (art. 46 LL, art. 73, 73a e 73b OLL 1)",
            DataSubjects = "Dipendenti",
            DataCategories = "Timbrature, pause, registrazioni giornaliere, correzioni con motivo e autore",
            Recipients = "Ispettorato del lavoro su richiesta; ufficio del personale",
            ForeignTransfers = "Nessuno",
            RetentionPeriod = "5 anni civili, poi cancellazione automatica (art. 73 cpv. 2 OLL 1)",
            SecurityMeasures = Measures + ", storico delle modifiche non modificabile",
        },
        new ProcessingActivity
        {
            Name = "Utenti e sicurezza dell'applicazione",
            Purpose = "Accesso al programma, autorizzazioni e tracciabilità",
            LegalBasis = "Interesse legittimo alla sicurezza dei dati; obbligo di sicurezza (art. 8 nLPD, art. 32 GDPR)",
            DataSubjects = "Utenti del programma",
            DataCategories = "Nome utente, nome, ruoli, hash della password, tentativi di accesso",
            Recipients = "Nessuno",
            ForeignTransfers = "Nessuno",
            RetentionPeriod = "Per la durata dell'account",
            SecurityMeasures = Measures,
        },
    ];
}
