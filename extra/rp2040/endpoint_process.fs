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

begin-module endpoint-process

  oo import
  task import
  net-misc import
  net import

  \ The endpoint packet handler class
  <object> begin-class <endpoint-handler>

    \ The next endpoint packet handler
    cell member next-endpoint-handler

    \ Get the next endpoint packet handler
    method next-endpoint-handler@ ( self -- handler | 0 )

    \ Set the next endpoint packet handler
    method next-endpoint-handler! ( handler self -- )

    \ Handle a endpoint packet
    method handle-endpoint ( src-addr protocol addr bytes self -- )
    
  end-class

  \ Implement the endpoint packet handler class
  <endpoint-handler> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 self next-endpoint-handler !
    ; define new

    \ Get the next endpoint packet handler
    :noname ( self -- handler | 0 )
      next-endpoint-handler @
    ; define next-endpoint-handler@

    \ Set the next endpoint packet handler
    :noname ( handler self -- )
      next-endpoint-handler !
    ; define next-endpoint-handler!

    \ Handle a endpoint packet
    :noname ( endpoint self -- )
      2drop
    ; define handle-endpoint
    
  end-implement
  
  \ endpoint processor class
  <object> begin-class <endpoint-process>

    \ The CYW43 driver control
    cell member interface

    \ The first endpoint handler
    cell member first-endpoint-handler

    \ Add a endpoint handler
    method add-endpoint-handler ( handler self -- )

    \ Process a endpoint
    method process-endpoint ( endpoint self -- )

    \ Run endpoint processor
    method run-endpoint-process ( self -- )
    
  end-class

  \ Implement the endpoint processor class
  <endpoint-process> begin-implement

    \ Constructor
    :noname { control self -- }
      self <object>->new
      control self interface !
      0 self first-endpoint-handler !
    ; define new

    \ Add a endpoint handler
    :noname { handler self -- }
      self first-endpoint-handler @ { current }
      current 0= if
        handler self first-endpoint-handler !
      else
        begin current next-endpoint-handler@ while
          current next-endpoint-handler@ to current
        repeat
        handler current next-endpoint-handler!
      then
    ; define add-endpoint-handler

    \ Process a endpoint
    :noname { endpoint self -- }
      self first-endpoint-handler @ { current }
      begin current while
        endpoint current handle-endpoint
        current next-endpoint-handler @ to current
      repeat
      endpoint retire-rx-packet
    ; define process-endpoint

    \ Run endpoint processor
    :noname { self -- }
      self 1 [: { self }
        begin
          self interface @ get-ready-rx-endpoint self process-endpoint
        again
      ;] 1024 256 1024 spawn run
    ; define run-endpoint-process

  end-implement
  
end-module
