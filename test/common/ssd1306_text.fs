\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module ssd1306-test

  oo import
  bitmap import
  ssd1306 import
  font import
  simple-font import
  
  \ An I2C SSD1306 device
  <ssd1306> class-size buffer: my-ssd1306
  
  \ The number of columns
  128 constant my-cols

  \ The number of rows
  64 constant my-rows

  \ My framebuffer size
  my-cols my-rows bitmap-buf-size constant my-buf-size

  \ My framebuffer
  my-buf-size buffer: my-buf
  
  \ Are we initialized
  false value inited?
    
  \ Initialize the test
  : init-test ( -- )
    14 15 my-buf my-cols my-rows SSD1306_I2C_ADDR 1 <ssd1306> my-ssd1306 init-object
    init-simple-font
  ;
  
  \ Draw text
  : draw-text { c-addr u col row -- }
    inited? not if init-test true to inited? then
    c-addr u col row op-set my-ssd1306 a-simple-font draw-string
    my-ssd1306 update-display
  ;
  
  \ Bounce a pixel around the display
  : bounce-text { c-addr u -- }
    inited? not if init-test true to inited? then
    0 0 1 1 { column row delta-column delta-row }
    begin key? not while
      c-addr u column row op-xor my-ssd1306 a-simple-font draw-string
      my-ssd1306 update-display
      c-addr u column row op-xor my-ssd1306 a-simple-font draw-string
      column 7 u * + my-cols >= if
        -1 to delta-column
      else
        column 0 <= if
          1 to delta-column
        then
      then
      row 8 + my-rows >= if
        -1 to delta-row
      else
        row 0 <= if
          1 to delta-row
        then
      then
      delta-column +to column
      delta-row +to row
      100 ms
    repeat
    key drop
  ;
  
  \ Bounce a pixel around the display, rotating the string rightward one character every second
  : bounce-text-rotate ( c-addr u -- )
    dup [:
      { c-addr u copy-addr }
      c-addr copy-addr u move
      inited? not if init-test true to inited? then
      0 0 1 1 0 { column row delta-column delta-row cycle }
      begin key? not while
        copy-addr u column row op-xor my-ssd1306 a-simple-font draw-string
        my-ssd1306 update-display
        copy-addr u column row op-xor my-ssd1306 a-simple-font draw-string
        column 7 u * + my-cols >= if
          -1 to delta-column
        else
          column 0 <= if
            1 to delta-column
          then
        then
        row 8 + my-rows >= if
          -1 to delta-row
        else
          row 0 <= if
            1 to delta-row
          then
        then
        delta-column +to column
        delta-row +to row
        1 +to cycle
        cycle 10 umod 0= if
          copy-addr u + 1- c@ { last-c }
          copy-addr copy-addr 1+ u 1- move
          last-c copy-addr c!
          0 to cycle
        then
        100 ms
      repeat
      key drop
    ;] with-allot
  ;
  
  \ Bounce a counter around the display, incrementing the counter each second
  : bounce-counter ( count -- )
    10 [:
      { count buffer }
      inited? not if init-test true to inited? then
      0 0 1 1 0 { column row delta-column delta-row cycle }
      begin key? not while
        count 0 <# #s #> { c-addr bytes }
        c-addr buffer bytes move
        buffer bytes column row op-xor my-ssd1306 a-simple-font draw-string
        my-ssd1306 update-display
        buffer bytes column row op-xor my-ssd1306 a-simple-font draw-string
        column 7 bytes * + my-cols >= if
          -1 to delta-column
        else
          column 0 <= if
            1 to delta-column
          then
        then
        row 8 + my-rows >= if
          -1 to delta-row
        else
          row 0 <= if
            1 to delta-row
          then
        then
        delta-column +to column
        delta-row +to row
        1 +to cycle
        cycle 10 umod 0= if
          1 +to count
          0 to cycle
        then
        100 ms
      repeat
      key drop
    ;] with-allot
  ;
  
  rtc::date-time-size aligned-buffer: my-date-time
  
  \ Bounce a time around the display
  : bounce-time ( -- )
    10 [:
      { buffer }
      inited? not if init-test true to inited? then
      0 0 1 1 { column row delta-column delta-row }
      begin key? not while
        my-date-time rtc::date-time@
        buffer { cur-buffer }
        my-date-time rtc::date-time-hour c@ s>d <# #s #> { c-addr bytes }
        bytes 2 < if [char] 0 cur-buffer c! 1 +to cur-buffer then
        c-addr cur-buffer bytes move bytes +to cur-buffer
        [char] : cur-buffer c! 1 +to cur-buffer
        my-date-time rtc::date-time-minute c@ s>d <# #s #> to bytes to c-addr
        bytes 2 < if [char] 0 cur-buffer c! 1 +to cur-buffer then
        c-addr cur-buffer bytes move bytes +to cur-buffer
        [char] : cur-buffer c! 1 +to cur-buffer
        my-date-time rtc::date-time-second c@ s>d <# #s #> to bytes to c-addr
        bytes 2 < if [char] 0 cur-buffer c! 1 +to cur-buffer then
        c-addr cur-buffer bytes move bytes +to cur-buffer
        buffer 8 column row op-xor my-ssd1306 a-simple-font draw-string
        my-ssd1306 update-display
        buffer 8 column row op-xor my-ssd1306 a-simple-font draw-string
        column 7 8 * + my-cols >= if
          -1 to delta-column
        else
          column 0 <= if
            1 to delta-column
          then
        then
        row 8 + my-rows >= if
          -1 to delta-row
        else
          row 0 <= if
            1 to delta-row
          then
        then
        delta-column +to column
        delta-row +to row
        100 ms
      repeat
      key drop
    ;] with-allot
  ;
  
end-module
