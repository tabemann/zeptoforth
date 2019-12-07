# Copyright (c) 2019, Travis Bemann
# All rights reserved.
# 
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are met:
# 
# 1. Redistributions of source code must retain the above copyright notice,
#    this list of conditions and the following disclaimer.
# 
# 2. Redistributions in binary form must reproduce the above copyright notice,
#    this list of conditions and the following disclaimer in the documentation
#    and/or other materials provided with the distribution.
# 
# 3. Neither the name of the copyright holder nor the names of its
#    contributors may be used to endorse or promote products derived from
#    this software without specific prior written permission.
# 
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
# ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
# LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
# CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
# SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
# CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
# ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
# POSSIBILITY OF SUCH DAMAGE.

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

$(ODIR)/zeptoforth.o: src/$(PLATFORM)/zeptoforth.s
	mkdir -p obj
	$(AS) $(ASFLAGS) -o $@ $<

zeptoforth.elf: $(OBJ)
	$(LD) $(OBJ) -T src/$(PLATFORM)/zeptoforth.ld --cref -Map zeptoforth.map -nostartfiles -o $@
	$(DUMP) -D $@ > zeptoforth.list
	$(COPY) $@ zeptoforth.bin -O binary
	$(COPY) $@ zeptoforth.ihex -O ihex

.PHONY: clean

clean:
	rm -f $(ODIR)/*.o zeptoforth.map zeptoforth.list zeptoforth.elf zeptoforth.bin zeptoforth.ihex
