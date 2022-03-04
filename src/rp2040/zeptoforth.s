@ Copyright (c) 2019-2022 Travis Bemann
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
	@@ Initialize the top of stack register
	ldr tos, =0xFEDCBA98
	@@ Get the dictionary base
	ldr r0, =dict_base
	ldr r1, =ram_current
	str r1, [r0]
	@@ Set a garbage dictionary base for the second core
	ldr r0, =dict_base + 4
	ldr r1, =0xEFBEADDE
	str r1, [r0]
	@@ Initialize HERE
	ldr r0, =ram_current + ram_here_offset
	ldr r1, =ram_current + user_offset
	str r1, [r0]
	@@ Initialize the data stack pointer
	ldr r1, =stack_top
	movs dp, r1
	@@ Initialize the return stack pointer
	ldr r1, =rstack_top
	mov sp, r1
	@@ Put a garbage value in HANDLER to force a crash if is used
	ldr r0, =ram_current + handler_offset
	ldr r1, =0xF0E1C2D3
	str r1, [r0]
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
	ldr r1, =begin_write
	str r0, [r1]
	ldr r1, =waiting_write_done
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
