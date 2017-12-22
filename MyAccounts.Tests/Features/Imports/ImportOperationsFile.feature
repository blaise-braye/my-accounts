@UnitTest

Feature: Import operations file
    
Scenario: import two times the same datasource and fetch imported operations
    Given I have an empty operations repository
    
    Given I am working with operations coming from a file having the following structure metadata
    | Key              | Value           |
    | SourceKind       | FortisCsvExport |
    | Encoding         | windows-1252    |
    | DecimalSeparator | ,               |

	Given I have an operations file with the following 'windows-1252' content
	"""
	Numéro de séquence;Date d'exécution;Date valeur;Montant;Devise du compte;Détails;Numéro de compte
    2017-0050;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0051;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0052;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0053;05/02/2017;05/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0060;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0060;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0061;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0062;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
    2017-0063;06/02/2017;06/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
	"""
    
	When I import the operations file with the current structure metadata

    And I import the operations file with the current structure metadata
    
    Then the imported operation data is
    | OperationId | ExecutionDate |
    | 2017-0050   | 2017-02-05    |
    | 2017-0051   | 2017-02-05    |
    | 2017-0052   | 2017-02-05    |
    | 2017-0053   | 2017-02-05    |
    | 2017-0060   | 2017-02-06    |
    | 2017-0061   | 2017-02-06    |
    | 2017-0062   | 2017-02-06    |
    | 2017-0063   | 2017-02-06    |
	

Scenario: custom encoding is taken into account during import
        Because each import can be done with a potential mistake in the serialization
	    I want to be able to replay the entire history with an eventual serialization setting fix

    Given I am working with operations coming from a file having the following structure metadata
    | Key              | Value           |
    | SourceKind       | FortisCsvExport |
    | Encoding         | UTF-8           |
    | DecimalSeparator | ,               |

	Given I have an operations file with the following 'windows-1252' content
	"""
	Numéro de séquence;Date d'exécution;Date valeur;Montant;Devise du compte;Détails;Numéro de compte
	2017-0049;04/02/2017;04/02/2017;-1,11;EUR;AVEC LA CARTE 6703 04XX XXXX X315 7 BOUL - PAT PRIMO  BRUXELLES04-02-2017 DATE VALEUR : 04/02/2017;BE02275045085140
	"""

	And I have an empty operations repository
	
	When I import the operations file with the current structure metadata
    
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
