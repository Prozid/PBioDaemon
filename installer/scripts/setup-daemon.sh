#/bin/bash

####################################################################
#
#   Autor: Daniel Sánchez Prolongo
#   Proyecto: P-Bio
#  
#   Instalador de P-Bio Daemon
#
####################################################################

echo "Supervisor..."
apt-install-if-needed supervisor


echo "Creating P-Bio folder..."
mkdir $HOME/pbio

echo "Creating P-Bio binaries folder..."
mkdir $HOME/pbio/bin

echo "Creating P-Bio log folder..."
mkdir $HOME/pbio/log

echo "Installing P-Bio Daemon"
cp ./bin/pbiod.exe $HOME/pbio/bin/pbiod.exe
cp ./bin/pbiod.exe.config $HOME/pbio/bin/pbiod.exe.config
cp ./bin/pbiolib.dll $HOME/pbio/bin/pbiolib.dll

echo "Creating pbio.conf..."
# command=mono-service2 pbiod.exe --no-daemon
cat > /tmp/pbio.conf <<EOF
[program:pbiod]
command=mono pbiod.exe
directory=$HOME/pbio/bin
autostart=true
autorestart=true
user=$USER
stdout_logfile=$HOME/pbio/log/out.log
stderr_logfile=$HOME/pbio/log/err.log
EOF

echo "Moving pbio.conf to /etc/supervisor/conf.d/"
sudo mv /tmp/pbio.conf /etc/supervisor/conf.d/

echo "Reloading supervisor..."
sudo service supervisor force-reload

