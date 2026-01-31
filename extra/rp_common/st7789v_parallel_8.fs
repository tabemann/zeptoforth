\ Copyright (c) 2023-2026 Travis Bemann
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

#include extra/rp_common/st7789v_8_common.fs

begin-module st7789v-8

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  pio import
  pio-pool import
  dma import
  dma-pool import
  armv6m import

  begin-module st7789v-8-internal

    \ Maximum PIO clock
    32_000_000 constant max-clock

    \ The PIO program used for communicating
    0 constant SIDE_0
    16 constant SIDE_1
    :pio st7789v-8-pio-program
      \ Set the starting address
      start>

      \ Set the address to wrap to
      wrap<

      \ Transmit eight bits and sidest SCK to be low
      8 SIDE_0 OUT_PINS out+,

      \ Pull and set SCK high
      SIDE_1 PULL_BLOCK PULL_NOT_EMPTY pull+,

      \ Set the address to wrap to
      <wrap
    ;pio

  end-module> import
  
  <st7789v-8-common> begin-class <st7789v-8>

    continue-module st7789v-8-internal
      
      \ PIO peripheral
      cell member st7789v-8-pio

      \ PIO state machine
      cell member st7789v-8-sm

      \ SCK pin
      cell member st7789v-8-sck-pin

      \ Base parallel data pins
      cell member st7789v-8-data-pins-base

      \ DMA channel
      cell member st7789v-8-dma-channel
      
    end-module

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7789V-8 device
    method update-display ( self -- )
    
  end-class

  <st7789v-8> begin-implement

    \ Constructor
    :noname
      { self }
      { data wr-sck rd-sck dc cs backlight buf round cols rows sm pio }
      dc cs backlight buf round cols rows self <st7789v-8-common>->new

      \ Set up the PIO program
      pio -1 <> sm -1 <> or if
        pio pio-internal::validate-pio
        sm pio-internal::validate-sm
        sm bit pio sm-disable
        pio st7789v-8-pio-program p-size alloc-piomem { pio-addr }
        sm pio st7789v-8-pio-program pio-addr setup-prog
      else
        st7789v-8-pio-program 1 allocate-pio-sms-w-prog to pio drop to sm
      then

      sm self st7789v-8-sm !
      wr-sck self st7789v-8-sck-pin !
      data self st7789v-8-data-pins-base !
      allocate-dma self st7789v-8-dma-channel !
      rd-sck output-pin
      high rd-sck pin!

      \ Disable the PIO state machine
      sm bit pio sm-disable

      \ Configure the pins to be PIO output and sideset pins
      data 8 pio pins-pio-alternate
      wr-sck 1 pio pins-pio-alternate
      data 8 sm pio sm-out-pins!
      wr-sck 1 sm pio sm-sideset-pins!
      true sm pio sm-join-tx-fifo!

      \ Set the output shift direction and autopush
      left sm pio sm-out-shift-dir
      on sm pio sm-autopush!
      8 sm pio sm-push-threshold!

      \ Set the clock divisor
      0 sysclk @ max-clock 1- + max-clock / sm pio sm-clkdiv!

      \ Initialize the SCK and data pins' direction and value
      out wr-sck sm pio sm-pindir!
      data 8 + data ?do out i sm pio sm-pindir! loop
      high wr-sck sm pio sm-pin!
      data 8 + data ?do low i sm pio sm-pin! loop

      \ Enable the PIO state machine
      sm bit pio sm-enable

      true self backlight!

      self st7789v-8-common-internal::init-st7789v-8
      0 cols 0 rows self st7789v-8-common-internal::st7789v-8-window!
    ; define new

    \ Destructor
    :noname { self -- }
      self st7789v-8-dma-channel @ free-dma
      self <st7789v-8-common>->destroy
    ; define destroy

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7789v-8-common::backlight!
    ; define backlight!

    \ Write blocking data over DMA
    :noname { addr count self -- }

      \ Transfer data from the buffer, one byte at a time, to the PIO state
      \ machine's TXF register
      addr self st7789v-8-sm @ self st7789v-8-pio @ pio-registers::TXF count 1
      self st7789v-8-sm @
      self st7789v-8-pio @ PIO0 = if 0 else 1 then DREQ_PIO_TX
      self st7789v-8-dma-channel @ start-buffer>register-dma

      \ Spin until DMA completes
      self st7789v-8-dma-channel @ spin-wait-dma
      
    ; define >st7789v-8

    \ Update the ST7789V device
    :noname { self -- }
      self st7789v-8-common::update-display
    ; define update-display

  end-implement
  
end-module