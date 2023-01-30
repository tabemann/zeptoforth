\ Copyright (c) 2020-2023 Travis Bemann
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

begin-module neopixel
  
  pin import
  pio import
  pio-registers import

  \ Out of range Neopixel exception
  : x-out-of-range-neopixel ( -- ) ." out of range Neopixel" cr ;

  \ Out of range color
  : x-out-of-range-color ( -- ) ." out of range color" cr ;
  
  begin-module neopixel-internal
    
    \ The Neopixel structure
    begin-structure neopixel-header-size

      \ The Neopixel count
      field: neopixel-count
      
      \ The PIO to use
      field: neopixel-pio

      \ The state machine to use
      field: neopixel-sm
      
      \ The LED pin
      field: neopixel-pin
      
    end-structure

    \ Neopixel program
    create neopixel-program
    1 %00000 2 or OUT_X out+,
    3 %10000 1 or COND_X0= jmp+,
    0 %10000 4 or COND_ALWAYS jmp+,
    MOV_SRC_Y %00000 4 or MOV_OP_NONE MOV_DEST_Y mov+,

    \ Send a Neopixel color
    : send-neopixel { grb addr -- }
      begin addr neopixel-sm @ addr neopixel-pio @ sm-tx-fifo-level@ 0= until
      grb addr neopixel-sm @ addr neopixel-pio @ sm-txf!
    ;
  
  end-module> import

  \ Get the size of a strip of Neopixels
  : neopixel-size ( count -- bytes ) cells neopixel-header-size + ;

  \ Get the Neopixel count
  : neopixel-count@ ( addr -- count ) neopixel-count @ ;
  
  \ Set the color of a Neopixel
  : neopixel! { r g b index addr -- }
    index addr neopixel-count @ u< averts x-out-of-range-neopixel
    r 256 u< averts x-out-of-range-color
    g 256 u< averts x-out-of-range-color
    b 256 u< averts x-out-of-range-color
    r $FF and 8 lshift
    g $FF and 16 lshift or
    b $FF and or 8 lshift
    addr neopixel-header-size + index cells + !
  ;

  \ Get the color of a Neopixel
  : neopixel@ { index addr -- r g b }
    index addr neopixel-count @ u< averts x-out-of-range-neopixel
    addr neopixel-header-size + index cells + @ 8 rshift { grb }
    grb 8 rshift $FF and
    grb 16 rshift $FF and
    grb $FF and
  ;

  \ Update the Neopixel strip
  : update-neopixel { addr -- }
    addr neopixel-count @ 0 ?do
      addr neopixel-header-size + i cells + @ addr send-neopixel
    loop
  ;

  \ Clear the Neopixel strip
  : clear-neopixel { addr -- }
    addr neopixel-count @ 0 ?do
      0 addr neopixel-header-size + i cells + !
    loop
  ;
  
  \ Initialize a Neopixel strip
  : init-neopixel { init-sm init-pio init-count init-pin addr -- }
    init-pin pio-internal::validate-pin
    init-pio pio-internal::validate-pio
    init-sm pio-internal::validate-sm
    init-pin addr neopixel-pin !
    init-count addr neopixel-count !
    init-pio addr neopixel-pio !
    init-sm addr neopixel-sm !
    addr clear-neopixel
    init-sm bit init-pio sm-disable
    init-sm bit init-pio sm-restart
    160 15 init-sm init-pio sm-clkdiv! \ 8000000 Hz
    left init-sm init-pio sm-out-shift-dir
    on init-sm init-pio sm-autopull!
    24 init-sm init-pio sm-pull-threshold!
    off init-pin init-sm init-pio sm-pin!
    out init-pin init-sm init-pio sm-pindir!
    init-pin 1 init-sm init-pio sm-sideset-pins!
    off init-sm init-pio sm-sideset-high-enable!
    off init-sm init-pio sm-sideset-pindir!
    0 3 init-sm init-pio sm-wrap!
    neopixel-program 4 0 init-pio pio-instr-relocate-mem!
    0 init-sm init-pio sm-addr!
    init-sm bit init-pio sm-enable
  ;

end-module
