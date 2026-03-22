\ Copyright (c) 2022-2026 Travis Bemann
\ Copyright (c) 2026 Ken Mitton
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

begin-module st7796s-8-print
  
  oo import
  bitmap import
  pixmap8 import
  st7796s-8-spi import
  font import
  simple-font import
  lock import
  task import
  
  begin-module st7796s-8-print-internal
  
    480 constant my-width
    320 constant my-height
    
    7 constant my-char-width
    8 constant my-char-height
    
    \ SPI device
    1 constant my-device

    \ Pins
    11 constant lcd-din
    10 constant lcd-clk \ aka lcd-sck
    14 constant lcd-dc
    15 constant lcd-rst
    13 constant lcd-cs
    12 constant lcd-bl

    my-width my-height pixmap8-buf-size constant my-buf-size
    my-buf-size 4 align buffer: my-buf
    <st7796s-8-spi> class-size buffer: my-st7796s-8

    my-width my-height bitmap-buf-size constant my-front-buf-size
    my-front-buf-size 4 align buffer: my-front-buf
    <bitmap> class-size buffer: my-front
    
    my-width my-char-width / constant my-chars-width
    my-height my-char-height / constant my-chars-height
    
    my-chars-width my-chars-height * constant my-char-buf-size
    my-char-buf-size 4 align buffer: my-char-buf

    0 value update-task
    variable update-mailbox
    
    variable old-cursor-col
    variable old-cursor-row
    variable cursor-col
    variable cursor-row
    variable dirty-start-col
    variable dirty-start-row
    variable dirty-end-col
    variable dirty-end-row
    
    lock-size buffer: my-lock

    \ Our colors
    0 255 0 rgb8 constant my-fore-color
    \ TODO (mittonk): back to  0 0 0 rgb8 constant my-back-color
    0 0 64 rgb8 constant my-back-color
    
    false value inited?

    : dirty-all-st7796s-text ( -- )
      0 dirty-start-col !
      0 dirty-start-row !
      my-chars-width dirty-end-col !
      my-chars-height dirty-end-row !
    ;

    : clear-st7796s-dirty ( -- )
      0 dirty-start-col !
      0 dirty-start-row !
      0 dirty-end-col !
      0 dirty-end-row !
    ;
    
    : dirty-st7796s-char { col row -- }
      dirty-start-col @ col min dirty-start-col !
      dirty-start-row @ row min dirty-start-row !
      dirty-end-col @ col 1+ max dirty-end-col !
      dirty-end-row @ row 1+ max dirty-end-row !
    ;
    
    : draw-cursor { col row -- }
      $FF col my-char-width * row my-char-height * my-char-width my-char-height op-xor my-front bitmap::draw-rect-const
    ;

    : dirty-rect@ ( -- x y width height)
      dirty-start-col @ cursor-col @ min old-cursor-col @ min { start-col }
      dirty-start-row @ cursor-row @ min old-cursor-row @ min { start-row }
      dirty-end-col @ cursor-col @ 1+ max old-cursor-col @ 1+ max { end-col }
      dirty-end-row @ cursor-row @ 1+ max old-cursor-row @ 1+ max { end-row }
      start-col my-char-width *
      start-row my-char-height *
      end-col start-col - my-char-width *
      end-row start-row - my-char-height *
    ;

    : copy-display ( -- )
      dirty-rect@ { start-col start-row width height }
      my-back-color start-col start-row width height
      my-st7796s-8 pixmap8::draw-rect-const
      my-fore-color start-col start-row start-col start-row width height
      my-front my-st7796s-8 pixmap8::draw-rect-const-mask
      my-st7796s-8 update-display
    ;

    : render-st7796s-text ( -- )
      old-cursor-col @ old-cursor-row @ draw-cursor
      dirty-end-row @ dirty-start-row @ ?do
        dirty-end-col @ dirty-start-col @ ?do 
          my-char-buf my-chars-width j * + i + c@
          my-char-width i * my-char-height j *
          op-set my-front a-simple-font draw-char
        loop
      loop
      cursor-col @ cursor-row @ draw-cursor
      copy-display
      clear-st7796s-dirty
      cursor-col @ old-cursor-col !
      cursor-row @ old-cursor-row !
    ;
    
    : do-update-st7796s ( -- )
      begin
        0 wait-notify drop
        [: render-st7796s-text ;] my-lock with-lock
        62 ms
      again
    ;

    : init-st7796s-text ( -- )
      my-lock init-lock
      [:
        lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
        my-buf my-width my-height my-device <st7796s-8-spi> my-st7796s-8 init-object
        my-front-buf my-width my-height <bitmap> my-front init-object
        my-char-buf my-char-buf-size $20 fill
        0 ['] do-update-st7796s 1024 128 512 spawn to update-task
        update-mailbox 1 update-task config-notify
        0 old-cursor-col !
        0 old-cursor-row !
        0 cursor-col !
        0 cursor-row !
        dirty-all-st7796s-text
        init-simple-font
        0 0 draw-cursor
        copy-display
        true to inited?
        update-task run
      ;] my-lock with-lock
    ;
    
    : signal-st7796s-update ( -- )
      update-task if 0 update-task notify then
    ;
    
    : scroll-st7796s-text ( -- )
      my-char-buf my-chars-width + my-char-buf my-chars-width my-chars-height 1- * move
      my-char-buf my-chars-width my-chars-height 1- * + my-chars-width $20 fill
      cursor-row @ 1- 0 max cursor-row !
      dirty-all-st7796s-text
    ;
    
    : pre-advance-st7796s-cursor ( -- )
      cursor-col @ my-chars-width = if
        0 cursor-col !
        1 cursor-row +!
      then
      cursor-row @ my-chars-height = if
        scroll-st7796s-text
        0 cursor-col !
      then
    ;

    : advance-st7796s-cursor ( -- )
      1 cursor-col +!
    ;
    
    : bs-st7796s-cursor ( -- )
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
    
    : add-st7796s-char { c -- }
      c $0A = if
        cursor-row @ 1+ my-chars-height min cursor-row !
        cursor-row @ my-chars-height = if
          scroll-st7796s-text
        then
      else
        c $0D = if
          0 cursor-col !
        else
          c $08 = if
            bs-st7796s-cursor
          else
            c $20 >= c $7F < and if
              pre-advance-st7796s-cursor
              c my-char-buf my-chars-width cursor-row @ * + cursor-col @ + c!
              cursor-col @ cursor-row @ dirty-st7796s-char
              advance-st7796s-cursor
            then
          then
        then
      then
    ;

  end-module> import
  
  : erase-st7796s ( -- )
    inited? not if init-st7796s-text then
    [:
      my-char-buf my-chars-width my-chars-height * $20 fill
      0 cursor-col !
      0 cursor-row !
      0 old-cursor-col !
      0 old-cursor-row !
      dirty-all-st7796s-text
      my-front clear-bitmap
      0 0 draw-cursor
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : clear-st7796s ( -- )
    inited? not if init-st7796s-text then
    [:
      my-char-buf my-chars-width my-chars-height * $20 fill
      0 cursor-col !
      0 cursor-row !
      dirty-all-st7796s-text
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : emit-st7796s ( c -- )
    inited? not if init-st7796s-text then
    [:
      add-st7796s-char
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : type-st7796s ( c-addr u -- )
    inited? not if init-st7796s-text then
    [: { c-addr u }
      u 0 ?do c-addr i + c@ add-st7796s-char loop
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : cr-st7796s ( -- )
    inited? not if init-st7796s-text then
    [:
      cursor-row @ 1+ my-chars-height min cursor-row !
      0 cursor-col !
      cursor-row @ my-chars-height = if
        scroll-st7796s-text
      then
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : bs-st7796s ( -- )
    inited? not if init-st7796s-text then
    [:
      bs-st7796s-cursor
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : goto-st7796s ( col row -- )
    inited? not if init-st7796s-text then
    [: { col row }
      col 0 max my-chars-width min cursor-col !
      row 0 max my-chars-height min cursor-row !
      signal-st7796s-update
    ;] my-lock with-lock
  ;
  
  : st7796s-cursor@ ( -- col row ) [: cursor-col @ cursor-row @ ;] my-lock with-lock ;
  
end-module
