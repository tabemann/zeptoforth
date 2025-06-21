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

#include extra/common/st7365p_text_common.fs

begin-module st7365p-text-spi

  oo import
  pin import
  spi import
  armv6m import
  st7365p-text-common import

  begin-module st7365p-text-spi-internal

    \ Maximum SPI clock
    62_500_000 constant max-clock

    \ Is this an RP2040 or RP2350?
    : rp? chip nip $7270 = ;

  end-module> import
  
  <st7365p-text-common> begin-class <st7365p-text-spi>

    continue-module st7365p-text-spi-internal

      \ SPI device
      cell member st7365p-text-device
      
      \ Reset pin
      cell member st7365p-text-reset-pin

      \ DMA channels
      cell member st7365p-text-dma0
      cell member st7365p-text-dma1

      \ Reset the ST7365P-TEXT
      method reset-st7365p-text ( self -- )
      
    end-module

    \ Update the ST7365P-TEXT device
    method update-display ( self -- )
    
  end-class

  <st7365p-text-spi> begin-implement

    \ Constructor
    :noname
      { device self }
      { invert the-font buf cols rows phys-cols phys-rows -- }
      { din sck dc cs reset }
      dc cs invert the-font buf cols rows phys-cols phys-rows self
      <st7365p-text-common>->new
      device self st7365p-text-device !
      reset self st7365p-text-reset-pin !

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
        dma-pool::allocate-dma self st7365p-text-dma0 !
        dma-pool::allocate-dma self st7365p-text-dma1 !
      [then]

      self reset-st7365p-text
      self st7365p-text-common-internal::init-st7365p-text
      0 phys-cols 0 phys-rows
      self st7365p-text-common-internal::st7365p-text-window!
    ; define new

    \ Destructor
    :noname { self -- }
      [ rp? ] [if]
        self st7365p-text-dma0 @ dma-pool::free-dma
        self st7365p-text-dma1 @ dma-pool::free-dma
      [then]
      self <st7365p-text-common>->destroy
    ; define destroy
    
    \ Reset the ST7365P-TEXT
    :noname { self -- }
      high self st7365p-text-reset-pin @ pin!
      200 ms
      low self st7365p-text-reset-pin @ pin!
      200 ms
      high self st7365p-text-reset-pin @ pin!
      200 ms
    ; define reset-st7365p-text

    \ Write blocking data
    :noname { addr count self -- }
      [ rp? ] [if]
        addr count self st7365p-text-dma0 @ self st7365p-text-dma1 @
        self st7365p-text-device @ buffer>spi-raw-dma drop
      [else]
        addr count self st7365p-text-device @ buffer>spi
      [then]
    ; define >st7365p-text

    \ Update the ST7365P device
    :noname { self -- }
      self st7365p-text-common::update-display
    ; define update-display

  end-implement
  
end-module