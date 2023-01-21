\ Copyright (c) 2022-2023 Travis Bemann
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

    end-structure
    
    \ The current filesystem
    variable current-fs

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
      execute-line-len dup include-stack-top@ frame-offset @ +
      include-stack-top@ frame-file file-size@ =
      include-stack-top@ frame-eof !
      dup dup 0> if
        1- include-buffer + c@ dup $0A = swap $0D = or if 1- then
      then
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
      frame-depth @ 1- 0 max frame-depth !
      frame-depth @ 0> if
        include-stack-top@ frame-offset @ seek-set
        include-stack-top@ frame-file seek-file
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

    \ List a directory
    : list-dir ( dir -- )
      cr ." filename      creation date              modification date"
      cr ." ------------  -------------------------  -------------------------"
      <fat32-entry> class-size [:
        swap
        begin
          2dup read-dir if
            12 [:
              date-time-size [: { file-name-buf date-time }
                file-name-buf 12 3 pick file-name@ { file-name-len } drop
                cr file-name-buf file-name-len type
                over entry-dir? if ." /" 1 +to file-name-len then
                14 file-name-len - spaces
                date-time 2 pick create-date-time@
                date-time date-time. 2 spaces
                date-time 2 pick modify-date-time@
                date-time date-time.
                false
              ;] with-aligned-allot
            ;] with-allot
          else
            2drop true
          then
        until
      ;] with-aligned-allot
    ;

    \ List a root direcotry
    : list-root ( -- )
      <fat32-dir> class-size [:
        dup current-fs @ root-dir@ list-dir
      ;] with-aligned-allot
    ;
    
    \ List a path directory
    : list-path ( c-addr u -- )
      <fat32-dir> class-size [:
        -rot [:
          3 pick swap open-dir
        ;] current-fs @ with-root-path
        list-dir
      ;] with-aligned-allot
    ;

    \ Strip the leading separators from a path
    : strip-leading-separators ( c-addr u -- c-addr' u' )
      begin
        dup 0<> if
          over c@ [char] / = if swap 1+ swap 1- false else true then
        else
          true
        then
      until
    ;

    \ Initialize FAT32 including
    : init-fat32-tools ( -- )
      fs-lock init-lock
      0 current-fs !
      0 include-buffer-content-len !
      0 frame-depth !
    ;
  
  end-module> import
  
  \ Set the current filesystem
  : current-fs! ( fs -- ) current-fs ! ;
  
  \ Get the current filesystem
  : current-fs@ ( fs -- ) current-fs @ ;
  
  \ Load a file
  : load-file ( file -- )
    current-fs @ averts x-fs-not-set
    frame-depth @ max-include-depth < averts x-include-stack-overflow
    [:
      include-stack-next@ frame-file <fat32-file> class-size move
      include-stack-next@ frame-file tell-file include-stack-next@
      frame-offset !
      1 frame-depth +!
      0 include-buffer-content-len !
    ;] fs-lock with-lock
    execute-file
  ;
  
  \ Include a file
  : included ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    frame-depth @ max-include-depth < averts x-include-stack-overflow
    [:
      [:
        include-stack-next@ frame-file swap open-file
      ;] current-fs@ with-root-path
      0 include-stack-next@ frame-offset !
      1 frame-depth +!
      0 include-buffer-content-len !
    ;] fs-lock with-lock
    execute-file
  ;
  
  \ Include a file
  : include ( "path" -- )
    token dup 0<> averts x-token-expected included
  ;

  \ List a directory
  : list-dir ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    strip-leading-separators
    [:
      dup 0= if 2drop list-root else list-path then
    ;] fs-lock with-lock
  ;

  \ Create a file
  : create-file ( data-addr data-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap create-file ;] current-fs @ with-root-path
        write-file drop
        current-fs @ flush
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Create a directory
  : create-dir ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-dir> class-size [:
        -rot [: 3 roll swap create-dir ;] current-fs @ with-root-path
        current-fs @ flush
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Copy a file
  : copy-file ( path-addr path-u new-path-addr new-path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      2swap
      <fat32-file> class-size [:
        <fat32-file> class-size [:
          2swap [: 3 pick swap open-file ;] current-fs @ with-root-path
          >r
          -rot [: 3 pick swap fat32::create-file ;]
          current-fs @ with-root-path
          r>
          begin
            read-buffer read-buffer-size 2 pick read-file dup 0> if
              read-buffer swap 3 pick write-file drop false
            else
              drop true
            then
          until
          2drop
          current-fs @ flush
        ;] with-aligned-allot
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Append to a file
  : append-file ( data-addr data-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        0 seek-end 2 pick seek-file
        write-file drop
        current-fs @ flush
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Write data at an offset in a file without truncating it
  : write-file-window ( data-addr data-u offset-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r seek-set r@ seek-file r>
        write-file drop
        current-fs @ flush
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Overwrite a file and then truncate it afterwards
  : write-file ( data-addr data-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        dup >r write-file drop r> truncate-file
        current-fs @ flush
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as raw data
  : dump-file-raw ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        begin
          read-buffer read-buffer-size 2 pick read-file dup 0> if
            read-buffer swap type false
          else
            drop true
          then
        until
        drop
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;
  
  \ Dump the contents of a window in a file to the console as raw data
  : dump-file-raw-window ( offset-u length-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r swap seek-set r@ seek-file r>
        begin
          read-buffer read-buffer-size 3 pick min 2 pick read-file dup 0> if
            rot over - -rot
            read-buffer swap type over 0=
          else
            drop true
          then
        until
        2drop
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as bytes plus ASCII
  : dump-file ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a window in a file to the console as bytes plus ASCII
  : dump-file-window ( offset-u length-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r swap seek-set r@ seek-file r>
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as ASCII
  : dump-file-ascii ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;
  
  \ Dump the contents of a window in a file to the console as ASCII
  : dump-file-ascii-window ( offset-u length-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r swap seek-set r@ seek-file r>
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as 16-bit values and ASCII
  : dump-file-halfs ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a window in a file to the console as 16-bit values
  \ and ASCII
  : dump-file-halfs-window ( offset-u length-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r swap seek-set r@ seek-file r>
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a file to the console as 32-bit values and ASCII
  : dump-file-cells ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Dump the contents of a window in  file to the console as 32-bit values
  \ and ASCII
  : dump-file-cells-window ( offset-u length-u path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r swap seek-set r@ seek-file r>
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
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Read a file, from an offset from the start of the file, to a fixed-sized
  \ buffer and return the length actually read
  : read-file ( buffer-addr buffer-u offset-u path-addr path-u -- read-u )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        >r seek-set r@ seek-file r>
        read-file
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;

  \ Get the size of a file
  : file-size@ ( path-addr path-u -- size-u )
    current-fs @ averts x-fs-not-set
    [:
      <fat32-file> class-size [:
        -rot [: 3 pick swap open-file ;] current-fs @ with-root-path
        file-size@
      ;] with-aligned-allot
    ;] fs-lock with-lock
  ;
  
  \ Remove a file
  : remove-file ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      ['] remove-file current-fs @ with-root-path
      current-fs @ flush
    ;] fs-lock with-lock
  ;

  \ Remove a directory
  : remove-dir ( path-addr path-u -- )
    current-fs @ averts x-fs-not-set
    [:
      ['] remove-dir current-fs @ with-root-path
      current-fs @ flush
    ;] fs-lock with-lock
  ;

  \ Rename a file
  : rename ( path-addr path-u new-name-addr new-name-u -- )
    current-fs @ averts x-fs-not-set
    2swap
    [:
      ['] rename current-fs @ with-root-path
      current-fs @ flush
    ;] fs-lock with-lock
  ;

  \ Get whether a file or directory at a given path exists
  : exists? ( path-addr path-u -- exists? )
    current-fs @ averts x-fs-not-set
    [:
      current-fs @ root-path-exists?
    ;] fs-lock with-lock
  ;

  \ Get whether a directory entry is a file
  : file? ( path-addr path-u -- file? )
    current-fs @ averts x-fs-not-set
    [:
      ['] file? current-fs @ with-root-path
    ;] fs-lock with-lock
  ;

  \ Get whether a directory entry is a directory
  : dir? ( path-addr path-u -- dir? )
    current-fs @ averts x-fs-not-set
    [:
      ['] dir? current-fs @ with-root-path
    ;] fs-lock with-lock
  ;

end-module> import

: init ( -- )
  init
  fat32-tools::fat32-tools-internal::init-fat32-tools
;

reboot
