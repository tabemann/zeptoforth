\ Copyright (c) 2020-2025 Travis Bemann
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

begin-module pico-w-cyw43-net-ipv4

  oo import
  simple-cyw43-net-ipv4 import

  begin-module pico-w-cyw43-net-ipv4-internal

    \ The pins used for the CYW43 interface
    23 constant pwr-pin
    24 constant dio-pin
    25 constant cs-pin
    29 constant clk-pin
    
  end-module> import

  \ A CYW43439 networking and interface class for the Pico W
  <simple-cyw43-net-ipv4> begin-class <pico-w-cyw43-net-ipv4>

    continue-module pico-w-cyw43-net-ipv4-internal

      \ LED state
      cell member pico-w-led-state
      
    end-module
    
    \ Initialize the CYW43439 network and interface object
    method init-cyw43-net ( self -- )

    \ Initialize the CYW43439 network and interface object without starting
    \ the endpoint process
    method init-cyw43-net-no-handler ( self -- )

    \ Set the state of the Pico W's LED
    method pico-w-led! ( state self -- ) 

    \ Get the state of the Pico W's LED
    method pico-w-led@ ( self -- state ) 

    \ Toggle the Pico W's LED
    method toggle-pico-w-led ( self -- ) 

  end-class

  \ Implement an IPv4 CYW43493 networking and interface class for the Pico W
  <pico-w-cyw43-net-ipv4> begin-implement

    \ Constructor, using a PIO state machine index, and PIO instance
    :noname { sm-index pio-instance self -- }
      pwr-pin dio-pin cs-pin clk-pin sm-index pio-instance
      self <simple-cyw43-net-ipv4>->new
      false self pico-w-led-state !
    ; define new

    \ Initialize the CYW43439 network and interface object
    :noname { self -- }
      self <simple-cyw43-net-ipv4>->init-cyw43-net
      self pico-w-led-state @ self pico-w-led!
    ; define init-cyw43-net

    \ Initialize the CYW43439 network and interface object without starting
    \ the endpoint process
    :noname { self -- }
      self <simple-cyw43-net-ipv4>->init-cyw43-net-no-handler
      self pico-w-led-state @ self pico-w-led!
    ; define init-cyw43-net-no-handler

    \ Set the state of the Pico W's LED
    :noname { state self -- }
      state 0 self <simple-cyw43-net-ipv4>->cyw43-gpio!
      state self pico-w-led-state !
    ; define pico-w-led!

    \ Get the state of the Pico W's LED
    :noname { self -- state }
      self pico-w-led-state @
    ; define pico-w-led@

    \ Toggle the Pico W's LED
    :noname { self -- }
      self pico-w-led-state @ not self pico-w-led!
    ; define toggle-pico-w-led

    \ Set a GPIO on the CYW43439
    :noname { state gpio self -- }
      gpio 0= if
        state self pico-w-led!
      else
        state gpio self <simple-cyw43-net-ipv4>->cyw43-gpio!
      then
    ; define cyw43-gpio!
    
  end-implement

end-module
