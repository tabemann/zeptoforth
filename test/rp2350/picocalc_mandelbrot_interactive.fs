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
\ Up: Pan up
\ Down: Pan down
\ Right: Pan right
\ Left: Pan left
\ 1-9: Set multiplier
\ -: Zoom in
\ =: Zoom out
\ {: Zoom X in
\ }: Zoom X out
\ [: Zoom Y in
\ ]: Zoom Y out
\ R: Reset pan and zoom
\ S: Take a screenshot
\ Q: Give up in shame

begin-module mandelbrot
  
  picocalc-term import
  picocalc-keys import
  picocalc-screenshot import
  pixmap8 import
  font import
  st7365p-8-common import
  float32 import
  
  $B4 constant LEFT_ARROW
  $B5 constant UP_ARROW
  $B6 constant DOWN_ARROW
  $B7 constant RIGHT_ARROW

  -2e0 constant init-xa
  0.47e0 constant init-xb
  -1.12e0 constant init-ya
  1.12e0 constant init-yb
  
  init-xa value xa
  init-xb value xb
  init-ya value ya
  init-yb value yb
 
  : string> { addr bytes buf-addr -- buf-addr' }
    addr buf-addr bytes move bytes buf-addr +
  ;
  
  : v> ( v buf-addr -- buf-addr' )
    base @ { saved-base }
    [: 10 base ! swap v>f64 format-fixed 8 min + ;] try
    saved-base base ! ?raise
  ;
  
  255 255 255 rgb8 constant info-color
  127 127 127 rgb8 constant info-shadow-color
  16 constant info-x
  16 constant info-y

  : draw-info ( -- )
    256 [:
      [: { buf display }
        buf { buf' }
        s" xa: " buf' string> to buf'
        xa buf' v> to buf'
        s"  xb: " buf' string> to buf'
        xb buf' v> to buf'
        s"  ya: " buf' string> to buf'
        ya buf' v> to buf'
        s"  yb: " buf' string> to buf'
        yb buf' v> to buf'
        info-shadow-color buf buf' buf - info-x 1+ info-y 1+
        display term-font@ draw-string-to-pixmap8
        info-color buf buf' buf - info-x info-y display
        term-font@ draw-string-to-pixmap8
      ;] with-term-display
    ;] with-allot
  ;
  
  32 constant max-iteration
  
  max-iteration 1+ cell align buffer: colors
  
  : vrgb { r g b -- rgb }
    r 255e0 v* v>n g 255e0 v* v>n b 255e0 v* v>n rgb8
  ;
  
  : convert-color { v -- color }
    v 5e0 v* to v
    v 1e0 v< if 1e0 v 0e0 vrgb exit then
    v 2e0 v< if 2e0 v v- 1e0 0e0 vrgb exit then
    v 3e0 v< if 0e0 1e0 v 2e0 v- vrgb exit then
    v 4e0 v< if 0e0 4e0 v v- 1e0 vrgb exit then
    v 4e0 v- 0e0 1e0 vrgb
  ;
  
  : init-colors ( -- )
    max-iteration 1+ 0 ?do
      i u>v max-iteration 1- u>v v/ 1e0 v+ vln
      2e0 vln v/ convert-color colors i + c!
    loop
  ;
  
  initializer init-colors
  
  : iteration>color { iteration -- color }
    iteration max-iteration < if
      colors iteration + c@
    else
      0 0 0 rgb8
    then
  ;
  
  : draw ( xa xb ya yb -- )
    [: { xa xb ya yb display }
      display clear-pixmap
      xb xa v- { x-mult }
      yb ya v- { y-mult }
      display dim@ { width height }
      height 0 ?do
        i u>v height u>v v/ y-mult v* ya v+ { y0 }
        width 0 ?do
          i u>v width u>v v/ x-mult v* xa v+ { x0 }
          0e0 0e0 { x y }
          0 { iteration }
          begin
            x dup v* y dup v* v+ 4e0 v<=
            iteration max-iteration < and
          while
            x dup v* y dup v* v- x0 v+ { xtemp }
            x y v* 2e0 v* y0 v+ to y
            xtemp to x
            1 +to iteration
          repeat
          iteration iteration>color
          i height 1- j - display draw-pixel-const
        loop
      loop
      draw-info
      display update-display
    ;] with-term-display
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
  
  1e0 value multiplier
  
  0.0625e0 constant pan-factor
  
  : pan { xf yf -- }
    xb xa v- xf pan-factor multiplier v* v* v* { xr }
    yb ya v- yf pan-factor multiplier v* v* v* { yr }
    xr xa v+ to xa xr xb v+ to xb
    yr ya v+ to ya yr yb v+ to yb
  ;
  
  0.125e0 constant zoom-factor
  
  : zoom { xf yf -- }
    xa xb v+ 2e0 v/ ya yb v+ 2e0 v/ { x y }
    xb xa v- 2e0 v/ dup zoom-factor multiplier v* xf v* v* v+
    yb ya v- 2e0 v/ dup zoom-factor multiplier v* yf v* v* v+
    { xd yd }
    x xd v- to xa x xd v+ to xb
    y yd v- to ya y yd v+ to yb
  ;
  
  : reset ( -- )
    init-xa to xa init-xb to xb init-ya to ya init-yb to yb
  ;
  
  : run-mandelbrot ( -- )
    page
    [: dup clear-pixmap update-display ;] with-term-display
    true raw-keys-enabled!
    true { update }
    clear-keymap
    begin
      false { done }
      update if xa xb ya yb draw false to update then
      update-keymap
      RIGHT_ARROW keymap-pressed@ if
        1e0 0e0 pan true to update
      then
      LEFT_ARROW keymap-pressed@ if
        -1e0 0e0 pan true to update
      then
      UP_ARROW keymap-pressed@ if
        0e0 1e0 pan true to update
      then
      DOWN_ARROW keymap-pressed@ if
        0e0 -1e0 pan true to update
      then
      [char] = keymap-pressed@ [char] + keymap-pressed@ or if
        1e0 1e0 zoom true to update
      then
      [char] - keymap-pressed@ [char] _ keymap-pressed@ or if
        -1e0 -1e0 zoom true to update
      then
      [char] } keymap-pressed@ if
        1e0 0e0 zoom true to update
      then
      [char] { keymap-pressed@ if
        -1e0 0e0 zoom true to update
      then
      [char] ] keymap-pressed@ if
        0e0 1e0 zoom true to update
      then
      [char] [ keymap-pressed@ if
        0e0 -1e0 zoom true to update
      then
      [char] 9 keymap-pressed@ if 9e0 to multiplier then
      [char] 8 keymap-pressed@ if 8e0 to multiplier then
      [char] 7 keymap-pressed@ if 7e0 to multiplier then
      [char] 6 keymap-pressed@ if 6e0 to multiplier then
      [char] 5 keymap-pressed@ if 5e0 to multiplier then
      [char] 4 keymap-pressed@ if 4e0 to multiplier then
      [char] 3 keymap-pressed@ if 3e0 to multiplier then
      [char] 2 keymap-pressed@ if 2e0 to multiplier then
      [char] 1 keymap-pressed@ if 1e0 to multiplier then
      [char] r keymap-pressed@ if reset true to update then
      [char] s keymap-released@ if handle-screenshot then
      [char] q keymap-released@ if true to done then
      reset-keymap
    done until
    false raw-keys-enabled!
    1000 ms
    empty-keys
  ;
  
end-module