# Copyright (c) 2019 Travis Bemann
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

IDIR =src/include
AS=arm-none-eabi-as
LD=arm-none-eabi-ld
COPY=arm-none-eabi-objcopy
DUMP=arm-none-eabi-objdump
ASFLAGS=-mcpu=cortex-m4 -mthumb -g
PREFIX=/usr/local
PLATFORM=stm32l476

ODIR=obj

_OBJ = zeptoforth.o
OBJ = $(patsubst %,$(ODIR)/%,$(_OBJ))

zeptoforth.elf: $(OBJ)
	$(LD) $(OBJ) -T src/$(PLATFORM)/zeptoforth.ld --cref -Map zeptoforth.map -nostartfiles -o $@
	$(DUMP) -D $@ > zeptoforth.list
	$(COPY) $@ zeptoforth.bin -O binary
	$(COPY) $@ zeptoforth.ihex -O ihex

$(ODIR)/zeptoforth.o: src/$(PLATFORM)/*.s src/common/*.s
	mkdir -p obj
	$(AS) $(ASFLAGS) -o $@ src/$(PLATFORM)/zeptoforth.s

.PHONY: clean html

html:
	cd docs ; sphinx-build -b html . ../html

clean:
	rm -f $(ODIR)/*.o zeptoforth.map zeptoforth.list zeptoforth.elf zeptoforth.bin zeptoforth.ihex
