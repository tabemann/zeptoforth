#!/bin/sh

VERSION=$1

build_platform()
{
    PLATFORM=$1
    echo "Building platform $1..."
    make clean
    make PLATFORM=$PLATFORM
    mkdir bin/$VERSION/$PLATFORM
    cp zeptoforth.bin bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.bin
    cp zeptoforth.ihex bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.ihex
    cp zeptoforth.elf bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.elf
}

mkdir bin/$VERSION
build_platform stm32l476
build_platform stm32f407
build_platform nrf52832
build_platform nrf52840
make clean
