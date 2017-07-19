@IntegrationTest
Feature: ManageBankAccounts
	User must be able to manage its bank accounts in order to manage its bank operations

Background: the user is connected
	Given I connect on GererMesComptes with email 'Settings:GmcUserName' and password 'Settings:GmcPassword'

Scenario:Create an account
	When I create the bank account 'ScenarioContext:ScenarioInfo.Title'
	Then the bank account 'ScenarioContext:ScenarioInfo.Title' exists
	And the account information of the bank account 'ScenarioContext:ScenarioInfo.Title' are
	| Key      | Value             |
	| name     | Create an account |
	| currency | EUR               |

Scenario:delete an account
	Given I create the bank account 'ScenarioContext:ScenarioInfo.Title'
	When I delete the bank account 'ScenarioContext:ScenarioInfo.Title'
	Then the bank account 'ScenarioContext:ScenarioInfo.Title' does not exist


