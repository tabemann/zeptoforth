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

compile-to-flash

begin-module psram-blocks

  oo import
  block-dev import
  lock import

  \ Attempted to write to protected block zero
  : x-block-zero-protected ( - ) ." block block zero is protected" cr ;
  
  \ PSRAM block device class
  <block-dev> begin-class <psram-blocks>

    begin-module psram-blocks-internal
      
      \ Sector size
      512 constant sector-size

      \ Lock protecting the blocks device
      lock-size member psram-lock

      \ Protect block zero
      cell member psram-protect-block-zero
      
      \ Maximum block count
      cell member max-block-count
      
      \ Write through mode
      cell member write-through

    end-module> import

    \ Enable block zero writes
    method write-psram-block-zero! ( enabled blk -- )

    continue-module psram-blocks-internal
    
      \ Validate a block index
      method validate-block ( block blk -- )

    end-module
      
  end-class

  \ Implement PSRAM block device class
  <psram-blocks> begin-implement

    :noname { self -- }
      self <block-dev>->new
      self psram-lock init-lock
      false self write-through !
      true self psram-protect-block-zero !
      psram-size sector-size / self max-block-count !
    ; define new

    :noname { self -- bytes } sector-size ; define block-size
    
    :noname ( self -- blocks ) max-block-count @ ; define block-count

    \ Enable block zero writes
    :noname ( enabled self -- )
      swap not swap psram-protect-block-zero !
    ; define write-psram-block-zero!

    :noname ( c-addr u block blk -- )
      2dup validate-block
      [: { addr bytes block self }
        block 0= self psram-protect-block-zero @ and
        triggers x-block-zero-protected
        bytes 0 max sector-size min to bytes
        addr psram-base block sector-size * + bytes move
      ;] over psram-lock with-lock
    ; define block!
    
    :noname ( c-addr u offset block blk -- )
      2dup validate-block
      [: { addr bytes offset block self }
        block 0= self psram-protect-block-zero @ and
        triggers x-block-zero-protected
        offset 0 max sector-size min to offset
        bytes offset + sector-size min offset - to bytes
        addr psram-base block sector-size * + offset + bytes move
      ;] over psram-lock with-lock
    ; define block-part!

    :noname ( c-addr u block blk -- )
      2dup validate-block
      [: { addr bytes block self }
        block 0= self psram-protect-block-zero @ and
        triggers x-block-zero-protected
        bytes sector-size min to bytes
        psram-base block sector-size * + addr bytes move
      ;] over psram-lock with-lock
    ; define block@
    
    :noname ( c-addr u offset block blk -- )
      2dup validate-block
      [: { addr bytes offset block self }
        block 0= self psram-protect-block-zero @ and
        triggers x-block-zero-protected
        offset 0 max sector-size min to offset
        bytes offset + sector-size min offset - to bytes
        psram-base block sector-size * + offset + addr bytes move
      ;] over psram-lock with-lock
    ; define block-part@

    :noname { self -- }
    ; define flush-blocks
    
    :noname { self -- }
    ; define clear-blocks
    
    :noname ( write-through self -- )
      write-through !
    ; define write-through!
    
    :noname ( self -- write-through )
      write-through @
    ; define write-through@
    
    :noname ( block self -- )
      over 0 >= averts x-block-out-of-range
      block-count < averts x-block-out-of-range
    ; define validate-block
    
  end-implement
  
end-module

reboot
