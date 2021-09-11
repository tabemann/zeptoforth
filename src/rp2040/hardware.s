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

	.equ IO_BANK0_BASE, 0x40014000
	.equ PADS_BANK0_BASE, 0x4001c000
	.equ SIO_BASE, 0xd0000000

	.equ GPIO_OE_OFFSET, 0x020

	@@ Enable the GPIOs
	define_internal_word "enable-gpios", visible_flag
_gpio_init:
	ldr r0, =SIO_BASE
	movs r1, #0
	str r1, [r0, #GPIO_OE_OFFSET]
	ldr r0, =IO_BANK0_BASE + (2 * 8) + 4
	movs r1, #5
	ldr r2, =(30 * 8) + 4
1:	str r1, [r0]
	adds r0, #8
	cmp r0, r2
	bne 1b
	bx lr
	end_inlined
