\ Copyright (c) 2025 Travis Bemann
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

begin-module picocalc-screenshot

  oo import
  fat32 import
  text8 import
  font import
  picocalc-term-common import
  picocalc-term import

  use-5x8-font? [if]
    simple-font-5x8 import
  [then]
  use-6x8-font? [if]
    simple-font-6x8 import
  [then]
  use-7x8-font? [if]
    simple-font import
  [then]
  
  begin-module picocalc-screenshot-internal

    \ Sector size in bytes
    fat32-internal::sector-size constant sector-size

    \ Writer not open exception
    : x-writer-not-open ( -- ) ." writer not open" cr ;
    
    \ File writing class
    <object> begin-class <file-writer>

      \ Is the writer open?
      cell member writer-open
      
      \ The file being written to
      <fat32-file> class-size member written-file

      \ The write buffer
      sector-size cell align member write-buffer

      \ The write offset
      cell member write-offset

      \ Open the file
      method open-writer ( path-addr path-bytes fs writer -- )

      \ Close the file
      method close-writer ( writer -- )

      \ Do writing
      method do-write ( writer -- )
      
      \ Write a byte to the file
      method c>writer ( c writer -- )

      \ Write a halfword to the file
      method h>writer ( h writer -- )

      \ Write a word to the file
      method >writer ( x writer -- )

      \ Write a buffer to the file
      method buffer>writer ( addr bytes writer -- )

    end-class

    \ Data offset
    54 256 cells + constant data-offset
    
    \ *depth*
    : *depth* [: ." *" emit ." * " depth . flush-console ;] console::with-serial-output ;
    

    \ Implement file writing class
    <file-writer> begin-implement

      \ Constructor
      :noname { self -- }
        self <object>->new
        false self writer-open !
        0 self write-offset !
      ; define new

      \ Destructor
      :noname { self -- }
        self writer-open @ if self close-writer then
        self <object>->destroy
      ; define destroy

      \ Open the file
      :noname { path-addr path-bytes fs self -- }
        self writer-open @ if self close-writer then
\        path-addr path-bytes [: type space flush-console ;] console::with-serial-output
        self path-addr path-bytes [: { self file }
          self written-file file clone-file
          true self writer-open !
          0 self write-offset !
        ;] fs with-create-file-at-root-path
      ; define open-writer

      \ Close the file
      :noname { self -- }
        self writer-open @ if
          self do-write
          self written-file tell-file { W^ size }
          2 seek-set self written-file seek-file
          0 { offset }
          begin offset cell < while
            size offset + cell offset - self written-file write-file +to offset
          repeat
          34 seek-set self written-file seek-file
          size @ data-offset - size !
          0 to offset
          begin offset cell < while
            size offset + cell offset - self written-file write-file +to offset
          repeat
          self written-file close-file
          self written-file destroy
          false self writer-open !
        then
      ; define close-writer

      \ Do writing
      :noname { self -- }
        self writer-open @ averts x-writer-not-open
        0 { current-offset }
        begin current-offset self write-offset @ < while
          self write-buffer current-offset +
          self write-offset @ current-offset -
          self written-file write-file +to current-offset
        repeat
        0 self write-offset !
      ; define do-write
      
      \ Write a byte to the file
      :noname { c self -- }
        self writer-open @ averts x-writer-not-open
        self write-offset @ sector-size = if self do-write then
        c self write-buffer self write-offset @ + c!
        1 self write-offset +!
      ; define c>writer

      \ Write a halfword to the file
      :noname { h self -- }
        h $FF and self c>writer
        h 8 rshift $FF and self c>writer
      ; define h>writer

      \ Write a word to the file
      :noname { x self -- }
        x $FF and self c>writer
        x 8 rshift $FF and self c>writer
        x 16 rshift $FF and self c>writer
        x 24 rshift self c>writer
      ; define >writer

      \ Write a buffer to the file
      :noname { addr bytes self -- }
        begin bytes 0> while
          addr c@ self c>writer
          1 +to addr
          -1 +to bytes
        repeat
      ; define buffer>writer

    end-implement

    \ Strip path
    : strip-path { addr bytes -- bytes' }
      begin
        bytes 0> if
          addr bytes 1- + c@ [char] / = if
            -1 +to bytes false
          else
            true
          then
        else
          true
        then
      until
      bytes
    ;

    \ We are out of available screenshots
    : x-out-of-screenshots ( -- ) ." out of screenshots" cr ;

    \ Open a writer for the next screenshot
    : open-next-screenshot ( path-addr path-bytes writer fs -- )
      base @ { saved-base }
      [:
        10 base !
        <fat32-entry> [:
          3 pick 13 + cell align [:
            { path-addr path-bytes writer fs entry new-path-buffer }
            path-addr path-bytes strip-path to path-bytes
            path-addr path-bytes fs root-path-exists? not if
              path-addr path-bytes ['] drop fs with-create-dir-at-root-path
            then
            path-addr path-bytes writer fs entry new-path-buffer
            path-addr path-bytes
            [:
              { path-addr path-bytes writer fs entry new-path-buffer dir }
              -1 { index }
              begin entry dir read-dir while
                new-path-buffer 13 entry file-name@ { name-buffer name-bytes }
                name-bytes 7 > if
                  name-buffer 3 s" SCR" equal-strings? if
                    name-buffer 3 + name-bytes 7 - parse-unsigned if
                      index max to index
                    else
                      drop
                    then
                  then
                then
              repeat
              index 99999 < averts x-out-of-screenshots
              path-addr new-path-buffer path-bytes move
              s" /SCR00000.BMP" new-path-buffer path-bytes + swap move
              new-path-buffer path-bytes index 5 cell align [:
                { new-path-buffer path-bytes index num-buffer }
                num-buffer index 1+ format-unsigned { num-addr num-bytes }
                num-addr new-path-buffer path-bytes 9 + num-bytes - +
                num-bytes move
              ;] with-allot
            ;] fs with-open-dir-at-root-path
            new-path-buffer path-bytes 13 + fs writer open-writer
          ;] with-aligned-allot
        ;] with-object
      ;] try
      saved-base base !
      ?raise
    ;

    \ RLE 8-bit/pixel format
    1 constant BI_RLE8

    \ The screen dimensions (assuming a square screen) in meters
    pi 4,0 f/ sin 0,1016 f* 2constant screen-edge-len
    
    \ Write a color table
    : write-color-table { writer -- }
      256 0 do
        i $C0 and dup $40 and 0<> $3F and or writer c>writer
        i $38 and 2 lshift dup $20 and 0<> $1F and or writer c>writer
        i $07 and 5 lshift dup $20 and 0<> $1F and or writer c>writer
        $FF writer c>writer
      loop
    ;

    \ The out-of-range pixel color
    0 0 0 rgb8 constant out-of-range-color
    
    \ Get a pixel's color
    : pixel@ { x y display -- color }
      display dim@ { term-width term-height }
      term-char-dim@ { char-width char-height }
      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-7x8-font? ] [if] a-simple-font [then]
      { our-font }
      x 0< y 0< or
      x term-width char-width * >= or y term-height char-height * >= or if
        out-of-range-color exit
      then
      x char-width u/mod { font-x char-x }
      y char-height u/mod { font-y char-y }
      font-y char-height 1- = if
        char-x char-y display underlined@ if
          char-x char-y display fg-color@ exit
        then
      then
      char-x char-y display char@ font-x font-y our-font raw-char-pixel@ if
        char-x char-y display fg-color@
      else
        char-x char-y display bk-color@
      then
    ;

    \ Write a single RLE-compressed pixel
    : write-pixel
      { offset solid? last-color display writer buffer x y }
      ( -- offset' solid?' last-color' )
      x y display pixel@ dup { this-color } buffer offset + c!
      -1 last-color = if this-color to last-color then
      1 +to offset
      false { written? }
      
      solid? this-color last-color <> and if
        offset 3 > if
          \ Write pixels of the same color and reset the differing pixel
          \ to be the start of a new solid sequence of pixels
          offset 1- writer c>writer
          last-color writer c>writer
          this-color buffer c!
          1 to offset
          true to solid?
          this-color to last-color
          true to written?
        else
          \ Simply set the sequence of pixels to not be solid
          false to solid?
        then
      then
      
      written? not if
        solid? not if
          offset 3 > if
            buffer offset 2 - + c@ this-color =
            buffer offset 3 - + c@ this-color = and if
              offset 5 > if
                \ Write a sequence of pixels in absolute mode
                0 writer c>writer
                offset 3 - writer c>writer
                buffer offset 3 - writer buffer>writer
                offset 3 - 1 and if 0 writer c>writer then \ Padding
              else
                offset 5 = if
                  \ Write two pixels
                  1 writer c>writer
                  buffer c@ writer c>writer
                  1 writer c>writer
                  buffer 1+ c@ writer c>writer
                else
                  \ Write a single pixel
                  1 writer c>writer
                  buffer c@ writer c>writer
                then
              then
              \ Set the sequence to be a solid sequence of three pixels
              buffer 3 this-color fill
              3 to offset
              true to solid?
              this-color to last-color
              true to written?
            then
          then
        then
      then
      
      written? not if
        offset 255 = if
          solid? if
            \ Write 255 pixels of the same color
            255 writer c>writer
            last-color writer c>writer
          else
            \ Write 255 pixels in absolute mode
            0 writer c>writer
            255 writer c>writer
            buffer 255 writer buffer>writer
            0 writer c>writer \ Padding
          then
          \ Reset as an empty sequence of pixels
          0 to offset
          true to solid?
          -1 to last-color
        else
        then
      then
      
      offset solid? last-color
    ;
          
    \ Write RLE-compressed pixels
    : write-pixels ( display writer -- )
      255 [: { display writer buffer }
        term-pixels-dim@ { display-width display-height }
        0 { offset }
        true { solid? }
        -1 { last-color }
        display-width 0= display-height 0= or if
          \ Exit early
          $0000 writer h>writer exit
        then
        0 display-height 1- ?do
          display-width 0 ?do
            offset solid? last-color display writer buffer i j
            write-pixel to last-color to solid? to offset
          loop

          offset 0> if
            solid? if
              \ Complete the line with pixels of the same color
              offset writer c>writer
              last-color writer c>writer
            else
              offset 2 > if
                \ Complete the line with pixels in absolute mode
                0 writer c>writer
                offset writer c>writer
                buffer offset writer buffer>writer
                offset 1 and if 0 writer c>writer then \ Padding
              else
                offset 2 = if
                  \ Complete the line with two pixels
                  1 writer c>writer
                  buffer c@ writer c>writer
                  1 writer c>writer
                  buffer 1+ c@ writer c>writer
                else
                  \ Complete the line with a single pixel
                  1 writer c>writer
                  buffer c@ writer c>writer
                then
              then
            then
          then
          
          $0000 writer h>writer \ End of line

          0 to offset
          true to solid?
          -1 to last-color

\          [: ." end line " flush-console ;] console::with-serial-output
        -1 +loop

        $0100 writer h>writer \ End of bitmap

\        [: ." end data " flush-console ;] console::with-serial-output
      ;] with-allot
    ;
    
  end-module> import

  \ Take a screenshot
  : take-screenshot ( path-addr path-bytes fs -- )
    <file-writer> [:
      [: { path-addr path-bytes fs writer display }
        path-addr path-bytes writer fs open-next-screenshot
        term-pixels-dim@ { display-width display-height }
        $4D42 writer h>writer
        0 writer >writer \ File size to be filled in later
        0 writer h>writer
        0 writer h>writer
        data-offset writer >writer \ Data offset
        40 writer >writer \ BITMAPINFOHEADER size
        display-width writer >writer
        display-height writer >writer
        1 writer h>writer \ Number of planes
        8 writer h>writer \ Number of bits per pixel
        BI_RLE8 writer >writer \ The compression method
        0 writer >writer \ The raw image data size, to be filled in later
        display-width display-height max s>f screen-edge-len f/ round-zero
        dup writer >writer \ The horizontal resolution in pixels per meter
        writer >writer \ The vertical resolution in pixels per meter
        0 writer >writer \ Default palette size of 2^n
        0 writer >writer \ Number of 'important' colors
        writer write-color-table
        display writer write-pixels
        writer close-writer
      ;] with-term-display
    ;] with-object
  ;

  continue-module picocalc-screenshot-internal

    \ The filesystem to use for screenshots
    variable screenshot-fs

    \ The pathname to use for screenshots
    256 buffer: screenshot-path

  end-module

  \ Set the screenshot filesystem
  : screenshot-fs! ( fs -- ) screenshot-fs ! ;

  \ Get the screenshot filesystem
  : screenshot-fs@ ( -- fs ) screenshot-fs @ ;

  \ Path too long exception
  : x-path-too-long ( -- ) ." path too long" cr ;

  \ Set the screenshot path
  : screenshot-path! { path-addr path-bytes -- }
    path-bytes 256 u< averts x-path-too-long
    path-bytes screenshot-path c!
    path-addr screenshot-path 1+ path-bytes move
  ;
  
  \ Get the screenshot path
  : screenshot-path@ ( -- path-addr path-bytes ) screenshot-path count ;
  
  continue-module picocalc-screenshot-internal
    
    \ Initialize taking screenshot
    : init-screenshot ( -- )
      s" /SCREEN" screenshot-path!
      [ defined? sd-fs: ] [if]
        fat32-tools::sd-fs@ screenshot-fs!
      [else]
        [ defined? psram-fs: ] [if]
          fat32-tools::psram-fs@ screenshot-fs!
        [else]
          [ defined? blocks-fs: ] [if]
            fat32-tools::blocks-fs@ screenshot-fs!
          [else]
            0 screenshot-fs!
          [then]
        [then]
      [then]
      [:
        [:
          screenshot-fs@ { fs }
          fs if
            screenshot-path@ fs ['] take-screenshot try-and-display-error 0<> if
              drop 2drop
            then
          then
        ;] console::with-serial-error-output
      ;] screenshot-hook!
    ;
    initializer init-screenshot

  end-module
  
end-module
