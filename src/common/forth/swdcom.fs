compile-to-flash

begin-module swd

  internal import
  multicore import
  
  begin-module swd-internal

    here 256 2* cell+ buffer: swd
    swd 0 + constant swd-rx-w
    swd 1 + constant swd-rx-r
    swd 2 + constant swd-tx-w
    swd 3 + constant swd-tx-r
    swd cell+ dup constant swd-rx
    256 + constant swd-tx

    commit-flash

    : c-inc ( c-addr -- ) 1 swap c+! [inlined] ;
    : inc-rx-w ( -- ) swd-rx-w c-inc ;
    : inc-rx-r ( -- ) swd-rx-r c-inc ;
    : inc-tx-w ( -- ) swd-tx-w c-inc ;
    : inc-tx-r ( -- ) swd-tx-r c-inc ;

    variable use-sleep

    compress-flash
    
    : pause-until ( xt -- )
      use-sleep @ if
	wait
      else
	begin dup execute not while task::pause-reschedule-last repeat
	drop
      then
    ;

    commit-flash

    : swd-key? ( -- flag )
      serial-spinlock claim-spinlock
      disable-int swd h@ dup 8 rshift swap $ff and <> enable-int
      serial-spinlock release-spinlock
    ;
    
    : swd-key ( -- char )
      serial-spinlock claim-spinlock
      [: swd-key? ;] pause-until
      disable-int swd-rx swd-rx-r c@ + c@ inc-rx-r enable-int
      serial-spinlock release-spinlock
    ;

    : swd-emit? ( -- flag )
      serial-spinlock claim-spinlock
      disable-int swd-tx-w h@ dup 8 rshift swap $ff and 1+ $ff and <> enable-int
      serial-spinlock release-spinlock
    ;
    
    : swd-emit ( char -- )
      serial-spinlock claim-spinlock
      [: swd-emit? ;] pause-until
      disable-int swd-tx swd-tx-w c@ + c! inc-tx-w enable-int
      serial-spinlock release-spinlock
    ;

    : swd-flush-console ( -- )
      serial-spinlock claim-spinlock
      [: disable-int swd-tx-w h@ dup 8 rshift swap $FF and = enable-int ;] wait
      serial-spinlock release-spinlock
    ;

    : >r11 ( x -- ) [ $46b3 h, ] drop ; \ $46b3 = mov r11, r6

  end-module> import

  commit-flash

  : swd-init ( -- )
    false use-sleep ! 0 swd ! swd >r11
    ." The swd buffer address is: " swd h.8 cr
  ;

  : swd-console ( -- )
    ['] swd-key? key?-hook !
    ['] swd-key key-hook !
    ['] swd-emit? emit?-hook !
    ['] swd-emit emit-hook !
    ['] swd-emit? error-emit?-hook !
    ['] swd-emit error-emit-hook !
    ['] swd-flush-console flush-console-hook !
    ['] swd-flush-console error-flush-console-hook !
  ;

  : enable-sleep ( -- ) true use-sleep ! ;
  : disable-sleep ( -- ) false use-sleep ! ;

  \ Set the curent input to SWD within an xt
  : with-swd-input ( xt -- )
    ['] swd-key ['] swd-key? rot console::with-input
  ;

  \ Set the current output to SWD within an xt
  : with-swd-output ( xt -- )
    ['] swd-emit ['] swd-emit? rot ['] swd-flush-console swap
    console::with-output
  ;

  \ Set the current error output to SWD within an xt
  : with-swd-error-output ( xt -- )
    ['] swd-emit ['] swd-emit? rot ['] swd-flush-console swap
    console::with-error-output
  ;
  
end-module> import

commit-flash

: init ( -- )
  init
  swd-init
  swd-console
;

swd unimport

end-compress-flash

compile-to-ram

\ Do not reboot
