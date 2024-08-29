#!/bin/sh
set -e

# Copyright (c) 2024 Travis Bemann
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
PORT=$2
BLOCK_DEVICE=$3
FILESYSTEM=$4

# Get the directory of this script, we need this for the venv setup.
# See: https://stackoverflow.com/a/20434740
DIR="$( cd "$( dirname "$0" )" && pwd )"

# Handle some non-tivial common code.
. "${DIR}/common.sh"

check_screen

if [ ! $# -eq 4 ]; then
    cat 2>&! <<EOF
Usage:
    ${0} <version> <port> <block-device> <filesystem>
EOF
fi

RP2350_KERNEL="bin/${VERSION}/rp2350/zeptoforth_kernel-${VERSION}.uf2"

flash_rp ${RP2350_KERNEL} ${BLOCK_DEVICE} ${FILESYSTEM}
${DIR}/make_uf2_image.sh ${VERSION} rp2350 ${PORT} mini
issue_bootsel ${PORT}

flash_rp ${RP2350_KERNEL} ${BLOCK_DEVICE} ${FILESYSTEM}
${DIR}/make_uf2_image.sh ${VERSION} rp2350 ${PORT} full
issue_bootsel ${PORT}

flash_rp ${RP2350_KERNEL} ${BLOCK_DEVICE} ${FILESYSTEM}
${DIR}/make_uf2_image.sh ${VERSION} rp2350 ${PORT} full_usb
issue_bootsel ${PORT}
