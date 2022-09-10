#include src/common/forth/block_dev.fs
#include src/common/forth/sdcard.fs
#include src/common/forth/fat32.fs

begin-module fat32-test

  oo import
  block-dev import
  sd import
  spi import
  pin import
  fat32 import
  
  <sd> class-size buffer: my-sd
  <mbr> class-size buffer: my-mbr
  <partition> class-size buffer: my-partition
  <fat32-fs> class-size buffer: my-fs
  <fat32-dir> class-size buffer: my-dir
  <fat32-dir> class-size buffer: my-dir-1
  <fat32-entry> class-size buffer: my-entry
  <fat32-file> class-size buffer: my-file
  
  384 constant my-read-size
  my-read-size buffer: my-read-buffer
  
  16 constant small-read-size
  
  0 constant my-spi
  
  : init-test ( -- )
    init-fat32
    my-spi 2 spi-pin
    my-spi 3 spi-pin
    my-spi 4 spi-pin
    5 output-pin
    6 2 ?do i pull-up-pin loop
    5 my-spi <sd> my-sd init-object
    my-sd init-sd
    true my-sd write-through!
    my-sd <mbr> my-mbr init-object
    <partition> my-partition init-object
    my-partition 0 my-mbr partition@
    my-partition partition-first-sector @ .
    my-partition my-sd <fat32-fs> my-fs init-object
  ;
  
  : ls-root ( -- )
    <fat32-entry> my-entry init-object
    my-dir my-fs root-dir@
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
  ;
  
  : create-file ( data-addr data-u name-addr name-u )
    my-dir my-fs root-dir@
    my-file my-dir [ fat32 ] :: create-file
    my-file write-file
  ;
  
  : create-dir ( name-addr name-u )
    my-dir my-fs root-dir@
    my-dir-1 my-dir [ fat32 ] :: create-dir
  ;

  : ls ( name-addr name-u -- )
    <fat32-entry> my-entry init-object
    my-dir my-fs root-dir@
    my-dir-1 my-dir open-dir
    begin
      my-entry my-dir-1 read-dir if
        12 [:
          12 my-entry file-name@
          cr type space false
        ;] with-allot
      else
        true
      then
    until
  ;
  
  : cat ( name-addr name-u -- )
    cr
    my-dir my-fs root-dir@
    my-file my-dir open-file
    begin
      my-read-buffer my-read-size my-file read-file dup 0> if
        my-read-buffer swap type false
      else
        drop true
      then
    until
  ;
  
  : cat2 ( name-addr name-u dir-addr dir-u -- )
    cr
    my-dir my-fs root-dir@
    my-dir-1 my-dir open-dir
    my-file my-dir-1 open-file
    begin
      my-read-buffer my-read-size my-file read-file dup 0> if
        my-read-buffer swap type false
      else
        drop true
      then
    until
  ;
  
  : remove-file ( name-addr name-u -- )
    my-dir my-fs root-dir@
    my-dir remove-file
  ;
  
  : remove-dir ( name-addr name-u -- )
    my-dir my-fs root-dir@
    my-dir remove-dir
  ;
  
  : rename ( new-name-addr new-name-u name-addr name-u -- )
    my-dir my-fs root-dir@
    my-dir rename
  ;
  
  : seek-test ( -- )
    my-dir my-fs root-dir@
    s" TEST.TXT" my-file my-dir open-file
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
  ;

end-module
