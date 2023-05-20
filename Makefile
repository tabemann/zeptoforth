# Copyright (c) 2019-2023 Travis Bemann
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

export IDIR=src/include
export AS=arm-none-eabi-as
export LD=arm-none-eabi-ld
export COPY=arm-none-eabi-objcopy
export DUMP=arm-none-eabi-objdump
export ASFLAGS=-g
export PREFIX=/usr/local
export PLATFORM=stm32f407
export VERSION=1.0.1-dev

KERNEL_INFO=src/common/kernel_info.s

all: stm32f407 stm32f411 stm32l476 stm32f746 rp2040

install:
	$(MAKE) -C src/stm32f407 install
	$(MAKE) -C src/stm32f411 install
	$(MAKE) -C src/stm32l476 install
	$(MAKE) -C src/stm32f746 install
	$(MAKE) -C src/rp2040 install

stm32f407:
	$(MAKE) -C src/stm32f407

stm32f411:
	$(MAKE) -C src/stm32f411

stm32l476:
	$(MAKE) -C src/stm32l476

stm32f746:
	$(MAKE) -C src/stm32f746

rp2040:
	$(MAKE) -C src/rp2040

.PHONY: all install stm32f407 stm32f411 stm32l746 stm32f746 clean html epub

html:
	cd docs ; sphinx-build -b html . ../html

epub:
	cd docs ; sphinx-build -b epub . ../epub

clean:
	$(MAKE) -C src/stm32f407 clean
	$(MAKE) -C src/stm32f411 clean
	$(MAKE) -C src/stm32l476 clean
	$(MAKE) -C src/stm32f746 clean
	$(MAKE) -C src/rp2040 clean
	$(MAKE) -C src/common clean

