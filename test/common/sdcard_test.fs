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

begin-module sdcard-test

  oo import
  block-dev import
  sd import
  spi import
  pin import
  
  <sd> class-size buffer: my-sd
  
  512 constant block-size
  
  block-size aligned-buffer: my-buffer
  
  0 constant my-spi

  \ Start the SD Card test
  : init-test ( -- )
    my-spi 2 spi-pin
    my-spi 3 spi-pin
    my-spi 4 spi-pin
    5 output-pin
    6 2 ?do i pull-up-pin loop
    5 my-spi <sd> my-sd init-object
    my-sd init-sd
    my-buffer block-size 0 my-sd block@
    my-buffer my-buffer block-size + dump
  ;
  
  \ Flush and clear blocks
  : flush-clear ( -- )
    my-sd flush-blocks
    my-sd clear-blocks
  ;
  
  \ Read a block
  : read-block ( block -- )
    my-buffer block-size $7F fill
    my-buffer block-size rot my-sd block@
    my-buffer my-buffer block-size + dump
  ;
  
  \ Read/write a block
  : rw-block ( x block -- )
    my-buffer block-size $00 fill
    >r my-buffer !
    my-buffer block-size r@ my-sd block!
    my-sd flush-blocks
    my-sd clear-blocks
    my-buffer block-size $7F fill
    my-buffer block-size r> my-sd block@
    my-buffer my-buffer block-size + dump
  ;

end-module
