\ Copyright (c) 2023-2026 Travis Bemann
\ Copyright (c) 2026 Ken Mitton
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

#include extra/common/st7796s_text_common.fs

begin-module st7796s-text-spi

  oo import
  pin import
  spi import
  armv6m import
  st7796s-text-common import

  begin-module st7796s-text-spi-internal

    \ Maximum SPI clock
    62_500_000 constant max-clock

    \ Is this an RP2040 or RP2350?
    : rp? chip nip $7270 = ;

  end-module> import
  
  <st7796s-text-common> begin-class <st7796s-text-spi>

    continue-module st7796s-text-spi-internal

      \ SPI device
      cell member st7796s-text-device
      
      \ Reset pin
      cell member st7796s-text-reset-pin

      \ DMA channels
      cell member st7796s-text-dma0
      cell member st7796s-text-dma1

      \ Reset the ST7796S-TEXT
      method reset-st7796s-text ( self -- )
      
    end-module

    \ Update the ST7796S-TEXT device
    method update-display ( self -- )
    
  end-class

  <st7796s-text-spi> begin-implement

    \ Constructor
    :noname
      { device self }
      { the-font buf cols rows phys-cols phys-rows -- }
      { din sck dc cs backlight reset }
      dc cs backlight the-font buf cols rows phys-cols phys-rows self
      <st7796s-text-common>->new
      device self st7796s-text-device !
      reset self st7796s-text-reset-pin !

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
        dma-pool::allocate-dma self st7796s-text-dma0 !
        dma-pool::allocate-dma self st7796s-text-dma1 !
      [then]

      self reset-st7796s-text
      self st7796s-text-common-internal::init-st7796s-text
      0 phys-cols 0 phys-rows
      self st7796s-text-common-internal::st7796s-text-window!
    ; define new

    \ Destructor
    :noname { self -- }
      [ rp? ] [if]
        self st7796s-text-dma0 @ dma-pool::free-dma
        self st7796s-text-dma1 @ dma-pool::free-dma
      [then]
      self <st7796s-text-common>->destroy
    ; define destroy
    
    \ Reset the ST7796S-TEXT
    :noname { self -- }
      high self st7796s-text-reset-pin @ pin!
      200 ms
      low self st7796s-text-reset-pin @ pin!
      200 ms
      high self st7796s-text-reset-pin @ pin!
      200 ms
    ; define reset-st7796s-text

    \ Write blocking data
    :noname { addr count self -- }
      [ rp? ] [if]
        addr count self st7796s-text-dma0 @ self st7796s-text-dma1 @
        self st7796s-text-device @ buffer>spi-raw-dma drop
      [else]
        addr count self st7796s-text-device @ buffer>spi
      [then]
    ; define >st7796s-text

    \ Update the ST7796S device
    :noname { self -- }
      self st7796s-text-common::update-display
    ; define update-display

  end-implement
  
end-module
