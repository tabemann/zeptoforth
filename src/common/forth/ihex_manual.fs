\ Program Name: ihex.fs
\ Date: Sun 5 Jan 2020 21:19:51 AEDT
\ Copyright 2020 by t.j.porter <terry@tjporter.com.au>, licensed under the GPLV2
\ Copyright 2020 by Travis Bemann
\ Ported to zeptforth by Travis Bemann
\ https://github.com/tabemann/zeptoforth
\ Originally for Mecrsp-Stellars by Matthas Koch.
\ https://sourceforge.net/projects/mecrisp/
\ MCU: STM32Fxxx
\ This Forth Program : Dumps a iHex image of the target MCU dictionary to the
\ terminal where it can be captured in a terminal log and turned into a 
\ bootable binary file clone of the target. This relies on a user-provided
\ size rather than reading the size from the MCU, unlike ihex.fs
\ -----------------------------------------------------------------------------
\
\     Usage:    "flash-size clone"
\
\     'flash-size' = "Flash size in bytes (must be a multiple of 16)"
\
\     'clone' name of the program as listed below in the source
\
\     Example usage for:
\     STM32F051:     "1 $1FFFF7CC clone"
\     STM32F103CB:   "1 $1FFFF7E0 clone"
\     STM32F103C8:   "2 $1FFFF7E0 clone" 
\ 
\ 
\ The iHex dump starts with the word "clone" and ends with the word "clone_end"
\ for easy parsing to remove any extraneous text before using
\ 'arm-none-eabi-objcopy'.
\ 
\ Once you have the iHex File process it as below.
\
\ " arm-none-eabi-objcopy -I ihex -O binary <your-iHex-logfile> <binaryfile-name.bin> "
\ -----------------------------------------------------------------------------
\ Notes:
\ Can produce extended iHex files up to 32 bits, i.e. 4 GB.
\ iHex Extended Addressing type 04 is used
\ -------------------- Shouldn't need to change anything below ----------------

 : erased? ( b-addr -- ) \ Check Flash bytes NOT erased.
   0                     \ Flag of 0 = erased byte, ignore data afterwards
   over 16 + rot do      \ Scan data
      i c@ $FF <> or     \ Set flag if there is a non-$FF byte. Some chips have Flash = $00 as erased.
   loop
 ; 

 : ea04generate ( -- print 04 Extended Address Type record )
   ." :02000004"              \ Write 04 Extended Address Type record preamble
   dup h.4                    \ quotient = LBA
   $06 + not $FF and 1+      \ calculate checksum. $06 is record preamble value.
   h.2  cr                    \ print checksum
 ;

 : insert.04ea? ( 32 bit hex address  -- 04 Extended Address iHex Type records )
   dup $10000 >= IF               \ eliminate addresses under 64kB  ($10000)
     dup 16 rshift swap $FFFF and \ quotient is TOS, then remainder
     0= IF                 \ only want values where there is no remainder
       ea04generate        \ quotient passed to ea04generate as LBA
     ELSE
       drop                \ drop unwanted non zero remainder
     THEN
   ELSE
     drop
   THEN
 ;

 : clone ( flash-size -- )
   cr      \ Dumps a bootable Flash Image (core + all words)
   0 do   
     i erased?
     if
       i insert.04ea?  
       ." :10" i h.4 ." 00"    \ Write record-intro with 4 digits
       $10                      \ Begin checksum
       i          $FF and +      \ Sum the address bytes 
       i 8 rshift $FF and +       \ separately into the checksum
       i 16 + i do                 \ EOL               
	 i c@ h.2                  \ Print data with 2 digits
	 i c@ +                     \ Sum it up for Checksum
       loop                  
       negate h.2                      \ Write Checksum
       cr                         
     then                                 \ End of Flash or area of erased bytes ( $FFFF )    
   16 +loop 
   ." :00000001FF "                             \ iHex terminator
   ." clone_end " cr                             \ clone.sh search log terminator
 ;
