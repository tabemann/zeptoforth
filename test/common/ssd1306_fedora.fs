\ Copyright (c) 2023 Patrick Kaell
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

begin-module fedora-test

  oo import
  bitmap import
  bitmap-utils import
  ssd1306 import
  rng import
  systick import
  task import
  
  \ An I2C SSD1306 device
  <ssd1306> class-size buffer: my-ssd1306
  
  \ The number of columns
  128 constant my-cols

  \ The number of rows
  64 constant my-rows
  
  \ The width of the sprite
  12 constant my-sprite-cols
  
  \ The height of the sprite
  12 constant my-sprite-rows

  \ My framebuffer size
  my-cols my-rows bitmap-buf-size constant my-buf-size

  \ My framebuffer
  my-buf-size buffer: my-buf
  
  \ Has balls been initialized
  false value inited?

  \ Initialize the display
  : init-display ( -- )
    14 15 my-buf my-cols my-rows SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
  ;

  CREATE TRIGTABLE
  0 H, 286 H, 572 H, 857 H, 1143 H, 1428 H, 1713 H, 1997 H,
  2280 H, 2563 H, 2845 H, 3126 H, 3406 H, 3686 H, 3964 H, 4240 H,
  4516 H, 4790 H, 5063 H, 5334 H, 5604 H, 5872 H, 6138 H, 6402 H,
  6664 H, 6924 H, 7182 H, 7438 H, 7692 H, 7943 H, 8192 H, 8438 H,
  8682 H, 8923 H, 9162 H, 9397 H, 9630 H, 9860 H, 10087 H, 10311 H,
  10531 H, 10749 H, 10963 H, 11174 H, 11381 H, 11585 H, 11786 H, 11982 H,
  12176 H, 12365 H, 12551 H, 12733 H, 12911 H, 13085 H, 13255 H, 13421 H,
  13583 H, 13741 H, 13894 H, 14044 H, 14189 H, 14330 H, 14466 H, 14598 H,
  14726 H, 14849 H, 14968 H, 15082 H, 15191 H, 15296 H, 15396 H, 15491 H,
  15582 H, 15668 H, 15749 H, 15826 H, 15897 H, 15964 H, 16026 H, 16083 H,
  16135 H, 16182 H, 16225 H, 16262 H, 16294 H, 16322 H, 16344 H, 16362 H,
  16374 H, 16382 H, 16384 H,

  : SIN
    DUP 360 > IF
      360 MOD
    ELSE
      DUP 0< IF
        360 MOD
        360 +
      THEN
    THEN

    2* DUP 180 < IF
      TRIGTABLE + H@
    ELSE
      DUP 360 < IF
        NEGATE 360 + TRIGTABLE + H@
      ELSE
        DUP 540 < IF
          360 - TRIGTABLE + H@ NEGATE
        ELSE
          NEGATE 720 + TRIGTABLE + H@ NEGATE
        THEN
      THEN
    THEN
  ;

  : SQR
    DUP 1
    BEGIN 2DUP > WHILE
      + 2/ 2DUP /
    REPEAT
    ROT 2DROP
  ;

  CREATE RR 642 ALLOT
  VARIABLE ZS
  VARIABLE XL
  VARIABLE XI
  VARIABLE XT
  VARIABLE YY
  VARIABLE X1
  VARIABLE Y1

  : MODE { ignore -- }
    inited? not if
      init-display
      true to inited?
    then
    my-ssd1306 clear-bitmap
  ;
  
  : PLOT { ignore x y -- }
    $FF x my-cols * 1280 / y my-rows * -1024 / my-rows + op-set my-ssd1306
    draw-pixel-const
    my-ssd1306 update-display    
  ;
  
  : FEDORA
    0 MODE
    642 0 DO
      1000 RR I + H!
    2 +LOOP

    -64 64 DO
      I I * 81 16 */ ZS H!
      20736 ZS H@ - SQR XL H!

      XL H@ 1+ XL H@ NEGATE DO
        I I * ZS H@ + SQR 27192 16384 */ XT H!
        XT H@ SIN XT H@ 3 * SIN 2 5 */ + 56 16384 */ YY H!
        I J + 160 + X1 H!
        90 YY H@ - J + Y1 H!

        X1 H@ 0 < NOT IF
          RR X1 H@ 2* + H@ Y1 H@ > IF
            Y1 H@ RR X1 H@ 2* + H!
            69 X1 H@ 4 * 900 Y1 H@ 4 * - PLOT
          THEN
        THEN
      LOOP
    -2 +LOOP
  ;

end-module