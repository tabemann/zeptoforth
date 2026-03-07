\ Copyright (c) 2026 Travis Bemann
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

begin-module stars
  
  oo import
  picocalc-term import
  bitmap import
  bitmap-lit import
  pixmap8 import
  st7365p-8-common import
  tinymt32 import
  
  31 constant star-width
  31 constant star-height
  
  star-width star-height begin-bitmap-lit star-buf
    
    !!                #
    !!               ###
    !!              #####
    !!              #####
    !!             #######
    !!            #########
    !!            #########
    !!           ###########
    !! ###############################
    !!  #############################
    !!  #############################
    !!   ###########################
    !!    #########################
    !!    #########################
    !!     #######################
    !!      #####################
    !!     #######################
    !!    #########################
    !!    #########################
    !!   ###########################
    !!  #############################
    !!  #############################
    !! ###############################
    !!           ###########
    !!            #########
    !!            #########
    !!             #######
    !!              #####
    !!              #####
    !!               ###
    !!                #
    
  end-bitmap-lit
  
  : prnd { range prng -- n }
    range s>f prng tinymt32-generate-uint32 0 f* round-zero
  ;
  
  : prnd-coord { prng -- x y }
    term-pixels-dim@ { width height }
    width prng prnd [ star-width 2 / ] literal -
    height prng prnd [ star-height 2 / ] literal -
  ;
  
  : prnd-color { prng -- color }
    prng tinymt32-generate-uint32 { color }
    color $FF and
    color 8 rshift $FF and
    color 16 rshift $FF and
    rgb8
  ;
  
  : draw-stars ( -- )
    star-buf star-width star-height <bitmap-no-clear> [:
      tinymt32-size [: { star prng }
        rng::random prng tinymt32-init
        prng tinymt32-prepare-example
        begin key? not while
          prng prnd-color 0 0 prng prnd-coord
          star-width star-height star
          [: { display }
            display draw-rect-const-mask
            display update-display
          ;] with-term-display
        repeat
      ;] with-aligned-allot
    ;] with-object
  ;
  
end-module
