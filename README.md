# WebServeurLocal
Serveur Web minimaliste html, css, js et php
# Repertoire sources
## Projet
	Service Windows
	Visual Studio 2022
 	.NET 4.5
  	64 bits
# Repertoire bin
Application de tests
* Le service Windows "WebServeurLocal" s'installe avec l'outil .NET Installutil.exe
* Le fichier .config permet de configurer la racine des fichiers et le prefixe d'url
* Les types MIME peuvent être ajoutés au fichier mimesType.txt chargé au démarrage du service

* L'exemple de test supporte jQuery richtext et php 8.3 avec l'extension sqlite3
* Vous pouvez configurer le fichier php.ini pour ajouter des extensions dans le répertoire "ext"
* La methode POST ?site="filename" permet la lecture et l'écriture d'un fichier dans la racine des fichiers (attention aux droits d'écriture et à la localisation des fichiers)

  # Utilisations
  Limité à une utilisation locale (pas de sécurité)
 
