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

compile-to-flash

begin-module block-dev

  oo import
  
  \ Block out of range exception
  : x-block-out-of-range ( -- ) ." block out of range" cr ;
  
  \ Block device base class
  <object> begin-class <block-dev>
    \ Get block size
    method block-size ( dev -- bytes )
    
    \ Get block count
    method block-count ( dev -- blocks )

    \ Write block
    method block! ( c-addr u block-index dev -- )
    
    \ Write part of a block
    method block-part! ( c-addr u offset block-index dev -- )

    \ Read block
    method block@ ( c-addr u block-index dev -- )
    
    \ Read part of a block
    method block-part@ ( c-addr u offset block-index dev -- )

    \ Flush blocks
    method flush-blocks ( dev -- )
    
    \ Clear cached blocks
    method clear-blocks ( dev -- )
    
    \ Set write-through cache mode
    method write-through! ( write-through dev -- )
    
    \ Get write-through cache mode
    method write-through@ ( dev -- write-through )
  end-class

  \ Implement block device base class
  <block-dev> begin-implement
    ' abstract-method define block-size
    ' abstract-method define block-count
    ' abstract-method define block!
    ' abstract-method define block-part!
    ' abstract-method define block@
    ' abstract-method define block-part@
    ' abstract-method define flush-blocks
    ' abstract-method define clear-blocks
    ' abstract-method define write-through!
    ' abstract-method define write-through@
  end-implement
  
end-module

reboot
