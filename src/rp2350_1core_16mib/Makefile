# Copyright (c) 2021-2025 Travis Bemann
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

export PLATFORM=rp2350_1core_16mib
BASEDIR=../..
ODIR=$(BASEDIR)/obj
IDIR=$(BASEDIR)/bin/$(VERSION)/$(PLATFORM)
OBJ=$(ODIR)/zeptoforth.$(PLATFORM).o
BOOT_BIN=$(BASEDIR)/boot.rp2350.bin
ELF=$(BASEDIR)/zeptoforth.$(PLATFORM).elf
BIN=$(BASEDIR)/zeptoforth.$(PLATFORM).bin
UF2=$(BASEDIR)/zeptoforth.$(PLATFORM).uf2
IHEX=$(BASEDIR)/zeptoforth.$(PLATFORM).ihex
LIST=$(BASEDIR)/zeptoforth.$(PLATFORM).list
MAP=$(BASEDIR)/zeptoforth.$(PLATFORM).map
MINI_DICT_FILLER=$(BASEDIR)/src/rp2350/mini_dict_filler
MAKE_UF2=../rp2350/make_uf2.sh

all: kernel_info $(UF2)

install:
	mkdir -p $(IDIR)
	cp $(UF2) $(IDIR)/zeptoforth_kernel-$(VERSION).uf2
	cp $(BIN) $(IDIR)/zeptoforth_kernel-$(VERSION).bin
	cp $(IHEX) $(IDIR)/zeptoforth_kernel-$(VERSION).ihex
	cp $(ELF) $(IDIR)/zeptoforth_kernel-$(VERSION).elf

kernel_info:
	$(MAKE) -C ../common kernel_info

$(UF2): $(BIN)
	$(MAKE) -C ../rp2350/boot
	$(MAKE_UF2) $(BOOT_BIN) $(BIN) $(MINI_DICT_FILLER) $(UF2)

$(BIN): $(OBJ)
	$(LD) $(OBJ) -T ../rp2350/zeptoforth.ld --cref -Map $(MAP) -o $(ELF)
	$(DUMP) -D $(ELF) > $(LIST)
	$(COPY) $(ELF) $@ -O binary
	$(COPY) $(ELF) $(IHEX) -O ihex

$(OBJ): ../rp2350/*.s ../rp2350_1core/*.s ../common/*.s ../m33/*.s
	mkdir -p $(ODIR)
	$(AS) $(ASFLAGS) -o $@ ../rp2350_1core/zeptoforth.s

.PHONY: all install kernel_info clean

clean:
	make -C ../rp2350/boot clean
	rm -f $(OBJ) $(ELF) $(BIN) $(UF2) $(IHEX) $(LIST) $(MAP)
