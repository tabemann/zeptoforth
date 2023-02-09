# Building the Releases and Documentation on Ubuntu 22.04 LTS ("jammy")

1. Install Ubuntu 22.04 LTS ("Jammy Jellyfish") somewhere. I run this
on Windows 11 Windows Subsystem for Linux, but it will also work in a
Docker container obtained with

    docker pull ubuntu:jammy

    and should run with any Ubuntu 22.04 LTS or later.

2. Clone this repository:

    git clone https://github.com/AlgoCompSynth/zeptoforth.git

3. In a terminal, type:

    cd zeptoforth
    ./build-on-jammy.sh

    The script will install any uninstalled dependencies and then
    make the releases, and the HTML and EPUB documentation.
