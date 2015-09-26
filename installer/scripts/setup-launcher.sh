#/bin/bash

####################################################################
#
#   Autor: Daniel Sánchez Prolongo
#   Proyecto: P-Bio
#  
#   Instalador de P-Bio Launcher
#
####################################################################
echo "Creating P-Bio folder..."
mkdir $HOME/pbio

echo "Creating P-Bio binaries folder..."
mkdir $HOME/pbio/bin

echo "Creating P-Bio log folder..."
mkdir $HOME/pbio/log

echo "Installing P-Bio Launcher"
cp ./bin/pbiolauncher.exe $HOME/pbio/bin/pbiolauncher.exe
cp ./bin/pbiolauncher.exe.config $HOME/pbio/bin/pbiolauncher.exe.config
cp ./bin/pbiolib.dll $HOME/pbio/bin/pbiolib.dll


