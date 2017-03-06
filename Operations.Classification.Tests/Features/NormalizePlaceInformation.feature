Feature: NormalizePlaceInformation

#
#Scenario: find nearest zip code knowing city is at end of text
#	When I geocode those entries, knowing the city information is at the end of the text
#	| freetext                                                                  |
#	| SELLIGENT S.A. AVENUE DE FINLANDE, 2 1420        BRAINE-L'ALLEU           |
#	| BACQUELAINE SYLVIE RUE DU DUC 23 1150     WOLUWE-SAINT-PIERRE             |
#	| MUTUALITE LIBERALE LIEGE SOC MUTU RUE DE HERMEE 177 D 4040        HERSTAL |
#	| PROXIMUS MOBILE                                                           |
#	| BELFIUS37104704   BRAINE L'                                               |
#	| BELFIUS35792103   JOURDAN                                                 |
#	| AXA SCHMITZ       FRANCORCH                                               |
#	| CHAUDFONTAINE     CHAUDFONT                                               |
#	Then the geocoding results are
#	| freetext                                                                  | PostalCode | Locality            | SubLocality   |
#	| SELLIGENT S.A. AVENUE DE FINLANDE, 2 1420        BRAINE-L'ALLEU           | 1420       | Braine-l'Alleud     |               |
#	| BACQUELAINE SYLVIE RUE DU DUC 23 1150     WOLUWE-SAINT-PIERRE             | 1150       | Woluwe-Saint-Pierre |               |
#	| MUTUALITE LIBERALE LIEGE SOC MUTU RUE DE HERMEE 177 D 4040        HERSTAL | 4040       | Herstal             |               |
#	| PROXIMUS MOBILE                                                           | 9100       | Sint-Niklaas        |               |
#	| BELFIUS37104704   BRAINE L'                                               |            | Braine-l'Alleud     |               |
#	| BELFIUS35792103   JOURDAN                                                 |            |                     |               |
#	| AXA SCHMITZ       FRANCORCH                                               | 4970       | Stavelot            | Francorchamps |
#	| CHAUDFONTAINE     CHAUDFONT                                               |            | Chaudfontaine       |               |