﻿Feature: Unify Fortis Operations Export details
	In order to classify my personal operations,
	I want a more structure operation detail for my fortis transaction exports

Scenario: parse 'BankTransfert'
	Given I have read the following account operations from source of kind FortisCsvExport
	| OperationId | Note                                                                                                                                                                                                                             |
	| VE1         | PROXIMUS MOBILE BE31435411161155BIC KREDBEBB    VIA MOBILE BANKING COMMUNICATION : 285293318632 EXECUTE LE 05-02 DATE VALEUR : 05/02/2017                                                                                        |
	| VE2         | GEVELCO BE05 3701 2392 6075  BIC BBRUBEBBCOMMUNICATION: LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE DATE VALEUR : 01/02/2017                                                                                         |
	| VE3         | PARTENAMUT BE40310083000663BIC BBRUBEBB    VIA WEB BANKING COMMUNICATION : 170010267366 DATE VALEUR : 23/01/2017                                                                                                                 |
	| VE4         | SELLIGENT S.A. AVENUE DE FINLANDE, 21420        BRAINE-L'ALLEU BE02 3600 9651 8440  BIC BBRUBEBB REFERENCE DONNEUR D'ORDRE : 1CS0125-01-0000245-1701310102822878 COMMUNICATION : /A/ 1CS0125-01-0000245 DATE VALEUR : 13/01/2017 |
	| VE5         | BACQUELAINE SYLVIE RUE DU DUC 231150     WOLUWE-SAINT-PIERRE BE70 0013 5026 1925  BIC GEBABEBB PAS DE COMMUNICATION DATE VALEUR : 18/01/2017                                                                                     |
	| VE6         | COLINE BRAYE BE89750668924185BIC AXABBE22    VIA WEB BANKING COMMUNICATION : ANNIF PAPA DATE VALEUR : 05/12/2016                                                                                                                 |
	
	When I apply the cleanup transformation on unified operations

	Then the operations data is
	| OperationId | ThirdParty         | City                | Address                   | PatternName   | Communication                                           | IBAN                | BIC      | ThirdPartyOperationRef              |
	| VE1         | PROXIMUS MOBILE    |                     |                           | BankTransfert |                                                         | BE31435411161155    | KREDBEBB |                                     |
	| VE2         | GEVELCO            |                     |                           | BankTransfert | LOYER RUE DE PASCALE 15 - ETAGE 1  BR AYE - BACQUELAINE | BE05 3701 2392 6075 | BBRUBEBB |                                     |
	| VE3         | PARTENAMUT         |                     |                           | BankTransfert |                                                         | BE40310083000663    | BBRUBEBB |                                     |
	| VE4         | SELLIGENT S.A.     | BRAINE-L'ALLEU      | AVENUE DE FINLANDE, 21420 | BankTransfert | : /A/ 1CS0125-01-0000245                                | BE02 3600 9651 8440 | BBRUBEBB | 1CS0125-01-0000245-1701310102822878 |
	| VE5         | BACQUELAINE SYLVIE | WOLUWE-SAINT-PIERRE | RUE DU DUC 231150         | BankTransfert |                                                         | BE70 0013 5026 1925 | GEBABEBB |                                     |
	| VE6         | COLINE BRAYE       |                     |                           | BankTransfert |                                                         | BE89750668924185    | AXABBE22 |                                     |

Scenario: parse 'CartPayment'
    Given I have read the following account operations from source of kind FortisCsvExport
	| OperationId | Note                                                                                                                |
	| Ref1        | AVEC LA CARTE 6703 04XX XXXX X315 7 CARREFOUR EXPR   1000 BRUS.07-02-2017 EXECUTE LE 07-02 DATE VALEUR : 07/02/2017 |

	When I apply the cleanup transformation on unified operations

    Then the operations data is
	| OperationId | ThirdParty            | City  | PatternName |
	| Ref1        | CARREFOUR EXPR   1000 | BRUS. | CartPayment |


Scenario: parse 'Domiciliation'
    Given I have read the following account operations from source of kind FortisCsvExport
	| OperationId | Note                                                                                                                                                               |
	| Ref1        | PAYPAL EUROPE S.A.R.L. ET CIE S.C.A NUMERO DE MANDAT :43FJ224QUR62Q REFERENCE : 1000265758596 PAYPAL COMMUNICATION : 1000265758596 PAYPAL DATE VALEUR : 02/12/2016 |
	
	When I apply the cleanup transformation on unified operations

    Then the operations data is
	| OperationId | ThirdParty                          | PatternName   | Mandat        | ThirdPartyOperationRef | Communication        |
	| Ref1        | PAYPAL EUROPE S.A.R.L. ET CIE S.C.A | Domiciliation | 43FJ224QUR62Q | 1000265758596 PAYPAL   | 1000265758596 PAYPAL |


Scenario: parse 'CreditPayment'

    Given I have read the following account operations from source of kind FortisCsvExport
	| OperationId | Note                                                                               |
	| Ref1        | COMPTE INTERNE VISA : 17503879 ETAT DE DEPENSES NUMERO 336DATE VALEUR : 12/12/2016 |
	
	When I apply the cleanup transformation on unified operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName   | Communication               |
	| Ref1        | 17503879   | CreditPayment | ETAT DE DEPENSES NUMERO 336 |


Scenario: parse 'BankFees'
    Given I have read the following account operations from source of kind FortisCsvExport
	| OperationId | Note                                                  |
	| Ref1        | COMFORT PACK EXECUTE LE 06-02DATE VALEUR : 01/02/2017 |
	
	When I apply the cleanup transformation on unified operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName | Communication            |
	| Ref1        | Fortis     | BankFees    | MONTHLY FEE COMFORT PACK |
