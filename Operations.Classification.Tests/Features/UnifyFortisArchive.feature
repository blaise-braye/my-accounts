@UnitTest
Feature: Unify Fortis Operations Archive details
	In order to classify my personal operations,
	I want a more structure operation detail for my fortis transaction archives

#TODO
# what about choice not determinist?
# could multiple localization match is certain circonstances?
# could multiple patterns match in certain circonstances?
# what about generating knowledge from patterns, there must be some deterministic way to find some of them

Scenario: parse 'PermanentOrder'
	Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                                                                                                                  |
	| 2017-0001 | VOTRE ORDRE PERMANENT AU PROFIT DE       01-06          111,00-GEVELCO BE05 3701 2392 6075  BIC BBRUBEBB COMMUNICATION: LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE DATE VALEUR : 01/06/2016                                |
	| 2017-0002 | VOTRE ORDRE PERMANENT AU PROFIT DE       01-05          111,74-NOEL DE BURLIN PIERRE BE44 2100 4308 6745  BIC GEBABEBB COMMUNICATION: LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE EXECUTE LE 01-05 DATE VALEUR : 01/05/2016 |
	| 2017-0003 | VOTRE ORDRE PERMANENT AU PROFIT          01-12            5,00-DU COMPTE 000-0000028-28 DE OXFAM SOLIDARITE ASBL COMMUNICATION: 3209 SOLIDARITY PARTNER DATE VALEUR : 01/12/2011                                                        |
	
	When I unify and transform the read operations
	
	Then the operations data is
	| OperationId | ThirdParty            | PatternName    | Communication                                           | IBAN                | BIC      |
	| 2017-0001   | GEVELCO               | PermanentOrder | LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE | BE05 3701 2392 6075 | BBRUBEBB |
	| 2017-0002   | NOEL DE BURLIN PIERRE | PermanentOrder | LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE | BE44 2100 4308 6745 | GEBABEBB |
	| 2017-0003   | OXFAM SOLIDARITE ASBL | PermanentOrder | 3209 SOLIDARITY PARTNER                                 | 000-0000028-28      |          |
	
			
Scenario: parse 'BankTransfert'
	Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                                                                                                                                                                           |
    # VED
	| 2017-0001 | VIREMENT EUROPEEN DE                     25-11        1.111,29+SELLIGENT S.A. AVENUE DE FINLANDE, 2 1420        BRAINE-L'ALLEU BE02 3600 9651 8440  BIC BBRUBEBB REFERENCE DONNEUR D'ORDRE : 1CS0125-01-0000245-1633010083573582 COMMUNICATION : /A/ 1CS0125-01-0000245 DATE VALEUR : 25/11/2016 |
	| 2017-0002 | VIREMENT EUROPEEN DE                     18-11          111,00+BACQUELAINE SYLVIE RUE DU DUC 23 1150     WOLUWE-SAINT-PIERRE BE70 0013 5026 1925  BIC GEBABEBB PAS DE COMMUNICATION DATE VALEUR : 18/11/2016                                                                                     |
	| 2017-0003 | VIREMENT EUROPEEN DE                     12-10           11,00+MUTUALITE LIBERALE LIEGE SOC MUTU RUE DE HERMEE 177 D 4040        HERSTAL BE46 3400 1288 1436  BIC BBRUBEBB REFERENCE DONNEUR D'ORDRE : 79 COMMUNICATION : 00019161010/2030970/011/014/014/10101                                  |
	| 2017-0004 | VIREMENT EUROPEEN DE                     13-05          111,73+LEMON WAY FR7630004025110001118625268 BIC BNPAFRPP REFERENCE DONNEUR D'ORDRE : KADOLOG-1245 PAS DE COMMUNICATION DATE VALEUR : 13/05/2016                                                                                         |
	| 2017-0005 | VIREMENT EUROPEEN DE                     18-02          800,00+BACQUELAINE DANIEL LE ROSAI 11 4052 CHAUDFONTAINE BE58 0011 5220 3079  BIC GEBABEBB COMMUNICATION: RENTE DATE VALEUR : 18/02/2017                                                                                                 |
    # VEA
	| 2017-0010 | VIREMENT EUROPEEN A                      08-11           11,46-PROXIMUS MOBILE BE31435411161155 BIC KREDBEBB    VIA WEB BANKING COMMUNICATION : 285293318329 EXECUTE LE 08-11 DATE VALEUR : 08/11/2016                                                                                           |
	| 2017-0011 | VIREMENT EUROPEEN A                      08-11           11,96-ELECTRABEL BE46000325448336 BIC BPOTBEB1    VIA WEB BANKING COMMUNICATION : 441874258425 EXECUTE LE 08-11 DATE VALEUR : 08/11/2016                                                                                                |
	| 2017-0012 | VIREMENT EUROPEEN A                      08-11          111,00-CENTRE MEDICAL DU VAL SPRL BE10210039862204 BIC GEBABEBB    VIA WEB BANKING COMMUNICATION : KINE SHIRLEY - NILS BRAYE EXECUTE LE 08-11 DATE VALEUR : 08/11/2016                                                                   |
	| 2017-0013 | VIREMENT EUROPEEN A                      12-11          111,00-PAYPAL EUROPE DE56120700883000443706 BIC DEUTDEDBPAL VIA PC BANKING COMMUNICATION : 91L74R25AM47NYT EXECUTE LE 11-11 DATE VALEUR : 12/11/2014                                                                                     |
	| 2017-0014 | VIREMENT EUROPEEN A                      22-05           11,13-GEOFFRAY LESAGE FR7614707001013081995404823 BIC CCBPFRPPMTZ VIA PC BANKING COMMUNICATION : FOURTOU PARADIZE DATE VALEUR : 22/05/2014                                                                                              |
	| 2017-0015 | VIREMENT EUROPEEN A                      26-03        1.111,00-VERONIQUE THIEBAUT BE20732694113156 BIC CREGBEBB    VIA PC BANKING PAS DE COMMUNICATION DATE VALEUR : 26/03/2014                                                                                                                  |
	| 2017-0016 | VIREMENT EUROPEEN A                      30-07          111,00-SYNDICAT INTERCOMMUNAL DE FLAINE FR323000100302D741000000079 BIC BDFEFRPPCCT VIA PC BANKING COMMUNICATION : REGLEMENT FACTURE 1130654 DATE VALEUR : 30/07/2013                                                                    |
    # VA
	| 2017-0021 | VIREMENT AU COMPTE 001-4427276-78        30-07          111,00-DE INFORIM SPRL VIA PC/WEB BANKING COMMUNICATION: LOYER DU AOUT 2011 - RUE DE LA LOI 16 A 4020 LIEGE - BRAYE EXECUTE LE 30-07 DATE VALEUR : 30/07/2011                                                                            |
	| 2017-0022 | VIREMENT AU COMPTE 001-3502619-25        25-07            1,00-DE SYLVIE BACQUELAINE VIA PC/WEB BANKING COMMUNICATION: MC2 BOOMBOOM H DOMDOM XY EXECUTE LE 25-07 DATE VALEUR : 25/07/2011                                                                                                        |
	| 2017-0023 | VIREMENT AU COMPTE 001-3502619-25        15-07          111,00-DE SYLVIE BACQUELAINE VIA PC/WEB BANKING COMMUNICATION: PRET JUILLET EXECUTE LE 15-07 DATE VALEUR : 15/07/2011                                                                                                                    |
	| 2017-0024 | VIREMENT AU COMPTE 035-4838807-03        10-07          111,00-DE BRAYE BLAISE VIA PC/WEB BANKING EXECUTE LE 09-07 DATE VALEUR : 10/07/2011                                                                                                                                                      |
    # VD
	| 2017-0031 | VIREMENT DU COMPTE 001-3502619-25        29-12          111,00+DE BACQUELAINE SYLVIE LE ROSAI 11 4052 BEAUFAYS SANS COMMUNICATION DATE VALEUR : 29/12/2011                                                                                                                                       |
	| 2017-0032 | VIREMENT DU COMPTE 260-0192990-29        21-12          111,06+DE AUTO-SATELLITES SA RUE GUSTAVE BOEL 23A 7100 LA LOUVIERE COMMUNICATION: /A/ PRIME DE FIN D'ANNEE         31-1 2-2011 A1-82 511417 000232 AUTO-SATEL LITES SA DATE VALEUR : 21/12/2011                                          |
	| 2017-0033 | VIREMENT DU COMPTE 063-4286344-46        23-06           11,00+DE SCHMITZ ALISON RUE DES PEUPLIERS       38 4623 MAGNEE COMMUNICATION: MERCI ET SORRY POUR LE RETARD     BIZ BIZ DATE VALEUR : 23/06/2011                                                                                        |
    # VM
	| 2017-0041 | VOTRE VIREMENT AVEC DATE-MEMO            05-05          111,00-AU PROFIT DU COMPTE 260-0621922-27 DE BRAYE ANDRE VIA PC/WEB BANKING COMMUNICATION: REMBOURSEMENT 800/900 DATE VALEUR : 05/05/2011                                                                                                |
	
	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty                          | City                | Address                         | PatternName   | Communication                                                                        | IBAN                        | BIC         | ThirdPartyOperationRef              |
    # VED
	| 2017-0001   | SELLIGENT S.A.                      | BRAINE-L'ALLEU      | AVENUE DE FINLANDE, 2 1420      | BankTransfert | /A/ 1CS0125-01-0000245                                                               | BE02 3600 9651 8440         | BBRUBEBB    | 1CS0125-01-0000245-1633010083573582 |
	| 2017-0002   | BACQUELAINE SYLVIE                  | WOLUWE-SAINT-PIERRE | RUE DU DUC 23 1150              | BankTransfert |                                                                                      | BE70 0013 5026 1925         | GEBABEBB    |                                     |
	| 2017-0003   | MUTUALITE LIBERALE LIEGE SOC MUTU   | HERSTAL             | RUE DE HERMEE 177 D 4040        | BankTransfert | 00019161010/2030970/011/014/014/10101                                                | BE46 3400 1288 1436         | BBRUBEBB    | 79                                  |
	| 2017-0004   | LEMON WAY                           |                     |                                 | BankTransfert |                                                                                      | FR7630004025110001118625268 | BNPAFRPP    | KADOLOG-1245                        |
	| 2017-0005   | BACQUELAINE DANIEL LE ROSAI 11 4052 | CHAUDFONTAINE       |                                 | BankTransfert | RENTE                                                                                | BE58 0011 5220 3079         | GEBABEBB    |                                     |
    # VEA
	| 2017-0010   | PROXIMUS MOBILE                     |                     |                                 | BankTransfert | 285293318329                                                                         | BE31435411161155            | KREDBEBB    |                                     |
	| 2017-0011   | ELECTRABEL                          |                     |                                 | BankTransfert | 441874258425                                                                         | BE46000325448336            | BPOTBEB1    |                                     |
	| 2017-0012   | CENTRE MEDICAL DU VAL SPRL          |                     |                                 | BankTransfert | KINE SHIRLEY - NILS BRAYE                                                            | BE10210039862204            | GEBABEBB    |                                     |
	| 2017-0013   | PAYPAL EUROPE                       |                     |                                 | BankTransfert | 91L74R25AM47NYT                                                                      | DE56120700883000443706      | DEUTDEDBPAL |                                     |
	| 2017-0014   | GEOFFRAY LESAGE                     |                     |                                 | BankTransfert | FOURTOU PARADIZE                                                                     | FR7614707001013081995404823 | CCBPFRPPMTZ |                                     |
	| 2017-0015   | VERONIQUE THIEBAUT                  |                     |                                 | BankTransfert |                                                                                      | BE20732694113156            | CREGBEBB    |                                     |
	| 2017-0016   | SYNDICAT INTERCOMMUNAL DE FLAINE    |                     |                                 | BankTransfert | REGLEMENT FACTURE 1130654                                                            | FR323000100302D741000000079 | BDFEFRPPCCT |                                     |
    # VA
	| 2017-0021   | INFORIM SPRL                        |                     |                                 | BankTransfert | LOYER DU AOUT 2011 - RUE DE LA LOI 16 A 4020 LIEGE - BRAYE                           | 001-4427276-78              |             |                                     |
	| 2017-0022   | SYLVIE BACQUELAINE                  |                     |                                 | BankTransfert | MC2 BOOMBOOM H DOMDOM XY                                                             | 001-3502619-25              |             |                                     |
	| 2017-0023   | SYLVIE BACQUELAINE                  |                     |                                 | BankTransfert | PRET JUILLET                                                                         | 001-3502619-25              |             |                                     |
	| 2017-0024   | BRAYE BLAISE                        |                     |                                 | BankTransfert |                                                                                      | 035-4838807-03              |             |                                     |
    # VD
	| 2017-0031   | BACQUELAINE SYLVIE                  | BEAUFAYS            | LE ROSAI 11 4052                | BankTransfert |                                                                                      | 001-3502619-25              |             |                                     |
	| 2017-0032   | AUTO-SATELLITES SA                  | LA LOUVIERE         | RUE GUSTAVE BOEL 23A 7100       | BankTransfert | /A/ PRIME DE FIN D'ANNEE         31-1 2-2011 A1-82 511417 000232 AUTO-SATEL LITES SA | 260-0192990-29              |             |                                     |
	| 2017-0033   | SCHMITZ ALISON                      | MAGNEE              | RUE DES PEUPLIERS       38 4623 | BankTransfert | MERCI ET SORRY POUR LE RETARD     BIZ BIZ                                            | 063-4286344-46              |             |                                     |
    # VM
	| 2017-0041   | BRAYE ANDRE                         |                     |                                 | BankTransfert | REMBOURSEMENT 800/900                                                                | 260-0621922-27              |             |                                     |

Scenario: parse 'CartPayment'
    Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                                                              |
	| 2017-0001 | PAIEMENT PAR CARTE DE BANQUE             27-11           11,95-AVEC LA CARTE 6703 04XX XXXX X315 7 C'KI LE ROI SPRL  WOLUWE SA 27/11/2016 EXECUTE LE 27-11 DATE VALEUR : 27/11/2016 |
	| 2017-0002 | PAIEMENT PAR CARTE DE BANQUE             26-11           11,05-AVEC LA CARTE 6703 04XX XXXX X315 7 COLRUYT ET ETTERBE 26/11/2016 DATE VALEUR : 26/11/2016                           |
	| 2017-0003 | PAIEMENT PAR CARTE DE BANQUE             28-11          111,27-AVEC LA CARTE 6703 04XX XXXX X315 7 3088 COLRUYT AUD  AUDERGHEM 28/11/2016 DATE VALEUR : 28/11/2016                  |
	| 2017-0004 | PAIEMENT PAR CARTE DE BANQUE             26-11           11,18-AVEC LA CARTE 6703 04XX XXXX X315 7 CARREFOUR MARKET  BRUXELLES 26/11/2016 DATE VALEUR : 26/11/2016                  |

	When I unify and transform the read operations

    Then the operations data is
	| OperationId | ThirdParty       | City                 | PatternName |
	| 2017-0001   | C'KI LE ROI SPRL | WOLUWE-SAINT-LAMBERT | CartPayment |
	| 2017-0002   | COLRUYT ET       | ETTERBEEK            | CartPayment |
	| 2017-0003   | 3088 COLRUYT AUD | AUDERGHEM            | CartPayment |
	| 2017-0004   | CARREFOUR MARKET | BRUXELLES            | CartPayment |

Scenario: parse 'Withdrawal'	
    Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                                                              |
	| 2017-0001 | RETRAIT D'ARGENT AUTRES DISTRIBUTEURS    04-11           11,00-AVEC LA CARTE 6703 04XX XXXX X315 7 BELFIUS37104704   BRAINE L' 04/11/2016 DATE VALEUR : 04/11/2016                  |
	| 2017-0002 | RETRAIT D'ARGENT AUTRES DISTRIBUTEURS    19-11           11,00-AVEC LA CARTE 6703 04XX XXXX X315 7 BELFIUS35792103   JOURDAN 19/11/2016 DATE VALEUR : 19/11/2016                    |
	| 2017-0003 | RETRAIT D'ARGENT AUTRES DISTRIBUTEURS    15-10           11,00-AVEC LA CARTE 6703 04XX XXXX X315 7 AXA SCHMITZ       FRANCORCH 15/10/2016 DATE VALEUR : 15/10/2016                  |
	| 2017-0004 | RETRAIT D'ARGENT AUTRES DISTRIBUTEURS    10-11          111,00-AVEC LA CARTE 6703 04XX XXXX X315 7 CHAUDFONTAINE     CHAUDFONT 10/11/2016 EXECUTE LE 10-11 DATE VALEUR : 10/11/2016 |
	| 2017-0005 | RETRAIT D'ARGENT A NOS DISTRIBUTEURS     14-10           11,00-AVEC LA CARTE 6703 04XX XXXX X315 7 BRAINE L ALLEUD   BRAINE AL 14/10/2016 DATE VALEUR : 14/10/2016                  |
	| 2017-0006 | RETRAIT D'ARGENT AUTRES DISTRIBUTEURS    20-02           11,00-AVEC LA CARTE 6703 04XX XXXX X315 7 CRELAN CASH-MORE  /BEAUFAYS 20/02/2016 DATE VALEUR : 20/02/2016                  |
	
	When I unify and transform the read operations

    Then the operations data is
	| OperationId | ThirdParty | City            | PatternName | Communication    |
	| 2017-0001   |            | BRAINE L'ALLEUD | Withdrawal  | BELFIUS37104704  |
	| 2017-0002   |            | ETTERBEEK       | Withdrawal  | BELFIUS35792103  |
	| 2017-0003   |            | FRANCORCHAMPS   | Withdrawal  | AXA SCHMITZ      |
	| 2017-0004   |            | CHAUDFONTAINE   | Withdrawal  | CHAUDFONTAINE    |
	| 2017-0005   |            | BRAINE L'ALLEUD | Withdrawal  | BRAINE L ALLEUD  |
	| 2017-0006   |            | BEAUFAYS        | Withdrawal  | CRELAN CASH-MORE |

Scenario: parse 'Domiciliation'
    Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                                                                                                                                               |
	| 2017-0000 | PREMIER PRELEVEMENT D'UNE                09-03           11,03-DOMICILIATION EUROPEENNE DE PAYPAL EUROPE S.A.R.L. ET CIE S.C.A NUMERO DE MANDAT : 43FJ224QUR62Q REFERENCE : YYU43FJ29FU3PEAG PAYPAL COMMUNICATION : YYU43FJ29FU3PEAG PAYPAL DATE VALEUR : 09/03/2016 |
	| 2017-0001 | DOMICILIATION EUROPEENNE DE              01-06           11,00-PAYPAL EUROPE S.A.R.L. ET CIE S.C.A NUMERO DE MANDAT : 43FJ224QUR62Q REFERENCE : YYI43FJ29MWZV2LS PAYPAL COMMUNICATION : YYI43FJ29MWZV2LS PAYPAL DATE VALEUR : 01/06/2016                             |
	| 2017-0002 | DOMICILIATION EUROPEENNE DE              03-02           11,00-OXFAM SOLIDARITE ASBL NUMERO DE MANDAT : UP003632 REFERENCE : 7242969-1-107-861-10639 COMMUNICATION : SDD/7242969/167421/636509 DATE VALEUR : 03/02/2016                                              |
	
	When I unify and transform the read operations

    Then the operations data is
	| OperationId | ThirdParty                          | PatternName   | Mandat        | ThirdPartyOperationRef  | Communication             |
	| 2017-0000   | PAYPAL EUROPE S.A.R.L. ET CIE S.C.A | Domiciliation | 43FJ224QUR62Q | YYU43FJ29FU3PEAG PAYPAL | YYU43FJ29FU3PEAG PAYPAL   |
	| 2017-0001   | PAYPAL EUROPE S.A.R.L. ET CIE S.C.A | Domiciliation | 43FJ224QUR62Q | YYI43FJ29MWZV2LS PAYPAL | YYI43FJ29MWZV2LS PAYPAL   |
	| 2017-0002   | OXFAM SOLIDARITE ASBL               | Domiciliation | UP003632      | 7242969-1-107-861-10639 | SDD/7242969/167421/636509 |

Scenario: parse 'CreditPayment'

    Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                             |
	| 2017-0001 | PAIEMENT A BANK CARD COMPANY             10-11           11,98-COMPTE INTERNE VISA : 17503879 ETAT DE DEPENSES NUMERO 306 DATE VALEUR : 10/11/2016 |
	
	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName   | Communication               |
	| 2017-0001   | 17503879   | CreditPayment | ETAT DE DEPENSES NUMERO 306 |

Scenario: parse 'BankFees'
    Given I have read the following fortis operations from archive files
	| Reference | Detail                                                                                                                                                           |
	| 2017-0001 | REDEVANCE MENSUELLE                      01-10            1,60-COMFORT PACK EXECUTE LE 06-10 DATE VALEUR : 01/10/2016                                            |
	| 2017-0002 | FRAIS MENSUELS D'EQUIPEMENT              01-12            1,60-PERIODE DU 01-12-2011 AU 31-12-2011 DETAILS VOIR ANNEXE DATE VALEUR : 01/12/2011                  |
	| 2017-0003 | FRAIS MENSUELS D'UTILISATION             01-12            1,40-PERIODE DU 01-11-2014 AU 30-11-2014 DETAILS VOIR ANNEXE EXECUTE LE 04-12 DATE VALEUR : 01/12/2014 |
	
	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName | Communication            |
	| 2017-0001   | Fortis     | BankFees    | MONTHLY FEE COMFORT PACK |
	| 2017-0002   | Fortis     | BankFees    | MONTHLY FEE COMFORT PACK |
	| 2017-0003   | Fortis     | BankFees    | MONTHLY FEE COMFORT PACK |
