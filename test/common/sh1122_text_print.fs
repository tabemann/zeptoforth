\ Copyright (c) 2022-2024 Travis Bemann
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

begin-module sh1122-print
  
  oo import
  text-display import
  sh1122-text import
  simple-font import
  lock import
  task import
  
  begin-module sh1122-print-internal
  
    256 constant my-width
    64 constant my-height
    
    7 constant my-char-width
    8 constant my-char-height
    
    \ SPI device
    0 constant my-device
    
    \ Pins
    3 constant lcd-din 
    2 constant lcd-clk
    7 constant lcd-dc
    6 constant lcd-rst
    5 constant lcd-cs
    
    my-width my-height my-char-width my-char-height text-buf-size
    buffer: my-text-buf
    my-width my-height my-char-width my-char-height invert-buf-size
    buffer: my-invert-buf
    <sh1122-text> class-size buffer: my-sh1122
    
    my-width my-char-width / constant my-chars-width
    my-height my-char-height / constant my-chars-height

    \ My foreground color
    15 constant my-fore-color

    \ my background color
    0 constant my-back-color
    
    0 value update-task
    variable update-mailbox
    
    variable old-cursor-col
    variable old-cursor-row
    variable cursor-col
    variable cursor-row
    
    lock-size buffer: my-lock
    
    false value inited?

    : draw-cursor { col row -- }
      col row my-sh1122 toggle-invert!
    ;

    : render-sh1122-text ( -- )
      old-cursor-col @ old-cursor-row @ draw-cursor
      cursor-col @ cursor-row @ draw-cursor
      my-sh1122 update-display
      cursor-col @ old-cursor-col !
      cursor-row @ old-cursor-row !
    ;
    
    : do-update-sh1122 ( -- )
      begin
        0 wait-notify drop
        [: render-sh1122-text ;] my-lock with-lock
        62 ms
      again
    ;

    : init-sh1122-text ( -- )
      my-lock init-lock
      [:
        init-simple-font
        my-fore-color my-back-color lcd-din lcd-clk lcd-dc lcd-cs lcd-rst
        my-text-buf my-invert-buf a-simple-font my-width my-height
        my-device <sh1122-text> my-sh1122 init-object
        0 ['] do-update-sh1122 1024 128 512 spawn to update-task
        update-mailbox 1 update-task config-notify
        0 old-cursor-col !
        0 old-cursor-row !
        0 cursor-col !
        0 cursor-row !
        0 0 draw-cursor
        true to inited?
        update-task run
      ;] my-lock with-lock
    ;
    
    : signal-sh1122-update ( -- )
      update-task if 0 update-task notify then
    ;
    
    : scroll-sh1122-text ( -- )
      my-chars-height 1 ?do
        my-chars-width 0 ?do
          i j my-sh1122 char@ i j 1- my-sh1122 char!
        loop
      loop
      my-chars-width 0 ?do
        $20 i my-chars-height 1- my-sh1122 char!
      loop
      cursor-row @ 1- 0 max cursor-row !
    ;
    
    : pre-advance-sh1122-cursor ( -- )
      cursor-col @ my-chars-width = if
        0 cursor-col !
        1 cursor-row +!
      then
      cursor-row @ my-chars-height = if
        scroll-sh1122-text
        0 cursor-col !
      then
    ;

    : advance-sh1122-cursor ( -- )
      1 cursor-col +!
    ;
    
    : bs-sh1122-cursor ( -- )
      -1 cursor-col +!
      cursor-col @ 0< if
        my-chars-width 1- cursor-col !
        cursor-row @ 0> if
          -1 cursor-row +!
        else
          0 cursor-col !
        then
      then
    ;
    
    : add-sh1122-char { c -- }
      c $0A = if
        cursor-row @ 1+ my-chars-height min cursor-row !
        cursor-row @ my-chars-height = if
          scroll-sh1122-text
        then
      else
        c $0D = if
          0 cursor-col !
        else
          c $08 = if
            bs-sh1122-cursor
          else
            c $20 >= c $7F < and if
              pre-advance-sh1122-cursor
              c cursor-col @ cursor-row @ my-sh1122 char!
              advance-sh1122-cursor
            then
          then
        then
      then
    ;
    
  end-module> import
  
  : clear-sh1122 ( -- )
    inited? not if init-sh1122-text then
    [:
      my-chars-height 0 ?do
        my-chars-width 0 ?do
          $20 i j 1- my-sh1122 char!
        loop
      loop
      0 cursor-col !
      0 cursor-row !
      signal-sh1122-update
    ;] my-lock with-lock
  ;
  
  : emit-sh1122 ( c -- )
    inited? not if init-sh1122-text then
    [:
      add-sh1122-char
      signal-sh1122-update
    ;] my-lock with-lock
  ;
  
  : type-sh1122 ( c-addr u -- )
    inited? not if init-sh1122-text then
    [: { c-addr u }
      u 0 ?do c-addr i + c@ add-sh1122-char loop
      signal-sh1122-update
    ;] my-lock with-lock
  ;
  
  : cr-sh1122 ( -- )
    inited? not if init-sh1122-text then
    [:
      cursor-row @ 1+ my-chars-height min cursor-row !
      0 cursor-col !
      cursor-row @ my-chars-height = if
        scroll-sh1122-text
      then
      signal-sh1122-update
    ;] my-lock with-lock
  ;
  
  : bs-sh1122 ( -- )
    inited? not if init-sh1122-text then
    [:
      bs-sh1122-cursor
      signal-sh1122-update
    ;] my-lock with-lock
  ;
  
  : goto-sh1122 ( col row -- )
    inited? not if init-sh1122-text then
    [: { col row }
      col 0 max my-chars-width min cursor-col !
      row 0 max my-chars-height min cursor-row !
      signal-sh1122-update
    ;] my-lock with-lock
  ;
  
  : sh1122-cursor@ ( -- col row )
    [: cursor-col @ cursor-row @ ;] my-lock with-lock
  ;
  
end-module