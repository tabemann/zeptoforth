\ Copyright (c) 2025 Travis Bemann
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

begin-module picocalc-matrix

  picocalc-term import
  pixmap8 import
  font import
  st7365p-8-common import
  ansi-term import
  tinymt32 import
  picocalc-term-common::picocalc-term-common-internal
  import-from get-color

  \ Get the terminal dimensions
  term-dim@ constant term-height constant term-width

  begin-structure matrix-size

    \ Our PRNG
    tinymt32-size +field prng
    
    \ A character matrix
    term-width term-height * cell align +field chars

    \ Our head rows
    term-width cells +field head-rows

    \ Our column length
    term-width cells +field col-lens
    
  end-structure

  \ Return a head to the top
  : return-head-top { col matrix -- }
    term-height 3 * 2 / s>f matrix prng tinymt32-generate-uint32 0 f* round-zero
    negate col cells matrix head-rows + !
    term-height 3 * 2 / s>f matrix prng tinymt32-generate-uint32 0 f* round-zero
    col cells matrix col-lens + !
  ;

  \ Draw a row
  : draw-row { row matrix display -- }
    -1 { color }
    term-width 0 ?do
      i cells matrix head-rows + @ { head-row }
      i row term-width * + matrix chars + c@ { c }
      black { new-color }
      head-row row >= if
        head-row row = if
          b-white to new-color
          94,0 matrix prng tinymt32-generate-uint32 0 f* round-zero $21 + to c
          c i row term-width * + matrix chars + c!
        else
          i cells matrix col-lens + @ { col-len }
          head-row col-len 2 / - row > if
            head-row col-len - row <= if
              green to new-color
            else
              bl to c
            then
          else
            b-green to new-color
          then
        then
      else
        bl to c
      then
      new-color color <> if
        new-color to color
      then
      color get-color c
      i term-font@ char-cols @ *
      row term-font@ char-rows @ *
      display term-font@ draw-char-to-pixmap8
    loop
  ;

  : advance-matrix { matrix -- }
    term-width 0 ?do
      i cells matrix head-rows + { addr }
      addr @ { row }
      1 +to row
      row addr !
      i cells matrix col-lens + @ { col-len }
      row col-len - term-height 1+ > if
        i matrix return-head-top
      then
    loop
  ;

  : init-matrix { matrix -- }
    rng::random matrix prng tinymt32-init
    matrix prng tinymt32-prepare-example
    matrix chars term-width term-height * bl fill
    term-width 0 ?do i matrix return-head-top loop
  ;
  
  : run-matrix ( -- )
    matrix-size [: { matrix }
      matrix init-matrix
      begin key? not while
        matrix [: { matrix display }
          display clear-pixmap
          term-height 0 ?do
            i matrix display draw-row
          loop
          display update-display
        ;] with-term-display
        matrix advance-matrix
      repeat
      key drop
      none color-effect!
    ;] with-aligned-allot
  ;
  
end-module