\ Copyright (c) 2020-2023 Travis Bemann
\
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

\ This is not actual Forth code, but rather setup directives for e4thcom to be
\ executed from the root of the zeptoforth directory to initialize zeptoforth
\ on an STM32L476 device.

#include src/stm32l476/forth/clock.fs
#include src/common/forth/basic.fs
#include src/common/forth/module.fs
#include src/common/forth/armv6m.fs
#include src/common/forth/fast_basic.fs
#include src/common/forth/value.fs
#include src/common/forth/interrupt.fs
#include src/common/forth/exception.fs
#include src/common/forth/multicore.fs
#include src/stm32l476/forth/erase.fs
#include src/common/forth/systick.fs
#include src/stm32l476/forth/serial.fs
#include src/stm32l476/forth/gpio.fs
#include src/common/forth/task.fs


