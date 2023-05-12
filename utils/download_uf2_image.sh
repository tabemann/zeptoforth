#!/bin/sh

# Copyright (c) 2020-2023 Travis Bemann
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

PORT=$1
IMAGE=$2
PROJECT=zeptoforth

./utils/codeload3.py -B 115200 -p $PORT serial src/common/forth/ihex.fs
screen -d -m $PORT 115200
screen -X log on
screen -X stuff 'clone\n'
until grep 'clone_end' screenlog.0
do
    sleep 1
done
screen -X log off
screen -X quit
sleep 1
sed '1d' screenlog.0 > inter
sed '$d' inter > inter.1
sed '$d' inter.1 > inter
sed 's/:00000001FF clone_end/:00000001FF/' inter > inter.1
mv inter.1 ${IMAGE}.ihex
arm-none-eabi-objcopy -I ihex -O binary ${IMAGE}.ihex ${IMAGE}.bin
src/rp2040/make_uf2.py ${IMAGE}.bin ${IMAGE}.uf2
rm screenlog.0
rm inter

./utils/codeload3.py -B 115200 -p $PORT serial src/common/forth/ihex_minidict.fs
screen -d -m $PORT 115200
screen -X log on
screen -X stuff 'clone\n'
until grep 'clone_end' screenlog.0
do
    sleep 1
done
screen -X log off
screen -X quit
sleep 1
sed '1d' screenlog.0 > inter
sed '$d' inter > inter.1
sed '$d' inter.1 > inter
sed 's/:00000001FF clone_end/:00000001FF/' inter > inter.1
mv inter.1 ${IMAGE}.minidict.ihex
arm-none-eabi-objcopy -I ihex -O binary ${IMAGE}.minidict.ihex ${IMAGE}.minidict.bin
rm screenlog.0
rm inter

src/rp2040/make_uf2.py ${IMAGE}.bin ${IMAGE}.minidict.bin ${IMAGE}.uf2
