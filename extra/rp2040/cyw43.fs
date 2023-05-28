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

begin-module cyw43

  oo import
  pin import
  
  \ CYW43 cores
  0 constant cyw43-wlan
  1 constant cyw43-socsram
  2 constant cyw43-sdiod

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
  0 constant cyw43-pm-super-save

  \ Aggressive power saving mode.
  1 constant cyw43-pm-aggressive

  \ The default mode
  2 constant cyw43-pm-power-save.

  \ Performance is preferred over power consumption but still some power is
  \ conserved as opposed to cyw43-pm-none.
  3 constant cyw43-pm-performance

  \ Unlike all other PM modes, this lowers the power consumption at all times at
  \ the cost of a much lower throughput.
  4 constant cyw43-pm-throughput-throttling

  \ No power management is configured. This conserves the most power.
  5 constant cyw43-pm-none

  \ The default power management mode.
  cyw43-pm-power-save constant cyw43-pm-default
  
  \ CYW43 base addresses
  : CYW43_BASE_ADDR ( core -- addr )
    case
      cyw43-wlan of
        [ $18003000 CYW43_WRAPPER_REGISTER_OFFSET + ] literal
      endof
      cyw43-socsram of
        [ CYW43_SOCSRAM_BASE_ADDRESS CYW43_WRAPPER_REGISTER_OFFSET + ] literal
      endof
      cyw43-sdiod of CYW43_SDIOD_BASE_ADDRESS endof
    endcase
  ;

  \ Sleep ret ms
  : cyw43-pm-sleep-rest-ms ( mode -- ms )
    case
      cyw43-pm-super-save of 2000 endof
      cyw43-pm-aggressive of 2000 endof
      cyw43-pm-power-save of 200 endof
      cyw43-pm-performance of 20 endof
      cyw43-pm-throughput-throttling of 0 endof \ value doesn't matter
      cyw43-pm-none of 0 endof \ value doesn't matter
    endcase
  ;

  \ Beacon period
  : cyw43-pm-beacon-period ( mode -- period )
    case
      cyw43-pm-super-save of 255 endof
      cyw43-pm-aggressive of 1 endof
      cyw43-pm-power-save of 1 endof
      cyw43-pm-performance of 1 endof
      cyw43-pm-throughput-throttling of 0 endof \ value doesn't matter
      cyw43-pm-none of 0 endof \ value doesn't matter
    endof
  ;

  \ DTIM period
  : cyw43-pm-dtim-period ( mode -- period )
    case
      cyw43-pm-super-save of 255 endof
      cyw43-pm-aggresive of 1 endof
      cyw43-pm-power-save of 1 endof
      cyw43-pm-performance of 1 endof
      cyw43-pm-throughput-throttling of 0 endof \ value doesn't matter
      cyw43-pm-none of 0 endof \ value doesn't matter
    endof
  ;

  \ Assoc
  : cyw43-pm-assoc ( mode -- assoc )
    case
      cyw43-pm-super-save of 255 endof
      cyw43-pm-aggressive of 10 endof
      cyw43-pm-power-save of 10 endof
      cyw43-pm-performance of 1 endof
      cyw43-pm-throughput-throttling of 0 endof \ value doesn't matter
      cyw43-pm-none of 0 endof \ value doesn't matter
    endof
  ;

  \ Mode
  : cyw43-pm-mode ( mode -- mode' )
    case
      cyw43-pm-throughput-throttling of 1 endof
      cyw43-pm-none of 0 endof
      2 swap
    endcase
  ;

  \ CYW43 device class
  <object> begin-class <cyw43>

    \ CYW43 power pin
    cell member cyw43-pwr-pin

    \ CYW43 SPI device
    cell member cyw43-spi-device
    
  end-class

  \ Implement the CYW43 device class
  <cyw43> begin-implement
    
    \ Initialize CYW43 device
    :noname { pwr-pin sck-pin tx-pin rx-pin cs-pin spi-device self -- }
      
    ; define new

  end-implement
  
end-module