#! /bin/bash

set -e

echo "Updating Ubuntu"
sudo apt-get update
sudo apt-get -qqy upgrade

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

echo "Cloning zeptoforth if needed"
if [ ! -d zeptoforth ]
then
  git clone https://github.com/AlgoCompSynth/zeptoforth.git
else
  echo "zeptoforth found - no clone needed"
fi

echo "Building fresh releases"
cd zeptoforth
make clean
make

echo "Making HTML documentation"
make html
echo "Making EPUB documentation"
mkdir -p epub
make epub

echo "git status"
git status

echo "Done!"
