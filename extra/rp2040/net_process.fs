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

begin-module net-process

  oo import
  task import
  cyw43-control import

  \ The frame handler class
  <object> begin-class <net-handler>

    \ The next frame handler
    cell member next-net-handler

    \ Get the next frame handler
    method next-net-handler@ ( self -- handler | 0 )

    \ Set the next frame handler
    method next-net-handler! ( handler self -- )

    \ Handle a frame
    method handle-net-frame ( addr bytes self -- )
    
  end-class

  \ Implement the frame handler class
  <net-handler> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 self next-net-handler !
    ; define new

    \ Get the next frame handler
    :noname ( self -- handler | 0 )
      next-net-handler @
    ; define next-net-handler@

    \ Set the next frame handler
    :noname ( handler self -- )
      next-net-handler !
    ; define next-net-handler!

    \ Handle a frame
    :noname ( addr bytes self -- )
      2drop drop
    ; define handle-net-frame
    
  end-implement

  \ frame processor class
  <object> begin-class <net-process>

    \ The CYW43 driver control
    cell member net-control

    \ The first frame handler
    cell member first-net-handler

    \ The MTU buffer
    net-misc::mtu-size cell align member mtu-buf

    \ Add a frame handler
    method add-net-handler ( handler self -- )

    \ Process a frame
    method process-net-frame ( bytes self -- )

    \ Run frame processor
    method run-process-net ( self -- )
    
  end-class

  \ Implement the frame processor class
  <net-process> begin-implement

    \ Constructor
    :noname { control self -- }
      self <object>->new
      control self net-control !
      0 self first-net-handler !
    ; define new

    \ Add a frame handler
    :noname { handler self -- }
      self first-net-handler @ { current }
      current 0= if
        handler self first-net-handler !
      else
        begin current next-net-handler@ while
          current next-net-handler@ to current
        repeat
        handler current next-net-handler!
      then
    ;

    \ Process a frame
    :noname { bytes self -- }
      self first-net-handler @ { current }
      begin current while
        self mtu-buf bytes current handle-net-frame
        current next-net-handler@ to current
      repeat
    ; define process-net-frame

    \ Run frame processor
    :noname { self -- }
      self 1 [: { self }
        begin
          self mtu-buf self net-control @ get-cyw43-rx
          dup ethernet-header-size >= if
            self process-net-frame
          else
            drop
          then
        again
      ;] 1024 256 1024 spawn run
    ; define run-process-net

  end-implement
  
end-module
