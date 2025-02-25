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

#include extra/rp_common/st7789v_8_common.fs

begin-module st7789v-8-spi

  oo import
  pixmap8 import
  pixmap8-internal import
  pin import
  spi import
  armv6m import
  st7789v-8-common import

  begin-module st7789v-8-spi-internal

    \ Maximum PIO clock
    32_000_000 constant max-clock

  end-module> import
  
  <st7789v-8-common> begin-class <st7789v-8-spi>

    continue-module st7789v-8-spi-internal

      \ SPI device
      cell member st7789v-8-device
      
      \ Reset pin
      cell member st7789v-8-reset-pin

      \ Reset the ST7789V-8
      method reset-st7789v-8 ( self -- )
      
    end-module

    \ Set the backlight
    method backlight! ( backlight self -- )
      
    \ Update the ST7789V-8 device
    method update-display ( self -- )
    
  end-class

  <st7789v-8-spi> begin-implement

    \ Constructor
    :noname
      { self }
      { din sck reset dc cs backlight buf round cols rows device }
      dc cs backlight buf round cols rows self <st7789v-8-common>->new
      device self st7789v-8-device !
      reset self st7789v-8-reset-pin !

      reset output-pin
      low reset pin!
      device din spi-pin
      device sck spi-pin
      device master-spi
      max-clock device spi-baud!
      8 device spi-data-size!
      false false device motorola-spi
      device enable-spi

      true self backlight!
      self reset-st7789v-8
      self st7789v-8-common-internal::init-st7789v-8
      0 cols 0 rows self st7789v-8-common-internal::st7789v-8-window!
    ; define new

    \ Destructor
    :noname { self -- }
      self <st7789v-8-common>->destroy
    ; define destroy
    
    \ Reset the ST7789V-8
    :noname { self -- }
      high self st7789v-8-reset-pin @ pin!
      200 ms
      low self st7789v-8-reset-pin @ pin!
      200 ms
      high self st7789v-8-reset-pin @ pin!
      200 ms
    ; define reset-st7789v-8

    \ Set the backlight pin
    :noname { backlight self -- }
      backlight self st7789v-8-common::backlight!
    ; define backlight!

    \ Write blocking data
    :noname { addr count self -- }
      addr count self st7789v-8-device @ buffer>spi      
    ; define >st7789v-8

    \ Update the ST7789V device
    :noname { self -- }
      self st7789v-8-common::update-display
    ; define update-display

  end-implement
  
end-module