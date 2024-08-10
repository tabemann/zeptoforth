\ Copyright (c) 2021-2024 Travis Bemann
\ Copyright (c) 2023 Rob Probin
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

begin-module gpio
  
  begin-module gpio-internal
  
    \ IO bank 0 base
    $40028000 constant IO_BANK0_BASE

    \ PADS bank 0 base
    $40038000 constant PADS_BANK0_BASE

    \ SIO base
    $D0000000 constant SIO_BASE

    \ GPIO status registers
    : GPIO_STATUS ( index -- addr ) 3 lshift IO_BANK0_BASE + ;

    \ GPIO control registers
    : GPIO_CTRL ( index -- addr ) 3 lshift [ IO_BANK0_BASE 4 + ] literal + ;

    \ Pads registers
    : PAD ( index -- addr ) 2 lshift [ PADS_BANK0_BASE 4 + ] literal + ;

    \ GPIO raw interrupt registers
    IO_BANK0_BASE $0F0 + constant INTR0
    
    \ GPIO processor 0 interrupt enable registers
    IO_BANK0_BASE $100 + constant PROC0_INTE0

    \ GPIO processor 0 interrupt force registers
    IO_BANK0_BASE $110 + constant PROC0_INTF0

    \ GPIO processor 0 interrupt status registers
    IO_BANK0_BASE $120 + constant PROC0_INTS0

    \ GPIO processor 1 interrupt enable registers
    IO_BANK0_BASE $130 + constant PROC1_INTE0

    \ GPIO processor 1 interrupt force registers
    IO_BANK0_BASE $140 + constant PROC1_INTF0

    \ GPIO processor 1 interrupt status registers
    IO_BANK0_BASE $150 + constant PROC1_INTS0

    \ Pads bank 0 voltage select register
    PADS_BANK0_BASE $00 + constant PADS_BANK0_VOLTAGE_SELECT

    \ Level low bit offset
    0 constant LEVEL_LOW

    \ Level high bit offset
    1 constant LEVEL_HIGH

    \ Edge low bit offset
    2 constant EDGE_LOW

    \ Edge high bit offset
    3 constant EDGE_HIGH

  end-module> import

  \ Normal control
  0 constant CTRL_NORMAL

  \ Invert control
  1 constant CTRL_INVERT

  \ Force low / disable control
  2 constant CTRL_FORCE_LOW

  \ Force high / enable control
  3 constant CTRL_FORCE_HIGH

  \ Voltage set to 3.3V
  0 constant VOLTAGE_3.3V

  \ Voltage set to 1.8V
  1 constant VOLTAGE_1.8V

  \ Drive strength set to 2mA
  0 constant DRIVE_2MA

  \ Drive strength set to 4mA
  1 constant DRIVE_4MA

  \ Drive strength set to 8mA
  2 constant DRIVE_8MA

  \ Drive strength set to 12mA
  3 constant DRIVE_12MA

  \ SWCLK index
  30 constant PAD_SWCLK

  \ SWD index
  31 constant PAD_SWD

  \ IO IRQ BANK0
  21 constant IO_IRQ_BANK0
  
  \ GPIO input register
  SIO_BASE $004 + constant GPIO_IN

  \ High GPIO input register
  SIO_BASE $008 + constant GPIO_HI_IN
  
  \ GPIO output register
  SIO_BASE $010 + constant GPIO_OUT

  \ High GPIO output register
  SIO_BASE $014 + constant GPIO_HI_OUT

  \ GPIO output atomic bit-set register
  SIO_BASE $018 + constant GPIO_OUT_SET

  \ High GPIO output atomic bit-set register
  SIO_BASE $01C + constant GPIO_HI_OUT_SET
  
  \ GPIO output atomic bit-clear register
  SIO_BASE $020 + constant GPIO_OUT_CLR

  \ High GPIO output atomic bit-clear register
  SIO_BASE $024 + constant GPIO_HI_OUT_CLR

  \ GPIO output atomic bit-xor register
  SIO_BASE $028 + constant GPIO_OUT_XOR

  \ High GPIO output atomic bit-xor register
  SIO_BASE $02C + constant GPIO_HI_OUT_XOR
    
  \ GPIO output enable register
  SIO_BASE $030 + constant GPIO_OE

  \ High GPIO output enable register
  SIO_BASE $034 + constant GPIO_HI_OE

  \ GPIO output enable atomic bit-set register
  SIO_BASE $038 + constant GPIO_OE_SET

  \ High GPIO output enable atomic bit-set register
  SIO_BASE $03C + constant GPIO_HI_OE_SET
  
  \ GPIO output enable atomic bit-clear register
  SIO_BASE $040 + constant GPIO_OE_CLR

  \ High GPIO output enable atomic bit-clear register
  SIO_BASE $044 + constant GPIO_HI_OE_CLR
  
  \ GPIO output enable atomic bit-xor register
  SIO_BASE $048 + constant GPIO_OE_XOR

  \ High GPIO output enable atomic bit-xor register
  SIO_BASE $050 + constant GPIO_HI_OE_XOR

  \ Get interrupt to processors, after override is applied
  : GPIO_STATUS_IRQTOPROC@ ( index -- flag )
    26 bit swap GPIO_STATUS bit@
  ;

  \ Get interrupt from pad, before override is applied
  : GPIO_STATUS_IRQFROMPAD@ ( index -- flag )
    24 bit swap GPIO_STATUS bit@
  ;

  \ Get input signal to peripheral, after override is applied
  : GPIO_STATUS_INTOPERI@ ( index -- flag )
    19 bit swap GPIO_STATUS bit@
  ;

  \ Get input signal from pad, before override is applied
  : GPIO_STATUS_INFROMPAD@ ( index -- flag )
    17 bit swap GPIO_STATUS bit@
  ;

  \ Get output enable to pad, after register override is applied
  : GPIO_STATUS_OETOPAD@ ( index -- flag )
    13 bit swap GPIO_STATUS bit@
  ;

  \ Get output enable from selected peripheral, before register override is
  \ applied
  : GPIO_STATUS_OEFROMPERI@ ( index -- flag )
    12 bit swap GPIO_STATUS bit@
  ;

  \ Get output signal to pad, after register override is applied
  : GPIO_STATUS_OUTTOPAD@ ( index -- flag )
    9 bit swap GPIO_STATUS bit@
  ;

  \ Get output signal from selected peripheral, before register override is
  \ applied
  : GPIO_STATUS_OUTFROMPERI@ ( index -- flag )
    8 bit swap GPIO_STATUS bit@
  ;

  \ Set interrupt state for GPIO
  : GPIO_CTRL_IRQOVER! ( control index -- )
    GPIO_CTRL dup @ [ 3 28 lshift ] literal bic rot 3 and 28 lshift or swap !
  ;

  \ Set peripheral input state for GPIO
  : GPIO_CTRL_INOVER! ( control index -- )
    GPIO_CTRL dup @ [ 3 16 lshift ] literal bic rot 3 and 16 lshift or swap !
  ;

  \ Set output enable state for GPIO
  : GPIO_CTRL_OEOVER! ( control index -- )
    GPIO_CTRL dup @ [ 3 12 lshift ] literal bic rot 3 and 12 lshift or swap !
  ;

  \ Set output state for GPIO
  : GPIO_CTRL_OUTOVER! ( control index -- )
    GPIO_CTRL dup @ [ 3 8 lshift ] literal bic rot 3 and 8 lshift or swap !
  ;

  \ Set the function select for GPIO
  : GPIO_CTRL_FUNCSEL! ( function index -- )
    GPIO_CTRL dup @ $1F bic rot $1F and or swap !
  ;

  \ Get interrupt state for GPIO
  : GPIO_CTRL_IRQOVER@ ( index -- control )
    GPIO_CTRL @ 28 rshift 3 and
  ;

  \ Get peripheral input state for GPIO
  : GPIO_CTRL_INOVER@ ( index -- control )
    GPIO_CTRL @ 16 rshift 3 and
  ;

  \ Get output enable state for GPIO
  : GPIO_CTRL_OEOVER@ ( index -- control )
    GPIO_CTRL @ 12 rshift 3 and
  ;

  \ Get output state for GPIO
  : GPIO_CTRL_OUTOVER@ ( index -- control )
    GPIO_CTRL @ 8 rshift 3 and
  ;

  \ Set the function select for GPIO
  : GPIO_CTRL_FUNCSEL@ ( index -- function )
    GPIO_CTRL @ $1F and
  ;

  \ Clear a raw edge low interrupt
  : INTR_GPIO_EDGE_LOW! ( index -- )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and INTR0 + !
  ;
  
  \ Clear a raw edge high interrupt
  : INTR_GPIO_EDGE_HIGH! ( index -- )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and INTR0 + !
  ;
  
  \ Get an level low interrupt enable for processor 0
  : INTR_GPIO_LEVEL_LOW@ ( index -- enable )
    dup 7 and 2 lshift bit swap 1 rshift $c and INTR0 + bit@
  ;
  
  \ Get an level high interrupt enable for processor 0
  : INTR_GPIO_LEVEL_HIGH@ ( index -- enable )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and INTR0 + bit@
  ;

  \ Get an edge low interrupt enable for processor 0
  : INTR_GPIO_EDGE_LOW@ ( index -- enable )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and INTR0 + bit@
  ;
  
  \ Get an edge high interrupt enable for processor 0
  : INTR_GPIO_EDGE_HIGH@ ( index -- enable )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and INTR0 + bit@
  ;

  \ Set an level low interrupt enable for processor 0
  : PROC0_INTE_GPIO_LEVEL_LOW! ( enable index -- )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC0_INTE0 +
    rot if bis! else bic! then
  ;

  \ Set an level high interrupt enable for processor 0
  : PROC0_INTE_GPIO_LEVEL_HIGH! ( enable index -- )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC0_INTE0 +
    rot if bis! else bic! then
  ;

  \ Set an edge low interrupt enable for processor 0
  : PROC0_INTE_GPIO_EDGE_LOW! ( enable index -- )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC0_INTE0 +
    rot if bis! else bic! then
  ;

  \ Set an edge high interrupt enable for processor 0
  : PROC0_INTE_GPIO_EDGE_HIGH! ( enable index -- )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC0_INTE0 +
    rot if bis! else bic! then
  ;

  \ Get an level low interrupt enable for processor 0
  : PROC0_INTE_GPIO_LEVEL_LOW@ ( index -- enable )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC0_INTE0 + bit@
  ;
  
  \ Get an level high interrupt enable for processor 0
  : PROC0_INTE_GPIO_LEVEL_HIGH@ ( index -- enable )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC0_INTE0 + bit@
  ;

  \ Get an edge low interrupt enable for processor 0
  : PROC0_INTE_GPIO_EDGE_LOW@ ( index -- enable )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC0_INTE0 + bit@
  ;
  
  \ Get an edge high interrupt enable for processor 0
  : PROC0_INTE_GPIO_EDGE_HIGH@ ( index -- enable )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC0_INTE0 + bit@
  ;

  \ Set an level low interrupt force for processor 0
  : PROC0_INTF_GPIO_LEVEL_LOW! ( force index -- )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC0_INTF0 +
    rot if bis! else bic! then
  ;

  \ Set an level high interrupt force for processor 0
  : PROC0_INTF_GPIO_LEVEL_HIGH! ( force index -- )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC0_INTF0 +
    rot if bis! else bic! then
  ;

  \ Set an edge low interrupt force for processor 0
  : PROC0_INTF_GPIO_EDGE_LOW! ( force index -- )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC0_INTF0 +
    rot if bis! else bic! then
  ;

  \ Set an edge high interrupt force for processor 0
  : PROC0_INTF_GPIO_EDGE_HIGH! ( force index -- )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC0_INTF0 +
    rot if bis! else bic! then
  ;

  \ Get an level low interrupt force for processor 0
  : PROC0_INTF_GPIO_LEVEL_LOW@ ( index -- force )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC0_INTF0 + bit@
  ;
  
  \ Get an level high interrupt force for processor 0
  : PROC0_INTS_GPIO_LEVEL_HIGH@ ( index -- force )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC0_INTF0 + bit@
  ;

  \ Get an edge low interrupt force for processor 0
  : PROC0_INTF_GPIO_EDGE_LOW@ ( index -- force )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC0_INTF0 + bit@
  ;
  
  \ Get an edge high interrupt force for processor 0
  : PROC0_INTF_GPIO_EDGE_HIGH@ ( index -- force )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC0_INTF0 + bit@
  ;

  \ Get an level low interrupt status for processor 0
  : PROC0_INTS_GPIO_LEVEL_LOW@ ( index -- status )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC0_INTS0 + bit@
  ;
  
  \ Get an level high interrupt status for processor 0
  : PROC0_INTS_GPIO_LEVEL_HIGH@ ( index -- status )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC0_INTS0 + bit@
  ;

  \ Get an edge low interrupt status for processor 0
  : PROC0_INTS_GPIO_EDGE_LOW@ ( index -- status )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC0_INTS0 + bit@
  ;
  
  \ Get an edge high interrupt status for processor 0
  : PROC0_INTS_GPIO_EDGE_HIGH@ ( index -- status )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC0_INTS0 + bit@
  ;
  
  \ Set an level low interrupt enable for processor 1
  : PROC1_INTE_GPIO_LEVEL_LOW! ( enable index -- )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC1_INTE0 +
    rot if bis! else bic! then
  ;

  \ Set an level high interrupt enable for processor 1
  : PROC1_INTE_GPIO_LEVEL_HIGH! ( enable index -- )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC1_INTE0 +
    rot if bis! else bic! then
  ;

  \ Set an edge low interrupt enable for processor 1
  : PROC1_INTE_GPIO_EDGE_LOW! ( enable index -- )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC1_INTE0 +
    rot if bis! else bic! then
  ;

  \ Set an edge high interrupt enable for processor 1
  : PROC1_INTE_GPIO_EDGE_HIGH! ( enable index -- )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC1_INTE0 +
    rot if bis! else bic! then
  ;

  \ Get an level low interrupt enable for processor 1
  : PROC1_INTE_GPIO_LEVEL_LOW@ ( index -- enable )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC1_INTE0 + bit@
  ;
  
  \ Get an level high interrupt enable for processor 1
  : PROC1_INTE_GPIO_LEVEL_HIGH@ ( index -- enable )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC1_INTE0 + bit@
  ;

  \ Get an edge low interrupt enable for processor 1
  : PROC1_INTE_GPIO_EDGE_LOW@ ( index -- enable )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC1_INTE0 + bit@
  ;
  
  \ Get an edge high interrupt enable for processor 1
  : PROC1_INTE_GPIO_EDGE_HIGH@ ( index -- enable )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC1_INTE0 + bit@
  ;

  \ Set an level low interrupt force for processor 1
  : PROC1_INTF_GPIO_LEVEL_LOW! ( force index -- )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC1_INTF0 +
    rot if bis! else bic! then
  ;

  \ Set an level high interrupt force for processor 1
  : PROC1_INTF_GPIO_LEVEL_HIGH! ( force index -- )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC1_INTF0 +
    rot if bis! else bic! then
  ;

  \ Set an edge low interrupt force for processor 1
  : PROC1_INTF_GPIO_EDGE_LOW! ( force index -- )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC1_INTF0 +
    rot if bis! else bic! then
  ;

  \ Set an edge high interrupt force for processor 1
  : PROC1_INTF_GPIO_EDGE_HIGH! ( force index -- )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC1_INTF0 +
    rot if bis! else bic! then
  ;

  \ Get an level low interrupt force for processor 1
  : PROC1_INTF_GPIO_LEVEL_LOW@ ( index -- force )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC1_INTF0 + bit@
  ;
  
  \ Get an level high interrupt force for processor 1
  : PROC1_INTS_GPIO_LEVEL_HIGH@ ( index -- force )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC1_INTF0 + bit@
  ;

  \ Get an edge low interrupt force for processor 1
  : PROC1_INTF_GPIO_EDGE_LOW@ ( index -- force )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC1_INTF0 + bit@
  ;
  
  \ Get an edge high interrupt force for processor 1
  : PROC1_INTF_GPIO_EDGE_HIGH@ ( index -- force )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC1_INTF0 + bit@
  ;

  \ Get an level low interrupt status for processor 1
  : PROC1_INTS_GPIO_LEVEL_LOW@ ( index -- status )
    dup 7 and 2 lshift bit swap 1 rshift $c and PROC1_INTS0 + bit@
  ;
  
  \ Get an level high interrupt status for processor 1
  : PROC1_INTS_GPIO_LEVEL_HIGH@ ( index -- status )
    dup 7 and 2 lshift 1 + bit swap 1 rshift $c and PROC1_INTS0 + bit@
  ;

  \ Get an edge low interrupt status for processor 1
  : PROC1_INTS_GPIO_EDGE_LOW@ ( index -- status )
    dup 7 and 2 lshift 2 + bit swap 1 rshift $c and PROC1_INTS0 + bit@
  ;
  
  \ Get an edge high interrupt status for processor 1
  : PROC1_INTS_GPIO_EDGE_HIGH@ ( index -- status )
    dup 7 and 2 lshift 3 + bit swap 1 rshift $c and PROC1_INTS0 + bit@
  ;

  \ Select voltage for pads
  : PADS_BANK0_VOLTAGE_SELECT! ( voltage -- )
    1 and PADS_BANK0_VOLTAGE_SELECT !
  ;

  \ Get voltage for pads
  : PADS_BANK0_VOLTAGE_SELECT@ ( -- voltage )
    PADS_BANK0_VOLTAGE_SELECT @ 1 and
  ;

  \ Set output disable
  : PADS_BANK0_OD! ( disable index -- )
    7 bit swap PAD rot if bis! else bic! then
  ;

  \ Set input enable
  : PADS_BANK0_IE! ( enable index -- )
    6 bit swap PAD rot if bis! else bic! then
  ;

  \ Set drive strength
  : PADS_BANK0_DRIVE! ( strength index -- )
    PAD dup @ [ 3 4 lshift ] literal bic rot 3 and 4 lshift or swap !
  ;

  \ Set pull up enable
  : PADS_BANK0_PUE! ( enable index -- )
    3 bit swap PAD rot if bis! else bic! then
  ;

  \ Set pull down enable
  : PADS_BANK0_PDE! ( enable index -- )
    2 bit swap PAD rot if bis! else bic! then
  ;

  \ Set schmitt trigger
  : PADS_BANK0_SCHMITT! ( enable index -- )
    1 bit swap PAD rot if bis! else bic! then
  ;

  \ Set slew rate control
  : PADS_BANK0_SLEWFAST! ( fast index -- )
    0 bit swap PAD rot if bis! else bic! then
  ;  

end-module

\ Reboot
reboot
