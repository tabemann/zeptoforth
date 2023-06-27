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

begin-module ip-process

  oo import
  task import
  net-misc import
  net import

  \ The IP packet handler class
  <object> begin-class <ip-handler>

    \ The next IP packet handler
    cell member next-ip-handler

    \ Get the next IP packet handler
    method next-ip-handler@ ( self -- handler | 0 )

    \ Set the next IP packet handler
    method next-ip-handler! ( handler self -- )

    \ Handle a IP packet
    method handle-ip ( src-addr protocol addr bytes self -- )
    
  end-class

  \ Implement the IP packet handler class
  <ip-handler> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 self next-ip-handler !
    ; define new

    \ Get the next IP packet handler
    :noname ( self -- handler | 0 )
      next-ip-handler @
    ; define next-ip-handler@

    \ Set the next IP packet handler
    :noname ( handler self -- )
      next-ip-handler !
    ; define next-ip-handler!

    \ Handle a IP packet
    :noname ( endpoint self -- )
      2drop
    ; define handle-ip
    
  end-implement
  
  \ IP processor class
  <object> begin-class <ip-process>

    \ The CYW43 driver control
    cell member interface

    \ The first IP handler
    cell member first-ip-handler

    \ Add a IP handler
    method add-ip-handler ( handler self -- )

    \ Process a IP
    method process-ip ( endpoint self -- )

    \ Run IP processor
    method run-process-ip ( self -- )
    
  end-class

  \ Implement the IP processor class
  <ip-process> begin-implement

    \ Constructor
    :noname { control self -- }
      self <object>->new
      control self interface !
      0 self first-ip-handler !
    ; define new

    \ Add a IP handler
    :noname { handler self -- }
      self first-ip-handler @ { current }
      current 0= if
        handler self first-ip-handler !
      else
        begin current next-ip-handler@ while
          current next-ip-handler@ to current
        repeat
        handler current next-ip-handler!
      then
    ; define add-ip-handler

    \ Process a IP
    :noname { endpoint self -- }
      self first-ip-handler @ { current }
      begin current while
        endpoint current handle-ip
        current next-ip-handler@ to current
      repeat
      endpoint retire-rx-packet
    ; define process-ip

    \ Run IP processor
    :noname { self -- }
      self 1 [: { self }
        begin
          self interface @ get-ready-rx-endpoint self process-ip
        again
      ;] 1024 256 1024 spawn run
    ; define run-process-net

  end-implement
  
end-module
