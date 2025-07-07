#!/bin/sh

# Copyright (c) 2023 Travis Bemann
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

PORT="$1"
FW_IMAGE="$2"
CLM_IMAGE="$3"

./utils/convert_bin.py cyw43-fw $FW_IMAGE cyw43_fw.fs
./utils/codeload3.py -B 115200 -p $PORT serial cyw43_fw.fs
rm cyw43_fw.fs
./utils/convert_bin.py cyw43-clm $CLM_IMAGE cyw43_clm.fs
./utils/codeload3.py -B 115200 -p $PORT serial cyw43_clm.fs
rm cyw43_clm.fs
