\ Copyright (c) 2024 Travis Bemann
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

begin-module mpu

  begin-module mpu-internal

    \ MPU type register
    $E000ED90 constant MPU_TYPE

    \ MPU control register
    $E000ED94 constant MPU_CTRL

    \ MPU region number register
    $E000ED98 constant MPU_RNR

    \ MPU region base address register
    $E000ED9C constant MPU_RBAR

    \ MPU region attribute and size register
    $E000EDA0 constant MPU_RASR

    \ MPU type instruction region count (0 for unified) LSB
    16 constant MPU_TYPE_IREGION_LSB

    \ MPU type instruction region count (0 for unified) mask
    $FF MPU_TYPE_IREGION_LSB lshift constant MPU_TYPE_IREGION_MASK

    \ MPU type data region count (0 for unimplemented) LSB
    8 constant MPU_TYPE_DREGION_LSB

    \ MPU type data region count (0 for unimplemented) mask
    $FF MPU_TYPE_DREGION_LSB lshift constant MPU_TYPE_DREGION_MASK

    \ MPU type separate instruction and data regions bit (0 for unified)
    0 bit constant MPU_TYPE_SEPARATE

    \ MPU control use system map as default for privileged access when
    \ MPU_CTRL_ENABLE is set bit
    2 bit constant MPU_CTRL_PRIVDEFENA

    \ MPU control enable the MPU for HardFaults and NMIs bit
    1 bit constant MPU_CTRL_HFNIENA

    \ MPU control enable bit
    0 bit constant MPU_CTRL_ENABLE

    \ MPU region number region LSB
    0 constant MPU_RNR_REGION_LSB

    \ MPU region number region mask
    $FF MPU_RNR_REGION_LSB lshift constant MPU_RNR_REGION_MASK

    \ MPU region base address valid bit (update region number if set)
    4 bit constant MPU_RBAR_VALID

    \ MPU region base address region LSB
    0 constant MPU_RBAR_REGION_LSB

    \ MPU region base address region mask
    $F MPU_RBAR_REGION_LSB lshift constant MPU_RBAR_REGION_MASK

    \ MPU region Execute Never
    28 bit constant MPU_RASR_XN

    \ MPU region access and privilege LSB
    24 constant MPU_RASR_AP_LSB

    \ MPU region access and privilege mask
    $7 MPU_RASR_AP_LSB lshift constant MPU_RASR_AP_MASK

    \ MPU region TEX LSB
    19 constant MPU_RASR_TEX_LSB

    \ MPU region TEX
    $7 MPU_RASR_TEX_LSB lshift constant MPPU_RASR_TEX_MASK

    \ MPU region S
    18 bit constant MPU_RASR_S

    \ MPU region C
    17 bit constant MPU_RASR_C

    \ MPU region B
    16 bit constant MPU_RASR_B

    \ MPU region subregion disable LSB
    8 constant MPU_RASR_SRD_LSB

    \ MPU region subregion disable mask
    $FF MPU_RASR_SRD_LSB lshift constant MPU_RASR_SRD_MASK

    \ MPU region subregion size LSB
    1 constant MPU_RASR_SIZE_LSB

    \ MPU region subregion size mask
    $1F MPU_RASR_SIZE_LSB lshift constant MPU_RASR_SIZE_MASK

    \ MPU region enable
    0 bit constant MPU_RASR_ENABLE

    \ MPU region full access
    $3 MPU_RASR_AP_LSB lshift constant MPU_RASR_AP_FULL
    
    \ MPU region read-only access
    $7 MPU_RASR_AP_LSB lshift constant MPU_RASR_AP_READ_ONLY

    \ MPU enable count
    2 cells buffer: mpu-enable-count

    \ Initialize the MPU
    : init-mpu ( -- )
      MPU_CTRL_PRIVDEFENA MPU_CTRL bis!
      [ MPU_CTRL_HFNIENA MPU_CTRL_ENABLE or ] literal MPU_CTRL bic!
      cpu-count 0 ?do 0 i cells mpu-enable-count + ! loop
    ;

    initializer init-mpu
  
  end-module> import

  \ Out of range region
  : x-out-of-range-region ( -- ) ." out of range MPU region" cr ;

  \ Enable the MPU
  : enable-mpu ( -- )
    cpu-index cells mpu-enable-count +
    disable-int
    dup @ 1+ tuck swap !
    1 = if MPU_CTRL_ENABLE MPU_CTRL bis! then
    enable-int
  ;

  \ Disable the MPU
  : disable-mpu ( -- )
    cpu-index cells mpu-enable-count +
    disable-int
    dup @ 1- tuck swap !
    0= if MPU_CTRL_ENABLE MPU_CTRL bic! then
    enable-int
  ;

  \ Execute code with the MPU disabled
  : with-mpu-disabled ( xt -- ) disable-mpu try enable-mpu ?raise ;
  
end-module
