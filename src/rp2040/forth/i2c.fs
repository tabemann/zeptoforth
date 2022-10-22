\ Copyright (c) 2022 Travis Bemann
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

begin-module i2c

  gpio import
  pin import
  lock import
  interrupt import
  systick import
  task import

  \ Accepted master send
  1 constant accept-send

  \ Accepted master receive
  2 constant accept-recv
  
  \ Out of range clock rate
  : x-out-of-range-clock ( -- ) ." out of range I2C clock rate" cr ;
  
  \ Target address not acknowledged
  : x-i2c-target-noack ( -- ) ." I2C target address not acknowledged" cr ;
  
  \ Arbitration has been lost
  : x-arb-lost ( -- ) ." arbitration has been lost" cr ;
  
  \ Error transmitting data over I2C
  : x-i2c-tx-error ( -- ) ." I2C TX error" cr ;

  \ Overflow receiving from I2C
  : x-i2c-rx-over ( -- ) ." I2C RX overflow" cr ;
  
  \ Invalid I2C peripheral exception
  : x-invalid-i2c ( -- ) ." invalid I2C" cr ;

  \ Out of range address
  : x-out-of-range-addr ( -- ) ." out of range I2C address" cr ;

  \ Operation inappropriate for master mode
  : x-invalid-op-for-master-mode ( -- ) ." invalid op for master mode" cr ;

  \ Operation inappropriate for slave
  : x-invalid-op-for-slave  ( -- ) ." invalid op for slave" cr ;

  \ Operation inappropriate for master
  : x-invalid-op-for-master ( -- ) ." invalid op for master" cr ;
  
  begin-module i2c-internal

    \ I2C peripheral structure
    begin-structure i2c-size

      \ I2C peripheral address
      field: i2c-addr
      
      \ I2C lock
      lock-size +field i2c-lock

      \ I2C inner lock
      lock-size +field i2c-inner-lock
      
      \ Interrupt count - DEBUG
      field: int-count \ DEBUG

      \ I2C peripheral uses 10 bit addresses
      cfield: i2c-10-bit-addr

      \ I2C peripheral is a slave
      cfield: i2c-slave

      \ I2C peripheral receive/transmit mode
      cfield: i2c-mode

      \ Issue restart for I2C peripheral at beginning of transfer
      cfield: i2c-restart

      \ Issue stop for I2C peripheral at end of transfer
      cfield: i2c-stop
      
      \ A stop was detected on the I2C peripheral
      cfield: i2c-stop-det
      
      \ A previous stop was issued for the I2C master peripheral
      cfield: i2c-prev-stop

      \ Issue NACK at end of transfer
      cfield: i2c-nack-set

      \ Master mode
      cfield: i2c-master-mode

      \ Master operation pending
      cfield: i2c-pending
      
      \ Wait for I2C completion
      cfield: i2c-done

      \ I2C has aborted transmission
      cfield: i2c-tx-abort

      \ I2C has a TX error
      cfield: i2c-tx-error
      
      \ I2C has an unacknowledged target address
      cfield: i2c-tx-target-noack
      
      \ I2C bus arbitration has been lost
      cfield: i2c-arb-lost

      \ I2C has RX overflow
      cfield: i2c-rx-over

      \ I2C peripheral is disabled depth
      field: i2c-disabled
      
      \ I2C send or receive buffer address
      field: i2c-data-addr

      \ I2C send or receive byte count
      field: i2c-data-size

      \ I2C send or receive byte offset
      field: i2c-data-offset

    end-structure

    \ An optimization
    : i2c-addr [inlined] ;

    \ Not active
    0 constant mode-not-active

    \ Currently sending
    1 constant mode-send

    \ Currently receiving
    2 constant mode-recv

    \ Master is not active
    0 constant master-not-active

    \ Master is sending
    1 constant master-send

    \ Master is receiving
    2 constant master-recv

    \ No master operation is pending
    0 constant not-pending

    \ Master send is pending
    1 constant send-pending

    \ Master receive is pending
    2 constant recv-pending
    
    \ I2C peripheral count
    2 constant i2c-count

    \ I2C buffers
    i2c-size i2c-count * buffer: i2c-buffers

    \ Validate an I2C peripheral
    : validate-i2c ( i2c -- ) i2c-count u< averts x-invalid-i2c ;

    \ I2C base address
    : I2C_Base ( i2c -- addr ) $4000 * $40044000 + ;

    \ Select I2C structure
    : i2c-select ( i2c -- addr ) i2c-size * i2c-buffers + ;
    
    \ I2C registers

    \ I2C control register
    : IC_CON ( i2c -- addr ) ( I2C_Base ) $00 + ;
    \ I2C target address register
    : IC_TAR ( i2c -- addr ) ( I2C_Base ) $04 + ;
    \ I2C slave address register
    : IC_SAR ( i2c -- addr ) ( I2C_Base ) $08 + ;
    \ I2C Rx/Tx data buffer and command register
    : IC_DATA_CMD ( i2c -- addr ) ( I2C_Base ) $10 + ;
    \ Standard speed I2C clock SCL high count register
    : IC_SS_SCL_HCNT ( i2c -- addr ) ( I2C_Base ) $14 + ;
    \ Standard speed I2C clock SCL low count register
    : IC_SS_SCL_LCNT ( i2c -- addr ) ( I2C_Base ) $18 + ;
    \ Fast mode or fast mode plus I2C clock SCL high count register
    : IC_FS_SCL_HCNT ( i2c -- addr ) ( I2C_Base ) $1C + ;
    \ Fast mode or fast mode plus I2C clock SCL low count register
    : IC_FS_SCL_LCNT ( i2c -- addr ) ( I2C_Base ) $20 + ;
    \ I2C interrupt status register
    : IC_INTR_STAT ( i2c -- addr ) ( I2C_Base ) $2C + ;
    \ I2C interrupt mask register
    : IC_INTR_MASK ( i2c -- addr ) ( I2C_Base ) $30 + ;
    \ I2C raw interrupt status register
    : IC_RAW_INTR_STAT ( i2c -- addr ) ( I2C_Base ) $34 + ;
    \ I2C receive FIFO threshold register
    : IC_RX_TL ( i2c -- addr ) ( I2C_Base ) $38 + ;
    \ I2C transmit FIFO threshold register
    : IC_TX_TL ( i2c -- addr ) ( I2C_Base ) $3C + ;
    \ Clear combined and individual interrupt register
    : IC_CLR_INTR ( i2c -- addr ) ( I2C_Base ) $40 + ;
    \ Clear RX_UNDER interrupt register
    : IC_CLR_RX_UNDER ( i2c -- addr ) ( I2C_Base ) $44 + ;
    \ Clear RX_OVER interrupt register
    : IC_CLR_RX_OVER ( i2c -- addr ) ( I2C_Base ) $48 + ;
    \ Clear TX_OVER interrupt register
    : IC_CLR_TX_OVER ( i2c -- addr ) ( I2C_Base ) $4C + ;
    \ Clear RD_REQ interrupt register
    : IC_CLR_RD_REQ ( i2c -- addr ) ( I2C_Base ) $50 + ;
    \ Clear TX_ABRT interrupt register
    : IC_CLR_TX_ABRT ( i2c -- addr ) ( I2C_Base ) $54 + ;
    \ Clear RX_DONE interrupt register
    : IC_CLR_RX_DONE ( i2c -- addr ) ( I2C_Base ) $58 + ;
    \ Clear ACTIVITY interrupt register
    : IC_CLR_ACTIVITY ( i2c -- addr ) ( I2C_Base ) $5C + ;
    \ Clear STOP_DET interrupt register
    : IC_CLR_STOP_DET ( i2c -- addr ) ( I2C_Base ) $60 + ;
    \ Clear START_DET interrupt register
    : IC_CLR_START_DET ( i2c -- addr ) ( I2C_Base ) $64 + ;
    \ Clear GEN_CALL interrupt register
    : IC_CLR_GEN_CALL ( i2c -- addr ) ( I2C_Base ) $68 + ;
    \ I2C ENABLE register
    : IC_ENABLE ( i2c -- addr ) ( I2C_Base ) $6C + ;
    \ I2C STATUS register
    : IC_STATUS ( i2c -- addr ) ( I2C_Base ) $70 + ;
    \ I2C transmit FIFO level register
    : IC_TXFLR ( i2c -- addr ) ( I2C_Base ) $74 + ;
    \ I2C receive FIFO level register
    : IC_RXFLR ( i2c -- addr ) ( I2C_Base ) $78 + ;
    \ I2C SDA hold time length register
    : IC_SDA_HOLD ( i2c -- addr ) ( I2C_Base ) $7C + ;
    \ I2C transmit abort source register
    : IC_TX_ABRT_SOURCE ( i2c -- addr ) ( I2C_Base ) $80 + ;
    \ Generate slave data NACK register
    : IC_SLV_DATA_NACK_ONLY ( i2c -- addr ) ( I2C_Base ) $84 + ;
    \ DMA control register
    : IC_DMA_CR ( i2c -- addr ) ( I2C_Base ) $88 + ;
    \ DMA transmit data level register
    : IC_DMA_TDLR ( i2c -- addr ) ( I2C_Base ) $8C + ;
    \ DMA receive data level register
    : IC_DMA_RDLR ( i2c -- addr ) ( I2C_Base ) $90 + ;
    \ I2C SDA setup register
    : IC_SDA_SETUP ( i2c -- addr ) ( I2C_Base ) $94 + ;
    \ I2C ACK general call register
    : IC_ACK_GENERAL_CALL ( i2c -- addr ) ( I2C_Base ) $98 + ;
    \ I2C enable status register
    : IC_ENABLE_STATUS ( i2c -- addr ) ( I2C_Base ) $9C + ;
    \ I2C SS, FS or FM+ spike suppression limit
    : IC_FS_SPKLEN ( i2c -- addr ) ( I2C_Base ) $A0 + ;
    \ Clear RESTART_DET interrupt register
    : IC_CLR_RESTART_DET ( i2c -- addr ) ( I2C_Base ) $A8 + ;
    \ Component parameter register 1
    : IC_COMP_PARAM_1 ( i2c -- addr ) ( I2C_Base ) $F4 + ;
    \ I2C component version register
    : IC_COMP_VERSION ( i2c -- addr ) ( I2C_Base ) $F8 + ;
    \ I2C component type register
    : IC_COMP_TYPE ( i2c -- addr ) ( I2C_Base ) $FC + ;

    \ IC_CON bits
    10 bit constant STOP_DET_IF_MASTER_ACTIVE
    9 bit constant RX_FIFO_FULL_HLD_CTRL
    8 bit constant TX_EMPTY_CTRL
    7 bit constant STOP_DET_IF_ADDRESSED
    6 bit constant IC_SLAVE_DISABLE
    5 bit constant IC_RESTART_EN
    4 bit constant IC_10BITADDR_MASTER
    3 bit constant IC_10BITADDR_SLAVE
    $06 constant SPEED
    $04 constant SPEED_FAST
    0 bit constant MASTER_MODE

    \ IC_TAR bits
    11 bit constant SPECIAL
    10 bit constant GC_OR_START
    $3FF constant IC_TAR_ADDR
    
    \ IC_DATA_CMD bits
    11 bit constant FIRST_DATA_BYTE
    10 bit constant RESTART
    9 bit constant STOP
    8 bit constant CMD
    $FF constant DAT

    \ Interrupt bits
    12 bit constant RESTART_DET
    11 bit constant GEN_CALL
    10 bit constant START_DET
    9 bit constant STOP_DET
    8 bit constant ACTIVITY
    7 bit constant RX_DONE
    6 bit constant TX_ABRT
    5 bit constant RD_REQ
    4 bit constant TX_EMPTY
    3 bit constant TX_OVER
    2 bit constant RX_FULL
    1 bit constant RX_OVER
    0 bit constant RX_UNDER

    \ IC_TX_ABRT_SOURCE bits
    23 constant TX_FLUSH_CNT_SHIFT
    $FF100000 constant TX_FLUSH_CNT_MASK
    16 bit constant ABRT_USER_ABRT
    15 bit constant ABRT_SLVRD_INTX
    14 bit constant ABRT_SLV_ARBLOST
    13 bit constant ABRT_SLVFLUSH_TXFIFO
    12 bit constant ARB_LOST
    11 bit constant ABRT_MASTER_DIS
    10 bit constant ABRT_10B_RD_NORSTRT
    9 bit constant ABRT_SBYTE_NORSTRT
    8 bit constant ABRT_HS_NORSTRT
    7 bit constant ABRT_SBYTE_ACKDET
    6 bit constant ABRT_HS_ACKDET
    5 bit constant ABRT_GCALL_READ
    4 bit constant ABRT_GCALL_NOACK
    3 bit constant ABRT_TXDATA_NOACK
    2 bit constant ABRT_10ADDR2_NOACK
    1 bit constant ABRT_10ADDR1_NOACK
    0 bit constant ABRT_7B_ADDR_NOACK

    \ IC_ENABLE bits
    2 bit constant TX_CMD_BLOCK
    1 bit constant IC_ENABLE_ABORT
    0 bit constant IC_ENABLE_ENABLE

    \ IC_ENABLE_STATUS bits
    2 bit constant SLV_RX_DATA_LOST
    1 bit constant SLV_DISABLED_WHILE_BUSY
    0 bit constant IC_EN

    \ IC_STATUS bits
    6 bit constant SLV_ACTIVITY
    5 bit constant MST_ACTIVITY
    4 bit constant RFF
    3 bit constant RFNE
    2 bit constant TFE
    1 bit constant TFNF
    0 bit constant ACTIVITY_STATUS
    
    \ I2C IRQ
    : i2c-irq ( i2c -- irq ) 23 + ;

    \ I2C vector
    : i2c-vector ( i2c -- vector ) i2c-irq 16 + ;

    \ Validate an I2C address
    : validate-i2c-addr ( i2c-addr i2c -- )
      i2c-select i2c-10-bit-addr c@ if
        dup $400 u< averts x-out-of-range-addr
      else
        dup $80 u< averts x-out-of-range-addr
      then
      dup %111 bic %0000000 <> averts x-out-of-range-addr
      dup %111 bic %1111000 <> averts x-out-of-range-addr
      dup %100 bic %0001000 <> averts x-out-of-range-addr
      %1100001 <> averts x-out-of-range-addr
    ;

    \ Validate that sending is allowed with the current master mode
    : validate-send ( i2c -- )
      i2c-select
      dup i2c-slave c@ if
        dup i2c-master-mode c@ master-recv <> if
          i2c-lock release-lock
          ['] x-invalid-op-for-master-mode ?raise
        then
      then
      drop
    ;

    \ Validate that receiving is allowed with the current master mode
    : validate-recv ( i2c -- )
      i2c-select
      dup i2c-slave c@ if
        dup i2c-master-mode c@ master-send <> if
          i2c-lock release-lock
          ['] x-invalid-op-for-master-mode ?raise
        then
      then
      drop
    ;

    \ Validate that the I2C peripheral is a master
    : validate-master ( i2c -- )
      i2c-select
      dup i2c-slave c@ if
        i2c-lock release-lock
        ['] x-invalid-op-for-slave ?raise
      then
      drop
    ;

    \ Validate that the I2C peripheral is a slave
    : validate-slave ( i2c -- )
      i2c-select
      dup i2c-slave c@ 0= if
        i2c-lock release-lock
        ['] x-invalid-op-for-master ?raise
      then
      drop
    ;
    
    \ Wait until I2C peripheral is not busy and then claim control of it
    : claim-i2c ( i2c -- )
      i2c-select i2c-lock claim-lock
    ;

    \ Release the I2C peripheral
    : release-i2c ( i2c -- )
      i2c-select i2c-lock release-lock
    ;
      
    \ Restore the interrupt mask
    : restore-int-mask ( i2c-buffer -- )
      disable-int
      dup i2c-slave c@ if
        0
      else
        STOP_DET
      then
      over i2c-addr @ IC_INTR_MASK @ ACTIVITY and or
      swap i2c-addr @ IC_INTR_MASK !
      enable-int
    ;
    
    \ Signal done state
    : signal-done ( i2c-buffer -- )
      $FF swap i2c-done c!
      wake
    ;
    
    \ Set NACK state
    : set-nack ( i2c-buffer -- )
      dup i2c-disabled @ 0<= if
        IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bic!
        begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and 0= until
      then
      
      disable-int
      begin dup i2c-addr @ IC_STATUS @ SLV_ACTIVITY and 0= until
      $FF over i2c-nack-set c!
      ACTIVITY over i2c-addr @ IC_INTR_MASK bis!
      1 over i2c-addr @ IC_SLV_DATA_NACK_ONLY bis!
      enable-int
      
      dup i2c-disabled @ 0<= if
        IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bis!
        begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and until
      then
      drop
    ;
    
    \ Clear NACK state
    : clear-nack ( i2c-buffer -- )
      dup i2c-nack-set c@ if
        dup i2c-disabled @ 0<= if
          IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bic!
          begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and 0= until
        then
        
        disable-int
        begin dup i2c-addr @ IC_STATUS @ SLV_ACTIVITY and 0= until
        $00 over i2c-nack-set c!
        ACTIVITY over i2c-addr @ IC_INTR_MASK bic!
        1 over i2c-addr @ IC_SLV_DATA_NACK_ONLY bic!
        enable-int
        
        dup i2c-disabled @ 0<= if
          IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bis!
          begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and until
        then
      then
      drop
    ;

    \ Handle TX_ABRT
    : handle-tx-abrt ( i2c-buffer -- )
      dup i2c-mode c@ mode-send = if
        dup i2c-addr @ IC_TX_ABRT_SOURCE @
        TX_FLUSH_CNT_MASK and TX_FLUSH_CNT_SHIFT rshift
        negate over i2c-data-offset @ + 0 max
        over i2c-data-offset !
      then
      dup i2c-addr @ IC_TX_ABRT_SOURCE @
      [ ABRT_SLVRD_INTX
        ABRT_SLV_ARBLOST or
        ABRT_MASTER_DIS or
        ABRT_10B_RD_NORSTRT or
        ABRT_SBYTE_NORSTRT or
        ABRT_HS_NORSTRT or
        ABRT_SBYTE_ACKDET or
        ABRT_HS_ACKDET or
        ABRT_GCALL_READ or ] literal and if
        $FF over i2c-tx-error c!
      else
        dup i2c-addr @ IC_TX_ABRT_SOURCE @ ARB_LOST and if
          $FF over i2c-arb-lost c!
        else
          dup i2c-addr @ IC_TX_ABRT_SOURCE @
          [ ABRT_10ADDR2_NOACK
            ABRT_10ADDR1_NOACK or
            ABRT_7B_ADDR_NOACK or ] literal and if
            $FF over i2c-tx-target-noack c!
          then
        then
      then
      mode-not-active over i2c-mode c!
      dup restore-int-mask
      dup signal-done
      i2c-addr @ IC_CLR_TX_ABRT @ drop
    ;
    
    \ Handle TX_EMPTY
    : handle-tx-empty ( i2c-buffer -- )
      dup i2c-mode c@ mode-send = if
        begin
          dup i2c-data-offset @
          over i2c-data-size @ < if
            dup i2c-addr @ IC_STATUS @ TFNF and if
              dup i2c-data-offset @
              over i2c-data-addr @ + c@
              over i2c-slave c@ 0= if
                over i2c-data-offset @ 0= if
                  over i2c-restart c@ if RESTART or then
                then
                over i2c-data-offset @
                2 pick i2c-data-size @ 1- =
                2 pick i2c-stop c@ 0<> and if
                  $FF 3 pick i2c-prev-stop c!
                  STOP or
                then
              then
              over i2c-addr @ IC_DATA_CMD !
              1 over i2c-data-offset +!
              false
            else
              true
            then
          else
            mode-not-active over i2c-mode c!
            dup restore-int-mask
            dup signal-done
            true
          then
        until
      then
      drop
    ;

    \ Handle RX_FULL
    : handle-rx-full ( i2c-buffer -- )
      dup i2c-mode c@ mode-recv = if
        begin
          dup i2c-data-offset @
          over i2c-data-size @ < if
            dup i2c-addr @ IC_DATA_CMD @ $FF and
            over i2c-data-offset @
            2 pick i2c-data-addr @ + c!
            1 over i2c-data-offset +!
            dup i2c-data-offset @
            over i2c-data-size @ = if
              $00 over i2c-stop-det c!
              mode-not-active over i2c-mode c!
              dup restore-int-mask
              dup signal-done
              true
            else
              dup i2c-slave c@ 0= over i2c-stop-det c@ 0= and if
                dup i2c-data-offset @ over i2c-data-size @ 1- = if
                  dup i2c-stop c@ if
                    $FF over i2c-prev-stop c!
                    [ CMD STOP or ] literal
                  else
                    CMD
                  then
                else
                  CMD
                then
                over i2c-addr @ IC_DATA_CMD !
              then
              dup i2c-addr @ IC_STATUS @ RFNE and 0=
              over i2c-stop-det c@ 0<> over and if
                $00 2 pick i2c-stop-det c!
                over restore-int-mask
                mode-not-active 2 pick i2c-mode c!
                over signal-done
              then
            then
          else
            $00 2 pick i2c-stop-det c!
            mode-not-active over i2c-mode c!
            dup restore-int-mask
            dup signal-done
            true
          then
        until
      else
        dup i2c-mode c@ mode-not-active = over i2c-slave c@ 0<> and if
          master-send over i2c-master-mode c!
          send-pending over i2c-pending c!
          0 over i2c-addr @ IC_INTR_MASK !
          wake
        then
      then
      drop          
    ;

    \ Handle RD_REQ
    : handle-rd-req ( i2c-buffer -- )
      dup i2c-mode c@ mode-not-active = over i2c-slave c@ 0<> and if
        master-recv over i2c-master-mode c!
        recv-pending over i2c-pending c!
        wake
      then
      i2c-addr @ IC_CLR_RD_REQ @ drop
    ;

    \ Handle RX_DONE
    : handle-rx-done ( i2c-buffer -- )
      dup i2c-mode c@ mode-recv = over i2c-slave c@ 0<> and if
        dup restore-int-mask
        0 over i2c-addr @ IC_RX_TL !
        0 over i2c-addr @ IC_TX_TL !
        mode-not-active over i2c-mode c!
        master-not-active over i2c-master-mode c!
        dup signal-done
      then
      i2c-addr @ IC_CLR_RX_DONE @ drop
    ;

    \ Handle STOP_DET
    : handle-stop-det ( i2c-buffer -- )
      dup i2c-prev-stop c@ 0= if
        $FF over i2c-stop-det c!
      else
        $00 over i2c-prev-stop c!
      then
      i2c-addr @ IC_CLR_STOP_DET @ drop
    ;
    
    \ Handle ACTIVITY
    : handle-activity ( i2c-buffer -- )
      dup clear-nack
      i2c-addr @ IC_CLR_ACTIVITY @ drop
    ;

    \ Handle RX_OVER
    : handle-rx-over ( i2c-buffer -- )
      dup i2c-mode c@ mode-send = if
        dup restore-int-mask
        0 over i2c-addr @ IC_RX_TL !
        0 over i2c-addr @ IC_TX_TL !
        $FF over i2c-rx-over c!
        mode-not-active over i2c-mode c!
        dup signal-done
      then
      i2c-addr @ IC_CLR_RX_OVER @ drop
    ;
      
    \ Handle an I2C interrupt
    : handle-i2c-interrupt ( i2c -- )
      i2c-select
      dup i2c-addr @ IC_INTR_STAT @
      dup STOP_DET and if over handle-stop-det then
      dup ACTIVITY and if over handle-activity then
      dup TX_ABRT and if over handle-tx-abrt then
      dup TX_EMPTY and if over handle-tx-empty then
      dup RX_FULL and if over handle-rx-full then
      dup RX_OVER and if over handle-rx-over then
      dup RX_DONE and if over handle-rx-done then
      RD_REQ and if dup handle-rd-req then
      drop
    ;
    
  end-module
    
  \ This is so we can define setting the I2C clock later
  defer i2c-clock! ( clock i2c -- )

  continue-module i2c-internal
    
    \ Initialize I2C
    : init-i2c ( i2c -- )
      disable-int
      dup I2C_Base over i2c-select i2c-addr !
      dup i2c-select
      dup i2c-lock init-lock
      dup i2c-inner-lock init-lock
      0 over int-count !
      $00 over i2c-10-bit-addr c!
      $00 over i2c-slave c!
      mode-not-active over i2c-mode c!
      $00 over i2c-restart c!
      $00 over i2c-stop c!
      $00 over i2c-stop-det c!
      $00 over i2c-nack-set c!
      master-not-active over i2c-master-mode c!
      not-pending over i2c-pending c!
      $00 over i2c-done c!
      $00 over i2c-tx-abort c!
      $00 over i2c-tx-error c!
      $00 over i2c-tx-target-noack c!
      $00 over i2c-arb-lost c!
      $00 over i2c-rx-over c!
      1 over i2c-disabled !
      0 over i2c-data-addr !
      0 over i2c-data-size !
      0 over i2c-data-offset !
      IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bic!
      [ TX_EMPTY_CTRL RX_FIFO_FULL_HLD_CTRL or
      IC_RESTART_EN or
      IC_SLAVE_DISABLE or
      SPEED_FAST or
      MASTER_MODE or
      STOP_DET_IF_ADDRESSED or ] literal over i2c-addr @ IC_CON !
      0 over i2c-addr @ IC_TX_TL !
      0 over i2c-addr @ IC_RX_TL !
      0 swap i2c-addr @ IC_INTR_MASK !
      0 over i2c-irq NVIC_IPR_IP!
      dup 0= if
        [: 0 handle-i2c-interrupt ;]
      else
        [: 1 handle-i2c-interrupt ;]
      then
      over i2c-vector vector!
      400000 over i2c-clock!
      i2c-irq NVIC_ISER_SETENA!
      enable-int
    ;
    
    \ Wait for I2C completion or timeout
    : wait-i2c-complete-or-timeout ( i2c-buffer -- )
      timeout @ no-timeout = if
        [: dup i2c-done c@ 0<> ;] wait
      else
        systick-counter
        begin
          over i2c-done c@ 0<> if
            true
          else
            disable-int
            systick-counter over - timeout @ >= if
              over restore-int-mask
              mode-not-active 2 pick i2c-mode c!
              0 2 pick i2c-data-offset !
              0 2 pick i2c-data-size !
              $00 2 pick i2c-stop c!
              $00 2 pick i2c-restart c!
              $00 2 pick i2c-stop-det c!
              $00 2 pick i2c-prev-stop c!
              over i2c-lock release-lock
              ['] x-timed-out ?raise
            then
            enable-int current-task block-wait false
          then
        until
        drop
      then
    ;
    
    \ Do I2C send
    : do-i2c-send ( c-addr u i2c -- bytes-sent )
      i2c-select
      over if
        disable-int
        tuck i2c-data-size !
        tuck i2c-data-addr !
        0 over i2c-data-offset !
        mode-send over i2c-mode c!
        $00 over i2c-tx-abort c!
        $00 over i2c-tx-error c!
        $00 over i2c-tx-target-noack c!
        $00 over i2c-arb-lost c!
        $00 over i2c-rx-over c!
        $00 over i2c-stop-det c!
        not-pending over i2c-pending c!
        $00 over i2c-done c!
        0 over i2c-addr @ IC_RX_TL !
        7 over i2c-addr @ IC_TX_TL !
        [ TX_EMPTY TX_OVER or TX_ABRT or ] literal
        over i2c-slave c@ if [ RD_REQ RX_DONE or ] literal or then
        over i2c-addr @ IC_INTR_MASK !
        enable-int
        wait-i2c-complete-or-timeout
        $00 over i2c-stop-det c!
        mode-not-active over i2c-mode c!
        dup i2c-data-offset @
        over i2c-tx-target-noack c@
        2 pick i2c-arb-lost c@
        3 pick i2c-tx-error c@
        4 roll i2c-lock release-lock
        triggers x-i2c-tx-error
        triggers x-arb-lost
        triggers x-i2c-target-noack
      else
        i2c-lock release-lock nip
      then
    ;
    
    \ Do I2C receive
    : do-i2c-recv ( c-addr u i2c -- bytes-recv )
      i2c-select
      over if
        disable-int
        tuck i2c-data-size !
        tuck i2c-data-addr !
        0 over i2c-data-offset !
        mode-recv over i2c-mode c!
        $00 over i2c-tx-abort c!
        $00 over i2c-tx-error c!
        $00 over i2c-tx-target-noack c!
        $00 over i2c-arb-lost c!
        $00 over i2c-rx-over c!
        not-pending over i2c-pending c!
        $00 over i2c-done c!
        0 over i2c-addr @ IC_RX_TL !
        0 over i2c-addr @ IC_TX_TL !
        [ RX_FULL RX_OVER or STOP_DET or TX_ABRT or ] literal
        over i2c-slave c@ if RD_REQ or then
        over i2c-addr @ IC_INTR_MASK !
        dup i2c-slave c@ 0= if
          $00 over i2c-stop-det c!
          CMD over i2c-restart c@ if RESTART or then
          over i2c-data-size @ 1 = 2 pick i2c-stop c@ 0<> and if
            $FF 2 pick i2c-prev-stop c!
            STOP or
          then
          over i2c-addr @ IC_DATA_CMD !
        then
        enable-int
        wait-i2c-complete-or-timeout
        $00 over i2c-stop-det c!
        mode-not-active over i2c-mode c!
        dup i2c-data-offset @
        over i2c-tx-target-noack c@
        2 pick i2c-arb-lost c@
        3 pick i2c-rx-over c@
        4 roll i2c-lock release-lock
        triggers x-i2c-rx-over
        triggers x-arb-lost
        triggers x-i2c-target-noack
      else
        i2c-lock release-lock nip
      then
    ;

  end-module> import
  
  \ Disable I2C
  : disable-i2c ( i2c -- )
    dup validate-i2c i2c-select
    [:
      1 over i2c-disabled +!
      dup i2c-disabled @ 1 = if
        $000 over i2c-addr @ IC_INTR_MASK !
        IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bic!
        begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and 0= until
      then
      drop
    ;] over i2c-inner-lock with-lock
  ;
  
  \ Enable I2C
  : enable-i2c ( i2c -- )
    dup validate-i2c i2c-select
    [:
      -1 over i2c-disabled +!
      dup i2c-disabled @ 0= if
        dup restore-int-mask
        IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bis!
        begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and until
      then
      drop
    ;] over i2c-inner-lock with-lock
  ;

  continue-module i2c-internal
    
    \ Carry out an operation with I2C disabled
    : with-i2c-disabled ( ??? xt i2c -- ??? ) ( xt: ??? i2c -- ??? )
      dup validate-i2c
      dup claim-i2c
      dup disable-i2c
      dup >r
      swap try
      r> dup enable-i2c
      release-i2c
      ?raise
    ;

  end-module
  
  \ I2C alternate function
  : i2c-alternate ( i2c -- alternate ) validate-i2c 3 ;
  
  \ Set a pin to be an I2C pin
  : i2c-pin ( i2c pin -- )
    over validate-i2c
    false over PADS_BANK0_SLEWFAST!
    true over PADS_BANK0_SCHMITT!
    swap i2c-alternate swap alternate-pin
  ;
  
  \ Set the I2C peripheral clock; note that it will always use fast mode
  :noname ( clock i2c -- )
    dup validate-i2c ( clock i2c )
    over 0<> averts x-out-of-range-clock ( clock i2c )
    125000000 2 pick 2 / + 2 pick / ( clock i2c period )
    dup 3 * 5 / ( clock i2c period lcnt )
    tuck - ( clock i2c-buffer lcnt hcnt )
    dup 8 >= averts x-out-of-range-clock ( clock i2c lcnt hcnt )
    dup $FFFF <= averts x-out-of-range-clock ( clock i2c lcnt hcnt )
    over 8 >= averts x-out-of-range-clock ( clock i2c lcnt hcnt )
    over $FFFF <= averts x-out-of-range-clock ( clock i2c lcnt hcnt )
    rot [:
      i2c-select ( clock lcnt hcnt i2c-buffer )
      3 roll 1000000 < if ( lcnt hcnt i2c-buffer )
        dup i2c-addr @ IC_SDA_HOLD @ $FFFF bic ( lcnt hcnt i2c-buffer hold )
        [ 125000000 3 * 10000000 / 1 + ] literal ( lcnt hcnt i2c-buffer hold tx-hold )
        4 pick 2 - over > averts x-out-of-range-clock ( lcnt hcnt i2c-buffer hold tx-hold )
        or over i2c-addr @ IC_SDA_HOLD ! ( lcnt hcnt i2c-buffer )
      else ( lcnt hcnt i2c-buffer )
        dup i2c-addr @ IC_SDA_HOLD @ $FFFF bic ( lcnt hcnt i2c-buffer hold )
        [ 125000000 3 * 25000000 / 1 + ] literal ( lcnt hcnt i2c-buffer hold tx-hold )
        4 pick 2 - over > averts x-out-of-range-clock ( lcnt hcnt i2c-buffer hold tx-hold )
        or over i2c-addr @ IC_SDA_HOLD ! ( lcnt hcnt i2c-buffer )
      then
      tuck i2c-addr @ IC_FS_SCL_HCNT ! ( lcnt i2c-buffer )
      2dup i2c-addr @ IC_FS_SCL_LCNT ! ( lcnt i2c-buffer )
      swap 16 / 1 min swap i2c-addr @ IC_FS_SPKLEN ! ( )
    ;] swap with-i2c-disabled
  ; ' i2c-clock! defer!

  \ Set the I2C peripheral as master
  : master-i2c ( i2c -- )
    dup validate-i2c
    [:
      [ IC_SLAVE_DISABLE MASTER_MODE or ] literal over I2C_Base IC_CON bis!
      $00 swap i2c-select i2c-slave c!
    ;] swap with-i2c-disabled
  ;

  \ Set the I2C peripheral as slave
  : slave-i2c ( i2c -- )
    dup validate-i2c
    [:
      [ IC_SLAVE_DISABLE MASTER_MODE or ] literal over I2C_Base IC_CON bic!
      $FF swap i2c-select i2c-slave c!
    ;] swap with-i2c-disabled
  ;

  \ Set 7-bit addresses
  : 7-bit-i2c-addr ( i2c -- )
    dup validate-i2c
    [:
      [ IC_10BITADDR_MASTER IC_10BITADDR_SLAVE or ] literal
      over I2C_Base IC_CON bic!
      $00 swap i2c-select i2c-10-bit-addr c!
    ;] swap with-i2c-disabled
  ;

  \ Set 10-bit addresses
  : 10-bit-i2c-addr ( i2c -- )
    dup validate-i2c
    [:
      [ IC_10BITADDR_MASTER IC_10BITADDR_SLAVE or ] literal
      over I2C_Base IC_CON bis!
      $FF swap i2c-select i2c-10-bit-addr c!
    ;] swap with-i2c-disabled
  ;

  \ Set the I2C target address
  : i2c-target-addr! ( i2c-addr i2c -- )
    dup validate-i2c
    2dup validate-i2c-addr
    [:
      dup I2C_Base IC_TAR @ IC_TAR_ADDR bic rot IC_TAR_ADDR and or swap
      I2C_Base IC_TAR !
    ;] swap with-i2c-disabled
  ;

  \ Set the I2C slave address
  : i2c-slave-addr! ( i2c-addr i2c -- )
    dup validate-i2c
    2dup validate-i2c-addr
    [: I2C_Base IC_SAR ! ;] swap with-i2c-disabled
  ;
  
  continue-module i2c-internal
    
    \ Wait for master send or receive on I2C peripheral with a timeout
    : wait-i2c-master-timeout ( ticks i2c -- accepted )
      dup validate-i2c
      systick-counter swap
      dup i2c-select
      
      [ RX_FULL RD_REQ or ] literal over i2c-addr @ IC_INTR_MASK !
      
      begin
        systick-counter 3 pick - 4 pick < averts x-timed-out
        over claim-i2c
        over validate-slave
        dup i2c-pending c@ send-pending = if
          not-pending over i2c-pending c! accept-send true
        else
          dup i2c-pending c@ recv-pending = if
            not-pending over i2c-pending c! accept-recv true
          else
            0 false
          then
        then
        3 pick release-i2c dup not if
          nip pause current-task block-wait
        then
      until
      nip nip nip nip
    ;
    
    \ Wait for master send on I2C peripheral with a timeout
    : wait-i2c-master-send-timeout ( ticks i2c -- )
      dup validate-i2c
      systick-counter swap
      dup i2c-select
      
      RX_FULL over i2c-addr @ IC_INTR_MASK !
      
      begin
        systick-counter 3 pick - 4 pick < averts x-timed-out
        over claim-i2c
        over validate-slave
        dup i2c-pending c@ send-pending = if
          not-pending over i2c-pending c! true
        else
          false
        then
        2 pick release-i2c dup not if
          pause current-task block-wait
        then
      until
      2drop 2drop
    ;
  
    \ Wait for master receive on I2C peripheral with a timeout
    : wait-i2c-master-recv-timeout ( ticks i2c -- )
      dup validate-i2c
      systick-counter swap
      dup i2c-select
      
      RD_REQ over i2c-addr @ IC_INTR_MASK !
      
      begin
        systick-counter 3 pick - 4 pick < averts x-timed-out
        over claim-i2c
        over validate-slave
        dup i2c-pending c@ recv-pending = if
          not-pending over i2c-pending c! true
        else
          false
        then
        2 pick release-i2c dup not if
          pause current-task block-wait
        then
      until
      2drop 2drop
    ;
    
    \ Wait for master send or receive on I2C peripheral without a timeout
    : wait-i2c-master-indefinite ( i2c -- accepted )
      dup validate-i2c
      dup i2c-select
      
      [ RX_FULL RD_REQ or ] literal over i2c-addr @ IC_INTR_MASK !
      
      begin
        over claim-i2c
        over validate-slave
        dup i2c-pending c@ send-pending = if
          not-pending over i2c-pending c! accept-send true
        else
          dup i2c-pending c@ recv-pending = if
            not-pending over i2c-pending c! accept-recv true
          else
            0 false
          then
        then
        3 pick release-i2c dup not if
          nip pause current-task block-wait
        then
      until
      nip nip
    ;
    
    \ Wait for master send on I2C peripheral without a timeout
    : wait-i2c-master-send-indefinite ( i2c -- )
      dup validate-i2c
      dup i2c-select
      
      RX_FULL over i2c-addr @ IC_INTR_MASK !
      
      begin
        over claim-i2c
        over validate-slave
        dup i2c-pending c@ send-pending = if
          not-pending over i2c-pending c! true
        else
          false
        then
        2 pick release-i2c dup not if
          pause current-task block-wait
        then
      until
      2drop
    ;
  
    \ Wait for master receive on I2C peripheral without a timeout
    : wait-i2c-master-recv-indefinite ( i2c -- )
      dup validate-i2c
      dup i2c-select
      
      RD_REQ over i2c-addr @ IC_INTR_MASK !
      
      begin
        over claim-i2c
        over validate-slave
        dup i2c-pending c@ recv-pending = if
          not-pending over i2c-pending c! true
        else
          false
        then
        2 pick release-i2c dup not if
          pause current-task block-wait
        then
      until
      2drop
    ;
  
  end-module
    
  \ Wait for master send or receive on I2C peripheral with or without a timeout
  : wait-i2c-master ( i2c -- accepted )
    dup validate-i2c
    timeout @ no-timeout = if
      wait-i2c-master-indefinite
    else
      timeout @ swap wait-i2c-master-timeout
    then
  ;
  
  \ Wait for master send on I2C peripheral with or without a timeout
  : wait-i2c-master-send ( i2c -- )
    dup validate-i2c
    timeout @ no-timeout = if
      wait-i2c-master-send-indefinite
    else
      timeout @ swap wait-i2c-master-send-timeout
    then
  ;

  \ Wait for master receive on I2C peripheral with or without a timeout
  : wait-i2c-master-recv ( i2c -- )
    dup validate-i2c
    timeout @ no-timeout = if
      wait-i2c-master-recv-indefinite
    else
      timeout @ swap wait-i2c-master-recv-timeout
    then
  ;
  
  \ Specify a NACK to be sent on the next received byte
  : i2c-nack ( i2c -- )
    dup validate-i2c dup claim-i2c dup validate-slave
    dup i2c-select set-nack
    release-i2c
  ;
  
  \ Send data over I2C peripheral
  : >i2c ( c-addr u i2c -- bytes-sent )
    dup validate-i2c dup claim-i2c dup validate-send
    $00 over i2c-select i2c-stop c!
    $00 over i2c-select i2c-restart c!
    do-i2c-send
  ;

  \ Send data over I2C peripheral then stop
  : >i2c-stop ( c-addr u i2c -- bytes-sent )
    dup validate-i2c dup claim-i2c dup validate-master dup validate-send
    $FF over i2c-select i2c-stop c!
    $00 over i2c-select i2c-restart c!
    do-i2c-send
  ;

  \ Send data over I2C peripheral with restart
  : >i2c-restart ( c-addr u i2c -- bytes-sent )
    dup validate-i2c dup claim-i2c dup validate-master dup validate-send
    $00 over i2c-select i2c-stop c!
    $FF over i2c-select i2c-restart c!
    do-i2c-send
  ;

  \ Send data over I2C peripheral with restart then stop
  : >i2c-restart-stop ( c-addr u i2c -- bytes-sent )
    dup validate-i2c dup claim-i2c dup validate-master dup validate-send
    $FF over i2c-select i2c-stop c!
    $FF over i2c-select i2c-restart c!
    do-i2c-send
  ;

  \ Receive data from I2C peripheral
  : i2c> ( c-addr u i2c -- bytes-recv )
    dup validate-i2c dup claim-i2c dup validate-recv
    $00 over i2c-select i2c-stop c!
    $00 over i2c-select i2c-restart c!
    do-i2c-recv
  ;

  \ Receive data from I2C peripheral then stop
  : i2c-stop> ( c-addr u i2c -- bytes-recv )
    dup validate-i2c dup claim-i2c dup validate-master dup validate-recv
    $FF over i2c-select i2c-stop c!
    $00 over i2c-select i2c-restart c!
    do-i2c-recv
  ;

  \ Receive data from I2C peripheral with restart
  : i2c-restart> ( c-addr u i2c -- bytes-recv )
    dup validate-i2c dup claim-i2c dup validate-master dup validate-recv
    $00 over i2c-select i2c-stop c!
    $FF over i2c-select i2c-restart c!
    do-i2c-recv
  ;

  \ Receive data from I2C peripheral with restart then stop
  : i2c-restart-stop> ( c-addr u i2c -- bytes-recv )
    dup validate-i2c dup claim-i2c dup validate-master dup validate-recv
    $FF over i2c-select i2c-stop c!
    $FF over i2c-select i2c-restart c!
    do-i2c-recv
  ;
  
  \ Clear I2C peripheral state
  : clear-i2c ( i2c -- )
    dup validate-i2c
    dup claim-i2c
    dup i2c-select
    disable-int
    
    dup i2c-disabled @ 0<= if
      IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bic!
      begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and 0= until
    then
    
    begin dup i2c-addr @ IC_STATUS @ SLV_ACTIVITY and 0= until
    begin dup i2c-addr @ IC_STATUS @ MST_ACTIVITY and 0= until
        
    dup i2c-addr @ IC_CLR_STOP_DET @ drop
    dup i2c-addr @ IC_CLR_RX_DONE @ drop
    dup i2c-addr @ IC_CLR_TX_ABRT @ drop
    $00 over i2c-stop-det c!
    $00 over i2c-prev-stop c!
    $00 over i2c-tx-abort c!
    $00 over i2c-tx-error c!
    $00 over i2c-tx-target-noack c!
    $00 over i2c-arb-lost c!
    $00 over i2c-rx-over c!
    mode-not-active over i2c-mode c!
    master-not-active over i2c-master-mode c!
    not-pending over i2c-pending c!
    clear-nack    
    enable-int
    
    dup i2c-disabled @ 0<= if
      IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bis!
      begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and until
    then
    
    release-i2c
  ;

end-module> import

\ Initialize I2C
: init ( -- )
  init
  0 [ i2c-internal ] :: init-i2c
  1 [ i2c-internal ] :: init-i2c
;

reboot
