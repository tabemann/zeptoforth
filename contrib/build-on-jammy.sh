#! /bin/bash

set -e

echo ""
echo "Upgrading Ubuntu"
sudo apt-get update && \
  sudo apt-get -qqy upgrade

echo ""
echo "Installing build dependencies"
sudo apt-get install -qqy --no-install-recommends \
  binutils-arm-none-eabi \
  build-essential \
  ca-certificates \
  gcc-arm-none-eabi \
  python3-sphinx \
  python3-myst-parser

echo ""
echo "Building fresh releases"
make clean
make

echo ""
echo "Making HTML documentation"
make html
echo ""
echo "Making EPUB documentation"
mkdir --parents epub
make epub

echo ""
echo "git status"
git status

echo ""
echo "Done!"
