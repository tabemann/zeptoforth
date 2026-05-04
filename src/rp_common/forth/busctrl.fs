\ Copyright (c) 2026 Travis Bemann
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

begin-module busctrl

  begin-module busctrl-internal

    \ Bus control base
    rp2040? [if]
      $40030000 constant BUSCTRL_BASE
    [then]
    rp2350? [if]
      $40068000 constant BUSCTRL_BASE
    [then]

    \ Bus priority register
    BUSCTRL_BASE $00 + constant BUSCTRL_BUS_PRIORITY

    \ 1 for high DMA write priority, 0 for low DMA write priority
    12 bit constant BUSCTRL_BUS_PRIORITY_DMA_W

    \ 1 for high DMA read priority, 0 for low DMA read priority
    8 bit constant BUSCTRL_BUS_PRIORITY_DMA_R

    \ 1 for high CPU 1 priority, 0 for low CPU 1 priority
    4 bit constant BUSCTRL_BUS_PRIORITY_PROC1

    \ 1 for high CPU 0 priority, 0 for low CPU 0 priority
    0 bit constant BUSCTRL_BUS_PRIORITY_PROC0

    \ Bus priority acknowledgement register
    BUSCTRL_BASE $04 + constant BUSCTRL_BUS_PRIORITY_ACK

    \ Wait for acknowledgement
    : wait-bus-priority-ack ( -- )
      begin 1 BUSCTRL_BUS_PRIORITY_ACK bit@ until
    ;

  end-module> import

  \ High priority
  true constant high

  \ Low priority
  false constant low

  \ Set DMA write bus priority
  : dma-w-bus-priority! ( priority -- )
    BUSCTRL_BUS_PRIORITY_DMA_W BUSCTRL_BUS_PRIORITY rot if bis! else bic! then
    wait-bus-priority-ack
  ;

  \ Get DMA write bus priority
  : dma-w-bus-priority@ ( -- priority )
    BUSCTRL_BUS_PRIORITY_DMA_W BUSCTRL_BUS_PRIORITY bit@
  ;

  \ Set DMA read bus priority
  : dma-w-bus-priority! ( priority -- )
    BUSCTRL_BUS_PRIORITY_DMA_R BUSCTRL_BUS_PRIORITY rot if bis! else bic! then
    wait-bus-priority-ack
  ;

  \ Get DMA read bus priority
  : dma-w-bus-priority@ ( -- priority )
    BUSCTRL_BUS_PRIORITY_DMA_R BUSCTRL_BUS_PRIORITY bit@
  ;

  \ Set CPU 1 bus priority
  : proc1-bus-priority! ( priority -- )
    BUSCTRL_BUS_PRIORITY_PROC1 BUSCTRL_BUS_PRIORITY rot if bis! else bic! then
    wait-bus-priority-ack
  ;

  \ Get CPU 1 bus priority
  : proc1-bus-priority@ ( -- priority )
    BUSCTRL_BUS_PRIORITY_PROC1 BUSCTRL_BUS_PRIORITY bit@
  ;
  
  \ Set CPU 0 bus priority
  : proc0-bus-priority! ( priority -- )
    BUSCTRL_BUS_PRIORITY_PROC0 BUSCTRL_BUS_PRIORITY rot if bis! else bic! then
    wait-bus-priority-ack
  ;

  \ Get CPU 0 bus priority
  : proc0-bus-priority@ ( -- priority )
    BUSCTRL_BUS_PRIORITY_PROC0 BUSCTRL_BUS_PRIORITY bit@
  ;

end-module

reboot
