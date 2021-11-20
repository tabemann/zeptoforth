@ Copyright (c) 2019-2021 Travis Bemann
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
	
	.syntax	unified
	.cpu cortex-m0
	.thumb

	.include "config.s"
	.include "../m0/macro.s"
	.include "variables.s"
	
	.text

	.include "vectors.s"

	@@ The first (null) dictionary entry
	.p2align 2
	.word invisible_flag
	.word 0
10:	.byte 0
	.p2align 2
	
	@@ The entry point
 _handle_reset:
	@@ Initialize r11, relied upon by swdcom
	movs r0, #0
	mov r11, r0
	@@ Initalize r5, relied upon to provide a pointer to RAM
	ldr r5, =ram_real_start
	@@ Initialize the top of stack register
	ldr tos, =0xFEDCBA98
	@@ Initialize the data stack pointer
	ldr r0, =stack_top
	mov dp, r0
	@@ Just in case someone calls this we will restore the return stack
	@@ pointer.
	ldr r0, =rstack_top
	mov sp, r0
	@@ Put a deliberate garbage value in handler
	ldr r0, =0xF0E1C2D3
	ldr r1, =handler
	str r0, [r1, #0]
	str r0, [r1, #4]
	@@ Initialize HERE
	ldr r0, =here
	ldr r1, =ram_current
	str r1, [r0, #0]
	str r1, [r0, #4]
	@@ Call the rest of the runtime in an exception handler
2:	push_tos
	ldr tos, =outer_exc_handled
	bl _try
	@@ If the inner loop returns, reboot
	b _handle_reset

	@@ The outermost exception handling - if an exception happens here the
	@@ system will reboot
outer_exc_handled:
	bl _init_platform_variables
	bl _init_variables
	bl _init_hardware
	bl _init_flash
	bl _init_dict
	bl _init_flash_buffers
	bl _do_init
	bl _welcome
	bl _quit

_init_platform_variables:
	movs r0, #0
	ldr r1, =sio_hook
	str r0, [r1, #0]
	str r0, [r1, #4]
	ldr r1, =core_1_launched
	str r0, [r1]
	bx lr

	@ Reboot the RP2040 in BOOTSEL mode
	define_word "bootsel", visible_flag
_bootsel:
	movs r2, #0
	ldr r1, ='U | ('B << 8)
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	movs r3, r0
	movs r0, #0
	movs r1, #0
	blx r3
	bx lr
	end_inlined

	.ltorg
	
	.include "hardware.s"
	.include "flashrom.s"
	.include "console.s"
	.include "expose.s"
	.include "../m0/core.s"
	.include "divide.s"
	.include "../m0/outer.s"
	.include "../m0/cond.s"
	.include "../m0/asm.s"
	.include "../common/strings.s"
	.include "../m0/double.s"
	.include "../m0/exception.s"
	.include "multicore.s"
	.include "../common/final.s"
