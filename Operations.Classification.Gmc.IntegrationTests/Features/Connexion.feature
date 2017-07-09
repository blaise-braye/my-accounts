@IntegrationTest
Feature: Connexion
	User must be able to connect to the website in order to work with all the available features

Scenario:Connect
	Given I am disconnected
	When I connect on GererMesComptes with email 'Settings:GmcUserName' and password 'Settings:GmcPassword'
	Then I am connected
