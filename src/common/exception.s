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

	@@ Raise an exception with the exception type in the TOS register
	define_word "?raise", visible_flag
_raise:	movs r5, #0
	cmp tos, r5
	beq 1f
	ldr r0, =handler
	ldr sp, [r0]
	ldr r1, [sp], #4
	str r1, [r0]
	adds sp, #4
	ldr dp, [sp], #4
	pop {pc}
1:	bx lr

	@@ Try to see if an exception occurs
	define_word "try", visible_flag
_try:	push {lr}
	str dp, [sp, #-4]!
	str r0, [sp, #-4]!
	ldr r0, =handler
	ldr r0, [r0]
	str r0, [sp, #-4]!
	ldr r0, =handler
	str sp, [r0]
	mov r0, tos
	adds r0, #1
	pull_tos
	blx r0
	ldr r0, =handler
	str r1, [dp, #-4]!
	ldr r1, [sp], #4
	str r1, [r0]
	ldr r1, [dp], #4
	ldr r0, [sp], #8
	push_const 0
	pop {pc}
	
