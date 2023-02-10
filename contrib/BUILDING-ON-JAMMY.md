# Building the Releases and Documentation on Ubuntu 22.04 LTS ("jammy")
M. Edward (Ed) Borasky -- <znmeb@znmeb.net> -- 2023-02-09

1. Install Ubuntu 22.04.1 LTS ("Jammy Jellyfish") somewhere. I run this
on Windows 11 Windows Subsystem for Linux, but it should run on any
Ubuntu 22.04 LTS or later. It will ***not*** work on older long-term
support Ubuntu releases because the Python 3 package there is too old.

2. Open a terminal. Clone this repository and `cd` into it.

3. Type:

    ./build-on-jammy.sh

    The script will install any uninstalled dependencies. Then
    the script will remove the existing releases, make the
    releases, the HTML documentation and the EPUB documentation.
