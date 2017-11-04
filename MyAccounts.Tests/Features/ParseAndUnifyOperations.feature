@IntegrationTest
@UserPcHelperTest
Feature: ParseTransactionFiles
	In order to classify my personal operations,
	I want a more structure operation detail for any transactions coming for any csv file kind

Scenario Outline: merge fortis operations and parse details
	
	When I parse the details of the files '<Path>\'
	And I Filter the details where number is higher than '<LatestSynchronised>'
	And I store the operation details in file '<Path>.csv'
	And I store the operation details in qif file '<Path>.qif'
	
	Then File '<Path>.csv' exists
	And File '<Path>.qif' exists
	And pattern detection accuracy is higher that <MinAccuracy> %

	Examples: 
	| Path                                                                      | LatestSynchronised | MinAccuracy |
	| C:\Users\BBraye\OneDrive\Gestion\Data2\Blaise - Sodexo\operations         |                    | 100         |
	| C:\Users\BBraye\OneDrive\Gestion\Data2\Blaise - Compte courant\operations |                    | 99.59       |
	| C:\Users\BBraye\OneDrive\Gestion\Data2\Sylvie - Compte courant\operations |                    | 99.48       |