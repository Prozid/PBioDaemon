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

echo "Creating P-Bio log folder..."
mkdir $HOME/pbio

echo "Creating pbio.conf..."
cat > /tmp/pbio.conf <<EOF
[program:pbiod]
command=mono-service2 pbiod.exe --no-daemon
directory=./bin
autostart=true
autorestart=true
user=$USER
stdout_logfile=$HOME/pbio/out.log
stderr_logfile=$HOME/pbio/err.log
EOF

echo "Moving pbio.conf to /etc/supervisor/conf.d/"
sudo mv /tmp/pbio.conf /etc/supervisor/conf.d/

echo "Reloading supervisor..."
sudo service supervisor update
