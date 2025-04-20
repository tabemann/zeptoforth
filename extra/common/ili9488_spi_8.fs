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

#include extra/common/ili9488_8_common.fs

begin-module ili9488-8-spi

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  spi import
  armv6m import
  ili9488-8-common import

  begin-module ili9488-8-spi-internal

    \ Maximum PIO clock
    25_000_000 constant max-clock

  end-module> import
  
  <ili9488-8-common> begin-class <ili9488-8-spi>

    continue-module ili9488-8-spi-internal

      \ SPI device
      cell member ili9488-8-device
      
      \ Reset pin
      cell member ili9488-8-reset-pin

      \ Reset the ILI9488-8
      method reset-ili9488-8 ( self -- )
      
    end-module

    \ Update the ILI9488-8 device
    method update-display ( self -- )
    
  end-class

  <ili9488-8-spi> begin-implement

    \ Constructor
    :noname
      { din sck dc cs reset buf cols rows device self -- }
      dc cs buf cols rows self <ili9488-8-common>->new
      device self ili9488-8-device !
      reset self ili9488-8-reset-pin !

      reset output-pin
      low reset pin!
      device din spi-pin
      device sck spi-pin
      device master-spi
      max-clock device spi-baud!
      8 device spi-data-size!
      false false device motorola-spi
      device enable-spi

      self reset-ili9488-8
      self ili9488-8-common-internal::init-ili9488-8
      0 cols 0 rows self ili9488-8-common-internal::ili9488-8-window!
    ; define new

    \ Destructor
    :noname { self -- }
      self <ili9488-8-common>->destroy
    ; define destroy
    
    \ Reset the ILI9488-8
    :noname { self -- }
      high self ili9488-8-reset-pin @ pin!
      200 ms
      low self ili9488-8-reset-pin @ pin!
      200 ms
      high self ili9488-8-reset-pin @ pin!
      200 ms
    ; define reset-ili9488-8

    \ Write blocking data
    :noname { addr count self -- }
      addr count self ili9488-8-device @ buffer>spi      
    ; define >ili9488-8

    \ Update the ILI9488 device
    :noname { self -- }
      self ili9488-8-common::update-display
    ; define update-display

  end-implement
  
end-module