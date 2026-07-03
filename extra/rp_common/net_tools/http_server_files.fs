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
\ s" /" fat32-tools::current-fs@ s" /files/" net-http-files::init-files
\
\ You may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/files/

begin-module net-http-files
  
  net-http import
  oo import
  fat32 import
  lock import

  0 value files-fs
  0 0 2value files-uri
  0 0 2value files-base
  
  : filter-path { addr bytes -- allowed? }
    begin bytes 0> while
      0 { offset }
      begin
        offset bytes < if
          addr offset + c@ [char] / = dup not if 1 +to offset then
        else
          true
        then
      until
      addr offset s" ." equal-strings? if false exit then
      addr offset s" .." equal-strings? if false exit then
      offset +to addr offset negate +to bytes
      offset bytes < if 1 +to addr -1 +to bytes then
    repeat
    true
  ;

  lock-size buffer: files-lock
  : init-files-lock files-lock init-lock ;
  initializer init-files-lock

  256 constant path-buf-size
  path-buf-size buffer: path-buf
  0 value path-buf-bytes

  12 constant max-file-name-len
  max-file-name-len buffer: file-name-buf

  <fat32-dir> class-size buffer: parent-dir
  <fat32-file> class-size buffer: leaf-file
  <fat32-dir> class-size buffer: leaf-dir
  <fat32-entry> class-size buffer: dir-entry

  512 constant file-buf-size
  file-buf-size buffer: file-buf

  : init-dir-entry ( -- ) <fat32-entry> dir-entry init-object ;
  initializer init-dir-entry
  
  : find-parent-dir { addr bytes -- addr' bytes' leaf-addr' leaf-bytes' }
    bytes 1 > if
      bytes { parent-bytes }
      -1 +to parent-bytes
      addr parent-bytes + c@ [char] / = if
        -1 +to bytes
        -1 +to parent-bytes
      then
      begin
        parent-bytes 0> if
          addr parent-bytes + c@ [char] / = if
            addr parent-bytes addr parent-bytes + 1+ bytes parent-bytes 1+ -
            true
          else
            -1 +to parent-bytes false
          then
        else
          addr parent-bytes + c@ [char] / = if
            s" /" addr 1+ bytes 1-
          else
            s" /" addr bytes
          then
          true
        then
      until
    else
      s" /" 0 0
    then
  ;

  : strip-base { addr bytes -- addr' bytes' }
    addr bytes files-base equal-case-strings? if s" /" exit then
    addr files-base nip 1- + bytes files-base nip 1- -
  ;

  : serve-dir ( dir -- )
    path-buf path-buf-bytes + 1- c@ [char] / <> if
      http-protocol.
      ." 301 Moved Permanently" cr
      ." Location: " http-uri@ type ." /" cr
      ." Connection: close" cr cr
      exit
    then
    s" text/html" http-ok.
    http-method@ method-head = if drop exit then
    path-buf path-buf-bytes strip-base { D: path }
    ." <html>" cr
    ." <head><title>" path type ." </title></head>" cr
    ." <body>" cr
    ." <h1>" path type ." </h1>" cr
    ." <ul>" cr
    [: { dir }
      begin dir-entry dir read-dir while
        dir-entry entry-file? if
          file-name-buf max-file-name-len dir-entry file-name@ { D: file-name }
          .\" <li><a href=\"" file-name type .\" \">" file-name type ." </li>"
          cr
        else
          dir-entry entry-dir? if
            file-name-buf max-file-name-len dir-entry file-name@ { D: dir-name }
            .\" <li><a href=\"" dir-name type .\" /\">" dir-name type ." /</li>"
            cr
          then
        then
      repeat
    ;] try if drop then
    ." </ul>" cr
    ." </body>" cr
    ." </html>" cr
  ;

  : serve-file ( file -- )
    s" text/text" http-ok.
    http-method@ method-head = if drop exit then
    [: { file }
      begin
        file-buf file-buf-size file read-file { bytes }
        file-buf bytes type
        bytes 0=
      until
    ;] try if drop then
  ;

  : strip-? { addr bytes -- addr bytes' }
    bytes 0 ?do addr i + c@ [char] ? = if addr i unloop exit then loop
    addr bytes
  ;
  
  : serve-files ( -- )
    http-method@ dup method-head = over method-get = or swap method-post = or if
      [:
        http-uri@ strip-?
        path-buf path-buf-size files-base nip - url-decode not if
          2drop http-invalid. exit
        then
        { path-addr path-bytes }
        files-uri nip negate +to path-bytes
        path-addr files-uri nip + path-addr path-bytes move
        path-addr path-addr files-base nip + path-bytes move
        files-base nip +to path-bytes
        files-base path-addr swap move
        path-bytes to path-buf-bytes
        path-buf path-buf-bytes filter-path if
          [:
            parent-dir path-buf path-buf-bytes find-parent-dir 2drop
            ['] clone-dir files-fs with-open-dir-at-root-path
            [:
              path-buf path-buf-bytes find-parent-dir 2nip { D: leaf-name }
              leaf-name nip 0= if
                parent-dir serve-dir
              else
                leaf-name parent-dir file? if
                  leaf-name leaf-file parent-dir open-file
                  leaf-file ['] serve-file try
                  leaf-file close-file
                  leaf-file destroy
                  ?raise
                else
                  leaf-name parent-dir dir? if
                    leaf-name leaf-dir parent-dir open-dir
                    leaf-dir ['] serve-dir try
                    leaf-dir close-dir
                    leaf-dir destroy
                    ?raise
                  else
                    http-invalid.
                  then
                then
              then
            ;] try
            parent-dir close-dir
            parent-dir destroy
            if http-invalid. then
          ;] try if http-invalid. then
        else
          http-invalid.
        then
      ;] files-lock with-lock
    else
      http-method-not-allowed.
    then
  ;

  : init-files ( D: files-base fs D: files-uri -- )
    here -rot cstring, cell align, count to files-uri
    to files-fs
    here -rot cstring, cell align, count to files-base
    ['] serve-files files-uri register-prefix-uri
  ;

end-module
