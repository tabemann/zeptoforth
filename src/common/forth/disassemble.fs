\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ Compile this to flash
compile-to-flash

\ Begin compressing compiled code in flash
compress-flash

\ Type a condition
: (cond.) ( cond -- )
  drop
;

\ Get whether the condition is null
: null-cond? ( cond -- f ) 0< ;

\ Type an unsigned decimal value
: (udec.) ( u -- ) base @ >r decimal (u.) r> base  ! ;

\ Type a register
: reg. ( u -- ) ." R" (udec.) ;

\ Type a value
: val. ( u -- ) ." $" base @ >r hex (u.) r> base ! ;

\ Separator
: sep. ( -- ) ." , " ;

\ Separator with immediate
: sep-imm. ( -- ) ."  , #" ;

\ Type a 16-bit instruction in halfwords
: instr16. ( h -- ) h.4 ." :      " ;

\ Type a 32-bit instruction in halfwords
: instr32. ( low high -- ) swap h.4 space h.4 ." : " ;

\ Sign extend a value of a certain size in bits
: extend ( u bits -- ) 32 swap - tuck lshift swap arshift ;

\ Match 16-bit
: match16 ( u -- u f ) 2+ true ;

\ Match 32-bit
: match32 ( u -- u f ) 4+ true ;

\ Not match 16-bit
: not-match16 ( u u -- f ) 2drop false ;

\ Not match 32-bit
: not-match32 ( u u u -- f ) drop 2drop false ;

\ Rotate a 7-bit value encoded within a 12-bit value
: rotate7-in-12 ( u -- u )
  dup %1111111 and
  2dup swap rshift
  -rot
  dup 7 rshift %11111 and 32 swap - lshift
  or
;

\ Decode an imm12 constant
: decode-const12 ( u -- )
  %111111111111 and
  dup 10 rshift 0 = if
    dup 8 rshift case
      %00 of (u.) endof
      %01 of
	dup %11111111 and 0 = if
	  ." UNPREDICTABLE" drop
	else
	  dup 16 lshift val.
	then
      endof
      %10 of
	dup %11111111 and 0 = if
	  ." UNPREDICTABLE" drop
	else
	  dup 24 lshift swap 8 lshift or val.
	then
      endof
      %11 of
	dup %11111111 and 0 = if
	  ." UNPREDICTABLE" drop
	else
	  dup 24 lshift over 16 lshift or over 8 lshift or or val.
	then
      endof
    endcase
  else
    rotate7-in-12 val.
  then
;

\ Decode an immediate shift
: decode-imm-shift ( type u -- )
  swap case
    %00 of
      ?dup if
	." , LSL #" (udec.)
      then
    endof
    %01 of
      ?dup if
	." , LSR #" (udec.)
      else
	." , LSR #32"
      then
    endof
    %10 of
      ?dup if
	." , ASR #" (udec.)
      else
	." , ASR #32"
      then
    endof
    %11 of
      ?dup if
	." , ROR #" (udec.)
      else
	." , RRX #1"
      then
    endof
  endcase
;

\ Make a mask for a set number of bits
: bits ( bits -- mask ) $FFFFFFFF 32 rot - rshift ;

\ Extract a field
: bitfield ( data shift bits -- ) -rot rshift swap bits and ;

\ Empty halfword 2 test
: empty2? ( data -- f ) %1000000000000000 and %0000000000000000 = ;

\ Print out a condition if one is specified, otherwise do nothing
: cond. ( cond -- )
  dup null-cond? if
    (cond.)
  else
    drop
  then
;

\ Print out a condition if one is specified, otherwise do print out 'S'
: conds. ( cond -- )
  dup null-cond? if
    (cond.)
  else
    drop ." S"
  then
;

\ Print out 'S' if a bit is set
: s?. ( low shift -- ) 1 bitfield if ." S" then ;

\ Print out 'S' if bit 4 is set
: 4s?. ( low -- ) 4 s?. ;

\ Type a PC-relative address
: rel. ( pc rel extend -- ) swap 2 + 1 lshift swap 1 + extend + val. ;

\ Type a non-sign-extended PC-relative address
: nrel. ( pc rel -- ) swap 2 + 1 lshift + val. ;

\ 0 3 bitfield
: 0_3_bf ( data -- field ) 0 3 bitfield ;

\ 3 3 bitfield
: 3_3_bf ( data -- field ) 3 3 bitfield ;

\ 8 4 bitfield
: 8_4_bf ( data -- field ) 8 4 bitfield ;

\ 0 4 bitfield
: 0_4_bf ( data -- field ) 0 4 bitfield ;

\ 0 8 bitfield
: 0_8_bf ( data -- field ) 0 8 bitfield ;

\ 12 4 bitfield
: 12_4_bf ( data -- field ) 12 4 bitfield ;

\ 10 1 bitfield
: 10_1_bf ( data -- field ) 10 1 bitfield ;

\ 4 2 bitfield
: 4_2_bf ( data -- field ) 4 2 bitfield ;

\ 6 2 bitfield
: 6_2_bf ( data -- field ) 6 2 bitfield ;

\ 12 3 bitfield
: 12_3_bf ( data -- field ) 12 3 bitfield ;

\ 6 3 bitfield
: 6_3_bf ( data -- field ) 6 3 bitfield ;

\ Decode a 16-bit and register
: decode-and-reg-16 ( low -- )
  dup 0_3_bf reg. sep.
  3_3_bf reg.
;

\ Decode a 32-bit ADD immediate or ADC immediate instruction
: decode-add-imm-32 ( low high -- )
  dup 8_4_bf reg. sep.
  over 0_4_bf reg. sep-imm.
  dup 0_8_bf swap 12_4_bf 8 lshift or
  swap 10_1_bf 11 lshift or decode-const12
;

\ Decode the second kind of 32-bit ADD immediate or ADC immediate instruction
: decode-add-imm-32-2 ( low high --- )
  dup 8_4_bf reg. sep.
  swap 0_4_bf reg. sep.
  dup 0_4_bf reg.
  dup 4_2_bf
  over 6_2_bf rot 12_3_bf 2 lshift orr and decode-imm-shift
;

\ Parse an ADC immediate instruction
: parse-adc-imm ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111101111100000 and %1111000101000000 = if
    2 pick 2+ h@ dup empty2? if
      dup instr16. ." ADC" over 4s?. rot cond. space decode-add-imm-32
      match32
    else
      not-match32
    then
  else
    2not-match16
  then
;

\ Parse an ADC register instruction
: parse-adc-reg ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111111111000000 and %0100000101000000 = if
    dup instr16. ." ADC" swap conds. space
    decode-and-reg-16
    match16
  else
    dup %1111111111100000 and %1110101101000000 = if
      2 pick 2+ h@
      dup instr16. ." ADC" over 4s?. rot cond. ." .W "
      decode-add-reg-32
      match32
    else
      2not-match16
    then
  else
    2not-match16
  then
;

\ Parse an ADD immediate instruction
: parse-add-imm ( h-addr1 cond - h-addr2 f )
  over h@
  dup %1111111000000000 and %0001110000000000 = if
    dup instr16. ." ADD"
    swap conds.
    dup 0_3_bf reg. sep.
    dup 3_3_bf reg. sep-imm.
    6_3_bf val.
    match16
  else
    dup %1111100000000000 and %0011000000000000 = if
      dup instr16. ." ADD" swap conds. space
      dup 8 3 bitfield reg. sep-imm.
      0_8_bf val.
      match16
    else
      dup %1111101111100000 and %1111000100000000 = if
	2 pick 2+ h@ dup empty2? if
	  2dup instr32. ." ADD" over 4s?. rot cond. ." .W "
	  decode-add-imm-32
	  match32
	else
	  not-match32
	then
      else
	dup %1111101111110000 and %1111001000000000 = if
	  2 pick 2+ h@ dup empty2? if
	    dup instr32. ." ADDW" rot cond. space
	    dup 8_4_bf reg. sep.
	    over 0_4_bf reg. sep-imm.
	    dup 0_8_bf swap 12_4_bf 8 lshift or
	    swap 10_1_bf 11 lshift or val.
	    4 + true
	  else
	    not-match32
	  then
	else
	  2not-match16
	then
      then
    then
  then
;

\ Parse an ADD register instruction
: parse-add-reg ( h-addr1 cond h-addr2 f )
  over h@
  dup %1111111000000000 and %0001100000000000 = if
    dup instr16. ." ADD" swap conds.space
    dup 0_3_bf reg. sep.
    dup 3_3_bf reg. sep.
    dup 6_3_bf reg.
    match16
  else
    dup %1111111100000000 and %0100010000000000 = if
      dup instr16. ." ADD" swap cond. space
      dup 0_3_bf over 7 1 bitfield 3 lshift or reg. sep.
      3 4 bitfield reg.
      match16
    else
      dup %1111111111100000 and %1110101100000000 = if
	2 pick 2+ h@
	2dup instr32. ." ADD" over 4s?. rot cond. ." .W "
	decode-add-reg-32
	match32
      else
	2not-match16
      then
    then
  then
;

\ Parse an ADD SP to immediate instruction
: parse-add-sp-imm ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111100000000000 and %1010100000000000 = if
    dup instr16. ." ADD" swap cond. space
    dup 8 3 bitfield reg. ." , SP, #"
    0_8_bf val.
    match16
  else
    dup %1111111110000000 and %1011000000000000 = if
      dup instr16. ." ADD" swap cond. space ." SP, SP, #"
      0 7 bitfield val.
      match16
    else
      dup %1111101111101111 and %1111000100001101 = if
	over 2+ h@ dup empty2? if
	  2dup instr32. ." ADD" over 4s?. rot cond. space
	  dup 8_4_bf reg. ." , SP, #"
	  dup 0_8_bf swap 12_4_bf 8 lshift or
	  swap 10_1_bf 11 lshift or decode-const12
	  match32
	else
	  not-match32
	then
      else
	dup %1111101111111111 and %1111001000001101 = if
	  over 2+ h@ dup empty2? if
	    dup instr32. ." ADDW" rot cond. space
	    dup 8_4_bf reg. ." , SP, #"
	    dup 0_8_bf swap 12_4_bf 8 lshift or
	    swap 10_1_bf 11 lshift or val.
	    match32
	  else
	    not-match32
	  then
	else
	  2not-match16
	then
      then
    then
  then
;

\ Parse an ADD SP to register instruction
: parse-add-sp-reg ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111111101111000 and %0100010001101000 = if
    dup instr16. ." ADD" swap cond. space
    dup 0_3_bf over 7 1 bitfield 3 lshift or dup reg. ." , SP, "
    reg.
    match16
  else
    dup %1111111110000111 and %0100010010000101 = if
      dup instr16. ." ADD" swap cond. space ." SP, "
      2 4 bitfield reg.
      match16
    else
      dup %1111111111101111 and %1110101100001101 = if
	over 2+ h@ dup empty2? if
	  2dup instr32. ." ADD" swap 4s?. swap cond. ." W "
	  dup 8_4_bf reg. ." , SP, "
	  dup 0_4_bf reg.
	  dup 4_2_bf
	  over 6_2_bf rot 12_3_bf lshift 2 or decode-imm-shift
	  match32
	else
	  not-match32
	then
      else
	2not-match16
      then
    then
  then
;


\ Parse an ADR instruction
: parse-adr ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %111100000000000 and %1010000000000000 = if
    dup instr16. ." ADD" swap cond. space
    dup 8 3 bitfield reg. ." , PC, #"
    0_8_bf 2 lshift val.
    match16
  else
    dup %1111101111111111 and %1111001010101111 = if
      over 2+ h@ empty2? if
	2dup instr32. ." SUB" rot cond. space
	dup 8_4_bf reg. ." , PC, #"
	dup 0_8_bf swap 12_3_bf 8 lshift or
	swap 10_1_bf 12 lshift or val.
	match32
      else
	not-match32
      then
    else
      dup %1111101111111111 and %1111001000001111 = if
	over 2+ h@ empty2? if
	  2dup instr32. ." ADD" rot cond. space
	  dup 8_4_bf reg. ." , PC, #"
	  dup 0_8_bf swap 12_3_bf 8 lshift or
	  swap 10_1_bf 12 lshift or val.
	  match32
	else
	  not-match32
	then
      else
	2not-match16
      then
    then
  then
;

\ Parse an AND immediate instruction
: parse-and-imm ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111101111100000 and %1111000000000000 = if
    over 2+ h@ dup empty2? if
      2dup instr32. ." AND" over 4s?. rot cond. space
      dup 8_4_bf reg. sep.
      over 0_4_bf reg. sep-imm.
      dup 0_8_bf swap 12_3_bf 8 lshift or
      swap 10_1_bf 12 lshift or decode-const12
      match32
    else
      not-match32
    then
  else
    2not-match16
  then
;

\ Parse an AND register instruction
: parse-and-reg ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111111111000000 and %0100000000000000 = if
    dup instr16. ." AND" swap conds. space
    decode-and-reg
    match16
  else
    dup %1111111111100000 and %1110101000000000 = if
      over 2+ h@
      2dup instr32. ." AND" over 4s?. rot cond. ." .W "
      dup 8_4_bf reg. sep.
      swap 0_4_bf reg. sep.
      dup 0_4_bf reg.
      dup 4_2_bf
      over 6_2_bf rot 12_3_bf 2 lshift or decode-imm-shift
      match32
    else
      2not-match16
    then
  then
;

\ Parse an ASR immediate instruction
: parse-asr-imm ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111100000000000 and %0001000000000000 = if
    dup instr16. ." ASR" swap conds. space
    dup 0_3_bf reg. sep.
    dup 3_3_bf reg. sep-imm.
    6 5 bitfield val.
    match16
  else
    dup %1111111111101111 and %1110101001001111 = if
      over 2+ h@ dup %0000000000110000 and %00000000000100000 = if
	2dup instr32. ." ASR" swap 4s?. swap cond. ." .W "
	dup 8_4_bf reg. sep.
	dup 0_4_bf reg. sep.
	%10 over 6_2_bf rot 12_3_bf 2 lshift or decode-imm-shift
	match32
      else
	not-match32
      then
    else
      2not-match16
    then
  then
;

\ Parse an ASR register instruction
: parse-asr-reg ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111111111000000 and %0100000100000000 = if
    dup instr16. ." ASR"
    swap conds. space
    dup 0_3_bf reg. sep.
    3_3_bf reg.
    match16
  else
    dup %1111111111100000 and %1111101001000000 = if
      over 2+ h@ dup %1111000011110000 and %1111000000000000 = if
	2dup instr32. ." ASR" over 4s?. rot cond. ." .W "
	dup 8_4_bf reg. sep.
	swap 0_4_bf reg. sep.
	0_4_bf reg.
	4+ reg
      else
	not-match32
      then
    else
      2not-match16
    then
  then
;

\ Parse a B instruction
: parse-b ( h-addr1 cond -- h-addr2 f )
  over h@
  dup %1111000000000000 and %1101000000000000 = if
    dup instr16. ." B" nip dup 8_4_bf cond. space ." #"
    0_8_bf over swap 8 rel.
    match16
  else
    dup %1111100000000000 and %1110000000000000 = if
      dup instr16. ." B" swap cond. space ." #"
      0 11 bitfield over swap 11 rel.
      match16
    else
      dup %1111100000000000 and %1111000000000000 = if
	over 2+ h@ dup %1101000000000000 and %1000000000000000 = if
	  2dup instr32. ." B" rot drop over 6 4 bitfield cond. ." .W #"
	  dup 0 11 bitfield 2 pick 0 6 bitfield 11 lshift or
	  over 13 1 bitfield 17 lshift or swap 11 1 bitfield 18 lshift or
	  swap 10_1_bf 19 lshift or over swap 20 rel.
	  match32
	else
	  dup %1101000000000000 and %1001000000000000 = if
	    2dup instr32. ." B" rot cond. ." .W #"
	    dup 0 11 bitfield 2 pick 0 10 bitfield 11 lshift or
	    over 11 1 bitfield 3 pick 10_1_bf xor 21 lshift or
	    swap 13 1 bitfield 2 pick 10_1_bf xor 22 lshift or
	    swap 10_1_bf 23 lshift or over swap 24 rel
	    match32
	  else
	    not-match32
	  then
	then
      else
	not-match16
      then
    then
  then
;

\ Parse a BFC instruction
: parse-bfc ( h-addr1 cond -- h-addr2 flag )
  over h@
  dup %1111101111111111 and %1111001101101111 = if
    over 2+ h@ dup empty2? if
      2dup instr32. ." BFC" rot cond. space nip
      dup 8_4_bf reg. sep-imm.
      dup 6_2_bf over 12_3_bf 2 lshift or dup val. sep-imm.
      over 0 5 bitfield 1+ swap - val.
      match32
    else
      not-match32
    then
  else
    not-match16
  then
;

\ Parse a BFI instruction
: parse-bfi ( h-addr1 cond -- h-addr2 flag )
  over h@
  dup %1111101111110000 and %1111001101100000 = if
    over 2+ h@ dup empty2? if
      2dup instr32. ." BFI" rot cond. space
      dup 8_4_bf reg. sep.
      swap 0_4_bf reg. sep-imm.
      dup 6_2_bf over 12_3_bf 2 lshift or dup val. sep-imm.
      over 0 5 bitfield 1+ swap - val.
      match32
    else
      not-match32
    then
  else
    not-match16
  then
;

\ Parse a BIC immediate instruction
: parse-bic-imm ( h-addr1 cond -- h-addr2 flag )
  over h@
  dup %11111000111100000 and %1111000000100000 = if
    over 2+ h@ dup empty2? if
      2dup instr32. ." BIC" over 4s?. space
      decode-add-imm-32
      match32
    else
      not-match32
    then
  else
    not-match16
  then
;

\ Parse a BIC register instruction
: parse-bic-reg ( h-addr1 cond -- h-addr2 flag )
  over h@
  dup %1111111111000000 and %0100001110000000 = if
    dup instr16. ." BIC" swap conds. space
    decode-and-reg-16
    match16
  else
    dup %1111111111100000 and %1110101000100000 = if
      over 2+ h@ dup
      2dup instr32. ." BIC" over 4s?. rot cond. ." .W "
      decode-add-reg-32
      match32
    else
      not-match16
    then
  then
;

\ Parse a BKPT instruction
: parse-bkpt ( h-addr1 cond -- h-addr2 flag )
  over h@
  dup %1111111100000000 and %1011111000000000 = if
    dup instr16. ." BKPT" nip 0_8_bf ." #" val. match16
  else
    not-match16
  then
;

\ Parse a BL immediate instruction
: parse-bl-imm ( h-addr1 cond -- h-addr2 flag )
  over h@
  dup %1111100000000000 and %1111000000000000 = if
    over 2+ H@ dup %1101000000000000 and %1101000000000000 = if
      2dup instr32. ." BL" rot cond. space ." #"
      dup 0 11 bitfield
      over 0 10 bitfield 11 lshift or
      dup 11 1 bitfield 2 pick 10 1 bitfield xor invert 1 and 21 lshift or
      13 1 bitfield over 10 1 bitfield xor invert 1 and 22 lshift or
      swap 10 1 bitfield 23 lshift or over swap 24 rel.
      match32
    else
      not-match32
    then
  else
    not-match16
  then
;

\ Parse a BLX register instruction
: parse-blx-reg ( h-addr1 cond h-addr2 flag )
  over h@
  dup %1111111110000000 and %0100011110000000 = if
    dup instr16. ." BLX" swap cond. space
    3 4 bitfield reg.
    match16
  else
    not-match16
  then
;

\ Parse a BX register instruction
: parse-bx ( h-addr1 cond h-addr2 flag )
  over h@
  dup %1111111110000000 and %0100011110000000 = if
    dup instr16. ." BX" swap cond. space
    3 4 bitfield reg.
    match16
  else
    not-match16
  then
;

\ Commit to flash
commit-flash

\ All the words for compilation
create all-words
' parse-adc-imm ,
' parse-adc-reg ,
' parse-add-imm ,
' parse-add-reg ,
' parse-add-sp-imm ,
' parse-add-sp-reg ,
' parse-adr ,
' parse-and-imm ,
' parse-and-reg ,
' parse-asr-imm ,
' parse-asr-reg ,
' parse-b ,
' parse-bfc ,
' parse-bfi ,
' parse-bic-imm ,
' parse-bic-reg ,
' parse-bkpt ,
' parse-bl-imm ,
' parse-blx-reg ,
' parse-bx ,
0 ,

\ Commit to flash
commit-flash

\ Disassemble instructions
: disassemble ( start end -- )
  swap begin
    over u<
  while
    dup h.8 space
    all-words begin
      dup @ 0<> if
	2dup @ execute if
	  nip nip true
	else
	  drop cell+ false
	then
      else
	drop dup h@ instr16. ." ????" 2+ true
      then
    until
    cr
  repeat
  2drop
;

\ Finish compressing the code
end-compress-flash

\ Reboot
reboot