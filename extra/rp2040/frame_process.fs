\ Copyright (c) 2023 Travis Bemann
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
  net-misc import

  \ The frame handler class
  <object> begin-class <frame-handler>

    \ The next frame handler
    cell member next-frame-handler

    \ Get the next frame handler
    method next-frame-handler@ ( self -- handler | 0 )

    \ Set the next frame handler
    method next-frame-handler! ( handler self -- )

    \ Handle a frame
    method handle-frame ( addr bytes self -- )
    
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
    
  end-implement

  \ Frame processor class
  <object> begin-class <frame-process>

    \ The frame interface
    cell member in-frame-interface

    \ The first frame handler
    cell member first-frame-handler

    \ The MTU buffer
    net-misc::mtu-size cell align member mtu-buf

    \ Add a frame handler
    method add-frame-handler ( handler self -- )

    \ Process a frame
    method process-frame ( bytes self -- )

    \ Run frame processor
    method run-process-net ( self -- )
    
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
    :noname { bytes self -- }
      self first-frame-handler @ { current }
      begin current while
        self mtu-buf bytes current handle-frame
        current next-frame-handler@ to current
      repeat
    ; define process-frame

    \ Run frame processor
    :noname { self -- }
      self 1 [: { self }
        begin
          self mtu-buf self in-frame-interface @ get-rx-frame
          dup etherframe-header-size >= if
            self process-frame
          else
            drop
          then
        again
      ;] 1024 256 1024 spawn run
    ; define run-process-net

  end-implement
  
end-module
