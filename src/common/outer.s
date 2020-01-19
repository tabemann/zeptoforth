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
