\ Copyright (c) 2021 Travis Bemann
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
\ on an STM32F407 device.

#include src/stm32f407/forth/clock.fs
#include src/common/forth/basic.fs
#include src/common/forth/module.fs
#include src/common/forth/interrupt.fs
#include src/stm32f407/forth/erase.fs
#include src/common/forth/lambda.fs
#include src/common/forth/fixed.fs
#include src/common/forth/systick.fs
#include src/stm32f407/forth/int_io.fs
#include src/stm32f407/forth/gpio.fs
#include src/stm32f407/forth/exti.fs
#include src/common/forth/task.fs
#include src/common/forth/schedule.fs
#include src/stm32f407/forth/led.fs
#include src/common/forth/big_default.fs
#include src/stm32f407/forth/rng.fs
#include src/common/forth/swdcom.fs

\ Set a cornerstone to enable deleting everything compiled after this code
cornerstone restore-state
