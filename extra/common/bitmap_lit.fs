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

begin-module bitmap-lit
  
  bitmap import
  
  begin-module bitmap-lit-internal
    
    : >bitmap
      { addr width y line-addr -- }
      width 0 ?do
        line-addr i + c@ addr y 3 rshift width * + i + ccurrent!
      loop
      line-addr width $00 fill
    ;
    
  end-module> import

    
  : begin-bitmap-lit
    ( width height "name" -- addr width height y line-addr )
    create here -rot
    2dup bitmap-buf-size allot 0
    cell align,
    ram-here 3 pick ram-allot
    dup 4 pick $00 fill
  ;
    
  : !!
    { addr width height y line-addr -- addr width height y line-addr }
    y height u< if
      source nip >parse @ - width min 0 ?do
        source drop >parse @ + i + c@ bl > if
          y 7 and bit line-addr i + cbis!
        then
      loop
      y 7 and 7 = y height 1- = or if
        addr width y line-addr >bitmap
      then
    then
    source nip >parse !
    addr width height y 1+ line-addr
  ;

  : end-bitmap-lit { addr width height y line-addr -- }
    begin y height < while
      addr width y line-addr >bitmap
      y 1+ 8 align to y
    repeat
    line-addr ram-here - ram-allot
  ;
  
end-module
