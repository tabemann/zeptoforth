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

continue-module fat32-tools
  
  oo import
  fat32 import

  continue-module fat32-tools-internal
    
    \ Extract an element from a path
    : split-elem
      { addr bytes -- elem-addr elem-bytes rest-addr rest-bytes }
      addr bytes { cur-addr cur-bytes }
      begin cur-bytes 0> while
        cur-addr c@ [char] / <> if
          1 +to cur-addr -1 +to cur-bytes
        else
          cur-bytes 1 > if
            addr bytes cur-bytes - cur-addr 1+ cur-bytes 1- exit
          else
            addr bytes cur-bytes - cur-addr 1 exit
          then
        then
      repeat
      addr bytes 0 0
    ;

    \ Match a string against a glob
    : match-glob? { addr bytes glob-addr glob-bytes -- match? }
      addr bytes glob-addr glob-bytes equal-strings? not if
        addr bytes s" ." equal-strings? if false exit then
        addr bytes s" .." equal-strings? if false exit then
      then
      0 0 0 0 { px nx next-px next-nx }
      begin px glob-bytes < nx bytes < or while
        false { continue? }
        px glob-bytes < if
          glob-addr px + c@ fat32-internal::upcase-char { c }
          c [char] ? = if \ Single-character wildcard
            nx bytes < if
              1 +to px 1 +to nx true to continue?
            then
          else
            c [char] * = if \ Zero-or-greater character wildcard
              \ Try to match at nx.
              \ If that doesn't work out, restart at nx 1+ next.
              px to next-px nx 1+ to next-nx 1 +to px true to continue?
            else
              \ Ordinary character
              nx bytes < if
                addr nx + c@ fat32-internal::upcase-char c = if
                  1 +to px 1 +to nx true to continue?
                then
              then
            then
          then
        then
        continue? not if
          \ Mismatch. Maybe restart.
          next-nx 0> next-nx bytes <= and if
            next-px to px next-nx to nx true to continue?
          then
        then
        continue? not if false exit then
      repeat
      \ Matched all of pattern to all of name. Success.
      true
    ;

    \ Glob frame
    <object> begin-class <glob-frame>

      \ The directory for a frame
      <fat32-dir> class-size member glob-dir

      \ The glob pattern for the frame
      2 cells member glob-pattern

      \ The glob path buffer
      2 cells member glob-buf

      \ The glob path offset
      cell member glob-off

      \ The glob xt
      cell member glob-xt
      
      \ The previous frame
      cell member glob-prev

      \ Entry
      <fat32-entry> class-size member glob-entry
      
      \ Name buffer
      12 cell align member glob-name-buf
      
    end-class

    \ Implement a glob frame
    <glob-frame> begin-implement

      \ Constructor
      :noname { prev xt off D: buf D: pattern dir self -- }
        self <object>->new
        self glob-dir dir clone-dir
        pattern self glob-pattern 2!
        buf self glob-buf 2!
        off self glob-off !
        xt self glob-xt !
        prev self glob-prev !
        <fat32-entry> self glob-entry init-object
      ; define new

      \ Destructor
      :noname { self -- }
        self glob-entry destroy
        self glob-dir close-dir
        self glob-dir destroy
        self <object>->destroy
      ; define destroy
      
    end-implement
    
  end-module> import
  
  \ Execute an xt for each file or directory matching a glob path
  : glob ( addr bytes xt -- )
    256 [: { addr bytes xt buf }
      ram-here { frame }
      <glob-frame> class-size cell align ram-allot
      addr bytes xt buf frame [:
        <fat32-dir> class-size [:
          { addr bytes xt buf frame base-dir }
          0 { off }
          begin-critical
          bytes 0> if
            addr c@ [char] / = if
              base-dir current-fs@ root-dir@ 1 +to addr -1 +to bytes
              [char] / buf c! 1 to off
            else
              base-dir current-fs@ current-dir@
            then
          then
          0 xt off buf off + 256 off -
          addr bytes base-dir <glob-frame> frame init-object
          base-dir close-dir
          base-dir destroy
          addr bytes
        ;] with-aligned-allot
      ;] try
      ?dup if frame ram-here! end-critical ?raise then
      to bytes to addr
      end-critical
      begin frame while
        frame [: { frame }
          frame glob-entry frame glob-dir read-dir if
            frame glob-pattern 2@ split-elem
            { elem-addr elem-bytes rest-addr rest-bytes }
            frame glob-name-buf 12 frame glob-entry file-name@ nip
            { name-bytes }
            frame glob-name-buf name-bytes
            elem-addr elem-bytes match-glob? if
              name-bytes frame glob-buf 2@ nip <= if
                frame glob-name-buf frame glob-buf 2@ drop name-bytes move
                rest-bytes 0> if
                  rest-addr rest-bytes s" /" equal-strings? if
                    name-bytes frame glob-buf 2@ nip < if
                      [char] / frame glob-buf 2@ drop name-bytes + c!
                      frame glob-entry entry-dir? if
                        frame glob-buf 2@ drop
                        frame glob-off @ -
                        frame glob-off @ name-bytes 1+ +
                        frame glob-xt @ execute
                      then
                    then
                    false
                  else
                    name-bytes frame glob-buf 2@ nip < if
                      frame glob-entry entry-dir? if
                        [char] / frame
                        glob-buf 2@ drop name-bytes + c!
                        name-bytes 1+ true
                      else
                        false
                      then
                    else
                        false
                    then
                  then
                else
                  frame glob-entry entry-file?
                  frame glob-entry entry-dir? or if
                    frame glob-buf 2@ drop
                    frame glob-off @ -
                    frame glob-off @ name-bytes +
                    frame glob-xt @ execute
                  then
                  false
                then
              else
                false
              then
            else
              false
            then
            if { name-bytes }
              ram-here { new-frame }
              <glob-frame> class-size cell align ram-allot
              name-bytes frame new-frame <fat32-dir> class-size [:
                [:
                  { name-bytes frame new-frame dir }
                  frame glob-buf 2@ drop name-bytes 1-
                  dir frame glob-dir open-dir
                  name-bytes frame new-frame dir [:
                    { name-bytes frame new-frame dir }
                    frame frame glob-xt @ frame glob-off @ name-bytes +
                    frame glob-buf 2@
                    name-bytes - swap name-bytes + swap
                    frame glob-pattern 2@ split-elem 2nip
                    dir <glob-frame> new-frame init-object
                  ;] try
                  dir close-dir
                  dir destroy
                  ?raise
                ;] with-aligned-allot
              ;] critical
              new-frame
            else
              frame
            then
          else
            frame glob-prev @ { prev-frame }
            frame destroy
            frame ram-here!
            prev-frame
          then
        ;] try
        dup if
          begin frame while
            frame glob-prev @ { prev-frame }
            frame destroy
            frame ram-here!
            prev-frame to frame
            frame if frame <glob-frame> class-size + ram-here! then
          repeat
        else
          swap to frame
          frame if frame <glob-frame> class-size + ram-here! then
        then
        ?raise
      repeat
    ;] with-allot
  ;
  
end-module
