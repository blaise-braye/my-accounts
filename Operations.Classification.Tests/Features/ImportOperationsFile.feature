@UnitTest

Feature: Import operations file
	Because each import can be done with a potential mistake in the serialization
	I want to be able to replay the entire history with an eventual serialization setting fix

Scenario: custom encoding is taken into account during import
	Given I have an operations file with the following 'windows-1252' content
	"""
	Numéro de séquence;Date d'exécution;Date valeur;Montant;Devise du compte;Détails;Numéro de compte
	2017-0049;04/02/2017;04/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
	"""

	And I have an empty operations repository
	
	When I import the operations file with following parameters
    | Key        | Value           |
    | SourceKind | FortisCsvExport |
    | Encoding   | UTF-8           |
    
	Then the imported operation data is
    # no operation has been imported because operation id could no be read from the file, because of the accents (e.g. 'Numéro de séquence')
    | OperationId | ExecutionDate |

	When I change the last import command such that
    | Key      | Value        |
    | Encoding | windows-1252 |

	And I replay the entire reflog of operations

	Then the imported operation data is
    | OperationId | ExecutionDate |
    | 2017-0049   | 2017-02-04    |
