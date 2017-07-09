@IntegrationTest
Feature: ManageBankAccounts
	User must be able to manage its bank accounts in order to manage its bank operations

Background: the user is connected
	Given I connect on GererMesComptes with email 'Settings:GmcUserName' and password 'Settings:GmcPassword'

Scenario:Create an account
	When I create the bank account 'Automated Test Account'
	Then the bank account 'Automated Test Account' exists
	And the account information of the bank account 'Automated Test Account' are
	| Key               | Value                  |
	| name              | Automated Test Account |
	| currency          | EUR                    |

Scenario:delete an account
	Given I create the bank account 'Automated Test Account'
	When I delete the bank account 'Automated Test Account'
	Then the bank account 'Automated Test Account' does not exist


