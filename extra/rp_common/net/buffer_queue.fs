\ Copyright (c) 2024 Travis Bemann
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
  
  \ Get the size of buffer queue data
  : buffer-queue-size { size count -- size }
    count 1+ 3 * cells size cell align count * +
  ;

  <object> begin-class <buffer-queue>

    continue-module buffer-queue-internal

      \ Reserve simple lock
      slock-size member reserve-slock

      \ Put simple lock
      slock-size member put-slock

      \ Retire simple lock
      slock-size member retire-slock

      \ Circular buffer entry count
      cell member entry-count
      
      \ Free buffer circular buffer
      cell member free-buffers
      
      \ Free buffer circular buffer read index
      cell member free-read-index

      \ Free buffer circular buffer write index
      cell member free-write-index

      \ Ready buffer circular buffer
      cell member ready-buffers
      
      \ Ready buffer circular buffer read index
      cell member ready-read-index

      \ Ready buffer circular buffer write index
      cell member ready-write-index
      
    end-module

    \ Reserve a buffer
    method reserve-buffer ( self -- addr )

    \ Non-blockingly reserve a buffer
    method poll-reserve-buffer ( self -- addr found? )
    
    \ Put a buffer
    method put-buffer ( addr bytes self -- )

    \ Get a buffer
    method get-buffer ( self -- addr bytes )

    \ Poll a buffer
    method poll-buffer ( self -- addr bytes found? )

    \ Retire a buffer
    method retire-buffer ( addr self -- )

    \ Is a buffer queue full
    method buffers-full? ( self -- full? )
    
  end-class

  <buffer-queue> begin-implement

    \ Constructor
    :noname { data size count self -- }
      self <object>->new
      self reserve-slock init-slock
      self put-slock init-slock
      self retire-slock init-slock
      count 1+ self entry-count !
      size cell align to size
      data self free-buffers !
      self free-buffers @ count 1+ cells + self ready-buffers !
      self ready-buffers @ count 1+ 2 * cells + { buffer-data }
      0 self free-read-index !
      0 self free-write-index !
      0 self ready-read-index !
      0 self ready-write-index !
      count 0 ?do buffer-data size i * + self retire-buffer loop
    ; define new
    
    \ Reserve a buffer
    :noname { self -- addr }
      timeout @ no-timeout = if
        begin
          self poll-reserve-buffer not if
            drop pause-reschedule-last false
          else
            true
          then
        until
      else
        systick-counter { start-systick }
        begin
          systick-counter start-systick - timeout @ < averts x-timed-out
          self poll-reserve-buffer not if
            drop pause-reschedule-last false
          else
            true
          then
        until
      then
    ; define reserve-buffer

    \ Non-blockingly reserve a buffer
    :noname ( self -- addr found? )
      [: { self }
        self free-write-index @
        self free-read-index @ dup { read-index } <> if
          self free-buffers @ read-index cells + @ true
          read-index 1+ self entry-count @ umod self free-read-index !
        else
          0 false
        then
      ;] over reserve-slock with-slock
    ; define poll-reserve-buffer
    
    \ Put a buffer
    :noname ( addr bytes self -- )
      [: { addr bytes self }
        self ready-write-index @ { index }
        addr bytes self ready-buffers @ index 2 * cells + 2!
        index 1+ self entry-count @ umod self ready-write-index !
      ;] over put-slock with-slock
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
    ; define get-buffer

    \ Poll a buffer
    :noname { self -- addr bytes found? }
      self ready-write-index @
      self ready-read-index @ dup { read-index } <> if
        self ready-buffers @ read-index 2 * cells + 2@ true
        read-index 1+ self entry-count @ umod self ready-read-index !
      else
        0 0 false
      then
    ; define poll-buffer

    \ Retire a buffer
    :noname ( addr self -- )
      [: { addr self }
        self free-write-index @ { index }
        addr self free-buffers @ index cells + !
        index 1+ self entry-count @ umod self free-write-index !
      ;] over retire-slock with-slock
    ; define retire-buffer

    \ Is a buffer queue full
    :noname { self -- full? }
      self free-write-index @ self free-read-index @ =
    ; define buffers-full?

  end-implement
  
end-module
