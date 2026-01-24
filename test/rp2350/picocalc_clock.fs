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

begin-module picocalc-clock

  picocalc-term import
  pixmap8 import
  pixmap8-utils import
  font import
  st7365p-8-common import
  float32 import
  rtc import
  
  0 value tz-hour
  0 value tz-minute
  
  120e0 constant rim-radius
  110e0 constant mark-radius
  105e0 constant seconds-len
  100e0 constant minutes-len
  66e0 constant hours-len
  0.5e0 constant time-x
  0.75e0 constant time-y
  
  255 0 0 rgb8 constant rim-color
  0 0 255 rgb8 constant mark-color
  0 255 0 rgb8 constant seconds-color
  255 255 255 rgb8 constant minutes-color
  255 255 0 rgb8 constant hours-color
  0 255 255 rgb8 constant time-color
  
  : clock-hand { color len fract display -- }
    term-pixels-dim@ { width height }
    width 2 / height 2 / { width2/ height2/ }
    vpi 2e0 v/ vpi 2e0 v* fract v* v- { angle }
    width2/ angle vcos len v* v>n + { x }
    height2/ angle vsin len v* v>n - { y }
    color width2/ height2/ x y display draw-pixel-line
  ;
  
  : mark { fract display -- }
    term-pixels-dim@ { width height }
    width 2 / height 2 / { width2/ height2/ }
    vpi 2e0 v/ vpi 2e0 v* fract v* v- { angle }
    angle vcos angle vsin { cos sin }
    width2/ cos rim-radius v* v>n + { x }
    height2/ sin rim-radius v* v>n - { y }
    width2/ cos mark-radius v* v>n + { x' }
    height2/ sin mark-radius v* v>n - { y' }
    mark-color x y x' y' display draw-pixel-line
  ;
  
  : marks { display -- }
    12 0 do i n>v 12e0 v/ display mark loop
  ;
  
  : rim { display -- }
    term-pixels-dim@ { width height }
    width 2 / height 2 / { width2/ height2/ }
    rim-color width2/ height2/ rim-radius v>n
    display draw-pixel-circle
  ;
  
  : text { color buf bytes x y display -- }
    term-font@ { font }
    term-pixels-dim@ { width height }
    width n>v x v* v>n font char-cols @ bytes * 2 / - { x' }
    height n>v y v* v>n font char-rows @ 2 / - { y' }
    color buf bytes x' y' display font draw-string-to-pixmap8
  ;
  
  : adjust-tz { date-time -- }
    date-time date-time-minute c@ { minute }
    minute tz-minute + { minute' }
    minute' 60 mod begin dup 0< while 60 + repeat
    date-time date-time-minute c!
    date-time date-time-hour c@ { hour }
    hour tz-hour + { hour' }
    minute' 0< if
      minute' 60 / 1- +to hour'
    else
      minute' 60 / +to hour'
    then
    hour' 24 mod begin dup 0< while 24 + repeat
    date-time date-time-hour c!
  ;
  
  : time ( -- hours minutes seconds )
    date-time-size [: { date-time }
      date-time date-time@
      date-time adjust-tz
      date-time date-time-hour c@ n>v { hour }
      date-time date-time-minute c@ n>v { minute }
      date-time date-time-second c@ n>v { second }
      date-time date-time-msec h@ n>v { msec }
      hour minute 60e0 v/ v+ second 3600e0 v/ v+
      msec 3600000e0 v/ v+
      minute second 60e0 v/ v+ msec 60000e0 v/ v+
      second msec 1000e0 v/ v+
    ;] with-aligned-allot
  ;
    
  : format-time ( buf -- )
    date-time-size [: { buf date-time }
      date-time date-time@
      date-time adjust-tz
      s" 00:00:00" buf swap move
      date-time date-time-hour c@ s>d <# # # #>
      buf 0 + swap move
      date-time date-time-minute c@ s>d <# # # #>
      buf 3 + swap move
      date-time date-time-second c@ s>d <# # # #>
      buf 6 + swap move
    ;] with-aligned-allot
  ;
  
  : display-time ( display -- )
    8 [: { display buf -- }
      buf format-time
      time-color buf 8 time-x time-y display text
    ;] with-aligned-allot
  ;
  
  : tz! ( hours minutes -- ) to tz-minute to tz-hour ;
  
  : run-clock ( -- )
    page
    begin key? not while
      [: { display }
        display clear-pixmap
        time { hours minutes seconds }
        display rim
        display marks
        display display-time
        hours-color hours-len hours 12e0 v/ display clock-hand
        minutes-color minutes-len minutes 60e0 v/
        display clock-hand
         seconds-color seconds-len seconds 60e0 v/
         display clock-hand
        display update-display
      ;] with-term-display
    repeat
    key drop
  ;
  
end-module
      