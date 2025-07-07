\ Copyright (c) 2023-2025 Travis Bemann
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

#include extra/common/st7789v_text_common.fs

begin-module st7789v-text-spi

  oo import
  pin import
  spi import
  armv6m import
  st7789v-text-common import

  begin-module st7789v-text-spi-internal

    \ Maximum SPI clock
    32_000_000 constant max-clock

    \ Is this an RP2040 or RP2350?
    : rp? chip nip $7270 = ;

  end-module> import
  
  <st7789v-text-common> begin-class <st7789v-text-spi>

    continue-module st7789v-text-spi-internal

      \ SPI device
      cell member st7789v-text-device
      
      \ Reset pin
      cell member st7789v-text-reset-pin

      \ DMA channels
      cell member st7789v-text-dma0
      cell member st7789v-text-dma1

      \ Reset the ST7789V-text
      method reset-st7789v-text ( self -- )
      
    end-module

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7789V-text device
    method update-display ( self -- )
    
  end-class

  <st7789v-text-spi> begin-implement

    \ Constructor
    :noname
      { device self }
      { the-font buf round cols rows phys-cols phys-rows }
      { din sck dc cs backlight reset -- }
      dc cs backlight the-font buf round cols rows phys-cols phys-rows self
      <st7789v-text-common>->new
      device self st7789v-text-device !
      reset self st7789v-text-reset-pin !

      reset output-pin
      low reset pin!
      device din spi-pin
      device sck spi-pin
      device master-spi
      max-clock device spi-baud!
      8 device spi-data-size!
      false false device motorola-spi
      device enable-spi

      [ rp? ] [if]
        dma-pool::allocate-dma self st7789v-text-dma0 !
        dma-pool::allocate-dma self st7789v-text-dma1 !
      [then]

      true self backlight!
      self reset-st7789v-text
      self st7789v-text-common-internal::init-st7789v-text
      0 phys-cols 0 phys-rows
      self st7789v-text-common-internal::st7789v-text-window!
    ; define new

    \ Destructor
    :noname { self -- }
      [ rp? ] [if]
        self st7789v-text-dma0 @ dma-pool::free-dma
        self st7789v-text-dma1 @ dma-pool::free-dma
      [then]
      self <st7789v-text-common>->destroy
    ; define destroy
    
    \ Reset the ST7789V-text
    :noname { self -- }
      high self st7789v-text-reset-pin @ pin!
      200 ms
      low self st7789v-text-reset-pin @ pin!
      200 ms
      high self st7789v-text-reset-pin @ pin!
      200 ms
    ; define reset-st7789v-text

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7789v-text-common::backlight!
    ; define backlight!

    \ Write blocking data
    :noname { addr count self -- }
      [ rp? ] [if]
        addr count self st7789v-text-dma0 @ self st7789v-text-dma1 @
        self st7789v-text-device @ buffer>spi-raw-dma drop
      [else]
        addr count self st7789v-text-device @ buffer>spi
      [then]
    ; define >st7789v-text

    \ Update the ST7789V device
    :noname { self -- }
      self st7789v-text-common::update-display
    ; define update-display

  end-implement
  
end-module