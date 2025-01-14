\ Copyright (c) 2024-2025 Travis Bemann
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

begin-module buffer-queue

  oo import
  slock import
  task import
  systick import

  begin-module buffer-queue-internal

  end-module> import
  
  <object> begin-class <buffer-queue>

    continue-module buffer-queue-internal

      \ Put simple lock
      slock-size member put-slock

      \ End offset simple lock
      slock-size member end-slock
      
      \ Buffer queue data address
      cell member data-addr

      \ Buffer queue data size (must be a multiple of a cell)
      cell member data-size

      \ Buffer queue put offset
      cell member put-offset

      \ Buffer queue get offset
      cell member get-offset

      \ Buffer queue end offset
      cell member end-offset
      
    end-module

    \ Attempt to put a buffer
    method poll-put-buffer ( addr bytes self -- success? )

    \ Put a buffer
    method put-buffer ( addr bytes self -- )

    \ Get a buffer
    method get-buffer ( self -- addr bytes )

    \ Poll a buffer
    method poll-buffer ( self -- addr bytes found? )

    \ Retire a buffer
    method retire-buffer ( self -- )
    
  end-class

  <buffer-queue> begin-implement

    \ Constructor
    :noname { data size self -- }
      self <object>->new
      self put-slock init-slock
      self end-slock init-slock
      data self data-addr !
      size self data-size !
      0 self put-offset !
      0 self get-offset !
      0 self end-offset !
    ; define new

    \ Attempt to put a buffer
    :noname ( addr bytes self -- success? )
      [: { addr bytes self }
        bytes cell align cell+ { bytes' }
        self get-offset @ { get-offset' }
        self put-offset @ { put-offset' }
        put-offset' bytes' + dup self data-size @ > if
          drop put-offset' 0 bytes'
        else
          dup put-offset' swap
        then
        { end-offset' start-offset next-offset }
        put-offset' next-offset < if
          put-offset' get-offset' < get-offset' next-offset <= and if
\            ." !"
            false
          else
\            ." *"
            self data-addr @ start-offset + { start-addr }
            bytes start-addr !
            addr start-addr cell+ bytes move
            next-offset self put-offset !
            true
          then
        else
          get-offset' put-offset' <= next-offset get-offset' < and if
\            ." #"
            self data-addr @ start-offset + { start-addr }
            bytes start-addr !
            addr start-addr cell+ bytes move
            end-offset' next-offset self [: { end-offset' next-offset self }
              next-offset self put-offset !
              end-offset' self end-offset !
              end-offset' self get-offset @ = if 0 self get-offset ! then
            ;] self end-slock with-slock
            true
          else
\            ." $"
            false
          then
        then
      ;] over put-slock with-slock
    ; define poll-put-buffer

    \ Put a buffer
    :noname ( addr bytes self -- )
      timeout @ no-timeout = if
        begin
          3dup poll-put-buffer not if
            pause-reschedule-last false
          else
            2drop drop true
          then
        until
      else
        systick-counter { start-systick }
        begin
          systick-counter start-systick - timeout @ < averts x-timed-out
          3dup poll-put-buffer not if
            pause-reschedule-last false
          else
            2drop drop true
          then
        until
      then
    ; define put-buffer
    
    \ Get a buffer
    :noname { self -- addr bytes }
      timeout @ no-timeout = if
        begin
          self poll-buffer not if
            2drop pause-reschedule-last false
          else
            true
          then
        until
      else
        systick-counter { start-systick }
        begin
          systick-counter start-systick - timeout @ < averts x-timed-out
          self poll-buffer not if
            2drop pause-reschedule-last false
          else
            true
          then
        until
      then
\      dup cr ." Got " . ." bytes"
    ; define get-buffer

    \ Poll a buffer
    :noname { self -- addr bytes found? }
      self put-offset self end-offset self get-offset [:
        @ swap @ rot @
      ;] self end-slock with-slock
      { get-offset' end-offset' put-offset' }
      end-offset' 0<> get-offset' end-offset' = and if
        0 self get-offset !
        0 self end-offset !
        0 to get-offset'
      then
      put-offset' get-offset' <> if
        self data-addr @ get-offset' + { get-addr }
        get-addr cell+ get-addr @ true
      else
        0 0 false
      then
    ; define poll-buffer

    \ Retire a buffer
    :noname { self -- }
      self get-offset @ { get-offset' }
      self data-addr @ get-offset' + @ cell align cell+ get-offset' +
      self [: { self }
        dup self end-offset @ = if drop 0 0 self end-offset ! then
      ;] self end-slock with-slock
      self get-offset !
    ; define retire-buffer

  end-implement
  
end-module
