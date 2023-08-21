#!/usr/bin/env bash
set -e

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
PLATFORM=$2
PORT=$3
IMAGE=$4
PROJECT=zeptoforth

# Get the directory of this script, we need this for the venv setup.
# See: https://stackoverflow.com/a/20434740
DIR="$( cd "$( dirname "$0" )" && pwd )"

# Handle some non-tivial common code.
source "${DIR}/common.sh"

check_screen

if [ ! $# -eq 4 ]; then
  cat 2>&1 <<EOD
Usage:
    ${0} <version> <platform> <port> <image>
EOD
  exit 1
fi

TARGET="bin/${VERSION}/${PLATFORM}/zeptoforth_${IMAGE}-${VERSION}"

#st-flash erase
#st-flash write bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.bin 0x08000000
#st-flash reset
#sleep 3

codeloader ${PORT} src/$PLATFORM/forth/setup_$IMAGE.fs

screen_download_ihex ${PORT} ${TARGET} 

