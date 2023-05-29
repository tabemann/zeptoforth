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

begin-module cyw43-consts

  \ MTU size
  1500 constant mtu-size

  0 constant FUNC_BUS
  1 constant FUNC_BACKPLANE
  2 constant FUNC_WLAN
  3 constant FUNC_BT

  $00 constant REG_BUS_CTRL
  $04 constant REG_BUS_INTERRUPT
  $06 constant REG_BUS_INTERRUPT_ENABLE
  $08 constant REG_BUS_STATUS
  $14 constant REG_BUS_TEST_RO
  $18 constant REG_BUS_TEST_RW
  $1C constant REG_BUS_RESP_DELAY

  $01 constant WORD_LENGTH_32
  $10 constant HIGH_SPEED
  1 5 lshift constant INTERRUPT_HIGH
  1 7 lshift constant WAKE_UP
  1 16 lshift constant STATUS_ENABLE
  1 17 lshift constant INTERRUPT_WITH_STATUS

  \ SPI_STATUS_REGISTER bits
  $00000001 constant STATUS_DATA_NOT_AVAILABLE
  $00000002 constant STATUS_UNDERFLOW
  $00000004 constant STATUS_OVERFLOW
  $00000008 constant STATUS_F2_INTR
  $00000010 constant STATUS_F3_INTR
  $00000020 constant STATUS_F2_RX_READY
  $00000040 constant STATUS_F3_RX_READY
  $00000080 constant STATUS_HOST_CMD_DATA_ERR
  $00000100 constant STATUS_F2_PKT_AVAILABLE
  $000FFE00 constant STATUS_F2_PKT_LEN_MASK
  9 constant STATUS_F2_PKT_LEN_SHIFT
  $00100000 constant STATUS_F3_PKT_AVAILABLE
  $FFE00000 constant STATUS_F3_PKT_LEN_MASK
  21 constant STATUS_F3_PKT_LEN_SHIFT

  $10005 constant REG_BACKPLANE_GPIO_SELECT
  $10006 constant REG_BACKPLANE_GPIO_OUTPUT
  $10007 constant REG_BACKPLANE_GPIO_ENABLE
  $10008 constant REG_BACKPLANE_FUNCTION2_WATERMARK
  $10009 constant REG_BACKPLANE_DEVICE_CONTROL
  $1000A constant REG_BACKPLANE_BACKPLANE_ADDRESS_LOW
  $1000B constant REG_BACKPLANE_BACKPLANE_ADDRESS_MID
  $1000C constant REG_BACKPLANE_BACKPLANE_ADDRESS_HIGH
  $1000D constant REG_BACKPLANE_FRAME_CONTROL
  $1000E constant REG_BACKPLANE_CHIP_CLOCK_CSR
  $1000F constant REG_BACKPLANE_PULL_UP
  $1001B constant REG_BACKPLANE_READ_FRAME_BC_LOW
  $1001C constant REG_BACKPLANE_READ_FRAME_BC_HIGH
  $1001E constant REG_BACKPLANE_WAKEUP_CTRL
  $1001F constant REG_BACKPLANE_SLEEP_CSR

  $8000 constant BACKPLANE_WINDOW_SIZE
  $7FFF constant BACKPLANE_ADDRESS_MASK
  $08000 constant BACKPLANE_ADDRESS_32BIT_FLAG
  64 constant BACKPLANE_MAX_TRANSFER_SIZE
  \ Active Low Power (ALP) clock constants
  $08 constant BACKPLANE_ALP_AVAIL_REQ
  $40 constant BACKPLANE_ALP_AVAIL

  \ Broadcom AMBA (Advanced Microcontroller Bus Architecture) Interconnect
  \ (AI) pub (crate) constants
  $408 constant AI_IOCTRL_OFFSET
  $0002 constant AI_IOCTRL_BIT_FGC
  $0001 constant AI_IOCTRL_BIT_CLOCK_EN
  $0020 constant AI_IOCTRL_BIT_CPUHALT

  $800 constant AI_RESETCTRL_OFFSET
  1 constant AI_RESETCTRL_BIT_RESET

  $804 constant AI_RESETSTATUS_OFFSET

  $12345678 constant TEST_PATTERN
  $FEEDBEAD constant FEEDBEAD

  \ SPI_INTERRUPT_REGISTER and SPI_INTERRUPT_ENABLE_REGISTER Bits
  $0001 constant IRQ_DATA_UNAVAILABLE
  $0002 constant IRQ_F2_F3_FIFO_RD_UNDERFLOW
  $0004 constant IRQ_F2_F3_FIFO_WR_OVERFLOW
  $0008 constant IRQ_COMMAND_ERROR
  $0010 constant IRQ_DATA_ERROR
  $0020 constant IRQ_F2_PACKET_AVAILABLE
  $0040 constant IRQ_F3_PACKET_AVAILABLE
  $0080 constant IRQ_F1_OVERFLOW
  $0100 constant IRQ_MISC_INTR0
  $0200 constant IRQ_MISC_INTR1
  $0400 constant IRQ_MISC_INTR2
  $0800 constant IRQ_MISC_INTR3
  $1000 constant IRQ_MISC_INTR4
  $2000 constant IRQ_F1_INTR
  $4000 constant IRQ_F2_INTR
  $8000 constant IRQ_F3_INTR

  2 constant IOCTL_CMD_UP
  3 constant IOCTL_CMD_DOWN
  26 constant IOCTL_CMD_SET_SSID
  30 constant IOCTL_CMD_SET_CHANNEL
  64 constant IOCTL_CMD_ANTDIV
  118 constant IOCTL_CMD_SET_AP
  263 constant IOCTL_CMD_SET_VAR
  262 constant IOCTL_CMD_GET_VAR
  268 constant IOCTL_CMD_SET_PASSPHRASE

  0 constant CHANNEL_TYPE_CONTROL
  1 constant CHANNEL_TYPE_EVENT
  2 constant CHANNEL_TYPE_DATA

  \ CYW_SPID command structure constants.
  true constant CYW43_WRITE
  false constant CYW43_READ
  true constant INC_ADDR
  false constant FIXED_ADDR

  $0004 constant AES_ENABLED
  $00400000 constant WPA2_SECURITY

  8 constant MIN_PSK_LEN
  64 constant MAX_PSK_LEN

  \ Security type (authentication and encryption types are combined using bit
  \ mask)
  0 constant CYW43_OPEN
  WPA2_SECURITY AES_ENABLED or constant WPA_AES_PSK

  \ operation was successful
  0 constant ESTATUS_SUCCESS
  \ operation failed
  1 constant ESTATUS_FAIL
  \ operation timed out
  2 constant ESTATUS_TIMEOUT
  \ failed due to no matching network found
  3 constant ESTATUS_NO_NETWORKS
  \ operation was aborted
  4 constant ESTATUS_ABORT
  \ protocol failure: packet not ack'd
  5 constant ESTATUS_NO_ACK
  \ AUTH or ASSOC packet was unsolicited
  6 constant ESTATUS_UNSOLICITED
  \ attempt to assoc to an auto auth configuration
  7 constant ESTATUS_ATTEMPT
  \ scan results are incomplete
  8 constant ESTATUS_PARTIAL
  \ scan aborted by another scan
  9 constant ESTATUS_NEWSCAN
  \ scan aborted due to assoc in progress
  10 constant ESTATUS_NEWASSOC
  \ 802.11h quiet period started
  11 constant ESTATUS_11HQUIET
  \ user disabled scanning (WLC_SET_SCANSUPPRESS)
  12 constant ESTATUS_SUPPRESS
  \ no allowable channels to scan
  13 constant ESTATUS_NOCHANS
  \ scan aborted due to CCX fast roam
  14 constant ESTATUS_CCXFASTRM
  \ abort channel select
  15 constant ESTATUS_CS_ABORT

  \ CYW43 cores
  0 constant WLAN
  1 constant SOCSRAM
  2 constant SDIOD

  \ Wrapper regiser offset
  $100000 constant CYW43_WRAPPER_REGISTER_OFFSET

  \ SOCSRAM base address
  $18004000 constant CYW43_SOCSRAM_BASE_ADDRESS

  \ SDIOD base address
  $18002000 constant CYW43_SDIOD_BASE_ADDRESS
  
  \ PMU base address
  $18000000 constant CYW43_PMU_BASE_ADDRESS 

  \ Chip RAM size
  512 1024 * constant CYW43_CHIP_RAM_SIZE

  \ ATCM RAM base address
  0 constant CYW43_ATCM_RAM_BASE_ADDRESS

  \ SOCRAM SRMEM size
  64 1024 * constant CYW43_SOCRAM_SRMEM_SIZE

  \ Chanspec band mask
  $C000 constant CYW43_CHANSPEC_BAND_MASK

  \ Chanspec band 2G
  $0000 constant CYW43_CHANSPEC_BAND_2G

  \ Chanspec band 5G
  $C000 constant CYW43_CHANSPEC_BAND_5G

  \ Chanspec band shift
  14 constant CYW43_CHANSPEC_BAND_SHIFT

  \ Chanspec BW 10
  $0800 constant CYW43_CHANSPEC_BW_10

  \ Chanspec BW 20
  $1000 constant CYW43_CHANSPEC_BW_20

  \ Chanspec BW 40
  $1800 constant CYW43_CHANSPEC_BW_40

  \ Chanspec BW mask
  $3800 constant CYW43_CHANSPEC_BW_MASK

  \ Chanspec BW shift
  11 constant CYW43_CHANSPEC_BW_SHIFT

  \ Chanspec CTL SB lower
  $0000 constant CYW43_CHANSPEC_CTL_SB_LOWER

  \ Chanspec CTL SB upper
  $0100 constant CYW43_CHANSPEC_CTL_SB_UPPER

  \ Chanspec CTL SB none
  $0000 constant CYW43_CHANSPEC_CTL_SB_NONE

  \ Chanspec CTL SB mask
  $0700 constant CYW43_CHANSPEC_CTL_SB_MASK

  \ Power management modes

  \ Custom. officially unsupported mode. Use at your own risk.
  \ All power-saving features set to their max at only a marginal decrease in
  \ power consumption as opposed to cyw43-pm-aggressive.
  0 constant PM_SUPER_SAVE

  \ Aggressive power saving mode.
  1 constant PM_AGGRESSIVE

  \ The default mode
  2 constant PM_POWER_SAVE.

  \ Performance is preferred over power consumption but still some power is
  \ conserved as opposed to cyw43-pm-none.
  3 constant PM_PERFORMANCE

  \ Unlike all other PM modes, this lowers the power consumption at all times at
  \ the cost of a much lower throughput.
  4 constant PM_THROUGHPUT_THROTTLING

  \ No power management is configured. This conserves the most power.
  5 constant PM_NONE

  \ The default power management mode.
  PM_POWER_SAVE constant PM_DEFAULT
  
  \ CYW43 base addresses
  : CYW43_BASE_ADDR ( core -- addr )
    case
      WLAN of
        [ $18003000 CYW43_WRAPPER_REGISTER_OFFSET + ] literal
      endof
      SOCSRAM of
        [ CYW43_SOCSRAM_BASE_ADDRESS CYW43_WRAPPER_REGISTER_OFFSET + ] literal
      endof
      SDIOD of CYW43_SDIOD_BASE_ADDRESS endof
    endcase
  ;

  \ Sleep ret ms
  : cyw43-pm-sleep-rest-ms ( mode -- ms )
    case
      PM_SUPER_SAVE of 2000 endof
      PM_AGGRESSIVE of 2000 endof
      PM_POWER_SAVE of 200 endof
      PM_PERFORMANCE of 20 endof
      PM_THROUGHPUT_THROTTLING of 0 endof \ value doesn't matter
      PM_NONE of 0 endof \ value doesn't matter
    endcase
  ;

  \ Beacon period
  : cyw43-pm-beacon-period ( mode -- period )
    case
      PM_SUPER_SAVE of 255 endof
      PM_AGGRESSIVE of 1 endof
      PM_POWER_SAVE of 1 endof
      PM_PERFORMANCE of 1 endof
      PM_THROUGHPUT_THROTTLING of 0 endof \ value doesn't matter
      PM_NONE of 0 endof \ value doesn't matter
    endof
  ;

  \ DTIM period
  : cyw43-pm-dtim-period ( mode -- period )
    case
      PM_SUPER_SAVE of 255 endof
      cyw43-pm-aggresive of 1 endof
      PM_POWER_SAVE of 1 endof
      PM_PERFORMANCE of 1 endof
      PM_THROUGHPUT_THROTTLING of 0 endof \ value doesn't matter
      PM_NONE of 0 endof \ value doesn't matter
    endof
  ;

  \ Assoc
  : cyw43-pm-assoc ( mode -- assoc )
    case
      PM_SUPER_SAVE of 255 endof
      PM_AGGRESSIVE of 10 endof
      PM_POWER_SAVE of 10 endof
      PM_PERFORMANCE of 1 endof
      PM_THROUGHPUT_THROTTLING of 0 endof \ value doesn't matter
      PM_NONE of 0 endof \ value doesn't matter
    endof
  ;

  \ Mode
  : cyw43-pm-mode ( mode -- mode' )
    case
      PM_THROUGHPUT_THROTTLING of 1 endof
      PM_NONE of 0 endof
      2 swap
    endcase
  ;

end-module