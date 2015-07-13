#/bin/bash

####################################################################
#
#   Autor: Daniel Sánchez Prolongo
#   Proyecto: P-Bio
#  
#   Instalador de Mono
#
####################################################################
echo "Installing Mono"
apt-install-if-needed mono-complete

echo "Installin MySQL connector for .NET"
sudo gacutil /i ./bin/MySql.Data.dll

