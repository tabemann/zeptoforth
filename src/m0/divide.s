@ Copyright (c) 2021-2023 Travis Bemann
@ Copyright (c) 2013 Matthias Koch
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
	movs r1, tos
	pull_tos
	@ Catch divide by zero.
	cmp r1, #0
	bne 1f
	push_tos
	movs tos, #0
	bx lr    
1:
	@ Shift left the denominator until it is greater than the numerator
	movs r2, #1    @ Zähler
	movs r3, #0    @ Ergebnis
	cmp tos, r1
	bls 3f
	adds r1, #0    @ Don't shift if denominator would overflow
	bmi 3f
        
2:	lsls r2, r2, #1
	lsls r1, r1,  #1
	bmi 3f
	cmp tos, r1
	bhi 2b
	
3:	cmp tos, r1
	bcc 4f         @ if (num>denom)
	subs tos, r1     @ numerator -= denom
	orrs r3, r2      @ result(r3) |= bitmask(r2)
	
4:	lsrs r1, r1, #1    @ denom(r1) >>= 1
	lsrs r2, r2, #1    @ bitmask(r2) >>= 1
	bne 3b

	push_tos
	movs tos, r3
	bx lr
	end_inlined

	@@ Signed division with modulus ( u1 u2 -- remainder quotient )
	define_word "/mod", visible_flag
_divmod:	
	push {lr}
	movs r0, tos @ Divisor
	pull_tos @     TOS: Dividend
	
	cmp tos, #0
	bge.n _divmod_plus
	rsbs tos, tos, #0
	
_divmod_minus:
	cmp r0, #0
	bge.n _divmod_minus_plus
	
_divmod_minus_minus:
	rsbs r0, r0, #0
	push_tos
	movs tos, r0
	bl _udivmod
	movs r0, tos
	pull_tos
	rsbs tos, tos, #0
	push_tos
	movs tos, r0
	pop {pc}
	
_divmod_minus_plus:
	push_tos
	movs tos, r0
	bl _udivmod
	movs r0, tos
	pull_tos
	rsbs r0, r0, #0
	rsbs tos, tos, #0
	push_tos
	movs tos, r0
	pop {pc}
	
_divmod_plus:
	cmp r0, #0
	bge.n _divmod_plus_plus
	
_divmod_plus_minus:
	rsbs r0, r0, #0
	push_tos
	movs tos, r0
	bl _udivmod
	rsbs tos, tos, #0
	pop {pc}
	
_divmod_plus_plus:
	push_tos
	movs tos, r0
	bl _udivmod
	pop {pc}
	end_inlined
