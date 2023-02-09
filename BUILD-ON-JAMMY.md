# Building the Releases and Documentation on Ubuntu 22.04 LTS ("jammy")

1. Install Ubuntu 22.04 LTS ("Jammy Jellyfish") somewhere. I run this
on Windows 11 Windows Subsystem for Linux, but it should run on any
Ubuntu 22.04 LTS or later. It will ***not*** work on older long-term
support Ubuntu releases because the Python 3 package there is too old.

2. Open a terminal and `cd` into a working directory. Place a copy of

    build-on-jammy.sh

    in the directory and make it executable with `chmod +x build-on-jammy.sh`

3. Type:

    ./build-on-jammy.sh

    The script will install any uninstalled dependencies. Then if the 
    `zeptoforth` directory does not exist, it will clone the GitHub
    repository. Once `zeptoforth` exists, the script will `cd` into
    it and make the releases, the HTML documentation and the EPUB
    documentation.
