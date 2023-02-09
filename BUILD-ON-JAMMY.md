# Building the Releases and Documentation on Ubuntu 22.04 LTS ("jammy")

1. Install Ubuntu 22.04 LTS ("Jammy Jellyfish") somewhere. I run this
on Windows 11 Windows Subsystem for Linux, but it will also work in a
Docker container obtained with

    docker pull ubuntu:jammy

    and should run with any Ubuntu 22.04 LTS or later.

2. Open a terminal and `cd` into a working directory. Place a copy of

    build-on-jammy.sh

    in the directory and make it executable with `chmod +x build-on-jammy.sh`

3. Type:

    ./build-on-jammy.sh

    The script will install any uninstalled dependencies, download the repository,
    and then make the releases, the HTML documentation and the EPUB documentation.
