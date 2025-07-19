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

compile-to-flash

begin-module rng

  interrupt import
  task import
  slock import
  
  begin-module rng-internal

    \ TRNG base address
    $400F0000 constant TRNG_BASE

    \ Interrupt masking register
    TRNG_BASE $100 + constant RNG_IMR

    \ RNG status register
    TRNG_BASE $104 + constant RNG_ISR

    \ Interrupt/status bit clear register
    TRNG_BASE $108 + constant RNG_ICR

    \ RNG collected bits 0
    TRNG_BASE $114 + constant EHR_DATA0

    \ RNG collected bits 1
    TRNG_BASE $118 + constant EHR_DATA1

    \ RNG collected bits 2
    TRNG_BASE $11C + constant EHR_DATA2
    
    \ RNG collected bits 3
    TRNG_BASE $120 + constant EHR_DATA3
    
    \ RNG collected bits 4
    TRNG_BASE $124 + constant EHR_DATA4
    
    \ RNG collected bits 5
    TRNG_BASE $128 + constant EHR_DATA5

    \ Enable signal for the random source
    TRNG_BASE $12C + constant RND_SOURCE_ENABLE

    \ Generate internal SW reset within the RNG block
    TRNG_BASE $140 + constant TRNG_SW_RESET

    \ Von Neumann error bit
    3 bit constant VN_ERR

    \ RNG test failed error bit
    2 bit constant CRNGT_ERR

    \ Autocorrelation test error bit; when detected the RNG ceases functioning
    \ until next reset
    1 bit constant AUTOCORR_ERR

    \ Indicates 192 bits have been collected in the RNG, and are ready to be
    \ read
    0 bit constant EHR_VALID

    \ Select number of inverters LSB
    0 constant RND_SRC_SEL_LSB

    \ Entropy source is enabled
    0 bit constant RND_SRC_EN

    \ Writing 1 to this causes an internal RNG reset
    0 bit constant TRNG_SW_RESET_BIT

    \ Reflects rng_busy status
    0 bit constant TRNG_BUSY_BIT

    \ TRNG simple lock
    slock-size buffer: trng-slock
    
    \ Variable for pool buffer read-index
    variable pool-read-index

    \ Variable for pool buffer write-index
    variable pool-write-index

    \ Constant for number of bytes to buffer
    18 1+ constant pool-buffer-size

    \ Pool buffer
    pool-buffer-size cells buffer: pool-buffer

    \ Get whether the pool buffer is full
    : pool-full? ( -- f )
      pool-write-index @ pool-read-index @
      pool-buffer-size 1- + $FF and =
    ;

    \ Get whether the pool buffer is empty
    : pool-empty? ( -- f )
      pool-read-index @ pool-write-index @ =
    ;

    \ Write a byte to the pool buffer
    : write-pool ( x -- )
      pool-full? not if
        pool-write-index @ cells pool-buffer + !
        pool-write-index @ 1+ pool-buffer-size umod pool-write-index !
      else
        drop
      then
    ;

    \ Read a byte from the pool buffer
    : read-pool ( -- x )
      pool-read-index @ cells pool-buffer + @
      pool-read-index @ 1+ pool-buffer-size umod pool-read-index !
    ;

    \ TRNG IRQ
    39 constant TRNG_IRQ

    \ TRNG vector
    TRNG_IRQ 16 + constant trng-vector

    \ Start the TRNG
    : start-trng ( -- ) RND_SRC_EN RND_SOURCE_ENABLE bis! ;

    \ Stop the TRNG
    : stop-trng ( -- ) RND_SRC_EN RND_SOURCE_ENABLE bic! ;

    \ Reset the TRNG
    : reset-trng ( -- )
      TRNG_SW_RESET_BIT TRNG_SW_RESET !
      TRNG_SW_RESET @ drop
      TRNG_SW_RESET @ drop
      [ VN_ERR CRNGT_ERR or AUTOCORR_ERR or EHR_VALID or ] literal RNG_IMR bic!
    ;

    \ Collect TRNG data
    : collect-trng ( -- )
      EHR_DATA0 @ write-pool
      EHR_DATA1 @ write-pool
      EHR_DATA2 @ write-pool    
      EHR_DATA3 @ write-pool    
      EHR_DATA4 @ write-pool    
      EHR_DATA5 @ write-pool
    ;

    \ Attempt to continue TRNG data generation unless the pool is full
    : continue-trng ( -- )
      pool-full? not if start-trng else stop-trng then
    ;
    
    \ Handle a TRNG interrupt
    : handle-trng ( -- )
      RNG_ISR @ { status }
      status VN_ERR and if
        VN_ERR RNG_ICR !
        continue-trng
      then
      status CRNGT_ERR and if
        CRNGT_ERR RNG_ICR !
        continue-trng
      then
      status AUTOCORR_ERR and if
        AUTOCORR_ERR RNG_ICR !
        reset-trng
        continue-trng
      then
      status EHR_VALID and if
        EHR_VALID RNG_ICR !
        collect-trng
        continue-trng
      then
    ;

    \ Initialize the TRNG
    : init-trng ( -- )
      disable-int
      0 pool-read-index !
      0 pool-write-index !
      trng-slock init-slock
      ['] handle-trng trng-vector vector!
      TRNG_IRQ NVIC_ISER_SETENA!
      enable-int
      reset-trng
      start-trng
    ;
    initializer init-trng
    
  end-module> import

  \ Get a random number
  : random ( -- x )
    begin
      [:
        pool-empty? not if read-pool true else false then
        start-trng
      ;] trng-slock with-slock
      dup not if pause-reschedule-last then
    until
  ;
  
end-module

reboot
