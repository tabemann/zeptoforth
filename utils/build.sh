#!/bin/sh

# Copyright (c) 2020 Travis Bemann
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>.


VERSION=$1

build_platform()
{
    PLATFORM=$1
    echo "Building platform $1..."
    make clean
    make VERSION=$VERSION PLATFORM=$PLATFORM
    mkdir bin/$VERSION/$PLATFORM
    cp zeptoforth.bin bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.bin
    cp zeptoforth.ihex bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.ihex
    cp zeptoforth.elf bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.elf
}

set_version
mkdir bin/$VERSION
build_platform stm32l476
build_platform stm32f407
build_platform nrf52832
build_platform nrf52840
make clean
