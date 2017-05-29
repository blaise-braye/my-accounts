Feature: ImportBankOperations
	User must be able to import bank operations manually

Background: Ensure a bank account is created
	Given I connect on GererMesComptes with email 'blaisemail@gmail.com' and password 'kT5XeI!I9AI9'
	And I create the bank account 'Automated Test Account'

Scenario: Import an operation
	When I import the qif data on account 'Automated Test Account'
		"""
!Type:Bank
D11/04/2016
T0.01
MPARTENA - MUTUALITE LIBRE - BE74 2100 0818 2307 - BIC GEBABEBB - COMMUNICATION /C/ PAIE /0201733339803 DU 10/04/2017 POUR 001 PRESTATIONS CHEZ M.BAYET BEN 0201733339803 711041707696 - BankTransfert
N2017-0148
^
		"""
		Then the last qif data import succeeded

Scenario: Export imported operations
	Given I import the qif data on account 'Automated Test Account'
		"""
!Type:Bank
D11/04/2016
T0.01
MData Too Long Is Truncated To 129 Bytes You Should Not Be Able To Read Something After triple letter ooooooooooooooooooooooo SSS while there is something as you can read it :)
N2017-0148
^
D09/28/2016
T0.02
Mdates must be formatted in mm/dd/yyyy
N2017-0148
^
		"""

	When I export the qif data from account 'Automated Test Account', between '2016-04-11T00:00:00' and '2016-11-05T00:00:00'

	Then the last exported qif data are the following operations
	| Number | Date                | Amount | Memo                                                                                                                             |
	|        | 2016-11-04T00:00:00 | 0.01   | Data Too Long Is Truncated To 129 Bytes You Should Not Be Able To Read Something After Triple Letter Ooooooooooooooooooooooo Sss |
	|        | 2016-09-28T00:00:00 | 0.02   | Dates Must Be Formatted In Mm Dd Yyyy                                                                                            |

Scenario:Execute two successive Imports
	- latest version overwrites initial version
	- new operations are added
	Given I import the qif data on account 'Automated Test Account'
	"""
!Type:Bank
D09/28/2016
T0.01
MSome Memo
N2017-0148
^
	"""
	And I wait that last imported qifdata in account 'Automated Test Account' is available in export

	When I import the qif data on account 'Automated Test Account'
	"""
!Type:Bank
D09/28/2016
T0.01
MSome Updated Memo
N2017-0148
^
!Type:Bank
D04/01/2016
T0.02
MSome Added Memo
N2017-0149
^
	"""

	And I wait that last imported qifdata in account 'Automated Test Account' is available in export
	
	Then the last qif data import succeeded
	And the last exported qif data are the following operations
	| Number | Date                | Amount | Memo              |
	# latest version overwrites initial version
	|        | 2016-09-28T00:00:00 | 0.01   | Some Updated Memo |
	# extra item is imported
	|        | 2016-04-01T00:00:00 | 0.02   | Some Added Memo   |
	
Scenario: Identify delta between imported qif data and new available qif data
	Given I import the qif data on account 'Automated Test Account'
	"""
!Type:Bank
D09/27/2013
T0.01
MUnchanged
^
!Type:Bank
D09/28/2013
T0.01
MRemove
^
!Type:Bank
D09/29/2013
T0.02
MSome Memo
^
	"""

	Given I have an update of the qif data file to import
	"""
!Type:Bank
D09/27/2013
T0.01
MUnchanged
^
!Type:Bank
D09/29/2013
T0.02
MUpdated Memo
^
!Type:Bank
D01/11/2014
T0.01
MAdded Memo
^
	"""

	And I wait that last imported qifdata in account 'Automated Test Account' is available in export

	Then dry run import available qif data to account 'Automated Test Account' produces the following delta report
	| DeltaKey        | Action     |
	| 2013-09-27$0.01 | Nothing    |
	| 2013-09-29$0.02 | UpdateMemo |
	| 2014-01-11$0.01 | Add        |
	| 2013-09-28$0.01 | Remove     |
