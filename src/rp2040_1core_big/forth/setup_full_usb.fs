\ Copyright (c) 2021-2025 Travis Bemann
\ Copyright (c) 2024 Paul Koning
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
\ on an RP2040 device.

#include src/rp_common/forth/rp_init_basic.fs
#include src/common/forth/basic.fs
#include src/common/forth/module.fs
#include src/common/forth/armv6m.fs
#include src/common/forth/fast_basic.fs
#include src/common/forth/minidict.fs
#include src/common/forth/value.fs
#include src/common/forth/interrupt.fs
#include src/common/forth/exception.fs
#include src/common/forth/multicore.fs
#include src/rp2040_1core/forth/core1.fs
#include src/rp_common/forth/erase.fs
#include src/common/forth/lambda.fs
#include src/common/forth/fixed.fs
#include src/common/forth/systick.fs
#include src/rp_common/forth/serial.fs
#include src/rp2040/forth/gpio.fs
#include src/rp2040/forth/pin.fs
#include src/rp2040/forth/pio.fs
#include src/common/forth/task.fs
#include src/rp_common/forth/watchdog.fs
#include src/rp_common/forth/led.fs
#include src/common/forth/full_default.fs
#include src/rp2040/forth/timer.fs
#include src/rp2040/forth/rng.fs
#include src/rp2040_big/forth/qspi.fs
#include src/common/forth/block.fs
#include src/common/forth/edit.fs
#include src/rp2040/forth/dma.fs
#include src/rp_common/forth/dma_pool.fs
#include src/rp_common/forth/uart.fs
#include src/rp_common/forth/adc.fs
#include src/rp_common/forth/spi.fs
#include src/rp_common/forth/i2c.fs
#include src/rp2040/forth/pwm.fs
#include src/rp2040/forth/rtc.fs
#include src/rp_common/forth/clocks.fs
#include src/rp2040/forth/voltage.fs
#include src/common/forth/full_extra.fs
#include src/common/forth/blocks_block_dev.fs
#include src/common/forth/simple_blocks_fat32.fs
#include src/rp_common/forth/usb_constants.fs
#include src/rp_common/forth/usb_cdc_buffers.fs
#include src/rp_common/forth/usb_core_and_cdc.fs
#include src/rp_common/forth/usb.fs
#include src/common/forth/save_minidict.fs

\ Set a cornerstone to enable deleting everything compiled after this code
compile-to-flash
cornerstone restore-state
compile-to-ram

mini-dict::save-flash-mini-dict
