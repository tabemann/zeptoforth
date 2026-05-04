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

begin-module hstx

  pin import
  dma import
  dma-pool import
  
  \ Invalid HSTX bit exception
  : x-invalid-hstx-bit ( -- ) ." invalid HSTX bit" cr ;

  \ Out of range value
  : x-out-of-range ( -- ) ." out of range HSTX parameter" cr ;
  
  begin-module hstx-internal

    \ Validate an HSTX bit
    : validate-hstx-bit ( bit -- ) 8 u< averts x-invalid-hstx-bit ;
    
    \ HSTX control base address
    $400C0000 constant HSTX_CTRL_BASE

    \ HSTX_CTRL_CSR register
    HSTX_CTRL_BASE $00 + constant HSTX_CTRL_CSR

    \ HSTX clock period measured in HSTX clock cycles LSB
    28 constant HSTX_CTRL_CSR_CLKDIV_LSB

    \ HSTX clock period measured in HSTX clock cycles mask
    4 bit 1- HSTX_CTRL_CSR_CLKDIV_LSB lshift constant HSTX_CTRL_CSR_CLKDIV_MASK

    \ HSTX initial phase of the generated clock in half clk_hstx periods LSB
    24 constant HSTX_CTRL_CSR_CLKPHASE_LSB

    \ HSTX initial phase of the generated clock in half clk_hstx periods mask
    4 bit 1- HSTX_CTRL_CSR_CLKPHASE_LSB lshift
    constant HSTX_CTRL_CSR_CLKPHASE_MASK

    \ Number of times to shift the shift register before refilling it from the
    \ FIFO LSB; a register value of 0 means shift 32 times
    16 constant HSTX_CTRL_CSR_N_SHIFTS_LSB

    \ Number of times to shift the shift register before refilling it from the
    \ FIFO mask; a register value of 0 means shift 32 times
    5 bit 1- HSTX_CTRL_CSR_N_SHIFTS_LSB lshift
    constant HSTX_CTRL_CSR_N_SHIFTS_MASK

    \ How many bits to right-rotate the shift register by each cycle LSB
    8 constant HSTX_CTRL_CSR_SHIFT_LSB

    \ How many bits to right-rotate the shift register by each cycle mask
    5 bit 1- HSTX_CTRL_CSR_SHIFT_LSB lshift constant HSTX_CTRL_CSR_SHIFT_MASK

    \ Select which PIO to use for coupled mode operation LSB
    5 constant HSTX_CTRL_CSR_COUPLED_SEL_LSB

    \ Select which PIO to use for coupled mode operation mask
    2 bit 1- HSTX_CTRL_CSR_COUPLED_SEL_LSB lshift
    constant HSTX_CTRL_CSR_COUPLED_SEL_MASK

    \ Enable the PIO-to-HSTX 1:1 connection
    4 bit constant HSTX_CTRL_CSR_COUPLED_MODE

    \ Enable the command expander
    1 bit constant HSTX_CTRL_CSR_EXPAND_EN

    \ When EN is 1, the HSTX will shift out data as it appears in the FIFO; as
    \ long as there is data, the HSTX shift register will shift once per clock
    \ cycle, and the frequency of popping from the FIFO is determined by the
    \ ratio of SHIFT and SHIFT_THRES.
    \
    \ When EN is 0, the FIFO is not popped. The shift counter and clock
    \ generator are also reset to their initial state for as long as EN is low.
    \ Note the initial phase of the clock generator can be configured by the
    \ CLKPHASE field.
    \
    \ Once the HSTX is enabled again, and data is pushed to the FIFO, the
    \ generated clock's first rising edge will be one half-period after the
    \ first data is launched.
    0 bit constant HSTX_CTRL_CSR_EN
    
    \ HSTX data control register for output bit 0
    HSTX_CTRL_BASE $04 + constant HSTX_CTRL_BIT0
    
    \ HSTX data control register for output bit
    : HSTX_CTRL_BIT ( bit -- ) dup validate-hstx-bit cells HSTX_CTRL_BIT0 + ;

    \ Connect this output to the generated clock, rather than the data shift
    \ register. SEL_P and SEL_N are ignored if t his bit is est, but INV can
    \ still be set o generate an antiphase clock.
    17 bit constant HSTX_CTRL_BIT_CLK

    \ Invert this data output (logical NOT)
    16 bit constant HSTX_CTRL_BIT_INV

    \ Shift register bit select for the second half of the HSTX clock cycle LSB
    8 constant HSTX_CTRL_BIT_SEL_N_LSB

    \ Shift register bit select for the second half of the HSTX clock cycle mask
    5 bit 1- HSTX_CTRL_BIT_SEL_N_LSB lshift constant HSTX_CTRL_BIT_SEL_N_MASK

    \ Shift register bit select for the first half of the HSTX clock cycle LSB
    0 constant HSTX_CTRL_BIT_SEL_P_LSB

    \ Shift register bit select for the first half of the HSTX clock cycle mask
    5 bit 1- HSTX_CTRL_BIT_SEL_P_LSB lshift constant HSTX_CTRL_BIT_SEL_P_MASK

    \ HSTX_CTRL_EXPAND_SHIFT register
    HSTX_CTRL_BASE $24 + constant HSTX_CTRL_EXPAND_SHIFT

    \ Number of times to consume from the shift register before refilling it
    \ from the FIFO, when the current command is an encoded data command
    \ (e.g. TMDS) LSB. A register value of 0 means shift 32 times.
    24 constant HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_LSB

    \ Number of times to consume from the shift register before refilling it
    \ from the FIFO, when the current command is an encoded data command
    \ (e.g. TMDS) mask. A register value of 0 means shift 32 times.
    5 bit 1- HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_LSB lshift
    constant HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_MASK

    \ How many bits to right-rotate the shift register by each time data is
    \ pushed to the output shifter, when the current command is an encoded data
    \ command (e.g. TMDS) LSB.
    16 constant HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_LSB

    \ How many bits to right-rotate the shift register by each time data is
    \ pushed to the output shifter, when the current command is an encoded data
    \ command (e.g. TMDS) mask.
    5 bit 1- HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_LSB lshift
    constant HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_MASK

    \ Number of times to consume from the shift register before refilling it
    \ from the FIFO, when the current command is a raw data command LSB. A
    \ register value of 0 means shift 32 times.
    8 constant HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_LSB

    \ Number of times to consume from the shift register before refilling it
    \ from the FIFO, when the current command is a raw data command mask. A
    \ register value of 0 means shift 32 times.
    5 bit 1- HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_LSB lshift
    constant HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_MASK

    \ How many bits to right-rotate the shift register by each time data is
    \ pushed to the output shifter, when the current command is a raw data
    \ command LSB.
    0 constant HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_LSB

    \ How many bits to right-rotate the shift register by each time data is
    \ pushed to the output shifter, when the current command is a raw data
    \ command mask.
    5 bit 1- HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_LSB lshift
    constant HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_MASK

    \ HSTX_CTRL_EXPAND_TMDS register
    HSTX_CTRL_BASE $28 + constant HSTX_CTRL_EXPAND_TMDS

    \ Number of valid data bits for the lane 2 TMDS encoder, starting from bit
    \ 7 of the rotated data LSB. Field values of 0 to 7 encode counts of 1 to 8
    \ bits.
    21 constant HSTX_CTRL_EXPAND_TMDS_L2_NBITS_LSB
    
    \ Number of valid data bits for the lane 2 TMDS encoder, starting from bit
    \ 7 of the rotated data mask. Field values of 0 to 7 encode counts of 1 to 8
    \ bits.
    3 bit 1- HSTX_CTRL_EXPAND_TMDS_L2_NBITS_LSB lshift
    constant HSTX_CTRL_EXPAND_TMDS_L2_NBITS_MASK

    \ Right-rotate applied to the current shifter data before the lane 2 TMDS
    \ encoder LSB.
    16 constant HSTX_CTRL_EXPAND_TMDS_L2_ROT_LSB
    
    \ Right-rotate applied to the current shifter data before the lane 2 TMDS
    \ encoder mask.
    5 bit 1- HSTX_CTRL_EXPAND_TMDS_L2_ROT_LSB lshift
    constant HSTX_CTRL_EXPAND_TMDS_L2_ROT_MASK

    \ Number of valid data bits for the lane 1 TMDS encoder, starting from bit
    \ 7 of the rotated data LSB. Field values of 0 to 7 encode counts of 1 to 8
    \ bits.
    13 constant HSTX_CTRL_EXPAND_TMDS_L1_NBITS_LSB
    
    \ Number of valid data bits for the lane 1 TMDS encoder, starting from bit
    \ 7 of the rotated data mask. Field values of 0 to 7 encode counts of 1 to 8
    \ bits.
    3 bit 1- HSTX_CTRL_EXPAND_TMDS_L1_NBITS_LSB lshift
    constant HSTX_CTRL_EXPAND_TMDS_L1_NBITS_MASK

    \ Right-rotate applied to the current shifter data before the lane 1 TMDS
    \ encoder LSB.
    8 constant HSTX_CTRL_EXPAND_TMDS_L1_ROT_LSB
    
    \ Right-rotate applied to the current shifter data before the lane 1 TMDS
    \ encoder mask.
    5 bit 1- HSTX_CTRL_EXPAND_TMDS_L1_ROT_LSB lshift
    constant HSTX_CTRL_EXPAND_TMDS_L1_ROT_MASK

    \ Number of valid data bits for the lane 0 TMDS encoder, starting from bit
    \ 7 of the rotated data LSB. Field values of 0 to 7 encode counts of 1 to 8
    \ bits.
    5 constant HSTX_CTRL_EXPAND_TMDS_L0_NBITS_LSB
    
    \ Number of valid data bits for the lane 0 TMDS encoder, starting from bit
    \ 7 of the rotated data mask. Field values of 0 to 7 encode counts of 1 to 8
    \ bits.
    3 bit 1- HSTX_CTRL_EXPAND_TMDS_L0_NBITS_LSB lshift
    constant HSTX_CTRL_EXPAND_TMDS_L0_NBITS_MASK

    \ Right-rotate applied to the current shifter data before the lane 0 TMDS
    \ encoder LSB.
    0 constant HSTX_CTRL_EXPAND_TMDS_L0_ROT_LSB
    
    \ Right-rotate applied to the current shifter data before the lane 0 TMDS
    \ encoder mask.
    5 bit 1- HSTX_CTRL_EXPAND_TMDS_L0_ROT_LSB lshift
    constant HSTX_CTRL_EXPAND_TMDS_L0_ROT_MASK

    \ HSTX_FIFO_BASE address
    $50600000 constant HSTX_FIFO_BASE

    \ HSTX_FIFO_STAT register
    HSTX_FIFO_BASE $00 + constant HSTX_FIFO_STAT

    \ FIFO was written when full. Write 1 to clear
    10 bit constant HSTX_FIFO_STAT_WOF

    \ FIFO is empty
    9 bit constant HSTX_FIFO_STAT_EMPTY

    \ FIFO is full
    8 bit constant HSTX_FIFO_STAT_FULL

    \ FIFO level LSB
    0 constant HSTX_FIFO_STAT_LEVEL_LSB
    
    \ FIFO level mask
    8 bit 1- HSTX_FIFO_STAT_LEVEL_LSB constant HSTX_FIFO_STAT_LEVEL_MASK

    \ HSTX_FIFO_FIFO register
    HSTX_FIFO_BASE $04 + constant HSTX_FIFO_FIFO

    \ HSTX RAW command
    $0 constant HSTX_CMD_RAW

    \ HSTX RAW_REPEAT command
    $1 constant HSTX_CMD_RAW_REPEAT

    \ HSTX TMDS command
    $2 constant HSTX_CMD_TMDS

    \ HSTX TMDS repeat command
    $3 constant HSTX_CMD_TMDS_REPEAT

    \ HSTX NOP command
    $F constant HSTX_CMD_NOP

    \ HSTX command opcode LSB
    12 constant HSTX_CMD_OPCODE_LSB

    \ HSTX command opcode mask
    4 bit 1- HSTX_CMD_OPCODE_LSB lshift constant HSTX_CMD_OPCODE_MASK

    \ HSTX command count LSB
    0 constant HSTX_CMD_COUNT_LSB

    \ HSTX command count mask
    12 bit 1- HSTX_CMD_COUNT_LSB lshift constant HSTX_CMD_COUNT_MASK

    \ Write a raw HSTX command
    : hstx-raw-cmd! ( count cmd addr -- )
      { addr } over 4096 u< averts x-out-of-range or addr !
    ;

    \ Write a raw HSTX command to HERE
    : hstx-raw-cmd, ( count cmd -- ) here cell allot hstx-raw-cmd! ;
    
    \ Convert PIO's to indices
    : convert-from-pio ( pio -- pio-index )
      case
        pio::PIO0 of 0 endof
        pio::PIO1 of 1 endof
        pio::PIO2 of 2 endof
        dup
      endcase
    ;

    \ Convert an index to a PIO
    : convert-to-pio ( pio-index -- pio )
      case
        0 of pio::PIO0 endof
        1 of pio::PIO1 endof
        2 of pio::PIO2 endof
      endcase
    ;

  end-module> import

  \ Set HSTX clock period measured in HSTX clock cycles
  : hstx-clkdiv! ( clkdiv -- )
    dup 5 bit u< averts x-out-of-range
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_CLKDIV_MASK bic
    swap HSTX_CTRL_CSR_CLKDIV_LSB lshift or HSTX_CTRL_CSR !
  ;

  \ Get HSTX clock period measured in HSTX clock cycles
  : hstx-clkdiv@ ( -- clkdiv )
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_CLKDIV_MASK and
    HSTX_CTRL_CSR_CLKDIV_LSB rshift
  ;
  
  \ Set HSTX initial phase in half clk_hstx periods
  : hstx-clkphase! ( clkphase -- )
    dup 5 bit u< averts x-out-of-range
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_CLKPHASE_MASK bic
    swap HSTX_CTRL_CSR_CLKPHASE_LSB lshift or HSTX_CTRL_CSR !
  ;

  \ Get HSTX initial phase in half clk_hstx periods
  : hstx-clkphase@ ( -- clkphase )
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_CLKPHASE_MASK and
    HSTX_CTRL_CSR_CLKPHASE_LSB rshift
  ;

  \ Set number of times to shift the shift register before refilling it from
  \ the FIFO
  : hstx-n-shifts! ( n-shifts -- )
    dup 32 u<= over 0 u> and averts x-out-of-range
    dup 32 = if drop 0 then
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_N_SHIFTS_MASK bic
    swap HSTX_CTRL_CSR_N_SHIFTS_LSB lshift or HSTX_CTRL_CSR !
  ;

  \ Get number of times to shift the shift register before refilling it from
  \ the FIFO
  : hstx-n-shifts@ ( -- n-shifts )
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_N_SHIFTS_MASK and
    HSTX_CTRL_CSR_N_SHIFTS_LSB rshift
    dup 0= if drop 32 then
  ;
  
  \ Set how many bits to right-rotate the shift register by each cycle.
  : hstx-shift! ( shift -- )
    dup 32 u< averts x-out-of-range
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_SHIFT_MASK bic
    swap HSTX_CTRL_CSR_SHIFT_LSB lshift or HSTX_CTRL_CSR !
  ;

  \ Get how many bits to right-rotate the shift register by each cycle.
  : hstx-shift@ ( -- shift )
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_SHIFT_MASK and HSTX_CTRL_CSR_SHIFT_LSB rshift
  ;

  \ Set which PIO to use for coupled mode operation
  : hstx-coupled-sel! ( pio -- )
    dup 2 u> if convert-from-pio then dup 3 u< averts x-out-of-range
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_COUPLED_SEL_MASK bic
    swap HSTX_CTRL_CSR_COUPLED_SEL_LSB lshift or HSTX_CTRL_CSR !
  ;

  \ Get which PIO to use for coupled mode operation
  : hstx-coupled-sel@ ( -- pio )
    HSTX_CTRL_CSR @ HSTX_CTRL_CSR_COUPLED_SEL_MASK and
    HSTX_CTRL_CSR_COUPLED_SEL_MSB rshift convert-to-pio
  ;

  \ Set enabling the command expander
  : hstx-expand-en! ( expand-en -- )
    HSTX_CTRL_CSR_EXPAND_EN HSTX_CTRL_CSR rot if bis! else bic! then
  ;

  \ Get enabling the command expander
  : hstx-expand-en@ ( -- expand-en )
    HSTX_CTRL_CSR_EXPAND_EN HSTX_CTRL_CSR bit@
  ;

  \ Set enabling HSTX
  : hstx-en! ( en -- )
    HSTX_CTRL_CSR_EN HSTX_CTRL_CSR rot if bis! else bic! then
  ;

  \ Get enabling HSTX
  : hstx-en@ ( -- en )
    HSTX_CTRL_CSR_EN HSTX_CTRL_CSR bit@
  ;

  \ Set HSTX bit clock enable
  : hstx-bit-clk! ( clk bit -- )
    HSTX_CTRL_BIT HSTX_CTRL_BIT_CLK swap rot if bis! else bic! then
  ;
  
  \ Get HSTX bit clock enable
  : hstx-bit-clk@ ( bit -- clk )
    HSTX_CTRL_BIT HSTX_CTRL_BIT_CLK swap bit@
  ;

  \ Set HSTX bit invert
  : hstx-bit-inv! ( inv bit -- )
    HSTX_CTRL_BIT HSTX_CTRL_BIT_INV swap rot if bis! else bic! then
  ;

  \ Get HSTX bit invert
  : hstx-bit-inv@ ( bit -- inv )
    HSTX_CTRL_BIT HSTX_CTRL_BIT_INV swap bit@
  ;

  \ Set shift register data bit select for second half of the HSTX clock cycle
  : hstx-bit-sel-n! ( sel-n bit -- )
    over 32 u< averts x-out-of-range
    HSTX_CTRL_BIT dup { hstx-ctrl-bit } @
    HSTX_CTRL_BIT_SEL_N_MASK bic swap HSTX_CTRL_BIT_SEL_N_LSB lshift or
    hstx-ctrl-bit !
  ;

  \ Get shift register data bit select for second half of the HSTX clock cycle
  : hstx-bit-sel-n@ ( bit -- sel-n )
    HSTX_CTRL_BIT @ HSTX_CTRL_BIT_SEL_N_MASK and
    HSTX_CTRL_BIT_SEL_N_LSB rshift
  ;

  \ Set shift register data bit select for first half of the HSTX clock cycle
  : hstx-bit-sel-p! ( sel-p bit -- )
    over 32 u< averts x-out-of-range
    HSTX_CTRL_BIT dup { hstx-ctrl-bit } @
    HSTX_CTRL_BIT_SEL_P_MASK bic swap HSTX_CTRL_BIT_SEL_P_LSB lshift or
    hstx-ctrl-bit !
  ;

  \ Get shift register data bit select for first half of the HSTX clock cycle
  : hstx-bit-sel-p@ ( bit -- sel-p )
    HSTX_CTRL_BIT @ HSTX_CTRL_BIT_SEL_P_MASK and
    HSTX_CTRL_BIT_SEL_P_LSB rshift
  ;

  \ Set number of times to consume from the shift register before refilling it
  \ from the FIFO, when the current command is an encoded data command
  \ (e.g TMDS)
  : hstx-enc-n-shifts! ( shifts -- )
    dup 32 u<= over 0 u> and averts x-out-of-range
    dup 32 = if drop 0 then
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_MASK bic
    swap HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_LSB lshift or
    HSTX_CTRL_EXPAND_SHIFT !
  ;

  \ Get number of times to consume from the shift register before refilling it
  \ from the FIFO, when the current command is an encoded data command
  \ (e.g TMDS)
  : hstx-enc-n-shifts@ ( -- shifts )
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_MASK and
    HSTX_CTRL_EXPAND_SHIFT_ENC_N_SHIFTS_LSB rshift
    dup 0= if drop 32 then
  ;

  \ Set how many bits to right-rotate the shift register by each time data is
  \ pushed to the output shifter, when the current command is an encoded data
  \ command (e.g. TMDS)
  : hstx-enc-shift! ( shift -- )
    dup 32 u< averts x-out-of-range
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_MASK bic
    swap HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_LSB lshift or
    HSTX_CTRL_EXPAND_SHIFT !
  ;

  \ Get how many bits to right-rotate the shift register by each time data is
  \ pushed to the output shifter, when the current command is an encoded data
  \ command (e.g. TMDS)
  : hstx-enc-shift@ ( -- shift )
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_MASK and
    HSTX_CTRL_EXPAND_SHIFT_ENC_SHIFT_LSB rshift
  ;
  
  \ Set number of times to consume from the shift register before refilling it
  \ from the FIFO, when the current command is a raw data command
  : hstx-raw-n-shifts! ( shifts -- )
    dup 32 u<= over 0 u> and averts x-out-of-range
    dup 32 = if drop 0 then
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_MASK bic
    swap HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_LSB lshift or
    HSTX_CTRL_EXPAND_SHIFT !
  ;

  \ Get number of times to consume from the shift register before refilling it
  \ from the FIFO, when the current command is a raw data command
  : hstx-raw-n-shifts@ ( -- shifts )
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_MASK and
    HSTX_CTRL_EXPAND_SHIFT_RAW_N_SHIFTS_LSB rshift
    dup 0= if drop 32 then
  ;

  \ Set how many bits to right-rotate the shift register by each time data is
  \ pushed to the output shifter, when the current command is a raw data
  \ command
  : hstx-raw-shift! ( shift -- )
    dup 32 u< averts x-out-of-range
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_MASK bic
    swap HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_LSB lshift or
    HSTX_CTRL_EXPAND_SHIFT !
  ;

  \ Get how many bits to right-rotate the shift register by each time data is
  \ pushed to the output shifter, when the current command is a raw data
  \ command
  : hstx-raw-shift@ ( -- shift )
    HSTX_CTRL_EXPAND_SHIFT @ HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_MASK and
    HSTX_CTRL_EXPAND_SHIFT_RAW_SHIFT_LSB rshift
  ;

  \ Set number of valid data bits for a lane TMDS encoder, starting from bit 7
  \ of the rotated data.
  : hstx-l-nbits! ( nbits lane -- )
    over 8 u<= 2 pick 0 u> and averts x-out-of-range swap 1- { lane nbits }
    HSTX_CTRL_EXPAND_TMDS @ lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_NBITS_MASK endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_NBITS_MASK endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_NBITS_MASK endof
      ['] x-out-of-range ?raise
    endcase bic
    nbits lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_NBITS_LSB endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_NBITS_LSB endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_NBITS_LSB endof
    endcase lshift or
    HSTX_CTRL_EXPAND_TMDS !
  ;

  \ Get number of valid data bits for a lane TMDS encoder, starting from bit 7
  \ of the rotated data.
  : hstx-l-nbits@ { lane -- nbits }
    HSTX_CTRL_EXPAND_TMDS @ lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_NBITS_MASK endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_NBITS_MASK endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_NBITS_MASK endof
      ['] x-out-of-range ?raise
    endcase and
    lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_NBITS_LSB endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_NBITS_LSB endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_NBITS_LSB endof
    endcase rshift 1+
  ;

  \ Set right-rotate applied to the current shifter data before the lane TMDS
  \ encoder
  : hstx-l-rot! ( rot lane -- )
    over 32 u< averts x-out-of-range { _rot lane }
    HSTX_CTRL_EXPAND_TMDS @ lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_ROT_MASK endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_ROT_MASK endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_ROT_MASK endof
      ['] x-out-of-range ?raise
    endcase bic
    _rot lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_ROT_LSB endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_ROT_LSB endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_ROT_LSB endof
    endcase lshift or
    HSTX_CTRL_EXPAND_TMDS !
  ;
  
  \ Get right-rotate applied to the current shifter data before the lane TMDS
  \ encoder
  : hstx-l-rot@ ( lane -- rot )
    HSTX_CTRL_EXPAND_TMDS @ lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_ROT_MASK endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_ROT_MASK endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_ROT_MASK endof
      ['] x-out-of-range ?raise
    endcase and
    lane case
      0 of HSTX_CTRL_EXPAND_TMDS_L0_ROT_LSB endof
      1 of HSTX_CTRL_EXPAND_TMDS_L1_ROT_LSB endof
      2 of HSTX_CTRL_EXPAND_TMDS_L2_ROT_LSB endof
    endcase rshift
  ;

  \ Get whether the HSTX FIFO has been written to while full
  : hstx-fifo-wof? ( -- wof? ) HSTX_FIFO_STAT_WOF HSTX_FIFO_STAT bit@ ;

  \ Clear the HSTX FIFO written-while-full condition
  : hstx-fifo-wof-clear ( -- ) HSTX_FIFO_STAT_WOF HSTX_FIFO_STAT ! ;
  
  \ Get whether the HSTX FIFO is empty
  : hstx-fifo-empty? ( -- empty? ) HSTX_FIFO_STAT_EMPTY HSTX_FIFO_STAT bit@ ;

  \ Get whether the HSTX FIFO is full
  : hstx-fifo-full? ( -- full? ) HSTX_FIFO_STAT_FULL HSTX_FIFO_STAT bit@ ;

  \ Get the HSTX FIFO level
  : hstx-fifo-level@ ( -- level )
    HSTX_FIFO_STAT @ HSTX_FIFO_STAT_LEVEL_MASK and
    HSTX_FIFO_STAT_LEVEL_LSB rshift
  ;

  \ Raw write buffer to the HSTX FIFO
  : buffer>hstx-fifo-raw-dma { buffer bytes channel -- }
    buffer HSTX_FIFO_FIFO bytes 2 lshift cell DREQ_HSTX channel
    start-buffer>register-dma
  ;
  
  \ Write buffer to the HSTX FIFO
  : buffer>hstx-fifo { buffer bytes -- }
    allocate-dma { channel }
    buffer bytes channel ['] buffer>hstx-fifo-raw-dma try
    channel free-dma
    ?raise
  ;

  \ Write an HSTX command
  : hstx-cmd! ( count cmd addr -- )
    { addr } over 4096 u< averts x-out-of-range
    dup $10 u< averts x-out-of-range
    HSTX_CMD_OPCODE_LSB lshift or addr !
  ;

  \ Write an HSTX command to HERE
  : hstx-cmd, ( count cmd -- ) here cell allot hstx-cmd! ;

  \ Get an HSTX command
  : hstx-cmd@ ( addr -- count cmd )
    @ dup HSTX_CMD_COUNT_MASK and
    \ HSTX_CMD_COUNT_LSB rshift
    swap HSTX_CMD_OPCODE_MASK and HSTX_CMD_OCPODE_LSB rshift
  ;

  \ Write a RAW HSTX command
  : hstx-cmd-raw! ( count addr -- )
    [ HSTX_CMD_RAW HSTX_CMD_OPCODE_LSB lshift ] literal swap hstx-raw-cmd!
  ;

  \ Write a RAW HSTX command to HERE
  : hstx-cmd-raw, ( count -- )
    [ HSTX_CMD_RAW HSTX_CMD_OPCODE_LSB lshift ] literal hstx-raw-cmd,
  ;

  \ Write a RAW_REPEAT HSTX command
  : hstx-cmd-raw-repeat! ( count addr -- )
    [ HSTX_CMD_RAW_REPEAT HSTX_CMD_OPCODE_LSB lshift ] literal
    swap hstx-raw-cmd!
  ;

  \ Write a RAW_REPEAT HSTX command to HERE
  : hstx-cmd-raw-repeat, ( count -- )
    [ HSTX_CMD_RAW_REPEAT HSTX_CMD_OPCODE_LSB lshift ] literal hstx-raw-cmd,
  ;

  \ Write a TMDS HSTX command
  : hstx-cmd-tmds! ( count addr -- )
    [ HSTX_CMD_TMDS HSTX_CMD_OPCODE_LSB lshift ] literal swap hstx-raw-cmd!
  ;

  \ Write a TMDS HSTX command to HERE
  : hstx-cmd-tmds, ( count -- )
    [ HSTX_CMD_TMDS HSTX_CMD_OPCODE_LSB lshift ] literal hstx-raw-cmd,
  ;

  \ Write a TMDS_REPEAT HSTX command
  : hstx-cmd-tmds-repeat! ( count addr -- )
    [ HSTX_CMD_TMDS_REPEAT HSTX_CMD_OPCODE_LSB lshift ] literal
    swap hstx-raw-cmd!
  ;

  \ Write a TMDS_REPEAT HSTX command to HERE
  : hstx-cmd-tmds-repeat, ( count -- )
    [ HSTX_CMD_TMDS_REPEAT HSTX_CMD_OPCODE_LSB lshift ] literal hstx-raw-cmd,
  ;

  \ Write a NOP HSTX command
  : hstx-cmd-nop! ( count addr -- )
    [ HSTX_CMD_NOP HSTX_CMD_OPCODE_LSB lshift ] literal swap hstx-raw-cmd!
  ;

  \ Write a NOP HSTX command to HERE
  : hstx-cmd-nop, ( count -- )
    [ HSTX_CMD_NOP HSTX_CMD_OPCODE_LSB lshift ] literal hstx-raw-cmd,
  ;

  \ Set a pin to the HSTX function
  : hstx-pin { pin -- }
    pin 12 u>= pin 19 u<= and averts x-pin-out-of-range
    0 pin alternate-pin
  ;

end-module
