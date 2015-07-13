#/bin/bash

####################################################################
#
#   Autor: Daniel Sánchez Prolongo
#   Proyecto: P-Bio
#  
#   Instalador de MySQL
#
####################################################################
echo "Installing MySQL server"
apt-install-if-needed mysql-server

echo "Creating PBio database"
mysql -u root -proot < ./scripts/create_database.sql