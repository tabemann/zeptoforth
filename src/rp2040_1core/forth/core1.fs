\ Copyright (c) 2021-2025 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module core1

  internal import
  interrupt import

  begin-module core1-internal
      
    \ SIO base
    $D0000000 constant SIO_BASE  

    \ Signal an event to the other core
    : sev ( -- ) [inlined] [ undefer-lit %1011111101000000 h, ] ;
    
    \ Wait for an event
    : wfe ( -- ) [inlined] [ undefer-lit %1011111100100000 h, ] ;
    
    \ SIO processor 0 IRQ
    15 constant SIO_IRQ_PROC0
    
    \ SIO processor 1 RIQ
    16 constant SIO_IRQ_PROC1

    \ FIFO status register; note that core 0 can see the read side of the 1 -> 0
    \ FIFO and the write side of the 0 -> 1 FIFO; the converse is true of core 1
    SIO_BASE $050 + constant FIFO_ST
    
    \ Sticky flag indicating the RX FIFO was read when empty; write to clear
    3 bit constant FIFO_ST_ROE
    
    \ Sticky flag indicating the TX FIFO was written when full; write to clear
    2 bit constant FIFO_ST_WOF
    
    \ Set if this core's TX FIFO is not full
    1 bit constant FIFO_ST_RDY
    
    \ Set if this core's RX FIFO is not empty
    0 bit constant FIFO_ST_VLD
    
    \ Write access from this core's TX FIFO
    SIO_BASE $054 + constant FIFO_WR
    
    \ Read access to this core's RX FIFO
    SIO_BASE $058 + constant FIFO_RD
    
    \ Drain a multicore FIFO
    : fifo-drain ( -- )
      begin FIFO_ST_VLD FIFO_ST bit@ while FIFO_RD @ drop repeat sev
    ;
    
    \ Blocking FIFO push
    : fifo-push-blocking ( x -- )
      begin FIFO_ST_RDY FIFO_ST bit@ until FIFO_WR ! sev
    ;
    
    \ Blocking FIFO pop
    : fifo-pop-blocking ( -- x )
      begin FIFO_ST_VLD FIFO_ST bit@ until FIFO_RD @ sev
    ;
    
    \ Attempt to send data on a FIFO and confirm that the same data is sent
    \ back.
    : fifo-push-confirm ( x -- confirmed? )
      dup fifo-push-blocking fifo-pop-blocking =
    ;

  end-module> import
  
  \ Launch core 1
  : launch-core1 ( code-addr rstack-addr vector-table-addr -- )
    SIO_IRQ_PROC0 NVIC_ICER_CLRENA!
    6 0 do
      i case
        0 of fifo-drain 0 endof
        1 of fifo-drain 0 endof
	2 of 1 endof
	3 of dup endof
	4 of over endof
	5 of 2 pick 1+ endof
      endcase
      fifo-push-confirm if
	1
      else
	i negate
      then
    +loop
    2drop drop
    $00 SIO_IRQ_PROC0 NVIC_IPR_IP!
    SIO_IRQ_PROC0 NVIC_ISER_SETENA!
  ;
  
end-module

\ Reboot
reboot