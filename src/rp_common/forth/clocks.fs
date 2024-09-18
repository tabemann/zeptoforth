\ Copyright (c) 2024 Paul Koning
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
\
\ RP2040 system clock controls
\
\ This module allows changing the RP2040 clock settings.  At the
\ moment it only implements a word that sets the SYSCLK PLL
\ parameters.

compile-to-flash

begin-module clocks

  multicore import
  
  \ Exceptions
  : x-bad-refdiv ( -- ) ." reference divider value out of range" cr ;
  : x-bad-fbdiv ( -- ) ." VCO feedback divider value out of range" cr ;
  : x-bad-postdiv1 ( -- ) ." post divider 1 value out of range" cr ;
  : x-bad-postdiv2 ( -- ) ." post divider 2 value out of range" cr ;
  : x-bad-refclk ( -- ) ." VCO refclock frequency out of range", cr ;
  : x-bad-vcofreq ( -- ) ." VCO frequency out of range", cr ;
  : x-bad-sysclk ( -- ) ." sysclk frequency out of range", cr ;
  
  begin-module clocks-internal
    
    \ Atomic operation address modifiers, see RP2040 datasheet section 2.1.2
    $1000 constant MOD_XOR        \ toggle bits
    $2000 constant MOD_SET        \ set bits
    $3000 constant MOD_CLR        \ clear bits

    rp2040? [if]
      $40008000 constant CLOCKS_BASE
    [then]
    rp2350? [if]
      $40010000 constant CLOCKS_BASE
    [then]
    CLOCKS_BASE $3c + constant CLK_SYS_CTRL
    CLOCKS_BASE $40 + constant CLK_SYS_DIV
    CLOCKS_BASE $44 + constant CLK_SYS_SELECTED
    CLOCKS_BASE $6c + constant CLK_RTC_CTRL
    CLOCKS_BASE $70 + constant CLK_RTC_DIV
    CLOCKS_BASE $74 + constant CLK_RTC_SELECTED

    \ CLK_SYS_CTRL values
    0 constant SYS_CLKSRC_PLL_SYS
    4 5 lshift constant SYS_CLKSRC_GPIN0
    5 5 lshift constant SYS_CLKSRC_GPIN1
    0 constant SYS_CLK_REF
    1 constant SYS_CLKSRC_CLK_SYS_AUX

    \ CLK_RTC_CTRL values
    1 11 lshift constant RTC_ENABLE
    1 5 lshift constant RTC_CLKSRC_PLL_SYS
    4 5 lshift constant RTC_CLKSRC_GPIN0
    5 5 lshift constant RTC_CLKSRC_GPIN1

    rp2040? [if]
      $40028000 constant PLL_SYS_BASE
    [then]
    rp2350? [if]
      $40050000 constant PLL_SYS_BASE
    [then]
    PLL_SYS_BASE $00 + constant PLL_SYS_CS
    PLL_SYS_BASE $04 + constant PLL_SYS_PWR
    PLL_SYS_BASE $08 + constant PLL_SYS_FBDIV_INT
    PLL_SYS_BASE $0c + constant PLL_SYS_PRIM

    $2d constant PLL_PWR_OFF
    $0c constant PLL_PWR_POSTDIV_OFF
    $00 constant PLL_PWR_ON

    rp2040? [if]
      133000000 constant MAX_SYSCLK
    [then]
    rp2350? [if]
      150000000 constant MAX_SYSCLK
    [then]
    
    \ similar to compat::within but checks a closed range
    : bounds ( test low high -- flag ) over - >r - r> u<= ;

    \ Switch the main (glitchless) sysclk mux.  Argument is the clock source
    \ wanted, SYS_CLK_REF or SYS_CLKSRC_CLK_SYS_AUX.
    : sysclk-mainsrc ( src -- )
      dup if
        SYS_CLKSRC_CLK_SYS_AUX [ CLK_SYS_CTRL MOD_SET + ] literal !
      else
        SYS_CLKSRC_CLK_SYS_AUX [ CLK_SYS_CTRL MOD_CLR + ] literal !
      then
      1 swap lshift
      begin
        dup CLK_SYS_SELECTED @ and       \ Check for mux switch complete
      until drop ;

    \ Set the sysclk PLL parameters.  This must be done with the sysclk mux set
    \ to "refclk" because the PLL is turned off and unlocked during the process.
    \ On exit, the PLL is on and stable.
    \ The logic here is adapted from hardware.s in the Zeptoforth sources.
    : sysclk-pll ( refdiv fbdiv pdiv1 pdiv2 -- )
      PLL_PWR_OFF PLL_SYS_PWR !            \ Power off the whole PLL
      12 lshift swap 16 lshift or PLL_SYS_PRIM !
      PLL_SYS_FBDIV_INT !
      PLL_SYS_CS !
      PLL_PWR_POSTDIV_OFF PLL_SYS_PWR !    \ Power on VCO, leave post dividers off
      begin
        PLL_SYS_CS @ 0<                  \ Check for PLL lock
      until
      PLL_PWR_ON PLL_SYS_PWR !             \ Power on everything
    ;

    \ Check PLL parameters and return resulting PLL output frequency
    : check-pll { refdiv fbdiv pdiv1 pdiv2 -- freq }
      pdiv2 1 7 bounds averts x-bad-postdiv2
      pdiv1 1 7 bounds averts x-bad-postdiv1
      fbdiv 16 320 bounds averts x-bad-fbdiv
      refdiv 1 63 bounds averts x-bad-refdiv
      \ REFCLK / refdiv is the VCO reference frequency
      xosc-frequency refdiv / dup 5000000 < triggers x-bad-refclk
      \ VCO reference frequency * fbdiv is the VCO output frequency
      fbdiv * dup 750000000 1600000000 bounds averts x-bad-vcofreq
      \ VCO output frequency / (div1 * div2) is the new SYSCLK
      pdiv1 / pdiv2 / dup MAX_SYSCLK <= averts x-bad-sysclk
    ;

  end-module> import
    
\ Set sysclk parameters.
: set-sysclk ( refdiv fbdiv pdiv1 pdiv2 -- )
  2over 2over check-pll
  \ Now do the clock/pll dance.  This is a critical section.
  [:
    [: { refdiv fbdiv pdiv1 pdiv2 freq }
      disable-int
      freq sysclk !                   \ save resulting new sysclk frequency
      SYS_CLK_REF sysclk-mainsrc \ Switch sysclk to refclk (12 MHz)
      $e0 [ CLK_SYS_CTRL MOD_CLR + ] literal !  \ Set aux src to sys_pll (zero)
      refdiv fbdiv pdiv1 pdiv2 sysclk-pll                 \ Set the PLL
      SYS_CLKSRC_CLK_SYS_AUX sysclk-mainsrc \ Switch sysclk to PLL
      enable-int
    ;] with-hold-core
  ;] critical
;

end-module

reboot
