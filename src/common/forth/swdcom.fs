compile-to-flash

forth-wordlist 1 set-order
forth-wordlist set-current
wordlist constant swd-wordlist
wordlist constant swd-internal-wordlist

forth-wordlist internal-wordlist int-io-internal-wordlist
swd-internal-wordlist swd-wordlist 5 set-order
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

: swd-key? ( -- flag )
  disable-int swd h@ dup 8 rshift swap $ff and <> enable-int
;
: swd-key ( -- char )
  [: swd-key? ;] pause-until
  disable-int swd-rx swd-rx-r b@ + b@ inc-rx-r enable-int
;

: swd-emit? ( -- flag )
  disable-int swd-tx-w h@ dup 8 rshift swap $ff and 1+ $ff and <> enable-int
;
: swd-emit ( char -- )
  [: swd-emit? ;] pause-until
  disable-int swd-tx swd-tx-w b@ + b! inc-tx-w enable-int
;

: swd-flush-console ( -- )
  [: disable-int swd-tx-w h@ dup 8 rshift swap $FF and = enable-int ;] wait
;

: >r11 ( x -- ) [ $46b3 h, ] drop ; \ $46b3 = mov r11, r6
: swd-init ( -- ) false use-sleep ! 0 swd ! swd >r11  ;

swd-wordlist set-current

: swd-console ( -- )
  ['] swd-key? key?-hook !
  ['] swd-key key-hook !
  ['] swd-emit? emit?-hook !
  ['] swd-emit emit-hook !
  ['] swd-flush-console flush-console-hook !
;

: serial-console ( -- )
  ['] do-key? key?-hook !
  ['] do-key key-hook !
  ['] do-emit? emit?-hook !
  ['] do-emit emit-hook !
  ['] do-flush-console flush-console-hook !
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

