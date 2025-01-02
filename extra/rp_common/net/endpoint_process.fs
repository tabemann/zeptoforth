\ Copyright (c) 2023-2025 Travis Bemann
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
  systick import
  task import
  net-misc import
  net import

  \ The endpoint packet handler class
  <object> begin-class <endpoint-handler>

    begin-module endpoint-process-internal
      
      \ The next endpoint packet handler
      cell member next-endpoint-handler
      
      \ Get the next endpoint packet handler
      method next-endpoint-handler@ ( self -- handler | 0 )
      
      \ Set the next endpoint packet handler
      method next-endpoint-handler! ( handler self -- )

    end-module> import
      
    \ Handle a endpoint packet
    method handle-endpoint ( endpoint self -- )

    \ Handle a timeout
    method handle-timeout ( self -- )
    
    \ Get endpoint timeout
    method handler-timeout@ ( self -- timeout )
    
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

    \ Handle a timeout
    :noname ( self -- )
      drop
    ; define handle-timeout
    
    \ Get an handler timeout
    :noname ( self -- timeout )
      drop no-timeout
    ; define handler-timeout@
    
  end-implement

  \ Endpoint processor has not been started exception
  : x-endpoint-process-not-started ( -- )
    ." endpoint processor not started" cr
  ;
  
  \ Endpoint processor class
  <object> begin-class <endpoint-process>

    continue-module endpoint-process-internal
      
      \ The CYW43 driver control
      cell member endpoint-interface
      
      \ The first endpoint handler
      cell member first-endpoint-handler
      
      \ The endpoint processor task
      cell member endpoint-process-task

      \ The endpoint process priority
      cell member endpoint-process-priority

      \ The endpoint process interval
      cell member endpoint-process-interval
      
      \ Process a endpoint
      method process-endpoint ( endpoint self -- )
      
      \ Get the current timeout
      method get-handler-timeout ( self -- timeout )
      
      \ Handle a timeout
      method do-handler-timeouts ( start-systick self -- )
      
    end-module
    
    \ Add a endpoint handler
    method add-endpoint-handler ( handler self -- )
    
    \ Run endpoint processor
    method run-endpoint-process ( self -- )
    
    \ Set the endpoint processor priority
    method endpoint-process-priority! ( priority self -- )
    
    \ Get the endpoint processor priority
    method endpoint-process-priority@ ( self -- priority )
    
    \ Set the endpoint processor interval
    method endpoint-process-interval! ( interval self -- )
    
    \ Get the endpoint processor interval
    method endpoint-process-interval@ ( self -- interval )
    
    \ Set the endpoint processor deadline
    method endpoint-process-deadline! ( deadline self -- )
    
    \ Get the endpoint processor deadline
    method endpoint-process-deadline@ ( self -- deadline )
    
  end-class
  
  \ Implement the endpoint processor class
  <endpoint-process> begin-implement
    
    \ Constructor
    :noname { control self -- }
      self <object>->new
      control self endpoint-interface !
      0 self first-endpoint-handler !
      0 self endpoint-process-task !
      0 self endpoint-process-priority !
      -1 self endpoint-process-interval !
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
    ; define process-endpoint

    \ Get the current timeout
    :noname { self -- timeout }
      no-timeout { current-timeout }
      self first-endpoint-handler @ { current-handler }
      begin current-handler while
        current-handler handler-timeout@ dup no-timeout <> if
          current-timeout no-timeout <> if
            current-timeout min to current-timeout
          else
            to current-timeout
          then
        else
          drop
        then
        current-handler next-endpoint-handler @ to current-handler
      repeat
      current-timeout
    ; define get-handler-timeout
    
    \ Handle a timeout
    :noname { start-systick self -- }
      systick-counter { current-systick }
      self first-endpoint-handler @ { current-handler }
      begin current-handler while
        current-handler handler-timeout@ dup no-timeout <> if
          start-systick + current-systick - 0<= if
            current-handler handle-timeout
          then
        else
          drop
        then
        current-handler next-endpoint-handler @ to current-handler
      repeat
    ; define do-handler-timeouts

    \ Run endpoint processor
    :noname { self -- }
      self 1 [: { self }
        begin
          self get-handler-timeout { current-timeout }
          current-timeout no-timeout = if
            self endpoint-interface @ get-ready-endpoint self process-endpoint
          else
            systick-counter { start-systick }
            current-timeout start-systick self [:
              { current-timeout start-systick self }
              self [: endpoint-interface @ get-ready-endpoint ;]
              current-timeout start-systick with-timeout-from-start
            ;] try dup ['] x-timed-out = if
              2drop 2drop false 0
            else
              dup 0= if true swap then
            then
            ?raise  
            if
              self process-endpoint
            else
              start-systick self do-handler-timeouts
            then
          then
        again
      ;] 1024 256 1024 spawn dup { process-task } self endpoint-process-task !
      c" endpoint-process" process-task task-name!
      self endpoint-process-priority @ process-task task-priority!
      self endpoint-process-interval @ process-task task-interval!
      process-task run
    ; define run-endpoint-process

    \ Set the endpoint processor priority
    :noname { priority self -- }
      self endpoint-process-task @ ?dup if
        priority swap task-priority!
      else
        priority self endpoint-process-priority !
      then
    ; define endpoint-process-priority!
    
    \ Get the endpoint processor priority
    :noname { self -- priority }
      self endpoint-process-task @ ?dup if
        task-priority@
      else
        self endpoint-process-priority @
      then
    ; define endpoint-process-priority@
    
    \ Set the endpoint processor interval
    :noname { interval self -- }
      self endpoint-process-task @ ?dup if
        interval swap task-interval!
      else
        interval self endpoint-process-interval !
      then
    ; define endpoint-process-interval!
    
    \ Get the endpoint processor interval
    :noname { self -- interval }
      self endpoint-process-task @ ?dup if
        task-interval@
      else
        self endpoint-process-interval @
      then
    ; define endpoint-process-interval@
    
    \ Set the endpoint processor deadline
    :noname { deadline self -- }
      self endpoint-process-task @ dup averts x-endpoint-process-not-started
      deadline swap task-deadline!
    ; define endpoint-process-deadline!
    
    \ Get the endpoint processor deadline
    :noname { self -- deadline }
      self endpoint-process-task @ dup averts x-endpoint-process-not-started
      task-deadline@
    ; define endpoint-process-deadline@

  end-implement
  
end-module
