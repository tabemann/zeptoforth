#! /bin/bash

set -e

if [ "$UID" = "0" ]
then
  echo ""
  echo "Running as root - installing 'sudo'"
  export DEBIAN_FRONTEND=noninteractive
  apt-get update && \
    apt-get -qqy upgrade && \
    apt-get install -qqy --no-install-recommends sudo
else
  echo ""
  echo "Updating Ubuntu"
  sudo apt-get update && \
    sudo apt-get -qqy upgrade
fi

echo ""
echo "Installing build dependencies"
sudo apt-get install -qqy --no-install-recommends \
  binutils-arm-none-eabi \
  build-essential \
  ca-certificates \
  gcc-arm-none-eabi \
  git \
  python3 \
  python3-sphinx \
  python3-myst-parser

echo ""
echo "Cloning zeptoforth if needed"
if [ ! -d zeptoforth ]
then
  git clone https://github.com/AlgoCompSynth/zeptoforth.git
else
  echo ""
  echo "zeptoforth found - no clone needed"
fi

echo ""
echo "Building fresh releases"
cd zeptoforth
make clean
make

echo ""
echo "Making HTML documentation"
make html
echo ""
echo "Making EPUB documentation"
mkdir -p epub
make epub

echo ""
echo "git status"
git status

echo ""
echo "Done!"
