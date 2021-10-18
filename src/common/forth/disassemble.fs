\ Copyright (c) 2020-2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-import-module-once disassemble-internal-module

  import internal-module

  \ Disassemble for gas
  variable for-gas

  \ Local label count
  64 constant local-count

  \ Local label buffer
  local-count cells aligned-buffer: local-buffer

  \ Local label index
  variable local-index

  \ Does this follow an underscore
  variable prev-underscore

  \ Begin compressing compiled code in flash
  compress-flash

  \ Find a word in a dictionary by address
  : find-dict-by-address ( addr dict -- word|0 )
    begin
      dup 0<> if
	dup >body 2 pick = if
	  true
	else
	  next-word @ false
	then
      else
	true
      then
    until
    nip
  ;

  \ Look up a local label
  : lookup-local ( addr -- label|0 )
    local-index @ 0 ?do
      i cells local-buffer + @ over = if
	drop i 1 + unloop exit
      then
    loop
    drop 0
  ;

  \ Commit to flash
  commit-flash

  \ Find a word by address
  : find-by-address ( addr -- word|0 )
    dup ram-latest find-dict-by-address dup 0= if
      drop flash-latest find-dict-by-address
    else
      nip
    then
  ;

  \ Type a condition
  : (cond.) ( cond -- )
    case
      %0000 of ." EQ" endof
      %0001 of ." NE" endof
      %0010 of ." HS" endof \ or ." CS"
      %0011 of ." LO" endof \ or ." CC"
      %0100 of ." MI" endof
      %0101 of ." PL" endof
      %0110 of ." VS" endof
      %0111 of ." VC" endof
      %1000 of ." HI" endof
      %1001 of ." LS" endof
      %1010 of ." GE" endof
      %1011 of ." LT" endof
      %1100 of ." GT" endof
      %1101 of ." LE" endof
      \    %1110 of ." AL" endof
    endcase
  ;

  \ Get whether the condition is null
  : null-cond? ( cond -- f ) 0< ;

  \ Type an signed decimal value
  : (dec.) ( u -- ) base @ >r decimal (.) r> base  ! ;

  \ Type an unsigned decimal value
  : (udec.) ( u -- ) base @ >r decimal (u.) r> base  ! ;

  \ Type a register
  : reg. ( u -- )
    case
      15 of ." PC" endof
      14 of ." LR" endof
      13 of ." SP" endof
      dup ." R" (udec.)
    endcase
  ;

  \ Type a value
  : val. ( u -- )
    for-gas @ if ." 0x" else ." $" then base @ >r hex (u.) r> base !
  ;

  \ Type a bit size
  : size. ( size -- ) ?dup if emit then ;

  \ Generate words with a given size specified
  : w-size ( xt size "name" -- )
    <builds , , does> 2@ execute
  ;

  \ Type a leading underscore
  : lead-underscore ( index char-count -- )
    swap 0<> prev-underscore @ not and if ." _" 1+ then
  ;

  \ Type a tailing underscore
  : tail-underscore ( count index char-count -- )
    -rot 1 + <> if ." _" 1+ true prev-underscore ! then
  ;

  \ Commit to flash
  commit-flash

  \ Convert type character case
  : convert-type-char-case
    case
      [char] ` of dup 6 lead-underscore ." bquote" tail-underscore endof
      [char] ~ of dup 5 lead-underscore ." tilde" tail-underscore endof
      [char] ! of dup 5 lead-underscore ." store" tail-underscore endof
      [char] @ of dup 5 lead-underscore ." fetch" tail-underscore endof
      [char] # of dup 4 lead-underscore ." num" tail-underscore endof
      [char] $ of dup 6 lead-underscore ." dollar" tail-underscore endof
      [char] % of dup 7 lead-underscore ." percent" tail-underscore endof
      [char] ^ of dup 5 lead-underscore ." caret" tail-underscore endof
      [char] & of dup 3 lead-underscore ." amp" tail-underscore endof
      [char] * of dup 4 lead-underscore ." star" tail-underscore endof
      $28 of dup 5 lead-underscore ." paren" tail-underscore endof
      [char] ) of dup 6 lead-underscore ." cparen" tail-underscore endof
      [char] - of 2drop 1 ." _" endof
      [char] = of dup 5 lead-underscore ." equal" tail-underscore endof
      [char] + of dup 4 lead-underscore ." plus" tail-underscore endof
      [char] [ of dup 7 lead-underscore ." bracket" tail-underscore endof
      [char] { of dup 5 lead-underscore ." brace" tail-underscore endof
      [char] ] of dup 8 lead-underscore ." cbracket" tail-underscore endof
      [char] } of dup 6 lead-underscore ." cbrace" tail-underscore endof
      $5C of dup 4 lead-underscore ." back" tail-underscore endof
      [char] | of dup 4 lead-underscore ." pipe" tail-underscore endof
      [char] ; of dup 4 lead-underscore ." semi" tail-underscore endof
      [char] : of dup 5 lead-underscore ." colon" tail-underscore endof
      [char] ' of dup 5 lead-underscore ." quote" tail-underscore endof
      [char] " of dup 6 lead-underscore ." dquote" tail-underscore endof
      [char] , of dup 5 lead-underscore ." comma" tail-underscore endof
      [char] < of dup 2 lead-underscore ." lt" tail-underscore endof
      [char] . of dup 3 lead-underscore ." dot" tail-underscore endof
      [char] > of dup 2 lead-underscore ." gt" tail-underscore endof
      [char] / of dup 5 lead-underscore ." slash" tail-underscore endof
      [char] ? of dup 4 lead-underscore ." ques" tail-underscore endof
      dup emit nip nip 1 swap false prev-underscore !
    endcase
  ;
  
  \ Convert and type a character meant for an assembler
  : convert-type-char ( b count index -- len )
    2 pick [char] - = 2 pick 1 = and over 0 = and if
      ." minus" 2drop drop
    else
      rot convert-type-char-case
    then
  ;

  \ Type out a label, with two different display modes depending on whether the
  \ target is a user or an assembler
  : label-type ( b-addr u -- )
    false prev-underscore !
    for-gas @ if
      dup 0 0 -rot ?do 2 pick i + c@ 2 pick i convert-type-char + loop nip nip
    else
      tuck type
    then
  ;

  \ Commit to flash
  commit-flash

  \ Print out an exclamation point if a register is not in a register list
  : not-in-reglist. ( list reg size -- )
    0 ?do over i rshift 1 and if dup i = if unloop exit then then loop
    2drop ." !"
  ;

  \ Print out a register list
  : reglist. ( list size -- )
    ." {" false swap 0 ?do
      over i rshift 1 and if
	dup if ." , " else drop true then
	i reg.
      then
    loop
    2drop ." }"
  ;

  \ Print out an absolute address
  : addr. ( op-addr ref-addr -- )
    dup find-by-address ?dup if
      rot drop word-name count label-type drop
      for-gas @ if drop else space ." <" val. ." >" then
    else
      2dup <> if
	dup lookup-local ?dup if
	  (udec.) tuck > if ." B" else ." F" then
	  for-gas @ if drop else space ." <" val. ." >" then
	else
	  nip ." #" val.
	then
      else
	." ." for-gas @ if drop else space ." <" val. ." >" then drop
      then
    then
  ;

  \ Print out a label
  : label. ( addr -- )
    dup find-by-address ?dup if
      nip word-name count for-gas @ if
	label-type ." :" 20 swap - 0 max 1 + 0 ?do space loop
      else
	20 min label-type ." :" 21 swap - 0 ?do space loop
      then
    else
      lookup-local ?dup if
	base @ >r 10 base ! 0 <# #s #> r> base ! for-gas @ if
	  tuck type ." :" 20 swap - 0 max 1 + 0 ?do space loop
	else
	  20 min tuck type ." :" 21 swap - 0 ?do space loop
	then
      else
	22 0 ?do space loop
      then
    then
  ;

  \ Separator
  : sep. ( -- ) ." , " ;

  \ Separator with immediate
  : sep-imm. ( -- ) ."  , #" ;

  \ Type a 16-bit instruction in halfwords
  : instr16. ( h -- ) for-gas @ if drop else h.4 ." :      " then ;

  \ Type a 32-bit instruction in halfwords
  : instr32. ( low high -- )
    for-gas @ if 2drop else swap h.4 space h.4 ." : " then
  ;

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
    dup %1111111 and %10000000 or
    swap 7 rshift %11111 and 2dup rshift -rot 32 swap - lshift or
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
    dup null-cond? not if
      (cond.)
    else
      drop
    then
  ;

  \ Print out a condition if one is specified, otherwise do print out 'S'
  : conds. ( cond -- )
    dup null-cond? not if
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
  : rel. ( pc rel extend -- ) rot dup >r 4 + -rot extend + r> swap addr. ;

  \ Type a 4-aligned PC-relative address
  : rel4. ( pc rel extend -- )
    rot dup >r 4 + 4 align -rot extend + r> swap addr.
  ;

  \ Type a non-sign-extended PC-relative address
  : nrel. ( pc rel -- ) swap dup >r 4 + swap + r> swap addr. ;

  \ Type a non-sign-extended 4-aligned PC-relative address
  : nrel4. ( pc rel -- ) swap dup >r 4 + 4 align swap + r> swap addr. ;

  \ Type out .W
  : .w ( -- ) ." .W " ;

  \ Commit to flash
  commit-flash


  \ Type out a register and a separator
  : reg-sep. ( reg -- ) reg. sep. ;

  \ Type out a register, a separator, and an immediate marker
  : reg-sep-imm. ( reg -- ) reg. sep-imm. ;

  \ Type out a condition with a space for 16-bit words
  : csp. ( cond low -- low ) swap cond. space ;

  \ Type out a condition or S with a space for 16-bit words
  : cssp. ( cond low -- low ) swap conds. space ;

  \ Type out the condition and .W for 32-bit words
  : c.w ( cond low high -- low high ) rot cond. .w ;

  \ Type out the condition and a space for 32-bit words
  : c.sp ( cond low high -- low high ) rot cond. space ;

  \ Type out the S, the condition, and .W for 32-bit words
  : 4sc.w ( cond low high -- low high ) over 4s?. rot cond. .w ;

  \ Type out the S and the condition followed by a space for 32-bit words
  : 4sc.sp ( cond low high -- low high ) over 4s?. rot cond. space ;

  \ 0 3 bitfield
  : 0_3_bf ( data -- field ) 0 3 bitfield ;

  \ 3 3 bitfield
  : 3_3_bf ( data -- field ) 3 3 bitfield ;

  \ 3 4 bitfield
  : 3_4_bf ( data -- field ) 3 4 bitfield ;

  \ 8 3 bitfield
  : 8_3_bf ( data -- field ) 8 3 bitfield ;

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

  \ 0 12 bitfield
  : 0_12_bf ( data -- field ) 0 12 bitfield ;

  \ Commit to flash
  commit-flash

  \ Decode a 16-bit AND register instruction
  : decode-and-reg-16 ( low -- )
    dup 0_3_bf reg-sep. 3_3_bf reg.
  ;

  \ Decode a 32-bit AND register instruction
  : decode-and-reg-32 ( low high -- )
    dup 8_4_bf reg-sep. swap 0_4_bf reg-sep.
    dup 0_4_bf reg. dup 4_2_bf over 6_2_bf rot 12_3_bf 2 lshift or
    decode-imm-shift
  ;

  \ Decode a 16-bit ASR immediate instruction
  : decode-asr-imm-16 ( low -- )
    dup 0_3_bf reg-sep. dup 3_3_bf reg-sep-imm.  6 5 bitfield val.
  ;

  \ Decode a 32-bit ASR immediate instruction
  : decode-asr-imm-32 ( low high -- )
    dup 8_4_bf reg-sep. dup 0_4_bf reg-sep-imm.
    dup 6_2_bf swap 12_3_bf 2 lshift or (udec.)
  ;

  \ Decode a 32-bit ASR register instruction
  : decode-asr-reg-32 ( low high -- )
    dup 8_4_bf reg-sep. swap 0_4_bf reg-sep. 0_4_bf reg.
  ;

  \ Decode a 32-bit CMN immediate instruction
  : decode-cmn-imm-32 ( low high -- )
    over 0_4_bf reg-sep-imm.
    dup 0_8_bf swap 12_3_bf 8 lshift or swap 10_1_bf 11 lshift or decode-const12
  ;

  \ Decode a 32-bit CMN register instruction
  : decode-cmn-reg-32 ( low high -- )
    swap 0_4_bf reg-sep.
    dup 0_4_bf reg.
    dup 4_2_bf
    over 6_2_bf rot 12_3_bf 2 lshift or and decode-imm-shift
  ;

  \ Decode a 16-bit ADD immediate with a 3-bit unextended immediate
  : decode-add-imm-1 ( low -- )
    dup 0_3_bf reg-sep. dup 3_3_bf reg-sep-imm. 6_3_bf val.
  ;

  \ Decode a 16-bit ADD immediate with a 8-bit unextended immediate
  : decode-add-imm-2 ( low -- )
    dup 8_3_bf reg-sep-imm. 0_8_bf val.
  ;

  \ Decode a 32-bit ADD immediate or ADC immediate instruction
  : decode-add-imm-3 ( low high -- )
    dup 8_4_bf reg-sep.
    over 0_4_bf reg-sep-imm.
    dup 0_8_bf swap 12_3_bf 8 lshift or
    swap 10_1_bf 11 lshift or decode-const12
  ;

  \ Decode a 32-bit ADD immediate with a 12-bit unextended immediate
  : decode-add-imm-4 ( low high -- )
    dup 8_4_bf reg-sep.
    over 0_4_bf reg-sep-imm.
    dup 0_8_bf swap 12_4_bf 8 lshift or swap 10_1_bf 11 lshift or val.
  ;

  \ Decode the first kind of 16-bit ADD register instructions
  : decode-add-reg-16 ( low -- )
    dup 0_3_bf reg-sep. dup 3_3_bf reg-sep. 6_3_bf reg.
  ;

  \ Decode the second kind of 32-bit ADD register or ADC register instruction
  : decode-add-reg-32 ( low high --- )
    dup 8_4_bf reg-sep. decode-cmn-reg-32
  ;

  \ Decode a 16-bit immediate value 32-bit MOV immediate instruction
  : decode-mov-imm-32 ( low high -- )
    dup 8_4_bf reg-sep-imm. dup 0_8_bf swap 12_3_bf 8 lshift or
    over 10_1_bf 11 lshift or swap 0_4_bf 12 lshift or val.
  ;

  \ Decode a CBNZ/CBZ instruction
  : decode-cbz ( pc cond h -- )
    nip space dup 0_3_bf reg-sep.
    dup 3 5 bitfield swap 9 1 bitfield 5 lshift or 1 lshift nrel.
  ;

  \ Decode an SMULL instruction
  : decode-smull ( low high -- )
    dup 12_4_bf reg-sep. dup 8_4_bf reg-sep. swap 0_4_bf reg-sep. 0_4_bf reg.
  ;

  \ Decode an LDR immediate instruction
  : decode-ldr-imm-1
    dup 0_3_bf reg-sep. ." [" dup 3_3_bf reg.
    6 5 bitfield 2 lshift ?dup if
      sep-imm. val.
    then
    ." ]"
  ;

  \ Decode an LDR immediate instruction
  : decode-ldr-imm-2
    dup 8_3_bf reg-sep. ." [SP"
    0_8_bf 2 lshift ?dup if
      sep-imm. val.
    then
    ." ]"
  ;

  \ Decode an LDR immediate instruction
  : decode-ldr-imm-3
    dup 12_4_bf reg-sep. ." [" swap 0_4_bf reg.
    0 12 bitfield ?dup if
      2 lshift sep-imm. val.
    then
    ." ]"
  ;

  \ Decode an LDR immediate instruction
  : decode-ldr-imm-4
    dup 12_4_bf reg-sep. ." [" swap 0_4_bf reg.
    dup 10_1_bf if
      sep-imm. dup 0_8_bf over 9 1 bitfield 0= if negate then (dec.) ." ]"
      8 1 bitfield if ." !" then
    else
      ." ]" sep-imm. dup 0_8_bf swap 9 1 bitfield 0= if negate then (dec.)
    then
  ;

  \ Decode an LDR register instruction
  : decode-ldr-reg-1
    dup 0_3_bf reg. ." , [" dup 3_3_bf reg-sep.
    6_3_bf reg. ." ]"
  ;

  \ Decode an LDR register instruction
  : decode-ldr-reg-2
    dup 12_3_bf reg. ." , [" swap 0_3_bf reg-sep.
    0_3_bf reg. 4_2_bf ?dup if ." , LSL #" (udec.) then ." ]"
  ;

  thumb-2 [if]
    
  \ Parse an ADC immediate instruction
    : p-adc-imm
      ." ADC" 4sc.sp decode-add-imm-3 drop
    ;

  [then]

  \ Parse an ADC register instruction
  : p-adc-reg-1
    ." ADC" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an ADC register instruction
    : p-adc-reg-2
      ." ADC" 4sc.w decode-add-reg-32 drop
    ;

  [then]

  \ Parse an ADD immediate instruction
  : p-add-imm-1
    ." ADD" cssp. decode-add-imm-1 drop
  ;

  \ Parse an ADD immediate instruction
  : p-add-imm-2
    ." ADD" cssp. decode-add-imm-2 drop
  ;

  thumb-2 [if]
    
    \ Parse an ADD immediate instruction
    : p-add-imm-3
      ." ADD" 4sc.w decode-add-imm-3 drop
    ;
    
    \ Parse an ADD immediate instruction
    : p-add-imm-4
      ." ADDW" c.sp decode-add-imm-4 drop
    ;

  [then]

  \ Parse an ADD register instruction
  : p-add-reg-1
    ." ADD" cssp. decode-add-reg-16 drop
  ;

  \ Parse an ADD register instruction
  : p-add-reg-2
    ." ADD" csp.
    dup 0_3_bf over 7 1 bitfield 3 lshift or reg-sep. 3_4_bf reg. drop
  ;

  thumb-2 [if]

    \ Parse an ADD register instruction
    : p-add-reg-3
      ." ADD" 4sc.w decode-add-reg-32 drop
    ;

  [then]

  \ \ Parse an ADD SP to immediate instruction
  \ : p-add-sp-imm
  \   ." ADD" csp. dup 8_3_bf reg. ." , SP, #" 0_8_bf val. drop
  \ ;

  \ \ Parse an ADD SP to immediate instruction
  \ : p-add-sp-imm
  \   ." ADD" csp. ." SP, SP, #" 0 7 bitfield val. drop
  \ ;

  \ \ Parse an ADD SP to immediate instruction
  \ : p-add-sp-imm
  \   ." ADD" 4sc.sp
  \   dup 8_4_bf reg. ." , SP, #" dup 0_8_bf swap 12_4_bf 8 lshift or
  \   swap 10_1_bf 11 lshift or decode-const12 drop
  \ ;

  \ \ Parse an ADD SP to immediate instruction
  \ : p-add-sp-imm
  \   ." ADDW" c.sp
  \   dup 8_4_bf reg. ." , SP, #" dup 0_8_bf swap 12_4_bf 8 lshift or
  \   swap 10_1_bf 11 lshift or val. drop
  \ ;

  \ \ Parse an ADD SP to register instruction
  \ : p-add-sp-reg-1
  \   ." ADD" csp.
  \   dup 0_3_bf over 7 1 bitfield 3 lshift or dup reg. ." , SP, " reg. drop
  \ ;

  \ \ Parse an ADD SP to register instruction
  \ : p-add-sp-reg-2
  \   ." ADD" csp. ." SP, " 2 4 bitfield reg. drop
  \ ;
  \ \ Parse an ADD SP to register instruction
  \ : p-add-sp-reg-3
  \   ." ADD" swap 4s?. swap cond. .w
  \   dup 8_4_bf reg. ." , SP, " dup 0_4_bf reg. dup 4_2_bf
  \   over 6_2_bf rot 12_3_bf lshift 2 or decode-imm-shift drop
  \ ;

  \ Parse an ADR instruction
  : p-adr-1
    ." ADD" csp. dup 8_3_bf reg. ." , PC, #"
    0_8_bf 2 lshift val. drop
  ;

  thumb-2 [if]  

    \ Parse an ADR instruction
    : p-adr-2
      ." SUB" c.sp dup 8_4_bf reg. ." , PC, #"
      dup 0_8_bf swap 12_3_bf 8 lshift or swap 10_1_bf 12 lshift or val. drop
    ;
    
    \ Parse an ADR instruction
    : p-adr-3
      ." ADD" c.sp dup 8_4_bf reg. ." , PC, #"
      dup 0_8_bf swap 12_3_bf 8 lshift or swap 10_1_bf 12 lshift or val. drop
    ;
    
    \ Parse an AND immediate instruction
    : p-and-imm
      ." AND" 4sc.sp dup 8_4_bf reg-sep.
      over 0_4_bf reg-sep-imm. dup 0_8_bf swap 12_3_bf 8 lshift or
      swap 10_1_bf 12 lshift or decode-const12 drop
    ;

  [then]

  \ Parse an AND register instruction
  : p-and-reg-1
    ." AND" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an AND register instruction
    : p-and-reg-2
      ." AND" 4sc.w decode-and-reg-32 drop
    ;

  [then]

  \ Parse an ASR immediate instruction
  : p-asr-imm-1
    ." ASR" cssp. decode-asr-imm-16 drop
  ;
  
  thumb-2 [if]
    
    \ Parse an ASR immediate instruction
    : p-asr-imm-2
      ." ASR" swap 4s?. swap cond. .w decode-asr-imm-32 drop
    ;

  [then]

  \ Parse an ASR register instruction
  : p-asr-reg-1
    ." ASR" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an ASR register instruction
    : p-asr-reg-2
      ." ASR" 4sc.w decode-asr-reg-32 drop
    ;

  [then]

  \ Parse an SVC instruction
  : p-svc
    ." SVC" csp. 0_8_bf ." #" val. drop
  ;

  \ Parse a B instruction
  : p-b-1
    dup 8_4_bf $F = if
      p-svc
    else
      ." B" nip dup 8_4_bf cond. space 0_8_bf 1 lshift 9 rel.
    then
  ;

  \ Parse a B instruction
  : p-b-2
    ." B" csp. 0 11 bitfield 1 lshift 12 rel.
  ;
  
  thumb-2 [if]
    
    \ Parse a B instruction
    : p-b-3
      ." B" rot drop over 6 4 bitfield cond. .w
      dup 0 11 bitfield 2 pick 0 6 bitfield 11 lshift or
      over 13 1 bitfield 17 lshift or swap 11 1 bitfield 18 lshift or
      swap 10_1_bf 19 lshift or 1 lshift 21 rel.
    ;
    
    \ Parse a B instruction
    : p-b-4
      ." B" c.w
      dup 0 11 bitfield 2 pick 0 10 bitfield 11 lshift or
      over 11 1 bitfield 3 pick 10_1_bf xor not 1 and 21 lshift or
      swap 13 1 bitfield 2 pick 10_1_bf xor not 1 and 22 lshift or
      swap 10_1_bf 23 lshift or 1 lshift 25 rel.
    ;
    
    
    \ Parse a BFC instruction
    : p-bfc
      ." BFC" c.sp nip 8_4_bf reg-sep-imm.
      dup 6_2_bf over 12_3_bf 2 lshift or dup val. sep-imm.
      over 0 5 bitfield 1+ swap - val. drop
    ;
    
    \ Parse a BFI instruction
    : p-bfi
      ." BFI" c.sp dup 8_4_bf reg-sep. swap 0_4_bf reg-sep-imm.
      dup 6_2_bf over 12_3_bf 2 lshift or dup val. sep-imm.
      over 0 5 bitfield 1+ swap - val. drop
    ;

    \ Parse a BIC immediate instruction
    : p-bic-imm
      ." BIC" over 4s?. space decode-add-imm-3 drop
    ;

  [then]

  \ Parse a BIC register instruction
  : p-bic-reg-1
    ." BIC" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse a BIC register instruction
    : p-bic-reg-2
      ." BIC" 4sc.w decode-add-reg-32 drop
    ;

  [then]

  \ Parse a BKPT instruction
  : p-bkpt
    ." BKPT" space nip 0_8_bf ." #" val. drop
  ;

  \ Parse a BL immediate instruction
  : p-bl-imm
    ." BL" c.sp
    dup 0 11 bitfield
    2 pick 0 10 bitfield 11 lshift or
    over 11 1 bitfield 3 pick 10 1 bitfield xor not 1 and 21 lshift or
    swap 13 1 bitfield 2 pick 10 1 bitfield xor not 1 and 22 lshift or
    swap 10 1 bitfield 23 lshift or 1 lshift 25 rel.
  ;

  \ Parse a BLX register instruction
  : p-blx-reg
    ." BLX" csp. 3_4_bf reg. drop
  ;

  \ Parse a BX register instruction
  : p-bx
    ." BX" csp. 3_4_bf reg. drop
  ;

  \ Parse a CBNZ instruction
  : p-cbnz ." CBNZ" decode-cbz ;

  \ Parse a CBZ instruction
  : p-cbz ." CBZ" decode-cbz ;

  thumb-2 [if]
    
    \ Parse a CMN immediate instruction
    : p-cmn-imm
      ." CMN" csp. decode-cmn-imm-32 drop
    ;

  [then]

  \ Parse a CMN register instruction
  : p-cmn-reg-1
    ." CMN" csp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse a CMN register instruction
    : p-cmn-reg-2
      ." CMN" swap cond. .w decode-cmn-reg-32
    ;

  [then]

  \ Parse an CMP immediate instruction
  : p-cmp-imm-1
    ." CMP " nip decode-add-imm-2 drop
  ;

  thumb-2 [if]
    
    \ Parse a CMP immediate instruction
    : p-cmp-imm-2
      ." CMP" c.w decode-cmn-imm-32 drop
    ;

  [then]

  \ Parse a CMP register instruction
  : p-cmp-reg-1
    ." CMP" csp. decode-and-reg-16 drop
  ;

  \ Parse a CMP register instruction
  : p-cmp-reg-2
    ." CMP" csp. dup 0_3_bf over 7 1 bitfield 3 lshift or reg-sep.
    3_4_bf reg. drop
  ;

  thumb-2 [if]
    
    \ Parse a CMP register instruction
    : p-cmp-reg-3
      ." CMP" c.w decode-cmn-reg-32 drop
    ;

  [then]

  \ Parse a CPS instruction
  : p-cps-1
    ." CPS" nip dup 4 1 bitfield if [char] D else [char] E then emit space
    dup 2 1 bitfield if [char] A emit then
    dup 1 1 bitfield if [char] I emit then
    0 1 bitfield if [char] F emit then drop
  ;
  
  \ Parse a DMB instruction
  : p-dmb ." DMB" 2drop 2drop ;

  \ Parse a DSB instruction
  : p-dsb
    ." DSB" nip 0_4_bf case
      \ %1111 of endof
      %0111 of space ." UN" endof
      %1110 of space ." ST" endof
      %0110 of space ." UNST" endof
    endcase
    2drop
  ;

  \ Parse an ISB instruction
  : p-isb ." ISB" 2drop 2drop ;

  thumb-2 [if]
    
    \ Parse an EOR immediate instruction
    : p-eor-imm
      ." EOR" rot drop over 4s?. decode-add-imm-3 drop
    ;

  [then]

  \ Parse an EOR register instruction
  : p-eor-reg-1
    ." EOR" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an EOR register instruction
    : p-eor-reg-2
      ." EOR" 4sc.w decode-add-reg-32 drop
    ;

  [then]

  \ Parse an LDMIA instruction
  : p-ldmia-1
    ." LDMIA" csp. dup 8_3_bf dup reg.
    swap 0_8_bf dup rot 8 not-in-reglist. sep. 8 reglist. drop
  ;

  \ Parse an LDR immediate instruction
  : p-ldr-imm-1
    ." LDR" size. csp. decode-ldr-imm-1 drop
  ;

  \ Parse an LDR immediate instruction
  : p-ldr-imm-2
    ." LDR" size. csp. decode-ldr-imm-2 drop
  ;

  thumb-2 [if]
    
    \ Parse an LDR immediate instruction
    : p-ldr-imm-3
      ." LDR" size. c.w decode-ldr-imm-3 drop
    ;
    
    \ Parse an LDR immediate instruction
    : p-ldr-imm-4
      ." LDR" size. c.sp decode-ldr-imm-4 drop
    ;

  [then]

  \ Parse an LDR literal instruction
  : p-ldr-lit-1
    ." LDR" size. csp. dup 8_3_bf reg-sep. 0_8_bf 2 lshift
    for-gas @ if ." [PC, #" (udec.) ." ]" drop else nrel4. then
  ;

  thumb-2 [if]
    
    \ Parse an LDR literal instruction
    : p-ldr-lit-2
      ." LDR" size. c.w dup dup 12_4_bf reg-sep.
      0_12_bf swap 7 1 bitfield if negate then
      for-gas @ if ." [PC, #" (dec.) ." ]" drop else nrel4. then
    ;

  [then]
  
  \ Parse an LDR register instruction
  : p-ldr-reg-1
    ." LDR" size. csp. decode-ldr-reg-1 drop
  ;

  thumb-2 [if]
    
    \ Parse an LDR register instruction
    : p-ldr-reg-2
      ." LDR" size. c.w decode-ldr-reg-2 drop
    ;

  [then]

  \ Parse an LSL immediate instruction
  : p-lsl-imm-1
    ." LSL" cssp. decode-asr-imm-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an LSL immediate instruction
    : p-lsl-imm-2
      ." LSL" 4sc.w decode-asr-imm-32 drop
    ;

  [then]

  \ Parse an LSL register instruction
  : p-lsl-reg-1
    ." LSL" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an LSL register instruction
    : p-lsl-reg-2
      ." LSL" 4sc.w decode-asr-reg-32 drop
    ;

  [then]
  
  \ Parse an LSR immediate instruction
  : p-lsr-imm-1
    ." LSR" cssp. decode-asr-imm-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an LSR immediate instruction
    : p-lsr-imm-2
      ." LSR" 4sc.w decode-asr-imm-32 drop
    ;

  [then]

  \ Parse an LSR register instruction
  : p-lsr-reg-1
    ." LSR" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an LSR register instruction
    : p-lsr-reg-2
      ." LSR" 4sc.w decode-asr-reg-32 drop
    ;

  [then]

  \ Parse an MLS instruction
  : p-mls
    ." MLS" rot drop space dup 8_4_bf reg-sep. swap 0_4_bf reg-sep.
    dup 0_4_bf reg-sep. 12_4_bf reg. drop
  ;

  \ Parse a MOV immediate instruction
  : p-mov-imm-1
    ." MOV" cssp. decode-add-imm-2 drop
  ;

  thumb-2 [if]
    
    \ Parse a MOV immediate instruction
    : p-mov-imm-2
      ." MOV" 4sc.w dup 8_4_bf reg-sep-imm.
      dup 0_8_bf swap 12_3_bf 8 lshift or
      swap 10_1_bf 11 lshift or decode-const12 drop
    ;
    
    \ Parse a MOV immediate instruction
    : p-mov-imm-3
      ." MOVW" c.sp decode-mov-imm-32 drop
    ;

  [then]

  \ Parse a MOV register instruction
  : p-mov-reg-1
    ." MOV" csp. dup 0_3_bf over 7 1 bitfield or reg-sep. 3_4_bf reg. drop
  ;

  \ Parse a MOV register instruction
  : p-mov-reg-2
    ." MOVS" space nip decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse a MOV register instruction
    : p-mov-reg-3
      ." MOV" 4sc.w nip dup 8_4_bf reg-sep. 0_4_bf reg. drop
    ;

    \ Parse a MOVT instruction
    : p-movt
      ." MOVT" c.sp decode-mov-imm-32 drop
    ;

  [then]
  
  \ Parse a MUL instruction
  : p-mul-1
    ." MUL" cssp. decode-and-reg-16 drop
  ;
  
  thumb-2 [if]
    
    \ Parse a MUL instruction
    : p-mul-2
      ." MUL" over 4s?. space decode-asr-reg-32 drop
    ;

  [then]
    
  \ Parse an MVN register instruction
  : p-mvn-reg-1
    ." MVN" cssp. decode-and-reg-16 drop
  ;

  \ Parse a NOP instruction
  : p-nop-1
    ." NOP" drop cond. drop
  ;

  thumb-2 [if]
    
    \ Parse an ORR immediate instruction
    : p-orr-imm
      ." ORR" 4sc.sp decode-add-imm-3 drop
    ;

  [then]

  \ Parse an ORR register instruction
  : p-orr-reg-1
    ." ORR" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an ORR register instruction
    : p-orr-reg-2
      ." ORR" 4sc.w decode-and-reg-32 drop
    ;

  [then]

  \ Parse a POP instruction
  : p-pop-1
    ." POP" csp. dup 0_8_bf swap 8 1 bitfield 15 lshift or 16 reglist. drop
  ;

  thumb-2 [if]
    
    \ Parse a POP instruction
    : p-pop-2
      ." POP" c.w nip dup 0_12_bf swap 14 2 bitfield 14 lshift or 16 reglist.
      drop
    ;

  [then]

  \ Parse a PUSH instruction
  : p-push-1
    ." PUSH" csp. dup 0_8_bf swap 8 1 bitfield 14 lshift or 16 reglist. drop
  ;

  thumb-2 [if]  

    \ Parse a PUSH instruction
    : p-push-2
      ." PUSH" c.w nip dup 0_12_bf swap 14 1 bitfield 14 lshift or 16 reglist.
      drop
    ;

  [then]

  \ Parse an RSB immediate instruction
  : p-rsb-imm-1
    ." RSB " cssp. decode-and-reg-16 ." , #0" drop
  ;

  thumb-2 [if]
    
    \ Parse an SBC immediate instruction
    : p-sbc-imm
      ." SBC" 4sc.sp decode-add-imm-3 drop
    ;

  [then]

  \ Parse an SBC register instruction
  : p-sbc-reg-1
    ." SBC" cssp. decode-and-reg-16 drop
  ;

  thumb-2 [if]
    
    \ Parse an SBC register instruction
    : p-sbc-reg-2
      ." SBC" 4sc.w decode-and-reg-32 drop
    ;

    \ Parse an SDIV register instruction
    : p-sdiv
      ." SDIV" c.sp decode-asr-reg-32 drop
    ;
    
    \ Parse an SMULL instruction
    : p-smull
      ." SMULL" c.sp decode-smull drop
    ;

  [then]

  \ Parse an STR immediate instruction
  : p-str-imm-1
    ." STR" size. csp. decode-ldr-imm-1 drop
  ;

  \ Parse an STR immediate instruction
  : p-str-imm-2
    ." STR" size. csp. decode-ldr-imm-2 drop
  ;

  thumb-2 [if]

    \ Parse an STR immediate instruction
    : p-str-imm-3
      ." STR" size. c.w decode-ldr-imm-3 drop
    ;
    
    \ Parse an STR immediate instruction
    : p-str-imm-4
      ." STR" size. c.sp decode-ldr-imm-4 drop
    ;

  [then]

  \ Parse an STR register instruction
  : p-str-reg-1
    ." STR" size. csp. decode-ldr-reg-1 drop
  ;

  thumb-2 [if]

    \ Parse an STR register instruction
    : p-str-reg-2
      ." STR" size. c.w decode-ldr-reg-2 drop
    ;

  [then]
    
  \ Parse an SUB immediate instruction
  : p-sub-imm-1
    ." SUB" cssp. decode-add-imm-1 drop
  ;

  \ Parse an SUB immediate instruction
  : p-sub-imm-2
    ." SUB" cssp. decode-add-imm-2 drop
  ;

  thumb-2 [if]

    \ Parse an SUB immediate instruction
    : p-sub-imm-3
      ." SUB" 4sc.w decode-add-imm-3 drop
    ;
    
    \ Parse an SUB immediate instruction
    : p-sub-imm-4
      ." SUBW" c.sp decode-add-imm-4 drop
    ;

  [then]

  \ Parse an SUB register instruction
  : p-sub-reg-1
    ." SUB" cssp. decode-add-reg-16 drop
  ;

  thumb-2 [if]

    \ Parse an SUB register instruction
    : p-sub-reg-2
      ." SUB" 4sc.w decode-add-reg-32 drop
    ;
    
    \ Parse a TST immediate instruction
    : p-tst-imm
      ." TST" csp. decode-cmn-imm-32 drop
    ;

  [then]

  \ Parse a TST register instruction
  : p-tst-reg-1
    ." TST" csp. decode-and-reg-16 drop
  ;

  thumb-2 [if]

    \ Parse a CMP register instruction
    : p-tst-reg-2
      ." CMP" c.w decode-cmn-reg-32 drop
    ;
    
    \ Parse an UDIV register instruction
    : p-udiv
      ." UDIV" c.sp decode-asr-reg-32 drop
    ;
    
    \ Parse an UMULL instruction
    : p-umull
      ." UMULL" c.sp decode-smull drop
    ;

  [then]

  \ Parse an WFI instruction
  : p-wfi-1
    ." WFI" swap cond. 2drop
  ;

  \ Commit to flash
  commit-flash

  0 ' p-ldr-imm-1 w-size p-ldr-imm-1-w
  0 ' p-ldr-imm-2 w-size p-ldr-imm-2-w

  thumb-2 [if]
  
    0 ' p-ldr-imm-3 w-size p-ldr-imm-3-w
    0 ' p-ldr-imm-4 w-size p-ldr-imm-4-w

  [then]
  
  0 ' p-ldr-lit-1 w-size p-ldr-lit-1-w

  thumb-2 [if]

    0 ' p-ldr-lit-2 w-size p-ldr-lit-2-w

  [then]
  
  0 ' p-ldr-reg-1 w-size p-ldr-reg-1-w

  thumb-2 [if]

    0 ' p-ldr-reg-2 w-size p-ldr-reg-2-w

  [then]
  
  char B ' p-ldr-imm-1 w-size p-ldr-imm-1-b

  thumb-2 [if]

    char B ' p-ldr-imm-3 w-size p-ldr-imm-3-b
    char B ' p-ldr-imm-4 w-size p-ldr-imm-4-b
  [then]
  
  thumb-2 [if]
    
    char B ' p-ldr-lit-2 w-size p-ldr-lit-2-b

  [then]
  
  char B ' p-ldr-reg-1 w-size p-ldr-reg-1-b

  thumb-2 [if]

    char B ' p-ldr-reg-2 w-size p-ldr-reg-2-b

  [then]
  
  char H ' p-ldr-imm-1 w-size p-ldr-imm-1-h

  thumb-2 [if]

    char H ' p-ldr-imm-3 w-size p-ldr-imm-3-h
    char H ' p-ldr-imm-4 w-size p-ldr-imm-4-h
    char H ' p-ldr-lit-2 w-size p-ldr-lit-2-h

  [then]
  
  char H ' p-ldr-reg-1 w-size p-ldr-reg-1-h

  thumb-2 [if]

    char H ' p-ldr-reg-2 w-size p-ldr-reg-2-h

  [then]
  
  0 ' p-str-imm-1 w-size p-str-imm-1-w
  0 ' p-str-imm-2 w-size p-str-imm-2-w

  thumb-2 [if]

    0 ' p-str-imm-3 w-size p-str-imm-3-w
    0 ' p-str-imm-4 w-size p-str-imm-4-w

  [then]
  
  0 ' p-str-reg-1 w-size p-str-reg-1-w

  thumb-2 [if]

    0 ' p-str-reg-2 w-size p-str-reg-2-w

  [then]
  
  char B ' p-str-imm-1 w-size p-str-imm-1-b

  thumb-2 [if]

    char B ' p-str-imm-3 w-size p-str-imm-3-b
    char B ' p-str-imm-4 w-size p-str-imm-4-b

  [then]
  
  char B ' p-str-reg-1 w-size p-str-reg-1-b

  thumb-2 [if]
  
    char B ' p-str-reg-2 w-size p-str-reg-2-b

  [then]
  
  char H ' p-str-imm-1 w-size p-str-imm-1-h

  thumb-2 [if]
  
    char H ' p-str-imm-3 w-size p-str-imm-3-h
    char H ' p-str-imm-4 w-size p-str-imm-4-h

  [then]
  
  char H ' p-str-reg-1 w-size p-str-reg-1-h

  thumb-2 [if]

    char H ' p-str-reg-2 w-size p-str-reg-2-h

  [then]

  \ Highest bit mask
  %1000000000000000 constant highest

  \ Commit to flash
  commit-flash

  \ All the 16-bit ops
  create all-ops16
  ' p-adc-reg-1 ,    %1111111111000000 h, %0100000101000000 h,
  ' p-add-imm-1 ,    %1111111000000000 h, %0001110000000000 h,
  ' p-add-imm-2 ,    %1111100000000000 h, %0011000000000000 h,
  ' p-add-reg-1 ,    %1111111000000000 h, %0001100000000000 h,
  ' p-add-reg-2 ,    %1111111100000000 h, %0100010000000000 h,
  \ ' p-add-sp-imm-1 , %1111100000000000 h, %1010100000000000 h,
  \ ' p-add-sp-imm-2 , %1111111110000000 h, %1011000000000000 h,
  \ ' p-add-sp-reg-1 , %1111111101111000 h, %0100010001101000 h,
  \ ' p-add-sp-reg-2 , %1111111110000111 h, %0100010010000101 h,
  ' p-adr-1 ,        %1111000000000000 h, %1010000000000000 h,
  ' p-and-reg-1 ,    %1111111111000000 h, %0100000000000000 h,
  ' p-asr-imm-1 ,    %1111100000000000 h, %0001000000000000 h,
  ' p-asr-reg-1 ,    %1111111111000000 h, %0100000100000000 h,
  ' p-b-1 ,          %1111000000000000 h, %1101000000000000 h,
  ' p-b-2 ,          %1111100000000000 h, %1110000000000000 h,
  ' p-bic-reg-1 ,    %1111111111000000 h, %0100001110000000 h,
  ' p-bkpt ,         %1111111100000000 h, %1011111000000000 h,
  ' p-blx-reg ,      %1111111110000000 h, %0100011110000000 h,
  ' p-bx ,           %1111111110000000 h, %0100011100000000 h,
  ' p-cbnz ,         %1111110100000000 h, %1011100100000000 h,
  ' p-cbz ,          %1111110100000000 h, %1011000100000000 h,
  ' p-cmn-reg-1 ,    %1111111111000000 h, %0100001011000000 h,
  ' p-cmp-imm-1 ,    %1111100000000000 h, %0010100000000000 h,
  ' p-cmp-reg-1 ,    %1111111111000000 h, %0100001010000000 h,
  ' p-cmp-reg-2 ,    %1111111100000000 h, %0100010100000000 h,
  ' p-cps-1 ,        %1111111111101000 h, %1011011001100000 h,
  ' p-eor-reg-1 ,    %1111111111000000 h, %0100000001000000 h,
  ' p-ldmia-1 ,      %1111100000000000 h, %1100100000000000 h,
  ' p-ldr-imm-1-w ,  %1111100000000000 h, %0110100000000000 h,
  ' p-ldr-imm-2-w ,  %1111100000000000 h, %1001100000000000 h,
  ' p-ldr-lit-1-w ,  %1111100000000000 h, %0100100000000000 h,
  ' p-ldr-reg-1-w ,  %1111111000000000 h, %0101100000000000 h,
  ' p-ldr-imm-1-b ,  %1111100000000000 h, %0111100000000000 h,
  ' p-ldr-reg-1-b ,  %1111111000000000 h, %0101110000000000 h,
  ' p-ldr-imm-1-h ,  %1111100000000000 h, %1000100000000000 h,
  ' p-ldr-reg-1-h ,  %1111111000000000 h, %0101101000000000 h,
  ' p-mov-reg-2 ,    %1111111111000000 h, %0000000000000000 h,
  ' p-lsl-imm-1 ,    %1111100000000000 h, %0000000000000000 h,
  ' p-lsl-reg-1 ,    %1111111111000000 h, %0100000010000000 h,
  ' p-lsr-imm-1 ,    %1111100000000000 h, %0000100000000000 h,
  ' p-lsr-reg-1 ,    %1111111111000000 h, %0100000011000000 h,
  ' p-mov-imm-1 ,    %1111100000000000 h, %0010000000000000 h,
  ' p-mov-reg-1 ,    %1111111100000000 h, %0100011000000000 h,
  ' p-mul-1 ,        %1111111111000000 h, %0100001101000000 h,
  ' p-mvn-reg-1 ,    %1111111111000000 h, %0100001111000000 h,
  ' p-nop-1 ,        %1111111111111111 h, %1011111100000000 h,
  ' p-orr-reg-1 ,    %1111111111000000 h, %0100001100000000 h,
  ' p-pop-1 ,        %1111111000000000 h, %1011110000000000 h,
  ' p-push-1 ,       %1111111000000000 h, %1011010000000000 h,
  ' p-rsb-imm-1 ,    %1111111111000000 h, %0100001001000000 h,
  ' p-sbc-reg-1 ,    %1111111111000000 h, %0100000110000000 h,
  ' p-str-imm-1-w ,  %1111100000000000 h, %0110000000000000 h,
  ' p-str-imm-2-w ,  %1111100000000000 h, %1001000000000000 h,
  ' p-str-reg-1-w ,  %1111111000000000 h, %0101000000000000 h,
  ' p-str-imm-1-b ,  %1111100000000000 h, %0111000000000000 h,
  ' p-str-reg-1-b ,  %1111111000000000 h, %0101010000000000 h,
  ' p-str-imm-1-h ,  %1111100000000000 h, %1000000000000000 h,
  ' p-str-reg-1-h ,  %1111111000000000 h, %0101001000000000 h,
  ' p-sub-imm-1 ,    %1111111000000000 h, %0001111000000000 h,
  ' p-sub-imm-2 ,    %1111100000000000 h, %0011100000000000 h,
  ' p-sub-reg-1 ,    %1111111000000000 h, %0001101000000000 h,
  ' p-svc ,          %1111111100000000 h, %1101111100000000 h,
  ' p-tst-reg-1 ,    %1111111111000000 h, %0100001000000000 h,
  ' p-wfi-1 ,        %1111111111111111 h, %1011111100110000 h,
  0 ,

  \ All the 32-bit ops
  create all-ops32

  thumb-2 [if]
    
    ' p-adc-imm , %1111101111100000 h, highest h, %1111000101000000 h, 0 h,
    ' p-adc-reg-2 , %1111111111100000 h, 0 h, %1110101101000000 h, 0 h,
    ' p-add-imm-3 , %1111101111100000 h, highest h, %1111000100000000 h, 0 h,
    ' p-add-imm-4 , %1111101111110000 h, highest h, %1111001000000000 h, 0 h,
    ' p-add-reg-3 , %1111111111100000 h, 0 h, %1110101100000000 h, 0 h,
    \ ' p-add-sp-imm-3 , %1111101111101111 h, highest h, %1111000100001101 h, 0 h,
    \ ' p-add-sp-imm-4 , %1111101111111111 h, highest h, %1111001000001101 h, 0 h,
    \ ' p-add-sp-reg-3 , %1111111111101111 h, highest h, %1110101100001101 h, 0 h,
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

  [then]
    
  ' p-bl-imm , %1111100000000000 h, %1101000000000000 h,
  %1111000000000000 h, %1101000000000000 h,

  thumb-2 [if]
    
    \ ' p-cdp ,
    \ ' p-cdp2 ,
    \ ' p-clrex ,
    \ ' p-clz ,
    ' p-cmn-imm , %1111101111110000 h, %1000111100000000 h,
    %1111000100010000 h, %0000111100000000 h,
    ' p-cmn-reg-2 , %1111111111110000 h, %0000111100000000 h,
    %1110101100010000 h, %0000111100000000 h,
    ' p-cmp-imm-2 , %1111101111110000 h, %1000111100000000 h,
    %1111000110110000 h, %0000111100000000 h,
    ' p-cmp-reg-3 , %1111111111110000 h, %0000111100000000 h,
    %1110101110110000 h, %0000111100000000 h,
    \ ' p-cps-2 ,
    \ ' p-dbg ,

  [then]
  
  ' p-dmb , %1111111111110000 h, %1101000011110000 h,
  %1111001110110000 h, %1000000001010000 h,
  ' p-dsb , %1111111111110000 h, %1101000011110000 h,
  %1111001110110000 h, %1000000001000000 h,

  thumb-2 [if]
    
    ' p-eor-imm , %1111101111100000 h, highest h, %1111000010000000 h, 0 h,
    ' p-eor-reg-2 , %1111111111100000 h, 0 h, %1110101010000000 h, 0 h,

  [then]
  
  ' p-isb , %1111111111110000 h, %1101000011110000 h,
  %1111001110110000 h, %1000000001100000 h,

  thumb-2 [if]
    
    \ ' p-it ,
    \ ' p-ldc ,
    \ ' p-ldmdb ,
    \ ' p-ldmia-2 , %1111111111010000 h, 0 h, %1110100010010000 h, 0 h,
    ' p-ldr-imm-3-w , %1111111111110000 h, 0 h, %1111100011010000 h, 0 h,
    ' p-ldr-imm-4-w , %1111111111110000 h, %0000100000000000 h,
    %1111100001010000 h, %0000100000000000 h,
    ' p-ldr-lit-2-w , %1111111101111111 h, 0 h, %1111100001011111 h, 0 h,
    ' p-ldr-reg-2-w , %1111111111110000 h, %0000111111000000 h,
    %1111100001010000 h, 0 h,
    ' p-ldr-imm-3-b , %1111111111110000 h, 0 h, %1111100010010000 h, 0 h,
    ' p-ldr-imm-4-b , %1111111111110000 h, %0000100000000000 h,
    %1111100000010000 h, %0000100000000000 h,
    ' p-ldr-lit-2-b , %1111111101111111 h, 0 h, %1111100000011111 h, 0 h,
    ' p-ldr-reg-2-b , %1111111111110000 h, %0000111111000000 h,
    %1111100001010000 h, 0 h,
    \ ' p-ldrbt ,
    \ ' p-ldrd ,
    \ ' p-ldrex ,
    \ ' p-ldrexb ,
    \ ' p-ldrexd ,
    \ ' p-ldrexh ,
    ' p-ldr-imm-3-h , %1111111111110000 h, 0 h, %1111100010110000 h, 0 h,
    ' p-ldr-imm-4-h , %1111111111110000 h, %0000100000000000 h,
    %1111100000110000 h, %0000100000000000 h,
    ' p-ldr-lit-2-h , %1111111101111111 h, 0 h, %1111100000111111 h, 0 h,
    ' p-ldr-reg-2-h , %1111111111110000 h, %0000111111000000 h,
    %1111100000110000 h, 0 h,
    \ ' p-ldrht ,
    \ ' p-ldrsb ,
    \ ' p-ldrsbt ,
    \ ' p-ldrsh ,
    \ ' p-ldrsht ,
    \ ' p-ldrt ,
    ' p-lsl-imm-2 , %1111111111101111 h, %0000000000110000 h,
    %1110101001001111 h, 0 h,
    ' p-lsl-reg-2 , %1111111111100000 h, %1111000011110000 h,
    %1111101000000000 h, %1111000000000000 h,
    ' p-lsr-imm-2 , %1111111111101111 h, %0000000000110000 h,
    %1110101001001111 h, %0000000000010000 h,
    ' p-lsr-reg-2 , %1111111111100000 h, %1111000011110000 h,
    %1111101000100000 h, %1111000000000000 h,
    \ ' p-mcr ,
    \ ' p-mcrr ,
    \ ' p-mla ,
    ' p-mls , %1111111111110000 h, %0000000011110000 h,
    %1111101100000000 h, %0000000000010000 h,
    ' p-mov-imm-2 , %1111101111101111 h, highest h,
    %1111000001001111 h, 0 h,
    ' p-mov-imm-3 , %1111101111110000 h, highest h,
    %1111001001000000 h, 0 h,
    ' p-mov-reg-3 , %1111111111101111 h, %0111000011110000 h,
    %1110101001001111 h, 0 h,
    ' p-movt , %1111101111110000 h, highest h,
    %1111001011000000 h, 0 h,
    \ ' p-mrc ,
    \ ' p-mrrc ,
    \ ' p-mrs ,
    \ ' p-msr ,
    ' p-mul-2 , %1111111111110000 h, %1111000011110000 h,
    %1111101100000000 h, %1111000000000000 h,

    \ ' p-mvn-imm ,
    \ ' p-mvn-reg-2 , %1111111111101111 h, 0 h, %1110101001101111 h, 0 h,
    \ ' p-nop-3 ,
    \ ' p-orn-imm ,
    \ ' p-orn-reg ,
    ' p-orr-imm , %1111101111100000 h, highest h,
    %1111000001000000 h, 0 h,
    ' p-orr-reg-2 , %1111111111100000 h, 0 h, %1110101001000000 h, 0 h,
    \ ' p-pkh ,
    \ ' p-pld-imm ,
    \ ' p-pld-reg ,
    \ ' p-pli-imm ,
    \ ' p-pli-reg ,
    ' p-pop-2 , %1111111111111111 h, 0 h, %1110100010111101 h, 0 h,
    ' p-push-2 , %1111111111111111 h, 0 h, %1110100100101101 h, 0 h,
    \ ' p-qadd ,
    \ ' p-qadd16 ,
    \ ' p-qadd8 ,
    \ ' p-qasx ,
    \ ' p-qdadd ,
    \ ' p-qdsub ,
    \ ' p-qsax ,
    \ ' p-qsub ,
    \ ' p-qsub16 ,
    \ ' p-qsub8 ,
    \ ' p-rbit
    \ ' p-rev ,
    \ ' p-rev16 ,
    \ ' p-revsh
    \ ' p-rfe ,
    \ ' p-ror-imm ,
    \ ' p-ror-reg ,
    \ ' p-rrx ,
    \ ' p-rsb-imm-2 ,
    \ ' p-rsb-reg ,
    \ ' p-sadd16 ,
    \ ' p-sadd8 ,
    \ ' p-sasx .
    ' p-sbc-imm , %1111101111100000 h, highest h, %1111000101100000 h, 0 h,
    ' p-sbc-reg-2 , %1111111111100000 h, 0 h, %1110101101100000 h, 0 h,
    \ ' p-sbfx ,
    ' p-sdiv , %1111111111110000 h, %0000000011110000 h,
    %1111101110010000 h, %0000000011110000 h,
    \ ' p-sel ,
    \ ' p-setend ,
    \ ' p-sev ,
    \ ' p-shadd16 ,
    \ ' p-shadd8 ,
    \ ' p-shasx ,
    \ ' p-shsax ,
    \ ' p-shsub16 ,
    \ ' p-shsub8 ,
    \ ' p-smi ,
    \ ' p-smla* ,
    \ ' p-smlal* ,
    \ ' p-smlaw* ,
    \ ' p-smlsd ,
    \ ' p-smlsld ,
    \ ' p-smmla ,
    \ ' p-smmls ,
    \ ' p-smmul ,
    \ ' p-smuad ,
    \ ' p-smul* ,
    ' p-smull , %1111111111110000 h, %0000000011110000 h,
    %1111101110000000 h, 0 h,
    \ ' p-smulw* ,
    \ ' p-smusd ,
    \ ' p-srs* ,
    \ ' p-ssat ,
    \ ' p-ssat16 ,
    \ ' p-ssax ,
    \ ' p-ssub16 ,
    \ ' p-ssub8 ,
    \ ' p-stc ,
    \ ' p-stmdb ,
    \ ' p-stmia ,
    ' p-str-imm-3-w , %1111111111110000 h, 0 h, %1111100011000000 h, 0 h,
    ' p-str-imm-4-w , %1111111111110000 h, %0000100000000000 h,
    %1111100001000000 h, %0000100000000000 h,
    ' p-str-reg-2-w , %1111111111110000 h, %0000111111000000 h,
    %1111100001000000 h, 0 h,
    ' p-str-imm-3-b , %1111111111110000 h, 0 h, %1111100010000000 h, 0 h,
    ' p-str-imm-4-b , %1111111111110000 h, %0000100000000000 h,
    %1111100000000000 h, %0000100000000000 h,
    ' p-str-reg-2-b , %1111111111110000 h, %0000111111000000 h,
    %1111100000000000 h, 0 h,
    \ ' p-strbt ,
    \ ' p-strd ,
    \ ' p-strex ,
    \ ' p-strexb ,
    \ ' p-strexd ,
    \ ' p-strexh ,
    ' p-str-imm-3-h , %1111111111110000 h, 0 h, %1111100010100000 h, 0 h,
    ' p-str-imm-4-h , %1111111111110000 h, %0000100000000000 h,
    %1111100000100000 h, %0000100000000000 h,
    ' p-str-reg-2-h , %1111111111110000 h, %0000111111000000 h,
    %1111100000100000 h, 0 h,
    \ ' p-strht ,
    \ ' p-strt ,
    ' p-sub-imm-3 , %1111101111100000 h, highest h, %1111000110100000 0 h,
    ' p-sub-imm-4 , %1111101111110000 h, highest h, %1111001010100000 0 h,
    ' p-sub-reg-2 , %1111111111100000 h, 0 h, %1110101110100000 h, 0 h,
    \ ' p-sub-sp-imm ,
    \ ' p-sub-sp-reg ,
    \ ' p-sub-pc-lr ,
    \ ' p-svc ,
    \ ' p-sxtab ,
    \ ' p-sxtab16 ,
    \ ' p-sxtah ,
    \ ' p-sxtb ,
    \ ' p-sxtb16 ,
    \ ' p-sxth ,
    \ ' p-tbb ,
    \ ' p-tbh ,
    \ ' p-teq ,
    \ ' p-teqh ,
    ' p-tst-imm , %1111101111110000 h, %10001111000000 h,
    %1111000000010000 h, %00001111000000 h,
    ' p-tst-reg-2 , %1111111111110000 h, %0000111100000000 h,
    %1110101000010000 h, %0000111100000000 h,
    \ ' p-uadd16 ,
    \ ' p-uadd8 ,
    \ ' p-uasx ,
    \ ' p-ubfx ,
    ' p-udiv , %1111111111110000 h, %0000000011110000 h,
    %1111101110110000 h, %0000000011110000 h,
    \ ' p-uhadd16 ,
    \ ' p-uhadd8 ,
    \ ' p-uhasx ,
    \ ' p-uhsax ,
    \ ' p-uhsub16 ,
    \ ' p-uhsub8 ,
    \ ' p-umaal ,
    \ ' p-umlal ,
    ' p-umull , %1111111111110000 h, %0000000011110000 h,
    %1111101110100000 h, 0 h,
    \ ' p-uqadd16 ,
    \ ' p-uqadd8 ,
    \ ' p-uqasx ,
    \ ' p-uqsax ,
    \ ' p-uqsub16 ,
    \ ' p-uqsub8 ,
    \ ' p-usad8 ,
    \ ' p-usasa8 ,
    \ ' p-usat ,
    \ ' p-usat16 ,
    \ ' p-usax ,
    \ ' p-usub16 ,
    \ ' p-usub8 ,
    \ ' p-uxtab ,
    \ ' p-uxtab16
    \ ' p-uxtah ,
    \ ' p-uxtb ,
    \ ' p-uxtb16 ,
    \ ' p-uxth ,
    \ ' p-wfe ,
    \ ' p-wfi-2 ,
    \ ' p-yield ,

  [then]

  0 ,

  \ Commit to flash
  commit-flash

  \ Get condition
  : current-cond ( -- cond ) -1 ;

  \ Disassemble a 16-bit instruction
  : disassemble16 ( op-addr handler-addr -- match )
    over h@ over cell+ h@ and over 6 + h@ = if
      current-cond 2 pick h@ instr16.
      2 pick label. 2 pick h@ rot @ execute true
    else
      2drop false
    then
  ;

  \ Disassemble a 32-bit instruction
  : disassemble32 ( op-addr handler-addr -- match )
    over h@ over cell+ h@ and over 8 + h@ = if
      over 2+ h@ over 6 + h@ and over 10 + h@ = if
	current-cond 2 pick h@ 3 pick 2+ h@ instr32.
	2 pick label. 2 pick h@ 3 pick 2+ h@ 3 roll @ execute true
      else
	2drop false
      then
    else
      2drop false
    then
  ;

  \ Add a local label
  : add-local ( addr -- )
    local-index @ local-count < if
      local-index @ 0 ?do
	i cells local-buffer + @ over = if
	  drop unloop exit
	then
      loop
      local-index @ cells local-buffer + ! 1 local-index +!
    else
      drop
    then
  ;

  \ Parse a 16-bit instruction to find local labels
  : parse-local16 ( op-addr low -- )
    dup %1111000000000000 and %1101000000000000 = if
      0_8_bf 1 lshift swap 4+ swap 9 extend + add-local
    else
      dup %1111100000000000 and %1110000000000000 = if
	0 11 bitfield 1 lshift swap 4+ swap 12 extend + add-local
      else
	2drop
      then
    then
  ;

  \ Parse a 32-bit instruction to find local labels
  : parse-local32 ( op-addr low high -- )
    over %1111100000000000 and %1111000000000000 = if
      dup %1101000000000000 and %1000000000000000 = if
	dup 0 11 bitfield 2 pick 0 6 bitfield 11 lshift or
	over 13 1 bitfield 17 lshift or swap 11 1 bitfield 18 lshift or
	swap 10_1_bf 19 lshift or 1 lshift swap 4+ swap 21 extend + add-local
      else
	dup %1101000000000000 and %1001000000000000 = if
	  dup 0 11 bitfield 2 pick 0 10 bitfield 11 lshift or
	  over 11 1 bitfield 3 pick 10_1_bf xor not 1 and 21 lshift or
	  swap 13 1 bitfield 2 pick 10_1_bf xor not 1 and 22 lshift or
	  swap 10_1_bf 23 lshift or 1 lshift swap 4+ swap 25 extend + add-local
	else
	  2drop drop
	then
      then
    else
      2drop drop
    then
  ;

  \ Find a local label for a 16-bit instruction
  : find-local16 ( op-addr handler-addr -- match )
    over h@ over cell+ h@ and over 6 + h@ = if
      drop dup h@ parse-local16 true
    else
      2drop false
    then
  ;

  \ Find a local label for a 32-bit instruction
  : find-local32 ( op-addr handler-addr -- match )
    over h@ over cell+ h@ and over 8 + h@ = if
      over 2+ h@ over 6 + h@ and over 10 + h@ = if
	drop dup h@ over 2+ h@ parse-local32 true
      else
	2drop false
      then
    else
      2drop false
    then
  ;

  \ Commit to flash
  commit-flash

  \ The body of finding local labels
  : find-local ( addr -- addr )
    all-ops16 begin
      dup @ 0<> if
	2dup find-local16 if
	  drop 2+ true true
	else
	  [ 2 cells ] literal + false
	then
      else
	drop false true
      then
    until
    not if
      all-ops32 begin
	dup @ 0<> if
	  2dup find-local32 if
	    drop 4+ true true
	  else
	    [ 3 cells ] literal + false
	  then
	else
	  drop false true
	then
      until
      not if
	2+
      then
    then
  ;
  
  \ The body of disassembly
  : disassemble-main ( addr -- addr )
    for-gas @ not if dup h.8 space then
    all-ops16 begin
      dup @ 0<> if
	2dup disassemble16 if
	  drop 2+ true true
	else
	  [ 2 cells ] literal + false
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
	    [ 3 cells ] literal + false
	  then
	else
	  drop false true
	then
      until
      not if
	dup h@ instr16. ." ????" 2+
      then
    then
    cr
  ;

  \ Get whether an instruction is the end token
  : see-end? ( addr -- flag )
    dup h@ $003F <> if
      drop false
    else
      2- h@ dup $FF00 and $BD00 = if
	drop true
      else
	$FF80 and $4700 =
      then
    then
  ;

  \ Commit to flash
  commit-flash
  
end-module
  
\ Disassemble instructions
: disassemble ( start end -- )
  false for-gas !
  0 local-index !
  2dup swap begin 2dup swap u< while find-local repeat 2drop
  cr swap begin 2dup swap u< while disassemble-main repeat 2drop
;

\ Disassemble instructions for GAS
: disassemble-for-gas ( start end -- )
  true for-gas !
  0 local-index !
  2dup swap begin 2dup swap u< while find-local repeat 2drop
  cr swap begin 2dup swap u< while disassemble-main repeat 2drop
;

\ Disassemble a word by its xt
: see-xt ( xt -- )
  false for-gas !
  0 local-index !
  dup begin dup see-end? not while find-local repeat drop
  cr begin dup see-end? not while disassemble-main repeat drop
;

\ Disassemble a word by its xt for GAS
: see-xt-for-gas ( xt -- )
  true for-gas !
  0 local-index !
  dup begin dup see-end? not while find-local repeat drop
  cr begin dup see-end? not while disassemble-main repeat drop
;

\ SEE a word
: see ( "name" -- ) token-word >body see-xt ;

\ SEE a word for GAS
: see-for-gas ( "name" -- ) token-word >body see-xt-for-gas ;

\ Finish compressing the code
end-compress-flash

unimport disassemble-internal-module

\ Reboot
reboot
