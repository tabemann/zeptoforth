@ Copyright (c) 2019-2023 Travis Bemann
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

	@@ Signed division of two two's complement integers
	define_word "/", visible_flag | inlined_flag
_div:	movs r0, tos
	pull_tos
	sdiv tos, tos, r0
	bx lr
	end_inlined

	@@ Unsigned division of two integers
	define_word "u/", visible_flag | inlined_flag
_udiv:	movs r0, tos
	pull_tos
	udiv tos, tos, r0
	bx lr
	end_inlined

	@@ Signed modulus of two two's complement integers
	define_word "mod", visible_flag | inlined_flag
_mod:	movs r0, tos
	pull_tos
	sdiv r1, tos, r0
	mls tos, r1, r0, tos
	bx lr
	end_inlined

	@@ Unsigned modulus of two unsigned integers
	define_word "umod", visible_flag | inlined_flag
_umod:	movs r0, tos
	pull_tos
	udiv r1, tos, r0
	mls tos, r1, r0, tos
	bx lr
	end_inlined

	@@ Signed division and modulus of two two's complement integers
	@@ ( n1 n2 -- remainder quotient )
	define_word "/mod", visible_flag | inlined_flag
_divmod:
	movs r0, tos
	ldr r1, [dp]
	sdiv tos, r1, r0
	mls r2, tos, r0, r1
	str r2, [dp]
	bx lr
	end_inlined

	@@ Unsigned division and modulus of two unsigned integers
	@@ ( u1 u2 -- remainder quotient )
	define_word "u/mod", visible_flag | inlined_flag
_udivmod:
	movs r0, tos
	ldr r1, [dp]
	udiv tos, r1, r0
	mls r2, tos, r0, r1
	str r2, [dp]
	bx lr
	end_inlined

	
