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

#include extra/common/ili9488_text_common.fs

begin-module ili9488-text-spi

  oo import
  pin import
  spi import
  armv6m import
  ili9488-text-common import

  begin-module ili9488-text-spi-internal

    \ Maximum SPI clock
    62_500_000 constant max-clock

    \ Is this an RP2040 or RP2350?
    : rp? chip nip $7270 = ;

  end-module> import
  
  <ili9488-text-common> begin-class <ili9488-text-spi>

    continue-module ili9488-text-spi-internal

      \ SPI device
      cell member ili9488-text-device
      
      \ Reset pin
      cell member ili9488-text-reset-pin

      \ DMA channels
      cell member ili9488-text-dma0
      cell member ili9488-text-dma1

      \ Reset the ILI9488-TEXT
      method reset-ili9488-text ( self -- )
      
    end-module

    \ Update the ILI9488-TEXT device
    method update-display ( self -- )
    
  end-class

  <ili9488-text-spi> begin-implement

    \ Constructor
    :noname
      { device self }
      { invert the-font buf cols rows phys-cols phys-rows -- }
      { din sck dc cs reset }
      dc cs invert the-font buf cols rows phys-cols phys-rows self
      <ili9488-text-common>->new
      device self ili9488-text-device !
      reset self ili9488-text-reset-pin !

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
        dma-pool::allocate-dma self ili9488-text-dma0 !
        dma-pool::allocate-dma self ili9488-text-dma1 !
      [then]

      self reset-ili9488-text
      self ili9488-text-common-internal::init-ili9488-text
      0 phys-cols 0 phys-rows
      self ili9488-text-common-internal::ili9488-text-window!
    ; define new

    \ Destructor
    :noname { self -- }
      [ rp? ] [if]
        self ili9488-text-dma0 @ dma-pool::free-dma
        self ili9488-text-dma1 @ dma-pool::free-dma
      [then]
      self <ili9488-text-common>->destroy
    ; define destroy
    
    \ Reset the ILI9488-TEXT
    :noname { self -- }
      high self ili9488-text-reset-pin @ pin!
      200 ms
      low self ili9488-text-reset-pin @ pin!
      200 ms
      high self ili9488-text-reset-pin @ pin!
      200 ms
    ; define reset-ili9488-text

    \ Write blocking data
    :noname { addr count self -- }
      [ rp? ] [if]
        addr count self ili9488-text-dma0 @ self ili9488-text-dma1 @
        self ili9488-text-device @ buffer>spi-raw-dma drop
      [else]
        addr count self ili9488-text-device @ buffer>spi
      [then]
    ; define >ili9488-text

    \ Update the ILI9488 device
    :noname { self -- }
      self ili9488-text-common::update-display
    ; define update-display

  end-implement
  
end-module