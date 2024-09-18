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

compile-to-flash

begin-module dma-pool
  
  slock import
  dma import

  \ No DMA channels available exception
  : x-no-dma-channels-available ( -- ) cr ." no DMA channels available" ;

  \ DMA channel is already free
  : x-dma-channel-already-free ( -- ) cr ." DMA channel is already free" ;
  
  \ No DMA timers available exception
  : x-no-dma-timers-available ( -- ) cr ." no DMA timers available" ;

  \ DMA timer is already free
  : x-dma-timer-already-free ( -- ) cr ." DMA timer is already free" ;
  
  begin-module dma-pool-internal

    \ DMA pool simple lock
    slock-size buffer: dma-pool-slock
    
    \ DMA pool bits
    variable dma-pool-bits

    \ DMA timer pool bits
    variable dma-timer-pool-bits

  end-module> import

  \ Allocate a DMA channel
  : allocate-dma ( -- channel )
    [:
      dma-internal::dma-count 0 ?do
        i bit dma-pool-bits bit@ not if
          i bit dma-pool-bits bis!
          i unloop exit
        then
      loop
      ['] x-no-dma-channels-available ?raise
    ;] dma-pool-slock with-slock
  ;

  \ Free a DMA channel
  : free-dma ( channel -- )
    dup dma-internal::validate-dma
    [:
      dup bit dma-pool-bits bit@ averts x-dma-channel-already-free
      bit dma-pool-bits bic!
    ;] dma-pool-slock with-slock
  ;
  
  \ Allocate a DMA timer
  : allocate-timer ( -- timer )
    [:
      dma-internal::timer-count 0 ?do
        i bit dma-timer-pool-bits bit@ not if
          i bit dma-timer-pool-bits bis!
          i unloop exit
        then
      loop
      ['] x-no-dma-timers-available ?raise
    ;] dma-pool-slock with-slock
  ;

  \ Free a DMA timer
  : free-timer ( timer -- )
    dup dma-internal::validate-timer
    [:
      dup bit dma-timer-pool-bits bit@ averts x-dma-timer-already-free
      bit dma-timer-pool-bits bic!
    ;] dma-pool-slock with-slock
  ;
  
  \ Initialize DMA pool
  : init-dma-pool ( -- )
    dma-pool-slock init-slock
    0 dma-pool-bits !
    0 dma-timer-pool-bits !
  ;
  
end-module

\ Initialize
: init ( -- )
  init
  dma-pool::init-dma-pool
;

reboot
