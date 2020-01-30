@ Copyright (c) 2019 Travis Bemann
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
	.equ compile_only_flag, 0x0004

	@@ Inlined word
	.equ inlined_flag, 0x0008

	@@ Initialize the current RAM pointer
	.set ram_current, ram_start
	
	@@ Allot space in RAM
	.macro allot name, size
	.equ \name, ram_current
	.set ram_current, ram_current + \size
	.endm
	
	@@ Word header macro
	.macro define_word name, flags
	.p2align 2
	.word \flags
10:	.word 10b - 4
	.byte 12f - 11f
11:	.ascii "\name"
12:	.p2align 1
	.endm
	
	@@ Push the top of the stack onto the data stack
	.macro push_tos
	str tos, [dp, #-4]!
	.endm

	@@ Push a constant onto the top of the stack
	.macro push_const const
	push_tos
	ldr tos, =\const
	.endm

	@@ Pull the top of the stack into the TOS register
	.macro pull_tos
	ldr tos, [dp], #4
	.endm

	@@ String macro
	.macro cstring text, dest
	ldr \dest, =11f
	b 14f
11:	.byte 13f - 12f
12:	.ascii "\text"
13:	.p2align 1
14:	nop
	.endm

	@@ String with newline
	.macro cstring_ln text, dest
	ldr \dest, =11f
	b 14f
11:	.byte 13f - 12f
12:	.ascii "\text\r\n"
13:	.p2align 1
14:	nop
	.endm

	@@ Push a string onto the stack macro
	.macro string text
	push_tos
	ldr tos, =11f
	push_tos
	ldr tos, =12f - 11f
	b 13f
11:	.ascii "\text"
12:	.p2align 1
13:	nop.endm

	@@ Push a string onto the stack macro
	.macro string_ln text
	push_tos
	ldr tos, =11f
	push_tos
	ldr tos, =12f - 11f
	b 13f
11:	.ascii "\text\r\n"
12:	.p2align 1
13:	nop
	.endm

	@@ Blank initial word header
	.p2align 2
	.word 0
10:	.word 0
	.byte 12f - 11f
11:	.ascii "*blank*"
12:	.p2align 1
	.endm
