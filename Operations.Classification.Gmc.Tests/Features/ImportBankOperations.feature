@IntegrationTest
Feature: ImportBankOperations
	User must be able to import bank operations manually

Background: Ensure a bank account is created
	Given I connect on GererMesComptes with email 'Settings:GmcUserName' and password 'Settings:GmcPassword'
	And I create the bank account 'ScenarioContext:ScenarioInfo.Title'

Scenario: Import an operation
	When I import the qif data on account 'ScenarioContext:ScenarioInfo.Title'
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
	Given I import the qif data on account 'ScenarioContext:ScenarioInfo.Title'
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

	When I export the qif data from account 'ScenarioContext:ScenarioInfo.Title', between '2016-04-11T00:00:00' and '2016-11-05T00:00:00'

	Then the last exported qif data are the following operations
	| Number | Date                | Amount | Memo                                                                                                                             |
	|        | 2016-11-04T00:00:00 | 0.01   | Data Too Long Is Truncated To 129 Bytes You Should Not Be Able To Read Something After Triple Letter Ooooooooooooooooooooooo Sss |
	|        | 2016-09-28T00:00:00 | 0.02   | Dates Must Be Formatted In Mm Dd Yyyy                                                                                            |

Scenario:Execute two successive Imports
	- latest version does not overwrite initial version
	- new operations are added
	Given I import the qif data on account 'ScenarioContext:ScenarioInfo.Title'
	"""
!Type:Bank
D09/28/2016
T0.01
MA First Operation
N2017-0148
^
D09/28/2016
T0.02
MA Second Operation
N2017-0149
^
	"""
	And I wait that last imported qifdata in account 'ScenarioContext:ScenarioInfo.Title' is available in export
	
	When I import the qif data on account 'ScenarioContext:ScenarioInfo.Title'
	"""
!Type:Bank
D09/28/2016
T0.01
MA First Operation
N2017-0148
^
D09/28/2016
T0.02
MA Second Operation
N2017-0149
^
D04/01/2016
T0.02
MA Third Operation
N2017-0150
^
	"""

	And I wait that last imported qifdata in account 'ScenarioContext:ScenarioInfo.Title' is available in export
	
	Then the last qif data import succeeded
	And the last exported qif data are the following operations
	| Number | Date                | Amount | Memo               |
	|        | 2016-09-28T00:00:00 | 0.01   | A First Operation  |
	|        | 2016-09-28T00:00:00 | 0.02   | A Second Operation |
	# extra item is imported
	|        | 2016-04-01T00:00:00 | 0.02   | A Third Operation  |
	
Scenario: Identify remote with new available qif data
	Given I import the qif data on account 'ScenarioContext:ScenarioInfo.Title'
	"""
!Type:Bank
D09/27/2013
T1.00
MUnchanged
^
D09/28/2013
T0.01
MRemove
^
D09/29/2013
T0.02
MSome Memo
^
	"""
	
	And I wait that last imported qifdata in account 'ScenarioContext:ScenarioInfo.Title' is available in export

	When I apply a dry run for the available qif data to account 'ScenarioContext:ScenarioInfo.Title'
	"""
!Type:Bank
D09/27/2013
T1.00
MUnchanged
^
D09/29/2013
T0.02
MUpdated Memo
^
D01/11/2014
T0.01
MAdded Memo
^
	"""

	Then the last dry run result produces the following delta report
	| DeltaKey        | Action     |
	| 2013-09-27$1.00 | Nothing    |
	| 2013-09-29$0.02 | UpdateMemo |
	| 2014-01-11$0.01 | Add        |
	| 2013-09-28$0.01 | Remove     |
