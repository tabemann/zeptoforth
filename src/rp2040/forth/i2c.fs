\ Copyright (c) 2022-2023 Travis Bemann
\ Copyright (c) 2024 Paul Koning
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

  \ Attempt to send data as slave with unready master
  : x-master-not-ready ( -- ) ." master not ready" cr ;
  
  begin-module i2c-internal

    \ Print depth DEBUG
    : depth. depth [char] 0 + internal::serial-emit ;
    
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

      \ I2C user abort count
      field: i2c-user-abort-count

    end-structure

    \ An optimization
    : i2c-addr [inlined] ;

    \ Not active
    0 constant mode-not-active

    \ Currently sending
    1 constant mode-send

    \ Currently receiving
    2 constant mode-recv

    \ Currently completed a send
    3 constant mode-done-send

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

    \ I2C is not done
    0 constant not-done

    \ I2C is done with the immediate operation
    1 constant done-now

    \ I2C is finally done
    2 constant finally-done
    
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
    9 bit constant STOP_BIT
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
    $FF800000 constant TX_FLUSH_CNT_MASK
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
      STOP_DET
      over i2c-addr @ IC_INTR_MASK @ ACTIVITY and or
      swap i2c-addr @ IC_INTR_MASK !
      enable-int
    ;
    
    \ Signal done state
    : signal-done ( i2c-buffer -- )
      done-now swap i2c-done c!
      wake
    ;

    \ Signal final state
    : signal-final ( i2c-buffer -- )
      finally-done swap i2c-done c!
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

    \ Clear ABRT_SBYTE_NORSTRT
    : clear-abrt-sbyte-norstrt ( i2c-buffer -- )
      dup i2c-addr @ IC_TX_ABRT_SOURCE @ ABRT_SBYTE_NORSTRT and if
        IC_RESTART_EN over i2c-addr @ IC_CON bis!
        [ SPECIAL GC_OR_START or ] literal over i2c-addr @ IC_TAR bic!
      then
      drop
    ;
    
    \ Handle TX_ABRT
    : handle-tx-abrt ( i2c-buffer -- )
      \ dup clear-abrt-sbyte-norstrt
      dup i2c-mode c@ dup mode-send = swap mode-done-send = or if
        dup i2c-addr @ IC_TX_ABRT_SOURCE @
        TX_FLUSH_CNT_MASK and TX_FLUSH_CNT_SHIFT rshift
        negate over i2c-data-offset @ + 0 max
        over i2c-data-offset !
      then
      emit-hook @ emit?-hook @ { orig-emit-hook orig-emit?-hook }
      ['] internal::serial-emit emit-hook !
      ['] internal::serial-emit? emit?-hook !
      space dup i2c-addr @ IC_TX_ABRT_SOURCE @ h.8 space
      orig-emit-hook emit-hook ! orig-emit?-hook emit?-hook !
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
          dup i2c-addr @ IC_TX_ABRT_SOURCE @ ABRT_USER_ABRT and if
            1 over i2c-user-abort-count +!
          else
            dup i2c-addr @ IC_TX_ABRT_SOURCE @
            [ ABRT_10ADDR2_NOACK
            ABRT_10ADDR1_NOACK or
            ABRT_7B_ADDR_NOACK or ] literal and if
              $FF over i2c-tx-target-noack c!
            then
          then
        then
      then
      mode-not-active over i2c-mode c!
      dup restore-int-mask
      dup signal-done
      i2c-addr @ IC_CLR_TX_ABRT @ drop
    ;
    
    \ Handle TX_EMPTY
    : handle-tx-empty { buf -- }
      buf i2c-mode c@ mode-send = if \ [char] k internal::serial-emit depth.
        begin
          buf i2c-data-offset @
          buf i2c-data-size @ < if \ [char] l internal::serial-emit depth.
            buf i2c-addr @ IC_RAW_INTR_STAT @ RESTART_DET and if
              buf i2c-addr @ IC_CLR_RESTART_DET @ drop
              buf i2c-addr @ IC_CLR_STOP_DET @ drop
              buf i2c-addr @ IC_CLR_RX_DONE @ drop
              buf i2c-data-offset @ 0= if
                false
              else
                mode-done-send buf i2c-mode c!
                true
              then
            else
              buf i2c-addr @ IC_INTR_STAT @ RX_DONE and if \ [char] m internal::serial-emit depth.
                mode-done-send buf i2c-mode c!
                buf i2c-addr @ IC_CLR_RX_DONE @ drop
                true
              else
                buf i2c-addr @ IC_INTR_STAT @ TX_ABRT and if \ [char] n internal::serial-emit depth.
                  buf handle-tx-abrt
                  true
                else \ [char] o internal::serial-emit depth.
                  buf i2c-addr @ IC_STATUS @ TFE and if \ [char] p internal::serial-emit depth.
                    buf i2c-data-offset @ buf i2c-data-addr @ + c@ { command }
                    buf i2c-slave c@ 0= if \ [char] q internal::serial-emit depth.
                      buf i2c-data-offset @ 0= if \ [char] r internal::serial-emit depth.
                        buf i2c-restart c@ if RESTART command or to command then
                      then
                      buf i2c-data-offset @
                      buf i2c-data-size @ 1- =
                      buf i2c-stop c@ 0<> and if \ [char] s internal::serial-emit depth.
                        $FF buf i2c-prev-stop c!
                        STOP_BIT command or to command
                      then
                    then
                    \ emit-hook @ emit?-hook @ { orig-emit-hook orig-emit?-hook }
                    \ ['] internal::serial-emit emit-hook !
                    \ ['] internal::serial-emit? emit?-hook !
                    \ ."  *" command h.8 ." * "
                    \ orig-emit-hook emit-hook ! orig-emit?-hook emit?-hook !
                    command buf i2c-addr @ IC_DATA_CMD !
                    buf i2c-slave c@ if
                      buf i2c-addr @ IC_CLR_RD_REQ @ drop
                    then
                    1 buf i2c-data-offset +! \ [char] t internal::serial-emit depth.
                    false
                  else
                    true
                  then
                then
              then
            then
          else \ [char] u internal::serial-emit depth.
            mode-done-send buf i2c-mode c!
            true
          then
        until
      else
        \ [char] v internal::serial-emit  depth.
        buf restore-int-mask
      then
    ;

    \ Handle RX_FULL
    : handle-rx-full { buf -- }
      buf i2c-mode c@ mode-recv = if \ [char] a internal::serial-emit depth.
        buf i2c-addr @ IC_RAW_INTR_STAT @ RESTART_DET and if
          buf i2c-addr @ IC_CLR_RESTART_DET @ drop
          buf i2c-addr @ IC_CLR_STOP_DET @ drop
          buf i2c-data-offset @ 0<> if
            $FF buf i2c-stop-det c!
          then
        then
        buf i2c-addr @ IC_INTR_STAT @ STOP_DET and if
          buf i2c-prev-stop c@ 0= if \ [char] b internal::serial-emit depth.
            $FF buf i2c-stop-det c!
          else \ [char] c internal::serial-emit depth.
            $00 buf i2c-prev-stop c!
          then
          buf i2c-addr @ IC_CLR_STOP_DET @ drop
        then
        buf i2c-data-size @ buf i2c-data-offset @ -
        buf i2c-addr @ IC_RXFLR @ $1F and min { bytes }
        buf i2c-data-offset @ bytes + buf i2c-data-offset @ ?do \ [char] d internal::serial-emit depth.
          buf i2c-addr @ IC_DATA_CMD @ $FF and buf i2c-data-addr @ i + c!
          buf i2c-slave c@ 0=
          i buf i2c-data-size @ 1- < and if \ [char] e internal::serial-emit depth.
            CMD { command }
            buf i2c-stop c@ 0<> buf i2c-data-size @ 2 - i = and if \ [char] g internal::serial-emit depth.
              $FF buf i2c-prev-stop c!
              STOP_BIT command or to command
            then
            command buf i2c-addr @ IC_DATA_CMD !
          then
        loop
        bytes buf i2c-data-offset +!
        buf i2c-stop-det c@ 0<>
        buf i2c-addr @ IC_RXFLR @ $1F and 0= and if \ [char] h internal::serial-emit depth.
          mode-not-active buf i2c-mode c!
          buf restore-int-mask
          buf signal-final
        else
          buf i2c-data-offset @ buf i2c-data-size @ = if \ [char] i internal::serial-emit depth.
            mode-not-active buf i2c-mode c!
            buf restore-int-mask
            buf signal-done
          then
        then
      else
        buf i2c-mode c@ mode-not-active = buf i2c-slave c@ 0<> and if \ [char] j internal::serial-emit depth.
          master-send buf i2c-master-mode c!
          send-pending buf i2c-pending c!
          buf restore-int-mask
          wake
        then
      then
    ;

    \ Handle RD_REQ
    : handle-rd-req ( i2c-buffer -- )
      \ [char] $ internal::serial-emit
      dup i2c-mode c@ mode-not-active = over i2c-slave c@ 0<> and if
        master-recv over i2c-master-mode c!
        recv-pending over i2c-pending c!
        \ [char] * internal::serial-emit
        wake
      then
      i2c-addr @ IC_CLR_RD_REQ @ drop
    ;

    \ Handle RX_DONE
    : handle-rx-done ( i2c-buffer -- )
      dup i2c-mode c@ mode-send = over i2c-slave c@ 0<> and if
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
    : handle-stop-det { buf -- }
      buf i2c-prev-stop c@ 0= if
        $FF buf i2c-stop-det c!
      else
        $00 buf i2c-prev-stop c!
      then
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      
      buf i2c-mode c@ mode-recv =
      buf i2c-addr @ IC_STATUS @ RFNE and 0= and if
        buf i2c-stop-det c@ if
          buf restore-int-mask
          buf signal-final
        then
      then
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
    : handle-i2c-interrupt { index -- } \ [char] A internal::serial-emit \ depth.
      index i2c-irq NVIC_ICPR_CLRPEND! \ [char] A internal::serial-emit \ depth.
      index i2c-select { buf } \ [char] B internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ STOP_DET and if
        \ [char] Q internal::serial-emit \ depth.
        buf handle-stop-det
        \ [char] R internal::serial-emit \ depth.
      then \ [char] C internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ ACTIVITY and if
        \ [char] ! internal::serial-emit \ depth.
        buf handle-activity
      then \ [char] D internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ TX_EMPTY and if
        \ [char] # internal::serial-emit \ depth.
        buf handle-tx-empty
      then \ [char] E internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ RX_FULL and if
        \ [char] Y internal::serial-emit \ depth.
        buf handle-rx-full
        \ [char] Z internal::serial-emit \ depth.
      then \ [char] F internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ RX_OVER and if
        \ [char] $ internal::serial-emit \ depth.
        buf handle-rx-over
      then \ [char] G internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ RX_DONE and if
        \ [char] % internal::serial-emit \ depth.
        buf handle-rx-done
      then \ [char] H internal::serial-emit \ depth.
      \ buf clear-abrt-sbyte-norstrt
      buf i2c-addr @ IC_INTR_STAT @ TX_ABRT and if
        \ [char] @ internal::serial-emit \ depth.
        buf handle-tx-abrt
      then \ [char] I internal::serial-emit \ depth.
      buf i2c-addr @ IC_INTR_STAT @ RD_REQ and { rd-req-active } \ [char] J internal::serial-emit \ depth.
      buf i2c-mode c@ mode-done-send = if
        \ [char] @ internal::serial-emit \ depth.
        buf restore-int-mask
        \ [char] ~ internal::serial-emit \ depth.
        buf i2c-addr @ IC_CLR_STOP_DET @ drop
        \ [char] $ internal::serial-emit \ depth.
        buf signal-done
        \ [char] % internal::serial-emit \ depth.
        mode-not-active buf i2c-mode c!
        \ [char] ^ internal::serial-emit \ depth.
      then \ [char] K internal::serial-emit \ depth.
      rd-req-active if
        \ [char] < internal::serial-emit \ depth.
        buf handle-rd-req
      then \ [char] B internal::serial-emit depth.
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
      not-done over i2c-done c!
      $00 over i2c-tx-abort c!
      $00 over i2c-tx-error c!
      $00 over i2c-tx-target-noack c!
      $00 over i2c-arb-lost c!
      $00 over i2c-rx-over c!
      1 over i2c-disabled !
      0 over i2c-data-addr !
      0 over i2c-data-size !
      0 over i2c-data-offset !
      0 over i2c-user-abort-count !
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

    \ Saved second core hook
    variable core-init-hook-saved

    \ Initialize I2C on the second core
    : init-i2c-core-1 ( -- )
      task::core-init-hook @ core-init-hook-saved !
      [:
        core-init-hook-saved @ execute
        disable-int
        0 0 i2c-irq NVIC_IPR_IP!
        0 1 i2c-irq NVIC_IPR_IP!
        0 i2c-irq NVIC_ISER_SETENA!
        1 i2c-irq NVIC_ISER_SETENA!
        enable-int
      ;] task::core-init-hook !
    ;
    
    \ Wait for I2C completion or timeout
    : wait-i2c-complete-or-timeout { buf -- } \ [char] C internal::serial-emit depth.
      timeout @ no-timeout = if \ [char] D internal::serial-emit depth.
        \ [char] A internal::serial-emit depth.
        buf [: dup i2c-done c@ 0<> ;] wait drop \ [char] F internal::serial-emit depth.
        \ [char] B internal::serial-emit depth.
      else
        systick-counter { start-systick }
        begin \ [char] G internal::serial-emit depth.
          buf i2c-done c@ if \ [char] H internal::serial-emit depth.
            true
          else \ [char] I internal::serial-emit depth.
            disable-int
            systick-counter start-systick - timeout @ >= if \ [char] J internal::serial-emit depth.
              buf i2c-slave c@ 0= if
                IC_ENABLE_ABORT buf i2c-addr @ IC_ENABLE bis! \ [char] K internal::serial-emit depth.
                begin buf i2c-addr @ IC_ENABLE @ IC_ENABLE_ABORT and 0= until \ [char] L internal::serial-emit depth.
              then \ [char] M internal::serial-emit depth.
              buf i2c-addr @ IC_CLR_STOP_DET @ drop
              buf i2c-addr @ IC_CLR_RX_DONE @ drop
              \ dup clear-abrt-sbyte-norstrt
              buf i2c-addr @ IC_CLR_TX_ABRT @ drop
              mode-not-active buf i2c-mode c!
              0 buf i2c-data-offset !
              0 buf i2c-data-size !
              $00 buf i2c-stop c!
              $00 buf i2c-restart c!
              \ $00 buf i2c-stop-det c!
              $00 buf i2c-prev-stop c!
              $00 buf i2c-tx-abort c!
              $00 buf i2c-tx-error c!
              $00 buf i2c-tx-target-noack c!
              $00 buf i2c-arb-lost c!
              $00 buf i2c-rx-over c!
              buf restore-int-mask \ This enables interrupts
              buf i2c-lock release-lock \ [char] N internal::serial-emit depth.
              ['] x-timed-out ?raise
            then \ [char] O internal::serial-emit depth.
            enable-int wake-counter @ current-task block-wait false \ [char] P internal::serial-emit depth.
          then \ [char] Q internal::serial-emit depth.
        until \ [char] R internal::serial-emit depth.
      then \ [char] S internal::serial-emit depth.
    ;
    
    \ Do I2C send
    : do-i2c-send { addr bytes index -- bytes-sent }
      index i2c-select { buf }
      bytes if \ [char] A internal::serial-emit depth.
        disable-int
        buf i2c-slave c@ 0<> buf i2c-master-mode c@ master-recv <> and if \ [char] B internal::serial-emit depth.
          enable-int ['] x-master-not-ready ?raise
        then \ [char] C internal::serial-emit depth.
        bytes buf i2c-data-size !
        addr buf i2c-data-addr !
        0 buf i2c-data-offset !
        mode-send buf i2c-mode c!
        $00 buf i2c-tx-abort c!
        $00 buf i2c-tx-error c!
        $00 buf i2c-tx-target-noack c!
        $00 buf i2c-arb-lost c!
        $00 buf i2c-rx-over c!
        $00 buf i2c-stop-det c!
        not-pending buf i2c-pending c!
        not-done buf i2c-done c!
        0 buf i2c-addr @ IC_TX_TL !
        0 buf i2c-addr @ IC_RX_TL ! \ [char] D internal::serial-emit depth.
        buf i2c-restart c@ if \ [char] E internal::serial-emit depth.
          $00 buf i2c-stop-det c!
        then \ [char] F internal::serial-emit depth.
        [ TX_EMPTY TX_OVER or TX_ABRT or ] literal { mask }
        buf i2c-slave c@ if mask [ RD_REQ RX_DONE or ] literal or to mask then
        mask buf i2c-addr @ IC_INTR_MASK ! \ [char] G internal::serial-emit depth.
        enable-int \ [char] H internal::serial-emit depth.
        buf wait-i2c-complete-or-timeout \ [char] I internal::serial-emit depth.
        $00 buf i2c-stop-det c!
        mode-not-active buf i2c-mode c!
        buf i2c-data-offset @ { offset }
        buf i2c-tx-target-noack c@ { noack }
        buf i2c-arb-lost c@ { arb-lost }
        buf i2c-tx-error c@ { rx-over }
        buf i2c-lock release-lock
        rx-over triggers x-i2c-tx-error
        arb-lost triggers x-arb-lost
        noack triggers x-i2c-target-noack \ [char] J internal::serial-emit depth.
        offset
      else \ [char] K internal::serial-emit depth.
        buf i2c-lock release-lock
        0
      then \ [char] L internal::serial-emit depth.
    ;
    
    \ Do I2C receive
    : do-i2c-recv { addr bytes index -- bytes-recv }
      index i2c-select { buf } \ [char] a internal::serial-emit depth.
      bytes if
        disable-int
        buf i2c-slave c@ 0<> buf i2c-master-mode c@ master-send <> and if \ [char] b internal::serial-emit depth.
          enable-int ['] x-master-not-ready ?raise
        then \ [char] c internal::serial-emit depth.
        bytes buf i2c-data-size !
        addr buf i2c-data-addr !
        0 buf i2c-data-offset !
        mode-recv buf i2c-mode c!
        $00 buf i2c-tx-abort c!
        $00 buf i2c-tx-error c!
        $00 buf i2c-tx-target-noack c!
        $00 buf i2c-arb-lost c!
        $00 buf i2c-rx-over c!
        not-pending buf i2c-pending c!
        buf i2c-done c@ done-now = if not-done buf i2c-done c! then \ [char] d internal::serial-emit depth.
        0 buf i2c-addr @ IC_RX_TL ! \ [char] e internal::serial-emit depth.
        0 buf i2c-addr @ IC_TX_TL ! \ [char] f internal::serial-emit depth.
        [ RX_FULL RX_OVER or STOP_DET or TX_ABRT or ] literal { mask } \ [char] g internal::serial-emit depth.
        buf i2c-slave c@ if mask RD_REQ or to mask then \ [char] h internal::serial-emit depth.
        mask buf i2c-addr @ IC_INTR_MASK ! \ [char] i internal::serial-emit depth.
        buf i2c-done c@ finally-done <> if \ [char] j internal::serial-emit depth.
          buf i2c-slave c@ 0= if \ [char] k internal::serial-emit depth.
            CMD { command } \ [char] l internal::serial-emit depth.
            buf i2c-restart c@ if \ [char] m internal::serial-emit depth.
              $00 buf i2c-stop-det c!
              command RESTART or to command
            then \ [char] n internal::serial-emit depth.
            buf i2c-data-size @ 1 = buf i2c-stop c@ 0<> and if \ [char] o internal::serial-emit depth.
              $FF buf i2c-prev-stop c!
              command STOP_BIT or to command
            then \ [char] p internal::serial-emit depth.
            command buf i2c-addr @ IC_DATA_CMD ! \ [char] q internal::serial-emit depth.
          then \ [char] r internal::serial-emit depth.
        then \ [char] s internal::serial-emit depth.
        enable-int
        buf i2c-done c@ finally-done <> if \ [char] t internal::serial-emit depth.
          buf wait-i2c-complete-or-timeout \ [char] u internal::serial-emit depth.
        then \ [char] v internal::serial-emit depth.
        \ $00 buf i2c-stop-det c!
        mode-not-active buf i2c-mode c!
        buf i2c-data-offset @ { offset }
        offset 0= if
          buf i2c-done c@ finally-done = if
            $00 buf i2c-stop-det c!
            master-not-active buf i2c-master-mode c!
          then \ [char] w internal::serial-emit depth.
        then \ [char] x internal::serial-emit depth.
        buf i2c-tx-target-noack c@ { noack }
        buf i2c-arb-lost c@ { arb-lost }
        buf i2c-rx-over c@ { rx-over }
        buf i2c-lock release-lock
        rx-over triggers x-i2c-rx-over
        arb-lost triggers x-arb-lost
        noack triggers x-i2c-target-noack
        offset
      else \ [char] y internal::serial-emit depth.
        buf i2c-lock release-lock
        0
      then \ [char] z internal::serial-emit depth.
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
    sysclk @ 2 pick 2 / + 2 pick / ( clock i2c period )
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
        sysclk @ 3 * 10000000 / 1 + ( lcnt hcnt i2c-buffer hold tx-hold )
        4 pick 2 - over > averts x-out-of-range-clock ( lcnt hcnt i2c-buffer hold tx-hold )
        or over i2c-addr @ IC_SDA_HOLD ! ( lcnt hcnt i2c-buffer )
      else ( lcnt hcnt i2c-buffer )
        dup i2c-addr @ IC_SDA_HOLD @ $FFFF bic ( lcnt hcnt i2c-buffer hold )
        sysclk @ 3 * 25000000 / 1 + ( lcnt hcnt i2c-buffer hold tx-hold )
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
    : wait-i2c-master-timeout { ticks index -- accepted }
      index validate-i2c
      systick-counter { start-tick }
      index i2c-select { buf }
      
      disable-int
      $00 buf i2c-stop-det c!
      mode-not-active buf i2c-mode c!
      [ RX_FULL RD_REQ or ] literal buf i2c-addr @ IC_INTR_MASK !
      buf i2c-addr @ IC_CLR_RX_DONE @ drop
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      buf i2c-addr @ IC_CLR_TX_ABRT @ drop
      enable-int
      
      begin
        systick-counter start-tick - ticks < averts x-timed-out
        index claim-i2c
        index validate-slave
        buf i2c-pending c@ send-pending = if
          not-pending buf i2c-pending c! accept-send true
        else
          buf i2c-pending c@ recv-pending = if
            not-pending buf i2c-pending c! accept-recv true
          else
            false
          then
        then
        index release-i2c
        dup not if
          pause wake-counter @ current-task block-wait
        then
      until
    ;
    
    \ Wait for master send on I2C peripheral with a timeout
    : wait-i2c-master-send-timeout { ticks index -- }
      index validate-i2c
      systick-counter { start-systick }
      index i2c-select { buf }
      
      disable-int
      $00 buf i2c-stop-det c!
      mode-not-active buf i2c-mode c!
      RX_FULL buf i2c-addr @ IC_INTR_MASK !
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      buf i2c-addr @ IC_CLR_TX_ABRT @ drop
      enable-int
      
      begin
        systick-counter start-systick - ticks < averts x-timed-out
        index claim-i2c
        index validate-slave
        buf i2c-pending c@ send-pending = if
          not-pending buf i2c-pending c! true
        else
          false
        then
        index release-i2c
        dup not if
          pause wake-counter @ current-task block-wait
        then
      until
    ;
  
    \ Wait for master receive on I2C peripheral with a timeout
    : wait-i2c-master-recv-timeout { ticks index -- }
      index validate-i2c
      systick-counter { start-systick }
      index i2c-select { buf }
      
      disable-int
      $00 buf i2c-stop-det c!
      mode-not-active buf i2c-mode c!
      RD_REQ buf i2c-addr @ IC_INTR_MASK !
      buf i2c-addr @ IC_CLR_RX_DONE @ drop
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      enable-int
      
      begin
        systick-counter start-systick - ticks < averts x-timed-out
        index claim-i2c
        index validate-slave
        buf i2c-pending c@ recv-pending = if
          not-pending buf i2c-pending c! true
        else
          false
        then
        index release-i2c
        dup not if
          pause wake-counter @ current-task block-wait
        then
      until
    ;
    
    \ Wait for master send or receive on I2C peripheral without a timeout
    : wait-i2c-master-indefinite { index -- accepted }
      index validate-i2c
      index i2c-select { buf }
      
      disable-int
      $00 buf i2c-stop-det c!
      mode-not-active buf i2c-mode c!
      [ RX_FULL RD_REQ or ] literal buf i2c-addr @ IC_INTR_MASK !
      buf i2c-addr @ IC_CLR_RX_DONE @ drop
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      buf i2c-addr @ IC_CLR_TX_ABRT @ drop
      enable-int
      
      begin
        index claim-i2c
        index validate-slave
        buf i2c-pending c@ send-pending = if
          not-pending buf i2c-pending c! accept-send true
        else
          buf i2c-pending c@ recv-pending = if
            not-pending buf i2c-pending c! accept-recv true
          else
            false
          then
        then
        index release-i2c
        dup not if
          pause wake-counter @ current-task block-wait
        then
      until
    ;
    
    \ Wait for master send on I2C peripheral without a timeout
    : wait-i2c-master-send-indefinite { index -- }
      index validate-i2c
      index i2c-select { buf }
      
      disable-int
      $00 buf i2c-stop-det c!
      mode-not-active buf i2c-mode c!
      RX_FULL buf i2c-addr @ IC_INTR_MASK !
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      buf i2c-addr @ IC_CLR_TX_ABRT @ drop
      enable-int
      
      begin
        index claim-i2c
        index validate-slave
        buf i2c-pending c@ send-pending = if
          not-pending buf i2c-pending c! true
        else
          false
        then
        index release-i2c
        dup not if
          pause wake-counter @ current-task block-wait
        then
      until
    ;
  
    \ Wait for master receive on I2C peripheral without a timeout
    : wait-i2c-master-recv-indefinite { index -- }
      index validate-i2c
      index i2c-select { buf }
      
      disable-int
      $00 buf i2c-stop-det c!
      mode-not-active buf i2c-mode c!
      RD_REQ buf i2c-addr @ IC_INTR_MASK !
      buf i2c-addr @ IC_CLR_RX_DONE @ drop
      buf i2c-addr @ IC_CLR_STOP_DET @ drop
      enable-int
      
      begin
        index claim-i2c
        index validate-slave
        buf i2c-pending c@ recv-pending = if
          not-pending buf i2c-pending c! true
        else
          false
        then
        index release-i2c
        dup not if
          pause wake-counter @ current-task block-wait
        then
      until
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
    TX_ABRT over i2c-addr @ IC_INTR_MASK !

    dup i2c-disabled @ 0> if
      IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bis!
      enable-int
      begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and until
    else
      enable-int
    then

    dup i2c-slave c@ 0= if
      IC_ENABLE_ABORT over i2c-addr @ IC_ENABLE bis!
      begin dup i2c-addr @ IC_ENABLE @ IC_ENABLE_ABORT and 0= until
    then

    IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bic!
    begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and 0= until
    
    begin dup i2c-addr @ IC_STATUS @ SLV_ACTIVITY and 0= until
    begin dup i2c-addr @ IC_STATUS @ MST_ACTIVITY and 0= until
    
    disable-int
    dup i2c-addr @ IC_CLR_STOP_DET @ drop
    dup i2c-addr @ IC_CLR_RX_DONE @ drop
    \ dup clear-abrt-sbyte-norstrt
    dup i2c-addr @ IC_CLR_TX_ABRT @ drop
    0 over i2c-data-offset !
    0 over i2c-data-size !
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
    dup clear-nack    
    dup restore-int-mask
    enable-int
    
    dup i2c-disabled @ 0<= if
      IC_ENABLE_ENABLE over i2c-addr @ IC_ENABLE bis!
      begin dup i2c-addr @ IC_ENABLE_STATUS @ IC_EN and until
    then

    drop release-i2c
  ;

end-module> import

\ Initialize I2C
: init ( -- )
  init
  0 i2c-internal::init-i2c
  1 i2c-internal::init-i2c
  i2c-internal::init-i2c-core-1
;

reboot
