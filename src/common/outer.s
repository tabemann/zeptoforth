@ Copyright (c) 2019-2020 Travis Bemann
@
@ This program is free software: you can redistribute it and/or modify
@ it under the terms of the GNU General Public License as published by
@ the Free Software Foundation, either version 3 of the License, or
@ (at your option) any later version.
@
@ This program is distributed in the hope that it will be useful,
@ but WITHOUT ANY WARRANTY; without even the implied warranty of
@ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@ GNU General Public License for more details.
@
@ You should have received a copy of the GNU General Public License
@ along with this program.  If not, see <http://www.gnu.org/licenses/>.

	@@ Test whether a character is whitespace.
	define_word "ws?", visible_flag
_ws_q:	cmp tos, #0x09
	beq 1f
	cmp tos, #0x0A
	beq 1f
	cmp tos, #0x0D
	beq 1f
	cmp tos, #0x20
	beq 1f
	movs tos, #0
	bx lr
1:	movs tos, #-1
	bx lr

	@@ Parse the input buffer for the start of a token
	define_word "token-start", visible_flag
_token_start:
	push {lr}
	ldr r0, =index_buffer_index
	ldrh r1, [r0]
	push_tos
1:	ldr r0, =index_buffer_count
	ldrb r2, [r0]
	cmp r1, r2
	beq 2f
	ldr r0, =index_buffer
	adds r0, r0, r1
	ldrb tos, [r0]
	push {r0}
	bl _ws_q
	pop {r0}
	cmp tos, #0
	bne 2f
	adds r0, #1
	b 1b
2:	movs tos, r0
	pop {pc}

	@@ Parse the input buffer for the end of a token
	define_word "token-end", visible_flag
_token_end:
	push {lr}
	movs r0, tos
1:	ldr r0, =index_buffer_count
	ldrb r2, [r0]
	cmp r1, r2
	beq 2f
	ldr r0, =index_buffer
	adds r0, r0, r1
	ldrb tos, [r0]
	push {r0}
	bl _ws_q
	pop {r0}
	cmp tos, #0
	beq 2f
	adds r0, #1
	b 1b
2:	movs tos, r0
	pop {pc}

	@@ Parse a token
	define_word "token", visible_flag
_token:	push {lr}
	bl _token_start
	movs r0, tos
	push {r0}
	bl _token_end
	pop {r0}
	movs r1, tos
	ldr tos, =input_buffer
	adds tos, tos, r0
	push_tos
	subs tos, r1, r0
	ldr r0, =input_buffer_index
	strb r1, [r0]
	push {pc}
	
	@@ Convert a character to being uppercase
	define_word "to-upper-char", visible_flag
_to_upper_char:
	cmp tos, #0x61
	ble 1f
	cmp tos, #0x7A
	ble 1f
	subs tos, #0x20
1:	bx lr

	@@ Compare whether two strings are equal
	define_word "equal-case-strings?", visible_flag
_equal_case_strings:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r2, tos
	pull_tos
	movs r3, tos
1:	cmp r0, #0
	beq 2f
	cmp r2, #0
	beq 3f
	ldr tos, [r1]
	adds r1, #1
	bl _to_upper_char
	movs r4, tos
	ldr tos, [r3]
	adds r3, #1
	bl _to_upper_char
	cmp r4, tos
	beq 1b
	movs tos, #-1
	pop {pc}
2:	cmp r2, #0
	bne 3f
	movs tos, #-1
	pop {pc}
3:	movs tos, #0
	pop {pc}

	@@ Find a word in a specific dictionary
	@@ ( addr bytes mask dict -- addr|0 )
	define_word "find-dict", visible_flag
_find_dict:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r2, tos
	pull_tos
	movs r3, tos
	pull_tos
1:	cmp r0, #0
	beq 3f
	ldr r4, [r0]
	ands r4, r1
	cmp r4, r1
	bne 2f
	ldrb r4, [r0, #8]
	movs tos, r4
	push_tos
	movs tos, r0
	adds tos, #9
	push_tos
	movs tos, r2
	push_tos
	movs tos, r3
	push {r0, r1, r2, r3}
	bl _equal_case_strings
	pop {r0, r1, r2, r3}
	cmp tos, #0
	bne 4f
	pull_tos
2:	ldr r0, [r0, #4]
	subs r0, #4
	b 1b
3:	push_tos
	movs tos, #0
	pop {pc}
4:	movs tos, r0
	pop {pc}

	@@ Find a word in the dictionary
	@@ ( addr bytes mask -- addr|0 )
	define_word "find", visible_flag
_find:	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r2, tos
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	push_tos
	ldr r3, =ram_latest
	ldr tos, [r3]
	bl _find_dict
	cmp tos, #0
	bne 2f
	movs tos, r2
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	push_tos
	ldr r3, =flash_latest
	ldr tos, [r3]
	bl _flash_dict
	pop {pc}
1:	push_tos
	ldr r0, =flash_latest
	ldr tos, [r0]
	bl _find_dict
2:	pop {pc}

	@@ Get an xt from a word
	define_word ">xt", visible_flag
_to_xt:	push {lr}
	ldrb r0, [tos, #8]
	adds tos, #9
	adds tos, tos, r0
	movs r0, #1
	ands r0, tos
	bne 1f
	pop {pc}
1:	adds tos, #1
	pop {pc}

	@@ Get the STATE variable address
	define_word "state", visible_flag
_state:	push_tos
	ldr tos, =state
	bx lr

	@@ The inner loop of Forth
	define_word "quit", visible_flag
_quit:	ldr r0, =rstack_top
	mov sp, r0
1:	bl _token
	cmp tos, #0
	beq 2f
	movs r0, tos
	ldr r1, [dp]
	push_tos
	movs tos, #visible_flag
	push {r0, r1}
	bl _find
	pop {r0, r1}
	cmp tos, #0
	beq 3f
	ldr r0, =state
	ldr r0, [r0]
	bne r0, #0
	beq 4f
	ldr r0, [tos]
	movs r1, #compile_only_flag
	ands r0, r1
	bne 5f
6:	bl _to_xt
	adds tos, #1
	movs r0, tos
	pull_tos
	blx r0
	pop {pc}
2:	bl _refill
	b 1b
3:	movs tos, r1
	push_tos
	movs tos, r0
	bl _unknown_word
	b 1b
4:	ldr r0, [tos]
	movs r1, #immediate_flag
	ands r0, r1
	bne 6b
	bl _to_xt
	bl _asm_call
	b 1b
5:	ldr tos, =_not_compiling
	bl _raise
	b 1b
	
	@@ Start a colon definition
	define_word ":", visible_flag
_colon:	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
	bl _asm_start
	ldr r0, =latest_flags
	movs r1, #visible_flag
	str r1, [r0]
	pop {pc}
1:	push_tos
	ldr tos, =_token_expected
	bl _raise
	pop {pc}

	@@ End a colon definition
	define_word ";", visible_flag | immediate_flag
_semi:	push {lr}
	ldr r0, =state
	ldr r1, [r0]
	cmp r1, #0
	beq 1f
	movs r1, #0
	str r1, [r0]
	bl _asm_end
	pop {pc}
1:	push_tos
	ldr tos, =_not_compiling
	bl _raise
	pop {pc}

	@@ Token expected exception handler
	define_word "token-expected", visible_flag
_token_expected:
	b .

	@@ We are not currently compiling
	define_word "not-compiling", visible_flag
_not_compiling:
	b .
