\ Copyright (c) 2026 Travis Bemann
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

\ Note that prior to running this code, one must have uploaded the CYW43439
\ firmware, CYW43439 driver, and zeptoIP to one's Raspberry Pi Pico W (this
\ does not work on a Raspberry Pi Pico) as follows:
\ 
\ Execute from the base directory of zeptoforth:
\ 
\ utils/load_cyw43_fw.sh <tty device> <43439A0.bin path> <43439A0_clm.bin path>
\ 
\ 43439A0.bin and 43439A0_clm.bin can be gotten from:
\ https://github.com/tabemann/cyw43-firmware/tree/master/cyw43439-firmware
\ 
\ Then execute from the base directory of zeptoforth:
\ 
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv4_all.fs
\ 
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\
\ Then ensure that extra/rp_common/net_tools/pico_w_ipv4_base_no_handler.fs or
\ extra/rp_common/net_tools/pico_w_ipv6_base_no_handler.h4 is loaded into
\ RAM and configured per the directions therein.
\
\ Once that is done load extra/rp_common/net_tools/http_server.fs, execute:
\
\ 80 pico-w-net::my-interface @ net-http::start-server
\
\ where 80 is the HTTP port; a different value can be used if one wants to use
\ a different port
\
\ Then load this file and execute:
\
\ net-http-console-out::init-console-out
\
\ You may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/console-out

begin-module net-http-console-out

  net-http import
  
  0 value saved-emit-hook

  : console-out-form ( -- )
    s" text/html" http-ok.
    http-method@ method-head = if exit then
    ." <html>" cr
    ." <head><title>Console Output</title></head>" cr
    ." <body>" cr
    ." <h1>Console Output</h1>" cr
    .\" <form action=\"/console-out\" method=\"POST\">" cr
    .\" <textarea id=\"user-message\" name=\"s\" rows=\"24\" cols=\"80\" placeholder=\"Type your message here...\">" cr
    ." </textarea>" cr
    ." <br><br>" cr
    .\" <button type=\"submit\">Submit</button>" cr
    ." </form>" cr
    ." </body>" cr
    ." </html>" cr
  ;

  : parse-header
    { addr bytes -- key-addr' key-bytes' val-addr' val-bytes' valid? }
    begin
      bytes 0> if
        addr c@ dup bl = over $0D = or swap $0A = or if
          1 +to addr -1 +to bytes false
        else
          true
        then
      else
        true
      then
    until
    bytes 0 ?do
      addr i + c@ dup [char] : = swap bl = or if
        i { key-end }
        bytes i ?do
          addr i + c@ bl <> if
            addr i + c@ [char] : = if
              bytes i 1+ ?do
                addr i + c@ bl <> if
                  i { val-start }
                  bytes i ?do
                    addr i + c@ dup $0D = swap $0A = or if
                      unloop unloop unloop unloop
                      addr key-end addr val-start + i val-start - true exit
                    then
                  loop
                  unloop unloop unloop
                  addr key-end addr val-start + bytes val-start - true exit
                then
              loop
              unloop unloop addr key-end 0 0 true exit
            else
              unloop unloop 0 0 0 0 false exit
            then
          then
        loop
      then
      addr i + c@ dup $0D = swap $0A = or if unloop 0 0 0 0 false exit then
    loop
    0 0 0 0 false
  ;

  : parse-headers ( -- len )
    256 [: { buf }
      0 0 0 false false 0 { cr-count nl-count offset reject? urlencoded? len }
      begin cr-count 2 = nl-count 2 = and not while
        key? if
          key { c }
          c $0D = if 1 +to cr-count then
          c $0A = if 1 +to nl-count then
          c $0D <> c $0A <> and if
            0 to cr-count 0 to nl-count
            c buf offset + c!
            1 +to offset
            256 offset = if true to reject? then
          then
          cr-count 0> nl-count 0> and if
            offset 0> reject? not and if
              buf offset parse-header if { D: key D: val }
                key s" Content-Type" equal-case-strings? if
                  val s" application/x-www-form-urlencoded" equal-strings?
                  to urlencoded?
                then
                key s" Content-Length" equal-case-strings? if
                  base @ { saved-base }
                  val [: decimal parse-unsigned ;] try
                  saved-base base !
                  ?raise
                  if
                    to len
                  else
                    drop 0 to len
                  then
                then
              else
                2drop 2drop
              then
            then
            false to reject?
            0 to offset
          then
        else
          pause
        then
      repeat
      urlencoded? if len else 0 then
    ;] with-allot
  ;

  : parse-escape { len -- c len' valid? }
    0 false { W^ hbuf hdone? }
    begin len 0> hdone? not and while
      key? if
        key hbuf c! -1 +to len true to hdone?
      else
        pause
      then
    repeat
    hdone? if
      false to hdone?
      begin len 0> hdone? not and while
        key? if
          key hbuf 1+ c! -1 +to len true to hdone?
        else
          pause
        then
      repeat
      hdone? if
        base @ { saved-base }
        hbuf [: hex 2 parse-unsigned ;] try
        saved-base base !
        ?raise
        if len true else drop 0 len false then
      else
        0 len false
      then
    else
      0 len false
    then
  ;

  : parse-key { len buf -- len' s? }
    0 false { offset done? }
    begin len 0> done? not and while
      key? if
        key { c }
        -1 +to len
        c [char] % <> c [char] = <> and if
          c buf offset + c!
          1 +to offset
        then
        c [char] = = if true to done? then
        c [char] % = if
          len parse-escape swap to len if
            buf offset + c! 1 +to offset
          else
            drop
          then
        then
      else
        pause
      then
    repeat
    done? not if 0 to len 0 to offset then
    len buf offset s" s" equal-case-strings?
  ;

  : parse-request ( -- )
    parse-headers 256 [: { len buf }
      begin len 0> while
        len buf parse-key { s? } to len
        false { done? }
        begin len 0> done? not and while
          key? if
            key { c }
            -1 +to len
            c [char] % <> c [char] & <> and c [char] + <> and if
              s? if c saved-emit-hook execute then
            then
            c [char] + = if
              s? if bl saved-emit-hook execute then
            then
            c [char] & = if true to done? then
            c [char] % = if
              len parse-escape swap to len if
                s? if saved-emit-hook execute else drop then
              else
                drop
              then
            then
          else
            pause
          then
        repeat
        len 0> done? not and if exit then
      repeat
    ;] with-allot
  ;
  
  : do-console-out ( -- )
    http-method@ method-post = if
      parse-request
    then
    http-method@ dup method-head = over method-get = or swap method-post = or if
      console-out-form
    then
  ;
  
  : init-console-out ( -- )
    emit-hook @ to saved-emit-hook
    ['] do-console-out s" /console-out" register-fixed-uri
  ;
  
end-module
