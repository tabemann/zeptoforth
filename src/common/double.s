@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2020 Travis Bemann
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

	@@ Double drop
	define_word "2drop", visible_flag | inlined_flag
_2drop:	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Double swap
	define_word "2swap", visible_flag | inlined_flag
_2swap:	ldr r0, [dp]
	ldr r1, [dp, #4]
	ldr r2, [dp, #8]
	str tos, [dp, #4]
	str r0, [dp, #8]
	str r2, [dp]
	movs tos, r1
	bx lr
	end_inlined

	@@ Double over
	define_word "2over", visible_flag | inlined_flag
_2over:	ldr r0, [dp, #4]
	ldr r1, [dp, #8]
	subs dp, #8
	str tos, [dp, #4]
	str r1, [dp]
	movs tos, r0
	bx lr
	end_inlined
	
	@@ Double dup
	define_word "2dup", visible_flag | inlined_flag
_2dup:	ldr r0, [dp]
	subs dp, #8
	str tos, [dp, #4]
	str r0, [dp]
	bx lr
	end_inlined

	@@ Double nip
	define_word "2nip", visible_flag | inlined_flag
_2nip:	ldr r0, [dp]
	str r0, [dp, #8]
	adds dp, #8
	bx lr
	end_inlined

	@@ Double tuck
	define_word "2tuck", visible_flag
_2tuck:	ldr r0, [dp]
	ldr r1, [dp, #4]
	ldr r2, [dp, #8]
	str tos, [dp, #4]
	str r0, [dp, #8]
	subs dp, #8
	str r0, [dp]
	str r1, [dp, #4]
	str r2, [dp, #8]
	bx lr
	end_inlined

	@@ Test for the equality of two double words
	define_word "d=", visible_flag
_deq:	ldmia dp!, {r0, r1, r2}
	eors r0, r2
	eors tos, r1
	orrs tos, r0
	subs tos, #1
	sbcs tos, tos
	bx lr
	end_inlined

	@@ Test for the inequality of two double words
	define_word "d<>", visible_flag
_dne:	ldmia dp!, {r0, r1, r2}
	eors r0, r2
	eors tos, r1
	orrs tos, r0
	subs tos, #1
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@@ Unsigned double less than
	define_word "du<", visible_flag | inlined_flag
_dult:	ldmia dp!, {r0, r1, r2}
	subs r2, r0
	sbcs r1, tos
	sbcs tos, tos
	bx lr
	end_inlined

	@@ Unsigned double greater than
	define_word "du>", visible_flag | inlined_flag
_dugt:	ldmia dp!, {r0, r1, r2}
	subs r0, r2
	sbcs tos, r1
	sbcs tos, tos
	bx lr
	end_inlined

	@@ Unsigned double greater than or equal
	define_word "du>=", visible_flag | inlined_flag
_duge:	ldmia dp!, {r0, r1, r2}
	subs r2, r0
	sbcs r1, tos
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@@ Unsigned double less than or equal
	define_word "du<=", visible_flag | inlined_flag
_dule:	ldmia dp!, {r0, r1, r2}
	subs r0, r2
	sbcs tos, r1
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@@ Signed double less than
	define_word "d<", visible_flag
_dlt:	ldmia dp!, {r0, r1, r2}
	subs r2, r0
	sbcs r1, tos
	blt 1f
	movs tos, #0
	bx lr
1:	movs tos, #0
	subs tos, #1
	bx lr
	end_inlined

	@@ Signed double greater than
	define_word "d>", visible_flag
_dgt:	ldmia dp!, {r0, r1, r2}
	subs r0, r2
	sbcs tos, r1
	blt 1f
	movs tos, #0
	bx lr
1:	movs tos, #0
	subs tos, #1
	bx lr
	end_inlined

	@@ Signed double greater than or equal than
	define_word "d>=", visible_flag
_dge:	ldmia dp!, {r0, r1, r2}
	subs r2, r0
	sbcs r1, tos
	blt 1f
	movs tos, #0
	subs tos, #1
	bx lr
1:	movs tos, #0
	bx lr
	end_inlined

	@@ Signed double less than or equal than
	define_word "d<=", visible_flag
_dle:	ldmia dp!, {r0, r1, r2}
	subs r0, r2
	sbcs tos, r1
	blt 1f
	movs tos, #0
	subs tos, #1
	bx lr
1:	movs tos, #0
	bx lr
	end_inlined
	
	@@ Double equals zero
	define_word "d0=", visible_flag | inlined_flag
_d0eq:	ldmia dp!, {r0}
	subs r0, #1
	sbcs tos, #0
	sbcs tos, tos
	bx lr
	end_inlined

	@@ Double not equals zero
	define_word "d0<>", visible_flag | inlined_flag
_d0ne:	ldmia dp!, {r0}
	subs r0, #1
	sbcs tos, #0
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@@ Double less than zero
	define_word "d0<", visible_flag | inlined_flag
_d0lt:	adds dp, #4
	asrs tos, tos, #31
	bx lr
	end_inlined

	@@ Double greater than zero
	define_word "d0>", visible_flag
_d0gt:	ldmia dp!, {r0}
	movs r1, tos
	orrs r0, tos
	movs tos, #0
	asrs r1, r1, #31
	bne 1f
	cmp r0, #0
	beq 1f
	mvns tos, tos
1:	bx lr
	end_inlined
	
	@@ Double less than or equal to zero
	define_word "d0<=", visible_flag
_d0le:	ldmia dp!, {r0}
	movs r1, tos
	orrs r0, tos
	movs tos, #0
	mvns tos, tos
	asrs r1, r1, #31
	bne 1f
	cmp r0, #0
	beq 1f
	movs tos, #0
1:	bx lr
	end_inlined

	@@ Double greater than or equal to zero
	define_word "d0>=", visible_flag | inlined_flag
_d0ge:	adds dp, #4
	asrs tos, tos, #31
	mvns tos, tos
	bx lr
	end_inlined

	@@ Double left shift
	define_word "2lshift", visible_flag
_dlshift:	
	ldm dp!, {r0, r1}  @ r0 high, r1 low
	lsls r2, r1, r6
	str r2, [dp, #-4]!
	lsls r2, r0, r6
	rsbs r3, r6, #32
	lsrs r3, r1, r3
	orrs r2, r3
	subs r6, #32
	lsls r6, r1, r6
	orrs r6, r2
	bx lr
	end_inlined

	@@ Double right shift
	define_word "2rshift", visible_flag
_drshift:
	ldm dp!, {r0, r1}  @ r0 high, r1 low
	lsrs r2, r1, r6
	rsbs r3, r6, #32
	lsls r3, r0, r3
	orrs r2, r3
	subs r3, r6, #32
	lsrs r3, r0, r3
	orrs r2, r3
	str r2, [dp, #-4]!
	lsrs r6, r0, r6
	bx lr
	end_inlined

	@@ Double arithmetic right shift
	define_word "2arshift", visible_flag
_darshift:
	ldm dp!, {r0, r1}  @ r0 high, r1 low
	cmp r6, #32
	blo 1f
	subs r3, r6, #32
	asrs r2, r0, r3
	b 2f
1:      lsrs r2, r1, r6
	rsbs r3, r6, #32
	lsls r3, r0, r3
	orrs r2, r3
2:      str r2, [dp, #-4]! 
	asrs r6, r0, r6
	bx lr
	end_inlined
	
	@@ Negate a double word
	define_word "dnegate", visible_flag | inlined_flag
_dnegate:
	ldr r0, [dp]
	mvns r0, r0
	mvns tos, tos
	adds r0, #1
	adcs tos, #0
	str r0, [dp]
	bx lr
	end_inlined
	
	@@ Add two double words
	define_word "d+", visible_flag | inlined_flag
_dadd:	ldmia dp!, {r0, r1, r2}
	adds r2, r0
	adcs tos, r1
	subs dp, #4
	str r2, [dp]
	bx lr
	end_inlined

	@@ Subtract two double words
	define_word "d-", visible_flag | inlined_flag
_dsub:	ldmia dp!, {r0, r1, r2}
	subs r2, r0
	sbcs tos, r1
	subs dp, #4
	str r2, [dp]    
	bx lr
	end_inlined
	
	@@ Add with carry
	define_word "um+", visible_flag | inlined_flag
_umadd:	movs r0, tos
	pull_tos
	adds tos, r0
	push_tos
	adcs tos, #0
	bx lr
	end_inlined

	@@ Multiply two unsigned 32-bit values to get an unsigned 64-bit value
	define_word "um*", visible_flag | inlined_flag
_ummul:	ldr r0, [dp]
	umull r0, tos, r0, tos
	str r0, [dp]
	bx lr
	end_inlined

	@@ Multiply two signed 32-bit values to get a signed 64-bit value
	define_word "m*", visible_flag | inlined_flag
_mmul:	ldr r0, [dp]
	smull r0, tos, r0, tos
	str r0, [dp]
	bx lr
	end_inlined

	@@ Unsigned multiply 64 * 64 = 64
	define_word "ud*", visible_flag
_udmul:
	ldmia dp!, {r0, r1, r2}

	muls	tos, r2        @ High-1 * Low-2 --> tos
	muls	r1, r0         @ High-2 * Low-1 --> r1
	adds	tos, r1        @                    Sum into tos

	lsrs	r1, r0, #16
	lsrs	r3, r2, #16
	muls	r1, r3
	adds	tos, r1

	lsrs	r1, r0, #16
	uxth	r0, r0
	uxth	r2, r2
	muls	r1, r2
	muls	r3, r0
	muls	r0, r2

	movs	r2, #0
	adds	r1, r3
	adcs	r2, r2
	lsls	r2, #16
	adds	tos, r2

	lsls	r2, r1, #16
	lsrs	r1, #16
	adds	r0, r2
	adcs	tos, r1

        subs dp, #4
        str r0, [dp]

        bx lr
	end_inlined

	@@ Unsigned multiply 64 * 64 = 124
	@@ ( ud1 ud2 -- udl udh )
_udmmul:
	push {r4, lr}
	movs r4, #0

	@ ( d c b a )

	push_tos
	ldr tos, [dp, #4] @@ b
	push_tos
	ldr tos, [dp, #16] @@ d
	bl _ummul
	movs r1, tos @@ b*d-high
	pull_tos
	movs r0, tos @@ b*d-low

	ldr tos, [dp, #0] @@ a
	push_tos
	ldr tos, [dp, #12] @@ c
	push {r0, r1}
	bl _ummul
	pop {r0, r1}
	movs r3, tos @@ a*c-high
	pull_tos
	movs r2, tos @@ a*c-low

	ldr tos, [dp, #0] @@ a
	push_tos
	ldr tos, [dp, #16] @@ d
	push {r0, r1, r2, r3}
	bl _ummul
	pop {r0, r1, r2, r3}
	adds r2, tos @@ a*c-low + a*d-high
	adcs r3, r4 @@ carry
	pull_tos
	adds r1, tos @@ a*d-low + b*d-high
	adcs r2, r4 @@ carry
	adcs r3, r4 @@ carry

	ldr tos, [dp, #4] @@ b
	push_tos
	ldr tos, [dp, #12] @@ c
	push {r0, r1, r2, r3}
	bl _ummul
	pop {r0, r1, r2, r3}
	adds r2, tos @@ a*c-low + b*c-high + a*d-high
	adcs r3, r4 @@ carry
	pull_tos
	adds r1, tos @@ b*c-low + a*d-low + b*d-high
	adcs r2, r4 @@ carry
	adcs r3, r4 @@ carry
	pull_tos

	movs tos, r3
	str r2, [dp, #0]
	str r1, [dp, #4]
	str r0, [dp, #8]

	pop {r4, pc}
	end_inlined

	@ ( n1 n2 n3 -- n1*n2/n3 ) With double length intermediate result
	define_word "*/", visible_flag
_muldiv:	
	push {lr}
	push {tos}
	pull_tos
	bl _mmul
	push_tos
	pop {tos}
	bl _mdivmod
	bl _nip
	pop {pc}
	end_inlined
	
	@ ( u1 u2 u3 -- u1*u2/u3 ) With double length intermediate result
	define_word "*/mod", visible_flag
_muldivmod:	
	push {lr}
	push {tos}
	pull_tos
	bl _mmul
	push_tos
	pop {tos}
	bl _mdivmod
	pop {pc}
	end_inlined
	
	@ ( u1 u2 u3 -- u1*u2/u3 ) With double length intermediate result
	define_word "u*/", visible_flag
_umuldiv:	
	push {lr}
	push {tos}
	pull_tos
	bl _ummul
	push_tos
	pop {tos}
	bl _umdivmod
	bl _nip
	pop {pc}
	end_inlined
	
	@ ( u1 u2 u3 -- u1*u2/u3 ) With double length intermediate result
	define_word "u*/mod", visible_flag
_umuldivmod:	
	push {lr}
	push {tos}
	pull_tos
	bl _ummul
	push_tos
	pop {tos}
	bl _umdivmod
	pop {pc}
	end_inlined

	@@ Unsigned 64 / 32 = 32 remainder 32 division
	define_word "um/mod", visible_flag
_umdivmod:
	push {lr}
	push_tos
	movs tos, #0
	bl _uddivmod
	pull_tos
	adds dp, #4
	pop {pc}
	end_inlined

	@@ Signed 64 / 32 = 32 remainder 32 division
	define_word "m/mod", visible_flag
_mdivmod:
	push {lr}
	push_tos
	asrs tos, tos, #31
	bl _ddivmod
	pull_tos
	adds dp, #4
	bx lr
	end_inlined
	
        @@ Unsigned divide 64/64 = 64 remainder 64
        @@ ( ud1 ud2 -- ud ud)
        @@ ( 1L 1H 2L tos: 2H -- Rem-L Rem-H Quot-L tos: Quot-H )
	define_word "ud/mod", visible_flag
_uddivmod:
	push {r4, r5}
	
	@ ( DividendL DividendH DivisorL DivisorH -- RemainderL RemainderH ResultL ResultH )
	@   8         4         0        tos      -- 8          4          0       tos
	
	
	@ Shift-High Shift-Low Dividend-High Dividend-Low
	@         r3        r2            r1           r0
	
	movs r3, #0
	movs r2, #0
	ldr  r1, [dp, #4]
	ldr  r0, [dp, #8]
	
	@ Divisor-High Divisor-Low
	@          r5           r4

	movs r5, tos
	ldr  r4, [dp, #0]
	
	@ For this long division, we need 64 individual division steps.
	movs tos, #64

	@ Shift the long chain of four registers.
3:	lsls r0, #1
	adcs r1, r1
	adcs r2, r2
	adcs r3, r3
	
	@ Compare Divisor with top two registers
	cmp r3, r5 @ Check high part first
	bhi 1f
	blo 2f
	
	cmp r2, r4 @ High part is identical. Low part decides.
	blo 2f

	@ Subtract Divisor from two top registers
1:  	subs r2, r4 @ Subtract low part
	sbcs r3, r5 @ Subtract high part with carry
	
	@ Insert a bit into Result which is inside LSB of the long register.
	adds r0, #1

2:	subs tos, #1
	bne 3b

	@ Now place all values to their destination.
	movs tos, r1       @ Result-High
	str  r0, [dp, #0] @ Result-Low
	str  r3, [dp, #4] @ Remainder-High
	str  r2, [dp, #8] @ Remainder-Low
	
	pop {r4, r5}
	bx lr
	end_inlined

        @@ Unsigned divide 64/64 = 64 remainder 64
        @@ ( ud1 ud2 -- ud ud)
        @@ ( 1L 1H 2L tos: 2H -- Rem-L Rem-H Quot-L tos: Quot-H )
	define_word "uf/mod", visible_flag
_ufdivmod:
	push {r4, r5}

	movs r3, #0
	ldr  r2, [dp, #4]
	ldr  r1, [dp, #8]
	movs r0, #0
	
	@ Divisor-High Divisor-Low
	@          r5           r4

	movs r5, tos
	ldr  r4, [dp, #0]
	
	@ For this long division, we need 64 individual division steps.
	movs tos, #64

	@ Shift the long chain of four registers.
3:	lsls r0, #1
	adcs r1, r1
	adcs r2, r2
	adcs r3, r3
	
	@ Compare Divisor with top two registers
	cmp r3, r5 @ Check high part first
	bhi 1f
	blo 2f
	
	cmp r2, r4 @ High part is identical. Low part decides.
	blo 2f

	@ Subtract Divisor from two top registers
1:  	subs r2, r4 @ Subtract low part
	sbcs r3, r5 @ Subtract high part with carry
	
	@ Insert a bit into Result which is inside LSB of the long register.
	adds r0, #1

2:	subs tos, #1
	bne 3b

	@ Now place all values to their destination.
	movs tos, r1       @ Result-High
	str  r0, [dp, #0] @ Result-Low
	str  r3, [dp, #4] @ Remainder-High
	str  r2, [dp, #8] @ Remainder-Low
	
	pop {r4, r5}
	bx lr
	end_inlined

	@@ Signed divide 64 / 64 = 64 remainder 64
	@@ ( d1 d2 -- d d )
	@@ ( 1L 1H 2L tos: 2H -- Rem-L Rem-H Quot-L tos: Quot-H )
	define_word "d/mod", visible_flag
_ddivmod:
	push {lr}
	asrs r0, tos, #31
	beq 2f
	@@ ? / -
	bl _dnegate
	bl _2swap
	asrs r0, tos, #31
	beq 1f
	@@ - / -
	bl _dnegate
	bl _2swap
	bl _uddivmod
	bl _2swap
	bl _dnegate @ Negative remainder
	bl _2swap
	pop {pc}
1:	@@ + / -
	bl _2swap
	bl _uddivmod
	bl _dnegate @ Negative result
	pop {pc}
2:	@@ ? / +
	bl _2swap
	asrs r0, tos, #31
	beq 3f
	@@ - / +
	bl _dnegate
	bl _2swap
	bl _uddivmod
	bl _dnegate @ Negative result
	bl _2swap
	bl _dnegate @ Negative remainder
	bl _2swap
	pop {pc}
3:	@@ + / +
	bl _2swap
	bl _uddivmod
	pop {pc}
	end_inlined

	@@ Divide unsigned two double words and get a double word quotient
	define_word "ud/", visible_flag
_uddiv:	push {lr}
	bl _uddivmod
	bl _2nip
	pop {pc}
	end_inlined
	
	@@ Divide signed two double words and get a double word quotient
	define_word "d/", visible_flag
_ddiv:	push {lr}
	bl _ddivmod
	bl _2nip
	pop {pc}
	end_inlined

	@@ Signed multiply two s31.32 numbers, sign wrong in overflow
	define_word "f*", visible_flag
_fmul:  push {lr}
	asr r0, tos, #31 @ Turn MSB into 0xffffffff or 0x00000000
	beq 1f
	@ - * ?
	bl _dnegate
	bl _2swap
	asr r0, tos, #31 @ Turn MSB into 0xffffffff or 0x00000000
	beq 2f @ - * +
	
	@ - * -
	bl _dnegate
	
3:      @ + * +, - * -
	bl _udmmul
	@ ( LL L H HH )
	pull_tos
	ldmia dp!, {r0}
	str r0, [dp]
	@ ( L H )
	pop {pc}
	
1:      @ + * ?
	bl _2swap
	asr r0, tos, #31 @ Turn MSB into 0xffffffff or 0x00000000
	beq 3b @ + * +
	
	bl _dnegate
	
	@ - * + or + * -
2:      bl _udmmul
	@ ( LL L H HH )
	pull_tos
	ldmia dp!, {r0}
	str r0, [dp]
	@ ( L H )
	bl _dnegate
	pop {pc}
	end_inlined
	
	@@ Signed divide for s31.32, sign wrong in overflow
	define_word "f/", visible_flag
_fdiv:	@ Take care of sign ! ( 1L 1H 2L 2H - EL EH )
	push {lr}
	asr r0, tos, #31 @ Turn MSB into 0xffffffff or 0x00000000
	beq 2f
	@ ? / -
	bl _dnegate
	bl _2swap
	asr r0, tos, #31 @ Turn MSB into 0xffffffff or 0x00000000
	beq 3f @ + / -
	
	@ - / -
	bl _dnegate
1:      bl _2swap @ - / - or + / +
	bl _ufdivmod
	bl _2nip
	pop {pc}
	
2:      @ ? / +
	bl _2swap
	asr r0, tos, #31 @ Turn MSB into 0xffffffff or 0x00000000
	beq 1b @ + / +
	
	@ - / +
	bl _dnegate
3:      bl _2swap @ - / + or + / -
	bl _ufdivmod
	bl _dnegate
	bl _2nip
	pop {pc}
	end_inlined
