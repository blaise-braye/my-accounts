﻿@UnitTest
Feature: Unify Fortis Operations Export details
	In order to classify my personal operations,
	I want a more structure operation detail for my fortis transaction exports
    
Background: all the test below have been run under the following conditions
    Given I am working with operations coming from a file having the following structure metadata
    | Key              | Value |
    | Culture          | be-fr |
    | DecimalSeparator | ,     |

Scenario: parse fortis amounts
	Given I have read the following fortis operations from export files
	| Reference | Amount |
	| VE1       | 0,01   |
	| VE2       | -0,01  |

	When I unify and transform the read operations

	Then the operations data is
	| OperationId | Income | Outcome |
	| VE1         | 0.01   | 0       |
	| VE2         | 0      | 0.01    |
    
Scenario: parse fortis amounts dot case
    Given I am working with operations coming from a file having the following structure metadata
    | Key              | Value |
    | Culture          | be-fr |
    | DecimalSeparator | .     |
	Given I have read the following fortis operations from export files
	| Reference | Amount |
    | VE3       | 0.01   |
	| VE4       | -0.01  |

	When I unify and transform the read operations

	Then the operations data is
	| OperationId | Income | Outcome |
    | VE3         | 0.01   | 0       |
	| VE4         | 0      | 0.01    |

Scenario: parse 'BankTransfert'
	Given I have read the following fortis operations from export files
	| Reference | Detail                                                                                                                                                                                                                                             |
	| 2017-0010 | PROXIMUS MOBILE BE31435411161155BIC KREDBEBB    VIA MOBILE BANKING COMMUNICATION : 285293318632 EXECUTE LE 05-02 DATE VALEUR : 05/02/2017                                                                                                          |
	| 2017-0020 | GEVELCO BE05 3701 2392 6075  BIC BBRUBEBBCOMMUNICATION: LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE DATE VALEUR : 01/02/2017                                                                                                           |
	| 2017-0030 | GEVELCO BE05370123926075BIC BBRUBEBB VOTRE REFERENCE : 171828956 COMMUNICATION : LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE DATE VALEUR : 03/04/2017                                                                                  |
	| 2017-0040 | PARTENAMUT BE40310083000663BIC BBRUBEBB    VIA WEB BANKING COMMUNICATION : 170010267366 DATE VALEUR : 23/01/2017                                                                                                                                   |
	| 2017-0050 | PARTENA - MUTUALITE LIBRE BE74 2100 0818 2307  BIC GEBABEBBREFERENCE DONNEUR D'ORDRE : 5090411H691 COMMUNICATION : /C/ PAIE /0201733339803 DU 10/04/2017 POUR 001 PRESTATIONS CHEZ M.BAYET BEN 0201733339803 711041707696 DATE VALEUR : 11/04/2017 |
	| 2017-0060 | SELLIGENT S.A. AVENUE DE FINLANDE, 21420        BRAINE-L'ALLEU BE02 3600 9651 8440  BIC BBRUBEBB REFERENCE DONNEUR D'ORDRE : 1CS0125-01-0000245-1701310102822878 COMMUNICATION : /A/ 1CS0125-01-0000245 DATE VALEUR : 13/01/2017                   |
	| 2017-0070 | BACQUELAINE SYLVIE RUE DU DUC 231150     WOLUWE-SAINT-PIERRE BE70 0013 5026 1925  BIC GEBABEBB PAS DE COMMUNICATION DATE VALEUR : 18/01/2017                                                                                                       |
	| 2017-0080 | COLINE BRAYE BE89750668924185BIC AXABBE22    VIA WEB BANKING COMMUNICATION : ANNIF PAPA DATE VALEUR : 05/12/2016                                                                                                                                   |
	| 2018-0296 | ZALANDO SE DE86210700200123010101BIC DEUTDEHH210 REFERENCE DONNEUR D'ORDRE : 010101175333 COMMUNICATION : 11005022560201 ZALANDO SE DATE VALEUR : 02/05/2018                                                                                       |
	| 2018-0178 | COINBASE UK, LTD. EE957700771001355096BIC LHVBEE22    VIA WEB BANKING COMMUNICATION : CBAEURGIPCEDOT DATE VALEUR : 08/03/2018                                                                                                                      |
	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty                | City                | Address                   | PatternName   | Communication                                                                                          | IBAN                   | BIC         | ThirdPartyOperationRef              |
	| 2017-0010   | PROXIMUS MOBILE           |                     |                           | BankTransfert | 285293318632                                                                                           | BE31435411161155       | KREDBEBB    |                                     |
	| 2017-0020   | GEVELCO                   |                     |                           | BankTransfert | LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE                                                | BE05 3701 2392 6075    | BBRUBEBB    |                                     |
	| 2017-0030   | GEVELCO                   |                     |                           | BankTransfert | LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE                                                | BE05370123926075       | BBRUBEBB    | 171828956                           |
	| 2017-0040   | PARTENAMUT                |                     |                           | BankTransfert | 170010267366                                                                                           | BE40310083000663       | BBRUBEBB    |                                     |
	| 2017-0050   | PARTENA - MUTUALITE LIBRE |                     |                           | BankTransfert | /C/ PAIE /0201733339803 DU 10/04/2017 POUR 001 PRESTATIONS CHEZ M.BAYET BEN 0201733339803 711041707696 | BE74 2100 0818 2307    | GEBABEBB    | 5090411H691                         |
	| 2017-0060   | SELLIGENT S.A.            | BRAINE-L'ALLEU      | AVENUE DE FINLANDE, 21420 | BankTransfert | /A/ 1CS0125-01-0000245                                                                                 | BE02 3600 9651 8440    | BBRUBEBB    | 1CS0125-01-0000245-1701310102822878 |
	| 2017-0070   | BACQUELAINE SYLVIE        | WOLUWE-SAINT-PIERRE | RUE DU DUC 231150         | BankTransfert |                                                                                                        | BE70 0013 5026 1925    | GEBABEBB    |                                     |
	| 2017-0080   | COLINE BRAYE              |                     |                           | BankTransfert | ANNIF PAPA                                                                                             | BE89750668924185       | AXABBE22    |                                     |
	| 2018-0296   | ZALANDO SE                |                     |                           | BankTransfert | 11005022560201 ZALANDO SE                                                                              | DE86210700200123010101 | DEUTDEHH210 | 010101175333                        |
	| 2018-0178   | COINBASE UK, LTD.         |                     |                           | BankTransfert | CBAEURGIPCEDOT                                                                                         | EE957700771001355096   | LHVBEE22    |                                     |

Scenario: parse 'CartPayment'
    Given I have read the following fortis operations from export files
	| Reference | Detail                                                                                                              |
	| 2017-0001 | AVEC LA CARTE 6703 04XX XXXX X315 7 CARREFOUR EXPR   1000 BRUS.07-02-2017 EXECUTE LE 07-02 DATE VALEUR : 07/02/2017 |
	| 2017-0002 | AVEC LA CARTE 6703 04XX XXXX X600 0 CARREFOUR EXPRES  BRUXELLES31/08/2017 DATE VALEUR : 31/08/2017                  |
	| 2017-0010 | AVEC LA CARTE 6703 04XX XXXX X600 0 BEAUFAYS          BEAUFAYS19/08/2017 EXECUTE LE 19-08 DATE VALEUR : 19/08/2017  |
	| 2017-0020 | AVEC LA CARTE 6703 04XX XXXX X725 6 SNCB BXL          P2M MOBIL14-04-2017 EXECUTE LE 14-04 DATE VALEUR : 14/04/2017 |
	| 2017-0021 | AVEC LA CARTE 6703 04XX XXXX X725 6 INGENICO          P2M MOBIL14-08-2017 DATE VALEUR : 14/08/2017                  |
	| 2017-0022 | AVEC LA CARTE 6703 04XX XXXX X725 6 STEPH LAMPION      P2P MOBIL28-06-2017 DATE VALEUR : 28/06/2017                 |
	When I unify and transform the read operations

    Then the operations data is
	| OperationId | ThirdParty       | City      | PatternName    |
	| 2017-0001   | CARREFOUR EXPR   | BRUXELLES | CartPayment    |
	| 2017-0002   | CARREFOUR EXPRES | BRUXELLES | CartPayment    |
	| 2017-0010   | BEAUFAYS         | BEAUFAYS  | CartPayment    |
	| 2017-0020   | SNCB             | BRUXELLES | CartPaymentP2M |
	| 2017-0021   | INGENICO         |           | CartPaymentP2M |
	| 2017-0022   | STEPH LAMPION    |           | CartPaymentP2P |


Scenario: parse 'Domiciliation'
    Given I have read the following fortis operations from export files
	| Reference | Detail                                                                                                                                                             |
	| 2017-0001 | PAYPAL EUROPE S.A.R.L. ET CIE S.C.A NUMERO DE MANDAT :43FJ224QUR62Q REFERENCE : 1000265758596 PAYPAL COMMUNICATION : 1000265758596 PAYPAL DATE VALEUR : 02/12/2016 |
	
	When I unify and transform the read operations

    Then the operations data is
	| OperationId | ThirdParty                          | PatternName   | Mandat        | ThirdPartyOperationRef | Communication        |
	| 2017-0001   | PAYPAL EUROPE S.A.R.L. ET CIE S.C.A | Domiciliation | 43FJ224QUR62Q | 1000265758596 PAYPAL   | 1000265758596 PAYPAL |


Scenario: parse 'CreditPayment'

    Given I have read the following fortis operations from export files
	| Reference | Detail                                                                             |
	| 2017-0001 | COMPTE INTERNE VISA : 17503879 ETAT DE DEPENSES NUMERO 336DATE VALEUR : 12/12/2016 |
	
	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName   | Communication               |
	| 2017-0001   | 17503879   | CreditPayment | ETAT DE DEPENSES NUMERO 336 |


Scenario: parse 'BankFees'
    Given I have read the following fortis operations from export files
	| Reference | Detail                                                                                           | CounterpartyOfTheTransaction  |
	| 2017-0001 | COMFORT PACK EXECUTE LE 06-02DATE VALEUR : 01/02/2017                                            | ANY                           |
	| 2017-0002 | PERIODE DU 01-02-2017 AU 28-02-2017DETAILS VOIR ANNEXE EXECUTE LE 07-03 DATE VALEUR : 01/03/2017 | ANY                           |
	| 2017-0003 | EXECUTE LE 07-03 DATE VALEUR : 01/03/2017                                                        | RECTIFICATION VERSEMENT BONUS |
	| 2017-0004 | VOTRE FIDELITE EST RECOMPENSEE VOIR DETAILS EN ANNEXEEXECUTE LE 31-01 DATE VALEUR : 01/02/2018   | VERSEMENT DE VOTRE BONUS 2017 |
	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName | Communication                       |
	| 2017-0001   | Fortis     | BankFees    | MONTHLY FEE COMFORT PACK            |
	| 2017-0002   | Fortis     | BankFees    | PERIODE DU 01-02-2017 AU 28-02-2017 |
	| 2017-0003   | Fortis     | BankFees    | RECTIFICATION VERSEMENT BONUS       |
	| 2017-0004   | Fortis     | BankFees    | VOTRE FIDELITE EST RECOMPENSEE      |
