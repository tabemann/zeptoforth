compile-to-flash

forth-wordlist 1 set-order
forth-wordlist set-current
wordlist constant swd-wordlist
wordlist constant swd-internal-wordlist

forth-wordlist int-io-internal-wordlist swd-internal-wordlist swd-wordlist
4 set-order
swd-internal-wordlist set-current

here 256 2* cell+ buffer: swd
swd 0 + constant swd-rx-w
swd 1 + constant swd-rx-r
swd 2 + constant swd-tx-w
swd 3 + constant swd-tx-r
swd cell+ dup constant swd-rx
256 + constant swd-tx

: b-inc ( c-addr -- ) 1 swap b+! [inlined] ;
: inc-rx-w ( -- ) swd-rx-w b-inc ;
: inc-rx-r ( -- ) swd-rx-r b-inc ;
: inc-tx-w ( -- ) swd-tx-w b-inc ;
: inc-tx-r ( -- ) swd-tx-r b-inc ;

variable use-sleep

: pause-until ( xt -- )
  use-sleep @ if wait else begin dup execute not while pause repeat drop then
;

: swd-key? ( -- flag ) swd h@ dup 8 rshift swap $ff and <> ;
: swd-key ( -- char ) [: swd-key? ;] pause-until swd-rx swd-rx-r b@ + b@ inc-rx-r ;

: swd-emit? ( -- flag ) swd-tx-w h@ dup 8 rshift swap $ff and 1+ $ff and <> ;
: swd-emit ( char -- ) [: swd-emit? ;] pause-until swd-tx swd-tx-w b@ + b! inc-tx-w ;

: >r11 ( x -- ) [ $46b3 h, ] drop ; \ $46b3 = mov r11, r6
: swd-init ( -- ) true use-sleep ! 0 swd ! swd >r11  ;

swd-wordlist set-current

: swd-console ( -- )
  ['] swd-key? key?-hook !
  ['] swd-key key-hook !
  ['] swd-emit? emit?-hook !
  ['] swd-emit emit-hook !
;

: serial-console ( -- )
  ['] do-key? key?-hook !
  ['] do-key key-hook !
  ['] do-emit? emit?-hook !
  ['] do-emit emit-hook !
;

: enable-sleep ( -- ) true use-sleep ! ;
: disable-sleep ( -- ) false use-sleep ! ;

forth-wordlist set-current

: init ( -- )
  init
  swd-init
  ." The swd buffer address is: " swd h.8 cr
  swd-console
;

compile-to-ram

\ warm

