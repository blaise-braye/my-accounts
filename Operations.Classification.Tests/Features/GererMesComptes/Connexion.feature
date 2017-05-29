Feature: Connexion
	User must be able to connect to the website in order to work with all the available features

Scenario:Connect
	Given I am disconnected
	When I connect on GererMesComptes with email 'blaisemail@gmail.com' and password 'kT5XeI!I9AI9'
	Then I am connected
