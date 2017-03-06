Feature: Unify Sodexo Export details
	In order to classify my personal operations,
	I want a more structure operation detail for my sodexo transaction exports

Scenario: parse 'BankTransfert'
	Given I have read the following account operations from source of kind SodexoCsvExport
	| OperationId | Note                                                           |
	| 20170302-IN | Versement de 19 eLunch Pass d'une valeur de 1.5 € de SELLIGENT |
	| 20170302-IN | Versement de 21 eLunch Pass d'une valeur de 1.5 € de SELLIGENT |

	When I apply the cleanup transformation on unified operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName   | Communication                                     |
	| 20170302-IN | SELLIGENT  | BankTransfert | Versement de 19 eLunch Pass d'une valeur de 1.5 € |
	| 20170302-IN | SELLIGENT  | BankTransfert | Versement de 21 eLunch Pass d'une valeur de 1.5 € |

Scenario: parse 'CartPayment'
    Given I have read the following account operations from source of kind SodexoCsvExport
	| OperationId        | Note                                                                  |
	| 20170224-128260098 | Dépense CARREFOUR EXPRESS ETTERBEEK BRUXELLES (Transaction 128260098) |
	| 20170620-6038824   | Dépense DELHAIZE LE LION / DE LEEUW LIEGE (Transaction 6038824)       |

	When I apply the cleanup transformation on unified operations

    Then the operations data is
	| OperationId        | ThirdParty                  | City      | PatternName |
	| 20170224-128260098 | CARREFOUR EXPRESS ETTERBEEK | BRUXELLES | CartPayment |
	| 20170620-6038824   | DELHAIZE LE LION / DE LEEUW | LIEGE     | CartPayment |
