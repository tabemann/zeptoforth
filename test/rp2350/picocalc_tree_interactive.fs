\ Copyright (c) 2025-2026 Travis Bemann
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

\ The controls are:
\ 
\ Up: Increase base branch length
\ Down: Decrease base branch length
\ Right: Increase branch angle
\ Left: Decrease branch angle
\ =: Increase recursion depth
\ -: Decrease recursion depth
\ +: Increase inherited branch length proportion
\ _: Decrease inherited branch length proportion
\ S: Take a screenshot
\ Q: Exit

begin-module tree

  turtle import
  pixmap8 import
  font import
  picocalc-term import
  picocalc-keys import
  picocalc-screenshot import
  float32 import
  
  $B4 constant LEFT_ARROW
  $B5 constant UP_ARROW
  $B6 constant DOWN_ARROW
  $B7 constant RIGHT_ARROW
  
  5 constant len-inc
  1e0 48e0 v/ constant len-fract-inc
  2.5e0 constant angle-inc
  1 constant level-inc
  
  10 value level
  100 value len
  2e0 3e0 v/ value len-fract
  45e0 value angle
  
  : string> { addr bytes buf-addr -- buf-addr' }
    addr buf-addr bytes move bytes buf-addr +
  ;
  
  : n> ( n buf-addr -- buf-addr' )
    base @ { saved-base }
    [: 10 base ! swap format-integer + ;] try
    saved-base base ! ?raise
  ;
  
  : v> ( v buf-addr -- buf-addr' )
    base @ { saved-base }
    [: 10 base ! swap v>f64 format-fixed 6 min + ;] try
    saved-base base ! ?raise
  ;
  
  255 255 255 rgb8 constant info-color
  16 constant info-x
  16 constant info-y

  : draw-info ( -- )
    256 [:
      [: { buf display }
        buf { buf' }
        s" Level: " buf' string> to buf'
        level buf' n> to buf'
        s"  Len: " buf' string> to buf'
        len buf' n> to buf'
        s"  Angle: " buf' string> to buf'
        angle v>n buf' n> to buf'
        s"  Len-fract: " buf' string> to buf'
        len-fract buf' v> to buf'
        info-color buf buf' buf - info-x info-y display
        term-font@ draw-string-to-pixmap8
      ;] with-term-display
    ;] with-allot
  ;
  
  defer draw-tree-branch
  :noname { len level -- }
    angle v>n { angle }
    len n>v len-fract v* v>n to len
    len forward
    level 0> len 1 > and if
      angle left
      len level 1- draw-tree-branch
      angle 2 * right
      len level 1- draw-tree-branch
      angle left
    then
    penup len backward pendown
  ; is draw-tree-branch
  
  : draw-tree ( -- )
    updateoff
    penup clear 0 -160 setxy 0 setheading pendown
    len level draw-tree-branch draw-info
    updateon
  ;
  
  : empty-keys ( -- ) begin key? while key drop repeat ;

  : handle-screenshot ( -- )
    [:
      screenshot-fs@ { fs }
      fs if
        screenshot-path@ fs ['] take-screenshot try-and-display-error 0<> if
          drop 2drop
        then
      then
    ;] console::with-serial-error-output
  ;
  
  : run-tree ( -- )
    page
    true raw-keys-enabled!
    true { update }
    clear-keymap
    hideturtle
    begin
      false { done }
      update if draw-tree false to update then
      update-keymap
      RIGHT_ARROW keymap-pressed@ if
        angle angle-inc v+ to angle true to update
      then
      LEFT_ARROW keymap-pressed@ if
        angle angle-inc v- to angle true to update
      then
      UP_ARROW keymap-pressed@ if
        len-inc +to len true to update
      then
      DOWN_ARROW keymap-pressed@ if
        len-inc negate +to len true to update
      then
      [char] = keymap-pressed@ if
        level level-inc + 15 min to level true to update
      then
      [char] - keymap-pressed@ if
        level level-inc - 0 max to level true to update
      then
      [char] + keymap-pressed@ if
        len-fract len-fract-inc v+ to len-fract true to update
      then
      [char] _ keymap-pressed@ if
        len-fract len-fract-inc v- to len-fract true to update
      then
      [char] s keymap-released@ if handle-screenshot then
      [char] q keymap-released@ if true to done then
      reset-keymap
    done until
    false raw-keys-enabled!
    1000 ms
    empty-keys
    showturtle
  ;
  
end-module