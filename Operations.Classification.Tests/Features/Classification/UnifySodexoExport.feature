Feature: Unify Sodexo Export details
	In order to classify my personal operations,
	I want a more structured operation detail for my sodexo transaction exports

Scenario: parse 'BankTransfert'
	Given I have read the following sodexo operations
	"""
Date;Affilié;Amount
02-03-2017;"Versement de 19 eLunch Pass d'une valeur de 1.1 € de SELLIGENT";"+ 1.1 €"
02-02-2017;"Versement de 21 eLunch Pass d'une valeur de 1.1 € de SELLIGENT";"+ 1.1 €"
	"""

	When I unify and transform the read operations

	Then the operations data is
	| OperationId | ThirdParty | PatternName   | Communication                                     |
	| 20170302-IN | SELLIGENT  | BankTransfert | Versement de 19 eLunch Pass d'une valeur de 1.1 € |
	| 20170202-IN | SELLIGENT  | BankTransfert | Versement de 21 eLunch Pass d'une valeur de 1.1 € |

Scenario: parse 'CartPayment'
Given I have read the following sodexo operations
	"""
Date;Affilié;Amount
24-02-2017;"Dépense CARREFOUR EXPRESS ETTERBEEK BRUXELLES (Transaction 128260098)";"- 11.11 €"
23-02-2017;"Dépense DELHAIZE LE LION / DE LEEUW LIEGE (Transaction 128074107)";"- 11.11 €"
	"""
	
	When I unify and transform the read operations

    Then the operations data is
	| OperationId        | ThirdParty                  | City      | PatternName |
	| 20170224-128260098 | CARREFOUR EXPRESS ETTERBEEK | BRUXELLES | CartPayment |
	| 20170223-128074107 | DELHAIZE LE LION / DE LEEUW | LIEGE     | CartPayment |
