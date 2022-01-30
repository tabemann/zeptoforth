@ Copyright (c) 2021-2022 Travis Bemann
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

	@@ CPUID register (do not confuse with the ARM CPUID register)
	.equ SIO_CPUID, SIO_BASE + 0x000

	@@ FIFO status register
	.equ FIFO_ST, SIO_BASE + 0x050

	@@ Bit for core's TX FIFO is not full
	.equ FIFO_ST_RDY, 1 << 1

	@@ Bit for core's RX FIFO is not empty
	.equ FIFO_ST_VLD, 1 << 0

	@@ Write access to this core's TX FIFO
	.equ FIFO_WR, SIO_BASE + 0x054

	@@ Read access to this core's RX FIFO
	.equ FIFO_RD, SIO_BASE + 0x058

	@@ Synchronization spinlock index
	.equ SYNC_SPINLOCK_INDEX, 30

	@@ Spinlock 30, which we will use for synchronizing between
	.equ SYNC_SPINLOCK, SIO_BASE + 0x100 + (SYNC_SPINLOCK_INDEX * 4)

	@@ Synchronization value
	.equ SYNC_VALUE, 0x7FFFFFFF

	@@ Blocking FIFO push - reads value from R0
_core_fifo_push:
	ldr r1, =FIFO_ST
	movs r2, #FIFO_ST_RDY
1:	ldr r3, [r1]
	tst r3, r2
	beq 1b
	ldr r1, =FIFO_WR
	str r0, [r1]
	sev
	bx lr
	
	@@ Blocking FIFO pop - reads value into R0
_core_fifo_pop:
	ldr r0, =FIFO_ST
	movs r1, #FIFO_ST_VLD
1:	ldr r2, [r0]
	tst r2, r1
	beq 1b
	ldr r0, =FIFO_RD
	ldr r0, [r0]
	bx lr

	@@ Force the other core to wait
	define_internal_word "force-core-wait", visible_flag
_force_core_wait:
	push {lr}
@	ldr r0, =core_1_launched
@	ldr r0, [r0]
@	cmp r0, #0
@	beq 2f
@	ldr r0, =SYNC_SPINLOCK
@1:	ldr r1, [r0]
@	cmp r1, #0
@	beq 1b
@	ldr r0, =FIFO_ST
@	movs r1, #FIFO_ST_RDY
@	ldr r0, [r0]
@	tst r0, r1
@	beq 2f
@	ldr r0, =SYNC_VALUE
@	bl _core_fifo_push
2:	pop {pc}
	end_inlined

	@@ Release the other core
	define_internal_word "release-core", visible_flag
_release_core:	
@	ldr r0, =core_1_launched
@	ldr r0, [r0]
@	cmp r0, #0
@	beq 1f
@	ldr r0, =SYNC_SPINLOCK
@	movs r1, #1
@	str r1, [r0]
1:	bx lr
	end_inlined

	.ltorg
