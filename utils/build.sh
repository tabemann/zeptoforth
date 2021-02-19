#!/bin/sh

# Copyright (c) 2020 Travis Bemann
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

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
build_platform stm32f746
build_platform nrf52832
build_platform nrf52840
make clean
