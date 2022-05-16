\ Copyright (c) 2021-2022 Travis Bemann
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

begin-module pio

  internal import
  pin import

  \ On/off constants
  true constant on
  false constant off

  \ Direction constants
  true constant right
  false constant left
  
  \ PIO0 base register
  $50200000 constant PIO0
  
  \ PIO1 base register
  $50300000 constant PIO1

  \ State machine out of range exception
  : x-sm-out-of-range ( -- ) ." state machine out of range" cr ;

  \ PIO out of range exception
  : x-pio-out-of-range ( -- ) ." PIO out of range" cr ;

  \ Too many instructions exception
  : x-too-many-instructions ( -- ) ." too many PIO instructions" cr ;

  \ Address out of range exception
  : x-address-out-of-range ( -- ) ." PIO address out of range" cr ;

  \ Too many pins exception
  : x-too-many-pins ( -- ) ." too many pins" cr ;

  \ Pin out of range exception
  : x-pin-out-of-range ( -- ) ." pin out of range" cr ;

  \ Clock divisor out of range exception
  : x-clkdiv-out-of-range ( -- ) ." PIO clock divisor out of range" cr ;

  \ IRQ out of range exception
  : x-irq-out-of-range ( -- ) ." IRQ out of range" cr ;

  \ Interrupt out of range exception
  : x-interrupt-out-of-range ( -- ) ." interrupt out of range" cr ;  

  \ Buffer threshold out of range exception
  : x-threshold-out-of-range ( -- ) ." threshold out of range" cr ;
  
  begin-module pio-internal

    \ Validate a PIO
    : validate-pio ( pio -- )
      dup PIO0 = swap PIO1 = or averts x-pio-out-of-range
    ;

    \ Validate a state machine
    : validate-sm ( state-machine -- ) 4 u< averts x-sm-out-of-range ;

    \ Validate a state machine and PIO
    : validate-sm-pio ( state-machine pio -- ) validate-pio validate-sm ;

    \ Validate a pin
    : validate-pin ( pin -- ) 30 u< averts x-pin-out-of-range ;

    \ Validate an IRQ
    : validate-irq ( irq -- ) 2 u< averts x-irq-out-of-range ;

    \ Validate interrupt bits
    : validate-interrupt ( bits -- )
      $FFF bic 0= averts x-interrupt-out-of-range
    ;

    \ Create a multibit field setter and getter for a PIO
    : make-pio-field ( mask lsb "register" "setter" "getter" -- )
      ' >r swap 2dup
      token dup 0<> averts x-token-expected
      start-compile
      lit, postpone swap lit, postpone swap r@ lit, postpone execute
      postpone field!
      visible end-compile,
      token dup 0<> averts x-token-expected
      start-compile
      lit, postpone swap lit, postpone swap r> lit, postpone execute
      postpone field@
      visible end-compile,
    ;

    \ Create a multibit field getter for a PIO
    : make-pio-field-getter ( mask lsb "register" "getter" -- )
      ' >r swap
      token dup 0<> averts x-token-expected
      start-compile
      lit, postpone swap lit, postpone swap r> lit, postpone execute
      postpone field@
      visible end-compile,
    ;

    \ Create a single bit setter and getter for a state machine
    : make-bit ( bit "register" "setter" "getter" -- )
      ' >r dup
      token dup 0<> averts x-token-expected
      start-compile
      r@ lit, postpone execute postpone >r lit, postpone swap
      postpone 0<> postpone if
      postpone r@ postpone @ postpone or postpone else
      postpone r@ postpone @ postpone swap postpone bic postpone then
      postpone r> postpone !
      visible end-compile,
      token dup 0<> averts x-token-expected
      start-compile
      r> lit, postpone execute postpone bit@
      visible end-compile,
    ;

    \ Create a single bit getter for a state machine
    : make-bit-getter ( bit "register" "getter" -- )
      ' >r
      token dup 0<> averts x-token-expected
      start-compile
      r> lit, postpone execute postpone bit@
      visible end-compile,
    ;

    \ Create a multibit field setter and getter for a state machine
    : make-sm-field ( mask lsb "register" "setter" "getter" -- )
      ' >r swap 2dup
      token dup 0<> averts x-token-expected
      start-compile
      lit, postpone -rot lit, postpone -rot r@ lit, postpone execute
      postpone field!
      visible end-compile,
      token dup 0<> averts x-token-expected
      start-compile
      lit, postpone -rot lit, postpone -rot r> lit, postpone execute
      postpone field@
      visible end-compile,
    ;

  end-module> import

  \ PIO0 IRQ0 index
  7 constant PIO0_IRQ0

  \ PIO0 IRQ1 index
  8 constant PIO0_IRQ1
  
  \ PIO1 IRQ0 index
  9 constant PIO1_IRQ0

  \ PIO1 IRQ1 index
  10 constant PIO1_IRQ1

  begin-module pio-registers

    \ PIO control register
    : CTRL ( pio -- addr ) [inlined] $000 + ;
    
    \ Restart state machines' clock dividers LSB
    8 constant CTRL_CLKDIV_RESTART_LSB
    
    \ Restart state machines' clock dividers mask
    $F 8 lshift constant CTRL_CLKDIV_RESTART_MASK

    \ Set/get restart state machines' clock dividers
    CTRL_CLKDIV_RESTART_MASK CTRL_CLKDIV_RESTART_LSB
    make-pio-field CTRL CTRL_CLKDIV_RESTART! CTRL_CLKDIV_RESTART@
    
    \ Clear state machine internal state LSB
    4 constant CTRL_SM_RESTART_LSB
    
    \ Clear state machine internal state mask
    $F 4 lshift constant CTRL_SM_RESTART_MASK

    \ Set/get clear state machine internal state
    CTRL_SM_RESTART_MASK CTRL_SM_RESTART_LSB
    make-pio-field CTRL CTRL_SM_RESTART! CTRL_SM_RESTART@

    \ Enable/disable state machines LSB
    0 constant CTRL_SM_ENABLE_LSB

    \ Enable/disable state machines mask
    $F 0 lshift constant CTRL_SM_ENABLE_MASK

    \ Set/get enable/disable state machines
    CTRL_SM_ENABLE_MASK CTRL_SM_ENABLE_LSB
    make-pio-field CTRL CTRL_SM_ENABLE! CTRL_SM_ENABLE@
    
    \ FIFO status register
    : FSTAT ( pio -- addr ) [inlined] $004 + ;

    \ State machine TX FIFO is empty LSB
    24 constant FSTAT_TXEMPTY_LSB

    \ State machine TX FIFO is empty mask
    $F 24 lshift constant FSTAT_TXEMPTY_MASK

    \ Get state machine TX FIFO is empty
    FSTAT_TXEMPTY_MASK FSTAT_TXEMPTY_LSB
    make-pio-field-getter FSTAT FSTAT_TXEMPTY@

    \ State machine TX FIFO is full LSB
    16 constant FSTAT_TXFULL_LSB

    \ State machine TX FIFO is full mask
    $F 16 lshift constant FSTAT_TXFULL_MASK

    \ Get state machine TX FIFO is full
    FSTAT_TXFULL_MASK FSTAT_TXFULL_LSB
    make-pio-field-getter FSTAT FSTAT_TXFULL@

    \ State machine RX FIFO is empty LSB
    8 constant FSTAT_RXEMPTY_LSB

    \ State machine RX FIFO is empty mask
    $F 8 lshift constant FSTAT_RXEMPTY_MASK

    \ Get state machine RX FIFO is empty
    FSTAT_RXEMPTY_MASK FSTAT_RXEMPTY_LSB
    make-pio-field-getter FSTAT FSTAT_RXEMPTY@

    \ State machine RX FIFO is full LSB
    0 constant FSTAT_RXFULL_LSB

    \ State machine RX FIFO is full mask
    $F 0 lshift constant FSTAT_RXFULL_MASK

    \ Set/get state machine RX FIFO is full
    FSTAT_RXFULL_MASK FSTAT_RXFULL_LSB
    make-pio-field-getter FSTAT FSTAT_RXFULL@
    
    \ FIFO debug register
    : FDEBUG ( pio -- addr ) [inlined] $008 + ;
    
    \ State machine has stalled on empty TX FIFO during a blocking pull or an
    \ OUT with autopull enabled. Write 1 to clear.
    24 constant FDEBUG_TXSTALL_LSB

    \ State machine has stalled on empty TX FIFO during a blocking pull or an
    \ OUT with autopull enabled. Write 1 to clear.
    $F 24 lshift constant FDEBUG_TXSTALL_MASK

    \ Set/get state machine has stalled on empty TX FIFO during a blocking pull
    \ or an OUT with autopull enabled. Write 1 to clear.
    FDEBUG_TXSTALL_MASK FDEBUG_TXSTALL_LSB
    make-pio-field FDEBUG FDEBUG_TXSTALL! FDEBUG_TXSTALL@

    \ State machine TX FIFO overflow has occurred. Write 1 to clear.
    16 constant FDEBUG_TXOVER_LSB

    \ State machine TX FIFO overflow has occurred. Write 1 to clear.
    $F 16 lshift constant FDEBUG_TXOVER_MASK

    \ Set/get state machine TX FIFO overflow has occurred. Write 1 to clear.
    FDEBUG_TXOVER_MASK FDEBUG_TXOVER_LSB
    make-pio-field FDEBUG FDEBUG_TXOVER! FDEBUG_TXOVER@

    \ State machine RX FIFO underflow has occurred. Write 1 to clear.
    8 constant FDEBUG_RXUNDER_LSB

    \ State machine RX FIFO underflow has occurred. Write 1 to clear.
    $F 8 lshift constant FDEBUG_RXUNDER_MASK

    \ Set/get state machine RX FIFO underflow has occurred. Write 1 to clear.
    FDEBUG_RXUNDER_MASK FDEBUG_RXUNDER_LSB
    make-pio-field FDEBUG FDEBUG_RXUNDER! FDEBUG_RXUNDER@

    \ State machine has stalled on a full RX FIFO during a blocking PUSH or
    \ an IN with autopush enabled. Write 1 to clear.
    0 constant FDEBUG_RXSTALL_LSB

    \ State machine has stalled on a full RX FIFO during a blocking PUSH or
    \ an IN with autopush enabled. Write 1 to clear.
    $F 0 lshift constant FDEBUG_RXSTALL_MASK

    \ Set/get state machine has stalled on a full RX FIFO during a blocking PUSH
    \ or an IN with autopush enabled. Write 1 to clear.
    FDEBUG_RXSTALL_MASK FDEBUG_RXSTALL_LSB
    make-pio-field FDEBUG FDEBUG_RXSTALL! FDEBUG_RXSTALL@

    \ FIFO levels
    : FLEVEL ( pio -- addr ) [inlined] $00C + ;

    \ RX FIFO levels
    : FLEVEL_RX_LSB ( index -- lsb ) [inlined] 8 * 4 + ;

    \ RX FIFO masks
    : FLEVEL_RX_MASK ( index -- mask ) [inlined] $F swap FLEVEL_RX_LSB lshift ;

    \ Get RX FIFO levels
    : FLEVEL_RX@ ( index pio -- level )
      swap dup FLEVEL_RX_MASK swap FLEVEL_RX_LSB rot FLEVEL field@
    ;
    
    \ TX FIFO levels
    : FLEVEL_TX_LSB ( index -- lsb ) [inlined] 8 * ;

    \ TX FIFO masks
    : FLEVEL_TX_MASK ( index -- mask ) [inlined] $F swap FLEVEL_TX_LSB lshift ;

    \ Get TX FIFO levels
    : FLEVEL_TX@ ( index pio -- level )
      swap dup FLEVEL_TX_MASK swap FLEVEL_TX_LSB rot FLEVEL field@
    ;

    \ Direct write access to the TX FIFO for this state machine
    : TXF ( state-machine pio -- addr ) [inlined] swap cells + $010 + ;

    \ Direct read access to the RX FIFO for this state machine
    : RXF ( state-machine pio -- addr ) [inlined] swap cells + $020 + ;

    \ IRQ register (write 1 to clear)
    : IRQ ( pio -- addr ) [inlined] $030 + ;

    \ IRC force register (writing one to each of the bits forcibly asserts the
    \ IRQ for the PIO state machine)
    : IRQ_FORCE ( pio -- addr ) [inlined] $034 + ;

    \ Each bit corresponds to the state of a 2-flipflop synchronizer on a GPIO
    \ input; 0 (the default) corresponds to synchronization and 1 bypasses;
    \ when in doubt leave as 0.
    : INPUT_SYNC_BYPASS ( pio -- addr ) [inlined] $038 + ;

    \ Get the pad output values the PIO is driving for the GPIO's
    : DBG_PADOUT ( pio -- addr ) [inlined] $03C + ;

    \ Get the pad output enable values the PIO is drivng for the GPIO's
    : DBG_PADOE ( pio -- addr ) [inlined] $040 + ;

    \ Configuration info
    : DBG_CFGINFO ( pio -- addr ) [inlined] $044 + ;

    \ LSB of the size of the instruction memory in instructions
    16 constant DBG_CFGINFO_IMEM_SIZE_LSB

    \ Mask of the size of instruction memory in instructions
    $3F 16 lshift constant DBG_CFGINFO_IMEM_SIZE_MASK

    \ Get the size of instruction memory in instructions
    DBG_CFGINFO_IMEM_SIZE_MASK DBG_CFGINFO_IMEM_SIZE_LSB
    make-pio-field-getter DBG_CFGINFO DBG_CFGINFO_IMEM_SIZE@
    
    \ LSB of the number of state machines this PIO instance supports
    8 constant DBG_CFGINFO_SM_COUNT_LSB

    \ Mask of the number of state machines this PIO instance supports
    $F 8 lshift constant DBG_CFGINFO_SM_COUNT_MASK

    \ Get the number of state machines this PIO instance supports
    DBG_CFGINFO_SM_COUNT_MASK DBG_CFGINFO_SM_COUNT_LSB
    make-pio-field-getter DBG_CFGINFO DBG_CFGINFO_SM_COUNT@

    \ LSB of the depth of the state machine TX/RX FIFO's in words
    0 constant DBG_CFGINFO_FIFO_DEPTH_LSB

    \ Mask of the depth of the state machine TX/RX FIFO's in words
    $3F 0 lshift constant DBG_CFGINFO_FIFO_DEPTH_MASK
    
    \ Get the the depth of the state machine TX/RX FIFO's in words
    DBG_CFGINFO_FIFO_DEPTH_MASK DBG_CFGINFO_FIFO_DEPTH_LSB
    make-pio-field-getter DBG_CFGINFO DBG_CFGINFO_FIFO_DEPTH@

    \ Write only instruction memory (32 words in all)
    : INSTR_MEM ( index pio -- addr ) [inlined] $048 + swap cells + ;

    \ Clock divisor register for state machines
    : SM_CLKDIV ( state-machine pio -- addr ) [inlined] swap $18 * + $0C8 + ;

    \ LSB of integer portion of the clock divisor; 0 is treated as 65536, if 0
    \ then FRAC must also be 0
    16 constant SM_CLKDIV_INT_LSB

    \ Mask of integer portion of the clock divisor; 0 is treated as 65536, if 0
    \ then FRAC must also be 0
    $FFFF 16 lshift constant SM_CLKDIV_INT_MASK

    \ Set/get clock divisor register for state machines
    SM_CLKDIV_INT_MASK SM_CLKDIV_INT_LSB
    make-sm-field SM_CLKDIV SM_CLKDIV_INT! SM_CLKDIV_INT@

    \ LSB of the fractional portion of the clock divisor, i.e. divided by 256
    8 constant SM_CLKDIV_FRAC_LSB

    \ Mask of the fractional portion of the clock divisor, i.e. divided by 256
    $FF 8 lshift constant SM_CLKDIV_FRAC_MASK
    
    \ Set/get clock divisor register for state machines
    SM_CLKDIV_FRAC_MASK SM_CLKDIV_FRAC_LSB
    make-sm-field SM_CLKDIV SM_CLKDIV_FRAC! SM_CLKDIV_FRAC@

    \ Execution/behavioral settings for state machines
    : SM_EXECCTRL ( state-machine pio -- addr ) [inlined] swap $18 * + $0CC + ;

    \ State machine is stalled bit
    31 bit constant SM_EXECCTRL_EXEC_STALLED

    \ Get state machine is stalled bit
    SM_EXECCTRL_EXEC_STALLED
    make-bit-getter SM_EXECCTRL SM_EXECCTRL_EXEC_STALLED@

    \ MSB of delay/side-set instruction field is used as side-set enable bit
    30 bit constant SM_EXECCTRL_SIDE_EN

    \ Set/get MSB of delay/side-set instruction field is used as side-set enable
    \ bit
    SM_EXECCTRL_SIDE_EN
    make-bit SM_EXECCTRL SM_EXECCTRL_SIDE_EN! SM_EXECCTRL_SIDE_EN@

    \ Side-set data is asserted to pin directions bit
    29 bit constant SM_EXECCTRL_SIDE_PINDIR

    \ Set/get side-set data is asserted to pin directions bit
    SM_EXECCTRL_SIDE_PINDIR
    make-bit SM_EXECCTRL SM_EXECCTRL_SIDE_PINDIR! SM_EXECCTRL_SIDE_PINDIR@

    \ LSB of GPIO number to use as condition for JMP PIN
    24 constant SM_EXECCTRL_JMP_PIN_LSB

    \ Mask of GPIO number to use as condition for JMP PIN
    $1F 24 lshift constant SM_EXECCTRL_JMP_PIN_MASK

    \ Set/get GPIO number to use as condition for JMP PIN
    SM_EXECCTRL_JMP_PIN_MASK SM_EXECCTRL_JMP_PIN_LSB
    make-sm-field SM_EXECCTRL SM_EXECCTRL_JMP_PIN! SM_EXECCTRL_JMP_PIN@

    \ LSB of data bit to use for inline OUT enable
    19 constant SM_EXECCTRL_OUT_EN_SEL_LSB

    \ Mask of data bit to use for the inline OUT enable
    $1F 19 lshift constant SM_EXECCTRL_OUT_EN_SEL_MASK

    \ Set/get data bit to use for inline OUT enable
    SM_EXECCTRL_OUT_EN_SEL_MASK SM_EXECCTRL_OUT_EN_SEL_LSB
    make-sm-field SM_EXECCTRL SM_EXECCTRL_OUT_EN_SEL! SM_EXECCTRL_OUT_EN_SEL@

    \ Use a bit of OUT data as an auxiliary write enable bit
    18 bit constant SM_EXECCTRL_INLINE_OUT_EN

    \ Set/get use a bit of OUT data as an auxiliary write enable bit
    SM_EXECCTRL_INLINE_OUT_EN
    make-bit SM_EXECCTRL SM_EXECCTRL_INLINE_OUT_EN! SM_EXECCTRL_INLINE_OUT_EN@

    \ Continuously assert the most recent OUT/SET to the pins
    17 bit constant SM_EXECCTRL_OUT_STICKY

    \ Set/get continuously assert he most recent OUT/SET to the pins
    SM_EXECCTRL_OUT_STICKY
    make-bit SM_EXECCTRL SM_EXECCTRL_OUT_STICKY! SM_EXECCTRL_OUT_STICKY@

    \ LSB of address reached to wrap execution to WRAP_BOTTOM; if a jump and
    \ condition is true, jump takes priority
    12 constant SM_EXECCTRL_WRAP_TOP_LSB

    \ Mask of address reached to wrap execution to WRAP_BOTTOM; if a jump and
    \ condition is true, jump takes priority
    $1F 12 lshift constant SM_EXECCTRL_WRAP_TOP_MASK

    \ Set/get address reached to wrap execution to WRAP_BOTTOM; if a jump and
    \ condition is true, jump takes priority
    SM_EXECCTRL_WRAP_TOP_MASK SM_EXECCTRL_WRAP_TOP_LSB
    make-sm-field SM_EXECCTRL SM_EXECCTRL_WRAP_TOP! SM_EXECCTRL_WRAP_TOP@
    
    \ LSB of address execution is wrapped to
    7 constant SM_EXECCTRL_WRAP_BOTTOM_LSB

    \ Mask of address execution is wrapped to
    $1F 7 lshift constant SM_EXECCTRL_WRAP_BOTTOM_MASK

    \ Set/get address execution is wrapped to
    SM_EXECCTRL_WRAP_BOTTOM_MASK SM_EXECCTRL_WRAP_BOTTOM_LSB
    make-sm-field SM_EXECCTRL SM_EXECCTRL_WRAP_BOTTOM! SM_EXECCTRL_WRAP_BOTTOM@

    \ MOV x, STATUS comparison; 0 is all ones if TX FIFO level < N else all zeros
    \ 1 is all ones if RX FIFO level < N else all zeros
    4 bit constant SM_EXECCTRL_STATUS_SEL

    \ Set/get MOV x, STATUS comparison; 0 is all ones if TX FIFO level < N else
    \ all zeros 1 is all ones if RX FIFO level < N else all zeros
    SM_EXECCTRL_STATUS_SEL
    make-bit SM_EXECCTRL SM_EXECCTRL_STATUS_SEL! SM_EXECCTRL_STATUS_SEL@

    \ LSB of comparison level for MOV x, STATUS
    0 constant SM_EXECCTRL_STATUS_N_LSB

    \ Mask of comparison level for MOV x, STATUS
    $F 0 lshift constant SM_EXECCTRL_STATUS_N_MASK

    \ Set/get comparison level for MOV x, STATUS
    SM_EXECCTRL_STATUS_N_MASK SM_EXECCTRL_STATUS_N_LSB
    make-sm-field SM_EXECCTRL SM_EXECCTRL_STATUS_N! SM_EXECCTRL_STATUS_N@

    \ Control behavior of the input/output shift registers for state machines
    : SM_SHIFTCTRL ( state-machine pio -- addr ) [inlined] swap $18 * + $0D0 + ;

    \ RX FIFO steals the TX FIFO's storage and becomes twice as deep; FIFO's are
    \ flushed when set
    31 bit constant SM_SHIFTCTRL_FJOIN_RX

    \ Set/get RX FIFO steals the TX FIFO's storage and becomes twice as deep;
    \ FIFO's are flushed when set
    SM_SHIFTCTRL_FJOIN_RX
    make-bit SM_SHIFTCTRL SM_SHIFTCTRL_FJOIN_RX! SM_SHIFTCTRL_FJOIN_RX@

    \ TX FIFO steals the RX FIFO's storage and becomes twice as deep; FIFO's are
    \ flushed when set
    30 bit constant SM_SHIFTCTRL_FJOIN_TX

    \ Set/get TX FIFO steals the RX FIFO's storage and becomes twice as deep;
    \ FIFO's are flushed when set
    SM_SHIFTCTRL_FJOIN_TX
    make-bit SM_SHIFTCTRL SM_SHIFTCTRL_FJOIN_TX! SM_SHIFTCTRL_FJOIN_TX@

    \ LSB of number of bits shifted out of OSR before autopull or conditional
    \ pull will take place; 0 means 32
    25 constant SM_SHIFTCTRL_PULL_THRESH_LSB

    \ Mask of number of bits shifted out of OSR before autopull or conditional
    \ pull will take place; 0 means 32
    $1F 25 lshift constant SM_SHIFTCTRL_PULL_THRESH_MASK

    \ Set/get number of bits shifted out of OSR before autopull or conditional
    \ pull will take place; 0 means 32
    SM_SHIFTCTRL_PULL_THRESH_MASK SM_SHIFTCTRL_PULL_THRESH_LSB
    make-sm-field SM_SHIFTCTRL SM_SHIFTCTRL_PULL_THRESH! SM_SHIFTCTRL_PULL_THRESH@

    \ LSB of number of bits shifted into ISR before autopush or conditional
    \ push will take place; 0 means 32
    20 constant SM_SHIFTCTRL_PUSH_THRESH_LSB

    \ Mask of number of bits shifted into ISR before autopush or conditional
    \ push will take place; 0 means 32
    $1F 20 lshift constant SM_SHIFTCTRL_PUSH_THRESH_MASK

    \ Set/get number of bits shifted into ISR before autopush or conditional
    \ push will take place; 0 means 32
    SM_SHIFTCTRL_PUSH_THRESH_MASK SM_SHIFTCTRL_PUSH_THRESH_LSB
    make-sm-field SM_SHIFTCTRL SM_SHIFTCTRL_PUSH_THRESH! SM_SHIFTCTRL_PUSH_THRESH@

    \ 1 = shift out of output shift register to right; 0 = to left
    19 bit constant SM_SHIFTCTRL_OUT_SHIFTDIR

    \ Set/get 1 = shift out of output shift register to right; 0 = to left
    SM_SHIFTCTRL_OUT_SHIFTDIR
    make-bit SM_SHIFTCTRL SM_SHIFTCTRL_OUT_SHIFTDIR! SM_SHIFTCTRL_OUT_SHIFTDIR@

    \ 1 = shift input shift register to right (data enters from left); 0 = to left
    18 bit constant SM_SHIFTCTRL_IN_SHIFTDIR

    \ Set/get 1 = shift input shift register to right (data enters from left);
    \ 0 = to left
    SM_SHIFTCTRL_IN_SHIFTDIR
    make-bit SM_SHIFTCTRL SM_SHIFTCTRL_IN_SHIFTDIR! SM_SHIFTCTRL_IN_SHIFTDIR@

    \ Pull automatically when the output shift register is emptied
    17 bit constant SM_SHIFTCTRL_AUTOPULL

    \ Set/get pull automatically when the output shift register is emptied
    SM_SHIFTCTRL_AUTOPULL
    make-bit SM_SHIFTCTRL SM_SHIFTCTRL_AUTOPULL! SM_SHIFTCTRL_AUTOPULL@

    \ Push automatically when the input shift register is filled
    16 bit constant SM_SHIFTCTRL_AUTOPUSH
    
    \ Set/get push automatically when the input shift register is filled
    SM_SHIFTCTRL_AUTOPUSH
    make-bit SM_SHIFTCTRL SM_SHIFTCTRL_AUTOPUSH! SM_SHIFTCTRL_AUTOPUSH@

    \ Current instruction address of state machines
    : SM_ADDR ( state-machine pio -- addr ) [inlined] swap $18 * + $0D4 + ;

    \ Read to see the instruction currently addressed by state machines' program
    \ counters, write to execute an instruction immediately
    : SM_INSTR ( state-machine pio -- addr ) [inlined] swap $18 * + $0D8 + ;

    \ State machine pin control
    : SM_PINCTRL ( state-machine pio -- addr ) [inlined] swap $18 * + $0DC + ;

    \ LSB of side-set bit count, minimum of 0, maximum of 5
    29 constant SM_PINCTRL_SIDESET_COUNT_LSB

    \ Mask of side-set-bit count, minimum of 0, maximum of 5
    7 29 lshift constant SM_PINCTRL_SIDESET_COUNT_MASK

    \ Set/get side-set bit count, minimum of 0, maximum of 5
    SM_PINCTRL_SIDESET_COUNT_MASK SM_PINCTRL_SIDESET_COUNT_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_SIDESET_COUNT! SM_PINCTRL_SIDESET_COUNT@

    \ LSB of SET pin count, minimum of 0, maximum of 5
    26 constant SM_PINCTRL_SET_COUNT_LSB

    \ Mask of SET pin count, minimum of 0, maximum of 5
    7 26 lshift constant SM_PINCTRL_SET_COUNT_MASK

    \ Set/get SET pin count, minimum of 0, maximum of 5
    SM_PINCTRL_SET_COUNT_MASK SM_PINCTRL_SET_COUNT_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_SET_COUNT! SM_PINCTRL_SET_COUNT@

    \ LSB of OUT PINS, OUT PINDIRS, or MOV PINS pin count, minimum of 0, maximum
    \ of 32 (inclusive)
    20 constant SM_PINCTRL_OUT_COUNT_LSB
    
    \ Mask of OUT PINS, OUT PINDIRS, or MOV PINS pin count, minimum of 0, maximum
    \ of 32 (inclusive)
    $3F 20 lshift constant SM_PINCTRL_OUT_COUNT_MASK

    \ Set/get OUT PINS, OUT PINDIRS, or MOV PINS pin count, minimum of 0, maximum
    \ of 32 (inclusive)
    SM_PINCTRL_OUT_COUNT_MASK SM_PINCTRL_OUT_COUNT_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_OUT_COUNT! SM_PINCTRL_OUT_COUNT@

    \ LSB of pin mapped to the least-significant bit of a state machine's IN
    \ data bus; higher pins are mapped to more significant data bits, modulo 32
    15 constant SM_PINCTRL_IN_BASE_LSB

    \ Mask of pin mapped to the least-significant bit of a state machine's IN
    \ data bus; higher pins are mapped to more significant data bits, modulo 32
    $1F 15 lshift constant SM_PINCTRL_IN_BASE_MASK

    \ Set/get pin mapped to the least-significant bit of a state machine's IN
    \ data bus; higher pins are mapped to more significant data bits, modulo 32
    SM_PINCTRL_IN_BASE_MASK SM_PINCTRL_IN_BASE_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_IN_BASE! SM_PINCTRL_IN_BASE@

    \ LSB of lowest-number pin affected by side-set operation
    10 constant SM_PINCTRL_SIDESET_BASE_LSB

    \ Mask of lowest-number pin affected by side-set operation
    $1F 10 lshift constant SM_PINCTRL_SIDESET_BASE_MASK

    \ Set/get lowest-number pin affected by side-set operation
    SM_PINCTRL_SIDESET_BASE_MASK SM_PINCTRL_SIDESET_BASE_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_SIDESET_BASE! SM_PINCTRL_SIDESET_BASE@

    \ LSB of lowest-number pin affected by SET PINS or SET PINDIRS operation
    5 constant SM_PINCTRL_SET_BASE_LSB

    \ Mask of lowest-number pin affected by SET PINS or SET PINDIRS operation
    $1F 5 lshift constant SM_PINCTRL_SET_BASE_MASK

    \ Set/get lowest-number pin affected by SET PINS or SET PINDIRS operation
    SM_PINCTRL_SET_BASE_MASK SM_PINCTRL_SET_BASE_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_SET_BASE! SM_PINCTRL_SET_BASE@

    \ LSB of lowest-number pin affected by OUT PINS, OUT PINDIRS, or MOV PINS
    \ operation
    0 constant SM_PINCTRL_OUT_BASE_LSB

    \ Mask of lowest-number pin affected by OUT PINS, OUT PINDIRS, or MOV PINS
    \ operation
    $1F 0 lshift constant SM_PINCTRL_OUT_BASE_MASK

    \ Set/get lowest-number pin affected by OUT PINS, OUT PINDIRS, or MOV PINS
    \ operation
    SM_PINCTRL_OUT_BASE_MASK SM_PINCTRL_OUT_BASE_LSB
    make-sm-field SM_PINCTRL SM_PINCTRL_OUT_BASE! SM_PINCTRL_OUT_BASE@

    \ Raw interrupts
    : INTR ( pio -- addr ) [inlined] $128 + ;
    
    \ Interrupt enable registers
    : INTE ( irq pio -- addr ) [inlined] $12C + swap $0C * + ;
    
    \ Interrupt force regisers
    : INTF ( irq pio -- addr ) [inlined] $130 + swap $0C * + ;
    
    \ Interrupt status registers
    : INTS ( irq pio -- addr ) [inlined] $134 + swap $0C * + ;

  end-module> import

  \ Interrupt bits
  : INT_SM ( state-machine -- index ) [inlined] 8 + bit ;

  \ TXN not full interupt bits
  : INT_SM_TXNFULL ( state-machine -- index ) [inlined] 4 + bit ;

  \ RXN not empty interrupt bits
  : INT_SM_RXNEMPTY ( state-machine -- index ) [inlined] bit ;

  \ IRQ0
  0 constant IRQ0

  \ IRQ1
  1 constant IRQ1
  
  \ Add the state machine ID to the lower two bits of the IRQ index, by way of
  \ module-4 addition on the two LSB's.
  : REL %10000 or ;

  \ Always jump
  %000 constant COND_ALWAYS

  \ Jump if scratch X is zero
  %001 constant COND_X0=

  \ Jump if scratch X is non-zero, post-decrement
  %010 constant COND_X1-

  \ Jump if scratch Y is zero
  %011 constant COND_Y0=

  \ Jump if scratch Y is non-zero, post-decrement
  %100 constant COND_Y1-

  \ Jump if scratch X not equal scratch Y
  %101 constant COND_XY<>

  \ Jump on input pin
  %110 constant COND_PIN

  \ Jump on output shift register not empty
  %111 constant COND_IOSRE

  \ Wait for GPIO
  %00 constant WAIT_GPIO

  \ Wait for a pin
  %01 constant WAIT_PIN

  \ Wait for an IRQ
  %10 constant WAIT_IRQ

  \ Pins input
  %000 constant IN_PINS

  \ Scratch register X input
  %001 constant IN_X

  \ Scratch register Y input
  %010 constant IN_Y

  \ NULL input (all zeros)
  %011 constant IN_NULL

  \ ISR input
  %110 constant IN_ISR

  \ OSR input
  %111 constant IN_OSR

  \ Pins output
  %000 constant OUT_PINS

  \ Scratch register X output
  %001 constant OUT_X

  \ Scratch register Y output
  %010 constant OUT_Y

  \ NULL output (discard data)
  %011 constant OUT_NULL

  \ PINDIRs output
  %100 constant OUT_PINDIRS

  \ PC output (unconditional jump to shifted address )
  %101 constant OUT_PC

  \ ISR output (also sets ISR shift counter to bit count)
  %110 constant OUT_ISR

  \ Execute OSR shift data as instruction
  %111 constant OUT_EXEC

  \ Push data even if threshold is not met
  false constant PUSH_NOT_FULL

  \ Do nothing unless the total input shift count has reached its threshold
  true constant PUSH_IF_FULL

  \ Do not stall execution if RX FIFO is full, instead drop data from ISR
  false constant PUSH_NO_BLOCK
  
  \ Stall execution if RX FIFO is full.
  true constant PUSH_BLOCK

  \ Pull data even if threshold is not met
  false constant PULL_NOT_EMPTY

  \ Do nothing unless the total output shift count has reached its threshold
  true constant PULL_IF_EMPTY

  \ Do not stall execution if TX FIFO is empty, instead copy from scratch X
  false constant PULL_NO_BLOCK

  \ Stall execution if TX FIFO is empty
  true constant PULL_BLOCK

  \ Move to PINS
  %000 constant MOV_DEST_PINS

  \ Move to scratch register X
  %001 constant MOV_DEST_X

  \ Move to scratch register Y
  %010 constant MOV_DEST_Y

  \ Move to EXEC (execute data as instruction)
  %100 constant MOV_DEST_EXEC

  \ Move to PC (treat data as address for unconditional branch)
  %101 constant MOV_DEST_PC

  \ Move to ISR (input shift counter is reset to 0, i.e. empty)
  %110 constant MOV_DEST_ISR

  \ Move to OSR (output shift counter is reset to 0, i.e. full)
  %111 constant MOV_DEST_OSR

  \ Move operation none
  %00 constant MOV_OP_NONE

  \ Move operation invert
  %01 constant MOV_OP_INVERT

  \ Move operation bit-reverse
  %10 constant MOV_OP_REVERSE

  \ Move from PINS
  %000 constant MOV_SRC_PINS

  \ Move from scratch register X
  %001 constant MOV_SRC_X

  \ Move from scratch register Y
  %010 constant MOV_SRC_Y

  \ Move from NULL
  %011 constant MOV_SRC_NULL

  \ Move from STATUS
  %101 constant MOV_SRC_STATUS

  \ Move from ISR
  %110 constant MOV_SRC_ISR

  \ Move from OSR
  %111 constant MOV_SRC_OSR

  \ Raise an IRQ
  %00 constant IRQ_SET

  \ Clear an IRQ
  %10 constant IRQ_CLEAR

  \ Wait for an IRQ to be lowered
  %010 constant IRQ_WAIT

  \ Set PINS
  %000 constant SET_PINS

  \ Set scratch register X (5 LSBs are set to data, all others are cleared)
  %001 constant SET_X

  \ Set scratch register Y (5 LSBs are set to data, all others are cleared)
  %010 constant SET_Y

  \ Set PINDIRS
  %100 constant SET_PINDIRS

  \ PIO JMP instruction
  : jmp, ( address condition -- )
    $07 and 5 lshift swap $1F and or ( $0000 or ) h,
  ;

  \ PIO WAIT instruction
  : wait, ( index source polarity -- )
    0<> 1 and 7 lshift swap 3 and 5 lshift or swap $1F and or $2000 or h,
  ;

  \ PIO IN instruction
  : in, ( bit-count source -- )
    $07 and 5 lshift swap 1- $1F and or $4000 or h,
  ;

  \ PIO OUT instruction
  : out, ( bit-count destination -- )
    $07 and 5 lshift swap 1- $1F and or $6000 or h,
  ;

  \ PIO PUSH instruction
  : push, ( block if-full -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or $8000 or h,
  ;

  \ PIO PULL instruction
  : pull, ( block if-empty -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or $8080 or h,
  ;

  \ PIO MOV instruction
  : mov, ( source op destination -- )
    $07 and 5 lshift swap $03 and 3 lshift or swap $03 and or $A000 or h,
  ;

  \ PIO IRQ instruction
  : irq, ( index set/wait -- )
    $03 and 5 lshift swap $1F and or $C000 or h,
  ;

  \ PIO SET instruction
  : set, ( data destination -- )
    $07 and 5 lshift swap $1F and or $E000 or h,
  ;

  \ PIO JMP instruction with delay or side-set
  : jmp+, ( address delay/side-set condition -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap $1F and or ( $0000 or ) h,
  ;

  \ PIO WAIT instruction with delay or side-set
  : wait+, ( index delay/side-set source polarity -- )
    0<> 1 and 7 lshift swap 3 and 5 lshift or swap $1F and 8 lshift or
    swap $1F and or $2000 or h,
  ;

  \ PIO IN instruction with delay or side-set
  : in+, ( bit-count delay/side-set source -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap 1- $1F and or $4000 or h,
  ;

  \ PIO OUT instruction with delay or side-set
  : out+, ( bit-count delay/side-set destination -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap 1- $1F and or $6000 or h,
  ;

  \ PIO PUSH instruction with delay or side-set
  : push+, ( delay/side-set block if-full -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or swap $1F and 8 lshift or
    $8000 or h,
  ;

  \ PIO PULL instruction with delay or side-set
  : pull+, ( delay/side-set block if-empty -- )
    0<> 1 and 6 lshift swap 0<> 1 and 5 lshift or swap $1F and 8 lshift or
    $8080 or h,
  ;

  \ PIO MOV instruction with delay or side-set
  : mov+, ( source delay/side-set op destination -- )
    $07 and 5 lshift swap $03 and 3 lshift or swap $1F and 8 lshift or
    swap $03 and or $A000 or h,
  ;

  \ PIO IRQ instruction with delay or side-set
  : irq+, ( index delay/side-set set/wait -- )
    $03 and 5 lshift swap $1F and 8 lshift or swap $1F and or $C000 or h,
  ;

  \ PIO SET instruction with delay or side-set
  : set+, ( data delay/side-set destination -- )
    $07 and 5 lshift swap $1F and 8 lshift or swap $1F and or $E000 or h,
  ;

  \ Enable state machines
  : sm-enable ( state-machine-bits pio -- )
    dup validate-pio
    over %1111 bic 0= averts x-sm-out-of-range
    swap CTRL_SM_ENABLE_LSB lshift swap CTRL bis!
  ;

  \ Disable state machines
  : sm-disable ( state-machine-bits pio -- )
    dup validate-pio
    over %1111 bic 0= averts x-sm-out-of-range
    swap CTRL_SM_ENABLE_LSB lshift swap CTRL bic!
  ;

  \ Restart state machines
  : sm-restart ( state-machine-bits pio -- )
    dup validate-pio
    over %1111 bic 0= averts x-sm-out-of-range
    CTRL_SM_RESTART!
  ;

  \ Set the clock divisor for a state machine
  : sm-clkdiv! ( fractional integral state-machine pio -- )
    2dup validate-sm-pio
    2 pick $10000 u< averts x-clkdiv-out-of-range
    3 pick $100 u< averts x-clkdiv-out-of-range
    2dup 2>r SM_CLKDIV_INT! 2r> SM_CLKDIV_FRAC!
  ;

  \ Set the address for a state machine
  : sm-addr! ( address state-machine pio -- )
    2dup validate-sm-pio
    2 pick 32 u< averts x-address-out-of-range
    SM_ADDR !
  ;

  \ Set the wrapping of a state machine
  : sm-wrap! ( bottom-address top-address state-machine pio -- )
    2dup validate-sm-pio
    2 pick 32 u< averts x-address-out-of-range
    3 pick 32 u< averts x-address-out-of-range
    2dup 2>r SM_EXECCTRL_WRAP_TOP! 2r> SM_EXECCTRL_WRAP_BOTTOM!
  ;

  \ Set the sticky state of a state machine
  : sm-out-sticky! ( sticky state-machine pio -- )
    2dup validate-sm-pio
    SM_EXECCTRL_OUT_STICKY!
  ;

  \ Set delay enable (sideset disable) state for a statemachine
  : sm-delay-enable ( state-machine pio -- )
    2dup validate-sm-pio
    off -rot SM_EXECCTRL_SIDE_EN!
  ;

  \ Set sideset enable (delay disable) state for a state machine
  : sm-sideset-enable ( state-machine pio -- )
    2dup validate-sm-pio
    on -rot SM_EXECCTRL_SIDE_EN!
  ;

  \ Set OSR threshold before autopull or conditional pull will take place; value
  \ may be from 1 to 32
  : sm-pull-threshold! ( threshold state-machine pio -- )
    2dup validate-sm-pio
    2 pick dup 0 u> swap 33 u< and averts x-threshold-out-of-range
    rot dup 32 = if drop 0 then
    -rot SM_SHIFTCTRL_PULL_THRESH!
  ;

  \ Set ISR threshold before autopush or conditional push will take place; value
  \ may be from 1 to 32
  : sm-push-threshold! ( threshold state-machine pio -- )
    2dup validate-sm-pio
    2 pick dup 0 u> swap 33 u< and averts x-threshold-out-of-range
    rot dup 32 = if drop 0 then
    -rot SM_SHIFTCTRL_PUSH_THRESH!
  ;

  \ Write to the TX FIFO of a state machine
  : sm-txf! ( x state-machine pio -- )
    2dup validate-sm-pio
    TXF !
  ;

  \ Read from the RX FIFO of a state machine
  : sm-rxf@ ( x state-machine pio -- )
    2dup validate-sm-pio
    RXF @
  ;

  \ Set pins to PIO mode
  : pins-pio-alternate ( pin-base pin-count -- )
    dup 30 u<= averts x-too-many-pins
    over 30 u< averts x-pin-out-of-range
    over + swap ?do 6 i 30 umod alternate-pin loop
  ;

  \ Set the sideset pins for a state machine
  : sm-sideset-pins! ( pin-base pin-count state-matchine pio -- )
    2dup validate-sm-pio
    2 pin 6 u< averts x-too-many-pins
    3 pin validate-pin
    2>r 2dup pins-pio-alternate
    2r@ SM_PINCTRL_SIDESET_COUNT!
    2r> SM_PINCTRL_SIDESET_BASE!
  ;

  \ Set the SET pins for a state machine
  : sm-set-pins! ( pin-base pin-count state-machine pio -- )
    2dup validate-sm-pio
    2 pick 6 u< averts x-too-many-pins
    3 pick validate-pin
    2>r 2dup pins-pio-alternate
    2r@ SM_PINCTRL_SET_COUNT!
    2r> SM_PINCTRL_SET_BASE!
  ;

  \ Set the OUT pins for a state machine
  : sm-out-pins! ( pin-base pin-count state-machine pio -- )
    2dup validate-sm-pio
    2 pick 32 u<= averts x-too-many-pins
    3 pick validate-pin
    2>r 2dup pins-pio-alternate
    2r@ SM_PINCTRL_OUT_COUNT!
    2r> SM_PINCTRL_OUT_BASE!
  ;

  \ Set the IN pin base for a state machine
  : sm-in-pin-base! ( pin-base state-machine pio -- )
    2dup validate-sm-pio
    2 pick validate-pin
    SM_PINCTRL_IN_BASE!
  ;

  \ Get a state machine RX FIFO level
  : sm-rx-fifo-level@ ( state-machine pio -- level )
    2dup validate-sm-pio
    FLEVEL_RX@
  ;

  \ Get a state machine TX FIFO level
  : sm-tx-fifo-level@ ( state-machine pio -- level )
    2dup validate-sm-pio
    FLEVEL_TX@
  ;

  \ Set joining the RX FIFO and TX FIFO of a state machine into a single eight
  \ by 32-bit RX FIFO
  : sm-join-rx-fifo! ( join? state-machine pio -- )
    2dup validate-sm-pio
    SM_SHIFTCTRL_FJOIN_RX!
  ;

  \ Set joining the RX FIFO and TX FIFO of a state machine into a single eight
  \ by 32-bit TX FIFO
  : sm-join-tx-fifo! ( join? state-machine pio -- )
    2dup validate-sm-pio
    SM_SHIFTCTRL_FJOIN_TX!
  ;

  \ Set the output shift register direction
  : sm-out-shift-dir ( direction state-machine pio -- )
    2dup validate-sm-pio
    SM_SHIFTCTRL_OUT_SHIFTDIR!
  ;

  \ Set the input shift register direction
  : sm-in-shift-dir ( direction state-machine pio -- )
    2dup validate-sm-pio
    SM_SHIFTCTRL_IN_SHIFTDIR!
  ;

  \ Set autopull on/off
  : sm-autopull! ( on/off state-machine pio -- )
    2dup validate-sm-pio
    SM_SHIFTCTRL_AUTOPULL!
  ;

  \ Set autopush on/off
  : sm-autopush! ( on/off state-machine pio -- )
    2dup validate-sm-pio
    SM_SHIFTCTRL_AUTOPUSH!
  ;

  \ Manually write instructions to a state machine
  : sm-instr! ( addr count state-machine pio -- )
    2dup validate-sm-pio
    SM_INSTR -rot 0 ?do 2dup h@ swap ! 2 + loop 2drop
  ;
  
  \ Write a number of halfwords to instruction memory
  : pio-instr-mem! ( addr count pio -- )
    dup validate-pio
    over 32 u<= averts x-too-many-instructions
    -rot 0 ?do 2dup h@ i rot INSTR_MEM ! 2 + loop 2drop
  ;

  \ Enable interrupts
  : pio-interrupt-enable ( interrupt-bits irq pio -- )
    dup validate-pio
    over validate-irq
    2 pick validate-interrupt
    INTE bis!
  ;

  \ Disable interrupts
  : pio-interrupt-disable ( interrupt-bits irq pio -- )
    dup validate-pio
    over validate-irq
    2 pick validate-interrupt
    INTE bic!
  ;

  \ Enable forcing interrupts
  : pio-interrupt-enable-force ( interrupt-bits irq pio -- )
    dup validate-pio
    over validate-irq
    2 pick validate-interrupt
    INTF bis!
  ;

  \ Disable forcing interrupts
  : pio-interrupt-disable-force ( interrupt-bits irq pio -- )
    dup validate-pio
    over validate-irq
    2 pick validate-interrupt
    INTF bic!
  ;

  \ Get raw interrupts
  : pio-interrupt-raw@ ( pio -- interrupt-bits )
    dup validate-pio
    INTR @
  ;

  \ Get interrupt status
  : pio-interrupt@ ( irq pio -- interrupt-bits )
    dup validate-pio
    over validate-irq
    INTS @
  ;

end-module

\ Reboot
reboot
