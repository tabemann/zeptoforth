@ Copyright (c) 2021 Travis Bemann
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

	.equ SIO_DIV_UDIVIDEND_OFFSET, 0x60
	.equ SIO_DIV_UDIVISOR_OFFSET, 0x64
	.equ SIO_DIV_SDIVIDEND_OFFSET, 0x68
	.equ SIO_DIV_SDIVISOR_OFFSET, 0x6C
	.equ SIO_DIV_QUOTIENT_OFFSET, 0x70
	.equ SIO_DIV_REMAINDER_OFFSET, 0x74

	@@ Signed division of two two's complement integers
	define_word "/", visible_flag
_div:	push {lr}
	bl _divmod
	adds dp, #4
	pop {pc}
	end_inlined

	@@ Unsigned division of two integers
	define_word "u/", visible_flag
_udiv:	push {lr}
	bl _udivmod
	adds dp, #4
	pop {pc}
	end_inlined

	@@ Signed modulus of two two's complement integers
	define_word "mod", visible_flag
_mod:	push {lr}
	bl _divmod
	pull_tos
	pop {pc}
	end_inlined

	@@ Unsigned modulus of two unsigned integers
	define_word "umod", visible_flag
_umod:	push {lr}
	bl _udivmod
	pull_tos
	pop {pc}
	end_inlined

	@@ Unsigned division with modulus ( u1 u2 -- remainder quotient )
	define_word "u/mod", visible_flag
_udivmod:
	ldr r0, =SIO_BASE
	cpsid i
	ldr r1, [dp]
	str r1, [r0, #SIO_DIV_UDIVIDEND_OFFSET]
	str tos, [r0, #SIO_DIV_UDIVISOR_OFFSET]
	@@ Wait eight cycles
	b 1f
1:	b 1f
1:	b 1f
1:	b 1f
1:	ldr r1, [r0, #SIO_DIV_REMAINDER_OFFSET]
	ldr tos, [r0, #SIO_DIV_QUOTIENT_OFFSET]
	str r1, [dp]
	cpsie i
	bx lr
	end_inlined

	@@ Signed division with modulus ( u1 u2 -- remainder quotient )
	define_word "/mod", visible_flag
_divmod:
	ldr r0, =SIO_BASE
	cpsid i
	ldr r1, [dp]
	str r1, [r0, #SIO_DIV_SDIVIDEND_OFFSET]
	str tos, [r0, #SIO_DIV_SDIVISOR_OFFSET]
	@@ Wait eight cycles
	b 1f
1:	b 1f
1:	b 1f
1:	b 1f
1:	ldr r1, [r0, #SIO_DIV_REMAINDER_OFFSET]
	ldr tos, [r0, #SIO_DIV_QUOTIENT_OFFSET]
	str r1, [dp]
	cpsie i
	bx lr
	end_inlined

	.ltorg
	
