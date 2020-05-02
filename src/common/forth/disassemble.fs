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

\ Find a word in a dictionary by address
: find-dict-by-address ( addr dict -- word|0 )
  begin
    dup 0<> if
      dup >xt 2 pick = if
	true
      else
	prev-word false
      then
    else
      true
    then
  until
  nip
;

\ Commit to flash
commit-flash

\ Find a word by address
: find-by-address ( addr -- word|0 )
  dup ram-latest find-dict-by-address dup 0= if
    drop flash-latest find-dict-by-address
  then
;

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

\ Commit to flash
commit-flash

\ Print out an absolute address
: addr. ( addr -- )
  dup find-by-address ?dup if
    word-name count type space ." <" val. ." >"
  else
    ." #" val.
  then
;

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

\ Extract a field
: bitfield ( data shift bits -- )
  -rot rshift swap $FFFFFFFF 32 rot - rshift and
;

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
: 4s?. ( low -- ) 4 1 bitfield if ." S" then ;

\ Type a PC-relative address
: rel. ( pc rel extend -- ) swap 2 + 1 lshift swap 1 + extend + addr. ;

\ Type a non-sign-extended PC-relative address
: nrel. ( pc rel -- ) swap 2 + 1 lshift + addr. ;

\ Type out .W
: .w ( -- ) ." .W " ;

\ Commit to flash
commit-flash

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

\ Commit to flash
commit-flash

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
: p-adc-imm
  ." ADC" over 4s?. rot cond. space decode-add-imm-32 drop
;

\ Parse an ADC register instruction
: p-adc-reg-1
  ." ADC" swap conds. space decode-and-reg-16 drop
;

\ Parse an ADC register instruction
: p-adc-reg-2
  ." ADC" over 4s?. rot cond. .w decode-add-reg-32 drop
;

\ Parse an ADD immediate instruction
: p-add-imm-1
  ." ADD" swap conds. dup 0_3_bf reg. sep. dup 3_3_bf reg. sep-imm. 6_3_bf val.
  drop
;

\ Parse an ADD immediate instruction
: p-add-imm-2
  ." ADD" swap conds. space dup 8 3 bitfield reg. sep-imm. 0_8_bf val. drop
;

\ Parse an ADD immediate instruction
: p-add-imm-3
  ." ADD" over 4s?. rot cond. .w decode-add-imm-32 drop
;

\ Parse an ADD immediate instruction
: p-add-imm-4
  ." ADDW" rot cond. space
  dup 8_4_bf reg. sep.
  over 0_4_bf reg. sep-imm.
  dup 0_8_bf swap 12_4_bf 8 lshift or swap 10_1_bf 11 lshift or val.
  drop
;

\ Parse an ADD register instruction
: p-add-reg-1
  ." ADD" swap conds. space 0_3_bf reg. sep. dup 3_3_bf reg. sep. 6_3_bf reg.
  drop
;

\ Parse an ADD register instruction
: p-add-reg-2
  ." ADD" swap cond. space
  dup 0_3_bf over 7 1 bitfield 3 lshift or reg. sep. 3 4 bitfield reg.
  drop
;

\ Parse an ADD register instruction
: p-add-reg
  ." ADD" over 4s?. rot cond. .w decode-add-reg-32 drop
;

\ Parse an ADD SP to immediate instruction
: p-add-sp-imm
  ." ADD" swap cond. space dup 8 3 bitfield reg. ." , SP, #" 0_8_bf val. drop
;

\ Parse an ADD SP to immediate instruction
: p-add-sp-imm
  ." ADD" swap cond. space ." SP, SP, #" 0 7 bitfield val. drop
;

\ Parse an ADD SP to immediate instruction
: p-add-sp-imm
  ." ADD" over 4s?. rot cond. space
  dup 8_4_bf reg. ." , SP, #" dup 0_8_bf swap 12_4_bf 8 lshift or
  swap 10_1_bf 11 lshift or decode-const12 drop
;

\ Parse an ADD SP to immediate instruction
: p-add-sp-imm
  ." ADDW" rot cond. space
  dup 8_4_bf reg. ." , SP, #" dup 0_8_bf swap 12_4_bf 8 lshift or
  swap 10_1_bf 11 lshift or val. drop
;

\ Parse an ADD SP to register instruction
: p-add-sp-reg-1
  ." ADD" swap cond. space
  dup 0_3_bf over 7 1 bitfield 3 lshift or dup reg. ." , SP, " reg. drop
;

\ Parse an ADD SP to register instruction
: p-add-sp-reg-2
  ." ADD" swap cond. space ." SP, " 2 4 bitfield reg. drop
;
\ Parse an ADD SP to register instruction
: p-add-sp-reg-3
  ." ADD" swap 4s?. swap cond. .w
  dup 8_4_bf reg. ." , SP, " dup 0_4_bf reg. dup 4_2_bf
  over 6_2_bf rot 12_3_bf lshift 2 or decode-imm-shift drop
;

\ Parse an ADR instruction
: p-adr-1
  ." ADD" swap cond. space dup 8 3 bitfield reg. ." , PC, #"
  0_8_bf 2 lshift val. drop
;

\ Parse an ADR instruction
: p-adr-2
  ." SUB" rot cond. space dup 8_4_bf reg. ." , PC, #"
  dup 0_8_bf swap 12_3_bf 8 lshift or swap 10_1_bf 12 lshift or val. drop
;

\ Parse an ADR instruction
: p-adr-3
  ." ADD" rot cond. space dup 8_4_bf reg. ." , PC, #"
  dup 0_8_bf swap 12_3_bf 8 lshift or swap 10_1_bf 12 lshift or val. drop
;

\ Parse an AND immediate instruction
: p-and-imm
  ." AND" over 4s?. rot cond. space dup 8_4_bf reg. sep.
  over 0_4_bf reg. sep-imm. dup 0_8_bf swap 12_3_bf 8 lshift or
  swap 10_1_bf 12 lshift or decode-const12 drop
;

\ Parse an AND register instruction
: p-and-reg-1
  ." AND" swap conds. space decode-and-reg drop
;

\ Parse an AND register instruction
: p-and-reg-2
  ." AND" over 4s?. rot cond. .w dup 8_4_bf reg. sep. swap 0_4_bf reg. sep.
  dup 0_4_bf reg. dup 4_2_bf over 6_2_bf rot 12_3_bf 2 lshift or
  decode-imm-shift drop
;

\ Parse an ASR immediate instruction
: p-asr-imm-1
  ." ASR" swap conds. space dup 0_3_bf reg. sep. dup 3_3_bf reg. sep-imm.
  6 5 bitfield val. drop
;
  
\ Parse an ASR immediate instruction
: p-asr-imm-2
  ." ASR" swap 4s?. swap cond. .w dup 8_4_bf reg. sep. dup 0_4_bf reg. sep.
  %10 over 6_2_bf rot 12_3_bf 2 lshift or decode-imm-shift drop
;

\ Parse an ASR register instruction
: p-asr-reg-1
  ." ASR" swap conds. space dup 0_3_bf reg. sep. 3_3_bf reg. drop
;

\ Parse an ASR register instruction
: p-asr-reg-2
  ." ASR" over 4s?. rot cond. .w dup 8_4_bf reg. sep. swap 0_4_bf reg. sep.
  0_4_bf reg. drop
;

\ Parse a B instruction
: p-b-1
  ." B" nip dup 8_4_bf cond. space 0_8_bf 8 rel.
;

\ Parse a B instruction
: p-b-2
  ." B" swap cond. space 0 11 bitfield 11 rel.
;

\ Parse a B instruction
: p-b-3
  ." B" rot drop over 6 4 bitfield cond. .w
  dup 0 11 bitfield 2 pick 0 6 bitfield 11 lshift or
  over 13 1 bitfield 17 lshift or swap 11 1 bitfield 18 lshift or
  swap 10_1_bf 19 lshift or 20 rel.
;

\ Parse a B instruction
: p-b-4
  ." B" rot cond. .w
  dup 0 11 bitfield 2 pick 0 10 bitfield 11 lshift or
  over 11 1 bitfield 3 pick 10_1_bf xor 21 lshift or
  swap 13 1 bitfield 2 pick 10_1_bf xor 22 lshift or
  swap 10_1_bf 23 lshift or 24 rel.
;

\ Parse a BFC instruction
: p-bfc
  ." BFC" rot cond. space nip 8_4_bf reg. sep-imm.
  dup 6_2_bf over 12_3_bf 2 lshift or dup val. sep-imm.
  over 0 5 bitfield 1+ swap - val. drop
;

\ Parse a BFI instruction
: p-bfi
  ." BFI" rot cond. space dup 8_4_bf reg. sep. swap 0_4_bf reg. sep-imm.
  dup 6_2_bf over 12_3_bf 2 lshift or dup val. sep-imm.
  over 0 5 bitfield 1+ swap - val. drop
;

\ Parse a BIC immediate instruction
: p-bic-imm
  ." BIC" over 4s?. space decode-add-imm-32 drop
;

\ Parse a BIC register instruction
: p-bic-reg-1
  ." BIC" swap conds. space decode-and-reg-16 drop
;

\ Parse a BIC register instruction
: p-bic-reg-2
  ." BIC" over 4s?. rot cond. .w decode-add-reg-32 drop
;

\ Parse a BKPT instruction
: p-bkpt ( h-addr1 cond -- h-addr2 flag )
  ." BKPT" nip 0_8_bf ." #" val. drop
;

\ Parse a BL immediate instruction
: p-bl-imm
  ." BL" rot cond. space
  dup 0 11 bitfield
  over 0 10 bitfield 11 lshift or
  dup 11 1 bitfield 2 pick 10 1 bitfield xor invert 1 and 21 lshift or
  13 1 bitfield over 10 1 bitfield xor invert 1 and 22 lshift or
  swap 10 1 bitfield 23 lshift or 24 rel.
;

\ Parse a BLX register instruction
: p-blx-reg
  ." BLX" swap cond. space 3 4 bitfield reg. drop
;

\ Parse a BX register instruction
: p-bx
  ." BX" swap cond. space 3 4 bitfield reg. drop
;

\ Highest bit mask
%1000000000000000 constant highest

\ Commit to flash
commit-flash

\ All the 16-bit ops
create all-ops16
' p-adc-reg-1 , %1111111111000000 h, %0100000101000000 h,
' p-add-imm-1 , %1111111000000000 h, %0001110000000000 h,
' p-add-imm-2 , %1111100000000000 h, %0011000000000000 h,
' p-add-reg-1 , %1111111000000000 h, %0001100000000000 h,
' p-add-reg-2 , %1111111100000000 h, %0100010000000000 h,
' p-add-sp-imm-1 , %1111100000000000 h, %1010100000000000 h,
' p-add-sp-imm-2 , %1111111110000000 h, %1011000000000000 h,
' p-add-sp-reg-1 , %1111111101111000 h, %0100010001101000 h,
' p-add-sp-reg-2 , %1111111110000111 h, %0100010010000101 h,
' p-adr-1 , %111100000000000 h, %1010000000000000 h,
' p-and-reg-1 , %1111111111000000 h, %0100000000000000 h,
' p-asr-imm-1 , %1111100000000000 h, %0001000000000000 h,
' p-asr-reg-1 , %1111111111000000 h, %0100000100000000 h,
' p-b-1 , %1111000000000000 h, %1101000000000000 h,
' p-b-2 , %1111100000000000 h, %1110000000000000 h,
' p-bic-reg-1 , %1111111111000000 h, %0100001110000000 h,
' p-bkpt , %1111111100000000 h, %1011111000000000 h,
' p-blx-reg , %1111111110000000 h, %0100011110000000 h,
' p-bx , %1111111110000000 h, %0100011100000000 h,
0 ,

\ All the 32-bit ops
create all-ops32
' p-adc-imm , %1111101111100000 h, highest h, %1111000101000000 h, 0 h,
' p-adc-reg-2 , %1111111111100000 h, 0 h, %1110101101000000 h, 0 h,
' p-add-imm-3 , %1111101111100000 h, highest h, %1111000100000000 0 h,
' p-add-imm-4 , %1111101111110000 h, highest h, %1111001000000000 0 h,
' p-add-reg-3 , %1111111111100000 h, 0 h, %1110101100000000 h, 0 h,
' p-add-sp-imm-3 , %1111101111101111 h, highest h, %1111000100001101 h, 0 h,
' p-add-sp-imm-4 , %1111101111111111 h, highest h, %1111001000001101 h, 0 h,
' p-add-sp-reg-3 , %1111111111101111 h, highest h, %1110101100001101 h, 0 h,
' p-adr-2 , %1111101111111111 h, highest h, %1111001010101111 h, 0 h,
' p-adr-3 , %1111101111111111 h, highest h, %1111001000001111 h, 0 h,
' p-and-imm , %1111101111100000 h, highest h, %1111000000000000 h, 0 h,
' p-and-reg-2 , %1111111111100000 h, 0 h, %1110101000000000 h, 0 h,
' p-asr-imm-2 , %1111111111101111 h, %0000000000110000 h,
                %1110101001001111 h, %0000000000100000 h,
' p-asr-reg-2 , %1111111111100000 h, %1111000011110000 h,
                %1111101001000000 h, %1111000000000000 h,
' p-b-3 , %1111100000000000 h, %1101000000000000 h,
          %1111000000000000 h, %1000000000000000 h,
' p-b-4 , %1111100000000000 h, %1101000000000000 h,
          %1111000000000000 h, %1001000000000000 h,
' p-bfc , %1111101111111111 h, highest h, %1111001101101111 h, 0 h,
' p-bfi , %1111101111110000 h, highest h, %1111001101100000 h, 0 h,
' p-bic-imm , %11111000111100000 h, highest h, %1111000000100000 h, 0 h,
' p-bic-reg-2 , %1111111111100000 h, 0 h, %1110101000100000 h, 0 h,
' p-bl-imm , %1111100000000000 h, %1101000000000000 h,
             %1111000000000000 h, %1101000000000000 h,
0 ,

\ Commit to flash
commit-flash

\ Get condition
: current-cond ( -- cond ) -1 ;

\ Disassemble a 16-bit instruction
: disassemble16 ( op-addr handler-addr -- match )
  over h@ over cell+ h@ and over 6 + h@ = if
    current-cond 2 pick h@ instr16. 2 pick h@ rot execute true
  else
    2drop false
  then
;

\ Disassemble a 32-bit instruction
: disassemble32 ( op-addr handler-addr -- match )
  over h@ over cell+ h@ and over 8 + h@ = if
    over 2+ h@ over 6 + h@ and over 10 + h@ = if
      current-cond 2 pick h@ 3 pick 2+ h@ instr32.
      2 pick h@ 3 pick 2+ h@ 3 roll execute true
    else
      2drop false
    then
  else
    2drop false
  then
;

\ Disassemble instructions
: disassemble ( start end -- )
  swap begin
    over u<
  while
    dup h.8 space
    all-ops16 begin
      dup @ 0<> if
	2dup disassemble16 if
	  drop 2+ true true
	else
	  2 cells + false
	then
      else
	drop false true
      then
    until
    not if
      all-ops32 begin
	dup @ 0<> if
	  2dup disassemble32 if
	    drop 4+ true true
	  else
	    3 cells + false
	  then
	else
	  drop false true
	then
      until
      not if
	dup h@ instr16. ." ????" 2+ true
      then
    then
    cr
  repeat
  2drop
;

\ Finish compressing the code
end-compress-flash

\ Reboot
reboot