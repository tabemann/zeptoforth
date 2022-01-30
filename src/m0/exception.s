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

	@@ Raise an exception with the exception type in the TOS register
	define_word "?raise", visible_flag
_raise:	cmp tos, #0
	beq 1f
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #handler_offset
	pull_tos
	ldr r1, [r0]
	mov sp, r1
	pop {r1}
	str r1, [r0]
	pop {dp, pc}
1:	pull_tos
	bx lr
	end_inlined

	@@ Try to see if an exception occurs
	define_word "try", visible_flag
_try:	push {lr} @ #0
	bl _cpu_offset
	ldr r2, =dict_base
	adds r2, tos
	ldr r2, [r2]
	adds r2, #handler_offset
	pull_tos
	mov r1, sp
	subs r1, #4
	mov sp, r1
	str dp, [r1]
	movs r0, r2
	ldr r0, [r0]
	mov r1, sp
	subs r1, #4
	mov sp, r1
	str r0, [r1]
	movs r0, r2
	mov r2, sp
	str r2, [r0]
	mov r0, tos
	adds r0, #1 @ Commented out to deal with an issue with Cutter @@@
	pull_tos
	blx r0
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #handler_offset
	pull_tos
	pop {r1}
	str r1, [r0]
	pop {r1}
	push_tos
	movs tos, #0
	pop {pc}
	end_inlined
	
	.ltorg
	
