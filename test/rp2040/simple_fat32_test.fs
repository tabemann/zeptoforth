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

begin-module fat32-test

  oo import
  block-dev import
  sd import
  spi import
  pin import
  fat32 import
  simple-fat32 import
  
  <simple-fat32-fs> class-size buffer: my-fs
  <fat32-dir> class-size buffer: my-dir
  <fat32-dir> class-size buffer: my-dir-1
  <fat32-entry> class-size buffer: my-entry
  <fat32-file> class-size buffer: my-file
  
  384 constant my-read-size
  my-read-size buffer: my-read-buffer
  
  16 constant small-read-size
  
  0 constant my-spi
  
  : init-test ( -- )
    2 3 4 5 0 <simple-fat32-fs> my-fs init-object
    true my-fs write-through!
  ;
  
  
  : ls-root ( -- )
    <fat32-entry> my-entry init-object
    my-dir my-fs root-dir@
    [:
      begin
        my-entry my-dir read-dir if
          12 [:
            12 my-entry file-name@
            cr type space false
          ;] with-allot
        else
          true
        then
      until
    ;] try
    my-dir close-dir
    my-dir destroy
    ?raise
  ;
  
  : create-file ( data-addr data-u name-addr name-u )
    [: my-file swap create-file ;] my-fs with-root-path
    [:
      my-file write-file
    ;] try
    my-file close-file
    my-file destroy
    ?raise
  ;
  
  : create-dir ( name-addr name-u )
    [: my-dir swap create-dir ;] my-fs with-root-path
  ;

  : ls ( name-addr name-u -- )
    [: my-dir swap open-dir ;] my-fs with-root-path
    [:
      begin
        my-entry my-dir read-dir if
          12 [:
            12 my-entry file-name@
            cr type space false
          ;] with-allot
        else
          true
        then
      until
    ;] try
    my-dir close-dir
    my-dir destroy
    ?raise
  ;
  
  : cat ( name-addr name-u -- )
    cr
    [: my-file swap open-file ;] my-fs with-root-path
    [:
      begin
        my-read-buffer my-read-size my-file read-file dup 0> if
          my-read-buffer swap type false
        else
          drop true
        then
      until
    ;] try
    my-file close-file
    my-file destroy
    ?raise
  ;
  
  : remove-file ( name-addr name-u -- )
    ['] remove-file my-fs with-root-path
  ;
  
  : remove-dir ( name-addr name-u -- )
    ['] remove-dir my-fs with-root-path
  ;
  
  : rename ( new-name-addr new-name-u name-addr name-u -- )
    ['] rename my-fs with-root-path
  ;
  
  : seek-test ( -- )
    my-dir my-fs root-dir@
    [:
      s" TEST.TXT" my-file my-dir open-file
      [:
        cr ." Size : " my-file file-size@ .
        my-file tell-file cr . ." : "
        my-read-buffer small-read-size my-file read-file
        my-read-buffer swap type
        256 seek-cur my-file seek-file
        my-file tell-file cr . ." : "
        my-read-buffer small-read-size my-file read-file
        my-read-buffer swap type
        -256 seek-end my-file seek-file
        my-file tell-file cr . ." : "
        my-read-buffer my-read-size my-file read-file
        my-read-buffer swap type
        256 seek-set my-file seek-file
        my-file tell-file cr . ." : "
        my-read-buffer small-read-size my-file read-file
        my-read-buffer swap type
      ;] try
      my-file close-file
      my-file destroy
      ?raise
    ;] try
    my-dir close-dir
    my-dir destroy
    ?raise
  ;
  
  : create-big-file ( name-addr name-u -- )
    false my-sd write-through!
    [: my-file swap fat32::create-file ;] my-fs with-root-path
    [:
      $10000 0 ?do key? if key drop leave then
        hex i 0 <# # # # # # # # # #> decimal my-file write-file drop
        i $FF and 0= if i h.8 then
      loop
    ;] try
    my-file close-file
    my-file destroy
    true my-sd write-through!
    ?raise
  ;
  
  : create-big-binary-file ( name-addr name-u -- )
    false my-sd write-through!
    [: my-file swap fat32::create-file ;] my-fs with-root-path
    [:
      $10000 0 ?do key? if key drop leave then
        i 256 cells [: { index buffer }
          index 256 + index ?do i buffer i 255 and cells + ! loop
          buffer 256 cells my-file write-file drop
        ;] with-aligned-allot
        i h.8
      256 +loop
    ;] try
    my-file close-file
    my-file destroy
    true my-sd write-through!
    ?raise
  ;
  
  : create-many-big-binary-files { start -- }
    false my-sd write-through!
    [:
      100000000 start ?do key? if key drop leave then
        i 12 [: { file-index name-buffer }
          file-index 0 <# # # # # # # # # #> name-buffer swap move
          s" .BIN" name-buffer 8 + swap move
          cr name-buffer 12 type ." : "
          name-buffer 12 [: my-file swap fat32::create-file ;]
          my-fs with-root-path
          [:
            $10000 0 ?do
              i 256 cells [: { index buffer }
                index 256 + index ?do i buffer i 255 and cells + ! loop
                buffer 256 cells my-file write-file drop
              ;] with-aligned-allot
              i h.8
            256 +loop
          ;] try
          my-file close-file
          my-file destroy
          ?raise
        ;] with-aligned-allot
      loop
    ;] try
    true my-sd write-through!
    ?raise
  ;
  
  : create-one-big-binary-file { start -- }
    false my-sd write-through!
    [:
      begin key? not while
        start 12 [: { file-index name-buffer }
          file-index 0 <# # # # # # # # # #> name-buffer swap move
          s" .BIN" name-buffer 8 + swap move
          cr name-buffer 12 type ." : "
          name-buffer 12 [: my-file swap fat32::create-file ;]
          my-fs with-root-path
          [:
            $10000 0 ?do
              i 256 cells [: { index buffer }
                index 256 + index ?do i buffer i 255 and cells + ! loop
                buffer 256 cells my-file write-file drop
              ;] with-aligned-allot
              i h.8
            256 +loop
            my-fs flush
          ;] try
          my-file close-file
          my-file destroy
          ?raise
          name-buffer 12 ['] fat32::remove-file my-fs with-root-path
          my-fs flush
        ;] with-aligned-allot
      repeat
    ;] try
    true my-sd write-through!
    ?raise
  ;

  : create-many-small-binary-files { start -- }
    false my-sd write-through!
    [:
      100000000 start ?do key? if key drop leave then
        i 12 [: { file-index name-buffer }
          file-index 0 <# # # # # # # # # #> name-buffer swap move
          s" .BIN" name-buffer 8 + swap move
          cr name-buffer 12 type ." : "
          name-buffer 12 [: my-file swap fat32::create-file ;]
          my-fs with-root-path
          [:
            s" QUUX" my-file write-file drop
          ;] try
          my-file close-file
          my-file destroy
          ?raise
        ;] with-aligned-allot
      loop
    ;] try
    true my-sd write-through!
    ?raise
  ;

end-module
