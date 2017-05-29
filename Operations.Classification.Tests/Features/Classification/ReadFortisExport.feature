Feature: ReadFortisExport
	Trust in the way I parse the csv files is required,
	Those files are the gold source of the entire claissification tool,
	The trust in the way they are read is key in the success of this classification

Scenario: Read a fortis export csv
	When I read the following fortis operations from an export csv file
"""
Numéro de séquence;Date d'exécution;Date valeur;Montant;Devise du compte;CONTREPARTIE DE LA TRANSACTION;Détails;Numéro de compte;SourceKind
2017-0207;15-05-2017;15-05-2017;11,11;EUR;BE02360096518440;SELLIGENT S.A. AVENUE DE FINLANDE, 21420        BRAINE-L'ALLEU BE02 3600 9651 8440  BIC BBRUBEBB REFERENCE DONNEUR D'ORDRE : 00100110492016462016461 COMMUNICATION : JEEN 108 E0379 04/2017 DATE VALEUR : 15/05/2017;BE02275045085140;FortisCsvExport
"""
	Then the read fortis operations are
	| Reference | ExecutionDate       | ValueDate           | Amount | Currency | CounterpartyOfTheTransaction | Detail                                                                                                                                                                                                               | Account          |
	| 2017-0207 | 2017-05-15T00:00:00 | 2017-05-15T00:00:00 | 11,11  | EUR      | BE02360096518440             | SELLIGENT S.A. AVENUE DE FINLANDE, 21420        BRAINE-L'ALLEU BE02 3600 9651 8440  BIC BBRUBEBB REFERENCE DONNEUR D'ORDRE : 00100110492016462016461 COMMUNICATION : JEEN 108 E0379 04/2017 DATE VALEUR : 15/05/2017 | BE02275045085140 |