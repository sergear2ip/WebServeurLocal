# WebServeurLocal 

Minimalist web server with html, css, js, and php

# Source Directory
## Project
Windows Service
Visual Studio 2022
.NET 4.5
64-bit
# Bin Directory
Test Application
* The Windows service "WebServeurLocal" is installed using the .NET Installutil.exe tool.
* The .config file allows configuring the root of the files and the URL prefix.
* MIME types can be added to the mimesType.txt file loaded at service startup.
  
* The test example supports jQuery richtext and php 8.3 with the sqlite3 extension.
* You can configure the php.ini file to add extensions in the "ext" directory.
*The POST method ?site="filename" allows reading and writing a file in the root of the files (pay attention to write permissions and file location).

# Usage
Limited to local use (no security).

![image](https://github.com/sergear2ip/WebServeurLocal/assets/97619122/8e0ac9bc-b55d-4914-aa5a-80aa4b0bb39a)
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
 
