\ Copyright (c) 2025 Travis Bemann
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

begin-module int-overload-test

  interrupt import
  task import
  gpio import
  pin import
  int-overload import

  int-adapter-size buffer: my-gpio-adapter
  int-adapter-size buffer: my-dma-adapter
  int-overload-size buffer: my-gpio14-overload
  int-overload-size buffer: my-gpio15-overload

  10 constant DMA_IRQ_0
  21 constant IO_IRQ_BANK0

  16 constant vector-offset
  
  DMA_IRQ_0 vector-offset + constant dma-irq-0-vector
  IO_IRQ_BANK0 vector-offset + constant io-irq-bank0-vector

  0 bit constant gpio14-gpio15-low-flag
  1 bit constant gpio14-low-flag
  2 bit constant gpio15-low-flag

  variable recv-task
  variable mailbox

  : recv-notify ( -- )
    begin
      0 0 wait-notify-set
      dup gpio14-gpio15-low-flag and if cr ." GPIO14 or GPIO15 low" then
      dup gpio14-low-flag and if cr ." GPIO14 low" then
      gpio15-low-flag and if cr ." GPIO15 low" then
    again
  ;

  : gpio14-gpio15-low-handler ( -- )
    
  ;
  
  : run-test ( -- )
    0 ['] recv-notify 256 256 512 spawn recv-task !
    mailbox 1 recv-task @ config-notify
    recv-task @ run
    dma-irq-0-vector my-dma-adapter register-int-adapter
    io-irq-bank0-vector my-gpio-adapter register-int-adapter
    [:
      14 PROC0_INTS_GPIO_EDGE_LOW@
      15 PROC0_INTS_GPIO_EDGE_LOW@ or if
        gpio14-gpio15-low-flag ['] or 0 recv-task @ notify-update
      then
      14 INTR_GPIO_EDGE_LOW!
      15 INTR_GPIO_EDGE_LOW!
    ;] io-irq-bank0-vector vector!
    [:
      14 PROC0_INTS_GPIO_EDGE_LOW@ if
        gpio14-low-flag ['] or 0 recv-task @ notify-update
      then
    ;] io-irq-bank0-vector my-gpio14-overload register-int-overload
    [:
      15 PROC0_INTS_GPIO_EDGE_LOW@ if
        gpio15-low-flag ['] or 0 recv-task @ notify-update
      then
    ;] io-irq-bank0-vector my-gpio15-overload register-int-overload
    14 pull-up-pin
    15 pull-up-pin
    14 input-pin
    15 input-pin
    true 14 PROC0_INTE_GPIO_EDGE_LOW!
    true 15 PROC0_INTE_GPIO_EDGE_LOW!
    iO_IRQ_BANK0 NVIC_ISER_SETENA!
  ;

  : replace-base-handler ( -- )
    [:
      14 INTR_GPIO_EDGE_LOW!
      15 INTR_GPIO_EDGE_LOW!
    ;] io-irq-bank0-vector vector!
  ;

  : unregister-gpio14-overload ( -- )
    my-gpio14-overload unregister-int-overload
  ;

  : unregister-gpio15-overload ( -- )
    my-gpio15-overload unregister-int-overload
  ;

  : unregister-gpio-adapter ( -- )
    my-gpio-adapter unregister-int-adapter
  ;
  
end-module
