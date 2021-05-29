# Copyright (c) 2019-2021 Travis Bemann
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

IDIR=src/include
AS=arm-none-eabi-as
LD=arm-none-eabi-ld
COPY=arm-none-eabi-objcopy
DUMP=arm-none-eabi-objdump
ASFLAGS=-mcpu=cortex-m4 -mthumb -g
PREFIX=/usr/local
PLATFORM=stm32l476
VERSION=0.16.2.Dev
DATE:=$(shell date)

ODIR=obj

_OBJ = zeptoforth.o
OBJ = $(patsubst %,$(ODIR)/%,$(_OBJ))

zeptoforth.${PLATFORM}.elf: src/common/kernel_info.s $(OBJ)
	$(LD) $(OBJ) -T src/$(PLATFORM)/zeptoforth.ld --cref -Map zeptoforth.${PLATFORM}.map -nostartfiles -o $@
	$(DUMP) -D $@ > zeptoforth.${PLATFORM}.list
	$(COPY) $@ zeptoforth.${PLATFORM}.bin -O binary
	$(COPY) $@ zeptoforth.${PLATFORM}.ihex -O ihex

$(ODIR)/zeptoforth.o: src/$(PLATFORM)/*.s src/common/*.s
	mkdir -p obj
	$(AS) $(ASFLAGS) -o $@ src/$(PLATFORM)/zeptoforth.s

src/common/kernel_info.s:
	echo '        define_word "kernel-platform", visible_flag' > src/common/kernel_info.s
	echo '_kernel_platform:' >> src/common/kernel_info.s
	echo '        push {lr}' >> src/common/kernel_info.s
	echo '        string "'$(PLATFORM)'"' >> src/common/kernel_info.s
	echo '        pop {pc}' >> src/common/kernel_info.s
	echo '        end_inlined' >> src/common/kernel_info.s
	echo >> src/common/kernel_info.s
	echo '        define_word "kernel-version", visible_flag' >> src/common/kernel_info.s
	echo '_kernel_version:' >> src/common/kernel_info.s
	echo '        push {lr}' >> src/common/kernel_info.s
	echo '        string "'$(VERSION)'"' >> src/common/kernel_info.s
	echo '        pop {pc}' >> src/common/kernel_info.s
	echo '        end_inlined' >> src/common/kernel_info.s
	echo >> src/common/kernel_info.s
	echo '        define_word "kernel-date", visible_flag' >> src/common/kernel_info.s
	echo '_kernel_date:' >> src/common/kernel_info.s
	echo '        push {lr}' >> src/common/kernel_info.s
	echo '        string "'$(DATE)'"' >> src/common/kernel_info.s
	echo '        pop {pc}' >> src/common/kernel_info.s
	echo '        end_inlined' >> src/common/kernel_info.s
	echo >> src/common/kernel_info.s

.PHONY: clean html

html:
	cd docs ; sphinx-build -b html . ../html

clean:
	rm -f $(ODIR)/*.o zeptoforth.*.map zeptoforth.*.list zeptoforth.*.elf zeptoforth.*.bin zeptoforth.*.ihex src/common/kernel_info.s

