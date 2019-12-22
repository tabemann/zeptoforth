@ Copyright (c) 2019, Travis Bemann
@ All rights reserved.
@ 
@ Redistribution and use in source and binary forms, with or without
@ modification, are permitted provided that the following conditions are met:
@ 
@ 1. Redistributions of source code must retain the above copyright notice,
@    this list of conditions and the following disclaimer.
@ 
@ 2. Redistributions in binary form must reproduce the above copyright notice,
@    this list of conditions and the following disclaimer in the documentation
@    and/or other materials provided with the distribution.
@ 
@ 3. Neither the name of the copyright holder nor the names of its
@    contributors may be used to endorse or promote products derived from
@    this software without specific prior written permission.
@ 
@ THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
@ AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
@ IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
@ ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
@ LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
@ CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
@ SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
@ INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
@ CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
@ ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
@ POSSIBILITY OF SUCH DAMAGE.

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
	.p2align 1
	.hword \flags
10:	.word 10b
	.byte 12f - 11f
11:	.ascii "\name"
12:	.p2align 1
	.endm
	
	@@ String macro
	.macro string text, dest
	ldr \dest, =11f
	b 14f
11:	.byte 13f - 12f
12:	.ascii "\text"
13:	.p2align 1
14:	nop
	.endm

	@@ String with newline
	.macro string_ln text, dest
	ldr \dest, =11f
	b 14f
11:	.byte 13f - 12f
12:	.ascii "\text\r\n"
13:	.p2align 1
14:	nop
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
