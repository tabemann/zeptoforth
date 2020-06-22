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
PLATFORM=$2
PORT=$3
PROJECT=zeptoforth

rm screenlog.0
st-flash erase
st-flash write bin/$VERSION/$PLATFORM/zeptoforth_kernel-$VERSION.bin 0x08000000
sleep 4
./utils/codeload3.py -B 115200 -p $PORT serial src/$PLATFORM/forth/setup.fs
./utils/codeload3.py -B 115200 -p $PORT serial src/common/forth/ihex.fs
screen -d -m $PORT 115200
screen -X log on
screen -X stuff 'clone\n'
sleep 35
screen -X log off
screen -X quit
sleep 1
sed '1d' screenlog.0 > inter
sed '$d' inter > inter.1
sed '$d' inter.1 > inter
sed 's/:00000001FF clone_end/:00000001FF/' inter > inter.1
mv inter.1 bin/$VERSION/$PLATFORM/zeptoforth_full-$VERSION.ihex
arm-none-eabi-objcopy -I ihex -O binary bin/$VERSION/$PLATFORM/zeptoforth_full-$VERSION.ihex bin/$VERSION/$PLATFORM/zeptoforth_full-$VERSION.bin
rm screenlog.0
rm inter
