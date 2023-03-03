\ Copyright (c) 2023 Travis Bemann
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
\ SOFTWARE

begin-module wio-esp-at

  oo import
  pin import
  gpio import
  spi import
  interrupt import
  closure import
  esp-at import
  timer import

  \ The WIO ESP-AT SPI interface class
  <esp-at-spi> begin-class <wio-esp-at-spi>

    begin-module wio-esp-at-internal

      \ \ Handshake detected?
      \ cell member handshake?

      \ \ Handshake interrupt handler closure
      \ closure-size member handshake-handler

      \ \ Previous IO interrupt handler
      \ cell member prev-io-handler
      
      \ \ Handle handshake
      \ method handle-handshake ( intf -- )
      
    end-module> import
    
  end-class

  continue-module wio-esp-at-internal
    
    \ \ The IO IRQ
    \ 13 constant io-irq
    
    \ \ The IO IRQ vector
    \ io-irq 16 + constant io-vector

    \ SPI peripheral
    1 constant spi-index

    \ MISO pin
    8 constant miso-pin

    \ Chip select pin
    9 constant cs-pin

    \ Clock pin
    10 constant clk-pin

    \ MOSI pin
    11 constant mosi-pin

    \ Handshake pin
    21 constant handshake-pin

  end-module> import

  \ The WIO ESP-AT SPI interface class implementation
  <wio-esp-at-spi> begin-implement

\     \ IO interrupt handler
\     :noname { self -- }
\       self prev-io-handler @ ?execute
\       handshake-pin PROC0_INTS_GPIO_LEVEL_HIGH@
\       handshake-pin PROC1_INTS_GPIO_LEVEL_HIGH@ or
\       if
\         true self handshake? !
\         false handshake-pin PROC0_INTE_GPIO_LEVEL_HIGH!
\         false handshake-pin PROC1_INTE_GPIO_LEVEL_HIGH!
\       then
\ \      handshake-pin INTR_GPIO_EDGE_HIGH!
\ \      [char] * internal::serial-emit
\     ; define handle-handshake
    
    \ Constructor
    :noname { self -- }
      \ io-vector vector@ self prev-io-handler !
      \ self self handshake-handler ['] handle-handshake bind
      \ self handshake-handler io-vector vector!
      false false spi-index motorola-spi
      spi-index master-spi
      20000000 spi-index spi-baud!
      8 spi-index spi-data-size!
      cs-pin spi-index self <esp-at-spi>->new
      spi-index miso-pin spi-pin
      spi-index clk-pin spi-pin
      spi-index mosi-pin spi-pin
      cs-pin output-pin
      spi-index enable-spi      
    ; define new

    \ Power on an ESP-AT device
    :noname { self -- }
      \ false self handshake? !
      handshake-pin output-pin
      22 output-pin
      24 output-pin
      25 output-pin
      high handshake-pin pin!
      high 24 pin!
      high 22 pin!
      low cs-pin pin!
      low 25 pin!
      handshake-pin input-pin
      handshake-pin pull-down-pin
      high cs-pin pin!
      true handshake-pin PADS_BANK0_SCHMITT!
      \ cpu-index 0= if
      \   true handshake-pin PROC0_INTE_GPIO_LEVEL_HIGH!
      \ else
      \   true handshake-pin PROC1_INTE_GPIO_LEVEL_HIGH!
      \ then
      \ 0 io-irq NVIC_IPR_IP!
      \ io-irq NVIC_ISER_SETENA!
    ; define power-esp-at-on

    \ Power off an ESP-AT device
    :noname { self -- }
      \ io-irq NVIC_ICER_CLRENA!
      \ false handshake-pin PROC0_INTE_GPIO_LEVEL_HIGH!
      \ false handshake-pin PROC1_INTE_GPIO_LEVEL_HIGH!
      25 output-pin
      handshake-pin output-pin
      22 output-pin
      24 output-pin
      high 25 pin!
      low handshake-pin pin!
      low 24 pin!
      low 22 pin!
    ; define power-esp-at-off

    \ Get whether the ESP-AT device is ready
    :noname { self -- }
      handshake-pin pin@
      \ self handshake? @
      \ false self handshake? !
      \ cpu-index 0= if
      \   true handshake-pin PROC0_INTE_GPIO_LEVEL_HIGH!
      \ else
      \   true handshake-pin PROC1_INTE_GPIO_LEVEL_HIGH!
      \ then
    ; define esp-at-ready?

  end-implement
  
end-module