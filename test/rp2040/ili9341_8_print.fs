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

begin-module ili9341-8-print
  
  oo import
  bitmap import
  pixmap8 import
  ili9341-8-spi import
  font import
  simple-font import
  lock import
  task import
  
  begin-module ili9341-8-print-internal
  
    320 constant my-width
    240 constant my-height
    
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
    <ili9341-8-spi> class-size buffer: my-ili9341-8

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

    : dirty-all-ili9341-text ( -- )
      0 dirty-start-col !
      0 dirty-start-row !
      my-chars-width dirty-end-col !
      my-chars-height dirty-end-row !
    ;

    : clear-ili9341-dirty ( -- )
      0 dirty-start-col !
      0 dirty-start-row !
      0 dirty-end-col !
      0 dirty-end-row !
    ;
    
    : dirty-ili9341-char { col row -- }
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
      my-ili9341-8 pixmap8::draw-rect-const
      my-fore-color start-col start-row start-col start-row width height
      my-front my-ili9341-8 pixmap8::draw-rect-const-mask
      my-ili9341-8 update-display
    ;

    : render-ili9341-text ( -- )
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
      clear-ili9341-dirty
      cursor-col @ old-cursor-col !
      cursor-row @ old-cursor-row !
    ;
    
    : do-update-ili9341 ( -- )
      begin
        0 wait-notify drop
        [: render-ili9341-text ;] my-lock with-lock
        62 ms
      again
    ;

    : init-ili9341-text ( -- )
      my-lock init-lock
      [:
        lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
        my-buf my-width my-height my-device <ili9341-8-spi> my-ili9341-8 init-object
        my-front-buf my-width my-height <bitmap> my-front init-object
        my-char-buf my-char-buf-size $20 fill
        0 ['] do-update-ili9341 1024 128 512 spawn to update-task
        update-mailbox 1 update-task config-notify
        0 old-cursor-col !
        0 old-cursor-row !
        0 cursor-col !
        0 cursor-row !
        dirty-all-ili9341-text
        init-simple-font
        0 0 draw-cursor
        copy-display
        true to inited?
        update-task run
      ;] my-lock with-lock
    ;
    
    : signal-ili9341-update ( -- )
      update-task if 0 update-task notify then
    ;
    
    : scroll-ili9341-text ( -- )
      my-char-buf my-chars-width + my-char-buf my-chars-width my-chars-height 1- * move
      my-char-buf my-chars-width my-chars-height 1- * + my-chars-width $20 fill
      cursor-row @ 1- 0 max cursor-row !
      dirty-all-ili9341-text
    ;
    
    : pre-advance-ili9341-cursor ( -- )
      cursor-col @ my-chars-width = if
        0 cursor-col !
        1 cursor-row +!
      then
      cursor-row @ my-chars-height = if
        scroll-ili9341-text
        0 cursor-col !
      then
    ;

    : advance-ili9341-cursor ( -- )
      1 cursor-col +!
    ;
    
    : bs-ili9341-cursor ( -- )
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
    
    : add-ili9341-char { c -- }
      c $0A = if
        cursor-row @ 1+ my-chars-height min cursor-row !
        cursor-row @ my-chars-height = if
          scroll-ili9341-text
        then
      else
        c $0D = if
          0 cursor-col !
        else
          c $08 = if
            bs-ili9341-cursor
          else
            c $20 >= c $7F < and if
              pre-advance-ili9341-cursor
              c my-char-buf my-chars-width cursor-row @ * + cursor-col @ + c!
              cursor-col @ cursor-row @ dirty-ili9341-char
              advance-ili9341-cursor
            then
          then
        then
      then
    ;

  end-module> import
  
  : erase-ili9341 ( -- )
    inited? not if init-ili9341-text then
    [:
      my-char-buf my-chars-width my-chars-height * $20 fill
      0 cursor-col !
      0 cursor-row !
      0 old-cursor-col !
      0 old-cursor-row !
      dirty-all-ili9341-text
      my-front clear-bitmap
      0 0 draw-cursor
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : clear-ili9341 ( -- )
    inited? not if init-ili9341-text then
    [:
      my-char-buf my-chars-width my-chars-height * $20 fill
      0 cursor-col !
      0 cursor-row !
      dirty-all-ili9341-text
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : emit-ili9341 ( c -- )
    inited? not if init-ili9341-text then
    [:
      add-ili9341-char
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : type-ili9341 ( c-addr u -- )
    inited? not if init-ili9341-text then
    [: { c-addr u }
      u 0 ?do c-addr i + c@ add-ili9341-char loop
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : cr-ili9341 ( -- )
    inited? not if init-ili9341-text then
    [:
      cursor-row @ 1+ my-chars-height min cursor-row !
      0 cursor-col !
      cursor-row @ my-chars-height = if
        scroll-ili9341-text
      then
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : bs-ili9341 ( -- )
    inited? not if init-ili9341-text then
    [:
      bs-ili9341-cursor
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : goto-ili9341 ( col row -- )
    inited? not if init-ili9341-text then
    [: { col row }
      col 0 max my-chars-width min cursor-col !
      row 0 max my-chars-height min cursor-row !
      signal-ili9341-update
    ;] my-lock with-lock
  ;
  
  : ili9341-cursor@ ( -- col row ) [: cursor-col @ cursor-row @ ;] my-lock with-lock ;
  
end-module
