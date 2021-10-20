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

	@@ Blocking FIFO pop - reads value into R0
_aux_fifo_pop:
	push {lr}
	ldr r0, =FIFO_ST
	movs r1, #FIFO_ST_VLD
1:	ldr r2, [r0]
	tst r2, r1
	beq 1b
	ldr r0, =FIFO_RD
	ldr r0, [r0]
	pop {pc}

	@@ Auxiliary core entry
	define_internal_word "aux-core-entry", visible_flag
_aux_core_entry:
	ldr tos, =0xFEDCBA98
	bl _aux_fifo_pop
	movs dp, r0
	bl _aux_fifo_pop
	push_tos
	movs tos, r0
	bl _store_here
	bl _aux_fifo_pop
	adds r0, #1
	blx r0
	bx lr
	end_inlined

	.ltorg
