compile-to-flash

begin-import-module-once swd-module

  import internal-module

  begin-import-module swd-internal-module

    here 256 2* cell+ buffer: swd
    swd 0 + constant swd-rx-w
    swd 1 + constant swd-rx-r
    swd 2 + constant swd-tx-w
    swd 3 + constant swd-tx-r
    swd cell+ dup constant swd-rx
    256 + constant swd-tx

    : c-inc ( c-addr -- ) 1 swap c+! [inlined] ;
    : inc-rx-w ( -- ) swd-rx-w c-inc ;
    : inc-rx-r ( -- ) swd-rx-r c-inc ;
    : inc-tx-w ( -- ) swd-tx-w c-inc ;
    : inc-tx-r ( -- ) swd-tx-r c-inc ;

    variable use-sleep

    : pause-until ( xt -- )
      use-sleep @ if
	wait
      else
	begin dup execute not while pause repeat
	drop
      then
    ;

    : swd-key? ( -- flag )
      disable-int swd h@ dup 8 rshift swap $ff and <> enable-int
    ;
    : swd-key ( -- char )
      [: swd-key? ;] pause-until
      disable-int swd-rx swd-rx-r c@ + c@ inc-rx-r enable-int
    ;

    : swd-emit? ( -- flag )
      disable-int swd-tx-w h@ dup 8 rshift swap $ff and 1+ $ff and <> enable-int
    ;
    : swd-emit ( char -- )
      [: swd-emit? ;] pause-until
      disable-int swd-tx swd-tx-w c@ + c! inc-tx-w enable-int
    ;

    : swd-flush-console ( -- )
      [: disable-int swd-tx-w h@ dup 8 rshift swap $FF and = enable-int ;] wait
    ;

    : >r11 ( x -- ) [ $46b3 h, ] drop ; \ $46b3 = mov r11, r6

  end-module

  : swd-init ( -- )
    false use-sleep ! 0 swd ! swd >r11
    ." The swd buffer address is: " swd h.8 cr
  ;

  : swd-console ( -- )
    ['] swd-key? key?-hook !
    ['] swd-key key-hook !
    ['] swd-emit? emit?-hook !
    ['] swd-emit emit-hook !
    ['] swd-flush-console flush-console-hook !
  ;

  : enable-sleep ( -- ) true use-sleep ! ;
  : disable-sleep ( -- ) false use-sleep ! ;

end-module

: init ( -- )
  init
  swd-init
  swd-console
;

unimport swd-module

compile-to-ram

\ warm

