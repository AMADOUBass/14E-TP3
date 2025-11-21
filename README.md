# Locomotiv
Fichiers de départ pour le TP2
Amadou & Philippe
Résumé pour les tests – Système de planification et simulation ferroviaire
1. Structure du réseau

Le réseau ferroviaire est basé sur un ensemble de stations, de points d’arrêt et de blocks représentant les segments de rails.
Chaque block possède :

une position de départ / arrivée

un état (occupé / libre)

un signal (vert, jaune, rouge)

une polyline géométrique (vraie forme du rail ou courbé automatiquement)

Les stations possèdent une capacité maximale de trains et sont assignées à des employés.

2. Logique de planification des itinéraires

Un itinéraire est créé par l’administrateur via une fenêtre dédiée permettant :

choix d’un train (uniquement si non en transit)

sélection d’au moins 3 points d’arrêt (départ, arrêt intermédiaire, arrivée)

organisation de l’ordre des étapes

validation des règles de sécurité

Règles appliquées :

impossibilité de choisir deux fois le même arrêt

départ ≠ arrivée

au moins un arrêt intermédiaire

le train ne doit pas être déjà en transit

les blocks associés doivent respecter une distance minimale de sécurité

3. Association automatique des blocks

Pour chaque segment entre les étapes consécutives, le système :

prend l’arrêt i et l’arrêt i+1

calcule la distance vers tous les blocks

sélectionne le block le plus proche

Cela signifie que le train suit exactement les étapes choisies, et ne saute aucune station si l’utilisateur choisit les étapes dans le bon ordre.

4. Simulation du mouvement des trains

Le mouvement d’un train est simulé via un DispatcherTimer :

progression continue entre deux étapes avec interpolation

mouvement précis sur la polyline réelle du block si disponible

mise à jour de la position sur la carte

changement automatique d’état :

Programme → EnTransit

EnTransit → EnGare à la dernière étape

À chaque tick :

le train est assigné au block le plus proche

les blocks précédents sont libérés

la carte (markers + routes) est rafraîchie

5. Gestion des conflits

Le système détecte :

a) deux trains sur le même block

→ conflit direct

b) deux trains sur des blocks trop proches

→ conflit proximité (< 1 km)

Les conflits sont :

affichés dans une liste textuelle

dessinés sur la carte avec :

surlignage rouge/orange animé

étiquette des trains concernés

icône d’alerte 🚨

6. Itinéraires valides pour les tests

Ce sont les itinéraires “réalistes” qui suivent exactement les rails et ne sautent aucune station, recommandés pour les tests :

A → B → C (axe principal)

Gare Québec-Gatineau

Gare du palais

Gare CN

Est (vers Charlevoix)

Gare du palais

Port de Québec

Baie de Beauport

Vers Charlevoix

Centre → Rive sud

Gare Québec-Gatineau

Centre de distribution

Vers la rive-sud

Sud depuis Gare CN

Gare CN

Vers la rive-sud
