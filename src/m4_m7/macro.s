@ Copyright (c) 2019-2020 Travis Bemann
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.

	@@ Aliases for data stack and top of stack registers
tos 	.req r6
dp 	.req r7
	
	@@ True value
	.equ true_value, -1

	@@ False value
	.equ false_value, 0
	
	@@ Invisible word
	.equ invisible_flag, 0x0000

	@@ Visible word
	.equ visible_flag, 0x0001

	@@ Immediate word
	.equ immediate_flag, 0x0002

	@@ Compile-only word
	.equ compiled_flag, 0x0004

	@@ Inlined word
	.equ inlined_flag, 0x0008

	@@ Folded word
	.equ fold_flag, 0x0010
	
	@@ The internal wordlist
	.equ internal, 1

	@@ The maximum wordlist order size
	.equ max_order_size, 16

	@@ Initialize the current RAM pointer
	.set ram_current, ram_start
	
	@@ Allot space in RAM
	.macro allot name, size
	.equ \name, ram_current
	.set ram_current, ram_current + \size
	.endm

	@@ Finish an inlined word
	.macro end_inlined
	.hword 0x003F @@ movs r7, r7
	.endm
	
	@@ Word header macro
	.macro define_word name, flags
	.p2align 2
	.hword \flags
	.hword 0
	.word 10b - 8
10:	.byte 12f - 11f
11:	.ascii "\name"
12:	.p2align 1
	.endm

	@@ Internal word header macro
	.macro define_internal_word name, flags
	.p2align 2
	.hword \flags
	.hword internal
	.word 10b - 8
10:	.byte 12f - 11f
11:	.ascii "\name"
12:	.p2align 1
	.endm

	@@ Push the top of the stack onto the data stack
	.macro push_tos
@	stmdb dp!, {tos}
	str tos, [dp, #-4]!
	.endm

	@@ Push a register onto the data stack
	.macro push_reg reg
@	stmdb dp!, {\reg}
	str \reg, [dp, #-4]!
	.endm

	@@ Push a constant onto the top of the stack
	.macro push_const const
	ldr tos, =\const
@	stmdb dp!, {tos}
	str tos, [dp, #-4]!
	.endm

	@@ Pull the top of the stack into the TOS register
	.macro pull_tos
	ldmia dp!, {tos}
	.endm

	@@ String macro
	.macro cstring text, dest
	adr \dest, 11f
	b 14f
11:	.byte 13f - 12f
12:	.ascii "\text"
13:	.p2align 1
14:	nop
	.endm

	@@ String with newline
	.macro cstring_ln text, dest
	adr \dest, 11f
	b 14f
11:	.byte 13f - 12f
12:	.ascii "\text\r\n"
13:	.p2align 1
14:	nop
	.endm

	@@ Push a string onto the stack macro
	.macro string text
	push {r0}
	cstring "\text", r0
	push_tos
	movs tos, r0
	adds tos, #1
	push_tos
	ldrb tos, [r0]
	pop {r0}
	.endm

	@@ Push a string onto the stack macro
	.macro string_ln text
	push {r0}
	cstring_ln "\text", r0
	push_tos
	movs tos, r0
	adds tos, #1
	push_tos
	ldrb tos, [r0]
	pop {r0}
	.endm
