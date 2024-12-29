\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module frame-process

  oo import
  task import
  frame-interface import
  net-config import
  net-misc import

  \ The frame handler class
  <object> begin-class <frame-handler>

    begin-module frame-process-internal
      
      \ The next frame handler
      cell member next-frame-handler
      
      \ Get the next frame handler
      method next-frame-handler@ ( self -- handler | 0 )
      
      \ Set the next frame handler
      method next-frame-handler! ( handler self -- )

    end-module> import

    \ Handle a frame
    method handle-frame ( addr bytes self -- )

    \ Handle a refresh
    method handle-refresh ( self -- )
    
  end-class

  \ Implement the frame handler class
  <frame-handler> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 self next-frame-handler !
    ; define new

    \ Get the next frame handler
    :noname ( self -- handler | 0 )
      next-frame-handler @
    ; define next-frame-handler@

    \ Set the next frame handler
    :noname ( handler self -- )
      next-frame-handler !
    ; define next-frame-handler!

    \ Handle a frame
    :noname ( addr bytes self -- )
      2drop drop
    ; define handle-frame
    
    \ Handle a refresh
    :noname ( self -- )
      drop
    ; define handle-refresh

  end-implement

  \ Frame processor class
  <object> begin-class <frame-process>

    continue-module frame-process-internal
      
      \ The frame interface
      cell member in-frame-interface
      
      \ The first frame handler
      cell member first-frame-handler
      
      \ The MTU buffer
      mtu-size cell align member mtu-buf
      
      \ Process a frame
      method process-frame ( bytes self -- )
      
      \ Do a refresh
      method process-refresh ( self -- )

    end-module
    
    \ Add a frame handler
    method add-frame-handler ( handler self -- )

    \ Run frame processor
    method run-frame-process ( self -- )
    
  end-class

  \ Implement the frame processor class
  <frame-process> begin-implement

    \ Constructor
    :noname { frame-interface self -- }
      self <object>->new
      frame-interface self in-frame-interface !
      0 self first-frame-handler !
    ; define new

    \ Add a frame handler
    :noname { handler self -- }
      self first-frame-handler @ { current }
      current 0= if
        handler self first-frame-handler !
      else
        begin current next-frame-handler@ while
          current next-frame-handler@ to current
        repeat
        handler current next-frame-handler!
      then
    ; define add-frame-handler

    \ Process a frame
    :noname { addr bytes self -- }
      self first-frame-handler @ { current }
      begin current while
        addr bytes current handle-frame
        current next-frame-handler@ to current
      repeat
    ; define process-frame

    \ Do a refresh
    :noname { self -- }
      self first-frame-handler @ { current }
      begin current while
        current handle-refresh
        current next-frame-handler@ to current
      repeat
    ; define process-refresh

    \ Run frame processor
    :noname { self -- }
      self 1 [: { self }
        begin
          self [: { self }
            refresh-interval task::timeout !
            self in-frame-interface @ get-rx-frame { frame-addr frame-bytes }
            no-timeout task::timeout !
            frame-addr frame-bytes self [:
              { frame-addr frame-bytes self }
              frame-bytes ethernet-header-size >= if
                
                [ debug? ] [if]
                  frame-addr frame-bytes [: cr ." INCOMING: " over + dump ;]
                  debug-hook execute
                [then]
                
                frame-addr frame-bytes self process-frame
                self process-refresh
              then
            ;] try
            frame-addr self in-frame-interface @ retire-rx-frame
            ?raise
          ;] try
          dup ['] task::x-timed-out = if
            2drop
            no-timeout task::timeout !
            self process-refresh
            0
          then
          ?raise
        again
      ;] 1024 256 1024 1 spawn-on-core
      c" frame-process" over task-name!
      frame-process-interval over task-interval!
      run
    ; define run-frame-process

  end-implement
  
end-module
