\ Copyright (c) 2022 Travis Bemann
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

begin-module ssd1306-print
  
  oo import
  bitmap import
  ssd1306 import
  font import
  simple-font import
  lock import
  
  begin-module ssd1306-print-internal
  
    128 constant my-width
    64 constant my-height
    
    7 constant my-char-width
    8 constant my-char-height
    
    my-width my-height bitmap-buf-size constant my-buf-size
    my-buf-size 4 align buffer: my-buf
    <ssd1306> class-size buffer: my-ssd1306
    
    my-width my-char-width / constant my-chars-width
    my-height my-char-height / constant my-chars-height
    
    my-chars-width my-chars-height * constant my-char-buf-size
    my-char-buf-size 4 align buffer: my-char-buf
    
    variable old-cursor-col
    variable old-cursor-row
    variable cursor-col
    variable cursor-row
    variable dirty-start-col
    variable dirty-start-row
    variable dirty-end-col
    variable dirty-end-row
    
    lock-size buffer: my-lock
    
    false value inited?

    : dirty-all-ssd1306-text ( -- )
      0 dirty-start-col !
      0 dirty-start-row !
      my-chars-width dirty-end-col !
      my-chars-height dirty-end-row !
    ;

    : clear-ssd1306-dirty ( -- )
      0 dirty-start-col !
      0 dirty-start-row !
      0 dirty-end-col !
      0 dirty-end-row !
    ;
    
    : dirty-ssd1306-char { col row -- }
      dirty-start-col @ col min dirty-start-col !
      dirty-start-row @ row min dirty-start-row !
      dirty-end-col @ col 1+ max dirty-end-col !
      dirty-end-row @ row 1+ max dirty-end-row !
    ;
    
    : draw-cursor { col row -- }
      $FF col my-char-width * my-char-width row my-char-height * my-char-height my-ssd1306 xor-rect-const
    ;

    : init-ssd1306-text ( -- )
      my-lock init-lock
      [:
        14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1 <ssd1306> my-ssd1306 init-object
        my-char-buf my-char-buf-size $20 fill
        0 old-cursor-col !
        0 old-cursor-row !
        0 cursor-col !
        0 cursor-row !
        dirty-all-ssd1306-text
        init-simple-font
        0 0 draw-cursor
        true to inited?
      ;] my-lock with-lock
    ;
    
    : render-ssd1306-text ( -- )
      old-cursor-col @ old-cursor-row @ draw-cursor
      dirty-end-row @ dirty-start-row @ ?do
        dirty-end-col @ dirty-start-col @ ?do 
          my-char-buf my-chars-width j * + i + c@
          my-char-width i * my-char-height j * my-ssd1306 a-simple-font set-char
        loop
      loop
      cursor-col @ cursor-row @ draw-cursor
      my-ssd1306 update-display
      clear-ssd1306-dirty
      cursor-col @ old-cursor-col !
      cursor-row @ old-cursor-row !
    ;
    
    : scroll-ssd1306-text ( -- )
      my-char-buf my-chars-width + my-char-buf my-chars-width my-chars-height 1- * move
      my-char-buf my-chars-width my-chars-height 1- * + my-chars-width $20 fill
      cursor-row @ 1- 0 max cursor-row !
      dirty-all-ssd1306-text
    ;
    
    : pre-advance-ssd1306-cursor ( -- )
      cursor-col @ my-chars-width = if
        0 cursor-col !
        1 cursor-row +!
      then
      cursor-row @ my-chars-height = if
        scroll-ssd1306-text
        0 cursor-col !
      then
    ;

    : advance-ssd1306-cursor ( -- )
      1 cursor-col +!
    ;
    
    : bs-ssd1306-cursor ( -- )
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
    
    : add-ssd1306-char { c -- }
      c $0A = if
        cursor-row @ 1+ my-chars-height min cursor-row !
        cursor-row @ my-chars-height = if
          scroll-ssd1306-text
        then
      else
        c $0D = if
          0 cursor-col !
        else
          c $08 = if
            bs-ssd1306-cursor
          else
            pre-advance-ssd1306-cursor
            c my-char-buf my-chars-width cursor-row @ * + cursor-col @ + c!
            cursor-col @ cursor-row @ dirty-ssd1306-char
            advance-ssd1306-cursor
          then
        then
      then
    ;
    
  end-module> import
  
  : erase-ssd1306 ( -- )
    inited? not if init-ssd1306-text then
    [:
      my-char-buf my-chars-width my-chars-height * $20 fill
      0 cursor-col !
      0 cursor-row !
      0 old-cursor-col !
      0 old-cursor-row !
      dirty-all-ssd1306-text
      my-ssd1306 clear-bitmap
      my-ssd1306 update-display
      0 0 draw-cursor
    ;] my-lock with-lock
  ;
  
  : clear-ssd1306 ( -- )
    inited? not if init-ssd1306-text then
    [:
      my-char-buf my-chars-width my-chars-height * $20 fill
      0 cursor-col !
      0 cursor-row !
      dirty-all-ssd1306-text
      render-ssd1306-text
    ;] my-lock with-lock
  ;
  
  : emit-ssd1306 ( c -- )
    inited? not if init-ssd1306-text then
    [:
      add-ssd1306-char
      render-ssd1306-text
    ;] my-lock with-lock
  ;
  
  : type-ssd1306 ( c-addr u -- }
    inited? not if init-ssd1306-text then
    [: { c-addr u }
      u 0 ?do c-addr i + c@ add-ssd1306-char loop
      render-ssd1306-text
    ;] my-lock with-lock
  ;
  
  : cr-ssd1306 ( -- )
    inited? not if init-ssd1306-text then
    [:
      cursor-row @ 1+ my-chars-height min cursor-row !
      0 cursor-col !
      cursor-row @ my-chars-height = if
        scroll-ssd1306-text
      then
      render-ssd1306-text
    ;] my-lock with-lock
  ;
  
  : bs-ssd1306 ( -- )
    inited? not if init-ssd1306-text then
    [:
      bs-ssd1306-cursor
      render-ssd1306-text
    ;] my-lock with-lock
  ;
  
  : goto-ssd1306 ( col row -- )
    inited? not if init-ssd1306-text then
    [: { col row }
      col 0 max my-chars-width min cursor-col !
      row 0 max my-chars-height min cursor-row !
      render-ssd1306-text
    ;] my-lock with-lock
  ;
  
  : ssd1306-cursor@ ( -- col row ) [: cursor-col @ cursor-row @ ;] my-lock with-lock ;
  
end-module