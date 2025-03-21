\ Copyright (c) 2022-2024 Travis Bemann
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

begin-module fat32-tools

  internal import
  oo import
  fat32 import
  simple-fat32 import
  lock import
  rtc import
  
  \ Filesystem not set exception
  : x-fs-not-set ( -- ) ." filesystem not set" cr ;

  \ Include stack overflow exception
  : x-include-stack-overflow ( -- ) ." include stack overflow" cr ;
  
  begin-module fat32-tools-internal
  
    \ Maximum include depth
    8 constant max-include-depth
    
    \ Include input buffer size
    256 constant include-buffer-size

    \ Read buffer size
    128 constant read-buffer-size
    
    \ Include frame
    begin-structure include-frame-size
      
      \ Frame FAT32 file
      <fat32-file> class-size +field frame-file

      \ End of file condition
      field: frame-eof
      
      \ Frame offset
      field: frame-offset

      \ Frame newline
      field: frame-newline

    end-structure
    
    \ The filesystem lock
    lock-size buffer: fs-lock
    
    \ Frame stack depth
    variable frame-depth
    
    \ Frame stack
    max-include-depth include-frame-size * buffer: include-stack
    
    \ Include input buffer content length
    variable include-buffer-content-len
    
    \ Include input buffer
    include-buffer-size aligned-buffer: include-buffer

    \ Read buffer
    read-buffer-size aligned-buffer: read-buffer

    \ Whether echoing code is enabled (non-negative values are enabled)
    variable echo-enabled
    
    \ Get the top frame of the include stack
    : include-stack-top@ ( -- frame )
      include-stack frame-depth @ 1- include-frame-size * +
    ;
    
    \ Get the next include stack frame
    : include-stack-next@ ( -- frame )
      include-stack frame-depth @ include-frame-size * +
    ;

    \ Read code from a file
    : read-file-into-buffer ( -- )
      include-buffer include-buffer-content-len @ +
      include-buffer-size include-buffer-content-len @ -
      include-stack-top@ frame-file read-file
      include-buffer-content-len +!
    ;
    
    \ Get the executable line length
    : execute-line-len ( -- bytes )
      include-buffer-content-len @ 0 ?do
        include-buffer i + c@ dup $0A = swap $0D = or if
          i 1+ unloop exit
        then
      loop
      include-buffer-content-len @
    ;
    
    \ Update the EOF and get the input length
    : update-line ( -- u )
      include-stack-top@ frame-newline @ if
        echo-enabled @ 0>= if cr then
        false include-stack-top@ frame-newline !
      then
      execute-line-len dup include-stack-top@ frame-offset @ +
      include-stack-top@ frame-file file-size@ =
      include-stack-top@ frame-eof !
      dup dup 0> if
        1- include-buffer + c@ dup $0A = if
          true include-stack-top@ frame-newline !
        then
        dup $0A = swap $0D = or if 1- then
      then
      echo-enabled @ 0>= if include-buffer over type then
    ;

    \ Refill file
    : frame-eval-refill ( -- )
      [:
        execute-line-len dup include-stack-top@ frame-offset +!
        dup negate include-buffer-content-len +!
        include-buffer + include-buffer include-buffer-content-len @ move
        read-file-into-buffer
        include-buffer update-line feed-input
      ;] fs-lock with-lock
    ;
    
    \ Check end of file condition
    : frame-eval-eof ( -- eof? ) include-stack-top@ frame-eof @ ;
      
    \ Un-nest an include
    : unnest-include ( -- )
      include-stack-top@ frame-file file-open? if
        include-stack-top@ frame-file close-file
      then
      frame-depth @ 1- 0 max frame-depth !
      frame-depth @ 0> if
        include-stack-top@ frame-offset @ seek-set
        include-stack-top@ frame-file seek-file
        true include-stack-top@ frame-newline !
        0 include-buffer-content-len !
        read-file-into-buffer
      then 
    ;
    
    \ Execute an included file
    : execute-file ( -- )
      [:
        [: read-file-into-buffer ;] fs-lock with-lock
        include-buffer-content-len @ 0> if
          0 include-buffer update-line ['] frame-eval-refill ['] frame-eval-eof
          evaluate-with-input
        then
      ;] try
      [: unnest-include ;] fs-lock with-lock
      ?raise
    ;

    \ List a directory with file sizes
    : list-dir ( dir -- )
      cr ." filename     creation date             modification date        "
      ."  file size"
      cr ." ------------ ------------------------- -------------------------"
      ."  ----------"
      <fat32-entry> class-size [:
        swap
        begin
          2dup read-dir if
            12 [:
              date-time-size [: { file-name-buf date-time }
                file-name-buf 12 3 pick file-name@ { file-name-len } drop
                cr file-name-buf file-name-len type
                over entry-dir? if ." /" 1 +to file-name-len then
                13 file-name-len - spaces
                date-time 2 pick create-date-time@
                date-time date-time. space
                date-time 2 pick modify-date-time@
                date-time date-time. space
                over entry-file? if
                  over entry-file-size @ 10 compat::u.r
                then
                false
              ;] with-aligned-allot
            ;] with-allot
          else
            2drop true
          then
        until
      ;] with-aligned-allot
    ;

    \ Convert newlines in a read buffer; note that the original buffer is
    \ read-buffer-size bytes, and the data in the buffer is at most half its
    \ size
    : convert-newlines { src-addr src-bytes -- }
      src-bytes 0> if
        read-buffer read-buffer-size + 0 { dest-addr dest-bytes }
        src-addr src-addr src-bytes + 1- ?do
          i c@ case
            $0A of
              -2 +to dest-addr
              $0D dest-addr c!
              $0A dest-addr 1+ c!
              2 +to dest-bytes
            endof
            $0D of endof
            -1 +to dest-addr
            dup dest-addr c!
            1 +to dest-bytes
          endcase
        -1 +loop
        dest-addr dest-bytes
      else
        src-addr 0
      then
    ;

    \ The current directory
    <fat32-dir> class-size buffer: current-dir

    \ Whether the current directory has been initialized
    variable current-dir-inited?

    \ Initialize FAT32 including
    : init-fat32-tools ( -- )
      fs-lock init-lock
      false current-dir-inited? !
      0 include-buffer-content-len !
      0 frame-depth !
      0 echo-enabled !
    ;
  
  end-module> import
  
  \ Set the current filesystem
  : current-fs! ( fs -- )
    current-dir-inited? @ if
      current-dir dir-fs@ over <> if
        current-dir close-dir
        current-dir destroy
        current-dir swap root-dir@
      else
        drop
      then
    else
      current-dir swap root-dir@
      true current-dir-inited? !
    then
  ;
  
  \ Get the current filesystem
  : current-fs@ ( fs -- )
    current-dir-inited? @ if current-dir dir-fs@ else 0 then
  ;

  \ Change the current directory
  : change-dir ( addr bytes -- )
    [: { addr bytes }
      current-fs@ { fs }
      fs averts x-fs-not-set
      addr bytes [:
        current-dir-inited? @ if
          current-dir close-dir
          current-dir destroy
        then
        current-dir swap clone-dir
        true current-dir-inited? !
      ;] fs with-open-dir-at-root-path
      current-dir change-dir
    ;] fs-lock with-lock
  ;

  \ Enable echo
  : enable-echo ( -- ) 1 echo-enabled +! ;

  \ Disable echo
  : disable-echo ( -- ) -1 echo-enabled +! ;
  
  \ Simple SDHC/SDXC FAT32 card initializer; this creates a SDHC/SDXC card
  \ interface and FAT32 filesystem and, if successful, sets it as the current
  \ filesystem.
  \
  \ sck-pin, tx-pin, rx-pin, and cs-pin are the clock, transmit, receive, and
  \ chip select pins to use. spi-device is the SPI peripheral to use; it must
  \ match sck-pin, tx-pin, and rx-pin. write-through is whether to enable
  \ write-through; enabling write-through will result in greater data integrity
  \ in the case of failures, but slower performance. If write-through is not
  \ enabled, manually flushing at opportune moments is highly recommended.
  \
  \ Note that this permanently allots space for the FAT32 filesystem and its
  \ support structures in the current task's RAM dictionary.
  : init-simple-fat32
    { write-through sck-pin tx-pin rx-pin cs-pin spi-device -- }
    here { simple-fat32-fs } <simple-fat32-fs> class-size allot
    sck-pin tx-pin rx-pin cs-pin spi-device <simple-fat32-fs> simple-fat32-fs
    init-object
    write-through simple-fat32-fs write-through!
    simple-fat32-fs current-fs!
  ;
  
  \ Load a file
  : load-file ( file -- )
    current-fs@ averts x-fs-not-set
    frame-depth @ max-include-depth < averts x-include-stack-overflow
    [:
      include-stack-next@ frame-file swap clone-file
      include-stack-next@ frame-file tell-file include-stack-next@
      frame-offset !
      1 frame-depth +!
      0 include-buffer-content-len !
      true include-stack-top@ frame-newline !
    ;] fs-lock with-lock
    execute-file
  ;
  
  \ Include a file
  : included ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    frame-depth @ max-include-depth < averts x-include-stack-overflow
    [:
      [:
        include-stack-next@ frame-file swap open-file
      ;] current-fs@ with-root-path
      0 include-stack-next@ frame-offset !
      1 frame-depth +!
      0 include-buffer-content-len !
      true include-stack-top@ frame-newline !
    ;] fs-lock with-lock
    execute-file
  ;
  
  \ Include a file
  : include ( "path" -- )
    token dup 0<> averts x-token-expected included
  ;

  \ List a directory
  : list-dir ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      ['] list-dir current-fs@ with-open-dir-at-root-path
    ;] fs-lock with-lock
  ;

  \ Create a file
  : create-file ( data-addr data-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        write-file drop
        current-fs@ flush
      ;] current-fs@ with-create-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Create a directory
  : create-dir ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        drop
        current-fs@ flush
      ;] current-fs@ with-create-dir-at-root-path
    ;] fs-lock with-lock
  ;

  \ Copy a file
  : copy-file ( path-addr path-u new-path-addr new-path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      2swap [:
        -rot [:
          swap
          begin
            read-buffer read-buffer-size 2 pick read-file dup 0> if
              read-buffer swap 3 pick write-file drop false
            else
              drop true
            then
          until
          2drop
          current-fs@ flush
        ;] current-fs@ with-create-file-at-root-path
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Append to a file
  : append-file ( data-addr data-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [: { file }
        0 seek-end file seek-file
        file write-file drop
        current-fs@ flush
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Write data at an offset in a file without truncating it
  : write-file-window ( data-addr data-u offset-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [: { file }
        seek-set file seek-file
        file write-file drop
        current-fs@ flush
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Overwrite a file and then truncate it afterwards
  : write-file ( data-addr data-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [: { file }
        file write-file drop
        current-fs@ flush
        file truncate-file
        current-fs@ flush
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ List a file with newline conversions
  : list-file ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        cr
        begin
          read-buffer read-buffer-size 2 / 2 pick read-file dup 0> if
            read-buffer swap convert-newlines type false
          else
            drop true
          then
        until
        drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;
  
  \ List the contents of a window in a file with newline conversions
  : list-file-window ( offset-u length-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        cr
        swap seek-set file seek-file
        file
        begin
          read-buffer read-buffer-size 2 / 3 pick min 2 pick read-file
          dup 0> if
            rot over - -rot
            read-buffer swap convert-newlines type over 0=
          else
            drop true
          then
        until
        2drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as raw data
  : dump-file-raw ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        begin
          read-buffer read-buffer-size 2 pick read-file dup 0> if
            read-buffer swap type false
          else
            drop true
          then
        until
        drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;
  
  \ Dump the contents of a window in a file to the console as raw data
  : dump-file-raw-window ( offset-u length-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        swap seek-set file seek-file
        file
        begin
          read-buffer read-buffer-size 3 pick min 2 pick read-file
          dup 0> if
            rot over - -rot
            read-buffer swap type over 0=
          else
            drop true
          then
        until
        2drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as bytes plus ASCII
  : dump-file ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 2 pick read-file dup 0> if
            read-buffer swap r> dump-with-offset false
          else
            rdrop drop true
          then
        until
        drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a window in a file to the console as bytes plus ASCII
  : dump-file-window ( offset-u length-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        swap seek-set file seek-file
        file
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 3 pick min 2 pick read-file dup 0> if
            rot over - -rot
            read-buffer swap r> dump-with-offset over 0=
          else
            rdrop drop true
          then
        until
        2drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as ASCII
  : dump-file-ascii ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 2 pick read-file dup 0> if
            read-buffer swap r> dump-ascii-with-offset false
          else
            rdrop drop true
          then
        until
        drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;
  
  \ Dump the contents of a window in a file to the console as ASCII
  : dump-file-ascii-window ( offset-u length-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        swap seek-set file seek-file
        file
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 3 pick min 2 pick read-file dup 0> if
            rot over - -rot
            read-buffer swap r> dump-ascii-with-offset over 0=
          else
            rdrop drop true
          then
        until
        2drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as 16-bit values and ASCII
  : dump-file-halfs ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 2 pick read-file dup 0> if
            read-buffer swap 1 bic r> dump-halfs-with-offset false
          else
            rdrop drop true
          then
        until
        drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a window in a file to the console as 16-bit values
  \ and ASCII
  : dump-file-halfs-window ( offset-u length-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        swap seek-set file seek-file
        file
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 3 pick min 2 pick read-file dup 0> if
            rot over - -rot
            read-buffer swap 1 bic r> dump-halfs-with-offset over 0=
          else
            rdrop drop true
          then
        until
        2drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as 32-bit values and ASCII
  : dump-file-cells ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 2 pick read-file dup 0> if
            read-buffer swap 3 bic r> dump-cells-with-offset false
          else
            rdrop drop true
          then
        until
        drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a window in  file to the console as 32-bit values
  \ and ASCII
  : dump-file-cells-window ( offset-u length-u path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        swap seek-set file seek-file
        file
        cr
        begin
          dup tell-file >r
          read-buffer read-buffer-size 0 fill
          read-buffer read-buffer-size 3 pick min 2 pick read-file dup 0> if
            rot over - -rot
            read-buffer swap 3 bic r> dump-cells-with-offset over 0=
          else
            rdrop drop true
          then
        until
        2drop
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Read a file, from an offset from the start of the file, to a fixed-sized
  \ buffer and return the length actually read
  : read-file ( buffer-addr buffer-u offset-u path-addr path-u -- read-u )
    current-fs@ averts x-fs-not-set
    [:
      [:
        { file }
        seek-set file seek-file
        file read-file
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Get the size of a file
  : file-size@ ( path-addr path-u -- size-u )
    current-fs@ averts x-fs-not-set
    [:
      [:
        file-size@
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;
  
  \ Remove a file
  : remove-file ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      ['] remove-file current-fs@ with-root-path
      current-fs@ flush
    ;] fs-lock with-lock
  ;

  \ Remove a directory
  : remove-dir ( path-addr path-u -- )
    current-fs@ averts x-fs-not-set
    [:
      ['] remove-dir current-fs@ with-root-path
      current-fs@ flush
    ;] fs-lock with-lock
  ;

  \ Rename a file
  : rename ( path-addr path-u new-name-addr new-name-u -- )
    current-fs@ averts x-fs-not-set
    2swap
    [:
      ['] rename current-fs@ with-root-path
      current-fs@ flush
    ;] fs-lock with-lock
  ;

  \ Get whether a file or directory at a given path exists
  : exists? ( path-addr path-u -- exists? )
    current-fs@ averts x-fs-not-set
    [:
      current-fs@ root-path-exists?
    ;] fs-lock with-lock
  ;

  \ Get whether a directory entry is a file
  : file? ( path-addr path-u -- file? )
    current-fs@ averts x-fs-not-set
    [:
      ['] file? current-fs@ with-root-path
    ;] fs-lock with-lock
  ;

  \ Get whether a directory entry is a directory
  : dir? ( path-addr path-u -- dir? )
    current-fs@ averts x-fs-not-set
    [:
      ['] dir? current-fs@ with-root-path
    ;] fs-lock with-lock
  ;

  \ Set the current input to a file within an xt
  : with-file-input ( path-addr path-u xt -- )
    current-fs@ averts x-fs-not-set
    [:
      -rot
      [:
        swap with-file-input
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;
  
  \ Set the current output to a file within an xt
  : with-file-output ( path-addr path-u xt -- )
    current-fs@ averts x-fs-not-set
    [:
      -rot
      [:
        swap with-file-output
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

  \ Set the current error output to a file within an xt
  : with-file-error-output ( path-addr path-u xt -- )
    current-fs@ averts x-fs-not-set
    [:
      -rot
      [:
        swap with-file-error-output
      ;] current-fs@ with-open-file-at-root-path
    ;] fs-lock with-lock
  ;

end-module> import

: init ( -- )
  init
  fat32-tools::fat32-tools-internal::init-fat32-tools
;

reboot
