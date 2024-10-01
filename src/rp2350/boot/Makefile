# Copyright (c) 2021-2024 Travis Bemann
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

PLATFORM=rp2350
BASEDIR=../../..
ODIR=$(BASEDIR)/obj
OBJ=$(ODIR)/boot.$(PLATFORM).o
ELF=$(BASEDIR)/boot.$(PLATFORM).elf
BIN=$(BASEDIR)/boot.$(PLATFORM).bin
IHEX=$(BASEDIR)/boot.$(PLATFORM).ihex
LIST=$(BASEDIR)/boot.$(PLATFORM).list
MAP=$(BASEDIR)/boot.$(PLATFORM).map
BOOT_WITH_CHECKSUM=./boot_with_checksum.py

all: $(BIN)

$(BIN): $(OBJ)
	$(LD) $(OBJ) -T boot.ld --cref -Map $(MAP) -o $(ELF)
	$(DUMP) -D $(ELF) > $(LIST)
	$(COPY) $(ELF) $@ -O binary
	$(COPY) $(ELF) $(IHEX) -O ihex

$(OBJ): boot.s
	$(AS) $(ASFLAGS) -o $@ boot.s

.PHONY: clean

clean:
	rm -f $(OBJ) $(MAP) $(LIST) $(ELF) $(BIN) $(IHEX)
